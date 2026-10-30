using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Daily;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using Rapadura.UI.Housing;

namespace Rapadura.Tests
{
    /// <summary>
    /// H6 "Vida na casa" rules (Housing.md §6, §11.9, §4.6): comfort (2-copy cap, depot does not count, wilted plants
    /// count half), sets (3 / 6 / all, distinct pieces), the "Descansado" tiers, charges and fights, its place in the
    /// kill-XP chain, plant moods (never die), and the offline guest book.
    /// </summary>
    public class HouseLifeRulesTests
    {
        private static HouseItemDefinition Item(string id, int comfort, string set = "", HouseItemCategory category = HouseItemCategory.LivingRoom)
        {
            return new HouseItemDefinition(id, id, category, ItemRarity.Common, HousePlacement.Floor, 1, 1, 90, 0, HouseItemFunction.None, 0, comfort,
                set, 10, 0, true, true, false, 1, HouseSeason.None);
        }

        private static HousePlacedData Placed(int id, string itemId) => new HousePlacedData { instanceId = id, itemId = itemId, layer = HouseLayer.Floor };

        // ------------------------------------------------------------------
        // Comfort
        // ------------------------------------------------------------------

        [Test]
        public void Comfort_CountsAtMostTwoCopiesOfAnId()
        {
            var catalog = new[] { Item("stool", 2), Item("sofa", 5) };
            var placed = new[] { Placed(1, "stool"), Placed(2, "stool"), Placed(3, "stool"), Placed(4, "stool"), Placed(5, "sofa") };
            HouseComfortReport report = HouseComfortRules.Evaluate(placed, id => catalog.FirstOrDefault(i => i.ItemId == id), null, catalog);
            Assert.AreEqual(2 + 2 + 5, report.ItemComfort, "four stools count as two");
            Assert.AreEqual(0, report.SetComfort);
            Assert.AreEqual(0, HouseComfortRules.Evaluate(new HousePlacedData[0], id => null, null, catalog).Total, "an empty house (or a full depot) is 0");
        }

        [Test]
        public void Comfort_WiltedPlantLosesHalf()
        {
            var catalog = new[] { Item("fern", 5, category: HouseItemCategory.Plant), Item("chair", 5) };
            var placed = new[] { Placed(1, "fern"), Placed(2, "chair") };
            HouseComfortReport report = HouseComfortRules.Evaluate(placed, id => catalog.FirstOrDefault(i => i.ItemId == id), id => true, catalog);
            Assert.AreEqual(2 + 5, report.ItemComfort, "only plants wilt (5 → 2)");
            Assert.AreEqual(1, report.WiltedPlants);
            Assert.AreEqual(0, HouseComfortRules.PieceComfort(Item("tiny", 1, category: HouseItemCategory.Plant), true));
            Assert.AreEqual(1, HouseComfortRules.PieceComfort(Item("tiny", 1, category: HouseItemCategory.Plant), false), "watered: back in full");
        }

        [Test]
        public void Sets_ThreeSixAndAll_CountDistinctPieces_HighestStageOnly()
        {
            var catalog = Enumerable.Range(1, 8).Select(i => Item("set_piece_" + i, 1, "set_luz_maria")).ToList();
            HouseItemDefinition Resolve(string id) => catalog.FirstOrDefault(i => i.ItemId == id);

            HouseSetProgress Progress(params int[] pieces)
            {
                var placed = pieces.Select((p, n) => Placed(n + 1, "set_piece_" + p)).ToArray();
                return HouseComfortRules.Evaluate(placed, Resolve, null, catalog).Sets.First(s => s.Set.Id == "set_luz_maria");
            }

            Assert.AreEqual(0, Progress(1, 2).Stage);
            Assert.AreEqual(0, Progress(1, 1, 1).Stage, "copies are not pieces");
            Assert.AreEqual(1, Progress(1, 2, 3).Stage);
            Assert.AreEqual(5, Progress(1, 2, 3).Comfort);
            Assert.AreEqual(2, Progress(1, 2, 3, 4, 5, 6).Stage);
            Assert.AreEqual(12, Progress(1, 2, 3, 4, 5, 6).Comfort);
            HouseSetProgress all = Progress(1, 2, 3, 4, 5, 6, 7, 8);
            Assert.AreEqual(3, all.Stage);
            Assert.AreEqual(25, all.Comfort, "the highest stage counts, not the sum");
            Assert.IsTrue(all.PerkActive, "Luz da Maria's gift");

            var report = HouseComfortRules.Evaluate(Enumerable.Range(1, 8).Select(i => Placed(i, "set_piece_" + i)), Resolve, null, catalog);
            Assert.AreEqual(8 + 25, report.Total);
            Assert.IsTrue(report.HasPerk(HouseSetPerk.RestedExtraFights));
        }

