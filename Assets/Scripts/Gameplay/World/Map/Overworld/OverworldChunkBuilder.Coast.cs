using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 5 — Costa das Monções: the coast pieces of the chunk builder. Before the noise scatter, each chunk gets the big
    /// pieces of its areas (dhows beached on the sand, fish traps and net racks, coral heads on the reef and the shoals, the
    /// arches and pillar tombs of the coral ruins, the laterite sea stacks under the cliffs, acacias and termite mounds in the
    /// savanna, Meroë's pyramids), the runs of ruined coral wall, the mangroves crowding every channel of the mangroves and
    /// lagoons, the striped posts on both banks of each ford (the walkable shallows are marked), the wrack on the beaches and
    /// the cloves drying in the plantations; then the shared scatter fills around them.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>A Costa das Monções terrain (Região 5, 80–99).</summary>
        public static bool IsCoast(OverworldTerrain t) => OverworldTerrains.IsCoast(t);

        private static bool IsCoastDecor(OverworldDecor decor) => OverworldDecorKit.IsCoastDecor(decor);

        /// <summary>Room (m) a scattered coast piece keeps from the others.</summary>
        private static float CoastClearance(OverworldDecor decor)
        {
            switch (decor)
            {
                case OverworldDecor.BeachedDhow: return 3.6f;
                case OverworldDecor.FishTrap: return 2.6f;
                case OverworldDecor.NetRack: return 2f;
                case OverworldDecor.CoralArch: return 2.4f;
                case OverworldDecor.PillarTomb: return 1.9f;
                case OverworldDecor.Acacia: return 2.4f;
                case OverworldDecor.TermiteMound: return 1.4f;
                case OverworldDecor.MangroveClump: return 1.6f;
                case OverworldDecor.CoralHead: return 1.3f;
                default: return 0.9f;
            }
        }

        private static BigPiece[] CoastPiecesOf(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.WhiteBeach:
                    return new[]
                    {
                        new BigPiece(OverworldDecor.BeachedDhow, 0.16f, 3.4f, true), new BigPiece(OverworldDecor.FishTrap, 0.1f, 2.4f, true),
                        new BigPiece(OverworldDecor.NetRack, 0.14f, 1.9f, true),
                    };
                case OverworldTerrain.Reef:
                    return new[] { new BigPiece(OverworldDecor.CoralHead, 0.45f, 1.4f, false), new BigPiece(OverworldDecor.FishTrap, 0.12f, 2.4f, true) };
                case OverworldTerrain.Shoals:
                    return new[]
                    {
                        new BigPiece(OverworldDecor.CoralHead, 0.25f, 1.4f, false), new BigPiece(OverworldDecor.FishTrap, 0.12f, 2.4f, true),
                        new BigPiece(OverworldDecor.BeachedDhow, 0.07f, 3.4f, true),
                    };
                case OverworldTerrain.Atoll:
                    return new[]
                    {
                        new BigPiece(OverworldDecor.CoralHead, 0.3f, 1.4f, false), new BigPiece(OverworldDecor.NetRack, 0.1f, 1.9f, true),
                        new BigPiece(OverworldDecor.BeachedDhow, 0.06f, 3.4f, true),
                    };
                case OverworldTerrain.KelpFlats:
                    return new[] { new BigPiece(OverworldDecor.CoralHead, 0.4f, 1.4f, false) };
                case OverworldTerrain.SunkenCity:
                    return new[] { new BigPiece(OverworldDecor.CoralArch, 0.3f, 2.2f, true), new BigPiece(OverworldDecor.PillarTomb, 0.1f, 1.8f, true) };
                case OverworldTerrain.CoralRuins:
                    return new[] { new BigPiece(OverworldDecor.CoralArch, 0.28f, 2.2f, true), new BigPiece(OverworldDecor.PillarTomb, 0.22f, 1.8f, true) };
                case OverworldTerrain.SeaCliffs:
                    return new[] { new BigPiece(OverworldDecor.RockSpire, 0.4f, 2f, false) };
                case OverworldTerrain.Savanna:
                    return new[] { new BigPiece(OverworldDecor.Acacia, 0.35f, 2.2f, false), new BigPiece(OverworldDecor.TermiteMound, 0.25f, 1.3f, false) };
                case OverworldTerrain.BaobabWoods:
                    return new[] { new BigPiece(OverworldDecor.TermiteMound, 0.25f, 1.3f, false) };
                case OverworldTerrain.KushPyramids:
                    return new[] { new BigPiece(OverworldDecor.Pyramid, 0.4f, 3.4f, true), new BigPiece(OverworldDecor.Ushabtis, 0.15f, 2.2f, true) };
                case OverworldTerrain.CoastalDunes:
                    return new[] { new BigPiece(OverworldDecor.BeachedDhow, 0.05f, 3.4f, true), new BigPiece(OverworldDecor.NetRack, 0.05f, 1.9f, true) };
                case OverworldTerrain.TideLagoons:
                    return new[] { new BigPiece(OverworldDecor.FishTrap, 0.15f, 2.4f, true) };
                default:
                    return NoPieces;
            }
        }

        private static void ScatterCoast(OverworldLayout layout, OverworldChunkContent content)
        {
            bool any = false;
            foreach (ReliefInfo info in content.Grid.Info)
            {
                if (IsCoast(info.Terrain))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            string salt = "owc" + content.Coord.x + "_" + content.Coord.y;
            Rect r = content.Rect;
            ScatterFordPosts(layout, content);

            // Big pieces: whole inside the chunk, their footprint kept clear.
            for (int k = 0; k < 5; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 7f, r.xMax - 7f, TownStyle.Hash01(salt, k)), Mathf.Lerp(r.yMin + 7f, r.yMax - 7f, TownStyle.Hash01(salt, k + 100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                if (!IsCoast(terrain) || layout.InAuthoredArea(p) || DistanceToWall(layout, p) < EdgeBelt)
                {
                    continue;
                }

                BigPiece[] pieces = CoastPiecesOf(terrain);
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
                    float scale = piece.Decor == OverworldDecor.RockSpire ? 0.7f + 0.6f * s01 : 0.85f + 0.3f * s01;
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

            ScatterCoralWalls(layout, content, salt);
            ScatterMangroveFringe(layout, content, salt);
            ScatterShoreDetail(layout, content, salt);
        }

        /// <summary>Striped posts on both banks of every ford (baixio) of the chunk: the shallows the hero may wade are marked.</summary>
        private static void ScatterFordPosts(OverworldLayout layout, OverworldChunkContent content)
        {
            foreach (MapRiver river in content.NearRivers)
            {
                if (river.Width < OverworldRelief.CarveMinWidth)
                {
                    continue;
                }

                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (crossing.IsBridge || !MarksFords(content.Grid.Near(content.Rect.center).Terrain) && !MarksFords(TerrainNear(content, crossing.At)))
                    {
                        continue;
                    }

                    Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                    Vector2 along = new Vector2(-across.y, across.x);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        for (int edge = -1; edge <= 1; edge += 2)
                        {
                            Vector2 p = crossing.At + across * (side * (river.Width * 0.5f + 1.6f)) + along * (edge * (crossing.Width * 0.5f + 0.4f));
                            if (!content.Rect.Contains(p) || !IsOpen(layout, content, p, 0.3f, false))
                            {
                                continue;
                            }

                            var post = new OverworldProp { Kind = CityPropKind.Rock, Decor = OverworldDecor.FordPost, Position = p, Scale = 1f, Variant = side + 2 * edge };
                            content.Props.Add(post);
                            AddSolid(layout, content, post);
                        }
                    }
                }
            }
        }

        /// <summary>The fords of the coast and of the delta (Região 6) are marked by striped posts.</summary>
        private static bool MarksFords(OverworldTerrain t) => IsCoast(t) || OverworldTerrains.IsDelta(t);

        private static OverworldTerrain TerrainNear(OverworldChunkContent content, Vector2 p)
        {
            return content.Grid.Near(new Vector2(Mathf.Clamp(p.x, content.Rect.xMin, content.Rect.xMax), Mathf.Clamp(p.y, content.Rect.yMin, content.Rect.yMax))).Terrain;
        }

        /// <summary>Runs of ruined coral-stone wall in the coral ruins and the drowned streets of Kilwa.</summary>
        private static void ScatterCoralWalls(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 3; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin + 4f, r.xMax - 4f, TownStyle.Hash01(salt, k + 1000)), Mathf.Lerp(r.yMin + 4f, r.yMax - 4f, TownStyle.Hash01(salt, k + 1100)));
                OverworldTerrain terrain = content.Grid.Near(p).Terrain;
                float chance = terrain == OverworldTerrain.CoralRuins ? 0.6f : terrain == OverworldTerrain.SunkenCity ? 0.45f : 0f;
                if (TownStyle.Hash01(salt, k + 1200) >= chance || layout.InAuthoredArea(p))
                {
                    continue;
                }

                // Walls meet at right angles: an L of two straight runs.
                float yaw = Mathf.Round(TownStyle.Hash01(salt, k + 1300) * 4f) * 90f;
                int pieces = 2 + (int)(TownStyle.Hash(salt, k + 1400) % 4u);
                float height = 1.4f + TownStyle.Hash01(salt, k + 1500) * 1.4f;
                Vector2 a = p;
                for (int i = 0; i < pieces * 2; i++)
                {
                    if (i == pieces)
                    {
                        yaw += TownStyle.Hash01(salt, k + 1600) < 0.5f ? 90f : -90f;
                    }

                    Vector2 dir = new Vector2(Mathf.Cos(yaw * Mathf.Deg2Rad), Mathf.Sin(yaw * Mathf.Deg2Rad));
                    Vector2 c = a + dir * RowPiece;
                    Vector2 mid = (a + c) * 0.5f;
                    if (!r.Contains(a) || !r.Contains(c))
                    {
                        break;
                    }

                    if (TownStyle.Hash01(salt, k * 31 + i + 1700) > 0.18f && IsFree(layout, content, a, 1f) && IsFree(layout, content, c, 1f)
                        && IsFree(layout, content, mid, 1f))
                    {
                        var prop = new OverworldProp
                        {
                            Kind = CityPropKind.FenceRun, Decor = OverworldDecor.CoralWall, Position = a, End = c, Scale = height, Variant = k * 31 + i,
                        };
                        content.Props.Add(prop);
                        AddSolid(layout, content, prop);
                    }

                    a = c;
                }
            }
        }

        /// <summary>The mangroves crowd the channels of the mangroves and lagoons (one try every few metres along the water).</summary>
        private static void ScatterMangroveFringe(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            const float spacing = 4.4f;
            Rect r = content.Rect;
            int steps = Mathf.CeilToInt(layout.ChunkSize / spacing);
            int i = 0;
            for (int gx = 0; gx < steps; gx++)
            {
                for (int gz = 0; gz < steps; gz++, i++)
                {
                    var p = new Vector2(
                        r.xMin + (gx + 0.5f + (TownStyle.Hash01(salt, i + 3000) - 0.5f) * 0.8f) * spacing,
                        r.yMin + (gz + 0.5f + (TownStyle.Hash01(salt, i + 6000) - 0.5f) * 0.8f) * spacing);
                    if (!r.Contains(p))
                    {
                        continue;
                    }

                    ref ReliefInfo info = ref content.Grid.Near(p);
                    if ((info.Terrain != OverworldTerrain.Mangrove && info.Terrain != OverworldTerrain.TideLagoons) || info.RiverEdge < 1.6f || info.RiverEdge > 7f)
                    {
                        continue;
                    }

                    if (TownStyle.Hash01(salt, i + 9000) > (info.Terrain == OverworldTerrain.Mangrove ? 0.55f : 0.3f) || DistanceToWall(layout, p) < EdgeBelt)
                    {
                        continue;
                    }

                    TryAdd(layout, content, p, 1.5f, new OverworldProp
                    {
                        Kind = CityPropKind.Rock, Decor = OverworldDecor.MangroveClump, Position = p, Yaw = TownStyle.Hash01(salt, i + 12000) * 360f,
                        Scale = 0.85f + 0.4f * TownStyle.Hash01(salt, i + 15000), Variant = (int)(TownStyle.Hash(salt, i + 18000) & 3),
                    });
                }
            }
        }

        /// <summary>Wrack lines along the beaches' water and cloves drying in the plantations (ground detail: no colliders).</summary>
        private static void ScatterShoreDetail(OverworldLayout layout, OverworldChunkContent content, string salt)
        {
            Rect r = content.Rect;
            for (int k = 0; k < 40; k++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, k + 20000)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, k + 21000)));
                ref ReliefInfo info = ref content.Grid.Near(p);
                OverworldDecor decor;
                float chance;
                if (info.Terrain == OverworldTerrain.CloveRows)
                {
                    decor = OverworldDecor.CloveMat;
                    chance = 0.12f;
                }
                else if (OverworldTerrains.IsSeaShore(info.Terrain) && info.RiverEdge > 0.6f && info.RiverEdge < 5f)
                {
                    decor = OverworldDecor.Wrack;
                    chance = 0.7f;
                }
                else if (info.Terrain == OverworldTerrain.WhiteBeach || info.Terrain == OverworldTerrain.Atoll)
                {
                    decor = OverworldDecor.Wrack;
                    chance = 0.08f;
                }
                else
                {
                    continue;
                }

                if (TownStyle.Hash01(salt, k + 22000) >= chance || info.Clearing < 0.5f || DistanceToWall(layout, p) < -10f
                    || !IsOpen(layout, content, p, decor == OverworldDecor.CloveMat ? 1.3f : 1f, false))
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

        private static bool AddCoastDecorSolid(OverworldChunkContent content, OverworldProp prop, float ground, float sink)
        {
            if (!IsCoastDecor(prop.Decor))
            {
                return false;
            }

            Vector2 p = prop.Position;
            float s = prop.Scale;
            switch (prop.Decor)
            {
                case OverworldDecor.BeachedDhow:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(1.9f * s, 6f * s), Height = 1.1f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.FishTrap:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 1.7f * s, Height = 0.9f * s + sink });
                    break;
                case OverworldDecor.NetRack:
                    foreach (float x in new[] { -1.6f, 1.6f })
                    {
                        Vector2 at = p + Rotate(new Vector2(x * s, 0f), prop.Yaw);
                        content.Solids.Add(new OverworldSolid { Center = at, Base = content.Grid.Sample(at) - sink, Radius = 0.15f * s, Height = 2.1f * s + sink });
                    }

                    break;
                case OverworldDecor.CoralHead:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.85f * s, Height = 1f * s + sink });
                    break;
                case OverworldDecor.CoralWall:
                    AddRunSolid(content, prop, 0.6f, Mathf.Max(1f, prop.Scale), sink);
                    break;
                case OverworldDecor.CoralArch:
                    foreach (float x in new[] { -1.3f, 1.3f })
                    {
                        Vector2 at = p + Rotate(new Vector2(x * s, 0f), prop.Yaw);
                        content.Solids.Add(new OverworldSolid
                        {
                            Center = at, Base = content.Grid.Sample(at) - sink, IsBox = true, Size = new Vector2(0.7f * s, 0.8f * s), Height = 2.9f * s + sink, Yaw = prop.Yaw,
                        });
                    }

                    break;
                case OverworldDecor.PillarTomb:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, IsBox = true, Size = new Vector2(2.4f * s, 1.6f * s), Height = 1f * s + sink, Yaw = prop.Yaw });
                    break;
                case OverworldDecor.Acacia:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.22f * s, Height = 3f * s + sink });
                    break;
                case OverworldDecor.TermiteMound:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 2.4f * s + sink });
                    break;
                case OverworldDecor.MangroveClump:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.9f * s, Height = 2.5f + sink });
                    break;
                case OverworldDecor.FordPost:
                    content.Solids.Add(new OverworldSolid { Center = p, Base = ground, Radius = 0.14f, Height = 1.5f + sink });
                    break;
            }

            return true;
        }

        private static bool AppendCoastDecor(TownMeshBuilder b, OverworldGroundGrid g, OverworldProp prop, Vector3 at)
        {
            if (!IsCoastDecor(prop.Decor))
            {
                return false;
            }

            if (prop.Decor == OverworldDecor.CoralWall)
            {
                float low = Mathf.Min(at.y, g.Sample(prop.End));
                OverworldDecorKit.CoralWall(b, new Vector3(at.x, low, at.z), new Vector3(prop.End.x, low, prop.End.y), prop.Scale, prop.Variant);
                return true;
            }

            float s = prop.Scale;
            Matrix4x4 saved = b.Matrix;
            b.Place(at, prop.Yaw, 1f);
            switch (prop.Decor)
            {
                case OverworldDecor.BeachedDhow: OverworldDecorKit.BeachedDhow(b, s, prop.Variant); break;
                case OverworldDecor.FishTrap: OverworldDecorKit.FishTrap(b, s, prop.Variant); break;
                case OverworldDecor.NetRack: OverworldDecorKit.NetRack(b, s, prop.Variant); break;
                case OverworldDecor.CoralHead: OverworldDecorKit.CoralHead(b, s, prop.Variant); break;
                case OverworldDecor.CoralArch: OverworldDecorKit.CoralArch(b, s, prop.Variant); break;
                case OverworldDecor.PillarTomb: OverworldDecorKit.PillarTomb(b, s, prop.Variant); break;
                case OverworldDecor.Acacia: OverworldDecorKit.Acacia(b, s, prop.Variant); break;
                case OverworldDecor.TermiteMound: OverworldDecorKit.TermiteMound(b, s, prop.Variant); break;
                case OverworldDecor.Wrack: OverworldDecorKit.Wrack(b, s, prop.Variant); break;
                case OverworldDecor.CloveMat: OverworldDecorKit.CloveMat(b, s, prop.Variant); break;
                case OverworldDecor.FordPost: OverworldDecorKit.FordPost(b, s); break;
                case OverworldDecor.MangroveClump:
                    TownKit.Prop(b, new CityProp { Kind = CityPropKind.Mangrove, Scale = s, Variant = prop.Variant }, TownStyles.Coast);
                    break;
            }

            b.Matrix = saved;
            return true;
        }
    }
}
