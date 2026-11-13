using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One ford of the Delta de Jhilpur (<c>OverworldLayouts.DeltaFords</c>), in the region's own coordinates: the crossing point on the river's
    /// centre line, the river's direction there, its half width, half the ford's length along the river, and, for a ford on a road, the two
    /// moorings on that road, one on each bank (a boat from one goes to the other).
    /// </summary>
    public readonly struct DeltaFord
    {
        public readonly string Id;
        public readonly Vector2 Center;
        public readonly Vector2 Along;
        public readonly float HalfWidth;
        public readonly float HalfLength;
        public readonly bool HasMoorings;
        public readonly Vector2 MooringA;
        public readonly Vector2 MooringB;

        public DeltaFord(string id, Vector2 center, Vector2 along, float halfWidth, float halfLength, bool hasMoorings, Vector2 mooringA, Vector2 mooringB)
        {
            Id = id;
            Center = center;
            Along = along;
            HalfWidth = halfWidth;
            HalfLength = halfLength;
            HasMoorings = hasMoorings;
            MooringA = mooringA;
            MooringB = mooringB;
        }
    }

    /// <summary>What [E] at a ford's mooring does (<see cref="JhilpurMonsoonRules.CheckMooring"/>).</summary>
    public enum MooringCheck
    {
        /// <summary>The hero rows across alone (learned at Abiesous's school).</summary>
        Row = 0,

        /// <summary>Nuhzadag's ferry takes the hero over: a wait, then slowly.</summary>
        Ferry = 1,

        /// <summary>No monsoon: the ford is shallow, the hero wades it.</summary>
        Shallow = 2,

        /// <summary>The soft barrier would throw the hero back on the far bank.</summary>
        ZoneBlocked = 3,
    }

    /// <summary>
    /// The monsoon of the Delta de Jhilpur (Docs/Mapas100.md 31: "monção: na estação das chuvas a água sobe um andar; ruas viram canais e a cidade
    /// muda de rota; a escola de barco do Abiesous ensina a remar"), pure and server-runnable on the in-game day. Two days of every six the rains
    /// come: the six fords of the two canals go under, and the current throws back whoever wades in (the bridges stay). At the fords on the roads a
    /// mooring on each bank takes the hero over: rowing alone once the lesson at Abiesous's school (by Jhilpur's root bridge) is taken, otherwise
    /// with Nuhzadag the ferryman, who comes over and rows slowly. In the monsoon only, the high water also carries a rower from the school
    /// straight down the Canal de Jhilpur to the Tarchon ford (~1.1 km by water against ~1.8 km round by the floodplain road), and back up.
    /// <code>
    /// cycle 6 in-game days (20 real minutes each): days 5 and 6 are the monsoon (40 real minutes of every 2 hours)
    /// flooded ford: the current pushes 7 m/s towards the bank the hero came from (more than the 5.5 m/s walk)
    /// mooring: reach 4 m · row 6 m/s · ferry: 10 s wait, then 3 m/s · lesson: 6 s standing by Abiesous (once, saved)
    /// monsoon run: 9 m/s down (≈ 2 min), 5 m/s back up
    /// </code>
    /// </summary>
    public static class JhilpurMonsoonRules
    {
        public const int CycleDays = 6;
        public const int MonsoonDays = 2;
        public const float FordMargin = 3f;
        public const float PushSpeed = 7f;
        public const float MooringReach = 4f;
        public const float RowSpeed = 6f;
        public const float FerrySpeed = 3f;
        public const float FerryWaitSeconds = 10f;
        public const float LessonSeconds = 6f;
        public const float LessonLeash = 5f;
        public const float RunDownSpeed = 9f;
        public const float RunUpSpeed = 5f;

        /// <summary>The time mark (unix seconds) of the rowing lesson: set once, for good.</summary>
        public const string RowingKey = "delta.rowing.learned";

        /// <summary>Whether in-game day <paramref name="day"/> (1 = the first) is a monsoon day.</summary>
        public static bool IsMonsoon(int day) => day >= 1 && (day - 1) % CycleDays >= CycleDays - MonsoonDays;

        /// <summary>In-game days until the next monsoon day (0 in the monsoon).</summary>
        public static int DaysUntilMonsoon(int day)
        {
            for (int k = 0; k < CycleDays; k++)
            {
                if (IsMonsoon(Mathf.Max(1, day) + k))
                {
                    return k;
                }
            }

            return 0;
        }

        /// <summary>The right-hand side of a river's direction (the side of <c>MooringB</c> for the region's fords).</summary>
        public static Vector2 Normal(Vector2 along) => new Vector2(along.y, -along.x);

        /// <summary>Metres from the ford's centre line to <paramref name="p"/>, signed (+ towards <see cref="Normal"/>).</summary>
        public static float Across(DeltaFord ford, Vector2 p) => Vector2.Dot(p - ford.Center, Normal(ford.Along));

        /// <summary>Whether <paramref name="p"/> stands in the ford's water (the crossing and a margin along the river, the river's width across).</summary>
        public static bool InFordWater(DeltaFord ford, Vector2 p)
        {
            Vector2 d = p - ford.Center;
            return Mathf.Abs(Vector2.Dot(d, ford.Along)) <= ford.HalfLength && Mathf.Abs(Vector2.Dot(d, Normal(ford.Along))) <= ford.HalfWidth;
        }

        /// <summary>The ford whose water is under <paramref name="p"/>, or −1.</summary>
        public static int FordAt(IReadOnlyList<DeltaFord> fords, Vector2 p)
        {
            for (int i = 0; fords != null && i < fords.Count; i++)
            {
                if (InFordWater(fords[i], p))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The bank side of <paramref name="p"/> (+1 on the Normal side, −1 on the other).</summary>
        public static int SideOf(DeltaFord ford, Vector2 p) => Across(ford, p) >= 0f ? 1 : -1;

        /// <summary>One frame of the flooded ford's current: back towards bank <paramref name="side"/>, square to the river.</summary>
        public static Vector2 PushStep(DeltaFord ford, int side, float deltaTime) => Normal(ford.Along) * (side >= 0 ? 1f : -1f) * PushSpeed * Mathf.Max(0f, deltaTime);

        /// <summary>
        /// The mooring within <see cref="MooringReach"/> of <paramref name="p"/>: the ford's index (−1 for none) and whether it is the A one (the
        /// boat then goes to B).
        /// </summary>
        public static int MooringAt(IReadOnlyList<DeltaFord> fords, Vector2 p, out bool atA)
        {
            atA = false;
            float reach = MooringReach * MooringReach;
            for (int i = 0; fords != null && i < fords.Count; i++)
            {
                if (!fords[i].HasMoorings)
                {
                    continue;
                }

                if ((fords[i].MooringA - p).sqrMagnitude <= reach)
                {
                    atA = true;
                    return i;
                }

                if ((fords[i].MooringB - p).sqrMagnitude <= reach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>What [E] at a mooring does.</summary>
        public static MooringCheck CheckMooring(bool monsoon, bool rower, bool zonesOpen)
        {
            if (!monsoon)
            {
                return MooringCheck.Shallow;
            }

            if (!zonesOpen)
            {
                return MooringCheck.ZoneBlocked;
            }

            return rower ? MooringCheck.Row : MooringCheck.Ferry;
        }

        /// <summary>Whether the lesson is done after <paramref name="seconds"/> standing by Abiesous.</summary>
        public static bool LessonDone(float seconds) => seconds >= LessonSeconds;

        /// <summary>Whether the rowing lesson is in the save (a time mark &gt; 0).</summary>
        public static bool Learned(long mark) => mark > 0L;
    }
}
