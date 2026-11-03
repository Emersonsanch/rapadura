using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>One scattered object of a chunk: a kit prop (or an <see cref="OverworldDecor"/> piece) and the vegetation it grows in.</summary>
    public struct OverworldProp
    {
        public CityPropKind Kind;

        /// <summary>The overworld's own piece; <see cref="OverworldDecor.None"/> = a town-kit <see cref="Kind"/>.</summary>
        public OverworldDecor Decor;

        public Vector2 Position;
        public Vector2 End;
        public float Yaw;
        public float Scale;
        public int Variant;
        public TownVegetation Vegetation;
        public TownSwatch Color;
        public TownSwatch Color2;
    }

    /// <summary>A simple collider of a chunk (mobile budget: capsules and boxes only), standing on the ground at <see cref="Base"/>.</summary>
    public struct OverworldSolid
    {
        public Vector2 Center;
        public float Base;
        public float Radius;
        public float Height;
        public bool IsBox;
        public Vector2 Size;
        public float Yaw;
    }

    /// <summary>
    /// The relief of one chunk sampled on the world-aligned <see cref="OverworldRelief.Cell"/> grid (one cell of margin
    /// around it): heights for the facets, the collider and everything standing on the ground, and per vertex what the
    /// ground colours and the scatter read (terrain, river, trail, clearing, colour tone).
    /// </summary>
    public sealed class OverworldGroundGrid
    {
        public const int Margin = 1;

        public OverworldRelief Relief;

        /// <summary>World grid index of local vertex (0, 0).</summary>
        public int I0;
        public int J0;

        /// <summary>Vertices per side (cells of the chunk + 1 + 2 × <see cref="Margin"/>).</summary>
        public int N;

        public Vector2 Origin;
        public float[] Height;
        public ReliefInfo[] Info;
        public float[] Tone;
        public float[] Fine;

        public int Index(int li, int lj) => lj * N + li;

        public Vector2 VertexAt(int li, int lj) => new Vector2(Origin.x + (I0 + li) * OverworldRelief.Cell, Origin.y + (J0 + lj) * OverworldRelief.Cell);

        /// <summary>The drawn ground height at <paramref name="p"/> (same facets as the mesh; outside the grid: computed).</summary>
        public float Sample(Vector2 p)
        {
            float gx = (p.x - Origin.x) / OverworldRelief.Cell;
            float gz = (p.y - Origin.y) / OverworldRelief.Cell;
            int i = Mathf.FloorToInt(gx);
            int j = Mathf.FloorToInt(gz);
            int li = i - I0;
            int lj = j - J0;
            if (li < 0 || lj < 0 || li + 1 >= N || lj + 1 >= N)
            {
                return Relief.SurfaceAt(p);
            }

            int k = lj * N + li;
            return OverworldRelief.Facet(Height[k], Height[k + 1], Height[k + N], Height[k + N + 1], gx - i, gz - j, i, j);
        }

        /// <summary>What the nearest grid vertex knows (terrain, river, trail, clearing).</summary>
        public ref ReliefInfo Near(Vector2 p)
        {
            int li = Mathf.Clamp(Mathf.RoundToInt((p.x - Origin.x) / OverworldRelief.Cell) - I0, 0, N - 1);
            int lj = Mathf.Clamp(Mathf.RoundToInt((p.y - Origin.y) / OverworldRelief.Cell) - J0, 0, N - 1);
            return ref Info[lj * N + li];
        }

        /// <summary>Upward component of the ground's normal around <paramref name="p"/> (1 = flat).</summary>
        public float Flatness(Vector2 p)
        {
            float hx = Sample(p + new Vector2(0.75f, 0f)) - Sample(p - new Vector2(0.75f, 0f));
            float hz = Sample(p + new Vector2(0f, 0.75f)) - Sample(p - new Vector2(0f, 0.75f));
            return 1f / Mathf.Sqrt(1f + (hx * hx + hz * hz) / 2.25f);
        }
    }

    /// <summary>What one streamed chunk holds (pure data, deterministic from the layout and the chunk coordinate).</summary>
    public sealed class OverworldChunkContent
    {
        public Vector2Int Coord;
        public Rect Rect;

        /// <summary>Inside the walls (colliders) or the backdrop ring outside them (visual only).</summary>
        public bool Inner;

        /// <summary>The relief of the chunk (heights of the facets and the collider).</summary>
        public OverworldGroundGrid Grid;

        /// <summary>Things with a collider (or big enough to keep others away): trees, rocks, logs, hedges, bales…</summary>
        public readonly List<OverworldProp> Props = new List<OverworldProp>();

        /// <summary>Ground detail without colliders: crop rows, grass tufts, flowers, reeds, ferns, pebbles, puddles.</summary>
        public readonly List<OverworldProp> Details = new List<OverworldProp>();

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
        internal bool HasFields;
    }

    /// <summary>
    /// The streamed chunk of a continuous region (<see cref="OverworldLayout.ChunkSize"/> m square): its relief
    /// (<see cref="OverworldRelief"/>, sampled once on a grid), its scatter (trees, shrubs, boulders, logs, the area's own
    /// pieces, crop rows and hedgerows in the fields, and the ground detail: grass tufts, flowers, reeds, ferns), its simple
    /// colliders standing on the relief, and its two merged meshes (ground: low-poly facets coloured per facet, trails
    /// draped over them, rivers; props: everything standing on it). Pure until the meshes are made
    /// (<see cref="TownMeshBuilder"/>), deterministic: hashes and <see cref="OverworldNoise"/> only.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>Distance between two scatter samples (m).</summary>
        public const float SampleSpacing = 3.2f;

        /// <summary>Distance between two ground-detail samples (m).</summary>
        public const float DetailSpacing = 2.2f;

        /// <summary>Side of one ground facet cell (m).</summary>
        public const float GroundCell = OverworldRelief.Cell;

        /// <summary>Width (m) of the dense tree belt along the walls.</summary>
        public const float EdgeBelt = 8f;

        /// <summary>The facets are drawn this much below the relief (the town kit's ground at y = 0 wins where they meet).</summary>
        private const float GroundY = -0.03f;

        /// <summary>
        /// Span (m) of a bridge over <paramref name="river"/>: past the banks onto level ground (a carved bed's banks slope
        /// down within a facet or two of the water).
        /// </summary>
        public static float BridgeSpan(MapRiver river) => river.Width >= OverworldRelief.CarveMinWidth ? river.Width + 13f : river.Width + 4f;

        /// <summary>Length (m) of one crop-row or hedge piece.</summary>
        private const float RowPiece = 3f;

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

            content.Grid = BuildGrid(layout, coord);
            Gather(layout, content);
            AddFixed(layout, content);
            ScatterCaves(layout, content);
            ScatterDesert(layout, content);
            ScatterMountain(layout, content);
            ScatterCoast(layout, content);
            if (content.HasFields)
            {
                ScatterFields(layout, content);
            }

            ScatterNoise(layout, content);
            ScatterRows(layout, content);
            ScatterDetails(layout, content);

            foreach (int i in Near(IndexOf(layout).Landmarks, coord))
            {
                MapLandmark landmark = layout.World.Landmarks[i];
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

        /// <summary>The relief of chunk <paramref name="coord"/> on its grid (shared border vertices with the neighbours: no seams).</summary>
        public static OverworldGroundGrid BuildGrid(OverworldLayout layout, Vector2Int coord)
        {
            OverworldRelief relief = OverworldRelief.For(layout);
            int cells = Mathf.RoundToInt(layout.ChunkSize / OverworldRelief.Cell);
            int n = cells + 1 + 2 * OverworldGroundGrid.Margin;
            var grid = new OverworldGroundGrid
            {
                Relief = relief,
                I0 = coord.x * cells - OverworldGroundGrid.Margin,
                J0 = coord.y * cells - OverworldGroundGrid.Margin,
                N = n,
                Origin = new Vector2(layout.Bounds.xMin, layout.Bounds.yMin),
                Height = new float[n * n],
                Info = new ReliefInfo[n * n],
                Tone = new float[n * n],
                Fine = new float[n * n],
            };

            for (int lj = 0; lj < n; lj++)
            {
                for (int li = 0; li < n; li++)
                {
                    int k = lj * n + li;
                    Vector2 p = grid.VertexAt(li, lj);
                    ReliefInfo info = relief.Sample(p);
                    grid.Info[k] = info;
                    grid.Height[k] = info.Height;
                    grid.Tone[k] = OverworldTerrains.Tone(info.Terrain, p);
                    grid.Fine[k] = OverworldTerrains.Fine(p);
                }
            }

            return grid;
        }

        private static void Gather(OverworldLayout layout, OverworldChunkContent content)
        {
            Rect near = CityGeometry.Inflate(content.Rect, GatherMargin);
            ChunkIndex index = IndexOf(layout);
            foreach (int i in Near(index.Paths, content.Coord))
            {
                MapPath path = layout.World.Paths[i];
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

            foreach (int i in Near(index.Spawns, content.Coord))
            {
                MapSpawnZone spawn = layout.World.Zones[i];
                if (CityGeometry.DistanceToRect(spawn.Center, near) <= spawn.Radius)
                {
                    content.NearSpawns.Add(spawn);
                }
            }

            foreach (int i in Near(index.Landmarks, content.Coord))
            {
                MapLandmark landmark = layout.World.Landmarks[i];
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

                if (settlement.HasDock && CityGeometry.DistanceToRect(settlement.DockArrival, near) < KeepClearReach)
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

                if (CityGeometry.DistanceToRect(exit.Arrival, near) < KeepClearReach)
                {
                    content.KeepClearPoints.Add(exit.Arrival);
                }
            }

            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (info.Terrain == OverworldTerrain.Fields)
                {
                    content.HasFields = true;
                    break;
                }
            }
        }

        /// <summary>Arrival points farther than this (m) from the chunk cannot keep any of its spots clear (they reach 10 m).</summary>
        private const float KeepClearReach = 48f;

        private static bool SegmentNear(Rect r, Vector2 a, Vector2 b, float pad)
        {
            Rect seg = Rect.MinMaxRect(Mathf.Min(a.x, b.x) - pad, Mathf.Min(a.y, b.y) - pad, Mathf.Max(a.x, b.x) + pad, Mathf.Max(a.y, b.y) + pad);
            return seg.Overlaps(r);
        }

        /// <summary>The spot is free for a scattered prop of <paramref name="clearance"/> (m).</summary>
        public static bool IsFree(OverworldLayout layout, OverworldChunkContent content, Vector2 p, float clearance)
        {
            if (!IsOpen(layout, content, p, clearance, true))
            {
                return false;
            }

            foreach (OverworldSettlement town in content.NearTowns)
            {
                if (town.Town == null)
                {
                    continue;
                }

                Vector2 local = p - town.Offset;
                if (!town.Town.WalkBounds.Contains(local) || CityGeometry.DistanceToRect(local, CityGeometry.Inflate(new Rect(-town.GroundHalfExtent, -town.GroundHalfExtent, town.GroundHalfExtent * 2f, town.GroundHalfExtent * 2f), 40f)) > 0f)
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

        /// <summary>The spot is off the trails, rivers, settlements, gates, landmarks and clearing centres (props not checked).</summary>
        private static bool IsOpen(OverworldLayout layout, OverworldChunkContent content, Vector2 p, float clearance, bool clearings)
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

            ref ReliefInfo near = ref content.Grid.Near(p);
            if (near.TrailEdge < 4.5f + clearance)
            {
                foreach (MapPath path in content.NearPaths)
                {
                    if (MapLayoutRules.DistanceToSegment(p, path.A, path.B) <= path.Width * 0.5f + clearance + 0.6f)
                    {
                        return false;
                    }
                }
            }

            if (near.RiverEdge < 4.5f + clearance)
            {
                foreach (MapRiver river in content.NearRivers)
                {
                    if (MapLayoutRules.DistanceToPolyline(p, river.Points) <= river.Width * 0.5f + clearance + 0.8f)
                    {
                        return false;
                    }
                }
            }

            if (clearings)
            {
                foreach (MapSpawnZone spawn in content.NearSpawns)
                {
                    if (Vector2.Distance(p, spawn.Center) < spawn.Radius * 0.7f + clearance)
                    {
                        return false;
                    }
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

            return true;
        }

        private static float DistanceToWall(Rect bounds, Vector2 p)
        {
            return Mathf.Min(Mathf.Min(p.x - bounds.xMin, bounds.xMax - p.x), Mathf.Min(p.y - bounds.yMin, bounds.yMax - p.y));
        }

        /// <summary>Like <see cref="DistanceToWall(Rect, Vector2)"/>, the walls round the stitched world's holes included (<see cref="OverworldLayout.WallDistance"/>).</summary>
        private static float DistanceToWall(OverworldLayout layout, Vector2 p) => layout.WallDistance(p);

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
            return t != OverworldTerrain.Steppe && t != OverworldTerrain.Ruins && t != OverworldTerrain.Ravine && t != OverworldTerrain.HighHills && !IsCave(t)
                && (!IsDesert(t) || OverworldTerrains.For(t).WaterVegetation.HasValue) && !OverworldTerrains.IsSeaShore(t) && t != OverworldTerrain.Mangrove;
        }

        /// <summary>A crop grows here (no trees, shrubs or tufts in the rows).</summary>
        private static bool InCrop(OverworldTerrain terrain, Vector2 p)
        {
            if (terrain != OverworldTerrain.Fields)
            {
                return false;
            }

            FieldPlotInfo plot = OverworldTerrains.PlotAt(p);
            return !plot.Lane && plot.Crop != FieldCrop.Fallow;
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

                    OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                    OverworldTerrainStyle style = OverworldTerrains.For(terrain);
                    float wall = DistanceToWall(layout, p);
                    if (wall < -20f && TownStyle.Hash01(salt, i + 280000) < 0.5f)
                    {
                        // Far backdrop: half the samples (the camera barely sees it).
                        continue;
                    }

                    bool crop = InCrop(terrain, p);
                    float trees = style.Rows || crop ? 0f : style.Trees;
                    float river = float.MaxValue;
                    if (wall < EdgeBelt)
                    {
                        // The belt of trees along the walls (and the backdrop beyond them).
                        trees = Mathf.Max(trees, wall < 0f ? 0.5f : 0.45f);
                    }
                    else if (Green(terrain) && !crop)
                    {
                        river = NearestRiverEdge(content, p);
                        if (river > 1.5f && river < 10f)
                        {
                            trees += 0.24f;
                        }
                    }

                    float roll = TownStyle.Hash01(salt, i + 80000);
                    float t0 = trees;
                    float t1 = t0 + (crop ? 0f : style.Bushes);
                    float t2 = t1 + style.Rocks;
                    float t3 = t2 + style.Mounds;
                    float t4 = t3 + style.Walls;
                    float t5 = t4 + (crop ? 0f : style.Bales);
                    float t6 = t5 + style.Logs;
                    float t7 = t6 + style.Stumps;
                    float t8 = t7 + style.MossRocks;
                    float t9 = t8 + (style.Special != OverworldDecor.None || style.SpecialMix != null ? style.Specials : 0f);
                    float s01 = TownStyle.Hash01(salt, i + 120000);
                    float yaw = TownStyle.Hash01(salt, i + 160000) * 360f;
                    if (roll < t0)
                    {
                        // Beyond the walls only the first (cheap) trees: the dense backdrop stays within the chunk budget.
                        bool second = style.SecondVegetation.HasValue && wall >= 0f && TownStyle.Hash01(salt, i + 200000) < style.SecondShare;
                        TownVegetation vegetation = second ? style.SecondVegetation.Value : style.Vegetation;
                        int[] variants = second ? style.SecondVariants : style.TreeVariants;
                        if (wall < 0f && !second)
                        {
                            // The coast's backdrop is casuarinas only, its lightest tree (palms and baobabs would break the budget).
                            variants = style.Vegetation == TownVegetation.Forest ? new[] { 0, 1, 2 } : style.Vegetation == TownVegetation.Coast ? CoastBackdrop : style.TreeVariants;
                        }

                        int variant = variants[(int)(TownStyle.Hash(salt, i + 240000) % (uint)variants.Length)];
                        if (river < 10f && style.WaterVegetation.HasValue && style.WaterVariants != null && style.WaterVariants.Length > 0)
                        {
                            // The area's own trees along the water (cedars and tree aloes by the Kaggen torrents).
                            vegetation = style.WaterVegetation.Value;
                            variant = style.WaterVariants[(int)(TownStyle.Hash(salt, i + 260000) % (uint)style.WaterVariants.Length)];
                        }
                        else if (river < 10f)
                        {
                            // The tugai along the water: tamarisks on the bank, gold and green poplars behind them.
                            vegetation = TownVegetation.Forest;
                            variant = river < 4.5f ? 2 : (s01 < 0.5f ? 1 : 0);
                        }

                        float scale = Mathf.Lerp(style.TreeScale.x, style.TreeScale.y, s01);
                        TryAdd(layout, content, p, 1.1f, new OverworldProp
                        {
                            Kind = CityPropKind.Tree, Position = p, Yaw = yaw, Scale = scale, Vegetation = vegetation, Variant = variant,
                        });
                    }
                    else if (roll < t1)
                    {
                        TryAdd(layout, content, p, 0.6f, new OverworldProp
                        {
                            Kind = CityPropKind.Bush, Position = p, Yaw = yaw, Scale = 0.8f + s01 * 0.7f, Vegetation = style.BushVegetation ?? style.Vegetation,
                        });
                    }
                    else if (roll < t2)
                    {
                        float scale = Mathf.Lerp(style.RockScale.x, style.RockScale.y, s01);
                        TryAdd(layout, content, p, 0.5f * scale + 0.4f, new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Position = p, Yaw = yaw, Scale = scale, Color = RockColor(terrain),
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
                    else if (roll < t6)
                    {
                        TryAdd(layout, content, p, 1.5f, new OverworldProp { Kind = CityPropKind.Rock, Decor = OverworldDecor.Log, Position = p, Yaw = yaw, Scale = 0.8f + 0.5f * s01 });
                    }
                    else if (roll < t7)
                    {
                        TryAdd(layout, content, p, 0.6f, new OverworldProp { Kind = CityPropKind.Rock, Decor = OverworldDecor.Stump, Position = p, Yaw = yaw, Scale = 0.8f + 0.5f * s01 });
                    }
                    else if (roll < t8)
                    {
                        TryAdd(layout, content, p, 0.9f, new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Decor = OverworldDecor.MossRock, Position = p, Yaw = yaw, Scale = 0.9f + 0.9f * s01, Color = RockColor(terrain),
                        });
                    }
                    else if (roll < t9)
                    {
                        OverworldDecor special = style.SpecialMix != null && style.SpecialMix.Length > 0
                            ? style.SpecialMix[(int)(TownStyle.Hash(salt, i + 320000) % (uint)style.SpecialMix.Length)]
                            : style.Special;
                        float scale = special == OverworldDecor.Cypress ? 0.85f + 0.35f * s01 : 0.8f + 0.6f * s01;
                        float room = IsDesertDecor(special) ? DesertClearance(special) : IsCoastDecor(special) ? CoastClearance(special) : special == OverworldDecor.Basalt ? 1.4f : 0.9f;
                        TryAdd(layout, content, p, room, new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Decor = special, Position = p, Yaw = yaw, Scale = scale,
                            Variant = (int)(TownStyle.Hash(salt, i + 300000) & 3),
                        });
                    }
                }
            }
        }

        private static readonly int[] CoastBackdrop = { 3 };

        private static TownSwatch RockColor(OverworldTerrain terrain)
        {
            TownSwatch? tint = OverworldTerrains.For(terrain).RockTint;
            if (tint.HasValue)
            {
                return tint.Value;
            }

            switch (terrain)
            {
                case OverworldTerrain.Ravine: return TownSwatch.Basalt;
                case OverworldTerrain.Ruins: return TownSwatch.Sandstone;
                case OverworldTerrain.Steppe: return TownSwatch.Sandstone;
                case OverworldTerrain.HighHills: return TownSwatch.StoneDark;
                default: return IsCave(terrain) ? CaveRockColor(terrain) : TownSwatch.Stone;
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
                    if (DistanceToWall(layout, p) < EdgeBelt || layout.InAuthoredArea(p))
                    {
                        continue;
                    }

                    OverworldTerrain terrain = content.Grid.Near(p).Terrain;
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

        /// <summary>
        /// The Campos do Aryk: every plot gets its crop as real rows (wheat ridges, cotton with white bolls, vine trellises,
        /// melon beds, stubble, furrows) on a world lattice, hay bales in the hay plots, and hedgerows on some lanes.
        /// </summary>
        private static void ScatterFields(OverworldLayout layout, OverworldChunkContent content)
        {
            Rect r = content.Rect;
            int row0 = Mathf.FloorToInt(r.yMin / OverworldTerrains.PlotDepth);
            int row1 = Mathf.FloorToInt((r.yMax - 0.001f) / OverworldTerrains.PlotDepth);
            for (int row = row0; row <= row1; row++)
            {
                float zMin = row * OverworldTerrains.PlotDepth;
                FieldPlotInfo first = OverworldTerrains.PlotAt(new Vector2(r.xMin, zMin + OverworldTerrains.PlotDepth * 0.5f));
                for (float x = first.Rect.xMin; x < r.xMax; x += OverworldTerrains.PlotWidth)
                {
                    FieldPlotInfo plot = OverworldTerrains.PlotAt(new Vector2(x + OverworldTerrains.PlotWidth * 0.5f, zMin + OverworldTerrains.PlotDepth * 0.5f));
                    PlantPlot(layout, content, plot);
                }
            }
        }

        private static void PlantPlot(OverworldLayout layout, OverworldChunkContent content, FieldPlotInfo plot)
        {
            Rect r = content.Rect;
            Rect p = plot.Rect;
            int seed = plot.Col * 7919 + plot.Row * 104729;
            float spacing;
            switch (plot.Crop)
            {
                case FieldCrop.Wheat: spacing = 1.25f; break;
                case FieldCrop.Cotton: spacing = 1.5f; break;
                case FieldCrop.Vines: spacing = 2.4f; break;
                case FieldCrop.Melons: spacing = 2f; break;
                case FieldCrop.Hay: spacing = 1.7f; break;
                case FieldCrop.Plowed: spacing = 1.1f; break;
                default: spacing = 0f; break;
            }

            if (spacing > 0f)
            {
                float x0 = p.xMin + OverworldTerrains.LaneX + 0.4f;
                float x1 = p.xMax - 0.4f;
                for (float z = p.yMin + OverworldTerrains.LaneZ + 0.6f; z < p.yMax - 0.4f; z += spacing)
                {
                    if (z < r.yMin || z >= r.yMax)
                    {
                        continue;
                    }

                    int piece = 0;
                    for (float x = x0; x < x1 - 0.5f; x += RowPiece, piece++)
                    {
                        if (x < r.xMin || x >= r.xMax)
                        {
                            continue;
                        }

                        var a = new Vector2(x, z);
                        var c = new Vector2(Mathf.Min(x + RowPiece, x1), z);
                        Vector2 mid = (a + c) * 0.5f;
                        if (content.Grid.Near(mid).Terrain != OverworldTerrain.Fields || !IsOpen(layout, content, a, 0.3f, false)
                            || !IsOpen(layout, content, c, 0.3f, false) || !IsOpen(layout, content, mid, 0.3f, false))
                        {
                            continue;
                        }

                        content.Details.Add(new OverworldProp
                        {
                            Decor = OverworldDecor.CropRow, Position = a, End = c, Variant = (int)plot.Crop, Scale = 1f,
                            Yaw = seed + piece * 31 + Mathf.RoundToInt(z * 7f),
                        });
                    }
                }
            }

            if (plot.Crop == FieldCrop.Hay)
            {
                for (int k = 0; k < 4; k++)
                {
                    var at = new Vector2(Mathf.Lerp(p.xMin + 3f, p.xMax - 3f, TownStyle.Hash01("bale", seed + k)),
                        Mathf.Lerp(p.yMin + 2.5f, p.yMax - 2f, TownStyle.Hash01("bale", seed + k + 50)));
                    if (r.Contains(at) && content.Grid.Near(at).Terrain == OverworldTerrain.Fields)
                    {
                        TryAdd(layout, content, at, 1f, new OverworldProp { Kind = CityPropKind.HayBale, Position = at, Yaw = TownStyle.Hash01("bale", seed + k + 90) * 180f, Scale = 1f });
                    }
                }
            }

            // Hedgerows: along the lane on the plot's low-z side (east-west) or its low-x side (north-south), with gaps.
            if (TownStyle.Hash01("hedge", seed) < 0.34f)
            {
                float z = p.yMin + OverworldTerrains.LaneZ * 0.5f;
                Hedges(layout, content, new Vector2(p.xMin + OverworldTerrains.LaneX, z), new Vector2(p.xMax, z), seed);
            }

            if (TownStyle.Hash01("hedge", seed + 1) < 0.22f)
            {
                float x = p.xMin + OverworldTerrains.LaneX * 0.5f;
                Hedges(layout, content, new Vector2(x, p.yMin + OverworldTerrains.LaneZ), new Vector2(x, p.yMax), seed + 17);
            }
        }

        private static void Hedges(OverworldLayout layout, OverworldChunkContent content, Vector2 from, Vector2 to, int seed)
        {
            float length = Vector2.Distance(from, to);
            Vector2 dir = (to - from) / length;
            int pieces = Mathf.FloorToInt(length / RowPiece);
            for (int k = 0; k < pieces; k++)
            {
                if (TownStyle.Hash01("hedgegap", seed * 31 + k) < 0.18f)
                {
                    continue;
                }

                Vector2 a = from + dir * (k * RowPiece);
                Vector2 c = a + dir * (RowPiece - 0.1f);
                Vector2 mid = (a + c) * 0.5f;
                if (!content.Rect.Contains(a) || content.Grid.Near(mid).Terrain != OverworldTerrain.Fields
                    || !IsFree(layout, content, c, 0.8f) || !IsFree(layout, content, mid, 0.8f))
                {
                    continue;
                }

                TryAdd(layout, content, a, 0.8f, new OverworldProp
                {
                    Kind = CityPropKind.FenceRun, Decor = OverworldDecor.Hedge, Position = a, End = c, Scale = 1f, Variant = seed + k,
                });
            }
        }

        /// <summary>
        /// The ground detail of every area (no colliders): grass tufts, wild flowers, reeds along the water and in the mire,
        /// ferns under the trees, pebbles, puddles. Densities and colours per terrain (<see cref="OverworldTerrainStyle"/>).
        /// </summary>
        private static void ScatterDetails(OverworldLayout layout, OverworldChunkContent content)
        {
            string salt = "owd" + content.Coord.x + "_" + content.Coord.y;
            int steps = Mathf.CeilToInt(layout.ChunkSize / DetailSpacing);
            int i = 0;
            for (int gx = 0; gx < steps; gx++)
            {
                for (int gz = 0; gz < steps; gz++, i++)
                {
                    var p = new Vector2(
                        content.Rect.xMin + (gx + 0.5f + (TownStyle.Hash01(salt, i) - 0.5f) * 0.9f) * DetailSpacing,
                        content.Rect.yMin + (gz + 0.5f + (TownStyle.Hash01(salt, i + 30000) - 0.5f) * 0.9f) * DetailSpacing);
                    if (!content.Rect.Contains(p))
                    {
                        continue;
                    }

                    float wall = DistanceToWall(layout, p);
                    if (wall < -14f)
                    {
                        continue;
                    }

                    ref ReliefInfo info = ref content.Grid.Near(p);
                    if (info.Clearing < 0.45f)
                    {
                        continue;
                    }

                    OverworldTerrain terrain = info.Terrain;
                    OverworldTerrainStyle style = OverworldTerrains.For(terrain);
                    bool crop = InCrop(terrain, p);
                    bool wet = info.RiverEdge > 0.2f && info.RiverEdge < 3.2f && terrain != OverworldTerrain.Steppe && terrain != OverworldTerrain.Ruins && !IsCave(terrain)
                        && !OverworldTerrains.IsSeaShore(terrain);
                    float tufts = crop ? 0f : style.Tufts;
                    float flowers = crop ? 0f : style.Flowers;
                    float reeds = style.Reeds + (wet ? 0.45f : 0f);
                    float pebbles = style.Pebbles + (info.RiverEdge > 0.2f && info.RiverEdge < 2.5f ? 0.08f : 0f);

                    float roll = TownStyle.Hash01(salt, i + 60000);
                    float t0 = reeds;
                    float t1 = t0 + tufts;
                    float t2 = t1 + flowers;
                    float t3 = t2 + style.Ferns;
                    float t4 = t3 + pebbles;
                    float t5 = t4 + style.Puddles;
                    if (roll >= t5)
                    {
                        continue;
                    }

                    float s01 = TownStyle.Hash01(salt, i + 90000);
                    float yaw = TownStyle.Hash01(salt, i + 120000) * 360f;
                    var prop = new OverworldProp { Position = p, Yaw = yaw, Scale = 1.2f + 0.7f * s01, Variant = i };
                    float clearance = 0.2f;
                    if (roll < t0)
                    {
                        prop.Decor = OverworldDecor.Reeds;
                        prop.Scale = 0.85f + 0.4f * s01;
                        clearance = 0.3f;
                    }
                    else if (roll < t1)
                    {
                        prop.Decor = OverworldDecor.Tufts;
                        prop.Color = style.TuftColor;
                        prop.Color2 = style.TuftTip;
                    }
                    else if (roll < t2)
                    {
                        prop.Decor = OverworldDecor.Flowers;
                        TownSwatch[] colors = style.FlowerColors;
                        prop.Color = colors[(int)(TownStyle.Hash(salt, i + 150000) % (uint)colors.Length)];
                        prop.Color2 = colors[(int)(TownStyle.Hash(salt, i + 180000) % (uint)colors.Length)];
                    }
                    else if (roll < t3)
                    {
                        prop.Decor = OverworldDecor.Ferns;
                        prop.Color = terrain == OverworldTerrain.Canopy ? TownSwatch.Pine : TownSwatch.Vine;
                        prop.Scale = 0.9f + 0.6f * s01;
                    }
                    else if (roll < t4)
                    {
                        prop.Decor = OverworldDecor.Pebbles;
                        prop.Color = RockColor(terrain);
                        prop.Scale = 0.8f + 0.7f * s01;
                    }
                    else
                    {
                        prop.Decor = OverworldDecor.Puddle;
                        prop.Scale = 0.8f + 0.8f * s01;
                        clearance = 1.2f;
                        if (content.Grid.Flatness(p) < 0.995f)
                        {
                            continue;
                        }
                    }

                    if (IsOpen(layout, content, p, clearance, false))
                    {
                        content.Details.Add(prop);
                    }
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
                    Variant = prop.Variant, Vegetation = FixedVegetation(layout, prop.Position),
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

            // Colliders start a little under the ground (slopes) and reach their height above it.
            const float sink = 0.4f;
            float ground = content.Grid.Sample(p) - sink;
            if (prop.Decor != OverworldDecor.None)
            {
                AddDecorSolid(content, prop, ground, sink);
                return;
            }

            switch (prop.Kind)
            {
                case CityPropKind.Tree:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = Mathf.Clamp(0.24f * prop.Scale, 0.16f, 0.55f), Height = 3f + sink });
                    break;
                case CityPropKind.Rock:
                    float radius = prop.Variant == 9 ? 0.42f * prop.Scale : 0.48f * prop.Scale;
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = radius, Height = Mathf.Max(1f, prop.Scale * (prop.Variant == 9 ? 0.42f : 0.7f)) + sink });
                    break;
                case CityPropKind.FenceRun:
                    AddRunSolid(content, prop, 0.6f, 1.2f, sink);
                    break;
                case CityPropKind.HayBale:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.2f, 0.75f), Height = 0.8f + sink, Yaw = prop.Yaw });
                    break;
            }
        }

        private static void AddRunSolid(OverworldChunkContent content, OverworldProp prop, float width, float height, float sink)
        {
            Vector2 d = prop.End - prop.Position;
            Vector2 mid = (prop.Position + prop.End) * 0.5f;
            float low = Mathf.Min(content.Grid.Sample(prop.Position), content.Grid.Sample(prop.End));
            float high = Mathf.Max(content.Grid.Sample(prop.Position), content.Grid.Sample(prop.End));
            content.Solids.Add(new OverworldSolid
            {
                Center = mid, Base = low - sink, IsBox = true, Size = new Vector2(width, d.magnitude), Height = height + (high - low) + sink,
                Yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg,
            });
        }

        private static void AddDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            Vector2 p = prop.Position;
            float s = prop.Scale;
            if (AddCaveDecorSolid(content, prop, ground, sink) || AddMountainDecorSolid(content, prop, ground, sink)
                || AddDesertDecorSolid(content, prop, ground, sink) || AddCoastDecorSolid(content, prop, ground, sink))
            {
                return;
            }

            switch (prop.Decor)
            {
                case OverworldDecor.Hedge:
                    AddRunSolid(content, prop, 0.9f, 1.3f, sink);
                    break;
                case OverworldDecor.Log:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.6f * s, 0.5f * s), Height = 0.5f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.Stump:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.36f * s, Height = 0.6f * s + sink });
                    break;
                case OverworldDecor.MossRock:
                case OverworldDecor.Cairn:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.55f * s, Height = 1.1f * s + sink });
                    break;
                case OverworldDecor.Basalt:
                    content.Solids.Add(new OverworldSolid { Center = p + Rotate(new Vector2(0.2f, 0.3f) * s, prop.Yaw), Base = ground, Radius = 0.95f * s, Height = 2.2f * s + sink });
                    break;
                case OverworldDecor.Column:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.45f * s, Height = 1.8f * s + sink });
                    break;
                case OverworldDecor.Cypress:
                case OverworldDecor.DeadTree:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.22f * s, Height = 3f + sink });
                    break;
            }
        }

        /// <summary>A local offset turned like <see cref="TownMeshBuilder.With(Vector3,float,System.Action)"/> turns (yaw about +y).</summary>
        private static Vector2 Rotate(Vector2 v, float yaw)
        {
            float a = -yaw * Mathf.Deg2Rad;
            float c = Mathf.Cos(a);
            float s = Mathf.Sin(a);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
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
                        Vector2 at = landmark.Position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 7.5f * s;
                        content.Solids.Add(new OverworldSolid { Center = at, Base = content.Grid.Sample(at) - 0.4f, Radius = 0.5f, Height = 2.8f });
                    }

                    break;
                case MapLandmarkKind.WaterMill:
                    content.Solids.Add(new OverworldSolid
                    {
                        Center = landmark.Position, Base = content.Grid.Sample(landmark.Position) - 0.4f, IsBox = true, Size = new Vector2(4.4f, 3.4f), Height = 3.4f,
                        Yaw = landmark.Yaw,
                    });
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Meshes
        // ------------------------------------------------------------------

        /// <summary>
        /// The ground of the chunk: the relief as low-poly facets, each coloured after its terrain (borders dithered between
        /// the corners' terrains), its tone, its slope (bare earth and rock on the steep ones), the clearings, the river banks
        /// and beds; the trails draped over it with ragged edges; the water.
        /// </summary>
        public static void BuildGround(TownMeshBuilder b, OverworldLayout layout, OverworldChunkContent content)
        {
            OverworldGroundGrid g = content.Grid;
            int m = OverworldGroundGrid.Margin;
            int cells = g.N - 1 - 2 * m;
            for (int lj = m; lj < m + cells; lj++)
            {
                for (int li = m; li < m + cells; li++)
                {
                    Vector2 c = g.VertexAt(li, lj) + new Vector2(GroundCell * 0.5f, GroundCell * 0.5f);
                    if (InTownGround(content, c))
                    {
                        continue;
                    }

                    int k00 = g.Index(li, lj);
                    int k10 = k00 + 1;
                    int k01 = k00 + g.N;
                    int k11 = k01 + 1;
                    Vector3 v00 = Vertex(g, li, lj, k00);
                    Vector3 v10 = Vertex(g, li + 1, lj, k10);
                    Vector3 v01 = Vertex(g, li, lj + 1, k01);
                    Vector3 v11 = Vertex(g, li + 1, lj + 1, k11);
                    int wi = g.I0 + li;
                    int wj = g.J0 + lj;
                    uint hash = TownStyle.Hash("owtri", wi * 7349 + wj * 1301);
                    if (OverworldRelief.MainDiagonal(wi, wj))
                    {
                        b.Triangle(v00, v11, v10, Facet(layout, g, k00, k11, k10, v00, v11, v10, hash));
                        b.Triangle(v00, v01, v11, Facet(layout, g, k00, k01, k11, v00, v01, v11, hash >> 8));
                    }
                    else
                    {
                        b.Triangle(v00, v01, v10, Facet(layout, g, k00, k01, k10, v00, v01, v10, hash));
                        b.Triangle(v10, v01, v11, Facet(layout, g, k10, k01, k11, v10, v01, v11, hash >> 8));
                    }
                }
            }

            foreach (MapPath path in content.NearPaths)
            {
                DrapeTrail(b, layout, content, path);
            }

            BuildWater(b, content);
        }

        private static Vector3 Vertex(OverworldGroundGrid g, int li, int lj, int k)
        {
            Vector2 p = g.VertexAt(li, lj);
            return new Vector3(p.x, g.Height[k] + GroundY, p.y);
        }

        /// <summary>The colour of one facet (corners <paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/>).</summary>
        private static TownSwatch Facet(OverworldLayout layout, OverworldGroundGrid g, int a, int b, int c, Vector3 pa, Vector3 pb, Vector3 pc, uint hash)
        {
            // The terrain of one corner, picked by hash: borders between areas come out dithered, never straight.
            int pick = (int)(hash % 3u);
            ref ReliefInfo info = ref g.Info[pick == 0 ? a : pick == 1 ? b : c];
            OverworldTerrainStyle style = OverworldTerrains.For(info.Terrain);
            float jitter = ((hash >> 4) & 255) / 255f - 0.5f;

            float river = Mathf.Min(g.Info[a].RiverEdge, Mathf.Min(g.Info[b].RiverEdge, g.Info[c].RiverEdge));
            bool carved = g.Info[a].RiverCarved || g.Info[b].RiverCarved || g.Info[c].RiverCarved;
            if (carved && river < -0.3f)
            {
                return style.Bed;
            }

            if (river < 1.3f + jitter * 1.2f)
            {
                return style.Bank;
            }

            float clearing = Mathf.Min(g.Info[a].Clearing, Mathf.Min(g.Info[b].Clearing, g.Info[c].Clearing));
            if (clearing < 0.42f + jitter * 0.25f)
            {
                return TownSwatch.Dirt;
            }

            Vector3 n = Vector3.Cross(pb - pa, pc - pa);
            float up = n.y / Mathf.Max(1e-5f, n.magnitude);
            if (up < 0.83f + jitter * 0.05f)
            {
                return style.Cliff;
            }

            if (clearing < 0.92f + jitter * 0.3f)
            {
                return style.Clearing;
            }

            if (up < 0.94f + jitter * 0.04f && ((hash >> 12) & 3) != 0)
            {
                return style.Slope;
            }

            // Height bands: golden or bare tops, darker hollows (the relief reads even under a flat light).
            float tone = (g.Tone[a] + g.Tone[b] + g.Tone[c]) / 3f + jitter * 0.12f;
            float h = (pa.y + pb.y + pc.y) / 3f - GroundY;
            float rel = style.Relief > 0.5f ? h / style.Relief + jitter * 0.08f : 0.5f;
            if (style.Crest.HasValue && rel > 0.68f && tone > 0.3f && ((hash >> 16) & 7) != 0)
            {
                return style.Crest.Value;
            }

            bool free = g.Info[a].Free > 0.85f && g.Info[b].Free > 0.85f && g.Info[c].Free > 0.85f;
            if (style.Hollow.HasValue && free && rel < 0.18f && tone < 0.7f)
            {
                return style.Hollow.Value;
            }

            Vector2 centroid = new Vector2((pa.x + pb.x + pc.x) / 3f, (pa.z + pb.z + pc.z) / 3f);
            float fine = (g.Fine[a] + g.Fine[b] + g.Fine[c]) / 3f + jitter * 0.08f;
            return OverworldTerrains.GroundFor(info.Terrain, centroid, tone, fine);
        }

        /// <summary>A trail draped over the facets: a dark worn border and the trail surface, both with ragged edges.</summary>
        private static void DrapeTrail(TownMeshBuilder b, OverworldLayout layout, OverworldChunkContent content, MapPath path)
        {
            if (InTownGround(content, path.A) && InTownGround(content, path.B))
            {
                return;
            }

            TownSwatch street = OverworldTerrains.For(content.Grid.Near((path.A + path.B) * 0.5f).Terrain).Trail;
            Vector2 d = path.B - path.A;
            float length = d.magnitude;
            if (length < 0.01f)
            {
                return;
            }

            d /= length;
            float half = path.Width * 0.5f;
            float ext = half * 0.6f;
            Vector2 a = path.A - d * ext;
            float total = length + 2f * ext;
            int pieces = Mathf.Max(1, Mathf.CeilToInt(total / 2f));
            Vector2 n = new Vector2(-d.y, d.x);
            Rect r = content.Rect;
            for (int k = 0; k < pieces; k++)
            {
                Vector2 p0 = a + d * (total * k / pieces);
                Vector2 p1 = a + d * (total * (k + 1) / pieces);
                Vector2 mid = (p0 + p1) * 0.5f;
                if (!r.Contains(mid) || InTownGround(content, mid))
                {
                    continue;
                }

                ref ReliefInfo at = ref content.Grid.Near(mid);
                if (at.RiverCarved && at.RiverEdge < 1.5f)
                {
                    // Under a bridge or through a ford: the trail does not dive into the river bed.
                    continue;
                }

                float j00 = Ragged(p0, 0), j01 = Ragged(p0, 1), j10 = Ragged(p1, 0), j11 = Ragged(p1, 1);
                Quad(b, content.Grid, p0 + n * (half + 0.35f + j00), p1 + n * (half + 0.35f + j10), p1 - n * (half + 0.35f + j11), p0 - n * (half + 0.35f + j01), 0.03f, TownSwatch.DirtDark);
                Quad(b, content.Grid, p0 + n * (half + j00 * 0.6f), p1 + n * (half + j10 * 0.6f), p1 - n * (half + j11 * 0.6f), p0 - n * (half + j01 * 0.6f), 0.045f, street);
            }
        }

        /// <summary>A small offset (m) of a trail edge at a world point: the same on both chunks and both pieces sharing it.</summary>
        private static float Ragged(Vector2 p, int side)
        {
            int key = Mathf.RoundToInt(p.x * 4f) * 73856093 ^ Mathf.RoundToInt(p.y * 4f) * 19349663 ^ side * 83492791;
            return (TownStyle.Hash01("rag", key) - 0.5f) * 0.5f;
        }

        private static void Quad(TownMeshBuilder b, OverworldGroundGrid g, Vector2 a, Vector2 c, Vector2 e, Vector2 f, float lift, TownSwatch swatch)
        {
            var pa = new Vector3(a.x, g.Sample(a) + lift, a.y);
            var pc = new Vector3(c.x, g.Sample(c) + lift, c.y);
            var pe = new Vector3(e.x, g.Sample(e) + lift, e.y);
            var pf = new Vector3(f.x, g.Sample(f) + lift, f.y);
            b.TriangleFacing(pa, pc, pe, Vector3.up, swatch);
            b.TriangleFacing(pa, pe, pf, Vector3.up, swatch);
        }

        /// <summary>Rivers and ponds: carved ones at the water line in their bed, narrow ones on the valley floor with a sandy margin; fords.</summary>
        private static void BuildWater(TownMeshBuilder b, OverworldChunkContent content)
        {
            Rect r = content.Rect;
            Rect near = CityGeometry.Inflate(r, 12f);
            foreach (MapRiver river in content.NearRivers)
            {
                bool carved = river.Width >= OverworldRelief.CarveMinWidth;
                float y = carved ? OverworldRelief.WaterY : GroundY + 0.026f;
                float half = carved ? river.Width * 0.5f + 0.9f : river.Width * 0.5f;
                if (!carved)
                {
                    for (int i = 0; i + 1 < river.Points.Count; i++)
                    {
                        if (SegmentNear(near, river.Points[i], river.Points[i + 1], river.Width))
                        {
                            Strip(b, river.Points[i], river.Points[i + 1], half + 1.2f, GroundY + 0.02f, TownSwatch.Sand);
                        }
                    }

                    foreach (Vector2 p in river.Points)
                    {
                        if (near.Contains(p))
                        {
                            b.Disc(new Vector3(p.x, GroundY + 0.02f, p.y), half + 1.2f, 10, TownSwatch.Sand);
                        }
                    }
                }

                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    if (SegmentNear(near, river.Points[i], river.Points[i + 1], river.Width))
                    {
                        Strip(b, river.Points[i], river.Points[i + 1], half, y, TownSwatch.Water);
                    }
                }

                foreach (Vector2 p in river.Points)
                {
                    if (near.Contains(p))
                    {
                        b.Disc(new Vector3(p.x, y, p.y), half, 10, TownSwatch.Water);
                    }
                }

                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (crossing.IsBridge || !r.Contains(crossing.At))
                    {
                        continue;
                    }

                    if (!carved)
                    {
                        b.Disc(new Vector3(crossing.At.x, GroundY + 0.03f, crossing.At.y), crossing.Width * 0.5f, 10, TownSwatch.SandDark);
                    }

                    Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                    for (int s = -3; s <= 3; s++)
                    {
                        Vector2 p = crossing.At + across * (s * 1.3f * river.Width / 9f) + new Vector2(-across.y, across.x) * ((s & 1) * 0.6f - 0.3f);
                        float ground = carved ? Mathf.Max(content.Grid.Sample(p), OverworldRelief.WaterY - 0.1f) : 0f;
                        b.Crown(new Vector3(p.x, ground + 0.05f, p.y), new Vector3(0.8f, 0.18f, 0.7f), TownSwatch.Stone, s * 37f);
                    }
                }
            }
        }

        /// <summary>The scattered props, ground detail, landmarks and bridges of the chunk, standing on its relief, merged into one mesh.</summary>
        public static void BuildProps(TownMeshBuilder b, OverworldChunkContent content)
        {
            TownStyle forest = TownStyles.For(BiomeType.Forest);
            OverworldGroundGrid g = content.Grid;
            foreach (OverworldProp prop in content.Props)
            {
                OverworldProp captured = prop;
                var at = new Vector3(prop.Position.x, g.Sample(prop.Position), prop.Position.y);
                if (prop.Decor != OverworldDecor.None)
                {
                    AppendDecor(b, g, captured, at);
                    continue;
                }

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
                        float low = Mathf.Min(at.y, g.Sample(prop.End));
                        TownKit.WallRun(b, new Vector3(at.x, low, at.z), new Vector3(prop.End.x, low, prop.End.y), forest, prop.Scale);
                        break;
                    default:
                        var kit = new CityProp { Kind = prop.Kind, Scale = prop.Scale, Variant = prop.Variant };
                        b.With(at, prop.Yaw, () => TownKit.Prop(b, kit, forest));
                        break;
                }
            }

            foreach (OverworldProp detail in content.Details)
            {
                AppendDecor(b, g, detail, new Vector3(detail.Position.x, g.Sample(detail.Position), detail.Position.y));
            }

            foreach (MapLandmark landmark in content.Landmarks)
            {
                AppendLandmark(b, landmark, g);
            }

            foreach ((MapRiver river, MapCrossing crossing) in content.Bridges)
            {
                Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                float yaw = CityGeometry.YawFacing(across);
                float width = crossing.Width;
                float span = BridgeSpan(river);
                b.With(new Vector3(crossing.At.x, Mathf.Max(0f, g.Sample(crossing.At)), crossing.At.y), yaw, () => MapAreaView.BridgeMesh(b, width, span));
            }
        }

        private static void AppendDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.CropRow:
                    OverworldDecorKit.CropRow(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), (FieldCrop)prop.Variant, (int)prop.Yaw);
                    return;
                case OverworldDecor.Hedge:
                    OverworldDecorKit.Hedge(b, at - Vector3.up * 0.1f, new Vector3(prop.End.x, g.Sample(prop.End) - 0.1f, prop.End.y), prop.Variant);
                    return;
                case OverworldDecor.Tufts:
                    b.Stamp(OverworldDecorKit.TuftStamp, at, prop.Yaw, s, prop.Color, prop.Color2);
                    return;
                case OverworldDecor.Flowers:
                    b.Stamp(OverworldDecorKit.FlowerStamp, at, prop.Yaw, s, prop.Color, prop.Color2);
                    return;
                case OverworldDecor.Reeds:
                    b.Stamp(OverworldDecorKit.ReedStamp, at, prop.Yaw, s, TownSwatch.Vine);
                    return;
                case OverworldDecor.Pebbles:
                    b.Stamp(OverworldDecorKit.PebbleStamp, at, prop.Yaw, s, prop.Color);
                    return;
                case OverworldDecor.Ferns:
                    b.Stamp(OverworldDecorKit.FernStamp, at, prop.Yaw, s, prop.Color);
                    return;
                case OverworldDecor.Puddle:
                    b.Stamp(OverworldDecorKit.PuddleStamps[prop.Variant & 3], at, prop.Yaw, s, TownSwatch.Water);
                    return;
            }

            if (AppendCaveDecor(b, g, prop, at) || AppendMountainDecor(b, g, prop, at) || AppendDesertDecor(b, g, prop, at) || AppendCoastDecor(b, g, prop, at))
            {
                return;
            }

            // Small pieces: one placement matrix, no closure (the scatter has hundreds of them per chunk).
            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.Log: OverworldDecorKit.Log(b, s); break;
                case OverworldDecor.Stump: OverworldDecorKit.Stump(b, s); break;
                case OverworldDecor.MossRock: OverworldDecorKit.MossRock(b, s, prop.Color); break;
                case OverworldDecor.Cairn: OverworldDecorKit.Cairn(b, s); break;
                case OverworldDecor.Basalt: OverworldDecorKit.Basalt(b, s); break;
                case OverworldDecor.Column: OverworldDecorKit.Column(b, s, prop.Variant); break;
                case OverworldDecor.Cypress: OverworldDecorKit.Cypress(b, s); break;
                case OverworldDecor.DeadTree: OverworldDecorKit.DeadTree(b, s); break;
            }

            b.Matrix = saved;
        }

        public static void AppendLandmark(TownMeshBuilder b, MapLandmark landmark, OverworldGroundGrid grid = null)
        {
            float s = landmark.Scale <= 0f ? 1f : landmark.Scale;
            float y = grid != null ? grid.Sample(landmark.Position) : 0f;
            var at = new Vector3(landmark.Position.x, y, landmark.Position.y);
            switch (landmark.Kind)
            {
                case MapLandmarkKind.StoneRing:
                    for (int k = 0; k < 9; k++)
                    {
                        float a = k * Mathf.PI * 2f / 9f;
                        var p = landmark.Position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 7.5f * s;
                        int variant = k % 3;
                        float py = grid != null ? grid.Sample(p) : 0f;
                        b.With(new Vector3(p.x, py, p.y), a * Mathf.Rad2Deg + 90f, () => MapAreaView.RingStone(b, variant));
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

        /// <summary>On a town's ground (the towns near the chunk: <see cref="OverworldChunkContent.NearTowns"/> holds every one that reaches it).</summary>
        private static bool InTownGround(OverworldChunkContent content, Vector2 p)
        {
            foreach (OverworldSettlement settlement in content.NearTowns)
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

        // ------------------------------------------------------------------
        // Collider mesh
        // ------------------------------------------------------------------

        /// <summary>The chunk's ground collider: the same facets as the drawn ground (shared vertices), whole chunk, town ground included.</summary>
        public static void BuildColliderMesh(OverworldChunkContent content, List<Vector3> vertices, List<int> triangles)
        {
            vertices.Clear();
            triangles.Clear();
            OverworldGroundGrid g = content.Grid;
            int m = OverworldGroundGrid.Margin;
            int cells = g.N - 1 - 2 * m;
            int side = cells + 1;
            for (int lj = m; lj <= m + cells; lj++)
            {
                for (int li = m; li <= m + cells; li++)
                {
                    Vector2 p = g.VertexAt(li, lj);
                    vertices.Add(new Vector3(p.x, g.Height[g.Index(li, lj)], p.y));
                }
            }

            for (int j = 0; j < cells; j++)
            {
                for (int i = 0; i < cells; i++)
                {
                    int v00 = j * side + i;
                    int v10 = v00 + 1;
                    int v01 = v00 + side;
                    int v11 = v01 + 1;
                    if (OverworldRelief.MainDiagonal(g.I0 + m + i, g.J0 + m + j))
                    {
                        triangles.Add(v00); triangles.Add(v11); triangles.Add(v10);
                        triangles.Add(v00); triangles.Add(v01); triangles.Add(v11);
                    }
                    else
                    {
                        triangles.Add(v00); triangles.Add(v01); triangles.Add(v10);
                        triangles.Add(v10); triangles.Add(v01); triangles.Add(v11);
                    }
                }
            }
        }
    }
}
