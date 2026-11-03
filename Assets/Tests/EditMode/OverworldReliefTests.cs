using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Rapadura.Tests
{
    /// <summary>
    /// The relief and the finish of the continuous Região 1 (Docs/Mapas100.md §0.10, "Relevo e acabamento"): a pure,
    /// continuous height with no seams between chunks; flat ground under the settlements, the gates and across the trails;
    /// hills where the areas say hills, carved river beds; the dressing standing on it (crop rows, hedges, the areas' own
    /// pieces), deterministic; and the per-chunk budget (triangles, generation time).
    /// </summary>
    public class OverworldReliefTests
    {
        /// <summary>Mobile budget per chunk: ground + props mesh (Docs/PlatformRequirements.md).</summary>
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.ZarafshanId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [Test]
        public void Height_IsPureAndContinuous()
        {
            OverworldRelief relief = Relief;
            var rnd = new System.Random(7);
            for (int i = 0; i < 4000; i++)
            {
                var p = new Vector2(-1040f + (float)rnd.NextDouble() * 2480f, -1040f + (float)rnd.NextDouble() * 2480f);
                float h = relief.HeightAt(p);
                Assert.AreEqual(h, relief.HeightAt(p), 0f, "the same point, the same height");
                Assert.IsFalse(float.IsNaN(h) || float.IsInfinity(h), $"{p}");

                // Continuity: a 5 cm step never jumps (the steepest designed slope is well under 80°).
                var q = p + new Vector2((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f).normalized * 0.05f;
                Assert.Less(Mathf.Abs(relief.HeightAt(q) - h), 0.3f, $"a jump in the relief at {p}");
            }
        }

        [Test]
        public void Chunks_ShareTheirBorderVertices_NoSeams()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            var rnd = new System.Random(11);
            for (int i = 0; i < 60; i++)
            {
                var c = new Vector2Int(rnd.Next(-1, count.x), rnd.Next(-1, count.y));
                OverworldGroundGrid a = OverworldChunkBuilder.BuildGrid(w, c);
                OverworldGroundGrid east = OverworldChunkBuilder.BuildGrid(w, c + Vector2Int.right);
                OverworldGroundGrid north = OverworldChunkBuilder.BuildGrid(w, c + Vector2Int.up);
                Rect r = OverworldRules.ChunkRect(w, c);
                for (float t = 0f; t <= w.ChunkSize; t += 0.37f)
                {
                    var onEast = new Vector2(r.xMax, r.yMin + t);
                    var onNorth = new Vector2(r.xMin + t, r.yMax);
                    Assert.AreEqual(a.Sample(onEast), east.Sample(onEast), 1e-4f, $"seam between {c} and its east neighbour at {onEast}");
                    Assert.AreEqual(a.Sample(onNorth), north.Sample(onNorth), 1e-4f, $"seam between {c} and its north neighbour at {onNorth}");
                    Assert.AreEqual(OverworldRules.SurfaceAt(w, onEast), a.Sample(onEast), 1e-4f, "the pure surface is the drawn one");
                }
            }
        }

        [Test]
        public void Settlements_Gates_AndTheirArrivals_StandAtYZero()
        {
            OverworldLayout w = World;
            foreach (OverworldSettlement settlement in w.Settlements)
            {
                Rect r = settlement.GroundRect;
                for (float x = r.xMin; x <= r.xMax; x += 1.5f)
                {
                    for (float z = r.yMin; z <= r.yMax; z += 1.5f)
                    {
                        Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, new Vector2(x, z)), 1e-4f, $"{settlement.Id}: the town kit stands at y = 0 ({x}, {z})");
                    }
                }

                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, settlement.Offset), 1e-4f, settlement.Id);
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Trigger.center), 1e-4f, $"{exit.Id}: trigger on flat ground");
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Arrival), 1e-4f, $"{exit.Id}: the hero arrives on flat ground");
            }
        }

        [Test]
        public void Trails_AreLevelAcross_AndGentleAlong()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            var cross = new List<float>();
            foreach (MapPath path in w.World.Paths)
            {
                Vector2 d = path.B - path.A;
                float length = d.magnitude;
                if (length < 1f)
                {
                    continue;
                }

                d /= length;
                var n = new Vector2(-d.y, d.x) * (path.Width * 0.5f);
                for (float t = 0.1f; t < 0.95f; t += 0.2f)
                {
                    Vector2 q = Vector2.Lerp(path.A, path.B, t);
                    ReliefInfo left = relief.Sample(q + n);
                    ReliefInfo right = relief.Sample(q - n);
                    if (left.Free < 0.999f || right.Free < 0.999f)
                    {
                        // Where a trail runs down into a river valley or a town's flat the valley side tilts it a little.
                        continue;
                    }

                    cross.Add(Mathf.Abs(left.Height - right.Height) / path.Width);
                }

                float along = Mathf.Abs(relief.HeightAt(path.B) - relief.HeightAt(path.A)) / length;
                Assert.Less(Mathf.Atan(along) * Mathf.Rad2Deg, 30f, $"trail {path.A} → {path.B} too steep to walk comfortably");
            }

            cross.Sort();
            Assert.Greater(cross.Count, 1000);
            Assert.Less(cross[cross.Count / 2], 0.005f, "a trail is level across its width (median)");
            Assert.Less(cross[(int)(cross.Count * 0.98f)], 0.08f, "a trail is level across its width (98 %)");
            Assert.Less(cross.Last(), 0.3f, "no trail leans like a hillside");
        }

        [Test]
        public void Areas_RiseAsTheirNamesSay_AndRiversRunInTheirBeds()
        {
            OverworldLayout w = World;
            Assert.Greater(MaxHeight(w.Zone("colinas_mihryazd")), 10f, "the Colinas de Mihryazd have hills");
            Assert.Greater(MaxHeight(w.Zone("altos_do_mensageiro")), 15f, "the Altos are high");
            Assert.Greater(MaxHeight(w.Zone("ravina_bocas_de_luz")), 10f, "the Ravina is cut into high ground");
            Assert.Greater(MaxHeight(w.Zone("altos_do_mensageiro")), MaxHeight(w.Zone("campos_do_aryk")) + 10f, "the Altos rise far above the fields");
            Assert.Less(MaxHeight(w.Zone("campos_do_aryk")), 4f, "the irrigated fields are nearly flat");

            // Terraced plateaus: in the Altos a good share of the ground is level (the steps), not one constant slope.
            OverworldZone altos = w.Zone("altos_do_mensageiro");
            int level = 0;
            int total = 0;
            for (float x = altos.Bounds.xMin + 40f; x < altos.Bounds.xMax - 40f; x += 9f)
            {
                for (float z = altos.Bounds.yMin + 40f; z < altos.Bounds.yMax - 40f; z += 9f)
                {
                    var p = new Vector2(x, z);
                    float gx = Relief.HeightAt(p + Vector2.right) - Relief.HeightAt(p - Vector2.right);
                    float gz = Relief.HeightAt(p + Vector2.up) - Relief.HeightAt(p - Vector2.up);
                    total++;
                    if (Mathf.Sqrt(gx * gx + gz * gz) * 0.5f < 0.08f)
                    {
                        level++;
                    }
                }
            }

            Assert.Greater(level / (float)total, 0.3f, "the Altos have plateaus");

            // Wide rivers and ponds run below the banks; bridges land on y = 0 at both ends.
            foreach (MapRiver river in w.World.Rivers)
            {
                Vector2 mid = Vector2.Lerp(river.Points[0], river.Points[1], 0.5f);
                if (river.Width >= OverworldRelief.CarveMinWidth && !river.Crossings.Any(c => Vector2.Distance(c.At, mid) < c.Width + 4f))
                {
                    Assert.Less(Relief.HeightAt(mid), OverworldRelief.WaterY - 0.5f, $"a carved bed under the water line at {mid}");
                }

                foreach (MapCrossing crossing in river.Crossings.Where(c => c.IsBridge))
                {
                    Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                    float half = OverworldChunkBuilder.BridgeSpan(river) * 0.5f;
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At + across * half), 0.02f, $"bridge end at {crossing.At}");
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At - across * half), 0.02f, $"bridge end at {crossing.At}");
                }
            }
        }

        [Test]
        public void SpawnClearings_SitOnPlateaus()
        {
            OverworldLayout w = World;
            int flat = 0;
            foreach (MapSpawnZone zone in w.World.Zones)
            {
                float h0 = OverworldRules.SurfaceAt(w, zone.Center);
                float worst = 0f;
                for (int k = 0; k < 8; k++)
                {
                    float a = k * Mathf.PI / 4f;
                    Vector2 p = zone.Center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * zone.Radius * 0.7f;
                    worst = Mathf.Max(worst, Mathf.Abs(OverworldRules.SurfaceAt(w, p) - h0));
                }

                if (worst < 0.35f)
                {
                    flat++;
                }

                Assert.Less(worst, 3f, $"{zone.Id}: its monsters fight on level ground");
            }

            Assert.Greater(flat / (float)w.World.Zones.Count, 0.9f, "almost every clearing is a flat plateau");
        }

        [Test]
        public void Dressing_StandsOnTheGround_OffTheTrails_AndEveryAreaHasItsOwnLook()
        {
            OverworldLayout w = World;
            var expected = new Dictionary<string, OverworldDecor[]>
            {
                { "campos_do_aryk", new[] { OverworldDecor.CropRow, OverworldDecor.Hedge } },
                { "altos_do_mensageiro", new[] { OverworldDecor.Cairn } },
                { "ravina_bocas_de_luz", new[] { OverworldDecor.Basalt } },
                { "ruinas_aryk_velho", new[] { OverworldDecor.Column } },
                { "jardim_mihrshah", new[] { OverworldDecor.Cypress, OverworldDecor.Flowers } },
                { "brejo", new[] { OverworldDecor.DeadTree, OverworldDecor.Reeds } },
                { "mata_cinco_arvores", new[] { OverworldDecor.Ferns, OverworldDecor.Log } },
                { "prado_das_cegonhas", new[] { OverworldDecor.Flowers, OverworldDecor.Tufts } },
                { "varzea_dourada", new[] { OverworldDecor.Reeds } },
                { "garajonay", new[] { OverworldDecor.Ferns } },
            };

            var signatures = new Dictionary<string, HashSet<string>>();
            foreach (OverworldZone zone in w.Zones)
            {
                var seen = new HashSet<OverworldDecor>();
                var signature = new HashSet<string>();
                foreach (Vector2Int c in InnerChunks(w, zone, 6))
                {
                    OverworldChunkContent content = OverworldChunkBuilder.Content(w, c);
                    foreach (OverworldProp prop in content.Props.Concat(content.Details))
                    {
                        seen.Add(prop.Decor);
                        signature.Add(prop.Decor + "/" + prop.Kind + "/" + (prop.Kind == CityPropKind.Tree ? prop.Vegetation + ":" + prop.Variant : string.Empty));
                    }

                    foreach (OverworldProp detail in content.Details)
                    {
                        if (detail.Decor == OverworldDecor.CropRow)
                        {
                            Assert.IsFalse(MapLayoutRules.OnPath(w.World, detail.End, 0f), $"crop row on a trail at {detail.End}");
                        }

                        Assert.IsFalse(MapLayoutRules.OnPath(w.World, detail.Position, 0f), $"{detail.Decor} on a trail at {detail.Position}");
                        Assert.IsFalse(MapLayoutRules.InRiver(w.World, detail.Position, 0f), $"{detail.Decor} in a river at {detail.Position}");
                        Assert.IsNull(OverworldRules.SettlementAt(w, detail.Position), $"{detail.Decor} in a safe zone at {detail.Position}");
                    }

                    // Every collider stands on the relief: its base a little under the drawn ground.
                    foreach (OverworldSolid solid in content.Solids)
                    {
                        float ground = content.Grid.Sample(solid.Center);
                        Assert.LessOrEqual(solid.Base, ground + 0.01f, $"{c}: collider floating at {solid.Center}");
                        Assert.Greater(solid.Base + solid.Height, ground + 0.3f, $"{c}: collider sunk at {solid.Center}");
                    }
                }

                signatures[zone.Id] = signature;
                if (expected.TryGetValue(zone.Id, out OverworldDecor[] pieces))
                {
                    foreach (OverworldDecor piece in pieces)
                    {
                        Assert.IsTrue(seen.Contains(piece), $"{zone.Id}: no {piece} in its chunks");
                    }
                }

                Assert.GreaterOrEqual(seen.Count, 4, $"{zone.Id}: a varied dressing");
            }

            // Areas of different terrains never look the same (their sets of pieces differ).
            for (int i = 0; i < w.Zones.Count; i++)
            {
                for (int j = i + 1; j < w.Zones.Count; j++)
                {
                    if (w.Zones[i].Terrain != w.Zones[j].Terrain)
                    {
                        Assert.IsFalse(signatures[w.Zones[i].Id].SetEquals(signatures[w.Zones[j].Id]), $"{w.Zones[i].Id} and {w.Zones[j].Id} look alike");
                    }
                }
            }
        }

        [Test]
        public void ChunkContent_IsDeterministic_WithTheRelief()
        {
            OverworldLayout w = World;
            foreach (Vector2 p in new[] { new Vector2(100f, -240f), new Vector2(-560f, 560f), new Vector2(-850f, -350f), new Vector2(880f, -560f) })
            {
                Vector2Int c = OverworldRules.ChunkOf(w, p);
                OverworldChunkContent a = OverworldChunkBuilder.Content(w, c);
                OverworldChunkContent b = OverworldChunkBuilder.Content(w, c);
                Assert.AreEqual(a.Props.Count, b.Props.Count);
                Assert.AreEqual(a.Details.Count, b.Details.Count);
                CollectionAssert.AreEqual(a.Grid.Height, b.Grid.Height);
                for (int i = 0; i < a.Details.Count; i++)
                {
                    Assert.AreEqual(a.Details[i].Position, b.Details[i].Position);
                }
            }
        }

        [Test]
        public void EveryChunk_StaysWithinTheTriangleBudget_AndBuildsFast()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            var builder = new TownMeshBuilder();
            var colliderVertices = new List<Vector3>();
            var colliderTriangles = new List<int>();

            // Warm up (first-use costs: the relief buckets, the stamps, the JIT).
            OverworldChunkContent warm = OverworldChunkBuilder.Content(w, new Vector2Int(10, 10));
            OverworldChunkBuilder.BuildGround(builder, w, warm);

            var times = new List<double>();
            int worst = 0;
            Vector2Int worstChunk = default;
            long total = 0;
            for (int x = -1; x <= count.x; x += 3)
            {
                for (int y = -1; y <= count.y; y += 3)
                {
                    var c = new Vector2Int(x, y);
                    var watch = Stopwatch.StartNew();
                    OverworldChunkContent content = OverworldChunkBuilder.Content(w, c);
                    builder.Clear();
                    OverworldChunkBuilder.BuildGround(builder, w, content);
                    int ground = builder.TriangleCount;
                    builder.Clear();
                    OverworldChunkBuilder.BuildProps(builder, content);
                    int triangles = ground + builder.TriangleCount;
                    watch.Stop();
                    times.Add(watch.Elapsed.TotalMilliseconds);
                    total += triangles;
                    if (triangles > worst)
                    {
                        worst = triangles;
                        worstChunk = c;
                    }

                    Assert.LessOrEqual(triangles, MaxTrianglesPerChunk, $"chunk {c}: {triangles} triangles");

                    if (content.Inner)
                    {
                        OverworldChunkBuilder.BuildColliderMesh(content, colliderVertices, colliderTriangles);
                        int cells = Mathf.RoundToInt(w.ChunkSize / OverworldRelief.Cell);
                        Assert.AreEqual((cells + 1) * (cells + 1), colliderVertices.Count);
                        Assert.AreEqual(cells * cells * 6, colliderTriangles.Count);
                    }
                }
            }

            times.Sort();
            double average = times.Average();
            Debug.Log($"[Overworld] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
                $"generation avg {average:0.00} ms, p50 {times[times.Count / 2]:0.00} ms, p95 {times[(int)(times.Count * 0.95f)]:0.00} ms (Editor, Mono)");
            Assert.Less(average, 6.0, "chunk generation stays cheap even in the Editor (target ≤ 1 ms in a player build)");
        }

        [Test]
        public void ClearingChunks_CoverTheClearing()
        {
            OverworldLayout w = World;
            var chunks = new List<Vector2Int>();
            foreach (MapSpawnZone zone in w.World.Zones.Take(40))
            {
                OverworldRules.ChunksUnderClearing(w, zone, chunks);
                for (int k = 0; k < 16; k++)
                {
                    float a = k * Mathf.PI / 8f;
                    Vector2 p = zone.Center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * zone.Radius;
                    Assert.Contains(OverworldRules.ChunkOf(w, p), chunks, $"{zone.Id}: the ground under its edge");
                }
            }
        }

        private static float MaxHeight(OverworldZone zone)
        {
            float max = float.MinValue;
            for (float x = zone.Bounds.xMin + 2f; x < zone.Bounds.xMax; x += 6f)
            {
                for (float z = zone.Bounds.yMin + 2f; z < zone.Bounds.yMax; z += 6f)
                {
                    max = Mathf.Max(max, Relief.HeightAt(new Vector2(x, z)));
                }
            }

            return max;
        }

        /// <summary>Up to <paramref name="max"/> chunks lying wholly inside <paramref name="zone"/> (spread over it).</summary>
        private static IEnumerable<Vector2Int> InnerChunks(OverworldLayout w, OverworldZone zone, int max)
        {
            var all = new List<Vector2Int>();
            Vector2Int a = OverworldRules.ChunkOf(w, zone.Bounds.min);
            Vector2Int b = OverworldRules.ChunkOf(w, zone.Bounds.max - Vector2.one);
            for (int x = a.x; x <= b.x; x++)
            {
                for (int y = a.y; y <= b.y; y++)
                {
                    var c = new Vector2Int(x, y);
                    Rect r = OverworldRules.ChunkRect(w, c);
                    if (zone.Bounds.Contains(r.min) && zone.Bounds.Contains(r.max - Vector2.one * 0.01f))
                    {
                        all.Add(c);
                    }
                }
            }

            int step = Mathf.Max(1, all.Count / max);
            for (int i = 0; i < all.Count && i / step < max; i += step)
            {
                yield return all[i];
            }
        }
    }
}
