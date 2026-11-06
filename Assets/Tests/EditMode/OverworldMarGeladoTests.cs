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
    /// The continuous Região 11 — Mar Gelado (Docs/Mapas100.md §0.10, maps 59–64), in the mould of <see cref="OverworldFarnkeshTests"/>:
    /// one 2400 × 2400 m map with the Porto da Lua Branca inside (flat ground of ±30 m at y = 0 reserved for its recipe) and the
    /// posts of 60–64, maps 59–64 as zones, zones that tile it, open passages, level bands growing with the distance from the
    /// harbour from the catalog's 180, the soft barrier (Blizzard Resistance on the Mar das Auroras, Fine Thermal Clothes on Old
    /// Kul's lake, nothing in the town nor at the entrances), the gate towards Mingcheng (south edge) and the two towards the Picos
    /// de Farnkesh (east edge), the frozen sea's beasts (no boss), the relief (the fjords' walls, the ice vaults, the icebergs and
    /// the pressure ridges standing over low floors; the harbour, the bay, the tundra and the lake low; the canals carved and
    /// bridged, Omöl's lakes, the open leads and Old Kul's fishing holes) and the per-area dressing within the chunk budget.
    /// </summary>
    public class OverworldMarGeladoTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.MarGeladoRegionId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] GeloMaps =
        {
            OverworldLayouts.PortoLuaBrancaMapId, OverworldLayouts.CanaisGeloMapId, OverworldLayouts.TundraOmolMapId, OverworldLayouts.GaleriasMamutesMapId,
            OverworldLayouts.MarAurorasMapId, OverworldLayouts.FundoVelhoKulMapId,
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
        public void Catalog_HasMaps59To64_OnTheFrozenSea_ReturningToThePortoDaLuaBranca()
        {
            CollectionAssert.Contains(MapCatalog.Regions.ToList(), MapRegion.MarGelado);
            CollectionAssert.AreEqual(new[] { 59, 60, 61, 62, 63, 64 }, MapCatalog.InRegion(MapRegion.MarGelado).Select(m => m.MapId).ToArray());
            foreach (string map in GeloMaps)
            {
                Assert.AreEqual(MapCatalog.PortoDaLuaBranca, MapCatalog.ReturnCityOf(map), map);
                Assert.IsTrue(MapCatalog.Get(map).IsBuilt, map);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get(OverworldLayouts.PortoLuaBrancaMapId).Settlement);
            Assert.AreEqual(180, MapCatalog.Get(OverworldLayouts.PortoLuaBrancaMapId).MonsterLevelMin);
            Assert.AreEqual(240, MapCatalog.Get(OverworldLayouts.FundoVelhoKulMapId).MonsterLevelMax);
            Assert.AreEqual("Mar Gelado", MapTexts.RegionName(null, MapRegion.MarGelado));
            Assert.AreEqual("Porto da Lua Branca", MapTexts.Get(null, MapTexts.ReturnCityKey(MapCatalog.PortoDaLuaBranca)));
            Assert.IsNotNull(MapCatalog.RespawnCityFor(OverworldLayouts.FundoVelhoKulMapId), "a planned city falls back to an existing one");
        }

        [Test]
        public void Region_IsAsBigAsTheOthers_WithThePortoDaLuaBrancaInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.MarGelado, w.Region);
            Assert.AreEqual(MapCatalog.PortoDaLuaBranca, w.Vila.Id, "the harbour is the region's town (its first map, level 180)");
            Assert.AreEqual(CityTownRecipes.PortoDaLuaBrancaCityId, w.Vila.Recipe.CityId, "its own recipe");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, CityTownRecipes.PortoDaLuaBrancaSolidRadius + 5f, "room for the town's recipe");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais da Barca of the harbour (the Barca da Lua's quay)");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: the frozen sea is all round it");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, OverworldLayouts.MarGeladoCityGround, "±30 m of flat ground reserved for the city's recipe");
            Assert.AreEqual(Vector2.zero, w.Vila.Offset);
            Assert.AreEqual(OverworldLayouts.PortoLuaBrancaMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "the harbour stands in its own area (59)");

            foreach (string map in GeloMaps)
            {
                OverworldSettlement stop = w.SettlementOf(map);
                Assert.IsNotNull(stop, $"{map}: its town or post");
                Assert.AreEqual(map, OverworldRules.ZoneAt(w, stop.Offset).MapId, $"{stop.Id} stands in its map's area");
                Assert.AreEqual(MapCatalog.Get(map).SettlementId, stop.Id, $"{map}: the catalog's settlement id");
            }

            Assert.AreEqual(1, w.Settlements.Count(s => s.IsReturnCity), "one capital, five posts");
            Assert.AreEqual(6, w.Settlements.Count);
            Assert.Greater(w.SettlementOf(OverworldLayouts.FundoVelhoKulMapId).Offset.y, 1400f, "the Great Hole is far out on Old Kul's lake");
            Assert.Less(w.SettlementOf(OverworldLayouts.TundraOmolMapId).Offset.x, -800f, "Yen's tent is out west on the tundra");
            Assert.Greater(w.SettlementOf(OverworldLayouts.GaleriasMamutesMapId).Offset.x, 800f, "the Boca do Gelo is east, under the peaks");
        }

        [Test]
        public void Region_HoldsMaps59To64_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 13, "many kinds of shore, ice, tundra, fjord, gallery, sea and lake");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsMarGelado(z.Terrain)), "the frozen sea's own terrains");
            foreach (string id in GeloMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the frozen sea");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the frozen sea");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 250, "the frozen sea is full of clearings");
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
            Assert.AreEqual("porto_da_lua_branca", OverworldRules.ZoneAt(w, OverworldLayouts.PortoLuaBrancaOffset).Id);
            Assert.AreEqual("canais_de_gelo", OverworldRules.ZoneAt(w, OverworldLayouts.CasaDeNeveOffset).Id);
            Assert.AreEqual("tundra_de_omol", OverworldRules.ZoneAt(w, OverworldLayouts.TendaDeYenOffset).Id);
            Assert.AreEqual("galerias_dos_mamutes", OverworldRules.ZoneAt(w, OverworldLayouts.BocaDoGeloOffset).Id);
            Assert.AreEqual("ilha_do_farol", OverworldRules.ZoneAt(w, OverworldLayouts.FarolDeGeloOffset).Id);
            Assert.AreEqual("fundo_do_velho_kul", OverworldRules.ZoneAt(w, OverworldLayouts.BuracoGrandeOffset).Id);
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
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("porto_da_lua_branca"), w.Zone("baia_gelada")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("porto_da_lua_branca"), w.Zone("canais_de_gelo")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("canais_de_gelo"), w.Zone("mar_das_auroras")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("mar_das_auroras"), w.Zone("fundo_do_velho_kul")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("tundra_de_omol"), w.Zone("lagos_de_omol")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("porto_da_lua_branca"), w.Zone("fundo_do_velho_kul")));

            var seen = new HashSet<OverworldZone> { w.Zone("porto_da_lua_branca") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to the harbour");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromTheHarbour_FromTheCatalogsLevel180()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.PortoLuaBrancaMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 180 at the harbour (59 is 180–188)");
            Assert.AreEqual(180, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 226, "Old Kul's lake is the end of the frozen sea");
            Assert.AreEqual(240, w.Zones.Max(z => z.MaxLevel), "Old Kul's Deep of the ☆ sheets (64: 226–240)");
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.FundoVelhoKulMapId))
            {
                Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1300f, $"{zone.Id}: the lake is far out from the harbour");
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
        public void SoftBarrier_BlizzardOnTheAuroraSea_ThermalClothesOnKulsLake_NothingInTheTown_NorAtTheEntrances()
        {
            OverworldLayout w = World;
            var gates = new Dictionary<string, string>
            {
                { OverworldLayouts.MarAurorasMapId, MapRequirementTags.BlizzardResistance },
                { OverworldLayouts.FundoVelhoKulMapId, MapRequirementTags.ThermalClothes },
            };

            foreach (KeyValuePair<string, string> gate in gates)
            {
                Assert.AreEqual(gate.Value, MapCatalog.Get(gate.Key).SpecialRequirementTag, gate.Key);
                CollectionAssert.IsNotEmpty(w.ZonesOfMap(gate.Key).ToList());
                foreach (OverworldZone zone in w.ZonesOfMap(gate.Key))
                {
                    Assert.AreEqual(gate.Value, zone.RequirementTag, zone.Id);
                    Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 260, CharacterId.Lavine, null), $"{zone.Id}: asks for {gate.Value}");
                    Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 260, CharacterId.Lavine, tag => tag == gate.Value));
                }
            }

            foreach (string map in GeloMaps.Where(m => !gates.ContainsKey(m)))
            {
                Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(map).SpecialRequirementTag, map);
            }

            foreach (OverworldZone zone in w.Zones.Where(z => z.MapId == null || !gates.ContainsKey(z.MapId)))
            {
                Assert.IsTrue(string.IsNullOrEmpty(zone.RequirementTag), $"{zone.Id}: only the aurora sea and Kul's lake ask for an item");
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("porto_da_lua_branca"), 180, CharacterId.Lavine, null), "the harbour");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("passo_da_muralha"), 180, CharacterId.Lavine, null), "the way in from Mingcheng");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("terra_congelada"), 202, CharacterId.Lavine, null), "the way in from the peaks' foot woods");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("galerias_dos_mamutes"), 204, CharacterId.Lavine, null), "the way in from the pinnacle wood");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("porto_da_lua_branca"), 140, CharacterId.Lavine, null), "the frozen sea is for level 180");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("fundo_do_velho_kul"), 200, CharacterId.Lavine, tag => true));
        }

        [Test]
        public void Gates_OneToMingchengSouth_TwoToThePeaksEast_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            Assert.AreEqual(3, gates.Count);
            foreach (MapExitSpec gate in gates)
            {
                MapRegion target = MapCatalog.Get(gate.TargetMapId).Region;
                if (target == MapRegion.PlanicieDeMingcheng)
                {
                    Assert.Less(gate.Trigger.center.y - w.Bounds.yMin, 3f, $"{gate.Id}: on the south edge");
                }
                else
                {
                    Assert.AreEqual(MapRegion.PicosDeFarnkesh, target, $"{gate.Id}: to the peaks");
                    Assert.Less(w.Bounds.xMax - gate.Trigger.center.x, 3f, $"{gate.Id}: on the east edge");
                }

                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.PortoLuaBrancaMapId, gate.TargetMapId, out _), gate.Id);
                Assert.Greater(Vector2.Dot(gate.ArrivalFacing, w.Bounds.center - gate.Arrival), 0f, $"{gate.Id}: facing into the frozen sea");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
                Assert.IsTrue(string.IsNullOrEmpty(OverworldRules.ZoneAt(w, gate.Arrival).RequirementTag), $"{gate.Id}: the entrance asks for nothing");
            }

            Assert.AreEqual(1, gates.Count(g => MapCatalog.Get(g.TargetMapId).Region == MapRegion.PlanicieDeMingcheng));
            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by the harbour");

            Vector2 dock = w.ArrivalFor(OverworldLayouts.PortoLuaBrancaMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in GeloMaps)
            {
                OverworldSettlement town = w.SettlementOf(id);
                Assert.AreEqual(town.DockArrival, w.ArrivalFor(id, OverworldLayouts.PortoLuaBrancaMapId, out _), id);
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
            var rnd = new System.Random(59);
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
        public void TheFrozenSea_FjordsVaultsBergsAndRidgesStandUp_TheHarbourBayTundraAndLakeLow_TheCanalsCarvedAndBridged()
        {
            OverworldLayout w = World;
            foreach (string high in new[] { "fiorde_das_presas", "fiorde_negro", "fiorde_do_kul" })
            {
                Assert.Greater(MaxHeight(w.Zone(high)), 20f, $"{high}: the fjord's walls");
            }

            Assert.Greater(MaxHeight(w.Zone("galerias_dos_mamutes")), 14f, "the ice vaults of the galleries");
            Assert.Greater(MaxHeight(w.Zone("mar_das_auroras")), 16f, "the icebergs of the aurora sea");
            Assert.Greater(MaxHeight(w.Zone("gelo_rachado")), 5f, "the pressure ridges");
            foreach (string low in new[] { "porto_da_lua_branca", "baia_gelada", "tundra_de_omol", "fundo_do_velho_kul", "buracos_de_pesca" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 8f, $"{low}: low ground");
            }

            // Fjords, vaults and bergs stand over a low floor (where the trails and clearings run).
            OverworldRelief relief = Relief;
            foreach (string towers in new[] { "fiorde_negro", "galerias_dos_mamutes", "mar_das_auroras" })
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

            // The canals: carved black water, two whalebone bridges, deep away from them; Omöl's lakes, the leads and Old Kul's holes.
            List<MapRiver> canals = w.World.Rivers.Where(r => r.Points.Count > 2).ToList();
            Assert.AreEqual(3, canals.Count, "the west, east and middle canals");
            Assert.AreEqual(2, canals.Sum(r => r.Crossings.Count(c => c.IsBridge)), "a bridge on the west canal and one on the middle canal");
            foreach (MapRiver canal in canals)
            {
                Assert.GreaterOrEqual(canal.Width, OverworldRelief.CarveMinWidth, "a carved canal");
                foreach (MapCrossing crossing in canal.Crossings.Where(c => c.IsBridge))
                {
                    Vector2 across = MapAreaView.RiverNormal(canal, crossing.At);
                    float half = OverworldChunkBuilder.BridgeSpan(canal) * 0.5f;
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At + across * half), 0.02f, $"bridge end at {crossing.At}");
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At - across * half), 0.02f, $"bridge end at {crossing.At}");
                }
            }

            Vector2 deep = Vector2.Lerp(OverworldLayouts.CanalLeste[2], OverworldLayouts.CanalLeste[3], 0.5f);
            Assert.Less(relief.HeightAt(deep), OverworldRelief.WaterY - 0.5f, "deep in the east canal");
            int lakes = OverworldLayouts.OmolLakes.Length + OverworldLayouts.KulHoles.Length + OverworldLayouts.AuroraLeads.Length;
            Assert.AreEqual(lakes, w.World.Rivers.Count(r => r.Points.Count == 2), "every lake, lead and fishing hole is water");
            Assert.AreEqual(8, OverworldLayouts.KulHoles.Length, "seven fishing holes and the Great Hole");
            foreach ((Vector2 a, Vector2 b, float width) in OverworldLayouts.KulHoles)
            {
                Assert.AreEqual(OverworldLayouts.FundoVelhoKulMapId, OverworldRules.ZoneAt(w, (a + b) * 0.5f).MapId, "Old Kul's holes are on his lake");
                Assert.Less(relief.HeightAt((a + b) * 0.5f), OverworldRelief.WaterY, "a hole in the ice");
            }
        }

        [Test]
        public void EveryMarGeladoTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsMarGelado(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.MoonHarbor).Weather, "the harbour's clear polar sky");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Snow, OverworldAreaAmbience.ClimateOf(OverworldTerrain.PackIce).Weather, "the blizzard on the pack ice");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.IceCanals).Weather, "sea smoke on the canals");
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.BlackIce).Wild);
            Assert.IsFalse(OverworldAreaAmbience.ClimateOf(OverworldTerrain.MoonHarbor).Wild);
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
                { "passo_da_muralha", new[] { OverworldDecor.CrescentBoat } },
                { "porto_da_lua_branca", new[] { OverworldDecor.SnowDome } },
                { "baia_gelada", new[] { OverworldDecor.FishingHole } },
                { "canais_de_gelo", new[] { OverworldDecor.IceBlock, OverworldDecor.IceCrack } },
                { "gelo_rachado", new[] { OverworldDecor.IceBlock } },
                { "tundra_de_omol", new[] { OverworldDecor.LichenBoulder, OverworldDecor.LichenTuft } },
                { "pastos_de_rena", new[] { OverworldDecor.HerderTent } },
                { "galerias_dos_mamutes", new[] { OverworldDecor.MammothTusk, OverworldDecor.FrostCrystal } },
                { "terra_congelada", new[] { OverworldDecor.MammothTusk } },
                { "mar_das_auroras", new[] { OverworldDecor.Iceberg } },
                { "ilha_do_farol", new[] { OverworldDecor.IceBeacon } },
                { "fundo_do_velho_kul", new[] { OverworldDecor.FishRack, OverworldDecor.FishingHole } },
                { "gelo_negro", new[] { OverworldDecor.IceCrack } },
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
            Debug.Log($"[Overworld Mar Gelado] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
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
