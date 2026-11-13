using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One lock of the Canal das Lanternas (<c>OverworldLayouts.CanalLocks</c>), in the region's own coordinates: the gate across the canal from
    /// <see cref="WestEnd"/> to <see cref="EastEnd"/> through <see cref="Center"/>, and how far along the drum's barge course the gate stands
    /// (<see cref="CourseDistance"/>; the two locks north of the north bridge are beyond the course's end, so no barge ever reaches them).
    /// </summary>
    public readonly struct CanalLock
    {
        public readonly string Id;
        public readonly Vector2 Center;
        public readonly Vector2 WestEnd;
        public readonly Vector2 EastEnd;
        public readonly float CourseDistance;

        public CanalLock(string id, Vector2 center, Vector2 westEnd, Vector2 eastEnd, float courseDistance)
        {
            Id = id;
            Center = center;
            WestEnd = westEnd;
            EastEnd = eastEnd;
            CourseDistance = courseDistance;
        }
    }

    /// <summary>The drum's two barges (<see cref="DrumBargeRules"/>): up the canal from the Cais do Tambor, down it from the Cais da Ponte Norte.</summary>
    public enum BargeLine
    {
        North = 0,
        South = 1,
    }

    /// <summary>What [E] at the end of a lock gate does (<see cref="CanalLockRules.Check"/>).</summary>
    public enum LockCheck
    {
        /// <summary>The gate is shut: across on top of it.</summary>
        Cross = 0,

        /// <summary>The gate is open (by day): turn the wheel and shut it.</summary>
        Turn = 1,

        /// <summary>A barge is in the lock: the keeper keeps it open.</summary>
        BargePassing = 2,

        /// <summary>The soft barrier would throw the hero back on the far bank.</summary>
        ZoneBlocked = 3,
    }

    /// <summary>
    /// The barges of the drum on the Canal das Lanternas (Docs/Mapas100.md 41: "portões por hora: abrem e fecham com o tambor (dia) e o sino
    /// (noite); Barcas de canal ligam os bairros"), pure on the in-game hour so a server runs the same timetable. While the drum rules the day (6 h
    /// to 20 h), a barge leaves the Cais do Tambor (over the east bridge, on the east road) up the canal at every even hour from 6 to 18, and one
    /// leaves the Cais da Ponte Norte down it at every odd hour from 7 to 19: boarding opens <see cref="BoardLeadHours"/> before, and the barge
    /// carries the hero ~540 m up or down the canal (past the Eclusa da Lua) instead of the ~900 m of roads by the Eclusa's spur. When the bell
    /// rings at 20 h no barge leaves until the drum.
    /// <code>
    /// in-game hour = day length / 24 (50 real s by default) · board 0.4 h before (20 s), the barge leaves on the hour at 6 m/s (≈ 90 s)
    /// north: 6, 8, 10, 12, 14, 16, 18 h · south: 7, 9, 11, 13, 15, 17, 19 h · none from 20 h to 6 h
    /// </code>
    /// </summary>
    public static class DrumBargeRules
    {
        public const float DrumHour = 6f;
        public const float BellHour = 20f;
        public const float BoardLeadHours = 0.4f;
        public const float Speed = 6f;
        public const float LandingReach = 4f;
        public const float BoardLeash = 5f;

        /// <summary>Departures from the Cais do Tambor (up the canal).</summary>
        public static readonly int[] NorthDepartures = { 6, 8, 10, 12, 14, 16, 18 };

        /// <summary>Departures from the Cais da Ponte Norte (down the canal).</summary>
        public static readonly int[] SouthDepartures = { 7, 9, 11, 13, 15, 17, 19 };

        public static int[] Departures(BargeLine line) => line == BargeLine.North ? NorthDepartures : SouthDepartures;

        /// <summary>Whether the drum rules the hour (barges run, locks open); the bell rules the rest.</summary>
        public static bool IsDrumHour(float hour) => hour >= DrumHour && hour < BellHour;

        /// <summary>The departure boarding at <paramref name="hour"/> on <paramref name="line"/>, or −1.</summary>
        public static int BoardingDeparture(BargeLine line, float hour)
        {
            foreach (int departure in Departures(line))
            {
                if (hour >= departure - BoardLeadHours && hour < departure)
                {
                    return departure;
                }
            }

            return -1;
        }

        /// <summary>The next departure at or after <paramref name="hour"/> on <paramref name="line"/> today, or −1 (after the last one).</summary>
        public static int NextDeparture(BargeLine line, float hour)
        {
            foreach (int departure in Departures(line))
            {
                if (departure > hour)
                {
                    return departure;
                }
            }

            return -1;
        }

        /// <summary>Metres the barge that left at <paramref name="departure"/> has gone by <paramref name="hour"/> (negative before it leaves).</summary>
        public static float Travelled(float hour, int departure, float secondsPerHour) => (hour - departure) * secondsPerHour * Speed;

        /// <summary>
        /// The barges of <paramref name="line"/> under way at <paramref name="hour"/> on a course of <paramref name="length"/> m: each one's distance from its own
        /// landing (into <paramref name="found"/>, cleared first). A barge boarding at its landing counts at 0.
        /// </summary>
        public static void UnderWay(BargeLine line, float hour, float secondsPerHour, float length, List<float> found)
        {
            found.Clear();
            foreach (int departure in Departures(line))
            {
                float d = Travelled(hour, departure, secondsPerHour);
                if (hour >= departure - BoardLeadHours && d <= length)
                {
                    found.Add(Mathf.Clamp(d, 0f, length));
                }
            }
        }

        /// <summary>The seconds a ride of <paramref name="length"/> m takes.</summary>
        public static float RideSeconds(float length) => length / Speed;
    }

    /// <summary>
    /// The four locks of the Canal das Lanternas (Docs/Mapas100.md 43: "eclusas: abrir e fechar as eclusas muda o nível da água e abre os bairros
    /// baixos"; 41: "o sino fecha os anéis de fora"), pure. The canal is crossed only at two stone bridges; a shut lock gate is a third, fourth…
    /// way over. By day the gates stand open for the barges: at either end of a gate the hero turns the keeper's wheel and shuts it for
    /// <see cref="ShutSeconds"/>, then walks over on top of it; a barge coming through (within <see cref="BargeClearance"/>) opens it again, and the
    /// wheel will not turn while one is in the lock. At night the bell shuts them all: every gate is a crossing until the drum. The two locks
    /// north of the north bridge are where no bridge crosses at all.
    /// <code>
    /// gate ends 15 m off the centre line (30 m over the gate at 4 m/s) · reach 4 m · wheel: shut for 150 real s, by day
    /// bell (20 h–6 h): all shut · a barge within 45 m (along its course) keeps the gate open
    /// </code>
    /// </summary>
    public static class CanalLockRules
    {
        public const float EndReach = 4f;
        public const float ShutSeconds = 150f;
        public const float CrossSpeed = 4f;
        public const float BargeClearance = 45f;

        /// <summary>Whether the bell has every gate shut at <paramref name="hour"/>.</summary>
        public static bool BellShut(float hour) => !DrumBargeRules.IsDrumHour(hour);

        /// <summary>Whether the gate is shut (walkable): the bell or the wheel, unless a barge is in the lock.</summary>
        public static bool IsShut(float hour, double now, double wheelUntil, bool bargeNear) => !bargeNear && (BellShut(hour) || now < wheelUntil);

        /// <summary>What [E] at a gate's end does.</summary>
        public static LockCheck Check(bool shut, bool bargeNear, bool zonesOpen)
        {
            if (bargeNear)
            {
                return LockCheck.BargePassing;
            }

            if (!shut)
            {
                return LockCheck.Turn;
            }

            return zonesOpen ? LockCheck.Cross : LockCheck.ZoneBlocked;
        }

        /// <summary>Whether a barge <paramref name="bargeDistance"/> m along its course is in the lock at <paramref name="lockDistance"/> m.</summary>
        public static bool BargeNear(float bargeDistance, float lockDistance) => Mathf.Abs(bargeDistance - lockDistance) <= BargeClearance;

        /// <summary>The lock with an end within <see cref="EndReach"/> of <paramref name="p"/> (−1 for none) and whether it is the west end.</summary>
        public static int EndAt(IReadOnlyList<CanalLock> locks, Vector2 p, out bool west)
        {
            west = false;
            float reach = EndReach * EndReach;
            for (int i = 0; locks != null && i < locks.Count; i++)
            {
                if ((locks[i].WestEnd - p).sqrMagnitude <= reach)
                {
                    west = true;
                    return i;
                }

                if ((locks[i].EastEnd - p).sqrMagnitude <= reach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Metres along the polyline <paramref name="points"/> to the point of it nearest <paramref name="p"/>; −1 when that point is farther than
        /// <see cref="BargeClearance"/> from <paramref name="p"/> (the lock is off the course).
        /// </summary>
        public static float DistanceAlong(Vector2[] points, Vector2 p)
        {
            float best = float.MaxValue;
            float bestAlong = -1f;
            float walked = 0f;
            for (int i = 0; points != null && i + 1 < points.Length; i++)
            {
                Vector2 a = points[i];
                Vector2 ab = points[i + 1] - a;
                float leg = ab.magnitude;
                float t = leg > 1e-4f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / (leg * leg)) : 0f;
                float d = (a + ab * t - p).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestAlong = walked + leg * t;
                }

                walked += leg;
            }

            return best <= BargeClearance * BargeClearance ? bestAlong : -1f;
        }
    }
}
