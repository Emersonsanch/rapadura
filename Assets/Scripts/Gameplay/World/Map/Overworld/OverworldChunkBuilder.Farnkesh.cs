using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 10 — Picos de Farnkesh: the peaks' pieces of the chunk builder. Before the noise scatter, each chunk gets the big
    /// pieces of its areas (vines, gourd huts and stone stages in the satyrs' vale, willows by the Pactolo, sandstone pinnacles,
    /// hermitages, rope baskets and wind bells on the heights, shelter stones and twisted trees in the gorge, Huma's golden nest),
    /// the runs (the rope bridges along their line over the Pontes de Vento, lines of wind paper streamers in the meadows and
    /// the stone nests), and the ground detail of its own (cloud puffs in the sea of clouds and on the isles, the Pactolo's golden
    /// pebbles, mist banks in the pinnacle wood); then the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>Metres between two runs of the rope bridges.</summary>
        private const float RopeBridgeStep = 4f;

        /// <summary>A Picos de Farnkesh terrain (Região 10, 180–199).</summary>
        public static bool IsFarnkesh(OverworldTerrain t) => OverworldTerrains.IsFarnkesh(t);

        private static bool IsFarnkeshDecor(OverworldDecor decor) => OverworldDecorKit.IsFarnkeshDecor(decor);

        /// <summary>Room (m) a scattered Farnkesh piece keeps from the others.</summary>
        private static float FarnkeshClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.SandstonePinnacle: return 3.2f;
                case OverworldDecor.Hermitage: return 3f;
                case OverworldDecor.WindBell: return 1.4f;
                case OverworldDecor.RopeBasket: return 2.2f;
                case OverworldDecor.GrapeVine: return 0.6f;
                case OverworldDecor.GourdHut: return 2.2f;
                case OverworldDecor.StoneStage: return 3.6f;
                case OverworldDecor.ShelterStone: return 2.8f;
                case OverworldDecor.TwistedTree: return 1.6f;
                case OverworldDecor.GoldenNest: return 5f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] FarnkeshPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.VineVale:
                    return new[] { new BigPiece(OverworldDecor.GrapeVine, 0.5f, 0.8f, false), new BigPiece(OverworldDecor.GourdHut, 0.06f, 2.4f, true) };
                case OverworldTerrain.GoldenRiver:
                    return new[] { new BigPiece(OverworldDecor.Willow, 0.3f, 2.2f, false), new BigPiece(OverworldDecor.StoneStage, 0.04f, 3.8f, true) };
                case OverworldTerrain.SatyrGlades:
                    return new[] { new BigPiece(OverworldDecor.GourdHut, 0.3f, 2.4f, true), new BigPiece(OverworldDecor.StoneStage, 0.12f, 3.8f, true), new BigPiece(OverworldDecor.GrapeVine, 0.1f, 0.8f, false) };
                case OverworldTerrain.FootWoods:
                    return new[] { new BigPiece(OverworldDecor.TwistedTree, 0.12f, 1.8f, false), new BigPiece(OverworldDecor.GourdHut, 0.03f, 2.4f, true) };
                case OverworldTerrain.AlpineMeadow:
                    return new[] { new BigPiece(OverworldDecor.WindBell, 0.18f, 1.5f, false), new BigPiece(OverworldDecor.Cairn, 0.06f, 1.2f, false), new BigPiece(OverworldDecor.Hermitage, 0.04f, 3.2f, true) };
                case OverworldTerrain.PinnacleWood:
                    return new[] { new BigPiece(OverworldDecor.SandstonePinnacle, 0.3f, 3.4f, true), new BigPiece(OverworldDecor.Hermitage, 0.08f, 3.2f, true), new BigPiece(OverworldDecor.MountainPine, 0.2f, 2f, false) };
                case OverworldTerrain.StoneNests:
                    return new[] { new BigPiece(OverworldDecor.Hermitage, 0.3f, 3.2f, true), new BigPiece(OverworldDecor.RopeBasket, 0.15f, 2.4f, true), new BigPiece(OverworldDecor.WindBell, 0.15f, 1.5f, false), new BigPiece(OverworldDecor.SandstonePinnacle, 0.1f, 3.4f, true) };
                case OverworldTerrain.CloudSea:
                    return new[] { new BigPiece(OverworldDecor.SandstonePinnacle, 0.14f, 3.4f, true), new BigPiece(OverworldDecor.MountainPine, 0.06f, 2f, false) };
                case OverworldTerrain.WindBridges:
                    return new[] { new BigPiece(OverworldDecor.WindBell, 0.15f, 1.5f, false), new BigPiece(OverworldDecor.SandstonePinnacle, 0.18f, 3.4f, true) };
                case OverworldTerrain.BellCliffs:
                    return new[] { new BigPiece(OverworldDecor.Hermitage, 0.3f, 3.2f, true), new BigPiece(OverworldDecor.WindBell, 0.2f, 1.5f, false) };
                case OverworldTerrain.AysGorge:
                    return new[] { new BigPiece(OverworldDecor.ShelterStone, 0.3f, 3f, true), new BigPiece(OverworldDecor.TwistedTree, 0.2f, 1.8f, false) };
                case OverworldTerrain.WindScree:
                    return new[] { new BigPiece(OverworldDecor.TwistedTree, 0.25f, 1.8f, false), new BigPiece(OverworldDecor.ShelterStone, 0.12f, 3f, true), new BigPiece(OverworldDecor.Cairn, 0.1f, 1.2f, false) };
                case OverworldTerrain.CloudIsles:
                    return new[] { new BigPiece(OverworldDecor.SandstonePinnacle, 0.08f, 3.4f, true), new BigPiece(OverworldDecor.RockSpire, 0.1f, 1.6f, false), new BigPiece(OverworldDecor.MountainPine, 0.1f, 2f, false) };
                case OverworldTerrain.HumaNest:
                    return new[] { new BigPiece(OverworldDecor.GoldenNest, 0.14f, 5.2f, true), new BigPiece(OverworldDecor.RockSpire, 0.1f, 1.6f, false) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterFarnkesh(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsFarnkesh(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "fk" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;
            ScatterRopeBridges(layout, content);
            PlaceSkyRouteStations(layout, content);

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsFarnkesh(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = FarnkeshPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.SandstonePinnacle || piece.Decor == OverworldDecor.GoldenNest ? 0.9f + 0.4f * s01 : 0.85f + 0.3f * s01;
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

            ScatterPaperStreamers(layout, content, salt);
            ScatterFarnkeshDetail(layout, content, salt);
        }

        /// <summary>
        /// The rope bridges: one run every <see cref="RopeBridgeStep"/> m along each line of <see cref="OverworldLayouts.WindBridgeLines"/>
        /// (in the region's own coordinates, the zone's origin added back; the steps are counted from each line's start, so they
        /// line up from chunk to chunk) wherever the ground is the wind bridges' or the stone nests'; the trails, the water and the
        /// clearings leave gaps. The planks lie on the ground: no collider (the bridges are the way).
        /// </summary>
        private static void ScatterRopeBridges(OverworldLayout layout, OverworldChunkContent content)
        {
            Rect r = content.Rect;
            OverworldZone zone = OverworldRules.ZoneAt(layout, r.center);
            if (zone == null || !IsFarnkesh(zone.Terrain))
            {
                return;
            }

            Vector2 origin = zone.Origin;
            Rect inRegion = new Rect(r.x - origin.x - RopeBridgeStep, r.y - origin.y - RopeBridgeStep, r.width + RopeBridgeStep * 2f, r.height + RopeBridgeStep * 2f);
            foreach (Vector2[] line in OverworldLayouts.WindBridgeLines)
            {
                int step = 0;
                for (int i = 0; i + 1 < line.Length; i++)
                {
                    Vector2 a0 = line[i];
                    Vector2 a1 = line[i + 1];
                    int n = Mathf.Max(1, Mathf.RoundToInt(Vector2.Distance(a0, a1) / RopeBridgeStep));
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
                        if ((t != OverworldTerrain.WindBridges && t != OverworldTerrain.StoneNests) || DistanceToWall(layout, a) < EdgeBelt)
                        {
                            continue;
                        }

                        Vector2 mid = (a + c) * 0.5f;
                        if (IsFree(layout, content, a, 1.4f) && IsFree(layout, content, c, 1.4f) && IsFree(layout, content, mid, 1.4f))
                        {
                            content.Props.Add(new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.RopeBridgeRun, Position = a, End = c, Scale = 1f, Variant = step });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The rope baskets' posts and the wind bells that mark the currents (<see cref="OverworldLayouts.SkyRoutes"/>), placed
        /// before any scatter so nothing lands on the stations; the landings beside them stay clear.
        /// </summary>
        private static void PlaceSkyRouteStations(OverworldLayout layout, OverworldChunkContent content)
        {
            Rect r = content.Rect;
            OverworldZone anchor = layout.Zone(OverworldLayouts.FarnkeshAnchorZoneId);
            if (anchor == null)
            {
                return;
            }

            foreach (Mechanics.SkyRoute route in OverworldLayouts.SkyRoutes)
            {
                OverworldDecor decor = route.Kind == Mechanics.SkyRouteKind.RopeBasket ? OverworldDecor.RopeBasket : OverworldDecor.WindBell;
                PlaceStation(layout, content, r, decor, anchor.Origin + route.PropA, anchor.Origin + route.A, route.Id);
                PlaceStation(layout, content, r, decor, anchor.Origin + route.PropB, anchor.Origin + route.B, route.Id);
            }
        }

        private static void PlaceStation(OverworldLayout layout, OverworldChunkContent content, Rect r, OverworldDecor decor, Vector2 at, Vector2 landing, string salt)
        {
            if (!r.Contains(at) || content.Props.Exists(p => p.Decor == decor && p.Position == at))
            {
                return;
            }

            var prop = new OverworldProp
            {
                Kind = CityPropKind.Rock, Decor = decor, Position = at, Yaw = TownStyle.Hash01(salt, 1) * 360f, Scale = 1f,
                Variant = (int)(TownStyle.Hash(salt, 2) & 7), Color = RockColor(content.Grid.Near(at).Terrain),
            };
            content.Props.Add(prop);
            AddSolid(layout, content, prop);
            const float clear = 4.5f;
            content.Blocked.Add(new Rect(at.x - 2f, at.y - 2f, 4f, 4f));
            content.Blocked.Add(new Rect(landing.x - clear, landing.y - clear, clear * 2f, clear * 2f));
        }

        /// <summary>Lines of wind paper streamers: one short run between two poles on level ground in the meadows, nests and bridges.</summary>
        private static void ScatterPaperStreamers(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 3; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 6f, r.xMax - 6f, TownStyle.Hash01(salt, k + 1000)), Mathf.Lerp(r.yMin + 6f, r.yMax - 6f, TownStyle.Hash01(salt, k + 1100)));
                OverworldTerrain t = content.Grid.Near(p).Terrain;
                float chance = t == OverworldTerrain.AlpineMeadow ? 0.45f : t == OverworldTerrain.StoneNests ? 0.5f : t == OverworldTerrain.BellCliffs ? 0.25f : 0f;
                if (TownStyle.Hash01(salt, k + 1200) >= chance || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                float yaw = TownStyle.Hash01(salt, k + 1300) * Mathf.PI;
                Vector2 to = p + new Vector2(Mathf.Cos(yaw), Mathf.Sin(yaw)) * (5f + 3f * TownStyle.Hash01(salt, k + 1400));
                Vector2 mid = (p + to) * 0.5f;
                if (!r.Contains(to) || Mathf.Abs(content.Grid.Sample(p) - content.Grid.Sample(to)) > 0.8f)
                {
                    continue;
                }

                if (IsFree(layout, content, p, 0.8f) && IsFree(layout, content, to, 0.8f) && IsFree(layout, content, mid, 0.8f))
                {
                    content.Props.Add(new OverworldProp { Kind = CityPropKind.FenceRun, Decor = OverworldDecor.PaperStreamers, Position = p, End = to, Scale = 1f, Variant = k * 7 + content.Coord.x });
                }
            }
        }

        /// <summary>Cloud puffs, the Pactolo's golden pebbles, mist banks and snow drifts (ground detail: no colliders).</summary>
        private static void ScatterFarnkeshDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsFarnkesh(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                switch (info.Terrain)
                {
                    case OverworldTerrain.CloudSea: decor = OverworldDecor.CloudPuff; chance = 0.3f; break;
                    case OverworldTerrain.WindBridges: decor = OverworldDecor.CloudPuff; chance = 0.14f; break;
                    case OverworldTerrain.CloudIsles: decor = OverworldDecor.CloudPuff; chance = 0.2f; break;
                    case OverworldTerrain.HumaNest: decor = OverworldDecor.CloudPuff; chance = 0.1f; break;
                    case OverworldTerrain.StoneNests: decor = OverworldDecor.CloudPuff; chance = 0.04f; break;
                    case OverworldTerrain.GoldenRiver: decor = OverworldDecor.GoldPebbles; chance = 0.35f; break;
                    case OverworldTerrain.SatyrGlades: decor = OverworldDecor.GoldPebbles; chance = 0.06f; break;
                    case OverworldTerrain.PinnacleWood: decor = OverworldDecor.MistBank; chance = 0.05f; break;
                    case OverworldTerrain.FootWoods: decor = OverworldDecor.MistBank; chance = 0.03f; break;
                    case OverworldTerrain.BellCliffs: decor = OverworldDecor.MistBank; chance = 0.03f; break;
                    case OverworldTerrain.WindScree: decor = OverworldDecor.SnowDrift; chance = 0.05f; break;
                    default: continue;
                }

                float room = decor == OverworldDecor.MistBank || decor == OverworldDecor.CloudPuff ? 2.5f : decor == OverworldDecor.SnowDrift ? 1.2f : 0.6f;
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

        private static bool AddFarnkeshDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsFarnkeshDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.SandstonePinnacle:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2.4f * s, Height = 9f * s + sink });
                    break;
                case OverworldDecor.Hermitage:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(4f * s, 3.4f * s), Height = 3.6f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.WindBell:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.1f * s, 0.4f * s), Height = 2.9f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.RopeBasket:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 4.6f * s + sink });
                    break;
                case OverworldDecor.GrapeVine:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.25f * s, Height = 1.6f * s + sink });
                    break;
                case OverworldDecor.GourdHut:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.6f * s, Height = 3.6f * s + sink });
                    break;
                case OverworldDecor.StoneStage:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2.5f * s, Height = 0.45f * s + sink });
                    break;
                case OverworldDecor.ShelterStone:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2.6f * s, Height = 1.4f * s + sink });
                    break;
                case OverworldDecor.TwistedTree:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.35f * s, Height = 2.4f * s + sink });
                    break;
                case OverworldDecor.GoldenNest:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 4.2f * s, Height = 1.4f * s + sink });
                    break;
            }

            return true;
        }

        private static bool AppendFarnkeshDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsFarnkeshDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.RopeBridgeRun:
                    OverworldDecorKit.RopeBridgeRun(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.PaperStreamers:
                    OverworldDecorKit.PaperStreamers(b, at, new Vector3(prop.End.x, g.Sample(prop.End), prop.End.y), prop.Variant);
                    return true;
                case OverworldDecor.CloudPuff:
                    b.Stamp(OverworldDecorKit.CloudStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.GoldPebbles:
                    b.Stamp(OverworldDecorKit.GoldPebbleStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.SandstonePinnacle: OverworldDecorKit.SandstonePinnacle(b, s, prop.Variant); break;
                case OverworldDecor.Hermitage: OverworldDecorKit.Hermitage(b, s, prop.Variant); break;
                case OverworldDecor.WindBell: OverworldDecorKit.WindBell(b, s, prop.Variant); break;
                case OverworldDecor.RopeBasket: OverworldDecorKit.RopeBasket(b, s, prop.Variant); break;
                case OverworldDecor.GrapeVine: OverworldDecorKit.GrapeVine(b, s, prop.Variant); break;
                case OverworldDecor.GourdHut: OverworldDecorKit.GourdHut(b, s, prop.Variant); break;
                case OverworldDecor.StoneStage: OverworldDecorKit.StoneStage(b, s, prop.Variant); break;
                case OverworldDecor.ShelterStone: OverworldDecorKit.ShelterStone(b, s, prop.Variant); break;
                case OverworldDecor.TwistedTree: OverworldDecorKit.TwistedTree(b, s, prop.Variant); break;
                case OverworldDecor.GoldenNest: OverworldDecorKit.GoldenNest(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
