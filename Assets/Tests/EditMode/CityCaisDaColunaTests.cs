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
    /// The Cais da Coluna, the quay of the Reinos do Céu, as a walkable capital (Docs/Cities.md, Docs/Mapas100.md map 91): its <see cref="CitySpec"/> among the
    /// late-region cities, the two people of the ficha (Ammo and Adda, in recipes of their own), the SkyQuay style (pavilions of glass and gold, the five fountains, the foot of the Coluna, the two
    /// Barcas alongside their piers, the silent Remadores and the 27 cities far below), the whole town inside a circle of radius ~22.5 m at y = 0 with three roads out (W, E, S),
    /// A Última Caravana do Ammo (never the life or chaos stones) and the two city quests; maps 91–100 respawn on its plaza and their gates are on Ammo's counter (MapRequirementTests).
    /// </summary>
    public class CityCaisDaColunaTests
    {
        private static readonly string[] Cast = { "ammo_caravaneiro", "adda_roda" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        [Test]
        public void Cais_HasACitySpec_AsALateRegionCity_SameIdAsItsRecipe()
        {
            CitySpec city = CityCatalog.Get(CityCatalog.CaisDaColuna);
            Assert.IsNotNull(city, "the Cais has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, city.Region, "the Reinos do Céu borrow the Forest (always unlocked)");
            Assert.AreEqual("cais_da_coluna", city.CityId, "the return city id of maps 91–100");
            Assert.AreEqual(CityTownRecipes.CaisDaColunaCityId, city.CityId);

            Assert.IsTrue(CityCatalog.IsLateRegionCity(city));
            Assert.IsFalse(CityCatalog.IsRaceCity(city));
            Assert.IsFalse(CityCatalog.IsHubCity(city));
            Assert.IsFalse(city.HasGrandmaServices);
            Assert.IsFalse(city.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(city.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(city.CityId));

            var cities = new CityProgress();
            Assert.AreSame(city, cities.ApplyDefeatIn(city), "a defeat in the sky wakes the cousins at the five fountains");
            Assert.AreSame(city, cities.ConsumeRespawnNotice());
        }

        [Test]
        public void MapsOfTheSkyRealms_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.CaisDaColuna).ToList();
            CollectionAssert.AreEquivalent(new[] { 91, 92, 93, 94, 95, 96, 97, 98, 99, 100 }, maps.Select(m => m.MapId).ToArray(), "the Cais is the return city of 91–100 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.CaisDaColuna, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get("map_091_cais_da_coluna").Settlement);
            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_091_cais_da_coluna", _ => CityCatalog.CaisDaColuna, CityCatalog.VilaZefa)));
        }

        [Test]
        public void Cais_IsASkyQuay_WithFountainsColumnBargesRowersAndTheCitiesBelow()
        {
            CityTownRecipe recipe = CityTownRecipes.CaisDaColuna();
            Assert.AreSame(TownStyles.SkyQuay, recipe.Style);
            Assert.AreEqual(TownArchitecture.SkyQuay, TownStyles.SkyQuay.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.CaisDaColuna);
            Assert.AreSame(TownStyles.SkyQuay, layout.Style);

            // The five fountains, one for each light element, each with its own colour, on the plaza and off the roads and the respawn.
            List<CityProp> fountains = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.ElementFountain).ToList();
            Assert.AreEqual(5, fountains.Count, "one fountain for each element");
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, fountains.Select(f => f.Variant).ToArray());
            Assert.AreEqual(5, Enumerable.Range(0, 5).Select(TownKit.LightElementSwatch).Distinct().Count(), "each element has its own colour");
            foreach (CityProp fountain in fountains)
            {
                Assert.Less(fountain.Position.magnitude, layout.PlazaRadius, "on the plaza");
                Assert.Greater(fountain.Position.magnitude, TownKit.ElementFountainRadius + CityLayoutRules.WalkerRadius, "...but not on the respawn");
                Assert.IsTrue(CitySolids.PropSolid(fountain, layout.WalkBounds).HasValue, "a fountain stops the walker");
            }

            CityProp column = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.ColumnOfGlory);
            Assert.Greater(column.Position.y, layout.PlazaRadius, "the Coluna stands north of the plaza");
            Assert.IsTrue(CitySolids.PropSolid(column, layout.WalkBounds).HasValue);

            // The two barques are a chain of colliders with no gaps, south of the piers, off the roads.
            List<CityProp> barges = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.SkyBarge).ToList();
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, barges.Select(p => p.Variant).ToArray(), "the Barca do Sol and the Barca da Lua");
            Assert.Greater(barges.Single(p => p.Variant == 0).Position.x, 0f, "the Sun's pier is to the east");
            Assert.Less(barges.Single(p => p.Variant == 1).Position.x, 0f, "the Moon's pier is to the west");
            List<CitySolid> hull = layout.Solids.Where(s => s.Owner == nameof(CityPropKind.SkyBarge)).ToList();
            Assert.Greater(hull.Count, 4, "each barge is a chain of colliders");
            foreach (CityProp barge in barges)
            {
                List<Vector2> chain = TownKit.SkyBargeColliders(barge.Position, 1f).ToList();
                for (int i = 1; i < chain.Count; i++)
                {
                    Assert.Less(Vector2.Distance(chain[i - 1], chain[i]), TownKit.SkyBargeHalfBeam * 2f, "the hull has no holes");
                }
            }

            foreach (CitySolid solid in hull)
            {
                foreach (CityStreet street in layout.Streets)
                {
                    Assert.IsFalse(CityGeometry.Overlaps(solid.Footprint, street.Bounds), $"a barge sits on the {street.Arm} street");
                }
            }

            // The Remadores are silhouettes of light: props, no colliders, no residents.
            List<CityProp> rowers = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.LightRower).ToList();
            Assert.GreaterOrEqual(rowers.Count, 4);
            Assert.IsTrue(rowers.All(p => !CitySolids.PropSolid(p, layout.WalkBounds).HasValue), "the Remadores do not stop anyone");

            // The 27 cities of the island, shared between the two gaps in the cloud.
            List<CityProp> gaps = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.IslandLightsGap).ToList();
            Assert.AreEqual(2, gaps.Count);
            Assert.AreEqual(TownKit.IslandCities, gaps.Sum(g => TownKit.IslandLightsIn(g.Variant)), "27 lights");
            Assert.AreEqual(27, TownKit.IslandCities);
            Assert.IsTrue(gaps.All(p => !CitySolids.PropSolid(p, layout.WalkBounds).HasValue), "only light down there");

            List<CityLot> houses = layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).ToList();
            Assert.IsTrue(houses.All(l => l.Building.SkyPavilion && !l.Building.BasaltKeep && !l.Building.ZigguratHall), "every house is a pavilion");
            Assert.AreEqual(TownShopDressing.LastCaravan, recipe.Lots.Single(l => l.ResidentId == "ammo_caravaneiro").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the Coluna stands there");
            Assert.AreEqual(TownSwatch.QuayCloud, TownStyles.SkyQuay.Ground);
            Assert.Greater(TownPalette.GlowOf(TownSwatch.ColumnLight), 0.5f, "the Coluna glows");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.RowerLight), 0.5f, "the Remadores glow");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.QuayCloud), "cloud does not glow");
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);

            foreach (CityLot lot in houses)
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.ElementFountain && p.Kind <= CityPropKind.LightRower))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.SkyQuay);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gateMesh = new TownMeshBuilder();
            TownKit.Gate(gateMesh, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.SkyQuay);
            Assert.Greater(gateMesh.TriangleCount, 40, "the gate of two gold pylons");
        }

        [Test]
        public void Cais_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.CaisDaColuna);
            float radius = CityTownRecipes.CaisDaColunaSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld will keep a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.East, CityArm.South }, layout.Streets.Select(s => s.Arm).ToArray());
            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.CaisDaColunaHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.CaisDaColuna().EntryArm, "the road the last caravan comes in by");
        }

        [Test]
        public void Cais_Residents_AreTheFichas_InRecipesOfTheirOwn()
        {
            CitySpec city = CityCatalog.Get(CityCatalog.CaisDaColuna);
            CollectionAssert.AreEquivalent(Cast, city.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.QuayWhite, NpcSwatch.QuayGold, NpcSwatch.QuayLilac, NpcSwatch.QuaySky, NpcSwatch.QuayGlow, NpcSwatch.QuayAqua, NpcSwatch.QuayOrange, NpcSwatch.QuayRoadDust };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Satyr || look.Snout, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the quay");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsFalse(look.Beard == NpcBeard.LongBraided && (look.Layers & NpcLayer.Cape) != 0, $"{id}: a long braided beard and a cape blow the budget");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.UltimaCaravanaAmmo, city.Residents.Single(r => r.ResidentId == "ammo_caravaneiro").ShopId, "Ammo keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.UltimaCaravanaAmmo }, city.ShopIds.ToArray());
        }

        [Test]
        public void Cais_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
            }

            Assert.AreEqual("Ammo, o Caravaneiro", CityTexts.ResidentName(null, Resident("ammo_caravaneiro")));
            Assert.AreEqual("Adda, o Guardião da Roda", CityTexts.ResidentName(null, Resident("adda_roda")));
        }

        [Test]
        public void Cais_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.CaisDaColuna, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            // Ammo's stall (PlazaRadius · (0.575, −0.675)): nothing solid in front of his counter.
            CityTownRecipe recipe = CityTownRecipes.CaisDaColuna();
            var stall = new Vector2(recipe.PlazaRadius * 0.575f, -recipe.PlazaRadius * 0.675f);
            CityLayout withAmmo = CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.CaisDaColuna, true));
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(withAmmo, stall + Vector2.up * 1.4f), CityLayoutRules.WalkerRadius, "the cousins can step up to the peddler's counter");

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.CaisDaColuna);
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

            Assert.AreEqual("ultima_caravana", layout.LotOfShop(CityShopCatalog.UltimaCaravanaAmmo).Id);
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        [Test]
        public void UltimaCaravana_SellsTheHighRoadGear_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.UltimaCaravanaAmmo);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            var gear = new[] { "item_health_potion", "item_mana_potion", MapRequirementItems.RunicLamp, MapRequirementItems.EternalFireOil, MapRequirementItems.StormCloak, MapRequirementItems.CanvasWings, MapRequirementItems.ClimbingRope, MapRequirementItems.Theriac };
            foreach (string fixedItem in gear)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Ammo sells {fixedItem}");
                Assert.IsFalse(entry.Rotating, $"{fixedItem} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, $"{fixedItem} from the first day");
            }

            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, "item_pedra_vida"), "the life stone is never sold");
            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, "item_pedra_caos"), "the chaos stone is never sold");

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            Assert.Greater(shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main), shop.RotatingSlots, "more lines than slots: the counter turns");
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 9));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                Assert.Contains(MapRequirementItems.RunicLamp, today);
                Assert.Contains(MapRequirementItems.StormCloak, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec city = CityCatalog.Get(CityCatalog.CaisDaColuna);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(city, new DateTime(2026, 11, 9).AddDays(d), null);
                Assert.Contains(CityShopCatalog.UltimaCaravanaAmmo, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "the mule stops at the region's hub only");
            }
        }

        [Test]
        public void Cais_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec city = CityCatalog.Get(CityCatalog.CaisDaColuna);
            var keys = new List<string> { city.NameKey, city.ButtonKey, city.FlavourKey, CityShopCatalog.Get(CityShopCatalog.UltimaCaravanaAmmo).NameKey };
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
            StringAssert.Contains("estrada", all, "the road of Ammo");
            StringAssert.Contains("Roda", all, "the Wheel of Adda");
            StringAssert.Contains("Remadores", all, "the silent rowers are mentioned, not heard");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        [Test]
        public void CityQuests_AreGivenByAmmoAndAdda_OnExistingEnemies_WithAStormCloakAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.HomeQuestsIn(CityCatalog.CaisDaColuna).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "ammo_caravaneiro", "adda_roda" }, quests.Select(q => q.GiverResidentId).ToArray());
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
                Assert.IsFalse(spec.Quest.Rewards.Any(r => r.ItemId == "item_pedra_vida" || r.ItemId == "item_pedra_caos"), spec.QuestId);
                foreach (string suffix in new[] { ".title", ".description", ".journal", ".offer", ".active" })
                {
                    Assert.IsNotNull(CityQuestTexts.GetFallback(spec.KeyBase + suffix), spec.KeyBase + suffix);
                }
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.StormCloak), "Adda pays with the cloak he wore on the Tower");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("ammo_caravaneiro", active.Contains, done.Contains, out CityQuestSpec stowaways));
            Assert.AreEqual(CityQuestCatalog.CaisColunaStowaways, stowaways.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("adda_roda", active.Contains, done.Contains, out _), "the chain starts with Ammo");
            done.Add(CityQuestCatalog.CaisColunaStowaways);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("adda_roda", active.Contains, done.Contains, out CityQuestSpec birds));
            Assert.AreEqual(CityQuestCatalog.CaisColunaBirds, birds.QuestId);
        }
    }
}
