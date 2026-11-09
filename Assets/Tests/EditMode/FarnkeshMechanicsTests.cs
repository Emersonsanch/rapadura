using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Picos de Farnkesh mechanics (Docs/Mapas100.md 54–58): the gusts (<see cref="WindGustRules"/>), the rope baskets and the
    /// wind currents (<see cref="SkyRouteRules"/>), and the places the layout declares for them.
    /// </summary>
    public class FarnkeshMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.FarnkeshRegionId);

        private static IEnumerable<SkyRoute> Baskets => OverworldLayouts.SkyRoutes.Where(r => r.Kind == SkyRouteKind.RopeBasket);

        private static IEnumerable<SkyRoute> Currents => OverworldLayouts.SkyRoutes.Where(r => r.Kind == SkyRouteKind.Glide);

        private static SkyRoute Route(SkyRouteKind kind, int group = 0) => new SkyRoute { Id = "t", Kind = kind, A = Vector2.zero, B = new Vector2(180f, 0f), CurrentGroup = group };

        // ------------------------------------------------------------------
        // Gusts
        // ------------------------------------------------------------------

        [TestCase(0f, WindGustPhase.Calm)]
        [TestCase(15.4f, WindGustPhase.Calm)]
        [TestCase(15.5f, WindGustPhase.Warning)]
        [TestCase(18.4f, WindGustPhase.Warning)]
        [TestCase(18.5f, WindGustPhase.Blowing)]
        [TestCase(19.9f, WindGustPhase.Blowing)]
        [TestCase(20f, WindGustPhase.Calm)]
        [TestCase(38.6f, WindGustPhase.Blowing)]
        public void Gust_Phase_FollowsTheTwentySecondCycle(float time, WindGustPhase expected)
        {
            Assert.AreEqual(expected, WindGustRules.PhaseAt(time));
        }

        [Test]
        public void Gust_Direction_AlternatesSides()
        {
            Assert.AreEqual(Vector2.right, WindGustRules.Direction(0, Vector2.right));
            Assert.AreEqual(Vector2.left, WindGustRules.Direction(1, Vector2.right));
            Assert.AreEqual(0, WindGustRules.GustIndex(19.9f));
            Assert.AreEqual(1, WindGustRules.GustIndex(20f));
        }

        [Test]
        public void Gust_Push_HoldingStops_CloakHalves()
        {
            Assert.AreEqual(Vector2.zero, WindGustRules.PushVelocity(Vector2.right, true, false));
            Assert.AreEqual(WindGustRules.PushSpeed, WindGustRules.PushVelocity(Vector2.right, false, false).x, 1e-4f);
            Assert.AreEqual(WindGustRules.PushSpeed * WindGustRules.StormCloakFactor, WindGustRules.PushVelocity(Vector2.right, false, true).x, 1e-4f);
            Assert.AreEqual(7.5f, WindGustRules.PushSpeed * WindGustRules.BlowSeconds, 1e-4f, "metres pushed by a whole gust");
        }

        [Test]
        public void Gust_Axis_IsAcrossTheBridge_OrTheAreasOwn()
        {
            var bridge = new[] { new[] { new Vector2(0f, 0f), new Vector2(100f, 0f) } };
            var area = new[] { Rect.MinMaxRect(200f, 200f, 300f, 300f) };

            Assert.IsTrue(WindGustRules.TryGetAxis(new Vector2(50f, 2f), bridge, area, Vector2.up, out Vector2 axis));
            Assert.AreEqual(0f, axis.x, 1e-4f);
            Assert.AreEqual(1f, Mathf.Abs(axis.y), 1e-4f);
            Assert.IsFalse(WindGustRules.TryGetAxis(new Vector2(50f, WindGustRules.BridgeHalfWidth + 1f), bridge, area, Vector2.up, out _));
            Assert.IsTrue(WindGustRules.TryGetAxis(new Vector2(250f, 250f), bridge, area, new Vector2(0f, 2f), out axis));
            Assert.AreEqual(Vector2.up, axis);
        }

        // ------------------------------------------------------------------
        // Rope baskets and currents
        // ------------------------------------------------------------------

        [Test]
        public void Basket_UpIsSlow_DownGlidesFastWithWings()
        {
            SkyRoute basket = Route(SkyRouteKind.RopeBasket);
            Assert.AreEqual(SkyRouteRules.BasketUpSpeed, SkyRouteRules.Speed(basket, true, true));
            Assert.AreEqual(SkyRouteRules.BasketDownSpeed, SkyRouteRules.Speed(basket, false, false));
            Assert.AreEqual(SkyRouteRules.BasketGlideDownSpeed, SkyRouteRules.Speed(basket, false, true));
            Assert.AreEqual(20f, SkyRouteRules.Duration(basket, true, false), 1e-3f);
            Assert.AreEqual(10f, SkyRouteRules.Duration(basket, false, true), 1e-3f);
        }

        [Test]
        public void Current_NeedsWings_RestsOneHourInThree_OnlyLeavingFromA()
        {
            SkyRoute current = Route(SkyRouteKind.Glide, 1);
            Assert.AreEqual(SkyRideCheck.NeedsWings, SkyRouteRules.Check(current, true, 3f, false, true));
            Assert.AreEqual(SkyRideCheck.Ok, SkyRouteRules.Check(current, true, 3f, true, true));
            Assert.AreEqual(SkyRideCheck.CurrentResting, SkyRouteRules.Check(current, true, 4.5f, true, true));
            Assert.AreEqual(SkyRideCheck.Ok, SkyRouteRules.Check(current, false, 4.5f, true, true), "the way back never closes");
            Assert.AreEqual(SkyRideCheck.ZoneBlocked, SkyRouteRules.Check(current, true, 3f, true, false));
            Assert.AreEqual(SkyRideCheck.Ok, SkyRouteRules.Check(Route(SkyRouteKind.RopeBasket), true, 4.5f, false, true), "baskets need neither wings nor the hour");
        }

        [Test]
        public void Ride_ArcTouchesBothGrounds()
        {
            Assert.AreEqual(2f, SkyRouteRules.HeightAt(2f, 8f, 10f, 0f), 1e-4f);
            Assert.AreEqual(8f, SkyRouteRules.HeightAt(2f, 8f, 10f, 1f), 1e-4f);
            Assert.AreEqual(15f, SkyRouteRules.HeightAt(2f, 8f, 10f, 0.5f), 1e-4f);
            Assert.AreEqual(new Vector2(45f, 0f), SkyRouteRules.PositionAt(Route(SkyRouteKind.Glide), true, 0.25f));
            Assert.AreEqual(new Vector2(135f, 0f), SkyRouteRules.PositionAt(Route(SkyRouteKind.Glide), false, 0.25f));
        }

        [Test]
        public void Stations_FindsLandingsInRange_NearestFirst()
        {
            var near = new SkyRoute { Id = "near", A = new Vector2(1f, 0f), B = new Vector2(90f, 0f) };
            var nearer = new SkyRoute { Id = "nearer", A = new Vector2(50f, 0f), B = new Vector2(0.5f, 0f) };
            var far = new SkyRoute { Id = "far", A = new Vector2(30f, 0f), B = new Vector2(60f, 0f) };
            var found = new List<(SkyRoute Route, bool ToB)>();

            SkyRouteRules.StationsAt(new[] { near, nearer, far }, Vector2.zero, found);

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual("nearer", found[0].Route.Id);
            Assert.IsFalse(found[0].ToB);
            Assert.AreEqual("near", found[1].Route.Id);
            Assert.IsTrue(found[1].ToB);
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_DeclaresBasketsAndCurrents_WithUniqueIds()
        {
            Assert.GreaterOrEqual(Baskets.Count(), 3);
            Assert.GreaterOrEqual(Currents.Count(), 3);
            Assert.AreEqual(OverworldLayouts.SkyRoutes.Length, OverworldLayouts.SkyRoutes.Select(r => r.Id).Distinct().Count());
            Assert.IsNotNull(World.Zone(OverworldLayouts.FarnkeshAnchorZoneId));
        }

        [Test]
        public void Layout_LandingsAreInTheRegion_OnLevelGround_ClearOfTowns()
        {
            foreach (SkyRoute route in OverworldLayouts.SkyRoutes)
            {
                foreach (Vector2 landing in new[] { route.A, route.B })
                {
                    Assert.IsTrue(OverworldLayouts.FarnkeshBounds.Contains(landing), route.Id);
                    Assert.IsNotNull(OverworldRules.ZoneAt(World, landing), route.Id);
                    float h = OverworldRules.SurfaceAt(World, landing);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 near = landing + new Vector2(Mathf.Cos(i * Mathf.PI / 4f), Mathf.Sin(i * Mathf.PI / 4f)) * 3f;
                        Assert.Less(Mathf.Abs(OverworldRules.SurfaceAt(World, near) - h), 1.5f, route.Id + " " + landing);
                    }
                }
            }
        }

        [Test]
        public void Layout_StationProps_StandOffTrailsWaterAndTowns_NextToTheirLandings()
        {
            foreach (SkyRoute route in OverworldLayouts.SkyRoutes)
            {
                foreach ((Vector2 prop, Vector2 landing) in new[] { (route.PropA, route.A), (route.PropB, route.B) })
                {
                    Assert.IsFalse(MapLayoutRules.OnPath(World.World, prop, 0f), route.Id + " on a trail");
                    Assert.IsFalse(MapLayoutRules.InRiver(World.World, prop, 0f), route.Id + " in the water");
                    Assert.IsNull(OverworldRules.SettlementAt(World, prop), route.Id + " in a safe zone");
                    Assert.LessOrEqual(Vector2.Distance(prop, landing), SkyRouteRules.StationRadius, route.Id);
                }
            }
        }

        [Test]
        public void Layout_CurrentsLand_OnHighIsles()
        {
            foreach (SkyRoute route in Currents)
            {
                Assert.Greater(OverworldRules.SurfaceAt(World, route.B), 10f, route.Id);
            }
        }

        [Test]
        public void Layout_EveryRoute_IsRideable_ByAHeroWhoMayEnterItsZones()
        {
            foreach (SkyRoute route in OverworldLayouts.SkyRoutes)
            {
                Assert.IsTrue(SkyRouteRules.AllZonesOpen(World, Vector2.zero, route, 200, CharacterId.Maithe, tag => true), route.Id);
            }
        }

        [Test]
        public void Layout_RouteIntoARequirementZone_IsBlockedWithoutTheGear()
        {
            SkyRoute gruta = OverworldLayouts.SkyRoutes.First(r => r.Id == "cesto_gruta");
            Assert.IsFalse(SkyRouteRules.AllZonesOpen(World, Vector2.zero, gruta, 200, CharacterId.Maithe, tag => false));
            Assert.IsFalse(SkyRouteRules.AllZonesOpen(World, Vector2.zero, gruta, 100, CharacterId.Maithe, tag => true));
        }

        [Test]
        public void Layout_GustsReachTheBridgesAndTheGorge_NotTheTowns()
        {
            Vector2 onBridge = Vector2.Lerp(OverworldLayouts.WindBridgeLines[0][1], OverworldLayouts.WindBridgeLines[0][2], 0.5f);
            Assert.IsTrue(WindGustRules.TryGetAxis(onBridge, OverworldLayouts.WindBridgeLines, OverworldLayouts.GustAreas, OverworldLayouts.GorgeWindAxis, out _));
            Assert.AreEqual(World.Zone("garganta_dos_ays").Bounds, OverworldLayouts.GustAreas[0]);
            Assert.IsFalse(WindGustRules.TryGetAxis(OverworldLayouts.FarnkeshOffset, OverworldLayouts.WindBridgeLines, OverworldLayouts.GustAreas, OverworldLayouts.GorgeWindAxis, out _));
            Assert.IsFalse(WindGustRules.TryGetAxis(OverworldLayouts.ValeSatirosOffset, OverworldLayouts.WindBridgeLines, OverworldLayouts.GustAreas, OverworldLayouts.GorgeWindAxis, out _));
        }

        [Test]
        public void Texts_HaveBothLanguages()
        {
            Assert.GreaterOrEqual(FarnkeshMechanicsTexts.Entries.Count, 8);
            foreach (var entry in FarnkeshMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }
        }
    }
}
