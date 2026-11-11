using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The wandering reed islands of map 33 (Docs/Mapas100.md 33), pure and server-runnable on the UTC clock (<c>t</c> in seconds); the eight stations
    /// and the bog are <c>OverworldLayouts.ReedIslandStations</c> and <c>ReedBogCenter/Radius</c>.
    /// <code>
    /// 4 islands of 14 m on a ring of 8 stations; every 15 min each one drifts on to the next station (over 2 min, then it holds, bobbing 1.2 m)
    /// standing on an island the hero is carried along with it
    /// off the islands, inside the bog: −30% speed (none with the Reed Boat or an aquatic mount)
    /// every 15 min one island (taking turns) carries a caged spark: free it once per layout, for an item
    /// </code>
    /// No damage. The bog lies off every road and only slows; an island is firm ground the hero can step off whenever they like, and it drifts at
    /// well under walking speed, so nothing here can hold or trap anyone.
    /// </summary>
    public static class DeltaIslandRules
    {
        public const int Islands = 4;
        public const int Stations = 8;
        public const double LayoutSeconds = 900d;
        public const double DriftSeconds = 120d;
        public const float IslandRadius = 14f;
        public const float BobRadius = 1.2f;
        public const float BobRate = 0.25f;
        public const float BogMoveBonus = -0.3f;
        public const float SparkReach = 3.5f;

        public static readonly string[] SparkItems = { "item_mana_potion", "item_herb", "item_health_potion" };

        private static int Mod(long value, int length) => (int)(((value % length) + length) % length);

        public static long LayoutIndex(double t) => (long)Math.Floor(t / LayoutSeconds);

        public static bool IsDrifting(double t) => t - LayoutIndex(t) * LayoutSeconds < DriftSeconds;

        public static int StationOf(int island, long layout) => Mod(island * (Stations / Islands) + layout, Stations);

        /// <summary>The island's centre at <paramref name="t"/>: its glide between stations and its bob.</summary>
        public static Vector2 Position(IReadOnlyList<Vector2> stations, int island, double t)
        {
            long layout = LayoutIndex(t);
            double second = t - layout * LayoutSeconds;
            Vector2 to = stations[StationOf(island, layout)];
            Vector2 at = to;
            if (second < DriftSeconds)
            {
                float f = (float)(second / DriftSeconds);
                at = Vector2.Lerp(stations[StationOf(island, layout - 1)], to, f * f * (3f - 2f * f));
            }

            // Wrapped while still a double: at Unix seconds a float only holds steps of 32 radians, and the island would jump.
            float a = (float)((t * BobRate) % (2d * Math.PI)) + island;
            return at + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * BobRadius;
        }

        /// <summary>The way the island moved over the last <paramref name="deltaTime"/> seconds (what it carries the hero by).</summary>
        public static Vector2 Carry(IReadOnlyList<Vector2> stations, int island, double t, float deltaTime) => Position(stations, island, t) - Position(stations, island, t - deltaTime);

        public static int IslandAt(IReadOnlyList<Vector2> stations, double t, Vector2 p)
        {
            for (int i = 0; i < Islands; i++)
            {
                if (Vector2.Distance(p, Position(stations, i, t)) <= IslandRadius)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>True when <paramref name="p"/> is bog that slows: inside the circle and off the islands.</summary>
        public static bool InBog(Vector2 center, float radius, Vector2 p, int island) => island < 0 && Vector2.Distance(p, center) <= radius;

        /// <summary>The bog takes nobody with the Reed Boat or an aquatic mount.</summary>
        public static bool BogSlows(bool inBog, bool hasBoat, bool ridingAquatic) => inBog && !hasBoat && !ridingAquatic;

        public static int SparkIsland(long layout) => Mod(layout, Islands);

        public static bool CanFreeSpark(long lastFreedLayout, long layout) => lastFreedLayout != layout;

        public static string SparkItem(long layout) => SparkItems[Mod(layout, SparkItems.Length)];
    }
}
