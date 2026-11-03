using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 9 — Estepe dos Nartes: the steppe's pieces of the chunk builder. Before the noise scatter, each chunk gets the big
    /// pieces of its areas (felt tents and horse standards round the camp, stone babas and golden stags on the kurgans, birches,
    /// larches, dream tablets, giant pots, svan towers, stone wells, willows by the shallow rivers), the runs (Amirani's chain
    /// along its line up the Caucasus, the giants' earthen ramparts following the contour of their hills), and the ground
    /// detail of its own (feather-grass plumes, poppies, salt crust in the wormwood, snow drifts and mist banks on the high
    /// ground); then the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>Metres between two runs of Amirani's chain.</summary>
        private const float ChainStep = 5f;

        /// <summary>An Estepe dos Nartes terrain (Região 9, 160–179).</summary>
        public static bool IsNartes(OverworldTerrain t) => OverworldTerrains.IsNartes(t);

        private static bool IsNartesDecor(OverworldDecor decor) => OverworldDecorKit.IsNartesDecor(decor);

        /// <summary>Room (m) a scattered Nartes piece keeps from the others.</summary>
        private static float NartesClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.FeltYurt: return 3f;
                case OverworldDecor.HorseStandard: return 0.8f;
                case OverworldDecor.StoneBaba: return 0.9f;
                case OverworldDecor.GoldStag: return 1.6f;
                case OverworldDecor.Birch: return 1.2f;
                case OverworldDecor.Larch: return 2.2f;
                case OverworldDecor.DreamTablet: return 1.4f;
                case OverworldDecor.GiantCauldron: return 2.8f;
                case OverworldDecor.SvanTower: return 3f;
                case OverworldDecor.StoneWell: return 1.6f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] NartesPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.FeatherSteppe:
                    return new[] { new BigPiece(OverworldDecor.StoneBaba, 0.12f, 1f, false), new BigPiece(OverworldDecor.HorseStandard, 0.08f, 1f, false), new BigPiece(OverworldDecor.FeltYurt, 0.04f, 3.2f, true) };
                case OverworldTerrain.SteppeRiver:
                    return new[] { new BigPiece(OverworldDecor.Willow, 0.3f, 2.2f, false), new BigPiece(OverworldDecor.HorseStandard, 0.04f, 1f, false) };
                case OverworldTerrain.HorseCamp:
                    return new[] { new BigPiece(OverworldDecor.FeltYurt, 0.35f, 3.2f, true), new BigPiece(OverworldDecor.HorseStandard, 0.25f, 1f, false), new BigPiece(OverworldDecor.StoneWell, 0.06f, 1.8f, true) };
                case OverworldTerrain.KurganField:
                    return new[] { new BigPiece(OverworldDecor.StoneBaba, 0.32f, 1f, false), new BigPiece(OverworldDecor.GoldStag, 0.16f, 1.8f, false) };
                case OverworldTerrain.WormwoodSteppe:
                    return new[] { new BigPiece(OverworldDecor.StoneBaba, 0.1f, 1f, false), new BigPiece(OverworldDecor.HorseStandard, 0.04f, 1f, false) };
                case OverworldTerrain.BirchSteppe:
                    return new[] { new BigPiece(OverworldDecor.Birch, 0.6f, 1.4f, false) };
                case OverworldTerrain.GiantHills:
                    return new[] { new BigPiece(OverworldDecor.GiantCauldron, 0.14f, 3f, true), new BigPiece(OverworldDecor.StoneBaba, 0.05f, 1f, false) };
                case OverworldTerrain.DreamVale:
                    return new[] { new BigPiece(OverworldDecor.DreamTablet, 0.45f, 1.6f, false), new BigPiece(OverworldDecor.Birch, 0.15f, 1.4f, false) };
                case OverworldTerrain.Taiga:
                    return new[] { new BigPiece(OverworldDecor.Larch, 0.65f, 2.4f, false) };
                case OverworldTerrain.PoppyPrairie:
                    return new[] { new BigPiece(OverworldDecor.StoneBaba, 0.06f, 1f, false), new BigPiece(OverworldDecor.HorseStandard, 0.05f, 1f, false) };
                case OverworldTerrain.HorsePlains:
                    return new[] { new BigPiece(OverworldDecor.StoneWell, 0.12f, 1.8f, true), new BigPiece(OverworldDecor.StoneBaba, 0.08f, 1f, false) };
                case OverworldTerrain.SvanValley:
                    return new[] { new BigPiece(OverworldDecor.SvanTower, 0.28f, 3.2f, true), new BigPiece(OverworldDecor.Birch, 0.25f, 1.4f, false) };
                case OverworldTerrain.ChainPeak:
                    return new[] { new BigPiece(OverworldDecor.SvanTower, 0.04f, 3.2f, true), new BigPiece(OverworldDecor.MountainPine, 0.15f, 2f, false) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterNartes(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsNartes(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "own" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;
            ScatterAmiraniChain(layout, content);

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsNartes(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = NartesPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.Larch || piece.Decor == OverworldDecor.SvanTower ? 0.9f + 0.4f * s01 : 0.85f + 0.3f * s01;
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

            ScatterNartesRamparts(layout, content, salt);
            ScatterNartesDetail(layout, content, salt);
        }

        /// <summary>
        /// Amirani's chain: one run every <see cref="ChainStep"/> m along <see cref="OverworldLayouts.AmiraniChain"/> (in the
        /// region's own coordinates, the zone's origin added back; the steps are counted from the chain's foot, so they line up
        /// from chunk to chunk) wherever the ground is the chain's crags or the snow crest; the trails, the water and the
        /// clearings leave gaps in it. The links lie on the ground: no collider (the chain is the climbers' way).
        /// </summary>
        private static void ScatterAmiraniChain(OverworldLayout layout, OverworldChunkContent content)
        {
            Rect r = content.Rect;
            OverworldZone zone = OverworldRules.ZoneAt(layout, r.center);
            if (zone == null || !IsNartes(zone.Terrain))
            {
                return;
            }

            Vector2 origin = zone.Origin;
            Vector2[] line = OverworldLayouts.AmiraniChain;
            Rect inRegion = new Rect(r.x - origin.x - ChainStep, r.y - origin.y - ChainStep, r.width + ChainStep * 2f, r.height + ChainStep * 2f);
            int step = 0;
            for (int i = 0; i + 1 < line.Length; i++)
            {
                Vector2 a0 = line[i];
                Vector2 a1 = line[i + 1];
                float length = Vector2.Distance(a0, a1);
                int n = Mathf.Max(1, Mathf.RoundToInt(length / ChainStep));
                for (int j = 0; j < n; j++, step++)
                {
                    Vector2 la = Vector2.Lerp(a0, a1, j / (float)n);
                    if (!inRegion.Contains(la))
                    {
                        continue;
                    }

                    Vector2 a = la + origin;
                    Vector2 c = Vector2.Lerp(a0, a1, (j + 1) / (float)n) + origin;
                    if (!r.Contains(a))
                    {
                        continue;
                    }

                    OverworldTerrain t = content.Grid.Near(a).Terrain;
                    if ((t != OverworldTerrain.ChainPeak && t != OverworldTerrain.SnowCrest) || DistanceToWall(layout, a) < EdgeBelt)
                    {
                        continue;
                    }

                    Vector2 mid = (a + c) * 0.5f;
                    if (IsFree(layout, content, a, 1.2f) && IsFree(layout, content, c, 1.2f) && IsFree(layout, content, mid, 1.2f))
                    {
                        content.Props.Add(new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.ChainRun, Position = a, End = c, Scale = 1f, Variant = step });
                    }
                }
            }
        }

        /// <summary>The giants' earthen ramparts: runs along the contour of their hills (never over a terrace riser).</summary>
        private static void ScatterNartesRamparts(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 4; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 4f, r.xMax - 4f, TownStyle.Hash01(salt, k + 1000)), Mathf.Lerp(r.yMin + 4f, r.yMax - 4f, TownStyle.Hash01(salt, k + 1100)));
                if (content.Grid.Near(p).Terrain != OverworldTerrain.GiantHills || TownStyle.Hash01(salt, k + 1200) >= 0.55f || layout.InAuthoredArea(p)
                    || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                float yaw = TownStyle.Hash01(salt, k + 1300) * 180f;
                int pieces = 4 + (int)(TownStyle.Hash(salt, k + 1400) % 5u);
                Vector2 from = p;
                for (int i = 0; i < pieces; i++)
                {
                    float gx = content.Grid.Sample(from + Vector2.right) - content.Grid.Sample(from - Vector2.right);
                    float gz = content.Grid.Sample(from + Vector2.up) - content.Grid.Sample(from - Vector2.up);
                    if (gx * gx + gz * gz > 0.0004f)
                    {
                        float along = Mathf.Atan2(gz, gx) * Mathf.Rad2Deg + 90f;
                        yaw = Mathf.Abs(Mathf.DeltaAngle(along, yaw)) > 90f ? along + 180f : along;
                    }

                    Vector2 dir = new Vector2(Mathf.Cos(yaw * Mathf.Deg2Rad), Mathf.Sin(yaw * Mathf.Deg2Rad));
                    Vector2 to = from + dir * RowPiece;
                    Vector2 mid = (from + to) * 0.5f;
                    if (!r.Contains(from) || !r.Contains(to))
                    {
                        break;
                    }

                    float hFrom = content.Grid.Sample(from), hTo = content.Grid.Sample(to), hMid = content.Grid.Sample(mid);
                    bool level = Mathf.Abs(hFrom - hTo) < 0.5f && hMid > Mathf.Min(hFrom, hTo) - 0.15f;
                    if (level && IsFree(layout, content, from, 1.6f) && IsFree(layout, content, to, 1.6f) && IsFree(layout, content, mid, 1.6f))
                    {
                        var prop = new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.EarthRampart, Position = from, End = to, Scale = 1f, Variant = k * 31 + i };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                    }

                    from = to;
                }
            }
        }

        /// <summary>Feather grass, poppies, salt crust, snow drifts and mist banks (ground detail: no colliders).</summary>
        private static void ScatterNartesDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 50; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsNartes(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                switch (info.Terrain)
                {
                    case OverworldTerrain.FeatherSteppe: decor = OverworldDecor.FeatherTuft; chance = 0.6f; break;
                    case OverworldTerrain.HorseCamp: decor = OverworldDecor.FeatherTuft; chance = 0.2f; break;
                    case OverworldTerrain.KurganField: decor = OverworldDecor.FeatherTuft; chance = 0.35f; break;
                    case OverworldTerrain.PoppyPrairie: decor = OverworldDecor.PoppyTuft; chance = 0.6f; break;
                    case OverworldTerrain.HorsePlains:
                        decor = (k & 1) == 0 ? OverworldDecor.PoppyTuft : OverworldDecor.FeatherTuft;
                        chance = 0.25f;
                        break;
                    case OverworldTerrain.WormwoodSteppe: decor = OverworldDecor.SaltCrust; chance = 0.12f; break;
                    case OverworldTerrain.Taiga: decor = OverworldDecor.MistBank; chance = 0.04f; break;
                    case OverworldTerrain.DreamVale: decor = OverworldDecor.MistBank; chance = 0.05f; break;
                    case OverworldTerrain.SvanValley: decor = OverworldDecor.MistBank; chance = 0.03f; break;
                    case OverworldTerrain.ChainPeak: decor = OverworldDecor.SnowDrift; chance = 0.1f; break;
                    case OverworldTerrain.SnowCrest: decor = OverworldDecor.SnowDrift; chance = 0.2f; break;
                    default: continue;
                }

                float room = decor == OverworldDecor.MistBank ? 2.5f : decor == OverworldDecor.SnowDrift || decor == OverworldDecor.SaltCrust ? 1.2f : 0.8f;
                if (TownStyle.Hash01(salt, k + 22000) >= chance || info.Clearing < 0.5f || DistanceToWall(layout, p) < -10f || !IsOpen(layout, content, p, room, false))
                {
                    continue;
                }

                content.Details.Add(new OverworldProp
                {
                    Decor = decor, Position = p, Yaw = TownStyle.Hash01(salt, k + 23000) * 360f, Scale = 0.8f + 0.5f * TownStyle.Hash01(salt, k + 24000), Variant = k,
                });
            }
        }

        // ------------------------------------------------------------------
        // Colliders and meshes
        // ------------------------------------------------------------------

        private static bool AddNartesDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsNartesDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.FeltYurt:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.6f * s, Height = 2.9f * s + sink });
                    break;
                case OverworldDecor.HorseStandard:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.2f * s, Height = 4.2f * s + sink });
                    break;
                case OverworldDecor.StoneBaba:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.5f * s, Height = 2.4f * s + sink });
                    break;
                case OverworldDecor.GoldStag:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.4f * s, 2.2f * s), Height = 1.6f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.Birch:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.4f * s, Height = 4f * s + sink });
                    break;
                case OverworldDecor.Larch:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.45f * s, Height = 6f * s + sink });
                    break;
                case OverworldDecor.DreamTablet:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.7f * s, 0.6f * s), Height = 3f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.GiantCauldron:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2.4f * s, Height = 2.6f * s + sink });
                    break;
                case OverworldDecor.SvanTower:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(3.4f * s, 3.4f * s), Height = 9f * s + sink, Yaw = prop.Yaw + 45f });
                    break;
                case OverworldDecor.StoneWell:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.35f * s, Height = 1.1f * s + sink });
                    break;
                case OverworldDecor.EarthRampart:
                    AddRunSolid(content, prop, 2.4f, 1.3f, sink + 0.3f);
                    break;
            }

            return true;
        }

        private static bool AppendNartesDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsNartesDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.ChainRun:
                    OverworldDecorKit.ChainRun(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.EarthRampart:
                    OverworldDecorKit.EarthRampart(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.FeatherTuft:
                    b.Stamp(OverworldDecorKit.FeatherStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.PoppyTuft:
                    b.Stamp(OverworldDecorKit.PoppyStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.FeltYurt: OverworldDecorKit.FeltYurt(b, s, prop.Variant); break;
                case OverworldDecor.HorseStandard: OverworldDecorKit.HorseStandard(b, s, prop.Variant); break;
                case OverworldDecor.StoneBaba: OverworldDecorKit.StoneBaba(b, s, prop.Variant); break;
                case OverworldDecor.GoldStag: OverworldDecorKit.GoldStag(b, s, prop.Variant); break;
                case OverworldDecor.Birch: OverworldDecorKit.Birch(b, s, prop.Variant); break;
                case OverworldDecor.Larch: OverworldDecorKit.Larch(b, s, prop.Variant); break;
                case OverworldDecor.DreamTablet: OverworldDecorKit.DreamTablet(b, s, prop.Variant); break;
                case OverworldDecor.GiantCauldron: OverworldDecorKit.GiantCauldron(b, s, prop.Variant); break;
                case OverworldDecor.SvanTower: OverworldDecorKit.SvanTower(b, s, prop.Variant); break;
                case OverworldDecor.StoneWell: OverworldDecorKit.StoneWell(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
