using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The walkable towns (Docs/Cities.md §5): layout rules on every city's recipe (with and without the caravaneer's
    /// stall), the Vila's shops and keepers, determinism, the modular kit's meshes and budgets, and the safe-zone lookup.
    /// </summary>
    public class CityTownTests
    {
        private static IEnumerable<TestCaseData> AllLayouts()
        {
            foreach (CitySpec city in CityCatalog.Cities)
            {
                yield return new TestCaseData(city.CityId, false).SetName($"Layout_{city.CityId}");
                yield return new TestCaseData(city.CityId, true).SetName($"Layout_{city.CityId}_WithCaravaneer");
            }
        }

        private static IEnumerable<TestCaseData> CityIds()
        {
            foreach (CitySpec city in CityCatalog.Cities)
            {
                yield return new TestCaseData(city.CityId).SetName($"Town_{city.CityId}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            CityTownRecipes.ClearCache();
        }

        [TestCaseSource(nameof(AllLayouts))]
        public void EveryTown_PassesAllLayoutRules(string cityId, bool mascate)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId, mascate);
            Assert.IsNotNull(layout);
            List<string> problems = CityLayoutRules.Validate(layout);
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [TestCaseSource(nameof(AllLayouts))]
        public void NoBuilding_OverlapsAStreet_ThePlaza_OrAnotherLot(string cityId, bool mascate)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId, mascate);
            var problems = new List<string>();
            CityLayoutRules.CheckBuildingsOffStreets(layout, problems);
            CityLayoutRules.CheckLotsApart(layout, problems);
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [TestCaseSource(nameof(AllLayouts))]
        public void EveryDoor_IsReachableFromThePlaza_WithA1Point2MetreCorridor(string cityId, bool mascate)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId, mascate);
            var problems = new List<string>();
            CityLayoutRules.CheckReachability(layout, problems, CityLayoutRules.MinCorridor);
            Assert.IsEmpty(problems, string.Join("\n", problems));
            Assert.GreaterOrEqual(CityLayoutRules.MinCorridor, CityLayoutRules.WalkerRadius * 2f + 0.5f, "The corridor must fit the 0.35 m capsule with room to spare.");
        }

        [TestCaseSource(nameof(AllLayouts))]
        public void Keepers_StandClearOfEveryCollider_AndReachTheirDoorApproach(string cityId, bool mascate)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId, mascate);
            foreach (CityLot lot in layout.Lots.Where(l => l.HasKeeper))
            {
                Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, lot.StandPoint), CityLayoutRules.WalkerRadius, $"Keeper of {lot.Id} is inside a collider.");
                Assert.LessOrEqual(Vector2.Distance(lot.StandPoint, lot.DoorApproach), lot.InteractRadius, $"{lot.Id}: the door approach is out of range.");
            }
        }

        [TestCaseSource(nameof(AllLayouts))]
        public void SafeZone_CoversTheWholeTown(string cityId, bool mascate)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId, mascate);
            foreach (CityLot lot in layout.Lots)
            {
                Assert.IsTrue(CityGeometry.Contains(layout.SafeZone, lot.Footprint), lot.Id);
                Assert.IsTrue(layout.SafeZone.Contains(lot.StandPoint) && layout.SafeZone.Contains(lot.DoorApproach), lot.Id);
            }

            foreach (CityExit exit in layout.Exits)
            {
                Assert.IsTrue(layout.SafeZone.Contains(exit.Target), exit.Id);
            }

            Assert.IsTrue(layout.SafeZone.Contains(layout.Respawn));
            Assert.IsTrue(layout.SafeZone.Contains(layout.Arrival));
        }

        [Test]
        public void Vila_HasAShopFrontForEveryShopOfTheCity_KeptByItsResident()
        {
            CitySpec vila = CityCatalog.Get(CityCatalog.VilaZefa);
            CityLayout layout = CityTownRecipes.LayoutFor(vila.CityId);
            foreach (string shopId in vila.ShopIds)
            {
                CityLot lot = layout.LotOfShop(shopId);
                Assert.IsNotNull(lot, $"No shop front for {shopId}.");
                CityResidentSpec keeper = vila.Residents.FirstOrDefault(r => r.ShopId == shopId);
                if (keeper != null)
                {
                    Assert.AreEqual(keeper.ResidentId, lot.ResidentId, $"{shopId} is kept by the wrong resident.");
                }
            }

            Assert.IsNotNull(layout.Lot("casa_vo_edna"), "Vo Edna's house is missing.");
            Assert.IsNotNull(layout.Lots.Find(l => l.Kind == CityLotKind.LampHouse), "The lamp house is missing.");
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "The Cais da Barca is missing.");
            Assert.GreaterOrEqual(layout.Exits.Count(e => e.Kind == CityExitKind.ToMap), 1);
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "The respawn point is the plaza (Docs/Cities.md §3.8).");
        }

        [TestCaseSource(nameof(CityIds))]
        public void ShopFronts_AndResidentHomes_FaceTheIsometricCamera(string cityId)
        {
            // The camera looks from the south-west (yaw 45): fronts facing south or west read; north/east only for plain homes.
            CityLayout layout = CityTownRecipes.LayoutFor(cityId);
            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.Shop || l.Kind == CityLotKind.Workshop || l.Kind == CityLotKind.Grandma ||
                                                           (l.Kind == CityLotKind.House && l.HasKeeper)))
            {
                Assert.IsTrue(lot.Front.x < -0.5f || lot.Front.y < -0.5f, $"{cityId}/{lot.Id} turns its back to the camera (front {lot.Front}).");
            }
        }

        [TestCaseSource(nameof(CityIds))]
        public void EveryCity_HasItsOwnHandRecipe_WithAShopFrontPerShop_AndAHomePerResident(string cityId)
        {
            Assert.IsNotNull(CityTownRecipes.HandRecipe(cityId), $"{cityId} still walks on the default plan.");
            CitySpec city = CityCatalog.Get(cityId);
            CityLayout layout = CityTownRecipes.LayoutFor(cityId);
            foreach (string shopId in city.ShopIds)
            {
                CityLot lot = layout.LotOfShop(shopId);
                Assert.IsNotNull(lot, $"{cityId}: no shop front for {shopId}.");
                CityResidentSpec keeper = city.Residents.FirstOrDefault(r => r.ShopId == shopId);
                if (keeper != null)
                {
                    Assert.AreEqual(keeper.ResidentId, lot.ResidentId, $"{cityId}: {shopId} is kept by the wrong resident.");
                }
            }

            foreach (CityResidentSpec resident in city.Residents)
            {
                Assert.IsNotNull(layout.LotOfResident(resident.ResidentId), $"{cityId}: {resident.ResidentId} has no door to stand at.");
            }

            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), $"{cityId}: the Cais da Barca is missing.");
            Assert.GreaterOrEqual(layout.Exits.Count(e => e.Kind == CityExitKind.ToMap), 1, $"{cityId}: no road back to the map.");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "The respawn point is the plaza (Docs/Cities.md §3.8).");
            Assert.IsTrue(layout.Exits.Any(e => e.Id == "exit_" + CityArm.South.ToString().ToLowerInvariant()), $"{cityId}: the road in from the map must also lead out.");
        }

        [Test]
        public void Qocho_HasItsShapeSignature_Caravanserai_DryingLofts_Karez_Cistern_Ruins()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.AreiaBranca);
            CityLot caravanserai = layout.LotOfShop(CityShopCatalog.SelariaVava);
            Assert.IsTrue(caravanserai.Building.Arcade, "The Selaria is the caravanserai: an arcade on brick piers.");
            Assert.AreEqual(TownShopDressing.Selaria, caravanserai.Building.Dressing);
            Assert.GreaterOrEqual(layout.Lots.Count(l => l.Building != null && l.Building.DryingLoft), 3, "Grape-drying lofts on the roofs.");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House).All(l => l.Building.RammedEarth), "Qocho's houses are rammed earth.");
            Assert.AreEqual(TownRoofKind.Dome, layout.LotOfResident("socorro").Building.Roof, "Yardena lives under a cistern dome.");
            Assert.AreEqual(1, layout.Props.Count(p => p.Kind == CityPropKind.CisternDome));
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.KarezShaft), 4, "The karez: a line of well shafts.");
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.KarezChannel), 1);
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.Ruin && p.Variant == 0), 2, "Meroë pyramids at the edge.");
            Assert.AreEqual(1, layout.Props.Count(p => p.Kind == CityPropKind.Camel));
        }

        [Test]
        public void Litema_HasItsShapeSignature_Rondavels_CurvedWalls_Stela_RockArt()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.SerraAlta);
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House).All(l => l.Building.Roof == TownRoofKind.Cone), "Litema's homes are rondavels.");
            Assert.AreEqual(CityLotKind.Workshop, layout.LotOfShop(CityShopCatalog.ForjaTonho).Kind, "Mestre Mihr works at an open forge.");
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.FenceRun && p.Scale > 2f), 12, "The tall curved dry-stone enclosure.");
            Assert.AreEqual(1, layout.Props.Count(p => p.Kind == CityPropKind.Stela));
            Assert.AreEqual(1, layout.Props.Count(p => p.Kind == CityPropKind.ConicalTower));
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.RockArt), 2);
            Assert.IsTrue(layout.Props.Any(p => p.Kind == CityPropKind.Tree && p.Variant == 3), "A baobab on the plaza.");
            Assert.IsTrue(layout.LotOfResident("graca").Building.Dressing == TownShopDressing.Weaver);
        }

        [Test]
        public void Lamparinas_HasItsShapeSignature_CliffHouses_RopeBridges_LampLitStreets_AndAPermanentDusk()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Lamparinas);
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.Roof == TownRoofKind.Rock), "Houses cut into the tuff.");
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.Cliff), 4);
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.RopeBridge), 2);
            Assert.GreaterOrEqual(layout.Props.Count(p => p.Kind == CityPropKind.LampString), 6, "Lamps strung along the streets.");
            Assert.AreEqual(1, layout.Props.Count(p => p.Kind == CityPropKind.Vishapakar));
            Assert.AreEqual(1, layout.Props.Count(p => p.Kind == CityPropKind.Fountain));
            Assert.Greater(layout.AmbientNight, 0.5f, "The cave town keeps its lamps lit.");
            Assert.Greater(layout.NightLights, 6);

            // Every cliff stands behind the houses, never on a street or over a door path.
            foreach (CityProp cliff in layout.Props.Where(p => p.Kind == CityPropKind.Cliff))
            {
                CitySolid? solid = CitySolids.PropSolid(cliff, layout.WalkBounds);
                Assert.IsTrue(solid.HasValue);
                foreach (CityStreet street in layout.Streets)
                {
                    Assert.IsFalse(CityGeometry.Overlaps(solid.Value.Footprint, street.Bounds), $"A cliff at {cliff.Position} cuts the {street.Arm} street.");
                }

                foreach (CityLot lot in layout.Lots)
                {
                    Assert.IsFalse(CityGeometry.Overlaps(solid.Value.Footprint, lot.Footprint), $"A cliff at {cliff.Position} cuts into {lot.Id}.");
                }
            }
        }

        [Test]
        public void NightLights_KeepTheMinimumNight_OfACaveTown()
        {
            var go = new GameObject("lights");
            try
            {
                CityNightLights lights = go.AddComponent<CityNightLights>();
                lights.MinimumNight = 0.65f;
                lights.NightOverride = 0f;
                lights.Refresh();
                Assert.AreEqual(0.65f, lights.Night, 1e-4f, "Broad day outside, dusk in the cave.");
                lights.NightOverride = 1f;
                lights.Refresh();
                Assert.AreEqual(1f, lights.Night, 1e-4f);
            }
            finally
            {
                Object.DestroyImmediate(go);
                TownPalette.SetNightGlow(0f);
            }
        }

        [Test]
        public void Generation_IsDeterministic()
        {
            CityLayout a = CityLayoutGenerator.Generate(CityTownRecipes.VilaEdna());
            CityLayout b = CityLayoutGenerator.Generate(CityTownRecipes.VilaEdna());
            Assert.AreEqual(a.Lots.Count, b.Lots.Count);
            Assert.AreEqual(a.Props.Count, b.Props.Count);
            for (int i = 0; i < a.Lots.Count; i++)
            {
                Assert.AreEqual(a.Lots[i].Footprint, b.Lots[i].Footprint);
                Assert.AreEqual(a.Lots[i].Building.CacheKey, b.Lots[i].Building.CacheKey);
            }

            for (int i = 0; i < a.Props.Count; i++)
            {
                Assert.AreEqual(a.Props[i].Position, b.Props[i].Position);
            }
        }

        [Test]
        public void RegionStyles_GiveEachCityItsOwnShapeSignature()
        {
            Assert.AreEqual(TownArchitecture.Sogdian, TownStyles.For(BiomeType.Forest).Architecture);
            Assert.AreEqual(TownArchitecture.Oasis, TownStyles.For(BiomeType.Desert).Architecture);
            Assert.AreEqual(TownArchitecture.Rondavel, TownStyles.For(BiomeType.Mountain).Architecture);
            Assert.AreEqual(TownArchitecture.CaveTuff, TownStyles.For(BiomeType.Caves).Architecture);
            Assert.AreEqual(TownRoofKind.Cone, TownStyles.For(BiomeType.Mountain).HouseRoof);
        }

        [Test]
        public void Geometry_RotateMatchesUnityYaw()
        {
            foreach (float yaw in new[] { 0f, 90f, 180f, 270f, 33f })
            {
                Vector3 unity = Quaternion.Euler(0f, yaw, 0f) * new Vector3(1.3f, 0f, -0.7f);
                Vector2 ours = CityGeometry.Rotate(new Vector2(1.3f, -0.7f), yaw);
                Assert.AreEqual(unity.x, ours.x, 1e-4f);
                Assert.AreEqual(unity.z, ours.y, 1e-4f);
            }

            Vector3 front = Quaternion.Euler(0f, CityGeometry.YawFacing(Vector2.left), 0f) * Vector3.back;
            Assert.AreEqual(-1f, front.x, 1e-4f);
        }

        // ------------------------------------------------------------------
        // Kit
        // ------------------------------------------------------------------

        [Test]
        public void Palette_HasOneCellPerSwatch()
        {
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, System.Enum.GetValues(typeof(TownSwatch)).Length);
            foreach (TownSwatch swatch in System.Enum.GetValues(typeof(TownSwatch)))
            {
                Assert.AreEqual(swatch, TownPalette.SwatchAt(TownPalette.UvOf(swatch)));
            }
        }

        [Test]
        public void EveryBuildingOfEveryTown_BuildsWithinBudget()
        {
            foreach (CitySpec city in CityCatalog.Cities)
            {
                CityLayout layout = CityTownRecipes.LayoutFor(city.CityId, true);
                foreach (CityLot lot in layout.Lots.Where(l => l.Kind != CityLotKind.Market))
                {
                    var b = new TownMeshBuilder();
                    TownBuildingBuilder.Build(b, lot.Building);
                    Assert.Greater(b.TriangleCount, 40, $"{city.CityId}/{lot.Id} built (almost) nothing.");
                    Assert.Less(b.TriangleCount, 6000, $"{city.CityId}/{lot.Id} is over the per-building budget.");
                }
            }
        }

        [TestCaseSource(nameof(CityIds))]
        public void BuildingMeshes_StayInsideTheirLot_AboveTheGround(string cityId)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId);
            foreach (CityLot lot in layout.Lots.Where(l => l.Kind != CityLotKind.Market))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                float hw = lot.Width * 0.5f + 1.2f;
                float hd = lot.Depth * 0.5f + 1.2f;
                if (lot.Kind == CityLotKind.LampHouse)
                {
                    hw += TownBuildingBuilder.TowerSize;
                }

                foreach (Vector3 v in b.Vertices)
                {
                    Assert.LessOrEqual(Mathf.Abs(v.x), hw, $"{lot.Id} pokes out sideways ({v}).");
                    Assert.LessOrEqual(Mathf.Abs(v.z), hd, $"{lot.Id} pokes out front/back ({v}).");
                    Assert.GreaterOrEqual(v.y, -0.05f, $"{lot.Id} goes under the ground ({v}).");
                }
            }
        }

        [Test]
        public void Props_Ground_AndKeeperFigures_Build()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.VilaZefa);
            var b = new TownMeshBuilder();
            TownGroundBuilder.Build(b, layout);
            Assert.Greater(b.TriangleCount, 500);
            int total = b.TriangleCount;
            foreach (CityPropKind kind in System.Enum.GetValues(typeof(CityPropKind)))
            {
                var p = new TownMeshBuilder();
                TownKit.Prop(p, new CityProp { Kind = kind, Scale = 1f }, layout.Style);
                Assert.Less(p.TriangleCount, 2500, $"{kind} is too heavy.");
                total += p.TriangleCount;
            }

            foreach (TownVegetation vegetation in System.Enum.GetValues(typeof(TownVegetation)))
            {
                for (int variant = 0; variant < 4; variant++)
                {
                    var t = new TownMeshBuilder();
                    TownKit.Tree(t, vegetation, variant, 1f);
                    Assert.Greater(t.TriangleCount, 10, $"{vegetation} tree {variant} is empty.");
                    Assert.Less(t.TriangleCount, 600, $"{vegetation} tree {variant} is too heavy for the backdrop.");
                }
            }

            var villager = new TownMeshBuilder();
            TownKit.Villager(villager, "lazaro", layout.Style);
            Assert.Less(villager.TriangleCount, 400);
            Assert.Greater(total, 0);
        }

        [TestCaseSource(nameof(CityIds))]
        public void WholeTown_StaysUnderTheMobileTriangleBudget(string cityId)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId, true);
            var b = new TownMeshBuilder();
            TownGroundBuilder.Build(b, layout);
            foreach (CityLot lot in layout.Lots.Where(l => l.Kind != CityLotKind.Market))
            {
                TownBuildingBuilder.Build(b, lot.Building);
            }

            foreach (CityProp prop in layout.Props)
            {
                if (prop.Kind == CityPropKind.FenceRun)
                {
                    TownKit.WallRun(b, new Vector3(prop.Position.x, 0f, prop.Position.y), new Vector3(prop.End.x, 0f, prop.End.y), layout.Style, prop.Scale);
                }
                else if (prop.Kind != CityPropKind.Gate)
                {
                    TownKit.Prop(b, prop, layout.Style);
                }
            }

            Assert.Less(b.TriangleCount, 160000, $"The whole of {cityId} (ground, buildings, props, backdrop) is over the mobile budget.");
        }

        [Test]
        public void RegionalProps_EachBuildWithinBudget_AndTheSegmentsFollowTheirLength()
        {
            TownStyle style = TownStyles.For(BiomeType.Caves);
            foreach (CityPropKind kind in new[] { CityPropKind.Cliff, CityPropKind.RopeBridge, CityPropKind.LampString, CityPropKind.KarezChannel })
            {
                var shortRun = new TownMeshBuilder();
                TownKit.Prop(shortRun, new CityProp { Kind = kind, Position = Vector2.zero, End = new Vector2(6f, 0f) }, style);
                var longRun = new TownMeshBuilder();
                TownKit.Prop(longRun, new CityProp { Kind = kind, Position = Vector2.zero, End = new Vector2(24f, 0f) }, style);
                Assert.Greater(shortRun.TriangleCount, 20, $"{kind} built nothing.");
                Assert.Greater(longRun.TriangleCount, shortRun.TriangleCount, $"{kind} does not grow with its length.");
                Assert.Less(longRun.TriangleCount, 6000, $"{kind} is too heavy.");
                Assert.IsTrue(TownKit.IsSegment(kind));
            }

            foreach (CityPropKind kind in new[] { CityPropKind.KarezShaft, CityPropKind.CisternDome, CityPropKind.Camel, CityPropKind.RockArt, CityPropKind.Vishapakar,
                         CityPropKind.Fountain, CityPropKind.ConicalTower })
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, new CityProp { Kind = kind }, style);
                Assert.Greater(b.TriangleCount, 20, $"{kind} built nothing.");
                Assert.Less(b.TriangleCount, 2500, $"{kind} is too heavy.");
                Assert.IsTrue(CitySolids.PropSolid(new CityProp { Kind = kind }, new Rect(-10f, -10f, 20f, 20f)).HasValue, $"{kind} needs a primitive collider.");
            }

            for (int variant = 0; variant < 3; variant++)
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, new CityProp { Kind = CityPropKind.Ruin, Variant = variant }, style);
                Assert.Greater(b.TriangleCount, 20, $"Ruin {variant} built nothing.");
            }

            // The cliff's collider is the box behind its line (left of the direction), the bridge and the lamps are overhead.
            CitySolid? cliff = CitySolids.PropSolid(new CityProp { Kind = CityPropKind.Cliff, Position = new Vector2(-5f, 2f), End = new Vector2(5f, 2f) }, new Rect(-20f, -20f, 40f, 40f));
            Assert.AreEqual(Rect.MinMaxRect(-5f, 2f, 5f, 2f + TownKit.CliffDepth), cliff.Value.Footprint);
            Assert.IsFalse(CitySolids.PropSolid(new CityProp { Kind = CityPropKind.RopeBridge, Position = Vector2.zero, End = Vector2.one }, new Rect(-20f, -20f, 40f, 40f)).HasValue);
            Assert.IsFalse(CitySolids.PropSolid(new CityProp { Kind = CityPropKind.LampString, Position = Vector2.zero, End = Vector2.one }, new Rect(-20f, -20f, 40f, 40f)).HasValue);
        }

        [Test]
        public void RegionalBuildings_GetTheirModules()
        {
            var plain = new TownBuildingSpec { Kind = CityLotKind.House, Width = 7f, Depth = 5.5f, PorchDepth = 1.4f, Seed = "a" };
            var qocho = plain.Clone();
            qocho.RammedEarth = true;
            qocho.DryingLoft = true;
            qocho.Arcade = true;
            qocho.FullVaranda = true;
            Assert.AreNotEqual(plain.CacheKey, qocho.CacheKey, "The cache must tell the regional buildings apart.");
            Assert.Greater(Triangles(qocho), Triangles(plain) + 200, "Lattice loft, lift lines and the arcade add their pieces.");

            var rondavel = plain.Clone();
            rondavel.Roof = TownRoofKind.Cone;
            rondavel.Dressing = TownShopDressing.Weaver;
            var bare = rondavel.Clone();
            bare.Dressing = TownShopDressing.None;
            Assert.Greater(Triangles(rondavel), Triangles(bare), "A resident's house shows its dressing.");

            foreach (TownShopDressing dressing in System.Enum.GetValues(typeof(TownShopDressing)))
            {
                if (dressing == TownShopDressing.None || dressing == TownShopDressing.LampHouse)
                {
                    continue;
                }

                var shop = plain.Clone();
                shop.Kind = CityLotKind.Shop;
                shop.Dressing = dressing;
                var none = shop.Clone();
                none.Dressing = TownShopDressing.None;
                Assert.Greater(Triangles(shop), Triangles(none), $"Dressing {dressing} draws nothing.");
            }
        }

        private static int Triangles(TownBuildingSpec spec)
        {
            var b = new TownMeshBuilder();
            TownBuildingBuilder.Build(b, spec);
            return b.TriangleCount;
        }

        [TestCaseSource(nameof(CityIds))]
        public void TownView_BuildsFewRenderersAndOnlyPrimitiveColliders(string cityId)
        {
            CityLayout layout = CityTownRecipes.LayoutFor(cityId);
            CityTownView view = CityTownView.Build(layout, null, "test_view_" + cityId);
            try
            {
                Assert.Less(view.RendererCount, 60, "Too many renderers for a mobile town.");
                Assert.AreEqual(0, view.GetComponentsInChildren<MeshCollider>(true).Length, "No MeshCollider on town pieces.");
                Assert.IsNotNull(view.GetComponent<Rapadura.Gameplay.Environment.NonSolidPropZone>(), "The solid binder must skip the town.");
                Assert.IsNotNull(view.SafeZone);
                Assert.AreEqual(cityId, view.SafeZone.CityId);
                Assert.IsNotNull(view.Respawn);
                Assert.AreEqual(layout.Lots.Count(l => l.HasKeeper), view.Posts.Count);
                Assert.AreEqual(layout.Exits.Count, view.Exits.Count);
                Assert.LessOrEqual(view.NightLights.Lights.Count, 8, "Night lights over the mobile budget.");
                Assert.AreEqual(layout.AmbientNight, view.NightLights.MinimumNight);
                foreach (CityResidentPost post in view.Posts)
                {
                    Assert.IsTrue(post.GetComponent<SphereCollider>().isTrigger, post.LotId);
                }
            }
            finally
            {
                Object.DestroyImmediate(view.gameObject);
                TownMeshCache.Clear();
            }
        }

        // ------------------------------------------------------------------
        // Safe zone
        // ------------------------------------------------------------------

        [Test]
        public void SafeZones_AreWildsEverywhere_WithNoCityLoaded()
        {
            Assert.AreEqual(0, CitySafeZones.Count);
            Assert.AreEqual(ZoneKind.Wilds, CitySafeZones.ZoneAt(Vector3.zero));
            Assert.IsFalse(CitySafeZones.IsSafeAt(Vector3.zero));
        }

        [Test]
        public void SafeZoneVolume_MakesItsBoxACity_AndNothingCanBeHurtThere()
        {
            var go = new GameObject("zone");
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(10f, 10f, 10f);
            CitySafeZoneVolume volume = go.AddComponent<CitySafeZoneVolume>();
            volume.Configure(CityCatalog.VilaZefa);
            CitySafeZones.Register(volume);
            try
            {
                Assert.AreEqual(ZoneKind.City, CitySafeZones.ZoneAt(new Vector3(2f, 1f, -3f)));
                Assert.AreEqual(CityCatalog.VilaZefa, CitySafeZones.CityAt(Vector3.zero));
                Assert.IsTrue(CitySafeZones.IsSafeAt(Vector3.zero));
                Assert.AreEqual(ZoneKind.Wilds, CitySafeZones.ZoneAt(new Vector3(20f, 0f, 0f)));
            }
            finally
            {
                CitySafeZones.Unregister(volume);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SceneTransition_CityVisit_LoadsMainThenTheMap_WithTheDockRequest()
        {
            var loader = new RecordingLoader();
            var service = new SceneTransitionService(new WorldMapProgress(), loader);
            Assert.IsTrue(service.BeginCityVisit(CityCatalog.VilaZefa));
            Assert.AreEqual(SceneTransitionService.DefaultCombatSceneName, loader.Last);
            Assert.IsTrue(service.HasPendingCityVisit);
            Assert.IsFalse(service.BeginCityVisit(CityCatalog.VilaZefa), "A second visit while one is pending.");

            Assert.IsTrue(service.EndCityVisit(openFastTravel: true));
            Assert.AreEqual(SceneTransitionService.DefaultWorldMapSceneName, loader.Last);
            Assert.IsFalse(service.HasPendingCityVisit);
            Assert.IsTrue(service.OpenFastTravelOnMapReturn);
            Assert.IsFalse(service.EndCityVisit());

            service.AbandonPendingEncounter();
            Assert.IsFalse(service.OpenFastTravelOnMapReturn);
        }

        private sealed class RecordingLoader : ISceneLoader
        {
            public string Last;

            public void LoadScene(string sceneName) => Last = sceneName;
        }
    }
}
