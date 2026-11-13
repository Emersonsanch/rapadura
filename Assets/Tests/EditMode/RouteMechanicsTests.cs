using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The route mechanics of the story road (Docs/Mapas100.md 07, 31, 36, 41, 43; <see cref="RouteMechanicsDriver"/>): Ammo's caravan through the
    /// sandstorm (<see cref="CaravanRoadEscortRules"/>), the monsoon fords and boats of Jhilpur (<see cref="JhilpurMonsoonRules"/>), the lifts and
    /// chutes of Sangkent (<see cref="SangkentLiftRules"/>), the drum's barges (<see cref="DrumBargeRules"/>) and the locks of the Canal das
    /// Lanternas (<see cref="CanalLockRules"/>): pure rules, the places the layouts declare for them (stations on the roads, dry, courses on the
    /// water they sail) and their texts.
    /// </summary>
    public class RouteMechanicsTests
    {
        /// <summary>The hero's walking speed (m/s) used for "faster than walking".</summary>
        private const float WalkSpeed = 5.5f;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // Estrada das Caravanas (07)
        // ------------------------------------------------------------------

        [Test]
        public void Caravan_SetsOutOnlyWhenTheSandIsComing_AndAlwaysMeetsTheStorm()
        {
            double cycle = AreiasStormRules.CycleSeconds;
            double start = 1_800_000_000d - 1_800_000_000d % cycle;
            Assert.IsFalse(CaravanRoadEscortRules.CanDepart(start + 10d), "just after a storm: the sand is far");
            Assert.Greater(CaravanRoadEscortRules.SecondsToDeparture(start + 10d), 0f);
            double lead = start + AreiasStormRules.WarningStart - CaravanRoadEscortRules.DepartLead;
            Assert.IsFalse(CaravanRoadEscortRules.CanDepart(lead - 1d));
            Assert.IsTrue(CaravanRoadEscortRules.CanDepart(lead + 1d));
            Assert.AreEqual(0f, CaravanRoadEscortRules.SecondsToDeparture(lead + 1d));
            Assert.IsTrue(CaravanRoadEscortRules.CanDepart(start + AreiasStormRules.WarningStart + 20d), "in the storm itself");
            Assert.AreEqual(0f, CaravanRoadEscortRules.SecondsToWarning(start + AreiasStormRules.WarningStart + 5d));

            float walk = RidePath.Length(OverworldLayouts.AreiasCaravanRoad) / CaravanRoadEscortRules.Speed;
            Assert.Greater(walk, CaravanRoadEscortRules.DepartLead + AreiasStormRules.WarningSeconds, "set out at the earliest, the storm still meets it");
            Assert.Less(walk, 420f, "a walk, not an evening");
            Assert.LessOrEqual(cycle - CaravanRoadEscortRules.DepartLead - AreiasStormRules.WarningSeconds - AreiasStormRules.StormSeconds, 300d, "never more than 5 min to wait");
        }

        [Test]
        public void Caravan_InTheStormWalksOnlyBesideTheHero_AndShelters()
        {
            Assert.AreEqual(CaravanRoadState.Walking, CaravanRoadEscortRules.StateFor(AreiasStormPhase.Calm, 20f));
            Assert.AreEqual(CaravanRoadState.Waiting, CaravanRoadEscortRules.StateFor(AreiasStormPhase.Calm, 30f));
            Assert.AreEqual(CaravanRoadState.Walking, CaravanRoadEscortRules.StateFor(AreiasStormPhase.Warning, 20f));
            Assert.AreEqual(CaravanRoadState.Kneeling, CaravanRoadEscortRules.StateFor(AreiasStormPhase.Storm, 10f));
            Assert.AreEqual(CaravanRoadState.Walking, CaravanRoadEscortRules.StateFor(AreiasStormPhase.Storm, 5f));
            Assert.IsTrue(CaravanRoadEscortRules.Shelters(AreiasStormPhase.Storm, 6.5f));
            Assert.IsFalse(CaravanRoadEscortRules.Shelters(AreiasStormPhase.Storm, 8f));
            Assert.IsFalse(CaravanRoadEscortRules.Shelters(AreiasStormPhase.Calm, 1f), "no shelter needed in the calm");
            Assert.GreaterOrEqual(CaravanRoadEscortRules.ShelterRange, CaravanRoadEscortRules.StormBesideRange, "walking beside them is always sheltered");
            Assert.IsTrue(CaravanRoadEscortRules.SandStarted(AreiasStormPhase.Warning, AreiasStormPhase.Storm));
            Assert.IsFalse(CaravanRoadEscortRules.SandStarted(AreiasStormPhase.Storm, AreiasStormPhase.Storm));
            Assert.IsTrue(CaravanRoadEscortRules.Lost(121f));
            Assert.IsFalse(CaravanRoadEscortRules.Lost(60f));
            Assert.IsTrue(CaravanRoadEscortRules.Paid(20000L, 20000L));
            Assert.IsFalse(CaravanRoadEscortRules.Paid(19999L, 20000L));
        }

        [Test]
        public void Caravan_RoadRunsOnTheCaravanRoad_FromItsCampToQocho()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.AreiasId);
            Assert.IsNotNull(world);
            Vector2[] road = OverworldLayouts.AreiasCaravanRoad;
            Assert.AreEqual("estrada_das_caravanas", OverworldRules.ZoneAt(world, road[0])?.Id, "the camp is on the 07's own road");
            Assert.AreEqual(MapLayouts.CaravanasId, OverworldRules.ZoneAt(world, road[road.Length - 1])?.MapIdOrOwner, "it arrives in 07's map (Qocho's approach)");
            float length = RidePath.Length(road);
            for (float d = 0f; d <= length; d += 2f)
            {
                Vector2 p = RidePath.PointAt(road, d);
                bool inPost = OverworldRules.SettlementAt(world, p) != null;
                Assert.IsTrue(inPost || MapLayoutRules.OnPath(world.World, p, 0.5f), $"{p}: on the caravan road (or the Caravançará's street)");
                Assert.IsFalse(MapLayoutRules.InRiver(world.World, p, 1f), $"{p}: dry");
            }

            Assert.IsTrue(RouteMechanicsDriver.CaravanBounds.Contains(road[0]) && RouteMechanicsDriver.CaravanBounds.Contains(road[road.Length - 1]));
            AssertStation(world, road[0], "the camp");
        }

        // ------------------------------------------------------------------
        // Monção de Jhilpur (31)
        // ------------------------------------------------------------------

        [Test]
        public void Monsoon_TwoDaysInSix_WithTheCountdown()
        {
            int[] days = Enumerable.Range(1, 12).Where(JhilpurMonsoonRules.IsMonsoon).ToArray();
            CollectionAssert.AreEqual(new[] { 5, 6, 11, 12 }, days);
            Assert.IsFalse(JhilpurMonsoonRules.IsMonsoon(0));
            Assert.AreEqual(4, JhilpurMonsoonRules.DaysUntilMonsoon(1));
            Assert.AreEqual(0, JhilpurMonsoonRules.DaysUntilMonsoon(6));
            Assert.AreEqual(4, JhilpurMonsoonRules.DaysUntilMonsoon(7));
            Assert.IsFalse(JhilpurMonsoonRules.Learned(0L));
            Assert.IsTrue(JhilpurMonsoonRules.Learned(1_800_000_000L));
            Assert.IsFalse(JhilpurMonsoonRules.LessonDone(JhilpurMonsoonRules.LessonSeconds - 0.1f));
            Assert.IsTrue(JhilpurMonsoonRules.LessonDone(JhilpurMonsoonRules.LessonSeconds));
        }

        [Test]
        public void Monsoon_FloodedFordPushesBackToTheBankYouCameFrom_FasterThanAWalk()
        {
            var ford = new DeltaFord("t", Vector2.zero, Vector2.up, 13f, 9f, true, new Vector2(-22f, 0f), new Vector2(22f, 0f));
            Assert.IsTrue(JhilpurMonsoonRules.InFordWater(ford, new Vector2(5f, 3f)));
            Assert.IsFalse(JhilpurMonsoonRules.InFordWater(ford, new Vector2(5f, 12f)), "past the ford along the river");
            Assert.IsFalse(JhilpurMonsoonRules.InFordWater(ford, new Vector2(14f, 0f)), "on the bank");
            Assert.AreEqual(1, JhilpurMonsoonRules.SideOf(ford, new Vector2(20f, 0f)));
            Assert.AreEqual(-1, JhilpurMonsoonRules.SideOf(ford, new Vector2(-20f, 0f)));
            Vector2 step = JhilpurMonsoonRules.PushStep(ford, -1, 1f);
            Assert.Less(step.x, 0f, "back west");
            Assert.Greater(step.magnitude, WalkSpeed, "the current beats a walk");
            Assert.AreEqual(0, JhilpurMonsoonRules.MooringAt(new[] { ford }, new Vector2(-20f, 1f), out bool atA));
            Assert.IsTrue(atA);
            Assert.AreEqual(-1, JhilpurMonsoonRules.MooringAt(new[] { ford }, new Vector2(0f, 30f), out _));

            Assert.AreEqual(MooringCheck.Shallow, JhilpurMonsoonRules.CheckMooring(false, true, true));
            Assert.AreEqual(MooringCheck.Row, JhilpurMonsoonRules.CheckMooring(true, true, true));
            Assert.AreEqual(MooringCheck.Ferry, JhilpurMonsoonRules.CheckMooring(true, false, true), "nobody is stranded: the ferryman takes the non-rower");
            Assert.AreEqual(MooringCheck.ZoneBlocked, JhilpurMonsoonRules.CheckMooring(true, true, false));
            Assert.Less(JhilpurMonsoonRules.FerrySpeed, JhilpurMonsoonRules.RowSpeed, "rowing is the point of the lesson");
        }

        [Test]
        public void Monsoon_FordsAreTheRegionsFords_MooringsOnTheirRoads_TheRunOnTheCanal()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.DeltaId);
            Assert.IsNotNull(world);
            DeltaFord[] fords = OverworldLayouts.DeltaFords;
            Assert.AreEqual(6, fords.Length);
            Assert.AreEqual(4, fords.Count(f => f.HasMoorings));
            List<MapCrossing> crossings = world.World.Rivers.SelectMany(r => r.Crossings).ToList();
            foreach (DeltaFord ford in fords)
            {
                MapCrossing match = crossings.FirstOrDefault(c => Vector2.Distance(c.At, ford.Center) < 0.5f);
                Assert.IsNotNull(match, ford.Id + ": one of the region's crossings");
                Assert.IsFalse(match.IsBridge, ford.Id + ": a ford, not a bridge");
                Assert.IsTrue(MapLayoutRules.InRiver(world.World, ford.Center, 0f), ford.Id + ": in the water");
                Assert.Greater(ford.HalfLength, match.Width * 0.5f, ford.Id + ": the flood covers the whole ford");
                if (!ford.HasMoorings)
                {
                    continue;
                }

                foreach (Vector2 m in new[] { ford.MooringA, ford.MooringB })
                {
                    Assert.IsTrue(MapLayoutRules.OnPath(world.World, m, 0f), $"{ford.Id} {m}: on the road");
                    Assert.IsFalse(MapLayoutRules.InRiver(world.World, m, 6f), $"{ford.Id} {m}: well off the water");
                    Assert.IsFalse(JhilpurMonsoonRules.InFordWater(ford, m), $"{ford.Id} {m}: out of the flood");
                    AssertStation(world, m, ford.Id);
                }

                Assert.AreNotEqual(JhilpurMonsoonRules.SideOf(ford, ford.MooringA), JhilpurMonsoonRules.SideOf(ford, ford.MooringB), ford.Id + ": one on each bank");
            }

            Vector2 school = OverworldLayouts.JhilpurBoatSchool;
            Assert.IsTrue(MapLayoutRules.OnPath(world.World, school, 0f), "the school is on the east road");
            Assert.IsFalse(MapLayoutRules.InRiver(world.World, school, 6f));
            Assert.IsNull(OverworldRules.SettlementAt(world, school), "out of the town's ground");
            Assert.AreEqual(-1, JhilpurMonsoonRules.MooringAt(fords, school, out _), "its own station, not a ford's");
            Assert.Less(school.magnitude, 150f, "by Jhilpur");
            AssertStation(world, school, "the school");

            Vector2 landing = OverworldLayouts.TarchonRunLanding;
            Assert.IsTrue(MapLayoutRules.OnPath(world.World, landing, 0f), "the run lands on Tarchon's west road");
            Assert.IsFalse(MapLayoutRules.InRiver(world.World, landing, 6f));
            Assert.Greater(Vector2.Distance(landing, fords[0].MooringB), JhilpurMonsoonRules.MooringReach * 2f, "its own station, not the mooring's");
            AssertStation(world, landing, "the run's landing");

            Vector2[] run = OverworldLayouts.JhilpurMonsoonRun;
            for (int i = 1; i < run.Length - 3; i++)
            {
                Assert.IsTrue(MapLayoutRules.InRiver(world.World, run[i], 0f), $"run point {i} {run[i]}: on the canal");
            }

            float runSeconds = RidePath.Length(run) / JhilpurMonsoonRules.RunDownSpeed;
            float roadSeconds = 1800f / WalkSpeed;
            Assert.Less(runSeconds, roadSeconds * 0.5f, "the high water halves the way to Tarchon");
        }

        // ------------------------------------------------------------------
        // Os 7 terraços (36)
        // ------------------------------------------------------------------

        [Test]
        public void Terraces_OneBasketEvery16s_QueueLeavesOnTheRope_BellStopsThem()
        {
            Assert.AreEqual(SangkentLiftRules.BasketSeconds, SangkentLiftRules.SecondsToBasket(1600d), 1e-4f);
            Assert.AreEqual(6f, SangkentLiftRules.SecondsToBasket(1610d), 1e-4f);
            Assert.IsFalse(SangkentLiftRules.BasketLeft(1610d, 1615d));
            Assert.IsTrue(SangkentLiftRules.BasketLeft(1610d, 1616d));
            Assert.Greater(SangkentLiftRules.ChuteSpeedFor(true), SangkentLiftRules.ChuteSpeedFor(false), "frozen at night");

            var lift = new SangkentTerraceLine("l", true, Vector2.zero, Vector2.up);
            var chute = new SangkentTerraceLine("c", false, Vector2.up, Vector2.zero);
            Assert.AreEqual(TerraceCheck.Queue, SangkentLiftRules.Check(lift, false, true));
            Assert.AreEqual(TerraceCheck.Slide, SangkentLiftRules.Check(chute, false, true));
            Assert.AreEqual(TerraceCheck.Avalanche, SangkentLiftRules.Check(chute, true, true));
            Assert.AreEqual(TerraceCheck.ZoneBlocked, SangkentLiftRules.Check(lift, false, false));

            DateTime utc = new DateTime(2026, 11, 13, 12, 0, 0, DateTimeKind.Utc);
            for (int minute = 0; minute < 60; minute++)
            {
                DateTime t = utc.AddMinutes(minute);
                var window = SangkentAvalancheRules.WindowAt(t);
                Assert.AreEqual(window.IsOpen || SangkentAvalancheRules.IsWarning(window), SangkentLiftRules.Stopped(t), "minute " + minute);
            }
        }

        [Test]
        public void Terraces_StationsOnTheRoads_FasterThanTheRoad_ClearOfTheSnowLanes()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.TetoId);
            Assert.IsNotNull(world);
            SangkentTerraceLine[] lines = OverworldLayouts.SangkentTerraceLines;
            Assert.AreEqual(2, lines.Count(l => l.IsLift));
            Assert.AreEqual(2, lines.Count(l => !l.IsLift));
            for (int i = 0; i < lines.Length; i++)
            {
                SangkentTerraceLine line = lines[i];
                Assert.AreEqual(i, SangkentLiftRules.StationAt(lines, line.From), line.Id + ": one station, one button");
                foreach (Vector2 p in new[] { line.From, line.To })
                {
                    Assert.IsTrue(MapLayoutRules.OnPath(world.World, p, 0f), $"{line.Id} {p}: on the road");
                    Assert.IsFalse(MapLayoutRules.InRiver(world.World, p, 3f), $"{line.Id} {p}: dry");
                    Assert.IsNull(OverworldRules.SettlementAt(world, p), $"{line.Id} {p}: out of the town");
                    Assert.IsTrue(RouteMechanicsDriver.TerraceBounds.Contains(p), $"{line.Id} {p}: inside the part's bounds");
                    AssertStation(world, p, line.Id);
                }

                foreach (SangkentTerraceLane lane in OverworldLayouts.SangkentTerraceLanes)
                {
                    float gap = SegmentGap(line.From, line.To, lane.Lane.Top, lane.Lane.Bottom);
                    Assert.Greater(gap, lane.Lane.Width * 0.5f, line.Id + ": clear of the avalanche's snow lanes");
                }
            }

            Assert.AreEqual(OverworldLayouts.SangkentMapId, OverworldRules.ZoneAt(world, OverworldLayouts.SangkentTerraceFoot)?.MapIdOrOwner);
            Assert.AreEqual("terracos_altos", OverworldRules.ZoneAt(world, OverworldLayouts.SangkentTerraceTop)?.Id, "the 7th terrace is the Terraços Altos");
            float chute = Vector2.Distance(OverworldLayouts.SangkentTerraceTop, OverworldLayouts.SangkentTerraceFoot) / SangkentLiftRules.ChuteSpeed;
            float roadDown = (Vector2.Distance(OverworldLayouts.SangkentTerraceTop, OverworldLayouts.SangkentTerraceMiddle)
                + Vector2.Distance(OverworldLayouts.SangkentTerraceMiddle, new Vector2(149f, 21f)) + Vector2.Distance(new Vector2(149f, 21f), OverworldLayouts.SangkentTerraceFoot)) / WalkSpeed;
            Assert.Less(chute, roadDown * 0.5f, "the chute is the quick way down");
            float liftUp = SangkentLiftRules.BasketSeconds + Vector2.Distance(OverworldLayouts.SangkentTerraceFoot, OverworldLayouts.SangkentTerraceMiddle) / SangkentLiftRules.LiftSpeed;
            float roadUp = (Vector2.Distance(OverworldLayouts.SangkentTerraceFoot, new Vector2(149f, 21f)) + Vector2.Distance(new Vector2(149f, 21f), OverworldLayouts.SangkentTerraceMiddle)) / WalkSpeed;
            Assert.Less(liftUp, roadUp, "even after the longest wait the lift beats the winding road");
        }

        // ------------------------------------------------------------------
        // Barcas do Tambor (41) and the locks (43)
        // ------------------------------------------------------------------

        [Test]
        public void Barges_OnTheDrumsHours_NoneAfterTheBell()
        {
            Assert.IsTrue(DrumBargeRules.IsDrumHour(6f));
            Assert.IsTrue(DrumBargeRules.IsDrumHour(19.9f));
            Assert.IsFalse(DrumBargeRules.IsDrumHour(20f));
            Assert.IsFalse(DrumBargeRules.IsDrumHour(5.9f));
            Assert.AreEqual(8, DrumBargeRules.BoardingDeparture(BargeLine.North, 7.7f));
            Assert.AreEqual(-1, DrumBargeRules.BoardingDeparture(BargeLine.North, 7.5f));
            Assert.AreEqual(-1, DrumBargeRules.BoardingDeparture(BargeLine.North, 8f), "gone on the hour");
            Assert.AreEqual(9, DrumBargeRules.BoardingDeparture(BargeLine.South, 8.8f));
            Assert.AreEqual(8, DrumBargeRules.NextDeparture(BargeLine.North, 6.2f));
            Assert.AreEqual(-1, DrumBargeRules.NextDeparture(BargeLine.North, 18.5f));
            Assert.AreEqual(6, DrumBargeRules.NextDeparture(BargeLine.North, 2f));
            foreach (int d in DrumBargeRules.NorthDepartures.Concat(DrumBargeRules.SouthDepartures))
            {
                Assert.IsTrue(DrumBargeRules.IsDrumHour(d), d + " h is a drum hour");
            }

            float length = RidePath.Length(OverworldLayouts.DrumBargeCourse);
            float secondsPerHour = 50f;
            Assert.Less(DrumBargeRules.RideSeconds(length) / secondsPerHour, 2f, "one barge of a line on the water at a time");
            var found = new List<float>();
            DrumBargeRules.UnderWay(BargeLine.North, 8.5f, secondsPerHour, length, found);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(0.5f * secondsPerHour * DrumBargeRules.Speed, found[0], 1e-3f);
            DrumBargeRules.UnderWay(BargeLine.North, 7.8f, secondsPerHour, length, found);
            Assert.AreEqual(1, found.Count, "boarding at the landing");
            Assert.AreEqual(0f, found[0]);
            DrumBargeRules.UnderWay(BargeLine.North, 22f, secondsPerHour, length, found);
            CollectionAssert.IsEmpty(found, "night");
        }

        [Test]
        public void Locks_ShutByTheWheelOrTheBell_OpenForABarge()
        {
            Assert.IsTrue(CanalLockRules.BellShut(21f));
            Assert.IsTrue(CanalLockRules.BellShut(3f));
            Assert.IsFalse(CanalLockRules.BellShut(12f));
            Assert.IsFalse(CanalLockRules.IsShut(12f, 100d, 0d, false), "open by day");
            Assert.IsTrue(CanalLockRules.IsShut(12f, 100d, 200d, false), "the wheel holds it");
            Assert.IsFalse(CanalLockRules.IsShut(12f, 201d, 200d, false), "the wheel's time ran out");
            Assert.IsTrue(CanalLockRules.IsShut(22f, 100d, 0d, false), "the bell");
            Assert.IsFalse(CanalLockRules.IsShut(19.5f, 100d, 200d, true), "a barge in the lock opens it");
            Assert.AreEqual(LockCheck.Turn, CanalLockRules.Check(false, false, true));
            Assert.AreEqual(LockCheck.Cross, CanalLockRules.Check(true, false, true));
            Assert.AreEqual(LockCheck.BargePassing, CanalLockRules.Check(true, true, true));
            Assert.AreEqual(LockCheck.ZoneBlocked, CanalLockRules.Check(true, false, false));
            Assert.IsTrue(CanalLockRules.BargeNear(100f, 140f));
            Assert.IsFalse(CanalLockRules.BargeNear(100f, 150f));

            Vector2[] line = { Vector2.zero, new Vector2(0f, 100f), new Vector2(100f, 100f) };
            Assert.AreEqual(50f, CanalLockRules.DistanceAlong(line, new Vector2(3f, 50f)), 1e-3f);
            Assert.AreEqual(150f, CanalLockRules.DistanceAlong(line, new Vector2(50f, 104f)), 1e-3f);
            Assert.AreEqual(-1f, CanalLockRules.DistanceAlong(line, new Vector2(300f, 300f)), "off the course");
        }

        [Test]
        public void Canal_LandingsOnTheRoads_CourseOnTheCanal_FourGatesAcrossIt()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.MingchengId);
            Assert.IsNotNull(world);
            foreach (Vector2 landing in new[] { OverworldLayouts.DrumBargeSouthLanding, OverworldLayouts.DrumBargeNorthLanding })
            {
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, landing, 0f), $"{landing}: on the road");
                Assert.IsFalse(MapLayoutRules.InRiver(world.World, landing, 5f), $"{landing}: dry");
                Assert.IsTrue(RouteMechanicsDriver.CanalBounds.Contains(landing));
                AssertStation(world, landing, "landing");
            }

            Vector2[] course = OverworldLayouts.DrumBargeCourse;
            for (int i = 1; i < course.Length - 1; i++)
            {
                Assert.IsTrue(MapLayoutRules.InRiver(world.World, course[i], 0f), $"course point {i} {course[i]}: on the canal");
            }

            float barge = RidePath.Length(course);
            float road = Vector2.Distance(OverworldLayouts.DrumBargeSouthLanding, new Vector2(600f, 170f)) + Vector2.Distance(new Vector2(600f, 170f), new Vector2(590f, 400f))
                + Vector2.Distance(new Vector2(590f, 400f), OverworldLayouts.EclusaDaLuaOffset) + Vector2.Distance(OverworldLayouts.EclusaDaLuaOffset, new Vector2(700f, 700f))
                + Vector2.Distance(new Vector2(700f, 700f), OverworldLayouts.DrumBargeNorthLanding);
            Assert.Less(barge, road * 0.7f, "the barge is the short way up the canal");

            CanalLock[] locks = OverworldLayouts.CanalLocks;
            Assert.AreEqual(4, locks.Length);
            Assert.GreaterOrEqual(locks[0].CourseDistance, 0f, "the Eclusa da Lua is on the barges' course");
            Assert.GreaterOrEqual(locks[1].CourseDistance, 0f);
            Assert.AreEqual(-1f, locks[2].CourseDistance, "north of the barges");
            Assert.AreEqual(-1f, locks[3].CourseDistance);
            Assert.AreEqual(OverworldLayouts.CanalLanternasMapId, OverworldRules.ZoneAt(world, locks[1].Center)?.MapIdOrOwner, "in 43's own area");
            Assert.AreEqual(OverworldLayouts.CanalLanternasMapId, OverworldRules.ZoneAt(world, locks[2].Center)?.MapIdOrOwner);
            Vector2[] bridges = { OverworldLayouts.LanternCanalEastBridge, OverworldLayouts.LanternCanalNorthBridge };
            for (int i = 0; i < locks.Length; i++)
            {
                CanalLock gate = locks[i];
                Assert.Less(MapLayoutRules.DistanceToPolyline(gate.Center, OverworldLayouts.LanternCanalLine), 0.5f, gate.Id + ": across the canal");
                foreach (Vector2 bridge in bridges)
                {
                    Assert.Greater(Vector2.Distance(gate.Center, bridge), 100f, gate.Id + ": a way over where no bridge is");
                }

                foreach (Vector2 end in new[] { gate.WestEnd, gate.EastEnd })
                {
                    Assert.IsFalse(MapLayoutRules.InRiver(world.World, end, 3f), $"{gate.Id} {end}: on the bank");
                    Assert.Greater(OverworldRules.SurfaceAt(world, end), OverworldRelief.WaterY, $"{gate.Id} {end}: above the water");
                    Assert.IsTrue(world.World.Landmarks.Any(l => l.Kind == MapLandmarkKind.Signpost && Vector2.Distance(l.Position, end) < 3.5f), $"{gate.Id} {end}: its post");
                    Assert.IsTrue(RouteMechanicsDriver.CanalBounds.Contains(end));
                    Assert.IsNull(OverworldRules.SettlementAt(world, end));
                }

                Assert.AreEqual(i, CanalLockRules.EndAt(locks, gate.WestEnd, out bool west));
                Assert.IsTrue(west);
                Assert.AreEqual(i, CanalLockRules.EndAt(locks, gate.EastEnd, out west));
                Assert.IsFalse(west);
                Assert.Greater(Vector2.Distance(gate.WestEnd, OverworldLayouts.DrumBargeSouthLanding), DrumBargeRules.LandingReach * 2f);
                Assert.Greater(Vector2.Distance(gate.EastEnd, OverworldLayouts.DrumBargeNorthLanding), DrumBargeRules.LandingReach * 2f);
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_EveryKeyInBothLanguages_NoFolksyNames()
        {
            var keys = new HashSet<string>();
            foreach (var entry in RouteMechanicsTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate key " + entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                StringAssert.DoesNotContain("Dona ", entry.PtBR);
                StringAssert.DoesNotContain("Seu ", entry.PtBR);
            }

            foreach (FieldInfo field in typeof(RouteMechanicsTexts).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && f.FieldType == typeof(string)))
            {
                Assert.IsTrue(keys.Contains((string)field.GetRawConstantValue()), field.Name + " has an entry");
            }

            Assert.AreEqual("O Ammo espera a areia: a caravana sai em 3 min", RouteMechanicsTexts.Format(null, RouteMechanicsTexts.CaravanWaitsSand, 3));
            Assert.AreEqual("Próxima barca do tambor às 8h", RouteMechanicsTexts.Format(null, RouteMechanicsTexts.BargeNext, 8));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>A station a hero stands at: in an area, above the water and not on a slope (level within 1.5 m at 2 m round).</summary>
        private static void AssertStation(OverworldLayout world, Vector2 p, string what)
        {
            string at = $"{world.Id} {what} {p}";
            Assert.IsNotNull(OverworldRules.ZoneAt(world, p), at + ": in an area");
            float h = OverworldRules.SurfaceAt(world, p);
            Assert.Greater(h, OverworldRelief.WaterY, at + ": above the water line");
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI / 4f;
                Assert.AreEqual(h, OverworldRules.SurfaceAt(world, p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 2f), 1.5f, at + ": level underfoot");
            }
        }

        /// <summary>The least distance between segments a–b and c–d (sampled; they never cross here).</summary>
        private static float SegmentGap(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float best = float.MaxValue;
            for (int i = 0; i <= 50; i++)
            {
                best = Mathf.Min(best, MapLayoutRules.DistanceToSegment(Vector2.Lerp(a, b, i / 50f), c, d));
                best = Mathf.Min(best, MapLayoutRules.DistanceToSegment(Vector2.Lerp(c, d, i / 50f), a, b));
            }

            return best;
        }
    }
}
