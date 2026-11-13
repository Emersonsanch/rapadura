using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Where one defence of the lanterns stands (<see cref="PortoPolarNightRules.Resolve"/>).</summary>
    public enum PolarNightOutcome
    {
        Running,
        Dawn,
    }

    /// <summary>
    /// The Noite Polar of the Porto da Lua Branca (Docs/Mapas100.md 59: "dias sem sol: defender as lanternas da cidade"), pure and server-runnable
    /// on the game clock (its day and hour; the dark of the moon of <see cref="MoonCanalRules"/>, the same 8-day cycle as the canals and the
    /// Festa das Lanternas).
    /// <code>
    /// on the two dark-moon days of every 8 (moon 7 and 0: days 8 and 1 of the cycle) the sun does not come up: from 10 h to 15 h the Creeping Dark
    ///   comes for the three moon lanterns at the town's gates (W, E, S road); Makarios warns at 9 h
    /// starts by itself with the hero within 90 m of the Cais da Lua (not yet paid that day); abandoned past 150 m (the Dark goes back into the ice;
    ///   the lanterns keep what they lost, for the rest of the day)
    /// one wave an hour, 10 h to 14 h (2, 3, 3, 4, 4; a defence begun late starts at the hour's wave), at most 8 alive; each Dark walks to its lantern
    /// a Dark within 4 m of a lantern drinks its light: 8 a second each (at most 3 counted); at 0 the lantern goes out
    /// [E] within 3 m of a dark lantern (Lavine, the Moon Boatwoman, from 6 m) relights it to 60 with Pamour's oil: once per lantern a day,
    ///   and never with a Dark within 6 m of it
    /// at 15 h (the "dawn": the Barque of the Moon sends its light back) the Dark fades; paid once per dark day: 45 gold per lantern still lit,
    ///   all three: +1 Pedra-Faísca
    /// </code>
    /// Five game hours are 250 real seconds at 20 min a day. The Dark is fought outside the safe zone: the lanterns stand 40 m from the plaza.
    /// </summary>
    public static class PortoPolarNightRules
    {
        public const float StartHour = 10f;
        public const float EndHour = 15f;
        public const float WarningHour = 9f;

        public const float StartRange = 90f;
        public const float LeaveRange = 150f;
        public const float AlertRange = 150f;

        /// <summary>The Creeping Dark (the Treva Rastejante of the Céu) stands in for the dark of the polar night.</summary>
        public const string ShadowEnemyId = "enemy_forest_creeping_dark";

        /// <summary>The level of the harbour's beasts (180–188).</summary>
        public const int ShadowLevel = 184;

        public const int LanternCount = 3;
        public static readonly IReadOnlyList<int> WaveSizes = new[] { 2, 3, 3, 4, 4 };
        public const int MaxAlive = 8;

        public const float LightMax = 100f;
        public const float DrainPerSecond = 8f;
        public const int MaxDrinkers = 3;
        public const float DrinkReach = 4f;
        public const float RelightLight = 60f;
        public const float RelightReach = 3f;
        public const float MoonBoatwomanReach = 6f;
        public const float RelightClearRadius = 6f;

        public const int GoldPerLantern = 45;
        public const int AllLitStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last dark day paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.noite_polar.paid";

        /// <summary>A day without sun: the two days of the dark moon (moon 7 and 0 of 8).</summary>
        public static bool IsPolarDay(int day)
        {
            int moon = MoonCanalRules.MoonIndex(day);
            return moon == 0 || moon == MoonCanalRules.CycleDays - 1;
        }

        public static bool IsOpen(int day, float hour) => IsPolarDay(day) && hour >= StartHour && hour < EndHour;

        /// <summary>The hour before: Makarios sees that the sun will not come.</summary>
        public static bool IsWarning(int day, float hour) => IsPolarDay(day) && hour >= WarningHour && hour < StartHour;

        /// <summary>Game days until the next dark day (0: today).</summary>
        public static int DaysUntilPolar(int day)
        {
            for (int wait = 0; wait < MoonCanalRules.CycleDays; wait++)
            {
                if (IsPolarDay(day + wait))
                {
                    return wait;
                }
            }

            return 0;
        }

        /// <summary>What the save keeps for a paid dark day (the day + 1, so 0 is "never").</summary>
        public static long PaidMark(int day) => day + 1L;

        public static bool IsPaid(long mark, int day) => mark == PaidMark(day);

        public static bool CanStart(int day, float hour, long paidMark, float heroDistance) =>
            IsOpen(day, hour) && !IsPaid(paidMark, day) && heroDistance <= StartRange;

        public static int WaveCount => WaveSizes.Count;

        public static int WaveSize(int wave) => WaveSizes[Math.Max(0, Math.Min(wave, WaveSizes.Count - 1))];

        /// <summary>The wave the hour has reached (0 at 10 h … 4 at 14 h), −1 before 10 h.</summary>
        public static int WaveDue(float hour) => hour < StartHour ? -1 : Math.Min(WaveCount - 1, (int)Math.Floor(hour - StartHour));

        /// <summary>The first wave of a defence begun at <paramref name="hour"/>: the hour's own (the hours gone are not sent again).</summary>
        public static int FirstWave(float hour) => Math.Max(0, WaveDue(hour));

        /// <summary>Whether the next wave comes now: its hour has come and there is room under the cap.</summary>
        public static bool ShouldSpawnWave(int wavesSpawned, float hour, int alive) =>
            wavesSpawned < WaveCount && wavesSpawned <= WaveDue(hour) && alive + WaveSize(wavesSpawned) <= MaxAlive;

        /// <summary>The light of a lantern after <paramref name="seconds"/> with <paramref name="drinkers"/> Dark beside it (at most 3 counted).</summary>
        public static float Drain(float light, int drinkers, float seconds)
        {
            int counted = Mathf.Clamp(drinkers, 0, MaxDrinkers);
            return Mathf.Clamp(light - counted * DrainPerSecond * Mathf.Max(0f, seconds), 0f, LightMax);
        }

        public static bool IsOut(float light) => light <= 0f;

        public static bool Drinking(Vector2 lantern, Vector2 shadow) => Vector2.Distance(lantern, shadow) <= DrinkReach;

        public static float ReachFor(bool moonBoatwoman) => moonBoatwoman ? MoonBoatwomanReach : RelightReach;

        /// <summary>Whether [E] relights the lantern: out, its oil not yet used today, no Dark within 6 m and the hero within reach.</summary>
        public static bool CanRelight(float light, bool oilUsed, int darkNear, float heroDistance, bool moonBoatwoman) =>
            IsOut(light) && !oilUsed && darkNear <= 0 && heroDistance <= ReachFor(moonBoatwoman);

        public static PolarNightOutcome Resolve(int day, float hour, int runningDay) =>
            day != runningDay || hour >= EndHour ? PolarNightOutcome.Dawn : PolarNightOutcome.Running;

        public static int LitCount(float[] lights)
        {
            int lit = 0;
            for (int i = 0; lights != null && i < lights.Length; i++)
            {
                lit += IsOut(lights[i]) ? 0 : 1;
            }

            return lit;
        }

        public static int GoldFor(int lit) => Mathf.Clamp(lit, 0, LanternCount) * GoldPerLantern;

        public static int StonesFor(int lit) => lit >= LanternCount ? AllLitStones : 0;
    }
}
