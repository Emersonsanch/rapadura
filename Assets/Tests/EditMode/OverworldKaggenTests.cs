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
    /// The continuous Região 3 — Serra de Kaggen (Docs/Mapas100.md §0.10, maps 13–18), in the mould of
    /// <see cref="OverworldTests"/> and <see cref="OverworldReliefTests"/>: one 2400 × 2400 m map with the Vila de Litema
    /// inside, the hand-made 13 and 14 embedded, 15–18 and the in-between areas; zones that tile it, open passages, level
    /// bands growing with the distance from Litema, the soft barrier (wool cloak, pickaxe and torch, blizzard resistance),
    /// the gates (the Zarafshan's gate to 13 lands on Kaggen's east edge), mountain beasts only, the strong but walkable
    /// relief and the per-area dressing within the chunk budget.
    /// </summary>
    public class OverworldKaggenTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.KaggenId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] KaggenMaps =
        {
            MapLayouts.RondaveisId, MapLayouts.HahyaId, OverworldLayouts.AbrigosId, OverworldLayouts.PedraSecaId, OverworldLayouts.NapirasuId,
            OverworldLayouts.MazansId,
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
        public void Region_IsAsBigAsTheZarafshan_WithLitemaInside()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Bounds.width, 2400f);
            Assert.GreaterOrEqual(w.Bounds.height, 2400f);
            Assert.AreEqual(MapRegion.SerraDeKaggen, w.Region);
            Assert.AreEqual(CityTownRecipes.Litema().CityId, w.Vila.Id, "the Vila de Litema is the region's town");
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.AreEqual(MapLayouts.RondaveisId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "Litema stands in the Trilha dos Rondavéis");
            OverworldSettlement saloes = w.SettlementOf(OverworldLayouts.NapirasuId);
            Assert.IsNotNull(saloes);
            Assert.IsTrue(saloes.IsReturnCity, "the Salões de Bronze wake the heroes who fall on 17–18");
            Assert.AreEqual(MapCatalog.ReturnCityOf(OverworldLayouts.MazansId), saloes.Id);
        }

        [Test]
        public void Region_HoldsMaps13To18_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 12);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 12, "many kinds of mountain ground");
            foreach (string id in KaggenMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the Serra");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the Serra");
                Assert.IsNotNull(w.SettlementOf(id), $"{id}: its town or outpost stands in the region");
            }

            Assert.IsFalse(w.ContainsMap(MapCatalog.PomarId));
            Assert.AreNotSame(w, OverworldLayouts.ForMap(MapCatalog.PomarId), "the Zarafshan is its own region");

            foreach (OverworldZone zone in w.Zones)
            {
                List<MapSpawnZone> spawns = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id] == zone).ToList();
                Assert.GreaterOrEqual(spawns.Count, 2, $"{zone.Id}: several monster groups");
                int species = spawns.SelectMany(s => s.AllEntries()).Select(e => e.EnemyId).Distinct().Count();
                Assert.GreaterOrEqual(species, 2, $"{zone.Id}: more than one kind of beast");
                Assert.IsTrue(spawns.Any(s => s.Night != null && s.Night.Length > 0), $"{zone.Id}: a night variant");
            }

            Assert.GreaterOrEqual(w.World.Zones.Count, 150, "the Serra is full of clearings");
            Assert.IsTrue(w.World.Zones.SelectMany(s => s.AllEntries()).Any(e => e.EnemyId == "enemy_mountain_crested_hawk"), "the crested hawk hunts the Serra");
            Assert.IsTrue(w.World.Zones.SelectMany(s => s.AllEntries()).Any(e => e.EnemyId == PeriodSpawnTableIds.Golem), "the stone ushabti guards it");
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
            Assert.AreEqual("passo_do_hahya", OverworldRules.ZoneAt(w, OverworldLayouts.HahyaOffset).Id);
            Assert.AreEqual("abrigos_pintados", OverworldRules.ZoneAt(w, OverworldLayouts.AbrigoOffset).Id);
            Assert.AreEqual("muralhas_pedra_seca", OverworldRules.ZoneAt(w, OverworldLayouts.RecintoOffset).Id);
            Assert.AreEqual("saloes_de_napirasu", OverworldRules.ZoneAt(w, OverworldLayouts.SaloesOffset).Id);
            Assert.AreEqual("estela_alta", OverworldRules.ZoneAt(w, OverworldLayouts.EstelaOffset).Id);
            Assert.AreEqual("cume_dos_mazans", OverworldRules.ZoneAt(w, OverworldLayouts.MazanRing).Id);
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
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("trilha_dos_rondaveis"), w.Zone("terracos_de_litema")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("encosta_do_vento"), w.Zone("passo_do_hahya")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("passos_estreitos"), w.Zone("abrigos_pintados")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("escarpas_de_arenito"), w.Zone("saloes_de_napirasu")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("estela_alta"), w.Zone("cume_dos_mazans")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("trilha_dos_rondaveis"), w.Zone("cume_dos_mazans")));

            var seen = new HashSet<OverworldZone> { w.Zone("trilha_dos_rondaveis") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to Litema");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromLitema()
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
            Assert.AreEqual(9, byDistance[0].MinLevel, "level 9 at Litema (13 is 9–11)");
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 36, "the summit is the end of the Serra");
            foreach (string far in new[] { OverworldLayouts.AbrigosId, OverworldLayouts.PedraSecaId, OverworldLayouts.NapirasuId, OverworldLayouts.MazansId })
            {
                foreach (OverworldZone zone in w.ZonesOfMap(far))
                {
                    Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1000f, $"{zone.Id}: the high maps are far from Litema");
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
        public void SoftBarrier_WalkLevelHeroBonusAndSpecialGear()
        {
            OverworldLayout w = World;
            OverworldZone hahya = w.Zone("passo_do_hahya");
            Assert.AreEqual(MapRequirementTags.WoolCloak, hahya.RequirementTag);
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(hahya, 20, CharacterId.Lavine, null), "the pass asks for the wool cloak");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(hahya, 20, CharacterId.Lavine, tag => tag == MapRequirementTags.WoolCloak));

            OverworldZone saloes = w.Zone("saloes_de_napirasu");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(saloes, 12, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(saloes, 40, CharacterId.Lavine, null), "pickaxe and torch");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(saloes, 40, CharacterId.Lavine, tag => tag == MapRequirementTags.PickaxeTorch));

            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.MazansId))
            {
                Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 50, null, null), $"{zone.Id}: blizzard resistance");
                Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 50, null, tag => tag == MapRequirementTags.BlizzardResistance));
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("trilha_dos_rondaveis"), 8, null, null));

            // Maithe walks into the mountain 5 levels early (Docs/MetodologiaMapasERacas.md §3).
            OverworldZone passos = w.Zone("passos_estreitos");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(passos, passos.WalkLevel - 1, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(passos, passos.WalkLevel - 1, CharacterId.Maithe, null));
        }

        [Test]
        public void Gates_LeadOutOfTheRegion_AndTheZarafshanGateLandsOnTheEastEdge()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            CollectionAssert.AreEquivalent(new[] { MapCatalog.PomarId, MapLayouts.ArtificesId, MapLayouts.LagoLimiarId }, gates.Select(g => g.TargetMapId).ToArray());
            foreach (MapExitSpec gate in gates)
            {
                Assert.IsTrue(MapCatalog.Get(gate.TargetMapId).IsBuilt && MapLayouts.Has(gate.TargetMapId), gate.Id);
                Vector2 c = gate.Trigger.center;
                float toEdge = Mathf.Min(Mathf.Min(c.x - w.Bounds.xMin, w.Bounds.xMax - c.x), Mathf.Min(c.y - w.Bounds.yMin, w.Bounds.yMax - c.y));
                Assert.Less(toEdge, 3f, $"{gate.Id}: on the region's edge");
                Assert.AreEqual(gate.Arrival, w.ArrivalFor(MapLayouts.RondaveisId, gate.TargetMapId, out _), $"coming back from {gate.TargetMapId}");
            }

            // The Zarafshan's west gate (to 13) brings the hero to Kaggen's east edge, in an area that counts for 13, so the
            // way back lands at that same Zarafshan gate.
            OverworldLayout zarafshan = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            MapExitSpec west = zarafshan.World.Exits.Find(e => e.Kind == MapExitKind.Map && e.TargetMapId == MapLayouts.RondaveisId);
            Assert.IsNotNull(west);
            Assert.Less(west.Trigger.center.x - zarafshan.Bounds.xMin, 3f, "the Zarafshan's gate to 13 is on its west edge");
            Vector2 arrival = w.ArrivalFor(MapLayouts.RondaveisId, OverworldRules.ZoneAt(zarafshan, west.Arrival).MapIdOrOwner, out Vector2 facing);
            Assert.Less(w.Bounds.xMax - arrival.x, 12f, "…and lands on Kaggen's east edge");
            Assert.Less(facing.x, 0f, "facing into the Serra");
            Assert.AreEqual(MapLayouts.RondaveisId, OverworldRules.ZoneAt(w, arrival).MapIdOrOwner);
            Assert.AreEqual(west.Arrival, zarafshan.ArrivalFor(MapCatalog.PomarId, OverworldRules.ZoneAt(w, arrival).MapIdOrOwner, out _));

            // The Limiar gallery leaves from the Passo do Hahya, like the hand-made 14.
            MapExitSpec limiar = gates.Find(g => g.TargetMapId == MapLayouts.LagoLimiarId);
            Assert.AreEqual(MapLayouts.HahyaId, OverworldRules.ZoneAt(w, limiar.Arrival).MapIdOrOwner);

            // By barca (or a new game): Litema's dock, inside its safe zone; the Boca da Mina's dock for 14.
            Vector2 dock = w.ArrivalFor(MapLayouts.RondaveisId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            Assert.AreEqual(w.SettlementOf(MapLayouts.HahyaId).DockArrival, w.ArrivalFor(MapLayouts.HahyaId, MapLayouts.RondaveisId, out _));
        }

        [Test]
        public void Spawns_StayOutOfSafeZones_InsideTheirArea_WithExistingNonBossMonsters()
        {
            OverworldLayout w = World;
            var checkedIds = new HashSet<string>();
            foreach (MapSpawnZone spawn in w.World.Zones)
            {
                OverworldZone zone = w.ZoneOfSpawn[spawn.Id];
                Assert.IsTrue(zone.Bounds.Contains(spawn.Center), spawn.Id);
                Assert.GreaterOrEqual(OverworldRules.DistanceToSafety(w, spawn.Center) - spawn.Radius, MapLayoutRules.SpawnSafeMargin, spawn.Id);
                Assert.IsNull(OverworldRules.SettlementAt(w, spawn.Center), spawn.Id);
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
        // Relief and dressing
        // ------------------------------------------------------------------

        [Test]
        public void Height_IsPureAndContinuous_AndChunksHaveNoSeams()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            var rnd = new System.Random(13);
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
                OverworldGroundGrid east = OverworldChunkBuilder.BuildGrid(w, c + Vector2Int.right);
                Rect r = OverworldRules.ChunkRect(w, c);
                for (float t = 0f; t <= w.ChunkSize; t += 0.53f)
                {
                    var onEast = new Vector2(r.xMax, r.yMin + t);
                    Assert.AreEqual(a.Sample(onEast), east.Sample(onEast), 1e-4f, $"seam between {c} and its east neighbour at {onEast}");
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
        public void Trails_AreLevelAcross_AndWalkableAlong_EvenInTheHighMountain()
        {
            OverworldLayout w = World;
            OverworldRelief relief = Relief;
            var cross = new List<float>();
            float steepest = 0f;
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
                    if (left.Free < 0.999f || right.Free < 0.999f)
                    {
                        continue;
                    }

                    cross.Add(Mathf.Abs(left.Height - right.Height) / path.Width);
                }

                float along = Mathf.Atan(Mathf.Abs(relief.HeightAt(path.B) - relief.HeightAt(path.A)) / length) * Mathf.Rad2Deg;
                steepest = Mathf.Max(steepest, along);
                Assert.Less(along, 30f, $"trail {path.A} → {path.B} too steep to walk comfortably");
            }

            Debug.Log($"[Overworld Kaggen] steepest trail piece {steepest:0.0}°");
            cross.Sort();
            Assert.Greater(cross.Count, 1000);
            Assert.Less(cross[cross.Count / 2], 0.005f, "a trail is level across its width (median)");
            Assert.Less(cross[(int)(cross.Count * 0.98f)], 0.08f, "a trail is level across its width (98 %)");
            Assert.Less(cross.Last(), 0.3f, "no trail leans like a hillside");
        }

        [Test]
        public void TheMountainRises_PassesScarpsAndSummitHighAboveLitema()
        {
            OverworldLayout w = World;
            float litema = MaxHeight(w.Zone("trilha_dos_rondaveis"));
            Assert.Less(litema, 15f, "the golden slopes round Litema are gentle");
            Assert.Greater(MaxHeight(w.Zone("passos_estreitos")), 20f, "the narrow passes run between high walls");
            Assert.Greater(MaxHeight(w.Zone("escarpas_de_arenito")), 20f, "the sandstone scarps are high");
            Assert.Greater(MaxHeight(w.Zone("campos_de_neve")), 22f);
            Assert.Greater(MaxHeight(w.Zone("cume_dos_mazans")), 30f, "the summit is the highest ground");
            Assert.Greater(MaxHeight(w.Zone("cume_dos_mazans")), litema + 20f);

            // The farming terraces and the scarps step: a good share of their ground is level.
            foreach (string id in new[] { "terracos_de_litema", "escarpas_de_arenito" })
            {
                OverworldZone zone = w.Zone(id);
                int level = 0;
                int total = 0;
                for (float x = zone.Bounds.xMin + 40f; x < zone.Bounds.xMax - 40f; x += 9f)
                {
                    for (float z = zone.Bounds.yMin + 40f; z < zone.Bounds.yMax - 40f; z += 9f)
                    {
                        var p = new Vector2(x, z);
                        float gx = Relief.HeightAt(p + Vector2.right) - Relief.HeightAt(p - Vector2.right);
                        float gz = Relief.HeightAt(p + Vector2.up) - Relief.HeightAt(p - Vector2.up);
                        total++;
                        if (Mathf.Sqrt(gx * gx + gz * gz) * 0.5f < 0.08f)
                        {
                            level++;
                        }
                    }
                }

                Assert.Greater(level / (float)total, 0.3f, $"{id}: steps");
            }

            // The torrents run on their valley floor; their bridges land at y = 0 at both ends; ponds carve a bed.
            Assert.GreaterOrEqual(w.World.Rivers.Sum(r => r.Crossings.Count(c => c.IsBridge)), 5, "torrents with bridges");
            foreach (MapRiver river in w.World.Rivers)
            {
                Vector2 mid = Vector2.Lerp(river.Points[0], river.Points[1], 0.5f);
                if (river.Width >= OverworldRelief.CarveMinWidth && !river.Crossings.Any(c => Vector2.Distance(c.At, mid) < c.Width + 4f))
                {
                    Assert.Less(Relief.HeightAt(mid), OverworldRelief.WaterY - 0.5f, $"a carved bed under the water line at {mid}");
                }

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
        public void Dressing_EveryAreaHasItsOwnLook_StandsOnTheGround_OffTheTrails()
        {
            OverworldLayout w = World;
            var expected = new Dictionary<string, OverworldDecor[]>
            {
                { "terracos_de_litema", new[] { OverworldDecor.TerraceRow } },
                { "pastos_dos_currais", new[] { OverworldDecor.Kraal } },
                { "passos_estreitos", new[] { OverworldDecor.RockSpire } },
                { "abrigos_pintados", new[] { OverworldDecor.PaintedShelter } },
                { "muralhas_pedra_seca", new[] { OverworldDecor.DryStoneWall } },
                { "saloes_de_napirasu", new[] { OverworldDecor.BronzePillar } },
                { "campos_de_neve", new[] { OverworldDecor.SnowDrift } },
                { "cume_dos_mazans", new[] { OverworldDecor.GraniteBlock, OverworldDecor.SnowDrift, OverworldDecor.MistBank } },
                { "torrentes_de_qacha", new[] { OverworldDecor.Ferns } },
                { "encosta_do_vento", new[] { OverworldDecor.Cairn, OverworldDecor.Flowers } },
            };

            var signatures = new Dictionary<string, HashSet<string>>();
            bool forestTrees = false;
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
                        if ((prop.Kind == CityPropKind.Tree || prop.Kind == CityPropKind.Bush) && prop.Vegetation == TownVegetation.Forest && w.Bounds.Contains(prop.Position))
                        {
                            forestTrees = true;
                        }
                    }

                    foreach (OverworldProp prop in content.Props)
                    {
                        if (prop.Kind != CityPropKind.FenceRun)
                        {
                            Assert.IsFalse(MapLayoutRules.OnPath(w.World, prop.Position, 0f), $"chunk {c}: {prop.Decor} on a trail at {prop.Position}");
                        }

                        Assert.IsFalse(MapLayoutRules.InRiver(w.World, prop.Position, 0f), $"chunk {c}: prop in a river at {prop.Position}");
                        Assert.IsNull(OverworldRules.SettlementAt(w, prop.Position), $"chunk {c}: prop in a safe zone at {prop.Position}");
                    }

                    foreach (OverworldProp detail in content.Details)
                    {
                        Assert.IsFalse(MapLayoutRules.OnPath(w.World, detail.Position, 0f), $"{detail.Decor} on a trail at {detail.Position}");
                        Assert.IsFalse(MapLayoutRules.InRiver(w.World, detail.Position, 0f), $"{detail.Decor} in a river at {detail.Position}");
                        Assert.IsNull(OverworldRules.SettlementAt(w, detail.Position), $"{detail.Decor} in a safe zone at {detail.Position}");
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

            Assert.IsFalse(forestTrees, "the Serra grows cedars, aloes and baobabs, not the Zarafshan's poplars");
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
            Debug.Log($"[Overworld Kaggen] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
                $"generation avg {average:0.00} ms, p50 {times[times.Count / 2]:0.00} ms, p95 {times[(int)(times.Count * 0.95f)]:0.00} ms (Editor, Mono)");
            Assert.Less(average, 6.0, "chunk generation stays cheap even in the Editor");
        }

        private static float MaxHeight(OverworldZone zone)
        {
            float max = float.MinValue;
            for (float x = zone.Bounds.xMin + 2f; x < zone.Bounds.xMax; x += 6f)
            {
                for (float z = zone.Bounds.yMin + 2f; z < zone.Bounds.yMax; z += 6f)
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

        /// <summary>The stone ushabti's id (PeriodSpawnTable keeps it).</summary>
        private static class PeriodSpawnTableIds
        {
            public const string Golem = Rapadura.Gameplay.Night.PeriodSpawnTable.MountainGolem;
        }
    }
}
