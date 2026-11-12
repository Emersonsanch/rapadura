using System;
using System.Collections.Generic;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Where one defence of the ghats stands (<see cref="JhilpurMudTideRules.Resolve"/>).</summary>
    public enum MudTideOutcome
    {
        Running,
        Won,
        Silted,
    }

    /// <summary>
    /// The Maré de Lodo of Jhilpur (Docs/Mapas100.md 31: "ondas de Filhos do Lodo sobem os ghats; defender a Raiz-Mãe", MU Online's Crywolf as a
    /// model), pure and server-runnable on the real clock (<see cref="WorldEventCalendar"/>, Brasília like the world bosses and the Invasão de Āz).
    /// <code>
    /// every hour at :20, open 6 min; warned 60 s before (the hero within 150 m of the town)
    /// starts by itself when the hero is within 90 m of the Raiz-Mãe with the tide open and not yet paid; abandoned past 150 m (the Filhos sink back)
    /// four waves of Filhos do Lodo (3, 4, 5, 6) rise on the lake shore and climb to the foot of the ghats; the next wave with ≤ 1 alive or every 25 s, at most 8 alive
    /// a Filho standing at the foot of the ghats silts the Raiz-Mãe: 2.5 a second each (at most 6 counted), 100 = silted (lost: 20 s rest, then again while it is open)
    /// won: every wave sent and none alive · paid once per tide: 150 gold and 1 Pedra-Faísca, +1 Pedra-Faísca when the silt stayed under 25
    /// </code>
    /// The Filhos are fought outside the safe zone (nothing is hurt inside the town): the foot of the ghats is 4 m below it. The tide never touches the hero
    /// itself; the Filhos hit as the beasts they are.
    /// </summary>
    public static class JhilpurMudTideRules
    {
        /// <summary>Every hour at :20 (Brasília), 6 minutes.</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(60, 20, 6);

        public const double WarningSeconds = 60d;
        public const float StartRange = 90f;
        public const float LeaveRange = 150f;
        public const float AlertRange = 150f;

        /// <summary>The cave ooze stands in for the Filhos do Lodo (as in Salmaios's mission "Lodo nos Ghats").</summary>
        public const string FilhoEnemyId = "enemy_cave_ooze";

        /// <summary>The level of the Lago de Jhilpur's beasts (60–66).</summary>
        public const int FilhoLevel = 62;

        public static readonly IReadOnlyList<int> WaveSizes = new[] { 3, 4, 5, 6 };
        public const float WaveIntervalSeconds = 25f;
        public const int MaxAlive = 8;

        public const float SiltMax = 100f;
        public const float SiltPerSecond = 2.5f;
        public const int MaxSilters = 6;
        public const float GoodSilt = 25f;
        public const float RestSeconds = 20f;

        public const int RewardGold = 150;
        public const int RewardStones = 1;
        public const int CleanBonusStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last tide paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.mare_de_lodo.paid";

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        /// <summary>The bell before the tide: closed, and opening within <see cref="WarningSeconds"/>.</summary>
        public static bool IsWarning(RealTimeWindow window) => !window.IsOpen && window.SecondsToChange <= WarningSeconds;

        /// <summary>What the save keeps for a paid tide (the window index + 1, so 0 is "never").</summary>
        public static long PaidMark(long windowIndex) => windowIndex + 1L;

        public static bool IsPaid(long mark, long windowIndex) => mark == PaidMark(windowIndex);

        public static int WaveCount => WaveSizes.Count;

        public static int TotalFilhos
        {
            get
            {
                int total = 0;
                foreach (int size in WaveSizes)
                {
                    total += size;
                }

                return total;
            }
        }

        public static int WaveSize(int waveIndex) => WaveSizes[Math.Max(0, Math.Min(waveIndex, WaveSizes.Count - 1))];

        /// <summary>Whether the next wave rises now (the event rooms' rule: field almost clear or the interval passed).</summary>
        public static bool ShouldSpawnWave(int wavesSpawned, int alive, float sinceLastWave)
        {
            return wavesSpawned < WaveCount && EventRoomRules.ShouldSpawnWave(alive, sinceLastWave, WaveIntervalSeconds, MaxAlive);
        }

        /// <summary>Whether the hero starts a defence here now.</summary>
        public static bool CanStart(RealTimeWindow window, long paidMark, float heroDistance, bool resting)
        {
            return window.IsOpen && !IsPaid(paidMark, window.Index) && heroDistance <= StartRange && !resting;
        }

        /// <summary>A Filho at <paramref name="local"/> (region coordinates) silts the Raiz-Mãe when it stands in the band at the foot of the ghats.</summary>
        public static bool Silting(Rect ghatBand, Vector2 local) => ghatBand.Contains(local);

        /// <summary>The silt after <paramref name="seconds"/> with <paramref name="silters"/> Filhos at the ghats (at most <see cref="MaxSilters"/> counted).</summary>
        public static float AdvanceSilt(float silt, int silters, float seconds)
        {
            int counted = Mathf.Clamp(silters, 0, MaxSilters);
            return Mathf.Clamp(silt + counted * SiltPerSecond * Mathf.Max(0f, seconds), 0f, SiltMax);
        }

        public static MudTideOutcome Resolve(int wavesSpawned, int alive, float silt)
        {
            if (silt >= SiltMax)
            {
                return MudTideOutcome.Silted;
            }

            return wavesSpawned >= WaveCount && alive <= 0 ? MudTideOutcome.Won : MudTideOutcome.Running;
        }

        /// <summary>Pedras-Faísca of a won tide.</summary>
        public static int StonesFor(float silt) => RewardStones + (silt < GoodSilt ? CleanBonusStones : 0);

        /// <summary>The silt as a whole percentage, for the feedback.</summary>
        public static int Percent(float silt) => Mathf.Clamp(Mathf.FloorToInt(silt / SiltMax * 100f), 0, 100);
    }
}
