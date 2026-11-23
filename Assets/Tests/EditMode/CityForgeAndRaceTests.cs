using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Two race-city mechanics of the fichas (Docs/Mapas100.md 17 and 26, Docs/Cities.md): the five stages of the Grande Forja of the
    /// Salões de Bronze (<see cref="GreatForgeRules"/>, with its refine bonus in <see cref="RefineRules"/>) and the Corrida de Shulgi of
    /// Aratta with its Fitas de Corrida, Shulgi's list and the Auroque de Aratta (<see cref="ShulgiRaceRules"/>).
    /// </summary>
    public class CityForgeAndRaceTests
    {
        private static readonly CharacterId[] Heroes = (CharacterId[])System.Enum.GetValues(typeof(CharacterId));

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        // ---------------- Grande Forja ----------------

        [Test]
        public void Forge_FiveStages_InOrder_EachWaitsForItsBossQuestAndItems()
        {
            Assert.AreEqual(5, GreatForgeRules.Stages.Count);
            for (int i = 0; i < GreatForgeRules.Stages.Count; i++)
            {
                GreatForgeStage stage = GreatForgeRules.Stages[i];
                Assert.AreEqual(i + 1, stage.Number);
                Assert.AreSame(stage, GreatForgeRules.Next(i));
                Assert.Greater(stage.Reward.Length, 0, stage.Id + " pays at once");
                foreach ((string itemId, int count) in stage.Delivery.Concat(stage.Reward))
                {
                    Assert.IsNotNull(ItemDatabase.GetById(itemId), stage.Id + ": " + itemId);
                    Assert.Greater(count, 0);
                }
            }

            Assert.IsNull(GreatForgeRules.Next(5));
            Assert.AreEqual(RegionBossRules.DarkBellowsId, GreatForgeRules.Stages[0].BossId, "libertado, acende a 1ª etapa (ficha 17)");
            Assert.AreEqual(CityQuestCatalog.SaloesGolems, GreatForgeRules.Stages[1].QuestId, "offline: pelas missões");
            Assert.AreEqual(CityQuestCatalog.SaloesBats, GreatForgeRules.Stages[2].QuestId);
            Assert.IsNotNull(Resources.Load<EnemyDefinition>("Enemies/" + GreatForgeRules.Stages[4].BossId), "the last stage's boss exists");

            var bag = new Dictionary<string, int>();
            int Count(string id) => bag.TryGetValue(id, out int n) ? n : 0;
            var beaten = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(GreatForgeCheck.NeedBoss, GreatForgeRules.Check(0, beaten.Contains, done.Contains, Count));
            beaten.Add(RegionBossRules.DarkBellowsId);
            Assert.AreEqual(GreatForgeCheck.Ready, GreatForgeRules.Check(0, beaten.Contains, done.Contains, Count), "the Bellows burns nothing");
            Assert.AreEqual(GreatForgeCheck.NeedQuest, GreatForgeRules.Check(1, beaten.Contains, done.Contains, Count));
            done.Add(CityQuestCatalog.SaloesGolems);
            Assert.AreEqual(GreatForgeCheck.NeedItems, GreatForgeRules.Check(1, beaten.Contains, done.Contains, Count));
            bag["item_iron_ore"] = 10;
            bag["item_wood"] = 9;
            Assert.AreEqual(GreatForgeCheck.NeedItems, GreatForgeRules.Check(1, beaten.Contains, done.Contains, Count), "one wood short");
            bag["item_wood"] = 10;
            Assert.AreEqual(GreatForgeCheck.Ready, GreatForgeRules.Check(1, beaten.Contains, done.Contains, Count));
            Assert.AreEqual(GreatForgeCheck.AllLit, GreatForgeRules.Check(5, null, null, null));
            Assert.AreEqual(GreatForgeCheck.NeedBoss, GreatForgeRules.Check(0, null, null, null), "nothing known: nothing counts");
        }

        [Test]
        public void Forge_Services_GrowWithTheStages()
        {
            float[] refine = { 0f, 0.02f, 0.02f, 0.02f, 0.04f, 0.05f };
            int[] blessings = { 0, 0, 1, 1, 1, 2 };
            int[] runes = { 1, 1, 1, 2, 2, 2 };
            for (int lit = 0; lit <= 5; lit++)
            {
                Assert.AreEqual(refine[lit], GreatForgeRules.RefineBonus(lit), 1e-6f, "refine at " + lit);
                Assert.AreEqual(blessings[lit], GreatForgeRules.DailyBlessings(lit), "blessings at " + lit);
                Assert.AreEqual(runes[lit], GreatForgeRules.RunesPerDay(lit), "runes at " + lit);
            }

            Assert.AreEqual(GreatForgeRules.RefineBonus(5), GreatForgeRules.RefineBonus(9), "clamped");
            Assert.AreEqual(0f, GreatForgeRules.RefineBonus(-1));
            const long today = 20415L;
            Assert.IsFalse(GreatForgeRules.GiftReady(1, 0L, today), "the Bellows gives no stones");
            Assert.IsTrue(GreatForgeRules.GiftReady(2, today - 1L, today));
            Assert.IsFalse(GreatForgeRules.GiftReady(2, today, today), "once a UTC day");

            var progress = new WorldMapProgress();
            Assert.AreEqual(0, GreatForgeRules.StagesLit(progress));
            progress.SetDailyMark(GreatForgeRules.StageKey, 3);
            Assert.AreEqual(3, GreatForgeRules.StagesLit(progress));
            Assert.AreEqual(0, GreatForgeRules.StagesLit(null));
        }

        [Test]
        public void Forge_RefineBonus_OnlyOnRollsThatCanFail_UnderTheCap()
        {
            int failing = Enumerable.Range(0, RefineRules.MaxLevel).First(RefineRules.CanFail);
            float plain = RefineRules.ChanceFor(failing, false);
            Assert.AreEqual(plain, RefineRules.ChanceFor(failing, false, false, 0f), 1e-6f, "no forge, no change");
            Assert.AreEqual(Mathf.Min(RefineRules.MaxChance, plain + 0.05f), RefineRules.ChanceFor(failing, false, false, 0.05f), 1e-6f);
            Assert.LessOrEqual(RefineRules.ChanceFor(failing, true, true, 0.05f), RefineRules.MaxChance);

            int safe = Enumerable.Range(0, RefineRules.MaxLevel).FirstOrDefault(l => !RefineRules.CanFail(l));
            if (!RefineRules.CanFail(safe))
            {
                Assert.AreEqual(RefineRules.ChanceFor(safe, false), RefineRules.ChanceFor(safe, false, false, 0.05f), 1e-6f, "a sure roll stays sure");
            }

            double roll = plain + 0.01;
            Assert.IsFalse(RefineRules.Resolve(failing, false, roll, false, 1).Success);
            Assert.IsTrue(RefineRules.Resolve(failing, false, roll, false, 1, 0.05f).Success, "the lit forge turns a near miss");

            Assert.AreEqual(0f, RefineService.ForgeBonus(), 1e-6f, "no save: no forge");
            var progress = new WorldMapProgress();
            progress.SetDailyMark(GreatForgeRules.StageKey, 5);
            ServiceLocator.Register(progress);
            Assert.AreEqual(0.05f, RefineService.ForgeBonus(), 1e-6f);
        }

        [Test]
        public void Forge_TheMouthStandsInFrontOfTheForge_InsideTheHalls()
        {
            CityTownRecipe recipe = CityTownRecipes.BronzeHalls();
            CityProp forge = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.GreatForge);
            Vector2 mouth = CityTownRecipes.BronzeHallsForgeMouth;
            Assert.Less(mouth.y, forge.Position.y - TownKit.GreatForgeSize.y * 0.5f, "in front of the face, out of its collider");
            Assert.Less(Vector2.Distance(mouth, forge.Position), 4f);
            Assert.LessOrEqual(Mathf.Abs(mouth.x), CityTownRecipes.BronzeHallsHalfExtent);
            Assert.LessOrEqual(Mathf.Abs(mouth.y), CityTownRecipes.BronzeHallsHalfExtent);
            AssertInCity(OverworldLayouts.NapirasuId, OverworldLayouts.KaggenAnchorZoneId, OverworldLayouts.SaloesOffset + mouth, CityTownRecipes.BronzeHallsCityId);
        }

        // ---------------- Corrida de Shulgi ----------------

        [Test]
        public void Race_Medals_Ribbons_AndTheFirstOfTheDay()
        {
            Assert.AreEqual(NartesRaceMedal.Gold, ShulgiRaceRules.MedalFor(ShulgiRaceRules.GoldSeconds));
            Assert.AreEqual(NartesRaceMedal.Silver, ShulgiRaceRules.MedalFor(ShulgiRaceRules.GoldSeconds + 0.1f));
            Assert.AreEqual(NartesRaceMedal.Bronze, ShulgiRaceRules.MedalFor(ShulgiRaceRules.BronzeSeconds));
            Assert.AreEqual(NartesRaceMedal.None, ShulgiRaceRules.MedalFor(ShulgiRaceRules.BronzeSeconds + 0.1f));
            Assert.Less(ShulgiRaceRules.BronzeSeconds, NartesRaceRules.TimeLimitSeconds, "a bronze is possible before the session times out");

            Assert.AreEqual(3, ShulgiRaceRules.RibbonsFor(NartesRaceMedal.Gold, false));
            Assert.AreEqual(5, ShulgiRaceRules.RibbonsFor(NartesRaceMedal.Gold, true));
            Assert.AreEqual(2, ShulgiRaceRules.RibbonsFor(NartesRaceMedal.Silver, false));
            Assert.AreEqual(1, ShulgiRaceRules.RibbonsFor(NartesRaceMedal.Bronze, false));
            Assert.AreEqual(0, ShulgiRaceRules.RibbonsFor(NartesRaceMedal.None, true), "no medal, no Fita");
        }

        [Test]
        public void Race_Course_ThroughTheSavanna_RunnersGetBronze_RidersGold()
        {
            OverworldLayout layout = OverworldLayouts.ForMap(OverworldLayouts.AuroquesId);
            Assert.IsNotNull(layout);
            Vector2 origin = layout.Zone(OverworldLayouts.CostaAnchorZoneId).Origin;
            Vector2[] course = OverworldLayouts.ShulgiRaceCourse;
            Assert.AreEqual(OverworldLayouts.ShulgiRaceFinish, course[course.Length - 1]);

            float length = 0f;
            Vector2 at = OverworldLayouts.ShulgiRaceStart;
            foreach (Vector2 gate in course)
            {
                length += Vector2.Distance(at, gate);
                at = gate;
                OverworldZone zone = OverworldRules.ZoneAt(layout, origin + gate);
                Assert.IsNotNull(zone, gate.ToString());
                Assert.AreEqual(OverworldLayouts.AuroquesId, zone.MapId, gate + " is in the savannas of map 26");
                Assert.Greater(Vector2.Distance(gate, OverworldLayouts.TouroDeFumacaArena), 60f, gate + " keeps off the Touro de Fumaça's ring");
                Assert.Greater(Vector2.Distance(gate, OverworldLayouts.ArattaOffset), 30f, gate + " is out in the savanna");
            }

            Assert.That(length, Is.InRange(800f, 950f), "about 860 m");
            float onFoot = length / MountRules.HeroRunSpeed;
            Assert.AreEqual(NartesRaceMedal.Bronze, ShulgiRaceRules.MedalFor(onFoot), "a cousin running gets bronze");
            Assert.AreEqual(NartesRaceMedal.Gold, ShulgiRaceRules.MedalFor(length / (MountRules.HeroRunSpeed * MountRules.MaxRideMultiplier)), "a fast mount at full gallop gets gold");

            var session = new NartesRaceSession(course);
            session.Tick(NartesRaceRules.CountdownSeconds + 0.01f, OverworldLayouts.ShulgiRaceStart);
            foreach (Vector2 gate in course)
            {
                Assert.IsTrue(session.Tick(5f, gate), "gate " + gate);
            }

            Assert.AreEqual(NartesRaceState.Finished, session.State);
        }

        [Test]
        public void Race_StartAndRibbonPost_StandInsideAratta()
        {
            AssertInCity(OverworldLayouts.AuroquesId, OverworldLayouts.CostaAnchorZoneId, OverworldLayouts.ShulgiRaceStart, CityTownRecipes.ArattaCityId);
            AssertInCity(OverworldLayouts.AuroquesId, OverworldLayouts.CostaAnchorZoneId, OverworldLayouts.ShulgiRibbonPost, CityTownRecipes.ArattaCityId);
            CityTownRecipe aratta = CityTownRecipes.Aratta();
            Assert.IsTrue(aratta.PlazaProps.Any(p => p.Kind == CityPropKind.HornPost && Vector2.Distance(p.Position, CityTownRecipes.ArattaRibbonPost) < 1.5f),
                "the ribbon post is by a horn post");
            Assert.AreEqual(4, aratta.PlazaProps.Count(p => p.Kind == CityPropKind.HornPost && Mathf.Abs(p.Position.x) < 3f
                && Mathf.Abs(p.Position.y - CityTownRecipes.ArattaRaceStart.y) < 4f), "the start is between the four horn posts of the South street");
            Assert.Greater(Vector2.Distance(OverworldLayouts.ShulgiRaceStart, OverworldLayouts.ShulgiRibbonPost),
                ShulgiRaceRules.StartReach + ShulgiRaceRules.TradeReach, "the two [E] points don't overlap");
        }

        [Test]
        public void Ribbons_ShulgiTradesTheAurochsFirst_ThenTheTwoCosmetics()
        {
            Assert.AreEqual(ShulgiOfferKind.Mount, ShulgiRaceRules.Offers[0].Kind);
            Assert.AreEqual(MountSheetCatalog.ArattaAurochs, ShulgiRaceRules.Offers[0].Id);
            Assert.AreEqual(MountDropRules.RareRibbonCost, ShulgiRaceRules.Offers[0].Cost, "25, like a boss's Rare");
            Assert.AreEqual(MountDropRules.CostFor(MountCatalog.FindSpec(MountSheetCatalog.ArattaAurochs).Rarity), MountDropRules.CostOf(ShulgiRaceRules.RibbonKey,
                MountSheetCatalog.ArattaAurochs));
            foreach (ShulgiOffer offer in ShulgiRaceRules.Offers.Where(o => o.Kind == ShulgiOfferKind.Cosmetic))
            {
                MountCosmeticSpec spec = MountCosmetics.Get(offer.Id);
                Assert.IsNotNull(spec, offer.Id);
                Assert.AreEqual(offer.Cost, spec.RaceRibbons, offer.Id);
                Assert.IsFalse(spec.SoldForGold, offer.Id + " is not in the Selaria");
                Assert.IsFalse(MountCosmetics.ForGold.Contains(spec));
                Assert.IsTrue(MountTexts.Entries.Any(e => e.Key == spec.NameKey), spec.NameKey);
            }

            Assert.AreEqual(MountCosmetics.All.Count - 2, MountCosmetics.ForGold.Count(), "the Selaria keeps its six");

            var owned = new HashSet<string>();
            Assert.AreEqual(0, ShulgiRaceRules.NextOffer(o => owned.Contains(o.Id)));
            owned.Add(MountSheetCatalog.ArattaAurochs);
            Assert.AreEqual(1, ShulgiRaceRules.NextOffer(o => owned.Contains(o.Id)), "the quest's aurochs: on to the blanket");
            owned.UnionWith(ShulgiRaceRules.Offers.Select(o => o.Id));
            Assert.AreEqual(-1, ShulgiRaceRules.NextOffer(o => owned.Contains(o.Id)));
            Assert.AreEqual(ShulgiTradeCheck.AllOwned, ShulgiRaceRules.CheckTrade(-1, 99));
            Assert.AreEqual(ShulgiTradeCheck.NeedRibbons, ShulgiRaceRules.CheckTrade(0, 24));
            Assert.AreEqual(ShulgiTradeCheck.Ready, ShulgiRaceRules.CheckTrade(0, 25));
        }

        [Test]
        public void Ribbons_LiveInTheCollection_ApartFromEveryBoss_AndBuyTheAurochsAndACosmetic()
        {
            int n = 0;
            var collection = new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L, DropRandom = new System.Random(1), IsNight = () => false };
            collection.Initialize();
            try
            {
                Assert.IsNull(Resources.Load<EnemyDefinition>("Enemies/" + ShulgiRaceRules.RibbonKey), "no enemy has the race's key");
                collection.AddRibbons(ShulgiRaceRules.RibbonKey, 30);
                Assert.AreEqual(0, collection.RibbonsOf(RegionBossRules.AydaharId), "never mixed with a boss's Fitas");
                Assert.IsNull(collection.RedeemRibbons(ShulgiRaceRules.RibbonKey, MountBossCatalog.PamirArgali), "only Shulgi's list");
                Assert.IsNotNull(collection.RedeemRibbons(ShulgiRaceRules.RibbonKey, MountSheetCatalog.ArattaAurochs));
                Assert.IsTrue(collection.Owns(MountSheetCatalog.ArattaAurochs));
                Assert.AreEqual(5, collection.RibbonsOf(ShulgiRaceRules.RibbonKey));

                Assert.IsFalse(collection.SpendRibbons(ShulgiRaceRules.RibbonKey, 6), "not enough: nothing taken");
                Assert.AreEqual(5, collection.RibbonsOf(ShulgiRaceRules.RibbonKey));
                collection.AddRibbons(ShulgiRaceRules.RibbonKey, 1);
                Assert.IsTrue(collection.GrantCosmetic(ShulgiRaceRules.BlanketId) && collection.SpendRibbons(ShulgiRaceRules.RibbonKey, 6));
                Assert.IsTrue(collection.OwnsCosmetic(ShulgiRaceRules.BlanketId));
                Assert.AreEqual(0, collection.RibbonsOf(ShulgiRaceRules.RibbonKey));
            }
            finally
            {
                collection.Shutdown();
            }
        }

        // ---------------- Texts ----------------

        [Test]
        public void Texts_BothLanguages_EveryCousin_EveryStage_EveryMedal()
        {
            var keys = new HashSet<string>(CityServiceTexts.Entries.Select(e => e.Key));
            Assert.AreEqual(CityServiceTexts.Entries.Count, keys.Count, "no key twice");
            foreach (CharacterId hero in Heroes)
            {
                Assert.IsTrue(keys.Contains(CityServiceTexts.RaceLine(hero)), "race line " + hero);
                Assert.IsTrue(keys.Contains(CityServiceTexts.RibbonLine(hero)), "ribbon line " + hero);
                Assert.IsTrue(keys.Contains(CityServiceTexts.ForgeLine(hero)), "forge line " + hero);
                Assert.IsTrue(keys.Contains(CityServiceTexts.ForgeDoneLine(hero)), "forge done line " + hero);
            }

            foreach (GreatForgeStage stage in GreatForgeRules.Stages)
            {
                Assert.IsTrue(keys.Contains(CityServiceTexts.ForgeNeed(stage.Id)), "need " + stage.Id);
                Assert.IsTrue(keys.Contains(CityServiceTexts.ForgeLit(stage.Id)), "lit " + stage.Id);
            }

            foreach (NartesRaceMedal medal in new[] { NartesRaceMedal.Gold, NartesRaceMedal.Silver, NartesRaceMedal.Bronze })
            {
                Assert.IsTrue(keys.Contains(CityServiceTexts.MedalName(medal)), medal.ToString());
            }

            foreach (string key in new[]
                     {
                         CityServiceTexts.RacePrompt, CityServiceTexts.RaceGo, CityServiceTexts.RaceMedal, CityServiceTexts.RibbonPrompt, CityServiceTexts.RibbonNeed,
                         CityServiceTexts.RibbonTraded, CityServiceTexts.ForgePrompt, CityServiceTexts.ForgeGift, CityServiceTexts.ForgeAllLit,
                     })
            {
                Assert.IsTrue(keys.Contains(key), key);
            }

            Assert.AreEqual("Fita de ouro em 101.5 s: 5 Fitas de Corrida (vocês têm 12)",
                string.Format(CityServiceTexts.Get(null, CityServiceTexts.RaceMedal), CityServiceTexts.Get(null, CityServiceTexts.MedalName(NartesRaceMedal.Gold)), "101.5", 5, 12));
        }

        private static void AssertInCity(string mapId, string anchorZoneId, Vector2 regionLocal, string cityId)
        {
            OverworldLayout layout = OverworldLayouts.ForMap(mapId);
            Assert.IsNotNull(layout, mapId);
            OverworldZone anchor = layout.Zone(anchorZoneId);
            Assert.IsNotNull(anchor, anchorZoneId);
            OverworldSettlement city = layout.Settlements.FirstOrDefault(s => s.Recipe != null && s.Recipe.CityId == cityId);
            Assert.IsNotNull(city, cityId);
            Assert.AreSame(city, OverworldRules.SettlementAt(layout, anchor.Origin + regionLocal), $"{cityId}: {regionLocal} inside the city's safe zone");
        }
    }
}
