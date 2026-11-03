using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>One scattered object of a chunk: a kit prop and the vegetation it grows in.</summary>
    public struct OverworldProp
    {
        public CityPropKind Kind;
        public Vector2 Position;
        public Vector2 End;
        public float Yaw;
        public float Scale;
        public int Variant;
        public TownVegetation Vegetation;
        public TownSwatch Color;
    }

    /// <summary>A simple collider of a chunk (mobile budget: capsules and boxes only).</summary>
    public struct OverworldSolid
    {
        public Vector2 Center;
        public float Radius;
        public float Height;
        public bool IsBox;
        public Vector2 Size;
        public float Yaw;
    }

    /// <summary>What one streamed chunk holds (pure data, deterministic from the layout and the chunk coordinate).</summary>
    public sealed class OverworldChunkContent
    {
        public Vector2Int Coord;
        public Rect Rect;

        /// <summary>Inside the walls (colliders) or the backdrop ring outside them (visual only).</summary>
        public bool Inner;

        public readonly List<OverworldProp> Props = new List<OverworldProp>();
        public readonly List<OverworldSolid> Solids = new List<OverworldSolid>();
        public readonly List<MapLandmark> Landmarks = new List<MapLandmark>();
        public readonly List<(MapRiver River, MapCrossing Crossing)> Bridges = new List<(MapRiver, MapCrossing)>();

        // The world features near the chunk (filtered once; every free-spot test is local).
        internal readonly List<MapPath> NearPaths = new List<MapPath>();
        internal readonly List<MapRiver> NearRivers = new List<MapRiver>();
        internal readonly List<MapSpawnZone> NearSpawns = new List<MapSpawnZone>();
        internal readonly List<MapLandmark> NearLandmarks = new List<MapLandmark>();
        internal readonly List<Rect> Blocked = new List<Rect>();
        internal readonly List<Vector2> KeepClearPoints = new List<Vector2>();
        internal readonly List<OverworldSettlement> NearTowns = new List<OverworldSettlement>();
    }

    /// <summary>
    /// The streamed chunk of a continuous region (<see cref="OverworldLayout.ChunkSize"/> m square): its scatter (trees,
    /// shrubs, boulders, hill mounds, ruined walls, hay bales: by the terrain at each sample, kept off trails, rivers,
    /// clearings, landmarks, gates and the settlements), its simple colliders, and its two merged meshes (ground: organic
    /// noise colours per terrain, trails, rivers, clearings; props: scatter, landmarks, bridges). Pure until the meshes
    /// are made (<see cref="TownMeshBuilder"/>), deterministic: <see cref="TownStyle.Hash01"/> and Perlin noise only.
    /// </summary>
    public static class OverworldChunkBuilder
    {
        /// <summary>Distance between two scatter samples (m).</summary>
        public const float SampleSpacing = 3.2f;

        /// <summary>Side of one ground colour cell (m).</summary>
        public const float GroundCell = 3f;

        /// <summary>Width (m) of the dense tree belt along the walls.</summary>
        public const float EdgeBelt = 8f;

        private const float GroundY = -0.03f;

        // ------------------------------------------------------------------
        // Content
        // ------------------------------------------------------------------

        public static OverworldChunkContent Content(OverworldLayout layout, Vector2Int coord)
        {
            var content = new OverworldChunkContent
            {
                Coord = coord,
                Rect = OverworldRules.ChunkRect(layout, coord),
                Inner = OverworldRules.IsInnerChunk(layout, coord),
            };

            Gather(layout, content);
            AddFixed(layout, content);
            ScatterNoise(layout, content);
            ScatterRows(layout, content);

            foreach (MapLandmark landmark in layout.World.Landmarks)
            {
                if (content.Rect.Contains(landmark.Position))
                {
                    content.Landmarks.Add(landmark);
                    AddLandmarkSolids(content, landmark);
                }
            }

            foreach (MapRiver river in layout.World.Rivers)
            {
                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (crossing.IsBridge && content.Rect.Contains(crossing.At))
                    {
                        content.Bridges.Add((river, crossing));
                    }
                }
            }

            return content;
        }

        private static void Gather(OverworldLayout layout, OverworldChunkContent content)
        {
            Rect near = CityGeometry.Inflate(content.Rect, 16f);
            foreach (MapPath path in layout.World.Paths)
            {
                if (SegmentNear(near, path.A, path.B, path.Width))
                {
                    content.NearPaths.Add(path);
                }
            }

            foreach (MapRiver river in layout.World.Rivers)
            {
                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    if (SegmentNear(near, river.Points[i], river.Points[i + 1], river.Width + 4f))
                    {
                        content.NearRivers.Add(river);
                        break;
                    }
                }
            }

            foreach (MapSpawnZone spawn in layout.World.Zones)
            {
                if (CityGeometry.DistanceToRect(spawn.Center, near) <= spawn.Radius)
                {
                    content.NearSpawns.Add(spawn);
                }
            }

            foreach (MapLandmark landmark in layout.World.Landmarks)
            {
                if (CityGeometry.DistanceToRect(landmark.Position, near) <= 10f)
                {
                    content.NearLandmarks.Add(landmark);
                }
            }

            foreach (OverworldSettlement settlement in layout.Settlements)
            {
                Rect ground = CityGeometry.Inflate(settlement.GroundRect, 2f);
                if (ground.Overlaps(near))
                {
                    content.Blocked.Add(ground);
                }

                Rect safe = CityGeometry.Inflate(settlement.SafeZone, 2f);
                if (safe.Overlaps(near))
                {
                    content.Blocked.Add(safe);
                }

                if (CityGeometry.DistanceToRect(settlement.Offset, near) < 220f)
                {
                    content.NearTowns.Add(settlement);
                }

                if (settlement.HasDock)
                {
                    content.KeepClearPoints.Add(settlement.DockArrival);
                }
            }

            foreach (MapExitSpec exit in layout.World.Exits)
            {
                Rect keep = CityGeometry.Inflate(exit.Trigger, 6f);
                if (keep.Overlaps(near))
                {
                    content.Blocked.Add(keep);
                }

                content.KeepClearPoints.Add(exit.Arrival);
            }
        }

        private static bool SegmentNear(Rect r, Vector2 a, Vector2 b, float pad)
        {
            Rect seg = Rect.MinMaxRect(Mathf.Min(a.x, b.x) - pad, Mathf.Min(a.y, b.y) - pad, Mathf.Max(a.x, b.x) + pad, Mathf.Max(a.y, b.y) + pad);
            return seg.Overlaps(r);
        }

        /// <summary>The spot is free for a scattered prop of <paramref name="clearance"/> (m).</summary>
        public static bool IsFree(OverworldLayout layout, OverworldChunkContent content, Vector2 p, float clearance)
        {
            bool inside = layout.Bounds.Contains(p);
            if (inside && !CityGeometry.Inflate(layout.Bounds, -MapLayoutRules.WallMargin).Contains(p))
            {
                return false;
            }

            foreach (Rect r in content.Blocked)
            {
                if (CityGeometry.Inflate(r, clearance).Contains(p))
                {
                    return false;
                }
            }

            foreach (MapPath path in content.NearPaths)
            {
                if (MapLayoutRules.DistanceToSegment(p, path.A, path.B) <= path.Width * 0.5f + clearance + 0.6f)
                {
                    return false;
                }
            }

            foreach (MapRiver river in content.NearRivers)
            {
                if (MapLayoutRules.DistanceToPolyline(p, river.Points) <= river.Width * 0.5f + clearance + 0.8f)
                {
                    return false;
                }
            }

            foreach (MapSpawnZone spawn in content.NearSpawns)
            {
                if (Vector2.Distance(p, spawn.Center) < spawn.Radius * 0.7f + clearance)
                {
                    return false;
                }
            }

            foreach (MapLandmark landmark in content.NearLandmarks)
            {
                float reach = landmark.Kind == MapLandmarkKind.StoneRing ? 7.5f * landmark.Scale + 2.5f : 4.5f * Mathf.Max(0.5f, landmark.Scale);
                if (Vector2.Distance(p, landmark.Position) < reach + clearance)
                {
                    return false;
                }
            }

            foreach (Vector2 keep in content.KeepClearPoints)
            {
                if ((keep - p).sqrMagnitude < 16f)
                {
                    return false;
                }
            }

            foreach (OverworldSettlement town in content.NearTowns)
            {
                if (town.Town == null)
                {
                    continue;
                }

                Vector2 local = p - town.Offset;
                if (!CityGeometry.Inflate(town.Town.WalkBounds, 0f).Contains(local))
                {
                    continue;
                }

                foreach (CitySolid solid in town.Town.Solids)
                {
                    if (solid.DistanceTo(local) < clearance + 1.2f)
                    {
                        return false;
                    }
                }
            }

            foreach (OverworldProp other in content.Props)
            {
                float min = clearance + (other.Kind == CityPropKind.Rock ? 0.5f * other.Scale : 0.9f);
                if ((other.Position - p).sqrMagnitude < min * min)
                {
                    return false;
                }
            }

            return true;
        }

        private static float DistanceToWall(Rect bounds, Vector2 p)
        {
            return Mathf.Min(Mathf.Min(p.x - bounds.xMin, bounds.xMax - p.x), Mathf.Min(p.y - bounds.yMin, bounds.yMax - p.y));
        }

        private static float NearestRiverEdge(OverworldChunkContent content, Vector2 p)
        {
            float best = float.MaxValue;
            foreach (MapRiver river in content.NearRivers)
            {
                best = Mathf.Min(best, MapLayoutRules.DistanceToPolyline(p, river.Points) - river.Width * 0.5f);
            }

            return best;
        }

        private static bool Green(OverworldTerrain t)
        {
            return t != OverworldTerrain.Steppe && t != OverworldTerrain.Ruins && t != OverworldTerrain.Ravine && t != OverworldTerrain.HighHills;
        }

        private static void ScatterNoise(OverworldLayout layout, OverworldChunkContent content)
        {
            string salt = "ow" + content.Coord.x + "_" + content.Coord.y;
            int steps = Mathf.CeilToInt(layout.ChunkSize / SampleSpacing);
            int i = 0;
            for (int gx = 0; gx < steps; gx++)
            {
                for (int gz = 0; gz < steps; gz++, i++)
                {
                    var p = new Vector2(
                        content.Rect.xMin + (gx + 0.5f + (TownStyle.Hash01(salt, i) - 0.5f) * 0.8f) * SampleSpacing,
                        content.Rect.yMin + (gz + 0.5f + (TownStyle.Hash01(salt, i + 40000) - 0.5f) * 0.8f) * SampleSpacing);
                    if (!content.Rect.Contains(p) || layout.InAuthoredArea(p))
                    {
                        continue;
                    }

                    OverworldTerrain terrain = OverworldTerrains.TerrainAt(layout, p);
                    OverworldTerrainStyle style = OverworldTerrains.For(terrain);
                    float wall = DistanceToWall(layout.Bounds, p);
                    float trees = style.Rows ? 0f : style.Trees;
                    if (wall < EdgeBelt)
                    {
                        // The belt of trees along the walls (and the backdrop beyond them).
                        trees = Mathf.Max(trees, wall < 0f ? 0.55f : 0.5f);
                    }
                    else if (Green(terrain))
                    {
                        float river = NearestRiverEdge(content, p);
                        if (river > 1.5f && river < 9f)
                        {
                            trees += 0.22f;
                        }
                    }

                    float roll = TownStyle.Hash01(salt, i + 80000);
                    float t0 = trees;
                    float t1 = t0 + style.Bushes;
                    float t2 = t1 + style.Rocks;
                    float t3 = t2 + style.Mounds;
                    float t4 = t3 + style.Walls;
                    float t5 = t4 + style.Bales;
                    float s01 = TownStyle.Hash01(salt, i + 120000);
                    float yaw = TownStyle.Hash01(salt, i + 160000) * 360f;
                    if (roll < t0)
                    {
                        bool second = style.SecondVegetation.HasValue && TownStyle.Hash01(salt, i + 200000) < style.SecondShare;
                        TownVegetation vegetation = second ? style.SecondVegetation.Value : style.Vegetation;
                        int[] variants = second ? style.SecondVariants : style.TreeVariants;
                        if (wall < 0f && !second)
                        {
                            variants = style.Vegetation == TownVegetation.Forest ? new[] { 0, 1, 2 } : style.TreeVariants;
                        }

                        float scale = Mathf.Lerp(style.TreeScale.x, style.TreeScale.y, s01);
                        TryAdd(layout, content, p, 1.1f, new OverworldProp
                        {
                            Kind = CityPropKind.Tree, Position = p, Yaw = yaw, Scale = scale, Vegetation = vegetation,
                            Variant = variants[(int)(TownStyle.Hash(salt, i + 240000) % (uint)variants.Length)],
                        });
                    }
                    else if (roll < t1)
                    {
                        TryAdd(layout, content, p, 0.6f, new OverworldProp
                        {
                            Kind = CityPropKind.Bush, Position = p, Yaw = yaw, Scale = 0.8f + s01 * 0.7f, Vegetation = style.Vegetation,
                        });
                    }
                    else if (roll < t2)
                    {
                        float scale = Mathf.Lerp(style.RockScale.x, style.RockScale.y, s01);
                        TryAdd(layout, content, p, 0.5f * scale + 0.4f, new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Position = p, Yaw = yaw, Scale = scale,
                            Color = terrain == OverworldTerrain.Ravine ? TownSwatch.Basalt : terrain == OverworldTerrain.Ruins ? TownSwatch.Sandstone : TownSwatch.Stone,
                        });
                    }
                    else if (roll < t3)
                    {
                        float scale = 3f + s01 * 3.5f;
                        TryAdd(layout, content, p, 0.5f * scale + 0.6f, new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Variant = 9, Position = p, Yaw = yaw, Scale = scale, Color = style.Alt == TownSwatch.Stone ? TownSwatch.GrassDark : style.Alt,
                        });
                    }
                    else if (roll < t4)
                    {
                        float length = 3f + s01 * 6f;
                        Vector2 dir = new Vector2(Mathf.Cos(yaw * Mathf.Deg2Rad), Mathf.Sin(yaw * Mathf.Deg2Rad));
                        Vector2 end = p + dir * length;
                        if (IsFree(layout, content, end, 1.2f) && IsFree(layout, content, (p + end) * 0.5f, 1.2f))
                        {
                            TryAdd(layout, content, p, 1.2f, new OverworldProp
                            {
                                Kind = CityPropKind.FenceRun, Position = p, End = end, Scale = 0.6f + s01 * 0.8f,
                            });
                        }
                    }
                    else if (roll < t5)
                    {
                        TryAdd(layout, content, p, 1f, new OverworldProp { Kind = CityPropKind.HayBale, Position = p, Yaw = yaw, Scale = 1f });
                    }
                }
            }
        }

        /// <summary>Orchard and mulberry rows: a world-aligned lattice, so rows run on across chunk borders.</summary>
        private static void ScatterRows(OverworldLayout layout, OverworldChunkContent content)
        {
            const float dx = 6.5f;
            const float dz = 5.5f;
            string salt = "owrow";
            int x0 = Mathf.CeilToInt(content.Rect.xMin / dx);
            int x1 = Mathf.FloorToInt((content.Rect.xMax - 0.001f) / dx);
            int z0 = Mathf.CeilToInt(content.Rect.yMin / dz);
            int z1 = Mathf.FloorToInt((content.Rect.yMax - 0.001f) / dz);
            for (int ix = x0; ix <= x1; ix++)
            {
                for (int iz = z0; iz <= z1; iz++)
                {
                    int key = ix * 92821 + iz * 7919;
                    var p = new Vector2(ix * dx + (TownStyle.Hash01(salt, key) - 0.5f) * 0.8f, iz * dz + (TownStyle.Hash01(salt, key + 1) - 0.5f) * 0.8f);
                    if (DistanceToWall(layout.Bounds, p) < EdgeBelt || layout.InAuthoredArea(p))
                    {
                        continue;
                    }

                    OverworldTerrain terrain = OverworldTerrains.TerrainAt(layout, p);
                    OverworldTerrainStyle style = OverworldTerrains.For(terrain);
                    if (!style.Rows)
                    {
                        continue;
                    }

                    // Mulberry rows are wider apart: skip every other lattice line across.
                    if (terrain == OverworldTerrain.Mulberry && (iz & 1) == 1)
                    {
                        continue;
                    }

                    if (TownStyle.Hash01(salt, key + 2) > style.Trees)
                    {
                        continue;
                    }

                    int[] variants = style.TreeVariants;
                    TryAdd(layout, content, p, 0.9f, new OverworldProp
                    {
                        Kind = TownStyle.Hash01(salt, key + 3) < 0.07f ? CityPropKind.Bush : CityPropKind.Tree,
                        Position = p, Yaw = TownStyle.Hash01(salt, key + 4) * 360f,
                        Scale = Mathf.Lerp(style.TreeScale.x, style.TreeScale.y, TownStyle.Hash01(salt, key + 5)),
                        Variant = variants[(int)(TownStyle.Hash(salt, key + 6) % (uint)variants.Length)], Vegetation = style.Vegetation,
                    });
                }
            }
        }

        /// <summary>The embedded maps' own props (their orchards, reeds, mulberry rows), with the same simple colliders.</summary>
        private static void AddFixed(OverworldLayout layout, OverworldChunkContent content)
        {
            foreach (CityProp prop in layout.FixedPropsIn(content.Coord))
            {
                var item = new OverworldProp
                {
                    Kind = prop.Kind, Position = prop.Position, End = prop.End, Yaw = prop.Yaw, Scale = prop.Scale <= 0f ? 1f : prop.Scale,
                    Variant = prop.Variant, Vegetation = TownVegetation.Forest,
                    Color = prop.Kind == CityPropKind.Rock && prop.Variant == 9 ? TownSwatch.GrassDark : TownSwatch.Stone,
                };
                content.Props.Add(item);
                AddSolid(layout, content, item);
            }
        }

        private static void TryAdd(OverworldLayout layout, OverworldChunkContent content, Vector2 p, float clearance, OverworldProp prop)
        {
            if (!IsFree(layout, content, p, clearance))
            {
                return;
            }

            content.Props.Add(prop);
            AddSolid(layout, content, prop);
        }

        private static void AddSolid(OverworldLayout layout, OverworldChunkContent content, OverworldProp prop)
        {
            Vector2 p = prop.Position;
            if (!content.Inner || !layout.Bounds.Contains(p))
            {
                return;
            }

            switch (prop.Kind)
            {
                case CityPropKind.Tree:
                    content.Solids.Add(new OverworldSolid { Center = p, Radius = Mathf.Clamp(0.24f * prop.Scale, 0.16f, 0.55f), Height = 3f });
                    break;
                case CityPropKind.Rock:
                    float radius = prop.Variant == 9 ? 0.42f * prop.Scale : 0.48f * prop.Scale;
                    content.Solids.Add(new OverworldSolid { Center = p, Radius = radius, Height = Mathf.Max(1f, prop.Scale * (prop.Variant == 9 ? 0.42f : 0.7f)) });
                    break;
                case CityPropKind.FenceRun:
                    Vector2 d = prop.End - prop.Position;
                    content.Solids.Add(new OverworldSolid
                    {
                        Center = (prop.Position + prop.End) * 0.5f, IsBox = true, Size = new Vector2(0.6f, d.magnitude), Height = 1.2f,
                        Yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg,
                    });
                    break;
                case CityPropKind.HayBale:
                    content.Solids.Add(new OverworldSolid { Center = p, IsBox = true, Size = new Vector2(1.2f, 0.75f), Height = 0.8f, Yaw = prop.Yaw });
                    break;
            }
        }

        private static void AddLandmarkSolids(OverworldChunkContent content, MapLandmark landmark)
        {
            if (!content.Inner)
            {
                return;
            }

            float s = landmark.Scale <= 0f ? 1f : landmark.Scale;
            switch (landmark.Kind)
            {
                case MapLandmarkKind.StoneRing:
                    for (int k = 0; k < 9; k++)
                    {
                        float a = k * Mathf.PI * 2f / 9f;
                        content.Solids.Add(new OverworldSolid { Center = landmark.Position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 7.5f * s, Radius = 0.5f, Height = 2.4f });
                    }

                    break;
                case MapLandmarkKind.WaterMill:
                    content.Solids.Add(new OverworldSolid { Center = landmark.Position, IsBox = true, Size = new Vector2(4.4f, 3.4f), Height = 3f, Yaw = landmark.Yaw });
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Meshes
        // ------------------------------------------------------------------

        /// <summary>The ground of the chunk: terrain colours in noise blobs, scattered patches, clearings, trails, rivers and fords.</summary>
        public static void BuildGround(TownMeshBuilder b, OverworldLayout layout, OverworldChunkContent content)
        {
            Rect r = content.Rect;
            int cells = Mathf.CeilToInt(layout.ChunkSize / GroundCell);
            for (int ix = 0; ix < cells; ix++)
            {
                for (int iz = 0; iz < cells; iz++)
                {
                    float x0 = r.xMin + ix * GroundCell;
                    float z0 = r.yMin + iz * GroundCell;
                    var c = new Vector2(x0 + GroundCell * 0.5f, z0 + GroundCell * 0.5f);
                    if (InTownGround(layout, c))
                    {
                        continue;
                    }

                    OverworldTerrain terrain = OverworldTerrains.TerrainAt(layout, c);
                    b.Flat(x0, z0, Mathf.Min(x0 + GroundCell, r.xMax), Mathf.Min(z0 + GroundCell, r.yMax), GroundY, OverworldTerrains.GroundAt(terrain, c));
                }
            }

            // Patches: irregular little discs (flowers, puddles, stones) that break the cells up.
            string salt = "owpatch" + content.Coord.x + "_" + content.Coord.y;
            for (int i = 0; i < 14; i++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, i)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, i + 100)));
                if (InTownGround(layout, p))
                {
                    continue;
                }

                OverworldTerrainStyle style = OverworldTerrains.For(OverworldTerrains.TerrainAt(layout, p));
                TownSwatch swatch = style.Patches[(int)(TownStyle.Hash(salt, i + 200) % (uint)style.Patches.Length)];
                float radius = 1.2f + TownStyle.Hash01(salt, i + 300) * 3.6f;
                b.Disc(new Vector3(p.x, GroundY + 0.004f, p.y), radius, 9, swatch, TownStyle.Hash01(salt, i + 400) * 6f);
            }

            foreach (MapSpawnZone spawn in content.NearSpawns)
            {
                OverworldTerrainStyle style = OverworldTerrains.For(OverworldTerrains.TerrainAt(layout, spawn.Center));
                b.Disc(new Vector3(spawn.Center.x, GroundY + 0.008f, spawn.Center.y), spawn.Radius * 0.9f, 16, style.Clearing);
                b.Disc(new Vector3(spawn.Center.x, GroundY + 0.01f, spawn.Center.y), spawn.Radius * 0.42f, 12, TownSwatch.Dirt);
            }

            foreach (MapPath path in content.NearPaths)
            {
                TownSwatch street = OverworldTerrains.For(OverworldTerrains.TerrainAt(layout, (path.A + path.B) * 0.5f)).Trail;
                if (InTownGround(layout, path.A) && InTownGround(layout, path.B))
                {
                    continue;
                }

                Strip(b, path.A, path.B, path.Width * 0.5f + 0.3f, GroundY + 0.012f, TownSwatch.DirtDark);
                Strip(b, path.A, path.B, path.Width * 0.5f, GroundY + 0.016f, street);
                foreach (Vector2 end in new[] { path.A, path.B })
                {
                    if (content.Rect.Contains(end) && !InTownGround(layout, end))
                    {
                        b.Disc(new Vector3(end.x, GroundY + 0.0125f, end.y), path.Width * 0.5f + 0.3f, 10, TownSwatch.DirtDark);
                        b.Disc(new Vector3(end.x, GroundY + 0.0165f, end.y), path.Width * 0.5f, 10, street);
                    }
                }
            }

            Rect near = CityGeometry.Inflate(r, 12f);
            foreach (MapRiver river in content.NearRivers)
            {
                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    if (SegmentNear(near, river.Points[i], river.Points[i + 1], river.Width))
                    {
                        Strip(b, river.Points[i], river.Points[i + 1], river.Width * 0.5f + 1.4f, GroundY + 0.02f, TownSwatch.Sand);
                    }
                }

                foreach (Vector2 p in river.Points)
                {
                    if (near.Contains(p))
                    {
                        b.Disc(new Vector3(p.x, GroundY + 0.02f, p.y), river.Width * 0.5f + 1.4f, 14, TownSwatch.Sand);
                    }
                }

                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    if (SegmentNear(near, river.Points[i], river.Points[i + 1], river.Width))
                    {
                        Strip(b, river.Points[i], river.Points[i + 1], river.Width * 0.5f, GroundY + 0.026f, TownSwatch.Water);
                    }
                }

                foreach (Vector2 p in river.Points)
                {
                    if (near.Contains(p))
                    {
                        b.Disc(new Vector3(p.x, GroundY + 0.026f, p.y), river.Width * 0.5f, 14, TownSwatch.Water);
                    }
                }

                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (crossing.IsBridge || !r.Contains(crossing.At))
                    {
                        continue;
                    }

                    b.Disc(new Vector3(crossing.At.x, GroundY + 0.03f, crossing.At.y), crossing.Width * 0.5f, 14, TownSwatch.SandDark);
                    Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                    for (int s = -3; s <= 3; s++)
                    {
                        Vector2 p = crossing.At + across * (s * 1.3f * river.Width / 9f) + new Vector2(-across.y, across.x) * ((s & 1) * 0.6f - 0.3f);
                        b.Crown(new Vector3(p.x, 0.05f, p.y), new Vector3(0.8f, 0.18f, 0.7f), TownSwatch.Stone, s * 37f);
                    }
                }
            }
        }

        /// <summary>The scattered props, landmarks and bridges of the chunk, merged into one mesh.</summary>
        public static void BuildProps(TownMeshBuilder b, OverworldChunkContent content)
        {
            TownStyle forest = TownStyles.For(BiomeType.Forest);
            foreach (OverworldProp prop in content.Props)
            {
                OverworldProp captured = prop;
                var at = new Vector3(prop.Position.x, 0f, prop.Position.y);
                switch (prop.Kind)
                {
                    case CityPropKind.Tree:
                        b.With(at, prop.Yaw, () => TownKit.Tree(b, captured.Vegetation, captured.Variant, captured.Scale));
                        break;
                    case CityPropKind.Bush:
                        b.With(at, prop.Yaw, () => TownKit.Bush(b, captured.Vegetation, captured.Scale));
                        break;
                    case CityPropKind.Rock:
                        if (prop.Variant == 9)
                        {
                            b.Crown(at, new Vector3(prop.Scale, prop.Scale * 0.42f, prop.Scale * 0.9f), prop.Color, prop.Yaw);
                        }
                        else
                        {
                            b.Crown(at + Vector3.up * 0.3f * prop.Scale, new Vector3(1f, 0.7f, 0.9f) * prop.Scale, prop.Color, prop.Yaw);
                        }

                        break;
                    case CityPropKind.FenceRun:
                        TownKit.WallRun(b, at, new Vector3(prop.End.x, 0f, prop.End.y), forest, prop.Scale);
                        break;
                    default:
                        var kit = new CityProp { Kind = prop.Kind, Scale = prop.Scale, Variant = prop.Variant };
                        b.With(at, prop.Yaw, () => TownKit.Prop(b, kit, forest));
                        break;
                }
            }

            foreach (MapLandmark landmark in content.Landmarks)
            {
                AppendLandmark(b, landmark);
            }

            foreach ((MapRiver river, MapCrossing crossing) in content.Bridges)
            {
                Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                float yaw = CityGeometry.YawFacing(across);
                float width = crossing.Width;
                float span = river.Width + 4f;
                b.With(new Vector3(crossing.At.x, 0f, crossing.At.y), yaw, () => MapAreaView.BridgeMesh(b, width, span));
            }
        }

        public static void AppendLandmark(TownMeshBuilder b, MapLandmark landmark)
        {
            float s = landmark.Scale <= 0f ? 1f : landmark.Scale;
            var at = new Vector3(landmark.Position.x, 0f, landmark.Position.y);
            switch (landmark.Kind)
            {
                case MapLandmarkKind.StoneRing:
                    for (int k = 0; k < 9; k++)
                    {
                        float a = k * Mathf.PI * 2f / 9f;
                        var p = landmark.Position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 7.5f * s;
                        int variant = k % 3;
                        b.With(new Vector3(p.x, 0f, p.y), a * Mathf.Rad2Deg + 90f, () => MapAreaView.RingStone(b, variant));
                    }

                    break;
                case MapLandmarkKind.WaterMill:
                    b.With(at, landmark.Yaw, () => MapAreaView.WaterMill(b));
                    break;
                case MapLandmarkKind.Signpost:
                    b.With(at, landmark.Yaw, () => MapAreaView.Signpost(b));
                    break;
                case MapLandmarkKind.Campfire:
                    b.With(at, landmark.Yaw, new Vector3(s, s, s), () => MapAreaView.Campfire(b));
                    break;
                case MapLandmarkKind.MelonBed:
                    b.With(at, landmark.Yaw, () => MapAreaView.MelonBed(b));
                    break;
            }
        }

        private static bool InTownGround(OverworldLayout layout, Vector2 p)
        {
            foreach (OverworldSettlement settlement in layout.Settlements)
            {
                if (settlement.GroundRect.Contains(p))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Strip(TownMeshBuilder b, Vector2 a, Vector2 c, float halfWidth, float y, TownSwatch swatch)
        {
            Vector2 d = c - a;
            if (d.sqrMagnitude < 1e-4f)
            {
                return;
            }

            d.Normalize();
            var n = new Vector2(-d.y, d.x) * halfWidth;
            b.QuadFacing(new Vector3(a.x + n.x, y, a.y + n.y), new Vector3(c.x + n.x, y, c.y + n.y), new Vector3(c.x - n.x, y, c.y - n.y),
                new Vector3(a.x - n.x, y, a.y - n.y), Vector3.up, swatch);
        }
    }
}
