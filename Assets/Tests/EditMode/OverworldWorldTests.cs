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
    /// The stitched world (Docs/Mapas100.md §0.10, "Mundo costurado"): the Zarafshan, the Areias, the Serra de Kaggen and
    /// the Grutas do Limiar as ONE map of 4800 × 4800 m. Every point belongs to one region, the chunk grids line up, the gates
    /// between the regions became open passes (flat, joined by trails), the relief has no hole nor step on the borders, the
    /// Zarafshan is the same as on its own away from them, and the hero can walk on trails from the Vila to Qocho, Litema and
    /// the Lamparinas without a gate; the island rings, the Ossuário and the docks stay.
    /// </summary>
    public class OverworldWorldTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.WorldId);

        private static readonly string[] RegionIds = { OverworldLayouts.ZarafshanId, OverworldLayouts.AreiasId, OverworldLayouts.KaggenId, OverworldLayouts.LimiarId };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // The four regions in one world
        // ------------------------------------------------------------------

        [Test]
        public void World_IsValid_AndTilesTheFourRegions_EachPointInOneRegion()
        {
            OverworldLayout w = World;
            CollectionAssert.IsEmpty(OverworldRules.Validate(w), string.Join("\n", OverworldRules.Validate(w)));
            Assert.IsTrue(w.IsStitched);
            CollectionAssert.AreEquivalent(RegionIds, w.Parts.Select(p => p.Id));
            Assert.AreEqual(new Rect(-3400f, -3400f, 4800f, 4800f), w.Bounds, "2 × 2 regions of 2400 m");
            Assert.AreEqual(Vector2.zero, w.Parts.Find(p => p.Id == OverworldLayouts.ZarafshanId).Offset, "the Vila stays at (0, 0)");
            Assert.AreEqual(Vector2.zero, w.Vila.Offset);

            float area = 0f;
            foreach (OverworldPart part in w.Parts)
            {
                area += part.Bounds.width * part.Bounds.height;
                OverworldLayout alone = OverworldLayouts.Get(part.Id);
                Assert.AreEqual(alone.Bounds.size, part.Bounds.size, part.Id);
                Assert.AreEqual(alone.Bounds.position + part.Offset, part.Bounds.position, part.Id);
                Assert.AreEqual(0f, Mathf.Repeat(part.Bounds.xMin - w.Bounds.xMin, w.ChunkSize), 1e-3f, $"{part.Id}: on the chunk grid");
                Assert.AreEqual(0f, Mathf.Repeat(part.Bounds.yMin - w.Bounds.yMin, w.ChunkSize), 1e-3f, $"{part.Id}: on the chunk grid");
                Assert.AreEqual(alone.Zones.Count, w.Zones.Count(z => part.Bounds.Contains(z.Bounds.center)), $"{part.Id}: every area came along");
                Assert.AreSame(part.Vila, w.VilaAt(part.Bounds.center));
                Assert.AreEqual(alone.Region, w.RegionAt(part.Bounds.center));
                Assert.AreEqual(alone.World.Biome, part.Biome);
            }

            Assert.AreEqual(w.Bounds.width * w.Bounds.height, area, 1f, "the regions cover the world");

            // Every point (borders included) belongs to exactly one region and one area of it.
            for (float x = w.Bounds.xMin; x < w.Bounds.xMax; x += 25f)
            {
                for (float z = w.Bounds.yMin; z < w.Bounds.yMax; z += 25f)
                {
                    var p = new Vector2(x, z);
                    Assert.AreEqual(1, w.Parts.Count(part => part.Bounds.Contains(p)), $"{p}: one region");
                    OverworldZone zone = OverworldRules.ZoneAt(w, p);
                    Assert.IsNotNull(zone, $"{p}: one area");
                    Assert.IsTrue(w.PartAt(p).Bounds.Contains(zone.Bounds.center), $"{p}: the area belongs to its region");
                }
            }

            Assert.AreEqual(w.Zones.Count, w.Zones.Select(z => z.Id).Distinct().Count(), "area ids are unique in the world");
            Assert.AreEqual(w.World.Zones.Count, w.World.Zones.Select(z => z.Id).Distinct().Count(), "clearing ids are unique in the world");
            Assert.AreEqual(w.Settlements.Count, w.Settlements.Select(s => s.Id).Distinct().Count(), "town ids are unique in the world");
            Assert.AreEqual(w.World.Exits.Count, w.World.Exits.Select(e => e.Id).Distinct().Count(), "exit ids are unique in the world");
        }

        [Test]
        public void World_IsWhatMapModeLoads_ForEveryMapOfTheFourRegions()
        {
            OverworldLayout w = World;
            foreach (string id in RegionIds)
            {
                OverworldLayout alone = OverworldLayouts.Get(id);
                foreach (OverworldZone zone in alone.Zones)
                {
                    string map = zone.MapIdOrOwner;
                    Assert.AreSame(w, OverworldLayouts.WorldOf(map), $"{map}: map mode loads the world");
                    Assert.AreSame(alone, OverworldLayouts.ForMap(map), $"{map}: the region alone is still there (server, catalog)");
                    Assert.AreEqual(id, w.PartOfMap(map)?.Id, map);
                    Assert.IsTrue(w.ContainsMap(zone.MapId) || zone.MapId == null);
                }

                // Each town keeps its dock, the barca carries the hero there inside the same world.
                foreach (OverworldSettlement s in alone.Settlements.Where(s => s.HasDock))
                {
                    Assert.AreEqual(alone.ArrivalFor(s.MapId, null, out _) + w.Parts.Find(p => p.Id == id).Offset, w.ArrivalFor(s.MapId, null, out _), $"{s.Id}: barca stop");
                }
            }

            Assert.IsNull(OverworldLayouts.WorldOf("map_025_taa_pwani"), "region 5 is not part of the world yet");
        }

        [Test]
        public void Gates_BetweenTheRegions_AreOpenPasses_TheRingsTheOssuaryAndTheDocksStay()
        {
            OverworldLayout w = World;
            Assert.IsFalse(w.World.Exits.Exists(e => e.Kind == MapExitKind.Map && w.ContainsMap(e.TargetMapId)), "no gate inside the world");
            Assert.AreEqual(4, w.World.Exits.Count(e => e.Kind == MapExitKind.WorldMap && e.Id.StartsWith("exit_island")), "one stone ring per region");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == MapModeController.OssuaryExitId), "the Ossuário's portal keeps its id");
            Assert.AreEqual(w.Settlements.Count(s => s.HasDock), w.World.Exits.Count(e => e.Kind == MapExitKind.Dock));

            // The passes: Zarafshan–Areias (south road), Zarafshan–Kaggen (twice), Kaggen–Limiar, Limiar–Areias.
            Assert.GreaterOrEqual(w.Passes.Count, 6);
            OverworldRelief relief = OverworldRelief.For(w);
            foreach (Vector2 pass in w.Passes)
            {
                Assert.AreEqual(0f, relief.SurfaceAt(pass), 1e-3f, $"pass {pass}: flat ground");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, pass, 3f), $"pass {pass}: a trail runs through it");
            }

            foreach ((Vector2 a, Vector2 b) in new[]
            {
                (new Vector2(0f, -900f), new Vector2(-100f, -1100f)), (new Vector2(-900f, -150f), new Vector2(-1100f, -200f)),
                (new Vector2(-1700f, -900f), new Vector2(-1700f, -1100f)), (new Vector2(-1100f, -1110f), new Vector2(-900f, -1110f)),
            })
            {
                Assert.AreNotEqual(w.PartAt(a), w.PartAt(b), $"{a} and {b} are in two regions");
                Assert.Less(Vector2.Distance(Nearest(w.Passes, a), a), 220f, $"a pass near {a}");
            }
        }

        // ------------------------------------------------------------------
        // Relief across the borders
        // ------------------------------------------------------------------

        [Test]
        public void Relief_HasNoStepNorHole_OnTheBordersBetweenRegions()
        {
            OverworldLayout w = World;
            OverworldRelief relief = OverworldRelief.For(w);
            const float e = 0.02f;
            float worst = 0f;
            // The two inner borders: x = -1000 and z = -1000, the whole width of the world.
            for (float t = w.Bounds.xMin + 0.5f; t < w.Bounds.xMax; t += 1.5f)
            {
                foreach ((Vector2 p, Vector2 n) in new[] { (new Vector2(-1000f, t), Vector2.right), (new Vector2(t, -1000f), Vector2.up) })
                {
                    float a = relief.SurfaceAt(p - n * e);
                    float b = relief.SurfaceAt(p + n * e);
                    worst = Mathf.Max(worst, Mathf.Abs(a - b));
                    Assert.Less(Mathf.Abs(a - b), 0.1f, $"a step on the border at {p}");
                    Assert.IsFalse(float.IsNaN(a) || float.IsNaN(b), $"{p}");
                    Assert.Greater(a, -2.5f, $"a hole on the border at {p}");
                }
            }

            // The chunks either side of a border share their vertices.
            foreach (Vector2 pass in w.Passes)
            {
                Vector2Int c = OverworldRules.ChunkOf(w, pass);
                for (int dx = -2; dx <= 1; dx++)
                {
                    for (int dz = -2; dz <= 1; dz++)
                    {
                        Vector2Int k = c + new Vector2Int(dx, dz);
                        OverworldGroundGrid a = OverworldChunkBuilder.BuildGrid(w, k);
                        OverworldGroundGrid east = OverworldChunkBuilder.BuildGrid(w, k + Vector2Int.right);
                        OverworldGroundGrid north = OverworldChunkBuilder.BuildGrid(w, k + Vector2Int.up);
                        Rect r = OverworldRules.ChunkRect(w, k);
                        for (float s = 0f; s <= w.ChunkSize; s += 0.53f)
                        {
                            var onEast = new Vector2(r.xMax, r.yMin + s);
                            var onNorth = new Vector2(r.xMin + s, r.yMax);
                            Assert.AreEqual(a.Sample(onEast), east.Sample(onEast), 1e-4f, $"seam {k}/east at {onEast}");
                            Assert.AreEqual(a.Sample(onNorth), north.Sample(onNorth), 1e-4f, $"seam {k}/north at {onNorth}");
                        }
                    }
                }
            }

            Debug.Log($"[Overworld World] largest height change across 4 cm of a region border: {worst:0.000} m");
        }

        [Test]
        public void TheZarafshan_IsTheSameAsOnItsOwn_AwayFromTheBorders()
        {
            OverworldLayout w = World;
            OverworldLayout alone = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            OverworldRelief stitched = OverworldRelief.For(w);
            OverworldRelief own = OverworldRelief.For(alone);
            var rnd = new System.Random(7);
            int n = 0;
            for (int i = 0; i < 4000 && n < 1500; i++)
            {
                var p = new Vector2(-1000f + (float)rnd.NextDouble() * 2400f, -1000f + (float)rnd.NextDouble() * 2400f);
                if (p.x < -650f || p.y < -650f)
                {
                    continue; // the passes' trails and the blend with the neighbours reach this far
                }

                n++;
                Assert.AreEqual(own.HeightAt(p), stitched.HeightAt(p), 1e-3f, $"{p}");
                Assert.AreEqual(OverworldRules.ZoneAt(alone, p).Id, OverworldRules.ZoneAt(w, p).Id);
            }

            Assert.Greater(n, 1000);
        }

        [Test]
        public void Settlements_AndPasses_StandAtYZero_InTheWorld()
        {
            OverworldLayout w = World;
            foreach (OverworldSettlement settlement in w.Settlements)
            {
                Rect r = settlement.GroundRect;
                for (float x = r.xMin; x <= r.xMax; x += 4f)
                {
                    for (float z = r.yMin; z <= r.yMax; z += 4f)
                    {
                        Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, new Vector2(x, z)), 1e-4f, $"{settlement.Id} ({x}, {z})");
                    }
                }

                // The town walks the whole world: its invisible walls stand on the world's edge, not on its region's.
                Assert.AreEqual(w.Bounds.size, settlement.Town.WalkBounds.size, settlement.Id);
                Assert.AreEqual(w.Bounds.position - settlement.Offset, settlement.Town.WalkBounds.position, settlement.Id);
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Arrival), 1e-4f, $"{exit.Id}: the hero arrives on flat ground");
            }
        }

        // ------------------------------------------------------------------
        // Walking from the Vila to the other regions' towns
        // ------------------------------------------------------------------

        [Test]
        public void Trails_LeadOnFoot_FromTheVila_ToQocho_Litema_AndTheLamparinas()
        {
            OverworldLayout w = World;
            OverworldRelief relief = OverworldRelief.For(w);
            List<MapPath> paths = w.World.Paths;
            int count = paths.Count;

            // Trail pieces touch (end to end, or an end on another piece: a fork); a town joins every trail that ends in it.
            var links = new List<int>[count];
            for (int i = 0; i < count; i++)
            {
                links[i] = new List<int>();
            }

            var buckets = new Dictionary<Vector2Int, List<int>>();
            for (int i = 0; i < count; i++)
            {
                foreach (Vector2Int c in Cells(paths[i]))
                {
                    if (!buckets.TryGetValue(c, out List<int> list))
                    {
                        buckets[c] = list = new List<int>();
                    }

                    list.Add(i);
                }
            }

            for (int i = 0; i < count; i++)
            {
                foreach (Vector2Int c in Cells(paths[i]))
                {
                    foreach (int j in buckets[c])
                    {
                        if (j != i && Touch(paths[i], paths[j]))
                        {
                            links[i].Add(j);
                        }
                    }
                }
            }

            var townOf = new Dictionary<int, OverworldSettlement>();
            for (int i = 0; i < count; i++)
            {
                foreach (OverworldSettlement s in w.Settlements)
                {
                    Rect town = CityGeometry.Inflate(s.GroundRect, 10f);
                    if (town.Contains(paths[i].A) || town.Contains(paths[i].B))
                    {
                        townOf[i] = s;
                    }
                }
            }

            foreach (IGrouping<OverworldSettlement, KeyValuePair<int, OverworldSettlement>> group in townOf.GroupBy(p => p.Value))
            {
                List<int> ends = group.Select(p => p.Key).ToList();
                foreach (int a in ends)
                {
                    links[a].AddRange(ends.Where(b => b != a));
                }
            }

            OverworldSettlement vila = w.Vila;
            var report = new List<string>();
            foreach (string regionId in new[] { OverworldLayouts.AreiasId, OverworldLayouts.KaggenId, OverworldLayouts.LimiarId })
            {
                OverworldSettlement goal = w.Parts.Find(p => p.Id == regionId).Vila;
                List<int> route = Route(links, townOf, vila, goal);
                Assert.IsNotNull(route, $"no trail from the Vila to {goal.Id}");
                float length = 0f;
                float steepest = 0f;
                foreach (int i in route)
                {
                    MapPath path = paths[i];
                    float piece = Vector2.Distance(path.A, path.B);
                    length += piece;
                    for (float t = 0f; t < piece; t += 1.5f)
                    {
                        Vector2 a = Vector2.Lerp(path.A, path.B, t / piece);
                        Vector2 b = Vector2.Lerp(path.A, path.B, Mathf.Min(1f, (t + 1.5f) / piece));
                        float run = Vector2.Distance(a, b);
                        if (run < 0.2f)
                        {
                            continue;
                        }

                        float slope = Mathf.Atan(Mathf.Abs(relief.SurfaceAt(b) - relief.SurfaceAt(a)) / run) * Mathf.Rad2Deg;
                        steepest = Mathf.Max(steepest, slope);
                        Assert.Less(slope, 30f, $"to {goal.Id}: the trail at {a} is a wall ({slope:0}°)");
                        Assert.IsFalse(InRiverOffCrossing(w, a), $"to {goal.Id}: the trail wades a river at {a}");
                    }
                }

                report.Add($"Vila → {goal.Id}: {route.Count} trail pieces, {length:0} m, steepest {steepest:0}°");
                Assert.Less(length, 9000f, $"to {goal.Id}: a sensible road");
            }

            Debug.Log("[Overworld World] " + string.Join("; ", report));
        }

        // ------------------------------------------------------------------
        // Streaming and chunks
        // ------------------------------------------------------------------

        [Test]
        public void Chunks_AcrossTheBorders_AreWithinTheBudget_AndTheStreamingLoadsOnlyAroundTheHero()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            Assert.AreEqual(100, count.x);
            Assert.AreEqual(100, count.y);
            var wanted = new List<Vector2Int>();
            foreach (Vector2 pass in w.Passes)
            {
                OverworldRules.ChunksWithin(w, pass, OverworldRules.LoadRadius, wanted);
                Assert.Less(wanted.Count, 40, "only the chunks around the hero, whatever the region");
                Assert.IsTrue(wanted.Exists(c => w.PartAt(OverworldRules.ChunkRect(w, c).center) != w.PartAt(pass)), $"{pass}: the chunks across the border stream in too");
            }

            var builder = new TownMeshBuilder();
            OverworldChunkBuilder.BuildGround(builder, w, OverworldChunkBuilder.Content(w, OverworldRules.ChunkOf(w, Vector2.zero)));
            var times = new List<double>();
            int worst = 0;
            foreach (Vector2 pass in w.Passes)
            {
                Vector2Int c0 = OverworldRules.ChunkOf(w, pass);
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dz = -2; dz <= 2; dz++)
                    {
                        var c = c0 + new Vector2Int(dx, dz);
                        var watch = Stopwatch.StartNew();
                        OverworldChunkContent content = OverworldChunkBuilder.Content(w, c);
                        builder.Clear();
                        OverworldChunkBuilder.BuildGround(builder, w, content);
                        OverworldChunkBuilder.BuildProps(builder, content);
                        watch.Stop();
                        times.Add(watch.Elapsed.TotalMilliseconds);
                        worst = Mathf.Max(worst, builder.TriangleCount);
                        Assert.LessOrEqual(builder.TriangleCount, MaxTrianglesPerChunk, $"chunk {c}");
                    }
                }
            }

            Debug.Log($"[Overworld World] {times.Count} chunks at the passes: avg {times.Average():0.00} ms, worst {worst} triangles (Editor, Mono)");
            Assert.Less(times.Average(), 6.0);
        }

        // ------------------------------------------------------------------

        private static Vector2 Nearest(List<Vector2> points, Vector2 p) => points.OrderBy(q => Vector2.Distance(p, q)).First();

        private static IEnumerable<Vector2Int> Cells(MapPath path)
        {
            const float cell = 32f;
            int x0 = Mathf.FloorToInt((Mathf.Min(path.A.x, path.B.x) - 4f) / cell);
            int x1 = Mathf.FloorToInt((Mathf.Max(path.A.x, path.B.x) + 4f) / cell);
            int z0 = Mathf.FloorToInt((Mathf.Min(path.A.y, path.B.y) - 4f) / cell);
            int z1 = Mathf.FloorToInt((Mathf.Max(path.A.y, path.B.y) + 4f) / cell);
            for (int x = x0; x <= x1; x++)
            {
                for (int z = z0; z <= z1; z++)
                {
                    yield return new Vector2Int(x, z);
                }
            }
        }

        private static bool Touch(MapPath a, MapPath b)
        {
            float reach = Mathf.Max(a.Width, b.Width) * 0.5f + 0.75f;
            return MapLayoutRules.DistanceToSegment(a.A, b.A, b.B) <= reach || MapLayoutRules.DistanceToSegment(a.B, b.A, b.B) <= reach
                || MapLayoutRules.DistanceToSegment(b.A, a.A, a.B) <= reach || MapLayoutRules.DistanceToSegment(b.B, a.A, a.B) <= reach;
        }

        private static List<int> Route(List<int>[] links, Dictionary<int, OverworldSettlement> townOf, OverworldSettlement from, OverworldSettlement to)
        {
            var previous = new Dictionary<int, int>();
            var queue = new Queue<int>();
            foreach (KeyValuePair<int, OverworldSettlement> pair in townOf)
            {
                if (pair.Value == from)
                {
                    previous[pair.Key] = -1;
                    queue.Enqueue(pair.Key);
                }
            }

            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                if (townOf.TryGetValue(i, out OverworldSettlement town) && town == to)
                {
                    var route = new List<int>();
                    for (int k = i; k >= 0; k = previous[k])
                    {
                        route.Add(k);
                    }

                    route.Reverse();
                    return route;
                }

                foreach (int j in links[i])
                {
                    if (!previous.ContainsKey(j))
                    {
                        previous[j] = i;
                        queue.Enqueue(j);
                    }
                }
            }

            return null;
        }

        private static bool InRiverOffCrossing(OverworldLayout w, Vector2 p)
        {
            foreach (MapRiver river in w.World.Rivers)
            {
                if (MapLayoutRules.DistanceToPolyline(p, river.Points) >= river.Width * 0.5f - 0.5f)
                {
                    continue;
                }

                bool crossing = river.Crossings.Exists(c => Vector2.Distance(c.At, p) < Mathf.Max(c.Width, river.Width) * 0.5f + OverworldChunkBuilder.BridgeSpan(river));
                if (!crossing)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
