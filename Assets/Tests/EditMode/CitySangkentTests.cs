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
    /// Sangkent, a Cidade-Escadaria, as a walkable late-region city (Docs/Cities.md, Docs/Mapas100.md §2.1 and map 36): its
    /// <see cref="CitySpec"/> among the late-region cities, the four residents of the ficha (Abā, Sisinnios, Kustai and
    /// Baraies) at their doors with their own look, the StoneTerrace style (two-storey stone houses, the Marco das Caravanas,
    /// the terraces in the north with the fort, the stone chutes and their carts, the irrigation channels, the apricot
    /// trees), the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), Sisinnios's
    /// counter with the gear of every gate of the Teto do Mundo an item answers (36, 38, 40), the two city quests and the
    /// respawn of the maps whose return city it is (36–40).
    /// </summary>
    public class CitySangkentTests
    {
        private static readonly string[] Cast = { "aba", "sisinnios", "kustai", "baraies" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "aba", "Abā" }, { "sisinnios", "Sisinnios" }, { "kustai", "Kustai" }, { "baraies", "Baraies" },
        };

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
        public void Sangkent_HasACitySpec_AsALateRegionCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec sangkent = CityCatalog.Get(CityCatalog.Sangkent);
            Assert.IsNotNull(sangkent, "Sangkent has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, sangkent.Region, "the Teto do Mundo borrows the Forest (always unlocked), as Jhilpur");
            Assert.AreEqual(MapCatalog.Sangkent, sangkent.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.SangkentCityId, sangkent.CityId, "the recipe's city id");
            foreach (int map in new[] { 36, 37, 38, 39, 40 })
            {
                Assert.AreEqual(CityCatalog.Sangkent, MapCatalog.Get(map).ReturnCityId, $"the return city of map {map}");
            }

            Assert.IsTrue(CityCatalog.IsLateRegionCity(sangkent));
            Assert.IsFalse(CityCatalog.IsRaceCity(sangkent));
            Assert.IsFalse(CityCatalog.IsHubCity(sangkent));
            Assert.IsFalse(CityCatalog.Cities.Contains(sangkent), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.VilaZefa, CityCatalog.ForRegion(BiomeType.Forest).CityId, "the Forest hub is still the Vila");
            Assert.IsFalse(sangkent.HasGrandmaServices);
            Assert.IsFalse(sangkent.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(sangkent.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(sangkent.CityId));
        }

        [Test]
        public void MapsOfTheRoofOfTheWorld_RespawnInSangkentsPlaza()
        {
            foreach (string mapId in new[] { "map_036_sangkent", "map_037_calhas", "map_038_geleira", "map_039_forte_ventos", "map_040_wakhan" })
            {
                Assert.AreEqual(CityCatalog.Sangkent, MapCatalog.RespawnCityFor(mapId).CityId, mapId);
            }

            Assert.AreEqual(CityCatalog.Jhilpur, MapCatalog.RespawnCityFor("map_031_jhilpur").CityId, "the delta still wakes in Jhilpur");

            var cities = new CityProgress();
            CitySpec sangkent = CityCatalog.Get(CityCatalog.Sangkent);
            Assert.AreSame(sangkent, cities.ApplyDefeatIn(sangkent), "a defeat on 36–40 wakes the cousins by the Marco das Caravanas");
            Assert.AreSame(sangkent, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Sangkent_IsAStoneTerraceTown_WithStoneHousesTheTerracesChutesCartsAndApricots()
        {
            CityTownRecipe recipe = CityTownRecipes.Sangkent();
            Assert.AreSame(TownStyles.StoneTerrace, recipe.Style);
            Assert.AreEqual(TownArchitecture.StoneTerrace, TownStyles.StoneTerrace.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.StoneTerrace.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Sangkent);
            Assert.AreSame(TownStyles.StoneTerrace, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CaravanStone), "the Marco das Caravanas");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.TerraceWall), 3, "the terraces in three runs");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.TerraceWall && p.Variant == 1), "the fort of the 7th terrace");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StoneChute), 2, "stone chutes down the terraces");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ChuteCart), 2, "carts waiting");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ApricotTree), 4, "apricot trees");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.IrrigationChannel), 2, "irrigation channels");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.TerraceWall));
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.IrrigationChannel));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.StoneChute));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.IrrigationChannel), layout.WalkBounds), "the channels are walkable");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.TerraceWall), layout.WalkBounds).HasValue, "the terrace wall stops the walker");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.StoneChute), layout.WalkBounds).HasValue, "the chute landing stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.StoneTerrace && !l.Building.RootStilt && !l.Building.StiltLonghouse),
                "every house is a stone-and-loam house");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.Floors == 2), "two storeys");
            Assert.AreEqual(TownShopDressing.Outfitter, recipe.Lots.Single(l => l.ResidentId == "sisinnios").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the terraces");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.HearthGlow), 0f, "the terraces light up at dusk");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.TerraceStone), "the stone does not");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.ApricotBlossom), "nor the blossom");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.TerraceWall && p.Kind <= CityPropKind.CaravanStone))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.StoneTerrace);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }
        }

        [Test]
        public void TownPalette_AndNpcPalette_CoverEverySwatch()
        {
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, Enum.GetValues(typeof(TownSwatch)).Length);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);
            Assert.AreEqual(NpcPalette.Count, Enum.GetValues(typeof(NpcSwatch)).Length);
        }

        [Test]
        public void Sangkent_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Sangkent);
            float radius = CityTownRecipes.SangkentSolidRadius;
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
                Assert.AreEqual(CityTownRecipes.SangkentArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.SangkentHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Sangkent().EntryArm, "the caravan road in");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Sangkent_Residents_AreTheFichas_WithALookOfTheirOwn()
        {
            CitySpec sangkent = CityCatalog.Get(CityCatalog.Sangkent);
            CollectionAssert.AreEquivalent(Cast, sangkent.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var pamirSwatches = new[] { NpcSwatch.YakWool, NpcSwatch.Felt, NpcSwatch.ApricotBlossom, NpcSwatch.SilkRed, NpcSwatch.SilkBlue, NpcSwatch.PamirStone, NpcSwatch.Juniper, NpcSwatch.Glacier };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.ElfEars || look.Tusks || look.Snout || look.Horns || look.Gills, $"{id}: human");
                Assert.IsTrue(pamirSwatches.Contains(look.Main) || pamirSwatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the Pamir");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsNotNull(NpcModelBuilder.Build(look), $"{id} builds a model");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.AgasalhosSisinnios, sangkent.Residents.Single(r => r.ResidentId == "sisinnios").ShopId, "Sisinnios keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.AgasalhosSisinnios }, sangkent.ShopIds.ToArray());
        }

        [Test]
        public void Sangkent_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Abā, o Chefe das Calhas", CityTexts.ResidentName(null, Resident("aba")));
            Assert.AreEqual("Sisinnios, o Agasalheiro", CityTexts.ResidentName(null, Resident("sisinnios")));
        }

        [Test]
        public void Sangkent_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Sangkent, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Sangkent);
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

            Assert.AreEqual("agasalhos_sisinnios", layout.LotOfShop(CityShopCatalog.AgasalhosSisinnios).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Sisinnios's counter
        // ------------------------------------------------------------------

        [Test]
        public void SisinniossCounter_SellsTheGearOfEveryItemGateOfTheRoofOfTheWorld_AtBalancedPrices_AndTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.AgasalhosSisinnios);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            List<MapDefinition> gated = MapCatalog.Maps.Where(m => m.Region == MapRegion.TetoDoMundo && m.HasSpecialRequirement).ToList();
            CollectionAssert.AreEquivalent(new[] { 36, 38, 39, 40 }, gated.Select(m => m.MapId).ToArray());
            Assert.AreEqual(MapRequirementTags.WingsRequired, MapCatalog.Get(39).SpecialRequirementTag, "the Forte dos Ventos asks for wings or a flying mount");
            Assert.IsNull(MapRequirementRules.PrimaryItemOf(MapRequirementTags.WingsRequired), "no counter sells wings");
            List<string> gates = gated.Select(m => MapRequirementRules.PrimaryItemOf(m.SpecialRequirementTag)).Where(i => i != null).ToList();
            CollectionAssert.AreEquivalent(new[] { MapRequirementItems.ThermalClothes, MapRequirementItems.ClimbingCrampons, MapRequirementItems.StormCloak }, gates);
            foreach (string gate in gates)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, gate);
                Assert.IsNotNull(entry, $"Sisinnios sells {gate}");
                Assert.IsFalse(entry.Rotating, $"{gate} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, gate);
            }

            Assert.AreEqual(EquipmentSlot.Chest, ItemDatabase.GetById(MapRequirementItems.ThermalClothes).EquipSlot, "the thermal clothes are worn (map 36)");

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
                foreach (string gate in gates)
                {
                    Assert.Contains(gate, today);
                }

                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void Sangkent_OpensSisinniossCounter_AndNeverTheCaravaneer()
        {
            CitySpec sangkent = CityCatalog.Get(CityCatalog.Sangkent);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(sangkent, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.AgasalhosSisinnios, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Sangkent_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec sangkent = CityCatalog.Get(CityCatalog.Sangkent);
            var keys = new List<string> { sangkent.NameKey, sangkent.ButtonKey, sangkent.FlavourKey, CityShopCatalog.Get(CityShopCatalog.AgasalhosSisinnios).NameKey };
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
            StringAssert.Contains("terraço", all, "the terraces the city climbs");
            StringAssert.Contains("calha", all, "the chutes between them");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenBySangkentsResidents_WithTheStormCloakAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Sangkent).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "aba", "kustai" }, quests.Select(q => q.GiverResidentId).ToArray());
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
                foreach (string suffix in new[] { ".title", ".description", ".journal", ".offer", ".active" })
                {
                    Assert.IsNotNull(CityQuestTexts.GetFallback(spec.KeyBase + suffix), spec.KeyBase + suffix);
                }
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.StormCloak), "Kustai pays with the storm cloak (map 40)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_AbaOffersFirst_KustaiOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("aba", isActive, isDone, out CityQuestSpec chutes));
            Assert.AreEqual(CityQuestCatalog.SangkentChutes, chutes.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("kustai", isActive, isDone, out _), "the chain starts with Abā");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("sisinnios", isActive, isDone, out _), "Sisinnios gives no quest");

            done.Add(CityQuestCatalog.SangkentChutes);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("kustai", isActive, isDone, out CityQuestSpec board));
            Assert.AreEqual(CityQuestCatalog.SangkentBoard, board.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec first = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.SangkentChutes);
            Assert.AreEqual(first.OfferKey, CityQuestCatalog.Talk(quests, "aba"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.SangkentChutes));
            Assert.AreEqual(first.ActiveKey, CityQuestCatalog.Talk(quests, "aba"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "kustai"), "his quest waits for Abā's");
            quests.Shutdown();
        }
    }
}
