using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum CostaTidePhase
    {
        High,
        Low,
        Turning,
    }

    /// <summary>A reef flat the tide uncovers: a chest stands in its middle.</summary>
    public readonly struct CostaTideFlat
    {
        public readonly Vector2 Center;
        public readonly float Radius;

        public CostaTideFlat(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }

    /// <summary>
    /// The monsoon tide of Taa Pwani's reef (Docs/Mapas100.md 25), pure and server-runnable on the UTC clock; the flats are
    /// <c>OverworldLayouts.TideFlats</c>.
    /// <code>
    /// cycle of 8 min: 2 min 20 s low (first 20 s draining), 40 s turning (the warning: the flats are still dry), 5 min high
    /// low or turning: the chest of each flat opens, once per tide
    /// high, on a flat: −40% speed and a bite of 3% of max life every 2 s, never below 30%; riding an aquatic mount is free of both
    /// </code>
    /// The flats lie off every road (the east road over the reef stays dry at any tide) and are at most 40 m in radius, so the 40 s of the turning
    /// are many times the walk out. Water only slows: the flat is never a wall, the hero can always walk off it, and the tide never kills.
    /// </summary>
    public static class CostaTideRules
    {
        public const long CycleSeconds = 480L;
        public const long DrainSeconds = 20L;
        public const long LowSeconds = 140L;
        public const long TurningSeconds = 40L;
        public const float BiteFraction = 0.03f;
        public const float BiteTickSeconds = 2f;
        public const float BiteFloorFraction = 0.3f;
        public const float HighTideMoveBonus = -0.4f;
        public const float ChestReach = 3f;
        public const float MaxFlatRadius = 40f;

        public static readonly string[] ChestItems = { "item_health_potion", "item_mana_potion", "item_herb" };

        public static long TideIndex(long unix) => unix >= 0L ? unix / CycleSeconds : (unix - CycleSeconds + 1L) / CycleSeconds;

        private static long Second(long unix) => unix - TideIndex(unix) * CycleSeconds;

        public static CostaTidePhase PhaseAt(long unix)
        {
            long second = Second(unix);
            if (second < LowSeconds)
            {
                return CostaTidePhase.Low;
            }

            return second < LowSeconds + TurningSeconds ? CostaTidePhase.Turning : CostaTidePhase.High;
        }

        /// <summary>Seconds until the phase changes.</summary>
        public static long SecondsToChange(long unix)
        {
            long second = Second(unix);
            switch (PhaseAt(unix))
            {
                case CostaTidePhase.Low:
                    return LowSeconds - second;
                case CostaTidePhase.Turning:
                    return LowSeconds + TurningSeconds - second;
                default:
                    return CycleSeconds - second;
            }
        }

        /// <summary>How much of a flat is under water (0 dry, 1 covered), for the ring of the water's edge.</summary>
        public static float WaterLevel(long unix)
        {
            long second = Second(unix);
            if (second < DrainSeconds)
            {
                return 1f - second / (float)DrainSeconds;
            }

            if (second < LowSeconds)
            {
                return 0f;
            }

            return second < LowSeconds + TurningSeconds ? (second - LowSeconds) / (float)TurningSeconds : 1f;
        }

        public static int FlatAt(IReadOnlyList<CostaTideFlat> flats, Vector2 p)
        {
            for (int i = 0; i < flats.Count; i++)
            {
                if (Vector2.Distance(p, flats[i].Center) <= flats[i].Radius)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>True when the water takes hold of a hero on a flat.</summary>
        public static bool Drenches(CostaTidePhase phase, bool ridingAquatic) => phase == CostaTidePhase.High && !ridingAquatic;

        public static int ChestAt(IReadOnlyList<CostaTideFlat> flats, Vector2 p)
        {
            for (int i = 0; i < flats.Count; i++)
            {
                if (Vector2.Distance(p, flats[i].Center) <= ChestReach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>A chest opens only while the flat is dry, and once per tide.</summary>
        public static bool CanOpenChest(long lastOpenedTide, long tide, CostaTidePhase phase) => phase != CostaTidePhase.High && lastOpenedTide != tide;

        public static string ChestItem(int flat, long tide)
        {
            long length = ChestItems.Length;
            return ChestItems[(int)((((flat + tide) % length) + length) % length)];
        }

        /// <summary>Health one bite of the tide takes (never past the floor).</summary>
        public static float Bite(float current, float max) => ProfundoHazardRules.Bite(current, max, BiteFraction, BiteFloorFraction);
    }
}
