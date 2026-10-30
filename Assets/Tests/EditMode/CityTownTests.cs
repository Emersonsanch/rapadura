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

        [Test]
        public void ShopFronts_FaceTheIsometricCamera_InTheVila()
        {
            // The camera looks from the south-west (yaw 45): fronts facing south or west read; north/east only for homes.
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.VilaZefa);
            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.Shop || l.Kind == CityLotKind.Workshop || l.Kind == CityLotKind.Grandma))
            {
                Assert.IsTrue(lot.Front.x < -0.5f || lot.Front.y < -0.5f, $"{lot.Id} turns its back to the camera (front {lot.Front}).");
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

        [Test]
        public void BuildingMeshes_StayInsideTheirLot_AboveTheGround()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.VilaZefa);
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

        [Test]
        public void WholeVila_StaysUnderTheMobileTriangleBudget()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.VilaZefa, true);
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
                    TownKit.WallRun(b, new Vector3(prop.Position.x, 0f, prop.Position.y), new Vector3(prop.End.x, 0f, prop.End.y), layout.Style);
                }
                else if (prop.Kind != CityPropKind.Gate)
                {
                    TownKit.Prop(b, prop, layout.Style);
                }
            }

            Assert.Less(b.TriangleCount, 160000, "The whole Vila (ground, buildings, props, backdrop) is over the mobile budget.");
        }

        [Test]
        public void TownView_BuildsFewRenderersAndOnlyPrimitiveColliders()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.VilaZefa);
            CityTownView view = CityTownView.Build(layout, null, "test_view");
            try
            {
                Assert.Less(view.RendererCount, 60, "Too many renderers for a mobile town.");
                Assert.AreEqual(0, view.GetComponentsInChildren<MeshCollider>(true).Length, "No MeshCollider on town pieces.");
                Assert.IsNotNull(view.GetComponent<Rapadura.Gameplay.Environment.NonSolidPropZone>(), "The solid binder must skip the town.");
                Assert.IsNotNull(view.SafeZone);
                Assert.AreEqual(CityCatalog.VilaZefa, view.SafeZone.CityId);
                Assert.IsNotNull(view.Respawn);
                Assert.AreEqual(layout.Lots.Count(l => l.HasKeeper), view.Posts.Count);
                Assert.AreEqual(layout.Exits.Count, view.Exits.Count);
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
