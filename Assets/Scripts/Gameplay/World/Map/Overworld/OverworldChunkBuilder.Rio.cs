using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 13 — Rio das Doze Horas: the river's pieces of the chunk builder. Before the noise scatter, each chunk gets the big
    /// pieces of its areas (papyrus and obelisks on the quay and the piers, reed hulls on the beach, sheaves in the wheat, stone doorways
    /// and scarabs in the sands, Mehen's arches in the coils, palms by the Lake of Fire, Apep's scale slabs on the sandbank), then the
    /// ground detail of its own (lotus pads, sand ripples, fire puddles, purple scale chips); the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Rio terrain (Região 13, 240–259).</summary>
        public static bool IsRio(OverworldTerrain t) => OverworldTerrains.IsRio(t);

        private static bool IsRioDecor(OverworldDecor decor) => OverworldDecorKit.IsRioDecor(decor);

        /// <summary>Room (m) a scattered Rio piece keeps from the others.</summary>
        private static float RioClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.PapyrusClump: return 1.3f;
                case OverworldDecor.WheatSheaf: return 0.8f;
                case OverworldDecor.StoneDoorway: return 2.6f;
                case OverworldDecor.ScarabIdol: return 1.2f;
                case OverworldDecor.MehenArch: return 2.8f;
                case OverworldDecor.DatePalm: return 1.5f;
                case OverworldDecor.NightObelisk: return 1.3f;
                case OverworldDecor.ApepScaleSlab: return 1.7f;
                case OverworldDecor.StrandedHull: return 2.8f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] RioPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.NightQuay:
                    return new[] { new BigPiece(OverworldDecor.NightObelisk, 0.2f, 1.4f, true), new BigPiece(OverworldDecor.PapyrusClump, 0.35f, 1.5f, false) };
                case OverworldTerrain.PapyrusBank:
                    return new[] { new BigPiece(OverworldDecor.PapyrusClump, 0.55f, 1.5f, false), new BigPiece(OverworldDecor.StrandedHull, 0.12f, 3f, true) };
                case OverworldTerrain.AshDelta:
                    return new[] { new BigPiece(OverworldDecor.PapyrusClump, 0.25f, 1.5f, false), new BigPiece(OverworldDecor.Stump, 0.2f, 1f, false) };
                case OverworldTerrain.RowersBeach:
                    return new[] { new BigPiece(OverworldDecor.StrandedHull, 0.3f, 3f, true), new BigPiece(OverworldDecor.PapyrusClump, 0.2f, 1.5f, false) };
                case OverworldTerrain.HourPiers:
                    return new[] { new BigPiece(OverworldDecor.NightObelisk, 0.3f, 1.4f, true), new BigPiece(OverworldDecor.PapyrusClump, 0.2f, 1.5f, false), new BigPiece(OverworldDecor.StrandedHull, 0.1f, 3f, true) };
                case OverworldTerrain.BarqueCanal:
                    return new[] { new BigPiece(OverworldDecor.PapyrusClump, 0.4f, 1.5f, false), new BigPiece(OverworldDecor.NightObelisk, 0.15f, 1.4f, true) };
                case OverworldTerrain.ReedBeds:
                    return new[] { new BigPiece(OverworldDecor.PapyrusClump, 0.5f, 1.5f, false), new BigPiece(OverworldDecor.WheatSheaf, 0.2f, 1f, false) };
                case OverworldTerrain.ReedFields:
                    return new[] { new BigPiece(OverworldDecor.WheatSheaf, 0.4f, 1f, false), new BigPiece(OverworldDecor.PapyrusClump, 0.2f, 1.5f, false) };
                case OverworldTerrain.LightWheat:
                    return new[] { new BigPiece(OverworldDecor.WheatSheaf, 0.5f, 1f, false), new BigPiece(OverworldDecor.ScarabIdol, 0.1f, 1.2f, false) };
                case OverworldTerrain.SekhetCanals:
                    return new[] { new BigPiece(OverworldDecor.PapyrusClump, 0.35f, 1.5f, false), new BigPiece(OverworldDecor.NightObelisk, 0.12f, 1.4f, true) };
                case OverworldTerrain.HiddenSands:
                    return new[] { new BigPiece(OverworldDecor.ScarabIdol, 0.25f, 1.2f, false), new BigPiece(OverworldDecor.NightObelisk, 0.15f, 1.4f, true) };
                case OverworldTerrain.ClosedGates:
                    return new[] { new BigPiece(OverworldDecor.StoneDoorway, 0.3f, 2.8f, true), new BigPiece(OverworldDecor.ScarabIdol, 0.15f, 1.2f, false) };
                case OverworldTerrain.ZigzagSands:
                    return new[] { new BigPiece(OverworldDecor.ScarabIdol, 0.2f, 1.2f, false), new BigPiece(OverworldDecor.StoneDoorway, 0.15f, 2.8f, true) };
                case OverworldTerrain.MehenCoils:
                    return new[] { new BigPiece(OverworldDecor.MehenArch, 0.3f, 3f, true), new BigPiece(OverworldDecor.NightObelisk, 0.12f, 1.4f, true) };
                case OverworldTerrain.CoilKnot:
                    return new[] { new BigPiece(OverworldDecor.MehenArch, 0.4f, 3f, true), new BigPiece(OverworldDecor.ApepScaleSlab, 0.15f, 1.8f, false) };
                case OverworldTerrain.OuterCoil:
                    return new[] { new BigPiece(OverworldDecor.MehenArch, 0.25f, 3f, true), new BigPiece(OverworldDecor.ScarabIdol, 0.15f, 1.2f, false) };
                case OverworldTerrain.FireLakeShore:
                    return new[] { new BigPiece(OverworldDecor.DatePalm, 0.4f, 1.6f, false), new BigPiece(OverworldDecor.NightObelisk, 0.1f, 1.4f, true) };
                case OverworldTerrain.FireLake:
                    return new[] { new BigPiece(OverworldDecor.DatePalm, 0.45f, 1.6f, false) };
                case OverworldTerrain.ApepSandbank:
                    return new[] { new BigPiece(OverworldDecor.ApepScaleSlab, 0.3f, 1.8f, false), new BigPiece(OverworldDecor.StrandedHull, 0.15f, 3f, true) };
                case OverworldTerrain.ApepScales:
                    return new[] { new BigPiece(OverworldDecor.ApepScaleSlab, 0.45f, 1.8f, false), new BigPiece(OverworldDecor.ScarabIdol, 0.1f, 1.2f, false) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterRio(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsRio(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "rio" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsRio(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = RioPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.DatePalm || piece.Decor == OverworldDecor.NightObelisk ? 0.9f + 0.4f * s01 : 0.85f + 0.3f * s01;
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

            ScatterRioDetail(layout, content, salt);
        }

        /// <summary>Lotus pads, sand ripples, fire puddles and scale chips (ground detail: no colliders).</summary>
        private static void ScatterRioDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsRio(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                switch (info.Terrain)
                {
                    case OverworldTerrain.NightQuay: decor = OverworldDecor.LotusPad; chance = 0.08f; break;
                    case OverworldTerrain.PapyrusBank: decor = OverworldDecor.LotusPad; chance = 0.12f; break;
                    case OverworldTerrain.AshDelta: decor = OverworldDecor.SandRipple; chance = 0.1f; break;
                    case OverworldTerrain.RowersBeach: decor = OverworldDecor.SandRipple; chance = 0.2f; break;
                    case OverworldTerrain.HourPiers: decor = OverworldDecor.LotusPad; chance = 0.1f; break;
                    case OverworldTerrain.BarqueCanal: decor = OverworldDecor.LotusPad; chance = 0.2f; break;
                    case OverworldTerrain.ReedBeds: decor = OverworldDecor.LotusPad; chance = 0.06f; break;
                    case OverworldTerrain.ReedFields: decor = OverworldDecor.SandRipple; chance = 0.04f; break;
                    case OverworldTerrain.LightWheat: decor = OverworldDecor.SandRipple; chance = 0.04f; break;
                    case OverworldTerrain.SekhetCanals: decor = OverworldDecor.LotusPad; chance = 0.18f; break;
                    case OverworldTerrain.HiddenSands: decor = OverworldDecor.SandRipple; chance = 0.3f; break;
                    case OverworldTerrain.ClosedGates: decor = OverworldDecor.SandRipple; chance = 0.18f; break;
                    case OverworldTerrain.ZigzagSands: decor = OverworldDecor.SandRipple; chance = 0.25f; break;
                    case OverworldTerrain.MehenCoils: decor = OverworldDecor.ScaleChip; chance = 0.18f; break;
                    case OverworldTerrain.CoilKnot: decor = OverworldDecor.ScaleChip; chance = 0.22f; break;
                    case OverworldTerrain.OuterCoil: decor = OverworldDecor.ScaleChip; chance = 0.14f; break;
                    case OverworldTerrain.FireLakeShore: decor = OverworldDecor.FirePuddle; chance = 0.2f; break;
                    case OverworldTerrain.FireLake: decor = OverworldDecor.FirePuddle; chance = 0.3f; break;
                    case OverworldTerrain.ApepSandbank: decor = OverworldDecor.ScaleChip; chance = 0.2f; break;
                    case OverworldTerrain.ApepScales: decor = OverworldDecor.ScaleChip; chance = 0.32f; break;
                    default: continue;
                }

                float room = decor == OverworldDecor.FirePuddle ? 1.6f : decor == OverworldDecor.SandRipple ? 1.8f : 1.2f;
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

        private static bool AddRioDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsRioDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.PapyrusClump:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 2.4f * s + sink });
                    break;
                case OverworldDecor.WheatSheaf:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.45f * s, Height = 1.1f * s + sink });
                    break;
                case OverworldDecor.StoneDoorway:
                {
                    // Only the two posts: the doorway is walked through when its leaf is off; both stand either way.
                    Vector2 side = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad)) * 1.45f * s;
                    content.Solids.Add(new OverworldSolid { Center = p + side, Base = content.Grid.Sample(p + side), Radius = 0.5f * s, Height = 3f * s + sink });
                    content.Solids.Add(new OverworldSolid { Center = p - side, Base = content.Grid.Sample(p - side), Radius = 0.5f * s, Height = 3f * s + sink });
                    break;
                }
                case OverworldDecor.ScarabIdol:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 1.1f * s + sink });
                    break;
                case OverworldDecor.MehenArch:
                {
                    // The two feet of the arch: it is walked through.
                    Vector2 side = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad)) * 2.2f * s;
                    content.Solids.Add(new OverworldSolid { Center = p + side, Base = content.Grid.Sample(p + side), Radius = 0.5f * s, Height = 2.2f * s + sink });
                    content.Solids.Add(new OverworldSolid { Center = p - side, Base = content.Grid.Sample(p - side), Radius = 0.5f * s, Height = 2.2f * s + sink });
                    break;
                }
                case OverworldDecor.DatePalm:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.4f * s, Height = 3f * s + sink });
                    break;
                case OverworldDecor.NightObelisk:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 4f * s + sink });
                    break;
                case OverworldDecor.ApepScaleSlab:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 1.5f * s + sink });
                    break;
                case OverworldDecor.StrandedHull:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.2f * s, Height = 1f * s + sink });
                    break;
            }

            return true;
        }

        private static bool AppendRioDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsRioDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.LotusPad:
                    b.Stamp(OverworldDecorKit.LotusPadStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.SandRipple:
                    b.Stamp(OverworldDecorKit.SandRippleStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.FirePuddle:
                    b.Stamp(OverworldDecorKit.FirePuddleStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.ScaleChip:
                    b.Stamp(OverworldDecorKit.ScaleChipStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.PapyrusClump: OverworldDecorKit.PapyrusClump(b, s, prop.Variant); break;
                case OverworldDecor.WheatSheaf: OverworldDecorKit.WheatSheaf(b, s, prop.Variant); break;
                case OverworldDecor.StoneDoorway: OverworldDecorKit.StoneDoorway(b, s, prop.Variant); break;
                case OverworldDecor.ScarabIdol: OverworldDecorKit.ScarabIdol(b, s, prop.Variant); break;
                case OverworldDecor.MehenArch: OverworldDecorKit.MehenArch(b, s, prop.Variant); break;
                case OverworldDecor.DatePalm: OverworldDecorKit.DatePalm(b, s, prop.Variant); break;
                case OverworldDecor.NightObelisk: OverworldDecorKit.NightObelisk(b, s, prop.Variant); break;
                case OverworldDecor.ApepScaleSlab: OverworldDecorKit.ApepScaleSlab(b, s, prop.Variant); break;
                case OverworldDecor.StrandedHull: OverworldDecorKit.StrandedHull(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
