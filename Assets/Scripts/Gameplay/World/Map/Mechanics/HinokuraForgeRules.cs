using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The verdict at a sluice: what [E] would do there now.</summary>
    public enum HinokuraSluiceCheck
    {
        Open,
        AlreadyLit,
        Jammed,
    }

    /// <summary>
    /// The sluices, the Tremor and the Glutão's retreat (Docs/Mapas100.md 65 and 70), pure and server-runnable. Everything is derived from
    /// the unix clock and the moment each forge was lit, so a server only has to store three numbers.
    /// <code>
    /// 3 forges, each fed by one sluice; [E] within 7 m lights it for 20 min
    /// Tremor every 12 min on the clock, warned 30 s before: it puts out one forge in rotation (each forge is struck every 36 min)
    ///   and jams the sluices for 20 s. The other forges keep burning, and a struck forge can be lit again right after the jam
    /// Glutão de Brasa, every 10 s, per forge lit: −1.5% of max health (never below 40%) and 0.5 s of stagger (halved on bosses)
    /// </code>
    /// 20 min leaves time for the walk from the sluices to the caldera on foot. The burn stops at 40% so the last blow and the fight's
    /// last phases stay with the hero: the forges weaken the Glutão, they do not kill it. Nothing here touches the hero.
    /// </summary>
    public static class HinokuraForgeRules
    {
        public const int Forges = 3;
        public const float SluiceReach = 7f;
        public const long BurnSeconds = 1200L;
        public const long TremorPeriodSeconds = 720L;
        public const long TremorWarnSeconds = 30L;
        public const long TremorJamSeconds = 20L;

        public const float PulseSeconds = 10f;
        public const float PulseFractionPerForge = 0.015f;
        public const float PulseFloorFraction = 0.4f;
        public const float StaggerSecondsPerForge = 0.5f;

        /// <summary>The stored moment of a forge that has never been lit.</summary>
        public const long NeverLit = long.MinValue;

        public static long TremorCycle(long unix) => FloorDiv(unix, TremorPeriodSeconds);

        /// <summary>Seconds until the next Tremor (1…720: right at a Tremor the next one is a whole period away).</summary>
        public static long SecondsToNextTremor(long unix) => (TremorCycle(unix) + 1L) * TremorPeriodSeconds - unix;

        public static bool IsTremorWarning(long unix) => SecondsToNextTremor(unix) <= TremorWarnSeconds;

        /// <summary>Seconds the sluices stay jammed (0 when they are free).</summary>
        public static long JamSecondsLeft(long unix)
        {
            long since = unix - TremorCycle(unix) * TremorPeriodSeconds;
            return since < TremorJamSeconds ? TremorJamSeconds - since : 0L;
        }

        public static bool IsJammed(long unix) => JamSecondsLeft(unix) > 0L;

        /// <summary>The forge the Tremor of a cycle puts out.</summary>
        public static int StruckForge(long cycle) => (int)(cycle - FloorDiv(cycle, Forges) * Forges);

        /// <summary>The moment of the first Tremor after <paramref name="litAt"/> that strikes this forge.</summary>
        public static long NextStrikeAfter(int forge, long litAt)
        {
            long cycle = TremorCycle(litAt) + 1L;
            while (StruckForge(cycle) != forge)
            {
                cycle++;
            }

            return cycle * TremorPeriodSeconds;
        }

        public static bool IsLit(int forge, long litAt, long now)
        {
            if (litAt == NeverLit || now < litAt || now - litAt >= BurnSeconds)
            {
                return false;
            }

            return now < NextStrikeAfter(forge, litAt);
        }

        /// <summary>When a lit forge goes out: at the end of its burn or at its Tremor, whichever comes first.</summary>
        public static long GoesOutAt(int forge, long litAt) => System.Math.Min(litAt + BurnSeconds, NextStrikeAfter(forge, litAt));

        public static int LitCount(long[] litAt, long now)
        {
            int count = 0;
            for (int i = 0; i < litAt.Length; i++)
            {
                if (IsLit(i, litAt[i], now))
                {
                    count++;
                }
            }

            return count;
        }

        public static HinokuraSluiceCheck Check(int forge, long litAt, long now)
        {
            if (IsLit(forge, litAt, now))
            {
                return HinokuraSluiceCheck.AlreadyLit;
            }

            return IsJammed(now) ? HinokuraSluiceCheck.Jammed : HinokuraSluiceCheck.Open;
        }

        /// <summary>Damage one pulse does to the Glutão: nothing with the forges out, at the floor, or on a dead boss.</summary>
        public static float PulseDamage(float current, float max, int litForges)
        {
            float floor = max * PulseFloorFraction;
            if (litForges <= 0 || current <= floor)
            {
                return 0f;
            }

            return Mathf.Min(current - floor, max * PulseFractionPerForge * litForges);
        }

        /// <summary>Seconds of stagger of one pulse, before the bosses' stun scaling; it does not depend on the health floor.</summary>
        public static float StaggerSeconds(int litForges) => Mathf.Max(0, litForges) * StaggerSecondsPerForge;

        private static long FloorDiv(long a, long b)
        {
            long q = a / b;
            return a % b != 0L && (a < 0L) != (b < 0L) ? q - 1L : q;
        }
    }
}
