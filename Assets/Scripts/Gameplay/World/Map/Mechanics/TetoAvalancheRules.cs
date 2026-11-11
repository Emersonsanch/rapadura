using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum TetoAvalanchePhase
    {
        Quiet,
        Warning,
        Sliding,
    }

    /// <summary>A lane of the avalanche, from its top to its foot; the front sweeps Top → Bottom across its width.</summary>
    public readonly struct TetoChuteLane
    {
        public readonly Vector2 Top;
        public readonly Vector2 Bottom;
        public readonly float Width;

        public TetoChuteLane(Vector2 top, Vector2 bottom, float width)
        {
            Top = top;
            Bottom = bottom;
            Width = width;
        }
    }

    /// <summary>
    /// The avalanches of the Calhas de Pedra (Docs/Mapas100.md 37), pure and server-runnable on the UTC clock (<c>t</c> in seconds); the four lanes are
    /// <c>OverworldLayouts.ChuteLanes</c>.
    /// <code>
    /// cycle of 90 s, one lane in rotation: 60 s quiet, 20 s of warning (bell and the lane marked), 7 s of the front sliding from top to foot, 3 s quiet
    /// the front hits once per passage: 22% of max life, never below 20%; only a hero who was warned this cycle can be hit
    /// </code>
    /// The lanes are 18 m wide, two seconds' walk to either side against a 20 s warning; a lane blocks nothing (no wall, no snow left behind), so a road
    /// that crosses one is only dangerous for the seven seconds of the slide. The avalanche never holds the hero and never kills.
    /// </summary>
    public static class TetoAvalancheRules
    {
        public const int LaneCount = 4;
        public const double CycleSeconds = 90d;
        public const double WarningStart = 60d;
        public const double SlidingStart = 80d;
        public const double SlidingEnd = 87d;
        public const float HitFraction = 0.22f;
        public const float HitFloorFraction = 0.2f;
        public const float FrontBand = 4f;
        public const float AlertRange = 150f;

        public static long CycleOf(double t) => (long)Math.Floor(t / CycleSeconds);

        private static double Second(double t) => t - CycleOf(t) * CycleSeconds;

        public static TetoAvalanchePhase PhaseAt(double t)
        {
            double second = Second(t);
            if (second >= SlidingStart && second < SlidingEnd)
            {
                return TetoAvalanchePhase.Sliding;
            }

            return second >= WarningStart && second < SlidingStart ? TetoAvalanchePhase.Warning : TetoAvalanchePhase.Quiet;
        }

        public static int LaneAt(double t)
        {
            long lanes = LaneCount;
            return (int)(((CycleOf(t) % lanes) + lanes) % lanes);
        }

        /// <summary>How far down its lane the front has come (0 at the top, 1 at the foot); 0 outside the slide.</summary>
        public static float Progress(double t)
        {
            return PhaseAt(t) == TetoAvalanchePhase.Sliding ? Mathf.Clamp01((float)((Second(t) - SlidingStart) / (SlidingEnd - SlidingStart))) : 0f;
        }

        /// <summary>How far along the lane <paramref name="p"/> lies (0 top, 1 foot).</summary>
        public static float Along(TetoChuteLane lane, Vector2 p)
        {
            Vector2 line = lane.Bottom - lane.Top;
            float sqr = line.sqrMagnitude;
            return sqr > 0f ? Mathf.Clamp01(Vector2.Dot(p - lane.Top, line) / sqr) : 0f;
        }

        public static float DistanceToLane(TetoChuteLane lane, Vector2 p) => Vector2.Distance(p, Vector2.Lerp(lane.Top, lane.Bottom, Along(lane, p)));

        public static bool InLane(TetoChuteLane lane, Vector2 p) => DistanceToLane(lane, p) <= lane.Width * 0.5f;

        /// <summary>True when the front, moving from <paramref name="fromProgress"/> to <paramref name="toProgress"/>, passes over the hero in the lane.</summary>
        public static bool SweptBy(TetoChuteLane lane, Vector2 p, float fromProgress, float toProgress)
        {
            if (!InLane(lane, p))
            {
                return false;
            }

            float length = Vector2.Distance(lane.Top, lane.Bottom);
            float band = length > 0f ? FrontBand / length : 0f;
            float along = Along(lane, p);
            return along >= fromProgress - band && along <= toProgress + band;
        }

        /// <summary>Only a hero who heard the warning this cycle can be hit.</summary>
        public static bool CanHit(long warnedCycle, long cycle) => warnedCycle == cycle;

        /// <summary>Health the front takes (never past the floor).</summary>
        public static float Hit(float current, float max) => ProfundoHazardRules.Bite(current, max, HitFraction, HitFloorFraction);
    }
}
