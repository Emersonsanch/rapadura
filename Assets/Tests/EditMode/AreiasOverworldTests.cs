using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
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
    /// The continuous Região 2 — Areias Flamejantes (Docs/Mapas100.md §0.10 and §5): one 2400 × 2400 m map with the Oásis de
    /// Qocho inside, 07 and 08 embedded, 09–12 and the in-between areas generated; zones that tile it, open passages,
    /// level bands growing with the distance from Qocho, the fresh-water and poison gates as a soft barrier, the gates to
    /// the Pomar (the Zarafshan's gate to 07 lands here) and to the Rondavéis, the desert monsters (the [distância] ones
    /// included) with night tables; the desert relief (dunes, mesas, canyons, salt terraces) flat under the towns, gates
    /// and across the trails; every area dressed its own way; the per-chunk budget.
    /// </summary>
    public class AreiasOverworldTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.AreiasId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] RegionMaps =
        {
            MapLayouts.CaravanasId, MapLayouts.ArtificesId, OverworldLayouts.KarezId, OverworldLayouts.DunasId, OverworldLayouts.TamborRubroMapId,
            OverworldLayouts.NamraelId,
        };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [Test]
        public void Region_IsValid_AndHoldsTheMaps07To12()
        {
            OverworldLayout w = World;
            List<string> problems = OverworldRules.Validate(w);
            Assert.IsEmpty(problems, string.Join("\n", problems));
            Assert.AreEqual(MapRegion.AreiasFlamejantes, w.Region);
            Assert.AreEqual(BiomeType.Desert, w.World.Biome, "desert weather and ambience");
            Assert.GreaterOrEqual(w.Bounds.width, 2400f);
            Assert.GreaterOrEqual(w.Bounds.height, 2400f);
            foreach (string id in RegionMaps)
            {
                Assert.AreEqual(MapRegion.AreiasFlamejantes, MapCatalog.Get(id).Region, id);
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the region");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id} loads the Areias overworld");
                Assert.IsNotNull(w.SettlementOf(id), $"{id}: its post stands in the region");
            }

            Assert.AreSame(OverworldLayouts.Get(OverworldLayouts.ZarafshanId), OverworldLayouts.ForMap(MapCatalog.PomarId), "the Zarafshan is its own overworld");
        }

        [Test]
        public void Qocho_IsTheReturnCity_AndTheTamborRubroTakesIn11And12()
        {
            OverworldLayout w = World;
            Assert.AreEqual(CityCatalog.AreiaBranca, w.Vila.Id, "Qocho is the region's town");
            Assert.AreEqual(Vector2.zero, w.Vila.Offset);
            Assert.IsTrue(w.Vila.IsReturnCity && w.Vila.HasDock);
            Assert.AreEqual("oasis_de_qocho", OverworldRules.ZoneAt(w, w.Vila.Offset).Id);
            Assert.AreSame(w.Vila, w.SettlementOf(MapLayouts.CaravanasId), "the barca to 07 stops at Qocho");
            foreach (string id in RegionMaps)
            {
                string city = MapCatalog.ReturnCityOf(id);
                Assert.IsTrue(w.Settlements.Exists(s => s.IsReturnCity && s.Id == city), $"{id}: a defeat wakes the hero in {city}, inside the region");
            }

            OverworldSettlement tambor = w.SettlementOf(OverworldLayouts.TamborRubroMapId);
            Assert.AreEqual(MapCatalog.TamborRubro, tambor.Id);
            Assert.IsTrue(tambor.IsReturnCity);
            Assert.AreEqual("ermos_do_tambor_rubro", OverworldRules.ZoneAt(w, tambor.Offset).Id);
        }

        [Test]
        public void Areas_AreMany_EachWithItsOwnMonsters_TheRangedOnesIncluded()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 18);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 14, "dunes, ergs, oases, canyons, salt flats, ruins, karez…");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsDesert(z.Terrain)), "every area has a desert terrain");
            var species = new HashSet<string>();
            foreach (OverworldZone zone in w.Zones)
            {
                List<MapSpawnZone> spawns = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id] == zone).ToList();
                Assert.GreaterOrEqual(spawns.Count, 2, $"{zone.Id}: several monster groups");
                Assert.GreaterOrEqual(spawns.SelectMany(s => s.AllEntries()).Select(e => e.EnemyId).Distinct().Count(), 2, $"{zone.Id}: more than one kind of beast");
                foreach (MapSpawnZone spawn in spawns)
                {
                    Assert.IsTrue(spawn.Night.Any(), $"{spawn.Id}: night table");
                    foreach (var entry in spawn.AllEntries())
                    {
                        species.Add(entry.EnemyId);
                    }
                }
            }

            Assert.GreaterOrEqual(w.World.Zones.Count, 300, "the sands are full of clearings");
            CollectionAssert.IsSubsetOf(new[] { PeriodSpawnTable.DesertScorpion, "enemy_desert_ember_vulture", "enemy_desert_broken_lantern", PeriodSpawnTable.MountainGolem },
                species.ToArray(), "the desert's own monsters, the [distância] ones included");
            Assert.IsTrue(species.Contains(NightVariantRules.FireflySwarmEnemyId), "the Faíscas Presas at night");
        }

        [Test]
        public void Zones_TileTheRegion_AndAreConnected()
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
            Assert.AreEqual("estrada_das_caravanas", OverworldRules.ZoneAt(w, OverworldLayouts.CaravancaraOffset).Id);
            Assert.AreEqual("ruinas_dos_artifices", OverworldRules.ZoneAt(w, OverworldLayouts.ArtificesOffset).Id);
            Assert.AreEqual("trono_de_namrael", OverworldRules.ZoneAt(w, OverworldLayouts.NamraelThrone).Id);

            var seen = new HashSet<OverworldZone> { w.Zone("oasis_de_qocho") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area can be walked to from Qocho");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromQocho()
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
            }

            List<OverworldZone> byDistance = w.Zones.OrderBy(z => OverworldRules.DistanceFromVila(w, z)).ToList();
            Assert.AreEqual(5, byDistance[0].MinLevel, "level 5 at Qocho (07)");
            Assert.AreEqual(30, w.Zone("trono_de_namrael").MinLevel, "Namrael's throne (30–38) at the far end");
            Assert.Greater(OverworldRules.DistanceFromVila(w, w.Zone("trono_de_namrael")), 1200f);
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
        public void SoftBarrier_FreshWaterIn10_PoisonProtectionIn12()
        {
            OverworldLayout w = World;
            OverworldZone dunas = w.Zone("dunas_que_cantam");
            OverworldZone namrael = w.Zone("trono_de_namrael");
            Assert.AreEqual(MapRequirementTags.FreshWater, dunas.RequirementTag);
            Assert.AreEqual(MapRequirementTags.PoisonProtection, namrael.RequirementTag);
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(dunas, 10, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(dunas, 30, CharacterId.Lavine, null), "the Dunas ask for fresh water");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(dunas, 30, CharacterId.Lavine, tag => tag == MapRequirementTags.FreshWater));
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(namrael, 40, CharacterId.Lavine, tag => tag == MapRequirementTags.FreshWater));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(namrael, 40, CharacterId.Lavine, tag => tag == MapRequirementTags.PoisonProtection));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("estrada_das_caravanas"), 5, null, null), "the caravan road from level 5");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("galerias_do_karez"), 12, CharacterId.Lavine, null));
        }

        [Test]
        public void Gates_ThePomarOnTheNorthEdge_TheRondaveisOnTheEast_AndTheZarafshanGateLandsHere()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            CollectionAssert.AreEquivalent(new[] { MapCatalog.PomarId, MapLayouts.RondaveisId }, gates.Select(g => g.TargetMapId).ToArray());
            MapExitSpec pomar = gates.First(g => g.TargetMapId == MapCatalog.PomarId);
            Assert.AreEqual(w.Bounds.yMax, pomar.Trigger.yMax, 0.01f, "the way to the Zarafshan leaves by the north edge");
            MapExitSpec rondaveis = gates.First(g => g.TargetMapId == MapLayouts.RondaveisId);
            Assert.AreEqual(w.Bounds.xMax, rondaveis.Trigger.xMax, 0.01f, "the way to the Serra leaves by the east edge");
            foreach (MapExitSpec gate in gates)
            {
                Assert.IsTrue(MapCatalog.Get(gate.TargetMapId).IsBuilt && MapLayouts.Has(gate.TargetMapId), gate.Id);
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(MapLayouts.CaravanasId, gate.TargetMapId, out _), $"coming back from {gate.TargetMapId}");
            }

            // The Zarafshan's south gate (to 07) walks the hero onto the caravan road at the north gate.
            OverworldLayout zarafshan = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            MapExitSpec south = zarafshan.World.Exits.Find(e => e.Kind == MapExitKind.Map && e.TargetMapId == MapLayouts.CaravanasId);
            Assert.IsNotNull(south);
            Assert.AreEqual(zarafshan.Bounds.yMin, south.Trigger.yMin, 0.01f, "the Zarafshan's gate is on its south edge");
            string from = OverworldRules.ZoneAt(zarafshan, south.Arrival).MapIdOrOwner;
            Vector2 landing = w.ArrivalFor(MapLayouts.CaravanasId, from, out Vector2 facing);
            Assert.AreEqual(pomar.Arrival, landing);
            Assert.AreEqual("estrada_das_caravanas", OverworldRules.ZoneAt(w, landing).Id);
            Assert.Less(facing.y, 0f, "facing into the sands");

            // By barca (or from the island map): Qocho's dock, inside its safe zone.
            Assert.IsTrue(w.Vila.SafeZone.Contains(w.ArrivalFor(MapLayouts.CaravanasId, null, out _)));
            Assert.AreEqual(w.SettlementOf(OverworldLayouts.DunasId).DockArrival, w.ArrivalFor(OverworldLayouts.DunasId, null, out _));
        }

        [Test]
        public void Spawns_StayOutOfSafeZones_WithExistingNonBossMonsters()
        {
            OverworldLayout w = World;
            var checkedIds = new HashSet<string>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                OverworldZone zone = w.ZoneOfSpawn[spawn.Id];
                Assert.IsTrue(zone.Bounds.Contains(spawn.Center), spawn.Id);
                Assert.GreaterOrEqual(OverworldRules.DistanceToSafety(w, spawn.Center) - spawn.Radius, MapLayoutRules.SpawnSafeMargin, spawn.Id);
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
        public void Relief_IsContinuous_FlatUnderTownsAndGates_AndLevelAcrossTrails()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            var rnd = new System.Random(5);
            for (int i = 0; i < 3000; i++)
            {
                var p = new Vector2(w.Bounds.xMin - 40f + (float)rnd.NextDouble() * (w.Bounds.width + 80f), w.Bounds.yMin - 40f + (float)rnd.NextDouble() * (w.Bounds.height + 80f));
                float h = relief.HeightAt(p);
                Assert.AreEqual(h, relief.HeightAt(p), 0f);
                var q = p + new Vector2((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f).normalized * 0.05f;
                Assert.Less(Mathf.Abs(relief.HeightAt(q) - h), 0.3f, $"a jump in the relief at {p}");
            }

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
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Arrival), 1e-4f, $"{exit.Id}: the hero arrives on flat ground");
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
            Assert.Greater(cross.Count, 1000);
            Assert.Less(cross[cross.Count / 2], 0.005f, "a trail is level across its width (median)");
            Assert.Less(cross[(int)(cross.Count * 0.98f)], 0.08f, "a trail is level across its width (98 %)");
            Assert.Less(cross.Last(), 0.3f, "no trail leans like a dune's flank");
        }

        [Test]
        public void Areas_RiseTheDesertWay()
        {
            OverworldLayout w = World;
            Assert.Greater(MaxHeight(w.Zone("erg_dourado"), 60f), 6f, "the erg has tall dunes");
            Assert.Greater(MaxHeight(w.Zone("dunas_que_cantam"), 60f), 9f, "the singing dunes are the tallest");
            Assert.Greater(MaxHeight(w.Zone("mesas_do_fogo"), 60f), 14f, "the mesas stand high");
            Assert.Greater(MaxHeight(w.Zone("ermos_do_tambor_rubro"), 60f), 12f, "the badlands are cut deep");
            Assert.Less(MaxHeight(w.Zone("salinas_de_lop"), 90f), 1.5f, "the salt flats are flat");
            Assert.Less(MaxHeight(w.Zone("oasis_de_qocho"), 90f), 3f, "the oasis is low");

            // Dunes: a long windward back, a short steep slip face.
            int rising = 0;
            int total = 0;
            for (float x = 0f; x < 3f; x += 0.01f, total++)
            {
                if (OverworldRelief.DuneProfile(x + 0.005f, 0.3f) > OverworldRelief.DuneProfile(x - 0.005f, 0.3f))
                {
                    rising++;
                }
            }

            Assert.Greater(rising / (float)total, 0.6f, "dunes rise slowly against the wind and drop steeply");

            // Mesas: flat tops and floors, few slopes in between.
            OverworldZone mesas = w.Zone("mesas_do_fogo");
            int level = 0;
            int samples = 0;
            for (float x = mesas.Bounds.xMin + 60f; x < mesas.Bounds.xMax - 60f; x += 9f)
            {
                for (float z = mesas.Bounds.yMin + 60f; z < mesas.Bounds.yMax - 60f; z += 9f)
                {
                    var p = new Vector2(x, z);
                    float gx = Relief.HeightAt(p + Vector2.right) - Relief.HeightAt(p - Vector2.right);
                    float gz = Relief.HeightAt(p + Vector2.up) - Relief.HeightAt(p - Vector2.up);
                    samples++;
                    if (Mathf.Sqrt(gx * gx + gz * gz) * 0.5f < 0.1f)
                    {
                        level++;
                    }
                }
            }

            Assert.Greater(level / (float)samples, 0.5f, "the mesas are flat on top and below");

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

                Assert.Less(worst, 3f, $"{zone.Id}: its monsters fight on level ground");
                flat += worst < 0.35f ? 1 : 0;
            }

            Assert.Greater(flat / (float)w.World.Zones.Count, 0.9f, "almost every clearing is a flat plateau");
        }

        [Test]
        public void Dressing_EveryAreaHasItsOwnLook_AndStandsOnTheGround()
        {
            OverworldLayout w = World;
            var expected = new Dictionary<string, OverworldDecor[]>
            {
                { "estrada_das_caravanas", new[] { OverworldDecor.Marker } },
                { "ruinas_dos_artifices", new[] { OverworldDecor.LotusColumn, OverworldDecor.Ushabtis } },
                { "piramides_de_meroe", new[] { OverworldDecor.Pyramid } },
                { "planicie_dos_karez", new[] { OverworldDecor.KarezShaft } },
                { "galerias_do_karez", new[] { OverworldDecor.KarezShaft } },
                { "salinas_de_lop", new[] { OverworldDecor.SaltCrust } },
                { "canions_vermelhos", new[] { OverworldDecor.Hoodoo } },
                { "mesas_do_fogo", new[] { OverworldDecor.Hoodoo } },
                { "vale_das_ossadas", new[] { OverworldDecor.Bones } },
                { "cemiterio_de_caravanas", new[] { OverworldDecor.CaravanWreck } },
                { "ermos_do_tambor_rubro", new[] { OverworldDecor.ClanTent } },
                { "trono_de_namrael", new[] { OverworldDecor.SaltCrystal, OverworldDecor.TaipaTower, OverworldDecor.SaltCrust } },
                { "oasis_das_tamargueiras", new[] { OverworldDecor.Reeds } },
            };

            var signatures = new Dictionary<string, HashSet<string>>();
            var palms = 0;
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
                        if (prop.Kind == CityPropKind.Tree)
                        {
                            Assert.AreEqual(TownVegetation.Desert, prop.Vegetation, $"{zone.Id}: desert flora");
                            palms += prop.Variant == 0 || prop.Variant == 3 ? 1 : 0;
                        }

                        if (prop.Kind != CityPropKind.FenceRun)
                        {
                            Assert.IsFalse(MapLayoutRules.OnPath(w.World, prop.Position, 0f), $"{prop.Decor} on a trail at {prop.Position}");
                            Assert.IsFalse(MapLayoutRules.InRiver(w.World, prop.Position, 0f), $"{prop.Decor} in the water at {prop.Position}");
                            Assert.IsNull(OverworldRules.SettlementAt(w, prop.Position), $"{prop.Decor} in a safe zone at {prop.Position}");
                        }
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

            Assert.Greater(palms, 50, "date palms in the oases and the palm grove");
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

            Assert.Greater(OverworldLayouts.DesertPiecesOf(w).Count, 100, "the placed pyramids, karez lines, caravans, towers and camps");
            Assert.AreEqual(0, OverworldLayouts.DesertPiecesOf(OverworldLayouts.Get(OverworldLayouts.ZarafshanId)).Count);
        }

        [Test]
        public void EveryChunk_StaysWithinTheTriangleBudget_AndIsDeterministic()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            var builder = new TownMeshBuilder();
            OverworldChunkBuilder.Content(w, new Vector2Int(10, 10));
            var times = new List<double>();
            int worst = 0;
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
                    worst = Mathf.Max(worst, triangles);
                    Assert.LessOrEqual(triangles, MaxTrianglesPerChunk, $"chunk {c}: {triangles} triangles");
                }
            }

            foreach (Vector2 p in new[] { new Vector2(-480f, -650f), new Vector2(800f, -1800f), new Vector2(450f, -250f), new Vector2(-800f, -1500f) })
            {
                Vector2Int c = OverworldRules.ChunkOf(w, p);
                OverworldChunkContent a = OverworldChunkBuilder.Content(w, c);
                OverworldChunkContent b = OverworldChunkBuilder.Content(w, c);
                Assert.AreEqual(a.Props.Count, b.Props.Count, $"chunk {c}: deterministic");
                Assert.AreEqual(a.Details.Count, b.Details.Count, $"chunk {c}: deterministic");
                CollectionAssert.AreEqual(a.Grid.Height, b.Grid.Height);
            }

            Debug.Log($"[Overworld Areias] {times.Count} chunks: worst {worst} triangles; generation avg {times.Average():0.00} ms (Editor, Mono)");
            Assert.Less(times.Average(), 6.0, "chunk generation stays cheap");
        }

        /// <summary>Highest relief point of <paramref name="zone"/>, <paramref name="inset"/> m inside its borders (the 70 m zone blend left out).</summary>
        private static float MaxHeight(OverworldZone zone, float inset)
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
