using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Three more capital events (Docs/Mapas100.md): the Vendaval of Farnkesh (54, <see cref="FarnkeshGaleRules"/>), the Noite Polar of the Porto
    /// da Lua Branca (59, <see cref="PortoPolarNightRules"/>) and the Grande Tremor of Hinokura (65, <see cref="HinokuraGreatTremorRules"/>): pure
    /// rules, the places the layouts declare for them (on walkable ground, clear of decor, pinnacles and clearings) and their texts.
    /// </summary>
    public class LateCityEventsTests
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
        // The turning order
        // ------------------------------------------------------------------

        [Test]
        public void Turns_NeverTheSameTwiceInARow_FixedPerWindow_AllPlacesUsed()
        {
            for (long index = -5; index < 60; index++)
            {
                var seen = new HashSet<int>();
                for (int step = 0; step < 6; step++)
                {
                    int side = CityEventTurns.SideOf(index, step, 3);
                    Assert.That(side, Is.InRange(0, 2));
                    Assert.AreEqual(side, CityEventTurns.SideOf(index, step, 3), "the same for everyone");
                    if (step > 0)
                    {
                        Assert.AreNotEqual(CityEventTurns.SideOf(index, step - 1, 3), side, $"window {index}, step {step}: the wind turns");
                    }

                    seen.Add(side);
                }

                Assert.GreaterOrEqual(seen.Count, 2, $"window {index}");
            }

            Assert.IsTrue(Enumerable.Range(0, 12).Select(i => CityEventTurns.SideOf(i, 0, 3)).Distinct().Count() == 3, "each window starts elsewhere");
            Assert.AreEqual(0, CityEventTurns.SideOf(7, 3, 1));
        }

        // ------------------------------------------------------------------
        // Vendaval (54)
        // ------------------------------------------------------------------

        [Test]
        public void Gale_OpensEveryHourAtFifty_ForFiveMinutes_WarnedTheMinuteBefore()
        {
            Assert.IsTrue(FarnkeshGaleRules.Schedule.IsValid);
            Assert.IsTrue(FarnkeshGaleRules.WindowAt(Local(14, 50)).IsOpen);
            Assert.IsTrue(FarnkeshGaleRules.WindowAt(Local(14, 54, 59)).IsOpen);
            Assert.IsFalse(FarnkeshGaleRules.WindowAt(Local(14, 55)).IsOpen);
            Assert.IsTrue(FarnkeshGaleRules.IsWarning(FarnkeshGaleRules.WindowAt(Local(14, 49, 10))));
            Assert.IsFalse(FarnkeshGaleRules.IsWarning(FarnkeshGaleRules.WindowAt(Local(14, 48, 50))));
            Assert.AreEqual(FarnkeshGaleRules.WindowAt(Local(14, 51)).Index + 1, FarnkeshGaleRules.WindowAt(Local(15, 51)).Index, "one gale an hour");
            Assert.AreEqual(60d, FarnkeshGaleRules.SecondsInto(FarnkeshGaleRules.WindowAt(Local(14, 51)), Local(14, 51)), 1e-6);

            // Never at the same minutes as the other capital events: Maré de Lodo :20–:26, Avalanche :40–:45, Grande Tremor :05–:10.
            Assert.IsFalse(JhilpurMudTideRules.WindowAt(Local(14, 52)).IsOpen);
            Assert.IsFalse(SangkentAvalancheRules.WindowAt(Local(14, 52)).IsOpen);
            Assert.IsFalse(HinokuraGreatTremorRules.WindowAt(Local(14, 52)).IsOpen);
        }

        [Test]
        public void Gale_FiveGusts_WarnedInTurn_NeverOverlapping_InsideTheWindow()
        {
            Assert.AreEqual(FarnkeshGaleRules.GustCount, FarnkeshGaleRules.GustAt.Length);
            Assert.AreEqual(GalePhase.Quiet, FarnkeshGaleRules.PhaseOf(0, 5d));
            Assert.AreEqual(GalePhase.Warning, FarnkeshGaleRules.PhaseOf(0, 10d));
            Assert.AreEqual(GalePhase.Blowing, FarnkeshGaleRules.PhaseOf(0, 40d));
            Assert.AreEqual(GalePhase.Done, FarnkeshGaleRules.PhaseOf(0, 45d));
            Assert.AreEqual(GalePhase.Quiet, FarnkeshGaleRules.PhaseOf(1, 45d));
            Assert.AreEqual(-1, FarnkeshGaleRules.CurrentGust(50d), "a lull between gusts");
            Assert.AreEqual(1, FarnkeshGaleRules.CurrentGust(70d));
            Assert.AreEqual(25d, FarnkeshGaleRules.SecondsToGust(1, 70d), 1e-6);
            Assert.AreEqual(GalePhase.Quiet, FarnkeshGaleRules.PhaseOf(9, 70d));

            for (int i = 1; i < FarnkeshGaleRules.GustCount; i++)
            {
                Assert.Greater(FarnkeshGaleRules.GustAt[i] - FarnkeshGaleRules.GustWarnSeconds, FarnkeshGaleRules.GustAt[i - 1] + FarnkeshGaleRules.GustSeconds,
                    "one gust at a time");
            }

            Assert.Less(FarnkeshGaleRules.GustAt[FarnkeshGaleRules.GustCount - 1] + FarnkeshGaleRules.GustSeconds, FarnkeshGaleRules.Schedule.WindowMinutes * 60d,
                "the last gust blows inside the window");
            Assert.IsFalse(FarnkeshGaleRules.AllDone(264d));
            Assert.IsTrue(FarnkeshGaleRules.AllDone(265d));
        }

        [Test]
        public void Gale_RopeHoldsOnlyGrabbedWithTheHeroBeside_MaitheGrabsFromFarther_SwingThrowsNearby()
        {
            Vector2 mooring = OverworldLayouts.FarnkeshBridgeMoorings[0];
            Assert.IsTrue(FarnkeshGaleRules.CanGrab(mooring, mooring + new Vector2(2.9f, 0f), false));
            Assert.IsFalse(FarnkeshGaleRules.CanGrab(mooring, mooring + new Vector2(4f, 0f), false));
            Assert.IsTrue(FarnkeshGaleRules.CanGrab(mooring, mooring + new Vector2(4f, 0f), true), "Maithe hears the Living Wind");
            Assert.IsFalse(FarnkeshGaleRules.CanGrab(mooring, mooring + new Vector2(5.5f, 0f), true));

            Assert.IsTrue(FarnkeshGaleRules.Holds(true, mooring, mooring + new Vector2(0f, 5f)));
            Assert.IsFalse(FarnkeshGaleRules.Holds(true, mooring, mooring + new Vector2(0f, 7f)), "let go and ran off");
            Assert.IsFalse(FarnkeshGaleRules.Holds(false, mooring, mooring), "standing there is not holding");

            Assert.IsTrue(FarnkeshGaleRules.Thrown(false, mooring, mooring + new Vector2(10f, 0f)));
            Assert.IsFalse(FarnkeshGaleRules.Thrown(true, mooring, mooring), "a held bridge throws nobody");
            Assert.IsFalse(FarnkeshGaleRules.Thrown(false, mooring, mooring + new Vector2(20f, 0f)), "off the bridge");

            Assert.AreEqual(12f, FarnkeshGaleRules.Hit(100f, 100f), 1e-4f);
            Assert.AreEqual(5f, FarnkeshGaleRules.Hit(25f, 100f), 1e-4f, "never below 20%");
            Assert.AreEqual(0f, FarnkeshGaleRules.Hit(15f, 100f), 1e-4f);
        }

        [Test]
        public void Gale_PaidPerGustHeld_AllFiveAStone_OncePerWindow()
        {
            Assert.AreEqual(0, FarnkeshGaleRules.GoldFor(0));
            Assert.AreEqual(90, FarnkeshGaleRules.GoldFor(3));
            Assert.AreEqual(150, FarnkeshGaleRules.GoldFor(9), "clamped to five");
            Assert.AreEqual(0, FarnkeshGaleRules.StonesFor(4));
            Assert.AreEqual(1, FarnkeshGaleRules.StonesFor(5));
            Assert.IsTrue(FarnkeshGaleRules.IsPaid(FarnkeshGaleRules.PaidMark(7), 7));
            Assert.IsFalse(FarnkeshGaleRules.IsPaid(0L, 0), "0 means never");
            Assert.AreEqual(FarnkeshGaleRules.MooringOf(42, 3), CityEventTurns.SideOf(42, 3, FarnkeshGaleRules.MooringCount));
        }

        [Test]
        public void Gale_Moorings_OnTheRoads_Reachable_BeforeTheBellsStop()
        {
            Vector2[] moorings = OverworldLayouts.FarnkeshBridgeMoorings;
            Assert.AreEqual(FarnkeshGaleRules.MooringCount, moorings.Length);
            AssertSpots(OverworldLayouts.FarnkeshRegionId, OverworldLayouts.FarnkeshMapId, OverworldLayouts.FarnkeshOffset, moorings, FarnkeshGaleRules.HoldRadius);
            for (int i = 0; i < moorings.Length; i++)
            {
                for (int j = i + 1; j < moorings.Length; j++)
                {
                    float between = Vector2.Distance(moorings[i], moorings[j]);
                    Assert.Greater(between, FarnkeshGaleRules.SwingRadius * 2f, "a swinging bridge never throws a hero holding another");
                    Assert.Less(between / WalkSpeed, FarnkeshGaleRules.GustWarnSeconds, "time to run to the next mooring while the bells ring");
                }
            }
        }

        // ------------------------------------------------------------------
        // Noite Polar (59)
        // ------------------------------------------------------------------

        [Test]
        public void Polar_OnTheTwoDarkMoonDays_From10To15_WarnedAt9()
        {
            int[] polarDays = Enumerable.Range(1, 16).Where(PortoPolarNightRules.IsPolarDay).ToArray();
            CollectionAssert.AreEqual(new[] { 1, 8, 9, 16 }, polarDays, "moon 7 and 0 of every 8 days");
            int[] festaDays = Enumerable.Range(1, 16).Where(MingchengLanternFestaRules.IsFestaDay).ToArray();
            CollectionAssert.IsEmpty(polarDays.Intersect(festaDays), "the dark moon is never the bright one");
            Assert.IsTrue(polarDays.All(d => !MoonCanalRules.IsFrozen(d)), "the canals are open on the dark days");

            Assert.IsTrue(PortoPolarNightRules.IsOpen(8, 10f));
            Assert.IsTrue(PortoPolarNightRules.IsOpen(9, 14.9f));
            Assert.IsFalse(PortoPolarNightRules.IsOpen(9, 15f));
            Assert.IsFalse(PortoPolarNightRules.IsOpen(9, 9.9f));
            Assert.IsFalse(PortoPolarNightRules.IsOpen(5, 12f), "full moon");
            Assert.IsTrue(PortoPolarNightRules.IsWarning(1, 9.5f));
            Assert.IsFalse(PortoPolarNightRules.IsWarning(2, 9.5f));
            Assert.AreEqual(6, PortoPolarNightRules.DaysUntilPolar(2));
            Assert.AreEqual(0, PortoPolarNightRules.DaysUntilPolar(8));
            Assert.AreEqual(3, PortoPolarNightRules.DaysUntilPolar(13));
        }

        [Test]
        public void Polar_StartsNearAndOpenAndUnpaid_OneWaveAnHour_LateStartSkipsTheHoursGone()
        {
            Assert.IsTrue(PortoPolarNightRules.CanStart(8, 11f, 0L, 40f));
            Assert.IsFalse(PortoPolarNightRules.CanStart(8, 11f, 0L, PortoPolarNightRules.StartRange + 1f), "too far");
            Assert.IsFalse(PortoPolarNightRules.CanStart(8, 11f, PortoPolarNightRules.PaidMark(8), 40f), "paid today");
            Assert.IsTrue(PortoPolarNightRules.CanStart(9, 11f, PortoPolarNightRules.PaidMark(8), 40f), "yesterday's mark");
            Assert.IsFalse(PortoPolarNightRules.CanStart(8, 16f, 0L, 40f), "after the dawn");
            Assert.AreNotEqual(0L, PortoPolarNightRules.PaidMark(0));

            Assert.AreEqual(5, PortoPolarNightRules.WaveCount);
            Assert.AreEqual(-1, PortoPolarNightRules.WaveDue(9.5f));
            Assert.AreEqual(0, PortoPolarNightRules.WaveDue(10f));
            Assert.AreEqual(2, PortoPolarNightRules.WaveDue(12.4f));
            Assert.AreEqual(4, PortoPolarNightRules.WaveDue(14.9f));
            Assert.AreEqual(4, PortoPolarNightRules.WaveDue(20f), "capped at the last wave");
            Assert.AreEqual(0, PortoPolarNightRules.FirstWave(10.2f));
            Assert.AreEqual(3, PortoPolarNightRules.FirstWave(13.5f));

            Assert.IsTrue(PortoPolarNightRules.ShouldSpawnWave(0, 10f, 0));
            Assert.IsFalse(PortoPolarNightRules.ShouldSpawnWave(1, 10.9f, 0), "the next hour's wave waits for its hour");
            Assert.IsTrue(PortoPolarNightRules.ShouldSpawnWave(1, 11f, 2));
            Assert.IsFalse(PortoPolarNightRules.ShouldSpawnWave(4, 14f, 5), "never past the cap");
            Assert.IsFalse(PortoPolarNightRules.ShouldSpawnWave(5, 14f, 0), "all sent");
            Assert.AreEqual(4, PortoPolarNightRules.WaveSize(9), "clamped");
            Assert.LessOrEqual(PortoPolarNightRules.WaveSizes.Max(), PortoPolarNightRules.MaxAlive);
        }

        [Test]
        public void Polar_DarkDrinksTheLight_CappedCount_RelightOnceWithNoDarkNear_LavineFromFarther()
        {
            Assert.AreEqual(84f, PortoPolarNightRules.Drain(100f, 2, 1f), 1e-4f);
            Assert.AreEqual(100f - PortoPolarNightRules.MaxDrinkers * PortoPolarNightRules.DrainPerSecond, PortoPolarNightRules.Drain(100f, 9, 1f), 1e-4f, "at most 3 counted");
            Assert.AreEqual(0f, PortoPolarNightRules.Drain(10f, 3, 5f), 1e-4f, "never below zero");
            Assert.AreEqual(50f, PortoPolarNightRules.Drain(50f, 0, 5f), 1e-4f, "no Dark, no drinking");
            Assert.IsTrue(PortoPolarNightRules.IsOut(0f));
            Assert.IsFalse(PortoPolarNightRules.IsOut(0.5f));

            Vector2 lantern = OverworldLayouts.PortoPolarLanterns[2];
            Assert.IsTrue(PortoPolarNightRules.Drinking(lantern, lantern + new Vector2(3.9f, 0f)));
            Assert.IsFalse(PortoPolarNightRules.Drinking(lantern, lantern + new Vector2(4.1f, 0f)));

            Assert.IsTrue(PortoPolarNightRules.CanRelight(0f, false, 0, 2.5f, false));
            Assert.IsFalse(PortoPolarNightRules.CanRelight(30f, false, 0, 2.5f, false), "still lit");
            Assert.IsFalse(PortoPolarNightRules.CanRelight(0f, true, 0, 2.5f, false), "the oil is spent");
            Assert.IsFalse(PortoPolarNightRules.CanRelight(0f, false, 1, 2.5f, false), "the Dark is too close");
            Assert.IsFalse(PortoPolarNightRules.CanRelight(0f, false, 0, 5f, false));
            Assert.IsTrue(PortoPolarNightRules.CanRelight(0f, false, 0, 5f, true), "Lavine, the Moon Boatwoman");
            Assert.Greater(PortoPolarNightRules.RelightClearRadius, PortoPolarNightRules.DrinkReach, "a Dark that drinks always blocks the relight");
        }

        [Test]
        public void Polar_DawnAtFifteenOrANewDay_PaidPerLanternLit()
        {
            Assert.AreEqual(PolarNightOutcome.Running, PortoPolarNightRules.Resolve(8, 12f, 8));
            Assert.AreEqual(PolarNightOutcome.Dawn, PortoPolarNightRules.Resolve(8, 15f, 8));
            Assert.AreEqual(PolarNightOutcome.Dawn, PortoPolarNightRules.Resolve(9, 11f, 8), "slept through: a new day");

            Assert.AreEqual(2, PortoPolarNightRules.LitCount(new[] { 100f, 0f, 3f }));
            Assert.AreEqual(0, PortoPolarNightRules.LitCount(null));
            Assert.AreEqual(90, PortoPolarNightRules.GoldFor(2));
            Assert.AreEqual(135, PortoPolarNightRules.GoldFor(7), "clamped to three");
            Assert.AreEqual(0, PortoPolarNightRules.StonesFor(2));
            Assert.AreEqual(1, PortoPolarNightRules.StonesFor(3));
        }

        [Test]
        public void Polar_Lanterns_OnTheRoads_OutsideTheSafeZone_TheDarkRisesFartherOut()
        {
            Vector2[] lanterns = OverworldLayouts.PortoPolarLanterns;
            Vector2[] rises = OverworldLayouts.PortoPolarDarkRise;
            Assert.AreEqual(PortoPolarNightRules.LanternCount, lanterns.Length);
            Assert.AreEqual(lanterns.Length, rises.Length);
            Vector2 town = OverworldLayouts.PortoLuaBrancaOffset;
            AssertSpots(OverworldLayouts.MarGeladoRegionId, OverworldLayouts.PortoLuaBrancaMapId, town, lanterns, PortoPolarNightRules.RelightClearRadius);
            AssertSpots(OverworldLayouts.MarGeladoRegionId, OverworldLayouts.PortoLuaBrancaMapId, town, rises, 3f);
            Rect safe = Rect.MinMaxRect(town.x - 26f, town.y - 26f, town.x + 26f, town.y + 26f);
            for (int i = 0; i < lanterns.Length; i++)
            {
                Assert.IsFalse(safe.Contains(lanterns[i]), "the Dark is fought outside the safe zone");
                Assert.Greater(Vector2.Distance(rises[i], town), Vector2.Distance(lanterns[i], town) + 20f, "the Dark comes from outside");
                Assert.Less(Vector2.Distance(lanterns[i], town), PortoPolarNightRules.StartRange - 30f, "every lantern is well inside the defence");
                Assert.AreEqual(i, MingchengLanternFestaRules.NearestWithin(lanterns, lanterns[i], PortoPolarNightRules.MoonBoatwomanReach), "one lantern at a time");
                Assert.IsFalse(PortoPolarNightRules.Drinking(lanterns[i], rises[i]), "it has to walk to drink");
            }
        }

        // ------------------------------------------------------------------
        // Grande Tremor (65)
        // ------------------------------------------------------------------

        [Test]
        public void Tremor_OpensEveryHourAtFive_ClearOfTheSluicesTremors()
        {
            Assert.IsTrue(HinokuraGreatTremorRules.Schedule.IsValid);
            Assert.IsTrue(HinokuraGreatTremorRules.WindowAt(Local(9, 5)).IsOpen);
            Assert.IsFalse(HinokuraGreatTremorRules.WindowAt(Local(9, 10)).IsOpen);
            Assert.IsTrue(HinokuraGreatTremorRules.IsWarning(HinokuraGreatTremorRules.WindowAt(Local(9, 4, 30))));

            // Every second of the window: the sluices are free and no small Tremor is warned.
            RealTimeWindow window = HinokuraGreatTremorRules.WindowAt(Local(9, 5));
            long start = new DateTimeOffset(window.StartUtc, TimeSpan.Zero).ToUnixTimeSeconds();
            for (long s = 0; s < HinokuraGreatTremorRules.Schedule.WindowMinutes * 60L; s++)
            {
                Assert.IsFalse(HinokuraForgeRules.IsJammed(start + s), $"jammed {s} s in");
                Assert.IsFalse(HinokuraForgeRules.IsTremorWarning(start + s), $"small Tremor warned {s} s in");
            }
        }

        [Test]
        public void Tremor_FourSurges_CalledInTurn_ArriveAndSpillBeforeTheNext()
        {
            Assert.AreEqual(HinokuraGreatTremorRules.SurgeCount, HinokuraGreatTremorRules.SurgeAt.Length);
            Assert.AreEqual(TremorSurgePhase.Quiet, HinokuraGreatTremorRules.PhaseOf(0, 10d));
            Assert.AreEqual(TremorSurgePhase.Warning, HinokuraGreatTremorRules.PhaseOf(0, 15d));
            Assert.AreEqual(TremorSurgePhase.Surging, HinokuraGreatTremorRules.PhaseOf(0, 43d));
            Assert.AreEqual(0.5f, HinokuraGreatTremorRules.Progress(0, 43d), 1e-5f);
            Assert.IsFalse(HinokuraGreatTremorRules.Arrived(0, 45.9d));
            Assert.IsTrue(HinokuraGreatTremorRules.Arrived(0, 46d));
            Assert.AreEqual(TremorSurgePhase.Done, HinokuraGreatTremorRules.PhaseOf(0, 46d));
            Assert.AreEqual(-1, HinokuraGreatTremorRules.CurrentSurge(60d));
            Assert.AreEqual(1, HinokuraGreatTremorRules.CurrentSurge(80d));
            Assert.AreEqual(20d, HinokuraGreatTremorRules.SecondsToSurge(1, 80d), 1e-6);

            Assert.IsTrue(HinokuraGreatTremorRules.SpillBurning(0, 50d));
            Assert.IsFalse(HinokuraGreatTremorRules.SpillBurning(0, 86d));
            Assert.IsFalse(HinokuraGreatTremorRules.SpillBurning(0, 40d), "no spill before the surge arrives");
            for (int i = 1; i < HinokuraGreatTremorRules.SurgeCount; i++)
            {
                double previousSpillEnds = HinokuraGreatTremorRules.SurgeAt[i - 1] + HinokuraGreatTremorRules.SurgeSeconds + HinokuraGreatTremorRules.SpillSeconds;
                Assert.Less(previousSpillEnds, HinokuraGreatTremorRules.SurgeAt[i], "a spill is gone before the next surge");
                Assert.Greater(HinokuraGreatTremorRules.SurgeAt[i] - HinokuraGreatTremorRules.SurgeWarnSeconds,
                    HinokuraGreatTremorRules.SurgeAt[i - 1] + HinokuraGreatTremorRules.SurgeSeconds, "one surge called at a time");
            }

            double lastSpill = HinokuraGreatTremorRules.SurgeAt[HinokuraGreatTremorRules.SurgeCount - 1] + HinokuraGreatTremorRules.SurgeSeconds + HinokuraGreatTremorRules.SpillSeconds;
            Assert.LessOrEqual(lastSpill, HinokuraGreatTremorRules.Schedule.WindowMinutes * 60d, "the last spill cools inside the window");
            Assert.IsTrue(HinokuraGreatTremorRules.AllDone(226d));
            Assert.IsFalse(HinokuraGreatTremorRules.AllDone(225d));

            for (long index = 0; index < 30; index++)
            {
                for (int s = 1; s < HinokuraGreatTremorRules.SurgeCount; s++)
                {
                    Assert.AreNotEqual(HinokuraGreatTremorRules.ChannelOf(index, s - 1), HinokuraGreatTremorRules.ChannelOf(index, s), "the channels change");
                }
            }
        }

        [Test]
        public void Tremor_IcaroTurnsFromFarther_SpillBurnsNeverPastTheFloor_PaidPerForge()
        {
            HinokuraLavaDiverter diverter = OverworldLayouts.HinokuraLavaDiverters[1];
            Assert.IsTrue(HinokuraGreatTremorRules.CanTurn(diverter.Stone, diverter.Stone + new Vector2(0f, 3.4f), false));
            Assert.IsFalse(HinokuraGreatTremorRules.CanTurn(diverter.Stone, diverter.Stone + new Vector2(0f, 5f), false));
            Assert.IsTrue(HinokuraGreatTremorRules.CanTurn(diverter.Stone, diverter.Stone + new Vector2(0f, 5f), true), "Icaro and the Living Fire");
            Assert.IsTrue(HinokuraGreatTremorRules.InSpill(diverter.Spill, diverter.Spill + new Vector2(4.9f, 0f)));
            Assert.IsFalse(HinokuraGreatTremorRules.InSpill(diverter.Spill, diverter.Stone + (diverter.Stone - diverter.Spill)), "beyond the stone, dry road");

            Assert.AreEqual(6f, HinokuraGreatTremorRules.Burn(100f, 100f), 1e-4f);
            Assert.AreEqual(2f, HinokuraGreatTremorRules.Burn(22f, 100f), 1e-4f, "never below 20%");
            Assert.AreEqual(0f, HinokuraGreatTremorRules.Burn(20f, 100f), 1e-4f);

            Assert.AreEqual(120, HinokuraGreatTremorRules.GoldFor(3));
            Assert.AreEqual(160, HinokuraGreatTremorRules.GoldFor(8), "clamped to four");
            Assert.AreEqual(0, HinokuraGreatTremorRules.StonesFor(3));
            Assert.AreEqual(1, HinokuraGreatTremorRules.StonesFor(4));
            Assert.IsTrue(HinokuraGreatTremorRules.IsPaid(HinokuraGreatTremorRules.PaidMark(3), 3));
            Assert.AreNotEqual(FarnkeshGaleRules.PaidKey, HinokuraGreatTremorRules.PaidKey);
        }

        [Test]
        public void Tremor_Diverters_OnTheRoads_SpillOutsideTheSafeZone_MouthOutward_Reachable()
        {
            HinokuraLavaDiverter[] diverters = OverworldLayouts.HinokuraLavaDiverters;
            Assert.AreEqual(HinokuraGreatTremorRules.DiverterCount, diverters.Length);
            Vector2 town = OverworldLayouts.HinokuraOffset;
            string region = OverworldLayouts.HinokuraRegionId;
            AssertSpots(region, OverworldLayouts.HinokuraMapId, town, diverters.Select(d => d.Stone).ToArray(), HinokuraGreatTremorRules.LivingFireReach);
            AssertSpots(region, OverworldLayouts.HinokuraMapId, town, diverters.Select(d => d.Spill).ToArray(), HinokuraGreatTremorRules.SpillRadius);
            AssertSpots(region, OverworldLayouts.HinokuraMapId, town, diverters.Select(d => d.Mouth).ToArray(), 3f);
            Rect safe = Rect.MinMaxRect(town.x - 26f, town.y - 26f, town.x + 26f, town.y + 26f);
            OverworldLayout world = OverworldLayouts.Get(region);
            for (int i = 0; i < diverters.Length; i++)
            {
                HinokuraLavaDiverter d = diverters[i];
                Assert.Greater(Vector2.Distance(d.Mouth, town), Vector2.Distance(d.Stone, town) + 15f, "the lava comes from outside");
                Assert.Less(Vector2.Distance(d.Spill, town), Vector2.Distance(d.Stone, town), "it spills towards the town");
                Assert.IsFalse(safe.Overlaps(new Rect(d.Spill.x - HinokuraGreatTremorRules.SpillRadius, d.Spill.y - HinokuraGreatTremorRules.SpillRadius,
                    HinokuraGreatTremorRules.SpillRadius * 2f, HinokuraGreatTremorRules.SpillRadius * 2f)), "nothing burns in the safe zone");
                Assert.Greater(Vector2.Distance(d.Spill, d.Stone), HinokuraGreatTremorRules.TurnReach, "turning the stone is never done standing in the spill");
                foreach (MapRiver river in world.World.Rivers.Where(OverworldLayouts.IsLava))
                {
                    Assert.Greater(MapLayoutRules.DistanceToPolyline(d.Stone, river.Points), 100f, "the gates are far from the real lava channels");
                }

                for (int j = i + 1; j < diverters.Length; j++)
                {
                    Assert.Less(Vector2.Distance(d.Stone, diverters[j].Stone) / WalkSpeed, HinokuraGreatTremorRules.SurgeWarnSeconds, "time to run to the called stone");
                }
            }
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
        public void Texts_UniqueKeys_BothLanguages_EverySideAndCousinLine_NoClashWithTheFirstEvents()
        {
            var keys = new HashSet<string>();
            foreach (var entry in LateCityEventsTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate key " + entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                StringAssert.DoesNotContain("Dona ", entry.PtBR);
                StringAssert.DoesNotContain("Seu ", entry.PtBR);
            }

            foreach (var entry in CityEventsTexts.Entries)
            {
                Assert.IsFalse(keys.Contains(entry.Key), "shared with CityEventsTexts: " + entry.Key);
            }

            for (int side = 0; side < 3; side++)
            {
                Assert.IsTrue(keys.Contains(LateCityEventsTexts.SideKey(side)), "side " + side);
            }

            CharacterId?[] heroes = { null, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine };
            foreach (string kind in new[] { "gale", "polar", "tremor" })
            {
                var lines = new HashSet<string>();
                foreach (CharacterId? hero in heroes)
                {
                    string key = LateCityEventsTexts.LineFor(kind, hero);
                    Assert.IsTrue(keys.Contains(key), key);
                    lines.Add(key);
                }

                Assert.AreEqual(5, lines.Count, kind + ": one line per cousin");
            }

            Assert.AreEqual("A lanterna oeste apagou!", LateCityEventsTexts.Format(null, LateCityEventsTexts.PolarOut, LateCityEventsTexts.Get(null, LateCityEventsTexts.SideWest)));
        }
    }
}
