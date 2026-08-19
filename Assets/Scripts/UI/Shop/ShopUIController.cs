using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Shop
{
    /// <summary>
    /// Drives the shop window (ShopView.uxml/.uss), following the same single-<see cref="UIDocument"/>,
    /// <c>Q&lt;T&gt;()</c>-resolved-once pattern as <c>HudController</c>/<c>DialogueUIController</c>.
    /// Purely reactive: it never mutates state itself, it only reflects <see cref="ShopManager"/>'s
    /// active shop/catalog and forwards clicks back into it via <see cref="ShopManager.BuyItem"/>/
    /// <see cref="ShopManager.SellItem"/>.
    ///
    /// Buy rows are generated at runtime from <c>ShopDefinition.Catalog</c> (variable count),
    /// sell rows are generated at runtime from the buyer's <see cref="InventoryManager.Slots"/>
    /// (also variable count) — same "instantiate into a container" approach
    /// <c>DialogueUIController</c> uses for choice buttons.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ShopUIController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _shopNameLabel;
        private Label _currencyLabel;
        private VisualElement _buyListContainer;
        private VisualElement _sellListContainer;
        private Button _closeButton;

        private ShopManager _shopManager;
        private readonly List<VisualElement> _spawnedBuyRows = new List<VisualElement>();
        private readonly List<VisualElement> _spawnedSellRows = new List<VisualElement>();

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            ResolveShopManager();
            RegisterCallbacks();
            Hide();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("shop-root");
            _shopNameLabel = rootVisualElement.Q<Label>("shop-name");
            _currencyLabel = rootVisualElement.Q<Label>("shop-currency-amount");
            _buyListContainer = rootVisualElement.Q<VisualElement>("shop-buy-list");
            _sellListContainer = rootVisualElement.Q<VisualElement>("shop-sell-list");
            _closeButton = rootVisualElement.Q<Button>("shop-close-button");
        }

        /// <summary>
        /// Lets a bootstrap script assign the manager directly (useful in tests or before it is
        /// registered with the ServiceLocator). If never called, OnEnable falls back to resolving
        /// it from the ServiceLocator, since <see cref="ShopManager"/> is a plain C# class (not a
        /// Unity Object) and therefore cannot be assigned via the Inspector.
        /// </summary>
        public void SetShopManager(ShopManager manager)
        {
            _shopManager = manager;
        }

        private void ResolveShopManager()
        {
            if (_shopManager != null)
            {
                return;
            }

            ServiceLocator.TryGet(out _shopManager);
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<ShopOpenedEvent>(OnShopOpened);
            EventBus.Subscribe<ItemPurchasedEvent>(OnTransaction);
            EventBus.Subscribe<ItemSoldEvent>(OnTransaction);
            EventBus.Subscribe<ShopClosedEvent>(OnShopClosed);
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<ShopOpenedEvent>(OnShopOpened);
            EventBus.Unsubscribe<ItemPurchasedEvent>(OnTransaction);
            EventBus.Unsubscribe<ItemSoldEvent>(OnTransaction);
            EventBus.Unsubscribe<ShopClosedEvent>(OnShopClosed);
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);

            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }

            ClearRows();
        }

        private void OnShopOpened(ShopOpenedEvent evt)
        {
            Show();
            RefreshAll();
        }

        private void OnTransaction(ItemPurchasedEvent evt)
        {
            RefreshAll();
        }

        private void OnTransaction(ItemSoldEvent evt)
        {
            RefreshAll();
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (_shopManager != null && _shopManager.IsOpen)
            {
                RefreshAll();
            }
        }

        private void OnShopClosed(ShopClosedEvent evt)
        {
            Hide();
        }

        private void RefreshAll()
        {
            if (_shopManager == null || !_shopManager.IsOpen)
            {
                Hide();
                return;
            }

            ShopDefinition shop = _shopManager.ActiveShop;
            InventoryManager buyer = _shopManager.BuyerInventory;

            if (_shopNameLabel != null)
            {
                _shopNameLabel.text = LocalizeSafe(shop.DisplayNameKey);
            }

            if (_currencyLabel != null)
            {
                _currencyLabel.text = buyer.GetTotalCount(shop.CurrencyItemId).ToString();
            }

            RebuildBuyRows(shop, buyer);
            RebuildSellRows(shop, buyer);
        }

        private void RebuildBuyRows(ShopDefinition shop, InventoryManager buyer)
        {
            ClearRows(_buyListContainer, _spawnedBuyRows);

            if (_buyListContainer == null)
            {
                return;
            }

            IReadOnlyList<ShopInventoryItem> catalog = shop.Catalog;

            for (int i = 0; i < catalog.Count; i++)
            {
                ShopInventoryItem entry = catalog[i];

                if (entry?.Item == null)
                {
                    continue;
                }

                int catalogIndex = i;
                string stockText = entry.HasInfiniteStock ? "∞" : entry.Stock.ToString();
                string label = $"{entry.Item.DisplayName}  -  {entry.Price}  (x{stockText})";

                var row = new Button(() => OnBuyClicked(catalogIndex)) { text = label };
                row.AddToClassList("shop-row-button");
                row.SetEnabled(entry.HasStockFor(1) && buyer.GetTotalCount(shop.CurrencyItemId) >= entry.Price);

                _buyListContainer.Add(row);
                _spawnedBuyRows.Add(row);
            }
        }

        private void RebuildSellRows(ShopDefinition shop, InventoryManager buyer)
        {
            ClearRows(_sellListContainer, _spawnedSellRows);

            if (_sellListContainer == null)
            {
                return;
            }

            IReadOnlyList<InventorySlotData> slots = buyer.Slots;

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];

                if (slot.IsEmpty)
                {
                    continue;
                }

                ItemDefinition item = ItemDatabase.GetById(slot.itemId);

                if (item == null || item.ItemId == shop.CurrencyItemId)
                {
                    continue;
                }

                int slotIndex = i;
                int payout = ShopManager.CalculateSellBackPayout(item.Value, 1, shop.SellBackRate);
                string label = $"{item.DisplayName} x{slot.quantity}  -  {payout} ea.";

                var row = new Button(() => OnSellClicked(slotIndex)) { text = label };
                row.AddToClassList("shop-row-button");

                _sellListContainer.Add(row);
                _spawnedSellRows.Add(row);
            }
        }

        private void OnBuyClicked(int catalogIndex)
        {
            _shopManager?.BuyItem(catalogIndex, 1);
        }

        private void OnSellClicked(int slotIndex)
        {
            _shopManager?.SellItem(slotIndex, 1);
        }

        private void OnCloseClicked()
        {
            _shopManager?.CloseShop();
        }

        private static string LocalizeSafe(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return ServiceLocator.TryGet(out LocalizationManager localization) ? localization.Get(key) : $"[{key}]";
        }

        private void ClearRows()
        {
            ClearRows(_buyListContainer, _spawnedBuyRows);
            ClearRows(_sellListContainer, _spawnedSellRows);
        }

        private static void ClearRows(VisualElement container, List<VisualElement> spawned)
        {
            if (container != null)
            {
                foreach (VisualElement element in spawned)
                {
                    container.Remove(element);
                }
            }

            spawned.Clear();
        }

        private void Show()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }
        }

        private void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            ClearRows();
        }
    }
}
