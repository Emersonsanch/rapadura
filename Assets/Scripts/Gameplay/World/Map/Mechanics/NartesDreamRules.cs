using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The dreams of the Fortim dos Gigantes-da-Colina (Docs/Mapas100.md 49: "sonhos: o Virogdad lê um sonho por dia que aponta um baú
    /// escondido no mapa"), pure and server-runnable. Each UTC day one of eight hiding places of the map (the Vale das Tábuas de Sonho and the
    /// hills round the fort) holds the dream's chest, the same for everyone. Standing in the fort, the hero hears Virogdad tell the dream: how
    /// far and which way from the fort. Only then the chest shows (a column of dream light), and [E] at it (from 3 m) opens it once that day.
    /// <code>
    /// 8 hiding places · one per UTC day (a hash of the day) · heard in the fort → shown · [E] 3 m · 40 gold nuggets, once per day
    /// </code>
    /// </summary>
    public static class NartesDreamRules
    {
        public const float OpenReach = 3f;
        public const int Purse = 40;

        /// <summary>The Nartes driver's daily claim keys (its <c>nartes.day.</c> prefix is added by the driver).</summary>
        public const string HeardKey = "dream_heard";
        public const string ChestKey = "dream_chest";

        /// <summary>The hiding place of <paramref name="day"/> (a UTC day number) among <paramref name="count"/>.</summary>
        public static int SpotOf(long day, int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            unchecked
            {
                ulong h = (ulong)day * 0x9E3779B97F4A7C15UL;
                h ^= h >> 29;
                h *= 0xBF58476D1CE4E5B9UL;
                h ^= h >> 32;
                return (int)(h % (ulong)count);
            }
        }

        /// <summary>The compass point (0 north, 1 north-east … 7 north-west) from <paramref name="from"/> to <paramref name="to"/>.</summary>
        public static int CompassOf(Vector2 from, Vector2 to)
        {
            Vector2 d = to - from;
            if (d.sqrMagnitude < 1e-6f)
            {
                return 0;
            }

            float bearing = Mathf.Repeat(Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg, 360f);
            return Mathf.RoundToInt(bearing / 45f) % 8;
        }

        /// <summary>The distance told in the dream: rounded to 50 m.</summary>
        public static int ToldDistance(Vector2 from, Vector2 to) => Mathf.Max(50, Mathf.RoundToInt(Vector2.Distance(from, to) / 50f) * 50);

        /// <summary>Whether the chest shows and opens: the dream was heard today and the chest not opened today.</summary>
        public static bool Open(long heardDay, long openedDay, long today) => heardDay == today && openedDay != today;

        public static bool InReach(IReadOnlyList<Vector2> spots, int spot, Vector2 p)
        {
            return spots != null && spot >= 0 && spot < spots.Count && (spots[spot] - p).sqrMagnitude <= OpenReach * OpenReach;
        }
    }
}
