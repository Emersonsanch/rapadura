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
    /// The continuous Região 7 — Teto do Mundo (Docs/Mapas100.md §0.10, maps 36–40), in the mould of
    /// <see cref="OverworldDeltaTests"/>: one 2400 × 2400 m map with Sangkent (its own recipe) inside, 36–40 as zones with
    /// their posts, zones that tile it, open passages, level bands growing with the distance from Sangkent from the catalog's
    /// 70, the soft barrier (Thermal Clothes above the town, Climbing Crampons, wings, Storm Cloak), the two gates towards the
    /// Serra de Kaggen (south edge), the mountain's beasts (no boss), the high relief (peaks and seracs high, the valleys
    /// low, the river carved and crossed at stone bridges that land flat) and the per-area dressing within the chunk budget.
    /// </summary>
    public class OverworldTetoTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.TetoId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] TetoMaps =
        {
            OverworldLayouts.SangkentMapId, OverworldLayouts.CalhasId, OverworldLayouts.GeleiraId, OverworldLayouts.ForteVentosId, OverworldLayouts.WakhanId,
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
        public void Region_IsAsBigAsTheOthers_WithSangkentInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.TetoDoMundo, w.Region);
            Assert.AreEqual(Rapadura.Gameplay.World.Biomes.BiomeType.Mountain, w.World.Biome);
            Assert.AreEqual(CityTownRecipes.SangkentCityId, w.Vila.Id, "Sangkent is the region's town");
            Assert.AreEqual(MapCatalog.Sangkent, w.Vila.Id);
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais da Barca of Sangkent");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: the mountains are all round it");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, 28f, "Sangkent's ground: 28 m and more each way at y = 0");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, CityTownRecipes.SangkentSolidRadius + 5f, "room for the stair city");
            Assert.AreEqual(OverworldLayouts.SangkentMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "Sangkent stands on its terraces (36)");

            // Every map of the Teto wakes the hero in Sangkent, the return city of 36–40, standing in the region.
            foreach (string map in TetoMaps)
            {
                string city = MapCatalog.ReturnCityOf(map);
                Assert.AreEqual(MapCatalog.Sangkent, city, map);
                Assert.IsTrue(w.Settlements.Exists(s => s.IsReturnCity && s.Id == city), $"{map}: its return city {city} stands in the Teto");
            }

            Assert.AreEqual(1, w.Settlements.Count(s => s.IsReturnCity), "the posts of 37–40 are no return cities");
        }

        [Test]
        public void Region_HoldsMaps36To40_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 15, "many kinds of high mountain");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsPamir(z.Terrain)), "the Teto's own terrains");
            foreach (string id in TetoMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the Teto");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Teto");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 300, "the Teto is full of clearings");
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
            Assert.AreEqual("terracos_de_sangkent", OverworldRules.ZoneAt(w, OverworldLayouts.SangkentOffset).Id);
            Assert.AreEqual("estacao_da_calha_alta", OverworldRules.ZoneAt(w, OverworldLayouts.CalhaAltaOffset).Id);
            Assert.AreEqual("geleira_do_teto_do_mundo", OverworldRules.ZoneAt(w, OverworldLayouts.CabanaIaqueOffset).Id);
            Assert.AreEqual("forte_dos_ventos", OverworldRules.ZoneAt(w, OverworldLayouts.ForteVentosOffset).Id);
            Assert.AreEqual("passo_de_wakhan", OverworldRules.ZoneAt(w, OverworldLayouts.CaravancaraPedraOffset).Id);
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
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("terracos_de_sangkent"), w.Zone("vale_dos_damasqueiros")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("terracos_de_sangkent"), w.Zone("calhas_de_pedra")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("geleira_do_teto_do_mundo"), w.Zone("campo_de_seracs")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("desfiladeiro_dos_ventos"), w.Zone("forte_dos_ventos")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("passo_de_wakhan"), w.Zone("ninho_de_aydahar")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("terracos_de_sangkent"), w.Zone("passo_de_wakhan")));

            var seen = new HashSet<OverworldZone> { w.Zone("terracos_de_sangkent") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to Sangkent");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromSangkent_FromTheCatalogsLevel70()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.SangkentMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 70 at Sangkent (36 is 70–76)");
            Assert.AreEqual(70, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 86, "the ice peaks and the crests of the ill wind are the end of the Teto");
            Assert.AreEqual(98, w.Zones.Max(z => z.MaxLevel), "up to 40's 98");
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.WakhanId))
            {
                Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1200f, $"{zone.Id}: the Wakhan Pass is far from Sangkent");
            }

            foreach (string far in new[] { OverworldLayouts.GeleiraId, OverworldLayouts.ForteVentosId })
            {
                foreach (OverworldZone zone in w.ZonesOfMap(far))
                {
                    Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 800f, $"{zone.Id}: the glacier and the fort are far from Sangkent");
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
                { OverworldLayouts.GeleiraId, MapRequirementTags.ClimbingCrampons },
                { OverworldLayouts.ForteVentosId, MapRequirementTags.WingsRequired },
                { OverworldLayouts.WakhanId, MapRequirementTags.StormProtection },
            };
            foreach (KeyValuePair<string, string> pair in expected)
            {
                Assert.AreEqual(pair.Value, MapCatalog.Get(pair.Key).SpecialRequirementTag, pair.Key);
                foreach (OverworldZone zone in w.ZonesOfMap(pair.Key))
                {
                    Assert.AreEqual(pair.Value, zone.RequirementTag, zone.Id);
                    Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 100, CharacterId.Lavine, null), $"{zone.Id}: asks for {pair.Value}");
                    Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 100, CharacterId.Lavine, tag => tag == pair.Value));
                }
            }

            // 36 asks for the Thermal Clothes (worn) on the snow above the town, like the reef of Taa Pwani: the town's own
            // terraces and the ways in from the Serra ask for nothing but the level, so the hero can walk in and buy them there.
            Assert.AreEqual(MapRequirementTags.ThermalClothes, MapCatalog.Get(OverworldLayouts.SangkentMapId).SpecialRequirementTag);
            foreach (string cold in new[] { "terracos_altos", "campos_de_neve_de_sangkent" })
            {
                OverworldZone zone = w.Zone(cold);
                Assert.AreEqual(OverworldLayouts.SangkentMapId, zone.MapId, cold);
                Assert.AreEqual(MapRequirementTags.ThermalClothes, zone.RequirementTag, cold);
                Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 100, CharacterId.Lavine, null), cold);
                Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 100, CharacterId.Lavine, tag => tag == MapRequirementTags.ThermalClothes), cold);
            }

            // 37 asks for nothing.
            Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(OverworldLayouts.CalhasId).SpecialRequirementTag);
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.CalhasId))
            {
                Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 100, CharacterId.Lavine, null), zone.Id);
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("terracos_de_sangkent"), 70, CharacterId.Lavine, null), "the town's terraces");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("vale_dos_damasqueiros"), 70, CharacterId.Lavine, null), "the way in from the Serra (west pass)");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("encosta_dos_zimbros"), 75, CharacterId.Lavine, null), "the way in from the Serra (east pass)");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("vale_dos_damasqueiros"), 40, CharacterId.Lavine, null), "the Teto is for level 70");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("ninho_de_aydahar"), 80, CharacterId.Lavine, tag => true));
        }

        [Test]
        public void Gates_TheTwoKaggenGatesOnTheSouthEdge_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            CollectionAssert.AreEquivalent(new[] { OverworldLayouts.PedraSecaId, OverworldLayouts.AbrigosId }, gates.Select(g => g.TargetMapId).ToArray());
            foreach (MapExitSpec gate in gates)
            {
                Assert.AreEqual(MapRegion.SerraDeKaggen, MapCatalog.Get(gate.TargetMapId).Region, $"{gate.Id}: down to the Serra");
                Assert.Less(gate.Trigger.center.y - w.Bounds.yMin, 3f, $"{gate.Id}: on the south edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.SangkentMapId, gate.TargetMapId, out Vector2 facing), gate.Id);
                Assert.Greater(facing.y, 0f, $"{gate.Id}: facing into the Teto");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
            }

            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by Sangkent");

            // By barca (or a new game): Sangkent's dock inside its safe zone; each post's dock for its map.
            Vector2 dock = w.ArrivalFor(OverworldLayouts.SangkentMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in TetoMaps)
            {
                OverworldSettlement post = w.SettlementOf(id);
                Assert.AreEqual(post.DockArrival, w.ArrivalFor(id, OverworldLayouts.SangkentMapId, out _), id);
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
            var rnd = new System.Random(31);
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
        public void TheTeto_PeaksAndIceHigh_ValleysLow_TheRiverCarved_AndItsStoneBridgesFlat()
        {
            OverworldLayout w = World;
            Assert.Greater(MaxHeight(w.Zone("picos_gelados")), 30f, "the ice peaks are the roof of the world");
            foreach (string high in new[] { "campo_de_seracs", "ninho_de_aydahar", "cristas_do_vento_ruim", "torres_de_cata_vento" })
            {
                Assert.Greater(MaxHeight(w.Zone(high)), 20f, $"{high}: high ground");
            }

            foreach (string low in new[] { "vale_dos_damasqueiros", "choupal_dos_canais", "rio_de_degelo", "lago_turquesa" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 12f, $"{low}: a valley floor");
            }

            Assert.Greater(MaxHeight(w.Zone("picos_gelados")), MaxHeight(OverworldLayouts.Get(OverworldLayouts.KaggenId), "cume_dos_mazans") - 1f,
                "the Teto rises at least as high as the Serra's summit");

            // The Rio do Wakhan: carved (a bed under the water line), crossed only at its stone bridges, which land flat.
            MapRiver river = w.World.Rivers.Find(r => r.Points.Count > 0 && r.Points[0] == OverworldLayouts.WakhanRiver[0]);
            Assert.IsNotNull(river);
            Assert.GreaterOrEqual(river.Width, OverworldRelief.CarveMinWidth, "a carved river");
            Assert.GreaterOrEqual(river.Crossings.Count(c => c.IsBridge), 4, "stone bridges on the roads");
            Assert.IsFalse(river.Crossings.Exists(c => !c.IsBridge), "no ford: the meltwater is crossed on bridges");
            Vector2 deep = Vector2.Lerp(OverworldLayouts.WakhanRiver[3], OverworldLayouts.WakhanRiver[4], 0.4f);
            Assert.Less(Relief.HeightAt(deep), OverworldRelief.WaterY - 0.5f, "deep in the river");
            int bridges = 0;
            foreach (MapRiver r in w.World.Rivers)
            {
                foreach (MapCrossing crossing in r.Crossings.Where(c => c.IsBridge))
                {
                    bridges++;
                    Vector2 across = MapAreaView.RiverNormal(r, crossing.At);
                    float half = OverworldChunkBuilder.BridgeSpan(r) * 0.5f;
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At + across * half), 0.02f, $"bridge end at {crossing.At}");
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At - across * half), 0.02f, $"bridge end at {crossing.At}");
                }
            }

            Assert.GreaterOrEqual(bridges, 4);
        }

        [Test]
        public void EveryTetoTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsPamir(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Snow, OverworldAreaAmbience.ClimateOf(OverworldTerrain.Glacier).Weather, "snow on the glacier");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Snow, OverworldAreaAmbience.ClimateOf(OverworldTerrain.WakhanPass).Weather, "snow on the pass");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.TurquoiseLake).Weather, "mist on the lake");
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.IcePeaks).Wild);
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
                { "vale_dos_damasqueiros", new[] { OverworldDecor.ApricotBlossom } },
                { "terracos_de_sangkent", new[] { OverworldDecor.ApricotBlossom, OverworldDecor.DryStoneWall } },
                { "encosta_dos_zimbros", new[] { OverworldDecor.Juniper } },
                { "pastagens_dos_iaques", new[] { OverworldDecor.Kraal, OverworldDecor.StoneHut } },
                { "estepe_alta", new[] { OverworldDecor.MarmotMound } },
                { "calhas_de_pedra", new[] { OverworldDecor.StoneChute, OverworldDecor.ChuteStation } },
                { "geleira_do_teto_do_mundo", new[] { OverworldDecor.Crevasse, OverworldDecor.Serac } },
                { "campo_de_seracs", new[] { OverworldDecor.Serac, OverworldDecor.IceShards } },
                { "forte_dos_ventos", new[] { OverworldDecor.WatchTower, OverworldDecor.WindWheel } },
                { "torres_de_cata_vento", new[] { OverworldDecor.WindWheel } },
                { "passo_de_wakhan", new[] { OverworldDecor.RibbonLine } },
                { "ninho_de_aydahar", new[] { OverworldDecor.DragonScale } },
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
            Debug.Log($"[Overworld Teto] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
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

        private static float MaxHeight(OverworldLayout layout, string zoneId)
        {
            OverworldZone zone = layout.Zone(zoneId);
            OverworldRelief relief = OverworldRelief.For(layout);
            float max = float.MinValue;
            for (float x = zone.Bounds.xMin + 2f; x < zone.Bounds.xMax - 2f; x += 6f)
            {
                for (float z = zone.Bounds.yMin + 2f; z < zone.Bounds.yMax - 2f; z += 6f)
                {
                    max = Mathf.Max(max, relief.HeightAt(new Vector2(x, z)));
                }
            }

            return max;
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
