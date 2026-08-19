using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 8 "Lojas" backend: <see cref="ShopManager"/> and
    /// <see cref="ShopDefinition"/>. Items and shop definitions are ScriptableObjects with
    /// private [SerializeField] fields, populated via SerializedObject/reflection the same way
    /// <c>CraftingManagerTests</c> populates <c>ItemDefinition</c>/<c>RecipeDefinition</c>.
    /// </summary>
    public class ShopManagerTests
    {
        private GameObject _owner;
        private InventoryManager _inventory;
        private ShopManager _shop;

        private ItemDefinition _gold;
        private ItemDefinition _wood;
        private ItemDefinition _potion;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _owner = new GameObject("ShopBuyer");
            _inventory = _owner.AddComponent<InventoryManager>();
            _shop = new ShopManager();
            _shop.Initialize();

            _gold = CreateItem("item_gold", 999, value: 1);
            _wood = CreateItem("item_wood", 99, value: 2);
            _potion = CreateItem("item_potion", 99, value: 10);

            RegisterItems(_gold, _wood, _potion);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            _shop.Shutdown();
            UnregisterItems();
            Object.DestroyImmediate(_owner);
            Object.DestroyImmediate(_gold);
            Object.DestroyImmediate(_wood);
            Object.DestroyImmediate(_potion);
        }

        [Test]
        public void BuyItem_WithEnoughGoldAndStock_TransfersItemAndDeductsGold()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10) });
            _inventory.AddItem(_gold, 100);
            _shop.OpenShop(shop, _inventory);

            bool bought = _shop.BuyItem(0, 3);

            Assert.IsTrue(bought);
            Assert.AreEqual(3, _inventory.GetTotalCount("item_wood"));
            Assert.AreEqual(100 - 3 * _wood.Value, _inventory.GetTotalCount("item_gold"));
        }

        [Test]
        public void BuyItem_WithoutEnoughGold_FailsAndChangesNothing()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_potion, 0, 10) });
            _inventory.AddItem(_gold, 5); // potion costs 10 each, not enough

            _shop.OpenShop(shop, _inventory);
            bool bought = _shop.BuyItem(0, 1);

            Assert.IsFalse(bought);
            Assert.AreEqual(0, _inventory.GetTotalCount("item_potion"));
            Assert.AreEqual(5, _inventory.GetTotalCount("item_gold"));
        }

        [Test]
        public void BuyItem_WithDepletedStock_Fails()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 2) });
            _inventory.AddItem(_gold, 1000);
            _shop.OpenShop(shop, _inventory);

            bool firstBuy = _shop.BuyItem(0, 2);
            bool secondBuy = _shop.BuyItem(0, 1);

            Assert.IsTrue(firstBuy);
            Assert.IsFalse(secondBuy);
            Assert.AreEqual(2, _inventory.GetTotalCount("item_wood"));
        }

        [Test]
        public void BuyItem_InfiniteStock_NeverDepletes()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, -1) });
            _inventory.AddItem(_gold, 1000);
            _shop.OpenShop(shop, _inventory);

            for (int i = 0; i < 20; i++)
            {
                Assert.IsTrue(_shop.BuyItem(0, 1));
            }

            Assert.AreEqual(20, _inventory.GetTotalCount("item_wood"));
        }

        [Test]
        public void SellItem_PaysSellBackRateOfBaseValue()
        {
            ShopDefinition shop = CreateShop("shop_test", new (ItemDefinition, int, int)[0], sellBackRate: 0.5f);
            _inventory.AddItem(_potion, 4);
            _shop.OpenShop(shop, _inventory);

            int slotIndex = FindSlotIndex("item_potion");
            bool sold = _shop.SellItem(slotIndex, 4);

            Assert.IsTrue(sold);
            Assert.AreEqual(0, _inventory.GetTotalCount("item_potion"));
            // 10 (value) * 4 (qty) * 0.5 (rate) = 20 gold
            Assert.AreEqual(20, _inventory.GetTotalCount("item_gold"));
        }

        [Test]
        public void CalculateSellBackPayout_MatchesRateFormula()
        {
            Assert.AreEqual(5, ShopManager.CalculateSellBackPayout(baseValue: 10, quantity: 1, sellBackRate: 0.5f));
            Assert.AreEqual(15, ShopManager.CalculateSellBackPayout(baseValue: 10, quantity: 3, sellBackRate: 0.5f));
            Assert.AreEqual(0, ShopManager.CalculateSellBackPayout(baseValue: 10, quantity: 1, sellBackRate: 0f));
            Assert.AreEqual(10, ShopManager.CalculateSellBackPayout(baseValue: 10, quantity: 1, sellBackRate: 1f));
        }

        [Test]
        public void CloseShop_ClearsActiveSessionAndPreventsFurtherTransactions()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10) });
            _inventory.AddItem(_gold, 100);
            _shop.OpenShop(shop, _inventory);

            _shop.CloseShop();
            bool bought = _shop.BuyItem(0, 1);

            Assert.IsFalse(_shop.IsOpen);
            Assert.IsFalse(bought);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private int FindSlotIndex(string itemId)
        {
            for (int i = 0; i < _inventory.Slots.Count; i++)
            {
                if (_inventory.Slots[i].itemId == itemId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static ItemDefinition CreateItem(string itemId, int maxStack, int value)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_value").intValue = value;
            serialized.FindProperty("_weight").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        /// <summary>Builds a ShopDefinition with a catalog of (item, priceOverride, stock) entries.</summary>
        private static ShopDefinition CreateShop(string shopId, (ItemDefinition item, int priceOverride, int stock)[] entries, float sellBackRate = 0.5f)
        {
            var shop = ScriptableObject.CreateInstance<ShopDefinition>();
            SetPrivateField(shop, "_shopId", shopId);
            SetPrivateField(shop, "_displayNameKey", "shop.test.name");
            SetPrivateField(shop, "_currencyItemId", "item_gold");
            SetPrivateField(shop, "_sellBackRate", sellBackRate);

            var catalog = new System.Collections.Generic.List<ShopInventoryItem>();

            foreach ((ItemDefinition item, int priceOverride, int stock) in entries)
            {
                var entry = new ShopInventoryItem();
                SetPrivateField(entry, "_item", item);
                SetPrivateField(entry, "_priceOverride", priceOverride);
                SetPrivateField(entry, "_stock", stock);
                catalog.Add(entry);
            }

            SetPrivateField(shop, "_catalog", catalog);

            return shop;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Injects items directly into ItemDatabase's static lookup via reflection, same approach
        /// as InventoryManagerDurabilityTests — ShopManager resolves currency/sold items through
        /// ItemDatabase.GetById (the project's only sanctioned string-id -> ItemDefinition path),
        /// and these test items are transient CreateInstance objects, not real Resources assets.
        /// </summary>
        private static void RegisterItems(params ItemDefinition[] items)
        {
            FieldInfo field = typeof(ItemDatabase).GetField("_itemsById", BindingFlags.NonPublic | BindingFlags.Static);
            var map = new Dictionary<string, ItemDefinition>();

            foreach (ItemDefinition item in items)
            {
                map[item.ItemId] = item;
            }

            field.SetValue(null, map);
        }

        private static void UnregisterItems()
        {
            ItemDatabase.Invalidate();
        }
    }
}
