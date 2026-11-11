using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The mechanics of Regions 5–7: the tide of the reef (<see cref="CostaTideRules"/>, map 25), the wandering reed islands (<see cref="DeltaIslandRules"/>, map 33),
    /// the avalanches of the chutes (<see cref="TetoAvalancheRules"/>, map 37), the places the layouts declare for them, and the driver's one path of damage on a
    /// real PlayerStats.
    /// </summary>
    public class CostaDeltaTetoMechanicsTests
    {
        private const float HeroRunSpeed = 5f;

        private static OverworldLayout Costa => OverworldLayouts.Get(OverworldLayouts.CostaId);
        private static OverworldLayout Delta => OverworldLayouts.Get(OverworldLayouts.DeltaId);
        private static OverworldLayout Teto => OverworldLayouts.Get(OverworldLayouts.TetoId);

        // ------------------------------------------------------------------
        // Damage on a real PlayerStats
        // ------------------------------------------------------------------

        [TestCase(CostaTideRules.BiteFraction, CostaTideRules.BiteFloorFraction)]
        [TestCase(TetoAvalancheRules.HitFraction, TetoAvalancheRules.HitFloorFraction)]
        public void Driver_ApplyHazard_HurtsTheRealPlayerStats_AndNeverKills(float fraction, float floor)
        {
            var go = new GameObject("hero");
            try
            {
                PlayerStats stats = go.AddComponent<PlayerStats>();
                float max = stats.MaxHealth;
                float before = stats.CurrentHealth;
                Assert.Greater(CostaDeltaTetoMechanicsDriver.ApplyHazard(stats, fraction, floor), 0f);
                Assert.Less(stats.CurrentHealth, before, "the damage must really land on PlayerStats");
                for (int i = 0; i < 200; i++)
                {
                    stats.ClearInvulnerability();
                    CostaDeltaTetoMechanicsDriver.ApplyHazard(stats, fraction, floor);
                }

                Assert.IsTrue(stats.IsAlive);
                Assert.AreEqual(max * floor, stats.CurrentHealth, max * 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Driver_ApplyHazard_IgnoresNoHero()
        {
            Assert.AreEqual(0f, CostaDeltaTetoMechanicsDriver.ApplyHazard(null, 0.5f, 0.1f));
        }

        // ------------------------------------------------------------------
        // The tide
        // ------------------------------------------------------------------

        [TestCase(0L, CostaTidePhase.Low)]
        [TestCase(139L, CostaTidePhase.Low)]
        [TestCase(140L, CostaTidePhase.Turning)]
        [TestCase(179L, CostaTidePhase.Turning)]
        [TestCase(180L, CostaTidePhase.High)]
        [TestCase(479L, CostaTidePhase.High)]
        [TestCase(480L, CostaTidePhase.Low)]
        [TestCase(-1L, CostaTidePhase.High)]
        public void Tide_FollowsItsClock(long unix, CostaTidePhase expected)
        {
            Assert.AreEqual(expected, CostaTideRules.PhaseAt(unix));
        }

        [Test]
        public void Tide_LowLastsThreeMinutes_InACycleOfEight()
        {
            Assert.AreEqual(180L, CostaTideRules.LowSeconds + CostaTideRules.TurningSeconds);
            Assert.AreEqual(480L, CostaTideRules.CycleSeconds);
        }

        [Test]
        public void Tide_SecondsToChange_ReachTheNextPhase()
        {
            foreach (long unix in new[] { 0L, 100L, 150L, 300L })
            {
                long left = CostaTideRules.SecondsToChange(unix);
                Assert.Greater(left, 0L);
                Assert.AreEqual(CostaTideRules.PhaseAt(unix), CostaTideRules.PhaseAt(unix + left - 1L));
                Assert.AreNotEqual(CostaTideRules.PhaseAt(unix), CostaTideRules.PhaseAt(unix + left));
            }
        }

        [Test]
        public void Tide_WaterDrainsAndComesBack()
        {
            Assert.AreEqual(1f, CostaTideRules.WaterLevel(0L), 1e-4f);
            Assert.AreEqual(0f, CostaTideRules.WaterLevel(CostaTideRules.DrainSeconds), 1e-4f);
            Assert.AreEqual(0f, CostaTideRules.WaterLevel(CostaTideRules.LowSeconds), 1e-4f);
            Assert.AreEqual(1f, CostaTideRules.WaterLevel(CostaTideRules.LowSeconds + CostaTideRules.TurningSeconds), 1e-4f);
        }

        [Test]
        public void Tide_OnlyHighDrenches_AndAnAquaticMountIsFree()
        {
            Assert.IsTrue(CostaTideRules.Drenches(CostaTidePhase.High, false));
            Assert.IsFalse(CostaTideRules.Drenches(CostaTidePhase.High, true));
            Assert.IsFalse(CostaTideRules.Drenches(CostaTidePhase.Low, false));
            Assert.IsFalse(CostaTideRules.Drenches(CostaTidePhase.Turning, false), "the turning is only the warning");
        }

        [TestCase(1000f, 1000f, 30f)]
        [TestCase(310f, 1000f, 10f)]
        [TestCase(300f, 1000f, 0f)]
        [TestCase(100f, 1000f, 0f)]
        public void Tide_BiteStopsAtTheFloor(float current, float max, float expected)
        {
            Assert.AreEqual(expected, CostaTideRules.Bite(current, max), 1e-3f);
        }

        [Test]
        public void Tide_OnlySlows_NeverStops()
        {
            Assert.Greater(1f + CostaTideRules.HighTideMoveBonus, 0f);
        }

        [Test]
        public void Tide_TheTurningIsEnoughToWalkOffTheWidestFlat()
        {
            Assert.GreaterOrEqual(CostaTideRules.TurningSeconds * HeroRunSpeed, CostaTideRules.MaxFlatRadius);
        }

        [Test]
        public void Tide_AChestOpensOncePerTide_OnlyWhileDry()
        {
            Assert.IsTrue(CostaTideRules.CanOpenChest(4L, 5L, CostaTidePhase.Low));
            Assert.IsTrue(CostaTideRules.CanOpenChest(4L, 5L, CostaTidePhase.Turning));
            Assert.IsFalse(CostaTideRules.CanOpenChest(5L, 5L, CostaTidePhase.Low));
            Assert.IsFalse(CostaTideRules.CanOpenChest(4L, 5L, CostaTidePhase.High));
        }

        [Test]
        public void Tide_ChestItemWorksOnAnyTide()
        {
            Assert.Contains(CostaTideRules.ChestItem(3, -7L), CostaTideRules.ChestItems);
            Assert.Contains(CostaTideRules.ChestItem(0, 123456L), CostaTideRules.ChestItems);
        }

        [Test]
        public void Tide_FlatsAndChestsAreFoundByDistance()
        {
            var flats = new[] { new CostaTideFlat(new Vector2(10f, 0f), 5f) };
            Assert.AreEqual(0, CostaTideRules.FlatAt(flats, new Vector2(14f, 0f)));
            Assert.AreEqual(-1, CostaTideRules.FlatAt(flats, new Vector2(16f, 0f)));
            Assert.AreEqual(0, CostaTideRules.ChestAt(flats, new Vector2(10f + CostaTideRules.ChestReach, 0f)));
            Assert.AreEqual(-1, CostaTideRules.ChestAt(flats, new Vector2(10.1f + CostaTideRules.ChestReach, 0f)));
        }

        [Test]
        public void Layout_Tide_FlatsLieOnTheReefOfMap25_OffRoadsAndWater()
        {
            Assert.GreaterOrEqual(OverworldLayouts.TideFlats.Length, 5);
            foreach (CostaTideFlat flat in OverworldLayouts.TideFlats)
            {
                Assert.LessOrEqual(flat.Radius, CostaTideRules.MaxFlatRadius);
                Assert.AreEqual(OverworldLayouts.TaaPwaniMapId, OverworldRules.ZoneAt(Costa, flat.Center).MapIdOrOwner, flat.Center.ToString());
                Assert.IsNull(OverworldRules.SettlementAt(Costa, flat.Center), flat.Center.ToString());
                foreach (float angle in Enumerable.Range(0, 24).Select(i => i * 15f * Mathf.Deg2Rad))
                {
                    Vector2 rim = flat.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * flat.Radius;
                    Assert.IsFalse(MapLayoutRules.OnPath(Costa.World, rim, 0f), rim + " on a road: the tide must never close one");
                    Assert.IsFalse(MapLayoutRules.InRiver(Costa.World, rim, 0f), rim + " in the water");
                }

                Assert.IsFalse(MapLayoutRules.OnPath(Costa.World, flat.Center, flat.Radius), flat.Center + " a road runs through the flat");
                Assert.IsFalse(MapLayoutRules.InRiver(Costa.World, flat.Center, 2f), flat.Center + " the chest would be in the water");
            }
        }

        // ------------------------------------------------------------------
        // The wandering islands
        // ------------------------------------------------------------------

        private static Vector2[] Stations => OverworldLayouts.ReedIslandStations;

        [Test]
        public void Islands_ChangeStationEveryFifteenMinutes()
        {
            Assert.AreEqual(900d, DeltaIslandRules.LayoutSeconds, 1e-9);
            Assert.AreEqual(0L, DeltaIslandRules.LayoutIndex(899d));
            Assert.AreEqual(1L, DeltaIslandRules.LayoutIndex(900d));
            Assert.AreEqual(-1L, DeltaIslandRules.LayoutIndex(-1d));
        }

        [Test]
        public void Islands_HoldDistinctStations_InEveryLayout()
        {
            for (long layout = -9L; layout < 9L; layout++)
            {
                var seen = Enumerable.Range(0, DeltaIslandRules.Islands).Select(i => DeltaIslandRules.StationOf(i, layout)).ToArray();
                Assert.AreEqual(DeltaIslandRules.Islands, seen.Distinct().Count(), layout.ToString());
                Assert.IsTrue(seen.All(s => s >= 0 && s < DeltaIslandRules.Stations), layout.ToString());
            }
        }

        [Test]
        public void Islands_ReachTheirStation_AfterTheDrift()
        {
            double t = DeltaIslandRules.LayoutSeconds * 3d + DeltaIslandRules.DriftSeconds + 10d;
            Vector2 at = DeltaIslandRules.Position(Stations, 1, t);
            Assert.LessOrEqual(Vector2.Distance(at, Stations[DeltaIslandRules.StationOf(1, 3L)]), DeltaIslandRules.BobRadius + 0.01f);
            Assert.IsFalse(DeltaIslandRules.IsDrifting(t));
            Assert.IsTrue(DeltaIslandRules.IsDrifting(DeltaIslandRules.LayoutSeconds * 3d + 1d));
        }

        [Test]
        public void Islands_NeverJump_AndDriftBelowWalkingSpeed()
        {
            float worst = 0f;
            for (double t = 800d; t < 2000d; t += 1d)
            {
                worst = Mathf.Max(worst, Vector2.Distance(DeltaIslandRules.Position(Stations, 0, t), DeltaIslandRules.Position(Stations, 0, t + 1d)));
            }

            Assert.Less(worst, HeroRunSpeed * 0.5f, "an island can always be stepped off");
        }

        [Test]
        public void Islands_NeverJump_AtRealUnixSeconds()
        {
            double start = DeltaIslandRules.LayoutSeconds * 2000000d + DeltaIslandRules.DriftSeconds + 10d;
            float worst = 0f;
            for (int second = 0; second < 300; second++)
            {
                worst = Mathf.Max(worst, Vector2.Distance(DeltaIslandRules.Position(Stations, 0, start + second), DeltaIslandRules.Position(Stations, 0, start + second + 1d)));
            }

            Assert.Less(worst, 0.5f, "holding at its station, an island only bobs");
        }

        [Test]
        public void Push_PressesAHeroOnTheGround_ButNotOneInTheAir()
        {
            Assert.Less(GroundedPush.Motion(new Vector2(0.006f, 0f), true).y, 0f, "the slowest carry still reaches the ground");
            Assert.AreEqual(-0.1f, GroundedPush.Motion(new Vector2(0.1f, 0f), true).y, 1e-5f, "follows a 45 degree downhill");
            Assert.AreEqual(0f, GroundedPush.Motion(new Vector2(0.1f, 0f), false).y, "never shortens a jump");
        }

        [Test]
        public void Mechanics_NeverMoveTheHeroWithoutGroundedPush()
        {
            string folder = System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/World/Map/Mechanics");
            var offenders = System.IO.Directory.GetFiles(folder, "*.cs")
                .Where(f => System.Text.RegularExpressions.Regex.IsMatch(System.IO.File.ReadAllText(f), @"_character\.Move\("))
                .Select(System.IO.Path.GetFileName).ToArray();
            CollectionAssert.IsEmpty(offenders, "a flat Move leaves isGrounded false: use GroundedPush.Shove");
        }

        [Test]
        public void Islands_CarryTheHeroByTheirOwnMovement()
        {
            double t = DeltaIslandRules.LayoutSeconds * 2d + 60d;
            Vector2 carry = DeltaIslandRules.Carry(Stations, 2, t, 0.5f);
            Vector2 expected = DeltaIslandRules.Position(Stations, 2, t) - DeltaIslandRules.Position(Stations, 2, t - 0.5d);
            Assert.AreEqual(expected.x, carry.x, 1e-4f);
            Assert.AreEqual(expected.y, carry.y, 1e-4f);
            Assert.Greater(carry.magnitude, 0f);
        }

        [Test]
        public void Islands_AreFoundWhereTheyAre()
        {
            const double t = 5000d;
            Vector2 island = DeltaIslandRules.Position(Stations, 3, t);
            Assert.AreEqual(3, DeltaIslandRules.IslandAt(Stations, t, island));
            Assert.AreEqual(-1, DeltaIslandRules.IslandAt(Stations, t, OverworldLayouts.ReedBogCenter));
        }

        [Test]
        public void Bog_SlowsOffTheIslands_AndNotWithABoat()
        {
            Vector2 center = OverworldLayouts.ReedBogCenter;
            float radius = OverworldLayouts.ReedBogRadius;
            Assert.IsTrue(DeltaIslandRules.InBog(center, radius, center, -1));
            Assert.IsFalse(DeltaIslandRules.InBog(center, radius, center, 0), "an island is firm ground");
            Assert.IsFalse(DeltaIslandRules.InBog(center, radius, center + Vector2.right * (radius + 1f), -1));
            Assert.IsTrue(DeltaIslandRules.BogSlows(true, false, false));
            Assert.IsFalse(DeltaIslandRules.BogSlows(true, true, false));
            Assert.IsFalse(DeltaIslandRules.BogSlows(true, false, true));
            Assert.Greater(1f + DeltaIslandRules.BogMoveBonus, 0f, "it only slows");
        }

        [Test]
        public void Spark_MovesBetweenIslands_AndIsFreedOncePerLayout()
        {
            Assert.AreEqual(0, DeltaIslandRules.SparkIsland(0L));
            Assert.AreEqual(1, DeltaIslandRules.SparkIsland(5L));
            Assert.AreEqual(3, DeltaIslandRules.SparkIsland(-1L));
            Assert.IsTrue(DeltaIslandRules.CanFreeSpark(4L, 5L));
            Assert.IsFalse(DeltaIslandRules.CanFreeSpark(5L, 5L));
            Assert.Contains(DeltaIslandRules.SparkItem(-4L), DeltaIslandRules.SparkItems);
        }

        [Test]
        public void Layout_Islands_StationsAreEightOnTheRing_InsideTheBog()
        {
            Assert.AreEqual(DeltaIslandRules.Stations, Stations.Length);
            Assert.AreEqual(Stations.Length, Stations.Distinct().Count());
            foreach (Vector2 station in Stations)
            {
                Assert.LessOrEqual(Vector2.Distance(station, OverworldLayouts.ReedBogCenter) + DeltaIslandRules.IslandRadius + DeltaIslandRules.BobRadius, OverworldLayouts.ReedBogRadius + 0.1f);
            }
        }

        [Test]
        public void Layout_Bog_LiesInTheWanderingIslands_OffRoadsWaterAndTowns()
        {
            Vector2 center = OverworldLayouts.ReedBogCenter;
            float radius = OverworldLayouts.ReedBogRadius;
            Assert.AreEqual(OverworldLayouts.TarchonMapId, OverworldRules.ZoneAt(Delta, center).MapIdOrOwner);
            for (float x = -radius; x <= radius; x += 10f)
            {
                for (float z = -radius; z <= radius; z += 10f)
                {
                    var offset = new Vector2(x, z);
                    if (offset.magnitude > radius)
                    {
                        continue;
                    }

                    Vector2 p = center + offset;
                    Assert.AreEqual(OverworldLayouts.TarchonMapId, OverworldRules.ZoneAt(Delta, p).MapIdOrOwner, p.ToString());
                    Assert.IsFalse(MapLayoutRules.OnPath(Delta.World, p, 0f), p + " on a road");
                    Assert.IsFalse(MapLayoutRules.InRiver(Delta.World, p, 0f), p + " in the water: an island would carry the hero into it");
                    Assert.IsNull(OverworldRules.SettlementAt(Delta, p), p.ToString());
                }
            }
        }

        // ------------------------------------------------------------------
        // The avalanches
        // ------------------------------------------------------------------

        private static readonly TetoChuteLane OneLane = new TetoChuteLane(new Vector2(0f, 100f), new Vector2(0f, 0f), 18f);

        [TestCase(0d, TetoAvalanchePhase.Quiet)]
        [TestCase(59.9d, TetoAvalanchePhase.Quiet)]
        [TestCase(60d, TetoAvalanchePhase.Warning)]
        [TestCase(79.9d, TetoAvalanchePhase.Warning)]
        [TestCase(80d, TetoAvalanchePhase.Sliding)]
        [TestCase(86.9d, TetoAvalanchePhase.Sliding)]
        [TestCase(87d, TetoAvalanchePhase.Quiet)]
        [TestCase(90d, TetoAvalanchePhase.Quiet)]
        public void Avalanche_FollowsItsClock(double t, TetoAvalanchePhase expected)
        {
            Assert.AreEqual(expected, TetoAvalancheRules.PhaseAt(t));
        }

        [Test]
        public void Avalanche_LanesTakeTurns()
        {
            Assert.AreEqual(0, TetoAvalancheRules.LaneAt(0d));
            Assert.AreEqual(1, TetoAvalancheRules.LaneAt(TetoAvalancheRules.CycleSeconds));
            Assert.AreEqual(0, TetoAvalancheRules.LaneAt(TetoAvalancheRules.CycleSeconds * TetoAvalancheRules.LaneCount));
            Assert.AreEqual(3, TetoAvalancheRules.LaneAt(-1d));
        }

        [Test]
        public void Avalanche_TheWarningIsEnoughToWalkOutOfAnyLane()
        {
            double warning = TetoAvalancheRules.SlidingStart - TetoAvalancheRules.WarningStart;
            foreach (TetoChuteLane lane in OverworldLayouts.ChuteLanes)
            {
                Assert.GreaterOrEqual(warning * HeroRunSpeed, lane.Width * 0.5f);
            }
        }

        [Test]
        public void Avalanche_FrontRunsFromTopToFoot()
        {
            Assert.AreEqual(0f, TetoAvalancheRules.Progress(TetoAvalancheRules.SlidingStart), 1e-4f);
            Assert.AreEqual(0.5f, TetoAvalancheRules.Progress((TetoAvalancheRules.SlidingStart + TetoAvalancheRules.SlidingEnd) * 0.5d), 1e-3f);
            Assert.AreEqual(0f, TetoAvalancheRules.Progress(10d), 1e-4f, "no front outside the slide");
        }

        [Test]
        public void Avalanche_SweepsOnlyWhereTheFrontPasses_InsideTheLane()
        {
            Assert.IsTrue(TetoAvalancheRules.SweptBy(OneLane, new Vector2(5f, 50f), 0.4f, 0.55f));
            Assert.IsFalse(TetoAvalancheRules.SweptBy(OneLane, new Vector2(5f, 50f), 0.7f, 0.8f), "the front has gone past");
            Assert.IsFalse(TetoAvalancheRules.SweptBy(OneLane, new Vector2(5f, 50f), 0.1f, 0.2f), "the front has not come yet");
            Assert.IsFalse(TetoAvalancheRules.SweptBy(OneLane, new Vector2(9.5f, 50f), 0.4f, 0.55f), "one step out of the lane is safe");
        }

        [Test]
        public void Avalanche_NoFastFrontJumpsOverTheHero()
        {
            const float step = 1f / 60f / 7f * 60f; // a whole second of slide in one frame
            bool hit = false;
            float from = 0f;
            for (float to = step; to <= 1f + 1e-4f; to += step)
            {
                hit |= TetoAvalancheRules.SweptBy(OneLane, new Vector2(0f, 37f), from, to);
                from = to;
            }

            Assert.IsTrue(hit);
        }

        [Test]
        public void Avalanche_HitsOnlyTheWarned()
        {
            Assert.IsTrue(TetoAvalancheRules.CanHit(5L, 5L));
            Assert.IsFalse(TetoAvalancheRules.CanHit(4L, 5L), "no blow without a warning");
        }

        [TestCase(1000f, 1000f, 220f)]
        [TestCase(300f, 1000f, 100f)]
        [TestCase(200f, 1000f, 0f)]
        public void Avalanche_HitStopsAtTheFloor(float current, float max, float expected)
        {
            Assert.AreEqual(expected, TetoAvalancheRules.Hit(current, max), 1e-3f);
        }

        [Test]
        public void Layout_Avalanche_FourLanes_InMap37_ClearOfTownsAndBridges()
        {
            Assert.AreEqual(TetoAvalancheRules.LaneCount, OverworldLayouts.ChuteLanes.Length);
            foreach (TetoChuteLane lane in OverworldLayouts.ChuteLanes)
            {
                for (float f = 0f; f <= 1f; f += 0.05f)
                {
                    Vector2 p = Vector2.Lerp(lane.Top, lane.Bottom, f);
                    Assert.AreEqual(OverworldLayouts.CalhasId, OverworldRules.ZoneAt(Teto, p).MapIdOrOwner, p.ToString());
                    Assert.IsNull(OverworldRules.SettlementAt(Teto, p), p.ToString());
                    Assert.IsFalse(Teto.World.Rivers.Any(river => MapLayoutRules.OnCrossing(river, p, lane.Width * 0.5f)), p + " over a bridge: the only way across must stay clear");
                }
            }
        }

        // ------------------------------------------------------------------
        // Anchors and texts
        // ------------------------------------------------------------------

        [Test]
        public void Layout_Anchors_AndBoundsAreTheRegions()
        {
            Assert.IsNotNull(Costa.Zone(OverworldLayouts.CostaAnchorZoneId));
            Assert.IsNotNull(Delta.Zone(OverworldLayouts.DeltaAnchorZoneId));
            Assert.IsNotNull(Teto.Zone(OverworldLayouts.TetoAnchorZoneId));
            Assert.AreEqual(Costa.Bounds, OverworldLayouts.CostaBounds);
            Assert.AreEqual(Delta.Bounds, OverworldLayouts.DeltaBounds);
            Assert.AreEqual(Teto.Bounds, OverworldLayouts.TetoBounds);
        }

        [Test]
        public void Layout_InTheStitchedWorld_TheAnchorsSayWhereTheRegionsSit()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.WorldId);
            OverworldZone costa = world.Zone(OverworldLayouts.CostaAnchorZoneId);
            OverworldZone delta = world.Zone(OverworldLayouts.DeltaAnchorZoneId);
            OverworldZone teto = world.Zone(OverworldLayouts.TetoAnchorZoneId);
            Assert.AreEqual(OverworldLayouts.CostaWorldOffset, costa.Origin);
            Assert.AreEqual(OverworldLayouts.DeltaWorldOffset, delta.Origin);
            Assert.AreEqual(OverworldLayouts.TetoWorldOffset, teto.Origin);
            Assert.AreEqual(OverworldLayouts.CostaId, world.PartAt(costa.Origin + OverworldLayouts.TideFlats[0].Center).Id);
            Assert.AreEqual(OverworldLayouts.DeltaId, world.PartAt(delta.Origin + OverworldLayouts.ReedBogCenter).Id);
            Assert.AreEqual(OverworldLayouts.TetoId, world.PartAt(teto.Origin + OverworldLayouts.ChuteLanes[0].Top).Id);
        }

        [Test]
        public void Layout_OtherRegions_HaveNoAnchor_SoTheDriverStaysOff()
        {
            OverworldLayout other = OverworldLayouts.Get(OverworldLayouts.MisturaRegionId);
            Assert.IsNull(other.Zone(OverworldLayouts.CostaAnchorZoneId));
            Assert.IsNull(other.Zone(OverworldLayouts.DeltaAnchorZoneId));
            Assert.IsNull(other.Zone(OverworldLayouts.TetoAnchorZoneId));
        }

        [Test]
        public void Texts_HaveBothLanguages()
        {
            Assert.GreaterOrEqual(CostaDeltaTetoMechanicsTexts.Entries.Count, 14);
            foreach (var entry in CostaDeltaTetoMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }
        }
    }
}
