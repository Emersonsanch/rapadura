using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 12 — Arquipélago de Hinokura: the volcanic islands' pieces of the chunk builder. Before the noise scatter, each chunk
    /// gets the big pieces of its areas (stone lanterns and lava boulders round the forge isle and the lava channels, pumice on the
    /// black beach, fumaroles on the coast and the springs, cloud pines, lanterns and stone lions in the gardens, lacquer gates by
    /// the red bridges, broom, stone roses and canary pines on Echeyde, lava boulders in the malpaís, obsidian and ember coral on the
    /// reef, gates and furnace towers in the castle, slag heaps, cinder and black glass in the caldera), then the ground detail of
    /// its own (hot pools, raked gravel, ash drifts, glowing lava cracks, steam banks); the shared scatter fills round them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Hinokura terrain (Região 12, 220–239).</summary>
        public static bool IsHinokura(OverworldTerrain t) => OverworldTerrains.IsHinokura(t);

        private static bool IsHinokuraDecor(OverworldDecor decor) => OverworldDecorKit.IsHinokuraDecor(decor);

        /// <summary>Room (m) a scattered Hinokura piece keeps from the others.</summary>
        private static float HinokuraClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.LavaBoulder: return 1.4f;
                case OverworldDecor.PumiceRock: return 1.2f;
                case OverworldDecor.StoneLantern: return 0.7f;
                case OverworldDecor.CloudPine: return 2f;
                case OverworldDecor.LacquerGate: return 2.6f;
                case OverworldDecor.Fumarole: return 1.6f;
                case OverworldDecor.ObsidianShard: return 1.2f;
                case OverworldDecor.EmberCoral: return 1f;
                case OverworldDecor.BroomBush: return 1.2f;
                case OverworldDecor.StoneRose: return 1f;
                case OverworldDecor.CanaryPine: return 2f;
                case OverworldDecor.FurnaceTower: return 2.2f;
                case OverworldDecor.SlagHeap: return 1.8f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] HinokuraPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.ForgeIsle:
                    return new[] { new BigPiece(OverworldDecor.StoneLantern, 0.25f, 0.8f, false), new BigPiece(OverworldDecor.LavaBoulder, 0.2f, 1.5f, false), new BigPiece(OverworldDecor.CloudPine, 0.1f, 2.2f, false) };
                case OverworldTerrain.BlackSandBeach:
                    return new[] { new BigPiece(OverworldDecor.PumiceRock, 0.3f, 1.3f, false), new BigPiece(OverworldDecor.LavaBoulder, 0.15f, 1.5f, false) };
                case OverworldTerrain.ThawStrait:
                    return new[] { new BigPiece(OverworldDecor.IceBlock, 0.3f, 1.6f, false), new BigPiece(OverworldDecor.Fumarole, 0.15f, 1.7f, false) };
                case OverworldTerrain.LavaChannels:
                    return new[] { new BigPiece(OverworldDecor.LavaBoulder, 0.35f, 1.5f, false), new BigPiece(OverworldDecor.StoneLantern, 0.2f, 0.8f, false) };
                case OverworldTerrain.HotSprings:
                    return new[] { new BigPiece(OverworldDecor.Fumarole, 0.3f, 1.7f, false), new BigPiece(OverworldDecor.CloudPine, 0.15f, 2.2f, false), new BigPiece(OverworldDecor.PumiceRock, 0.1f, 1.3f, false) };
                case OverworldTerrain.FumaroleCoast:
                    return new[] { new BigPiece(OverworldDecor.Fumarole, 0.4f, 1.7f, false), new BigPiece(OverworldDecor.LavaBoulder, 0.15f, 1.5f, false) };
                case OverworldTerrain.LavaGardens:
                    return new[] { new BigPiece(OverworldDecor.CloudPine, 0.3f, 2.2f, false), new BigPiece(OverworldDecor.StoneLantern, 0.25f, 0.8f, false), new BigPiece(OverworldDecor.StoneLion, 0.1f, 1.6f, false) };
                case OverworldTerrain.CloudPines:
                    return new[] { new BigPiece(OverworldDecor.CloudPine, 0.45f, 2.2f, false), new BigPiece(OverworldDecor.StoneLantern, 0.1f, 0.8f, false) };
                case OverworldTerrain.RedBridges:
                    return new[] { new BigPiece(OverworldDecor.LacquerGate, 0.2f, 2.8f, true), new BigPiece(OverworldDecor.StoneLantern, 0.25f, 0.8f, false), new BigPiece(OverworldDecor.CloudPine, 0.15f, 2.2f, false) };
                case OverworldTerrain.BroomFields:
                    return new[] { new BigPiece(OverworldDecor.BroomBush, 0.35f, 1.3f, false), new BigPiece(OverworldDecor.CanaryPine, 0.2f, 2.2f, false), new BigPiece(OverworldDecor.StoneRose, 0.15f, 1.1f, false) };
                case OverworldTerrain.EcheydeCone:
                    return new[] { new BigPiece(OverworldDecor.PumiceRock, 0.3f, 1.3f, false), new BigPiece(OverworldDecor.StoneRose, 0.2f, 1.1f, false), new BigPiece(OverworldDecor.Fumarole, 0.1f, 1.7f, false) };
                case OverworldTerrain.Malpais:
                    return new[] { new BigPiece(OverworldDecor.LavaBoulder, 0.4f, 1.5f, false), new BigPiece(OverworldDecor.Fumarole, 0.15f, 1.7f, false) };
                case OverworldTerrain.HotChannels:
                    return new[] { new BigPiece(OverworldDecor.ObsidianShard, 0.3f, 1.3f, false), new BigPiece(OverworldDecor.EmberCoral, 0.25f, 1.1f, false) };
                case OverworldTerrain.ObsidianReef:
                    return new[] { new BigPiece(OverworldDecor.ObsidianShard, 0.45f, 1.3f, false), new BigPiece(OverworldDecor.EmberCoral, 0.2f, 1.1f, false) };
                case OverworldTerrain.GlassIslets:
                    return new[] { new BigPiece(OverworldDecor.EmberCoral, 0.3f, 1.1f, false), new BigPiece(OverworldDecor.ObsidianShard, 0.2f, 1.3f, false) };
                case OverworldTerrain.CurvedWalls:
                    return new[] { new BigPiece(OverworldDecor.LacquerGate, 0.15f, 2.8f, true), new BigPiece(OverworldDecor.StoneLantern, 0.2f, 0.8f, false), new BigPiece(OverworldDecor.CloudPine, 0.15f, 2.2f, false) };
                case OverworldTerrain.NineFurnaces:
                    return new[] { new BigPiece(OverworldDecor.FurnaceTower, 0.3f, 2.4f, true), new BigPiece(OverworldDecor.SlagHeap, 0.2f, 2f, false), new BigPiece(OverworldDecor.LacquerGate, 0.1f, 2.8f, true) };
                case OverworldTerrain.SlagFields:
                    return new[] { new BigPiece(OverworldDecor.SlagHeap, 0.4f, 2f, false), new BigPiece(OverworldDecor.FurnaceTower, 0.1f, 2.4f, true) };
                case OverworldTerrain.AshPlain:
                    return new[] { new BigPiece(OverworldDecor.SlagHeap, 0.2f, 2f, false), new BigPiece(OverworldDecor.LavaBoulder, 0.15f, 1.5f, false) };
                case OverworldTerrain.Caldera:
                    return new[] { new BigPiece(OverworldDecor.LavaBoulder, 0.3f, 1.5f, false), new BigPiece(OverworldDecor.ObsidianShard, 0.25f, 1.3f, false) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterHinokura(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsHinokura(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "hk" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsHinokura(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = HinokuraPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.CanaryPine || piece.Decor == OverworldDecor.FurnaceTower ? 0.9f + 0.4f * s01 : 0.85f + 0.3f * s01;
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

            ScatterHinokuraDetail(layout, content, salt);
        }

        /// <summary>Hot pools, raked gravel, ash drifts, glowing lava cracks and steam banks (ground detail: no colliders).</summary>
        private static void ScatterHinokuraDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsHinokura(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                switch (info.Terrain)
                {
                    case OverworldTerrain.ForgeIsle: decor = OverworldDecor.LavaCrack; chance = 0.08f; break;
                    case OverworldTerrain.BlackSandBeach: decor = OverworldDecor.AshDrift; chance = 0.06f; break;
                    case OverworldTerrain.ThawStrait: decor = OverworldDecor.MistBank; chance = 0.06f; break;
                    case OverworldTerrain.LavaChannels: decor = OverworldDecor.LavaCrack; chance = 0.25f; break;
                    case OverworldTerrain.HotSprings: decor = OverworldDecor.HotPool; chance = 0.25f; break;
                    case OverworldTerrain.FumaroleCoast: decor = OverworldDecor.MistBank; chance = 0.05f; break;
                    case OverworldTerrain.LavaGardens: decor = OverworldDecor.RakedGravel; chance = 0.3f; break;
                    case OverworldTerrain.CloudPines: decor = OverworldDecor.RakedGravel; chance = 0.06f; break;
                    case OverworldTerrain.RedBridges: decor = OverworldDecor.HotPool; chance = 0.08f; break;
                    case OverworldTerrain.BroomFields: decor = OverworldDecor.AshDrift; chance = 0.06f; break;
                    case OverworldTerrain.EcheydeCone: decor = OverworldDecor.AshDrift; chance = 0.15f; break;
                    case OverworldTerrain.Malpais: decor = OverworldDecor.LavaCrack; chance = 0.3f; break;
                    case OverworldTerrain.HotChannels: decor = OverworldDecor.MistBank; chance = 0.06f; break;
                    case OverworldTerrain.ObsidianReef: decor = OverworldDecor.MistBank; chance = 0.04f; break;
                    case OverworldTerrain.GlassIslets: decor = OverworldDecor.HotPool; chance = 0.1f; break;
                    case OverworldTerrain.CurvedWalls: decor = OverworldDecor.RakedGravel; chance = 0.08f; break;
                    case OverworldTerrain.NineFurnaces: decor = OverworldDecor.LavaCrack; chance = 0.1f; break;
                    case OverworldTerrain.SlagFields: decor = OverworldDecor.AshDrift; chance = 0.2f; break;
                    case OverworldTerrain.AshPlain: decor = OverworldDecor.AshDrift; chance = 0.35f; break;
                    case OverworldTerrain.Caldera: decor = OverworldDecor.LavaCrack; chance = 0.3f; break;
                    default: continue;
                }

                float room = decor == OverworldDecor.MistBank ? 2.5f : decor == OverworldDecor.HotPool ? 1.6f : decor == OverworldDecor.RakedGravel ? 1.8f : decor == OverworldDecor.LavaCrack ? 1.7f : 1.4f;
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

        private static bool AddHinokuraDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsHinokuraDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.LavaBoulder:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.05f * s, Height = 1.3f * s + sink });
                    break;
                case OverworldDecor.PumiceRock:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 1.1f * s + sink });
                    break;
                case OverworldDecor.StoneLantern:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.45f * s, Height = 2f * s + sink });
                    break;
                case OverworldDecor.CloudPine:
                case OverworldDecor.CanaryPine:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.5f * s, Height = 3f * s + sink });
                    break;
                case OverworldDecor.LacquerGate:
                    // Only the two posts: the gate is walked through.
                    Vector2 side = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad)) * 1.6f * s;
                    content.Solids.Add(new OverworldSolid { Center = p + side, Base = ground, Radius = 0.3f * s, Height = 2.9f * s + sink });
                    content.Solids.Add(new OverworldSolid { Center = p - side, Base = ground, Radius = 0.3f * s, Height = 2.9f * s + sink });
                    break;
                case OverworldDecor.Fumarole:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.2f * s, Height = 1.1f * s + sink });
                    break;
                case OverworldDecor.ObsidianShard:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 1.8f * s + sink });
                    break;
                case OverworldDecor.EmberCoral:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 1f * s + sink });
                    break;
                case OverworldDecor.BroomBush:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 1.2f * s + sink });
                    break;
                case OverworldDecor.StoneRose:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.8f * s, Height = 0.8f * s + sink });
                    break;
                case OverworldDecor.FurnaceTower:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.8f * s, Height = 5.6f * s + sink });
                    break;
                case OverworldDecor.SlagHeap:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.4f * s, Height = 1.1f * s + sink });
                    break;
            }

            return true;
        }

        private static bool AppendHinokuraDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsHinokuraDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.HotPool:
                    b.Stamp(OverworldDecorKit.HotPoolStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.RakedGravel:
                    b.Stamp(OverworldDecorKit.RakedGravelStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.AshDrift:
                    b.Stamp(OverworldDecorKit.AshDriftStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.LavaCrack:
                    b.Stamp(OverworldDecorKit.LavaCrackStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.LavaBoulder: OverworldDecorKit.LavaBoulder(b, s, prop.Variant); break;
                case OverworldDecor.PumiceRock: OverworldDecorKit.PumiceRock(b, s, prop.Variant); break;
                case OverworldDecor.StoneLantern: OverworldDecorKit.StoneLantern(b, s, prop.Variant); break;
                case OverworldDecor.CloudPine: OverworldDecorKit.CloudPine(b, s, prop.Variant); break;
                case OverworldDecor.LacquerGate: OverworldDecorKit.LacquerGate(b, s, prop.Variant); break;
                case OverworldDecor.Fumarole: OverworldDecorKit.Fumarole(b, s, prop.Variant); break;
                case OverworldDecor.ObsidianShard: OverworldDecorKit.ObsidianShard(b, s, prop.Variant); break;
                case OverworldDecor.EmberCoral: OverworldDecorKit.EmberCoral(b, s, prop.Variant); break;
                case OverworldDecor.BroomBush: OverworldDecorKit.BroomBush(b, s, prop.Variant); break;
                case OverworldDecor.StoneRose: OverworldDecorKit.StoneRose(b, s, prop.Variant); break;
                case OverworldDecor.CanaryPine: OverworldDecorKit.CanaryPine(b, s, prop.Variant); break;
                case OverworldDecor.FurnaceTower: OverworldDecorKit.FurnaceTower(b, s, prop.Variant); break;
                case OverworldDecor.SlagHeap: OverworldDecorKit.SlagHeap(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
