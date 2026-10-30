using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Durability;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Equipment durability and the ferreiro's repair (FASE 16, Docs/Balance/README.md "Durabilidade e conserto"): the pure
    /// <see cref="DurabilityRules"/> (wear, broken, repair cost by value × rarity × refine), <see cref="RepairService"/> on a
    /// live bag (wear by attack/hit/death, a broken piece gives no stats but stays equipped, repair paid in gold) and the
    /// save (the durability already travelled, old saves load unchanged).
    /// </summary>
    public class DurabilityTests
    {
        // ------------------------------------------------------------------ pure rules

        [Test]
        public void Wear_NeverGoesBelowZero_AndBrokenOnlyForTrackedItems()
        {
            Assert.AreEqual(9.9f, DurabilityRules.Wear(10f, 0.1f), 1e-5f);
            Assert.AreEqual(0f, DurabilityRules.Wear(0.05f, 1f));
            Assert.AreEqual(10f, DurabilityRules.Wear(10f, -5f), "negative wear never repairs");
            Assert.IsTrue(DurabilityRules.IsBroken(true, 120f, 0f));
            Assert.IsFalse(DurabilityRules.IsBroken(false, 120f, 0f), "untracked items never break");
            Assert.IsFalse(DurabilityRules.IsBroken(true, 0f, 0f));
            Assert.AreEqual(12f, DurabilityRules.DeathWear(120f), 1e-5f, "10% per death");
            Assert.IsTrue(DurabilityRules.IsWorn(100f, 20f));
            Assert.IsFalse(DurabilityRules.IsWorn(100f, 30f));
        }

        [Test]
        public void RepairCost_GrowsWithRarityRefineAndOptions_AndScalesWithWhatIsMissing()
        {
            int common = DurabilityRules.FullRepairCost(100, ItemRarity.Common, 0, false);
            Assert.AreEqual(25, common, "a full repair is 25% of the value");
            Assert.Greater(DurabilityRules.FullRepairCost(100, ItemRarity.Epic, 0, false), common);
            Assert.AreEqual(50, DurabilityRules.FullRepairCost(100, ItemRarity.Common, 10, false), "+10 doubles it");
            Assert.Greater(DurabilityRules.FullRepairCost(100, ItemRarity.Common, 0, true), common);

            Assert.AreEqual(0, DurabilityRules.RepairCost(100, ItemRarity.Common, 0, false, 100f, 100f), "nothing missing");
            Assert.AreEqual(13, DurabilityRules.RepairCost(100, ItemRarity.Common, 0, false, 100f, 50f), "half: ceil(12.5)");
            Assert.AreEqual(1, DurabilityRules.RepairCost(100, ItemRarity.Common, 0, false, 100f, 99.9f), "at least 1 gold");
            Assert.AreEqual(32, DurabilityRules.RepairCost(100, ItemRarity.Common, 0, false, 100f, 0f), "broken: ×1.25");
            for (ItemRarity rarity = ItemRarity.Common; rarity < ItemRarity.Legendary; rarity++)
            {
                Assert.Less(DurabilityRules.RarityMultiplier(rarity), DurabilityRules.RarityMultiplier(rarity + 1));
            }
        }

        // ------------------------------------------------------------------ live bag

        private GameObject _owner;
        private InventoryManager _bag;
        private BuffController _buffs;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            _owner = new GameObject("DurabilityBag");
            _buffs = _owner.AddComponent<BuffController>();
            _bag = _owner.AddComponent<InventoryManager>();
            WireBuffs(_bag, _buffs);
        }

        /// <summary>EditMode does not run Awake: hand the bag its BuffController like the game does.</summary>
        private static void WireBuffs(InventoryManager bag, BuffController buffs) =>
            typeof(InventoryManager).GetField("_buffController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bag, buffs);

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_owner);
        }

        private void Equip(string itemId)
        {
            _bag.AddItem(ItemDatabase.GetById(itemId), 1);
            int index = _bag.Slots.Select((s, i) => (s, i)).First(p => p.s.itemId == itemId).i;
            Assert.IsTrue(_bag.EquipFromSlot(index), itemId);
        }

        [Test]
        public void Wear_WeaponByAttack_ArmourByHit_EverythingByDeath()
        {
            Equip("item_iron_sword");
            Equip("item_leather_chest");
            InventorySlotData sword = _bag.Equipped[EquipmentSlot.MainHand];
            InventorySlotData chest = _bag.Equipped[EquipmentSlot.Chest];
            Assert.AreEqual(120f, sword.currentDurability);
            Assert.AreEqual(90f, chest.currentDurability);

            RepairService.WearWeapon(_bag);
            Assert.AreEqual(120f - DurabilityRules.WeaponWearPerAttack, sword.currentDurability, 1e-4f);
            Assert.AreEqual(90f, chest.currentDurability, "attacking does not wear the armour");

            RepairService.WearArmour(_bag);
            Assert.AreEqual(90f - DurabilityRules.ArmourWearPerHit, chest.currentDurability, 1e-4f);

            float swordBefore = sword.currentDurability;
            RepairService.WearOnDeath(_bag);
            Assert.AreEqual(swordBefore - 12f, sword.currentDurability, 1e-4f);
            Assert.AreEqual(90f - DurabilityRules.ArmourWearPerHit - 9f, chest.currentDurability, 1e-4f);
        }

        [Test]
        public void Tracker_WearsFromTheGameEvents_OfItsOwnObject()
        {
            Equip("item_iron_sword");
            EquipmentWearTracker tracker = _owner.AddComponent<EquipmentWearTracker>();
            Call(tracker, "Awake");
            Call(tracker, "OnEnable");
            InventorySlotData sword = _bag.Equipped[EquipmentSlot.MainHand];

            EventBus.Publish(new PlayerAttackPerformedEvent(new GameObject("SomeoneElse")));
            Assert.AreEqual(120f, sword.currentDurability, "another attacker does not wear our sword");
            EventBus.Publish(new PlayerAttackPerformedEvent(_owner));
            Assert.Less(sword.currentDurability, 120f);
            Call(tracker, "OnDisable");
            Object.DestroyImmediate(GameObject.Find("SomeoneElse"));
        }

        private static void Call(object target, string method) =>
            target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);

        [Test]
        public void Broken_GivesNoStats_StaysEquipped_AndTheRepairBringsThemBack()
        {
            Equip("item_iron_sword");
            InventorySlotData sword = _bag.Equipped[EquipmentSlot.MainHand];
            sword.excellentMask = (int)Rapadura.Gameplay.Items.Options.ExcellentOption.WeaponDamage;
            _bag.NotifyItemDataChanged(EquipmentSlot.MainHand);
            float working = _buffs.GetPercentTotal(StatType.AttackDamage);
            Assert.Greater(working, 0f, "the excellent line applies");

            int broke = 0;
            EventBus.Subscribe<EquipmentBrokenEvent>(_ => broke++);
            for (int i = 0; i < 20 && sword.currentDurability > 0f; i++)
            {
                RepairService.WearOnDeath(_bag);
            }

            Assert.AreEqual(0f, sword.currentDurability);
            Assert.AreEqual(1, broke, "the broken event fires once");
            Assert.IsTrue(_bag.Equipped.ContainsKey(EquipmentSlot.MainHand), "never unequipped by force");
            Assert.AreEqual("item_iron_sword", sword.itemId, "never destroyed");
            Assert.AreEqual(0f, _buffs.GetPercentTotal(StatType.AttackDamage), "broken: no stats at all");
            Assert.IsTrue(DurabilityRules.IsBroken(ItemDatabase.GetById("item_iron_sword"), sword));

            TooltipContent tooltip = TooltipContentBuilder.ForItem(ItemDatabase.GetById("item_iron_sword"), sword);
            Assert.IsTrue(tooltip.StatLines.Contains(DurabilityTexts.Get(Rapadura.Gameplay.Content.ContentText.CurrentManager, DurabilityTexts.TooltipBroken)));

            RepairQuote quote = RepairService.Quote(_bag, RefineTarget.Equipped(EquipmentSlot.MainHand));
            Assert.AreEqual(RepairCheck.NotEnoughGold, quote.Check);
            Assert.IsTrue(quote.IsBroken);
            _bag.AddItem(ItemDatabase.GetById(RefineItems.Gold), quote.Cost + 5);
            Assert.IsTrue(RepairService.TryRepair(_bag, RefineTarget.Equipped(EquipmentSlot.MainHand), out int cost, out RepairCheck check));
            Assert.AreEqual(RepairCheck.Ok, check);
            Assert.AreEqual(quote.Cost, cost);
            Assert.AreEqual(5, _bag.GetTotalCount(RefineItems.Gold));
            Assert.AreEqual(120f, sword.currentDurability);
            Assert.AreEqual(working, _buffs.GetPercentTotal(StatType.AttackDamage), 1e-6f, "the stats come back");
        }

        [Test]
        public void RepairAll_IsAllOrNothing_AndListsEquippedFirst()
        {
            Equip("item_iron_sword");
            Equip("item_leather_chest");
            _bag.AddItem(ItemDatabase.GetById("item_leather_boots"), 1);
            int boots = _bag.Slots.Select((s, i) => (s, i)).First(p => p.s.itemId == "item_leather_boots").i;
            _bag.Slots[boots].currentDurability = 1f;
            RepairService.WearOnDeath(_bag);

            var worn = RepairService.ListWorn(_bag);
            Assert.AreEqual(3, worn.Count);
            Assert.IsTrue(worn[0].IsEquipped && worn[1].IsEquipped && !worn[2].IsEquipped);

            int total = RepairService.RepairAllCost(_bag, out int pieces);
            Assert.AreEqual(3, pieces);
            _bag.AddItem(ItemDatabase.GetById(RefineItems.Gold), total - 1);
            Assert.IsFalse(RepairService.TryRepairAll(_bag, out _, out _, out RepairCheck check));
            Assert.AreEqual(RepairCheck.NotEnoughGold, check);
            Assert.AreEqual(total - 1, _bag.GetTotalCount(RefineItems.Gold), "nothing spent");

            _bag.AddItem(ItemDatabase.GetById(RefineItems.Gold), 1);
            Assert.IsTrue(RepairService.TryRepairAll(_bag, out int paid, out int repaired, out check));
            Assert.AreEqual(total, paid);
            Assert.AreEqual(3, repaired);
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.Gold));
            Assert.AreEqual(0, RepairService.ListWorn(_bag).Count);
            Assert.IsFalse(RepairService.TryRepairAll(_bag, out _, out _, out check));
            Assert.AreEqual(RepairCheck.NothingToRepair, check);
        }

        [Test]
        public void Untracked_Items_AreNotRepairable()
        {
            _bag.AddItem(ItemDatabase.GetById(RefineItems.Gold), 10);
            int index = _bag.Slots.Select((s, i) => (s, i)).First(p => p.s.itemId == RefineItems.Gold).i;
            Assert.AreEqual(RepairCheck.NotRepairable, RepairService.Quote(_bag, RefineTarget.Bag(index)).Check);
        }

        [Test]
        public void Save_KeepsTheWear_AndABrokenPieceLoadsWithoutStats()
        {
            Equip("item_iron_sword");
            _bag.Equipped[EquipmentSlot.MainHand].currentDurability = 0f;
            _bag.NotifyItemDataChanged(EquipmentSlot.MainHand);

            string json = JsonUtility.ToJson((InventorySaveState)_bag.CaptureState());
            var other = new GameObject("DurabilityLoad");
            BuffController otherBuffs = other.AddComponent<BuffController>();
            InventoryManager loaded = other.AddComponent<InventoryManager>();
            WireBuffs(loaded, otherBuffs);
            try
            {
                loaded.RestoreState(JsonUtility.FromJson<InventorySaveState>(json));
                Assert.AreEqual(0f, loaded.Equipped[EquipmentSlot.MainHand].currentDurability);
                Assert.AreEqual(0f, otherBuffs.GetPercentTotal(StatType.AttackDamage));
                Assert.AreEqual(0f, otherBuffs.GetFlatTotal(StatType.AttackDamage));
            }
            finally
            {
                Object.DestroyImmediate(other);
            }
        }
    }
}
