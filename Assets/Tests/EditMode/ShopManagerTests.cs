using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using Rapadura.Save;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 8 "Lojas" backend: <see cref="ShopManager"/> and
    /// <see cref="ShopDefinition"/>. Items and shop definitions are ScriptableObjects with
    /// private [SerializeField] fields, populated via SerializedObject/reflection the same way
    /// <c>CraftingManagerTests</c> populates <c>ItemDefinition</c>/<c>RecipeDefinition</c>.
    /// Also covers the runtime stock table: purchases never write into the ShopDefinition asset,
    /// and stock is saved ("Shops"), reset per session and restocked at in-game dawn.
    /// </summary>
    public class ShopManagerTests
    {
        private const int TestSlot = 90051;
        private GameObject _owner;
        private InventoryManager _inventory;
        private ShopManager _shop;

        private ItemDefinition _gold;
        private ItemDefinition _wood;
        private ItemDefinition _potion;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
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
            TestSaveRoot.Exit();
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
        // Runtime stock (never in the asset)
        // ---------------------------------------------------------------

        [Test]
        public void BuyAndSell_NeverMutateTheShopDefinition()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10), (_potion, 0, 3) });
            string before = EditorJsonUtility.ToJson(shop);
            _inventory.AddItem(_gold, 1000);
            _shop.OpenShop(shop, _inventory);

            Assert.IsTrue(_shop.BuyItem(0, 4));
            Assert.IsTrue(_shop.BuyItem(1, 3));
            Assert.IsTrue(_shop.SellItem(FindSlotIndex("item_wood"), 1));

            Assert.AreEqual(before, EditorJsonUtility.ToJson(shop), "Purchases/sales must not write into the ScriptableObject.");
            Assert.AreEqual(10, shop.Catalog[0].InitialStock);
            Assert.AreEqual(7, _shop.GetStock(shop, 0));
            Assert.AreEqual(0, _shop.GetStock(shop, 1));
        }

        [Test]
        public void Buying_FromTheSeededGeneralStore_LeavesTheAssetOnDiskUntouched()
        {
            var store = Resources.Load<ShopDefinition>("Shops/shop_general_store");
            Assume.That(store, Is.Not.Null, "Seeded shop asset missing.");

            int finiteIndex = -1;

            for (int i = 0; i < store.Catalog.Count; i++)
            {
                if (store.Catalog[i]?.Item != null && !store.Catalog[i].HasInfiniteStock && store.Catalog[i].InitialStock > 0)
                {
                    finiteIndex = i;
                    break;
                }
            }

            Assume.That(finiteIndex, Is.GreaterThanOrEqualTo(0), "Seeded shop has no finite-stock entry.");

            ShopInventoryItem entry = store.Catalog[finiteIndex];
            RegisterItems(_gold, _wood, _potion, entry.Item);
            string before = EditorJsonUtility.ToJson(store);
            bool wasDirty = EditorUtility.IsDirty(store);

            _inventory.AddItem(_gold, 999);
            _shop.OpenShop(store, _inventory);
            Assume.That(_shop.BuyItem(finiteIndex, 1), "Could not buy from the seeded store.");

            Assert.AreEqual(entry.InitialStock - 1, _shop.GetStock(store, finiteIndex));
            Assert.AreEqual(before, EditorJsonUtility.ToJson(store));
            Assert.AreEqual(wasDirty, EditorUtility.IsDirty(store), "The asset must not be marked dirty (it would be written to disk).");
        }

        [Test]
        public void SellingACatalogItem_RaisesItsLiveStock()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10) });
            _inventory.AddItem(_wood, 2);
            _shop.OpenShop(shop, _inventory);

            Assert.IsTrue(_shop.SellItem(FindSlotIndex("item_wood"), 2));

            Assert.AreEqual(12, _shop.GetStock(shop, 0));
            Assert.AreEqual(10, shop.Catalog[0].InitialStock);
        }

        [Test]
        public void Stock_SaveAndLoad_RoundTripsThroughASaveSlot()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10), (_potion, 0, 5) });
            _inventory.AddItem(_gold, 1000);
            _shop.OpenShop(shop, _inventory);
            Assert.IsTrue(_shop.BuyItem(0, 3));
            _shop.CloseShop();
            _shop.Restock(1); // no-op for the day number, just to check it is kept below
            Assert.IsTrue(OpenAndBuy(shop, 1, 2));

            var saveManager = new SaveManager();
            saveManager.Initialize();
            saveManager.Register(_shop);
            saveManager.Save(TestSlot);

            var restored = new ShopManager();
            restored.Initialize();

            try
            {
                var loader = new SaveManager();
                loader.Initialize();
                loader.Register(restored);
                Assert.IsTrue(loader.Load(TestSlot));

                Assert.AreEqual(10, restored.GetStock(shop, 0), "The restock above refilled the wood before the save.");
                Assert.AreEqual(3, restored.GetStock(shop, 1));
                Assert.AreEqual(ShopManager.Key, restored.SaveKey);
            }
            finally
            {
                restored.Shutdown();
            }
        }

        [Test]
        public void RestoreState_ReplacesLiveStock_AndKeepsTheRestockDay()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10) });
            Assert.IsTrue(OpenAndBuy(shop, 0, 4));
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Night, TimeOfDayPhase.Dawn, 3));
            Assert.IsTrue(OpenAndBuy(shop, 0, 1));
            object state = _shop.CaptureState();

            var restored = new ShopManager();
            restored.RestoreState(state);

            Assert.AreEqual(9, restored.GetStock(shop, 0));
            Assert.AreEqual(3, restored.LastRestockDay);
        }

        [Test]
        public void ResetForNewSession_BringsBackAuthoredStock()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10) });
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Night, TimeOfDayPhase.Dawn, 4));
            Assert.IsTrue(OpenAndBuy(shop, 0, 10));
            Assume.That(_shop.GetStock(shop, 0), Is.EqualTo(0));
            Assume.That(_shop.LastRestockDay, Is.EqualTo(4));

            _shop.ResetForNewSession();

            Assert.IsFalse(_shop.IsOpen);
            Assert.AreEqual(10, _shop.GetStock(shop, 0));
            Assert.AreEqual(1, _shop.LastRestockDay);
        }

        [Test]
        public void Dawn_OfANewDay_Restocks_OnlyOncePerDay()
        {
            ShopDefinition shop = CreateShop("shop_test", new[] { (_wood, 0, 10) });
            _inventory.AddItem(_wood, 5);
            Assert.IsTrue(OpenAndBuy(shop, 0, 6));
            int restockEvents = 0;
            EventBus.Subscribe<ShopRestockedEvent>(_ => restockEvents++);

            // Day phase of the same day, and dawn of day 1 (already the session's start day): nothing.
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Dawn, TimeOfDayPhase.Day, 1));
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Night, TimeOfDayPhase.Dawn, 1));
            Assert.AreEqual(4, _shop.GetStock(shop, 0));

            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Night, TimeOfDayPhase.Dawn, 2));
            Assert.AreEqual(10, _shop.GetStock(shop, 0));
            Assert.AreEqual(1, restockEvents);

            // A save restored at dawn republishes the phase change for the same day: no second restock.
            Assert.IsTrue(_shop.BuyItem(0, 2));
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Day, TimeOfDayPhase.Dawn, 2));
            Assert.AreEqual(8, _shop.GetStock(shop, 0));

            // Surplus the player sold to the shop is gone by the next dawn.
            Assert.IsTrue(_shop.SellItem(FindSlotIndex("item_wood"), 5));
            Assert.AreEqual(13, _shop.GetStock(shop, 0));
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Night, TimeOfDayPhase.Dawn, 3));
            Assert.AreEqual(10, _shop.GetStock(shop, 0));
            Assert.AreEqual(2, restockEvents);
        }

        [Test]
        public void TwoShops_SellingTheSameItem_KeepSeparateStock()
        {
            ShopDefinition a = CreateShop("shop_a", new[] { (_wood, 0, 10) });
            ShopDefinition b = CreateShop("shop_b", new[] { (_wood, 0, 10) });

            Assert.IsTrue(OpenAndBuy(a, 0, 7));

            Assert.AreEqual(3, _shop.GetStock(a, 0));
            Assert.AreEqual(10, _shop.GetStock(b, 0));
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        /// <summary>Opens <paramref name="shop"/> (closing any open one), tops up gold and buys; leaves the shop open.</summary>
        private bool OpenAndBuy(ShopDefinition shop, int catalogIndex, int quantity)
        {
            _shop.CloseShop();
            _inventory.AddItem(_gold, 500);
            _shop.OpenShop(shop, _inventory);
            return _shop.BuyItem(catalogIndex, quantity);
        }

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
