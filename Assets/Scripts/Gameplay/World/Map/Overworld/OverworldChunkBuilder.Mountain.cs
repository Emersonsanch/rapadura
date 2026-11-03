using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 3 — Serra de Kaggen: the mountain pieces of the chunk builder. Before the noise scatter, each chunk gets the
    /// big pieces of its areas (rondavels and kraals in the pastures, painted shelters, the rock walls of the passes, conical
    /// towers, bronze pillars, giant granite blocks, stelae: placed whole inside the chunk, their footprint kept clear of
    /// the rest), the dry-stone wall runs (curved in the Muralhas), the terrace rows on the flat steps of the Terraços and the
    /// ground snow and mist of the high areas; then the shared scatter fills around them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Serra de Kaggen terrain (Região 3, 60–79).</summary>
        public static bool IsMountain(OverworldTerrain t)
        {
            int i = (int)t;
            return i >= 60 && i < 80;
        }

        /// <summary>The flora the embedded maps' trees and shrubs grow in: the one of the area they stand in.</summary>
        private static TownVegetation FixedVegetation(OverworldLayout layout, Vector2 p)
        {
            OverworldZone zone = OverworldRules.NearestZone(layout, p);
            OverworldTerrainStyle style = zone != null ? OverworldTerrains.For(zone.Terrain) : null;
            return style != null ? style.Vegetation : TownVegetation.Forest;
        }

        /// <summary>A big piece of an area and its chance per attempt.</summary>
        private struct BigPiece
        {
            public OverworldDecor Decor;
            public float Chance;
            public float Radius;
            public bool NeedsFlat;

            public BigPiece(OverworldDecor decor, float chance, float radius, bool flat)
            {
                Decor = decor;
                Chance = chance;
                Radius = radius;
                NeedsFlat = flat;
            }
        }

        private static readonly BigPiece[] NoPieces = new BigPiece[0];

        private static BigPiece[] BigPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.GoldenSlopes:
                    return new[] { new BigPiece(OverworldDecor.Rondavel, 0.14f, 3f, true), new BigPiece(OverworldDecor.Kraal, 0.08f, 4.8f, true) };
                case OverworldTerrain.FarmTerraces:
                    return new[] { new BigPiece(OverworldDecor.Rondavel, 0.1f, 3f, true) };
                case OverworldTerrain.AloeSlopes:
                    return new[] { new BigPiece(OverworldDecor.RockSpire, 0.08f, 2f, false) };
                case OverworldTerrain.BaobabGrass:
                    return new[] { new BigPiece(OverworldDecor.Kraal, 0.08f, 4.8f, true), new BigPiece(OverworldDecor.Rondavel, 0.08f, 3f, true) };
                case OverworldTerrain.HighPass:
                    return new[] { new BigPiece(OverworldDecor.RockSpire, 0.25f, 2f, false) };
                case OverworldTerrain.Torrents:
                    return new[] { new BigPiece(OverworldDecor.RockSpire, 0.06f, 2f, false) };
                case OverworldTerrain.Kraals:
                    return new[] { new BigPiece(OverworldDecor.Kraal, 0.4f, 4.8f, true), new BigPiece(OverworldDecor.Rondavel, 0.25f, 3f, true) };
                case OverworldTerrain.NarrowPass:
                    return new[] { new BigPiece(OverworldDecor.RockSpire, 0.85f, 2f, false) };
                case OverworldTerrain.SandstoneScarp:
                    return new[] { new BigPiece(OverworldDecor.RockSpire, 0.35f, 2f, false), new BigPiece(OverworldDecor.PaintedShelter, 0.06f, 3.6f, true) };
                case OverworldTerrain.PaintedShelters:
                    return new[] { new BigPiece(OverworldDecor.PaintedShelter, 0.4f, 3.6f, true), new BigPiece(OverworldDecor.RockSpire, 0.25f, 2f, false) };
                case OverworldTerrain.DryStone:
                    return new[] { new BigPiece(OverworldDecor.ConicalTower, 0.14f, 2.4f, true), new BigPiece(OverworldDecor.Kraal, 0.2f, 4.8f, true) };
                case OverworldTerrain.BronzeHalls:
                    return new[] { new BigPiece(OverworldDecor.BronzePillar, 0.45f, 1.1f, false), new BigPiece(OverworldDecor.GraniteBlock, 0.2f, 2.4f, false) };
                case OverworldTerrain.Snowfield:
                    return new[] { new BigPiece(OverworldDecor.GraniteBlock, 0.22f, 2.4f, false), new BigPiece(OverworldDecor.RockSpire, 0.12f, 2f, false) };
                case OverworldTerrain.Summit:
                    return new[] { new BigPiece(OverworldDecor.GraniteBlock, 0.45f, 2.4f, false), new BigPiece(OverworldDecor.Stela, 0.06f, 1.4f, true) };
                case OverworldTerrain.StelaPlateau:
                    return new[] { new BigPiece(OverworldDecor.Stela, 0.12f, 1.4f, true), new BigPiece(OverworldDecor.Rondavel, 0.12f, 3f, true) };
                default:
                    return NoPieces;
            }
        }

        /// <summary>Snow drifts and mist banks per detail attempt (a few dozen attempts a chunk).</summary>
        private static void SnowAndMist(OverworldTerrain terrain, out float snow, out float mist)
        {
            switch (terrain)
            {
                case OverworldTerrain.HighPass: snow = 0.25f; mist = 0.02f; break;
                case OverworldTerrain.NarrowPass: snow = 0.1f; mist = 0.05f; break;
                case OverworldTerrain.Snowfield: snow = 0.45f; mist = 0.1f; break;
                case OverworldTerrain.Summit: snow = 0.5f; mist = 0.18f; break;
                case OverworldTerrain.StelaPlateau: snow = 0.3f; mist = 0.06f; break;
                default: snow = 0f; mist = 0f; break;
            }
        }

        private static void ScatterMountain(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsMountain(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "owm" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk (no neighbour ever overlaps them), their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsMountain(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = BigPiecesOf(terrain);
                float roll = TownStyle.Hash01(salt, k + 200);
                float sum = 0f;
                foreach (BigPiece piece in pieces)
                {
                    sum += piece.Chance;
                    if (roll >= sum)
                    {
                        continue;
                    }

                    float s01 = TownStyle.Hash01(salt, k + 300);
                    float scale = piece.Decor == OverworldDecor.RockSpire ? 0.8f + 0.6f * s01 : 0.9f + 0.25f * s01;
                    float radius = piece.Radius * scale;
                    if ((!piece.NeedsFlat || content.Grid.Flatness(p) > 0.95f) && IsFree(layout, content, p, radius + 1f))
                    {
                        var prop = new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Decor = piece.Decor, Position = p, Yaw = TownStyle.Hash01(salt, k + 400) * 360f, Scale = scale,
                            Variant = (int)(TownStyle.Hash(salt, k + 500) & 7), Color = RockColor(terrain),
                        };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                        content.Blocked.Add(new Rect(p.x - radius, p.y - radius, radius * 2f, radius * 2f));
                    }

                    break;
                }
            }

            ScatterMountainWalls(layout, content, salt);
            ScatterTerraceRows(layout, content);
            ScatterSnowAndMist(layout, content, salt);
        }

        /// <summary>Dry-stone wall runs: curved enclosures in the Muralhas, straight field walls in the pastures and terraces.</summary>
        private static void ScatterMountainWalls(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 3; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 4f, r.xMax - 4f, TownStyle.Hash01(salt, k + 1000)), Mathf.Lerp(r.yMin + 4f, r.yMax - 4f, TownStyle.Hash01(salt, k + 1100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                float chance;
                float bend;
                switch (terrain)
                {
                    case OverworldTerrain.DryStone: chance = 0.85f; bend = 9f; break;
                    case OverworldTerrain.Kraals: chance = 0.35f; bend = 0f; break;
                    case OverworldTerrain.GoldenSlopes: chance = 0.12f; bend = 0f; break;
                    case OverworldTerrain.StelaPlateau: chance = 0.15f; bend = 4f; break;
                    default: chance = 0f; bend = 0f; break;
                }

                if (TownStyle.Hash01(salt, k + 1200) >= chance || layout.InAuthoredArea(p))
                {
                    continue;
                }

                float yaw = TownStyle.Hash01(salt, k + 1300) * 360f;
                int pieces = 3 + (int)(TownStyle.Hash(salt, k + 1400) % 6u);
                float height = terrain == OverworldTerrain.DryStone ? 1.5f + TownStyle.Hash01(salt, k + 1500) * 0.8f : 0.9f;
                float turn = (TownStyle.Hash01(salt, k + 1600) < 0.5f ? -1f : 1f) * bend;
                Vector2 a = p;
                for (int i = 0; i < pieces; i++, yaw += turn)
                {
                    Vector2 dir = new Vector2(Mathf.Cos(yaw * Mathf.Deg2Rad), Mathf.Sin(yaw * Mathf.Deg2Rad));
                    Vector2 c = a + dir * RowPiece;
                    Vector2 mid = (a + c) * 0.5f;
                    if (!r.Contains(a) || !r.Contains(c))
                    {
                        break;
                    }

                    if (TownStyle.Hash01(salt, k * 31 + i + 1700) > 0.12f && IsFree(layout, content, a, 1f) && IsFree(layout, content, c, 1f)
                        && IsFree(layout, content, mid, 1f))
                    {
                        var prop = new OverworldProp
                        {
                            Kind = CityPropKind.FenceRun, Decor = OverworldDecor.DryStoneWall, Position = a, End = c, Scale = height, Variant = k * 31 + i,
                        };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                    }

                    a = c;
                }
            }
        }

        /// <summary>The Terraços: on the flat steps, east–west rows of low retaining wall with sorghum on top (world lattice).</summary>
        private static void ScatterTerraceRows(OverworldLayout layout, OverworldChunkContent content)
        {
            const float rowSpacing = 6f;
            Rect r = content.Rect;
            int z0 = Mathf.CeilToInt(r.yMin / rowSpacing);
            int z1 = Mathf.FloorToInt((r.yMax - 0.001f) / rowSpacing);
            for (int iz = z0; iz <= z1; iz++)
            {
                float z = iz * rowSpacing;
                int x0 = Mathf.CeilToInt(r.xMin / RowPiece);
                int x1 = Mathf.FloorToInt((r.xMax - RowPiece) / RowPiece);
                for (int ix = x0; ix <= x1; ix++)
                {
                    var a = new Vector2(ix * RowPiece, z);
                    var c = new Vector2(ix * RowPiece + RowPiece - 0.3f, z);
                    Vector2 mid = (a + c) * 0.5f;
                    int seed = ix * 92821 + iz * 7919;
                    if (content.Grid.Near(mid).Terrain != OverworldTerrain.FarmTerraces || TownStyle.Hash01("terrace", seed) < 0.22f
                        || layout.InAuthoredArea(mid) || content.Grid.Flatness(mid) < 0.985f)
                    {
                        continue;
                    }

                    if (!IsFree(layout, content, a, 0.6f) || !IsFree(layout, content, c, 0.6f) || !IsFree(layout, content, mid, 0.6f))
                    {
                        continue;
                    }

                    var prop = new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.TerraceRow, Position = a, End = c, Scale = 1f, Variant = seed };
                    content.Props.Add(prop);
                    AddSolid(layout, content, prop);
                }
            }
        }

        /// <summary>Snow drifts and low mist on the high ground (ground detail: no colliders).</summary>
        private static void ScatterSnowAndMist(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 36; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 2000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 2100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                SnowAndMist(terrain, out float snow, out float mist);
                float roll = TownStyle.Hash01(salt, k + 2200);
                OverworldDecor decor;
                if (roll < mist)
                {
                    decor = OverworldDecor.MistBank;
                }
                else if (roll < mist + snow)
                {
                    decor = OverworldDecor.SnowDrift;
                }
                else
                {
                    continue;
                }

                if (DistanceToWall(layout, p) < -10f || !IsOpen(layout, content, p, decor == OverworldDecor.MistBank ? 2.5f : 1.2f, false))
                {
                    continue;
                }

                content.Details.Add(new OverworldProp
                {
                    Decor = decor, Position = p, Yaw = TownStyle.Hash01(salt, k + 2300) * 360f, Scale = 0.8f + 0.6f * TownStyle.Hash01(salt, k + 2400), Variant = k,
                });
            }
        }

        // ------------------------------------------------------------------
        // Colliders and meshes
        // ------------------------------------------------------------------

        private static bool AddMountainDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.Rondavel:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2.3f * s, Height = 3.2f * s + sink });
                    return true;
                case OverworldDecor.Kraal:
                    // The ring of wall with its gap (the first segment).
                    for (int i = 1; i < OverworldDecorKit.KraalSegments; i++)
                    {
                        float a = (i + 0.5f) * Mathf.PI * 2f / OverworldDecorKit.KraalSegments;
                        Vector2 at = p + Rotate(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * OverworldDecorKit.KraalRadius * s, prop.Yaw);
                        content.Solids.Add(new OverworldSolid { Center = at, Base = content.Grid.Sample(at) - sink, Radius = 0.75f * s, Height = 1f + sink });
                    }

                    return true;
                case OverworldDecor.DryStoneWall:
                    AddRunSolid(content, prop, 0.8f, Mathf.Max(1f, prop.Scale), sink);
                    return true;
                case OverworldDecor.TerraceRow:
                    AddRunSolid(content, prop, 0.7f, 0.8f, sink);
                    return true;
                case OverworldDecor.PaintedShelter:
                    Vector2 back = p + Rotate(new Vector2(0f, 0.8f) * s, prop.Yaw);
                    float backGround = Mathf.Min(ground, content.Grid.Sample(back) - sink);
                    content.Solids.Add(new OverworldSolid
                    {
                        Center = back, Base = backGround, IsBox = true, Size = new Vector2(6.2f * s, 1.6f * s), Height = 4f * s + sink + (ground - backGround),
                        Yaw = prop.Yaw,
                    });
                    return true;
                case OverworldDecor.RockSpire:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.25f * s, Height = 6.5f * s + sink });
                    return true;
                case OverworldDecor.ConicalTower:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.9f * s, Height = 5.8f * s + sink });
                    return true;
                case OverworldDecor.BronzePillar:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.65f * s, Height = 4.2f * s + sink });
                    return true;
                case OverworldDecor.GraniteBlock:
                    content.Solids.Add(new OverworldSolid
                    {
                        Center = p, Base = ground, IsBox = true, Size = new Vector2(3.2f * s, 2.6f * s), Height = 2.4f * s + sink, Yaw = prop.Yaw,
                    });
                    return true;
                case OverworldDecor.Stela:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.6f * s, 1.1f * s), Height = 6.5f * s + sink, Yaw = prop.Yaw });
                    return true;
                case OverworldDecor.SnowDrift:
                case OverworldDecor.MistBank:
                    return true;
                default:
                    return false;
            }
        }

        private static bool AppendMountainDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            switch (prop.Decor)
            {
                case OverworldDecor.DryStoneWall:
                {
                    float low = Mathf.Min(at.y, g.Sample(prop.End)) - 0.15f;
                    TownKit.WallRun(b, new Vector3(at.x, low, at.z), new Vector3(prop.End.x, low, prop.End.y), TownStyles.For(BiomeType.Mountain), prop.Scale);
                    return true;
                }

                case OverworldDecor.TerraceRow:
                    OverworldDecorKit.TerraceRow(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.Rondavel:
                case OverworldDecor.Kraal:
                case OverworldDecor.PaintedShelter:
                case OverworldDecor.RockSpire:
                case OverworldDecor.ConicalTower:
                case OverworldDecor.BronzePillar:
                case OverworldDecor.SnowDrift:
                case OverworldDecor.MistBank:
                case OverworldDecor.GraniteBlock:
                case OverworldDecor.Stela:
                    break;
                default:
                    return false;
            }

            float s = prop.Scale;
            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.Rondavel: OverworldDecorKit.Rondavel(b, s, prop.Variant); break;
                case OverworldDecor.Kraal: OverworldDecorKit.Kraal(b, s, prop.Variant); break;
                case OverworldDecor.PaintedShelter: OverworldDecorKit.PaintedShelter(b, s, prop.Variant); break;
                case OverworldDecor.RockSpire: OverworldDecorKit.RockSpire(b, s, prop.Variant, prop.Color); break;
                case OverworldDecor.ConicalTower: OverworldDecorKit.ConicalTower(b, s); break;
                case OverworldDecor.BronzePillar: OverworldDecorKit.BronzePillar(b, s, prop.Variant); break;
                case OverworldDecor.SnowDrift: OverworldDecorKit.SnowDrift(b, s); break;
                case OverworldDecor.MistBank: OverworldDecorKit.MistBank(b, s); break;
                case OverworldDecor.GraniteBlock: OverworldDecorKit.GraniteBlock(b, s, prop.Variant); break;
                case OverworldDecor.Stela: OverworldDecorKit.Stela(b, s); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
