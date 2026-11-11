using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The four mechanics on the way of the two main stories (Docs/Mapas100.md): the sparks and canals of the Jardim de Mihrshah (06,
    /// <see cref="ZarafshanGardenRules"/>), the cart of the Calhas de Pedra (37, <see cref="TetoCartRules"/>), the silence of the Ermidas Suspensas
    /// (56, <see cref="FarnkeshSilenceRules"/>) and the current of the Rio de Luz (92, <see cref="CeuRiverRules"/>): pure rules, and the places the
    /// layouts declare for them on good ground.
    /// </summary>
    public class StoryPathMechanicsTests
    {
        private const long T = 1_700_000_000L;

        // ------------------------------------------------------------------
        // 06 · Jardim de Mihrshah
        // ------------------------------------------------------------------

        [Test]
        public void Garden_ASparkIsFreedOnlyWhenNoBeastGuardsIt_AndStaysFree()
        {
            Assert.AreEqual(ZarafshanSparkCheck.Free, ZarafshanGardenRules.Check(0L, 0));
            Assert.AreEqual(ZarafshanSparkCheck.Guarded, ZarafshanGardenRules.Check(0L, 2));
            Assert.AreEqual(ZarafshanSparkCheck.AlreadyFree, ZarafshanGardenRules.Check(T, 3), "a freed spark never goes back");
            Assert.IsTrue(ZarafshanGardenRules.Guards(Vector2.zero, new Vector2(10f, 9f)));
            Assert.IsFalse(ZarafshanGardenRules.Guards(Vector2.zero, new Vector2(10f, 10f)), "14 m round the cage");
            Assert.AreEqual("zarafshan.garden.spark.noroeste", ZarafshanGardenRules.FreedKey("noroeste"));
        }

        [Test]
        public void Garden_LitArmsMendOutOfCombat_ThePavilionOpensWithAllFour()
        {
            ZarafshanGardenSpark[] sparks = OverworldLayouts.MihrshahSparks;
            var three = new[] { T, T, T, 0L };
            Assert.AreEqual(3, ZarafshanGardenRules.FreeCount(three));
            Assert.IsFalse(ZarafshanGardenRules.PavilionOpen(three, 4));
            Assert.IsTrue(ZarafshanGardenRules.PavilionOpen(new[] { T, T, T, T }, 4));

            Vector2 onSouthArm = OverworldLayouts.MihrshahGardenCentre + new Vector2(2f, -30f);
            Assert.AreEqual(-1, ZarafshanGardenRules.LitCanalAt(sparks, three, onSouthArm), "the south arm's spark (south-east) is still caught");
            Assert.AreEqual(3, ZarafshanGardenRules.LitCanalAt(sparks, new[] { 0L, 0L, 0L, T }, onSouthArm));
            Assert.AreEqual(-1, ZarafshanGardenRules.LitCanalAt(sparks, new[] { T, T, T, T }, onSouthArm + new Vector2(2f, 0f)), "3.5 m beside the arm");

            Assert.IsFalse(ZarafshanGardenRules.OutOfCombat(3.9f));
            Assert.IsTrue(ZarafshanGardenRules.OutOfCombat(4f));
            Assert.AreEqual(40f, ZarafshanGardenRules.MendOver(100f, 1000f, ZarafshanGardenRules.CanalHealPerSecond, 1f), 1e-3f);
            Assert.AreEqual(10f, ZarafshanGardenRules.MendOver(990f, 1000f, ZarafshanGardenRules.PavilionHealPerSecond, 1f), 1e-3f, "never past full");
            Assert.AreEqual(0f, ZarafshanGardenRules.MendOver(1000f, 1000f, 0.06f, 1f));
        }

        [Test]
        public void Garden_CagesOnTheQuadrantClearings_CanalsInsideTheWalls()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            ZarafshanGardenSpark[] sparks = OverworldLayouts.MihrshahSparks;
            Assert.AreEqual(4, sparks.Length);
            Vector2 c = OverworldLayouts.MihrshahGardenCentre;
            for (int i = 0; i < sparks.Length; i++)
            {
                ZarafshanGardenSpark s = sparks[i];
                OverworldZone zone = OverworldRules.ZoneAt(world, s.Cage);
                Assert.IsNotNull(zone, s.Id);
                Assert.AreEqual(MapLayouts.MihrshahId, zone.MapId ?? zone.OwnerMapId, s.Id + " cage outside the garden's map");
                Assert.IsNull(OverworldRules.SettlementAt(world, s.Cage), s.Id + " cage on the post's safe ground");
                Assert.IsFalse(MapLayoutRules.InRiver(world.World, s.Cage, 2f), s.Id + " cage in the water");
                Assert.AreEqual(i, ZarafshanGardenRules.CageAt(sparks, s.Cage));
                Assert.Less(Vector2.Distance(c, s.CanalA), ZarafshanGardenMechanicsDriver.GardenHalf, s.Id);
                Assert.Less(Vector2.Distance(c, s.CanalB), ZarafshanGardenMechanicsDriver.GardenHalf, s.Id);
                Assert.Greater(Vector2.Distance(s.Cage, c), ZarafshanGardenRules.PavilionRadius + ZarafshanGardenRules.CageReach, s.Id + " cage in the pavilion");
            }
        }

        // ------------------------------------------------------------------
        // 37 · Calhas de Pedra
        // ------------------------------------------------------------------

        [Test]
        public void Cart_SteersInsideTheChute_CatchesSparksItPasses()
        {
            Assert.AreEqual(4f, TetoCartRules.Steer(0f, 1f, 0.5f), 1e-4f);
            Assert.AreEqual(TetoCartRules.SteerLimit, TetoCartRules.Steer(5f, 1f, 1f), 1e-4f);
            Assert.AreEqual(-TetoCartRules.SteerLimit, TetoCartRules.Steer(-5f, -3f, 1f), 1e-4f, "steer clamped to −1");

            var spark = new TetoCartSpark(120f, 2f);
            Assert.IsTrue(TetoCartRules.Catches(spark, 119f, 121f, 3.7f, false));
            Assert.IsFalse(TetoCartRules.Catches(spark, 119f, 121f, 3.7f, true), "narrower at night");
            Assert.IsFalse(TetoCartRules.Catches(spark, 121f, 125f, 2f, false), "already passed");
            Assert.IsFalse(TetoCartRules.Catches(spark, 100f, 119f, 2f, false), "not reached yet");
        }

        [Test]
        public void Cart_ScoreRecordAndNuggets()
        {
            Assert.AreEqual(30000L, TetoCartRules.ScoreMs(48f, 12));
            Assert.AreEqual(1L, TetoCartRules.ScoreMs(1f, 12), "never under 1 ms");
            Assert.AreEqual(30000L, new TetoCartRun(12, 48f).ScoreMs);
            Assert.IsTrue(TetoCartRules.IsRecord(0L, 40000L));
            Assert.IsTrue(TetoCartRules.IsRecord(41000L, 40000L));
            Assert.IsFalse(TetoCartRules.IsRecord(40000L, 40000L));
            Assert.AreEqual(36, TetoCartRules.Nuggets(12, 0, 19700));
            Assert.AreEqual(0, TetoCartRules.Nuggets(12, 19700, 19700), "once per UTC day");
            Assert.AreEqual(15, TetoCartRules.Nuggets(5, 19699, 19700));
        }

        [Test]
        public void Cart_TheStationHoldsTheCartWhenTheAvalancheWouldMeetIt()
        {
            float run = TetoCartRules.RunSeconds(RidePath.Length(OverworldLayouts.TetoCartCourse), false);
            Assert.AreEqual(47.8f, run, 0.5f);
            Assert.AreEqual(38.7f, TetoCartRules.RunSeconds(RidePath.Length(OverworldLayouts.TetoCartCourse), true), 0.5f);

            // Cycle 0 slides lane 0 (on the course), cycle 1 lane 1 (off it), cycle 3 lane 3 (on it).
            double c0 = 0d, c1 = TetoAvalancheRules.CycleSeconds, c3 = 3 * TetoAvalancheRules.CycleSeconds;
            Assert.IsTrue(TetoCartRules.AvalancheAllows(c0 + 5d, run), "the run ends before the bell");
            Assert.IsFalse(TetoCartRules.AvalancheAllows(c0 + 20d, run), "the bell would ring with the cart on the chute");
            Assert.IsFalse(TetoCartRules.AvalancheAllows(c3 + 82d, run), "sliding now");
            Assert.IsTrue(TetoCartRules.AvalancheAllows(c3 + 88d, run), "the slide is over");
            Assert.IsTrue(TetoCartRules.AvalancheAllows(c1 + 70d, run), "this cycle slides another chute");

            Assert.AreEqual(TetoCartCheck.ZoneBlocked, TetoCartRules.Check(false, true, 99f));
            Assert.AreEqual(TetoCartCheck.Resting, TetoCartRules.Check(true, true, 1f));
            Assert.AreEqual(TetoCartCheck.Avalanche, TetoCartRules.Check(true, false, 99f));
            Assert.AreEqual(TetoCartCheck.Ride, TetoCartRules.Check(true, true, 99f));
        }

        [Test]
        public void Cart_CourseRunsDownTheTwoChutes_OnDryGround_SparksAlongIt()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.TetoId);
            Vector2[] course = OverworldLayouts.TetoCartCourse;
            float length = RidePath.Length(course);
            Assert.AreEqual(813f, length, 5f);
            Assert.IsTrue(TetoAvalancheRules.InLane(OverworldLayouts.ChuteLanes[3], course[0]), "boards in the east chute of the Calhas Geladas");
            Assert.IsTrue(TetoAvalancheRules.InLane(OverworldLayouts.ChuteLanes[3], course[1]));
            Assert.IsTrue(TetoAvalancheRules.InLane(OverworldLayouts.ChuteLanes[0], course[2]), "on to the east chute of the Calhas de Pedra");
            Assert.IsTrue(TetoAvalancheRules.InLane(OverworldLayouts.ChuteLanes[0], course[3]));
            for (float d = 0f; d <= length; d += 5f)
            {
                Vector2 p = RidePath.PointAt(course, d);
                Assert.IsFalse(MapLayoutRules.InRiver(world.World, p, TetoCartRules.SteerLimit + 1f), "the course meets water at " + p);
                OverworldZone zone = OverworldRules.ZoneAt(world, p);
                Assert.IsNotNull(zone, "off the region at " + p);
                Assert.AreEqual(OverworldLayouts.CalhasId, zone.MapId ?? zone.OwnerMapId, "the course leaves map 37 at " + p);
                Assert.IsTrue(TetoCartMechanicsDriver.ChutesBounds.Contains(p), "outside the chutes' bounds at " + p);
            }

            Assert.IsNull(OverworldRules.SettlementAt(world, course[0]));
            Assert.IsNull(OverworldRules.SettlementAt(world, course[course.Length - 1]));
            float last = 0f;
            foreach (TetoCartSpark spark in TetoCartRules.Sparks)
            {
                Assert.Greater(spark.Distance, last);
                Assert.Less(spark.Distance, length);
                Assert.LessOrEqual(Mathf.Abs(spark.Offset), TetoCartRules.SteerLimit);
                last = spark.Distance;
            }
        }

        // ------------------------------------------------------------------
        // 56 · Ermidas Suspensas
        // ------------------------------------------------------------------

        [Test]
        public void Silence_RunningFillsWalkingEmpties_LavineAndNightAreQuiet()
        {
            Assert.AreEqual(FarnkeshPace.Still, FarnkeshSilenceRules.PaceOf(0.1f, false));
            Assert.AreEqual(FarnkeshPace.Walking, FarnkeshSilenceRules.PaceOf(2.5f, false));
            Assert.AreEqual(FarnkeshPace.Running, FarnkeshSilenceRules.PaceOf(5.5f, false));
            Assert.AreEqual(FarnkeshPace.Mounted, FarnkeshSilenceRules.PaceOf(2f, true));
            Assert.AreEqual(FarnkeshPace.Still, FarnkeshSilenceRules.PaceOf(0f, true), "a mount standing still is silent");

            Assert.AreEqual(8f, FarnkeshSilenceRules.Step(0f, FarnkeshPace.Running, 0, 1f, 1f), 1e-4f);
            Assert.AreEqual(44f, FarnkeshSilenceRules.Step(50f, FarnkeshPace.Walking, 0, 1f, 1f), 1e-4f);
            Assert.AreEqual(0f, FarnkeshSilenceRules.Step(10f, FarnkeshPace.Still, 0, 1f, 1f), 1e-4f, "never below 0");
            Assert.AreEqual(100f, FarnkeshSilenceRules.Step(99f, FarnkeshPace.Mounted, 2, 1f, 1f), 1e-4f, "never above 100");
            Assert.AreEqual(6f, FarnkeshSilenceRules.Step(0f, FarnkeshPace.Still, 2, 1f, 0f), 1e-4f, "+3 a blow");

            Assert.AreEqual(0.6f, FarnkeshSilenceRules.Quiet(true, false), 1e-4f);
            Assert.AreEqual(0.36f, FarnkeshSilenceRules.Quiet(true, true), 1e-4f);
            Assert.AreEqual(8f * 0.36f, FarnkeshSilenceRules.Rate(FarnkeshPace.Running, 0.36f), 1e-4f);
            Assert.AreEqual(FarnkeshSilenceRules.WalkRate, FarnkeshSilenceRules.Rate(FarnkeshPace.Walking, 0.36f), "the quiet factor only on what fills it");

            float meter = 0f;
            float seconds = 0f;
            while (!FarnkeshSilenceRules.Full(meter))
            {
                meter = FarnkeshSilenceRules.Step(meter, FarnkeshPace.Running, 0, 1f, 0.1f);
                seconds += 0.1f;
            }

            Assert.AreEqual(12.5f, seconds, 0.15f, "running from empty fills it in 12.5 s");
        }

        [Test]
        public void Silence_TheEchoMarksThenStrikes_StepAsideAndItMisses_NeverKills()
        {
            Assert.AreEqual(FarnkeshEchoPhase.None, FarnkeshSilenceRules.EchoPhaseAt(-1f));
            Assert.AreEqual(FarnkeshEchoPhase.Marked, FarnkeshSilenceRules.EchoPhaseAt(1.4f));
            Assert.AreEqual(FarnkeshEchoPhase.Strike, FarnkeshSilenceRules.EchoPhaseAt(1.5f));
            Assert.IsTrue(FarnkeshSilenceRules.StrikeHits(Vector2.zero, new Vector2(3f, 1.5f)));
            Assert.IsFalse(FarnkeshSilenceRules.StrikeHits(Vector2.zero, new Vector2(3.6f, 0f)));
            Assert.AreEqual(120f, FarnkeshSilenceRules.StrikeAmount(1000f, 1000f), 1e-3f);
            Assert.AreEqual(50f, FarnkeshSilenceRules.StrikeAmount(300f, 1000f), 1e-3f, "never under 25 %");
            Assert.AreEqual(0f, FarnkeshSilenceRules.StrikeAmount(200f, 1000f), 1e-3f);
            Assert.AreEqual(1, FarnkeshSilenceRules.WarningCrossed(59f, 61f));
            Assert.AreEqual(2, FarnkeshSilenceRules.WarningCrossed(84f, 86f));
            Assert.AreEqual(0, FarnkeshSilenceRules.WarningCrossed(61f, 70f));
        }

        [Test]
        public void Silence_AreaIsMap56_HoldsThePostAndTheBellsArena()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.FarnkeshRegionId);
            Rect area = OverworldLayouts.ErmidasSilenceArea;
            Assert.IsTrue(area.Contains(OverworldLayouts.SinoQueNaoParaArena));
            Assert.IsTrue(area.Contains(OverworldLayouts.PinaculoDoSinoOffset));
            Assert.IsNotNull(OverworldRules.SettlementAt(world, OverworldLayouts.PinaculoDoSinoOffset), "the post's safe ground stays quiet");
            for (float x = area.xMin + 20f; x < area.xMax; x += 80f)
            {
                for (float z = area.yMin + 20f; z < area.yMax; z += 80f)
                {
                    OverworldZone zone = OverworldRules.ZoneAt(world, new Vector2(x, z));
                    Assert.IsNotNull(zone);
                    Assert.AreEqual(OverworldLayouts.ErmidasMapId, zone.MapId ?? zone.OwnerMapId, "outside map 56 at " + x + ", " + z);
                }
            }
        }

        // ------------------------------------------------------------------
        // 92 · Rio de Luz
        // ------------------------------------------------------------------

        [Test]
        public void River_DarkBuoysLight_LitOnesCarryToTheNextLitTheWayTheHeroFaces()
        {
            var lit = new[] { T, 0L, T, 0L, T };
            Assert.AreEqual(CeuRiverCheck.Light, CeuRiverRules.Check(lit, 1, true, true, 99f));
            Assert.AreEqual(CeuRiverCheck.Ride, CeuRiverRules.Check(lit, 0, true, true, 99f));
            Assert.AreEqual(2, CeuRiverRules.NextLit(lit, 0, true), "the current goes past the dark buoy");
            Assert.AreEqual(0, CeuRiverRules.NextLit(lit, 2, false));
            Assert.AreEqual(-1, CeuRiverRules.NextLit(lit, 4, true), "the east mouth");
            Assert.AreEqual(CeuRiverCheck.NoBuoy, CeuRiverRules.Check(lit, 0, false, true, 99f));
            Assert.AreEqual(CeuRiverCheck.ZoneBlocked, CeuRiverRules.Check(lit, 2, true, false, 99f));
            Assert.AreEqual(CeuRiverCheck.Resting, CeuRiverRules.Check(lit, 2, true, true, 1f));
            Assert.IsTrue(CeuRiverRules.FacesDownstream(new Vector2(0.2f, -1f)));
            Assert.IsFalse(CeuRiverRules.FacesDownstream(new Vector2(-0.2f, 1f)));
            Assert.AreEqual(18f, CeuRiverRules.Speed(true, true));
            Assert.AreEqual(6f, CeuRiverRules.Speed(false, true), "against the current, night or day");
        }

        [Test]
        public void River_TheCourseFollowsTheLight_FastWithTheCurrent_SlowAgainstIt()
        {
            Vector2[] river = OverworldLayouts.RioDeLuz;
            Vector2[] buoys = OverworldLayouts.RioDeLuzBuoys;
            Vector2[] down = CeuRiverRules.Course(river, buoys, 0, 2);
            Assert.AreEqual(buoys[0], down[0]);
            Assert.AreEqual(buoys[2], down[down.Length - 1]);
            for (int i = 1; i + 1 < down.Length; i++)
            {
                Assert.Greater(down[i].x, down[i - 1].x - 1e-3f, "east all the way");
            }

            Vector2[] up = CeuRiverRules.Course(river, buoys, 3, 1);
            for (int i = 2; i + 1 < up.Length; i++)
            {
                Assert.Less(up[i].x, up[i - 1].x + 1e-3f, "west all the way");
            }

            float[] hops = { 655f, 521f, 488f, 715f };
            for (int i = 0; i < hops.Length; i++)
            {
                Assert.AreEqual(hops[i], RidePath.Length(CeuRiverRules.Course(river, buoys, i, i + 1)), 15f, "hop " + i);
                float with = CeuRiverRules.RideSeconds(river, buoys, i, i + 1, false);
                float against = CeuRiverRules.RideSeconds(river, buoys, i + 1, i, false);
                Assert.AreEqual(2.5f, against / with, 0.01f, "15 m/s with, 6 m/s against");
            }

            Assert.AreEqual(new Vector2(0f, 492.5f), CeuRiverRules.RiverAt(river, 0f));
        }

        [Test]
        public void River_BuoysOnTheSouthBank_DryAndInMap92()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.CeuRegionId);
            Vector2[] buoys = OverworldLayouts.RioDeLuzBuoys;
            Assert.AreEqual(5, buoys.Length);
            for (int i = 0; i < buoys.Length; i++)
            {
                Vector2 b = buoys[i];
                if (i > 0)
                {
                    Assert.Greater(b.x, buoys[i - 1].x, "west to east");
                }

                Assert.IsFalse(MapLayoutRules.InRiver(world.World, b, 2f), "buoy " + i + " in the water");
                float off = Vector2.Distance(b, CeuRiverRules.RiverAt(OverworldLayouts.RioDeLuz, b.x));
                Assert.That(off, Is.InRange(10f, 13f), "buoy " + i + " is not on the bank");
                Assert.Less(b.y, CeuRiverRules.RiverAt(OverworldLayouts.RioDeLuz, b.x).y, "buoy " + i + " on the north bank");
                OverworldZone zone = OverworldRules.ZoneAt(world, b);
                Assert.IsNotNull(zone, "buoy " + i);
                Assert.AreEqual(OverworldLayouts.RioDeLuzMapId, zone.MapId ?? zone.OwnerMapId, "buoy " + i + " outside map 92");
                Assert.IsNull(OverworldRules.SettlementAt(world, b), "buoy " + i + " on a post's safe ground");
                Assert.IsTrue(OverworldLayouts.RioDeLuzRow.Contains(b));
                Assert.AreEqual(i, CeuRiverRules.BuoyAt(buoys, b));
            }
        }
    }
}
