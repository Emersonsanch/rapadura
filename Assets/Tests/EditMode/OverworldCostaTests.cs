using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
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
    /// The continuous Região 5 — Costa das Monções (Docs/Mapas100.md §0.10, maps 25–30), in the mould of
    /// <see cref="OverworldKaggenTests"/>: one 2400 × 2400 m map with Taa Pwani inside, 25–30 as zones (Aratta's and Kirta's
    /// ground reserved flat with a post), zones that tile it, open passages, level bands growing with the distance from Taa
    /// Pwani from the catalog's 45, the soft barrier (Dhow Passage, Artificers' Relic, Breathing Shell, Air Bubble, Root
    /// Boots), the gate towards the Zarafshan on the west edge, the coast's beasts (the thief gull and the coconut crab
    /// among them, no boss), the coast's relief (cliffs high, beaches and shallows low, the carved water blocking and its
    /// fords and bridges marked and walkable) and the per-area dressing within the chunk budget.
    /// </summary>
    public class OverworldCostaTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.CostaId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] CostaMaps =
        {
            OverworldLayouts.TaaPwaniMapId, OverworldLayouts.AuroquesId, OverworldLayouts.MeroeId, OverworldLayouts.KirtaMapId, OverworldLayouts.KilwaId,
            OverworldLayouts.LiyongoId,
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
        public void Region_IsAsBigAsTheOthers_WithTaaPwaniInside_AndTheRaceCitiesGroundReserved()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Bounds.width, 2400f);
            Assert.GreaterOrEqual(w.Bounds.height, 2400f);
            Assert.AreEqual(MapRegion.CostaDasMoncoes, w.Region);
            Assert.AreEqual(CityTownRecipes.TaaPwaniCityId, w.Vila.Id, "Taa Pwani is the region's town");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais da Barca of Taa Pwani");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: the coast is all round it");
            Assert.AreEqual(OverworldLayouts.TaaPwaniMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "Taa Pwani stands on its beach (25)");
            Assert.AreEqual(CityCatalog.TaaPwani, MapCatalog.ReturnCityOf(OverworldLayouts.LiyongoId), "30 wakes the hero in Taa Pwani too");

            // Aratta (26) and Kirta (28): their ids, return cities of 26–27 and 28–29, wide flat ground kept for their recipes.
            foreach ((string map, string next, string city) in new[]
            {
                (OverworldLayouts.AuroquesId, OverworldLayouts.MeroeId, MapCatalog.Aratta), (OverworldLayouts.KirtaMapId, OverworldLayouts.KilwaId, MapCatalog.Kirta),
            })
            {
                OverworldSettlement s = w.SettlementOf(map);
                Assert.IsNotNull(s, map);
                Assert.AreEqual(city, s.Id, $"{map}: the race city's id");
                Assert.IsTrue(s.IsReturnCity, city);
                Assert.AreEqual(MapCatalog.ReturnCityOf(map), s.Id);
                Assert.AreEqual(MapCatalog.ReturnCityOf(next), s.Id);
                Assert.GreaterOrEqual(s.GroundHalfExtent, OverworldLayouts.RaceCityGround, $"{city}: room for the race city");
                Assert.AreEqual(map, OverworldRules.ZoneAt(w, s.Offset).MapId);
            }
        }

        [Test]
        public void Region_HoldsMaps25To30_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 15, "many kinds of coast");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsCoast(z.Terrain)), "the coast's own terrains");
            foreach (string id in CostaMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the Costa");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Costa");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 300, "the coast is full of clearings");
            var all = w.World.Zones.SelectMany(s => s.AllEntries()).Select(e => e.EnemyId).ToList();
            Assert.Contains("enemy_forest_thief_gull", all, "the Gaivota Ladra steals on the beaches");
            Assert.Contains("enemy_forest_coconut_crab", all, "the Caranguejo-dos-Coqueiros climbs the palms");
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
            Assert.AreEqual("praia_de_taa_pwani", OverworldRules.ZoneAt(w, OverworldLayouts.TaaPwaniOffset).Id);
            Assert.AreEqual("savana_dos_auroques", OverworldRules.ZoneAt(w, OverworldLayouts.ArattaOffset).Id);
            Assert.AreEqual("piramides_altas_de_meroe", OverworldRules.ZoneAt(w, OverworldLayouts.PatioRelevosOffset).Id);
            Assert.AreEqual("atol_de_kirta", OverworldRules.ZoneAt(w, OverworldLayouts.KirtaOffset).Id);
            Assert.AreEqual("kilwa_afundada", OverworldRules.ZoneAt(w, OverworldLayouts.FarolDeCoralOffset).Id);
            Assert.AreEqual("canais_de_liyongo", OverworldRules.ZoneAt(w, OverworldLayouts.CaisDasDhowsOffset).Id);
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
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("praia_de_taa_pwani"), w.Zone("recife_de_taa_pwani")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("mata_da_restinga"), w.Zone("dunas_das_casuarinas")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("recife_de_taa_pwani"), w.Zone("atol_de_kirta")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("kilwa_afundada"), w.Zone("palacio_de_husuni")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("canais_de_liyongo"), w.Zone("mangues_de_liyongo")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("praia_de_taa_pwani"), w.Zone("mangues_de_liyongo")));

            var seen = new HashSet<OverworldZone> { w.Zone("praia_de_taa_pwani") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to Taa Pwani");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromTaaPwani_FromTheCatalogsLevel45()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.TaaPwaniMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 45 at Taa Pwani (25 is 45–50)");
            Assert.AreEqual(45, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 62, "the mangroves of Liyongo are the end of the coast");
            Assert.AreEqual(72, w.Zones.Max(z => z.MaxLevel), "up to 30's 72");
            foreach (string far in new[] { OverworldLayouts.KilwaId, OverworldLayouts.LiyongoId })
            {
                foreach (OverworldZone zone in w.ZonesOfMap(far))
                {
                    Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1200f, $"{zone.Id}: the far maps are far from Taa Pwani");
                }
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
        public void SoftBarrier_TheRegionsRequirements_WithTheWalkLevel()
        {
            OverworldLayout w = World;
            var expected = new Dictionary<string, string>
            {
                { OverworldLayouts.MeroeId, MapRequirementTags.ArtificerRelic },
                { OverworldLayouts.KirtaMapId, MapRequirementTags.BreathingShell },
                { OverworldLayouts.KilwaId, MapRequirementTags.AirBubble },
                { OverworldLayouts.LiyongoId, MapRequirementTags.RootBoots },
            };
            foreach (KeyValuePair<string, string> pair in expected)
            {
                Assert.AreEqual(pair.Value, MapCatalog.Get(pair.Key).SpecialRequirementTag, pair.Key);
                foreach (OverworldZone zone in w.ZonesOfMap(pair.Key))
                {
                    Assert.AreEqual(pair.Value, zone.RequirementTag, zone.Id);
                    Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 80, CharacterId.Lavine, null), $"{zone.Id}: asks for {pair.Value}");
                    Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 80, CharacterId.Lavine, tag => tag == pair.Value));
                }
            }

            // 25: the Dhow Passage for the reef past the canal, never for Taa Pwani's own beach (its bazaar sells the passage).
            OverworldZone reef = w.Zone("recife_de_taa_pwani");
            Assert.AreEqual(MapRequirementTags.BoatOrDock, reef.RequirementTag);
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(reef, 50, CharacterId.Lavine, null), "the reef asks for the Dhow Passage");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(reef, 50, CharacterId.Lavine, tag => tag == MapRequirementTags.BoatOrDock));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("praia_de_taa_pwani"), 45, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("mata_da_restinga"), 45, CharacterId.Lavine, null), "the way in from the Zarafshan");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("mata_da_restinga"), 30, CharacterId.Lavine, null), "the coast is for level 45");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("mangues_de_liyongo"), 50, CharacterId.Lavine, tag => true));

            // Ícaro walks into the ruins 5 levels early (Docs/MetodologiaMapasERacas.md §3).
            OverworldZone gedi = w.Zone("ruinas_de_gedi");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(gedi, gedi.WalkLevel - 1, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(gedi, gedi.WalkLevel - 1, CharacterId.Icaro, null));
        }

        [Test]
        public void Gates_TheZarafshanGateOnTheWestEdge_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            CollectionAssert.AreEquivalent(new[] { MapCatalog.PomarId }, gates.Select(g => g.TargetMapId).ToArray());
            MapExitSpec west = gates[0];
            Assert.Less(west.Trigger.center.x - w.Bounds.xMin, 3f, "towards the Zarafshan, on the west edge");
            Assert.AreEqual(west.Arrival, w.ArrivalFor(OverworldLayouts.TaaPwaniMapId, MapCatalog.PomarId, out Vector2 facing));
            Assert.Greater(facing.x, 0f, "facing into the Costa");
            Assert.AreEqual("mata_da_restinga", OverworldRules.ZoneAt(w, west.Arrival).Id);
            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by Taa Pwani");

            // By barca (or a new game): Taa Pwani's dock inside its safe zone; each post's dock for its map.
            Vector2 dock = w.ArrivalFor(OverworldLayouts.TaaPwaniMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in CostaMaps)
            {
                OverworldSettlement post = w.SettlementOf(id);
                Assert.AreEqual(post.DockArrival, w.ArrivalFor(id, OverworldLayouts.TaaPwaniMapId, out _), id);
                Assert.IsTrue(post.SafeZone.Contains(post.DockArrival), id);
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
            var rnd = new System.Random(25);
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
        public void TheCoast_CliffsHigh_BeachesAndShallowsLow_TheWaterBlocks_TheShallowsAreMarked()
        {
            OverworldLayout w = World;
            Assert.Greater(MaxHeight(w.Zone("falesias_vermelhas")), 14f, "the red cliffs stand high over the coves");
            Assert.Greater(MaxHeight(w.Zone("falesias_do_norte")), 14f);
            foreach (string low in new[] { "recife_de_taa_pwani", "atol_de_kirta", "jardins_de_alga", "canais_de_liyongo", "mangues_de_liyongo" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 2.5f, $"{low}: a flat just over the water line");
            }

            // The Canal do Recife separates the beach from the reef: deep water (a carved bed under the water line) crossed only at
            // its fords (the bed rises to a wade) and its causeway.
            MapRiver canal = w.World.Rivers.Find(r => r.Points.Count > 0 && r.Points[0] == OverworldLayouts.ReefCanal[0]);
            Assert.IsNotNull(canal);
            Assert.GreaterOrEqual(canal.Crossings.Count(c => !c.IsBridge), 3, "baixios across the canal");
            Assert.GreaterOrEqual(canal.Crossings.Count(c => c.IsBridge), 1, "a coral causeway");
            Vector2 deep = Vector2.Lerp(OverworldLayouts.ReefCanal[3], OverworldLayouts.ReefCanal[4], 0.5f);
            Assert.Less(Relief.HeightAt(deep), OverworldRelief.WaterY - 0.5f, "deep in the canal");
            foreach (MapCrossing ford in canal.Crossings.Where(c => !c.IsBridge))
            {
                float h = Relief.HeightAt(ford.At);
                Assert.Greater(h, -OverworldRelief.FordDepth - 0.05f, $"the baixio at {ford.At} is shallow");
                Assert.Less(h, OverworldRelief.WaterY, $"the baixio at {ford.At} is still under water");
            }

            // Every ford of the coast is marked by striped posts on its banks.
            int fords = 0;
            int marked = 0;
            foreach (MapRiver river in w.World.Rivers.Where(r => r.Width >= OverworldRelief.CarveMinWidth))
            {
                foreach (MapCrossing ford in river.Crossings.Where(c => !c.IsBridge))
                {
                    fords++;
                    OverworldChunkContent content = OverworldChunkBuilder.Content(w, OverworldRules.ChunkOf(w, ford.At));
                    var near = new List<OverworldProp>(content.Props);
                    foreach (Vector2Int dc in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
                    {
                        near.AddRange(OverworldChunkBuilder.Content(w, OverworldRules.ChunkOf(w, ford.At) + dc).Props);
                    }

                    if (near.Count(p => p.Decor == OverworldDecor.FordPost && Vector2.Distance(p.Position, ford.At) < river.Width * 0.5f + ford.Width) >= 2)
                    {
                        marked++;
                    }
                }
            }

            Assert.GreaterOrEqual(fords, 10, "islets linked by shallows");
            Assert.AreEqual(fords, marked, "every baixio has its posts");

            // Bridges land on flat ground at both ends.
            foreach (MapRiver river in w.World.Rivers)
            {
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
        public void Dressing_EveryAreaHasItsOwnLook_StandsOnTheGround_OffTheTrailsAndTheWater()
        {
            OverworldLayout w = World;
            var expected = new Dictionary<string, OverworldDecor[]>
            {
                { "praia_dos_dhows", new[] { OverworldDecor.BeachedDhow, OverworldDecor.NetRack } },
                { "recife_de_taa_pwani", new[] { OverworldDecor.CoralHead, OverworldDecor.FishTrap } },
                { "mangue_raso", new[] { OverworldDecor.MangroveClump } },
                { "canais_de_liyongo", new[] { OverworldDecor.MangroveClump } },
                { "ruinas_de_gedi", new[] { OverworldDecor.CoralArch, OverworldDecor.CoralWall, OverworldDecor.PillarTomb } },
                { "kilwa_afundada", new[] { OverworldDecor.CoralWall } },
                { "savana_dos_auroques", new[] { OverworldDecor.Acacia, OverworldDecor.TermiteMound } },
                { "piramides_altas_de_meroe", new[] { OverworldDecor.Pyramid } },
                { "falesias_vermelhas", new[] { OverworldDecor.RockSpire } },
                { "cravais_do_vento_leste", new[] { OverworldDecor.CloveMat } },
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
            Debug.Log($"[Overworld Costa] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
                $"generation avg {average:0.00} ms, p50 {times[times.Count / 2]:0.00} ms, p95 {times[(int)(times.Count * 0.95f)]:0.00} ms (Editor, Mono)");
            Assert.Less(average, 6.0, "chunk generation stays cheap even in the Editor");
        }

        [Test]
        public void EveryCoastTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Rain, OverworldAreaAmbience.ClimateOf(OverworldTerrain.Mangrove).Weather, "the monsoon over the mangroves");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.WhiteBeach).Weather);
        }

        // ------------------------------------------------------------------

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
