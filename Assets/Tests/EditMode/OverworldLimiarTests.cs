using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Rapadura.Tests
{
    /// <summary>
    /// The continuous Região 4 — Grutas do Limiar (Docs/Mapas100.md §0.10 and 19–24): one 2400 × 2400 m underground map with
    /// the Vila das Lamparinas inside (the hand-made 19 and 20 embedded as zones, 21–24 and the halls between them), zones
    /// that tile it, open passages, level bands growing with the distance from the Vila, the special gates of each map (soft
    /// barrier), gates on the right edges (Zarafshan east, Passo do Hahya west), clearings with the existing cave beasts
    /// (lantern fish included) and their night forms, the relief rules (flat towns and gates, level trails, plateaus), the
    /// cave dressing (walls between the halls, chasms with rope bridges, crystals, mushrooms, columns…) and the chunk budget.
    /// Same mould as <see cref="OverworldTests"/> and <see cref="OverworldReliefTests"/>.
    /// </summary>
    public class OverworldLimiarTests
    {
        private const float OldMapSide = 120f;
        private const int MaxTrianglesPerChunk = 12000;
        private const string Pedras = "map_021_pedras_dragao";
        private const string Mercado = "map_022_mercado_tuneis";
        private const string Canion = "map_023_canion_roda";
        private const string Ninho = "map_024_mahaway";
        private const string LanternFish = "enemy_cave_lantern_fish";

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.LimiarId);

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
        public void Region_IsOneHugeCave_WithTheVilaAndEveryMapOfTheRegion()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Bounds.width, OldMapSide * 20f);
            Assert.GreaterOrEqual(w.Bounds.height, OldMapSide * 20f);
            Assert.AreEqual(MapRegion.GrutasDoLimiar, w.Region);
            foreach (string id in new[] { MapLayouts.LagoLimiarId, MapLayouts.TagesId, Pedras, Mercado, Canion, Ninho })
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the cave");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Limiar");
                Assert.IsNotNull(w.SettlementOf(id), $"{id}: its town or outpost stands in the cave");
            }

            Assert.AreNotSame(w, OverworldLayouts.ForMap(MapCatalog.PomarId), "the Zarafshan is its own region");
            Assert.AreEqual(CityCatalog.Lamparinas, w.Vila.Id, "the Vila das Lamparinas is the region's town");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.AreEqual(MapLayouts.LagoLimiarId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "the Vila stands in the Lago do Limiar");
            OverworldSettlement market = w.SettlementOf(Mercado);
            Assert.AreEqual(MapCatalog.MercadoDosTuneis, market.Id);
            Assert.IsTrue(market.IsReturnCity, "the kobolds' market wakes the heroes fallen in 22–23");
            Assert.AreEqual(MapCatalog.ReturnCityOf(Canion), market.Id);
            Assert.AreEqual(MapCatalog.ReturnCityOf(Ninho), w.Vila.Id);
        }

        [Test]
        public void Region_HasManyHalls_EachWithItsOwnBeasts_AndTheirNightForms()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 10, "many kinds of cave");
            Assert.IsTrue(w.Zones.All(z => OverworldChunkBuilder.IsCave(z.Terrain)), "every area is underground");
            foreach (OverworldZone zone in w.Zones)
            {
                List<MapSpawnZone> spawns = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id] == zone).ToList();
                Assert.GreaterOrEqual(spawns.Count, 2, $"{zone.Id}: several monster groups");
                Assert.GreaterOrEqual(spawns.SelectMany(s => s.AllEntries()).Select(e => e.EnemyId).Distinct().Count(), 2, $"{zone.Id}: more than one beast");
                foreach (MapSpawnZone spawn in spawns)
                {
                    Assert.IsTrue(spawn.Night.Any(e => e.Form != NightForm.Normal), $"{spawn.Id}: its night table has the night forms (Ubır, variants)");
                }
            }

            Assert.GreaterOrEqual(w.World.Zones.Count, 300, "the cave is full of clearings");
            Assert.IsTrue(w.World.Zones.SelectMany(s => s.AllEntries()).Any(e => e.EnemyId == LanternFish), "the lantern fish swims in the lakes");
            Assert.IsTrue(w.World.Zones.SelectMany(s => s.Night).Any(e => e.SpeciesId == NightVariantRules.SpeciesMorcegoVampiro), "Ubır at night");
        }

        [Test]
        public void Zones_TileTheRegion_AndAreAllConnected()
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
            Assert.AreEqual("salao_de_tages", OverworldRules.ZoneAt(w, OverworldLayouts.TagesOffset).Id);
            Assert.AreEqual("campo_pedras_dragao", OverworldRules.ZoneAt(w, OverworldLayouts.PedrasDragaoPost).Id);
            Assert.AreEqual("mercado_dos_tuneis", OverworldRules.ZoneAt(w, OverworldLayouts.MercadoTuneisPost).Id);
            Assert.AreEqual("canion_da_roda", OverworldRules.ZoneAt(w, OverworldLayouts.TorreJarrasPost).Id);
            Assert.AreEqual("ninho_de_mahaway", OverworldRules.ZoneAt(w, OverworldLayouts.UltimaPontePost).Id);
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("lago_do_limiar"), w.Zone("salao_de_tages")), "the Vila's tunnel to Tages");
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("mercado_dos_tuneis"), w.Zone("campo_pedras_dragao")), "the market's west gallery");
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("mercado_dos_tuneis"), w.Zone("canion_da_roda")), "the market's east gallery");
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("mercado_dos_tuneis"), w.Zone("ninho_de_mahaway")), "down to the roost");

            var seen = new HashSet<OverworldZone> { w.Zone("lago_do_limiar") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every hall is reachable from the Vila on foot");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromTheVila_AndFollowTheCatalog()
        {
            OverworldLayout w = World;
            foreach (OverworldZone a in w.Zones)
            {
                foreach (OverworldZone b in w.Zones)
                {
                    if (OverworldRules.DistanceFromVila(w, b) >= OverworldRules.DistanceFromVila(w, a) + 250f)
                    {
                        Assert.GreaterOrEqual(b.MinLevel, a.MinLevel, $"{b.Id} is farther than {a.Id} but easier");
                    }
                }
            }

            List<OverworldZone> byDistance = w.Zones.OrderBy(z => OverworldRules.DistanceFromVila(w, z)).ToList();
            Assert.AreEqual(13, byDistance[0].MinLevel, "the Vila's hall is the 19's band");
            Assert.GreaterOrEqual(byDistance.Last().MaxLevel, 50, "the bottom of the caves is the region's end game");
            foreach (string id in new[] { Pedras, Mercado, Canion, Ninho })
            {
                MapDefinition map = MapCatalog.Get(id);
                foreach (OverworldZone zone in w.ZonesOfMap(id))
                {
                    Assert.GreaterOrEqual(zone.MinLevel, map.MonsterLevelMin, zone.Id);
                    Assert.LessOrEqual(zone.MaxLevel, map.MonsterLevelMax, zone.Id);
                    Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1100f, $"{zone.Id}: deep in the caves");
                }
            }
        }

        [Test]
        public void SoftBarrier_EachMapAsksForItsLight_OrItsWings()
        {
            OverworldLayout w = World;
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(w.Zone("salao_de_tages"), 20, CharacterId.Lavine, null), "the 20 asks for a torch");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("salao_de_tages"), 20, CharacterId.Lavine, t => t == MapRequirementTags.TorchRequired));
            Assert.AreEqual(MapRequirementTags.TorchRequired, w.Zone("lago_do_limiar").RequirementTag);
            Assert.AreEqual(MapRequirementTags.RunicLamp, w.Zone("campo_pedras_dragao").RequirementTag);
            Assert.AreEqual(MapRequirementTags.EternalFireOil, w.Zone("canion_da_roda").RequirementTag);
            Assert.AreEqual(MapRequirementTags.WingsOrDoubleJump, w.Zone("ninho_de_mahaway").RequirementTag);
            Assert.AreEqual(string.Empty, w.Zone("mercado_dos_tuneis").RequirementTag, "the market is open to everyone of its level");

            OverworldZone pedras = w.Zone("campo_pedras_dragao");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(pedras, 20, CharacterId.Lavine, t => true));
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(pedras, 40, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(pedras, 40, CharacterId.Lavine, t => t == MapRequirementTags.RunicLamp));

            // Maithe walks the rope-bridge chasms 5 levels early (mountain terrain bonus).
            OverworldZone chasm = w.Zone("fenda_do_bronze");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(chasm, chasm.WalkLevel - 1, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(chasm, chasm.WalkLevel - 1, CharacterId.Maithe, null));
        }

        [Test]
        public void Gates_OnTheRightEdges_BringTheHeroToTheMatchingGate()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            CollectionAssert.AreEquivalent(new[] { MapLayouts.HahyaId, MapCatalog.PomarId }, gates.Select(g => g.TargetMapId).ToArray());
            foreach (MapExitSpec gate in gates)
            {
                Assert.IsTrue(MapCatalog.Get(gate.TargetMapId).IsBuilt, gate.Id);
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(MapLayouts.LagoLimiarId, gate.TargetMapId, out _), $"coming back from {gate.TargetMapId}");
                Assert.AreEqual(MapLayouts.LagoLimiarId, OverworldRules.ZoneAt(w, gate.Arrival).MapIdOrOwner, $"{gate.Id}: a hall of the 19 (the map the gate leads back to)");
            }

            // The Zarafshan's gate on its west edge leads down to this cave's east edge, and back.
            OverworldLayout zarafshan = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            MapExitSpec down = zarafshan.World.Exits.Find(e => e.Kind == MapExitKind.Map && e.TargetMapId == MapLayouts.LagoLimiarId);
            Assert.IsNotNull(down);
            Vector2 arrival = w.ArrivalFor(MapLayouts.LagoLimiarId, OverworldRules.ZoneAt(zarafshan, down.Arrival).MapIdOrOwner, out Vector2 facing);
            Assert.Less(w.Bounds.xMax - arrival.x, 12f, "lands on the Limiar's east edge");
            Assert.Less(facing.x, 0f, "facing into the cave");
            Assert.AreEqual(down.Arrival, zarafshan.ArrivalFor(MapCatalog.PomarId, OverworldRules.ZoneAt(w, arrival).MapIdOrOwner, out _), "…and back to the Ravina's gate");
            MapExitSpec hahya = gates.Find(g => g.TargetMapId == MapLayouts.HahyaId);
            Assert.Less(hahya.Trigger.center.x - w.Bounds.xMin, 3f, "the Passo do Hahya is up the west edge");

            // The island's ring by the Vila, the Ossuário's barred portal in the colonnade, the Vila's dock by barca.
            Assert.IsTrue(w.World.Exits.Exists(e => e.Kind == MapExitKind.WorldMap && Vector2.Distance(e.Arrival, w.Vila.Offset) < 120f), "the island ring by the Vila");
            MapExitSpec ossuary = w.World.Exits.Find(e => e.Id == "exit_ossuario");
            Assert.IsNotNull(ossuary);
            Assert.Less(Vector2.Distance(ossuary.Trigger.center, OverworldLayouts.OssuaryPortal), 4f);
            Assert.IsTrue(w.Vila.SafeZone.Contains(w.ArrivalFor(MapLayouts.LagoLimiarId, null, out _)));
            Assert.AreEqual(w.SettlementOf(Mercado).DockArrival, w.ArrivalFor(Mercado, MapLayouts.LagoLimiarId, out _));
        }

        [Test]
        public void Spawns_StayOutOfSafeZones_WithExistingNonBossCaveBeasts()
        {
            OverworldLayout w = World;
            var checkedIds = new HashSet<string>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                OverworldZone zone = w.ZoneOfSpawn[spawn.Id];
                Assert.IsTrue(zone.Bounds.Contains(spawn.Center), spawn.Id);
                Assert.GreaterOrEqual(OverworldRules.DistanceToSafety(w, spawn.Center) - spawn.Radius, MapLayoutRules.SpawnSafeMargin, spawn.Id);
                foreach (PeriodSpawnEntry entry in spawn.AllEntries())
                {
                    if (!checkedIds.Add(entry.EnemyId))
                    {
                        continue;
                    }

                    EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{entry.EnemyId}");
                    Assert.IsNotNull(definition, $"{spawn.Id}: '{entry.EnemyId}' not in Resources/Enemies");
                    Assert.IsFalse(definition.IsBoss, $"{spawn.Id}: no boss in the open cave ({entry.EnemyId})");
                }
            }
        }

        [Test]
        public void Relief_FlatTownsAndGates_LevelTrails_PlateauClearings_AndCaveShapes()
        {
            OverworldLayout w = World;
            OverworldRelief relief = OverworldRelief.For(w);
            foreach (OverworldSettlement settlement in w.Settlements)
            {
                Rect r = settlement.GroundRect;
                for (float x = r.xMin; x <= r.xMax; x += 3f)
                {
                    for (float z = r.yMin; z <= r.yMax; z += 3f)
                    {
                        Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, new Vector2(x, z)), 1e-4f, $"{settlement.Id}: the town kit stands at y = 0");
                    }
                }
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Trigger.center), 1e-4f, exit.Id);
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Arrival), 1e-4f, exit.Id);
            }

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
                    if (left.Free >= 0.999f && right.Free >= 0.999f)
                    {
                        cross.Add(Mathf.Abs(left.Height - right.Height) / path.Width);
                    }
                }

                float along = Mathf.Abs(relief.HeightAt(path.B) - relief.HeightAt(path.A)) / length;
                Assert.Less(Mathf.Atan(along) * Mathf.Rad2Deg, 30f, $"trail {path.A} → {path.B} too steep");
            }

            cross.Sort();
            Assert.Less(cross[cross.Count / 2], 0.005f, "a trail is level across its width (median)");
            Assert.Less(cross.Last(), 0.3f, "no trail leans like a hillside");

            int flat = 0;
            foreach (MapSpawnZone zone in w.World.Zones)
            {
                float h0 = OverworldRules.SurfaceAt(w, zone.Center);
                float worst = 0f;
                for (int k = 0; k < 8; k++)
                {
                    float a = k * Mathf.PI / 4f;
                    worst = Mathf.Max(worst, Mathf.Abs(OverworldRules.SurfaceAt(w, zone.Center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * zone.Radius * 0.7f) - h0));
                }

                flat += worst < 0.35f ? 1 : 0;
                Assert.Less(worst, 3f, $"{zone.Id}: its monsters fight on level ground");
            }

            Assert.Greater(flat / (float)w.World.Zones.Count, 0.85f, "almost every clearing is a flat plateau");
            Assert.Greater(MaxHeight(relief, w.Zone("ninho_de_mahaway")), 15f, "Mahaway's roost climbs in ledges");
            Assert.Greater(MaxHeight(relief, w.Zone("terracos_de_tufo")), 12f, "the tuff terraces step up");
            Assert.Greater(MaxHeight(relief, w.Zone("fenda_do_bronze")), 10f, "the rifts cut high ground");
            Assert.Less(MaxHeight(relief, w.Zone("mercado_dos_tuneis")), MaxHeight(relief, w.Zone("ninho_de_mahaway")) - 5f, "the market's tunnels are level ground");
        }

        [Test]
        public void Dressing_EveryHallHasItsOwnLook_WallsChasmsAndGlow()
        {
            OverworldLayout w = World;
            var expected = new Dictionary<string, OverworldDecor[]>
            {
                { "campo_de_cristais", new[] { OverworldDecor.Crystal } },
                { "bosque_de_cogumelos", new[] { OverworldDecor.GiantMushroom } },
                { "colunata_do_lago", new[] { OverworldDecor.CaveColumn } },
                { "salao_de_tages", new[] { OverworldDecor.Stalagmite } },
                { "campo_pedras_dragao", new[] { OverworldDecor.Vishapakar } },
                { "mercado_dos_tuneis", new[] { OverworldDecor.MineCart } },
                { "canion_da_roda", new[] { OverworldDecor.Jar } },
                { "ninho_de_mahaway", new[] { OverworldDecor.Nest } },
                { "abismo_oriental", new[] { OverworldDecor.Abyss } },
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

                    AssertStandsClear(w, content);
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

                // The light of the caves: glowing moss or mushrooms in every hall's ground detail.
                OverworldTerrainStyle style = OverworldTerrains.For(zone.Terrain);
                Assert.Greater(Mathf.Max(TownPalette.GlowOf(style.TuftColor), TownPalette.GlowOf(style.TuftTip)), 0f, $"{zone.Id}: glowing moss");
            }

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

            // The walls between the halls (with gaps at every trail), the chasms with their rope bridges, the monuments.
            int walls = 0;
            int ropePosts = 0;
            var monuments = new HashSet<OverworldDecor>();
            Vector2Int count = OverworldRules.ChunkCount(w);
            for (int x = 0; x < count.x; x++)
            {
                for (int y = 0; y < count.y; y++)
                {
                    OverworldChunkContent content = OverworldChunkBuilder.Content(w, new Vector2Int(x, y));
                    foreach (OverworldProp prop in content.Props)
                    {
                        walls += prop.Decor == OverworldDecor.TuffWall ? 1 : 0;
                        ropePosts += prop.Decor == OverworldDecor.RopePosts ? 1 : 0;
                        if (prop.Decor == OverworldDecor.JarWheel || prop.Decor == OverworldDecor.OssuaryPortal || prop.Decor == OverworldDecor.Vishapakar)
                        {
                            monuments.Add(prop.Decor);
                        }

                        if (prop.Decor == OverworldDecor.TuffWall)
                        {
                            Assert.IsFalse(MapLayoutRules.OnPath(w.World, (prop.Position + prop.End) * 0.5f, 2f), $"a wall across a trail at {prop.Position}");
                        }
                    }
                }
            }

            Assert.Greater(walls, 600, "tall tuff walls between the halls");
            int abyssBridges = w.World.Rivers.Where(r => OverworldChunkBuilder.IsAbyss(w, r)).Sum(r => r.Crossings.Count(c => c.IsBridge));
            Assert.GreaterOrEqual(abyssBridges, 5, "rope bridges over the chasms");
            Assert.AreEqual(abyssBridges * 2, ropePosts, "rope posts at both ends of every chasm bridge");
            CollectionAssert.IsSupersetOf(monuments, new[] { OverworldDecor.JarWheel, OverworldDecor.OssuaryPortal, OverworldDecor.Vishapakar });
        }

        [Test]
        public void EveryChunk_StaysWithinTheBudget_IsDeterministic_AndBuildsFast()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            Assert.AreEqual(50, count.x);
            Assert.AreEqual(50, count.y);
            var builder = new TownMeshBuilder();
            OverworldChunkBuilder.Content(w, new Vector2Int(10, 10));
            var times = new List<double>();
            int worst = 0;
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
                    worst = Mathf.Max(worst, triangles);
                    Assert.LessOrEqual(triangles, MaxTrianglesPerChunk, $"chunk {c}: {triangles} triangles");
                }
            }

            foreach (Vector2 p in new[] { new Vector2(0f, -560f), new Vector2(-500f, -1475f), new Vector2(950f, -560f), new Vector2(0f, -1960f) })
            {
                Vector2Int c = OverworldRules.ChunkOf(w, p);
                OverworldChunkContent a = OverworldChunkBuilder.Content(w, c);
                OverworldChunkContent b = OverworldChunkBuilder.Content(w, c);
                Assert.AreEqual(a.Props.Count, b.Props.Count);
                Assert.AreEqual(a.Details.Count, b.Details.Count);
                CollectionAssert.AreEqual(a.Grid.Height, b.Grid.Height);
            }

            times.Sort();
            Debug.Log($"[Overworld Limiar] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst}; generation avg {times.Average():0.00} ms, " +
                $"p95 {times[(int)(times.Count * 0.95f)]:0.00} ms (Editor, Mono)");
            Assert.Less(times.Average(), 6.0, "chunk generation stays cheap even in the Editor");
        }

        /// <summary>Props and details off the trails, rivers and safe zones; every collider on the relief.</summary>
        private static void AssertStandsClear(OverworldLayout w, OverworldChunkContent content)
        {
            foreach (OverworldProp prop in content.Props)
            {
                if (prop.Decor == OverworldDecor.RopePosts || !w.Bounds.Contains(prop.Position))
                {
                    continue;
                }

                Assert.IsFalse(MapLayoutRules.OnPath(w.World, prop.Position, 0f), $"{prop.Decor}/{prop.Kind} on a trail at {prop.Position}");
                Assert.IsFalse(MapLayoutRules.InRiver(w.World, prop.Position, 0f), $"{prop.Decor}/{prop.Kind} in a river at {prop.Position}");
                Assert.IsNull(OverworldRules.SettlementAt(w, prop.Position), $"{prop.Decor} in a safe zone at {prop.Position}");
            }

            foreach (OverworldProp detail in content.Details)
            {
                if (detail.Decor == OverworldDecor.Abyss)
                {
                    continue;
                }

                Assert.IsFalse(MapLayoutRules.OnPath(w.World, detail.Position, 0f), $"{detail.Decor} on a trail at {detail.Position}");
                Assert.IsFalse(MapLayoutRules.InRiver(w.World, detail.Position, 0f), $"{detail.Decor} in a river at {detail.Position}");
            }

            foreach (OverworldSolid solid in content.Solids)
            {
                float ground = content.Grid.Sample(solid.Center);
                Assert.LessOrEqual(solid.Base, ground + 0.01f, $"{content.Coord}: collider floating at {solid.Center}");
                Assert.Greater(solid.Base + solid.Height, ground + 0.3f, $"{content.Coord}: collider sunk at {solid.Center}");
            }
        }

        private static float MaxHeight(OverworldRelief relief, OverworldZone zone)
        {
            float max = float.MinValue;
            for (float x = zone.Bounds.xMin + 2f; x < zone.Bounds.xMax; x += 6f)
            {
                for (float z = zone.Bounds.yMin + 2f; z < zone.Bounds.yMax; z += 6f)
                {
                    max = Mathf.Max(max, relief.HeightAt(new Vector2(x, z)));
                }
            }

            return max;
        }

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
