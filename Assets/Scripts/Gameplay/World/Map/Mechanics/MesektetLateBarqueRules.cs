using System;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// A Barca Atrasou of the Cais de Mesektet (Docs/Mapas100.md 71: "defender a barca no cais quando a sombra a segura"), pure and
    /// server-runnable on the real clock (<see cref="WorldEventCalendar"/>, Brasília). Seconds below are seconds into the window.
    /// <code>
    /// every hour at :30, open 5 min; Sinuhe warns 60 s before (the hero within 260 m of the quay) that the Mesektet will not leave on time
    /// the Shadow ties the barque with three ropes of shadow to three posts on the quay's roads (W, E, S, 42 m out); while one rope holds, the
    ///   Mesektet cannot sail
    /// it starts by itself with the hero within 90 m of the quay (not yet paid that window); abandoned past 150 m (the scarabs go back into the river,
    ///   the ropes keep what was cut)
    /// two Shadow Scarabs guard each post at the start; every 45 s one more comes out of the water for each rope still tied; at most 9 alive
    /// [E] within 3 m of a tied post (Maria, whose element is the Light, from 5 m) cuts its rope, never with a scarab within 6 m of the post
    /// all three cut: the Mesektet sails (paid at once); the window ends first: the barque is dragged off late (paid for what was cut)
    /// paid once per window: 40 gold per rope cut, all three: +1 Pedra-Faísca
    /// </code>
    /// The posts stand 59–84 m apart: 15 s on foot at most between two of them.
    /// </summary>
    public static class MesektetLateBarqueRules
    {
        /// <summary>Every hour at :30 (Brasília), 5 minutes: clear of the Tremor (:05), the Maré de Lodo (:20), the Avalanche (:40) and the Vendaval (:50).</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(60, 30, 5);

        public const double WarningSeconds = 60d;
        public const float AlertRange = 260f;
        public const float StartRange = 90f;
        public const float LeaveRange = 150f;

        public const int RopeCount = 3;

        /// <summary>The Escaravelho de Sombra of the quay's margins (map 71) guards the ropes.</summary>
        public const string GuardEnemyId = "enemy_forest_shadow_scarab";

        /// <summary>The level of the quay's beasts (260–268).</summary>
        public const int GuardLevel = 264;

        public const int GuardsPerPostAtStart = 2;
        public const double ReinforceEverySeconds = 45d;
        public const int MaxAlive = 9;

        public const float CutReach = 3f;
        public const float LightReach = 5f;
        public const float GuardClearRadius = 6f;

        public const int GoldPerRope = 40;
        public const int AllCutStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last window paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.barca_atrasou.paid";

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        public static bool IsWarning(RealTimeWindow window) => !window.IsOpen && window.SecondsToChange <= WarningSeconds;

        public static double SecondsInto(RealTimeWindow window, DateTime utc) => (utc - window.StartUtc).TotalSeconds;

        public static long PaidMark(long windowIndex) => windowIndex + 1L;

        public static bool IsPaid(long mark, long windowIndex) => mark == PaidMark(windowIndex);

        public static bool CanStart(bool open, long paidMark, long windowIndex, float heroDistance) =>
            open && !IsPaid(paidMark, windowIndex) && heroDistance <= StartRange;

        /// <summary>How many reinforcement waves are due <paramref name="secondsIn"/> into the window (0 before 45 s).</summary>
        public static int ReinforcementsDue(double secondsIn) => secondsIn < ReinforceEverySeconds ? 0 : (int)Math.Floor(secondsIn / ReinforceEverySeconds);

        /// <summary>How many guards a wave may raise now: one per rope still tied, never past the cap.</summary>
        public static int WaveSize(int ropesTied, int alive) => Mathf.Clamp(Mathf.Min(ropesTied, MaxAlive - alive), 0, RopeCount);

        /// <summary>The guards raised when the defence starts (two per post still tied), never past the cap.</summary>
        public static int StartGuards(int ropesTied) => Mathf.Clamp(ropesTied * GuardsPerPostAtStart, 0, MaxAlive);

        public static float ReachFor(bool light) => light ? LightReach : CutReach;

        /// <summary>Whether [E] cuts the rope: tied, no scarab within 6 m of the post and the hero within reach.</summary>
        public static bool CanCut(bool tied, int guardsNear, float heroDistance, bool light) => tied && guardsNear <= 0 && heroDistance <= ReachFor(light);

        public static int CutCount(bool[] cut)
        {
            int n = 0;
            for (int i = 0; cut != null && i < cut.Length; i++)
            {
                n += cut[i] ? 1 : 0;
            }

            return n;
        }

        /// <summary>The Mesektet sails: every rope cut.</summary>
        public static bool Sails(bool[] cut) => CutCount(cut) >= RopeCount;

        public static int GoldFor(int cut) => Mathf.Clamp(cut, 0, RopeCount) * GoldPerRope;

        public static int StonesFor(int cut) => cut >= RopeCount ? AllCutStones : 0;
    }
}
