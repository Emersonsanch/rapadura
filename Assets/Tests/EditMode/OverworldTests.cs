using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The continuous Região 1 (Docs/Mapas100.md §0.10): one 2400 × 2400 m map, the start area around the Vila at least
    /// 20× the old Pomar, zones that tile it without overlapping, zone by position, open passages between neighbours, level
    /// bands that grow with the distance from the Vila, the soft barrier rules, the chunk streaming, spawn clearings outside
    /// the safe zones with existing non-boss monsters, gates out of the region and a varied, deterministic chunk content.
    /// </summary>
    public class OverworldTests
    {
        private const float OldPomarSide = 120f;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.ZarafshanId);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [Test]
        public void Region_IsValid()
        {
            List<string> problems = OverworldRules.Validate(World);
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void Sizes_RegionAtLeast20TimesTheSide_StartAreaAtLeast20TimesTheArea()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Bounds.width, OldPomarSide * 20f, "the whole region: ≥ 20× the old Pomar's side");
            Assert.GreaterOrEqual(w.Bounds.height, OldPomarSide * 20f);

            List<OverworldZone> start = w.ZonesOfMap(MapCatalog.PomarId).ToList();
            Assert.GreaterOrEqual(start.Count, 2, "the Pomar and the cultivated fields");
            Rect box = start[0].Bounds;
            float area = 0f;
            foreach (OverworldZone zone in start)
            {
                box = Rect.MinMaxRect(Mathf.Min(box.xMin, zone.Bounds.xMin), Mathf.Min(box.yMin, zone.Bounds.yMin), Mathf.Max(box.xMax, zone.Bounds.xMax), Mathf.Max(box.yMax, zone.Bounds.yMax));
                area += zone.Bounds.width * zone.Bounds.height;
                Assert.LessOrEqual(zone.MaxLevel, 3, $"{zone.Id}: the start area is level 1–3");
            }

            Assert.GreaterOrEqual(area, OldPomarSide * OldPomarSide * 20f, "start area ≥ 20× the old Pomar's area");
            Assert.GreaterOrEqual(box.width, 540f);
            Assert.GreaterOrEqual(box.height, 540f);
            Assert.IsTrue(box.Contains(w.Vila.Offset), "the Vila is inside the start area");
        }

        [Test]
        public void Region_HasManyDistinctAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 12);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 10, "many kinds of terrain");
            foreach (string id in new[] { MapCatalog.PomarId, MapCatalog.TugaiId, MapLayouts.BrejoId, MapLayouts.AmoreirasId, MapLayouts.GarajonayId, MapLayouts.MihrshahId })
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the region");
                Assert.IsTrue(OverworldLayouts.IsContinuous(id));
                Assert.IsNotNull(w.SettlementOf(id), $"{id}: its settlement stands in the region");
            }

            Assert.IsFalse(w.ContainsMap(MapLayouts.CaravanasId), "07 is in the Areias overworld, not this one");

            foreach (OverworldZone zone in w.Zones)
            {
                List<MapSpawnZone> spawns = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id] == zone).ToList();
                Assert.GreaterOrEqual(spawns.Count, 2, $"{zone.Id}: several monster groups");
                int species = spawns.SelectMany(s => s.AllEntries()).Select(e => e.EnemyId).Distinct().Count();
                Assert.GreaterOrEqual(species, 2, $"{zone.Id}: more than one kind of beast");
            }

            Assert.GreaterOrEqual(w.World.Zones.Count, 150, "the region is full of clearings, not an empty stretched field");
        }

        [Test]
        public void Zones_TileTheRegion_WithoutOverlap_AndZoneAtFindsThem()
        {
            OverworldLayout w = World;
            for (int i = 0; i < w.Zones.Count; i++)
            {
                for (int j = i + 1; j < w.Zones.Count; j++)
                {
                    Assert.IsFalse(w.Zones[i].Bounds.Overlaps(w.Zones[j].Bounds), $"{w.Zones[i].Id} overlaps {w.Zones[j].Id}");
                }
            }

            Assert.AreEqual(w.Bounds.width * w.Bounds.height, w.Zones.Sum(z => z.Bounds.width * z.Bounds.height), 1f, "every metre belongs to an area");
            Assert.AreEqual("pomar", OverworldRules.ZoneAt(w, w.Vila.Offset).Id, "the Vila stands in the Pomar");
            Assert.AreEqual("campos_do_aryk", OverworldRules.ZoneAt(w, new Vector2(0f, -200f)).Id);
            Assert.AreEqual("tugai_margem", OverworldRules.ZoneAt(w, OverworldLayouts.MoinhoOffset).Id);
            Assert.AreEqual("brejo", OverworldRules.ZoneAt(w, OverworldLayouts.BrejoOffset).Id);
            Assert.IsNull(OverworldRules.ZoneAt(w, new Vector2(w.Bounds.xMax + 5f, 0f)), "outside the walls: no zone");
            Assert.AreEqual("pomar", OverworldRules.ZoneAt(w, new Vector2(0f, -100f)).Id, "a border belongs to exactly one zone");
            foreach (OverworldZone zone in w.Zones)
            {
                Assert.AreSame(zone, OverworldRules.ZoneAt(w, zone.Bounds.center));
            }
        }

        [Test]
        public void Neighbours_HaveOpenPassages_AndTheRegionIsConnected()
        {
            OverworldLayout w = World;
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("pomar"), w.Zone("campos_do_aryk")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("pomar"), w.Zone("tugai_margem")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("tugai_margem"), w.Zone("brejo")), "the Tugai leads on to the Brejo");
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("brejo"), w.Zone("amoreiras")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("amoreiras"), w.Zone("garajonay")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("garajonay"), w.Zone("jardim_mihrshah")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("pomar"), w.Zone("brejo")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("pomar"), w.Zone("pomar")));

            // Breadth-first over the open passages: every area is reachable from the Vila on foot.
            var seen = new HashSet<OverworldZone> { w.Zone("pomar") };
            var queue = new Queue<OverworldZone>(seen);
            while (queue.Count > 0)
            {
                foreach (OverworldZone next in OverworldRules.Neighbours(w, queue.Dequeue()))
                {
                    if (seen.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to the Vila");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromTheVila()
        {
            OverworldLayout w = World;
            foreach (OverworldZone a in w.Zones)
            {
                foreach (OverworldZone b in w.Zones)
                {
                    float da = OverworldRules.DistanceFromVila(w, a);
                    float db = OverworldRules.DistanceFromVila(w, b);
                    if (db >= da + 250f)
                    {
                        Assert.GreaterOrEqual(b.MinLevel, a.MinLevel, $"{b.Id} ({db:0} m) is farther than {a.Id} ({da:0} m) but easier");
                    }
                }
            }

            List<OverworldZone> byDistance = w.Zones.OrderBy(z => OverworldRules.DistanceFromVila(w, z)).ToList();
            Assert.AreEqual(1, byDistance[0].MinLevel, "level 1 at the Vila");
            Assert.Greater(byDistance.Last().MinLevel, 20, "the far end is endgame of the region");
            Assert.AreEqual(16, w.Zone("brejo").MinLevel, "the Brejo (16–22) is far from the Vila");
            Assert.Greater(OverworldRules.DistanceFromVila(w, w.Zone("brejo")), 600f);

            // Inside an area, the clearings farther from the Vila are never easier than the nearest one.
            foreach (OverworldZone zone in w.Zones)
            {
                List<MapSpawnZone> generated = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id] == zone && s.Id.StartsWith(zone.Id + "_")).OrderBy(s => s.Center.magnitude).ToList();
                if (generated.Count > 1)
                {
                    Assert.LessOrEqual(generated.First().MinLevel, generated.Last().MinLevel, zone.Id);
                }
            }
        }

        [Test]
        public void SoftBarrier_WalkLevelHeroBonusAndSpecialGear()
        {
            OverworldLayout w = World;
            OverworldZone brejo = w.Zone("brejo");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(brejo, 1, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(brejo, 30, CharacterId.Lavine, null), "the Brejo asks for leather boots");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(brejo, 30, CharacterId.Lavine, tag => tag == MapRequirementTags.LeatherBoots));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("pomar"), 1, null, null));
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(w.Zone("jardim_mihrshah"), 40, null, null), "the garden key");

            // Maithe walks into the mountain areas 5 levels early (Docs/MetodologiaMapasERacas.md §3).
            OverworldZone altos = w.Zone("altos_do_mensageiro");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(altos, altos.WalkLevel - 1, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(altos, altos.WalkLevel - 1, CharacterId.Maithe, null));
        }

        [Test]
        public void Gates_LeadOutOfTheRegion_AndBringTheHeroBackToThem()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            CollectionAssert.AreEquivalent(new[] { MapLayouts.CaravanasId, MapLayouts.RondaveisId, MapLayouts.LagoLimiarId }, gates.Select(g => g.TargetMapId).ToArray());
            foreach (MapExitSpec gate in gates)
            {
                Assert.IsTrue(MapCatalog.Get(gate.TargetMapId).IsBuilt && MapLayouts.Has(gate.TargetMapId), gate.Id);
                Vector2 c = gate.Trigger.center;
                float toEdge = Mathf.Min(Mathf.Min(c.x - w.Bounds.xMin, w.Bounds.xMax - c.x), Mathf.Min(c.y - w.Bounds.yMin, w.Bounds.yMax - c.y));
                Assert.Less(toEdge, 3f, $"{gate.Id}: on the region's edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(MapCatalog.PomarId, gate.TargetMapId, out _), $"coming back from {gate.TargetMapId}");
            }

            // 07's way to the Pomar lands at the south gate, in the Estepe below the Vila.
            Assert.AreEqual("estepe_de_ohrmazd", OverworldRules.ZoneAt(w, w.ArrivalFor(MapCatalog.PomarId, MapLayouts.CaravanasId, out _)).Id);
            // By barca (or a new game): the Vila's dock, inside its safe zone.
            Vector2 arrival = w.ArrivalFor(MapCatalog.PomarId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(arrival));
            // From a neighbouring area of the same region nothing changes scene: the barca arrives at that area's dock.
            Assert.AreEqual(w.SettlementOf(MapLayouts.BrejoId).DockArrival, w.ArrivalFor(MapLayouts.BrejoId, MapCatalog.TugaiId, out _));
        }

        [Test]
        public void Spawns_StayOutOfSafeZones_InsideTheirArea_WithExistingNonBossMonsters()
        {
            OverworldLayout w = World;
            var checkedIds = new HashSet<string>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                OverworldZone zone = w.ZoneOfSpawn[spawn.Id];
                Assert.IsTrue(zone.Bounds.Contains(spawn.Center), spawn.Id);
                Assert.GreaterOrEqual(OverworldRules.DistanceToSafety(w, spawn.Center) - spawn.Radius, MapLayoutRules.SpawnSafeMargin, spawn.Id);
                Assert.IsNull(OverworldRules.SettlementAt(w, spawn.Center), spawn.Id);
                foreach (var entry in spawn.AllEntries())
                {
                    if (!checkedIds.Add(entry.EnemyId))
                    {
                        continue;
                    }

                    EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{entry.EnemyId}");
                    Assert.IsNotNull(definition, $"{spawn.Id}: '{entry.EnemyId}' not in Resources/Enemies");
                    Assert.IsFalse(definition.IsBoss, $"{spawn.Id}: no boss in the open world ({entry.EnemyId})");
                }
            }
        }

        [Test]
        public void Chunks_OnlyTheOnesAroundTheHeroAreActive()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            Assert.AreEqual(50, count.x);
            Assert.AreEqual(50, count.y);
            var around = new List<Vector2Int>();
            OverworldRules.ChunksWithin(w, Vector2.zero, OverworldRules.LoadRadius, around);
            Assert.Contains(OverworldRules.ChunkOf(w, Vector2.zero), around);
            Assert.AreEqual(OverworldRules.ChunkOf(w, Vector2.zero), around[0], "nearest first");
            Assert.LessOrEqual(around.Count, 40, "a few dozen chunks around the hero, never the whole 1 km²");
            Assert.IsFalse(around.Contains(OverworldRules.ChunkOf(w, new Vector2(400f, 0f))), "far chunks stay unbuilt");
            foreach (Vector2Int c in around)
            {
                Assert.LessOrEqual(OverworldRules.DistanceToChunk(w, Vector2.zero, c), OverworldRules.LoadRadius);
                Assert.IsTrue(OverworldRules.ChunkRect(w, c).Contains(OverworldRules.ChunkRect(w, c).center));
                Assert.AreEqual(c, OverworldRules.ChunkOf(w, OverworldRules.ChunkRect(w, c).center));
            }

            // The backdrop ring outside the walls exists (seen by the camera) but has no colliders.
            var corner = new Vector2Int(-1, -1);
            Assert.IsTrue(OverworldRules.IsValidChunk(w, corner));
            Assert.IsFalse(OverworldRules.IsInnerChunk(w, corner));
            Assert.IsFalse(OverworldRules.IsValidChunk(w, new Vector2Int(-2, 0)));

            // Spawners: awake near the hero, asleep far, with hysteresis.
            var spawn = new MapSpawnZone { Center = new Vector2(80f, 0f), Radius = 10f };
            Assert.IsTrue(OverworldRules.SpawnerShouldRun(spawn, Vector2.zero, false));
            Assert.IsFalse(OverworldRules.SpawnerShouldRun(spawn, new Vector2(-30f, 0f), false));
            Assert.IsTrue(OverworldRules.SpawnerShouldRun(spawn, new Vector2(-30f, 0f), true), "hysteresis: an awake spawner stays awake a little farther");
            Assert.IsFalse(OverworldRules.SpawnerShouldRun(spawn, new Vector2(-300f, 0f), true));
        }

        [Test]
        public void ChunkContent_IsDeterministic_Varied_AndKeepsOffTrailsRiversAndTowns()
        {
            OverworldLayout w = World;
            var samples = new[]
            {
                new Vector2(-120f, 160f), new Vector2(0f, -230f), new Vector2(500f, 0f), new Vector2(-520f, -300f), new Vector2(-850f, -400f),
                new Vector2(880f, 400f), new Vector2(-400f, 1100f), new Vector2(0f, -800f), new Vector2(880f, -800f), new Vector2(-900f, 600f),
            };
            int totalProps = 0;
            var swatches = new HashSet<TownSwatch>();
            foreach (Vector2 p in samples)
            {
                Vector2Int c = OverworldRules.ChunkOf(w, p);
                OverworldChunkContent a = OverworldChunkBuilder.Content(w, c);
                OverworldChunkContent b = OverworldChunkBuilder.Content(w, c);
                Assert.AreEqual(a.Props.Count, b.Props.Count, $"chunk {c}: deterministic");
                totalProps += a.Props.Count;
                foreach (OverworldProp prop in a.Props)
                {
                    if (prop.Kind == CityPropKind.FenceRun)
                    {
                        continue;
                    }

                    Assert.IsFalse(MapLayoutRules.OnPath(w.World, prop.Position, 0f), $"chunk {c}: prop on a trail at {prop.Position}");
                    Assert.IsFalse(MapLayoutRules.InRiver(w.World, prop.Position, 0f), $"chunk {c}: prop in a river at {prop.Position}");
                    Assert.IsNull(OverworldRules.SettlementAt(w, prop.Position), $"chunk {c}: prop in a safe zone at {prop.Position}");
                }

                Rect r = OverworldRules.ChunkRect(w, c);
                for (float x = r.xMin + 1f; x < r.xMax; x += 6f)
                {
                    for (float z = r.yMin + 1f; z < r.yMax; z += 6f)
                    {
                        var q = new Vector2(x, z);
                        swatches.Add(OverworldTerrains.GroundAt(OverworldTerrains.TerrainAt(w, q), q));
                    }
                }

                var builder = new TownMeshBuilder();
                OverworldChunkBuilder.BuildGround(builder, w, a);
                Assert.Greater(builder.TriangleCount, 0, $"chunk {c}: ground");
                builder.Clear();
                OverworldChunkBuilder.BuildProps(builder, a);
                Assert.Less(builder.TriangleCount, 60000, $"chunk {c}: props mesh within the mobile budget");
            }

            Assert.Greater(totalProps, 300, "the region is dressed");
            Assert.GreaterOrEqual(swatches.Count, 10, "many ground colours across the areas");

            // Inside one area the ground mixes colours organically (not a single flat colour).
            var orchard = new HashSet<TownSwatch>();
            for (int i = 0; i < 400; i++)
            {
                var q = new Vector2(-280f + (i % 20) * 13f, 60f + (i / 20) * 13f);
                orchard.Add(OverworldTerrains.GroundAt(OverworldTerrain.Orchard, q));
            }

            Assert.GreaterOrEqual(orchard.Count, 2);
        }

        [Test]
        public void Rivers_BlockExceptAtCrossings_AndTrailsCrossThemThere()
        {
            OverworldLayout w = World;
            MapRiver dourado = w.World.Rivers.OrderByDescending(r => r.Points.Count).First();
            Assert.GreaterOrEqual(dourado.Points.Count, 15, "the Rio Dourado runs the whole region");
            Assert.GreaterOrEqual(dourado.Crossings.Count(c => c.IsBridge), 3, "bridges on the golden river");
            foreach (MapRiver river in w.World.Rivers)
            {
                foreach ((Vector2 center, float length, float yaw) in MapLayoutRules.RiverBlockers(river))
                {
                    Assert.IsFalse(MapLayoutRules.OnCrossing(river, center), $"blocker on a crossing at {center}");
                }
            }
        }
    }
}
