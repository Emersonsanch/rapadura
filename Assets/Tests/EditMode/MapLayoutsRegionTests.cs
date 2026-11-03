using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The regional maps after 01–02 (Docs/Mapas100.md: 07 Estrada das Caravanas, 08 Ruínas dos Artífices, 13 Trilha dos
    /// Rondavéis, 14 Passo do Hahya, 19 Lago do Limiar, 20 Salão de Tages): each has a layout and is built in the catalog,
    /// passes <see cref="MapLayoutRules.Validate"/>, spawns inside the catalog's level band, only existing non-boss monsters
    /// with a night table, keeps its safe settlement free of spawns, and its exits lead to existing maps that lead back.
    /// </summary>
    public class MapLayoutsRegionTests
    {
        private static IEnumerable<string> RegionalIds => MapLayouts.RegionalMapIds;

        [SetUp]
        public void SetUp() => MapLayouts.ClearCache();

        [TearDown]
        public void TearDown() => MapLayouts.ClearCache();

        [Test]
        public void TheSixRegionalMaps_HaveALayout_AndAreBuiltInTheCatalog()
        {
            CollectionAssert.AreEqual(
                new[] { "map_007_caravanas", "map_008_artifices", "map_013_rondaveis", "map_014_hahya", "map_019_lago_limiar", "map_020_tages" },
                RegionalIds.ToArray());
            foreach (string id in RegionalIds)
            {
                MapDefinition map = MapCatalog.Get(id);
                Assert.IsNotNull(map, $"{id}: in MapCatalog");
                Assert.IsTrue(map.IsBuilt, $"{id}: built");
                Assert.IsTrue(MapLayouts.Has(id), $"{id}: Has");
                MapLayout layout = MapLayouts.For(id);
                Assert.IsNotNull(layout, id);
                Assert.AreEqual(id, layout.MapId);
                Assert.AreSame(layout, MapLayouts.For(id), "cached");
                Assert.AreEqual(map.LegacyBiome, layout.Biome, $"{id}: the region's biome (weather, ambience, town style)");
            }
        }

        [Test]
        public void TheSixRegionalMaps_PassTheLayoutRules()
        {
            foreach (string id in RegionalIds)
            {
                List<string> problems = MapLayoutRules.Validate(MapLayouts.For(id));
                Assert.IsEmpty(problems, string.Join("\n", problems));
            }
        }

        [Test]
        public void Spawns_StayInsideTheCatalogBand_AndCoverItsEnds()
        {
            foreach (string id in RegionalIds)
            {
                MapDefinition map = MapCatalog.Get(id);
                MapLayout layout = MapLayouts.For(id);
                Assert.GreaterOrEqual(layout.Zones.Count, 4, $"{id}: at least four hunting clearings (M map)");
                foreach (MapSpawnZone zone in layout.Zones)
                {
                    Assert.GreaterOrEqual(zone.MinLevel, map.MonsterLevelMin, $"{id}/{zone.Id}");
                    Assert.LessOrEqual(zone.MaxLevel, map.MonsterLevelMax, $"{id}/{zone.Id}");
                    Assert.LessOrEqual(zone.MinLevel, zone.MaxLevel, $"{id}/{zone.Id}");
                    Assert.IsFalse(string.IsNullOrEmpty(zone.Archetypes), $"{id}/{zone.Id}: archetype tags");
                }

                Assert.AreEqual(map.MonsterLevelMin, layout.Zones.Min(z => z.MinLevel), $"{id}: some clearing starts at the band's floor");
                Assert.AreEqual(map.MonsterLevelMax, layout.Zones.Max(z => z.MaxLevel), $"{id}: some clearing reaches the band's top");
            }
        }

        [Test]
        public void Spawns_AreExistingNonBossMonsters_WithANightTable()
        {
            foreach (string id in RegionalIds)
            {
                foreach (MapSpawnZone zone in MapLayouts.For(id).Zones)
                {
                    Assert.IsNotEmpty(zone.Day, $"{id}/{zone.Id}: day table");
                    Assert.IsNotEmpty(zone.Night, $"{id}/{zone.Id}: night table (NightVariantRules)");
                    Assert.IsTrue(zone.Night.Any(e => e.Form != Rapadura.Gameplay.Night.NightForm.Normal), $"{id}/{zone.Id}: the night brings variants or night species");
                    foreach (var entry in zone.AllEntries())
                    {
                        EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{entry.EnemyId}");
                        Assert.IsNotNull(definition, $"{id}/{zone.Id}: enemy '{entry.EnemyId}' not in Resources/Enemies");
                        Assert.IsFalse(definition.IsBoss, $"{id}/{zone.Id}: the open map never spawns a boss ({entry.EnemyId})");
                    }
                }
            }
        }

        [Test]
        public void SafeSettlement_HasNoSpawnInside_AndTheBarcaLeavesTheHeroInIt()
        {
            foreach (string id in RegionalIds)
            {
                MapLayout layout = MapLayouts.For(id);
                Assert.IsNotNull(layout.Settlement, $"{id}: city or outpost");
                Assert.IsNotNull(layout.Town, $"{id}: generated town");
                foreach (MapSpawnZone zone in layout.Zones)
                {
                    float gap = MapLayoutRules.DistanceToRect(zone.Center, layout.SafeZone) - zone.Radius;
                    Assert.GreaterOrEqual(gap, MapLayoutRules.SpawnSafeMargin, $"{id}/{zone.Id}: spawns {gap:0.0} m from the safe zone");
                }

                Assert.IsTrue(layout.SafeZone.Contains(layout.DockArrival), $"{id}: the Barca leaves the hero in the safe zone");
                Assert.IsTrue(layout.Exits.Exists(e => e.Kind == MapExitKind.Dock), $"{id}: Cais da Barca");
                Assert.IsTrue(layout.Exits.Exists(e => e.Kind == MapExitKind.WorldMap), $"{id}: back to the island map");
                Assert.IsTrue(layout.SafeZone.Contains(layout.Town.Respawn), $"{id}: plaza inside the safe zone");
            }
        }

        [Test]
        public void Cities_HostTheirReturnCity_OutpostsDoNot()
        {
            foreach (string id in RegionalIds)
            {
                MapDefinition map = MapCatalog.Get(id);
                MapLayout layout = MapLayouts.For(id);
                Assert.AreEqual(map.Settlement == MapSettlementKind.City, layout.SettlementIsReturnCity, $"{id}: only a city map respawns the hero in it");
                if (layout.SettlementIsReturnCity)
                {
                    Assert.AreEqual(map.ReturnCityId, layout.Town.CityId, $"{id}: the map's town is its return city");
                }
            }
        }

        [Test]
        public void Exits_LeadToBuiltMaps_ThatLeadBack()
        {
            var all = new List<string> { MapCatalog.PomarId, MapCatalog.TugaiId };
            all.AddRange(RegionalIds);
            foreach (string id in RegionalIds)
            {
                MapLayout layout = MapLayouts.For(id);
                foreach (MapExitSpec exit in layout.Exits.Where(e => e.Kind == MapExitKind.Map))
                {
                    MapDefinition target = MapCatalog.Get(exit.TargetMapId);
                    Assert.IsNotNull(target, $"{id}/{exit.Id}: '{exit.TargetMapId}' is not a map");
                    Assert.IsTrue(target.IsBuilt && MapLayouts.Has(target.Id), $"{id}/{exit.Id}: '{exit.TargetMapId}' is not playable");
                    MapLayout back = MapLayouts.For(target.Id);
                    Assert.IsTrue(back.Exits.Exists(e => e.Kind == MapExitKind.Map && e.TargetMapId == id), $"{target.Id} has no way back to {id}");
                    Assert.AreEqual(exit.Arrival, layout.ArrivalFrom(target.Id, out _), $"{id}: arriving from {target.Id} stands at this border");
                }
            }

            // The campaign chain on foot: Pomar → 07 → 08 → 13 → 14 → 19 → 20.
            string[] chain = { MapCatalog.PomarId, MapLayouts.CaravanasId, MapLayouts.ArtificesId, MapLayouts.RondaveisId, MapLayouts.HahyaId, MapLayouts.LagoLimiarId, MapLayouts.TagesId };
            for (int i = 0; i + 1 < chain.Length; i++)
            {
                Assert.IsTrue(MapLayouts.For(chain[i]).Exits.Exists(e => e.Kind == MapExitKind.Map && e.TargetMapId == chain[i + 1]), $"{chain[i]} → {chain[i + 1]}");
            }
        }

        [Test]
        public void WildProps_KeepOffTrails_AndOffTheSettlementSolids()
        {
            foreach (string id in RegionalIds)
            {
                MapLayout layout = MapLayouts.For(id);
                Assert.IsNotEmpty(layout.WildProps, id);
                foreach (MapPath path in layout.Paths)
                {
                    Assert.IsFalse(MapLayoutRules.IsFreeForProp(layout, (path.A + path.B) * 0.5f, 0.5f), $"{id}: a trail is never free for a prop");
                }

                IReadOnlyList<CitySolid> solids = layout.Town.Solids;
                foreach (CityProp prop in layout.WildProps)
                {
                    if (!layout.Bounds.Contains(prop.Position))
                    {
                        continue;
                    }

                    foreach (CitySolid solid in solids)
                    {
                        Assert.Greater(solid.DistanceTo(prop.Position), 1f, $"{id}: wild {prop.Kind} at {prop.Position} inside {solid.Owner}");
                    }
                }
            }
        }

        [Test]
        public void Rivers_BlockExceptAtCrossings()
        {
            foreach (string id in RegionalIds)
            {
                foreach (MapRiver river in MapLayouts.For(id).Rivers)
                {
                    foreach ((Vector2 center, float length, float yaw) in MapLayoutRules.RiverBlockers(river))
                    {
                        Assert.IsFalse(MapLayoutRules.OnCrossing(river, center), $"{id}: blocker on a crossing at {center}");
                    }

                    foreach (MapCrossing crossing in river.Crossings)
                    {
                        Assert.Less(MapLayoutRules.DistanceToPolyline(crossing.At, river.Points), 0.01f, $"{id}: crossing {crossing.At} on the river line");
                    }
                }
            }

            Assert.AreEqual(2, MapLayouts.For(MapLayouts.HahyaId).Rivers[0].Crossings.Count(c => c.IsBridge), "Passo do Hahya: two bridges");
        }
    }
}
