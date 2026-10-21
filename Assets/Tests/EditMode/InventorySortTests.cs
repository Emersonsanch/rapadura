using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Mochila's "Organizar" button: the pure
    /// <see cref="InventoryManager.BuildSortedSlots"/> (type → rarity → name ordering, plain stacks
    /// merged up to MaxStack, generated items and durability items never merged, empty slots last,
    /// nothing lost) and <see cref="InventoryManager.SortInventory"/> on a real component. Same
    /// ScriptableObject-via-SerializedObject setup as <c>GeneratedItemPreservationTests</c>.
    /// </summary>
    public class InventorySortTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        private ItemDefinition _sword;
        private ItemDefinition _axe;
        private ItemDefinition _helmet;
        private ItemDefinition _potion;
        private ItemDefinition _herb;
        private ItemDefinition _ore;
        private ItemDefinition _pickaxe;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _sword = Create("sort_sword", "Espada", ItemType.Weapon, ItemRarity.Common, maxStack: 1, hasDurability: true, EquipmentSlot.MainHand);
            _axe = Create("sort_axe", "Machado", ItemType.Weapon, ItemRarity.Rare, maxStack: 1, hasDurability: true, EquipmentSlot.MainHand);
            _helmet = Create("sort_helmet", "Elmo", ItemType.Armor, ItemRarity.Common, maxStack: 1, hasDurability: true, EquipmentSlot.Head);
            _potion = Create("sort_potion", "Poção", ItemType.Consumable, ItemRarity.Common, maxStack: 5, hasDurability: false, EquipmentSlot.None);
            _herb = Create("sort_herb", "Erva", ItemType.Material, ItemRarity.Common, maxStack: 10, hasDurability: false, EquipmentSlot.None);
            _ore = Create("sort_ore", "Minério", ItemType.Material, ItemRarity.Uncommon, maxStack: 10, hasDurability: false, EquipmentSlot.None);
            _pickaxe = Create("sort_pickaxe", "Picareta", ItemType.Tool, ItemRarity.Common, maxStack: 3, hasDurability: true, EquipmentSlot.Tool);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            _items.Clear();
        }

        // ---------------------------------------------------------------
        // Ordering
        // ---------------------------------------------------------------

        [Test]
        public void BuildSortedSlots_OrdersByTypeThenRarityThenName_EmptySlotsLast()
        {
            InventorySlotData[] slots =
            {
                Plain(_herb, 2),
                InventorySlotData.Empty(),
                Plain(_potion, 1),
                Plain(_helmet, 1),
                Plain(_sword, 1),
                Plain(_ore, 1),
                InventorySlotData.Empty(),
                Plain(_axe, 1),
                Plain(_pickaxe, 1),
            };

            InventorySlotData[] sorted = InventoryManager.BuildSortedSlots(slots, Resolve);

            Assert.AreEqual(slots.Length, sorted.Length);
            CollectionAssert.AreEqual(
                new[] { "sort_axe", "sort_sword", "sort_helmet", "sort_pickaxe", "sort_potion", "sort_ore", "sort_herb", null, null },
                sorted.Select(s => s.IsEmpty ? null : s.itemId).ToArray(),
                "Weapon (Rare before Common) → Armor → Tool → Consumable → Material (Uncommon before Common) → empties");
        }

        [Test]
        public void BuildSortedSlots_SameTypeAndRarity_SortsByNameIgnoringCase()
        {
            ItemDefinition apple = Create("sort_apple", "maçã", ItemType.Consumable, ItemRarity.Common, 5, false, EquipmentSlot.None);
            ItemDefinition bread = Create("sort_bread", "Pão", ItemType.Consumable, ItemRarity.Common, 5, false, EquipmentSlot.None);

            InventorySlotData[] sorted = InventoryManager.BuildSortedSlots(new[] { Plain(bread, 1), Plain(_potion, 1), Plain(apple, 1) }, Resolve);

            CollectionAssert.AreEqual(new[] { "sort_apple", "sort_bread", "sort_potion" }, sorted.Select(s => s.itemId).ToArray());
        }

        [Test]
        public void BuildSortedSlots_GeneratedItem_SortsByRolledRarityAndGeneratedName()
        {
            // Base sword is Common, but this roll is Legendary: it must go before the Rare axe.
            InventorySlotData legendary = Generated(_sword, "Zéfiro Lendário", ItemRarity.Legendary);
            InventorySlotData[] sorted = InventoryManager.BuildSortedSlots(new[] { Plain(_sword, 1), Plain(_axe, 1), legendary }, Resolve);

            Assert.AreSame(legendary, sorted[0]);
            Assert.AreEqual("sort_axe", sorted[1].itemId);
            Assert.AreEqual("sort_sword", sorted[2].itemId);
        }

        // ---------------------------------------------------------------
        // Stacking / no-merge
        // ---------------------------------------------------------------

        [Test]
        public void BuildSortedSlots_MergesPlainStacksUpToMaxStack()
        {
            InventorySlotData[] slots =
            {
                Plain(_herb, 4),
                Plain(_potion, 3),
                Plain(_herb, 9),
                Plain(_potion, 4),
                Plain(_herb, 1),
            };

            InventorySlotData[] sorted = InventoryManager.BuildSortedSlots(slots, Resolve);

            // Potions: 7 total, MaxStack 5 → 5 + 2. Herbs: 14 total, MaxStack 10 → 10 + 4.
            Assert.AreEqual("sort_potion", sorted[0].itemId);
            Assert.AreEqual(5, sorted[0].quantity);
            Assert.AreEqual("sort_potion", sorted[1].itemId);
            Assert.AreEqual(2, sorted[1].quantity);
            Assert.AreEqual("sort_herb", sorted[2].itemId);
            Assert.AreEqual(10, sorted[2].quantity);
            Assert.AreEqual("sort_herb", sorted[3].itemId);
            Assert.AreEqual(4, sorted[3].quantity);
            Assert.IsTrue(sorted[4].IsEmpty);
        }

        [Test]
        public void BuildSortedSlots_NeverMergesGeneratedItems()
        {
            InventorySlotData genA = Generated(_herb, "Erva Rúnica", ItemRarity.Epic);
            InventorySlotData genB = Generated(_herb, "Erva Rúnica", ItemRarity.Epic);
            InventorySlotData[] slots = { Plain(_herb, 3), genA, Plain(_herb, 2), genB };

            InventorySlotData[] sorted = InventoryManager.BuildSortedSlots(slots, Resolve);

            Assert.AreEqual(3, sorted.Count(s => !s.IsEmpty), "2 generated + 1 merged plain stack");
            Assert.IsTrue(sorted.Contains(genA), "generated slot objects are carried over untouched");
            Assert.IsTrue(sorted.Contains(genB));
            Assert.AreEqual(1, genA.quantity);
            Assert.AreEqual(1, genB.quantity);
            CollectionAssert.AreEqual(new[] { "affix_test" }, genA.generatedAffixIds);

            InventorySlotData plain = sorted.Single(s => !s.IsEmpty && !s.HasGeneratedData);
            Assert.AreEqual(5, plain.quantity);
        }

        [Test]
        public void BuildSortedSlots_StackableItemsWithDurability_AreNotMerged()
        {
            InventorySlotData worn = Plain(_pickaxe, 1);
            worn.currentDurability = 12f;
            InventorySlotData fresh = Plain(_pickaxe, 1);
            fresh.currentDurability = 100f;

            InventorySlotData[] sorted = InventoryManager.BuildSortedSlots(new[] { worn, fresh }, Resolve);

            Assert.IsTrue(sorted.Contains(worn));
            Assert.IsTrue(sorted.Contains(fresh));
            Assert.AreEqual(12f, worn.currentDurability);
        }

        [Test]
        public void BuildSortedSlots_KeepsTotalQuantityAndUnknownItemsLast()
        {
            var unknown = new InventorySlotData { itemId = "sort_missing_id", quantity = 2 };
            InventorySlotData[] slots = { unknown, Plain(_herb, 6), Plain(_herb, 6), Plain(_sword, 1) };

            InventorySlotData[] sorted = InventoryManager.BuildSortedSlots(slots, Resolve);

            Assert.AreEqual(slots.Sum(s => s.quantity), sorted.Where(s => !s.IsEmpty).Sum(s => s.quantity));
            Assert.AreSame(unknown, sorted.Last(s => !s.IsEmpty), "unknown ids keep their data and go after every known item");
        }

        [Test]
        public void BuildSortedSlots_IsIdempotent()
        {
            InventorySlotData[] slots = { Plain(_herb, 3), Plain(_axe, 1), Plain(_potion, 2), Plain(_herb, 3) };

            InventorySlotData[] once = InventoryManager.BuildSortedSlots(slots, Resolve);
            InventorySlotData[] twice = InventoryManager.BuildSortedSlots(once, Resolve);

            CollectionAssert.AreEqual(once.Select(s => (s.itemId, s.quantity)).ToArray(), twice.Select(s => (s.itemId, s.quantity)).ToArray());
        }

        // ---------------------------------------------------------------
        // InventoryManager.SortInventory
        // ---------------------------------------------------------------

        [Test]
        public void SortInventory_ReordersSlots_PreservesGeneratedData_AndPublishesChange()
        {
            RegisterInDatabase();
            var owner = new GameObject("SortOwner");
            _created.Add(owner);
            InventoryManager inventory = owner.AddComponent<InventoryManager>();

            inventory.AddItem(_herb, 4);
            inventory.AddItem(_potion, 2);
            int slotCount = inventory.Slots.Count;
            int changes = 0;
            EventBus.Subscribe<InventoryChangedEvent>(_ => changes++);

            // Put a generated sword in the last slot by hand (AddGeneratedItem needs an affix asset).
            inventory.MoveOrMergeSlot(0, slotCount - 1);
            InventorySlotData generated = Generated(_sword, "Lâmina Antiga", ItemRarity.Epic);
            CopyInto(inventory.Slots[slotCount - 2], generated);
            changes = 0;

            inventory.SortInventory();

            Assert.AreEqual(1, changes);
            Assert.AreEqual(slotCount, inventory.Slots.Count);
            Assert.AreEqual("sort_sword", inventory.Slots[0].itemId);
            Assert.AreEqual("Lâmina Antiga", inventory.Slots[0].generatedName);
            Assert.AreEqual(ItemRarity.Epic, inventory.Slots[0].generatedRarity);
            CollectionAssert.AreEqual(new[] { "affix_test" }, inventory.Slots[0].generatedAffixIds);
            Assert.AreEqual("sort_potion", inventory.Slots[1].itemId);
            Assert.AreEqual("sort_herb", inventory.Slots[2].itemId);
            Assert.AreEqual(4, inventory.Slots[2].quantity);
            Assert.IsTrue(inventory.Slots[3].IsEmpty);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private ItemDefinition Resolve(string itemId) => itemId != null && _items.TryGetValue(itemId, out ItemDefinition item) ? item : null;

        private static InventorySlotData Plain(ItemDefinition item, int quantity)
        {
            return new InventorySlotData { itemId = item.ItemId, quantity = quantity, currentDurability = item.HasDurability ? item.MaxDurability : 0f };
        }

        private static InventorySlotData Generated(ItemDefinition item, string name, ItemRarity rarity)
        {
            return new InventorySlotData
            {
                itemId = item.ItemId,
                quantity = 1,
                currentDurability = item.HasDurability ? item.MaxDurability : 0f,
                generatedAffixIds = new[] { "affix_test" },
                generatedAffixValues = new[] { 4f },
                generatedName = name,
                generatedRarity = rarity,
            };
        }

        private static void CopyInto(InventorySlotData target, InventorySlotData source)
        {
            target.itemId = source.itemId;
            target.quantity = source.quantity;
            target.currentDurability = source.currentDurability;
            target.CopyGeneratedDataFrom(source);
        }

        private ItemDefinition Create(string itemId, string displayName, ItemType type, ItemRarity rarity, int maxStack, bool hasDurability, EquipmentSlot equipSlot)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);

            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_weight").floatValue = 0f;
            serialized.FindProperty("_hasDurability").boolValue = hasDurability;
            serialized.FindProperty("_maxDurability").floatValue = 100f;
            serialized.FindProperty("_equipSlot").enumValueIndex = (int)equipSlot;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _created.Add(item);
            _items[itemId] = item;
            return item;
        }

        private void RegisterInDatabase()
        {
            FieldInfo field = typeof(ItemDatabase).GetField("_itemsById", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, new Dictionary<string, ItemDefinition>(_items));
        }
    }
}
