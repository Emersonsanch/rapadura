using System;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum GalePhase
    {
        Quiet,
        Warning,
        Blowing,
        Done,
    }

    /// <summary>
    /// The Vendaval of Farnkesh (Docs/Mapas100.md 54: "o vento vira e as pontes balançam: segurar as cordas"), pure and server-runnable on the
    /// real clock (<see cref="WorldEventCalendar"/>, Brasília). Seconds below are seconds into the window.
    /// <code>
    /// every hour at :50, open 5 min; Shad-Ohrmizd's wind bells warn 60 s before (the hero within 260 m of the town)
    /// five gusts at 40, 95, 150, 205 and 260 s, 5 s each; the wind turns: each gust falls on one of the three bridge moorings (W, E, S road),
    ///   never the same twice in a row, the order fixed by the window (<see cref="CityEventTurns"/>); the bells of that side ring 30 s before
    /// [E] within 3 m of the warned mooring grabs the rope (Maithe, who hears the Living Wind, from 5 m) any time in those 30 s
    /// the rope holds when it was grabbed and the hero is within 6 m of the mooring as the gust comes: that bridge stays still
    /// otherwise the bridge swings: a hero within 16 m of its mooring is thrown, 12% of max life, never below 20% (once per gust)
    /// paid once per window after the last gust: 30 gold per gust held, all five: +1 Pedra-Faísca
    /// </code>
    /// The moorings stand 73–125 m apart through the town: 23 s on foot at most against 30 s of bells, so a gust missed is a gust run late.
    /// </summary>
    public static class FarnkeshGaleRules
    {
        /// <summary>Every hour at :50 (Brasília), 5 minutes.</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(60, 50, 5);

        public const double WarningSeconds = 60d;
        public const float AlertRange = 260f;
        public const int MooringCount = 3;
        public const int GustCount = 5;

        public static readonly double[] GustAt = { 40d, 95d, 150d, 205d, 260d };
        public const double GustWarnSeconds = 30d;
        public const double GustSeconds = 5d;

        public const float RopeReach = 3f;
        public const float WindListenerReach = 5f;
        public const float HoldRadius = 6f;
        public const float SwingRadius = 16f;
        public const float HitFraction = 0.12f;
        public const float HitFloorFraction = 0.2f;

        public const int GoldPerGust = 30;
        public const int AllHeldStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last window paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.vendaval_farnkesh.paid";

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        public static bool IsWarning(RealTimeWindow window) => !window.IsOpen && window.SecondsToChange <= WarningSeconds;

        public static double SecondsInto(RealTimeWindow window, DateTime utc) => (utc - window.StartUtc).TotalSeconds;

        public static long PaidMark(long windowIndex) => windowIndex + 1L;

        public static bool IsPaid(long mark, long windowIndex) => mark == PaidMark(windowIndex);

        /// <summary>The mooring (0 W, 1 E, 2 S) gust <paramref name="gust"/> of window <paramref name="windowIndex"/> falls on.</summary>
        public static int MooringOf(long windowIndex, int gust) => CityEventTurns.SideOf(windowIndex, gust, MooringCount);

        public static GalePhase PhaseOf(int gust, double secondsIn)
        {
            if (gust < 0 || gust >= GustCount)
            {
                return GalePhase.Quiet;
            }

            double at = GustAt[gust];
            if (secondsIn >= at + GustSeconds)
            {
                return GalePhase.Done;
            }

            if (secondsIn >= at)
            {
                return GalePhase.Blowing;
            }

            return secondsIn >= at - GustWarnSeconds ? GalePhase.Warning : GalePhase.Quiet;
        }

        /// <summary>The gust being warned or blowing at <paramref name="secondsIn"/>, or −1 between gusts (the warnings never overlap).</summary>
        public static int CurrentGust(double secondsIn)
        {
            for (int i = 0; i < GustCount; i++)
            {
                GalePhase phase = PhaseOf(i, secondsIn);
                if (phase == GalePhase.Warning || phase == GalePhase.Blowing)
                {
                    return i;
                }
            }

            return -1;
        }

        public static double SecondsToGust(int gust, double secondsIn) => gust < 0 || gust >= GustCount ? 0d : Math.Max(0d, GustAt[gust] - secondsIn);

        /// <summary>The last gust is over: the window's result can be paid.</summary>
        public static bool AllDone(double secondsIn) => secondsIn >= GustAt[GustCount - 1] + GustSeconds;

        /// <summary>How near the mooring the hero grabs the rope (Maithe hears the Living Wind and grabs from farther).</summary>
        public static float ReachFor(bool windListener) => windListener ? WindListenerReach : RopeReach;

        public static bool CanGrab(Vector2 mooring, Vector2 hero, bool windListener) => Vector2.Distance(mooring, hero) <= ReachFor(windListener);

        /// <summary>The rope holds the bridge: grabbed and the hero still beside it.</summary>
        public static bool Holds(bool grabbed, Vector2 mooring, Vector2 hero) => grabbed && Vector2.Distance(mooring, hero) <= HoldRadius;

        /// <summary>A bridge that swings throws whoever stands on it or at its mooring.</summary>
        public static bool Thrown(bool held, Vector2 mooring, Vector2 hero) => !held && Vector2.Distance(mooring, hero) <= SwingRadius;

        public static float Hit(float current, float max) => ProfundoHazardRules.Bite(current, max, HitFraction, HitFloorFraction);

        public static int GoldFor(int held) => Mathf.Clamp(held, 0, GustCount) * GoldPerGust;

        public static int StonesFor(int held) => held >= GustCount ? AllHeldStones : 0;
    }
}
