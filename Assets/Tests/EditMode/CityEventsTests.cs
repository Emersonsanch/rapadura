using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The three capital events (Docs/Mapas100.md): the Maré de Lodo of Jhilpur (31, <see cref="JhilpurMudTideRules"/>), the Avalanche of
    /// Sangkent (36, <see cref="SangkentAvalancheRules"/>) and the Festa das Lanternas of Mingcheng (41, <see cref="MingchengLanternFestaRules"/>):
    /// pure rules, the places the layouts declare for them and their texts.
    /// </summary>
    public class CityEventsTests
    {
        /// <summary>Server-local (Brasília, UTC−3) instant of a day at hh:mm:ss.</summary>
        private static DateTime Local(int hour, int minute, int second = 0) => new DateTime(2026, 11, 12, hour, minute, second, DateTimeKind.Utc).AddHours(3);

        // ------------------------------------------------------------------
        // Maré de Lodo (31)
        // ------------------------------------------------------------------

        [Test]
        public void Tide_OpensEveryHourAtTwenty_ForSixMinutes_WarnedTheMinuteBefore()
        {
            Assert.IsTrue(JhilpurMudTideRules.Schedule.IsValid);
            Assert.IsTrue(JhilpurMudTideRules.WindowAt(Local(14, 20)).IsOpen);
            Assert.IsTrue(JhilpurMudTideRules.WindowAt(Local(14, 25, 59)).IsOpen);
            Assert.IsFalse(JhilpurMudTideRules.WindowAt(Local(14, 26)).IsOpen);
            Assert.IsFalse(JhilpurMudTideRules.WindowAt(Local(14, 19, 59)).IsOpen);

            Assert.IsTrue(JhilpurMudTideRules.IsWarning(JhilpurMudTideRules.WindowAt(Local(14, 19, 10))));
            Assert.IsFalse(JhilpurMudTideRules.IsWarning(JhilpurMudTideRules.WindowAt(Local(14, 18, 50))));
            Assert.IsFalse(JhilpurMudTideRules.IsWarning(JhilpurMudTideRules.WindowAt(Local(14, 21))), "open is not a warning");

            long a = JhilpurMudTideRules.WindowAt(Local(14, 21)).Index;
            long b = JhilpurMudTideRules.WindowAt(Local(15, 21)).Index;
            Assert.AreEqual(a + 1, b, "one tide an hour");
        }

        [Test]
        public void Tide_StartsNearAndOpenAndUnpaid_PaidOncePerWindow()
        {
            RealTimeWindow open = JhilpurMudTideRules.WindowAt(Local(9, 21));
            Assert.IsTrue(JhilpurMudTideRules.CanStart(open, 0L, 40f, false));
            Assert.IsFalse(JhilpurMudTideRules.CanStart(open, 0L, JhilpurMudTideRules.StartRange + 1f, false), "too far");
            Assert.IsFalse(JhilpurMudTideRules.CanStart(open, 0L, 40f, true), "resting after a silted root");
            Assert.IsFalse(JhilpurMudTideRules.CanStart(open, JhilpurMudTideRules.PaidMark(open.Index), 40f, false), "already paid");
            Assert.IsTrue(JhilpurMudTideRules.CanStart(open, JhilpurMudTideRules.PaidMark(open.Index - 1), 40f, false), "last hour's mark");
            Assert.IsFalse(JhilpurMudTideRules.CanStart(JhilpurMudTideRules.WindowAt(Local(9, 40)), 0L, 40f, false), "closed");
            Assert.AreNotEqual(0L, JhilpurMudTideRules.PaidMark(0L), "0 means never");
        }

        [Test]
        public void Tide_FourWaves_NextWhenFieldClearOrInterval_CappedAlive()
        {
            Assert.AreEqual(4, JhilpurMudTideRules.WaveCount);
            Assert.AreEqual(18, JhilpurMudTideRules.TotalFilhos);
            Assert.IsTrue(JhilpurMudTideRules.ShouldSpawnWave(0, 0, 0f));
            Assert.IsTrue(JhilpurMudTideRules.ShouldSpawnWave(1, 1, 3f), "one left: next wave");
            Assert.IsFalse(JhilpurMudTideRules.ShouldSpawnWave(1, 3, 10f));
            Assert.IsTrue(JhilpurMudTideRules.ShouldSpawnWave(1, 3, JhilpurMudTideRules.WaveIntervalSeconds));
            Assert.IsFalse(JhilpurMudTideRules.ShouldSpawnWave(2, JhilpurMudTideRules.MaxAlive, 99f), "never past the cap");
            Assert.IsFalse(JhilpurMudTideRules.ShouldSpawnWave(4, 0, 99f), "all sent");
            Assert.AreEqual(6, JhilpurMudTideRules.WaveSize(9), "clamped");
        }

        [Test]
        public void Tide_SiltRisesWithFilhosAtTheGhats_CappedCount_ResolvesWonOrSilted()
        {
            Assert.AreEqual(5f, JhilpurMudTideRules.AdvanceSilt(0f, 2, 1f), 1e-4f);
            Assert.AreEqual(JhilpurMudTideRules.MaxSilters * JhilpurMudTideRules.SiltPerSecond, JhilpurMudTideRules.AdvanceSilt(0f, 30, 1f), 1e-4f, "at most 6 counted");
            Assert.AreEqual(JhilpurMudTideRules.SiltMax, JhilpurMudTideRules.AdvanceSilt(90f, 6, 10f), 1e-4f, "capped");
            Assert.AreEqual(10f, JhilpurMudTideRules.AdvanceSilt(10f, 0, 5f), 1e-4f, "no Filho, no silt");

            Assert.AreEqual(MudTideOutcome.Running, JhilpurMudTideRules.Resolve(2, 0, 10f));
            Assert.AreEqual(MudTideOutcome.Running, JhilpurMudTideRules.Resolve(4, 1, 10f));
            Assert.AreEqual(MudTideOutcome.Won, JhilpurMudTideRules.Resolve(4, 0, 99f));
            Assert.AreEqual(MudTideOutcome.Silted, JhilpurMudTideRules.Resolve(4, 0, 100f), "silted wins over the last kill");

            Assert.AreEqual(2, JhilpurMudTideRules.StonesFor(10f));
            Assert.AreEqual(1, JhilpurMudTideRules.StonesFor(JhilpurMudTideRules.GoodSilt));
            Assert.AreEqual(40, JhilpurMudTideRules.Percent(40f));
        }

        [Test]
        public void Tide_Places_ShoreBelowTheGhats_FeetOutsideTheSafeZone_InTheBand()
        {
            Vector2[] shore = OverworldLayouts.JhilpurTideShore;
            Vector2[] feet = OverworldLayouts.JhilpurGhatFoot;
            Rect band = OverworldLayouts.JhilpurGhatBand;
            Rect safe = Rect.MinMaxRect(-26f, -26f, 26f, 26f);
            Assert.AreEqual(shore.Length, feet.Length);
            for (int i = 0; i < shore.Length; i++)
            {
                Assert.Greater(shore[i].y, -65f, "on land, above the lake's water");
                Assert.Less(shore[i].y, band.yMin, "below the ghats' band");
                Assert.IsFalse(safe.Contains(feet[i]), "the Filhos are fought outside the safe zone");
                Assert.IsTrue(JhilpurMudTideRules.Silting(band, feet[i]), "a Filho at its foot silts the root");
                Assert.IsFalse(JhilpurMudTideRules.Silting(band, shore[i]));
            }
        }

        // ------------------------------------------------------------------
        // Avalanche (36)
        // ------------------------------------------------------------------

        [Test]
        public void Snow_OpensEveryHourAtForty_ForFiveMinutes_ThreeLanesInTurn()
        {
            Assert.IsTrue(SangkentAvalancheRules.Schedule.IsValid);
            RealTimeWindow open = SangkentAvalancheRules.WindowAt(Local(20, 41));
            Assert.IsTrue(open.IsOpen);
            Assert.AreEqual(60d, SangkentAvalancheRules.SecondsInto(open, Local(20, 41)), 1e-6);
            Assert.IsFalse(SangkentAvalancheRules.WindowAt(Local(20, 45)).IsOpen);
            Assert.IsTrue(SangkentAvalancheRules.IsWarning(SangkentAvalancheRules.WindowAt(Local(20, 39, 30))));

            Assert.AreEqual(SangkentLanePhase.Quiet, SangkentAvalancheRules.PhaseOf(0, 10d));
            Assert.AreEqual(SangkentLanePhase.Warning, SangkentAvalancheRules.PhaseOf(0, 20d));
            Assert.AreEqual(SangkentLanePhase.Sliding, SangkentAvalancheRules.PhaseOf(0, 60d));
            Assert.AreEqual(SangkentLanePhase.Done, SangkentAvalancheRules.PhaseOf(0, 68d));
            Assert.AreEqual(SangkentLanePhase.Quiet, SangkentAvalancheRules.PhaseOf(1, 68d));
            Assert.AreEqual(SangkentLanePhase.Sliding, SangkentAvalancheRules.PhaseOf(2, 225d));
            Assert.AreEqual(0.5f, SangkentAvalancheRules.Progress(1, 144d), 1e-5f);
            Assert.AreEqual(30d, SangkentAvalancheRules.SecondsToSlide(1, 110d), 1e-6);
            Assert.IsFalse(SangkentAvalancheRules.AllDone(227d));
            Assert.IsTrue(SangkentAvalancheRules.AllDone(228d));
            Assert.Less(SangkentAvalancheRules.SlideAt[SangkentAvalancheRules.LaneCount - 1] + SangkentAvalancheRules.SlideSeconds,
                SangkentAvalancheRules.Schedule.WindowMinutes * 60d, "the last lane slides inside the window");
        }

        [Test]
        public void Snow_FenceHoldsOnlyPlantedWithTheHeroBeside_FrontStopsAtIt()
        {
            SangkentTerraceLane lane = OverworldLayouts.SangkentTerraceLanes[1];
            Vector2 fence = lane.Fence;
            Assert.IsTrue(SangkentAvalancheRules.CanPlant(fence, fence + new Vector2(3f, 0f)));
            Assert.IsFalse(SangkentAvalancheRules.CanPlant(fence, fence + new Vector2(4f, 0f)));
            Assert.IsTrue(SangkentAvalancheRules.Holds(true, fence, fence + new Vector2(0f, -10f)));
            Assert.IsFalse(SangkentAvalancheRules.Holds(false, fence, fence), "unplanted stakes don't hold");
            Assert.IsFalse(SangkentAvalancheRules.Holds(true, fence, fence + new Vector2(0f, 20f)), "nobody holding them");

            float stop = SangkentAvalancheRules.StopAlong(lane, true);
            Assert.Greater(stop, 0.8f);
            Assert.Less(stop, 1f);
            Assert.AreEqual(1f, SangkentAvalancheRules.StopAlong(lane, false));

            Vector2 above = Vector2.Lerp(lane.Lane.Top, lane.Lane.Bottom, 0.5f);
            Vector2 below = lane.Lane.Bottom + new Vector2(0f, 4f);
            Assert.IsTrue(SangkentAvalancheRules.Sweeps(lane, above, 0.4f, 0.6f, true), "above the fence the front still hits");
            Assert.IsFalse(SangkentAvalancheRules.Sweeps(lane, fence, stop - 0.05f, stop, true), "at a held fence: behind the stakes");
            Assert.IsFalse(SangkentAvalancheRules.Sweeps(lane, below, stop - 0.05f, stop, true), "below a held fence: untouched");
            Assert.IsTrue(SangkentAvalancheRules.Sweeps(lane, below, 0.9f, 1f, false), "below a fence that gave way: buried with the terrace");
            Assert.IsFalse(SangkentAvalancheRules.Sweeps(lane, above + new Vector2(20f, 0f), 0.4f, 0.6f, false), "out of the lane");
        }

        [Test]
        public void Snow_HitNeverPastTheFloor_PayPerTerrace()
        {
            Assert.AreEqual(22f, SangkentAvalancheRules.Hit(100f, 100f), 1e-4f);
            Assert.AreEqual(5f, SangkentAvalancheRules.Hit(25f, 100f), 1e-4f, "never below 20%");
            Assert.AreEqual(0f, SangkentAvalancheRules.Hit(15f, 100f), 1e-4f);

            Assert.AreEqual(0, SangkentAvalancheRules.GoldFor(0));
            Assert.AreEqual(100, SangkentAvalancheRules.GoldFor(2));
            Assert.AreEqual(150, SangkentAvalancheRules.GoldFor(9), "clamped to three");
            Assert.AreEqual(0, SangkentAvalancheRules.StonesFor(2));
            Assert.AreEqual(1, SangkentAvalancheRules.StonesFor(3));
            Assert.IsTrue(SangkentAvalancheRules.IsPaid(SangkentAvalancheRules.PaidMark(7), 7));
            Assert.IsFalse(SangkentAvalancheRules.IsPaid(0L, 0));
        }

        [Test]
        public void Snow_Lanes_AboveTheTown_OffTheGateRingAndTheChutes_FencesApartAndReachable()
        {
            SangkentTerraceLane[] lanes = OverworldLayouts.SangkentTerraceLanes;
            Assert.AreEqual(SangkentAvalancheRules.LaneCount, lanes.Length);
            Vector2 gateRing = new Vector2(-80f, 80f);
            for (int i = 0; i < lanes.Length; i++)
            {
                SangkentTerraceLane lane = lanes[i];
                Assert.IsTrue(TetoAvalancheRules.InLane(lane.Lane, lane.Fence), "the fence stands across its lane");
                Assert.Greater(lane.Lane.Bottom.y, 30f, "the foot stays off the town's flat ground");
                Assert.Greater(lane.Lane.Top.y, lane.Lane.Bottom.y, "the snow comes down to the town");
                Assert.Greater(TetoAvalancheRules.DistanceToLane(lane.Lane, gateRing), lane.Lane.Width * 0.5f + 5f, "clear of the island gate");
                foreach (TetoChuteLane chute in OverworldLayouts.ChuteLanes)
                {
                    Assert.Greater(Vector2.Distance(lane.Lane.Top, chute.Bottom), 200f, "far from the chutes of map 37");
                }

                if (i > 0)
                {
                    float between = Vector2.Distance(lanes[i - 1].Fence, lane.Fence);
                    Assert.Greater(between, lane.Lane.Width, "lanes do not overlap");
                    Assert.Less(between / 5.5f, SangkentAvalancheRules.SlideAt[i] - SangkentAvalancheRules.SlideAt[i - 1], "time to walk to the next fence");
                }
            }
        }

        // ------------------------------------------------------------------
        // Festa das Lanternas (41)
        // ------------------------------------------------------------------

        [Test]
        public void Festa_OnTheThreeBrightNights_From19To1_WarnedAt18()
        {
            int[] festaDays = Enumerable.Range(1, 16).Where(MingchengLanternFestaRules.IsFestaDay).ToArray();
            CollectionAssert.AreEqual(new[] { 4, 5, 6, 12, 13, 14 }, festaDays, "moon 3–5 of every 8 days");

            Assert.IsTrue(MingchengLanternFestaRules.IsOpen(4, 19f, out int a));
            Assert.AreEqual(4, a);
            Assert.IsTrue(MingchengLanternFestaRules.IsOpen(7, 0.5f, out int b), "past midnight it is still day 6's festa");
            Assert.AreEqual(6, b);
            Assert.IsFalse(MingchengLanternFestaRules.IsOpen(7, 1f));
            Assert.IsFalse(MingchengLanternFestaRules.IsOpen(7, 19f));
            Assert.IsFalse(MingchengLanternFestaRules.IsOpen(4, 18.9f));
            Assert.IsFalse(MingchengLanternFestaRules.IsOpen(4, 0.5f), "day 3 had no festa");

            Assert.IsTrue(MingchengLanternFestaRules.IsWarning(5, 18.5f));
            Assert.IsFalse(MingchengLanternFestaRules.IsWarning(3, 18.5f));
            Assert.AreEqual(3, MingchengLanternFestaRules.DaysUntilFesta(1));
            Assert.AreEqual(0, MingchengLanternFestaRules.DaysUntilFesta(5));
            Assert.AreEqual(5, MingchengLanternFestaRules.DaysUntilFesta(7));
        }

        [Test]
        public void Festa_FiveDifferentRiddlesANight_TwoPerAnswer_SameForEveryone()
        {
            for (int day = 1; day < 40; day++)
            {
                var riddles = new HashSet<int>();
                for (int lantern = 0; lantern < MingchengLanternFestaRules.LanternCount; lantern++)
                {
                    int riddle = MingchengLanternFestaRules.RiddleAt(day, lantern);
                    Assert.That(riddle, Is.InRange(0, MingchengLanternFestaRules.RiddleCount - 1));
                    riddles.Add(riddle);
                }

                Assert.AreEqual(MingchengLanternFestaRules.LanternCount, riddles.Count, $"day {day}");
            }

            Assert.AreNotEqual(MingchengLanternFestaRules.RiddleAt(4, 0), MingchengLanternFestaRules.RiddleAt(5, 0), "a new night, new riddles");
            Assert.AreEqual(MingchengLanternFestaRules.RiddleCount, MingchengLanternFestaRules.RiddleAnswers.Length);
            foreach (LanternAnswer answer in Enum.GetValues(typeof(LanternAnswer)))
            {
                Assert.AreEqual(2, MingchengLanternFestaRules.RiddleAnswers.Count(a => a == answer), answer.ToString());
            }

            Assert.IsTrue(MingchengLanternFestaRules.IsRight(2, LanternAnswer.Carp));
            Assert.IsFalse(MingchengLanternFestaRules.IsRight(2, LanternAnswer.Bell));
        }

        [Test]
        public void Festa_SolvedMarksBelongToOneNight_BonusOncePerNight()
        {
            long solved = 0L;
            for (int i = 0; i < MingchengLanternFestaRules.LanternCount; i++)
            {
                Assert.IsFalse(MingchengLanternFestaRules.AllSolved(solved));
                solved = MingchengLanternFestaRules.Solve(solved, i);
                Assert.IsTrue(MingchengLanternFestaRules.IsSolved(solved, i));
                Assert.AreEqual(i + 1, MingchengLanternFestaRules.SolvedCount(solved));
            }

            Assert.AreEqual(solved, MingchengLanternFestaRules.Solve(solved, 2), "solving twice changes nothing");
            Assert.AreEqual(solved, MingchengLanternFestaRules.Solve(solved, 9), "no such lantern");
            Assert.IsTrue(MingchengLanternFestaRules.AllSolved(solved));
            Assert.IsTrue(MingchengLanternFestaRules.BonusDue(solved, 0L, 12));
            Assert.IsFalse(MingchengLanternFestaRules.BonusDue(solved, 12L, 12), "paid tonight");
            Assert.IsTrue(MingchengLanternFestaRules.BonusDue(solved, 4L, 12), "last festa's bonus");

            Assert.AreEqual(solved, MingchengLanternFestaRules.SolvedTonight(12L, solved, 12));
            Assert.AreEqual(0L, MingchengLanternFestaRules.SolvedTonight(4L, solved, 12), "last festa's lanterns are dark again");
        }

        [Test]
        public void Festa_Places_AnswersApartOnThePlaza_RiddlesOnTheStreetsAndRoads()
        {
            Vector2[] answers = OverworldLayouts.MingchengAnswerLanterns;
            Vector2[] riddles = OverworldLayouts.MingchengRiddleLanterns;
            Assert.AreEqual(Enum.GetValues(typeof(LanternAnswer)).Length, answers.Length);
            Assert.AreEqual(MingchengLanternFestaRules.LanternCount, riddles.Length);
            for (int i = 0; i < answers.Length; i++)
            {
                Assert.Less(answers[i].magnitude, 7f, "on the plaza");
                Assert.Less(answers[i].y, 0f, "south of the bell-and-drum tower");
                Assert.AreEqual(i, MingchengLanternFestaRules.NearestWithin(answers, answers[i], MingchengLanternFestaRules.AnswerReach));
                for (int j = i + 1; j < answers.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(answers[i], answers[j]), MingchengLanternFestaRules.AnswerReach * 1.6f, "one answer at a time");
                }
            }

            foreach (Vector2 riddle in riddles)
            {
                Assert.IsTrue(Mathf.Abs(riddle.x) < 0.01f || Mathf.Abs(riddle.y) < 0.01f, "on a street or road axis (W, E, S)");
                Assert.LessOrEqual(riddle.y, 0f, "no street to the north");
                Assert.AreEqual(-1, MingchengLanternFestaRules.NearestWithin(answers, riddle, MingchengLanternFestaRules.LanternReach), "away from the answers");
            }

            Assert.AreEqual(-1, MingchengLanternFestaRules.NearestWithin(riddles, new Vector2(100f, 100f), 3f));
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_UniqueKeys_BothLanguages_EveryRiddleAndCousinLine()
        {
            var keys = new HashSet<string>();
            foreach (var entry in CityEventsTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate key " + entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                StringAssert.DoesNotContain("Dona ", entry.PtBR);
                StringAssert.DoesNotContain("Seu ", entry.PtBR);
            }

            for (int i = 0; i < MingchengLanternFestaRules.RiddleCount; i++)
            {
                Assert.IsTrue(keys.Contains(CityEventsTexts.RiddleKey(i)), "riddle " + i);
            }

            foreach (LanternAnswer answer in Enum.GetValues(typeof(LanternAnswer)))
            {
                Assert.IsTrue(keys.Contains(CityEventsTexts.AnswerKey(answer)), answer.ToString());
            }

            CharacterId?[] heroes = { null, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine };
            foreach (string kind in new[] { "tide", "snow", "festa" })
            {
                var lines = new HashSet<string>();
                foreach (CharacterId? hero in heroes)
                {
                    string key = CityEventsTexts.LineFor(kind, hero);
                    Assert.IsTrue(keys.Contains(key), key);
                    lines.Add(key);
                }

                Assert.AreEqual(5, lines.Count, kind + ": one line per cousin");
            }

            Assert.AreEqual("Lodo na Raiz-Mãe: 40%", CityEventsTexts.Format(null, CityEventsTexts.TideSilt, 40));
        }
    }
}
