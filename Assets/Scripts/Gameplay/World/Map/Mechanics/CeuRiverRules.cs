using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What [E] at a buoy of the Rio de Luz does (<see cref="CeuRiverRules.Check"/>).</summary>
    public enum CeuRiverCheck
    {
        /// <summary>The buoy is dark: [E] lights it (for ever).</summary>
        Light = 0,

        /// <summary>A lit buoy lies that way: [E] steps onto the light and the current carries the hero there.</summary>
        Ride = 1,

        /// <summary>No lit buoy that way (the hero faces the mouth, or the buoys beyond are still dark).</summary>
        NoBuoy = 2,
        ZoneBlocked = 3,
        Resting = 4,
    }

    /// <summary>
    /// Walking on the Rio de Luz (Docs/Mapas100.md 92: "o herói anda sobre a luz; a correnteza leva (rápido a favor, lento contra)"; "Noite: o rio
    /// fica prateado (Lua)"), pure and server-runnable. Five buoys stand on the south bank, from the west reach to the east reach, two of them
    /// beside the bridges' roads. [E] at a dark buoy lights it for ever (a time mark in <c>WorldMapProgress</c>); [E] at a lit buoy steps onto the
    /// light and the current carries the hero to the next lit buoy the way they face: east, with the current, fast; west, against it, slowly
    /// but out of the reach of the beasts of the banks. The river row has no road from end to end, so the current is its only way along.
    /// <code>
    /// 5 buoys, reach 4 m, on the south bank 11–12 m from the centre line (the river is 10 m wide) · x −1140, −510, −14, 450, 1140
    /// with the current (east) 15 m/s, 18 m/s at night (the silver river) · against it (west) 6 m/s; on foot 5.5 m/s cross-country
    /// the hops: ≈ 655, 521, 488, 715 m (≈ 44, 35, 33, 48 s with the current, 27–40 s at night; ≈ 109, 87, 81, 119 s against it)
    /// the current goes past the dark buoys to the next lit one · 3 s between rides · the hero rides 0.4 m over the light
    /// </code>
    /// </summary>
    public static class CeuRiverRules
    {
        public const float BuoyReach = 4f;
        public const float DownstreamSpeed = 15f;
        public const float NightDownstreamSpeed = 18f;
        public const float UpstreamSpeed = 6f;
        public const float Hover = 0.4f;
        public const float CooldownSeconds = 3f;

        /// <summary>The save key of a buoy (a time mark: the unix second it was lit, 0 = dark).</summary>
        public static string LitKey(int buoy) => "ceu.river.buoy." + buoy;

        public static bool IsLit(long litAt) => litAt > 0L;

        /// <summary>Speed (m/s) of the ride: with the current (east) or against it.</summary>
        public static float Speed(bool downstream, bool night) => downstream ? (night ? NightDownstreamSpeed : DownstreamSpeed) : UpstreamSpeed;

        /// <summary>The buoy within <see cref="BuoyReach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int BuoyAt(IReadOnlyList<Vector2> buoys, Vector2 p)
        {
            int best = -1;
            float bestSq = BuoyReach * BuoyReach;
            for (int i = 0; buoys != null && i < buoys.Count; i++)
            {
                float sq = (buoys[i] - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Which way the hero faces along the river (the buoys run west to east): true = east, with the current.</summary>
        public static bool FacesDownstream(Vector2 facing) => facing.x >= 0f;

        /// <summary>The next lit buoy from <paramref name="from"/> the way chosen (buoys ordered west to east), or −1.</summary>
        public static int NextLit(IReadOnlyList<long> litMarks, int from, bool downstream)
        {
            int step = downstream ? 1 : -1;
            for (int i = from + step; litMarks != null && i >= 0 && i < litMarks.Count; i += step)
            {
                if (IsLit(litMarks[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>What [E] at buoy <paramref name="buoy"/> does.</summary>
        public static CeuRiverCheck Check(IReadOnlyList<long> litMarks, int buoy, bool downstream, bool zonesOpen, float sinceRide)
        {
            if (litMarks == null || buoy < 0 || buoy >= litMarks.Count)
            {
                return CeuRiverCheck.NoBuoy;
            }

            if (!IsLit(litMarks[buoy]))
            {
                return CeuRiverCheck.Light;
            }

            if (NextLit(litMarks, buoy, downstream) < 0)
            {
                return CeuRiverCheck.NoBuoy;
            }

            if (!zonesOpen)
            {
                return CeuRiverCheck.ZoneBlocked;
            }

            return sinceRide < CooldownSeconds ? CeuRiverCheck.Resting : CeuRiverCheck.Ride;
        }

        /// <summary>The point of the river's centre line at <paramref name="x"/> (the river runs west to east, one z for each x).</summary>
        public static Vector2 RiverAt(IReadOnlyList<Vector2> river, float x)
        {
            if (river == null || river.Count == 0)
            {
                return new Vector2(x, 0f);
            }

            if (x <= river[0].x)
            {
                return new Vector2(x, river[0].y);
            }

            for (int i = 0; i + 1 < river.Count; i++)
            {
                if (x <= river[i + 1].x)
                {
                    float t = Mathf.InverseLerp(river[i].x, river[i + 1].x, x);
                    return new Vector2(x, Mathf.Lerp(river[i].y, river[i + 1].y, t));
                }
            }

            return new Vector2(x, river[river.Count - 1].y);
        }

        /// <summary>
        /// The course from buoy <paramref name="from"/> to buoy <paramref name="to"/>: off the bank onto the light, along the centre line through
        /// every bend between, and back onto the bank at the other buoy.
        /// </summary>
        public static Vector2[] Course(IReadOnlyList<Vector2> river, IReadOnlyList<Vector2> buoys, int from, int to)
        {
            Vector2 a = buoys[from];
            Vector2 b = buoys[to];
            var course = new List<Vector2> { a, RiverAt(river, a.x) };
            bool east = b.x > a.x;
            if (east)
            {
                for (int i = 0; i < river.Count; i++)
                {
                    if (river[i].x > a.x && river[i].x < b.x)
                    {
                        course.Add(river[i]);
                    }
                }
            }
            else
            {
                for (int i = river.Count - 1; i >= 0; i--)
                {
                    if (river[i].x < a.x && river[i].x > b.x)
                    {
                        course.Add(river[i]);
                    }
                }
            }

            course.Add(RiverAt(river, b.x));
            course.Add(b);
            return course.ToArray();
        }

        /// <summary>Seconds of the ride from <paramref name="from"/> to <paramref name="to"/>.</summary>
        public static float RideSeconds(IReadOnlyList<Vector2> river, IReadOnlyList<Vector2> buoys, int from, int to, bool night) =>
            RidePath.Length(Course(river, buoys, from, to)) / Speed(buoys[to].x > buoys[from].x, night);
    }
}
