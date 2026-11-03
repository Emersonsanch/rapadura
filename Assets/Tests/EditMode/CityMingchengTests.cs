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
    /// Mingcheng, a Cidade dos Nove Anéis, as a walkable late-region city (Docs/Cities.md, Docs/Mapas100.md §2.1 and map 41):
    /// its <see cref="CitySpec"/> among the late-region cities, the four residents (Bögü, Mōzak and Lin Deng of the ficha, and
    /// Fuduodan) at their doors with their own look, the RingWall style (courtyard halls of grey brick with curved roofs, the
    /// Torre do Tambor e do Sino, the wall of the 7th ring with the closed citadel gate, the canals with their humpback bridges,
    /// willows, lantern frames), the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S),
    /// Mōzak's counter with the Sun Seal (the only item gate of the region, map 42), the two city quests and the respawn of the
    /// maps whose return city it is.
    /// </summary>
    public class CityMingchengTests
    {
        private static readonly string[] Cast = { "bogu", "mozak", "lin_deng", "fuduodan" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "bogu", "Bögü" }, { "mozak", "Mestre Mōzak" }, { "lin_deng", "Lin Deng" }, { "fuduodan", "Fuduodan" },
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
        public void Mingcheng_HasACitySpec_AsALateRegionCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec mingcheng = CityCatalog.Get(CityCatalog.Mingcheng);
            Assert.IsNotNull(mingcheng, "Mingcheng has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, mingcheng.Region, "the Planície de Mingcheng borrows the Forest (always unlocked), as Sangkent");
            Assert.AreEqual(MapCatalog.Mingcheng, mingcheng.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.MingchengCityId, mingcheng.CityId, "the recipe's city id");
            Assert.AreEqual(CityCatalog.Mingcheng, MapCatalog.Get(41).ReturnCityId, "the return city of map 41");

            Assert.IsTrue(CityCatalog.IsLateRegionCity(mingcheng));
            Assert.IsFalse(CityCatalog.IsRaceCity(mingcheng));
            Assert.IsFalse(CityCatalog.IsHubCity(mingcheng));
            Assert.IsFalse(CityCatalog.Cities.Contains(mingcheng), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.VilaZefa, CityCatalog.ForRegion(BiomeType.Forest).CityId, "the Forest hub is still the Vila");
            Assert.IsFalse(mingcheng.HasGrandmaServices);
            Assert.IsFalse(mingcheng.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(mingcheng.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(mingcheng.CityId));
        }

        [Test]
        public void MapsOfMingcheng_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.Mingcheng).ToList();
            Assert.IsTrue(maps.Any(m => m.MapId == 41));
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.Mingcheng, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.AreEqual(CityCatalog.Sangkent, MapCatalog.RespawnCityFor("map_040_wakhan").CityId, "the Wakhan Pass still wakes in Sangkent");

            var cities = new CityProgress();
            CitySpec mingcheng = CityCatalog.Get(CityCatalog.Mingcheng);
            Assert.AreSame(mingcheng, cities.ApplyDefeatIn(mingcheng), "a defeat on the plain wakes the cousins by the Drum and Bell Tower");
            Assert.AreSame(mingcheng, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Mingcheng_IsARingWallTown_WithCourtyardHallsTheBellTowerTheRingWallCanalsWillowsAndLanterns()
        {
            CityTownRecipe recipe = CityTownRecipes.Mingcheng();
            Assert.AreSame(TownStyles.RingWall, recipe.Style);
            Assert.AreEqual(TownArchitecture.RingWall, TownStyles.RingWall.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.RingWall.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Mingcheng);
            Assert.AreSame(TownStyles.RingWall, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.BellDrumTower), "the Torre do Tambor e do Sino");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.RingWall), 3, "the wall of the 7th ring in three runs");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.RingWall && p.Variant == 1), "the closed citadel gate");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CanalBridge), 2, "canals with humpback bridges");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Willow), 4, "willows");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.LanternFrame), 2, "lantern frames of the Festa das Lanternas");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.RingWall));
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.CanalBridge));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.BellDrumTower));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.CanalBridge), layout.WalkBounds), "the bridges are walkable");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.RingWall), layout.WalkBounds).HasValue, "the ring wall stops the walker");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.BellDrumTower), layout.WalkBounds).HasValue, "the tower stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.RingWallHall && !l.Building.StoneTerrace && !l.Building.SpireHouse),
                "every house is a courtyard hall");
            Assert.AreEqual(TownShopDressing.LanternMaker, recipe.Lots.Single(l => l.ResidentId == "mozak").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the wall of the inner rings");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.LanternPaper), 0f, "thousands of lanterns at night");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.GreyBrick), "the brick does not");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.Lacquer), "nor the lacquer");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.RingWall && p.Kind <= CityPropKind.LanternFrame))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.RingWall);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.RingWall);
            Assert.Greater(gate.TriangleCount, 40, "the three-arch gate");
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
        public void Mingcheng_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Mingcheng);
            float radius = CityTownRecipes.MingchengSolidRadius;
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
                Assert.AreEqual(CityTownRecipes.MingchengArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.MingchengHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Mingcheng().EntryArm, "the river road in");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Mingcheng_Residents_AreTheFichas_WithALookOfTheirOwn()
        {
            CitySpec mingcheng = CityCatalog.Get(CityCatalog.Mingcheng);
            CollectionAssert.AreEquivalent(Cast, mingcheng.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.Lacquer, NpcSwatch.ImperialGold, NpcSwatch.Jade, NpcSwatch.LanternPaper, NpcSwatch.GreyBrick, NpcSwatch.Ink, NpcSwatch.Celadon, NpcSwatch.Willow };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.ElfEars || look.Tusks || look.Snout || look.Horns || look.Gills, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of Mingcheng");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsNotNull(NpcModelBuilder.Build(look), $"{id} builds a model");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.LanternasMozak, mingcheng.Residents.Single(r => r.ResidentId == "mozak").ShopId, "Mōzak keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.LanternasMozak }, mingcheng.ShopIds.ToArray());
        }

        [Test]
        public void Mingcheng_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Bögü, o Chefe da Guarda dos Anéis", CityTexts.ResidentName(null, Resident("bogu")));
            Assert.AreEqual("Mestre Mōzak, o Guardião das Lanternas", CityTexts.ResidentName(null, Resident("mozak")));
        }

        [Test]
        public void Mingcheng_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Mingcheng, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Mingcheng);
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

            Assert.AreEqual("lanternas_mozak", layout.LotOfShop(CityShopCatalog.LanternasMozak).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Mōzak's counter
        // ------------------------------------------------------------------

        [Test]
        public void MozaksCounter_SellsTheSunSeal_TheOnlyItemGateOfThePlain_AtBalancedPrices_AndTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.LanternasMozak);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            List<MapDefinition> gated = MapCatalog.Maps.Where(m => m.Region == MapRegion.PlanicieDeMingcheng && m.HasSpecialRequirement).ToList();
            CollectionAssert.AreEquivalent(new[] { 42 }, gated.Select(m => m.MapId).ToArray());
            List<string> gates = gated.Select(m => MapRequirementRules.PrimaryItemOf(m.SpecialRequirementTag)).Where(i => i != null).ToList();
            CollectionAssert.AreEquivalent(new[] { MapRequirementItems.SunSeal }, gates);
            foreach (string gate in gates)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, gate);
                Assert.IsNotNull(entry, $"Mōzak sells {gate}");
                Assert.IsFalse(entry.Rotating, $"{gate} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, gate);
            }

            Assert.IsTrue(ItemDatabase.GetById(MapRequirementItems.SunSeal).IsBound, "the Sun Seal is bound to the hero");

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
        public void Mingcheng_OpensMozaksCounter_AndNeverTheCaravaneer()
        {
            CitySpec mingcheng = CityCatalog.Get(CityCatalog.Mingcheng);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(mingcheng, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.LanternasMozak, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Mingcheng_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec mingcheng = CityCatalog.Get(CityCatalog.Mingcheng);
            var keys = new List<string> { mingcheng.NameKey, mingcheng.ButtonKey, mingcheng.FlavourKey, CityShopCatalog.Get(CityShopCatalog.LanternasMozak).NameKey };
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
            StringAssert.Contains("anel", all, "the rings of the city");
            StringAssert.Contains("lanterna", all, "its lanterns");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByMingchengsResidents_WithTheSunSealAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Mingcheng).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "bogu", "lin_deng" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.SunSeal), "Lin Deng pays with the Sun Seal (map 42)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_BoguOffersFirst_LinDengOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("bogu", isActive, isDone, out CityQuestSpec lions));
            Assert.AreEqual(CityQuestCatalog.MingchengLions, lions.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("lin_deng", isActive, isDone, out _), "the chain starts with Bögü");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("mozak", isActive, isDone, out _), "Mōzak gives no quest");

            done.Add(CityQuestCatalog.MingchengLions);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("lin_deng", isActive, isDone, out CityQuestSpec canal));
            Assert.AreEqual(CityQuestCatalog.MingchengCanal, canal.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec first = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.MingchengLions);
            Assert.AreEqual(first.OfferKey, CityQuestCatalog.Talk(quests, "bogu"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.MingchengLions));
            Assert.AreEqual(first.ActiveKey, CityQuestCatalog.Talk(quests, "bogu"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "lin_deng"), "his quest waits for Bögü's");
            quests.Shutdown();
        }
    }
}
