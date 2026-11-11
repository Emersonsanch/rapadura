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
    /// The Estepe dos Nartes mechanics (Docs/Mapas100.md 47–52): the steppe race (<see cref="NartesRaceSession"/>), the climb up
    /// Amirani's chain (<see cref="NartesChainRules"/>), the Golds of Heaven (<see cref="NartesSkyGoldRules"/>) and their places.
    /// </summary>
    public class NartesMechanicsTests
    {
        /// <summary>The steepest rise, in metres per metre, that a hero can walk (CharacterController's 45° slope limit).</summary>
        private const float WalkableRise = 1f;

        private static readonly Vector2[] Course = { new Vector2(100f, 0f), new Vector2(200f, 0f), Vector2.zero };

        private static readonly Vector2[] Line = { Vector2.zero, new Vector2(100f, 0f), new Vector2(100f, 100f) };

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.NartesId);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // The race
        // ------------------------------------------------------------------

        private static NartesRaceSession Started()
        {
            var race = new NartesRaceSession(Course);
            race.Tick(NartesRaceRules.CountdownSeconds + 0.1f, Vector2.zero);
            Assert.AreEqual(NartesRaceState.Running, race.State);
            return race;
        }

        [Test]
        public void Race_CountdownTakesNoGates_AndStopsNoClock()
        {
            var race = new NartesRaceSession(Course);
            Assert.IsFalse(race.Tick(1f, Course[0]));
            Assert.AreEqual(NartesRaceState.Countdown, race.State);
            Assert.AreEqual(0f, race.Elapsed);
            Assert.AreEqual(2, race.CountdownLeft);
        }

        [Test]
        public void Race_GatesCountOnlyInOrder()
        {
            NartesRaceSession race = Started();
            Assert.IsFalse(race.Tick(1f, Course[1]), "the second gate cannot be taken first");
            Assert.AreEqual(0, race.NextGate);
            Assert.IsTrue(race.Tick(1f, Course[0]));
            Assert.AreEqual(1, race.NextGate);
        }

        [Test]
        public void Race_FinishesAtTheLastGate()
        {
            NartesRaceSession race = Started();
            foreach (Vector2 gate in Course)
            {
                race.Tick(10f, gate);
            }

            Assert.AreEqual(NartesRaceState.Finished, race.State);
            Assert.AreEqual(30f, race.Elapsed, 1e-3f);
        }

        [Test]
        public void Race_EndsByItself_AtTheLimit_OrWhenTheHeroStrays()
        {
            NartesRaceSession late = Started();
            late.Tick(NartesRaceRules.TimeLimitSeconds + 1f, Vector2.zero);
            Assert.AreEqual(NartesRaceState.TimedOut, late.State);

            NartesRaceSession lost = Started();
            lost.Tick(1f, new Vector2(100f, NartesRaceRules.LeashMetres + 1f));
            Assert.AreEqual(NartesRaceState.Abandoned, lost.State);

            NartesRaceSession dead = Started();
            dead.Abandon();
            Assert.IsTrue(dead.IsOver);
        }

        [TestCase(150f, NartesRaceMedal.Gold)]
        [TestCase(150.1f, NartesRaceMedal.Silver)]
        [TestCase(185f, NartesRaceMedal.Silver)]
        [TestCase(225f, NartesRaceMedal.Bronze)]
        [TestCase(225.1f, NartesRaceMedal.None)]
        public void Race_MedalsByTime(float seconds, NartesRaceMedal medal)
        {
            Assert.AreEqual(medal, NartesRaceRules.MedalFor(seconds));
        }

        [Test]
        public void Race_FirstMedalOfTheDayPaysTriple_AndNoMedalPaysNothing()
        {
            Assert.AreEqual(NartesRaceRules.GoldPurse, NartesRaceRules.Purse(NartesRaceMedal.Gold, false));
            Assert.AreEqual(NartesRaceRules.GoldPurse * NartesRaceRules.FirstOfDayFactor, NartesRaceRules.Purse(NartesRaceMedal.Gold, true));
            Assert.AreEqual(0, NartesRaceRules.Purse(NartesRaceMedal.None, true));
        }

        [Test]
        public void Race_DayRoundsDown()
        {
            Assert.AreEqual(0L, NartesRaceRules.DayOf(86399L));
            Assert.AreEqual(1L, NartesRaceRules.DayOf(86400L));
            Assert.AreEqual(-1L, NartesRaceRules.DayOf(-1L));
        }

        // ------------------------------------------------------------------
        // The chain
        // ------------------------------------------------------------------

        [TestCase(0d, WindGustPhase.Calm)]
        [TestCase(10.4d, WindGustPhase.Calm)]
        [TestCase(10.5d, WindGustPhase.Warning)]
        [TestCase(13.4d, WindGustPhase.Warning)]
        [TestCase(13.5d, WindGustPhase.Blowing)]
        [TestCase(15.9d, WindGustPhase.Blowing)]
        [TestCase(16d, WindGustPhase.Calm)]
        [TestCase(-0.1d, WindGustPhase.Blowing)]
        public void Chain_GustCycle(double time, WindGustPhase phase)
        {
            Assert.AreEqual(phase, NartesChainRules.PhaseAt(time));
        }

        [Test]
        public void Chain_GustsAlternateSides()
        {
            Assert.AreEqual(-NartesChainRules.GustSide(1d), NartesChainRules.GustSide(17d));
            Assert.AreEqual(NartesChainRules.GustSide(1d), NartesChainRules.GustSide(33d));
        }

        [Test]
        public void Chain_AGust_ClearsTheLane_ButARunnerCanHold()
        {
            Assert.Greater(NartesChainRules.PushSpeed * NartesChainRules.BlowSeconds, NartesChainRules.HalfWidth);
            Assert.Less(NartesChainRules.PushSpeed, 5.5f);
        }

        [Test]
        public void Chain_Project_MeasuresAlongAndAcross()
        {
            NartesChainRules.Project(Line, new Vector2(40f, 2f), out float along, out float distance, out Vector2 across);
            Assert.AreEqual(40f, along, 1e-3f);
            Assert.AreEqual(2f, distance, 1e-3f);
            Assert.AreEqual(1f, across.y, 1e-3f);
            NartesChainRules.Project(Line, new Vector2(102f, 50f), out along, out distance, out across);
            Assert.AreEqual(150f, along, 1e-3f);
            Assert.AreEqual(2f, distance, 1e-3f);
        }

        [Test]
        public void Chain_Anchors_RunFromFootToTop_AndAreClose()
        {
            Vector2[] anchors = NartesChainRules.Anchors(Line);
            Assert.AreEqual(Line[0], anchors[0]);
            Assert.AreEqual(Line[2], anchors[anchors.Length - 1]);
            for (int i = 1; i < anchors.Length; i++)
            {
                Assert.LessOrEqual(Vector2.Distance(anchors[i - 1], anchors[i]), NartesChainRules.AnchorSpacing + 10f);
            }
        }

        [Test]
        public void Chain_Shelter_IsRoundTheRings()
        {
            Vector2[] anchors = NartesChainRules.Anchors(Line);
            Assert.IsTrue(NartesChainRules.IsSheltered(anchors, anchors[1] + new Vector2(0f, NartesChainRules.ShelterRadius)));
            Assert.IsFalse(NartesChainRules.IsSheltered(anchors, new Vector2(20f, 0f)));
        }

        [Test]
        public void Chain_AFall_GoesOnlyToTheRingBelow()
        {
            var climb = new NartesChainClimb(5, 200f, 0f);
            climb.Reach(100f);
            Assert.AreEqual(2, climb.SafeAnchor);
            climb.Reach(110f);
            Assert.AreEqual(2, climb.SafeAnchor);
            climb.Reach(119f);
            Assert.AreEqual(3, climb.SafeAnchor, "within the shelter of the next ring, that ring already holds");
            climb.Reach(70f);
            Assert.AreEqual(1, climb.SafeAnchor, "a hero who walked back down must not fall upwards");
            Assert.AreEqual(3, climb.BestAnchor);
        }

        [Test]
        public void Chain_Top_IsReached()
        {
            var climb = new NartesChainClimb(5, 200f, 0f);
            Assert.IsFalse(climb.ReachedTop);
            climb.Reach(199f);
            Assert.IsTrue(climb.ReachedTop);
        }

        [TestCase(1000f, 1000f, 40f)]
        [TestCase(10f, 1000f, 9f)]
        [TestCase(1f, 1000f, 0f)]
        [TestCase(0.5f, 1000f, 0f)]
        public void Chain_FallDamage_CanNeverKill(float current, float max, float expected)
        {
            Assert.AreEqual(expected, NartesChainRules.FallDamage(current, max), 1e-3f);
            Assert.Greater(current - NartesChainRules.FallDamage(current, max), 0f);
        }

        // ------------------------------------------------------------------
        // The Golds of Heaven
        // ------------------------------------------------------------------

        [TestCase(CharacterId.Joaquim)]
        [TestCase(CharacterId.Maria)]
        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Icaro)]
        public void Golds_AreTooHotForTheOlderCousins(CharacterId hero)
        {
            Assert.AreEqual(NartesSkyGoldCheck.TooHot, NartesSkyGoldRules.Check(hero, NartesSkyGoldRules.NeverTaken, 10L));
        }

        [Test]
        public void Golds_OnlyLavine_OncePerDay()
        {
            Assert.AreEqual(NartesSkyGoldCheck.Take, NartesSkyGoldRules.Check(CharacterId.Lavine, NartesSkyGoldRules.NeverTaken, 10L));
            Assert.AreEqual(NartesSkyGoldCheck.Cooled, NartesSkyGoldRules.Check(CharacterId.Lavine, 10L, 10L));
            Assert.AreEqual(NartesSkyGoldCheck.Take, NartesSkyGoldRules.Check(CharacterId.Lavine, 9L, 10L));
            Assert.AreEqual(NartesSkyGoldCheck.TooHot, NartesSkyGoldRules.Check(null, NartesSkyGoldRules.NeverTaken, 10L));
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        private static void AssertLandInZone(Vector2 p, params string[] zones)
        {
            OverworldZone zone = OverworldRules.ZoneAt(World, p);
            Assert.IsNotNull(zone, $"{p} has no zone");
            CollectionAssert.Contains(zones, zone.Id, $"{p} is in {zone.Id}");
            Assert.Greater(OverworldRules.SurfaceAt(World, p), -0.2f, $"{p} is under water");
        }

        [Test]
        public void Layout_Bounds_MatchTheRegion()
        {
            Assert.AreEqual(World.Bounds, OverworldLayouts.NartesBounds);
            Assert.IsNotNull(World.Zone(OverworldLayouts.NartesAnchorZoneId));
        }

        [Test]
        public void Layout_Race_StaysOnTheTracks_AndEndsAtTheStart()
        {
            AssertLandInZone(OverworldLayouts.NartesRaceStart, "pistas_de_corrida");
            foreach (Vector2 gate in OverworldLayouts.NartesRaceCourse)
            {
                AssertLandInZone(gate, "pistas_de_corrida");
            }

            Assert.AreEqual(OverworldLayouts.NartesRaceStart, OverworldLayouts.NartesRaceCourse.Last());
        }

        [Test]
        public void Layout_Race_LengthMatchesTheMedals()
        {
            float length = Vector2.Distance(OverworldLayouts.NartesRaceStart, OverworldLayouts.NartesRaceCourse[0]) + NartesChainRules.Length(OverworldLayouts.NartesRaceCourse);
            Assert.AreEqual(1035f, length, 40f);
            Assert.Greater(length / 5.5f, NartesRaceRules.SilverSeconds, "on foot the silver stays out of reach");
            Assert.Less(length / 5.5f, NartesRaceRules.BronzeSeconds, "on foot the bronze is possible");
            Assert.Less(length / (5.5f * 1.45f), NartesRaceRules.GoldSeconds, "a good mount can win the gold");
        }

        [Test]
        public void Layout_Golds_LieInTheKurgans()
        {
            Assert.AreEqual(NartesMechanicsTexts.Golds.Length, OverworldLayouts.NartesSkyGolds.Length);
            foreach (Vector2 gold in OverworldLayouts.NartesSkyGolds)
            {
                AssertLandInZone(gold, "kurgans_de_ouro", "kurgan_aberto", "kurgans_do_leste");
            }
        }

        [Test]
        public void Layout_Chain_RunsThroughTheCaucasus_OnLand()
        {
            foreach (Vector2 anchor in NartesChainRules.Anchors(OverworldLayouts.AmiraniChain))
            {
                AssertLandInZone(anchor, "corrente_de_amirani", "cume_de_ohya");
            }
        }

        [Test]
        public void Layout_Chain_IsWalkableAllTheWay()
        {
            Vector2[] line = OverworldLayouts.AmiraniChain;
            float length = NartesChainRules.Length(line);
            const float step = 3f;
            for (float along = 0f; along + step <= length; along += step)
            {
                Vector2 a = NartesChainRules.PointAt(line, along);
                Vector2 b = NartesChainRules.PointAt(line, along + step);
                float rise = Mathf.Abs(OverworldRules.SurfaceAt(World, b) - OverworldRules.SurfaceAt(World, a)) / step;
                Assert.LessOrEqual(rise, WalkableRise, $"too steep at {a}");
            }
        }

        [Test]
        public void Texts_HaveBothLanguages()
        {
            foreach (var entry in NartesMechanicsTexts.Entries)
            {
                Assert.IsNotEmpty(entry.En, entry.Key);
                Assert.IsNotEmpty(entry.PtBR, entry.Key);
            }
        }
    }
}