        [Test]
        public void Sets_TheCatalogsTwentyOneSets_AreAllKnown_WithTheDocsPieceCounts()
        {
            var catalogSets = HouseCatalog.All.Select(i => i.SetId).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            Assert.AreEqual(21, HouseSetCatalog.All.Count);
            CollectionAssert.AreEquivalent(catalogSets, HouseSetCatalog.All.Select(s => s.Id));
            Assert.AreEqual(26, HouseSetCatalog.PieceCount("set_varanda_vo_zefa", HouseCatalog.All));
            Assert.AreEqual(7, HouseSetCatalog.PieceCount("set_cantinho_caramelo", HouseCatalog.All));
            Assert.AreEqual(6, HouseSetCatalog.PieceCount("set_cristal_sol", HouseCatalog.All));
            Assert.AreEqual(HouseSetAmbient.LilacFireflies, HouseSetCatalog.Get("set_noite_rubra_lavine").Ambient);
            Assert.AreEqual(HouseSetAmbient.WorkshopSparks, HouseSetCatalog.Get("set_oficina_icaro").Ambient);
            Assert.AreEqual(HouseSetAmbient.CarameloWag, HouseSetCatalog.Get("set_cantinho_caramelo").Ambient);
            Assert.AreEqual(HouseSetPerk.RestedExtraFights, HouseSetCatalog.Get("set_luz_maria").Perk);

            var keys = HouseLifeTexts.Entries.Select(e => e.Key).ToHashSet();
            foreach (HouseSetDefinition set in HouseSetCatalog.All)
            {
                Assert.IsTrue(keys.Contains(set.NameKey), set.Id);
                Assert.IsTrue(keys.Contains(set.TitleKey), set.Id);
            }
        }

        // ------------------------------------------------------------------
        // Descansado
        // ------------------------------------------------------------------

        [Test]
        public void RestedTiers_FollowTheDocTable()
        {
            Assert.AreEqual(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4 }, new[] { 0, 19, 20, 59, 60, 149, 150, 299, 300 }.Select(HouseRestedRules.TierOf).ToArray());
            Assert.AreEqual(new[] { 2, 2, 3, 3, 3 }, Enumerable.Range(0, 5).Select(HouseRestedRules.PercentOf).ToArray(), "capped at +3% (power-stack budget)");
            Assert.AreEqual(new[] { 10, 15, 20, 25, 30 }, Enumerable.Range(0, 5).Select(HouseRestedRules.FightsOf).ToArray());
            Assert.AreEqual(1, HouseRestedRules.ComfortToNextTier(19));
            Assert.AreEqual(0, HouseRestedRules.ComfortToNextTier(999));
            Assert.AreEqual(0.9f, HouseRestedRules.ExtraFightsWorthOfXp(4), 1e-4f, "the best night is worth less than one fight of XP");
        }

        [Test]
        public void Rested_TwoNightsADay_NoCarryOver_NewNightReplaces()
        {
            var state = new HouseRestedState();
            const int day1 = 20261030, day2 = 20261031;
            Assert.IsTrue(HouseRestedRules.Sleep(state, day1, tier: 2));
            Assert.AreEqual(3, state.percent);
            Assert.AreEqual(20, state.fightsLeft);
            Assert.IsTrue(HouseRestedRules.Sleep(state, day1, tier: 0));
            Assert.AreEqual(2, state.percent, "replaces, never stacks");
            Assert.AreEqual(10, state.fightsLeft, "not 30");
            Assert.IsFalse(HouseRestedRules.Sleep(state, day1, tier: 4), "only twice a day");
            Assert.AreEqual(0, HouseRestedRules.ChargesLeft(state, day1));
            Assert.AreEqual(2, HouseRestedRules.ChargesLeft(state, day2), "a new day refills to 2, not 4");
            Assert.IsTrue(HouseRestedRules.Sleep(state, day2, tier: 4, extraFights: HouseSetCatalog.LuzMariaExtraFights));
            Assert.AreEqual(35, state.fightsLeft, "Luz da Maria: +5 fights");
            Assert.AreEqual(1.03f, HouseRestedRules.Multiplier(state), 1e-5f);

            for (int i = 0; i < 35; i++) HouseRestedRules.ConsumeFight(state);
            Assert.IsFalse(HouseRestedRules.IsRested(state));
            Assert.AreEqual(1f, HouseRestedRules.Multiplier(state));
            HouseRestedRules.ConsumeFight(state);
            Assert.AreEqual(0, state.fightsLeft, "never negative");
        }

