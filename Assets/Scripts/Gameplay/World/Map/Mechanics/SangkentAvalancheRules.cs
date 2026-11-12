using System;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A snow lane above Sangkent and the fence of juniper stakes across it (<c>OverworldLayouts.SangkentTerraceLanes</c>).</summary>
    public readonly struct SangkentTerraceLane
    {
        public readonly TetoChuteLane Lane;
        public readonly Vector2 Fence;

        public SangkentTerraceLane(TetoChuteLane lane, Vector2 fence)
        {
            Lane = lane;
            Fence = fence;
        }
    }

    public enum SangkentLanePhase
    {
        Quiet,
        Warning,
        Sliding,
        Done,
    }

    /// <summary>
    /// The Avalanche of Sangkent (Docs/Mapas100.md 36: "desce a neve, os primos protegem os terraços de baixo"), pure and server-runnable on the
    /// real clock (<see cref="WorldEventCalendar"/>, Brasília). Seconds below are seconds into the window.
    /// <code>
    /// every hour at :40, open 5 min; Kustai's bell warns 60 s before (the hero within 260 m of the town)
    /// three lanes slide in turn, west to east, at 60, 140 and 220 s; each is marked 40 s before; the front takes 8 s from the top to the town's edge
    /// [E] within 3.5 m of a lane's fence plants the juniper stakes (any time in the window before its slide)
    /// the fence holds when it was planted and the hero is within 12 m of it as the front comes: the front stops at the fence and that terrace is saved
    /// otherwise the snow runs down to the terrace below and buries it for the rest of the window
    /// the front hits once per lane: 22% of max life, never below 20%; only a hero warned for that lane, and never one standing below a fence that held
    /// paid once per window when the last lane is done: 50 gold per saved terrace, all three: +1 Pedra-Faísca
    /// </code>
    /// The fences are 60–65 m apart: from one to the next is 12 s on foot against 80 s between slides, so the beasts of the terraces are the only hurry.
    /// </summary>
    public static class SangkentAvalancheRules
    {
        /// <summary>Every hour at :40 (Brasília), 5 minutes.</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(60, 40, 5);

        public const double WarningSeconds = 60d;
        public const float AlertRange = 260f;
        public const int LaneCount = 3;

        public static readonly double[] SlideAt = { 60d, 140d, 220d };
        public const double LaneWarnSeconds = 40d;
        public const double SlideSeconds = 8d;

        public const float FenceReach = 3.5f;
        public const float HoldRadius = 12f;
        public const float HitFraction = TetoAvalancheRules.HitFraction;
        public const float HitFloorFraction = TetoAvalancheRules.HitFloorFraction;

        public const int GoldPerTerrace = 50;
        public const int AllSavedStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last window paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.avalanche_sangkent.paid";

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        public static bool IsWarning(RealTimeWindow window) => !window.IsOpen && window.SecondsToChange <= WarningSeconds;

        /// <summary>Seconds into the open window <paramref name="window"/> at <paramref name="utc"/>.</summary>
        public static double SecondsInto(RealTimeWindow window, DateTime utc) => (utc - window.StartUtc).TotalSeconds;

        public static long PaidMark(long windowIndex) => windowIndex + 1L;

        public static bool IsPaid(long mark, long windowIndex) => mark == PaidMark(windowIndex);

        public static SangkentLanePhase PhaseOf(int lane, double secondsIn)
        {
            if (lane < 0 || lane >= LaneCount)
            {
                return SangkentLanePhase.Quiet;
            }

            double slide = SlideAt[lane];
            if (secondsIn >= slide + SlideSeconds)
            {
                return SangkentLanePhase.Done;
            }

            if (secondsIn >= slide)
            {
                return SangkentLanePhase.Sliding;
            }

            return secondsIn >= slide - LaneWarnSeconds ? SangkentLanePhase.Warning : SangkentLanePhase.Quiet;
        }

        /// <summary>How far down its lane the front has come (0 top, 1 foot) at <paramref name="secondsIn"/>, before any fence stops it.</summary>
        public static float Progress(int lane, double secondsIn)
        {
            if (lane < 0 || lane >= LaneCount)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)((secondsIn - SlideAt[lane]) / SlideSeconds));
        }

        /// <summary>Seconds until lane <paramref name="lane"/> slides (0 once it has).</summary>
        public static double SecondsToSlide(int lane, double secondsIn) => lane < 0 || lane >= LaneCount ? 0d : Math.Max(0d, SlideAt[lane] - secondsIn);

        /// <summary>The last lane done: the window's result can be paid.</summary>
        public static bool AllDone(double secondsIn) => secondsIn >= SlideAt[LaneCount - 1] + SlideSeconds;

        public static bool CanPlant(Vector2 fence, Vector2 hero) => Vector2.Distance(fence, hero) <= FenceReach;

        /// <summary>The fence holds the front: planted and the hero beside it.</summary>
        public static bool Holds(bool planted, Vector2 fence, Vector2 hero) => planted && Vector2.Distance(fence, hero) <= HoldRadius;

        /// <summary>Where the front stops along its lane (0 top, 1 foot): at the fence when it held, at the foot otherwise.</summary>
        public static float StopAlong(SangkentTerraceLane lane, bool held) => held ? TetoAvalancheRules.Along(lane.Lane, lane.Fence) : 1f;

        /// <summary>
        /// True when the front, from <paramref name="fromProgress"/> to <paramref name="toProgress"/> (both already capped at the stop), passes over the
        /// hero; nobody at or below a held fence is touched (the band of the front never reaches past the stop).
        /// </summary>
        public static bool Sweeps(SangkentTerraceLane lane, Vector2 hero, float fromProgress, float toProgress, bool held)
        {
            if (!TetoAvalancheRules.InLane(lane.Lane, hero))
            {
                return false;
            }

            float along = TetoAvalancheRules.Along(lane.Lane, hero);
            if (held && along >= StopAlong(lane, true) - FenceShelter(lane))
            {
                return false;
            }

            return TetoAvalancheRules.SweptBy(lane.Lane, hero, fromProgress, toProgress);
        }

        /// <summary>The front's band as a fraction of the lane: standing that close above a held fence is behind the stakes.</summary>
        private static float FenceShelter(SangkentTerraceLane lane)
        {
            float length = Vector2.Distance(lane.Lane.Top, lane.Lane.Bottom);
            return length > 0f ? TetoAvalancheRules.FrontBand / length : 0f;
        }

        public static float Hit(float current, float max) => ProfundoHazardRules.Bite(current, max, HitFraction, HitFloorFraction);

        /// <summary>Gold of a window with <paramref name="saved"/> terraces saved.</summary>
        public static int GoldFor(int saved) => Mathf.Clamp(saved, 0, LaneCount) * GoldPerTerrace;

        public static int StonesFor(int saved) => saved >= LaneCount ? AllSavedStones : 0;
    }
}
