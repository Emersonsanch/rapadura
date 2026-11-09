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
    /// The continuous Região 14 — Margens da Mistura (Docs/Mapas100.md §0.10, maps 77–82), in the mould of <see cref="OverworldRioTests"/>: one
    /// 2400 × 2400 m map with Dur-Jarras inside (flat ground of ±30 m at y = 0, roads W, E, S) and the posts of 78–82, maps 77–82 as zones,
    /// zones that tile it, level bands growing with the distance from Dur-Jarras from the catalog's 300, the soft barrier (a waterskin in the
    /// desert, a Reed Boat on the pitch, a Climbing Rope in the cedars, Crampons on the shoulder, a lit lamp on the field of the tablets;
    /// nothing at the city nor at the entrances), the two gates towards the Rio das Doze Horas (west edge), the Margins' beasts (no boss),
    /// the relief (the shoulder and the diorite standing up, the city and the desert low; the canal of bitumen carved and bridged) and
    /// the per-area dressing.
    /// </summary>
    public class OverworldMisturaTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.MisturaRegionId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] MisturaMaps =
        {
            OverworldLayouts.DurJarrasMapId, OverworldLayouts.DesertoRelogioMapId, OverworldLayouts.PocosBetumeMapId, OverworldLayouts.CedralHobabishMapId,
            OverworldLayouts.OmbroUpelluriMapId, OverworldLayouts.TabuasSonhoMapId,
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
        public void Catalog_HasMaps77To82_OnTheMargins_ReturningToDurJarras()
        {
            CollectionAssert.Contains(MapCatalog.Regions.ToList(), MapRegion.MargensDaMistura);
            CollectionAssert.AreEqual(new[] { 77, 78, 79, 80, 81, 82 }, MapCatalog.InRegion(MapRegion.MargensDaMistura).Select(m => m.MapId).ToArray());
            foreach (string map in MisturaMaps)
            {
                Assert.AreEqual(MapCatalog.DurJarras, MapCatalog.ReturnCityOf(map), map);
                Assert.IsTrue(MapCatalog.Get(map).IsBuilt, map);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get(OverworldLayouts.DurJarrasMapId).Settlement);
            Assert.AreEqual(300, MapCatalog.Get(OverworldLayouts.DurJarrasMapId).MonsterLevelMin);
            Assert.AreEqual(360, MapCatalog.Get(OverworldLayouts.TabuasSonhoMapId).MonsterLevelMax);
            Assert.AreEqual("Margens da Mistura", MapTexts.RegionName(null, MapRegion.MargensDaMistura));
            Assert.AreEqual("Dur-Jarras", MapTexts.Get(null, MapTexts.ReturnCityKey(MapCatalog.DurJarras)));
            Assert.IsNotNull(MapCatalog.RespawnCityFor(OverworldLayouts.TabuasSonhoMapId));
        }

        [Test]
        public void Region_IsAsBigAsTheOthers_WithDurJarrasInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.MargensDaMistura, w.Region);
            Assert.AreEqual(MapCatalog.DurJarras, w.Vila.Id, "the city is the region's town (its first map, level 300)");
            Assert.AreEqual(CityTownRecipes.DurJarrasCityId, w.Vila.Recipe.CityId, "its own recipe");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, CityTownRecipes.DurJarrasSolidRadius + 5f, "room for the town's recipe");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais das Balsas");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: open land all round it");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, OverworldLayouts.DurJarrasGround, "±30 m of flat ground reserved for the city's recipe");
            Assert.AreEqual(Vector2.zero, w.Vila.Offset);
            Assert.AreEqual(OverworldLayouts.DurJarrasMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "the town stands in its own area (77)");

            foreach (string map in MisturaMaps)
            {
                OverworldSettlement stop = w.SettlementOf(map);
                Assert.IsNotNull(stop, $"{map}: its town or post");
                Assert.AreEqual(map, OverworldRules.ZoneAt(w, stop.Offset).MapId, $"{stop.Id} stands in its map's area");
                Assert.AreEqual(MapCatalog.Get(map).SettlementId, stop.Id, $"{map}: the catalog's settlement id");
            }

            Assert.AreEqual(1, w.Settlements.Count(s => s.IsReturnCity), "one capital, five posts");
            Assert.AreEqual(6, w.Settlements.Count);
            Assert.Greater(w.SettlementOf(OverworldLayouts.TabuasSonhoMapId).Offset.x, 1400f, "the tent of the interpreters is far east");
        }

        [Test]
        public void Region_HoldsMaps77To82_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.AreEqual(25, w.Zones.Count);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 18, "many kinds of brick, sand, pitch, cedar and stone");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsMistura(z.Terrain)), "the Margins' own terrains");
            foreach (string id in MisturaMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the Margins");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Margins");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 150, "the Margins are full of clearings");
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
            Assert.AreEqual("dur_jarras", OverworldRules.ZoneAt(w, OverworldLayouts.DurJarrasOffset).Id);
            Assert.AreEqual("deserto_do_relogio", OverworldRules.ZoneAt(w, OverworldLayouts.GnomonCaidoOffset).Id);
            Assert.AreEqual("pocos_de_betume", OverworldRules.ZoneAt(w, OverworldLayouts.BalsaJuncoOffset).Id);
            Assert.AreEqual("cedral_de_hobabish", OverworldRules.ZoneAt(w, OverworldLayouts.ClareiraLenhadorOffset).Id);
            Assert.AreEqual("ombro_de_upelluri", OverworldRules.ZoneAt(w, OverworldLayouts.NinhoOmbroOffset).Id);
            Assert.AreEqual("campo_das_tabuas", OverworldRules.ZoneAt(w, OverworldLayouts.TendaInterpretesOffset).Id);
            foreach (OverworldZone zone in w.Zones)
            {
                Assert.AreSame(zone, OverworldRules.ZoneAt(w, zone.Bounds.center));
            }
        }

        [Test]
        public void Neighbours_HaveOpenPassages_AndTheRegionIsConnected()
        {
            OverworldLayout w = World;
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("dur_jarras"), w.Zone("portao_de_tijolo")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("dur_jarras"), w.Zone("deserto_do_relogio")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("dur_jarras"), w.Zone("tabuas_do_sonho")));

            var seen = new HashSet<OverworldZone> { w.Zone("dur_jarras") };
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
        public void LevelBands_GrowWithTheDistanceFromDurJarras_FromTheCatalogsLevel300()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.DurJarrasMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 300 at Dur-Jarras (77 is 300–308)");
            Assert.AreEqual(300, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 344, "the dream field is the end of the Margins");
            Assert.AreEqual(360, w.Zones.Max(z => z.MaxLevel), "the end of the sheets (82: 344–360)");
        }

        [Test]
        public void SoftBarrier_EachMapAsksForItsGear_NothingAtTheCity_NorAtTheEntrances()
        {
            OverworldLayout w = World;
            var gates = new Dictionary<string, string>
            {
                { OverworldLayouts.DesertoRelogioMapId, MapRequirementTags.FreshWater },
                { OverworldLayouts.PocosBetumeMapId, MapRequirementTags.SaddleRequired },
                { OverworldLayouts.CedralHobabishMapId, MapRequirementTags.ElevatedPlatforms },
                { OverworldLayouts.OmbroUpelluriMapId, MapRequirementTags.ClimbingCrampons },
                { OverworldLayouts.TabuasSonhoMapId, MapRequirementTags.TorchRequired },
            };

            foreach (KeyValuePair<string, string> gate in gates)
            {
                Assert.AreEqual(gate.Value, MapCatalog.Get(gate.Key).SpecialRequirementTag, gate.Key);
                CollectionAssert.IsNotEmpty(w.ZonesOfMap(gate.Key).ToList());
                foreach (OverworldZone zone in w.ZonesOfMap(gate.Key))
                {
                    Assert.AreEqual(gate.Value, zone.RequirementTag, zone.Id);
                    Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 400, CharacterId.Lavine, null), $"{zone.Id}: asks for {gate.Value}");
                    Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 400, CharacterId.Lavine, tag => tag == gate.Value));
                }
            }

            Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(OverworldLayouts.DurJarrasMapId).SpecialRequirementTag);
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.DurJarrasMapId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(zone.RequirementTag), $"{zone.Id}: the city and its fields ask for nothing");
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("portao_de_tijolo"), 300, CharacterId.Lavine, null), "the way in from the river");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("dur_jarras"), 200, CharacterId.Lavine, null), "the Margins are for level 300");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("tabuas_do_sonho"), 320, CharacterId.Lavine, tag => true));
        }

        [Test]
        public void Gates_TwoToTheRiverWest_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            Assert.AreEqual(2, gates.Count);
            foreach (MapExitSpec gate in gates)
            {
                Assert.AreEqual(MapRegion.RioDasDozeHoras, MapCatalog.Get(gate.TargetMapId).Region, gate.Id);
                Assert.Less(gate.Trigger.center.x - w.Bounds.xMin, 3f, $"{gate.Id}: on the west edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.DurJarrasMapId, gate.TargetMapId, out _), gate.Id);
                Assert.Greater(Vector2.Dot(gate.ArrivalFacing, w.Bounds.center - gate.Arrival), 0f, $"{gate.Id}: facing into the Margins");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
                Assert.IsTrue(string.IsNullOrEmpty(OverworldRules.ZoneAt(w, gate.Arrival).RequirementTag), $"{gate.Id}: the entrance asks for nothing");
            }

            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by the town");
            Vector2 dock = w.ArrivalFor(OverworldLayouts.DurJarrasMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in MisturaMaps)
            {
                OverworldSettlement town = w.SettlementOf(id);
                Assert.AreEqual(town.DockArrival, w.ArrivalFor(id, OverworldLayouts.DurJarrasMapId, out _), id);
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
        public void TheShoulderAndDioriteStandUp_TheCityAndDesertLow_TheCanalCarvedAndBridged()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            Assert.Greater(MaxHeight(w.Zone("ombro_de_upelluri")), 8f, "Upelluri's shoulder");
            Assert.Greater(MaxHeight(w.Zone("diorito_crescente")), 12f, "the growing diorite");
            Assert.Greater(MaxHeight(w.Zone("dobras_de_pedra")), 5f, "the folds of stone");
            Assert.Greater(MaxHeight(w.Zone("cedral_de_hobabish")), 3f, "Hobabish's rings of roots");
            foreach (string low in new[] { "dur_jarras", "portao_de_tijolo", "terracos_de_cevada", "canal_das_palmeiras" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 8f, $"{low}: low ground");
            }

            List<MapRiver> water = w.World.Rivers.ToList();
            Assert.AreEqual(1 + OverworldLayouts.MisturaPools.Length, water.Count, "the canal and the ponds");
            List<MapRiver> channels = water.Where(r => r.Points.Count > 2).ToList();
            Assert.AreEqual(1, channels.Count);
            MapRiver canal = channels[0];
            Assert.AreEqual(3, canal.Crossings.Count(c => c.IsBridge), "three bridges on the canal");
            Assert.GreaterOrEqual(canal.Width, OverworldRelief.CarveMinWidth, "a carved channel");
            foreach (MapCrossing crossing in canal.Crossings.Where(c => c.IsBridge))
            {
                Vector2 across = MapAreaView.RiverNormal(canal, crossing.At);
                float half = OverworldChunkBuilder.BridgeSpan(canal) * 0.5f;
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At + across * half), 0.02f, $"bridge end at {crossing.At}");
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At - across * half), 0.02f, $"bridge end at {crossing.At}");
            }

            Vector2 deep = (OverworldLayouts.CanalDeBetume[1] + OverworldLayouts.CanalDeBetume[2]) * 0.5f;
            Assert.Less(relief.HeightAt(deep), OverworldRelief.WaterY, "the canal of bitumen is under the water line");
        }

        [Test]
        public void EveryMisturaTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsMistura(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.ZigguratTerrace).Weather, "the sky over the city");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Sandstorm, OverworldAreaAmbience.ClimateOf(OverworldTerrain.ClockDesert).Weather, "dust on the Desert of the Clock");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.CedarRings).Weather, "mist among the cedars");
            Assert.IsFalse(OverworldAreaAmbience.ClimateOf(OverworldTerrain.ZigguratTerrace).Wild);
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.DioriteRise).Wild);
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
                { "deserto_do_relogio", new[] { OverworldDecor.ClockRipple } },
                { "pocos_de_betume", new[] { OverworldDecor.TarBubble } },
                { "cedral_de_hobabish", new[] { OverworldDecor.ResinDrip } },
                { "cedros_anciaos", new[] { OverworldDecor.GiantCedar } },
                { "diorito_crescente", new[] { OverworldDecor.DioriteChip } },
                { "campo_das_tabuas", new[] { OverworldDecor.TabletChip } },
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
        public void TheTwelveGnomons_AndTheSevenLanterns_StandInTheirRings()
        {
            OverworldLayout w = World;
            int gnomons = 0;
            int lanterns = 0;
            foreach (Vector2Int c in ChunksOver(w, OverworldLayouts.GnomonDialCenter, OverworldLayouts.GnomonRingRadius + 12f))
            {
                gnomons += OverworldChunkBuilder.Content(w, c).Props.Count(p => p.Decor == OverworldDecor.FallenGnomon);
            }

            foreach (Vector2Int c in ChunksOver(w, OverworldLayouts.CedarRingCenter, OverworldLayouts.ResinLanternRingRadius + 12f))
            {
                lanterns += OverworldChunkBuilder.Content(w, c).Props.Count(p => p.Decor == OverworldDecor.ResinLantern);
            }

            Assert.GreaterOrEqual(gnomons, 9, "the ring of gnomons of the Desert of the Clock (a piece can give way to a trail)");
            Assert.GreaterOrEqual(lanterns, 5, "the lanterns of resin of the Cedral de Hobabish");
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
            Debug.Log($"[Overworld Mistura] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
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
