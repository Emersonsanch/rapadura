using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests proving that a procedurally-generated item (rolled affixes, generated name,
    /// generated rarity on <see cref="InventorySlotData"/>) survives every inventory operation that
    /// used to silently turn it back into a plain base item: equip → unequip, equipping over an
    /// occupied slot (swap), save/load of equipped gear (plus backward compatibility with old
    /// saves), re-applying affix buffs on load, move/merge, split, stacking and remove-by-id.
    /// Also guards PlayerControls.inputactions against duplicated action/binding GUIDs (saved
    /// rebinds are matched by binding id). Same ScriptableObject-via-reflection setup as
    /// <c>InventoryManagerDurabilityTests</c>; save slot follows the <c>SaveManagerTests</c> pattern.
    /// </summary>
    public class GeneratedItemPreservationTests
    {
        private const int TestSlot = 90031;

        private readonly List<Object> _created = new List<Object>();
        private GameObject _owner;
        private InventoryManager _inventory;
        private ItemDefinition _sword;
        private ItemDefinition _herb;
        private ItemAffixDefinition _affix;

        private static string SlotPath(int slot) => Path.Combine(Application.persistentDataPath, "Saves", $"slot_{slot}.json");

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            DeleteSlotFiles();

            _owner = Track(new GameObject("InventoryOwner"));
            _inventory = _owner.AddComponent<InventoryManager>();

            _sword = Track(CreateItem("gen_test_sword", ItemType.Weapon, EquipmentSlot.MainHand, maxStack: 1, hasDurability: true));
            _herb = Track(CreateItem("gen_test_herb", ItemType.Material, EquipmentSlot.None, maxStack: 10, hasDurability: false));
            RegisterItems(_sword, _herb);

            _affix = Track(CreateAffix("gen_test_affix_brutal", StatType.AttackDamage));
            RegisterAffixes(_affix);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            DeleteSlotFiles();
            ItemDatabase.Invalidate();
            AffixDatabase.Invalidate();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        // ---------------------------------------------------------------
        // Equip / Unequip
        // ---------------------------------------------------------------

        [Test]
        public void UnequipSlot_GeneratedItem_KeepsAffixesNameAndRarity()
        {
            Assert.IsTrue(_inventory.AddGeneratedItem(CreateGenerated("Espada Brutal", ItemRarity.Epic, 7.5f)));
            Assert.IsTrue(_inventory.EquipFromSlot(FindSlotIndex(_sword.ItemId)));
            _inventory.Equipped[EquipmentSlot.MainHand].currentDurability = 42f;

            Assert.IsTrue(_inventory.UnequipSlot(EquipmentSlot.MainHand));

            InventorySlotData restored = _inventory.Slots[FindSlotIndex(_sword.ItemId)];
            AssertGenerated(restored, "Espada Brutal", ItemRarity.Epic, 7.5f);
            Assert.AreEqual(42f, restored.currentDurability);
        }

        [Test]
        public void EquipFromSlot_OverOccupiedSlot_ReturnsPreviousGeneratedItemIntact()
        {
            _inventory.AddGeneratedItem(CreateGenerated("Primeira", ItemRarity.Legendary, 3f));
            _inventory.EquipFromSlot(FindSlotIndex(_sword.ItemId));
            _inventory.AddGeneratedItem(CreateGenerated("Segunda", ItemRarity.Rare, 9f));

            Assert.IsTrue(_inventory.EquipFromSlot(FindSlotIndex(_sword.ItemId)));

            AssertGenerated(_inventory.Equipped[EquipmentSlot.MainHand], "Segunda", ItemRarity.Rare, 9f);
            AssertGenerated(_inventory.Slots[FindSlotIndex(_sword.ItemId)], "Primeira", ItemRarity.Legendary, 3f);
        }

        [Test]
        public void EquipFromSlot_SwapWithFullInventory_DoesNotDestroyPreviouslyEquippedItem()
        {
            _inventory.AddGeneratedItem(CreateGenerated("Primeira", ItemRarity.Legendary, 3f));
            _inventory.EquipFromSlot(FindSlotIndex(_sword.ItemId));
            _inventory.AddGeneratedItem(CreateGenerated("Segunda", ItemRarity.Rare, 9f));

            // Fill every remaining slot so the only free slot during the swap is the one the
            // new item vacates. Used to: UnequipSlot failed silently, old item overwritten.
            ItemDefinition filler = Track(CreateItem("gen_test_filler", ItemType.Material, EquipmentSlot.None, maxStack: 1, hasDurability: false));
            RegisterItems(_sword, _herb, filler);
            _inventory.AddItem(filler, _inventory.Slots.Count);
            Assert.AreEqual(-1, FindEmptySlotIndex(), "Precondition: inventory must be full.");

            Assert.IsTrue(_inventory.EquipFromSlot(FindSlotIndex(_sword.ItemId)));

            AssertGenerated(_inventory.Equipped[EquipmentSlot.MainHand], "Segunda", ItemRarity.Rare, 9f);
            AssertGenerated(_inventory.Slots[FindSlotIndex(_sword.ItemId)], "Primeira", ItemRarity.Legendary, 3f);
        }

        // ---------------------------------------------------------------
        // Save / Load
        // ---------------------------------------------------------------

        [Test]
        public void SaveAndLoad_EquippedGeneratedItem_KeepsAffixesNameAndRarity()
        {
            _inventory.AddGeneratedItem(CreateGenerated("Espada Salva", ItemRarity.Epic, 5f));
            _inventory.EquipFromSlot(FindSlotIndex(_sword.ItemId));

            var saveManager = new SaveManager();
            saveManager.Initialize();
            saveManager.Register(_inventory);
            saveManager.Save(TestSlot);
            saveManager.Unregister(_inventory);

            GameObject otherOwner = Track(new GameObject("LoadedInventoryOwner"));
            var loaded = otherOwner.AddComponent<InventoryManager>();
            saveManager.Register(loaded);

            Assert.IsTrue(saveManager.Load(TestSlot));
            AssertGenerated(loaded.Equipped[EquipmentSlot.MainHand], "Espada Salva", ItemRarity.Epic, 5f);

            // And it still survives being unequipped after the load.
            Assert.IsTrue(loaded.UnequipSlot(EquipmentSlot.MainHand));
            InventorySlotData back = null;
            foreach (InventorySlotData slot in loaded.Slots)
            {
                if (!slot.IsEmpty && slot.itemId == _sword.ItemId)
                {
                    back = slot;
                }
            }

            AssertGenerated(back, "Espada Salva", ItemRarity.Epic, 5f);
            saveManager.Shutdown();
        }

        [Test]
        public void RestoreState_OldSaveWithoutGeneratedFields_LoadsEquippedItemAsPlain()
        {
            // Shape written by builds before EquippedEntry carried generated data.
            string oldJson = "{\"slots\":[],\"equipped\":[{\"slot\":" + (int)EquipmentSlot.MainHand +
                             ",\"itemId\":\"gen_test_sword\",\"durability\":12.0}]}";
            var state = JsonUtility.FromJson<InventorySaveState>(oldJson);

            _inventory.RestoreState(state);

            InventorySlotData equipped = _inventory.Equipped[EquipmentSlot.MainHand];
            Assert.AreEqual("gen_test_sword", equipped.itemId);
            Assert.AreEqual(12f, equipped.currentDurability);
            Assert.IsFalse(equipped.HasGeneratedData);

            Assert.IsTrue(_inventory.UnequipSlot(EquipmentSlot.MainHand));
            Assert.AreEqual(1, _inventory.GetTotalCount("gen_test_sword"));
        }

        [Test]
        public void RestoreState_EquippedGeneratedItem_ReappliesAffixBuffs()
        {
            BuffController buffs = _owner.AddComponent<BuffController>();
            SetPrivateField(_inventory, "_buffController", buffs);

            _inventory.AddGeneratedItem(CreateGenerated("Espada Brutal", ItemRarity.Epic, 7.5f));
            _inventory.EquipFromSlot(FindSlotIndex(_sword.ItemId));
            Assert.AreEqual(17.5f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f);

            string json = JsonUtility.ToJson(_inventory.CaptureState());
            _inventory.RestoreState(JsonUtility.FromJson<InventorySaveState>(json));

            Assert.AreEqual(17.5f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f,
                "Affix bonus of the equipped generated item must be re-applied after a load.");
        }

        // ---------------------------------------------------------------
        // Move / Merge / Split / Stack / RemoveById
        // ---------------------------------------------------------------

        [Test]
        public void MoveOrMergeSlot_GeneratedOntoPlainOfSameId_SwapsInsteadOfMerging()
        {
            _inventory.AddItem(_herb, 3);
            int plainIndex = FindSlotIndex(_herb.ItemId);
            _inventory.AddGeneratedItem(new GeneratedItemInstance(_herb, ItemRarity.Rare, new[] { new RolledAffix(_affix, 2f) }, "Erva Rara"));
            int generatedIndex = FindGeneratedSlotIndex();

            _inventory.MoveOrMergeSlot(generatedIndex, plainIndex);

            AssertGenerated(_inventory.Slots[plainIndex], "Erva Rara", ItemRarity.Rare, 2f);
            Assert.AreEqual(1, _inventory.Slots[plainIndex].quantity);
            Assert.AreEqual(3, _inventory.Slots[generatedIndex].quantity);
            Assert.IsFalse(_inventory.Slots[generatedIndex].HasGeneratedData);
        }

        [Test]
        public void AddItem_PlainStackableItem_DoesNotStackIntoGeneratedSlot()
        {
            _inventory.AddGeneratedItem(new GeneratedItemInstance(_herb, ItemRarity.Rare, new[] { new RolledAffix(_affix, 2f) }, "Erva Rara"));
            int generatedIndex = FindGeneratedSlotIndex();

            _inventory.AddItem(_herb, 4);

            Assert.AreEqual(1, _inventory.Slots[generatedIndex].quantity);
            Assert.AreEqual(5, _inventory.GetTotalCount(_herb.ItemId));
        }

        [Test]
        public void SplitStack_CopiesGeneratedDataToNewStack()
        {
            _inventory.AddGeneratedItem(new GeneratedItemInstance(_herb, ItemRarity.Rare, new[] { new RolledAffix(_affix, 2f) }, "Erva Rara"));
            int generatedIndex = FindGeneratedSlotIndex();
            _inventory.Slots[generatedIndex].quantity = 2;

            Assert.IsTrue(_inventory.SplitStack(generatedIndex, 1));

            int splitCount = 0;
            foreach (InventorySlotData slot in _inventory.Slots)
            {
                if (!slot.IsEmpty)
                {
                    AssertGenerated(slot, "Erva Rara", ItemRarity.Rare, 2f);
                    splitCount++;
                }
            }

            Assert.AreEqual(2, splitCount);
            Assert.AreNotSame(_inventory.Slots[0].generatedAffixIds, _inventory.Slots[1].generatedAffixIds,
                "Split stacks must not share the same affix array instance.");
        }

        [Test]
        public void RemoveById_ConsumesPlainItemsBeforeGeneratedOnes()
        {
            _inventory.AddGeneratedItem(CreateGenerated("Espada Rara", ItemRarity.Epic, 4f));
            _inventory.AddItem(_sword, 1);

            Assert.IsTrue(_inventory.RemoveById(_sword.ItemId, 1));

            Assert.AreEqual(1, _inventory.GetTotalCount(_sword.ItemId));
            AssertGenerated(_inventory.Slots[FindSlotIndex(_sword.ItemId)], "Espada Rara", ItemRarity.Epic, 4f);
        }

        // ---------------------------------------------------------------
        // Input actions asset
        // ---------------------------------------------------------------

        [Test]
        public void PlayerControls_AllActionAndBindingIdsAreUnique()
        {
            string json = File.ReadAllText(Path.Combine("Assets", "InputActions", "PlayerControls.inputactions"));
            InputActionAsset asset = Track(InputActionAsset.FromJson(json));
            var seen = new HashSet<string>();

            foreach (InputActionMap map in asset.actionMaps)
            {
                Assert.IsTrue(seen.Add(map.id.ToString()), $"Duplicate map id {map.id} ({map.name}).");

                foreach (InputAction action in map.actions)
                {
                    Assert.IsTrue(seen.Add(action.id.ToString()), $"Duplicate action id {action.id} ({action.name}).");
                }

                foreach (InputBinding binding in map.bindings)
                {
                    Assert.IsTrue(seen.Add(binding.id.ToString()),
                        $"Duplicate binding id {binding.id} ({binding.action} {binding.path}) — saved rebinds match by binding id.");
                }
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private T Track<T>(T obj) where T : Object
        {
            _created.Add(obj);
            return obj;
        }

        private GeneratedItemInstance CreateGenerated(string name, ItemRarity rarity, float affixValue)
        {
            return new GeneratedItemInstance(_sword, rarity, new[] { new RolledAffix(_affix, affixValue) }, name);
        }

        private void AssertGenerated(InventorySlotData slot, string expectedName, ItemRarity expectedRarity, float expectedAffixValue)
        {
            Assert.IsNotNull(slot);
            Assert.AreEqual(expectedName, slot.generatedName);
            Assert.AreEqual(expectedRarity, slot.generatedRarity);
            Assert.IsTrue(slot.HasGeneratedAffixes);
            CollectionAssert.AreEqual(new[] { _affix.AffixId }, slot.generatedAffixIds);
            CollectionAssert.AreEqual(new[] { expectedAffixValue }, slot.generatedAffixValues);
        }

        private int FindSlotIndex(string itemId)
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

        private int FindGeneratedSlotIndex()
        {
            var slots = _inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty && slots[i].HasGeneratedData)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindEmptySlotIndex()
        {
            var slots = _inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void DeleteSlotFiles()
        {
            string path = SlotPath(TestSlot);
            foreach (string file in new[] { path, path + ".bak", path + ".tmp" })
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private static ItemDefinition CreateItem(string itemId, ItemType type, EquipmentSlot equipSlot, int maxStack, bool hasDurability)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);

            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_hasDurability").boolValue = hasDurability;
            serialized.FindProperty("_maxDurability").floatValue = 100f;
            serialized.FindProperty("_equipSlot").enumValueIndex = (int)equipSlot;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return item;
        }

        private static ItemAffixDefinition CreateAffix(string affixId, StatType stat)
        {
            var affix = ScriptableObject.CreateInstance<ItemAffixDefinition>();
            var serialized = new SerializedObject(affix);

            serialized.FindProperty("_affixId").stringValue = affixId;
            serialized.FindProperty("_affectedStat").enumValueIndex = (int)stat;
            serialized.FindProperty("_application").enumValueIndex = (int)ModifierApplication.Flat;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return affix;
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

        private static void RegisterAffixes(params ItemAffixDefinition[] affixes)
        {
            FieldInfo field = typeof(AffixDatabase).GetField("_affixesById", BindingFlags.NonPublic | BindingFlags.Static);
            var map = new Dictionary<string, ItemAffixDefinition>();

            foreach (ItemAffixDefinition affix in affixes)
            {
                map[affix.AffixId] = affix;
            }

            field.SetValue(null, map);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
