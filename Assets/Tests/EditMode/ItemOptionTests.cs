using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Options;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// MU-style item options (FASE 16, Docs/Balance/README.md "Opções de item"): seeded rolls (excellent, luck, ancient,
    /// sockets) and their drop chances, the bonuses on a live bag (piece options, ancient set tiers, broken pieces do not
    /// count), luck on refines, socket engraving at the ferreiro, the tooltip's green lines and the save round-trip.
    /// </summary>
    public class ItemOptionTests
    {
        // ------------------------------------------------------------------ rolls

        [Test]
        public void Roll_IsReproducible_AndOnlyEquipmentGetsOptions()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                ItemOptionRoll a = ItemOptionRules.Roll(true, true, seed);
                ItemOptionRoll b = ItemOptionRules.Roll(true, true, seed);
                Assert.AreEqual(a.ExcellentMask, b.ExcellentMask);
                Assert.AreEqual(a.Luck, b.Luck);
                Assert.AreEqual(a.AncientSetId, b.AncientSetId);
                Assert.AreEqual(a.SocketCount, b.SocketCount);
            }

            Assert.IsTrue(ItemOptionRules.Roll(ItemDatabase.GetById(RefineItems.Gold), true, 1).IsEmpty);
            Assert.IsTrue(ItemOptionRules.Roll(null, true, 1).IsEmpty);
        }

        [Test]
        public void Roll_Frequencies_FollowTheCatalog_AndAncientIsNeverExcellent()
        {
            const int n = 40000;
            int excellentCommon = 0, excellentBoss = 0, ancientBoss = 0, luckCommon = 0, socketsBoss = 0, twoLines = 0;
            for (int seed = 0; seed < n; seed++)
            {
                ItemOptionRoll common = ItemOptionRules.Roll(true, false, seed * 7919 + 1);
                ItemOptionRoll boss = ItemOptionRules.Roll(false, true, seed * 7919 + 1);
                Assert.IsNull(common.AncientSetId, "ancient pieces only come from bosses");
                Assert.LessOrEqual(common.SocketCount, 1);
                Assert.IsFalse(boss.AncientSetId != null && boss.ExcellentMask != 0, "never ancient and excellent");
                Assert.AreEqual(0, boss.ExcellentMask & 0xFF, "armour rolls only armour lines");
                Assert.AreEqual(0, common.ExcellentMask & ~0xFF, "weapons roll only weapon lines");
                excellentCommon += common.ExcellentMask != 0 ? 1 : 0;
                excellentBoss += boss.ExcellentMask != 0 ? 1 : 0;
                twoLines += CountBits(boss.ExcellentMask) == 2 ? 1 : 0;
                ancientBoss += boss.AncientSetId != null ? 1 : 0;
                luckCommon += common.Luck ? 1 : 0;
                socketsBoss += boss.SocketCount > 0 ? 1 : 0;
                Assert.LessOrEqual(CountBits(common.ExcellentMask), 1);
                Assert.LessOrEqual(boss.SocketCount, ItemOptionCatalog.MaxSockets);
            }

            Assert.AreEqual(ItemOptionCatalog.ExcellentChanceCommon, excellentCommon / (float)n, 0.004f);
            Assert.AreEqual(ItemOptionCatalog.ExcellentChanceBoss * (1f - ItemOptionCatalog.AncientChanceBoss), excellentBoss / (float)n, 0.008f);
            Assert.AreEqual(ItemOptionCatalog.AncientChanceBoss, ancientBoss / (float)n, 0.006f);
            Assert.AreEqual(ItemOptionCatalog.LuckChanceCommon, luckCommon / (float)n, 0.006f);
            Assert.AreEqual(ItemOptionCatalog.SocketChanceBoss, socketsBoss / (float)n, 0.008f);
            Assert.Greater(twoLines, 0, "a boss can drop two excellent lines");
        }

        [Test]
        public void OptionSeed_IsDecorrelatedFromThePieceSeed()
        {
            Assert.AreNotEqual(ItemOptionRules.OptionSeed(1), ItemOptionRules.OptionSeed(2));
            Assert.AreNotEqual(1, ItemOptionRules.OptionSeed(1));
        }

        [Test]
        public void Catalog_OneAncientSetPerCousin_AndEveryIdIsUnique()
        {
            Assert.AreEqual(5, ItemOptionCatalog.AncientSets.Length);
            Assert.AreEqual(5, ItemOptionCatalog.AncientSets.Select(s => s.Cousin).Distinct().Count());
            Assert.AreEqual(ItemOptionCatalog.Engravings.Length, ItemOptionCatalog.Engravings.Select(e => e.Id).Distinct().Count());
            foreach (Engraving engraving in ItemOptionCatalog.Engravings)
            {
                Assert.IsNotNull(ItemDatabase.GetById(engraving.MaterialId), engraving.MaterialId);
            }

            Assert.IsTrue(ItemOptionCatalog.Engravings.Any(e => e.Weapon) && ItemOptionCatalog.Engravings.Any(e => !e.Weapon));
        }

        private static int CountBits(int mask)
        {
            int count = 0;
            for (; mask != 0; mask &= mask - 1)
            {
                count++;
            }

            return count;
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
            _owner = new GameObject("OptionBag");
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

        private int IndexOf(string itemId) => _bag.Slots.Select((s, i) => (s, i)).First(p => p.s.itemId == itemId).i;

        private InventorySlotData Equip(string itemId, string ancientSetId = null)
        {
            _bag.AddItem(ItemDatabase.GetById(itemId), 1);
            int index = IndexOf(itemId);
            _bag.Slots[index].ancientSetId = ancientSetId;
            Assert.IsTrue(_bag.EquipFromSlot(index), itemId);
            return _bag.Equipped[ItemDatabase.GetById(itemId).EquipSlot];
        }

        [Test]
        public void GeneratedDrop_CarriesItsOptions_IntoTheBag()
        {
            var instance = new GeneratedItemInstance(ItemDatabase.GetById("item_iron_sword"), ItemRarity.Rare, new List<RolledAffix>(), null)
            {
                Options = new ItemOptionRoll((int)ExcellentOption.WeaponSpeed, true, null, 2),
            };
            Assert.IsTrue(_bag.AddGeneratedItem(instance));
            InventorySlotData slot = _bag.Slots[IndexOf("item_iron_sword")];
            Assert.AreEqual((int)ExcellentOption.WeaponSpeed, slot.excellentMask);
            Assert.IsTrue(slot.luck);
            Assert.AreEqual(2, slot.sockets.Length);
            Assert.IsTrue(slot.sockets.All(string.IsNullOrEmpty));
            Assert.IsTrue(slot.HasItemOptions && slot.HasGeneratedData, "an optioned piece never merges into a plain stack");
        }

        [Test]
        public void PieceOptions_Apply_AndAncientTiersNeedPiecesThatWork()
        {
            InventorySlotData sword = Equip("item_iron_sword", "ancient_vaqueiro");
            float defenseOne = _buffs.GetFlatTotal(StatType.Defense);
            InventorySlotData chest = Equip("item_leather_chest", "ancient_vaqueiro");
            float chestOnly = _buffs.GetFlatTotal(StatType.Defense) - defenseOne;

            AncientSet set = ItemOptionCatalog.FindSet("ancient_vaqueiro");
            float tier2 = set.Tiers.First(t => t.Pieces == 2).Modifier.Value;
            Assert.AreEqual(ModifierApplication.Flat, set.Tiers.First(t => t.Pieces == 2).Modifier.Application);
            Assert.AreEqual(set.Tiers.First(t => t.Pieces == 2).Modifier.Stat, StatType.Defense);
            Assert.Greater(chestOnly, tier2 - 1e-4f, "two pieces: the 2-piece bonus is on");

            List<OptionModifier> bonuses = ItemOptionRules.SetBonuses(new[]
            {
                (ItemDatabase.GetById("item_iron_sword"), sword, false),
                (ItemDatabase.GetById("item_leather_chest"), chest, false),
            });
            Assert.AreEqual(1, bonuses.Count);

            // Break the chest: neither its stats nor its set piece count.
            chest.currentDurability = 0f;
            _bag.NotifyItemDataChanged(EquipmentSlot.Chest);
            Assert.AreEqual(defenseOne, _buffs.GetFlatTotal(StatType.Defense), 1e-4f);

            // Unequip it: same, and the bonus is not left behind.
            chest.currentDurability = 90f;
            _bag.NotifyItemDataChanged(EquipmentSlot.Chest);
            Assert.IsTrue(_bag.UnequipSlot(EquipmentSlot.Chest));
            Assert.AreEqual(defenseOne, _buffs.GetFlatTotal(StatType.Defense), 1e-4f);
        }

        [Test]
        public void ExcellentLines_FollowThePieceKind()
        {
            InventorySlotData sword = Equip("item_iron_sword");
            float before = _buffs.GetPercentTotal(StatType.AttackDamage);
            sword.excellentMask = (int)(ExcellentOption.WeaponDamage | ExcellentOption.ArmourHealth);
            _bag.NotifyItemDataChanged(EquipmentSlot.MainHand);
            Assert.AreEqual(before + ItemOptionCatalog.FindLine(ExcellentOption.WeaponDamage).Value.Modifier.Value,
                _buffs.GetPercentTotal(StatType.AttackDamage), 1e-5f);
            Assert.AreEqual(0f, _buffs.GetPercentTotal(StatType.MaxHealth), "an armour bit on a weapon gives nothing");
        }

        [Test]
        public void Luck_AddsToRefinesThatCanFail_UpToTheCap()
        {
            Assert.AreEqual(RefineRules.ChanceFor(8, false) + RefineRules.LuckBonus, RefineRules.ChanceFor(8, false, true), 1e-6f);
            Assert.AreEqual(1f, RefineRules.ChanceFor(0, false, true), "a certain refine stays certain");
            Assert.LessOrEqual(RefineRules.ChanceFor(7, true, true), RefineRules.MaxChance);
        }

        [Test]
        public void Engrave_SpendsMaterialAndGold_FillsTheFirstSocket_AndRespectsTheKind()
        {
            _bag.AddItem(ItemDatabase.GetById("item_iron_sword"), 1);
            int index = IndexOf("item_iron_sword");
            _bag.Slots[index].sockets = new[] { string.Empty, string.Empty };
            RefineTarget target = RefineTarget.Bag(index);
            Engraving ferro = ItemOptionCatalog.FindEngraving("engrave_ferro");
            Engraving pedra = ItemOptionCatalog.FindEngraving("engrave_pedra");

            Assert.AreEqual(EngraveCheck.MissingMaterial, EngraveService.Check(_bag, target, ferro.Id));
            _bag.AddItem(ItemDatabase.GetById(ferro.MaterialId), ferro.MaterialQuantity);
            Assert.AreEqual(EngraveCheck.NotEnoughGold, EngraveService.Check(_bag, target, ferro.Id));
            _bag.AddItem(ItemDatabase.GetById(RefineItems.Gold), ferro.Gold);
            _bag.AddItem(ItemDatabase.GetById(pedra.MaterialId), pedra.MaterialQuantity);
            Assert.AreEqual(EngraveCheck.WrongKind, EngraveService.Check(_bag, target, pedra.Id), "stone goes in armour");
            Assert.AreEqual(EngraveCheck.UnknownEngraving, EngraveService.Check(_bag, target, "nope"));

            Assert.IsTrue(EngraveService.TryEngrave(_bag, target, ferro.Id, out EngraveCheck check));
            Assert.AreEqual(EngraveCheck.Ok, check);
            index = IndexOf("item_iron_sword");
            Assert.AreEqual(ferro.Id, _bag.Slots[index].sockets[0]);
            Assert.AreEqual(string.Empty, _bag.Slots[index].sockets[1]);
            Assert.AreEqual(0, _bag.GetTotalCount(ferro.MaterialId));
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.Gold));
            Assert.IsTrue(EngraveService.ListSocketed(_bag).Any(t => !t.IsEquipped));

            Assert.AreEqual(1, ItemOptionRules.PieceModifiers(ItemDatabase.GetById("item_iron_sword"), _bag.Slots[index]).Count);
        }

        [Test]
        public void Tooltip_ShowsTheOptionLinesInGreen()
        {
            _bag.AddItem(ItemDatabase.GetById("item_iron_sword"), 1);
            InventorySlotData slot = _bag.Slots[IndexOf("item_iron_sword")];
            slot.excellentMask = (int)ExcellentOption.WeaponCritical;
            slot.luck = true;
            slot.sockets = new[] { "engrave_caos", string.Empty };

            TooltipContent content = TooltipContentBuilder.ForItem(ItemDatabase.GetById("item_iron_sword"), slot);
            Assert.AreEqual(4, content.OptionLines.Count, "excellent + luck + two sockets");
            Assert.IsTrue(content.OptionLines.All(content.StatLines.Contains));
            StringAssert.Contains("<color=" + TooltipController.OptionLineColor + ">", TooltipController.JoinStatLines(content));
        }

        [Test]
        public void Options_TravelThroughEquip_Copies_AndTheSave_OldSavesArePlain()
        {
            _bag.AddItem(ItemDatabase.GetById("item_iron_sword"), 1);
            int index = IndexOf("item_iron_sword");
            InventorySlotData slot = _bag.Slots[index];
            slot.excellentMask = (int)ExcellentOption.WeaponDamage;
            slot.luck = true;
            slot.ancientSetId = "ancient_carcara";
            slot.sockets = new[] { "engrave_ferro", string.Empty };

            InventorySlotData copy = slot.CloneWithQuantity(1);
            Assert.AreEqual("ancient_carcara", copy.ancientSetId);
            Assert.AreNotSame(slot.sockets, copy.sockets, "sockets are cloned");

            Assert.IsTrue(_bag.EquipFromSlot(index));
            string json = JsonUtility.ToJson((InventorySaveState)_bag.CaptureState());
            var other = new GameObject("OptionLoad").AddComponent<InventoryManager>();
            try
            {
                other.RestoreState(JsonUtility.FromJson<InventorySaveState>(json));
                InventorySlotData loaded = other.Equipped[EquipmentSlot.MainHand];
                Assert.AreEqual((int)ExcellentOption.WeaponDamage, loaded.excellentMask);
                Assert.IsTrue(loaded.luck);
                Assert.AreEqual("ancient_carcara", loaded.ancientSetId);
                CollectionAssert.AreEqual(new[] { "engrave_ferro", string.Empty }, loaded.sockets);

                Assert.IsTrue(other.UnequipSlot(EquipmentSlot.MainHand));
                Assert.IsTrue(other.Slots.Any(s => s.itemId == "item_iron_sword" && s.luck && s.ancientSetId == "ancient_carcara"));
            }
            finally
            {
                Object.DestroyImmediate(other.gameObject);
            }

            var old = JsonUtility.FromJson<InventorySaveState.EquippedEntry>("{\"slot\":5,\"itemId\":\"item_iron_sword\"}");
            Assert.AreEqual(0, old.excellentMask);
            Assert.IsFalse(old.luck);
            var oldSlot = JsonUtility.FromJson<InventorySlotData>("{\"itemId\":\"item_iron_sword\",\"quantity\":1}");
            Assert.IsFalse(oldSlot.HasItemOptions);
        }
    }
}
