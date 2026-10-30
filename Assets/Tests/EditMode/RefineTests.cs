using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.UI.Common;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Item refining +0 → +15 with the "Pedras da Luz Presa" (FASE 16, Docs/Balance/README.md "Refino de item"): the pure
    /// <see cref="RefineRules"/> (chance table, failure = −1 level, stones, stat curve, seeded rolls), the drop table,
    /// <see cref="RefineService"/> on a live bag, the save round-trip (old items load as +0) and the window's texts.
    /// </summary>
    public class RefineTests
    {
        // ------------------------------------------------------------------ rules: chance table and stones

        [Test]
        public void Chances_BlessingBandIsCertain_ThenFallFrom80To35()
        {
            for (int level = 0; level < RefineRules.BlessingCap; level++)
            {
                Assert.AreEqual(1f, RefineRules.BaseChance(level), $"+{level}");
                Assert.IsFalse(RefineRules.CanFail(level));
                Assert.AreEqual(RefineStone.Blessing, RefineRules.StoneFor(level));
                Assert.AreEqual(0, RefineRules.GoldCostFor(level));
            }

            CollectionAssert.AreEqual(new[] { 0.80f, 0.75f, 0.70f, 0.60f, 0.55f, 0.50f, 0.45f, 0.40f, 0.35f },
                Enumerable.Range(6, 9).Select(RefineRules.BaseChance).ToArray());
            for (int level = RefineRules.BlessingCap; level < RefineRules.MaxLevel; level++)
            {
                Assert.AreEqual(RefineStone.Soul, RefineRules.StoneFor(level), $"+{level}");
                Assert.IsTrue(RefineRules.CanFail(level));
                Assert.Less(RefineRules.BaseChance(level), RefineRules.BaseChance(level - 1) + 1e-6f, "never rises with the level");
            }

            Assert.AreEqual(0, RefineRules.GoldCostFor(8), "+6..+9 with the stone alone");
            CollectionAssert.AreEqual(new[] { 10, 15, 20, 30, 40, 50 }, Enumerable.Range(9, 6).Select(RefineRules.GoldCostFor).ToArray());
            Assert.AreEqual(RefineStone.None, RefineRules.StoneFor(15));
            Assert.AreEqual(0f, RefineRules.BaseChance(15));
            Assert.AreEqual(RefineItems.BlessingStone, RefineItems.IdOf(RefineStone.Blessing));
            Assert.AreEqual(RefineItems.ChaosStone, RefineItems.IdOf(RefineStone.Chaos), "defined now for the Tacho Encantado");
        }

        [Test]
        public void LuckCharm_AddsTenPoints_OnlyWhereARollCanFail_CappedAt95()
        {
            Assert.AreEqual(1f, RefineRules.ChanceFor(2, true), "certain rolls ignore the charm");
            Assert.AreEqual(0.90f, RefineRules.ChanceFor(6, true), 1e-6f);
            Assert.AreEqual(0.45f, RefineRules.ChanceFor(14, true), 1e-6f);
            Assert.LessOrEqual(RefineRules.ChanceFor(6, true), RefineRules.MaxChance);
            Assert.IsFalse(RefineRules.Attempt(3, true, 1).UsedCharm, "never spent on a certain roll");
            Assert.IsTrue(RefineRules.Attempt(7, true, 1).UsedCharm);
        }

        [Test]
        public void Failure_GoesBackOneLevel_AboveSix_AndNeverDestroys()
        {
            Assert.AreEqual(6, RefineRules.FailureLevel(6), "+6 has nothing below the safe band to lose");
            Assert.AreEqual(6, RefineRules.FailureLevel(7));
            Assert.AreEqual(7, RefineRules.FailureLevel(8), "user decision: +8 failed → +7");
            Assert.AreEqual(13, RefineRules.FailureLevel(14));

            RefineAttempt fail = RefineRules.Resolve(8, false, 0.99);
            Assert.IsFalse(fail.Success);
            Assert.IsTrue(fail.Dropped);
            Assert.AreEqual(7, fail.ToLevel);

            RefineAttempt ok = RefineRules.Resolve(8, false, 0.10);
            Assert.IsTrue(ok.Success);
            Assert.AreEqual(9, ok.ToLevel);
            Assert.AreEqual(0.70f, ok.Chance, 1e-6f);

            Assert.IsTrue(RefineRules.Resolve(0, false, 0.999999).Success, "Bênção band always succeeds");
            Assert.AreEqual(15, RefineRules.Resolve(15, false, 0.0).ToLevel, "+15 stays +15");
        }

        [Test]
        public void Checks_NeedTheStone_GoldAboveNine_AndACharmWhenAsked()
        {
            Assert.AreEqual(RefineCheck.NotRefinable, RefineRules.Check(false, 0, 5, 999, false, 0));
            Assert.AreEqual(RefineCheck.MaxLevel, RefineRules.Check(true, 15, 5, 999, false, 0));
            Assert.AreEqual(RefineCheck.MissingStone, RefineRules.Check(true, 0, 0, 999, false, 0));
            Assert.AreEqual(RefineCheck.NotEnoughGold, RefineRules.Check(true, 9, 1, 9, false, 0));
            Assert.AreEqual(RefineCheck.Ok, RefineRules.Check(true, 9, 1, 10, false, 0));
            Assert.AreEqual(RefineCheck.MissingCharm, RefineRules.Check(true, 7, 1, 0, true, 0));
            Assert.AreEqual(RefineCheck.Ok, RefineRules.Check(true, 2, 1, 0, true, 0), "no charm needed where it is not used");

            Assert.AreEqual(RefineCheck.Ok, RefineRules.CheckOption(true, 0, 1));
            Assert.AreEqual(RefineCheck.MaxOption, RefineRules.CheckOption(true, RefineRules.MaxOptionLevel, 3));
            Assert.AreEqual(RefineCheck.MissingStone, RefineRules.CheckOption(true, 1, 0));
            Assert.AreEqual(2, RefineRules.ResolveOption(1, 0.1));
            Assert.AreEqual(1, RefineRules.ResolveOption(1, 0.9), "a failed Pedra da Vida changes nothing");
            Assert.AreEqual(4, RefineRules.ResolveOption(4, 0.0));
        }

        // ------------------------------------------------------------------ rules: stat curve, glow, name

        [Test]
        public void StatCurve_RisesEveryLevel_AndDoublesAtFifteen()
        {
            Assert.AreEqual(0f, RefineRules.StatBonusFraction(0));
            for (int level = 1; level <= RefineRules.MaxLevel; level++)
            {
                Assert.Greater(RefineRules.StatBonusFraction(level), RefineRules.StatBonusFraction(level - 1), $"+{level}");
            }

            Assert.AreEqual(0.24f, RefineRules.StatBonusFraction(6), 1e-6f);
            Assert.AreEqual(0.42f, RefineRules.StatBonusFraction(9), 1e-6f);
            Assert.AreEqual(1f, RefineRules.StatBonusFraction(15), 1e-6f);
            Assert.AreEqual(18f, RefineRules.RefinedStat(18f, 0));
            Assert.AreEqual(25.6f, RefineRules.RefinedStat(18f, 9), 1e-4f);
            Assert.AreEqual(36f, RefineRules.RefinedStat(18f, 15), 1e-4f);
            Assert.AreEqual(0f, RefineRules.ArmourDefenseBonus(0f, 9));
            Assert.AreEqual(3.4f, RefineRules.ArmourDefenseBonus(8f, 9), 1e-4f);
            Assert.AreEqual(0.042f + 2 * RefineRules.WeaponOptionAttackPercent, RefineRules.WeaponAttackPercent(9, 2), 1e-5f);
        }

        [Test]
        public void Glow_StepsUpAt7_9_11_13_15_AndTheNameCarriesTheLevel()
        {
            CollectionAssert.AreEqual(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5 },
                Enumerable.Range(5, 11).Select(RefineRules.GlowTier).ToArray());
            Assert.AreEqual(string.Empty, RefineRules.NameSuffix(0));
            Assert.AreEqual(" +7", RefineRules.NameSuffix(7));
            Assert.IsNull(VillageRefinePanel.GlowClass(6));
            Assert.AreEqual("refine-glow--5", VillageRefinePanel.GlowClass(15));
        }

        // ------------------------------------------------------------------ rules: seeded rolls (server parity)

        [Test]
        public void Rolls_AreSeeded_Uniform_AndReproducible()
        {
            Assert.AreEqual(RefineRules.Roll(42), RefineRules.Roll(42));
            Assert.AreNotEqual(RefineRules.Roll(42), RefineRules.Roll(43));

            int below = 0;
            const int n = 20000;
            for (int seed = 0; seed < n; seed++)
            {
                double roll = RefineRules.Roll(seed);
                Assert.That(roll, Is.InRange(0.0, 0.9999999999));
                if (roll < 0.55)
                {
                    below++;
                }
            }

            Assert.AreEqual(0.55, below / (double)n, 0.02, "the stated chance is the real one");
            RefineAttempt a = RefineRules.Attempt(8, true, 1234);
            RefineAttempt b = RefineRules.Attempt(8, true, 1234);
            Assert.AreEqual(a.ToLevel, b.ToLevel);
            Assert.AreEqual(a.Roll, b.Roll);
        }

        [Test]
        public void ExpectedCost_CountsTheReclimbAfterADrop()
        {
            (double six, double sixGold) = RefineRules.ExpectedCostTo(6);
            Assert.AreEqual(6.0, six, 1e-9);
            Assert.AreEqual(0.0, sixGold, 1e-9);
            (double nine, _) = RefineRules.ExpectedCostTo(9);
            Assert.Greater(nine, 6 + 3, "+6→+9 needs more than one stone per level");
            Assert.Less(RefineRules.ExpectedCostTo(9, true).stones, nine, "the charm saves stones");
            Assert.Greater(RefineRules.ExpectedCostTo(15).gold, RefineRules.ExpectedCostTo(12).gold);
        }

        // ------------------------------------------------------------------ drops and shops

        [Test]
        public void Drops_BlessingFromEveryBoss_SoulFromTheDesertOn_SeededAndOrdered()
        {
            Assert.AreEqual(2f, RefineStoneDrops.Expected(RefineItems.BlessingStone, BiomeType.Forest, true));
            Assert.AreEqual(0f, RefineStoneDrops.Expected(RefineItems.SoulStone, BiomeType.Forest, false));
            Assert.Greater(RefineStoneDrops.Expected(RefineItems.SoulStone, BiomeType.Desert, true), 0f);
            Assert.AreEqual(0f, RefineStoneDrops.Expected(RefineItems.ChaosStone, BiomeType.Desert, true), "Caos from the Serra on");
            Assert.Greater(RefineStoneDrops.ChanceOf(RefineStoneDrops.Get(RefineItems.BlessingStone), BiomeType.Caves, false, 1),
                RefineStoneDrops.ChanceOf(RefineStoneDrops.Get(RefineItems.BlessingStone), BiomeType.Caves, false, 0), "NG+ raises commons");
            foreach (RefineDropSpec spec in RefineStoneDrops.Table)
            {
                for (int region = 1; region < 4; region++)
                {
                    Assert.GreaterOrEqual(spec.CommonChance[region], spec.CommonChance[region - 1], $"{spec.ItemId}: deeper regions drop more");
                }
            }

            CollectionAssert.AreEqual(RefineItems.All, RefineStoneDrops.Table.Select(s => s.ItemId).ToArray());
            Assert.IsEmpty(RefineStoneDrops.Roll(false, null, new System.Random(1)), "no region, nothing");

            List<string> first = RefineStoneDrops.Roll(true, BiomeType.Caves, new System.Random(7)).Select(d => d.Item.ItemId + d.Quantity).ToList();
            List<string> again = RefineStoneDrops.Roll(true, BiomeType.Caves, new System.Random(7)).Select(d => d.Item.ItemId + d.Quantity).ToList();
            CollectionAssert.AreEqual(first, again);
            Assert.Contains(RefineItems.BlessingStone + "3", first, "Cavernas boss: 3 Bênçãos, always");
        }

        [Test]
        public void StoneItems_Exist_Stack_AndAreSoldAtBalancedPrices()
        {
            ItemDatabase.Invalidate();
            foreach (string id in RefineItems.All)
            {
                ItemDefinition item = ItemDatabase.GetById(id);
                Assert.IsNotNull(item, $"{id} missing from Resources/Items");
                Assert.IsTrue(item.IsStackable, id);
                Assert.IsFalse(RefineRules.IsRefinable(item), id);
            }

            Assert.IsTrue(RefineRules.IsRefinable(ItemDatabase.GetById("item_iron_sword")));
            Assert.IsTrue(RefineRules.IsRefinable(ItemDatabase.GetById("item_leather_chest")));
            Assert.IsFalse(RefineRules.IsRefinable(ItemDatabase.GetById("item_axe")), "tools have no base stat to raise");

            List<CityShopEntry> stones = CityShopCatalog.Shops.SelectMany(s => s.Entries).Where(e => RefineItems.All.Contains(e.ItemId)).ToList();
            Assert.IsTrue(stones.Any(e => e.ItemId == RefineItems.BlessingStone && !e.Unlock.IsGated), "Bênção on sale from the start");
            Assert.IsFalse(stones.Any(e => e.ItemId == RefineItems.LifeStone || e.ItemId == RefineItems.ChaosStone), "Vida and Caos: drops only");
        }

        // ------------------------------------------------------------------ service on a live bag + save

        private GameObject _owner;
        private InventoryManager _bag;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            _owner = new GameObject("RefineBag");
            _bag = _owner.AddComponent<InventoryManager>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_owner);
        }

        private int AddSword()
        {
            _bag.AddItem(ItemDatabase.GetById("item_iron_sword"), 1);
            return _bag.Slots.Select((s, i) => (s, i)).First(p => p.s.itemId == "item_iron_sword").i;
        }

        [Test]
        public void Service_SpendsTheStone_RaisesTheLevel_AndNamesIt()
        {
            int index = AddSword();
            _bag.AddItem(ItemDatabase.GetById(RefineItems.BlessingStone), 2);
            RefineTarget target = RefineTarget.Bag(index);
            Assert.IsTrue(RefineService.ListTargets(_bag).Any(t => !t.IsEquipped && t.SlotIndex == index));

            Assert.IsTrue(RefineService.TryRefine(_bag, target, false, 1, out RefineAttempt attempt, out RefineCheck check));
            Assert.AreEqual(RefineCheck.Ok, check);
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(1, _bag.Slots[index].refineLevel);
            Assert.AreEqual(1, _bag.GetTotalCount(RefineItems.BlessingStone));

            ItemDefinition sword = ItemDatabase.GetById("item_iron_sword");
            StringAssert.EndsWith(" +1", GeneratedItemName.Compose(sword, _bag.Slots[index], null));
            TooltipContent tooltip = TooltipContentBuilder.ForItem(sword, _bag.Slots[index]);
            StringAssert.EndsWith(" +1", tooltip.Title);
            Assert.IsTrue(tooltip.StatLines.Any(l => l.Contains("18.7")), "refined weapon damage 18 × 1.04");
        }

        [Test]
        public void Service_WithoutStones_SpendsNothing_AndAFailureKeepsTheItem()
        {
            int index = AddSword();
            Assert.IsFalse(RefineService.TryRefine(_bag, RefineTarget.Bag(index), false, 1, out _, out RefineCheck check));
            Assert.AreEqual(RefineCheck.MissingStone, check);

            _bag.Slots[index].refineLevel = 8;
            _bag.AddItem(ItemDatabase.GetById(RefineItems.SoulStone), 1);
            int failSeed = Enumerable.Range(0, 1000).First(s => RefineRules.Roll(s) >= 0.70);
            Assert.IsTrue(RefineService.TryRefine(_bag, RefineTarget.Bag(index), false, failSeed, out RefineAttempt attempt, out _));
            Assert.IsTrue(attempt.Dropped);
            Assert.AreEqual(7, _bag.Slots[index].refineLevel, "+8 failed → +7");
            Assert.AreEqual("item_iron_sword", _bag.Slots[index].itemId, "never destroyed");
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.SoulStone));
        }

        [Test]
        public void Service_HighRefinePaysGold_AndTheCharmIsSpent()
        {
            int index = AddSword();
            _bag.Slots[index].refineLevel = 9;
            _bag.AddItem(ItemDatabase.GetById(RefineItems.SoulStone), 1);
            _bag.AddItem(ItemDatabase.GetById(RefineItems.LuckCharm), 1);
            _bag.AddItem(ItemDatabase.GetById(RefineItems.Gold), 25);

            RefinePreview preview = RefineService.Preview(_bag, RefineTarget.Bag(index), true);
            Assert.AreEqual(0.70f, preview.Chance, 1e-6f);
            Assert.AreEqual(10, preview.GoldCost);
            Assert.IsTrue(RefineService.TryRefine(_bag, RefineTarget.Bag(index), true, 5, out RefineAttempt attempt, out _));
            Assert.IsTrue(attempt.UsedCharm);
            Assert.AreEqual(15, _bag.GetTotalCount(RefineItems.Gold));
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.LuckCharm));
        }

        [Test]
        public void RefineData_TravelsThroughEquip_Copies_AndTheSave_OldItemsLoadAsPlusZero()
        {
            int index = AddSword();
            _bag.Slots[index].refineLevel = 7;
            _bag.Slots[index].refineOptionLevel = 2;
            Assert.IsTrue(_bag.Slots[index].HasGeneratedData, "a refined piece never merges into a plain stack");

            InventorySlotData copy = _bag.Slots[index].CloneWithQuantity(1);
            Assert.AreEqual(7, copy.refineLevel);
            Assert.AreEqual(2, copy.refineOptionLevel);

            Assert.IsTrue(_bag.EquipFromSlot(index));
            Assert.AreEqual(7, _bag.Equipped[EquipmentSlot.MainHand].refineLevel);

            var state = (InventorySaveState)_bag.CaptureState();
            string json = JsonUtility.ToJson(state);
            var loaded = JsonUtility.FromJson<InventorySaveState>(json);
            var other = new GameObject("RefineLoad").AddComponent<InventoryManager>();
            try
            {
                other.RestoreState(loaded);
                Assert.AreEqual(7, other.Equipped[EquipmentSlot.MainHand].refineLevel);
                Assert.AreEqual(2, other.Equipped[EquipmentSlot.MainHand].refineOptionLevel);

                Assert.IsTrue(other.UnequipSlot(EquipmentSlot.MainHand));
                Assert.IsTrue(other.Slots.Any(s => s.itemId == "item_iron_sword" && s.refineLevel == 7), "unequip keeps the refine");
            }
            finally
            {
                Object.DestroyImmediate(other.gameObject);
            }

            // A save from before refining: no refine keys at all.
            var old = JsonUtility.FromJson<InventorySlotData>("{\"itemId\":\"item_iron_sword\",\"quantity\":1}");
            Assert.AreEqual(0, old.refineLevel);
            Assert.AreEqual(0, old.refineOptionLevel);
            Assert.IsFalse(old.HasGeneratedData);
            var oldEquip = JsonUtility.FromJson<InventorySaveState.EquippedEntry>("{\"slot\":5,\"itemId\":\"item_iron_sword\"}");
            Assert.AreEqual(0, oldEquip.refineLevel);
        }

        // ------------------------------------------------------------------ window texts

        [Test]
        public void Window_Lines_ShowCostFailureAndResult()
        {
            Assert.AreEqual("75", VillageRefinePanel.Percent(0.75f));
            var safe = new RefinePreview(null, null, 3, 0, RefineStone.Blessing, 1, 0, 0, 0, 0, 1f, false, 3, RefineCheck.Ok, RefineCheck.Ok);
            Assert.AreEqual("Nunca falha.", VillageRefinePanel.FailureLine(safe, null));
            Assert.AreEqual("Custo: 1 Pedra-Faísca", VillageRefinePanel.CostLine(safe, null));

            var risky = new RefinePreview(null, null, 10, 0, RefineStone.Soul, 1, 30, 50, 0, 0, 0.45f, true, 9, RefineCheck.Ok, RefineCheck.Ok);
            Assert.AreEqual("Se falhar: volta para +9 (o item nunca some).", VillageRefinePanel.FailureLine(risky, null));
            Assert.AreEqual("Custo: 1 Pedra da Alma Viva + 30 de ouro", VillageRefinePanel.CostLine(risky, null));
            Assert.AreEqual("Falta ouro: custa 30.", VillageRefinePanel.CheckLine(RefineCheck.NotEnoughGold, risky, null));

            Assert.AreEqual("Deu certo! Agora é Espada +11.",
                VillageRefinePanel.ResultLine(RefineRules.Resolve(10, false, 0.0), "Espada +11", null));
            StringAssert.StartsWith("Não pegou... voltou para Espada +9",
                VillageRefinePanel.ResultLine(RefineRules.Resolve(10, false, 0.99), "Espada +9", null));
        }
    }
}
