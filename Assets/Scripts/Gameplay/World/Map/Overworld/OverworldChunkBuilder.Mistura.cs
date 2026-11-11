using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 14 — Margens da Mistura: the Margins' pieces of the chunk builder. Before the noise scatter, each chunk gets the fixed pieces of
    /// the sheets that stand in it (the twelve gnomons in their circle on the Desert of the Clock, Hobabish's seven lanterns of resin round
    /// the woodcutter's clearing), then the big pieces of its areas, then the ground detail of its own; the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Mistura terrain (Região 14, 260–279).</summary>
        public static bool IsMistura(OverworldTerrain t) => OverworldTerrains.IsMistura(t);

        private static bool IsMisturaDecor(OverworldDecor decor) => OverworldDecorKit.IsMisturaDecor(decor);

        /// <summary>Room (m) a scattered Mistura piece keeps from the others.</summary>
        private static float MisturaClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.GlazedLionStatue: return 1.4f;
                case OverworldDecor.BrickKiln: return 2f;
                case OverworldDecor.BrickStack: return 1.2f;
                case OverworldDecor.FallenGnomon: return 3.8f;
                case OverworldDecor.HourglassPillar: return 1.2f;
                case OverworldDecor.BitumenJar: return 1f;
                case OverworldDecor.ResinLantern: return 0.8f;
                case OverworldDecor.GiantCedar: return 3f;
                case OverworldDecor.DioriteShard: return 1.8f;
                case OverworldDecor.TabletStone: return 1.8f;
                case OverworldDecor.CutTrunk: return 3f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] MisturaPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.ZigguratTerrace:
                    return new[] { new BigPiece(OverworldDecor.GlazedLionStatue, 0.2f, 1.5f, true), new BigPiece(OverworldDecor.DatePalm, 0.25f, 1.6f, false) };
                case OverworldTerrain.BrickGate:
                    return new[] { new BigPiece(OverworldDecor.BrickStack, 0.3f, 1.3f, false), new BigPiece(OverworldDecor.GlazedLionStatue, 0.1f, 1.5f, true) };
                case OverworldTerrain.BrickKilns:
                    return new[] { new BigPiece(OverworldDecor.BrickKiln, 0.3f, 2f, true), new BigPiece(OverworldDecor.BrickStack, 0.25f, 1.3f, false) };
                case OverworldTerrain.TellMounds:
                    return new[] { new BigPiece(OverworldDecor.BrickStack, 0.2f, 1.3f, false), new BigPiece(OverworldDecor.BitumenJar, 0.15f, 1.1f, false) };
                case OverworldTerrain.PalmCanal:
                    return new[] { new BigPiece(OverworldDecor.DatePalm, 0.45f, 1.6f, false), new BigPiece(OverworldDecor.BitumenJar, 0.1f, 1.1f, false) };
                case OverworldTerrain.BarleyTerraces:
                    return new[] { new BigPiece(OverworldDecor.WheatSheaf, 0.35f, 1f, false), new BigPiece(OverworldDecor.GlazedLionStatue, 0.06f, 1.5f, true) };
                case OverworldTerrain.ClockDesert:
                    return new[] { new BigPiece(OverworldDecor.HourglassPillar, 0.18f, 1.3f, true) };
                case OverworldTerrain.ShadowSands:
                    return new[] { new BigPiece(OverworldDecor.HourglassPillar, 0.25f, 1.3f, true), new BigPiece(OverworldDecor.FallenGnomon, 0.08f, 3.8f, true) };
                case OverworldTerrain.HourSands:
                    return new[] { new BigPiece(OverworldDecor.HourglassPillar, 0.3f, 1.3f, true), new BigPiece(OverworldDecor.ScarabIdol, 0.1f, 1.2f, false) };
                case OverworldTerrain.TarPools:
                    return new[] { new BigPiece(OverworldDecor.BitumenJar, 0.35f, 1.1f, false), new BigPiece(OverworldDecor.PapyrusClump, 0.2f, 1.5f, false) };
                case OverworldTerrain.BitumenWells:
                    return new[] { new BigPiece(OverworldDecor.BitumenJar, 0.4f, 1.1f, false), new BigPiece(OverworldDecor.StrandedHull, 0.15f, 3f, true) };
                case OverworldTerrain.TarReeds:
                    return new[] { new BigPiece(OverworldDecor.PapyrusClump, 0.5f, 1.5f, false), new BigPiece(OverworldDecor.Stump, 0.1f, 1f, false) };
                case OverworldTerrain.CedarEdge:
                    return new[] { new BigPiece(OverworldDecor.GiantCedar, 0.3f, 3f, false), new BigPiece(OverworldDecor.Stump, 0.2f, 1f, false) };
                case OverworldTerrain.CedarRings:
                    return new[] { new BigPiece(OverworldDecor.GiantCedar, 0.45f, 3f, false) };
                case OverworldTerrain.ElderCedars:
                    return new[] { new BigPiece(OverworldDecor.GiantCedar, 0.6f, 3f, false), new BigPiece(OverworldDecor.ResinLantern, 0.1f, 0.9f, false) };
                case OverworldTerrain.ShoulderStone:
                    return new[] { new BigPiece(OverworldDecor.DioriteShard, 0.25f, 1.9f, false), new BigPiece(OverworldDecor.Basalt, 0.15f, 1.4f, false) };
                case OverworldTerrain.SkinFolds:
                    return new[] { new BigPiece(OverworldDecor.DioriteShard, 0.2f, 1.9f, false) };
                case OverworldTerrain.DioriteRise:
                    return new[] { new BigPiece(OverworldDecor.DioriteShard, 0.55f, 1.9f, false) };
                case OverworldTerrain.DreamTablets:
                    return new[] { new BigPiece(OverworldDecor.TabletStone, 0.45f, 2f, true), new BigPiece(OverworldDecor.CutTrunk, 0.1f, 3f, true) };
                case OverworldTerrain.CutGarden:
                    return new[] { new BigPiece(OverworldDecor.CutTrunk, 0.4f, 3f, true), new BigPiece(OverworldDecor.TabletStone, 0.15f, 2f, true) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterMistura(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsMistura(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "mistura" + content.Coord.x + "_" + content.Coord.y;
            PlaceMisturaFixedPieces(layout, content, salt);
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsMistura(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = MisturaPiecesOf(terrain);
                float roll = TownStyle.Hash01(salt, k + 200);
                float sum = 0f;
                foreach (BigPiece piece in pieces)
                {
                    sum += piece.Chance;
                    if (roll >= sum)
                    {
                        continue;
                    }

                    float scale = 0.85f + 0.35f * TownStyle.Hash01(salt, k + 300);
                    float radius = piece.Radius * scale;
                    if ((!piece.NeedsFlat || content.Grid.Flatness(p) > 0.95f) && IsFree(layout, content, p, radius + 1f))
                    {
                        AddMisturaProp(layout, content, piece.Decor, p, TownStyle.Hash01(salt, k + 400) * 360f, scale, (int)(TownStyle.Hash(salt, k + 500) & 7), radius, terrain);
                    }

                    break;
                }
            }

            ScatterMisturaDetail(layout, content, salt);
        }

        /// <summary>The fixed pieces of the sheets that fall in this chunk: the twelve gnomons and the seven resin lanterns.</summary>
        private static void PlaceMisturaFixedPieces(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Vector2 offset = layout.PartAt(content.Rect.center)?.Offset ?? Vector2.zero;
            for (int i = 0; i < OverworldLayouts.GnomonCount; i++)
            {
                Vector2 local = OverworldLayouts.GnomonPosition(i);
                Vector2 p = local + offset;
                if (content.Rect.Contains(p))
                {
                    TryPlaceFixedMistura(layout, content, OverworldDecor.FallenGnomon, p, 15f + 30f * i, 1.1f, i, 3.8f);
                }
            }

            for (int i = 0; i < OverworldLayouts.ResinLanternCount; i++)
            {
                Vector2 p = OverworldLayouts.ResinLanternPosition(i) + offset;
                if (content.Rect.Contains(p))
                {
                    TryPlaceFixedMistura(layout, content, OverworldDecor.ResinLantern, p, 0f, 1.1f, i, 1f);
                }
            }

            for (int i = 0; i < OverworldLayouts.BreathStones.Length; i++)
            {
                Vector2 p = OverworldLayouts.BreathStones[i] + offset;
                if (content.Rect.Contains(p))
                {
                    TryPlaceFixedMistura(layout, content, OverworldDecor.DioriteShard, p, 40f * i, 1.2f, i, 1.9f);
                }
            }
        }

        private static void TryPlaceFixedMistura(OverworldLayout layout, OverworldChunkContent content, OverworldDecor decor, Vector2 p, float yaw, float scale, int variant, float radius)
        {
            if (!IsMistura(content.Grid.Near(p).Terrain) || DistanceToWall(layout, p) < EdgeBelt || !IsFree(layout, content, p, radius + 0.5f))
            {
                return;
            }

            AddMisturaProp(layout, content, decor, p, yaw, scale, variant, radius, content.Grid.Near(p).Terrain);
        }

        private static void AddMisturaProp(OverworldLayout layout, OverworldChunkContent content, OverworldDecor decor, Vector2 p, float yaw, float scale, int variant, float radius, OverworldTerrain terrain)
        {
            var prop = new OverworldProp { Kind = CityPropKind.Rock, Decor = decor, Position = p, Yaw = yaw, Scale = scale, Variant = variant, Color = RockColor(terrain) };
            content.Props.Add(prop);
            AddSolid(layout, content, prop);
            content.Blocked.Add(new Rect(p.x - radius, p.y - radius, radius * 2f, radius * 2f));
        }

        /// <summary>Ground detail: brick shards, dial marks, pitch bubbles, resin drops, diorite and tablet chips (no colliders).</summary>
        private static void ScatterMisturaDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsMistura(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                switch (info.Terrain)
                {
                    case OverworldTerrain.ZigguratTerrace: decor = OverworldDecor.BrickShard; chance = 0.12f; break;
                    case OverworldTerrain.BrickGate: decor = OverworldDecor.BrickShard; chance = 0.22f; break;
                    case OverworldTerrain.BrickKilns: decor = OverworldDecor.BrickShard; chance = 0.3f; break;
                    case OverworldTerrain.TellMounds: decor = OverworldDecor.BrickShard; chance = 0.18f; break;
                    case OverworldTerrain.PalmCanal: decor = OverworldDecor.TarBubble; chance = 0.06f; break;
                    case OverworldTerrain.BarleyTerraces: decor = OverworldDecor.BrickShard; chance = 0.05f; break;
                    case OverworldTerrain.ClockDesert: decor = OverworldDecor.ClockRipple; chance = 0.3f; break;
                    case OverworldTerrain.ShadowSands: decor = OverworldDecor.ClockRipple; chance = 0.16f; break;
                    case OverworldTerrain.HourSands: decor = OverworldDecor.ClockRipple; chance = 0.22f; break;
                    case OverworldTerrain.TarPools: decor = OverworldDecor.TarBubble; chance = 0.3f; break;
                    case OverworldTerrain.BitumenWells: decor = OverworldDecor.TarBubble; chance = 0.22f; break;
                    case OverworldTerrain.TarReeds: decor = OverworldDecor.TarBubble; chance = 0.12f; break;
                    case OverworldTerrain.CedarEdge: decor = OverworldDecor.ResinDrip; chance = 0.1f; break;
                    case OverworldTerrain.CedarRings: decor = OverworldDecor.ResinDrip; chance = 0.24f; break;
                    case OverworldTerrain.ElderCedars: decor = OverworldDecor.ResinDrip; chance = 0.3f; break;
                    case OverworldTerrain.ShoulderStone: decor = OverworldDecor.DioriteChip; chance = 0.12f; break;
                    case OverworldTerrain.SkinFolds: decor = OverworldDecor.DioriteChip; chance = 0.08f; break;
                    case OverworldTerrain.DioriteRise: decor = OverworldDecor.DioriteChip; chance = 0.34f; break;
                    case OverworldTerrain.DreamTablets: decor = OverworldDecor.TabletChip; chance = 0.3f; break;
                    case OverworldTerrain.CutGarden: decor = OverworldDecor.TabletChip; chance = 0.1f; break;
                    default: continue;
                }

                float room = decor == OverworldDecor.ClockRipple ? 1.8f : 1.1f;
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

        private static bool AddMisturaDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsMisturaDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.GlazedLionStatue:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 1.6f * s + sink });
                    break;
                case OverworldDecor.BrickKiln:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.4f * s, Height = 2.1f * s + sink });
                    break;
                case OverworldDecor.BrickStack:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 0.9f * s + sink });
                    break;
                case OverworldDecor.FallenGnomon:
                {
                    // The long stone is a row of capsules along its length.
                    Vector2 along = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad));
                    foreach (float t in new[] { -2.6f, 0f, 2.6f })
                    {
                        Vector2 c = p + along * (t * s);
                        content.Solids.Add(new OverworldSolid { Center = c, Base = content.Grid.Sample(c) - 0.1f, Radius = 0.9f * s, Height = 1.4f * s + sink });
                    }

                    break;
                }
                case OverworldDecor.HourglassPillar:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 3f * s + sink });
                    break;
                case OverworldDecor.BitumenJar:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 1.4f * s + sink });
                    break;
                case OverworldDecor.ResinLantern:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.35f * s, Height = 2.4f * s + sink });
                    break;
                case OverworldDecor.GiantCedar:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 6.5f * s + sink });
                    break;
                case OverworldDecor.DioriteShard:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 2.2f * s + sink });
                    break;
                case OverworldDecor.TabletStone:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.2f * s, Height = 3.8f * s + sink });
                    break;
                case OverworldDecor.CutTrunk:
                {
                    Vector2 along = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad));
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.75f * s, Height = 0.75f * s + sink });
                    Vector2 c = p + along * (2.4f * s);
                    content.Solids.Add(new OverworldSolid { Center = c, Base = content.Grid.Sample(c) - 0.1f, Radius = 0.8f * s, Height = 1f * s + sink });
                    break;
                }
            }

            return true;
        }

        private static bool AppendMisturaDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsMisturaDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.BrickShard:
                    b.Stamp(OverworldDecorKit.BrickShardStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.ClockRipple:
                    b.Stamp(OverworldDecorKit.ClockRippleStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.TarBubble:
                    b.Stamp(OverworldDecorKit.TarBubbleStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.ResinDrip:
                    b.Stamp(OverworldDecorKit.ResinDripStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.DioriteChip:
                    b.Stamp(OverworldDecorKit.DioriteChipStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.TabletChip:
                    b.Stamp(OverworldDecorKit.TabletChipStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.GlazedLionStatue: OverworldDecorKit.GlazedLionStatue(b, s, prop.Variant); break;
                case OverworldDecor.BrickKiln: OverworldDecorKit.BrickKiln(b, s, prop.Variant); break;
                case OverworldDecor.BrickStack: OverworldDecorKit.BrickStack(b, s, prop.Variant); break;
                case OverworldDecor.FallenGnomon: OverworldDecorKit.FallenGnomon(b, s, prop.Variant); break;
                case OverworldDecor.HourglassPillar: OverworldDecorKit.HourglassPillar(b, s, prop.Variant); break;
                case OverworldDecor.BitumenJar: OverworldDecorKit.BitumenJar(b, s, prop.Variant); break;
                case OverworldDecor.ResinLantern: OverworldDecorKit.ResinLantern(b, s, prop.Variant); break;
                case OverworldDecor.GiantCedar: OverworldDecorKit.GiantCedar(b, s, prop.Variant); break;
                case OverworldDecor.DioriteShard: OverworldDecorKit.DioriteShard(b, s, prop.Variant); break;
                case OverworldDecor.TabletStone: OverworldDecorKit.TabletStone(b, s, prop.Variant); break;
                case OverworldDecor.CutTrunk: OverworldDecorKit.CutTrunk(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
