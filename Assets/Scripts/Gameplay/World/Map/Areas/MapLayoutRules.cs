using System.Collections.Generic;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// Pure geometry and validation of a <see cref="MapLayout"/> (EditMode-tested): distances to trails and rivers, the
    /// "is this spot free for a prop" test used by the deterministic scatter, where the river blocks (everything but its
    /// crossings) and the invariants every playable map must hold (spawns outside the safe zone, exits on the edge,
    /// arrivals inside the walls and outside the exits, level bands inside the catalog band).
    /// </summary>
    public static class MapLayoutRules
    {
        /// <summary>Spawn clearings keep this far (m) from the safe zone: monsters never spawn at the city gate.</summary>
        public const float SpawnSafeMargin = 10f;

        /// <summary>Props keep off the invisible walls by this much.</summary>
        public const float WallMargin = 1.5f;

        /// <summary>Length of one river blocker piece (m).</summary>
        public const float RiverPieceLength = 3f;

        public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 1e-6f)
            {
                return Vector2.Distance(p, a);
            }

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
            return Vector2.Distance(p, a + ab * t);
        }

        public static float DistanceToPolyline(Vector2 p, IReadOnlyList<Vector2> points)
        {
            if (points == null || points.Count == 0)
            {
                return float.MaxValue;
            }

            if (points.Count == 1)
            {
                return Vector2.Distance(p, points[0]);
            }

            float best = float.MaxValue;
            for (int i = 0; i + 1 < points.Count; i++)
            {
                best = Mathf.Min(best, DistanceToSegment(p, points[i], points[i + 1]));
            }

            return best;
        }

        public static bool OnPath(MapLayout layout, Vector2 p, float clearance)
        {
            foreach (MapPath path in layout.Paths)
            {
                if (DistanceToSegment(p, path.A, path.B) <= path.Width * 0.5f + clearance)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool InRiver(MapLayout layout, Vector2 p, float clearance)
        {
            foreach (MapRiver river in layout.Rivers)
            {
                if (DistanceToPolyline(p, river.Points) <= river.Width * 0.5f + clearance)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>True when <paramref name="p"/> is on a crossing of <paramref name="river"/> (bridge deck or ford), inflated by <paramref name="extra"/>.</summary>
        public static bool OnCrossing(MapRiver river, Vector2 p, float extra = 0f)
        {
            foreach (MapCrossing crossing in river.Crossings)
            {
                if (Vector2.Distance(p, crossing.At) <= crossing.Width * 0.5f + extra)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The spot is free for a wild prop (not on a trail, the river, a clearing, an exit, a landmark, the settlement).</summary>
        public static bool IsFreeForProp(MapLayout layout, Vector2 p, float clearance, bool backdrop = false)
        {
            if (!backdrop)
            {
                Rect inner = CityGeometry.Inflate(layout.Bounds, -WallMargin);
                if (!inner.Contains(p))
                {
                    return false;
                }
            }

            if (CityGeometry.Inflate(layout.SafeZone, 2f + clearance).Contains(p))
            {
                return false;
            }

            if (OnPath(layout, p, clearance + 0.6f) || InRiver(layout, p, clearance + 0.8f))
            {
                return false;
            }

            foreach (MapSpawnZone zone in layout.Zones)
            {
                if (Vector2.Distance(p, zone.Center) < zone.Radius * 0.65f + clearance)
                {
                    return false;
                }
            }

            foreach (MapExitSpec exit in layout.Exits)
            {
                if (CityGeometry.Inflate(exit.Trigger, 4f).Contains(p) || Vector2.Distance(p, exit.Arrival) < 3f)
                {
                    return false;
                }
            }

            foreach (MapLandmark landmark in layout.Landmarks)
            {
                if (Vector2.Distance(p, landmark.Position) < 4.5f * Mathf.Max(0.5f, landmark.Scale) + clearance)
                {
                    return false;
                }
            }

            if (Vector2.Distance(p, layout.DockArrival) < 3f)
            {
                return false;
            }

            foreach (CityProp other in layout.WildProps)
            {
                if ((other.Position - p).sqrMagnitude < (clearance + 0.9f) * (clearance + 0.9f))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The river blocker pieces (centre, length, yaw): the polyline cut in <see cref="RiverPieceLength"/> steps, minus the
        /// steps on a crossing. The hero (and the beasts) can only cross at bridges and fords.
        /// </summary>
        public static List<(Vector2 Center, float Length, float Yaw)> RiverBlockers(MapRiver river)
        {
            var pieces = new List<(Vector2, float, float)>();
            if (river == null)
            {
                return pieces;
            }

            for (int i = 0; i + 1 < river.Points.Count; i++)
            {
                Vector2 a = river.Points[i];
                Vector2 b = river.Points[i + 1];
                float length = Vector2.Distance(a, b);
                if (length < 0.01f)
                {
                    continue;
                }

                int steps = Mathf.Max(1, Mathf.CeilToInt(length / RiverPieceLength));
                float step = length / steps;
                Vector2 d = (b - a) / length;
                float yaw = CityGeometry.YawFacing(d);
                for (int s = 0; s < steps; s++)
                {
                    Vector2 c = a + d * (step * (s + 0.5f));
                    if (OnCrossing(river, c, step * 0.5f))
                    {
                        continue;
                    }

                    pieces.Add((c, step + 0.35f, yaw));
                }
            }

            return pieces;
        }

        /// <summary>Every problem of <paramref name="layout"/> against its catalog entry (empty = valid).</summary>
        public static List<string> Validate(MapLayout layout)
        {
            var problems = new List<string>();
            if (layout == null)
            {
                problems.Add("null layout");
                return problems;
            }

            MapDefinition map = MapCatalog.Get(layout.MapId);
            if (map == null)
            {
                problems.Add($"{layout.MapId}: not in MapCatalog");
            }

            if (layout.Settlement == null)
            {
                problems.Add($"{layout.MapId}: no safe settlement");
            }

            if (layout.SafeZone.width <= 0f || !layout.Bounds.Overlaps(layout.SafeZone))
            {
                problems.Add($"{layout.MapId}: safe zone outside the map");
            }

            if (!layout.SafeZone.Contains(layout.DockArrival))
            {
                problems.Add($"{layout.MapId}: the Barca leaves the hero outside the safe zone");
            }

            if (!layout.Exits.Exists(e => e.Kind == MapExitKind.Dock))
            {
                problems.Add($"{layout.MapId}: no Cais da Barca");
            }

            foreach (MapExitSpec exit in layout.Exits)
            {
                if (!layout.Bounds.Contains(exit.Arrival))
                {
                    problems.Add($"{layout.MapId}/{exit.Id}: arrival outside the walls");
                }

                if (exit.Trigger.Contains(exit.Arrival))
                {
                    problems.Add($"{layout.MapId}/{exit.Id}: arrival inside its own trigger");
                }

                if (exit.Kind != MapExitKind.Dock && !TouchesEdge(layout.Bounds, exit.Trigger))
                {
                    problems.Add($"{layout.MapId}/{exit.Id}: border exit not on the map edge");
                }

                if (exit.Kind == MapExitKind.Map && string.IsNullOrEmpty(exit.TargetMapId))
                {
                    problems.Add($"{layout.MapId}/{exit.Id}: no target map");
                }

                if (InRiver(layout, exit.Arrival, 0.5f))
                {
                    problems.Add($"{layout.MapId}/{exit.Id}: arrival in the river");
                }
            }

            if (layout.Zones.Count == 0)
            {
                problems.Add($"{layout.MapId}: no spawners");
            }

            foreach (MapSpawnZone zone in layout.Zones)
            {
                float toSafe = DistanceToRect(zone.Center, layout.SafeZone) - zone.Radius;
                if (toSafe < SpawnSafeMargin)
                {
                    problems.Add($"{layout.MapId}/{zone.Id}: spawns {toSafe:0.0} m from the safe zone (< {SpawnSafeMargin})");
                }

                if (!CityGeometry.Inflate(layout.Bounds, -1f).Contains(zone.Center))
                {
                    problems.Add($"{layout.MapId}/{zone.Id}: outside the map");
                }

                if (map != null && (zone.MinLevel < map.MonsterLevelMin || zone.MaxLevel > map.MonsterLevelMax || zone.MinLevel > zone.MaxLevel))
                {
                    problems.Add($"{layout.MapId}/{zone.Id}: level band {zone.MinLevel}-{zone.MaxLevel} outside the map's {map.MonsterLevelMin}-{map.MonsterLevelMax}");
                }

                if (zone.Day == null || zone.Day.Length == 0 || zone.Night == null || zone.Night.Length == 0)
                {
                    problems.Add($"{layout.MapId}/{zone.Id}: needs a day and a night table");
                }

                if (InRiver(layout, zone.Center, 0f))
                {
                    problems.Add($"{layout.MapId}/{zone.Id}: centre in the river");
                }
            }

            foreach (CityProp prop in layout.WildProps)
            {
                if (layout.Bounds.Contains(prop.Position) && (OnPath(layout, prop.Position, 0f) || CityGeometry.Inflate(layout.SafeZone, 1f).Contains(prop.Position)))
                {
                    problems.Add($"{layout.MapId}: wild prop on a trail or in the settlement at {prop.Position}");
                    break;
                }
            }

            return problems;
        }

        public static float DistanceToRect(Vector2 p, Rect r) => CityGeometry.DistanceToRect(p, r);

        private static bool TouchesEdge(Rect bounds, Rect trigger)
        {
            const float e = 0.01f;
            return trigger.xMin <= bounds.xMin + e || trigger.xMax >= bounds.xMax - e || trigger.yMin <= bounds.yMin + e || trigger.yMax >= bounds.yMax - e;
        }
    }
}
