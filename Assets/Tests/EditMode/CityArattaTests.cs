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
    /// Aratta, the Cidade dos Chifres, as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 26): its
    /// <see cref="CitySpec"/> among the race cities, the four bull folk of the ficha at their doors with their own look (big,
    /// horns, a broad muzzle), the ReedHorn style (reed halls, horn posts, stone pens, the bull-headed lyre, acacias), the
    /// whole town inside a circle of radius ~22 m at y = 0 with its three roads out (W, E, S), Lugalbanda's Alforje with the
    /// Artífices' relic of map 27, the two city quests and the respawn of 26–27.
    /// </summary>
    public class CityArattaTests
    {
        private static readonly string[] Cast = { "enmerkar", "lugalbanda", "shulgi", "ensuhkeshdanna" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        /// <summary>Distance from the plaza centre to the farthest point of <paramref name="solid"/>.</summary>
        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void Aratta_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec aratta = CityCatalog.Get(CityCatalog.Aratta);
            Assert.IsNotNull(aratta, "Aratta has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, aratta.Region, "the Costa das Monções borrows the Forest (always unlocked), as Taa Pwani");
            Assert.AreEqual(MapCatalog.Aratta, aratta.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.ArattaCityId, aratta.CityId, "the recipe's city id");
            Assert.IsTrue(CityCatalog.IsRaceCity(aratta));
            Assert.IsFalse(CityCatalog.IsHubCity(aratta));
            Assert.IsFalse(CityCatalog.Cities.Contains(aratta), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.VilaZefa, CityCatalog.ForRegion(BiomeType.Forest).CityId, "the Forest hub is still the Vila");
            Assert.IsFalse(aratta.HasGrandmaServices);
            Assert.IsFalse(aratta.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(aratta.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(aratta.CityId));
        }

        [Test]
        public void MapsOfAratta_RespawnInItsPlaza()
        {
            Assert.AreEqual(CityCatalog.Aratta, MapCatalog.RespawnCityFor("map_026_auroques").CityId);
            Assert.AreEqual(CityCatalog.Aratta, MapCatalog.RespawnCityFor("map_027_meroe").CityId);
            Assert.AreEqual(CityCatalog.TaaPwani, MapCatalog.RespawnCityFor("map_025_taa_pwani").CityId, "25 still wakes in Taa Pwani");

            var cities = new CityProgress();
            CitySpec aratta = CityCatalog.Get(CityCatalog.Aratta);
            Assert.AreSame(aratta, cities.ApplyDefeatIn(aratta), "a defeat on 26/27 wakes the cousins by the lyre");
            Assert.AreSame(aratta, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Aratta_IsAReedHornTown_WithReedHallsHornPostsPensAndTheLyre()
        {
            CityTownRecipe recipe = CityTownRecipes.Aratta();
            Assert.AreSame(TownStyles.ReedHorn, recipe.Style);
            Assert.AreEqual(TownArchitecture.ReedHorn, TownStyles.ReedHorn.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.ReedHorn.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Aratta);
            Assert.AreSame(TownStyles.ReedHorn, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.BullLyre), "the Lira do Touro on the plaza");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.HornPost), 4, "horn posts mark the ways");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StoneKraal), 2, "round stone pens");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Acacia), 2);
            Assert.IsTrue(recipe.PlazaProps.Any(p => p.Kind == CityPropKind.Bonfire), "the low fire of the Fogo Vivo");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.ReedHall && !l.Building.ReefTower),
                "every house is a reed hall");
            Assert.AreEqual(TownShopDressing.Wanderer, recipe.Lots.Single(l => l.ResidentId == "lugalbanda").Spec.Dressing);
            foreach (CityPropKind kind in new[] { CityPropKind.HornPost, CityPropKind.StoneKraal, CityPropKind.BullLyre, CityPropKind.Acacia })
            {
                Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == kind), layout.WalkBounds).HasValue, $"{kind} is solid");
            }

            // Taa Pwani (the region's other city) keeps its coral stone.
            Assert.IsFalse(CityTownRecipes.LayoutFor(CityCatalog.TaaPwani).Lots.Any(l => l.Building != null && l.Building.ReedHall));
        }

        [Test]
        public void Aratta_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Aratta);
            float radius = CityTownRecipes.ArattaSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld keeps a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.East, CityArm.South }, layout.Streets.Select(s => s.Arm).ToArray());
            foreach (CityStreet street in layout.Streets)
            {
                Assert.AreEqual(CityTownRecipes.ArattaArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.ArattaHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.Aratta().EntryArm, "the road in from the savanna");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Aratta_Residents_AreTheFichasBullFolk_WithALookOfTheirOwn()
        {
            CitySpec aratta = CityCatalog.Get(CityCatalog.Aratta);
            CollectionAssert.AreEquivalent(Cast, aratta.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.Horns, $"{id}: aurochs horns and a muzzle");
                Assert.IsFalse(look.Tusks || look.ElfEars || look.Snout || look.Gills);
                Assert.IsTrue(look.Skin == NpcSwatch.SkinAurochs || look.Skin == NpcSwatch.SkinAurochsTawny, $"{id}: a dark or tawny coat");
                Assert.Greater(look.Height, 1.1f, $"{id}: the bull folk are big");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                StringAssert.EndsWith("/bull", look.SilhouetteKey);
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsNotNull(NpcModelBuilder.Build(look), $"{id} builds a model");
            }

            Assert.AreEqual(CityShopCatalog.AlforjeLugalbanda, aratta.Residents.Single(r => r.ResidentId == "lugalbanda").ShopId, "Lugalbanda keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.AlforjeLugalbanda }, aratta.ShopIds.ToArray());
        }

        [Test]
        public void Aratta_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Enmerkar, o Chefe Construtor", CityTexts.ResidentName(null, Resident("enmerkar")));
            Assert.AreEqual("Shulgi, o Corredor", CityTexts.ResidentName(null, Resident("shulgi")));
        }

        [Test]
        public void Aratta_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Aratta, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Aratta);
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

            Assert.AreEqual("alforje_lugalbanda", layout.LotOfShop(CityShopCatalog.AlforjeLugalbanda).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Lugalbanda's saddlebag
        // ------------------------------------------------------------------

        [Test]
        public void Alforje_SellsTheRelicOf27_AtBalancedPrices_AndItsCounterTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.AlforjeLugalbanda);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            // Map 26 has no gate; map 27 (the Pirâmides de Meroé, return city Aratta) asks for the Artífices' relic.
            Assert.IsTrue(string.IsNullOrEmpty(MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 26).SpecialRequirementTag)));
            string relic = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 27).SpecialRequirementTag);
            Assert.AreEqual(MapRequirementItems.ArtificerRelic, relic);
            CityShopEntry gate = CityShopCatalog.EntryOf(shop.ShopId, relic);
            Assert.IsNotNull(gate, "Lugalbanda sells the relic");
            Assert.IsFalse(gate.Rotating);
            Assert.IsFalse(gate.Unlock.IsGated);

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            Assert.Greater(shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main), shop.RotatingSlots, "more lines than slots: the counter turns");
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 3));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                CollectionAssert.AreEqual(today, ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList(), "same day, same counter");
                Assert.Contains(relic, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void Aratta_OpensTheAlforje_AndNeverTheCaravaneer()
        {
            CitySpec aratta = CityCatalog.Get(CityCatalog.Aratta);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(aratta, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.AlforjeLugalbanda, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Aratta_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec aratta = CityCatalog.Get(CityCatalog.Aratta);
            var keys = new List<string> { aratta.NameKey, aratta.ButtonKey, aratta.FlavourKey, CityShopCatalog.Get(CityShopCatalog.AlforjeLugalbanda).NameKey };
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
            StringAssert.Contains("Fogo Vivo", all, "the low fire the bull folk keep");
            foreach (string folk in new[] { "Saci", "Curupira", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByArattasResidents_OnExistingBeasts()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Aratta).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "enmerkar", "shulgi" }, quests.Select(q => q.GiverResidentId).ToArray());
            foreach (CityQuestSpec spec in quests)
            {
                foreach (RegionObjectiveSpec objective in spec.Quest.Objectives)
                {
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{objective.TargetId}");
                    Assert.IsNotNull(enemy, objective.TargetId);
                    Assert.IsFalse(enemy.IsBoss, objective.TargetId);
                    StringAssert.Contains(objective.ObjectiveId, string.Join(" ", CityQuestTexts.All.Select(e => e.Key)), "objective text");
                }

                QuestDefinition quest = CityQuestCatalog.GetQuest(spec.QuestId);
                Assert.IsNotNull(quest);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), $"{spec.QuestId}: rewards resolve");
                foreach (string suffix in new[] { ".title", ".description", ".journal", ".offer", ".active" })
                {
                    Assert.IsNotNull(CityQuestTexts.GetFallback(spec.KeyBase + suffix), spec.KeyBase + suffix);
                }
            }

            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_EnmerkarOffersFirst_ShulgiOnlyAfterHisQuest()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("enmerkar", isActive, isDone, out CityQuestSpec builder));
            Assert.AreEqual(CityQuestCatalog.ArattaJackals, builder.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("shulgi", isActive, isDone, out _), "the chain starts with the builder");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("lugalbanda", isActive, isDone, out _), "Lugalbanda gives no quest");

            done.Add(CityQuestCatalog.ArattaJackals);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("shulgi", isActive, isDone, out CityQuestSpec runner));
            Assert.AreEqual(CityQuestCatalog.ArattaRace, runner.QuestId);
        }

        [Test]
        public void TownPalette_AndNpcPalette_CoverEverySwatch()
        {
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, Enum.GetValues(typeof(TownSwatch)).Length);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);
            Assert.AreEqual(NpcPalette.Count, Enum.GetValues(typeof(NpcSwatch)).Length);
        }
    }
}
