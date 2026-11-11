using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 8 — Planície de Mingcheng: the plain's pieces of the chunk builder. Before the noise scatter, each chunk gets the
    /// big pieces of its areas (willows and red lanterns round the city and along the water, pavilions, stone lions and stele
    /// turtles on the spirit way, terracotta pits, bamboo clumps, karst towers and mountain pines, white spires, mirrors and
    /// Meroë pyramids by the Torres Brancas, the paddies' water wheel and scarecrows, earth pillars in the loess), the runs (the
    /// great wall along its line on the crest with a beacon tower every 120 m, tea rows and paddy bunds following the
    /// contour, field banks in the millet), and the ground detail of its own (rice in the paddies, millet heads, bamboo shoots,
    /// lotus pads along the canal, mist banks in the pines, the karst and under the wall); then the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>Metres between two runs of the great wall (and the beacon tower spacing, in runs).</summary>
        private const float WallStep = 6f;

        private const int BeaconEvery = 20;

        /// <summary>Top (m over the ground) of the wall's and the towers' colliders: under the deck the hero walks on (3.4 m), well above a jump.</summary>
        private const float WallBodyTop = 3.1f;

        /// <summary>A Planície de Mingcheng terrain (Região 8, 140–159).</summary>
        public static bool IsMingcheng(OverworldTerrain t) => OverworldTerrains.IsMingcheng(t);

        private static bool IsMingchengDecor(OverworldDecor decor) => OverworldDecorKit.IsMingchengDecor(decor);

        /// <summary>Room (m) a scattered Mingcheng piece keeps from the others.</summary>
        private static float MingchengClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.Willow: return 2f;
                case OverworldDecor.RedLantern: return 0.8f;
                case OverworldDecor.Pavilion: return 2.4f;
                case OverworldDecor.StoneLion: return 1.4f;
                case OverworldDecor.StoneTurtle: return 1.6f;
                case OverworldDecor.TerracottaPit: return 4.4f;
                case OverworldDecor.BeaconTower: return 3.6f;
                case OverworldDecor.Bamboo: return 1.4f;
                case OverworldDecor.KarstPillar: return 3f;
                case OverworldDecor.MountainPine: return 1.8f;
                case OverworldDecor.MirrorPost: return 0.8f;
                case OverworldDecor.WhiteSpire: return 2f;
                case OverworldDecor.WaterWheel: return 3f;
                case OverworldDecor.Scarecrow: return 0.9f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] MingchengPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.NineRings:
                    return new[] { new BigPiece(OverworldDecor.Willow, 0.3f, 2.2f, false), new BigPiece(OverworldDecor.Pavilion, 0.1f, 2.6f, true), new BigPiece(OverworldDecor.RedLantern, 0.15f, 1f, false) };
                case OverworldTerrain.MilletFields:
                    return new[] { new BigPiece(OverworldDecor.Scarecrow, 0.18f, 1f, false), new BigPiece(OverworldDecor.Pavilion, 0.05f, 2.6f, true) };
                case OverworldTerrain.WillowRiver:
                    return new[] { new BigPiece(OverworldDecor.Willow, 0.5f, 2.2f, false), new BigPiece(OverworldDecor.WaterWheel, 0.05f, 3.2f, true) };
                case OverworldTerrain.LanternCanal:
                    return new[] { new BigPiece(OverworldDecor.Willow, 0.35f, 2.2f, false), new BigPiece(OverworldDecor.StoneTurtle, 0.1f, 1.8f, true), new BigPiece(OverworldDecor.Pavilion, 0.08f, 2.6f, true) };
                case OverworldTerrain.MirrorPaddies:
                    return new[] { new BigPiece(OverworldDecor.Scarecrow, 0.25f, 1f, false), new BigPiece(OverworldDecor.WaterWheel, 0.1f, 3.2f, true) };
                case OverworldTerrain.TeaTerraces:
                    return new[] { new BigPiece(OverworldDecor.Pavilion, 0.12f, 2.6f, true), new BigPiece(OverworldDecor.RedLantern, 0.05f, 1f, false) };
                case OverworldTerrain.BambooGrove:
                    return new[] { new BigPiece(OverworldDecor.Bamboo, 0.7f, 1.6f, false) };
                case OverworldTerrain.WhiteHills:
                    return new[] { new BigPiece(OverworldDecor.WhiteSpire, 0.12f, 2.2f, true), new BigPiece(OverworldDecor.MirrorPost, 0.2f, 1f, false) };
                case OverworldTerrain.SpireGardens:
                    return new[] { new BigPiece(OverworldDecor.WhiteSpire, 0.25f, 2.2f, true), new BigPiece(OverworldDecor.Pyramid, 0.1f, 3.6f, true), new BigPiece(OverworldDecor.MirrorPost, 0.2f, 1f, false) };
                case OverworldTerrain.KarstPeaks:
                    return new[] { new BigPiece(OverworldDecor.KarstPillar, 0.35f, 3.2f, true), new BigPiece(OverworldDecor.MountainPine, 0.2f, 2f, false) };
                case OverworldTerrain.PineRidge:
                    return new[] { new BigPiece(OverworldDecor.MountainPine, 0.6f, 2f, false) };
                case OverworldTerrain.GreatWall:
                    return new[] { new BigPiece(OverworldDecor.MountainPine, 0.25f, 2f, false) };
                case OverworldTerrain.TerracottaPits:
                    return new[] { new BigPiece(OverworldDecor.TerracottaPit, 0.4f, 4.6f, true), new BigPiece(OverworldDecor.StoneLion, 0.06f, 1.6f, true) };
                case OverworldTerrain.SpiritWay:
                    return new[] { new BigPiece(OverworldDecor.StoneLion, 0.3f, 1.6f, true), new BigPiece(OverworldDecor.StoneTurtle, 0.15f, 1.8f, true), new BigPiece(OverworldDecor.MountainPine, 0.15f, 2f, false) };
                case OverworldTerrain.LoessGullies:
                    return new[] { new BigPiece(OverworldDecor.Hoodoo, 0.3f, 2f, false), new BigPiece(OverworldDecor.Pavilion, 0.03f, 2.6f, true) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterMingcheng(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsMingcheng(info.Terrain))
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
            ScatterGreatWall(layout, content);

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsMingcheng(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = MingchengPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.KarstPillar ? 0.9f + 0.6f * s01 : 0.85f + 0.3f * s01;
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

            ScatterMingchengRuns(layout, content, salt);
            ScatterMingchengDetail(layout, content, salt);
        }

        /// <summary>
        /// The great wall: one run every <see cref="WallStep"/> m along <see cref="OverworldLayouts.GreatWallZ"/> (in the
        /// region's own coordinates, the zone's origin added back) wherever the ground is the wall's terrain, a beacon tower every
        /// <see cref="BeaconEvery"/> runs; the trails, the water and the clearings open gates in it. The runs are chained on a
        /// grid that lines up from chunk to chunk, so the wall never breaks at a chunk border.
        /// </summary>
        private static void ScatterGreatWall(OverworldLayout layout, OverworldChunkContent content)
        {
            Rect r = content.Rect;
            OverworldZone zone = OverworldRules.ZoneAt(layout, r.center);
            if (zone == null || !IsMingcheng(zone.Terrain))
            {
                return;
            }

            Vector2 origin = zone.Origin;
            int i0 = Mathf.CeilToInt((r.xMin - origin.x) / WallStep - 0.001f);
            int i1 = Mathf.CeilToInt((r.xMax - origin.x) / WallStep - 0.001f);
            for (int i = i0; i < i1; i++)
            {
                float x0 = i * WallStep;
                var a = new Vector2(x0 + origin.x, OverworldLayouts.GreatWallZ(x0) + origin.y);
                if (a.y < r.yMin || a.y >= r.yMax || content.Grid.Near(a).Terrain != OverworldTerrain.GreatWall || DistanceToWall(layout, a) < EdgeBelt)
                {
                    continue;
                }

                var c = new Vector2(x0 + WallStep + origin.x, OverworldLayouts.GreatWallZ(x0 + WallStep) + origin.y);
                if (i % BeaconEvery == 0)
                {
                    if (IsFree(layout, content, a, 4f))
                    {
                        var tower = new OverworldProp
                        {
                            Kind = CityPropKind.Rock, Decor = OverworldDecor.BeaconTower, Position = a, Yaw = Mathf.Atan2(c.x - a.x, c.y - a.y) * Mathf.Rad2Deg, Scale = 1f,
                            Variant = i & 7, Color = TownSwatch.Stone,
                        };
                        content.Props.Add(tower);
                        AddSolid(layout, content, tower);
                        content.Blocked.Add(new Rect(a.x - 3.2f, a.y - 3.2f, 6.4f, 6.4f));
                    }

                    AddWallDeckSpan(content, a, c);
                    continue;
                }

                Vector2 mid = (a + c) * 0.5f;
                if (IsFree(layout, content, a, 2f) && IsFree(layout, content, c, 2f) && IsFree(layout, content, mid, 2f))
                {
                    var run = new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.GreatWallRun, Position = a, End = c, Scale = 1f, Variant = i };
                    content.Props.Add(run);
                    AddSolid(layout, content, run);
                }
                else
                {
                    AddWallDeckSpan(content, a, c);
                }
            }
        }

        /// <summary>Marks a <see cref="OverworldDecor.GreatWallRun"/> as only the deck over a gap (a gate over a trail, the stretch beside a tower): no body, no collider.</summary>
        private const int WallDeckVariant = int.MinValue;

        /// <summary>The wall's walk carried over a gap in its body, high enough to pass under.</summary>
        private static void AddWallDeckSpan(OverworldChunkContent content, Vector2 a, Vector2 c)
        {
            content.Props.Add(new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.GreatWallRun, Position = a, End = c, Scale = 1f, Variant = WallDeckVariant });
        }

        /// <summary>Tea rows and paddy bunds along the contour; straight field banks in the millet.</summary>
        private static void ScatterMingchengRuns(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 6; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 4f, r.xMax - 4f, TownStyle.Hash01(salt, k + 1000)), Mathf.Lerp(r.yMin + 4f, r.yMax - 4f, TownStyle.Hash01(salt, k + 1100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                OverworldDecor decor;
                float chance;
                bool contour = true;
                switch (terrain)
                {
                    case OverworldTerrain.TeaTerraces: decor = OverworldDecor.Hedge; chance = 0.8f; break;
                    case OverworldTerrain.MirrorPaddies: decor = OverworldDecor.PaddyBund; chance = 0.6f; break;
                    case OverworldTerrain.MilletFields: decor = OverworldDecor.PaddyBund; chance = 0.3f; contour = false; break;
                    case OverworldTerrain.LoessGullies: decor = OverworldDecor.PaddyBund; chance = 0.15f; break;
                    default: continue;
                }

                if (TownStyle.Hash01(salt, k + 1200) >= chance || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                float yaw = TownStyle.Hash01(salt, k + 1300) < 0.5f ? 0f : 90f;
                int pieces = 4 + (int)(TownStyle.Hash(salt, k + 1400) % 5u);
                Vector2 from = p;
                for (int i = 0; i < pieces; i++)
                {
                    if (contour)
                    {
                        // Across the slope: perpendicular to the steepest way the ground falls (else keep the heading).
                        float gx = content.Grid.Sample(from + Vector2.right) - content.Grid.Sample(from - Vector2.right);
                        float gz = content.Grid.Sample(from + Vector2.up) - content.Grid.Sample(from - Vector2.up);
                        if (gx * gx + gz * gz > 0.0004f)
                        {
                            float along = Mathf.Atan2(gz, gx) * Mathf.Rad2Deg + 90f;
                            yaw = Mathf.Abs(Mathf.DeltaAngle(along, yaw)) > 90f ? along + 180f : along;
                        }
                    }

                    Vector2 dir = new Vector2(Mathf.Cos(yaw * Mathf.Deg2Rad), Mathf.Sin(yaw * Mathf.Deg2Rad));
                    Vector2 to = from + dir * RowPiece;
                    Vector2 mid = (from + to) * 0.5f;
                    if (!r.Contains(from) || !r.Contains(to))
                    {
                        break;
                    }

                    // Never over a terrace riser: both ends and the middle on the same tread.
                    float hFrom = content.Grid.Sample(from), hTo = content.Grid.Sample(to), hMid = content.Grid.Sample(mid);
                    bool level = Mathf.Abs(hFrom - hTo) < 0.5f && hMid > Mathf.Min(hFrom, hTo) - 0.15f;
                    if (level && IsFree(layout, content, from, 1f) && IsFree(layout, content, to, 1f) && IsFree(layout, content, mid, 1f))
                    {
                        var prop = new OverworldProp { Kind = CityPropKind.FenceRun, Decor = decor, Position = from, End = to, Scale = 1f, Variant = k * 31 + i };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                    }

                    from = to;
                }
            }
        }

        /// <summary>Rice in the paddies, millet heads, bamboo shoots, lotus along the canal, mist banks (ground detail: no colliders).</summary>
        private static void ScatterMingchengDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 50; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsMingcheng(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                switch (info.Terrain)
                {
                    case OverworldTerrain.MirrorPaddies: decor = OverworldDecor.RiceTuft; chance = 0.55f; break;
                    case OverworldTerrain.MilletFields: decor = OverworldDecor.MilletTuft; chance = 0.6f; break;
                    case OverworldTerrain.BambooGrove: decor = OverworldDecor.BambooShoots; chance = 0.35f; break;
                    case OverworldTerrain.LanternCanal:
                    case OverworldTerrain.WillowRiver:
                        decor = OverworldDecor.LotusPads;
                        chance = info.RiverEdge > 0.6f && info.RiverEdge < 4f ? 0.5f : 0f;
                        break;
                    case OverworldTerrain.PineRidge: decor = OverworldDecor.MistBank; chance = 0.06f; break;
                    case OverworldTerrain.KarstPeaks: decor = OverworldDecor.MistBank; chance = 0.05f; break;
                    case OverworldTerrain.GreatWall: decor = OverworldDecor.MistBank; chance = 0.05f; break;
                    case OverworldTerrain.TeaTerraces: decor = OverworldDecor.MistBank; chance = 0.03f; break;
                    default: continue;
                }

                float room = decor == OverworldDecor.MistBank ? 2.5f : 0.8f;
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

        private static bool AddMingchengDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsMingchengDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.Willow:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.3f * s, Height = 2.6f * s + sink });
                    break;
                case OverworldDecor.RedLantern:
                case OverworldDecor.MirrorPost:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.22f * s, Height = 2.6f * s + sink });
                    break;
                case OverworldDecor.Pavilion:
                    foreach (Vector2 c in new[] { new Vector2(-1.2f, -1.2f), new Vector2(1.2f, -1.2f), new Vector2(-1.2f, 1.2f), new Vector2(1.2f, 1.2f) })
                    {
                        Vector2 at = p + Rotate(c * s, prop.Yaw);
                        content.Solids.Add(new OverworldSolid { Center = at, Base = content.Grid.Sample(at) - sink, Radius = 0.2f * s, Height = 2.6f * s + sink });
                    }

                    break;
                case OverworldDecor.StoneLion:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.5f * s, 2.1f * s), Height = 2.3f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.StoneTurtle:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.8f * s, 2.6f * s), Height = 3.3f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.TerracottaPit:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 3.4f * s, Height = 1.2f * s + sink });
                    break;
                case OverworldDecor.BeaconTower:
                    // Only up to under the wall's deck: the walk goes through the tower's open passage (Mechanics.MingchengWallRules).
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(5.1f * s, 5.1f * s), Height = WallBodyTop * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.Bamboo:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.55f * s, Height = 4f * s + sink });
                    break;
                case OverworldDecor.KarstPillar:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2.4f * s, Height = 9f * s + sink });
                    break;
                case OverworldDecor.MountainPine:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.35f * s, Height = 3.2f * s + sink });
                    break;
                case OverworldDecor.WhiteSpire:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.6f * s, 2.6f * s), Height = 5.6f * s + sink, Yaw = prop.Yaw + 45f });
                    break;
                case OverworldDecor.WaterWheel:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(4.6f * s, 3.2f * s), Height = 2.8f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.Scarecrow:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.25f * s, Height = 2.2f * s + sink });
                    break;
                case OverworldDecor.GreatWallRun:
                {
                    if (prop.Variant == WallDeckVariant)
                    {
                        break;
                    }

                    // The body stops under the deck (at the low end of the run, so no point of it pokes through), still far above a jump.
                    Vector2 d = prop.End - prop.Position;
                    float low = Mathf.Min(content.Grid.Sample(prop.Position), content.Grid.Sample(prop.End));
                    float deep = sink + 1.4f;
                    content.Solids.Add(new OverworldSolid
                    {
                        Center = (prop.Position + prop.End) * 0.5f, Base = low - deep, IsBox = true, Size = new Vector2(3.6f, d.magnitude), Height = WallBodyTop + deep,
                        Yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg,
                    });
                    break;
                }
            }

            return true;
        }

        private static bool AppendMingchengDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsMingchengDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.GreatWallRun:
                    if (prop.Variant == WallDeckVariant)
                    {
                        OverworldDecorKit.GreatWallDeckSpan(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y));
                        return true;
                    }

                    OverworldDecorKit.GreatWallRun(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.MilletTuft:
                    b.Stamp(OverworldDecorKit.MilletStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.BambooShoots:
                    b.Stamp(OverworldDecorKit.BambooShootStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.Willow: OverworldDecorKit.Willow(b, s, prop.Variant); break;
                case OverworldDecor.RedLantern: OverworldDecorKit.RedLantern(b, s, prop.Variant); break;
                case OverworldDecor.Pavilion: OverworldDecorKit.Pavilion(b, s, prop.Variant); break;
                case OverworldDecor.StoneLion: OverworldDecorKit.StoneLion(b, s, prop.Variant); break;
                case OverworldDecor.StoneTurtle: OverworldDecorKit.StoneTurtle(b, s, prop.Variant); break;
                case OverworldDecor.TerracottaPit: OverworldDecorKit.TerracottaPit(b, s, prop.Variant); break;
                case OverworldDecor.BeaconTower: OverworldDecorKit.BeaconTower(b, s, prop.Variant); break;
                case OverworldDecor.Bamboo: OverworldDecorKit.Bamboo(b, s, prop.Variant); break;
                case OverworldDecor.KarstPillar: OverworldDecorKit.KarstPillar(b, s, prop.Variant); break;
                case OverworldDecor.MountainPine: OverworldDecorKit.MountainPine(b, s, prop.Variant); break;
                case OverworldDecor.MirrorPost: OverworldDecorKit.MirrorPost(b, s, prop.Variant); break;
                case OverworldDecor.WhiteSpire: OverworldDecorKit.WhiteSpire(b, s, prop.Variant); break;
                case OverworldDecor.WaterWheel: OverworldDecorKit.WaterWheel(b, s, prop.Variant); break;
                case OverworldDecor.Scarecrow: OverworldDecorKit.Scarecrow(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
