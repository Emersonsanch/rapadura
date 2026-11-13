using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The last capital events (Docs/Mapas100.md): A Barca Atrasou of the Cais de Mesektet (71, <see cref="MesektetLateBarqueRules"/>) and the
    /// river's everlasting night (<see cref="RioEverNightRules"/>), A Cheia of Dur-Jarras and the ziggurat's steps (77,
    /// <see cref="DurJarrasFloodRules"/>), the Chegada da Caravana of the Cais da Coluna and its low gravity (91, <see cref="CaisCaravanRules"/>,
    /// <see cref="CeuLowGravityRules"/>) and the pay of the Maré de Treva (83, <see cref="ProfundoTideRewardRules"/>): pure rules, the places the
    /// layouts declare for them and their texts.
    /// </summary>
    public class FinalCityEventsTests
    {
        /// <summary>The hero's walking speed (m/s) used for "time to get there".</summary>
        private const float WalkSpeed = 5.5f;

        /// <summary>Server-local (Brasília, UTC−3) instant of a day at hh:mm:ss.</summary>
        private static DateTime Local(int hour, int minute, int second = 0) => new DateTime(2026, 11, 13, hour, minute, second, DateTimeKind.Utc).AddHours(3);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // The hour of every capital event
        // ------------------------------------------------------------------

        [Test]
        public void Schedules_Valid_OpenAtTheirMinutes_NeverTwoCapitalEventsAtOnce()
        {
            Assert.IsTrue(MesektetLateBarqueRules.Schedule.IsValid);
            Assert.IsTrue(DurJarrasFloodRules.Schedule.IsValid);
            Assert.IsTrue(CaisCaravanRules.Schedule.IsValid);

            Assert.IsTrue(MesektetLateBarqueRules.WindowAt(Local(14, 30)).IsOpen);
            Assert.IsTrue(MesektetLateBarqueRules.WindowAt(Local(14, 34, 59)).IsOpen);
            Assert.IsFalse(MesektetLateBarqueRules.WindowAt(Local(14, 35)).IsOpen);
            Assert.IsTrue(MesektetLateBarqueRules.IsWarning(MesektetLateBarqueRules.WindowAt(Local(14, 29, 10))));

            Assert.IsTrue(DurJarrasFloodRules.WindowAt(Local(14, 13)).IsOpen);
            Assert.IsFalse(DurJarrasFloodRules.WindowAt(Local(14, 18)).IsOpen);
            Assert.IsTrue(DurJarrasFloodRules.IsWarning(DurJarrasFloodRules.WindowAt(Local(14, 12, 30))));

            Assert.IsTrue(CaisCaravanRules.WindowAt(Local(14, 57)).IsOpen);
            Assert.IsTrue(CaisCaravanRules.WindowAt(Local(15, 1, 59)).IsOpen, "the caravan crosses the hour");
            Assert.IsFalse(CaisCaravanRules.WindowAt(Local(15, 2)).IsOpen);
            Assert.AreEqual(CaisCaravanRules.WindowAt(Local(14, 58)).Index, CaisCaravanRules.WindowAt(Local(15, 1)).Index, "one window across the hour");
            Assert.AreEqual(CaisCaravanRules.WindowAt(Local(14, 58)).Index + 1, CaisCaravanRules.WindowAt(Local(15, 58)).Index, "one caravan an hour");

            // Every second of an hour: at most one capital event open.
            DateTime start = Local(9, 0);
            for (int s = 0; s < 3600; s += 5)
            {
                DateTime at = start.AddSeconds(s);
                int open = 0;
                open += JhilpurMudTideRules.WindowAt(at).IsOpen ? 1 : 0;
                open += SangkentAvalancheRules.WindowAt(at).IsOpen ? 1 : 0;
                open += FarnkeshGaleRules.WindowAt(at).IsOpen ? 1 : 0;
                open += HinokuraGreatTremorRules.WindowAt(at).IsOpen ? 1 : 0;
                open += MesektetLateBarqueRules.WindowAt(at).IsOpen ? 1 : 0;
                open += DurJarrasFloodRules.WindowAt(at).IsOpen ? 1 : 0;
                open += CaisCaravanRules.WindowAt(at).IsOpen ? 1 : 0;
                Assert.LessOrEqual(open, 1, $"{s} s into the hour");
            }

            var keys = new[]
            {
                MesektetLateBarqueRules.PaidKey, DurJarrasFloodRules.PaidKey, DurJarrasFloodRules.SavedTotalKey, DurJarrasFloodRules.StepPaidKey,
                CaisCaravanRules.PaidKey, ProfundoTideRewardRules.PaidKey, FarnkeshGaleRules.PaidKey, HinokuraGreatTremorRules.PaidKey, PortoPolarNightRules.PaidKey,
            };
            Assert.AreEqual(keys.Length, keys.Distinct().Count(), "every event keeps its own marks");
        }

        // ------------------------------------------------------------------
        // A Barca Atrasou (71)
        // ------------------------------------------------------------------

        [Test]
        public void Barque_StartsNearOpenUnpaid_GuardsTwoPerRope_OneMorePerTiedRopeEvery45s_Capped()
        {
            Assert.IsTrue(MesektetLateBarqueRules.CanStart(true, 0L, 7, 40f));
            Assert.IsFalse(MesektetLateBarqueRules.CanStart(false, 0L, 7, 40f), "closed");
            Assert.IsFalse(MesektetLateBarqueRules.CanStart(true, MesektetLateBarqueRules.PaidMark(7), 7, 40f), "paid this window");
            Assert.IsTrue(MesektetLateBarqueRules.CanStart(true, MesektetLateBarqueRules.PaidMark(6), 7, 40f), "last window's mark");
            Assert.IsFalse(MesektetLateBarqueRules.CanStart(true, 0L, 7, MesektetLateBarqueRules.StartRange + 1f), "too far");
            Assert.Less(MesektetLateBarqueRules.StartRange, MesektetLateBarqueRules.LeaveRange);

            Assert.AreEqual(6, MesektetLateBarqueRules.StartGuards(3));
            Assert.AreEqual(2, MesektetLateBarqueRules.StartGuards(1));
            Assert.AreEqual(0, MesektetLateBarqueRules.ReinforcementsDue(44d));
            Assert.AreEqual(1, MesektetLateBarqueRules.ReinforcementsDue(45d));
            Assert.AreEqual(6, MesektetLateBarqueRules.ReinforcementsDue(299d));
            Assert.AreEqual(3, MesektetLateBarqueRules.WaveSize(3, 0));
            Assert.AreEqual(1, MesektetLateBarqueRules.WaveSize(1, 0));
            Assert.AreEqual(1, MesektetLateBarqueRules.WaveSize(3, MesektetLateBarqueRules.MaxAlive - 1), "never past the cap");
            Assert.AreEqual(0, MesektetLateBarqueRules.WaveSize(3, MesektetLateBarqueRules.MaxAlive));
            Assert.AreEqual(0, MesektetLateBarqueRules.WaveSize(0, 0), "no rope tied, no guard");
        }

        [Test]
        public void Barque_CutNeedsAClearPost_MariaCutsFromFarther_SailsWithAllThree_PaidPerRope()
        {
            Assert.IsTrue(MesektetLateBarqueRules.CanCut(true, 0, 2.9f, false));
            Assert.IsFalse(MesektetLateBarqueRules.CanCut(true, 0, 4f, false));
            Assert.IsTrue(MesektetLateBarqueRules.CanCut(true, 0, 4f, true), "Maria and the Light");
            Assert.IsFalse(MesektetLateBarqueRules.CanCut(true, 1, 1f, true), "a scarab guards the post");
            Assert.IsFalse(MesektetLateBarqueRules.CanCut(false, 0, 1f, false), "already cut");

            Assert.IsFalse(MesektetLateBarqueRules.Sails(new[] { true, true, false }));
            Assert.IsTrue(MesektetLateBarqueRules.Sails(new[] { true, true, true }));
            Assert.AreEqual(2, MesektetLateBarqueRules.CutCount(new[] { true, false, true }));
            Assert.AreEqual(0, MesektetLateBarqueRules.CutCount(null));

            Assert.AreEqual(80, MesektetLateBarqueRules.GoldFor(2));
            Assert.AreEqual(120, MesektetLateBarqueRules.GoldFor(9), "clamped to three");
            Assert.AreEqual(0, MesektetLateBarqueRules.StonesFor(2));
            Assert.AreEqual(1, MesektetLateBarqueRules.StonesFor(3));
            Assert.IsFalse(MesektetLateBarqueRules.IsPaid(0L, 0), "0 means never");
        }

        [Test]
        public void Barque_Posts_OnTheQuaysRoads_RisesFartherOut_Reachable()
        {
            Vector2[] posts = OverworldLayouts.MesektetShadowPosts;
            Vector2[] rises = OverworldLayouts.MesektetScarabRise;
            Vector2 town = OverworldLayouts.CaisMesektetOffset;
            Assert.AreEqual(MesektetLateBarqueRules.RopeCount, posts.Length);
            Assert.AreEqual(posts.Length, rises.Length);
            AssertSpots(OverworldLayouts.RioRegionId, OverworldLayouts.CaisMesektetMapId, town, posts, MesektetLateBarqueRules.GuardClearRadius);
            AssertSpots(OverworldLayouts.RioRegionId, OverworldLayouts.CaisMesektetMapId, town, rises, 3f);
            Assert.AreEqual(CityTownRecipes.MesektetBarque, OverworldLayouts.MesektetBarqueBerth, "the ropes run to the Mesektet of the recipe");
            for (int i = 0; i < posts.Length; i++)
            {
                Assert.Greater(Vector2.Distance(rises[i], town), Vector2.Distance(posts[i], town) + 15f, "the scarabs come from outside");
                for (int j = i + 1; j < posts.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(posts[i], posts[j]), MesektetLateBarqueRules.GuardClearRadius * 4f, "one post's guards never block another");
                    Assert.Less(Vector2.Distance(posts[i], posts[j]) / WalkSpeed, MesektetLateBarqueRules.ReinforceEverySeconds / 2d, "the posts are near each other");
                }
            }
        }

        // ------------------------------------------------------------------
        // The everlasting night of the river (71–76)
        // ------------------------------------------------------------------

        [Test]
        public void EverNight_EasesInOver2s_HoldsTheSkyAtMidnight_GivesItBack()
        {
            float hold = 0f;
            hold = RioEverNightRules.Step(hold, true, 1f);
            Assert.AreEqual(0.5f, hold, 1e-4f);
            hold = RioEverNightRules.Step(hold, true, 5f);
            Assert.AreEqual(1f, hold, 1e-4f);
            hold = RioEverNightRules.Step(hold, false, 0.5f);
            Assert.AreEqual(0.75f, hold, 1e-4f);
            Assert.AreEqual(0f, RioEverNightRules.Step(0.1f, false, 10f));

            try
            {
                EnvironmentOverrides.SetNightHold(0f);
                Assert.AreEqual(0.5f, EnvironmentOverrides.HeldNormalizedTime(0.5f), 1e-5f, "no hold: the clock's sky");
                EnvironmentOverrides.SetNightHold(1f);
                foreach (float t in new[] { 0.1f, 0.3f, 0.5f, 0.7f, 0.95f })
                {
                    float held = Mathf.Repeat(EnvironmentOverrides.HeldNormalizedTime(t), 1f);
                    Assert.AreEqual(0f, held, 1e-5f, $"noon or dusk, the river shows midnight ({t})");
                    Assert.AreEqual(0f, TimeOfDayManager.GetDaylightFactor(held * TimeOfDayManager.HoursPerDay), 1e-4f);
                }

                EnvironmentOverrides.Reset();
                Assert.AreEqual(0f, EnvironmentOverrides.NightHold, "a reset gives the sky back");
            }
            finally
            {
                EnvironmentOverrides.Reset();
            }

            Assert.IsTrue(OverworldLayouts.RioBounds.Contains(OverworldLayouts.CaisMesektetOffset));
            Assert.IsTrue(OverworldLayouts.RioBounds.Contains(OverworldLayouts.ProaEncalhadaOffset), "the whole river, 71 to 76");
        }

        // ------------------------------------------------------------------
        // A Cheia (77) and the ziggurat's steps
        // ------------------------------------------------------------------

        [Test]
        public void Flood_ClimbsAStepAMinute_TheCurrentClosesIn_JoaquimSwims_NeverAtTheTablets()
        {
            Assert.AreEqual(1, DurJarrasFloodRules.StepAt(0d));
            Assert.AreEqual(1, DurJarrasFloodRules.StepAt(59.9d));
            Assert.AreEqual(2, DurJarrasFloodRules.StepAt(60d));
            Assert.AreEqual(5, DurJarrasFloodRules.StepAt(299d));
            Assert.AreEqual(5, DurJarrasFloodRules.StepAt(900d));
            for (int step = 2; step <= DurJarrasFloodRules.StepCount; step++)
            {
                Assert.Less(DurJarrasFloodRules.FloodRadius(step), DurJarrasFloodRules.FloodRadius(step - 1), "the water closes in");
            }

            float farthestTablet = OverworldLayouts.DurJarrasFloodTablets.Max(p => Vector2.Distance(p, OverworldLayouts.DurJarrasOffset));
            Assert.Greater(DurJarrasFloodRules.FloodRadius(DurJarrasFloodRules.StepCount), farthestTablet + DurJarrasFloodRules.PickReach + 2f,
                "picking a tablet is never done in the current");
            Assert.Greater(DurJarrasFloodRules.FloodRadius(DurJarrasFloodRules.StepCount), 26f + 10f, "the safe zone and its edge stay dry");

            Assert.IsTrue(DurJarrasFloodRules.Bites(130f, 1 + 1, false));
            Assert.IsFalse(DurJarrasFloodRules.Bites(130f, 1, false));
            Assert.IsFalse(DurJarrasFloodRules.Bites(200f, 5, true), "Joaquim and the Living Water");
            Assert.AreEqual(2f, DurJarrasFloodRules.Bite(100f, 100f), 1e-4f);
            Assert.AreEqual(1f, DurJarrasFloodRules.Bite(41f, 100f), 1e-4f, "never below 40%");
            Assert.AreEqual(0f, DurJarrasFloodRules.Bite(40f, 100f), 1e-4f);
        }

        [Test]
        public void Flood_FiveTablets_OneAtATime_OnTurningRoads_LostAfter30s_InsideTheWindow()
        {
            Assert.AreEqual(DurJarrasFloodRules.TabletCount, DurJarrasFloodRules.TabletAt.Length);
            Assert.AreEqual(FloodTabletPhase.Waiting, DurJarrasFloodRules.PhaseOf(0, 29d));
            Assert.AreEqual(FloodTabletPhase.Lying, DurJarrasFloodRules.PhaseOf(0, 30d));
            Assert.AreEqual(FloodTabletPhase.Lost, DurJarrasFloodRules.PhaseOf(0, 60d));
            Assert.AreEqual(-1, DurJarrasFloodRules.LyingTablet(70d), "a lull between tablets");
            Assert.AreEqual(1, DurJarrasFloodRules.LyingTablet(90d));
            Assert.AreEqual(25d, DurJarrasFloodRules.SecondsLeft(1, 90d), 1e-6);
            Assert.AreEqual(FloodTabletPhase.Waiting, DurJarrasFloodRules.PhaseOf(9, 90d));

            for (int i = 1; i < DurJarrasFloodRules.TabletCount; i++)
            {
                Assert.Greater(DurJarrasFloodRules.TabletAt[i], DurJarrasFloodRules.TabletAt[i - 1] + DurJarrasFloodRules.TabletLifeSeconds, "one tablet at a time");
            }

            double last = DurJarrasFloodRules.TabletAt[DurJarrasFloodRules.TabletCount - 1] + DurJarrasFloodRules.TabletLifeSeconds;
            float trip = Vector2.Distance(OverworldLayouts.DurJarrasFloodTablets[0], OverworldLayouts.DurJarrasZigguratFoot) / WalkSpeed;
            Assert.Less(last + trip, DurJarrasFloodRules.Schedule.WindowMinutes * 60d, "the last tablet can still go up before the window closes");
            Assert.IsFalse(DurJarrasFloodRules.AllDone(299d));
            Assert.IsTrue(DurJarrasFloodRules.AllDone(300d));

            for (long index = 0; index < 30; index++)
            {
                for (int t = 1; t < DurJarrasFloodRules.TabletCount; t++)
                {
                    Assert.AreNotEqual(DurJarrasFloodRules.RoadOf(index, t - 1), DurJarrasFloodRules.RoadOf(index, t), "the water changes road");
                }
            }
        }

        [Test]
        public void Flood_PaidPerTablet_StepsAddTenPercent_StepsOpenWithTheTabletsSaved()
        {
            Assert.AreEqual(105, DurJarrasFloodRules.GoldFor(3, 1));
            Assert.AreEqual(Mathf.RoundToInt(175 * 1.4f), DurJarrasFloodRules.GoldFor(5, 5));
            Assert.AreEqual(175, DurJarrasFloodRules.GoldFor(9, 1), "clamped to five");
            Assert.AreEqual(0, DurJarrasFloodRules.StonesFor(4));
            Assert.AreEqual(1, DurJarrasFloodRules.StonesFor(5));

            Assert.AreEqual(1, DurJarrasFloodRules.StepsOpen(0), "the market is always open");
            Assert.AreEqual(1, DurJarrasFloodRules.StepsOpen(2));
            Assert.AreEqual(2, DurJarrasFloodRules.StepsOpen(3));
            Assert.AreEqual(3, DurJarrasFloodRules.StepsOpen(8));
            Assert.AreEqual(4, DurJarrasFloodRules.StepsOpen(24));
            Assert.AreEqual(5, DurJarrasFloodRules.StepsOpen(25));
            Assert.IsTrue(DurJarrasFloodRules.TowerOpen(100));
            Assert.AreEqual(1, DurJarrasFloodRules.TabletsToNextStep(2));
            Assert.AreEqual(7, DurJarrasFloodRules.TabletsToNextStep(8));
            Assert.AreEqual(0, DurJarrasFloodRules.TabletsToNextStep(40));
            Assert.AreEqual(500, DurJarrasFloodRules.GoldForStep(5));
            for (int step = 2; step <= DurJarrasFloodRules.StepCount; step++)
            {
                Assert.Greater(DurJarrasFloodRules.TabletsForStep(step), DurJarrasFloodRules.TabletsForStep(step - 1));
            }
        }

        [Test]
        public void Flood_Tablets_OnTheTownsRoads_ZigguratFootInThePlaza_Reachable()
        {
            Vector2[] tablets = OverworldLayouts.DurJarrasFloodTablets;
            Vector2 town = OverworldLayouts.DurJarrasOffset;
            Assert.AreEqual(DurJarrasFloodRules.RoadCount, tablets.Length);
            AssertSpots(OverworldLayouts.MisturaRegionId, OverworldLayouts.DurJarrasMapId, town, tablets, DurJarrasFloodRules.PickReach);

            Vector2 foot = OverworldLayouts.DurJarrasZigguratFoot;
            Assert.Less(Vector2.Distance(foot, town), 22.5f, "in the plaza");
            Assert.Greater(Vector2.Distance(foot, CityTownRecipes.DurJarrasClock), 3f, "not on the Relógio de Zurvan");
            Assert.Greater(Vector2.Distance(foot, CityTownRecipes.DurJarrasZiggurat), 8f, "before the ziggurat, not in it");
            foreach (Vector2 tablet in tablets)
            {
                Assert.Less(Vector2.Distance(tablet, foot) / WalkSpeed, DurJarrasFloodRules.TabletLifeSeconds / 2d, "from the ziggurat to the tablet before the water takes it");
            }
        }

        // ------------------------------------------------------------------
        // Chegada da Caravana (91) and the low gravity
        // ------------------------------------------------------------------

        [Test]
        public void Caravan_SixGifts_OneBarqueAtATime_TurningPiers_InsideTheWindow()
        {
            Assert.AreEqual(CaisCaravanRules.GiftCount, CaisCaravanRules.GiftAt.Length);
            Assert.AreEqual(CaravanGiftPhase.Waiting, CaisCaravanRules.PhaseOf(0, 29d));
            Assert.AreEqual(CaravanGiftPhase.Ready, CaisCaravanRules.PhaseOf(0, 30d));
            Assert.AreEqual(CaravanGiftPhase.Gone, CaisCaravanRules.PhaseOf(0, 55d));
            Assert.AreEqual(-1, CaisCaravanRules.ReadyGift(60d));
            Assert.AreEqual(1, CaisCaravanRules.ReadyGift(80d));
            Assert.AreEqual(20d, CaisCaravanRules.SecondsLeft(1, 80d), 1e-6);
            for (int i = 1; i < CaisCaravanRules.GiftCount; i++)
            {
                Assert.Greater(CaisCaravanRules.GiftAt[i], CaisCaravanRules.GiftAt[i - 1] + CaisCaravanRules.BarqueWaitSeconds, "one barque waits at a time");
            }

            Assert.Less(CaisCaravanRules.GiftAt[CaisCaravanRules.GiftCount - 1] + CaisCaravanRules.BarqueWaitSeconds, CaisCaravanRules.Schedule.WindowMinutes * 60d);
            Assert.IsFalse(CaisCaravanRules.AllDone(279d));
            Assert.IsTrue(CaisCaravanRules.AllDone(280d));
            for (long index = 0; index < 30; index++)
            {
                for (int g = 1; g < CaisCaravanRules.GiftCount; g++)
                {
                    Assert.AreNotEqual(CaisCaravanRules.PierOf(index, g - 1), CaisCaravanRules.PierOf(index, g), "never the same pier twice in a row");
                }
            }

            Assert.AreEqual(150, CaisCaravanRules.GoldFor(5));
            Assert.AreEqual(180, CaisCaravanRules.GoldFor(10), "clamped to six");
            Assert.AreEqual(0, CaisCaravanRules.StonesFor(5));
            Assert.AreEqual(1, CaisCaravanRules.StonesFor(6));
        }

        [Test]
        public void Caravan_Piers_OnTheQuaysRoads_MooringInThePlaza_LavineBoardsFromFarther()
        {
            Vector2[] piers = OverworldLayouts.CaisCaravanPiers;
            Vector2 town = OverworldLayouts.CaisDaColunaOffset;
            Assert.AreEqual(CaisCaravanRules.PierCount, piers.Length);
            AssertSpots(OverworldLayouts.CeuRegionId, OverworldLayouts.CaisDaColunaMapId, town, piers, CaisCaravanRules.MoonBoatwomanReach);

            Vector2 mooring = OverworldLayouts.CaisCaravanMooring;
            Assert.Less(Vector2.Distance(mooring, town), 22.5f, "in the plaza");
            foreach (Vector2 fountain in CityTownRecipes.CaisDaColunaFountains)
            {
                Assert.Greater(Vector2.Distance(mooring, fountain), 3f, "not on a fountain");
            }

            foreach (Vector2 pier in piers)
            {
                Assert.Less(Vector2.Distance(mooring, pier) / WalkSpeed, CaisCaravanRules.BarqueWaitSeconds / 2d, "from the caravan to the pier before the barque leaves");
                Assert.IsTrue(CaisCaravanRules.CanBoard(pier, pier + new Vector2(0f, 3.4f), false));
                Assert.IsFalse(CaisCaravanRules.CanBoard(pier, pier + new Vector2(0f, 5f), false));
                Assert.IsTrue(CaisCaravanRules.CanBoard(pier, pier + new Vector2(0f, 5f), true), "Lavine, the Moon Boatwoman");
            }

            Assert.IsTrue(CaisCaravanRules.CanTake(mooring, mooring + new Vector2(3f, 0f)));
            Assert.IsFalse(CaisCaravanRules.CanTake(mooring, mooring + new Vector2(4f, 0f)));
        }

        [Test]
        public void LowGravity_OnMap91_JumpsOneAndAHalfHigher()
        {
            Assert.AreEqual(1.8f, CeuLowGravityRules.JumpHeight(1.2f, CeuLowGravityRules.GravityScale), 1e-4f);
            Assert.AreEqual(1.2f, CeuLowGravityRules.JumpHeight(1.2f, 1f), 1e-4f);
            Assert.Less(CeuLowGravityRules.GravityScale, 1f, "falls are slower");
            Assert.IsTrue(CeuLowGravityRules.InLowGravity(OverworldLayouts.CaisDaColunaOffset));
            Assert.IsTrue(CeuLowGravityRules.InLowGravity(new Vector2(-460f, 0f)), "the Pier of the Moon");
            Assert.IsFalse(CeuLowGravityRules.InLowGravity(OverworldLayouts.BoiaDeLuzOffset), "the River of Light (92) has its own weight");
            Assert.AreEqual(1f, CeuLowGravityRules.ScaleAt(OverworldLayouts.BoiaDeLuzOffset));

            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.CeuRegionId);
            foreach (OverworldZone zone in world.Zones)
            {
                bool inMap = zone.MapIdOrOwner == OverworldLayouts.CaisDaColunaMapId;
                Assert.AreEqual(inMap, CeuLowGravityRules.InLowGravity(zone.Bounds.center), zone.Id + ": low gravity exactly on map 91");
            }
        }

        // ------------------------------------------------------------------
        // Maré de Treva (83)
        // ------------------------------------------------------------------

        [Test]
        public void DarkTide_FirstHeldTideOfTheHourPaid_PerRing_AllThreeAStone()
        {
            long hour = ProfundoTideRewardRules.HourIndex(new DateTime(2026, 11, 13, 14, 59, 59, DateTimeKind.Utc));
            Assert.AreEqual(hour + 1, ProfundoTideRewardRules.HourIndex(new DateTime(2026, 11, 13, 15, 0, 0, DateTimeKind.Utc)));
            Assert.IsTrue(ProfundoTideRewardRules.ShouldPay(2, 0L, hour));
            Assert.IsFalse(ProfundoTideRewardRules.ShouldPay(0, 0L, hour), "a tide that fell pays nothing");
            Assert.IsFalse(ProfundoTideRewardRules.ShouldPay(3, ProfundoTideRewardRules.PaidMark(hour), hour), "once an hour");
            Assert.IsTrue(ProfundoTideRewardRules.ShouldPay(3, ProfundoTideRewardRules.PaidMark(hour - 1), hour));
            Assert.AreEqual(40, ProfundoTideRewardRules.GoldFor(2));
            Assert.AreEqual(60, ProfundoTideRewardRules.GoldFor(5), "clamped to three rings");
            Assert.AreEqual(0, ProfundoTideRewardRules.StonesFor(2));
            Assert.AreEqual(1, ProfundoTideRewardRules.StonesFor(3));
        }

        // ------------------------------------------------------------------
        // Places: shared checks
        // ------------------------------------------------------------------

        /// <summary>
        /// Every spot on a road of its town (no decor is placed on a trail), past the town's flat ground and in the town's own map area, on dry
        /// ground that is level for <paramref name="clearRadius"/> round it (no step, no pinnacle: within 0.6 m at 3 m and within 1.6 m out to
        /// 10 m), far from every clearing (no beast camps on an event) and from every landmark.
        /// </summary>
        private static void AssertSpots(string regionId, string mapId, Vector2 town, Vector2[] spots, float clearRadius)
        {
            OverworldLayout world = OverworldLayouts.Get(regionId);
            Assert.IsNotNull(world, regionId);
            foreach (Vector2 p in spots)
            {
                string at = $"{regionId} {p}";
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, p, 0f), at + ": on a road (no decor)");
                Assert.IsFalse(MapLayoutRules.InRiver(world.World, p, 2f), at + ": dry");
                Assert.Greater(Vector2.Distance(p, town), 30f, at + ": past the town's flat ground (no house, no wall)");
                Assert.Less(Vector2.Distance(p, town), 120f, at + ": by the town");
                Assert.IsNull(OverworldRules.SettlementAt(world, p), at + ": not on a settlement's ground");
                Assert.AreEqual(mapId, OverworldRules.ZoneAt(world, p)?.MapIdOrOwner, at + ": in the capital's own area");

                float h = OverworldRules.SurfaceAt(world, p);
                for (int k = 0; k < 16; k++)
                {
                    float a = k * Mathf.PI / 8f;
                    var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    Assert.AreEqual(h, OverworldRules.SurfaceAt(world, p + dir * 3f), 0.6f, at + ": level underfoot");
                    Assert.AreEqual(h, OverworldRules.SurfaceAt(world, p + dir * clearRadius), 0.8f, at + ": level round it");
                    for (float r = 1f; r <= 10f; r += 1f)
                    {
                        Assert.Less(OverworldRules.SurfaceAt(world, p + dir * r), h + 1.6f, at + ": no pinnacle or wall within 10 m");
                    }
                }

                foreach (MapSpawnZone clearing in world.World.Zones)
                {
                    Assert.Greater(Vector2.Distance(clearing.Center, p) - clearing.Radius, 25f, at + ": away from the clearing " + clearing.Id);
                }

                foreach (MapLandmark landmark in world.World.Landmarks)
                {
                    Assert.Greater(Vector2.Distance(landmark.Position, p), 20f, at + ": away from the landmark " + landmark.Kind);
                }
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_UniqueKeys_BothLanguages_EverySideStepAndCousinLine_NoClashWithTheOtherEvents()
        {
            var keys = new HashSet<string>();
            foreach (var entry in FinalCityEventsTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate key " + entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                StringAssert.DoesNotContain("Dona ", entry.PtBR);
                StringAssert.DoesNotContain("Seu ", entry.PtBR);
            }

            foreach (var entry in CityEventsTexts.Entries.Concat(LateCityEventsTexts.Entries))
            {
                Assert.IsFalse(keys.Contains(entry.Key), "shared with another event table: " + entry.Key);
            }

            for (int side = 0; side < 3; side++)
            {
                Assert.IsTrue(keys.Contains(FinalCityEventsTexts.SideKey(side)), "side " + side);
            }

            for (int step = 1; step <= DurJarrasFloodRules.StepCount; step++)
            {
                Assert.IsTrue(keys.Contains(FinalCityEventsTexts.StepKey(step)), "step " + step);
            }

            CharacterId?[] heroes = { null, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine };
            foreach (string kind in FinalCityEventsTexts.LineKinds)
            {
                var lines = new HashSet<string>();
                foreach (CharacterId? hero in heroes)
                {
                    string key = FinalCityEventsTexts.LineFor(kind, hero);
                    Assert.IsTrue(keys.Contains(key), key);
                    lines.Add(key);
                }

                Assert.AreEqual(5, lines.Count, kind + ": one line per cousin");
            }

            Assert.AreEqual("A corda oeste foi cortada (1/3)",
                FinalCityEventsTexts.Format(null, FinalCityEventsTexts.BarqueRopeCut, FinalCityEventsTexts.Get(null, FinalCityEventsTexts.SideWest), 1, 3));
            Assert.AreEqual("A água chegou ao degrau da Torre (5/5)",
                FinalCityEventsTexts.Format(null, FinalCityEventsTexts.FloodStep, FinalCityEventsTexts.Get(null, FinalCityEventsTexts.StepKey(5)), 5, 5));
        }
    }
}
