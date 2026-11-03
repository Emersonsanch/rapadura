using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The stitched world (Docs/Mapas100.md §0.10, "Mundo costurado"): the four continuous regions of this build as ONE map,
    /// walked from one to the other with no gate and no scene change. Each region keeps its own layout in its own
    /// coordinates (the Zarafshan's Vila stays at (0, 0)) and is moved here by a whole number of streamed chunks, so the four
    /// tile one 4800 × 4800 m square (2 × 2):
    /// <code>
    ///   z  1400 ┌──────────────┬──────────────┐
    ///           │ Serra de     │ Bosque do    │
    ///           │ Kaggen       │ Zarafshan    │
    ///   z -1000 ├──────────────┼──────────────┤
    ///           │ Grutas do    │ Areias       │
    ///           │ Limiar       │ Flamejantes  │
    ///   z -3400 └──────────────┴──────────────┘
    ///        x -3400        -1000           1400
    /// </code>
    /// The gates between two of them become open passes: their flat ground stays (y = 0 on both sides), the two trails are
    /// joined across the border (or the lone one is led to the nearest trail of the other region), and the cave vault of the
    /// Limiar opens there. Gates to maps outside the world, the island stone rings, the Ossuário's portal and the docks stay.
    /// Região 5 (the Costa das Monções) sits east of the Zarafshan (x 1400…3800, z −1000…1400) and Região 6 (the Delta de
    /// Jhilpur) south of it, east of the Areias (x 1400…3800, z −3400…−1000): a 3 × 2 rectangle of regions. Região 7 (the Teto do
    /// Mundo) goes on north of the Serra de Kaggen (x −3400…−1000, z 1400…3800), its two gates on the south edge led to the
    /// Serra's trails; the corners north of the Zarafshan and of the Costa, which no region covers, are holes walled off like
    /// the world's edge (<see cref="OverworldLayout.Holes"/>).
    /// The relief blends across the borders like between any two areas (one relief for the whole world, each area shaped in
    /// its own region's coordinates), the chunk grids line up (offsets are multiples of the chunk), the streaming, the soft
    /// barrier, the levels and the requirements work per area as before.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string WorldId = "overworld_mundo";

        /// <summary>Where each region sits in the world (region coordinates + offset = world): multiples of the chunk.</summary>
        public static readonly Vector2 ZarafshanWorldOffset = Vector2.zero;
        public static readonly Vector2 AreiasWorldOffset = new Vector2(200f, -1500f);
        public static readonly Vector2 KaggenWorldOffset = new Vector2(-1700f, -100f);
        public static readonly Vector2 LimiarWorldOffset = new Vector2(-2200f, -1300f);

        /// <summary>Região 5 east of the Zarafshan (Taa Pwani at world (2000, 200)): the world becomes an L, its south-east corner a hole.</summary>
        public static readonly Vector2 CostaWorldOffset = new Vector2(2000f, 200f);

        /// <summary>
        /// Região 6 south of the Costa and east of the Areias (Jhilpur at world (2600, −1600)): it fills the corner the L left
        /// walled, so the world is a whole 7200 × 4800 m rectangle again.
        /// </summary>
        public static readonly Vector2 DeltaWorldOffset = new Vector2(2600f, -1600f);

        /// <summary>
        /// Região 7 north of the Serra de Kaggen (Sangkent at world (−2200, 2000)): the Teto do Mundo goes on where the Serra's
        /// summit and drystone walls rise, so the world is no longer a rectangle: north of the Zarafshan and of the Costa two
        /// walled holes (<see cref="OverworldLayout.Holes"/>).
        /// </summary>
        public static readonly Vector2 TetoWorldOffset = new Vector2(-2200f, 2000f);

        /// <summary>Width (m) of the trail that joins two passes (or a pass to the nearest trail across the border).</summary>
        public const float PassTrailWidth = 4f;

        /// <summary>Longest trail (m) led from a lone pass to the other region's nearest trail.</summary>
        public const float PassTrailReach = 700f;

        /// <summary>The stitched world (built on first use, cached like the regions).</summary>
        public static OverworldLayout StitchedWorld => Get(WorldId);

        /// <summary>
        /// The stitched world when catalog map <paramref name="mapId"/> lies in one of its regions, else null (map mode loads
        /// it for any of them; <see cref="ForMap"/> still answers the region on its own: tests, the server, the catalog).
        /// </summary>
        public static OverworldLayout WorldOf(string mapId)
        {
            OverworldLayout region = ForMap(mapId);
            if (region == null)
            {
                return null;
            }

            OverworldLayout world = Get(WorldId);
            return world != null && world.Parts.Exists(p => p.Id == region.Id) ? world : null;
        }

        private static OverworldLayout BuildWorld()
        {
            // Fresh builds (the cached regions stay as they are): moved in place, then gathered.
            var placed = new List<(OverworldLayout Layout, Vector2 Offset)>
            {
                (Zarafshan(), ZarafshanWorldOffset),
                (Areias(), AreiasWorldOffset),
                (Kaggen(), KaggenWorldOffset),
                (Limiar(), LimiarWorldOffset),
                (Costa(), CostaWorldOffset),
                (DeltaRegion(), DeltaWorldOffset),
                (TetoRegion(), TetoWorldOffset),
            };

            Rect bounds = default;
            for (int i = 0; i < placed.Count; i++)
            {
                MoveRegion(placed[i].Layout, placed[i].Offset);
                Rect b = placed[i].Layout.Bounds;
                bounds = i == 0 ? b : Rect.MinMaxRect(Mathf.Min(bounds.xMin, b.xMin), Mathf.Min(bounds.yMin, b.yMin), Mathf.Max(bounds.xMax, b.xMax), Mathf.Max(bounds.yMax, b.yMax));
            }

            var w = new OverworldLayout { Id = WorldId, Region = MapRegion.Zarafshan, Bounds = bounds };
            w.World = new MapLayout { MapId = WorldId, Biome = BiomeType.Forest, Bounds = bounds };
            var regionPaths = new List<List<MapPath>>();
            var removed = new List<(MapExitSpec Gate, int Region)>();
            var desert = new List<OverworldProp>();
            for (int i = 0; i < placed.Count; i++)
            {
                (OverworldLayout r, Vector2 offset) = placed[i];
                w.Parts.Add(new OverworldPart { Id = r.Id, Region = r.Region, Biome = r.World.Biome, Bounds = r.Bounds, Offset = offset, Vila = r.Vila });
                w.Zones.AddRange(r.Zones);
                w.Settlements.AddRange(r.Settlements);
                w.AuthoredAreas.AddRange(r.AuthoredAreas);
                w.FixedProps.AddRange(r.FixedProps);
                foreach (KeyValuePair<string, OverworldZone> pair in r.ZoneOfSpawn)
                {
                    w.ZoneOfSpawn[pair.Key] = pair.Value;
                }

                w.World.Paths.AddRange(r.World.Paths);
                regionPaths.Add(new List<MapPath>(r.World.Paths));
                w.World.Rivers.AddRange(r.World.Rivers);
                w.World.Landmarks.AddRange(r.World.Landmarks);
                w.World.Zones.AddRange(r.World.Zones);
                string suffix = r.Id.StartsWith("overworld_") ? r.Id.Substring("overworld_".Length) : r.Id;
                foreach (MapExitSpec exit in r.World.Exits)
                {
                    if (exit.Kind == MapExitKind.Map && placed.Exists(p => p.Layout.ContainsMap(exit.TargetMapId)))
                    {
                        removed.Add((exit, i));
                        continue;
                    }

                    if (i > 0 && exit.Kind == MapExitKind.WorldMap && exit.Id == "exit_island")
                    {
                        exit.Id = "exit_island_" + suffix;
                    }

                    w.World.Exits.Add(exit);
                }

                foreach (OverworldProp piece in DesertPiecesOf(r))
                {
                    OverworldProp moved = piece;
                    moved.Position += offset;
                    moved.End += offset;
                    desert.Add(moved);
                }

                OverworldChunkBuilder.CopyCaveData(r, w, offset);
            }

            OverworldSettlement vila = placed[0].Layout.Vila;
            w.Vila = vila;
            w.World.Settlement = vila.Recipe;
            w.World.Town = vila.Town;
            w.World.SafeZone = vila.SafeZone;
            w.World.SettlementGroundHalfExtent = vila.GroundHalfExtent;
            w.World.SettlementIsReturnCity = true;
            w.World.DockArrival = vila.HasDock ? vila.DockArrival : vila.Town.Respawn + vila.Offset;
            w.World.DockArrivalFacing = vila.DockArrivalFacing;

            // The towns walk the whole world (their invisible walls stand on its edge, not on their region's).
            foreach (OverworldSettlement settlement in w.Settlements)
            {
                settlement.Town.WalkBounds = Shift(w.Bounds, -settlement.Offset);
                settlement.Town.Invalidate();
            }

            OpenPasses(w, removed, regionPaths);
            _ = w.Holes; // The corner no region covers (walled, backdrop), found once now that every part is in.
            if (desert.Count > 0)
            {
                DesertPieces.AddOrUpdate(w, desert);
            }

            return w;
        }

        /// <summary>Moves a freshly built region (its own objects) by <paramref name="o"/>: everything it holds in world coordinates.</summary>
        private static void MoveRegion(OverworldLayout r, Vector2 o)
        {
            if (o == Vector2.zero)
            {
                return;
            }

            r.Bounds = Shift(r.Bounds, o);
            r.World.Bounds = Shift(r.World.Bounds, o);
            r.World.SafeZone = Shift(r.World.SafeZone, o);
            r.World.DockArrival += o;
            r.ReliefCache = null;
            foreach (OverworldZone zone in r.Zones)
            {
                zone.Bounds = Shift(zone.Bounds, o);
                zone.Origin += o;
            }

            foreach (OverworldSettlement s in r.Settlements)
            {
                s.Offset += o;
                s.SafeZone = Shift(s.SafeZone, o);
                s.DockTrigger = Shift(s.DockTrigger, o);
                s.DockArrival += o;
            }

            for (int i = 0; i < r.World.Paths.Count; i++)
            {
                MapPath path = r.World.Paths[i];
                r.World.Paths[i] = new MapPath(path.A + o, path.B + o, path.Width);
            }

            foreach (MapRiver river in r.World.Rivers)
            {
                for (int i = 0; i < river.Points.Count; i++)
                {
                    river.Points[i] += o;
                }

                foreach (MapCrossing crossing in river.Crossings)
                {
                    crossing.At += o;
                }
            }

            for (int i = 0; i < r.World.Landmarks.Count; i++)
            {
                MapLandmark l = r.World.Landmarks[i];
                r.World.Landmarks[i] = new MapLandmark(l.Kind, l.Position + o, l.Yaw, l.Scale);
            }

            foreach (MapSpawnZone spawn in r.World.Zones)
            {
                spawn.Center += o;
            }

            foreach (MapExitSpec exit in r.World.Exits)
            {
                exit.Trigger = Shift(exit.Trigger, o);
                exit.Arrival += o;
            }

            for (int i = 0; i < r.AuthoredAreas.Count; i++)
            {
                r.AuthoredAreas[i] = Shift(r.AuthoredAreas[i], o);
            }

            foreach (CityProp prop in r.FixedProps)
            {
                prop.Position += o;
                prop.End += o;
            }
        }

        // ------------------------------------------------------------------
        // The passes between the regions
        // ------------------------------------------------------------------

        /// <summary>
        /// Turns the gates between two regions of the world into open passes: flat ground kept, the two gates' trails joined
        /// (a gate whose partner stands within 160 m on the same border), a lone gate on a border led to the nearest trail
        /// across it; the clearings and props left on the new trails are dropped. A gate on the world's outer wall whose map is
        /// now inside the world just closes (its trail ends at the wall).
        /// </summary>
        private static void OpenPasses(OverworldLayout w, List<(MapExitSpec Gate, int Region)> removed, List<List<MapPath>> regionPaths)
        {
            var joins = new List<(Vector2 A, Vector2 B)>();
            var done = new HashSet<MapExitSpec>();
            foreach ((MapExitSpec gate, int region) in removed)
            {
                w.OpenFlats.Add((gate.Trigger, OverworldRelief.GateFlat));
                w.OpenFlats.Add((new Rect(gate.Arrival, Vector2.zero), 4f));
                int across = AcrossBorder(w, gate, region);
                if (across < 0)
                {
                    continue;
                }

                w.Passes.Add(gate.Trigger.center);
                if (done.Contains(gate))
                {
                    continue;
                }

                // Its partner: the gate of the other region leading back here, on the same border.
                MapExitSpec partner = null;
                foreach ((MapExitSpec other, int otherRegion) in removed)
                {
                    if (otherRegion == across && !done.Contains(other) && w.Parts[region].Bounds.Contains(Ahead(other, w.Parts[otherRegion].Bounds))
                        && Vector2.Distance(other.Trigger.center, gate.Trigger.center) < 160f)
                    {
                        partner = other;
                        break;
                    }
                }

                done.Add(gate);
                if (partner != null)
                {
                    done.Add(partner);
                    joins.Add((gate.Arrival, partner.Arrival));
                    continue;
                }

                if (NearestTrailAcross(w, gate.Arrival, w.Parts[across].Bounds, regionPaths[across], out Vector2 to))
                {
                    joins.Add((gate.Arrival, to));
                }
            }

            foreach ((Vector2 a, Vector2 b) in joins)
            {
                int pieces = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) / PathPiece));
                for (int i = 0; i < pieces; i++)
                {
                    w.World.Paths.Add(new MapPath(Vector2.Lerp(a, b, i / (float)pieces), Vector2.Lerp(a, b, (i + 1) / (float)pieces), PassTrailWidth));
                }
            }

            // Nothing grows or spawns on the new trails, nor right at a pass.
            w.World.Zones.RemoveAll(spawn =>
            {
                foreach ((Vector2 a, Vector2 b) in joins)
                {
                    if (MapLayoutRules.DistanceToSegment(spawn.Center, a, b) < spawn.Radius + PassTrailWidth * 0.5f + 6f)
                    {
                        return true;
                    }
                }

                foreach (Vector2 pass in w.Passes)
                {
                    if (Vector2.Distance(spawn.Center, pass) < spawn.Radius + 30f)
                    {
                        return true;
                    }
                }

                return false;
            });

            var kept = new HashSet<string>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                kept.Add(spawn.Id);
            }

            var stale = new List<string>();
            foreach (string id in w.ZoneOfSpawn.Keys)
            {
                if (!kept.Contains(id))
                {
                    stale.Add(id);
                }
            }

            foreach (string id in stale)
            {
                w.ZoneOfSpawn.Remove(id);
            }

            CleanFixedProps(w);
        }

        /// <summary>The region on the other side of the border <paramref name="gate"/> stands on, or −1 (the world's outer wall).</summary>
        private static int AcrossBorder(OverworldLayout w, MapExitSpec gate, int region)
        {
            Vector2 ahead = Ahead(gate, w.Parts[region].Bounds);
            for (int i = 0; i < w.Parts.Count; i++)
            {
                if (i != region && w.Parts[i].Bounds.Contains(ahead))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>A point a few metres past the border the gate's trigger touches (outwards from its region).</summary>
        private static Vector2 Ahead(MapExitSpec gate, Rect region)
        {
            Vector2 c = gate.Trigger.center;
            float left = Mathf.Abs(c.x - region.xMin);
            float right = Mathf.Abs(region.xMax - c.x);
            float down = Mathf.Abs(c.y - region.yMin);
            float up = Mathf.Abs(region.yMax - c.y);
            float min = Mathf.Min(Mathf.Min(left, right), Mathf.Min(down, up));
            if (min == left)
            {
                return new Vector2(region.xMin - 6f, c.y);
            }

            if (min == right)
            {
                return new Vector2(region.xMax + 6f, c.y);
            }

            return min == down ? new Vector2(c.x, region.yMin - 6f) : new Vector2(c.x, region.yMax + 6f);
        }

        /// <summary>
        /// The nearest end of a trail of the region across the border (<paramref name="paths"/>, inside <paramref name="across"/>)
        /// that a straight trail from <paramref name="from"/> reaches without wading a river or crossing a town or a landmark.
        /// </summary>
        private static bool NearestTrailAcross(OverworldLayout w, Vector2 from, Rect across, List<MapPath> paths, out Vector2 to)
        {
            to = from;
            float best = PassTrailReach;
            Rect inside = CityGeometry.Inflate(across, -4f);
            var tried = new HashSet<Vector2>();
            var candidates = new List<(float Distance, Vector2 Point)>();
            foreach (MapPath path in paths)
            {
                foreach (Vector2 q in new[] { path.A, path.B })
                {
                    float d = Vector2.Distance(from, q);
                    if (d < best && inside.Contains(q) && tried.Add(q))
                    {
                        candidates.Add((d, q));
                    }
                }
            }

            candidates.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            foreach ((float distance, Vector2 q) in candidates)
            {
                if (ClearRun(w, from, q))
                {
                    to = q;
                    return true;
                }
            }

            return false;
        }

        private static bool ClearRun(OverworldLayout w, Vector2 a, Vector2 b)
        {
            float length = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(length / 2f));
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(a, b, i / (float)steps);
                if (MapLayoutRules.InRiver(w.World, p, PassTrailWidth * 0.5f + 1f))
                {
                    return false;
                }

                foreach (OverworldSettlement settlement in w.Settlements)
                {
                    if (CityGeometry.Inflate(settlement.GroundRect, 3f).Contains(p))
                    {
                        return false;
                    }
                }

                foreach (MapLandmark landmark in w.World.Landmarks)
                {
                    if (Vector2.Distance(p, landmark.Position) < 6f)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
