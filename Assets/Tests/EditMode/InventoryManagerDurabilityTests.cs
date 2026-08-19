using System.Collections.Generic;
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
    /// EditMode tests covering that <see cref="InventoryManager"/> preserves an item's existing
    /// durability when it returns to the inventory (e.g. via <c>UnequipSlot</c>) instead of
    /// resetting it to <c>MaxDurability</c>, while brand-new items added via <c>AddItem</c> still
    /// start at full durability. Mirrors the ScriptableObject-via-reflection setup used by
    /// <c>WeaponSystemTests</c>/<c>CraftingManagerTests</c>.
    /// </summary>
    public class InventoryManagerDurabilityTests
    {
        private GameObject _owner;
        private InventoryManager _inventory;
        private ItemDefinition _sword;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _owner = new GameObject("InventoryOwner");
            _inventory = _owner.AddComponent<InventoryManager>();

            _sword = CreateItem("weapon_sword", EquipmentSlot.MainHand, hasDurability: true, maxDurability: 100f);
            RegisterItems(_sword);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            UnregisterItems();
            Object.DestroyImmediate(_owner);
            Object.DestroyImmediate(_sword);
        }

        [Test]
        public void AddItem_NewInstance_StartsAtMaxDurability()
        {
            _inventory.AddItem(_sword, 1);

            InventorySlotData slot = FindSlotWithItem("weapon_sword");
            Assert.IsNotNull(slot);
            Assert.AreEqual(100f, slot.currentDurability);
        }

        [Test]
        public void UnequipSlot_PreservesCurrentDurability_InsteadOfResettingToMax()
        {
            _inventory.AddItem(_sword, 1);
            int slotIndex = FindSlotIndexWithItem("weapon_sword");
            Assert.IsTrue(_inventory.EquipFromSlot(slotIndex));

            // Simulate a weapon worn down to zero durability while equipped.
            InventorySlotData equipped = _inventory.Equipped[EquipmentSlot.MainHand];
            equipped.currentDurability = 0f;

            bool unequipped = _inventory.UnequipSlot(EquipmentSlot.MainHand);

            Assert.IsTrue(unequipped);
            InventorySlotData restored = FindSlotWithItem("weapon_sword");
            Assert.IsNotNull(restored);
            Assert.AreEqual(0f, restored.currentDurability, "Broken weapon should NOT come back at full durability.");
        }

        [Test]
        public void UnequipSlot_PartiallyDamagedWeapon_PreservesExactValue()
        {
            _inventory.AddItem(_sword, 1);
            int slotIndex = FindSlotIndexWithItem("weapon_sword");
            _inventory.EquipFromSlot(slotIndex);

            InventorySlotData equipped = _inventory.Equipped[EquipmentSlot.MainHand];
            equipped.currentDurability = 37.5f;

            _inventory.UnequipSlot(EquipmentSlot.MainHand);

            InventorySlotData restored = FindSlotWithItem("weapon_sword");
            Assert.AreEqual(37.5f, restored.currentDurability);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private InventorySlotData FindSlotWithItem(string itemId)
        {
            foreach (InventorySlotData slot in _inventory.Slots)
            {
                if (!slot.IsEmpty && slot.itemId == itemId)
                {
                    return slot;
                }
            }

            return null;
        }

        private int FindSlotIndexWithItem(string itemId)
        {
            var slots = _inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty && slots[i].itemId == itemId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static ItemDefinition CreateItem(string itemId, EquipmentSlot equipSlot, bool hasDurability, float maxDurability)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);

            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Weapon;
            serialized.FindProperty("_maxStack").intValue = 1;
            serialized.FindProperty("_hasDurability").boolValue = hasDurability;
            serialized.FindProperty("_maxDurability").floatValue = maxDurability;
            serialized.FindProperty("_equipSlot").enumValueIndex = (int)equipSlot;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return item;
        }

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
