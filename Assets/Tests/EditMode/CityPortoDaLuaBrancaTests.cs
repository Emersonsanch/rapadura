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
    /// Porto da Lua Branca as a walkable capital (Docs/Cities.md, Docs/Mapas100.md map 59): its <see cref="CitySpec"/> among the
    /// late-region cities, the four people of the ice harbour of the ficha and of the Kellis letters (Yirkap, Pamour, Pera,
    /// Makarios) at their doors, the IceCanal style (long plank houses with a moon on the gable, snow domes, the Cais da Lua,
    /// canals of black water with whale-rib bridges, crescent boats, fish racks, ridges of sea ice, the gate of whale ribs), the
    /// whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), the Peles e Barcos do Pamour
    /// with the gear of every gate of the Mar Gelado that returns here (never the life or chaos stones), the two city quests and
    /// the respawn of maps 59–64.
    /// </summary>
    public class CityPortoDaLuaBrancaTests
    {
        private static readonly string[] Cast = { "yirkap", "pamour", "pera", "makarios" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "yirkap", "Yirkap" }, { "pamour", "Pamour" }, { "pera", "Pera" }, { "makarios", "Makarios" },
        };

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void Porto_HasACitySpec_AsALateRegionCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec porto = CityCatalog.Get(CityCatalog.PortoDaLuaBranca);
            Assert.IsNotNull(porto, "the Porto has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, porto.Region, "the Mar Gelado borrows the Forest (always unlocked)");
            Assert.AreEqual("porto_da_lua_branca", porto.CityId, "the maps' return city id");
            Assert.AreEqual(CityTownRecipes.PortoDaLuaBrancaCityId, porto.CityId, "the recipe's city id");
            Assert.AreEqual(MapCatalog.PortoDaLuaBranca, porto.CityId, "MapCatalog's id");
            CollectionAssert.Contains(MapCatalog.ReturnCityIds.ToArray(), porto.CityId);

            Assert.IsTrue(CityCatalog.IsLateRegionCity(porto), "a capital of the late regions, as Farnkesh");
            Assert.IsFalse(CityCatalog.IsRaceCity(porto));
            Assert.IsFalse(CityCatalog.IsHubCity(porto));
            Assert.IsFalse(porto.HasGrandmaServices);
            Assert.IsFalse(porto.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(porto.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(porto.CityId));
        }

        [Test]
        public void MapsOfTheFrozenSea_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.PortoDaLuaBranca).ToList();
            CollectionAssert.IsSubsetOf(maps.Select(m => m.MapId).ToArray(), new[] { 59, 60, 61, 62, 63, 64 }, "the Porto is the return city of 59–64 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.PortoDaLuaBranca, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_059_porto_lua_branca", _ => CityCatalog.PortoDaLuaBranca, CityCatalog.VilaZefa)));

            var cities = new CityProgress();
            CitySpec porto = CityCatalog.Get(CityCatalog.PortoDaLuaBranca);
            Assert.AreSame(porto, cities.ApplyDefeatIn(porto), "a defeat on the frozen sea wakes the cousins by the Cais da Lua");
            Assert.AreSame(porto, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Porto_IsAnIceCanal_WithTheMoonQuayCanalsBoatsDomesAndIce()
        {
            CityTownRecipe recipe = CityTownRecipes.PortoDaLuaBranca();
            Assert.AreSame(TownStyles.IceCanal, recipe.Style);
            Assert.AreEqual(TownArchitecture.IceCanal, TownStyles.IceCanal.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.PortoDaLuaBranca);
            Assert.AreSame(TownStyles.IceCanal, layout.Style);
            CityProp quay = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.MoonQuay);
            Assert.Less(quay.Position.magnitude, layout.PlazaRadius, "the Cais da Lua stands on the plaza");
            Assert.Greater(quay.Position.magnitude, TownKit.MoonQuayRadius + CityLayoutRules.WalkerRadius, "...but not on the respawn");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CrescentBoat), 3, "crescent boats drawn up on the ice");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CrescentBoat && p.Variant == 1), 1, "a boat with a moon lantern");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.SnowDomeHut), 2, "small snow domes");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.FishRack), 2, "the fishing quarter's racks");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.IceBlock), 6, "ridges of sea ice and the frozen bay");
            List<CityProp> canals = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.IceCanal).ToList();
            Assert.GreaterOrEqual(canals.Count, 2, "canals of black water with whale-rib bridges");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.IceCanal));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.MoonQuay));
            Assert.IsNull(CitySolids.PropSolid(canals[0], layout.WalkBounds), "the whale-rib bridges are walkable");
            foreach (CityProp canal in canals)
            {
                Assert.IsTrue(layout.Streets.Any(s => s.Road.Contains((canal.Position + canal.End) * 0.5f)), $"the bridge at {canal.Position} carries a street");
            }

            Assert.IsTrue(CitySolids.PropSolid(quay, layout.WalkBounds).HasValue, "the quay's platform stops the walker");
            List<CityLot> houses = layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).ToList();
            Assert.IsTrue(houses.All(l => l.Building.MoonLonghouse != l.Building.SnowDome && !l.Building.PinnacleNest && !l.Building.GourdHut),
                "every house is a long plank house or a snow dome");
            Assert.GreaterOrEqual(houses.Count(l => l.Building.SnowDome), 2, "snow domes among the houses");
            Assert.IsTrue(houses.Where(l => l.ResidentId != null).All(l => l.Building.MoonLonghouse), "the cast lives in the long houses with a moon on the gable");
            Assert.AreEqual(TownShopDressing.FurAndBoat, recipe.Lots.Single(l => l.ResidentId == "pamour").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the frozen bay");
            Assert.AreEqual(TownSwatch.Snowfield, TownStyles.IceCanal.Ground, "the islands are snow on the ice");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.MoonGlow), 0.5f, "the moon lanterns glow");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.MoonPlank), "the planks do not glow");
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);

            foreach (CityLot lot in houses)
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.MoonQuay && p.Kind <= CityPropKind.IceBlock))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.IceCanal);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.IceCanal);
            Assert.Greater(gate.TriangleCount, 40, "the gate of whale ribs");
        }

        [Test]
        public void Porto_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.PortoDaLuaBranca);
            float radius = CityTownRecipes.PortoDaLuaBrancaSolidRadius;
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
            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.PortoDaLuaBrancaHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.PortoDaLuaBranca().EntryArm, "the road in from the shore");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Porto_Residents_AreTheFichas_PeopleOfTheIceWithALookOfTheirOwn()
        {
            CitySpec porto = CityCatalog.Get(CityCatalog.PortoDaLuaBranca);
            CollectionAssert.AreEquivalent(Cast, porto.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.ReindeerHide, NpcSwatch.SealGrey, NpcSwatch.PolarNight, NpcSwatch.MoonLilac, NpcSwatch.PolarSnow };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Satyr || look.Snout, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the ice harbour");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsFalse(look.Beard == NpcBeard.LongBraided && (look.Layers & NpcLayer.Cape) != 0, $"{id}: a long braided beard and a cape blow the budget");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.Greater(NpcRecipes.Find("pera").Height, NpcRecipes.Named.Where(r => Cast.Contains(r.Id) && r.Id != "pera").Max(r => r.Height), "Pera is the giant of the harbour");
            Assert.AreEqual(CityShopCatalog.PelesPamour, porto.Residents.Single(r => r.ResidentId == "pamour").ShopId, "Pamour keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.PelesPamour }, porto.ShopIds.ToArray());
        }

        [Test]
        public void Porto_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Yirkap, o Caçador de Esqui", CityTexts.ResidentName(null, Resident("yirkap")));
            Assert.AreEqual("Pera, o Gigante Bom", CityTexts.ResidentName(null, Resident("pera")));
        }

        [Test]
        public void Porto_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.PortoDaLuaBranca, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            // Ammo's stall (PlazaRadius · (0.575, −0.675)): nothing solid in front of his counter.
            CityTownRecipe recipe = CityTownRecipes.PortoDaLuaBranca();
            var stall = new Vector2(recipe.PlazaRadius * 0.575f, -recipe.PlazaRadius * 0.675f);
            CityLayout withAmmo = CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.PortoDaLuaBranca, true));
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(withAmmo, stall + Vector2.up * 1.4f), CityLayoutRules.WalkerRadius, "the cousins can step up to Ammo's counter");

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.PortoDaLuaBranca);
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

            Assert.AreEqual("peles_do_pamour", layout.LotOfShop(CityShopCatalog.PelesPamour).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // The Peles e Barcos do Pamour
        // ------------------------------------------------------------------

        [Test]
        public void PelesDoPamour_SellTheGearOfEveryGateOfTheFrozenSea_AtBalancedPrices_TurnDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.PelesPamour);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            // The gates of the maps that return to the Porto (63 ginger tea, 64 thermal clothes today): fixed from the first day.
            List<MapDefinition> gated = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.PortoDaLuaBranca && m.HasSpecialRequirement).ToList();
            foreach (MapDefinition map in gated)
            {
                foreach (string itemId in MapRequirementRules.KitOf(map.SpecialRequirementTag))
                {
                    CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, itemId);
                    Assert.IsNotNull(entry, $"{map}: Pamour sells {itemId}");
                    Assert.IsFalse(entry.Rotating, $"{itemId} is always on the counter");
                    Assert.IsFalse(entry.Unlock.IsGated, $"{itemId} from the first day");
                }
            }

            var gear = new[]
            {
                MapRequirementItems.ThermalClothes, MapRequirementItems.GingerTea, MapRequirementItems.WoolCloak, MapRequirementItems.DhowPassage,
                MapRequirementItems.ReedBoat, MapRequirementItems.OilLamp, MapRequirementItems.RunicLamp, MapRequirementItems.ClimbingCrampons,
                MapRequirementItems.StormCloak,
            };
            foreach (string fixedItem in gear)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Pamour sells {fixedItem}");
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
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 6));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                Assert.Contains(MapRequirementItems.ThermalClothes, today);
                Assert.Contains(MapRequirementItems.GingerTea, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec porto = CityCatalog.Get(CityCatalog.PortoDaLuaBranca);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(porto, new DateTime(2026, 11, 6).AddDays(d), null);
                Assert.Contains(CityShopCatalog.PelesPamour, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Porto_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec porto = CityCatalog.Get(CityCatalog.PortoDaLuaBranca);
            var keys = new List<string> { porto.NameKey, porto.ButtonKey, porto.FlavourKey, CityShopCatalog.Get(CityShopCatalog.PelesPamour).NameKey };
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
            StringAssert.Contains("gelo", all, "the ice");
            StringAssert.Contains("Barca da Lua", all, "the Moon Ship that moors here");
            StringAssert.Contains("Ar Sereno", CityTexts.ResidentLine(null, Resident("makarios"), CharacterId.Lavine), "Lavine's element");
            StringAssert.Contains("Barqueira da Lua", CityTexts.ResidentLine(null, Resident("makarios"), CharacterId.Lavine), "Lavine's G4 is here");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByYirkapAndPera_OnExistingEnemies_WithTheThermalClothesAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.HomeQuestsIn(CityCatalog.PortoDaLuaBranca).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "yirkap", "pera" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.ThermalClothes), "Pera pays with the thermal clothes (map 64)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("yirkap", active.Contains, done.Contains, out CityQuestSpec gulls));
            Assert.AreEqual(CityQuestCatalog.PortoGulls, gulls.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("pera", active.Contains, done.Contains, out _), "the chain starts with the ski hunter");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("pamour", active.Contains, done.Contains, out _), "Pamour gives no quest");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("makarios", active.Contains, done.Contains, out _), "Makarios gives no quest");
            done.Add(CityQuestCatalog.PortoGulls);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("pera", active.Contains, done.Contains, out CityQuestSpec bears));
            Assert.AreEqual(CityQuestCatalog.PortoBears, bears.QuestId);
        }
    }
}
