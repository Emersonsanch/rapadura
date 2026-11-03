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
    /// The continuous Região 6 — Delta de Jhilpur (Docs/Mapas100.md §0.10, maps 31–35), in the mould of
    /// <see cref="OverworldCostaTests"/>: one 2400 × 2400 m map with Jhilpur (its own recipe) inside, 31–35 as zones (Tarchon's
    /// ground reserved flat with a post), zones that tile it, open passages, level bands growing with the distance from Jhilpur
    /// from the catalog's 60, the soft barrier (Balance Pole, Reed Boat, Antivenom Amulet, Purifying Fruit), the gates towards
    /// the Costa (north edge) and the Areias (west edge), the delta's beasts (no boss), the delta's relief (gorges high, swamp
    /// low, the carved channels blocking, their fords marked and their root bridges flat) and the per-area dressing within the
    /// chunk budget.
    /// </summary>
    public class OverworldDeltaTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.DeltaId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] DeltaMaps =
        {
            OverworldLayouts.JhilpurMapId, OverworldLayouts.RaizVivaId, OverworldLayouts.TarchonMapId, OverworldLayouts.NinkiNankaId, OverworldLayouts.CoracaoLodoId,
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
        public void Region_IsAsBigAsTheOthers_WithJhilpurAndTarchonInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.DeltaDeJhilpur, w.Region);
            Assert.AreEqual(CityTownRecipes.JhilpurCityId, w.Vila.Id, "Jhilpur is the region's town");
            Assert.AreEqual(MapCatalog.Jhilpur, w.Vila.Id);
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais da Barca of Jhilpur");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: the delta is all round it");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, CityTownRecipes.JhilpurSolidRadius + 5f, "room for the city in the roots");
            Assert.AreEqual(OverworldLayouts.JhilpurMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "Jhilpur stands on its lake (31)");

            // Every map of the delta wakes the hero in a return city standing in the region (Jhilpur; Tarchon for 33–34).
            foreach (string map in DeltaMaps)
            {
                string city = MapCatalog.ReturnCityOf(map);
                Assert.IsTrue(w.Settlements.Exists(s => s.IsReturnCity && s.Id == city), $"{map}: its return city {city} stands in the delta");
            }

            // The Palafitas de Tarchon (33): the trolls' race city, its own recipe on wide flat ground.
            OverworldSettlement tarchon = w.SettlementOf(OverworldLayouts.TarchonMapId);
            Assert.IsNotNull(tarchon);
            Assert.AreEqual(MapCatalog.PalafitasTarchon, tarchon.Id);
            Assert.IsTrue(tarchon.IsReturnCity, "the return city of 33–34");
            Assert.IsFalse(tarchon.Recipe.Backdrop);
            Assert.GreaterOrEqual(tarchon.GroundHalfExtent, OverworldLayouts.RaceCityGround, "room for the trolls' stilts");
            Assert.AreEqual(OverworldLayouts.TarchonMapId, OverworldRules.ZoneAt(w, tarchon.Offset).MapId);
        }

        [Test]
        public void Region_HoldsMaps31To35_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 12, "many kinds of delta");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsDelta(z.Terrain)), "the delta's own terrains");
            foreach (string id in DeltaMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the Delta");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Delta");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 300, "the delta is full of clearings");
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
            Assert.AreEqual("lago_de_jhilpur", OverworldRules.ZoneAt(w, OverworldLayouts.JhilpurOffset).Id);
            Assert.AreEqual("pontes_de_raiz_viva", OverworldRules.ZoneAt(w, OverworldLayouts.PlataformaGarcasOffset).Id);
            Assert.AreEqual("palafitas_de_tarchon", OverworldRules.ZoneAt(w, OverworldLayouts.TarchonOffset).Id);
            Assert.AreEqual("ilhota_das_lanternas", OverworldRules.ZoneAt(w, OverworldLayouts.IlhotaLanternasOffset).Id);
            Assert.AreEqual("coracao_do_lodo", OverworldRules.ZoneAt(w, OverworldLayouts.RaizMaeOffset).Id);
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
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("lago_de_jhilpur"), w.Zone("figueiral_das_raizes")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("pontes_de_raiz_viva"), w.Zone("gargantas_esquecidas")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("palafitas_de_tarchon"), w.Zone("ilhota_das_lanternas")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("coracao_do_lodo"), w.Zone("olho_do_lodo")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("lago_de_jhilpur"), w.Zone("olho_do_lodo")));

            var seen = new HashSet<OverworldZone> { w.Zone("lago_de_jhilpur") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to Jhilpur");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromJhilpur_FromTheCatalogsLevel60()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.JhilpurMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 60 at Jhilpur (31 is 60–66)");
            Assert.AreEqual(60, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 80, "the mire and the dragon's den are the end of the delta");
            Assert.AreEqual(88, w.Zones.Max(z => z.MaxLevel), "up to 35's 88");
            foreach (string far in new[] { OverworldLayouts.NinkiNankaId, OverworldLayouts.CoracaoLodoId })
            {
                foreach (OverworldZone zone in w.ZonesOfMap(far))
                {
                    Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1200f, $"{zone.Id}: the far maps are far from Jhilpur");
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
                { OverworldLayouts.RaizVivaId, MapRequirementTags.RootBridgeBalance },
                { OverworldLayouts.TarchonMapId, MapRequirementTags.SaddleRequired },
                { OverworldLayouts.NinkiNankaId, MapRequirementTags.AntiVenomAmulet },
                { OverworldLayouts.CoracaoLodoId, MapRequirementTags.PurifyingFruit },
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

            // Jhilpur's lake and the in-between areas ask for nothing but the level.
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("lago_de_jhilpur"), 60, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("diques_do_norte"), 60, CharacterId.Lavine, null), "the way in from the Costa");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("cascatas_da_chuva"), 64, CharacterId.Lavine, null), "the way in from the Areias");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("cascatas_da_chuva"), 30, CharacterId.Lavine, null), "the delta is for level 60");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("olho_do_lodo"), 70, CharacterId.Lavine, tag => true));
        }

        [Test]
        public void Gates_TheCostaGatesOnTheNorthEdge_TheAreiasGateOnTheWest_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            CollectionAssert.AreEquivalent(new[] { OverworldLayouts.TaaPwaniMapId, OverworldLayouts.LiyongoId, MapLayouts.ArtificesId }, gates.Select(g => g.TargetMapId).ToArray());
            foreach (MapExitSpec gate in gates)
            {
                bool north = gate.TargetMapId != MapLayouts.ArtificesId;
                Assert.Less(north ? w.Bounds.yMax - gate.Trigger.center.y : gate.Trigger.center.x - w.Bounds.xMin, 3f, $"{gate.Id}: on its edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.JhilpurMapId, gate.TargetMapId, out Vector2 facing), gate.Id);
                Assert.Greater(north ? -facing.y : facing.x, 0f, $"{gate.Id}: facing into the Delta");
            }

            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by Jhilpur");

            // By barca (or a new game): Jhilpur's dock inside its safe zone; each post's dock for its map.
            Vector2 dock = w.ArrivalFor(OverworldLayouts.JhilpurMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in DeltaMaps)
            {
                OverworldSettlement post = w.SettlementOf(id);
                Assert.AreEqual(post.DockArrival, w.ArrivalFor(id, OverworldLayouts.JhilpurMapId, out _), id);
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
        public void TheDelta_GorgesHigh_SwampLow_TheChannelsBlock_TheFordsAreMarked()
        {
            OverworldLayout w = World;
            Assert.Greater(MaxHeight(w.Zone("pontes_de_raiz_viva")), 10f, "the gorges of the root bridges are deep");
            Assert.Greater(MaxHeight(w.Zone("gargantas_esquecidas")), 10f);
            foreach (string low in new[] { "palafitas_de_tarchon", "ilhas_de_junco", "lagoas_de_lotus", "coracao_do_lodo", "charco_do_ninki_nanka", "bancos_de_jhilpur" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 3f, $"{low}: a swamp just over the water");
            }

            // The Canal de Jhilpur: deep water (a carved bed under the water line) crossed only at its fords and its root bridge.
            MapRiver canal = w.World.Rivers.Find(r => r.Points.Count > 0 && r.Points[0] == OverworldLayouts.JhilpurCanal[0]);
            Assert.IsNotNull(canal);
            Assert.GreaterOrEqual(canal.Crossings.Count(c => !c.IsBridge), 2, "fords across the canal");
            Assert.GreaterOrEqual(canal.Crossings.Count(c => c.IsBridge), 1, "a root bridge");
            Vector2 deep = Vector2.Lerp(OverworldLayouts.JhilpurCanal[5], OverworldLayouts.JhilpurCanal[6], 0.4f);
            Assert.Less(Relief.HeightAt(deep), OverworldRelief.WaterY - 0.5f, "deep in the canal");
            foreach (MapCrossing ford in canal.Crossings.Where(c => !c.IsBridge))
            {
                float h = Relief.HeightAt(ford.At);
                Assert.Greater(h, -OverworldRelief.FordDepth - 0.05f, $"the ford at {ford.At} is shallow");
                Assert.Less(h, OverworldRelief.WaterY, $"the ford at {ford.At} is still under water");
            }

            // Every ford of the delta is marked by striped posts on its banks.
            int fords = 0;
            int marked = 0;
            foreach (MapRiver river in w.World.Rivers.Where(r => r.Width >= OverworldRelief.CarveMinWidth))
            {
                foreach (MapCrossing ford in river.Crossings.Where(c => !c.IsBridge))
                {
                    fords++;
                    var near = new List<OverworldProp>(OverworldChunkBuilder.Content(w, OverworldRules.ChunkOf(w, ford.At)).Props);
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

            Assert.GreaterOrEqual(fords, 6, "channels crossed at fords");
            Assert.AreEqual(fords, marked, "every ford has its posts");

            // The root bridges land on flat ground at both ends.
            int bridges = 0;
            foreach (MapRiver river in w.World.Rivers)
            {
                foreach (MapCrossing crossing in river.Crossings.Where(c => c.IsBridge))
                {
                    bridges++;
                    Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                    float half = OverworldChunkBuilder.BridgeSpan(river) * 0.5f;
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At + across * half), 0.02f, $"bridge end at {crossing.At}");
                    Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, crossing.At - across * half), 0.02f, $"bridge end at {crossing.At}");
                }
            }

            Assert.GreaterOrEqual(bridges, 4, "the gorges' root bridges and the canal's");
        }

        [Test]
        public void EveryDeltaTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Rain, OverworldAreaAmbience.ClimateOf(OverworldTerrain.RootGorge).Weather, "the monsoon over the gorges");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.BlackMire).Weather, "the mist over the mire");
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
                { "diques_do_norte", new[] { OverworldDecor.PaddyBund, OverworldDecor.RiceTuft } },
                { "varzea_dos_bufalos", new[] { OverworldDecor.PaddyBund, OverworldDecor.RiceTuft } },
                { "figueiral_das_raizes", new[] { OverworldDecor.Banyan } },
                { "pontes_de_raiz_viva", new[] { OverworldDecor.RootArch } },
                { "palafitas_de_tarchon", new[] { OverworldDecor.StiltHut } },
                { "ilhota_das_lanternas", new[] { OverworldDecor.LanternPole } },
                { "coracao_do_lodo", new[] { OverworldDecor.RottenRoot, OverworldDecor.MireBubbles } },
                { "lagoas_de_lotus", new[] { OverworldDecor.LotusPads } },
                { "mata_morta", new[] { OverworldDecor.DeadTree } },
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
            Debug.Log($"[Overworld Delta] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
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