        [Test]
        public void RestedXp_IsOneMoreMultiplierInTheChain()
        {
            Assert.AreEqual(55, HouseRestedRules.ScaleExperience(50, 1.1f));
            Assert.AreEqual(1, HouseRestedRules.ScaleExperience(1, 1.02f), "rounds, a positive reward stays positive");
            Assert.AreEqual(50, HouseRestedRules.ScaleExperience(50, 1f));
            Assert.AreEqual(0, HouseRestedRules.ScaleExperience(0, 1.1f));
            // Composes with the fôlego tier at the same choke point (EnemyRewardGranter): 100 × 1.3 → 130 × 1.1 → 143.
            int stamina = DailyStaminaRules.ScaleExperience(100, 1.3f);
            Assert.AreEqual(143, HouseRestedRules.ScaleExperience(stamina, 1.1f));
        }

        [Test]
        public void RestedMultiplier_ResolvesFromTheLiveHouse_OrIsOne()
        {
            bool had = ServiceLocator.TryGet(out HouseService previous);
            try
            {
                ServiceLocator.Unregister<HouseService>();
                Assert.AreEqual(1f, HouseService.ResolveRestedMultiplier());
                var house = new HouseService(() => 3);
                house.GrantHouse();
                house.MarkEntered();
                ServiceLocator.Register(house);
                Assert.AreEqual(1f, HouseService.ResolveRestedMultiplier(), "not rested yet");
                Assert.IsTrue(house.RestInBed());
                Assert.AreEqual(1.02f, HouseService.ResolveRestedMultiplier(), 1e-5f, "an empty house: tier 0");
                for (int i = 0; i < 10; i++) house.ConsumeRestedFight();
                Assert.AreEqual(1f, HouseService.ResolveRestedMultiplier(), "ten fights later it is over");
            }
            finally
            {
                ServiceLocator.Unregister<HouseService>();
                if (had) ServiceLocator.Register(previous);
            }
        }

        // ------------------------------------------------------------------
        // Plants
        // ------------------------------------------------------------------

        [Test]
        public void PlantMood_HappyThreeDays_ThenWilts_NeverDies()
        {
            Assert.AreEqual(HousePlantMood.Happy, HousePlantRules.MoodOf(10, 10));
            Assert.AreEqual(HousePlantMood.Happy, HousePlantRules.MoodOf(10, 12));
            Assert.AreEqual(HousePlantMood.Thirsty, HousePlantRules.MoodOf(10, 13));
            Assert.AreEqual(HousePlantMood.Wilted, HousePlantRules.MoodOf(10, 14));
            Assert.AreEqual(HousePlantMood.Wilted, HousePlantRules.MoodOf(10, 10000), "a year later: still there, just drooping");
            Assert.AreEqual(HousePlantMood.Happy, HousePlantRules.MoodOf(10, 5), "a clock that went back is not a problem");
        }

        [Test]
        public void PlantTimers_NewPlantsStartWatered_PutAwayOnesForgotten_WaterPerksAll()
        {
            var fern = Item("fern", 3, category: HouseItemCategory.Plant);
            var chair = Item("chair", 3);
            HouseItemDefinition Resolve(string id) => id == "fern" ? fern : id == "chair" ? chair : null;
            var timers = HousePlantRules.Sync(null, new[] { Placed(1, "fern"), Placed(2, "chair"), Placed(3, "fern") }, Resolve, 5);
            Assert.AreEqual(new[] { 1, 3 }, timers.Select(t => t.instanceId).ToArray());
            Assert.IsTrue(timers.All(t => t.lastWateredDay == 5));

            timers = HousePlantRules.Sync(timers, new[] { Placed(3, "fern") }, Resolve, 9);
            Assert.AreEqual(1, timers.Count);
            Assert.AreEqual(5, timers[0].lastWateredDay, "keeps its own timer");
            Assert.AreEqual(1, HousePlantRules.Water(timers, 9), "it was wilted");
            Assert.AreEqual(9, timers[0].lastWateredDay);
            Assert.AreEqual(0, HousePlantRules.Water(timers, 9));
        }

