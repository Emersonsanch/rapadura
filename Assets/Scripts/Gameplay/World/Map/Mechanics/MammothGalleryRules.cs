using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What [E] at a tunnel mouth of the Galerias dos Mamutes does (<see cref="MammothGalleryRules.Check"/>).</summary>
    public enum GalleryMouthCheck
    {
        /// <summary>Into this week's tunnel, out at the mouth it leads to.</summary>
        Walk = 0,

        /// <summary>This week the mammoth has dug past this mouth: it is caved in.</summary>
        CavedIn = 1,

        /// <summary>The soft barrier would throw the hero back where the tunnel comes out.</summary>
        ZoneBlocked = 2,
    }

    /// <summary>
    /// The tunnels that change of the Galerias dos Mamutes (Docs/Mapas100.md 62: "túneis que mudam: o mapa se refaz a cada semana do jogo
    /// (semente fixa por semana; online igual para todos)"), pure and server-runnable on the real clock. Six mouths open on the three roads out of
    /// the Boca do Gelo, two on each (south, east, north). Every week (UTC, Monday to Monday) the mammoth under the earth digs anew: a seed fixed
    /// by the week's number (the same for every hero and every server) pairs the six mouths into three tunnels, never two mouths of the same road,
    /// and caves one of the three in. So each week two tunnels run under the galleries and two mouths are rubble: the quick way between two roads
    /// is a different one every week.
    /// <code>
    /// mouths: 0–1 south road, 2–3 east road, 4–5 north road (near the Boca, then far) · reach 4 m · in the tunnel at 11 m/s
    /// week: floor((unix / 86400 + 3) / 7) (weeks start on Monday, 00:00 UTC) · seed: splitmix64 of the week
    /// </code>
    /// </summary>
    public static class MammothGalleryRules
    {
        public const int MouthCount = 6;
        public const float StationReach = 4f;
        public const float TunnelSpeed = 11f;
        public const double WeekSeconds = 7d * 86400d;

        /// <summary>The road a mouth opens on (0 south, 1 east, 2 north): two mouths of a road are never joined.</summary>
        public static int RoadOf(int mouth) => mouth / 2;

        /// <summary>The number of the week at <paramref name="unixSeconds"/> (weeks start on Monday, 00:00 UTC; 1970-01-01 was a Thursday).</summary>
        public static long WeekAt(double unixSeconds) => (long)Math.Floor((unixSeconds / 86400d + 3d) / 7d);

        /// <summary>Seconds until the mammoth digs again (next Monday, 00:00 UTC).</summary>
        public static double SecondsToNextWeek(double unixSeconds) => (WeekAt(unixSeconds) + 1) * WeekSeconds - 3d * 86400d - unixSeconds;

        /// <summary>
        /// The tunnels of <paramref name="week"/>: for each mouth, the mouth its tunnel leads to, or −1 when it is caved in this week. Always two
        /// tunnels between mouths of different roads and two caved-in mouths, the same for everyone.
        /// </summary>
        public static int[] TunnelsFor(long week)
        {
            ulong state = unchecked((ulong)week * 0x9E3779B97F4A7C15UL + 0x632BE59BD9B4E019UL);
            var order = new int[MouthCount];
            for (int attempt = 0; attempt < 64; attempt++)
            {
                for (int i = 0; i < MouthCount; i++)
                {
                    order[i] = i;
                }

                for (int i = MouthCount - 1; i > 0; i--)
                {
                    int j = (int)(Next(ref state) % (ulong)(i + 1));
                    (order[i], order[j]) = (order[j], order[i]);
                }

                if (RoadOf(order[0]) != RoadOf(order[1]) && RoadOf(order[2]) != RoadOf(order[3]) && RoadOf(order[4]) != RoadOf(order[5]))
                {
                    break;
                }

                if (attempt == 63)
                {
                    order = new[] { 0, 3, 1, 4, 2, 5 };
                }
            }

            var partner = new int[MouthCount];
            int caved = (int)(Next(ref state) % 3UL);
            for (int pair = 0; pair < 3; pair++)
            {
                int a = order[pair * 2];
                int b = order[pair * 2 + 1];
                partner[a] = pair == caved ? -1 : b;
                partner[b] = pair == caved ? -1 : a;
            }

            return partner;
        }

        /// <summary>What [E] at <paramref name="mouth"/> does this week.</summary>
        public static GalleryMouthCheck Check(int[] tunnels, int mouth, bool zonesOpen)
        {
            if (tunnels == null || mouth < 0 || mouth >= tunnels.Length || tunnels[mouth] < 0)
            {
                return GalleryMouthCheck.CavedIn;
            }

            return zonesOpen ? GalleryMouthCheck.Walk : GalleryMouthCheck.ZoneBlocked;
        }

        /// <summary>The mouth within <see cref="StationReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int MouthAt(IReadOnlyList<Vector2> mouths, Vector2 p)
        {
            for (int i = 0; mouths != null && i < mouths.Count; i++)
            {
                if ((mouths[i] - p).sqrMagnitude <= StationReach * StationReach)
                {
                    return i;
                }
            }

            return -1;
        }

        private static ulong Next(ref ulong state)
        {
            unchecked
            {
                state += 0x9E3779B97F4A7C15UL;
                ulong z = state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}
