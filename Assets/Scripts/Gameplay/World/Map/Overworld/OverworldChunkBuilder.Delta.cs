using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 6 — Delta de Jhilpur: the delta pieces of the chunk builder. Before the noise scatter, each chunk gets the big
    /// pieces of its areas (banyans in the groves, by the lake and in the tidal forest, root arches in the gorges, the fishers'
    /// stilt huts in the reed swamp, lotus lanterns in the bog, springs of Living Water and rotten roots in the black mire,
    /// snags in the dead wood), the earthen dikes of the paddies, the striped posts on the banks of every ford (the shallows
    /// the hero may wade are marked as on the coast), and the ground detail of its own (lotus pads along the water, rice in
    /// the paddies, the Lodo's violet bubbles); then the shared scatter fills around them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Delta de Jhilpur terrain (Região 6, 100–119).</summary>
        public static bool IsDelta(OverworldTerrain t) => OverworldTerrains.IsDelta(t);

        private static bool IsDeltaDecor(OverworldDecor decor) => OverworldDecorKit.IsDeltaDecor(decor);

        /// <summary>Room (m) a scattered delta piece keeps from the others.</summary>
        private static float DeltaClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.Banyan: return 3.2f;
                case OverworldDecor.RootArch: return 2.8f;
                case OverworldDecor.StiltHut: return 2.4f;
                case OverworldDecor.LivingSpring: return 2.2f;
                case OverworldDecor.RottenRoot: return 1.6f;
                case OverworldDecor.LanternPole: return 1f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] DeltaPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.RootLake:
                    return new[] { new BigPiece(OverworldDecor.Banyan, 0.3f, 3.6f, false), new BigPiece(OverworldDecor.LanternPole, 0.2f, 1f, false) };
                case OverworldTerrain.BanyanGrove:
                    return new[] { new BigPiece(OverworldDecor.Banyan, 0.55f, 3.6f, false), new BigPiece(OverworldDecor.RootArch, 0.15f, 2.8f, false) };
                case OverworldTerrain.RootGorge:
                    return new[] { new BigPiece(OverworldDecor.RootArch, 0.35f, 2.8f, false), new BigPiece(OverworldDecor.Banyan, 0.2f, 3.6f, false) };
                case OverworldTerrain.RainForest:
                    return new[] { new BigPiece(OverworldDecor.Banyan, 0.25f, 3.6f, false) };
                case OverworldTerrain.ReedIslands:
                    return new[] { new BigPiece(OverworldDecor.StiltHut, 0.22f, 2.4f, true) };
                case OverworldTerrain.RaffiaBog:
                    return new[] { new BigPiece(OverworldDecor.LanternPole, 0.35f, 1f, false), new BigPiece(OverworldDecor.StiltHut, 0.06f, 2.4f, true) };
                case OverworldTerrain.BlackMire:
                    return new[] { new BigPiece(OverworldDecor.LivingSpring, 0.1f, 2.2f, true), new BigPiece(OverworldDecor.RottenRoot, 0.4f, 1.6f, false) };
                case OverworldTerrain.DeadWood:
                    return new[] { new BigPiece(OverworldDecor.RottenRoot, 0.35f, 1.6f, false) };
                case OverworldTerrain.TidalForest:
                    return new[] { new BigPiece(OverworldDecor.Banyan, 0.2f, 3.6f, false) };
                case OverworldTerrain.LotusMarsh:
                    return new[] { new BigPiece(OverworldDecor.LanternPole, 0.12f, 1f, false) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterDelta(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            bool coast = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                any |= IsDelta(info.Terrain);
                coast |= IsCoast(info.Terrain);
            }

            if (!any)
            {
                return;
            }

            string salt = "owj" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;
            if (!coast)
            {
                ScatterFordPosts(layout, content); // a chunk with coast ground got them from the coast already
            }

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsDelta(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = DeltaPiecesOf(terrain);
                float roll = TownStyle.Hash01(salt, k + 200);
                float sum = 0f;
                foreach (BigPiece piece in pieces)
                {
                    sum += piece.Chance;
                    if (roll >= sum)
                    {
                        continue;
                    }

                    float scale = 0.85f + 0.3f * TownStyle.Hash01(salt, k + 300);
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

            ScatterPaddyBunds(layout, content, salt);
            ScatterDeltaDetail(layout, content, salt);
        }

        /// <summary>The earthen dikes of the paddies: straight runs across the chunk (about north–south, like the levees' relief).</summary>
        private static void ScatterPaddyBunds(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 3; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 4f, r.xMax - 4f, TownStyle.Hash01(salt, k + 1000)), Mathf.Lerp(r.yMin + 4f, r.yMax - 4f, TownStyle.Hash01(salt, k + 1100)));
                if (content.Grid.Near(p).Terrain != OverworldTerrain.PaddyLevees || TownStyle.Hash01(salt, k + 1200) > 0.75f || layout.InAuthoredArea(p))
                {
                    continue;
                }

                float yaw = TownStyle.Hash01(salt, k + 1300) < 0.7f ? 90f : 0f;
                int pieces = 3 + (int)(TownStyle.Hash(salt, k + 1400) % 5u);
                Vector2 dir = new Vector2(Mathf.Cos(yaw * Mathf.Deg2Rad), Mathf.Sin(yaw * Mathf.Deg2Rad));
                Vector2 a = p;
                for (int i = 0; i < pieces; i++)
                {
                    Vector2 c = a + dir * RowPiece;
                    Vector2 mid = (a + c) * 0.5f;
                    if (!r.Contains(a) || !r.Contains(c))
                    {
                        break;
                    }

                    if (IsFree(layout, content, a, 1f) && IsFree(layout, content, c, 1f) && IsFree(layout, content, mid, 1f))
                    {
                        var prop = new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.PaddyBund, Position = a, End = c, Scale = 1f, Variant = k * 31 + i };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                    }

                    a = c;
                }
            }
        }

        /// <summary>Lotus pads along the delta's water, rice in the paddies, the Lodo's bubbles on the black mud (no colliders).</summary>
        private static void ScatterDeltaDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 60; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsDelta(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                if (info.Terrain == OverworldTerrain.PaddyLevees)
                {
                    decor = OverworldDecor.RiceTuft;
                    chance = 0.75f;
                }
                else if (info.Terrain == OverworldTerrain.BlackMire)
                {
                    decor = OverworldDecor.MireBubbles;
                    chance = 0.35f;
                }
                else if (info.RiverEdge > 0.6f && info.RiverEdge < 4f && (info.Terrain == OverworldTerrain.LotusMarsh || info.Terrain == OverworldTerrain.RootLake
                    || info.Terrain == OverworldTerrain.ReedIslands || info.Terrain == OverworldTerrain.MudFlats))
                {
                    decor = OverworldDecor.LotusPads;
                    chance = 0.7f;
                }
                else if (info.Terrain == OverworldTerrain.LotusMarsh)
                {
                    decor = OverworldDecor.LotusPads;
                    chance = 0.2f;
                }
                else
                {
                    continue;
                }

                if (TownStyle.Hash01(salt, k + 22000) >= chance || info.Clearing < 0.5f || DistanceToWall(layout, p) < -10f || !IsOpen(layout, content, p, 0.8f, false))
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

        private static bool AddDeltaDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsDeltaDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.Banyan:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.85f * s, Height = 3.4f * s + sink });
                    break;
                case OverworldDecor.RootArch:
                    foreach (float x in new[] { -2.3f, 2.3f })
                    {
                        Vector2 at = p + Rotate(new Vector2(x * s, 0f), prop.Yaw);
                        content.Solids.Add(new OverworldSolid { Center = at, Base = content.Grid.Sample(at) - sink, Radius = 0.45f * s, Height = 1.6f * s + sink });
                    }

                    break;
                case OverworldDecor.StiltHut:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.4f * s, 2.2f * s), Height = 3.4f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.LanternPole:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.12f, Height = 2.8f * s + sink });
                    break;
                case OverworldDecor.LivingSpring:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.8f * s, Height = 0.6f + sink });
                    break;
                case OverworldDecor.RottenRoot:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 1.2f * s + sink });
                    break;
                case OverworldDecor.PaddyBund:
                    AddRunSolid(content, prop, 1.2f, 0.6f, sink);
                    break;
            }

            return true;
        }

        private static bool AppendDeltaDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsDeltaDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.PaddyBund:
                    OverworldDecorKit.PaddyBund(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y));
                    return true;
                case OverworldDecor.LotusPads:
                    b.Stamp(OverworldDecorKit.LotusStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.RiceTuft:
                    b.Stamp(OverworldDecorKit.RiceStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.MireBubbles:
                    b.Stamp(OverworldDecorKit.BubbleStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.Banyan: OverworldDecorKit.Banyan(b, s, prop.Variant); break;
                case OverworldDecor.RootArch: OverworldDecorKit.RootArch(b, s, prop.Variant); break;
                case OverworldDecor.StiltHut: OverworldDecorKit.StiltHut(b, s, prop.Variant); break;
                case OverworldDecor.LanternPole: OverworldDecorKit.LanternPole(b, s, prop.Variant); break;
                case OverworldDecor.LivingSpring: OverworldDecorKit.LivingSpring(b, s, prop.Variant); break;
                case OverworldDecor.RottenRoot: OverworldDecorKit.RottenRoot(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
