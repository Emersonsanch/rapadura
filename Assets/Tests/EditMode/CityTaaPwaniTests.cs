using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Taa Pwani as a walkable late-region city (Docs/Cities.md, Docs/Mapas100.md 25): its <see cref="CitySpec"/> outside the
    /// one-hub-per-region list, the five Swahili sailors and merchants of the ficha at their doors with their own look, the
    /// coral-stone style, Mwana's counter with the gear of the gates whose way starts at Taa Pwani (25, 28, 30; the relic of 27
    /// is sold in Aratta and the air bubble of 29 in Kirta), the two city quests and the respawn of its maps (25 and 30).
    /// </summary>
    public class CityTaaPwaniTests
    {
        private static readonly string[] Cast = { "liyongo", "mwana", "nuru", "zawadi", "mbaraka" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly string[] RegionGates =
        {
            MapRequirementItems.DhowPassage, MapRequirementItems.ArtificerRelic, MapRequirementItems.BreathingShell,
            MapRequirementItems.AirBubble, MapRequirementItems.RootBoots,
        };

        /// <summary>
        /// The gates on Mwana's counter: the dhow out (25), the breathing shell (28: Kirta stands on that map, so the shell must
        /// be bought before reaching it) and the root boots (30). The relic (27) is Lugalbanda's in Aratta, the air bubble (29)
        /// Danel's in Kirta.
        /// </summary>
        private static readonly string[] MwanaGates =
        {
            MapRequirementItems.DhowPassage, MapRequirementItems.BreathingShell, MapRequirementItems.RootBoots,
        };

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void TaaPwani_HasACitySpec_AsALateRegionCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec taa = CityCatalog.Get(CityCatalog.TaaPwani);
            Assert.IsNotNull(taa, "Taa Pwani has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, taa.Region, "borrows the always-unlocked Forest");
            Assert.AreEqual(MapCatalog.TaaPwani, taa.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.TaaPwaniCityId, taa.CityId, "the recipe's city id");
            Assert.IsTrue(CityCatalog.IsLateRegionCity(taa));
            Assert.IsFalse(CityCatalog.IsRaceCity(taa));
            Assert.IsFalse(CityCatalog.IsHubCity(taa));
            Assert.IsFalse(CityCatalog.Cities.Contains(taa), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.VilaZefa, CityCatalog.ForRegion(BiomeType.Forest).CityId, "the Forest hub is still the Vila");
            Assert.IsFalse(taa.HasGrandmaServices, "Baú and forno stay in Vó Edna's village");
            Assert.IsFalse(taa.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(taa.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(taa.CityId), "the city walk has a layout");
        }

        [Test]
        public void MapsOfTaaPwani_RespawnInItsPlaza()
        {
            Assert.AreEqual(CityCatalog.TaaPwani, MapCatalog.RespawnCityFor("map_025_taa_pwani").CityId);
            Assert.AreEqual(CityCatalog.TaaPwani, MapCatalog.RespawnCityFor("map_030_liyongo").CityId);
            Assert.AreEqual(CityCatalog.VilaZefa, MapCatalog.RespawnCityFor(MapCatalog.TugaiId).CityId);

            var cities = new CityProgress();
            CitySpec taa = CityCatalog.Get(CityCatalog.TaaPwani);
            Assert.AreSame(taa, cities.ApplyDefeatIn(taa), "a defeat on 25/30 wakes the cousins on the coral square");
            Assert.AreSame(taa, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void TaaPwani_IsCoralStone_WithDhowsTheLampTowerAndMangroves()
        {
            CityTownRecipe recipe = CityTownRecipes.TaaPwani();
            Assert.AreSame(TownStyles.Coast, recipe.Style);
            Assert.AreEqual(TownArchitecture.CoralStone, TownStyles.Coast.Architecture);
            Assert.AreEqual(TownVegetation.Coast, TownStyles.Coast.Vegetation);
            Assert.AreEqual(TownRoofKind.Flat, TownStyles.Coast.HouseRoof, "roof terraces");

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TaaPwani);
            Assert.AreSame(TownStyles.Coast, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(CityTownRecipes.TaaPwaniDhows.Length, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Dhow));
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CoralTower), "the Farol de Coral");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Mangrove), 3);
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Stall && p.Variant == 3), 2, "the pearl market");
            Assert.IsTrue(recipe.Lots.Where(l => l.Spec != null).All(l => l.Spec.CoralStone), "every house is coral stone");
            Assert.AreEqual(TownShopDressing.Pearls, recipe.Lots.Single(l => l.ResidentId == "mwana").Spec.Dressing);
            Assert.AreEqual(TownShopDressing.RootWeaver, recipe.Lots.Single(l => l.ResidentId == "zawadi").Spec.Dressing);

            // Other towns keep their own style (the coral flag is set only by the coast style or a hand spec).
            Assert.AreNotSame(TownStyles.Coast, CityTownRecipes.LayoutFor(CityCatalog.VilaZefa).Style);
        }

        [Test]
        public void TownPalette_AndNpcPalette_CoverEverySwatch()
        {
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, Enum.GetValues(typeof(TownSwatch)).Length);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);
            Assert.AreEqual(NpcPalette.Count, Enum.GetValues(typeof(NpcSwatch)).Length);
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void TaaPwani_Residents_AreTheFichasSailors_WithALookOfTheirOwn()
        {
            CitySpec taa = CityCatalog.Get(CityCatalog.TaaPwani);
            CollectionAssert.AreEquivalent(Cast, taa.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.ElfEars, $"{id}: human");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
            }

            Assert.AreEqual(CityShopCatalog.BazarMwana, taa.Residents.Single(r => r.ResidentId == "mwana").ShopId, "Mwana keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.BazarMwana }, taa.ShopIds.ToArray());
        }

        [Test]
        public void TaaPwani_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotStartWith("Dona ", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Capitão Liyongo, o Nakhoda", CityTexts.ResidentName(null, Resident("liyongo")));
            Assert.AreEqual("Mwana, a Mercadora de Pérolas", CityTexts.ResidentName(null, Resident("mwana")));
        }

        [Test]
        public void TaaPwani_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TaaPwani);
            List<string> problems = CityLayoutRules.Validate(layout);
            Assert.IsEmpty(problems, string.Join("\n", problems));

            var grid = CityLayoutRules.WalkGrid.Build(layout, CityLayoutRules.MinCorridor * 0.5f);
            Assert.IsTrue(grid.Flood(layout.PlazaCenter));
            foreach (string id in Cast)
            {
                CityLot lot = layout.LotOfResident(id);
                Assert.IsNotNull(lot, $"{id} has no door to stand at");
                Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, lot.StandPoint), CityLayoutRules.WalkerRadius, $"{id} stands inside a collider");
                Assert.LessOrEqual(Vector2.Distance(lot.StandPoint, lot.DoorApproach), lot.InteractRadius, $"{id} out of reach");
                Assert.IsTrue(grid.IsReached(lot.DoorApproach), $"nobody can walk up to {id}");
                Assert.IsTrue(lot.Front.x < -0.5f || lot.Front.y < -0.5f, $"{id}'s door turns its back to the camera");
                Assert.IsTrue(layout.SafeZone.Contains(lot.StandPoint));
            }

            Assert.AreEqual("casa_do_farol", layout.LotOfResident("nuru").Id, "Nuru at the foot of her lamp tower");
            Assert.AreEqual("bazar_mwana", layout.LotOfShop(CityShopCatalog.BazarMwana).Id, "Mwana's counter on the bazaar street");
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn at the plaza");
        }

        // ------------------------------------------------------------------
        // Mwana's counter
        // ------------------------------------------------------------------

        [Test]
        public void MwanasShop_SellsTheGearOfTheGatesFromTaaPwani_TheRegionsCitiesSellTheRest_AndItsCounterTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.BazarMwana);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            // Every gate of the Costa das Monções (MapCatalog 25–30) is on a counter of the region, always, from day one:
            // Mwana's for the way out of Taa Pwani, Lugalbanda's (Aratta) and Danel's (Kirta) for their own maps.
            List<string> gates = MapCatalog.Maps.Where(m => m.MapId >= 25 && m.MapId <= 30)
                .Select(m => MapRequirementRules.PrimaryItemOf(m.SpecialRequirementTag)).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            CollectionAssert.AreEquivalent(RegionGates, gates, "the gates of the region (Docs/MetodologiaMapasERacas.md §2.1)");
            var regionShops = new[] { CityShopCatalog.BazarMwana, CityShopCatalog.AlforjeLugalbanda, CityShopCatalog.MergulhoDanel };
            foreach (string itemId in RegionGates)
            {
                Assert.IsTrue(regionShops.Any(s => CityShopCatalog.EntryOf(s, itemId) != null && !CityShopCatalog.EntryOf(s, itemId).Rotating), $"{itemId} has a counter in the region");
            }

            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, MapRequirementItems.ArtificerRelic), "the relic moved to Lugalbanda's (Aratta)");
            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, MapRequirementItems.AirBubble), "the air bubble moved to Danel's (Kirta)");
            foreach (string itemId in MwanaGates)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, itemId);
                Assert.IsNotNull(entry, $"Mwana sells {itemId}");
                Assert.IsFalse(entry.Rotating, $"{itemId} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, $"{itemId} from day one");
                Assert.IsNotNull(ItemDatabase.GetById(itemId), itemId);
            }

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            int rotating = shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main);
            Assert.Greater(rotating, shop.RotatingSlots, "more lines than slots: the counter turns");
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 3));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                CollectionAssert.AreEqual(today, ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList(), "same day, same counter");
                foreach (string itemId in MwanaGates)
                {
                    Assert.Contains(itemId, today);
                }

                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void TaaPwani_OpensMwanasCounter_AndNeverTheCaravaneer_WhoStillVisitsTheVila()
        {
            CitySpec taa = CityCatalog.Get(CityCatalog.TaaPwani);
            CitySpec vila = CityCatalog.Get(CityCatalog.VilaZefa);
            bool vilaGotHim = false;
            for (int d = 0; d < 28; d++)
            {
                DateTime date = new DateTime(2026, 11, 3).AddDays(d);
                List<string> open = CityShopService.ShopsOpenIn(taa, date, null);
                Assert.Contains(CityShopCatalog.BazarMwana, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
                vilaGotHim |= CityShopService.ShopsOpenIn(vila, date, null).Contains(CityShopCatalog.MascateBento);
            }

            Assert.IsTrue(vilaGotHim, "the hub of the Forest still gets the peddler");
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void TaaPwani_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec taa = CityCatalog.Get(CityCatalog.TaaPwani);
            var keys = new List<string> { taa.NameKey, taa.ButtonKey, taa.FlavourKey, CityShopCatalog.Get(CityShopCatalog.BazarMwana).NameKey };
            foreach (string id in Cast)
            {
                CityResidentSpec resident = Resident(id);
                keys.Add(resident.NameKey);
                keys.Add(resident.AnyLineKey);
                keys.AddRange(Heroes.Select(resident.LineKey));
                Assert.AreEqual(Heroes.Length, Heroes.Select(h => CityTexts.ResidentLine(null, resident, h)).Distinct().Count(), $"{id} repeats a line");
            }

            foreach (string key in keys)
            {
                Assert.IsTrue(manager.HasKey(key), $"missing key {key}");
                Assert.IsTrue(byKey.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"no pt-BR for {key}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"no English for {key}");
                if (key.Contains(".line."))
                {
                    Assert.AreNotEqual(entry.En, entry.PtBR, $"{key} is not translated");
                }
            }

            foreach (string id in Cast)
            {
                foreach (CharacterId hero in Heroes)
                {
                    string line = CityTexts.ResidentLine(null, Resident(id), hero);
                    string name = hero == CharacterId.Icaro ? "Ícaro" : hero.ToString();
                    StringAssert.Contains(name, line, $"{id} → {hero}");
                }
            }

            string all = string.Join(" ", Cast.SelectMany(id => Heroes.Select(h => CityTexts.ResidentLine(null, Resident(id), h))));
            StringAssert.Contains("Jorge", all);
            StringAssert.Contains("Edna", all);
            StringAssert.DoesNotContain("Saci", all);
            StringAssert.DoesNotContain("Curupira", all);
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByTaaPwanisResidents_WithTheRootBootsAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.TaaPwani).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "liyongo", "zawadi" }, quests.Select(q => q.GiverResidentId).ToArray());
            foreach (CityQuestSpec spec in quests)
            {
                foreach (RegionObjectiveSpec objective in spec.Quest.Objectives)
                {
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{objective.TargetId}");
                    Assert.IsNotNull(enemy, objective.TargetId);
                    Assert.IsFalse(enemy.IsBoss, objective.TargetId);
                }

                QuestDefinition quest = CityQuestCatalog.GetQuest(spec.QuestId);
                Assert.IsNotNull(quest);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), $"{spec.QuestId}: rewards resolve");
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.RootBoots), "Zawadi pays with his root boots (map 30)");
        }

        [Test]
        public void CityQuests_LiyongoOffersFirst_ZawadiOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("liyongo", isActive, isDone, out CityQuestSpec liyongo));
            Assert.AreEqual(CityQuestCatalog.TaaPwaniGulls, liyongo.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("zawadi", isActive, isDone, out _), "the chain starts with the captain");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("nuru", isActive, isDone, out _), "Nuru gives no quest");

            done.Add(CityQuestCatalog.TaaPwaniGulls);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("zawadi", isActive, isDone, out CityQuestSpec zawadi));
            Assert.AreEqual(CityQuestCatalog.TaaPwaniClaws, zawadi.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec gulls = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.TaaPwaniGulls);
            Assert.AreEqual(gulls.OfferKey, CityQuestCatalog.Talk(quests, "liyongo"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.TaaPwaniGulls));
            Assert.AreEqual(gulls.ActiveKey, CityQuestCatalog.Talk(quests, "liyongo"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "zawadi"), "his quest waits for the captain's");
            quests.Shutdown();
        }
    }
}
