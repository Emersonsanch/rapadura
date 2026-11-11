using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 15 — Limiar Profundo: the pits' pieces of the chunk builder. Before the noise scatter, each chunk gets the fixed pieces of the sheets
    /// that stand in it (the ten pylons of the Bridge of the Bōlos, the dark globe, the five lamp pedestals, the Gate of the Land of Darkness), then
    /// the big pieces of its areas, then the ground detail of its own; the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Limiar Profundo terrain (Região 15, 280–309).</summary>
        public static bool IsProfundo(OverworldTerrain t) => OverworldTerrains.IsProfundo(t);

        private static bool IsProfundoDecor(OverworldDecor decor) => OverworldDecorKit.IsProfundoDecor(decor);

        /// <summary>Room (m) a scattered Profundo piece keeps from the others.</summary>
        private static float ProfundoClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.LampNicheWall: return 2.2f;
                case OverworldDecor.WickRack: return 1.4f;
                case OverworldDecor.OilCistern: return 1.6f;
                case OverworldDecor.ArtificerVent: return 1.6f;
                case OverworldDecor.SmokeColumn: return 1.6f;
                case OverworldDecor.EmberStone: return 1.6f;
                case OverworldDecor.CinderPile: return 1.4f;
                case OverworldDecor.WindSpire: return 1.2f;
                case OverworldDecor.FloatStone: return 1.4f;
                case OverworldDecor.SluiceGate: return 1.8f;
                case OverworldDecor.DrownedColumn: return 1f;
                case OverworldDecor.BlindStalagmite: return 1.2f;
                case OverworldDecor.BridgePylon: return 0.8f;
                case OverworldDecor.NoveloGlobe: return 9f;
                case OverworldDecor.LampPedestal: return 1f;
                case OverworldDecor.SealSlab: return 1.8f;
                case OverworldDecor.DarkGate: return 18f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] ProfundoPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.LampFortress: return new[] { new BigPiece(OverworldDecor.LampNicheWall, 0.25f, 2.2f, true), new BigPiece(OverworldDecor.WickRack, 0.15f, 1.4f, false) };
                case OverworldTerrain.RampartArc: return new[] { new BigPiece(OverworldDecor.LampNicheWall, 0.3f, 2.2f, true), new BigPiece(OverworldDecor.Basalt, 0.2f, 1.4f, false) };
                case OverworldTerrain.BasaltNiches: return new[] { new BigPiece(OverworldDecor.LampNicheWall, 0.5f, 2.2f, true) };
                case OverworldTerrain.OilCisterns: return new[] { new BigPiece(OverworldDecor.OilCistern, 0.4f, 1.6f, true), new BigPiece(OverworldDecor.WickRack, 0.2f, 1.4f, false) };
                case OverworldTerrain.WickYards: return new[] { new BigPiece(OverworldDecor.WickRack, 0.45f, 1.4f, false), new BigPiece(OverworldDecor.OilCistern, 0.15f, 1.6f, true) };
                case OverworldTerrain.AbyssBrink: return new[] { new BigPiece(OverworldDecor.SealSlab, 0.3f, 1.8f, true) };
                case OverworldTerrain.SmokePit: return new[] { new BigPiece(OverworldDecor.SmokeColumn, 0.4f, 1.6f, false), new BigPiece(OverworldDecor.ArtificerVent, 0.15f, 1.6f, true) };
                case OverworldTerrain.ArtificerVents: return new[] { new BigPiece(OverworldDecor.ArtificerVent, 0.45f, 1.6f, true), new BigPiece(OverworldDecor.SmokeColumn, 0.15f, 1.6f, false) };
                case OverworldTerrain.CinderCorridors: return new[] { new BigPiece(OverworldDecor.CinderPile, 0.4f, 1.4f, false), new BigPiece(OverworldDecor.SmokeColumn, 0.2f, 1.6f, false) };
                case OverworldTerrain.DevouringFirePit: return new[] { new BigPiece(OverworldDecor.EmberStone, 0.45f, 1.6f, true), new BigPiece(OverworldDecor.CinderPile, 0.2f, 1.4f, false) };
                case OverworldTerrain.GlowPatternFloor: return new[] { new BigPiece(OverworldDecor.EmberStone, 0.65f, 1.6f, true) };
                case OverworldTerrain.HotAshHalls: return new[] { new BigPiece(OverworldDecor.CinderPile, 0.5f, 1.4f, false), new BigPiece(OverworldDecor.Basalt, 0.15f, 1.4f, false) };
                case OverworldTerrain.WindShaft: return new[] { new BigPiece(OverworldDecor.WindSpire, 0.35f, 1.2f, false), new BigPiece(OverworldDecor.FloatStone, 0.2f, 1.4f, false) };
                case OverworldTerrain.LoosePlatforms: return new[] { new BigPiece(OverworldDecor.FloatStone, 0.45f, 1.4f, false) };
                case OverworldTerrain.Updrafts: return new[] { new BigPiece(OverworldDecor.WindSpire, 0.45f, 1.2f, false), new BigPiece(OverworldDecor.FloatStone, 0.15f, 1.4f, false) };
                case OverworldTerrain.MirePit: return new[] { new BigPiece(OverworldDecor.DrownedColumn, 0.3f, 1f, false), new BigPiece(OverworldDecor.SluiceGate, 0.2f, 1.8f, true) };
                case OverworldTerrain.FloodedHalls: return new[] { new BigPiece(OverworldDecor.DrownedColumn, 0.5f, 1f, false) };
                case OverworldTerrain.Sluices: return new[] { new BigPiece(OverworldDecor.SluiceGate, 0.45f, 1.8f, true), new BigPiece(OverworldDecor.DrownedColumn, 0.15f, 1f, false) };
                case OverworldTerrain.DarknessPit: return new[] { new BigPiece(OverworldDecor.BlindStalagmite, 0.5f, 1.2f, false), new BigPiece(OverworldDecor.Basalt, 0.1f, 1.4f, false) };
                case OverworldTerrain.BlindTunnels: return new[] { new BigPiece(OverworldDecor.BlindStalagmite, 0.6f, 1.2f, false) };
                case OverworldTerrain.BridgeHead: return new[] { new BigPiece(OverworldDecor.SealSlab, 0.3f, 1.8f, true), new BigPiece(OverworldDecor.Basalt, 0.15f, 1.4f, false) };
                case OverworldTerrain.NoveloBridge: return new[] { new BigPiece(OverworldDecor.SealSlab, 0.2f, 1.8f, true) };
                case OverworldTerrain.NoveloGlobeFlights: return new[] { new BigPiece(OverworldDecor.BlindStalagmite, 0.25f, 1.2f, false) };
                case OverworldTerrain.PedestalPlaza: return new[] { new BigPiece(OverworldDecor.SealSlab, 0.3f, 1.8f, true) };
                case OverworldTerrain.DarkGateGround: return new[] { new BigPiece(OverworldDecor.SealSlab, 0.4f, 1.8f, true), new BigPiece(OverworldDecor.BlindStalagmite, 0.2f, 1.2f, false) };
                default: return NoPieces;
            }
        }

        /// <summary>The ground details a terrain strews: its map's mark and one or two more, so no two areas look alike (Dressing test).</summary>
        private static OverworldDecor[] ProfundoDetailsOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                // 83: amber spills, with ash / copper / wick in six distinct combinations.
                case OverworldTerrain.LampFortress: return new[] { OverworldDecor.LampEmber, OverworldDecor.AshSpeck };
                case OverworldTerrain.RampartArc: return new[] { OverworldDecor.LampEmber, OverworldDecor.CopperFiling };
                case OverworldTerrain.BasaltNiches: return new[] { OverworldDecor.LampEmber, OverworldDecor.WickFiber };
                case OverworldTerrain.OilCisterns: return new[] { OverworldDecor.LampEmber, OverworldDecor.AshSpeck, OverworldDecor.CopperFiling };
                case OverworldTerrain.WickYards: return new[] { OverworldDecor.LampEmber, OverworldDecor.AshSpeck, OverworldDecor.WickFiber };
                case OverworldTerrain.AbyssBrink: return new[] { OverworldDecor.LampEmber, OverworldDecor.CopperFiling, OverworldDecor.WickFiber };
                // 84: soot.
                case OverworldTerrain.SmokePit: return new[] { OverworldDecor.SootPatch, OverworldDecor.AshSpeck };
                case OverworldTerrain.ArtificerVents: return new[] { OverworldDecor.SootPatch, OverworldDecor.CopperFiling };
                case OverworldTerrain.CinderCorridors: return new[] { OverworldDecor.SootPatch, OverworldDecor.WickFiber };
                // 85: cracks of fire.
                case OverworldTerrain.DevouringFirePit: return new[] { OverworldDecor.EmberCrack, OverworldDecor.AshSpeck };
                case OverworldTerrain.GlowPatternFloor: return new[] { OverworldDecor.EmberCrack, OverworldDecor.CopperFiling };
                case OverworldTerrain.HotAshHalls: return new[] { OverworldDecor.EmberCrack, OverworldDecor.WickFiber };
                // 86: streaks of wind.
                case OverworldTerrain.WindShaft: return new[] { OverworldDecor.WindStreak, OverworldDecor.AshSpeck };
                case OverworldTerrain.LoosePlatforms: return new[] { OverworldDecor.WindStreak, OverworldDecor.CopperFiling };
                case OverworldTerrain.Updrafts: return new[] { OverworldDecor.WindStreak, OverworldDecor.WickFiber };
                // 87: rings of sludge.
                case OverworldTerrain.MirePit: return new[] { OverworldDecor.SludgeRing, OverworldDecor.AshSpeck };
                case OverworldTerrain.FloodedHalls: return new[] { OverworldDecor.SludgeRing, OverworldDecor.CopperFiling };
                case OverworldTerrain.Sluices: return new[] { OverworldDecor.SludgeRing, OverworldDecor.WickFiber };
                // 88: pools of shadow.
                case OverworldTerrain.DarknessPit: return new[] { OverworldDecor.ShadowPool, OverworldDecor.AshSpeck };
                case OverworldTerrain.BlindTunnels: return new[] { OverworldDecor.ShadowPool, OverworldDecor.CopperFiling };
                // 89: threads of light.
                case OverworldTerrain.BridgeHead: return new[] { OverworldDecor.LightThread, OverworldDecor.AshSpeck };
                case OverworldTerrain.NoveloBridge: return new[] { OverworldDecor.LightThread, OverworldDecor.CopperFiling };
                case OverworldTerrain.NoveloGlobeFlights: return new[] { OverworldDecor.LightThread, OverworldDecor.WickFiber };
                // 90: runes of the seal.
                case OverworldTerrain.PedestalPlaza: return new[] { OverworldDecor.SealRune, OverworldDecor.AshSpeck };
                case OverworldTerrain.DarkGateGround: return new[] { OverworldDecor.SealRune, OverworldDecor.CopperFiling };
                default: return null;
            }
        }

        private static void ScatterProfundo(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsProfundo(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "profundo" + content.Coord.x + "_" + content.Coord.y;
            PlaceProfundoFixedPieces(layout, content);
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsProfundo(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = ProfundoPiecesOf(terrain);
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
                        AddProfundoProp(layout, content, piece.Decor, p, TownStyle.Hash01(salt, k + 400) * 360f, scale, (int)(TownStyle.Hash(salt, k + 500) & 7), radius, terrain);
                    }

                    break;
                }
            }

            ScatterProfundoDetail(layout, content, salt);
        }

        /// <summary>The fixed pieces of the sheets that fall in this chunk.</summary>
        private static void PlaceProfundoFixedPieces(OverworldLayout layout, OverworldChunkContent content)
        {
            Vector2 offset = layout.PartAt(content.Rect.center)?.Offset ?? Vector2.zero;
            for (int i = 0; i < OverworldLayouts.NoveloFlightCount; i++)
            {
                Vector2 p = OverworldLayouts.NoveloPylonPosition(i) + offset;
                if (content.Rect.Contains(p))
                {
                    TryPlaceFixedProfundo(layout, content, OverworldDecor.BridgePylon, p, 0f, 1f, i, 0.8f);
                }
            }

            for (int i = 0; i < OverworldLayouts.LampPedestalCount; i++)
            {
                Vector2 p = OverworldLayouts.LampPedestalPosition(i) + offset;
                if (content.Rect.Contains(p))
                {
                    TryPlaceFixedProfundo(layout, content, OverworldDecor.LampPedestal, p, 72f * i, 1.1f, i, 1f);
                }
            }

            for (int i = 0; i < OverworldLayouts.WallLamps.Length; i++)
            {
                Vector2 p = OverworldLayouts.WallLamps[i] + offset;
                if (content.Rect.Contains(p))
                {
                    TryPlaceFixedProfundo(layout, content, OverworldDecor.LampPedestal, p, 90f * i, 1.1f, i, 1f);
                }
            }

            foreach (Mechanics.ProfundoSmokeBank bank in OverworldLayouts.SmokeBanks)
            {
                Vector2 p = bank.Vent + offset;
                if (content.Rect.Contains(p))
                {
                    TryPlaceFixedProfundo(layout, content, OverworldDecor.ArtificerVent, p, 0f, 1f, 0, 1.6f);
                }
            }

            Vector2 globe = OverworldLayouts.NoveloGlobeCenter + offset;
            if (content.Rect.Contains(globe))
            {
                TryPlaceFixedProfundo(layout, content, OverworldDecor.NoveloGlobe, globe, 0f, 1f, 0, 9f);
            }

            Vector2 gate = OverworldLayouts.DarkGateCenter + offset;
            if (content.Rect.Contains(gate))
            {
                TryPlaceFixedProfundo(layout, content, OverworldDecor.DarkGate, gate, 0f, 1f, 0, 17f);
            }
        }

        private static void TryPlaceFixedProfundo(OverworldLayout layout, OverworldChunkContent content, OverworldDecor decor, Vector2 p, float yaw, float scale, int variant, float radius)
        {
            OverworldTerrain terrain = content.Grid.Near(p).Terrain;
            if (!IsProfundo(terrain) || DistanceToWall(layout, p) < EdgeBelt || !IsFree(layout, content, p, radius + 0.5f))
            {
                return;
            }

            AddProfundoProp(layout, content, decor, p, yaw, scale, variant, radius, terrain);
        }

        private static void AddProfundoProp(OverworldLayout layout, OverworldChunkContent content, OverworldDecor decor, Vector2 p, float yaw, float scale, int variant, float radius, OverworldTerrain terrain)
        {
            var prop = new OverworldProp { Kind = CityPropKind.Rock, Decor = decor, Position = p, Yaw = yaw, Scale = scale, Variant = variant, Color = RockColor(terrain) };
            content.Props.Add(prop);
            AddSolid(layout, content, prop);
            content.Blocked.Add(new Rect(p.x - radius, p.y - radius, radius * 2f, radius * 2f));
        }

        /// <summary>Ground detail: each terrain strews its own set (no colliders).</summary>
        private static void ScatterProfundoDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsProfundo(info.Terrain))
                {
                    continue;
                }

                OverworldDecor[] details = ProfundoDetailsOf(info.Terrain);
                if (details == null || details.Length == 0 || TownStyle.Hash01(salt, k + 22000) >= 0.4f || info.Clearing < 0.5f || DistanceToWall(layout, p) < -10f)
                {
                    continue;
                }

                OverworldDecor decor = details[k % details.Length];
                float room = decor == OverworldDecor.WindStreak || decor == OverworldDecor.LightThread ? 1.8f : 1.1f;
                if (!IsOpen(layout, content, p, room, false))
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

        private static bool AddProfundoDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsProfundoDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.LampNicheWall:
                {
                    Vector2 along = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad));
                    foreach (float t in new[] { -1.2f, 0f, 1.2f })
                    {
                        Vector2 c = p + along * (t * s);
                        content.Solids.Add(new OverworldSolid { Center = c, Base = content.Grid.Sample(c) - 0.1f, Radius = 0.7f * s, Height = 2f * s + sink });
                    }

                    break;
                }
                case OverworldDecor.WickRack:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.5f * s, Height = 2f * s + sink });
                    break;
                case OverworldDecor.OilCistern:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.2f * s, Height = 1.3f * s + sink });
                    break;
                case OverworldDecor.ArtificerVent:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.2f * s, Height = 1.5f * s + sink });
                    break;
                case OverworldDecor.SmokeColumn:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 4.5f * s + sink });
                    break;
                case OverworldDecor.EmberStone:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.2f * s, Height = 0.45f * s + sink });
                    break;
                case OverworldDecor.CinderPile:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 0.85f * s + sink });
                    break;
                case OverworldDecor.WindSpire:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 4f * s + sink });
                    break;
                case OverworldDecor.FloatStone:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 3.3f * s + sink });
                    break;
                case OverworldDecor.SluiceGate:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.4f * s, Height = 2.2f * s + sink });
                    break;
                case OverworldDecor.DrownedColumn:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 2.5f * s + sink });
                    break;
                case OverworldDecor.BlindStalagmite:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 3f * s + sink });
                    break;
                case OverworldDecor.BridgePylon:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.5f * s, Height = 3.5f * s + sink });
                    break;
                case OverworldDecor.NoveloGlobe:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 7f * s, Height = 12f * s + sink });
                    break;
                case OverworldDecor.LampPedestal:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 1.7f * s + sink });
                    break;
                case OverworldDecor.SealSlab:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.3f * s, Height = 0.6f * s + sink });
                    break;
                case OverworldDecor.DarkGate:
                {
                    Vector2 right = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad));
                    foreach (float t in new[] { -14f, 14f })
                    {
                        Vector2 c = p + right * (t * s);
                        content.Solids.Add(new OverworldSolid { Center = c, Base = content.Grid.Sample(c) - 0.1f, Radius = 2.4f * s, Height = 14f * s + sink });
                    }

                    foreach (float t in new[] { -8f, -3f, 3f, 8f })
                    {
                        Vector2 c = p + right * (t * s);
                        content.Solids.Add(new OverworldSolid { Center = c, Base = content.Grid.Sample(c) - 0.1f, Radius = 3f * s, Height = 12f * s + sink });
                    }

                    break;
                }
            }

            return true;
        }

        private static bool AppendProfundoDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsProfundoDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.LampEmber: b.Stamp(OverworldDecorKit.LampEmberStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.SootPatch: b.Stamp(OverworldDecorKit.SootPatchStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.EmberCrack: b.Stamp(OverworldDecorKit.EmberCrackStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.WindStreak: b.Stamp(OverworldDecorKit.WindStreakStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.SludgeRing: b.Stamp(OverworldDecorKit.SludgeRingStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.ShadowPool: b.Stamp(OverworldDecorKit.ShadowPoolStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.LightThread: b.Stamp(OverworldDecorKit.LightThreadStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.SealRune: b.Stamp(OverworldDecorKit.SealRuneStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.AshSpeck: b.Stamp(OverworldDecorKit.AshSpeckStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.CopperFiling: b.Stamp(OverworldDecorKit.CopperFilingStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
                case OverworldDecor.WickFiber: b.Stamp(OverworldDecorKit.WickFiberStamp, at, prop.Yaw, s, TownSwatch.Black); return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.LampNicheWall: OverworldDecorKit.LampNicheWall(b, s, prop.Variant); break;
                case OverworldDecor.WickRack: OverworldDecorKit.WickRack(b, s, prop.Variant); break;
                case OverworldDecor.OilCistern: OverworldDecorKit.OilCistern(b, s, prop.Variant); break;
                case OverworldDecor.ArtificerVent: OverworldDecorKit.ArtificerVent(b, s, prop.Variant); break;
                case OverworldDecor.SmokeColumn: OverworldDecorKit.SmokeColumn(b, s, prop.Variant); break;
                case OverworldDecor.EmberStone: OverworldDecorKit.EmberStone(b, s, prop.Variant); break;
                case OverworldDecor.CinderPile: OverworldDecorKit.CinderPile(b, s, prop.Variant); break;
                case OverworldDecor.WindSpire: OverworldDecorKit.WindSpire(b, s, prop.Variant); break;
                case OverworldDecor.FloatStone: OverworldDecorKit.FloatStone(b, s, prop.Variant); break;
                case OverworldDecor.SluiceGate: OverworldDecorKit.SluiceGate(b, s, prop.Variant); break;
                case OverworldDecor.DrownedColumn: OverworldDecorKit.DrownedColumn(b, s, prop.Variant); break;
                case OverworldDecor.BlindStalagmite: OverworldDecorKit.BlindStalagmite(b, s, prop.Variant); break;
                case OverworldDecor.BridgePylon: OverworldDecorKit.BridgePylon(b, s, prop.Variant); break;
                case OverworldDecor.NoveloGlobe: OverworldDecorKit.NoveloGlobe(b, s, prop.Variant); break;
                case OverworldDecor.LampPedestal: OverworldDecorKit.LampPedestal(b, s, prop.Variant); break;
                case OverworldDecor.SealSlab: OverworldDecorKit.SealSlab(b, s, prop.Variant); break;
                case OverworldDecor.DarkGate: OverworldDecorKit.DarkGate(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
