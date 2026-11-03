using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 2 — Areias Flamejantes: the desert pieces of the chunk builder: the region's placed pieces (the big Meroë
    /// pyramids, the karez shaft lines, the caravans on the roads, Namrael's towers and throne, the clan camps:
    /// <see cref="OverworldLayouts.DesertPiecesOf"/>) streamed with their chunk, the salt crust of the salt flats, and the
    /// meshes and colliders of every desert piece (<see cref="OverworldDecorKit"/> 80–99).
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>Salt crust: one sample every this many metres on the salt flats and Namrael's terraces.</summary>
        private const float SaltSpacing = 2.6f;

        private static readonly ConditionalWeakTable<OverworldLayout, Dictionary<Vector2Int, List<OverworldProp>>> DesertBuckets =
            new ConditionalWeakTable<OverworldLayout, Dictionary<Vector2Int, List<OverworldProp>>>();

        private static bool IsDesert(OverworldTerrain t) => OverworldTerrains.IsDesert(t);

        private static bool IsDesertDecor(OverworldDecor decor) => OverworldDecorKit.IsDesertDecor(decor);

        /// <summary>Room (m) a scattered desert piece keeps from the others (scale up to 1.4).</summary>
        private static float DesertClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.CaravanWreck: return 2.6f;
                case OverworldDecor.Pyramid: return 3.5f;
                case OverworldDecor.Ushabtis: return 2.2f;
                case OverworldDecor.Hoodoo: return 1.8f;
                case OverworldDecor.RockArch: return 4f;
                case OverworldDecor.TaipaTower: return 2.6f;
                case OverworldDecor.CaravanCamp: return 4.5f;
                case OverworldDecor.ClanTent: return 2.8f;
                case OverworldDecor.Marker: return 1f;
                default: return 1.6f;
            }
        }

        /// <summary>Half the side (m, at scale 1) of the ground a desert piece covers (nothing else grows there).</summary>
        public static float DesertFootprint(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.Pyramid: return 2.1f;
                case OverworldDecor.TaipaTower: return 1.7f;
                case OverworldDecor.CaravanCamp: return 3.2f;
                case OverworldDecor.RockArch: return 3.4f;
                case OverworldDecor.CaravanWreck: return 2f;
                case OverworldDecor.ClanTent: return 2.6f;
                case OverworldDecor.Hoodoo: return 1.2f;
                case OverworldDecor.SaltCrystal: return 1.1f;
                default: return 0.9f;
            }
        }

        private static void ScatterDesert(OverworldLayout layout, OverworldChunkContent content)
        {
            if (layout.Region != MapRegion.AreiasFlamejantes)
            {
                return;
            }

            // The placed pieces of this chunk first (they win their spot), then the ground they cover is kept clear of the
            // scatter, theirs and the neighbouring chunks' (a pyramid near a chunk border).
            Dictionary<Vector2Int, List<OverworldProp>> buckets = Buckets(layout);
            if (buckets.TryGetValue(content.Coord, out List<OverworldProp> own))
            {
                foreach (OverworldProp piece in own)
                {
                    TryAdd(layout, content, piece.Position, 0.5f, piece);
                }
            }

            Rect near = CityGeometry.Inflate(content.Rect, 16f);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (!buckets.TryGetValue(content.Coord + new Vector2Int(dx, dz), out List<OverworldProp> pieces))
                    {
                        continue;
                    }

                    foreach (OverworldProp piece in pieces)
                    {
                        float half = DesertFootprint(piece.Decor) * piece.Scale;
                        var foot = new Rect(piece.Position.x - half, piece.Position.y - half, half * 2f, half * 2f);
                        if (half > 1.5f && foot.Overlaps(near))
                        {
                            content.Blocked.Add(foot);
                        }
                    }
                }
            }

            ScatterSaltCrust(layout, content);
        }

        private static Dictionary<Vector2Int, List<OverworldProp>> Buckets(OverworldLayout layout)
        {
            if (DesertBuckets.TryGetValue(layout, out Dictionary<Vector2Int, List<OverworldProp>> buckets))
            {
                return buckets;
            }

            buckets = new Dictionary<Vector2Int, List<OverworldProp>>();
            foreach (OverworldProp piece in OverworldLayouts.DesertPiecesOf(layout))
            {
                Vector2Int c = OverworldRules.ChunkOf(layout, piece.Position);
                if (!buckets.TryGetValue(c, out List<OverworldProp> list))
                {
                    list = new List<OverworldProp>();
                    buckets[c] = list;
                }

                list.Add(piece);
            }

            DesertBuckets.AddOrUpdate(layout, buckets);
            return buckets;
        }

        /// <summary>Plates of salt crust on the salt flats and on Namrael's terraces (ground detail, no colliders).</summary>
        private static void ScatterSaltCrust(OverworldLayout layout, OverworldChunkContent content)
        {
            string salt = "owsalt" + content.Coord.x + "_" + content.Coord.y;
            int steps = Mathf.CeilToInt(layout.ChunkSize / SaltSpacing);
            int i = 0;
            for (int gx = 0; gx < steps; gx++)
            {
                for (int gz = 0; gz < steps; gz++, i++)
                {
                    var p = new Vector2(
                        content.Rect.xMin + (gx + 0.5f + (TownStyle.Hash01(salt, i) - 0.5f) * 0.8f) * SaltSpacing,
                        content.Rect.yMin + (gz + 0.5f + (TownStyle.Hash01(salt, i + 20000) - 0.5f) * 0.8f) * SaltSpacing);
                    if (!content.Rect.Contains(p) || DistanceToWall(layout.Bounds, p) < -14f)
                    {
                        continue;
                    }

                    ref ReliefInfo info = ref content.Grid.Near(p);
                    float density = info.Terrain == OverworldTerrain.SaltFlat ? 0.16f : info.Terrain == OverworldTerrain.SaltThrone ? 0.07f : 0f;
                    if (density <= 0f || info.Clearing < 0.5f || TownStyle.Hash01(salt, i + 40000) >= density)
                    {
                        continue;
                    }

                    if (IsOpen(layout, content, p, 0.9f, false))
                    {
                        content.Details.Add(new OverworldProp
                        {
                            Decor = OverworldDecor.SaltCrust, Position = p, Yaw = TownStyle.Hash01(salt, i + 60000) * 360f,
                            Scale = 0.8f + 0.8f * TownStyle.Hash01(salt, i + 80000), Variant = i,
                        });
                    }
                }
            }
        }

        private static bool AddDesertDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsDesertDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.Bones:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 0.9f * s + sink });
                    break;
                case OverworldDecor.CaravanWreck:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.6f * s, 1.6f * s), Height = 1.3f * s + sink, Yaw = prop.Yaw + 8f });
                    break;
                case OverworldDecor.Pyramid:
                {
                    Vector2 c = p + Rotate(new Vector2(0f, -0.2f * s), prop.Yaw);
                    content.Solids.Add(new OverworldSolid { Center = c, Base = content.Grid.Sample(c) - sink, IsBox = true, Size = new Vector2(2.9f * s, 3.6f * s), Height = 4f * s + sink, Yaw = prop.Yaw });
                    break;
                }

                case OverworldDecor.LotusColumn:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.45f * s, Height = 3.2f * s + sink });
                    break;
                case OverworldDecor.Ushabtis:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(3f * s, 0.7f * s), Height = 1.6f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.KarezShaft:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 1.4f + sink });
                    break;
                case OverworldDecor.Marker:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(0.9f * s, 0.7f * s), Height = 3f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.Hoodoo:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 4.5f * s + sink });
                    break;
                case OverworldDecor.RockArch:
                    foreach (float side in new[] { -2.3f, 2.3f })
                    {
                        Vector2 leg = p + Rotate(new Vector2(side * s, 0f), prop.Yaw);
                        content.Solids.Add(new OverworldSolid { Center = leg, Base = content.Grid.Sample(leg) - sink, Radius = 1f * s, Height = 3.6f * s + sink });
                    }

                    break;
                case OverworldDecor.SaltCrystal:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 2.3f * s + sink });
                    break;
                case OverworldDecor.TaipaTower:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.8f * s, 2.8f * s), Height = 5.5f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.CaravanCamp:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(5f * s, 4f * s), Height = 2f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.ClanTent:
                {
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.8f * s, Height = 2.6f * s + sink });
                    Vector2 drum = p + Rotate(new Vector2(2.3f * s, -0.6f * s), prop.Yaw);
                    content.Solids.Add(new OverworldSolid { Center = drum, Base = content.Grid.Sample(drum) - sink, Radius = 0.5f * s, Height = 0.8f * s + sink });
                    break;
                }
            }

            return true;
        }

        private static bool AppendDesertDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsDesertDecor(prop.Decor))
            {
                return false;
            }

            // Big pieces sit at the lowest corner of their footprint (no floating plinth on a slope).
            float half = DesertFootprint(prop.Decor) * prop.Scale;
            if (half > 1.5f)
            {
                Vector2 p = prop.Position;
                at.y = Mathf.Min(Mathf.Min(at.y, g.Sample(p + new Vector2(half, half))), Mathf.Min(g.Sample(p + new Vector2(-half, half)),
                    Mathf.Min(g.Sample(p + new Vector2(half, -half)), g.Sample(p + new Vector2(-half, -half)))));
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            OverworldDecorKit.DesertPiece(b, prop.Decor, prop.Scale, prop.Variant);
            b.Matrix = saved;
            return true;
        }
    }
}
