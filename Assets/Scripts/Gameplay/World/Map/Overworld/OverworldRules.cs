using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Pure rules of a continuous region (EditMode-tested, server-runnable): which zone a position is in, which zones are
    /// neighbours (open passages), whether a hero may set foot in a zone (walk level with the hero terrain bonus and the
    /// special gate: the soft barrier), the chunk grid of the streaming (which chunks must exist around the hero) and the
    /// invariants a region must hold (<see cref="Validate"/>).
    /// </summary>
    public static class OverworldRules
    {
        /// <summary>Spawn clearings keep this far (m) from any safe zone.</summary>
        public const float SpawnSafeMargin = 12f;

        /// <summary>Minimum shared edge (m) for two zones to count as neighbours with an open passage.</summary>
        public const float MinPassage = 20f;

        /// <summary>Chunks whose rectangle comes this close to the hero (m) are built.</summary>
        public const float LoadRadius = 96f;

        /// <summary>Built chunks farther than this (m) are torn down (hysteresis over <see cref="LoadRadius"/>).</summary>
        public const float UnloadRadius = 136f;

        /// <summary>Spawners whose clearing edge comes this close to the hero (m) run; farther ones are despawned.</summary>
        public const float SpawnerActiveRadius = 85f;

        public const float SpawnerSleepRadius = 125f;

        // ------------------------------------------------------------------
        // Zones
        // ------------------------------------------------------------------

        /// <summary>The zone holding <paramref name="p"/> (half-open rectangles: borders belong to one zone), or null outside.</summary>
        public static OverworldZone ZoneAt(OverworldLayout layout, Vector2 p)
        {
            if (layout == null)
            {
                return null;
            }

            foreach (OverworldZone zone in layout.Zones)
            {
                if (zone.Bounds.Contains(p))
                {
                    return zone;
                }
            }

            return null;
        }

        /// <summary>The zone holding <paramref name="p"/>, else the nearest one (backdrop ground outside the walls).</summary>
        public static OverworldZone NearestZone(OverworldLayout layout, Vector2 p)
        {
            OverworldZone inside = ZoneAt(layout, p);
            if (inside != null)
            {
                return inside;
            }

            OverworldZone best = null;
            float bestDistance = float.MaxValue;
            foreach (OverworldZone zone in layout.Zones)
            {
                float d = CityGeometry.DistanceToRect(p, zone.Bounds);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = zone;
                }
            }

            return best;
        }

        /// <summary>Length of the edge two zones share (0 when they only touch at a corner or do not touch).</summary>
        public static float SharedEdge(Rect a, Rect b)
        {
            const float e = 0.01f;
            if (Mathf.Abs(a.xMax - b.xMin) < e || Mathf.Abs(b.xMax - a.xMin) < e)
            {
                return Mathf.Max(0f, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
            }

            if (Mathf.Abs(a.yMax - b.yMin) < e || Mathf.Abs(b.yMax - a.yMin) < e)
            {
                return Mathf.Max(0f, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin));
            }

            return 0f;
        }

        public static bool AreNeighbours(OverworldZone a, OverworldZone b) => a != null && b != null && a != b && SharedEdge(a.Bounds, b.Bounds) >= MinPassage;

        public static List<OverworldZone> Neighbours(OverworldLayout layout, OverworldZone zone)
        {
            var list = new List<OverworldZone>();
            foreach (OverworldZone other in layout.Zones)
            {
                if (AreNeighbours(zone, other))
                {
                    list.Add(other);
                }
            }

            return list;
        }

        /// <summary>Distance (m) from the Vila's plaza to the nearest point of <paramref name="zone"/>.</summary>
        public static float DistanceFromVila(OverworldLayout layout, OverworldZone zone)
        {
            Vector2 vila = layout.Vila != null ? layout.Vila.Offset : Vector2.zero;
            return CityGeometry.DistanceToRect(vila, zone.Bounds);
        }

        /// <summary>
        /// May a hero step into <paramref name="zone"/>: the zone's walk level (with Ícaro's ruins / Maithe's mountain bonus)
        /// and its special gate. Never gold: the overworld is walked, not sailed.
        /// </summary>
        public static MapAccessCheck CheckEnter(OverworldZone zone, int heroLevel, CharacterId? hero, Func<string, bool> hasRequirement)
        {
            if (zone == null)
            {
                return MapAccessCheck.NotBuilt;
            }

            int required = MapAccessRules.EffectiveLevelRequirement(zone.WalkLevel, hero, zone.RegionType);
            if (heroLevel < required)
            {
                return MapAccessCheck.LevelTooLow;
            }

            return MapAccessRules.MeetsRequirement(zone.RequirementTag, hasRequirement) ? MapAccessCheck.Ok : MapAccessCheck.MissingRequirement;
        }

        /// <summary>The settlement whose safe zone holds <paramref name="p"/>, or null.</summary>
        public static OverworldSettlement SettlementAt(OverworldLayout layout, Vector2 p)
        {
            foreach (OverworldSettlement settlement in layout.Settlements)
            {
                if (settlement.SafeZone.Contains(p))
                {
                    return settlement;
                }
            }

            return null;
        }

        /// <summary>Distance (m) to the nearest safe zone of the region.</summary>
        public static float DistanceToSafety(OverworldLayout layout, Vector2 p)
        {
            float best = float.MaxValue;
            foreach (OverworldSettlement settlement in layout.Settlements)
            {
                best = Mathf.Min(best, CityGeometry.DistanceToRect(p, settlement.SafeZone));
            }

            return best;
        }

        // ------------------------------------------------------------------
        // Relief
        // ------------------------------------------------------------------

        /// <summary>Height (m) of the region's relief at <paramref name="p"/> (pure, continuous; 0 at the towns and gates).</summary>
        public static float HeightAt(OverworldLayout layout, Vector2 p) => OverworldRelief.For(layout).HeightAt(p);

        /// <summary>Height (m) of the drawn and collided ground at <paramref name="p"/> (the facet under it): stand things here.</summary>
        public static float SurfaceAt(OverworldLayout layout, Vector2 p) => OverworldRelief.For(layout).SurfaceAt(p);

        // ------------------------------------------------------------------
        // Chunks
        // ------------------------------------------------------------------

        public static Vector2Int ChunkOf(OverworldLayout layout, Vector2 p)
        {
            return new Vector2Int(Mathf.FloorToInt((p.x - layout.Bounds.xMin) / layout.ChunkSize), Mathf.FloorToInt((p.y - layout.Bounds.yMin) / layout.ChunkSize));
        }

        public static Rect ChunkRect(OverworldLayout layout, Vector2Int chunk)
        {
            return new Rect(layout.Bounds.xMin + chunk.x * layout.ChunkSize, layout.Bounds.yMin + chunk.y * layout.ChunkSize, layout.ChunkSize, layout.ChunkSize);
        }

        /// <summary>Chunks along one axis inside the walls.</summary>
        public static Vector2Int ChunkCount(OverworldLayout layout)
        {
            return new Vector2Int(Mathf.CeilToInt(layout.Bounds.width / layout.ChunkSize - 0.001f), Mathf.CeilToInt(layout.Bounds.height / layout.ChunkSize - 0.001f));
        }

        /// <summary>A chunk exists from one ring outside the walls (the backdrop seen by the camera) to one ring past the far edge.</summary>
        public static bool IsValidChunk(OverworldLayout layout, Vector2Int chunk)
        {
            Vector2Int count = ChunkCount(layout);
            return chunk.x >= -1 && chunk.y >= -1 && chunk.x <= count.x && chunk.y <= count.y;
        }

        /// <summary>True when the chunk is inside the walls (gets colliders, spawners); false for the backdrop ring.</summary>
        public static bool IsInnerChunk(OverworldLayout layout, Vector2Int chunk)
        {
            Vector2Int count = ChunkCount(layout);
            return chunk.x >= 0 && chunk.y >= 0 && chunk.x < count.x && chunk.y < count.y;
        }

        /// <summary>Distance (m) from <paramref name="p"/> to the chunk's rectangle.</summary>
        public static float DistanceToChunk(OverworldLayout layout, Vector2 p, Vector2Int chunk) => CityGeometry.DistanceToRect(p, ChunkRect(layout, chunk));

        /// <summary>Every valid chunk within <paramref name="radius"/> of <paramref name="p"/> (cleared into <paramref name="result"/>), nearest first.</summary>
        public static void ChunksWithin(OverworldLayout layout, Vector2 p, float radius, List<Vector2Int> result)
        {
            result.Clear();
            Vector2Int min = ChunkOf(layout, p - new Vector2(radius, radius));
            Vector2Int max = ChunkOf(layout, p + new Vector2(radius, radius));
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (IsValidChunk(layout, c) && DistanceToChunk(layout, p, c) <= radius)
                    {
                        result.Add(c);
                    }
                }
            }

            result.Sort((a, b) => DistanceToChunk(layout, p, a).CompareTo(DistanceToChunk(layout, p, b)));
        }

        /// <summary>The chunks a spawn clearing (and the few metres its monsters wander past it) stands on (cleared into <paramref name="result"/>).</summary>
        public static void ChunksUnderClearing(OverworldLayout layout, MapSpawnZone zone, List<Vector2Int> result)
        {
            result.Clear();
            float reach = zone.Radius + 6f;
            Vector2Int min = ChunkOf(layout, zone.Center - new Vector2(reach, reach));
            Vector2Int max = ChunkOf(layout, zone.Center + new Vector2(reach, reach));
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (IsInnerChunk(layout, c) && DistanceToChunk(layout, zone.Center, c) <= reach)
                    {
                        result.Add(c);
                    }
                }
            }
        }

        /// <summary>Whether a spawner must run for a hero at <paramref name="hero"/>; <paramref name="wasActive"/> gives the hysteresis.</summary>
        public static bool SpawnerShouldRun(MapSpawnZone zone, Vector2 hero, bool wasActive)
        {
            float edge = Vector2.Distance(zone.Center, hero) - zone.Radius;
            return wasActive ? edge <= SpawnerSleepRadius : edge <= SpawnerActiveRadius;
        }

        // ------------------------------------------------------------------
        // Validation
        // ------------------------------------------------------------------

        /// <summary>Every problem of the region (empty = valid).</summary>
        public static List<string> Validate(OverworldLayout layout)
        {
            var problems = new List<string>();
            if (layout == null || layout.World == null)
            {
                problems.Add("null overworld");
                return problems;
            }

            float covered = 0f;
            for (int i = 0; i < layout.Zones.Count; i++)
            {
                OverworldZone a = layout.Zones[i];
                covered += a.Bounds.width * a.Bounds.height;
                if (!Contains(layout.Bounds, a.Bounds))
                {
                    problems.Add($"{a.Id}: outside the region");
                }

                if (a.MinLevel > a.MaxLevel || a.WalkLevel > a.MinLevel + 2)
                {
                    problems.Add($"{a.Id}: band {a.MinLevel}-{a.MaxLevel} / walk {a.WalkLevel}");
                }

                MapDefinition map = MapCatalog.Get(a.MapId);
                if (!string.IsNullOrEmpty(a.MapId) && (map == null || a.MinLevel < map.MonsterLevelMin || a.MaxLevel > map.MonsterLevelMax))
                {
                    problems.Add($"{a.Id}: band outside the catalog band of {a.MapId}");
                }

                if (MapCatalog.Get(a.MapIdOrOwner) == null)
                {
                    problems.Add($"{a.Id}: no owner map");
                }

                if (a.Fauna.Count == 0)
                {
                    problems.Add($"{a.Id}: no monsters");
                }

                if (Neighbours(layout, a).Count == 0)
                {
                    problems.Add($"{a.Id}: no open passage to any zone");
                }

                for (int j = i + 1; j < layout.Zones.Count; j++)
                {
                    if (a.Bounds.Overlaps(layout.Zones[j].Bounds))
                    {
                        problems.Add($"{a.Id} overlaps {layout.Zones[j].Id}");
                    }
                }
            }

            if (Mathf.Abs(covered - layout.Bounds.width * layout.Bounds.height) > 1f)
            {
                problems.Add($"the zones cover {covered:0} m² of {layout.Bounds.width * layout.Bounds.height:0} m²");
            }

            foreach (MapSpawnZone spawn in layout.World.Zones)
            {
                if (!layout.ZoneOfSpawn.TryGetValue(spawn.Id, out OverworldZone zone))
                {
                    problems.Add($"{spawn.Id}: belongs to no zone");
                    continue;
                }

                if (!zone.Bounds.Contains(spawn.Center))
                {
                    problems.Add($"{spawn.Id}: centre outside {zone.Id}");
                }

                if (spawn.MinLevel < zone.MinLevel || spawn.MaxLevel > zone.MaxLevel || spawn.MinLevel > spawn.MaxLevel)
                {
                    problems.Add($"{spawn.Id}: band {spawn.MinLevel}-{spawn.MaxLevel} outside {zone.Id} {zone.MinLevel}-{zone.MaxLevel}");
                }

                float toSafe = DistanceToSafety(layout, spawn.Center) - spawn.Radius;
                if (toSafe < MapLayoutRules.SpawnSafeMargin)
                {
                    problems.Add($"{spawn.Id}: {toSafe:0.0} m from a safe zone");
                }

                if (MapLayoutRules.InRiver(layout.World, spawn.Center, 0f))
                {
                    problems.Add($"{spawn.Id}: centre in a river");
                }

                if (spawn.Day == null || spawn.Day.Length == 0 || spawn.Night == null || spawn.Night.Length == 0)
                {
                    problems.Add($"{spawn.Id}: needs a day and a night table");
                }
            }

            foreach (MapExitSpec exit in layout.World.Exits)
            {
                if (!layout.Bounds.Contains(exit.Arrival))
                {
                    problems.Add($"{exit.Id}: arrival outside the walls");
                }

                if (exit.Trigger.Contains(exit.Arrival))
                {
                    problems.Add($"{exit.Id}: arrival inside its own trigger");
                }

                if (exit.Kind == MapExitKind.Map && (layout.ContainsMap(exit.TargetMapId) || MapCatalog.Get(exit.TargetMapId) == null))
                {
                    problems.Add($"{exit.Id}: a gate must lead out of the region ({exit.TargetMapId})");
                }

                if (MapLayoutRules.InRiver(layout.World, exit.Arrival, 0.5f))
                {
                    problems.Add($"{exit.Id}: arrival in a river");
                }
            }

            foreach (OverworldSettlement settlement in layout.Settlements)
            {
                if (settlement.HasDock && !settlement.SafeZone.Contains(settlement.DockArrival))
                {
                    problems.Add($"{settlement.Id}: the Barca leaves the hero outside the safe zone");
                }

                if (ZoneAt(layout, settlement.Offset)?.MapId != settlement.MapId)
                {
                    problems.Add($"{settlement.Id}: not inside a zone of {settlement.MapId}");
                }
            }

            if (layout.Vila == null || !layout.Vila.IsReturnCity)
            {
                problems.Add("no return city");
            }

            return problems;
        }

        private static bool Contains(Rect outer, Rect inner)
        {
            const float e = 0.01f;
            return inner.xMin >= outer.xMin - e && inner.yMin >= outer.yMin - e && inner.xMax <= outer.xMax + e && inner.yMax <= outer.yMax + e;
        }
    }
}
