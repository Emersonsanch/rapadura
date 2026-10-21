using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Shop
{
    /// <summary>JSON-serializable snapshot of <see cref="ShopManager"/>'s live stock (parallel arrays: JsonUtility has no dictionaries).</summary>
    [Serializable]
    public class ShopSaveState
    {
        /// <summary>"&lt;shopId&gt;/&lt;itemId&gt;" for every finite catalog entry whose stock differs from the authored value.</summary>
        public string[] stockKeys = Array.Empty<string>();
        public int[] stockValues = Array.Empty<int>();

        /// <summary>In-game day of the last dawn restock (see <see cref="ShopManager.Restock"/>).</summary>
        public int lastRestockDay = 1;
    }

    /// <summary>
    /// Central runtime authority over shop buy/sell transactions: plain <see cref="IManager"/>
    /// (not a MonoBehaviour), same shape as <c>CraftingManager</c>/<c>DialogueManager</c>. Holds
    /// which <see cref="ShopDefinition"/> is currently open and which <see cref="InventoryManager"/>
    /// is buying/selling against it, validates currency and stock, and moves items/currency
    /// between the shop's catalog and the buyer's inventory. Publishes
    /// <see cref="ShopOpenedEvent"/>/<see cref="ItemPurchasedEvent"/>/<see cref="ItemSoldEvent"/>/
    /// <see cref="ShopClosedEvent"/> on the shared <see cref="EventBus"/> so UI/analytics/quests
    /// can react without a direct reference.
    ///
    /// Currency: money is just an inventory item (see <see cref="ShopDefinition"/> doc comment for
    /// why item_gold was chosen over adding a dedicated currency system). Buying removes
    /// <c>ShopDefinition.CurrencyItemId</c> from the buyer's inventory and adds the purchased item;
    /// selling does the reverse at <c>Price * SellBackRate</c>.
    ///
    /// Stock: <see cref="ShopDefinition"/> assets are read-only authored data. The live stock is a
    /// runtime table keyed "&lt;shopId&gt;/&lt;itemId&gt;" that only holds entries that differ from the
    /// authored <see cref="ShopInventoryItem.InitialStock"/> (missing key = authored value), so nothing
    /// ever writes into the ScriptableObject (which in the Editor used to rewrite the asset on disk,
    /// and in builds leaked depleted stock across saves). The table is per save: saved under
    /// <see cref="SaveKey"/> ("Shops"), cleared by <see cref="ResetForNewSession"/> (GameManager
    /// registers it with NewSessionReset), and an older save without a "Shops" entry simply starts
    /// with authored stock.
    ///
    /// Restock rule: at every in-game dawn (<see cref="TimeOfDayPhaseChangedEvent"/> entering
    /// <see cref="TimeOfDayPhase.Dawn"/>) of a day later than <see cref="LastRestockDay"/>, every shop
    /// goes back to its authored stock — sold-out items come back, and surplus the player sold to the
    /// shop is gone (the merchant sold it overnight). Keyed on the day number, so a save restored at
    /// dawn (which republishes the phase change) never restocks twice for the same day.
    ///
    /// Built and registered by <c>GameManager.BuildManagers()</c> (ServiceLocator, SaveManager,
    /// NewSessionReset), mirroring CraftingManager/DialogueManager.
    /// </summary>
    public class ShopManager : IManager, ISaveable, IResettable
    {
        public const string Key = "Shops";
        private const string LogCategory = "Shop";
        private const int FirstDay = 1;

        private readonly Dictionary<string, int> _stock = new Dictionary<string, int>(StringComparer.Ordinal);
        private bool _subscribed;

        public ShopDefinition ActiveShop { get; private set; }
        public InventoryManager BuyerInventory { get; private set; }
        public bool IsOpen => ActiveShop != null && BuyerInventory != null;

        public string SaveKey => Key;

        /// <summary>In-game day of the last dawn restock (1 = never restocked this session).</summary>
        public int LastRestockDay { get; private set; } = FirstDay;

        public void Initialize()
        {
            if (!_subscribed)
            {
                _subscribed = true;
                EventBus.Subscribe<TimeOfDayPhaseChangedEvent>(OnTimeOfDayPhaseChanged);
            }

            GameLogger.Info(LogCategory, "ShopManager initialized.");
        }

        public void Shutdown()
        {
            if (IsOpen)
            {
                CloseShop();
            }

            if (_subscribed)
            {
                _subscribed = false;
                EventBus.Unsubscribe<TimeOfDayPhaseChangedEvent>(OnTimeOfDayPhaseChanged);
            }
        }

        /// <summary>New game / load: drops the open shop session (its buyer inventory belongs to the old session's player) and puts every shop back to authored stock.</summary>
        public void ResetForNewSession()
        {
            if (IsOpen)
            {
                CloseShop();
            }

            _stock.Clear();
            LastRestockDay = FirstDay;
        }

        // ---------------------------------------------------------------
        // Stock
        // ---------------------------------------------------------------

        /// <summary>Live stock of <paramref name="shop"/>'s catalog entry at <paramref name="catalogIndex"/>: -1 when infinite, 0 when the index is invalid.</summary>
        public int GetStock(ShopDefinition shop, int catalogIndex)
        {
            if (shop == null || catalogIndex < 0 || catalogIndex >= shop.Catalog.Count)
            {
                return 0;
            }

            return GetStock(shop, shop.Catalog[catalogIndex]);
        }

        /// <summary>Live stock of <paramref name="entry"/> in <paramref name="shop"/>: -1 when infinite.</summary>
        public int GetStock(ShopDefinition shop, ShopInventoryItem entry)
        {
            if (shop == null || entry?.Item == null)
            {
                return 0;
            }

            if (entry.HasInfiniteStock)
            {
                return -1;
            }

            return _stock.TryGetValue(GetStockKey(shop, entry), out int live) ? live : entry.InitialStock;
        }

        /// <summary>Whether <paramref name="shop"/> can sell <paramref name="quantity"/> of <paramref name="entry"/> right now.</summary>
        public bool HasStockFor(ShopDefinition shop, ShopInventoryItem entry, int quantity)
        {
            if (shop == null || entry?.Item == null)
            {
                return false;
            }

            return entry.HasInfiniteStock || GetStock(shop, entry) >= quantity;
        }

        /// <summary>Refills every shop to its authored stock (drops the whole live table) and publishes <see cref="ShopRestockedEvent"/>.</summary>
        public void Restock(int day)
        {
            _stock.Clear();
            LastRestockDay = Math.Max(LastRestockDay, day);
            EventBus.Publish(new ShopRestockedEvent(day));
        }

        private void OnTimeOfDayPhaseChanged(TimeOfDayPhaseChangedEvent evt)
        {
            if (evt.CurrentPhase == TimeOfDayPhase.Dawn && evt.Day > LastRestockDay)
            {
                Restock(evt.Day);
            }
        }

        private void AddStock(ShopDefinition shop, ShopInventoryItem entry, int delta)
        {
            if (entry.HasInfiniteStock)
            {
                return;
            }

            int value = Math.Max(0, GetStock(shop, entry) + delta);
            string key = GetStockKey(shop, entry);

            if (value == entry.InitialStock)
            {
                _stock.Remove(key);
            }
            else
            {
                _stock[key] = value;
            }
        }

        /// <summary>
        /// Table key of a catalog entry. By item id rather than catalog index so a save keeps
        /// working when designers reorder a catalog (two entries for the same item share stock).
        /// </summary>
        private static string GetStockKey(ShopDefinition shop, ShopInventoryItem entry) => $"{shop.ShopId}/{entry.Item.ItemId}";

        // ---------------------------------------------------------------
        // Session
        // ---------------------------------------------------------------

        /// <summary>Opens a shop session for the given buyer inventory. No-ops (with a warning) if a shop is already open.</summary>
        public void OpenShop(ShopDefinition shop, InventoryManager buyerInventory)
        {
            if (shop == null || buyerInventory == null)
            {
                GameLogger.Warning(LogCategory, "OpenShop called with a null shop or buyer inventory.");
                return;
            }

            if (IsOpen)
            {
                GameLogger.Warning(LogCategory, $"OpenShop('{shop.ShopId}') ignored, shop '{ActiveShop.ShopId}' is already open.");
                return;
            }

            ActiveShop = shop;
            BuyerInventory = buyerInventory;

            EventBus.Publish(new ShopOpenedEvent(shop.ShopId));
        }

        /// <summary>Ends the active shop session.</summary>
        public void CloseShop()
        {
            if (!IsOpen)
            {
                return;
            }

            string shopId = ActiveShop.ShopId;
            ActiveShop = null;
            BuyerInventory = null;

            EventBus.Publish(new ShopClosedEvent(shopId));
        }

        // ---------------------------------------------------------------
        // Buying
        // ---------------------------------------------------------------

        /// <summary>
        /// Buys <paramref name="quantity"/> of the catalog entry at <paramref name="catalogIndex"/>
        /// for the current buyer. Fails if no shop is open, the index is invalid, stock is
        /// insufficient, the buyer lacks enough currency, or there's no inventory space.
        /// </summary>
        public bool BuyItem(int catalogIndex, int quantity)
        {
            if (!IsOpen || quantity <= 0)
            {
                return false;
            }

            if (catalogIndex < 0 || catalogIndex >= ActiveShop.Catalog.Count)
            {
                GameLogger.Warning(LogCategory, $"BuyItem({catalogIndex}) out of range ({ActiveShop.Catalog.Count} entries).");
                return false;
            }

            ShopInventoryItem entry = ActiveShop.Catalog[catalogIndex];

            if (entry?.Item == null)
            {
                return false;
            }

            if (!HasStockFor(ActiveShop, entry, quantity))
            {
                return false;
            }

            int totalPrice = entry.Price * quantity;
            ItemDefinition currency = ItemDatabase.GetById(ActiveShop.CurrencyItemId);

            if (currency == null || BuyerInventory.GetTotalCount(ActiveShop.CurrencyItemId) < totalPrice)
            {
                return false;
            }

            if (!BuyerInventory.HasWeightCapacityFor(entry.Item, quantity))
            {
                return false;
            }

            int leftover = BuyerInventory.AddItem(entry.Item, quantity);

            if (leftover > 0)
            {
                // Not enough room for the full amount — refund the partial add and abort.
                if (leftover < quantity)
                {
                    BuyerInventory.RemoveById(entry.Item.ItemId, quantity - leftover);
                }

                return false;
            }

            if (!BuyerInventory.RemoveById(ActiveShop.CurrencyItemId, totalPrice))
            {
                // Should not happen given the balance check above, but roll back defensively.
                BuyerInventory.RemoveById(entry.Item.ItemId, quantity);
                return false;
            }

            AddStock(ActiveShop, entry, -quantity);

            EventBus.Publish(new ItemPurchasedEvent(ActiveShop.ShopId, entry.Item.ItemId, quantity, totalPrice));
            return true;
        }

        // ---------------------------------------------------------------
        // Selling
        // ---------------------------------------------------------------

        /// <summary>
        /// Sells <paramref name="quantity"/> of the item in the buyer's inventory slot
        /// <paramref name="slotIndex"/> to the active shop, paying <c>Price * SellBackRate</c>
        /// (using the item's own <see cref="ItemDefinition.Value"/> as the base price, since sold
        /// items need not already be in the shop's catalog).
        /// </summary>
        public bool SellItem(int slotIndex, int quantity)
        {
            if (!IsOpen || quantity <= 0)
            {
                return false;
            }

            if (slotIndex < 0 || slotIndex >= BuyerInventory.Slots.Count)
            {
                return false;
            }

            InventorySlotData slot = BuyerInventory.Slots[slotIndex];

            if (slot.IsEmpty || slot.quantity < quantity)
            {
                return false;
            }

            ItemDefinition item = ItemDatabase.GetById(slot.itemId);

            if (item == null || item.ItemId == ActiveShop.CurrencyItemId)
            {
                // Selling currency for itself makes no sense; reject.
                return false;
            }

            int payout = CalculateSellBackPayout(item.Value, quantity, ActiveShop.SellBackRate);

            if (!BuyerInventory.RemoveFromSlot(slotIndex, quantity))
            {
                return false;
            }

            ItemDefinition currency = ItemDatabase.GetById(ActiveShop.CurrencyItemId);

            if (currency != null)
            {
                BuyerInventory.AddItem(currency, payout);
            }

            ShopInventoryItem catalogEntry = FindCatalogEntry(item.ItemId);

            if (catalogEntry != null)
            {
                AddStock(ActiveShop, catalogEntry, quantity);
            }

            EventBus.Publish(new ItemSoldEvent(ActiveShop.ShopId, item.ItemId, quantity, payout));
            return true;
        }

        /// <summary>Computes the currency payout for selling <paramref name="quantity"/> units of an item worth <paramref name="baseValue"/> each, at <paramref name="sellBackRate"/> (e.g. 0.5 = 50%).</summary>
        public static int CalculateSellBackPayout(int baseValue, int quantity, float sellBackRate)
        {
            return UnityEngine.Mathf.Max(0, UnityEngine.Mathf.FloorToInt(baseValue * quantity * sellBackRate));
        }

        private ShopInventoryItem FindCatalogEntry(string itemId)
        {
            foreach (ShopInventoryItem entry in ActiveShop.Catalog)
            {
                if (entry?.Item != null && entry.Item.ItemId == itemId)
                {
                    return entry;
                }
            }

            return null;
        }

        // ---------------------------------------------------------------
        // ISaveable
        // ---------------------------------------------------------------

        public object CaptureState()
        {
            string[] keys = _stock.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();

            return new ShopSaveState
            {
                stockKeys = keys,
                stockValues = keys.Select(k => _stock[k]).ToArray(),
                lastRestockDay = LastRestockDay
            };
        }

        /// <summary>Replaces the live stock table with the saved one (SaveManager.Load has already reset it through NewSessionReset).</summary>
        public void RestoreState(object state)
        {
            if (state is not ShopSaveState saved)
            {
                return;
            }

            _stock.Clear();

            if (saved.stockKeys != null && saved.stockValues != null)
            {
                for (int i = 0; i < Math.Min(saved.stockKeys.Length, saved.stockValues.Length); i++)
                {
                    if (!string.IsNullOrEmpty(saved.stockKeys[i]))
                    {
                        _stock[saved.stockKeys[i]] = Math.Max(0, saved.stockValues[i]);
                    }
                }
            }

            LastRestockDay = Math.Max(FirstDay, saved.lastRestockDay);
        }
    }
}
