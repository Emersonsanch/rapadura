using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Rapadura.Tests
{
    /// <summary>
    /// The continuous Região 15 — Limiar Profundo (Docs/Mapas100.md §0.10, maps 83–90), in the mould of <see cref="OverworldProfundoTests"/>: one 2400 × 2400 m map with
    /// the Fortaleza da Última Lâmpada inside (flat ground of ±30 m at y = 0, roads W, E, S) and the posts of 84–90, maps 83–90 as zones, zones that tile it, level bands
    /// growing with the distance from the fortress from the catalog's 340, the soft barrier (a Theriac in the smoke, Eternal Fire Oil in the fire, Canvas Wings in the wind,
    /// an Air Bubble in the mire, a Runic Lamp in the dark, a Balance Pole on the bridge, a lit lamp at the gate; nothing at the city nor at the entrances), the two gates towards
    /// the Margens da Mistura (west edge), the pits' beasts (no boss), the relief (the shaft, the causeway and the labyrinth standing up, the fortress low; pools of black water)
    /// and the per-area dressing.
    /// </summary>
    public class OverworldProfundoTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.ProfundoRegionId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] ProfundoMaps =
        {
            OverworldLayouts.FortalezaUltimaLampadaMapId, OverworldLayouts.CovaFumacaMapId, OverworldLayouts.CovaFogoMapId, OverworldLayouts.CovaVentoMapId,
            OverworldLayouts.CovaLodoMapId, OverworldLayouts.CovaTrevaMapId, OverworldLayouts.PonteNoveloMapId, OverworldLayouts.PortaoTrevaMapId,
        };

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
        public void Catalog_HasMaps83To90_OnTheDeepThreshold_ReturningToTheFortress()
        {
            CollectionAssert.Contains(MapCatalog.Regions.ToList(), MapRegion.LimiarProfundo);
            CollectionAssert.AreEqual(new[] { 83, 84, 85, 86, 87, 88, 89, 90 }, MapCatalog.InRegion(MapRegion.LimiarProfundo).Select(m => m.MapId).ToArray());
            foreach (string map in ProfundoMaps)
            {
                Assert.AreEqual(MapCatalog.FortalezaUltimaLampada, MapCatalog.ReturnCityOf(map), map);
                Assert.IsTrue(MapCatalog.Get(map).IsBuilt, map);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get(OverworldLayouts.FortalezaUltimaLampadaMapId).Settlement);
            Assert.AreEqual(340, MapCatalog.Get(OverworldLayouts.FortalezaUltimaLampadaMapId).MonsterLevelMin);
            Assert.AreEqual(440, MapCatalog.Get(OverworldLayouts.PortaoTrevaMapId).MonsterLevelMax);
            Assert.AreEqual("Limiar Profundo", MapTexts.RegionName(null, MapRegion.LimiarProfundo));
            Assert.AreEqual("Fortaleza da Última Lâmpada", MapTexts.Get(null, MapTexts.ReturnCityKey(MapCatalog.FortalezaUltimaLampada)));
            Assert.IsNotNull(MapCatalog.RespawnCityFor(OverworldLayouts.PortaoTrevaMapId));
        }

        [Test]
        public void Region_IsAsBigAsTheOthers_WithTheFortressInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.LimiarProfundo, w.Region);
            Assert.AreEqual(MapCatalog.FortalezaUltimaLampada, w.Vila.Id, "the city is the region's town (its first map, level 340)");
            Assert.AreEqual(CityTownRecipes.FortalezaUltimaLampadaCityId, w.Vila.Recipe.CityId, "its own recipe");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, CityTownRecipes.UltimaLampadaSolidRadius + 5f, "room for the town's recipe");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais das Balsas");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: open land all round it");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, OverworldLayouts.FortalezaProfundoGround, "±30 m of flat ground reserved for the city's recipe");
            Assert.AreEqual(Vector2.zero, w.Vila.Offset);
            Assert.AreEqual(OverworldLayouts.FortalezaUltimaLampadaMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "the town stands in its own area (83)");

            foreach (string map in ProfundoMaps)
            {
                OverworldSettlement stop = w.SettlementOf(map);
                Assert.IsNotNull(stop, $"{map}: its town or post");
                Assert.AreEqual(map, OverworldRules.ZoneAt(w, stop.Offset).MapId, $"{stop.Id} stands in its map's area");
                Assert.AreEqual(MapCatalog.Get(map).SettlementId, stop.Id, $"{map}: the catalog's settlement id");
            }

            Assert.AreEqual(1, w.Settlements.Count(s => s.IsReturnCity), "one capital, seven posts");
            Assert.AreEqual(8, w.Settlements.Count);
            Assert.Greater(w.SettlementOf(OverworldLayouts.PortaoTrevaMapId).Offset.x, 1400f, "the Selo dos Artífices is far east");
        }

        [Test]
        public void Region_HoldsMaps83To90_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.AreEqual(25, w.Zones.Count);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 24, "one ground per area: lamps, smoke, fire, wind, mire, dark, bridge, gate");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsProfundo(z.Terrain)), "the Limiar Profundo's own terrains");
            foreach (string id in ProfundoMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the Limiar Profundo");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Limiar Profundo");
                Assert.IsTrue(w.SettlementOf(id).HasDock, $"{id}: the barca stops there");
                Assert.IsTrue(MapLayouts.IsOverworldOnly(id), $"{id}: a zone of the overworld only");
            }

            foreach (OverworldZone zone in w.Zones)
            {
                List<MapSpawnZone> spawns = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id] == zone).ToList();
                Assert.GreaterOrEqual(spawns.Count, 2, $"{zone.Id}: several monster groups");
                int species = spawns.SelectMany(s => s.AllEntries()).Select(e => e.EnemyId).Distinct().Count();
                Assert.GreaterOrEqual(species, 2, $"{zone.Id}: more than one kind of beast");
                Assert.IsTrue(spawns.Any(s => s.Night != null && s.Night.Length > 0), $"{zone.Id}: a night variant");
            }

            Assert.GreaterOrEqual(w.World.Zones.Count, 150, "the pits are full of clearings");
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
            Assert.AreEqual("fortaleza_ultima_lampada", OverworldRules.ZoneAt(w, OverworldLayouts.FortalezaUltimaLampadaOffset).Id);
            Assert.AreEqual("cova_da_fumaca", OverworldRules.ZoneAt(w, OverworldLayouts.LampadaFumacaOffset).Id);
            Assert.AreEqual("cova_do_fogo_devorador", OverworldRules.ZoneAt(w, OverworldLayouts.LampadaFogoOffset).Id);
            Assert.AreEqual("cova_do_vento_destruidor", OverworldRules.ZoneAt(w, OverworldLayouts.LampadaVentoOffset).Id);
            Assert.AreEqual("cova_do_lodo", OverworldRules.ZoneAt(w, OverworldLayouts.LampadaLodoOffset).Id);
            Assert.AreEqual("ponte_do_novelo", OverworldRules.ZoneAt(w, OverworldLayouts.PilarDoMeioOffset).Id);
            foreach (OverworldZone zone in w.Zones)
            {
                Assert.AreSame(zone, OverworldRules.ZoneAt(w, zone.Bounds.center));
            }
        }

        [Test]
        public void Neighbours_HaveOpenPassages_AndTheRegionIsConnected()
        {
            OverworldLayout w = World;
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("fortaleza_ultima_lampada"), w.Zone("muralha_em_arco")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("fortaleza_ultima_lampada"), w.Zone("cova_da_fumaca")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("fortaleza_ultima_lampada"), w.Zone("portao_da_terra_da_treva")));

            var seen = new HashSet<OverworldZone> { w.Zone("fortaleza_ultima_lampada") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to the town");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromTheFortress_FromTheCatalogsLevel340()
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

                MapDefinition map = MapCatalog.Get(a.MapId);
                if (map != null)
                {
                    Assert.GreaterOrEqual(a.MinLevel, map.MonsterLevelMin, a.Id);
                    Assert.LessOrEqual(a.MaxLevel, map.MonsterLevelMax, a.Id);
                }
            }

            List<OverworldZone> byDistance = w.Zones.OrderBy(z => OverworldRules.DistanceFromVila(w, z)).ToList();
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.FortalezaUltimaLampadaMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 340 at the fortress (83 is 340–350)");
            Assert.AreEqual(340, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 412, "the head of the bridge and the gate are the end of the region");
            Assert.AreEqual(440, w.Zones.Max(z => z.MaxLevel), "the end of the sheets (90: 424–440)");
        }

        [Test]
        public void SoftBarrier_EachMapAsksForItsGear_NothingAtTheCity_NorAtTheEntrances()
        {
            OverworldLayout w = World;
            var gates = new Dictionary<string, string>
            {
                { OverworldLayouts.CovaFumacaMapId, MapRequirementTags.PoisonProtection },
                { OverworldLayouts.CovaFogoMapId, MapRequirementTags.EternalFireOil },
                { OverworldLayouts.CovaVentoMapId, MapRequirementTags.WingsOrDoubleJump },
                { OverworldLayouts.CovaLodoMapId, MapRequirementTags.AirBubble },
                { OverworldLayouts.CovaTrevaMapId, MapRequirementTags.RunicLamp },
                { OverworldLayouts.PonteNoveloMapId, MapRequirementTags.RootBridgeBalance },
                { OverworldLayouts.PortaoTrevaMapId, MapRequirementTags.TorchRequired },
            };

            foreach (KeyValuePair<string, string> gate in gates)
            {
                Assert.AreEqual(gate.Value, MapCatalog.Get(gate.Key).SpecialRequirementTag, gate.Key);
                CollectionAssert.IsNotEmpty(w.ZonesOfMap(gate.Key).ToList());
                foreach (OverworldZone zone in w.ZonesOfMap(gate.Key))
                {
                    Assert.AreEqual(gate.Value, zone.RequirementTag, zone.Id);
                    Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 500, CharacterId.Joaquim, null), $"{zone.Id}: asks for {gate.Value}");
                    Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 500, CharacterId.Joaquim, tag => tag == gate.Value));
                }
            }

            Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(OverworldLayouts.FortalezaUltimaLampadaMapId).SpecialRequirementTag);
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.FortalezaUltimaLampadaMapId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(zone.RequirementTag), $"{zone.Id}: the fortress and its yards ask for nothing");
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("muralha_em_arco"), 340, CharacterId.Joaquim, null), "the way in from the Margins");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("fortaleza_ultima_lampada"), 250, CharacterId.Joaquim, null), "the Limiar Profundo is for level 340");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("portao_da_terra_da_treva"), 380, CharacterId.Joaquim, tag => true));
        }

        [Test]
        public void Gates_TwoToTheMarginsWest_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            Assert.AreEqual(2, gates.Count);
            foreach (MapExitSpec gate in gates)
            {
                Assert.AreEqual(MapRegion.MargensDaMistura, MapCatalog.Get(gate.TargetMapId).Region, gate.Id);
                Assert.Less(gate.Trigger.center.x - w.Bounds.xMin, 3f, $"{gate.Id}: on the west edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.FortalezaUltimaLampadaMapId, gate.TargetMapId, out _), gate.Id);
                Assert.Greater(Vector2.Dot(gate.ArrivalFacing, w.Bounds.center - gate.Arrival), 0f, $"{gate.Id}: facing into the Limiar Profundo");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
                Assert.IsTrue(string.IsNullOrEmpty(OverworldRules.ZoneAt(w, gate.Arrival).RequirementTag), $"{gate.Id}: the entrance asks for nothing");
            }

            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by the town");
            Vector2 dock = w.ArrivalFor(OverworldLayouts.FortalezaUltimaLampadaMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in ProfundoMaps)
            {
                OverworldSettlement town = w.SettlementOf(id);
                Assert.AreEqual(town.DockArrival, w.ArrivalFor(id, OverworldLayouts.FortalezaUltimaLampadaMapId, out _), id);
                Assert.IsTrue(town.SafeZone.Contains(town.DockArrival), id);
            }
        }

        [Test]
        public void Spawns_StayOutOfSafeZonesAndWater_InsideTheirArea_WithExistingNonBossMonsters()
        {
            OverworldLayout w = World;
            var checkedIds = new HashSet<string>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                OverworldZone zone = w.ZoneOfSpawn[spawn.Id];
                Assert.IsTrue(zone.Bounds.Contains(spawn.Center), spawn.Id);
                Assert.GreaterOrEqual(OverworldRules.DistanceToSafety(w, spawn.Center) - spawn.Radius, MapLayoutRules.SpawnSafeMargin, spawn.Id);
                Assert.IsNull(OverworldRules.SettlementAt(w, spawn.Center), spawn.Id);
                Assert.IsFalse(MapLayoutRules.InRiver(w.World, spawn.Center, spawn.Radius * 0.5f), $"{spawn.Id}: in the water");
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
        public void Height_IsPureAndContinuous_AndChunksHaveNoSeams()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            var rnd = new System.Random(77);
            for (int i = 0; i < 3000; i++)
            {
                var p = new Vector2(w.Bounds.xMin - 40f + (float)rnd.NextDouble() * (w.Bounds.width + 80f), w.Bounds.yMin - 40f + (float)rnd.NextDouble() * (w.Bounds.height + 80f));
                float h = relief.HeightAt(p);
                Assert.AreEqual(h, relief.HeightAt(p), 0f);
                Assert.IsFalse(float.IsNaN(h) || float.IsInfinity(h), $"{p}");
                var q = p + new Vector2((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f).normalized * 0.05f;
                Assert.Less(Mathf.Abs(relief.HeightAt(q) - h), 0.3f, $"a jump in the relief at {p}");
            }

            Vector2Int count = OverworldRules.ChunkCount(w);
            for (int i = 0; i < 30; i++)
            {
                var c = new Vector2Int(rnd.Next(-1, count.x), rnd.Next(-1, count.y));
                OverworldGroundGrid a = OverworldChunkBuilder.BuildGrid(w, c);
                OverworldGroundGrid north = OverworldChunkBuilder.BuildGrid(w, c + Vector2Int.up);
                Rect r = OverworldRules.ChunkRect(w, c);
                for (float t = 0f; t <= w.ChunkSize; t += 0.53f)
                {
                    var onNorth = new Vector2(r.xMin + t, r.yMax);
                    Assert.AreEqual(a.Sample(onNorth), north.Sample(onNorth), 1e-4f, $"seam between {c} and its north neighbour at {onNorth}");
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
                for (float x = r.xMin; x <= r.xMax; x += 2f)
                {
                    for (float z = r.yMin; z <= r.yMax; z += 2f)
                    {
                        Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, new Vector2(x, z)), 1e-4f, $"{settlement.Id}: the town kit stands at y = 0 ({x}, {z})");
                    }
                }
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Trigger.center), 1e-4f, $"{exit.Id}: trigger on flat ground");
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Arrival), 1e-4f, $"{exit.Id}: the hero arrives on flat ground");
            }
        }

        [Test]
        public void Trails_AreLevel_Walkable_AndNeverWadeTheDeepWater()
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
                    if (left.Free >= 0.999f && right.Free >= 0.999f)
                    {
                        cross.Add(Mathf.Abs(left.Height - right.Height) / path.Width);
                    }
                }

                float along = Mathf.Atan(Mathf.Abs(relief.HeightAt(path.B) - relief.HeightAt(path.A)) / length) * Mathf.Rad2Deg;
                Assert.Less(along, 30f, $"trail {path.A} → {path.B} too steep to walk comfortably");
                for (float t = 0f; t <= 1f; t += 0.1f)
                {
                    Vector2 q = Vector2.Lerp(path.A, path.B, t);
                    Assert.IsFalse(InDeepWater(w, q), $"trail {path.A} → {path.B} wades the deep water at {q}");
                }
            }

            cross.Sort();
            Assert.Greater(cross.Count, 1000);
            Assert.Less(cross[cross.Count / 2], 0.005f, "a trail is level across its width (median)");
            Assert.Less(cross[(int)(cross.Count * 0.98f)], 0.08f, "a trail is level across its width (98 %)");
        }

        [Test]
        public void TheShaftCausewayAndLabyrinthStandUp_TheFortressLow_ThePoolsAreWater()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            Assert.Greater(MaxHeight(w.Zone("cova_do_vento_destruidor")), 8f, "the shaft of the wind climbs to its rim");
            Assert.Greater(MaxHeight(w.Zone("ponte_do_novelo")), 6f, "the causeway of the Bridge of the Bōlos");
            Assert.Greater(MaxHeight(w.Zone("cova_da_treva")), 4f, "the walls of the labyrinth");
            Assert.Greater(MaxHeight(w.Zone("cova_da_fumaca")), 4f, "the corridors of the Pit of Smoke");
            foreach (string low in new[] { "fortaleza_ultima_lampada", "muralha_em_arco", "cisternas_de_oleo" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 8f, $"{low}: low ground");
            }

            List<MapRiver> water = w.World.Rivers.ToList();
            Assert.AreEqual(OverworldLayouts.LodoPools.Length, water.Count, "only the pools of black water of the Pit of Mire");
            Vector2 bridgeCrest = new Vector2(OverworldLayouts.NoveloBridgeX, -300f);
            Vector2 bridgeSide = new Vector2(OverworldLayouts.NoveloBridgeX + 90f, -300f);
            Assert.Greater(relief.HeightAt(bridgeCrest), relief.HeightAt(bridgeSide) + 5f, "the bridge stands above the black ground");
        }

        [Test]
        public void EveryProfundoTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsProfundo(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.LampFortress).Weather, "under the fortress' lamps");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.SmokePit).Weather, "the smoke of the Pit of Smoke");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Sandstorm, OverworldAreaAmbience.ClimateOf(OverworldTerrain.HotAshHalls).Weather, "hot ash in the air");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.DarknessPit).Weather, "the dark");
            Assert.IsFalse(OverworldAreaAmbience.ClimateOf(OverworldTerrain.LampFortress).Wild);
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.DarkGateGround).Wild);
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
        public void Dressing_EveryAreaHasItsOwnLook_StandsOnTheGround_OffTheTrailsAndTheWater()
        {
            OverworldLayout w = World;
            var expected = new Dictionary<string, OverworldDecor[]>
            {
                { "nichos_de_basalto", new[] { OverworldDecor.LampEmber } },
                { "cova_da_fumaca", new[] { OverworldDecor.SootPatch } },
                { "cova_do_fogo_devorador", new[] { OverworldDecor.EmberCrack } },
                { "cova_do_vento_destruidor", new[] { OverworldDecor.WindStreak } },
                { "cova_do_lodo", new[] { OverworldDecor.SludgeRing } },
                { "cova_da_treva", new[] { OverworldDecor.ShadowPool } },
                { "ponte_do_novelo", new[] { OverworldDecor.LightThread } },
                { "portao_da_terra_da_treva", new[] { OverworldDecor.SealRune } },
            };

            var signatures = new Dictionary<string, HashSet<string>>();
            foreach (OverworldZone zone in w.Zones)
            {
                var seen = new HashSet<OverworldDecor>();
                var signature = new HashSet<string>();
                foreach (Vector2Int c in InnerChunks(w, zone, 8))
                {
                    OverworldChunkContent content = OverworldChunkBuilder.Content(w, c);
                    foreach (OverworldProp prop in content.Props.Concat(content.Details))
                    {
                        seen.Add(prop.Decor);
                        signature.Add(prop.Decor + "/" + prop.Kind + "/" + (prop.Kind == CityPropKind.Tree ? prop.Vegetation + ":" + prop.Variant : string.Empty));
                    }

                    foreach (OverworldProp prop in content.Props)
                    {
                        if (prop.Kind != CityPropKind.FenceRun)
                        {
                            Assert.IsFalse(MapLayoutRules.OnPath(w.World, prop.Position, 0f), $"chunk {c}: {prop.Decor} on a trail at {prop.Position}");
                        }

                        Assert.IsFalse(MapLayoutRules.InRiver(w.World, prop.Position, 0f), $"chunk {c}: prop in the water at {prop.Position}");
                        Assert.IsNull(OverworldRules.SettlementAt(w, prop.Position), $"chunk {c}: prop in a safe zone at {prop.Position}");
                    }

                    foreach (OverworldProp detail in content.Details)
                    {
                        Assert.IsFalse(MapLayoutRules.OnPath(w.World, detail.Position, 0f), $"{detail.Decor} on a trail at {detail.Position}");
                        Assert.IsFalse(MapLayoutRules.InRiver(w.World, detail.Position, 0f), $"{detail.Decor} in the water at {detail.Position}");
                    }

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
        public void TheTenPylons_TheFivePedestals_TheGlobeAndTheGate_Stand()
        {
            OverworldLayout w = World;
            int pylons = 0;
            int pedestals = 0;
            bool globe = false;
            bool gate = false;
            foreach (Vector2Int c in ChunksOver(w, new Vector2(OverworldLayouts.NoveloBridgeX, -400f), 760f))
            {
                pylons += OverworldChunkBuilder.Content(w, c).Props.Count(p => p.Decor == OverworldDecor.BridgePylon);
            }

            foreach (Vector2Int c in ChunksOver(w, OverworldLayouts.SeloArtificesOffset, OverworldLayouts.LampPedestalRingRadius + 12f))
            {
                pedestals += OverworldChunkBuilder.Content(w, c).Props.Count(p => p.Decor == OverworldDecor.LampPedestal);
            }

            foreach (Vector2Int c in ChunksOver(w, OverworldLayouts.NoveloGlobeCenter, 12f))
            {
                globe |= OverworldChunkBuilder.Content(w, c).Props.Exists(p => p.Decor == OverworldDecor.NoveloGlobe);
            }

            foreach (Vector2Int c in ChunksOver(w, OverworldLayouts.DarkGateCenter, 12f))
            {
                gate |= OverworldChunkBuilder.Content(w, c).Props.Exists(p => p.Decor == OverworldDecor.DarkGate);
            }

            Assert.GreaterOrEqual(pylons, 7, "the ten flights of the Bridge of the Bōlos (a piece can give way to a trail)");
            Assert.GreaterOrEqual(pedestals, 4, "the five lamp pedestals round the Selo dos Artífices");
            Assert.IsTrue(globe, "the dark globe of the Novelo");
            Assert.IsTrue(gate, "the Gate of the Land of Darkness");
        }

        [Test]
        public void EveryChunk_IsDeterministic_WithinTheTriangleBudget_AndBuildsFast()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            Assert.AreEqual(50, count.x);
            Assert.AreEqual(50, count.y);
            var builder = new TownMeshBuilder();
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
                    if ((x + y) % 9 == 0)
                    {
                        OverworldChunkContent again = OverworldChunkBuilder.Content(w, c);
                        Assert.AreEqual(content.Props.Count, again.Props.Count, $"chunk {c}: deterministic");
                        Assert.AreEqual(content.Details.Count, again.Details.Count, $"chunk {c}: deterministic");
                    }
                }
            }

            times.Sort();
            double average = times.Average();
            Debug.Log($"[Overworld Profundo] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
                $"generation avg {average:0.00} ms, p50 {times[times.Count / 2]:0.00} ms, p95 {times[(int)(times.Count * 0.95f)]:0.00} ms (Editor, Mono)");
            Assert.Less(average, 6.0, "chunk generation stays cheap even in the Editor");
        }

        private static IEnumerable<Vector2Int> ChunksOver(OverworldLayout w, Vector2 center, float radius)
        {
            Vector2Int a = OverworldRules.ChunkOf(w, center - new Vector2(radius, radius));
            Vector2Int b = OverworldRules.ChunkOf(w, center + new Vector2(radius, radius));
            for (int x = a.x; x <= b.x; x++)
            {
                for (int y = a.y; y <= b.y; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }

        private static bool InDeepWater(OverworldLayout w, Vector2 p)
        {
            foreach (MapRiver river in w.World.Rivers)
            {
                if (MapLayoutRules.DistanceToPolyline(p, river.Points) >= river.Width * 0.5f - 0.5f)
                {
                    continue;
                }

                if (!river.Crossings.Exists(c => Vector2.Distance(c.At, p) < Mathf.Max(c.Width, river.Width) * 0.5f + OverworldChunkBuilder.BridgeSpan(river)))
                {
                    return true;
                }
            }

            return false;
        }

        private static float MaxHeight(OverworldZone zone, float inset = 2f)
        {
            float max = float.MinValue;
            for (float x = zone.Bounds.xMin + inset; x < zone.Bounds.xMax - inset; x += 6f)
            {
                for (float z = zone.Bounds.yMin + inset; z < zone.Bounds.yMax - inset; z += 6f)
                {
                    max = Mathf.Max(max, Relief.HeightAt(new Vector2(x, z)));
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
