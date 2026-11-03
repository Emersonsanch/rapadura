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
    /// The continuous Região 9 — Estepe dos Nartes (Docs/Mapas100.md §0.10, maps 47–52), in the mould of
    /// <see cref="OverworldMingchengTests"/>: one 2400 × 2400 m map with the Acampamento de Soslan, the Fortim and the Arvoredo
    /// inside (their own recipes on flat ground) and the posts of 48, 51 and 52, maps 47–52 as zones, zones that tile it,
    /// open passages, level bands growing with the distance from the camp from the catalog's 100, the soft barrier (Climbing
    /// Crampons on Amirani's chain, nothing on the way into the camp nor at the entrances), the gates towards the Costa (south edge)
    /// and the Planície de Mingcheng (west edge), the steppe's beasts (no boss), the relief (the Caucasus high, the kurgans' mounds,
    /// the steppe low, the river carved and crossed at fords and a stone bridge) and the per-area dressing (Amirani's chain among
    /// it) within the chunk budget.
    /// </summary>
    public class OverworldNartesTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.NartesId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] NartesMaps =
        {
            OverworldLayouts.SoslanMapId, OverworldLayouts.KurgansMapId, OverworldLayouts.FortimMapId, OverworldLayouts.HaripushpaMapId,
            OverworldLayouts.MilCavalosMapId, OverworldLayouts.AmiraniMapId,
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
        public void Catalog_HasMaps47To52_InTheSteppe_WithTheirReturnCities()
        {
            CollectionAssert.Contains(MapCatalog.Regions.ToList(), MapRegion.EstepeDosNartes);
            CollectionAssert.AreEqual(new[] { 47, 48, 49, 50, 51, 52 }, MapCatalog.InRegion(MapRegion.EstepeDosNartes).Select(m => m.MapId).ToArray());
            Assert.AreEqual(MapCatalog.AcampamentoSoslan, MapCatalog.ReturnCityOf(OverworldLayouts.SoslanMapId));
            Assert.AreEqual(MapCatalog.AcampamentoSoslan, MapCatalog.ReturnCityOf(OverworldLayouts.KurgansMapId));
            Assert.AreEqual(MapCatalog.Fortim, MapCatalog.ReturnCityOf(OverworldLayouts.FortimMapId));
            Assert.AreEqual(MapCatalog.Arvoredo, MapCatalog.ReturnCityOf(OverworldLayouts.HaripushpaMapId));
            Assert.AreEqual(MapCatalog.AcampamentoSoslan, MapCatalog.ReturnCityOf(OverworldLayouts.MilCavalosMapId));
            Assert.AreEqual(MapCatalog.Fortim, MapCatalog.ReturnCityOf(OverworldLayouts.AmiraniMapId));
            Assert.AreEqual(100, MapCatalog.Get(OverworldLayouts.SoslanMapId).MonsterLevelMin);
            Assert.AreEqual(150, MapCatalog.Get(OverworldLayouts.AmiraniMapId).MonsterLevelMax);
            Assert.AreEqual("Estepe dos Nartes", MapTexts.RegionName(null, MapRegion.EstepeDosNartes));
        }

        [Test]
        public void Region_IsAsBigAsTheOthers_WithTheCampTheFortAndTheGroveInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.EstepeDosNartes, w.Region);
            Assert.AreEqual(MapCatalog.AcampamentoSoslan, w.Vila.Id, "the centaurs' camp is the region's town");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais da Barca of the camp");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: the steppe is all round it");
            Assert.AreEqual(OverworldLayouts.SoslanMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "the camp stands in its own area (47)");

            Assert.AreEqual(CityTownRecipes.SoslanCityId, w.Vila.Id, "its own recipe");
            foreach ((string map, string city, string recipe, float solid) in new[]
            {
                (OverworldLayouts.SoslanMapId, MapCatalog.AcampamentoSoslan, CityTownRecipes.SoslanCityId, CityTownRecipes.SoslanSolidRadius),
                (OverworldLayouts.FortimMapId, MapCatalog.Fortim, CityTownRecipes.FortimCityId, CityTownRecipes.FortimSolidRadius),
                (OverworldLayouts.HaripushpaMapId, MapCatalog.Arvoredo, CityTownRecipes.ArvoredoCityId, CityTownRecipes.ArvoredoSolidRadius),
            })
            {
                OverworldSettlement town = w.SettlementOf(map);
                Assert.AreEqual(city, town.Id, $"{map}: the town keeps the catalog's city id");
                Assert.AreEqual(recipe, town.Recipe.CityId, $"{map}: the town's own recipe");
                Assert.IsTrue(town.IsReturnCity, $"{map}: its town wakes the hero");
                Assert.IsFalse(town.Recipe.Backdrop);
                Assert.GreaterOrEqual(town.GroundHalfExtent, solid + 5f, $"{map}: room for the town's recipe");
                Assert.AreEqual(map, OverworldRules.ZoneAt(w, town.Offset).MapId, $"{city} stands in its map's area");
            }

            foreach (string map in NartesMaps)
            {
                string city = MapCatalog.ReturnCityOf(map);
                Assert.IsTrue(w.Settlements.Exists(s => s.IsReturnCity && s.Id == city), $"{map}: its return city {city} stands in the steppe");
            }

            Assert.AreEqual(3, w.Settlements.Count(s => s.IsReturnCity));
            Assert.Greater(w.SettlementOf(OverworldLayouts.FortimMapId).Offset.y, 800f, "the hill giants' fort is north of the camp");
            Assert.Greater(w.SettlementOf(OverworldLayouts.HaripushpaMapId).Offset.y, 1300f, "the grove is in the far north-west taiga");
        }

        [Test]
        public void Region_HoldsMaps47To52_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 13, "many kinds of steppe, hill, wood and mountain");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsNartes(z.Terrain)), "the steppe's own terrains");
            foreach (string id in NartesMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the steppe");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the steppe");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 300, "the steppe is full of clearings");
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
            Assert.AreEqual("acampamento_de_soslan", OverworldRules.ZoneAt(w, OverworldLayouts.SoslanOffset).Id);
            Assert.AreEqual("fortim_dos_gigantes", OverworldRules.ZoneAt(w, OverworldLayouts.FortimOffset).Id);
            Assert.AreEqual("arvoredo_de_haripushpa", OverworldRules.ZoneAt(w, OverworldLayouts.ArvoredoOffset).Id);
            Assert.AreEqual("torre_de_vigia", OverworldRules.ZoneAt(w, OverworldLayouts.TorreDeVigiaOffset).Id);
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
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("acampamento_de_soslan"), w.Zone("vau_largo")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("acampamento_de_soslan"), w.Zone("kurgans_de_ouro")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("kurgans_de_ouro"), w.Zone("kurgan_aberto")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("fortim_dos_gigantes"), w.Zone("taiga_de_laricos")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("torre_de_vigia"), w.Zone("corrente_de_amirani")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("acampamento_de_soslan"), w.Zone("corrente_de_amirani")));

            var seen = new HashSet<OverworldZone> { w.Zone("acampamento_de_soslan") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to the camp");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromTheCamp_FromTheCatalogsLevel100()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.SoslanMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 100 at the camp (47 is 100–108)");
            Assert.AreEqual(100, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 120, "the grove and Ohya's crest are the end of the steppe");
            Assert.GreaterOrEqual(w.Zones.Max(z => z.MaxLevel), 145, "Amirani's chain of the ☆ sheets (52: 134–150)");
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.AmiraniMapId))
            {
                Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1200f, $"{zone.Id}: the Caucasus is far from the camp");
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
        public void SoftBarrier_CramponsOnAmiranisChain_NothingOnTheWayToTheCamp_NorAtTheEntrances()
        {
            OverworldLayout w = World;
            // 52 asks for Climbing Crampons (sold in Sangkent): every zone of 52 asks for them, no other zone asks for anything.
            Assert.AreEqual(MapRequirementTags.ClimbingCrampons, MapCatalog.Get(OverworldLayouts.AmiraniMapId).SpecialRequirementTag);
            CollectionAssert.IsNotEmpty(w.ZonesOfMap(OverworldLayouts.AmiraniMapId).ToList());
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.AmiraniMapId))
            {
                Assert.AreEqual(MapRequirementTags.ClimbingCrampons, zone.RequirementTag, zone.Id);
                Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 160, CharacterId.Lavine, null), $"{zone.Id}: asks for the crampons");
                Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 160, CharacterId.Lavine, tag => tag == MapRequirementTags.ClimbingCrampons));
            }

            foreach (string map in NartesMaps.Where(m => m != OverworldLayouts.AmiraniMapId))
            {
                Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(map).SpecialRequirementTag, map);
            }

            foreach (OverworldZone zone in w.Zones.Where(z => z.MapId != OverworldLayouts.AmiraniMapId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(zone.RequirementTag), $"{zone.Id}: only Amirani's chain asks for an item");
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("acampamento_de_soslan"), 100, CharacterId.Lavine, null), "the camp");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("vau_largo"), 100, CharacterId.Lavine, null), "the way in from the Costa");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("estepe_dos_ventos"), 107, CharacterId.Lavine, null), "the way in from Mingcheng's loess");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("vale_das_tabuas_de_sonho"), 112, CharacterId.Lavine, null), "the way in from the Torres Brancas");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("vau_largo"), 50, CharacterId.Lavine, null), "the steppe is for level 100");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("cume_de_ohya"), 120, CharacterId.Lavine, tag => true));
        }

        [Test]
        public void Gates_ThreeToTheCostaSouth_ThreeToMingchengWest_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            Assert.AreEqual(6, gates.Count);
            foreach (MapExitSpec gate in gates)
            {
                MapRegion region = MapCatalog.Get(gate.TargetMapId).Region;
                if (region == MapRegion.CostaDasMoncoes)
                {
                    Assert.Less(gate.Trigger.center.y - w.Bounds.yMin, 3f, $"{gate.Id}: on the south edge");
                }
                else
                {
                    Assert.AreEqual(MapRegion.PlanicieDeMingcheng, region, $"{gate.Id}: to the Costa or to Mingcheng");
                    Assert.Less(gate.Trigger.center.x - w.Bounds.xMin, 3f, $"{gate.Id}: on the west edge");
                }

                // Two passes lead to the Torres Brancas' map: walking in from it, the hero arrives at the first one.
                MapExitSpec first = gates.First(e => e.TargetMapId == gate.TargetMapId);
                Assert.AreEqual(first.Arrival, w.ArrivalFor(OverworldLayouts.SoslanMapId, gate.TargetMapId, out _), gate.Id);
                Assert.Greater(Vector2.Dot(gate.ArrivalFacing, w.Bounds.center - gate.Arrival), 0f, $"{gate.Id}: facing into the steppe");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
            }

            Assert.AreEqual(3, gates.Count(g => MapCatalog.Get(g.TargetMapId).Region == MapRegion.CostaDasMoncoes));
            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by the camp");

            Vector2 dock = w.ArrivalFor(OverworldLayouts.SoslanMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in NartesMaps)
            {
                OverworldSettlement town = w.SettlementOf(id);
                Assert.AreEqual(town.DockArrival, w.ArrivalFor(id, OverworldLayouts.SoslanMapId, out _), id);
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
            var rnd = new System.Random(37);
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
        public void TheSteppe_TheCaucasusHigh_TheKurgansRise_TheSteppeLow_TheRiverCarved_WithFordsAndAStoneBridge()
        {
            OverworldLayout w = World;
            Assert.Greater(MaxHeight(w.Zone("cume_de_ohya")), 25f, "Ohya's crest");
            Assert.Greater(MaxHeight(w.Zone("corrente_de_amirani")), 22f, "the chain's crags");
            foreach (string high in new[] { "torre_de_vigia", "vale_das_torres_svan", "fortim_dos_gigantes", "pastos_dos_gigantes" })
            {
                Assert.Greater(MaxHeight(w.Zone(high)), 11f, $"{high}: high ground");
            }

            Assert.Greater(MaxHeight(w.Zone("kurgans_de_ouro")), 7f, "the kurgans' mounds");
            foreach (string low in new[] { "acampamento_de_soslan", "vau_largo", "estepe_de_soslan", "estepe_das_saigas" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 10f, $"{low}: the open steppe");
            }

            // The kurgans: round mounds on a flat floor (the zone's floor is low, its mounds stand well over it).
            OverworldRelief relief = Relief;
            OverworldZone kurgans = w.Zone("kurgans_de_ouro");
            var heights = new List<float>();
            for (float x = kurgans.Bounds.xMin + 80f; x < kurgans.Bounds.xMax - 80f; x += 9f)
            {
                for (float z = kurgans.Bounds.yMin + 80f; z < kurgans.Bounds.yMax - 80f; z += 9f)
                {
                    heights.Add(relief.HeightAt(new Vector2(x, z)));
                }
            }

            heights.Sort();
            Assert.Less(heights[heights.Count / 4], 2f, "the floor between the kurgans");
            Assert.Greater(heights[heights.Count - 1], heights[heights.Count / 4] + 5f, "the kurgans stand over it");

            // The Rio dos Nartes: carved, crossed at the two wide fords of the camp's roads (shallow) and a stone bridge.
            MapRiver river = w.World.Rivers.Find(r => r.Points.Count > 2 && r.Points[0] == OverworldLayouts.NartesRiver[0]);
            Assert.IsNotNull(river);
            Assert.GreaterOrEqual(river.Width, OverworldRelief.CarveMinWidth, "a carved river");
            Assert.GreaterOrEqual(river.Crossings.Count(c => !c.IsBridge), 2, "the wide fords");
            Assert.GreaterOrEqual(river.Crossings.Count(c => c.IsBridge), 1, "a stone bridge under the Torre de Vigia");
            foreach (MapCrossing ford in river.Crossings.Where(c => !c.IsBridge))
            {
                float h = relief.HeightAt(ford.At);
                Assert.Greater(h, -OverworldRelief.FordDepth - 0.05f, $"the ford at {ford.At} is shallow");
                Assert.Less(h, OverworldRelief.WaterY, $"the ford at {ford.At} is still under water");
            }

            Vector2 deep = Vector2.Lerp(OverworldLayouts.NartesRiver[3], OverworldLayouts.NartesRiver[4], 0.5f);
            Assert.Less(relief.HeightAt(deep), OverworldRelief.WaterY - 0.5f, "deep in the river away from the fords");
            foreach (MapCrossing crossing in river.Crossings.Where(c => c.IsBridge))
            {
                Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                float half = OverworldChunkBuilder.BridgeSpan(river) * 0.5f;
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At + across * half), 0.02f, $"bridge end at {crossing.At}");
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At - across * half), 0.02f, $"bridge end at {crossing.At}");
            }
        }

        [Test]
        public void EveryNartesTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsNartes(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.FeatherSteppe).Weather, "clear sky over the steppe");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Snow, OverworldAreaAmbience.ClimateOf(OverworldTerrain.ChainPeak).Weather, "snow on the chain");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.Taiga).Weather, "mist in the taiga");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Sandstorm, OverworldAreaAmbience.ClimateOf(OverworldTerrain.WormwoodSteppe).Weather, "dust over the wormwood");
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.SnowCrest).Wild);
            Assert.IsFalse(OverworldAreaAmbience.ClimateOf(OverworldTerrain.HorseCamp).Wild);
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
                { "acampamento_de_soslan", new[] { OverworldDecor.FeltYurt, OverworldDecor.HorseStandard } },
                { "estepe_de_soslan", new[] { OverworldDecor.FeatherTuft } },
                { "estepe_das_saigas", new[] { OverworldDecor.SaltCrust } },
                { "vau_largo", new[] { OverworldDecor.Willow } },
                { "kurgans_de_ouro", new[] { OverworldDecor.StoneBaba } },
                { "bosque_de_betulas", new[] { OverworldDecor.Birch } },
                { "taiga_de_laricos", new[] { OverworldDecor.Larch } },
                { "vale_das_tabuas_de_sonho", new[] { OverworldDecor.DreamTablet } },
                { "fortim_dos_gigantes", new[] { OverworldDecor.EarthRampart } },
                { "campos_de_papoula", new[] { OverworldDecor.PoppyTuft } },
                { "vale_das_torres_svan", new[] { OverworldDecor.SvanTower } },
                { "corrente_de_amirani", new[] { OverworldDecor.ChainRun } },
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
        public void AmiranisChain_LiesAlongItsLine_UpTheCaucasus_AndHasNoCollider()
        {
            OverworldLayout w = World;
            int links = 0;
            foreach (OverworldZone zone in w.Zones.Where(z => z.Terrain == OverworldTerrain.ChainPeak || z.Terrain == OverworldTerrain.SnowCrest))
            {
                foreach (Vector2Int c in InnerChunks(w, zone, 200))
                {
                    OverworldChunkContent content = OverworldChunkBuilder.Content(w, c);
                    foreach (OverworldProp run in content.Props.Where(p => p.Decor == OverworldDecor.ChainRun))
                    {
                        links++;
                        Assert.Less(MapLayoutRules.DistanceToPolyline(run.Position, OverworldLayouts.AmiraniChain), 0.5f, $"a chain run off its line at {run.Position}");
                        Assert.IsFalse(content.Solids.Exists(s => Vector2.Distance(s.Center, (run.Position + run.End) * 0.5f) < 0.3f), "the chain is the climbers' way: no collider");
                    }
                }
            }

            Assert.Greater(links, 40, "the chain runs a long way up the mountain");
            Vector2[] line = OverworldLayouts.AmiraniChain;
            Assert.Greater(Relief.HeightAt(line[line.Length - 1]), Relief.HeightAt(line[0]) - 5f, "it climbs (or at least never ends in a hollow)");
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
            Debug.Log($"[Overworld Nartes] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
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
