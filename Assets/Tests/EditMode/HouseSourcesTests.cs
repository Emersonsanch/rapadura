using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using Rapadura.UI.Housing;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// H3 "Fontes" (Housing.md §7): house-item drops with an injected RNG (chance, groups, rarity fallback), the boss's
    /// guaranteed first-win trophy and bad-luck protection, quest / daily / achievement rewards paid once, delivery to the
    /// Depósito without touching the bag, the workbench recipes and the save of the source records.
    /// </summary>
    public class HouseSourcesTests
    {
        private const string Wolf = "enemy_forest_alpha_wolf";

        private sealed class FakeWallet : IHouseWallet
        {
            public readonly Dictionary<string, int> Items = new Dictionary<string, int>();
            public int Count(string itemId) => Items.TryGetValue(itemId, out int n) ? n : 0;

            public bool Remove(string itemId, int quantity)
            {
                if (Count(itemId) < quantity) return false;
                Items[itemId] = Count(itemId) - quantity;
                return true;
            }
        }

        /// <summary>A Random whose NextDouble is fixed (Next stays real, seeded).</summary>
        private sealed class FixedDouble : System.Random
        {
            private readonly double _value;
            public FixedDouble(double value) : base(7) => _value = value;
            public override double NextDouble() => _value;
        }

        private static HouseService NewHouse(double roll = 0.0)
        {
            return new HouseService(() => 0) { SourceRandom = new FixedDouble(roll), NightProvider = () => false, DailyQuestProvider = _ => false };
        }

        [Test]
        public void CommonDrop_RespectsTheChance_AndNightIsHigher()
        {
            Assert.IsNull(HouseDropRules.RollCommon("enemy_forest_wolf", false, new FixedDouble(HouseDropRules.CommonChance + 0.001)));
            Assert.IsNotNull(HouseDropRules.RollCommon("enemy_forest_wolf", false, new FixedDouble(HouseDropRules.CommonChance - 0.001)));
            Assert.IsNotNull(HouseDropRules.RollCommon("enemy_forest_wolf", true, new FixedDouble(HouseDropRules.NightChance - 0.001)), "night = 5%");
            Assert.Greater(HouseDropRules.NightChance, HouseDropRules.CommonChance);
        }

        [Test]
        public void CommonPool_HasTheEnemysOwnItems_AndBiomeShopItemsUpToRare()
        {
            List<(HouseItemDefinition Item, int Weight)> pool = HouseDropRules.CommonPool("enemy_forest_wolf");
            Assert.IsNotEmpty(pool);
            foreach ((HouseItemDefinition item, int weight) in pool)
            {
                if (item.PrimarySource.Type == HouseItemSourceType.Shop)
                {
                    Assert.LessOrEqual(item.Rarity, ItemRarity.Rare, item.ItemId);
                    Assert.IsTrue(HouseDropRules.ShopIn(item.PrimarySource.RefId, "forest"), item.ItemId);
                    Assert.AreEqual(HouseDropRules.ShopItemWeight, weight);
                }
                else
                {
                    Assert.AreEqual("enemy_forest_wolf", item.PrimarySource.RefId);
                }

                Assert.AreEqual(HouseSeason.None, item.Seasonal, "seasonal items never drop");
            }
        }

        [Test]
        public void PickFromPool_FallsBackToLowerTiers_ThenHigher()
        {
            CollectionAssert.AreEqual(new[] { ItemRarity.Rare, ItemRarity.Uncommon, ItemRarity.Common, ItemRarity.Epic, ItemRarity.Legendary },
                HouseDropRules.FallbackOrder(ItemRarity.Rare).ToArray());
            List<(HouseItemDefinition Item, int Weight)> pool = HouseDropRules.BossPool(Wolf);
            Assert.IsFalse(pool.Any(p => p.Item.ItemId == "casa_trofeu_001"), "the plaque is the first-win gift, not in the group");
            Assert.IsNotNull(HouseDropRules.PickFromPool(pool, ItemRarity.Common, new System.Random(1)), "no Common in the wolf's group → Uncommon");
        }

        [Test]
        public void FirstBossWin_GivesTheTrophyOnce_AndEverythingGoesToTheDepot()
        {
            HouseService house = NewHouse(0.9);
            List<string> first = house.OnBossVictory(Wolf);
            Assert.Contains("casa_trofeu_001", first);
            Assert.AreEqual(1, house.Layout.DepotCount("casa_trofeu_001"));
            Assert.AreEqual(2, first.Count, "trophy + 1 roll (0.9 ≥ 25% → no second roll)");

            List<string> second = house.OnBossVictory(Wolf);
            Assert.IsFalse(second.Contains("casa_trofeu_001"), "the trophy only comes once");
            Assert.AreEqual(1, second.Count);
            Assert.AreEqual(2, house.Sources.FindBoss(Wolf).wins);

            HouseService lucky = NewHouse(0.1);
            Assert.AreEqual(3, lucky.OnBossVictory(Wolf).Count, "trophy + 1 + the 25% second roll");
        }

        [Test]
        public void BadLuckProtection_ForcesALegendaryAfterThePityStreak()
        {
            var record = new HouseBossRecord { bossId = Wolf, trophyGiven = true, sinceLegendary = HouseDropRules.PityWins };
            List<string> drops = HouseDropRules.RollBoss(Wolf, record, new FixedDouble(0.9));
            Assert.AreEqual(1, drops.Count);
            Assert.AreEqual(ItemRarity.Legendary, HouseCatalog.Get(drops[0]).Rarity);
            Assert.AreEqual(0, record.sinceLegendary, "the streak resets");

            var unlucky = new HouseBossRecord { bossId = Wolf, trophyGiven = true, sinceLegendary = 3 };
            List<string> plain = HouseDropRules.RollBoss(Wolf, unlucky, new FixedDouble(0.9));
            if (HouseCatalog.Get(plain[0]).Rarity != ItemRarity.Legendary) Assert.AreEqual(4, unlucky.sinceLegendary);
        }

        [Test]
        public void QuestAndAchievementRewards_ArePaidOnce()
        {
            HouseService house = NewHouse();
            List<string> quest = house.OnQuestCompleted("quest_region_forest_howling_trail");
            Assert.Contains("casa_regional_011", quest, "§7.3 extra item");
            Assert.IsEmpty(house.OnQuestCompleted("quest_region_forest_howling_trail"), "only once");

            string achievement = HouseCatalog.All.First(i => i.PrimarySource?.Type == HouseItemSourceType.Achievement).PrimarySource.RefId;
            Assert.IsNotEmpty(house.OnAchievementUnlocked(achievement));
            Assert.IsEmpty(house.OnAchievementUnlocked(achievement));
        }

        [Test]
        public void Dailies_GiveTheBoxEveryTime_AndTheFixedItemOnlyTheFirstTime()
        {
            HouseService house = NewHouse();
            house.DailyQuestProvider = _ => true;
            string daily = HouseCatalog.All.First(i => i.PrimarySource?.Type == HouseItemSourceType.DailyQuest).PrimarySource.RefId;
            List<string> first = house.OnQuestCompleted(daily);
            List<string> again = house.OnQuestCompleted(daily);
            Assert.AreEqual(first.Count - 1, again.Count, "the fixed item is gone the second time");
            Assert.AreEqual(1, again.Count, "the Caixinha da Vó always comes");
            Assert.IsTrue(HouseRewardRules.DailyBoxPool().All(p => p.Item.Rarity <= ItemRarity.Rare));
        }

        [Test]
        public void GrantItem_GoesToTheDepot_EvenPastTheLimit_AndUnknownIdsAreRefused()
        {
            HouseService house = NewHouse();
            Assert.IsFalse(house.GrantItem("casa_nao_existe", HouseGrantReason.Quest));
            for (int i = 0; i < 3; i++) Assert.IsTrue(house.GrantItem("casa_sala_042", HouseGrantReason.Craft));
            Assert.AreEqual(3, house.Layout.DepotCount("casa_sala_042"));
        }

        [Test]
        public void Recipes_CheckHouseStationBossAndMaterials_AndPayAllOrNothing()
        {
            HouseCraftRecipe stool = HouseCraftRules.ForResult("casa_sala_042");
            var wallet = new FakeWallet();
            Func<HouseItemFunction, string, bool> bench = (f, id) => f == HouseItemFunction.Bench && id == null;
            Assert.AreEqual(HouseCraftBlock.NoHouse, HouseCraftRules.Check(stool, false, bench, _ => true, wallet.Count));
            Assert.AreEqual(HouseCraftBlock.NoStation, HouseCraftRules.Check(stool, true, (f, id) => false, _ => true, wallet.Count));
            Assert.AreEqual(HouseCraftBlock.MissingMaterials, HouseCraftRules.Check(stool, true, bench, _ => true, wallet.Count));
            wallet.Items["item_wood"] = 4;
            Assert.AreEqual(HouseCraftBlock.None, HouseCraftRules.Check(stool, true, bench, _ => true, wallet.Count));

            HouseCraftRecipe fireplace = HouseCraftRules.ForResult("casa_sala_041");
            Assert.AreEqual(HouseCraftRules.MasterBenchId, fireplace.StationItemId);
            Assert.AreEqual(HouseCraftBlock.NoStation, HouseCraftRules.Check(fireplace, true, bench, _ => true, wallet.Count), "needs the master's bench");
            Assert.AreEqual(HouseCraftBlock.BossNeeded, HouseCraftRules.Check(fireplace, true, (f, id) => true, _ => false, wallet.Count));

            Assert.IsTrue(HouseCraftRules.All.All(r => r.Kind == HouseCraftResultKind.Wardrobe
                ? Rapadura.Gameplay.Wardrobe.WardrobeCatalog.Contains(r.ResultId)
                : HouseCatalog.Get(r.ResultId) != null), "every recipe makes something that exists");
            Assert.AreEqual(HouseCraftRules.All.Count, HouseCraftRules.All.Select(r => r.Id).Distinct().Count());
        }

        [Test]
        public void SourceRecords_RoundTripThroughJsonUtility()
        {
            HouseService house = NewHouse(0.9);
            house.OnBossVictory(Wolf);
            house.OnQuestCompleted("quest_region_forest_howling_trail");
            string json = JsonUtility.ToJson(house.CaptureState());

            HouseService loaded = NewHouse(0.9);
            loaded.RestoreState(JsonUtility.FromJson<HouseSaveState>(json));
            Assert.IsTrue(loaded.Sources.FindBoss(Wolf).trophyGiven);
            Assert.AreEqual(1, loaded.Sources.FindBoss(Wolf).wins);
            Assert.IsEmpty(loaded.OnQuestCompleted("quest_region_forest_howling_trail"), "a reload never pays twice");
            Assert.IsFalse(loaded.OnBossVictory(Wolf).Contains("casa_trofeu_001"));

            var old = JsonUtility.FromJson<HouseSaveState>("{\"level\":1}");
            HouseService fromOld = NewHouse();
            fromOld.RestoreState(old);
            Assert.IsEmpty(fromOld.Sources.Claimed, "a v2 save has no source records");
        }

        [Test]
        public void ToastAndWorkbenchTexts_AreLocalized()
        {
            var toast = HouseGrantToastRelay.ForGrant(new HouseItemGrantedEvent("casa_trofeu_001", HouseGrantReason.BossTrophy, Wolf), null);
            StringAssert.Contains("Depósito", toast.Body);
            foreach (HouseGrantReason reason in Enum.GetValues(typeof(HouseGrantReason)))
            {
                Assert.IsNotNull(HouseSourceTexts.GetFallback(HouseSourceTexts.ReasonKey(reason)), reason.ToString());
            }

            foreach (HouseCraftBlock block in Enum.GetValues(typeof(HouseCraftBlock)))
            {
                string key = HouseWorkbenchPanel.BlockKey(block);
                if (key != null) Assert.IsNotNull(HouseSourceTexts.GetFallback(key), block.ToString());
            }
        }
    }
}
