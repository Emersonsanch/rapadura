using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 11 — Mar Gelado: the frozen sea's pieces of the chunk builder. Before the noise scatter, each chunk gets the big
    /// pieces of its areas (snow domes, plank houses and ice lanterns round the harbour, whalebone arches and crescent boats on
    /// the shore, ice blocks and icebergs on the canals, the pack ice and the aurora sea, herders' tents, racks and lichen
    /// boulders on Omöl's tundra, mammoth tusks and frost crystals in the galleries and the frozen earth, the ice beacons of the
    /// lighthouse isle, the fishers' racks and boats on Old Kul's lake), then the ground detail of its own (fishing holes on the
    /// bay and the lake, cracks in the ice, tufts of reindeer lichen, snow drifts and mist banks); the shared scatter fills round
    /// them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Mar Gelado terrain (Região 11, 200–219).</summary>
        public static bool IsMarGelado(OverworldTerrain t) => OverworldTerrains.IsMarGelado(t);

        private static bool IsMarGeladoDecor(OverworldDecor decor) => OverworldDecorKit.IsMarGeladoDecor(decor);

        /// <summary>Room (m) a scattered Mar Gelado piece keeps from the others.</summary>
        private static float MarGeladoClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.SnowDome: return 2.6f;
                case OverworldDecor.PlankLonghouse: return 4.4f;
                case OverworldDecor.CrescentBoat: return 1.8f;
                case OverworldDecor.WhaleboneArch: return 2.2f;
                case OverworldDecor.IceBlock: return 1.4f;
                case OverworldDecor.Iceberg: return 3.4f;
                case OverworldDecor.HerderTent: return 2.6f;
                case OverworldDecor.LichenBoulder: return 1.2f;
                case OverworldDecor.MammothTusk: return 1.6f;
                case OverworldDecor.IceLantern: return 0.6f;
                case OverworldDecor.FishRack: return 1.6f;
                case OverworldDecor.IceBeacon: return 2.2f;
                case OverworldDecor.FrostCrystal: return 1f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] MarGeladoPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.SnowShore:
                    return new[] { new BigPiece(OverworldDecor.WhaleboneArch, 0.12f, 2.4f, true), new BigPiece(OverworldDecor.CrescentBoat, 0.2f, 2f, false), new BigPiece(OverworldDecor.LichenBoulder, 0.12f, 1.3f, false) };
                case OverworldTerrain.MoonHarbor:
                    return new[] { new BigPiece(OverworldDecor.SnowDome, 0.25f, 2.8f, true), new BigPiece(OverworldDecor.PlankLonghouse, 0.12f, 4.6f, true), new BigPiece(OverworldDecor.IceLantern, 0.2f, 0.7f, false), new BigPiece(OverworldDecor.CrescentBoat, 0.1f, 2f, false) };
                case OverworldTerrain.FrozenBay:
                    return new[] { new BigPiece(OverworldDecor.CrescentBoat, 0.2f, 2f, false), new BigPiece(OverworldDecor.IceBlock, 0.2f, 1.6f, false), new BigPiece(OverworldDecor.SnowDome, 0.05f, 2.8f, true) };
                case OverworldTerrain.IceCanals:
                    return new[] { new BigPiece(OverworldDecor.IceBlock, 0.4f, 1.6f, false), new BigPiece(OverworldDecor.IceLantern, 0.14f, 0.7f, false), new BigPiece(OverworldDecor.SnowDome, 0.06f, 2.8f, true) };
                case OverworldTerrain.CrackedIce:
                    return new[] { new BigPiece(OverworldDecor.IceBlock, 0.45f, 1.6f, false), new BigPiece(OverworldDecor.Iceberg, 0.1f, 3.6f, true) };
                case OverworldTerrain.Tundra:
                    return new[] { new BigPiece(OverworldDecor.LichenBoulder, 0.35f, 1.3f, false), new BigPiece(OverworldDecor.HerderTent, 0.05f, 2.8f, true) };
                case OverworldTerrain.TundraLakes:
                    return new[] { new BigPiece(OverworldDecor.LichenBoulder, 0.3f, 1.3f, false), new BigPiece(OverworldDecor.FishRack, 0.12f, 1.8f, false) };
                case OverworldTerrain.ReindeerPasture:
                    return new[] { new BigPiece(OverworldDecor.HerderTent, 0.25f, 2.8f, true), new BigPiece(OverworldDecor.FishRack, 0.1f, 1.8f, false), new BigPiece(OverworldDecor.LichenBoulder, 0.15f, 1.3f, false) };
                case OverworldTerrain.Fjord:
                    return new[] { new BigPiece(OverworldDecor.Iceberg, 0.2f, 3.6f, true), new BigPiece(OverworldDecor.IceBlock, 0.2f, 1.6f, false), new BigPiece(OverworldDecor.MountainPine, 0.15f, 2f, false) };
                case OverworldTerrain.MammothGalleries:
                    return new[] { new BigPiece(OverworldDecor.MammothTusk, 0.35f, 1.8f, false), new BigPiece(OverworldDecor.FrostCrystal, 0.3f, 1.1f, false) };
                case OverworldTerrain.FrozenEarth:
                    return new[] { new BigPiece(OverworldDecor.MammothTusk, 0.3f, 1.8f, false), new BigPiece(OverworldDecor.MountainPine, 0.15f, 2f, false), new BigPiece(OverworldDecor.LichenBoulder, 0.1f, 1.3f, false) };
                case OverworldTerrain.PackIce:
                    return new[] { new BigPiece(OverworldDecor.IceBlock, 0.3f, 1.6f, false), new BigPiece(OverworldDecor.Iceberg, 0.15f, 3.6f, true) };
                case OverworldTerrain.AuroraSea:
                    return new[] { new BigPiece(OverworldDecor.Iceberg, 0.3f, 3.6f, true), new BigPiece(OverworldDecor.IceBlock, 0.15f, 1.6f, false) };
                case OverworldTerrain.LighthouseIsle:
                    return new[] { new BigPiece(OverworldDecor.IceBeacon, 0.2f, 2.4f, true), new BigPiece(OverworldDecor.IceLantern, 0.2f, 0.7f, false), new BigPiece(OverworldDecor.LichenBoulder, 0.12f, 1.3f, false) };
                case OverworldTerrain.FrozenLake:
                    return new[] { new BigPiece(OverworldDecor.FishRack, 0.25f, 1.8f, false), new BigPiece(OverworldDecor.CrescentBoat, 0.15f, 2f, false), new BigPiece(OverworldDecor.SnowDome, 0.06f, 2.8f, true) };
                case OverworldTerrain.BlackIce:
                    return new[] { new BigPiece(OverworldDecor.FrostCrystal, 0.25f, 1.1f, false), new BigPiece(OverworldDecor.IceBlock, 0.25f, 1.6f, false) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterMarGelado(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsMarGelado(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "mg" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsMarGelado(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = MarGeladoPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.Iceberg || piece.Decor == OverworldDecor.MammothTusk ? 0.9f + 0.4f * s01 : 0.85f + 0.3f * s01;
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

            ScatterMarGeladoDetail(layout, content, salt);
        }

        /// <summary>Fishing holes, cracks in the ice, lichen tufts, snow drifts and mist banks (ground detail: no colliders).</summary>
        private static void ScatterMarGeladoDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                if (!IsMarGelado(info.Terrain))
                {
                    continue;
                }

                OverworldDecor decor;
                float chance;
                switch (info.Terrain)
                {
                    case OverworldTerrain.SnowShore: decor = OverworldDecor.SnowDrift; chance = 0.12f; break;
                    case OverworldTerrain.MoonHarbor: decor = OverworldDecor.FishingHole; chance = 0.06f; break;
                    case OverworldTerrain.FrozenBay: decor = OverworldDecor.FishingHole; chance = 0.14f; break;
                    case OverworldTerrain.IceCanals: decor = OverworldDecor.IceCrack; chance = 0.2f; break;
                    case OverworldTerrain.CrackedIce: decor = OverworldDecor.IceCrack; chance = 0.35f; break;
                    case OverworldTerrain.Tundra: decor = OverworldDecor.LichenTuft; chance = 0.35f; break;
                    case OverworldTerrain.TundraLakes: decor = OverworldDecor.LichenTuft; chance = 0.25f; break;
                    case OverworldTerrain.ReindeerPasture: decor = OverworldDecor.LichenTuft; chance = 0.3f; break;
                    case OverworldTerrain.Fjord: decor = OverworldDecor.MistBank; chance = 0.04f; break;
                    case OverworldTerrain.MammothGalleries: decor = OverworldDecor.IceShards; chance = 0.12f; break;
                    case OverworldTerrain.FrozenEarth: decor = OverworldDecor.SnowDrift; chance = 0.1f; break;
                    case OverworldTerrain.PackIce: decor = OverworldDecor.IceCrack; chance = 0.2f; break;
                    case OverworldTerrain.AuroraSea: decor = OverworldDecor.SnowDrift; chance = 0.08f; break;
                    case OverworldTerrain.LighthouseIsle: decor = OverworldDecor.SnowDrift; chance = 0.1f; break;
                    case OverworldTerrain.FrozenLake: decor = OverworldDecor.FishingHole; chance = 0.22f; break;
                    case OverworldTerrain.BlackIce: decor = OverworldDecor.IceCrack; chance = 0.3f; break;
                    default: continue;
                }

                float room = decor == OverworldDecor.MistBank ? 2.5f : decor == OverworldDecor.FishingHole ? 1.4f : decor == OverworldDecor.IceCrack ? 1.8f : decor == OverworldDecor.SnowDrift ? 1.2f : 0.6f;
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

        private static bool AddMarGeladoDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsMarGeladoDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.SnowDome:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2.1f * s, Height = 2f * s + sink });
                    break;
                case OverworldDecor.PlankLonghouse:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(5f * s, 8f * s), Height = 3.4f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.CrescentBoat:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.3f * s, 3.4f * s), Height = 0.8f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.WhaleboneArch:
                    // Only the two feet: the arch is walked under.
                    Vector2 side = new Vector2(Mathf.Cos(-prop.Yaw * Mathf.Deg2Rad), Mathf.Sin(-prop.Yaw * Mathf.Deg2Rad)) * 1.8f * s;
                    content.Solids.Add(new OverworldSolid { Center = p + side, Base = ground, Radius = 0.5f * s, Height = 2.2f * s + sink });
                    content.Solids.Add(new OverworldSolid { Center = p - side, Base = ground, Radius = 0.5f * s, Height = 2.2f * s + sink });
                    break;
                case OverworldDecor.IceBlock:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.2f * s, Height = 1.5f * s + sink });
                    break;
                case OverworldDecor.Iceberg:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 3f * s, Height = 5f * s + sink });
                    break;
                case OverworldDecor.HerderTent:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 2f * s, Height = 3.6f * s + sink });
                    break;
                case OverworldDecor.LichenBoulder:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1f * s, Height = 1.4f * s + sink });
                    break;
                case OverworldDecor.MammothTusk:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 3f * s + sink });
                    break;
                case OverworldDecor.IceLantern:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.3f * s, Height = 2.2f * s + sink });
                    break;
                case OverworldDecor.FishRack:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(3f * s, 0.3f * s), Height = 2.1f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.IceBeacon:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.8f * s, Height = 6.2f * s + sink });
                    break;
                case OverworldDecor.FrostCrystal:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.7f * s, Height = 1.6f * s + sink });
                    break;
            }

            return true;
        }

        private static bool AppendMarGeladoDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsMarGeladoDecor(prop.Decor))
            {
                return false;
            }

            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.FishingHole:
                    b.Stamp(OverworldDecorKit.FishingHoleStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.IceCrack:
                    b.Stamp(OverworldDecorKit.IceCrackStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
                case OverworldDecor.LichenTuft:
                    b.Stamp(OverworldDecorKit.LichenStamp, at, prop.Yaw, s, TownSwatch.Black);
                    return true;
            }

            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.SnowDome: OverworldDecorKit.SnowDome(b, s, prop.Variant); break;
                case OverworldDecor.PlankLonghouse: OverworldDecorKit.PlankLonghouse(b, s, prop.Variant); break;
                case OverworldDecor.CrescentBoat: OverworldDecorKit.CrescentBoat(b, s, prop.Variant); break;
                case OverworldDecor.WhaleboneArch: OverworldDecorKit.WhaleboneArch(b, s, prop.Variant); break;
                case OverworldDecor.IceBlock: OverworldDecorKit.IceBlock(b, s, prop.Variant); break;
                case OverworldDecor.Iceberg: OverworldDecorKit.Iceberg(b, s, prop.Variant); break;
                case OverworldDecor.HerderTent: OverworldDecorKit.HerderTent(b, s, prop.Variant); break;
                case OverworldDecor.LichenBoulder: OverworldDecorKit.LichenBoulder(b, s, prop.Variant); break;
                case OverworldDecor.MammothTusk: OverworldDecorKit.MammothTusk(b, s, prop.Variant); break;
                case OverworldDecor.IceLantern: OverworldDecorKit.IceLantern(b, s, prop.Variant); break;
                case OverworldDecor.FishRack: OverworldDecorKit.FishRack(b, s, prop.Variant); break;
                case OverworldDecor.IceBeacon: OverworldDecorKit.IceBeacon(b, s, prop.Variant); break;
                case OverworldDecor.FrostCrystal: OverworldDecorKit.FrostCrystal(b, s, prop.Variant); break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
