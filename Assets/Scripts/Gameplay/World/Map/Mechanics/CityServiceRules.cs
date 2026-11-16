using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What [E] at a city's service point answers now (<see cref="CityServiceRules"/>).</summary>
    public enum CityServiceCheck
    {
        /// <summary>It works now.</summary>
        Ready,

        /// <summary>Already taken today (UTC): come back tomorrow.</summary>
        TakenToday,

        /// <summary>Not while fighting.</summary>
        InCombat,

        /// <summary>The Têmpera waits for the drums to play.</summary>
        WaitDrums,
    }

    /// <summary>
    /// Three small services of the race cities (Docs/Mapas100.md §3.3, Docs/Cities.md), pure and server-runnable; the scene side is
    /// <c>CityServicesMechanicsDriver</c>. Days are UTC days (the same for everyone), saved as daily marks.
    /// <code>
    /// Salões de Bronze · Puzur's rune pillar · [E] 2.4 m · one bronze rune a day, free (Clareza, Couro, Cura, Vento; the day picks it)
    /// Aratta · Lira do Touro · [E] 2.6 m out of combat · the song of the day (5 songs) · +5 % walking speed for 5 min, on foot and out of combat
    /// Tambor Rubro · war drum by the Salão Grande · [E] 2.6 m while the drums play (DrumRhythmRules) · once a day · +8 % attack damage for 10 min
    /// </code>
    /// </summary>
    public static class CityServiceRules
    {
        // ---- Salões de Bronze: the rune Puzur engraves free each day ----

        public const float RuneReach = 2.4f;

        /// <summary>The daily mark of the free rune (the UTC day it was last taken).</summary>
        public const string RuneDailyKey = "city.saloes_de_bronze.puzur_rune";

        /// <summary>The bronze runes Puzur engraves for free (the cheap ones of his counter; the Cura Grande stays for sale).</summary>
        public static readonly string[] FreeRunes = { "item_runa_clareza", "item_runa_couro", "item_runa_cura", "item_runa_vento" };

        // ---- Aratta: the Lira do Touro ----

        public const float LyreReach = 2.6f;
        public const int SongCount = 5;
        public const float SongSeconds = 300f;

        /// <summary>Walking speed bonus of the song (PercentAdditive on MoveSpeed).</summary>
        public const float SongSpeedBonus = 0.05f;

        // ---- Tambor Rubro: the Têmpera do Tambor ----

        public const float TemperReach = 2.6f;
        public const float TemperSeconds = 600f;

        /// <summary>Attack damage bonus of the tempered weapon (PercentAdditive on AttackDamage).</summary>
        public const float TemperDamageBonus = 0.08f;

        /// <summary>The daily mark of the Têmpera (the UTC day it was last given).</summary>
        public const string TemperDailyKey = "city.tambor_rubro.temper";

        /// <summary>The UTC day of a unix time in seconds.</summary>
        public static long DayOf(long unixSeconds) => (long)System.Math.Floor(unixSeconds / 86400d);

        /// <summary>Whether <paramref name="hero"/> stands within <paramref name="reach"/> of <paramref name="point"/> (same frame).</summary>
        public static bool InReach(Vector2 point, Vector2 hero, float reach) => (point - hero).sqrMagnitude <= reach * reach;

        /// <summary>The rune of <paramref name="day"/> (a UTC day): the same for everyone that day.</summary>
        public static string RuneOf(long day) => FreeRunes[Pick(day, FreeRunes.Length)];

        /// <summary>The free rune: once per UTC day (<paramref name="takenDay"/> is the saved day, 0 when never).</summary>
        public static CityServiceCheck CheckRune(long takenDay, long today) => takenDay == today ? CityServiceCheck.TakenToday : CityServiceCheck.Ready;

        /// <summary>The song of <paramref name="day"/> (0 … <see cref="SongCount"/> − 1): a new one each UTC day, never the same two days running.</summary>
        public static int SongOf(long day)
        {
            int today = Pick(day, SongCount);
            int yesterday = Pick(day - 1L, SongCount);
            return today != yesterday ? today : (today + 1) % SongCount;
        }

        /// <summary>The lyre plays any time out of combat (playing again renews the 5 minutes).</summary>
        public static CityServiceCheck CheckLyre(bool inCombat) => inCombat ? CityServiceCheck.InCombat : CityServiceCheck.Ready;

        /// <summary>Whether the song's speed counts now: time left, on foot, out of combat, alive.</summary>
        public static bool SongApplies(float secondsLeft, bool inCombat, bool mounted, bool alive) => secondsLeft > 0f && !inCombat && !mounted && alive;

        /// <summary>The Têmpera: only while the drums play, once per UTC day, never in a fight.</summary>
        public static CityServiceCheck CheckTemper(bool drumsPlaying, long givenDay, long today, bool inCombat)
        {
            if (givenDay == today)
            {
                return CityServiceCheck.TakenToday;
            }

            if (inCombat)
            {
                return CityServiceCheck.InCombat;
            }

            return drumsPlaying ? CityServiceCheck.Ready : CityServiceCheck.WaitDrums;
        }

        /// <summary>A well-mixed pick of 0 … <paramref name="count"/> − 1 for a day number.</summary>
        private static int Pick(long day, int count)
        {
            unchecked
            {
                ulong h = (ulong)day * 0xD6E8FEB86659FD93UL;
                h ^= h >> 32;
                h *= 0x9E3779B97F4A7C15UL;
                h ^= h >> 29;
                return (int)(h % (ulong)count);
            }
        }
    }
}
