using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 4 — Grutas do Limiar: the cave part of the chunk builder (Docs/Mapas100.md 19–24). Pure and deterministic like
    /// the rest: per region (computed once, bucketed by chunk) the tall tuff walls along the borders of the halls (open at
    /// every trail, river, clearing and in noise gaps: the halls stay connected), the vault walls round the region (outside
    /// it, no colliders), the black void over the chasms and the rope posts at their bridges, the lamp niches and air pipes
    /// along the trails and the region's monuments (<see cref="AddCavePiece"/>); per chunk, the scatter of each cave terrain
    /// (stalagmites, crystals, columns, giant mushrooms, nests, jars, mine carts…). Non-cave regions pay one table lookup.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>Distance (m) between two cave-scatter samples.</summary>
        public const float CaveSampleSpacing = 5f;

        /// <summary>Length (m) of one piece of tuff wall.</summary>
        public const float CaveWallPiece = 9f;

        private enum CavePieceKind
        {
            Placed = 0,
            Forced = 1,
            InnerWall = 2,
            Detail = 3,
        }

        private struct CavePiece
        {
            public OverworldProp Prop;
            public CavePieceKind Kind;
        }

        private sealed class CaveData
        {
            public readonly List<CavePiece> Explicit = new List<CavePiece>();
            public readonly HashSet<MapRiver> Abysses = new HashSet<MapRiver>();
            public Dictionary<Vector2Int, List<CavePiece>> ByChunk;
            public bool HasCaves;
        }

        private struct CaveScatter
        {
            public OverworldDecor Decor;
            public float Density;
            public float Clearance;
            public float MinScale;
            public float MaxScale;

            public CaveScatter(OverworldDecor decor, float density, float clearance, float minScale, float maxScale)
            {
                Decor = decor;
                Density = density;
                Clearance = clearance;
                MinScale = minScale;
                MaxScale = maxScale;
            }
        }

        private static readonly ConditionalWeakTable<OverworldLayout, CaveData> CaveTable = new ConditionalWeakTable<OverworldLayout, CaveData>();
        private static readonly CaveScatter[][] CaveTables = BuildCaveTables();
        private static readonly CaveScatter[] NoCaveScatter = new CaveScatter[0];

        /// <summary>A cave terrain (Região 4, 40–59).</summary>
        public static bool IsCave(OverworldTerrain t)
        {
            int i = (int)t;
            return i >= 40 && i < 60;
        }

        /// <summary>The colour of the rocks and pebbles of a cave terrain.</summary>
        private static TownSwatch CaveRockColor(OverworldTerrain terrain) => OverworldTerrains.For(terrain).RockTint ?? TownSwatch.StoneDark;

        /// <summary>A monument of the region (the vishapakar, the wheel, the ossuary portal…): placed when free, or always with <paramref name="force"/>.</summary>
        internal static void AddCavePiece(OverworldLayout layout, OverworldDecor decor, Vector2 at, float yaw, float scale, int variant = 0, bool force = false)
        {
            CaveData data = CaveDataOf(layout);
            data.Explicit.Add(new CavePiece
            {
                Prop = new OverworldProp { Kind = CityPropKind.Rock, Decor = decor, Position = at, Yaw = yaw, Scale = scale, Variant = variant },
                Kind = force ? CavePieceKind.Forced : CavePieceKind.Placed,
            });
            data.ByChunk = null;
        }

        /// <summary>Marks <paramref name="river"/> as a chasm: drawn as a black void, rope posts at its bridges.</summary>
        internal static void MarkAbyss(OverworldLayout layout, MapRiver river)
        {
            CaveData data = CaveDataOf(layout);
            data.Abysses.Add(river);
            data.ByChunk = null;
        }

        /// <summary>The chasms of the region (tests, tools).</summary>
        public static bool IsAbyss(OverworldLayout layout, MapRiver river) => CaveDataOf(layout).Abysses.Contains(river);

        private static CaveData CaveDataOf(OverworldLayout layout) => CaveTable.GetValue(layout, _ => new CaveData());

        /// <summary>
        /// The stitched world takes over the monuments and chasms of a cave region already moved to world coordinates
        /// (<paramref name="from"/>'s pieces shifted by <paramref name="offset"/>; its rivers are the same objects).
        /// </summary>
        internal static void CopyCaveData(OverworldLayout from, OverworldLayout to, Vector2 offset)
        {
            CaveData source = CaveDataOf(from);
            CaveData target = CaveDataOf(to);
            foreach (CavePiece piece in source.Explicit)
            {
                CavePiece moved = piece;
                moved.Prop.Position += offset;
                moved.Prop.End += offset;
                target.Explicit.Add(moved);
            }

            target.Abysses.UnionWith(source.Abysses);
            target.ByChunk = null;
        }

        // ------------------------------------------------------------------
        // Per chunk
        // ------------------------------------------------------------------

        private static void ScatterCaves(OverworldLayout layout, OverworldChunkContent content)
        {
            CaveData data = CaveDataOf(layout);
            EnsureCaveBuckets(layout, data);
            if (!data.HasCaves)
            {
                return;
            }

            int blocked = content.Blocked.Count;
            if (data.ByChunk.TryGetValue(content.Coord, out List<CavePiece> pieces))
            {
                foreach (CavePiece piece in pieces)
                {
                    OverworldProp prop = piece.Prop;
                    switch (piece.Kind)
                    {
                        case CavePieceKind.Detail:
                            content.Details.Add(prop);
                            break;
                        case CavePieceKind.InnerWall:
                            if (WallFits(layout, content, prop, blocked))
                            {
                                content.Props.Add(prop);
                                AddSolid(layout, content, prop);
                                content.Blocked.Add(WallRect(prop));
                            }

                            break;
                        case CavePieceKind.Forced:
                            content.Props.Add(prop);
                            AddSolid(layout, content, prop);
                            if (prop.Decor != OverworldDecor.TuffWall)
                            {
                                float r = CaveClearance(prop.Decor, prop.Scale) + 0.5f;
                                content.Blocked.Add(new Rect(prop.Position - new Vector2(r, r), new Vector2(r * 2f, r * 2f)));
                            }

                            break;
                        default:
                            TryAdd(layout, content, prop.Position, CaveClearance(prop.Decor, prop.Scale), prop);
                            break;
                    }
                }
            }

            // Only where a cave terrain is drawn (the stitched world's other regions skip the per-sample pass).
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsCave(info.Terrain))
                {
                    ScatterCaveGrid(layout, content);
                    break;
                }
            }
        }

        /// <summary>Each cave terrain's own pieces on a jittered grid (<see cref="CaveSampleSpacing"/>).</summary>
        private static void ScatterCaveGrid(OverworldLayout layout, OverworldChunkContent content)
        {
            string salt = "owcave" + content.Coord.x + "_" + content.Coord.y;
            int steps = Mathf.CeilToInt(layout.ChunkSize / CaveSampleSpacing);
            int i = 0;
            for (int gx = 0; gx < steps; gx++)
            {
                for (int gz = 0; gz < steps; gz++, i++)
                {
                    var p = new Vector2(
                        content.Rect.xMin + (gx + 0.5f + (TownStyle.Hash01(salt, i) - 0.5f) * 0.8f) * CaveSampleSpacing,
                        content.Rect.yMin + (gz + 0.5f + (TownStyle.Hash01(salt, i + 50000) - 0.5f) * 0.8f) * CaveSampleSpacing);
                    if (!content.Rect.Contains(p) || DistanceToWall(layout.Bounds, p) < EdgeBelt || layout.InAuthoredArea(p))
                    {
                        continue;
                    }

                    OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                    CaveScatter[] table = CaveTableOf(terrain);
                    float roll = TownStyle.Hash01(salt, i + 100000);
                    float acc = 0f;
                    foreach (CaveScatter entry in table)
                    {
                        acc += entry.Density;
                        if (roll >= acc)
                        {
                            continue;
                        }

                        float s01 = TownStyle.Hash01(salt, i + 150000);
                        float scale = Mathf.Lerp(entry.MinScale, entry.MaxScale, s01);
                        TryAdd(layout, content, p, entry.Clearance * scale, new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Decor = entry.Decor, Position = p, Yaw = TownStyle.Hash01(salt, i + 200000) * 360f, Scale = scale,
                            Variant = (int)(TownStyle.Hash(salt, i + 250000) & 7), Color = CaveRockColor(terrain),
                        });
                        break;
                    }
                }
            }
        }

        private static CaveScatter[] CaveTableOf(OverworldTerrain terrain)
        {
            int i = (int)terrain - 40;
            return i >= 0 && i < CaveTables.Length && CaveTables[i] != null ? CaveTables[i] : NoCaveScatter;
        }

        private static CaveScatter[][] BuildCaveTables()
        {
            var t = new CaveScatter[20][];
            CaveScatter Stalagmite(float d) => new CaveScatter(OverworldDecor.Stalagmite, d, 1.0f, 0.7f, 1.6f);
            CaveScatter Crystal(float d) => new CaveScatter(OverworldDecor.Crystal, d, 1.0f, 0.7f, 1.5f);
            CaveScatter Column(float d) => new CaveScatter(OverworldDecor.CaveColumn, d, 1.6f, 0.85f, 1.2f);
            CaveScatter Mushroom(float d) => new CaveScatter(OverworldDecor.GiantMushroom, d, 1.5f, 0.8f, 1.4f);
            t[OverworldTerrain.CaveMouth - OverworldTerrain.CaveMouth] = new[] { Stalagmite(0.03f), Mushroom(0.012f), Crystal(0.005f) };
            t[OverworldTerrain.LakeShore - OverworldTerrain.CaveMouth] = new[] { Mushroom(0.025f), Stalagmite(0.02f), Crystal(0.008f) };
            t[OverworldTerrain.StalactiteHall - OverworldTerrain.CaveMouth] = new[] { Stalagmite(0.09f), Column(0.012f) };
            t[OverworldTerrain.MushroomGrove - OverworldTerrain.CaveMouth] = new[] { Mushroom(0.08f), Stalagmite(0.01f) };
            t[OverworldTerrain.CrystalField - OverworldTerrain.CaveMouth] = new[] { Crystal(0.09f), Stalagmite(0.015f) };
            t[OverworldTerrain.Chasm - OverworldTerrain.CaveMouth] = new[] { Stalagmite(0.025f), Crystal(0.01f) };
            t[OverworldTerrain.TuffTerraces - OverworldTerrain.CaveMouth] = new[] { Stalagmite(0.02f), Mushroom(0.012f), Crystal(0.01f) };
            t[OverworldTerrain.ColumnForest - OverworldTerrain.CaveMouth] = new[] { Column(0.07f), Stalagmite(0.02f) };
            t[OverworldTerrain.MossMeadow - OverworldTerrain.CaveMouth] = new[] { Mushroom(0.02f), Crystal(0.008f), Stalagmite(0.008f) };
            t[OverworldTerrain.TunnelMarket - OverworldTerrain.CaveMouth] = new[]
            {
                new CaveScatter(OverworldDecor.MineCart, 0.01f, 1.8f, 1f, 1f), new CaveScatter(OverworldDecor.AirPipe, 0.012f, 1.8f, 1f, 1.2f), Stalagmite(0.02f),
            };
            t[OverworldTerrain.WheelCanyon - OverworldTerrain.CaveMouth] = new[] { new CaveScatter(OverworldDecor.Jar, 0.025f, 0.8f, 0.9f, 1.5f), Column(0.01f), Stalagmite(0.02f) };
            t[OverworldTerrain.Roost - OverworldTerrain.CaveMouth] = new[] { new CaveScatter(OverworldDecor.Nest, 0.015f, 1.8f, 0.9f, 1.4f), Stalagmite(0.025f) };
            return t;
        }

        private static float CaveClearance(OverworldDecor decor, float s)
        {
            switch (decor)
            {
                case OverworldDecor.CaveColumn: return 1.6f * s;
                case OverworldDecor.GiantMushroom: return 1.5f * s;
                case OverworldDecor.Vishapakar: return 1.2f * s;
                case OverworldDecor.Nest: return 1.8f * s;
                case OverworldDecor.MineCart:
                case OverworldDecor.AirPipe: return 1.8f;
                case OverworldDecor.JarWheel: return 7f;
                case OverworldDecor.OssuaryPortal: return 4.5f;
                case OverworldDecor.LampNiche: return 0.6f;
                case OverworldDecor.RopePosts: return 0.5f;
                default: return 1f * s;
            }
        }

        // ------------------------------------------------------------------
        // Region-wide features (computed once, bucketed by chunk)
        // ------------------------------------------------------------------

        private static void EnsureCaveBuckets(OverworldLayout layout, CaveData data)
        {
            if (data.ByChunk != null)
            {
                return;
            }

            data.ByChunk = new Dictionary<Vector2Int, List<CavePiece>>();
            data.HasCaves = layout.Zones.Exists(z => IsCave(z.Terrain));
            if (!data.HasCaves)
            {
                return;
            }

            var all = new List<(CavePiece Piece, Vector2 Anchor)>();
            foreach (CavePiece piece in data.Explicit)
            {
                all.Add((piece, piece.Prop.Position));
            }

            AddHallWalls(layout, all);
            AddVaultWalls(layout, all);
            AddAbysses(layout, data, all);
            AddTrailDressing(layout, all);
            foreach ((CavePiece piece, Vector2 anchor) in all)
            {
                Vector2Int c = OverworldRules.ChunkOf(layout, anchor);
                if (!data.ByChunk.TryGetValue(c, out List<CavePiece> list))
                {
                    list = new List<CavePiece>();
                    data.ByChunk[c] = list;
                }

                list.Add(piece);
            }
        }

        /// <summary>Tuff walls along every border between two cave halls, with noise gaps (the trail gaps are cut per chunk, <see cref="WallFits"/>).</summary>
        private static void AddHallWalls(OverworldLayout layout, List<(CavePiece, Vector2)> all)
        {
            const float e = 0.01f;
            for (int i = 0; i < layout.Zones.Count; i++)
            {
                OverworldZone zi = layout.Zones[i];
                if (!IsCave(zi.Terrain))
                {
                    continue;
                }

                for (int j = i + 1; j < layout.Zones.Count; j++)
                {
                    OverworldZone zj = layout.Zones[j];
                    if (!IsCave(zj.Terrain))
                    {
                        continue;
                    }

                    Rect a = zi.Bounds;
                    Rect b = zj.Bounds;
                    bool vertical;
                    float fixedCoord;
                    float from;
                    float to;
                    if (Mathf.Abs(a.xMax - b.xMin) < e || Mathf.Abs(b.xMax - a.xMin) < e)
                    {
                        vertical = true;
                        fixedCoord = Mathf.Abs(a.xMax - b.xMin) < e ? a.xMax : a.xMin;
                        from = Mathf.Max(a.yMin, b.yMin);
                        to = Mathf.Min(a.yMax, b.yMax);
                    }
                    else if (Mathf.Abs(a.yMax - b.yMin) < e || Mathf.Abs(b.yMax - a.yMin) < e)
                    {
                        vertical = false;
                        fixedCoord = Mathf.Abs(a.yMax - b.yMin) < e ? a.yMax : a.yMin;
                        from = Mathf.Max(a.xMin, b.xMin);
                        to = Mathf.Min(a.xMax, b.xMax);
                    }
                    else
                    {
                        continue;
                    }

                    if (to - from < 1f)
                    {
                        continue;
                    }

                    string salt = "hallwall_" + zi.Id + "_" + zj.Id;
                    float off = TownStyle.Hash01(salt, 1) * 100f;
                    TownSwatch rock = WallRock(zi.Terrain, zj.Terrain);
                    int k = 0;
                    for (float t = from; t < to - 2f; t += CaveWallPiece, k++)
                    {
                        float t1 = Mathf.Min(t + CaveWallPiece, to);
                        if (OverworldNoise.Perlin((t + t1) * 0.5f / 38f + off, off * 0.37f) > 0.62f)
                        {
                            continue;
                        }

                        float j0 = OverworldNoise.Signed(t / 55f + off, 3.7f) * 5f;
                        float j1 = OverworldNoise.Signed(t1 / 55f + off, 3.7f) * 5f;
                        Vector2 p0 = vertical ? new Vector2(fixedCoord + j0, t) : new Vector2(t, fixedCoord + j0);
                        Vector2 p1 = vertical ? new Vector2(fixedCoord + j1, t1) : new Vector2(t1, fixedCoord + j1);
                        float h01 = TownStyle.Hash01(salt, k + 10);
                        // Walls running north–south stand tall; east–west ones (across the camera's view) stay low ledges.
                        float height = vertical ? 8f + 5f * h01 : 3.2f + 1.6f * h01;
                        all.Add((new CavePiece
                        {
                            Kind = CavePieceKind.InnerWall,
                            Prop = new OverworldProp
                            {
                                Kind = CityPropKind.Rock, Decor = OverworldDecor.TuffWall, Position = p0, End = p1, Scale = height,
                                Variant = (int)(TownStyle.Hash(salt, k + 20) & 0xFFFF), Color = rock,
                            },
                        }, p0));
                    }
                }
            }
        }

        /// <summary>The vault round the region: tall walls just outside the north, west and east walls, a low ledge on the camera (south) side; open at the gates.</summary>
        private static void AddVaultWalls(OverworldLayout layout, List<(CavePiece, Vector2)> all)
        {
            if (!layout.IsStitched)
            {
                AddVault(layout, all, layout.Bounds, 45f, "vault");
                return;
            }

            // The stitched world: a vault round each cave region, open at the passes to its neighbours.
            foreach (OverworldPart part in layout.Parts)
            {
                if (layout.Zones.Exists(z => IsCave(z.Terrain) && part.Bounds.Contains(z.Bounds.center)))
                {
                    AddVault(layout, all, part.Bounds, 0f, "vault_" + part.Id);
                }
            }
        }

        private static void AddVault(OverworldLayout layout, List<(CavePiece, Vector2)> all, Rect r, float extra, string salt)
        {
            const float out1 = 3.5f;
            AddVaultRun(layout, all, new Vector2(r.xMin - extra, r.yMax + out1), new Vector2(r.xMax + extra, r.yMax + out1), 16f, 7f, salt + "_n");
            AddVaultRun(layout, all, new Vector2(r.xMin - out1, r.yMin - extra), new Vector2(r.xMin - out1, r.yMax + extra), 14f, 7f, salt + "_w");
            AddVaultRun(layout, all, new Vector2(r.xMax + out1, r.yMin - extra), new Vector2(r.xMax + out1, r.yMax + extra), 14f, 7f, salt + "_e");
            AddVaultRun(layout, all, new Vector2(r.xMin - extra, r.yMin - out1), new Vector2(r.xMax + extra, r.yMin - out1), 2.4f, 1.2f, salt + "_s");
        }

        private static void AddVaultRun(OverworldLayout layout, List<(CavePiece, Vector2)> all, Vector2 from, Vector2 to, float height, float spread, string salt)
        {
            float length = Vector2.Distance(from, to);
            Vector2 d = (to - from) / length;
            int k = 0;
            for (float t = 0f; t < length - 1f; t += CaveWallPiece, k++)
            {
                Vector2 p0 = from + d * t;
                Vector2 p1 = from + d * Mathf.Min(t + CaveWallPiece, length);
                Vector2 mid = (p0 + p1) * 0.5f;
                bool atGate = false;
                foreach (MapExitSpec exit in layout.World.Exits)
                {
                    if (Vector2.Distance(exit.Trigger.center, mid) < 14f)
                    {
                        atGate = true;
                        break;
                    }
                }

                foreach (Vector2 pass in layout.Passes)
                {
                    if (Vector2.Distance(pass, mid) < 16f)
                    {
                        atGate = true;
                        break;
                    }
                }

                if (atGate)
                {
                    continue;
                }

                OverworldZone near = OverworldRules.NearestZone(layout, mid);
                TownSwatch rock = near != null && IsCave(near.Terrain) ? WallRock(near.Terrain, near.Terrain) : TownSwatch.Basalt;
                all.Add((new CavePiece
                {
                    Kind = CavePieceKind.Forced,
                    Prop = new OverworldProp
                    {
                        Kind = CityPropKind.Rock, Decor = OverworldDecor.TuffWall, Position = p0, End = p1, Scale = height + spread * TownStyle.Hash01(salt, k),
                        Variant = (int)(TownStyle.Hash(salt, k + 5) & 0xFFFF), Color = rock,
                    },
                }, p0));
            }
        }

        private static TownSwatch WallRock(OverworldTerrain a, OverworldTerrain b)
        {
            bool tuff = a == OverworldTerrain.TuffTerraces || a == OverworldTerrain.WheelCanyon || b == OverworldTerrain.TuffTerraces || b == OverworldTerrain.WheelCanyon
                || a == OverworldTerrain.LakeShore || a == OverworldTerrain.CaveMouth;
            if (tuff)
            {
                return TownSwatch.PinkTuff;
            }

            return a == OverworldTerrain.CrystalField || a == OverworldTerrain.MushroomGrove ? TownSwatch.Indigo : TownSwatch.Basalt;
        }

        /// <summary>The black void over each chasm (pieces of ≤ 14 m) and the rope posts at both ends of its bridges.</summary>
        private static void AddAbysses(OverworldLayout layout, CaveData data, List<(CavePiece, Vector2)> all)
        {
            float y = OverworldRelief.WaterY + 0.04f;
            foreach (MapRiver river in layout.World.Rivers)
            {
                if (!data.Abysses.Contains(river))
                {
                    continue;
                }

                float half = river.Width >= OverworldRelief.CarveMinWidth ? river.Width * 0.5f + 0.9f : river.Width * 0.5f;
                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    Vector2 a = river.Points[i];
                    Vector2 c = river.Points[i + 1];
                    int pieces = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, c) / 14f));
                    for (int k = 0; k < pieces; k++)
                    {
                        Vector2 p0 = Vector2.Lerp(a, c, k / (float)pieces);
                        Vector2 p1 = Vector2.Lerp(a, c, (k + 1) / (float)pieces);
                        int ends = (k == 0 ? 1 : 0) | (k == pieces - 1 && i + 2 == river.Points.Count ? 2 : 0);
                        all.Add((new CavePiece
                        {
                            Kind = CavePieceKind.Detail,
                            Prop = new OverworldProp { Decor = OverworldDecor.Abyss, Position = p0, End = p1, Scale = half, Variant = ends, Yaw = y },
                        }, (p0 + p1) * 0.5f));
                    }
                }

                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (!crossing.IsBridge)
                    {
                        continue;
                    }

                    Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                    float span = BridgeSpan(river) * 0.5f;
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Vector2 end = crossing.At + across * (side * span);
                        Vector2 toBridge = -across * side;
                        all.Add((new CavePiece
                        {
                            Kind = CavePieceKind.Forced,
                            Prop = new OverworldProp
                            {
                                Kind = CityPropKind.Rock, Decor = OverworldDecor.RopePosts, Position = end, Scale = crossing.Width * 0.5f,
                                Yaw = Mathf.Atan2(toBridge.x, toBridge.y) * Mathf.Rad2Deg,
                            },
                        }, end));
                    }
                }
            }
        }

        /// <summary>Lamp niches beside the cave trails (the Lamparinas' lamps that keep the way), copper air pipes along the market's tunnels.</summary>
        private static void AddTrailDressing(OverworldLayout layout, List<(CavePiece, Vector2)> all)
        {
            int k = 0;
            foreach (MapPath path in layout.World.Paths)
            {
                k++;
                Vector2 d = path.B - path.A;
                float length = d.magnitude;
                if (length < 6f)
                {
                    continue;
                }

                d /= length;
                Vector2 mid = (path.A + path.B) * 0.5f;
                OverworldZone zone = OverworldRules.ZoneAt(layout, mid);
                if (zone == null || !IsCave(zone.Terrain))
                {
                    continue;
                }

                float roll = TownStyle.Hash01("cavetrail", k);
                float side = TownStyle.Hash01("cavetrail", k + 7000) < 0.5f ? -1f : 1f;
                var n = new Vector2(-d.y, d.x) * side;
                if (roll < 0.24f)
                {
                    Vector2 at = mid + n * (path.Width * 0.5f + 2.2f);
                    all.Add((new CavePiece
                    {
                        Kind = CavePieceKind.Placed,
                        Prop = new OverworldProp { Kind = CityPropKind.Rock, Decor = OverworldDecor.LampNiche, Position = at, Yaw = Mathf.Atan2(n.x, n.y) * Mathf.Rad2Deg, Scale = 1f },
                    }, at));
                }
                else if (zone.Terrain == OverworldTerrain.TunnelMarket && roll < 0.55f)
                {
                    Vector2 at = mid + n * (path.Width * 0.5f + 2.4f);
                    all.Add((new CavePiece
                    {
                        Kind = CavePieceKind.Placed,
                        Prop = new OverworldProp { Kind = CityPropKind.Rock, Decor = OverworldDecor.AirPipe, Position = at, Yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg, Scale = 1f },
                    }, at));
                }
            }
        }

        /// <summary>A hall-wall piece stands only clear of trails (a wide gap), rivers, towns, gates, clearings and landmarks.</summary>
        private static bool WallFits(OverworldLayout layout, OverworldChunkContent content, OverworldProp wall, int blocked)
        {
            Vector2 a = wall.Position;
            Vector2 c = wall.End;
            for (int s = 0; s <= 2; s++)
            {
                Vector2 q = Vector2.Lerp(a, c, s * 0.5f);
                if (!CityGeometry.Inflate(layout.Bounds, -MapLayoutRules.WallMargin).Contains(q))
                {
                    return false;
                }

                for (int i = 0; i < blocked; i++)
                {
                    if (CityGeometry.Inflate(content.Blocked[i], 4f).Contains(q))
                    {
                        return false;
                    }
                }

                foreach (MapPath path in content.NearPaths)
                {
                    if (MapLayoutRules.DistanceToSegment(q, path.A, path.B) < path.Width * 0.5f + 7f)
                    {
                        return false;
                    }
                }

                foreach (MapRiver river in content.NearRivers)
                {
                    if (MapLayoutRules.DistanceToPolyline(q, river.Points) < river.Width * 0.5f + 4f)
                    {
                        return false;
                    }
                }

                foreach (MapSpawnZone spawn in content.NearSpawns)
                {
                    if (Vector2.Distance(q, spawn.Center) < spawn.Radius + 5f)
                    {
                        return false;
                    }
                }

                foreach (MapLandmark landmark in content.NearLandmarks)
                {
                    if (Vector2.Distance(q, landmark.Position) < 12f)
                    {
                        return false;
                    }
                }

                foreach (Vector2 keep in content.KeepClearPoints)
                {
                    if ((keep - q).sqrMagnitude < 100f)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static Rect WallRect(OverworldProp wall)
        {
            float half = OverworldDecorKit.CaveWallThickness(wall.Variant) * 0.5f + 0.4f;
            return Rect.MinMaxRect(Mathf.Min(wall.Position.x, wall.End.x) - half, Mathf.Min(wall.Position.y, wall.End.y) - half,
                Mathf.Max(wall.Position.x, wall.End.x) + half, Mathf.Max(wall.Position.y, wall.End.y) + half);
        }

        // ------------------------------------------------------------------
        // Colliders and meshes
        // ------------------------------------------------------------------

        private static bool AddCaveDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.TuffWall:
                {
                    Vector2 c = prop.End;
                    Vector2 mid = (p + c) * 0.5f;
                    float ga = content.Grid.Sample(p);
                    float gc = content.Grid.Sample(c);
                    float gm = content.Grid.Sample(mid);
                    float low = Mathf.Min(ga, Mathf.Min(gc, gm));
                    float high = Mathf.Max(ga, Mathf.Max(gc, gm));
                    Vector2 d = c - p;
                    content.Solids.Add(new OverworldSolid
                    {
                        Center = mid, Base = low - sink - 0.4f, IsBox = true, Size = new Vector2(OverworldDecorKit.CaveWallThickness(prop.Variant), d.magnitude),
                        Height = s + (high - low) + sink + 0.4f, Yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg,
                    });
                    return true;
                }

                case OverworldDecor.Abyss:
                    return true;
                case OverworldDecor.Basalt:
                {
                    // The chasms' basalt columns on steep gorge sides: the collider (off-centre) starts under the lower ground.
                    if (!IsCave(content.Grid.Near(p).Terrain))
                    {
                        return false;
                    }

                    Vector2 center = p + Rotate(new Vector2(0.2f, 0.3f) * s, prop.Yaw);
                    float baseY = Mathf.Min(ground, content.Grid.Sample(center) - sink);
                    content.Solids.Add(new OverworldSolid { Center = center, Base = baseY, Radius = 0.95f * s, Height = 2.2f * s + sink + (ground - baseY) });
                    return true;
                }

                case OverworldDecor.Stalagmite:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 2.6f * s + sink });
                    return true;
                case OverworldDecor.Crystal:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.75f * s, Height = 1.8f * s + sink });
                    return true;
                case OverworldDecor.CaveColumn:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.1f * s, Height = 8f * s + sink });
                    return true;
                case OverworldDecor.GiantMushroom:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.45f * s, Height = 3.6f * s + sink });
                    return true;
                case OverworldDecor.Vishapakar:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.55f * s, Height = 3f * s + sink });
                    return true;
                case OverworldDecor.MineCart:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.3f, 2f), Height = 1.4f + sink, Yaw = prop.Yaw });
                    return true;
                case OverworldDecor.AirPipe:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(0.6f, 5.4f), Height = 2.2f * s + sink, Yaw = prop.Yaw });
                    return true;
                case OverworldDecor.Jar:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.6f * s, Height = 1.5f * s + sink });
                    return true;
                case OverworldDecor.Nest:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.5f * s, Height = 0.8f * s + sink });
                    return true;
                case OverworldDecor.RopePosts:
                    foreach (float x in new[] { -s - 0.25f, s + 0.25f })
                    {
                        Vector2 at = p + Rotate(new Vector2(x, 0f), prop.Yaw);
                        content.Solids.Add(new OverworldSolid { Center = at, Base = content.Grid.Sample(at) - sink, Radius = 0.15f, Height = 1.6f + sink });
                    }

                    return true;
                case OverworldDecor.LampNiche:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.42f * s, Height = 1.9f * s + sink });
                    return true;
                case OverworldDecor.JarWheel:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(4.2f, 11f), Height = 11.5f + sink, Yaw = prop.Yaw });
                    return true;
                case OverworldDecor.OssuaryPortal:
                    content.Solids.Add(new OverworldSolid
                    {
                        Center = p + Rotate(new Vector2(0f, 0.6f), prop.Yaw), Base = ground, IsBox = true, Size = new Vector2(8f, 2f), Height = 6.6f + sink, Yaw = prop.Yaw,
                    });
                    return true;
                default:
                    return false;
            }
        }

        private static bool AppendCaveDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.TuffWall:
                {
                    Vector2 mid = (prop.Position + prop.End) * 0.5f;
                    float low = Mathf.Min(at.y, Mathf.Min(g.Sample(prop.End), g.Sample(mid)));
                    OverworldDecorKit.CaveTuffWall(b, new Vector3(at.x, low, at.z), new Vector3(prop.End.x, low, prop.End.y), s, prop.Variant, prop.Color);
                    return true;
                }

                case OverworldDecor.Abyss:
                    OverworldDecorKit.CaveAbyss(b, prop.Position, prop.End, s, prop.Yaw);
                    if ((prop.Variant & 1) != 0)
                    {
                        b.Disc(new Vector3(prop.Position.x, prop.Yaw, prop.Position.y), s, 10, TownSwatch.Black);
                    }

                    if ((prop.Variant & 2) != 0)
                    {
                        b.Disc(new Vector3(prop.End.x, prop.Yaw, prop.End.y), s, 10, TownSwatch.Black);
                    }

                    return true;
            }

            if ((int)prop.Decor < (int)OverworldDecor.TuffWall || (int)prop.Decor >= 80)
            {
                return false;
            }

            TownSwatch rock = CaveRockColor(g.Near(prop.Position).Terrain);
            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.Stalagmite: OverworldDecorKit.CaveStalagmite(b, s, prop.Variant, rock); break;
                case OverworldDecor.Crystal: OverworldDecorKit.CaveCrystal(b, s, prop.Variant); break;
                case OverworldDecor.CaveColumn: OverworldDecorKit.CaveColumn(b, s, rock); break;
                case OverworldDecor.GiantMushroom: OverworldDecorKit.CaveMushrooms(b, s, prop.Variant); break;
                case OverworldDecor.Vishapakar: OverworldDecorKit.CaveVishapakar(b, s, prop.Variant); break;
                case OverworldDecor.MineCart: OverworldDecorKit.CaveMineCart(b, s); break;
                case OverworldDecor.AirPipe: OverworldDecorKit.CaveAirPipe(b, s); break;
                case OverworldDecor.Jar: OverworldDecorKit.CaveJar(b, s, prop.Variant); break;
                case OverworldDecor.Nest: OverworldDecorKit.CaveNest(b, s); break;
                case OverworldDecor.RopePosts: OverworldDecorKit.CaveRopePosts(b, s); break;
                case OverworldDecor.LampNiche: OverworldDecorKit.CaveLampNiche(b, s); break;
                case OverworldDecor.JarWheel: OverworldDecorKit.CaveJarWheel(b); break;
                case OverworldDecor.OssuaryPortal: OverworldDecorKit.CaveOssuaryPortal(b); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
