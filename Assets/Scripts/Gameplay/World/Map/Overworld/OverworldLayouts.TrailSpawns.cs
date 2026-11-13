using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Beasts on the roads and trails of the stitched world. The clearings keep off every trail (<see cref="ClearingFits"/>), so whoever walked the road
    /// met nothing between one clearing and the next: now, every <see cref="TrailSpawnSpacing"/> m along a trail, a small group stands on it, copied
    /// from the nearest clearing of the same area (same species by day and by night, same levels), so no area gets a beast it did not have. Only the
    /// cities (and every safe place round them), the boss arenas, the passes and the gates stay empty. They are <see cref="MapSpawnZone.AlongTrail"/>:
    /// no plateau is raised under them (the road keeps its slope) and they never feed the per-region placement (the regions on their own keep only
    /// their clearings).
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>Metres between two groups along a trail.</summary>
        public const float TrailSpawnSpacing = 70f;

        /// <summary>The reach of a group on the trail (the road is 4–8 m wide: they wander a little off it).</summary>
        public const float TrailSpawnRadius = 8f;

        /// <summary>At most this many alive in a group on the trail: a few on the way, the packs stay in the clearings.</summary>
        public const int TrailSpawnMaxAlive = 2;

        /// <summary>Candidate points this far apart along every piece of trail.</summary>
        private const float TrailSpawnStep = 10f;

        private const float TrailSpawnCell = 80f;

        private static void AddTrailSpawns(OverworldLayout w)
        {
            // The clearings of each area, to copy from.
            var clearingsOf = new Dictionary<OverworldZone, List<MapSpawnZone>>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                if (w.ZoneOfSpawn.TryGetValue(spawn.Id, out OverworldZone zone) && zone != null)
                {
                    if (!clearingsOf.TryGetValue(zone, out List<MapSpawnZone> list))
                    {
                        clearingsOf[zone] = list = new List<MapSpawnZone>();
                    }

                    list.Add(spawn);
                }
            }

            var arenas = new List<Vector2>();
            foreach (RegionBossSpawn boss in RegionBossRules.DirectorSpawns)
            {
                arenas.Add(boss.LocalArena + boss.WorldOffset);
            }

            var taken = new Dictionary<Vector2Int, List<Vector2>>();
            var clearings = new Dictionary<Vector2Int, List<MapSpawnZone>>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                Bucket(clearings, spawn.Center).Add(spawn);
            }

            var added = new List<MapSpawnZone>();
            int n = 0;
            foreach (MapPath path in w.World.Paths)
            {
                float length = Vector2.Distance(path.A, path.B);
                int steps = Mathf.Max(1, Mathf.FloorToInt(length / TrailSpawnStep));
                for (int i = 0; i <= steps; i++)
                {
                    Vector2 c = Vector2.Lerp(path.A, path.B, i / (float)steps);
                    if (Near(taken, c, TrailSpawnSpacing) || !TrailSpawnFits(w, c, arenas, clearings))
                    {
                        continue;
                    }

                    OverworldZone zone = OverworldRules.ZoneAt(w, c);
                    if (zone == null || !clearingsOf.TryGetValue(zone, out List<MapSpawnZone> own))
                    {
                        continue;
                    }

                    MapSpawnZone from = NearestOf(own, c);
                    var spawn = new MapSpawnZone
                    {
                        Id = zone.Id + "_trilha_" + (++n).ToString("000"), Center = c, Radius = TrailSpawnRadius, MinLevel = from.MinLevel, MaxLevel = from.MaxLevel,
                        MaxAlive = Mathf.Min(TrailSpawnMaxAlive, from.MaxAlive), RespawnSeconds = from.RespawnSeconds * 1.5f, Archetypes = from.Archetypes,
                        Day = from.Day, Dawn = from.Dawn, Dusk = from.Dusk, Night = from.Night, PassiveSpeciesId = from.PassiveSpeciesId,
                        PassiveBelowHeroLevel = from.PassiveBelowHeroLevel, AlongTrail = true,
                    };
                    added.Add(spawn);
                    w.ZoneOfSpawn[spawn.Id] = zone;
                    Bucket(taken, c).Add(c);
                }
            }

            w.World.Zones.AddRange(added);
        }

        private static bool TrailSpawnFits(OverworldLayout w, Vector2 c, List<Vector2> arenas, Dictionary<Vector2Int, List<MapSpawnZone>> clearings)
        {
            const float r = TrailSpawnRadius;
            if (OverworldRules.DistanceToSafety(w, c) < r + OverworldRules.SpawnSafeMargin + 8f)
            {
                return false;
            }

            foreach (OverworldSettlement settlement in w.Settlements)
            {
                if (CityGeometry.Inflate(settlement.GroundRect, r + 20f).Contains(c))
                {
                    return false;
                }
            }

            foreach (Rect authored in w.AuthoredAreas)
            {
                if (CityGeometry.Inflate(authored, r + 6f).Contains(c))
                {
                    return false;
                }
            }

            foreach (Vector2 arena in arenas)
            {
                if (Vector2.Distance(c, arena) < RegionBossRules.ArenaClearRadius + r + 20f)
                {
                    return false;
                }
            }

            foreach (Vector2 pass in w.Passes)
            {
                if (Vector2.Distance(c, pass) < r + 30f)
                {
                    return false;
                }
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                if (Vector2.Distance(c, exit.Arrival) < r + 25f)
                {
                    return false;
                }
            }

            foreach (MapLandmark landmark in w.World.Landmarks)
            {
                if (Vector2.Distance(c, landmark.Position) < r + 12f)
                {
                    return false;
                }
            }

            foreach (MapRiver river in w.World.Rivers)
            {
                if (MapLayoutRules.DistanceToPolyline(c, river.Points) < river.Width * 0.5f + r + 2f)
                {
                    return false;
                }
            }

            Vector2Int cell = CellOf(c);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (!clearings.TryGetValue(new Vector2Int(cell.x + dx, cell.y + dz), out List<MapSpawnZone> near))
                    {
                        continue;
                    }

                    foreach (MapSpawnZone clearing in near)
                    {
                        if (Vector2.Distance(c, clearing.Center) < clearing.Radius + r + 15f)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static MapSpawnZone NearestOf(List<MapSpawnZone> spawns, Vector2 p)
        {
            MapSpawnZone best = spawns[0];
            float bestD = float.MaxValue;
            foreach (MapSpawnZone s in spawns)
            {
                float d = (s.Center - p).sqrMagnitude;
                if (d < bestD)
                {
                    bestD = d;
                    best = s;
                }
            }

            return best;
        }

        private static Vector2Int CellOf(Vector2 p) => new Vector2Int(Mathf.FloorToInt(p.x / TrailSpawnCell), Mathf.FloorToInt(p.y / TrailSpawnCell));

        private static List<T> Bucket<T>(Dictionary<Vector2Int, List<T>> cells, Vector2 p)
        {
            Vector2Int cell = CellOf(p);
            if (!cells.TryGetValue(cell, out List<T> list))
            {
                cells[cell] = list = new List<T>();
            }

            return list;
        }

        private static bool Near(Dictionary<Vector2Int, List<Vector2>> cells, Vector2 p, float distance)
        {
            Vector2Int cell = CellOf(p);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (!cells.TryGetValue(new Vector2Int(cell.x + dx, cell.y + dz), out List<Vector2> points))
                    {
                        continue;
                    }

                    foreach (Vector2 q in points)
                    {
                        if (Vector2.Distance(p, q) < distance)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
