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
    /// The continuous Região 10 — Picos de Farnkesh (Docs/Mapas100.md §0.10, maps 53–58), in the mould of
    /// <see cref="OverworldNartesTests"/>: one 2400 × 2400 m map with the Vale dos Sátiros and Farnkesh inside (their own recipes on
    /// flat ground of ±30 m at y = 0) and the posts
    /// of 55–58, maps 53–58 as zones, zones that tile it, open passages, level bands growing with the distance from the vale from
    /// the catalog's 140, the soft barrier (Storm Protection in the Gorge of the Ays, wings or a glide on the cloud isles, nothing
    /// in the towns nor at the entrances), the two gates towards the Estepe dos Nartes (south edge), the peaks' beasts (no boss),
    /// the relief (pinnacles, the hermitages' cliff, the gorge and the cloud isles standing over low floors; the vale and the sea
    /// of clouds low; the Pactolo carved and crossed at a ford and a stone bridge) and the per-area dressing (the rope bridges
    /// among it) within the chunk budget.
    /// </summary>
    public class OverworldFarnkeshTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.FarnkeshRegionId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] FarnkeshMaps =
        {
            OverworldLayouts.ValeSatirosMapId, OverworldLayouts.FarnkeshMapId, OverworldLayouts.PontesVentoMapId, OverworldLayouts.ErmidasMapId,
            OverworldLayouts.GargantaAysMapId, OverworldLayouts.NinhoHumaMapId,
        };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // The region
        // ------------------------------------------------------------------

        [Test]
        public void Region_IsValid()
        {
            List<string> problems = OverworldRules.Validate(World);
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void Catalog_HasMaps53To58_InThePeaks_WithTheirReturnCities()
        {
            CollectionAssert.Contains(MapCatalog.Regions.ToList(), MapRegion.PicosDeFarnkesh);
            CollectionAssert.AreEqual(new[] { 53, 54, 55, 56, 57, 58 }, MapCatalog.InRegion(MapRegion.PicosDeFarnkesh).Select(m => m.MapId).ToArray());
            Assert.AreEqual(MapCatalog.ValeDosSatiros, MapCatalog.ReturnCityOf(OverworldLayouts.ValeSatirosMapId));
            foreach (string map in FarnkeshMaps.Skip(1))
            {
                Assert.AreEqual(MapCatalog.Farnkesh, MapCatalog.ReturnCityOf(map), map);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get(OverworldLayouts.FarnkeshMapId).Settlement);
            Assert.AreEqual(MapSettlementKind.RaceCity, MapCatalog.Get(OverworldLayouts.ValeSatirosMapId).Settlement);
            Assert.AreEqual(140, MapCatalog.Get(OverworldLayouts.ValeSatirosMapId).MonsterLevelMin);
            Assert.AreEqual(200, MapCatalog.Get(OverworldLayouts.NinhoHumaMapId).MonsterLevelMax);
            Assert.AreEqual("Picos de Farnkesh", MapTexts.RegionName(null, MapRegion.PicosDeFarnkesh));
            Assert.IsNotNull(MapCatalog.RespawnCityFor(OverworldLayouts.FarnkeshMapId), "a planned city falls back to an existing one");
        }

        [Test]
        public void Region_IsAsBigAsTheOthers_WithTheValeAndFarnkeshInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.PicosDeFarnkesh, w.Region);
            Assert.AreEqual(MapCatalog.ValeDosSatiros, w.Vila.Id, "the satyrs' vale is the region's town (its first map, level 140)");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais da Barca of the vale");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: the peaks are all round it");
            Assert.AreEqual(OverworldLayouts.ValeSatirosMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "the vale stands in its own area (53)");

            Assert.AreEqual(CityTownRecipes.ValeCityId, w.Vila.Id, "its own recipe");
            foreach ((string map, string city, string recipe, float solid) in new[]
            {
                (OverworldLayouts.ValeSatirosMapId, MapCatalog.ValeDosSatiros, CityTownRecipes.ValeCityId, CityTownRecipes.ValeSolidRadius),
                (OverworldLayouts.FarnkeshMapId, MapCatalog.Farnkesh, CityTownRecipes.FarnkeshCityId, CityTownRecipes.FarnkeshSolidRadius),
            })
            {
                OverworldSettlement town = w.SettlementOf(map);
                Assert.AreEqual(city, town.Id, $"{map}: the town keeps the catalog's city id");
                Assert.AreEqual(recipe, town.Recipe.CityId, $"{map}: the town's own recipe");
                Assert.IsTrue(town.IsReturnCity, $"{map}: its town wakes the hero");
                Assert.IsTrue(town.HasDock, $"{map}: the barca stops there");
                Assert.IsFalse(town.Recipe.Backdrop);
                Assert.GreaterOrEqual(town.GroundHalfExtent, solid + 5f, $"{map}: room for the town's recipe");
                Assert.AreEqual(map, OverworldRules.ZoneAt(w, town.Offset).MapId, $"{city} stands in its map's area");
            }

            foreach (string map in FarnkeshMaps)
            {
                string city = MapCatalog.ReturnCityOf(map);
                Assert.IsTrue(w.Settlements.Exists(s => s.IsReturnCity && s.Id == city), $"{map}: its return city {city} stands in the peaks");
            }

            Assert.AreEqual(2, w.Settlements.Count(s => s.IsReturnCity));
            Assert.Greater(w.SettlementOf(OverworldLayouts.FarnkeshMapId).Offset.y, 1000f, "Farnkesh is up the climb north of the vale");
            Assert.Greater(w.SettlementOf(OverworldLayouts.NinhoHumaMapId).Offset.y, 1400f, "the Último Pináculo is above the clouds");
        }

        [Test]
        public void Region_HoldsMaps53To58_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 13, "many kinds of vale, wood, meadow, pinnacle, cloud and gorge");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsFarnkesh(z.Terrain)), "the peaks' own terrains");
            foreach (string id in FarnkeshMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the peaks");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the peaks");
                Assert.IsNotNull(w.SettlementOf(id), $"{id}: its town or post stands in the region");
                Assert.IsTrue(w.SettlementOf(id).HasDock, $"{id}: the barca stops there");
                Assert.IsTrue(MapCatalog.Get(id).IsBuilt, $"{id}: built");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 250, "the peaks are full of clearings");
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
            Assert.AreEqual("vale_dos_satiros", OverworldRules.ZoneAt(w, OverworldLayouts.ValeSatirosOffset).Id);
            Assert.AreEqual("farnkesh", OverworldRules.ZoneAt(w, OverworldLayouts.FarnkeshOffset).Id);
            Assert.AreEqual("ermida_baixa", OverworldRules.ZoneAt(w, OverworldLayouts.ErmidaBaixaOffset).Id);
            Assert.AreEqual("pinaculo_do_sino", OverworldRules.ZoneAt(w, OverworldLayouts.PinaculoDoSinoOffset).Id);
            Assert.AreEqual("gruta_abrigo", OverworldRules.ZoneAt(w, OverworldLayouts.GrutaAbrigoOffset).Id);
            Assert.AreEqual("ilhas_de_nuvem", OverworldRules.ZoneAt(w, OverworldLayouts.UltimoPinaculoOffset).Id);
            Assert.IsNull(OverworldRules.ZoneAt(w, new Vector2(w.Bounds.xMax + 5f, 0f)), "outside the walls: no zone");
            foreach (OverworldZone zone in w.Zones)
            {
                Assert.AreSame(zone, OverworldRules.ZoneAt(w, zone.Bounds.center));
            }
        }

        [Test]
        public void Neighbours_HaveOpenPassages_AndTheRegionIsConnected()
        {
            OverworldLayout w = World;
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("vale_dos_satiros"), w.Zone("vau_do_pactolo")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("vale_dos_satiros"), w.Zone("clareiras_do_baile")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("escadaria_dos_cestos"), w.Zone("farnkesh")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("farnkesh"), w.Zone("pontes_de_vento")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("ilhas_de_nuvem"), w.Zone("ninho_da_huma")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("vale_dos_satiros"), w.Zone("farnkesh")));

            var seen = new HashSet<OverworldZone> { w.Zone("vale_dos_satiros") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to the vale");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromTheVale_FromTheCatalogsLevel140()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.ValeSatirosMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 140 at the vale (53 is 140–148)");
            Assert.AreEqual(140, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 166, "the gorge and Huma's nest are the end of the peaks");
            Assert.AreEqual(200, w.Zones.Max(z => z.MaxLevel), "Huma's nest of the ☆ sheets (58: 180–200)");
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.NinhoHumaMapId))
            {
                Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1300f, $"{zone.Id}: the nest is far above the vale");
            }

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
        public void SoftBarrier_StormInTheGorge_WingsOnTheIsles_NothingInTheTowns_NorAtTheEntrances()
        {
            OverworldLayout w = World;
            var gates = new Dictionary<string, string>
            {
                { OverworldLayouts.GargantaAysMapId, MapRequirementTags.StormProtection },
                { OverworldLayouts.NinhoHumaMapId, MapRequirementTags.WingsOrDoubleJump },
            };

            foreach (KeyValuePair<string, string> gate in gates)
            {
                Assert.AreEqual(gate.Value, MapCatalog.Get(gate.Key).SpecialRequirementTag, gate.Key);
                CollectionAssert.IsNotEmpty(w.ZonesOfMap(gate.Key).ToList());
                foreach (OverworldZone zone in w.ZonesOfMap(gate.Key))
                {
                    Assert.AreEqual(gate.Value, zone.RequirementTag, zone.Id);
                    Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 220, CharacterId.Lavine, null), $"{zone.Id}: asks for {gate.Value}");
                    Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 220, CharacterId.Lavine, tag => tag == gate.Value));
                }
            }

            foreach (string map in FarnkeshMaps.Where(m => !gates.ContainsKey(m)))
            {
                Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(map).SpecialRequirementTag, map);
            }

            foreach (OverworldZone zone in w.Zones.Where(z => z.MapId == null || !gates.ContainsKey(z.MapId)))
            {
                Assert.IsTrue(string.IsNullOrEmpty(zone.RequirementTag), $"{zone.Id}: only the gorge and the isles ask for an item");
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("vale_dos_satiros"), 140, CharacterId.Lavine, null), "the vale");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("farnkesh"), 145, CharacterId.Lavine, null), "Farnkesh");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("vinhas_de_gordias"), 140, CharacterId.Lavine, null), "the way in from the meltwater lake");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("prados_do_vento"), 146, CharacterId.Lavine, null), "the way in from Ohya's crest");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("vau_do_pactolo"), 100, CharacterId.Lavine, null), "the peaks are for level 140");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("ninho_da_huma"), 160, CharacterId.Lavine, tag => true));
        }

        [Test]
        public void Gates_TwoToTheSteppeSouth_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            Assert.AreEqual(2, gates.Count);
            foreach (MapExitSpec gate in gates)
            {
                Assert.AreEqual(MapRegion.EstepeDosNartes, MapCatalog.Get(gate.TargetMapId).Region, $"{gate.Id}: to the steppe");
                Assert.Less(gate.Trigger.center.y - w.Bounds.yMin, 3f, $"{gate.Id}: on the south edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.ValeSatirosMapId, gate.TargetMapId, out _), gate.Id);
                Assert.Greater(Vector2.Dot(gate.ArrivalFacing, w.Bounds.center - gate.Arrival), 0f, $"{gate.Id}: facing into the peaks");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
                Assert.IsTrue(string.IsNullOrEmpty(OverworldRules.ZoneAt(w, gate.Arrival).RequirementTag), $"{gate.Id}: the entrance asks for nothing");
            }

            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by the vale");

            Vector2 dock = w.ArrivalFor(OverworldLayouts.ValeSatirosMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in FarnkeshMaps)
            {
                OverworldSettlement town = w.SettlementOf(id);
                Assert.AreEqual(town.DockArrival, w.ArrivalFor(id, OverworldLayouts.ValeSatirosMapId, out _), id);
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

        // ------------------------------------------------------------------
        // Relief, water and dressing
        // ------------------------------------------------------------------

        [Test]
        public void Height_IsPureAndContinuous_AndChunksHaveNoSeams()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            var rnd = new System.Random(53);
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
        public void ThePeaks_PinnaclesCliffsGorgeAndIslesStandUp_TheValeAndTheCloudsLow_ThePactoloCarved_WithAFordAndAStoneBridge()
        {
            OverworldLayout w = World;
            foreach (string high in new[] { "farnkesh", "escadaria_dos_cestos", "bosque_de_pinaculos", "pinaculos_do_leste", "pontes_de_vento" })
            {
                Assert.Greater(MaxHeight(w.Zone(high)), 18f, $"{high}: the sandstone pinnacles");
            }

            Assert.Greater(MaxHeight(w.Zone("ermidas_suspensas")), 18f, "the hermitages' cliff");
            Assert.Greater(MaxHeight(w.Zone("garganta_dos_ays")), 20f, "the gorge's walls");
            Assert.Greater(MaxHeight(w.Zone("ninho_da_huma")), 18f, "the isles round Huma's nest");
            Assert.Greater(MaxHeight(w.Zone("ilhas_de_nuvem")), 14f, "the cloud isles");
            foreach (string low in new[] { "vale_dos_satiros", "vau_do_pactolo", "clareiras_do_baile", "mar_de_nuvens" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 10f, $"{low}: low ground");
            }

            // Pinnacles, gorge and isles stand over a low floor (where the trails and clearings run).
            OverworldRelief relief = Relief;
            foreach (string towers in new[] { "bosque_de_pinaculos", "garganta_dos_ays", "ilhas_de_nuvem" })
            {
                OverworldZone zone = w.Zone(towers);
                var heights = new List<float>();
                for (float x = zone.Bounds.xMin + 80f; x < zone.Bounds.xMax - 80f; x += 9f)
                {
                    for (float z = zone.Bounds.yMin + 80f; z < zone.Bounds.yMax - 80f; z += 9f)
                    {
                        heights.Add(relief.HeightAt(new Vector2(x, z)));
                    }
                }

                heights.Sort();
                Assert.Less(heights[heights.Count / 5], 4f, $"{towers}: the floor between them");
                Assert.Greater(heights[heights.Count - 1], heights[heights.Count / 5] + 12f, $"{towers}: they stand over it");
            }

            // The Pactolo: carved, crossed at the vale's wide ford (shallow) and a stone bridge; it ends on the south edge, where the
            // Rio dos Nartes begins in the stitched world.
            MapRiver river = w.World.Rivers.Find(r => r.Points.Count > 2 && r.Points[0] == OverworldLayouts.PactoloRiver[0]);
            Assert.IsNotNull(river);
            Assert.GreaterOrEqual(river.Width, OverworldRelief.CarveMinWidth, "a carved river");
            Assert.AreEqual(w.Bounds.yMin, river.Points[river.Points.Count - 1].y, 1e-3f, "down to the south edge");
            Assert.GreaterOrEqual(river.Crossings.Count(c => !c.IsBridge), 1, "the wide ford");
            Assert.GreaterOrEqual(river.Crossings.Count(c => c.IsBridge), 1, "a stone bridge under the sea of clouds");
            foreach (MapCrossing ford in river.Crossings.Where(c => !c.IsBridge))
            {
                float h = relief.HeightAt(ford.At);
                Assert.Greater(h, -OverworldRelief.FordDepth - 0.05f, $"the ford at {ford.At} is shallow");
                Assert.Less(h, OverworldRelief.WaterY, $"the ford at {ford.At} is still under water");
            }

            Vector2 deep = Vector2.Lerp(OverworldLayouts.PactoloRiver[6], OverworldLayouts.PactoloRiver[7], 0.5f);
            Assert.Less(relief.HeightAt(deep), OverworldRelief.WaterY - 0.5f, "deep in the river away from the crossings");
            foreach (MapCrossing crossing in river.Crossings.Where(c => c.IsBridge))
            {
                Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                float half = OverworldChunkBuilder.BridgeSpan(river) * 0.5f;
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At + across * half), 0.02f, $"bridge end at {crossing.At}");
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At - across * half), 0.02f, $"bridge end at {crossing.At}");
            }
        }

        [Test]
        public void EveryFarnkeshTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsFarnkesh(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.StoneNests).Weather, "Farnkesh's clear sky");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.CloudSea).Weather, "the sea of clouds");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Sandstorm, OverworldAreaAmbience.ClimateOf(OverworldTerrain.AysGorge).Weather, "dust in the Gorge of the Ays");
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.HumaNest).Wild);
            Assert.IsFalse(OverworldAreaAmbience.ClimateOf(OverworldTerrain.SatyrGlades).Wild);
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
                { "vinhas_de_gordias", new[] { OverworldDecor.GrapeVine } },
                { "vale_dos_satiros", new[] { OverworldDecor.GourdHut } },
                { "rio_pactolo", new[] { OverworldDecor.Willow, OverworldDecor.GoldPebbles } },
                { "mata_das_castanheiras", new[] { OverworldDecor.TwistedTree } },
                { "prados_dos_sinos", new[] { OverworldDecor.WindBell } },
                { "bosque_de_pinaculos", new[] { OverworldDecor.SandstonePinnacle } },
                { "farnkesh", new[] { OverworldDecor.Hermitage } },
                { "mar_de_nuvens", new[] { OverworldDecor.CloudPuff } },
                { "pontes_de_vento", new[] { OverworldDecor.RopeBridgeRun } },
                { "ermidas_suspensas", new[] { OverworldDecor.Hermitage } },
                { "garganta_dos_ays", new[] { OverworldDecor.ShelterStone } },
                { "ninho_da_huma", new[] { OverworldDecor.GoldenNest } },
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
        public void RopeBridges_LieAlongTheirLines_OverTheWindBridges_AndHaveNoCollider()
        {
            OverworldLayout w = World;
            int planks = 0;
            foreach (OverworldZone zone in w.Zones.Where(z => z.Terrain == OverworldTerrain.WindBridges || z.Terrain == OverworldTerrain.StoneNests))
            {
                foreach (Vector2Int c in InnerChunks(w, zone, 200))
                {
                    OverworldChunkContent content = OverworldChunkBuilder.Content(w, c);
                    foreach (OverworldProp run in content.Props.Where(p => p.Decor == OverworldDecor.RopeBridgeRun))
                    {
                        planks++;
                        float off = OverworldLayouts.WindBridgeLines.Min(line => MapLayoutRules.DistanceToPolyline(run.Position, line));
                        Assert.Less(off, 0.5f, $"a rope bridge run off its line at {run.Position}");
                        Assert.IsFalse(content.Solids.Exists(s => Vector2.Distance(s.Center, (run.Position + run.End) * 0.5f) < 0.3f), "the bridges are the way: no collider");
                    }
                }
            }

            Assert.Greater(planks, 40, "the rope bridges run a long way between the pinnacles");
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
            Debug.Log($"[Overworld Farnkesh] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
                $"generation avg {average:0.00} ms, p50 {times[times.Count / 2]:0.00} ms, p95 {times[(int)(times.Count * 0.95f)]:0.00} ms (Editor, Mono)");
            Assert.Less(average, 6.0, "chunk generation stays cheap even in the Editor");
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
