using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 7 — Teto do Mundo: the high mountain's pieces of the chunk builder. Before the noise scatter, each chunk gets the
    /// big pieces of its areas (apricots in flower on Sangkent's terraces and in the apricot valley, old junipers on the
    /// slopes, the herders' stone huts and the yak pens, the chute stations, seracs on the ice, wind wheels on the ridges,
    /// watch towers round the fort, Aydahar's shed scales on the black crags, moraine boulders), the runs (polished stone
    /// chutes down the slopes of the Calhas, the caravans' ribbon lines along the Wakhan, terrace and pen walls of dry
    /// stone), and the ground detail of its own (crevasse lines on the glacier, marmot burrows on the steppe and the pastures,
    /// ice shards, snow drifts and mist banks on the high ground); then the shared scatter fills around them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Teto do Mundo terrain (Região 7, 120–139).</summary>
        public static bool IsPamir(OverworldTerrain t) => OverworldTerrains.IsPamir(t);

        private static bool IsPamirDecor(OverworldDecor decor) => OverworldDecorKit.IsPamirDecor(decor);

        /// <summary>Room (m) a scattered Teto piece keeps from the others.</summary>
        private static float PamirClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.ApricotBlossom: return 1.8f;
                case OverworldDecor.Juniper: return 1.8f;
                case OverworldDecor.StoneHut: return 2.6f;
                case OverworldDecor.ChuteStation: return 3f;
                case OverworldDecor.Serac: return 2f;
                case OverworldDecor.WindWheel: return 2f;
                case OverworldDecor.WatchTower: return 2.4f;
                case OverworldDecor.DragonScale: return 1.6f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] PamirPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.StairTerraces:
                    return new[] { new BigPiece(OverworldDecor.ApricotBlossom, 0.35f, 2f, false), new BigPiece(OverworldDecor.StoneHut, 0.12f, 2.8f, true) };
                case OverworldTerrain.ApricotValley:
                    return new[] { new BigPiece(OverworldDecor.ApricotBlossom, 0.55f, 2f, false), new BigPiece(OverworldDecor.StoneHut, 0.05f, 2.8f, true) };
                case OverworldTerrain.PoplarCanals:
                    return new[] { new BigPiece(OverworldDecor.StoneHut, 0.1f, 2.8f, true), new BigPiece(OverworldDecor.ApricotBlossom, 0.12f, 2f, false) };
                case OverworldTerrain.HighSteppe:
                    return new[] { new BigPiece(OverworldDecor.Kraal, 0.06f, 4.8f, true), new BigPiece(OverworldDecor.StoneHut, 0.06f, 2.8f, true), new BigPiece(OverworldDecor.GraniteBlock, 0.1f, 2.4f, false) };
                case OverworldTerrain.MeltRiver:
                    return new[] { new BigPiece(OverworldDecor.GraniteBlock, 0.2f, 2.4f, false) };
                case OverworldTerrain.JuniperSlope:
                    return new[] { new BigPiece(OverworldDecor.Juniper, 0.5f, 2f, false), new BigPiece(OverworldDecor.RockSpire, 0.1f, 2f, false) };
                case OverworldTerrain.YakPasture:
                    return new[] { new BigPiece(OverworldDecor.Kraal, 0.25f, 4.8f, true), new BigPiece(OverworldDecor.StoneHut, 0.15f, 2.8f, true) };
                case OverworldTerrain.TurquoiseLake:
                    return new[] { new BigPiece(OverworldDecor.GraniteBlock, 0.12f, 2.4f, false), new BigPiece(OverworldDecor.StoneHut, 0.05f, 2.8f, true) };
                case OverworldTerrain.StoneChutes:
                    return new[] { new BigPiece(OverworldDecor.ChuteStation, 0.14f, 3.2f, true), new BigPiece(OverworldDecor.RockSpire, 0.12f, 2f, false) };
                case OverworldTerrain.SnowTerraces:
                    return new[] { new BigPiece(OverworldDecor.StoneHut, 0.2f, 2.8f, true), new BigPiece(OverworldDecor.GraniteBlock, 0.1f, 2.4f, false) };
                case OverworldTerrain.Moraine:
                    return new[] { new BigPiece(OverworldDecor.GraniteBlock, 0.45f, 2.4f, false) };
                case OverworldTerrain.Glacier:
                    return new[] { new BigPiece(OverworldDecor.Serac, 0.25f, 2.2f, false) };
                case OverworldTerrain.Seracs:
                    return new[] { new BigPiece(OverworldDecor.Serac, 0.7f, 2.2f, false) };
                case OverworldTerrain.WindRidge:
                    return new[] { new BigPiece(OverworldDecor.WindWheel, 0.22f, 2.2f, true), new BigPiece(OverworldDecor.RockSpire, 0.25f, 2f, false) };
                case OverworldTerrain.FortRock:
                    return new[] { new BigPiece(OverworldDecor.WatchTower, 0.2f, 2.6f, true), new BigPiece(OverworldDecor.WindWheel, 0.1f, 2.2f, true), new BigPiece(OverworldDecor.RockSpire, 0.2f, 2f, false) };
                case OverworldTerrain.WakhanValley:
                    return new[] { new BigPiece(OverworldDecor.GraniteBlock, 0.12f, 2.4f, false), new BigPiece(OverworldDecor.StoneHut, 0.06f, 2.8f, true) };
                case OverworldTerrain.WakhanPass:
                    return new[] { new BigPiece(OverworldDecor.StoneHut, 0.08f, 2.8f, true), new BigPiece(OverworldDecor.GraniteBlock, 0.15f, 2.4f, false) };
                case OverworldTerrain.DragonCrags:
                    return new[] { new BigPiece(OverworldDecor.DragonScale, 0.35f, 1.8f, false), new BigPiece(OverworldDecor.RockSpire, 0.3f, 2f, false) };
                case OverworldTerrain.IcePeaks:
                    return new[] { new BigPiece(OverworldDecor.Serac, 0.25f, 2.2f, false), new BigPiece(OverworldDecor.GraniteBlock, 0.25f, 2.4f, false) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterPamir(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsPamir(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "owp" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsPamir(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = PamirPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.RockSpire ? 0.8f + 0.5f * s01 : 0.85f + 0.3f * s01;
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

            ScatterPamirRuns(layout, content, salt);
            ScatterPamirDetail(layout, content, salt);
        }

        /// <summary>
        /// The runs: stone chutes straight down the slope in the Calhas, the caravans' ribbon lines in the Wakhan and round the
        /// fort, dry-stone terrace and pen walls on Sangkent's terraces, the snowy terraces, the pastures and the channels.
        /// </summary>
        private static void ScatterPamirRuns(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 3; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 4f, r.xMax - 4f, TownStyle.Hash01(salt, k + 1000)), Mathf.Lerp(r.yMin + 4f, r.yMax - 4f, TownStyle.Hash01(salt, k + 1100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                OverworldDecor decor;
                float chance;
                float bend = 0f;
                switch (terrain)
                {
                    case OverworldTerrain.StoneChutes: decor = OverworldDecor.StoneChute; chance = 0.7f; break;
                    case OverworldTerrain.WakhanValley: decor = OverworldDecor.RibbonLine; chance = 0.3f; break;
                    case OverworldTerrain.WakhanPass: decor = OverworldDecor.RibbonLine; chance = 0.4f; break;
                    case OverworldTerrain.FortRock: decor = OverworldDecor.RibbonLine; chance = 0.15f; break;
                    case OverworldTerrain.StairTerraces: decor = OverworldDecor.DryStoneWall; chance = 0.55f; break;
                    case OverworldTerrain.SnowTerraces: decor = OverworldDecor.DryStoneWall; chance = 0.3f; break;
                    case OverworldTerrain.YakPasture: decor = OverworldDecor.DryStoneWall; chance = 0.25f; bend = 9f; break;
                    case OverworldTerrain.PoplarCanals: decor = OverworldDecor.DryStoneWall; chance = 0.2f; break;
                    default: continue;
                }

                if (TownStyle.Hash01(salt, k + 1200) >= chance || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                if (decor == OverworldDecor.RibbonLine)
                {
                    // One line of pennants, 6 m between its poles.
                    float a = TownStyle.Hash01(salt, k + 1300) * Mathf.PI * 2f;
                    Vector2 c = p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 6f;
                    if (r.Contains(c) && IsFree(layout, content, p, 0.8f) && IsFree(layout, content, c, 0.8f) && IsFree(layout, content, (p + c) * 0.5f, 0.8f))
                    {
                        var prop = new OverworldProp { Kind = CityPropKind.FenceRun, Decor = decor, Position = p, End = c, Scale = 1f, Variant = k * 7 + (int)(a * 10f) };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                    }

                    continue;
                }

                float yaw = TownStyle.Hash01(salt, k + 1300) * 360f;
                if (decor == OverworldDecor.StoneChute)
                {
                    // Straight down the slope (the steepest way the ground falls), else any way.
                    float gx = content.Grid.Sample(p + Vector2.right) - content.Grid.Sample(p - Vector2.right);
                    float gz = content.Grid.Sample(p + Vector2.up) - content.Grid.Sample(p - Vector2.up);
                    if (gx * gx + gz * gz > 0.0004f)
                    {
                        yaw = Mathf.Atan2(-gz, -gx) * Mathf.Rad2Deg;
                    }
                }

                int pieces = 3 + (int)(TownStyle.Hash(salt, k + 1400) % 5u);
                float height = decor == OverworldDecor.DryStoneWall ? 0.9f + 0.4f * TownStyle.Hash01(salt, k + 1500) : 1f;
                float turn = (TownStyle.Hash01(salt, k + 1600) < 0.5f ? -1f : 1f) * bend;
                Vector2 from = p;
                for (int i = 0; i < pieces; i++, yaw += turn)
                {
                    Vector2 dir = new Vector2(Mathf.Cos(yaw * Mathf.Deg2Rad), Mathf.Sin(yaw * Mathf.Deg2Rad));
                    Vector2 to = from + dir * RowPiece;
                    Vector2 mid = (from + to) * 0.5f;
                    if (!r.Contains(from) || !r.Contains(to))
                    {
                        break;
                    }

                    if (IsFree(layout, content, from, 1f) && IsFree(layout, content, to, 1f) && IsFree(layout, content, mid, 1f))
                    {
                        var prop = new OverworldProp { Kind = CityPropKind.FenceRun, Decor = decor, Position = from, End = to, Scale = height, Variant = k * 31 + i };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                    }

                    from = to;
                }
            }
        }

        /// <summary>Crevasse lines, marmot burrows, ice shards, snow drifts and mist banks (ground detail: no colliders).</summary>
        private static void ScatterPamirDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsPamir(info.Terrain))
                {
                    continue;
                }

                PamirDetailChances(info.Terrain, out float crevasse, out float marmot, out float shards, out float snow, out float mist);
                float roll = TownStyle.Hash01(salt, k + 22000);
                OverworldDecor decor;
                if ((roll -= crevasse) < 0f)
                {
                    decor = OverworldDecor.Crevasse;
                }
                else if ((roll -= marmot) < 0f)
                {
                    decor = OverworldDecor.MarmotMound;
                }
                else if ((roll -= shards) < 0f)
                {
                    decor = OverworldDecor.IceShards;
                }
                else if ((roll -= mist) < 0f)
                {
                    decor = OverworldDecor.MistBank;
                }
                else if ((roll -= snow) < 0f)
                {
                    decor = OverworldDecor.SnowDrift;
                }
                else
                {
                    continue;
                }

                float room = decor == OverworldDecor.MistBank ? 2.5f : decor == OverworldDecor.Crevasse ? 2.6f : 1.2f;
                if (info.Clearing < 0.5f || DistanceToWall(layout, p) < -10f || !IsOpen(layout, content, p, room, false))
                {
                    continue;
                }

                content.Details.Add(new OverworldProp
                {
                    Decor = decor, Position = p, Yaw = TownStyle.Hash01(salt, k + 23000) * 360f, Scale = 0.8f + 0.5f * TownStyle.Hash01(salt, k + 24000), Variant = k,
                });
            }
        }

        /// <summary>Per detail attempt: crevasse lines, marmot burrows, ice shards, snow drifts and mist banks of each Teto terrain.</summary>
        private static void PamirDetailChances(OverworldTerrain terrain, out float crevasse, out float marmot, out float shards, out float snow, out float mist)
        {
            crevasse = marmot = shards = snow = mist = 0f;
            switch (terrain)
            {
                case OverworldTerrain.HighSteppe: marmot = 0.2f; break;
                case OverworldTerrain.YakPasture: marmot = 0.14f; break;
                case OverworldTerrain.WakhanValley: marmot = 0.12f; snow = 0.04f; break;
                case OverworldTerrain.MeltRiver: marmot = 0.05f; mist = 0.04f; break;
                case OverworldTerrain.TurquoiseLake: marmot = 0.06f; mist = 0.06f; break;
                case OverworldTerrain.StairTerraces: marmot = 0.04f; break;
                case OverworldTerrain.ApricotValley: marmot = 0.04f; break;
                case OverworldTerrain.JuniperSlope: snow = 0.06f; break;
                case OverworldTerrain.StoneChutes: snow = 0.2f; shards = 0.04f; break;
                case OverworldTerrain.SnowTerraces: snow = 0.35f; mist = 0.04f; break;
                case OverworldTerrain.Moraine: snow = 0.12f; marmot = 0.04f; break;
                case OverworldTerrain.Glacier: crevasse = 0.35f; shards = 0.15f; snow = 0.1f; mist = 0.04f; break;
                case OverworldTerrain.Seracs: crevasse = 0.12f; shards = 0.3f; snow = 0.15f; mist = 0.06f; break;
                case OverworldTerrain.WindRidge: snow = 0.3f; mist = 0.08f; break;
                case OverworldTerrain.FortRock: snow = 0.12f; mist = 0.06f; break;
                case OverworldTerrain.WakhanPass: snow = 0.4f; mist = 0.08f; shards = 0.04f; break;
                case OverworldTerrain.DragonCrags: snow = 0.15f; mist = 0.12f; break;
                case OverworldTerrain.IcePeaks: shards = 0.25f; snow = 0.4f; mist = 0.15f; break;
            }
        }

        // ------------------------------------------------------------------
        // Colliders and meshes
        // ------------------------------------------------------------------

        private static bool AddPamirDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsPamirDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.ApricotBlossom:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.3f * s, Height = 2.4f * s + sink });
                    break;
                case OverworldDecor.Juniper:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.42f * s, Height = 2.8f * s + sink });
                    break;
                case OverworldDecor.StoneHut:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(3.8f * s, 3.2f * s), Height = 2.1f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.ChuteStation:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(4.2f * s, 3.2f * s), Height = 3.6f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.Serac:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.4f * s, Height = 5.4f * s + sink });
                    break;
                case OverworldDecor.WindWheel:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.5f * s, Height = 5.4f * s + sink });
                    break;
                case OverworldDecor.WatchTower:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.9f * s, 2.9f * s), Height = 7.6f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.DragonScale:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.6f * s, 0.7f * s), Height = 3.3f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.StoneChute:
                    AddRunSolid(content, prop, 1.4f, 1.1f, sink);
                    break;
                case OverworldDecor.RibbonLine:
                    foreach (Vector2 at in new[] { prop.Position, prop.End })
                    {
                        content.Solids.Add(new OverworldSolid { Center = at, Base = content.Grid.Sample(at) - sink, Radius = 0.1f, Height = 2.4f + sink });
                    }

                    break;
            }

            return true;
        }

        private static bool AppendPamirDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsPamirDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.StoneChute:
                    OverworldDecorKit.StoneChute(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.RibbonLine:
                    OverworldDecorKit.RibbonLine(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.Crevasse:
                    b.Stamp(OverworldDecorKit.CrevasseStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.MarmotMound:
                    b.Stamp(OverworldDecorKit.MarmotStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.IceShards:
                    b.Stamp(OverworldDecorKit.IceShardStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.ApricotBlossom: OverworldDecorKit.ApricotBlossom(b, s, prop.Variant); break;
                case OverworldDecor.Juniper: OverworldDecorKit.Juniper(b, s, prop.Variant); break;
                case OverworldDecor.StoneHut: OverworldDecorKit.StoneHut(b, s, prop.Variant); break;
                case OverworldDecor.ChuteStation: OverworldDecorKit.ChuteStation(b, s, prop.Variant); break;
                case OverworldDecor.Serac: OverworldDecorKit.Serac(b, s, prop.Variant); break;
                case OverworldDecor.WindWheel: OverworldDecorKit.WindWheel(b, s, prop.Variant); break;
                case OverworldDecor.WatchTower: OverworldDecorKit.WatchTower(b, s, prop.Variant); break;
                case OverworldDecor.DragonScale: OverworldDecorKit.DragonScale(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
