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
    /// The continuous Região 8 — Planície de Mingcheng (Docs/Mapas100.md §0.10, maps 41–46), in the mould of
    /// <see cref="OverworldTetoTests"/>: one 2400 × 2400 m map with Mingcheng and the Torres Brancas (their own recipes) inside,
    /// 41 and 42 as zones, zones that tile it, open passages, level bands growing with the distance from Mingcheng from the
    /// catalog's 90, the soft barrier (the Sun Seal at the Torres Brancas, nothing on the way into Mingcheng), the gates towards
    /// the Zarafshan (south edge) and the Teto do Mundo (west edge), the plain's beasts (no boss), the relief (karst, pines and the
    /// wall's crest high, the plain, the river and the canal low, the river carved and crossed at stone bridges that land flat)
    /// and the per-area dressing (the great wall on its crest among them) within the chunk budget.
    /// </summary>
    public class OverworldMingchengTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.MingchengId);

        private static OverworldRelief Relief => OverworldRelief.For(World);

        private static readonly string[] MingchengMaps = { OverworldLayouts.MingchengMapId, OverworldLayouts.TorresBrancasMapId };

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
        public void Region_IsAsBigAsTheOthers_WithMingchengAndTheTorresBrancasInside()
        {
            OverworldLayout w = World;
            Assert.AreEqual(2400f, w.Bounds.width);
            Assert.AreEqual(2400f, w.Bounds.height);
            Assert.AreEqual(MapRegion.PlanicieDeMingcheng, w.Region);
            Assert.AreEqual(CityTownRecipes.MingchengCityId, w.Vila.Id, "Mingcheng is the region's town");
            Assert.AreEqual(MapCatalog.Mingcheng, w.Vila.Id);
            Assert.IsTrue(w.Vila.IsReturnCity);
            Assert.IsTrue(w.Vila.HasDock, "the Cais da Barca of Mingcheng");
            Assert.IsFalse(w.Vila.Recipe.Backdrop, "no backdrop of its own: the plain is all round it");
            Assert.GreaterOrEqual(w.Vila.GroundHalfExtent, CityTownRecipes.MingchengSolidRadius + 5f, "room for the ring city");
            Assert.AreEqual(OverworldLayouts.MingchengMapId, OverworldRules.ZoneAt(w, w.Vila.Offset).MapId, "Mingcheng stands in its rings (41)");

            OverworldSettlement torres = w.SettlementOf(OverworldLayouts.TorresBrancasMapId);
            Assert.AreEqual(CityTownRecipes.TorresBrancasCityId, torres.Id);
            Assert.IsTrue(torres.IsReturnCity, "the high elves' city wakes the hero of 42");
            Assert.IsFalse(torres.Recipe.Backdrop);
            Assert.GreaterOrEqual(torres.GroundHalfExtent, CityTownRecipes.TorresBrancasSolidRadius + 5f, "room for the white spires");
            Assert.Greater(torres.Offset.x, w.Vila.Offset.x + 600f, "the white hills east of Mingcheng");

            foreach (string map in MingchengMaps)
            {
                string city = MapCatalog.ReturnCityOf(map);
                Assert.IsTrue(w.Settlements.Exists(s => s.IsReturnCity && s.Id == city), $"{map}: its return city {city} stands in the plain");
            }

            Assert.AreEqual(MapCatalog.Mingcheng, MapCatalog.ReturnCityOf(OverworldLayouts.MingchengMapId), "the catalog's return city is kept");
            Assert.AreEqual(2, w.Settlements.Count(s => s.IsReturnCity));
        }

        [Test]
        public void Region_HoldsMaps41And42_ManyAreas_EachWithItsOwnMonsters()
        {
            OverworldLayout w = World;
            Assert.GreaterOrEqual(w.Zones.Count, 20);
            Assert.GreaterOrEqual(w.Zones.Select(z => z.Terrain).Distinct().Count(), 15, "many kinds of plain, hill and mountain");
            Assert.IsTrue(w.Zones.All(z => OverworldTerrains.IsMingcheng(z.Terrain)), "the plain's own terrains");
            foreach (string id in MingchengMaps)
            {
                Assert.IsTrue(w.ContainsMap(id), $"{id} is an area of the plain");
                Assert.AreSame(w, OverworldLayouts.ForMap(id), $"{id}: walking there loads the plain");
                Assert.IsNotNull(w.SettlementOf(id), $"{id}: its town stands in the region");
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

            Assert.GreaterOrEqual(w.World.Zones.Count, 300, "the plain is full of clearings");
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
            Assert.AreEqual("aneis_de_mingcheng", OverworldRules.ZoneAt(w, OverworldLayouts.MingchengOffset).Id);
            Assert.AreEqual("torres_brancas", OverworldRules.ZoneAt(w, OverworldLayouts.TorresBrancasOffset).Id);
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
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("aneis_de_mingcheng"), w.Zone("caminho_dos_leoes")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("aneis_de_mingcheng"), w.Zone("eclusa_da_lua")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("jardins_em_terraco"), w.Zone("torres_brancas")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("arrozais_espelhados"), w.Zone("moinho_dagua")));
            Assert.IsTrue(OverworldRules.AreNeighbours(w.Zone("torre_quinhentos"), w.Zone("muralha_das_mil_torres")));
            Assert.IsFalse(OverworldRules.AreNeighbours(w.Zone("aneis_de_mingcheng"), w.Zone("muralha_das_mil_torres")));

            var seen = new HashSet<OverworldZone> { w.Zone("aneis_de_mingcheng") };
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

            Assert.AreEqual(w.Zones.Count, seen.Count, "every area is connected to Mingcheng");
        }

        [Test]
        public void LevelBands_GrowWithTheDistanceFromMingcheng_FromTheCatalogsLevel90()
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
            Assert.AreEqual(MapCatalog.Get(OverworldLayouts.MingchengMapId).MonsterLevelMin, byDistance[0].MinLevel, "level 90 at Mingcheng (41 is 90–96)");
            Assert.AreEqual(90, byDistance[0].MinLevel);
            Assert.GreaterOrEqual(byDistance.Last().MinLevel, 115, "the wall and the nian's peaks are the end of the plain");
            Assert.GreaterOrEqual(w.Zones.Max(z => z.MaxLevel), 120, "the wall of the ☆ sheets (45: 115–125)");
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.TorresBrancasMapId))
            {
                Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 600f, $"{zone.Id}: the Torres Brancas are on the white hills, east");
            }

            foreach (OverworldZone zone in w.Zones.Where(z => z.Terrain == OverworldTerrain.GreatWall))
            {
                Assert.Greater(OverworldRules.DistanceFromVila(w, zone), 1200f, $"{zone.Id}: the wall is far from Mingcheng");
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
        public void SoftBarrier_TheSunSealAtTheTorresBrancas_NothingOnTheWayToMingcheng()
        {
            OverworldLayout w = World;
            // 42 asks for the Sun Seal, sold only in Mingcheng: every zone of 42 asks for it, nothing on the way to Mingcheng does.
            Assert.AreEqual(MapRequirementTags.SunSeal, MapCatalog.Get(OverworldLayouts.TorresBrancasMapId).SpecialRequirementTag);
            CollectionAssert.IsNotEmpty(w.ZonesOfMap(OverworldLayouts.TorresBrancasMapId).ToList());
            foreach (OverworldZone zone in w.ZonesOfMap(OverworldLayouts.TorresBrancasMapId))
            {
                Assert.AreEqual(MapRequirementTags.SunSeal, zone.RequirementTag, zone.Id);
                Assert.AreEqual(MapAccessCheck.MissingRequirement, OverworldRules.CheckEnter(zone, 110, CharacterId.Lavine, null), $"{zone.Id}: asks for the Sun Seal");
                Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(zone, 110, CharacterId.Lavine, tag => tag == MapRequirementTags.SunSeal));
            }

            Assert.AreEqual(MapRequirementTags.None, MapCatalog.Get(OverworldLayouts.MingchengMapId).SpecialRequirementTag);
            foreach (OverworldZone zone in w.Zones.Where(z => z.MapId != OverworldLayouts.TorresBrancasMapId))
            {
                Assert.IsTrue(string.IsNullOrEmpty(zone.RequirementTag), $"{zone.Id}: only the Torres Brancas ask for an item");
            }

            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("aneis_de_mingcheng"), 90, CharacterId.Lavine, null), "the city's rings");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("caminho_dos_leoes"), 91, CharacterId.Lavine, null), "the way in from the Zarafshan");
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(w.Zone("arrozais_espelhados"), 96, CharacterId.Lavine, null), "the way in from the Teto");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("caminho_dos_leoes"), 40, CharacterId.Lavine, null), "the plain is for level 90");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(w.Zone("muralha_oeste"), 100, CharacterId.Lavine, tag => true));
        }

        [Test]
        public void Gates_TwoToTheZarafshanSouth_TwoToTheTetoWest_TheStoneRing_AndTheDocks()
        {
            OverworldLayout w = World;
            List<MapExitSpec> gates = w.World.Exits.Where(e => e.Kind == MapExitKind.Map).ToList();
            Assert.AreEqual(4, gates.Count);
            foreach (MapExitSpec gate in gates)
            {
                MapRegion region = MapCatalog.Get(gate.TargetMapId).Region;
                if (region == MapRegion.Zarafshan)
                {
                    Assert.Less(gate.Trigger.center.y - w.Bounds.yMin, 3f, $"{gate.Id}: on the south edge");
                }
                else
                {
                    Assert.AreEqual(MapRegion.TetoDoMundo, region, $"{gate.Id}: to the Zarafshan or the Teto");
                    Assert.Less(gate.Trigger.center.x - w.Bounds.xMin, 3f, $"{gate.Id}: on the west edge");
                }

                Assert.AreEqual(gate.Arrival, w.ArrivalFor(OverworldLayouts.MingchengMapId, gate.TargetMapId, out Vector2 facing), gate.Id);
                Assert.Greater(Vector2.Dot(facing, w.Bounds.center - gate.Arrival), 0f, $"{gate.Id}: facing into the plain");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, gate.Arrival, 1f), $"{gate.Id}: a trail starts at the gate");
            }

            Assert.AreEqual(2, gates.Count(g => MapCatalog.Get(g.TargetMapId).Region == MapRegion.Zarafshan));
            Assert.IsNotNull(w.World.Exits.Find(e => e.Kind == MapExitKind.WorldMap && e.Id == "exit_island"), "the stone ring by Mingcheng");

            Vector2 dock = w.ArrivalFor(OverworldLayouts.MingchengMapId, null, out _);
            Assert.IsTrue(w.Vila.SafeZone.Contains(dock));
            foreach (string id in MingchengMaps)
            {
                OverworldSettlement town = w.SettlementOf(id);
                Assert.AreEqual(town.DockArrival, w.ArrivalFor(id, OverworldLayouts.MingchengMapId, out _), id);
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
        public void ThePlain_KarstPinesAndTheWallHigh_ThePlainLow_TheRiverCarved_AndItsStoneBridgesFlat()
        {
            OverworldLayout w = World;
            Assert.Greater(MaxHeight(w.Zone("picos_de_calcario")), 22f, "the karst towers");
            foreach (string high in new[] { "cristas_dos_pinheiros", "muralha_das_mil_torres", "torre_quinhentos", "picos_do_nian" })
            {
                Assert.Greater(MaxHeight(w.Zone(high)), 15f, $"{high}: high ground");
            }

            foreach (string low in new[] { "aneis_de_mingcheng", "caminho_dos_leoes", "eclusa_da_lua", "rio_dos_salgueiros" })
            {
                Assert.Less(MaxHeight(w.Zone(low), 60f), 10f, $"{low}: the river plain");
            }

            // The wall's crest: higher on the wall's line than 150 m south of it, all along the wall's areas.
            OverworldRelief relief = Relief;
            foreach (OverworldZone zone in w.Zones.Where(z => z.Terrain == OverworldTerrain.GreatWall))
            {
                float x = zone.Bounds.center.x;
                float crest = relief.HeightAt(new Vector2(x, OverworldLayouts.GreatWallZ(x)));
                Assert.Greater(crest, relief.HeightAt(new Vector2(x, OverworldLayouts.GreatWallZ(x) - 150f)) + 5f, $"{zone.Id}: the wall rides a crest");
            }

            // The upper Rio Dourado: it meets its lower course on the south edge (the Zarafshan's (520, 1400) in the world).
            Assert.AreEqual(new Vector2(320f, w.Bounds.yMin), OverworldLayouts.DouradoHead[OverworldLayouts.DouradoHead.Length - 1]);
            Assert.AreEqual(OverworldLayouts.LanternCanalLine[0], OverworldLayouts.DouradoHead[0], "the canal runs on from the river's head");
            foreach (MapRiver river in w.World.Rivers.Where(r => r.Points.Count > 2))
            {
                Assert.GreaterOrEqual(river.Width, OverworldRelief.CarveMinWidth, "a carved river");
                Assert.IsFalse(river.Crossings.Exists(c => !c.IsBridge), "no ford: stone bridges");
            }

            Vector2 deep = Vector2.Lerp(OverworldLayouts.LanternCanalLine[2], OverworldLayouts.LanternCanalLine[3], 0.3f);
            Assert.Less(Relief.HeightAt(deep), OverworldRelief.WaterY - 0.5f, "deep in the canal");
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

            Assert.GreaterOrEqual(bridges, 3);
        }

        [Test]
        public void EveryMingchengTerrain_HasAClimate()
        {
            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            foreach (OverworldTerrain terrain in System.Enum.GetValues(typeof(OverworldTerrain)))
            {
                if (OverworldTerrains.IsMingcheng(terrain))
                {
                    Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), terrain.ToString());
                }
            }

            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.LanternCanal).Weather, "mist on the canal");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.GreatWall).Weather, "mist under the wall");
            Assert.AreEqual(Rapadura.Gameplay.Environment.WeatherType.Clear, OverworldAreaAmbience.ClimateOf(OverworldTerrain.WhiteHills).Weather, "clear over the white hills");
            Assert.IsTrue(OverworldAreaAmbience.ClimateOf(OverworldTerrain.KarstPeaks).Wild);
            Assert.IsFalse(OverworldAreaAmbience.ClimateOf(OverworldTerrain.NineRings).Wild);
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
                { "aneis_de_mingcheng", new[] { OverworldDecor.Willow, OverworldDecor.RedLantern } },
                { "caminho_dos_leoes", new[] { OverworldDecor.StoneLion, OverworldDecor.StoneTurtle } },
                { "rio_dos_salgueiros", new[] { OverworldDecor.Willow } },
                { "canal_das_lanternas", new[] { OverworldDecor.Willow, OverworldDecor.RedLantern } },
                { "arrozais_espelhados", new[] { OverworldDecor.RiceTuft, OverworldDecor.Scarecrow } },
                { "campos_de_painco", new[] { OverworldDecor.MilletTuft } },
                { "colinas_de_cha", new[] { OverworldDecor.Hedge } },
                { "bambuzal_do_oeste", new[] { OverworldDecor.Bamboo } },
                { "torres_brancas", new[] { OverworldDecor.MirrorPost, OverworldDecor.WhiteSpire } },
                { "picos_de_calcario", new[] { OverworldDecor.KarstPillar } },
                { "cristas_dos_pinheiros", new[] { OverworldDecor.MountainPine } },
                { "muralha_das_mil_torres", new[] { OverworldDecor.GreatWallRun, OverworldDecor.BeaconTower } },
                { "fossos_de_terracota", new[] { OverworldDecor.TerracottaPit } },
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
            Debug.Log($"[Overworld Mingcheng] {times.Count} chunks: {total / times.Count} triangles on average, worst {worst} ({worstChunk}); " +
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
