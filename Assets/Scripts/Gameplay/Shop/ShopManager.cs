using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Shop
{
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
    /// NOTE: this class is intentionally NOT wired into GameManager.cs by this change (per task
    /// instructions, GameManager.cs is not to be edited here) — a follow-up edit needs to add,
    /// inside <c>GameManager.BuildManagers()</c>:
    ///   ShopManager = new ShopManager();
    ///   RegisterManager(ShopManager);
    /// plus a public <c>ShopManager</c> property, mirroring CraftingManager/DialogueManager.
    /// </summary>
    public class ShopManager : IManager
    {
        private const string LogCategory = "Shop";

        public ShopDefinition ActiveShop { get; private set; }
        public InventoryManager BuyerInventory { get; private set; }
        public bool IsOpen => ActiveShop != null && BuyerInventory != null;

        public void Initialize()
        {
            GameLogger.Info(LogCategory, "ShopManager initialized.");
        }

        public void Shutdown()
        {
            if (IsOpen)
            {
                CloseShop();
            }
        }

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

            if (!entry.HasStockFor(quantity))
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

            entry.ConsumeStock(quantity);

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
            catalogEntry?.RestockFromSale(quantity);

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
    }
}