        [Test]
        public void House_WiltedPlantsLowerComfort_WateringBringsItBack()
        {
            int day = 1;
            var house = new HouseService(() => 3) { GameDayProvider = () => day };
            house.GrantHouse();
            house.MarkEntered();
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.At("casa_planta_001", HouseLayer.Ceiling, 3, 3, 0), out int fern));
            int fresh = house.Comfort.Total;
            day = 4;
            Assert.AreEqual(HousePlantMood.Thirsty, house.PlantMood(fern));
            Assert.AreEqual(fresh, house.Comfort.Total, "thirsty still counts in full");
            day = 6;
            Assert.AreEqual(HousePlantMood.Wilted, house.PlantMood(fern));
            Assert.Less(house.Comfort.Total, fresh);
            Assert.AreEqual(1, house.PlantsNeedingWater);
            Assert.AreEqual(1, house.WaterPlants());
            Assert.AreEqual(fresh, house.Comfort.Total);
            Assert.IsNotNull(house.Layout.Find(fern), "never removed");
        }

        // ------------------------------------------------------------------
        // Guest book
        // ------------------------------------------------------------------

        [Test]
        public void Visits_NeverTheActiveCousin_NpcsComeWithTheBosses()
        {
            List<string> start = HouseVisitRules.Candidates("Maithe", 0);
            CollectionAssert.DoesNotContain(start, "Maithe");
            CollectionAssert.IsSubsetOf(new[] { "Maria", "Joaquim", "Icaro", "Lavine", HouseVisitRules.VoZefa, HouseVisitRules.DonaCida }, start);
            CollectionAssert.DoesNotContain(start, HouseVisitRules.SeuBento);
            List<string> late = HouseVisitRules.Candidates("Maithe", 3);
            CollectionAssert.IsSubsetOf(new[] { HouseVisitRules.SeuBento, HouseVisitRules.MestreTonho, HouseVisitRules.Benedita }, late);
            Assert.IsTrue(HouseVisitRules.CanVisit(3, 4, true));
            Assert.IsFalse(HouseVisitRules.CanVisit(4, 4, true), "one visit a day");
            Assert.IsFalse(HouseVisitRules.CanVisit(3, 4, false), "needs the guest book");
            Assert.AreEqual(HouseVisitRules.PickVisitor(12, late, null), HouseVisitRules.PickVisitor(12, late, null), "deterministic by day");
        }

        [Test]
        public void GuestBook_KeepsTheNewestHundred_AndEveryLineIsLocalized()
        {
            var book = new List<HouseGuestEntry>();
            for (int i = 0; i < 130; i++) HouseVisitRules.Append(book, new HouseGuestEntry { visitorId = "Maria", day = i });
            Assert.AreEqual(HouseVisitRules.MaxEntries, book.Count);
            Assert.AreEqual(30, book[0].day, "oldest dropped");

            var keys = HouseLifeTexts.Entries.Select(e => e.Key).ToHashSet();
            foreach (string visitor in HouseVisitRules.Cousins.Concat(HouseVisitRules.Npcs))
            {
                Assert.IsTrue(keys.Contains(HouseLifeTexts.VisitorKey(visitor)), visitor);
                for (int line = 0; line < HouseVisitRules.LinesPerVisitor; line++) Assert.IsTrue(keys.Contains(HouseLifeTexts.VisitLineKey(visitor, line)), visitor + line);
            }

            foreach (HouseQuestDefinition quest in HouseQuestChain.All)
            {
                Assert.IsTrue(keys.Contains(quest.NameKey), quest.Id);
                Assert.IsTrue(keys.Contains(quest.GoalKey), quest.Id);
                foreach (string reward in quest.Rewards) Assert.IsNotNull(HouseCatalog.Get(reward), reward);
                Assert.IsTrue(HouseCatalog.All.Any(i => i.Sources.Any(s => s.RefId == quest.Id)), quest.Id + " is a catalog source");
            }

            for (int level = 2; level <= 5; level++) Assert.IsTrue(keys.Contains(HouseLifeTexts.SceneKey(level)), "moving day N" + level);
            for (int tier = 0; tier < HouseRestedRules.TierCount; tier++) Assert.IsTrue(keys.Contains(HouseLifeTexts.TierKey(tier)));
        }

        [Test]
        public void Visit_SignsOncePerDay_OnlyWithTheBookPlaced()
        {
            int day = 1;
            var house = new HouseService(() => 3) { GameDayProvider = () => day, BossDefeatedProvider = _ => false };
            house.GrantHouse();
            house.MarkEntered();
            Assert.IsNull(house.TryVisit("Maithe"), "the book is still in the depot");
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.At("casa_sala_004", HouseLayer.Floor, 4, 3, 0), out int table));
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.OnSurface("casa_util_023", table, 0), out _));
            HouseGuestEntry entry = house.TryVisit("Maithe");
            Assert.IsNotNull(entry);
            Assert.AreNotEqual("Maithe", entry.visitorId);
            Assert.IsNull(house.TryVisit("Maithe"), "one a day");
            day = 2;
            Assert.IsNotNull(house.TryVisit("Maithe"));
            Assert.AreEqual(2, house.GuestBook.Count);
        }
    }
}
