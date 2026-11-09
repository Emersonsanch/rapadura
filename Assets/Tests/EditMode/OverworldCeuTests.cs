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
    /// The continuous Região 16 — Reinos do Céu (Docs/Mapas100.md §0.10, maps 91–100): one 2400 × 3600 m map with the Cais da Coluna inside (flat ground of ±30 m at y = 0, roads W, E, S) and the posts of
    /// 92–100, maps 91–100 as zones, 35 zones that tile it, level bands growing with the distance from the quay from the catalog's 420, the soft barrier (a Climbing Rope on the Wheel, Canvas Wings in the
    /// Spheres, a Storm Cloak on the Lookout; nothing at the city nor at the way in), the two gates down to the Limiar Profundo (south edge), the sky's beasts (no boss), the relief (the rim of the Wheel, the
    /// deck tiers, the orbit ridges and the rings of the Coluna standing up, the Cais low; the Rio de Luz, a river with three bridges), the fixed pieces and the per-area dressing.
    /// </summary>
    public class OverworldCeuTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.CeuRegionId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] CeuMaps =
        {
            OverworldLayouts.CaisDaColunaMapId, OverworldLayouts.RioDeLuzMapId, OverworldLayouts.BarcaDaLuaMapId, OverworldLayouts.RodaCelesteMapId, OverworldLayouts.EsferasArcontesMapId,
            OverworldLayouts.JardimDeLuzMapId, OverworldLayouts.BarcaDoSolMapId, OverworldLayouts.PesDaColunaMapId, OverworldLayouts.MiranteCeuMapId, OverworldLayouts.VarandaMapId,
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
        public void Catalog_HasMaps91To100_InTheSkyRealms_ReturningToTheQuay()
        {
            CollectionAssert.Contains(MapCatalog.Regions.ToList(), MapRegion.ReinosDoCeu);
            CollectionAssert.AreEqual(Enumerable.Range(91, 10).ToArray(), MapCatalog.InRegion(MapRegion.ReinosDoCeu).Select(m => m.MapId).ToArray());
            foreach (string map in CeuMaps)
            {
                Assert.AreEqual(MapCatalog.CaisDaColuna, MapCatalog.ReturnCityOf(map), map);
                Assert.IsTrue(MapCatalog.Get(map).IsBuilt, map);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get(OverworldLayouts.CaisDaColunaMapId).Settlement);
            Assert.AreEqual(420, MapCatalog.Get(OverworldLayouts.CaisDaColunaMapId).MonsterLevelMin);
            Assert.AreEqual(500, MapCatalog.Get(OverworldLayouts.VarandaMapId).MonsterLevelMax);
            Assert.AreEqual("Reinos do Céu", MapTexts.RegionName(null, MapRegion.ReinosDoCeu));
            Assert.AreEqual("Cais da Coluna", MapTexts.Get(null, MapTexts.ReturnCityKey(MapCatalog.CaisDaColuna)));
            Assert.IsNotNull(MapCatalog.RespawnCityFor(OverworldLayouts.VarandaMapId));
        }

        [Test]
        public void Region_IsTheBiggestOfAll_WithTheQuayInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(3600f, w.Bounds.height);
            Assert.AreEqual(MapRegion.ReinosDoCeu, w.Region);
            Assert.AreEqual(MapCatalog.CaisDaColuna, w.Vila.Id, "the city is the region's town (its first map, level 420)");
            Assert.AreEqual(CityTownRecipes.CaisDaColunaCityId, w.Vila.Recipe.CityId, "its own recipe");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, CityTownRecipes.CaisDaColunaSolidRadius + 5f, "room for the town's recipe");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the barca's stop");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: open sky round it");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, OverworldLayouts.CaisColunaGround, "±30 m of flat ground reserved for the city's recipe");
            Assert.AreEqual(Vector2.zero, w.Vila.Offset);
            Assert.AreEqual(OverworldLayouts.CaisDaColunaMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "the town stands in its own area (91)");

            foreach (string map in CeuMaps)
            {
                OverworldSettlement stop = w.SettlementOf(map);
                Assert.IsNotNull(stop, $"{map}: its town or post");
                Assert.AreEqual(map, OverworldRules.ZoneAt(w, stop.Offset).MapId, $"{stop.Id} stands in its map's area");
                Assert.AreEqual(MapCatalog.Get(map).SettlementId, stop.Id, $"{map}: the catalog's settlement id");
            }

            Assert.AreEqual(1, w.Settlements.Count(s => s.IsReturnCity), "one capital, nine posts");
            Assert.AreEqual(10, w.Settlements.Count);
            Assert.Greater(w.SettlementOf(OverworldLayouts.VarandaMapId).Offset.y, 2800f, "the Porch is far north, at the end of everything");
        }

        [Test]
        public void Region_HoldsMaps91To100_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.AreEqual(35, w.Zones.Count);
            Assert.AreEqual(35, w.Zones.Select(z => z.Terrain).Distinct().Count(), "one ground per area: quay, river, decks, wheel, spheres, garden, column, cracks, porch");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsCeu(z.Terrain)), "the Reinos do Céu's own terrains");
            foreach (string id in CeuMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the Reinos do Céu");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Reinos do Céu");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 200, "the sky is full of clearings");
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
            Assert.AreEqual("cais_da_coluna", OverworldRules.ZoneAt(w, OverworldLayouts.CaisDaColunaOffset).Id);
            Assert.AreEqual("rio_de_luz", OverworldRules.ZoneAt(w, OverworldLayouts.BoiaDeLuzOffset).Id);
            Assert.AreEqual("conves_da_lua", OverworldRules.ZoneAt(w, OverworldLayouts.CabineDaProaOffset).Id);
            Assert.AreEqual("roda_celeste", OverworldRules.ZoneAt(w, OverworldLayouts.EixoDaRodaOffset).Id);
            Assert.AreEqual("esferas_dos_arcontes", OverworldRules.ZoneAt(w, OverworldLayouts.IlhotaParadaOffset).Id);
            Assert.AreEqual("pergola_do_jardineiro", OverworldRules.ZoneAt(w, OverworldLayouts.PergolaOffset).Id);
            Assert.AreEqual("conves_do_sol", OverworldRules.ZoneAt(w, OverworldLayouts.CabineDaPopaOffset).Id);
            Assert.AreEqual("pes_da_coluna", OverworldRules.ZoneAt(w, OverworldLayouts.DegrauUmOffset).Id);
            Assert.AreEqual("mirante_do_ceu", OverworldRules.ZoneAt(w, OverworldLayouts.MaoAbertaOffset).Id);
            Assert.AreEqual("varanda_do_tempo_de_depois", OverworldRules.ZoneAt(w, OverworldLayouts.VarandaOffset).Id);
            foreach (OverworldZone zone in w.Zones)
            {
                Assert.AreSame(zone, OverworldRules.ZoneAt(w, zone.Bounds.center));
            }
        }

        [Test]
        public void Neighbours_HaveOpenPassages_AndTheRegionIsConnected()
        {
            OverworldLayout w = World;
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("cais_da_coluna"), w.Zone("rio_de_luz")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("cais_da_coluna"), w.Zone("pier_do_sol")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("cais_da_coluna"), w.Zone("varanda_do_tempo_de_depois")));

            var seen = new HashSet<OverworldZone> { w.Zone("cais_da_coluna") };
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
        public void LevelBands_GrowWithTheDistanceFromTheQuay_FromTheCatalogsLevel420()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.CaisDaColunaMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 420 at the quay (91 is 420–430)");
            Assert.AreEqual(420, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 482, "the north row is the end of the region");
            Assert.AreEqual(500, w.Zones.Max(z => z.MaxLevel), "the end of the sheets (100: 495–500)");
        }

        [Test]
        public void SoftBarrier_ThreeMapsAskForGear_NothingAtTheCity_NorAtTheEntrances()
        {
            OverworldLayout w = World;
            var gates = new Dictionary<string, string>
            {
                { OverworldLayouts.RodaCelesteMapId, MapRequirementTags.ElevatedPlatforms },
                { OverworldLayouts.EsferasArcontesMapId, MapRequirementTags.WingsOrDoubleJump },
                { OverworldLayouts.MiranteCeuMapId, MapRequirementTags.StormProtection },
            };

            foreach (KeyValuePair<string, string> gate in gates)
            {
                Assert.AreEqual(gate.Value, MapCatalog.Get(gate.Key).SpecialRequirementTag, gate.Key);
                CollectionAssert.IsNotEmpty(w.ZonesOfMap(gate.Key).ToList());
                foreach (OverworldZone zone in w.ZonesOfMap(gate.Key))
                {
                    Assert.AreEqual(gate.Value, zone.RequirementTag, zone.Id);
                    Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 600, CharacterId.Joaquim, null), $"{zone.Id}: asks for {gate.Value}");
                    Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 600, CharacterId.Joaquim, tag => tag == gate.Value));
                }
            }

            foreach (string open in new[] { OverworldLayouts.CaisDaColunaMapId, OverworldLayouts.RioDeLuzMapId, OverworldLayouts.BarcaDaLuaMapId, OverworldLayouts.JardimDeLuzMapId, OverworldLayouts.BarcaDoSolMapId, OverworldLayouts.PesDaColunaMapId, OverworldLayouts.VarandaMapId })
            {
                Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(open).SpecialRequirementTag, open);
                foreach (OverworldZone zone in w.ZonesOfMap(open))
                {
                    Assert.IsTrue(string.IsNullOrEmpty(zone.RequirementTag), $"{zone.Id}: asks for nothing");
                }
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("fresta_das_luzinhas"), 424, CharacterId.Joaquim, null), "the way in from the Limiar Profundo");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("cais_da_coluna"), 300, CharacterId.Joaquim, null), "the Reinos do Céu are for level 420");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("varanda_do_tempo_de_depois"), 440, CharacterId.Joaquim, tag => true));
        }

        [Test]
        public void Gates_TwoDownToTheLimiar_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            Assert.AreEqual(2, gates.Count);
            foreach (MapExitSpec gate in gates)
            {
                Assert.AreEqual(MapRegion.LimiarProfundo, MapCatalog.Get(gate.TargetMapId).Region, gate.Id);
                Assert.Less(gate.Trigger.center.y - w.Bounds.yMin, 3f, $"{gate.Id}: on the south edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.CaisDaColunaMapId, gate.TargetMapId, out _), gate.Id);
                Assert.Greater(Vector2.Dot(gate.ArrivalFacing, w.Bounds.center - gate.Arrival), 0f, $"{gate.Id}: facing into the Reinos do Céu");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
                Assert.IsTrue(string.IsNullOrEmpty(OverworldRules.ZoneAt(w, gate.Arrival).RequirementTag), $"{gate.Id}: the entrance asks for nothing");
            }

            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by the town");
            Vector2 dock = w.ArrivalFor(OverworldLayouts.CaisDaColunaMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in CeuMaps)
            {
                OverworldSettlement town = w.SettlementOf(id);
                Assert.AreEqual(town.DockArrival, w.ArrivalFor(id, OverworldLayouts.CaisDaColunaMapId, out _), id);
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
            var rnd = new System.Random(91);
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
        public void TheRimDecksOrbitsAndColumnStandUp_TheQuayLow_TheRiverIsWater()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            Assert.Greater(MaxHeight(w.Zone("roda_celeste")), 6f, "the rim of the Celestial Wheel");
            Assert.Greater(MaxHeight(w.Zone("conves_da_lua")), 4f, "the tiers of the moon deck");
            Assert.Greater(MaxHeight(w.Zone("orbita_oeste")), 3f, "the orbit ridges");
            Assert.Greater(MaxHeight(w.Zone("pes_da_coluna")), 8f, "the rings climbing to the Coluna");
            foreach (string low in new[] { "cais_da_coluna", "pier_da_lua" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 8f, $"{low}: low ground");
            }

            List<MapRiver> water = w.World.Rivers.ToList();
            Assert.AreEqual(1, water.Count, "the Rio de Luz");
            Assert.AreEqual(3, water[0].Crossings.Count, "three bridges where the roads north cross it");
            Vector2 rim = OverworldLayouts.WheelCenter + new Vector2(OverworldRelief.WheelRimRadius, 12f);
            Vector2 off = OverworldLayouts.WheelCenter + new Vector2(OverworldRelief.WheelRimRadius + 180f, 12f);
            Assert.Greater(relief.HeightAt(rim), relief.HeightAt(off) + 4f, "the rim stands above the drifts");
        }

        [Test]
        public void EveryCeuTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsCeu(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.ColumnQuayGround).Weather, "clear over the Cais");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.MoonHold).Weather, "the dim hold of the Barca da Lua");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Sandstorm, OverworldAreaAmbience.ClimateOf(OverworldTerrain.GearDrift).Weather, "bronze dust off the gears");
            Assert.IsFalse(OverworldAreaAmbience.ClimateOf(OverworldTerrain.ColumnQuayGround).Wild);
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.SkyCracksWest).Wild);
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
                { "cais_da_coluna", new[] { OverworldDecor.LightSpeck } },
                { "rio_de_luz", new[] { OverworldDecor.GoldRipple } },
                { "conves_da_lua", new[] { OverworldDecor.LilacPlank } },
                { "roda_celeste", new[] { OverworldDecor.BronzeFiling } },
                { "esferas_dos_arcontes", new[] { OverworldDecor.CrystalChip } },
                { "pergola_do_jardineiro", new[] { OverworldDecor.LightPetal } },
                { "conves_do_sol", new[] { OverworldDecor.SunGrain } },
                { "pes_da_coluna", new[] { OverworldDecor.StepMark } },
                { "mirante_do_ceu", new[] { OverworldDecor.SkyCrackLine } },
                { "varanda_do_tempo_de_depois", new[] { OverworldDecor.ApricotDust } },
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
        public void TheFixedPieces_OfTheSheets_Stand()
        {
            OverworldLayout w = World;
            int jars = CountDecor(w, OverworldDecor.CelestialJar, OverworldLayouts.CelestialJarCount, OverworldLayouts.CelestialJarPosition);
            int spheres = CountDecor(w, OverworldDecor.ArchonSphere, OverworldLayouts.ArchonSphereCount, OverworldLayouts.ArchonSpherePosition);
            int cracks = CountDecor(w, OverworldDecor.SkyCrack, OverworldLayouts.SkyCrackCount, OverworldLayouts.SkyCrackPosition);
            Assert.AreEqual(OverworldLayouts.CelestialJarCount, jars, "the twelve jars of the Celestial Wheel");
            Assert.AreEqual(OverworldLayouts.ArchonSphereCount, spheres, "the seven spheres of the Archons");
            Assert.AreEqual(OverworldLayouts.SkyCrackCount, cracks, "the five cracks in the sky round the Lookout");
            Assert.IsTrue(HasDecor(w, OverworldDecor.MoonMast, OverworldLayouts.MoonMastCenter), "the mast of the Barca da Lua");
            Assert.IsTrue(HasDecor(w, OverworldDecor.SunSail, OverworldLayouts.SunMastCenter), "the sail of the Barca do Sol");
            Assert.IsTrue(HasDecor(w, OverworldDecor.GloryColumn, OverworldLayouts.GloryColumnCenter), "the Coluna da Glória");
            Assert.IsTrue(HasDecor(w, OverworldDecor.RockingChair, OverworldLayouts.RockingChairPosition), "Vô Jorge's empty rocking chair");
        }

        [Test]
        public void EveryChunk_IsDeterministic_WithinTheTriangleBudget_AndBuildsFast()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            Assert.AreEqual(50, count.x);
            Assert.AreEqual(75, count.y);
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
            Debug.Log($"[Overworld Céu] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
                $"generation avg {average:0.00} ms, p50 {times[times.Count / 2]:0.00} ms, p95 {times[(int)(times.Count * 0.95f)]:0.00} ms (Editor, Mono)");
            Assert.Less(average, 6.0, "chunk generation stays cheap even in the Editor");
        }

        private static int CountDecor(OverworldLayout w, OverworldDecor decor, int n, System.Func<int, Vector2> at)
        {
            var done = new HashSet<Vector2Int>();
            int count = 0;
            for (int i = 0; i < n; i++)
            {
                Vector2Int c = OverworldRules.ChunkOf(w, at(i));
                if (done.Add(c))
                {
                    count += OverworldChunkBuilder.Content(w, c).Props.Count(p => p.Decor == decor);
                }
            }

            return count;
        }

        private static bool HasDecor(OverworldLayout w, OverworldDecor decor, Vector2 at) => OverworldChunkBuilder.Content(w, OverworldRules.ChunkOf(w, at)).Props.Exists(p => p.Decor == decor);

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
