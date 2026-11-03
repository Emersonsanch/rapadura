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
    /// The Zarafshan maps after 01–02 (Docs/Mapas100.md: 03 Brejo do Lodo Escuro, 04 Bosque das Amoreiras de Seda,
    /// 05 Copas de Garajonay, 06 Jardim Esquecido de Mihrshah): same invariants as <see cref="MapLayoutsRegionTests"/>
    /// (catalog, layout rules, level band, existing non-boss monsters with a night table, safe settlement, exits that lead
    /// back) plus the walk chain from the Tugai, the race city of the elfos-da-mata and its canopy, and the walled garden.
    /// </summary>
    public class MapLayoutsZarafshanTests
    {
        private static IEnumerable<string> Ids => MapLayouts.ZarafshanMapIds;

        [SetUp]
        public void SetUp() => MapLayouts.ClearCache();

        [TearDown]
        public void TearDown() => MapLayouts.ClearCache();

        [Test]
        public void TheFourZarafshanMaps_HaveALayout_AndAreBuiltInTheCatalog()
        {
            CollectionAssert.AreEqual(new[] { "map_003_brejo", "map_004_amoreiras", "map_005_garajonay", "map_006_mihrshah" }, Ids.ToArray());
            foreach (string id in Ids)
            {
                MapDefinition map = MapCatalog.Get(id);
                Assert.IsNotNull(map, $"{id}: in MapCatalog");
                Assert.IsTrue(map.IsBuilt, $"{id}: built");
                Assert.IsTrue(MapLayouts.Has(id), $"{id}: Has");
                MapLayout layout = MapLayouts.For(id);
                Assert.IsNotNull(layout, id);
                Assert.AreEqual(id, layout.MapId);
                Assert.AreSame(layout, MapLayouts.For(id), "cached");
                Assert.AreEqual(map.LegacyBiome, layout.Biome, $"{id}: the region's biome");
                Assert.AreEqual(map.SettlementId, layout.Town.CityId, $"{id}: the catalog's settlement");
            }
        }

        [Test]
        public void TheFourZarafshanMaps_PassTheLayoutRules()
        {
            foreach (string id in Ids)
            {
                List<string> problems = MapLayoutRules.Validate(MapLayouts.For(id));
                Assert.IsEmpty(problems, string.Join("\n", problems));
            }
        }

        [Test]
        public void Spawns_StayInsideTheCatalogBand_AndCoverItsEnds()
        {
            foreach (string id in Ids)
            {
                MapDefinition map = MapCatalog.Get(id);
                MapLayout layout = MapLayouts.For(id);
                Assert.GreaterOrEqual(layout.Zones.Count, map.Size == MapSize.G ? 6 : 4, $"{id}: hunting clearings for its size");
                foreach (MapSpawnZone zone in layout.Zones)
                {
                    Assert.GreaterOrEqual(zone.MinLevel, map.MonsterLevelMin, $"{id}/{zone.Id}");
                    Assert.LessOrEqual(zone.MaxLevel, map.MonsterLevelMax, $"{id}/{zone.Id}");
                    Assert.LessOrEqual(zone.MinLevel, zone.MaxLevel, $"{id}/{zone.Id}");
                    Assert.IsFalse(string.IsNullOrEmpty(zone.Archetypes), $"{id}/{zone.Id}: archetype tags");
                    Assert.IsTrue(layout.Bounds.Contains(zone.Center), $"{id}/{zone.Id}: inside its own bounds");
                }

                Assert.AreEqual(map.MonsterLevelMin, layout.Zones.Min(z => z.MinLevel), $"{id}: some clearing starts at the band's floor");
                Assert.AreEqual(map.MonsterLevelMax, layout.Zones.Max(z => z.MaxLevel), $"{id}: some clearing reaches the band's top");
            }
        }

        [Test]
        public void Spawns_AreExistingNonBossMonsters_WithANightTable()
        {
            foreach (string id in Ids)
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
            foreach (string id in Ids)
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
        public void TheRaceCity_IsTheReturnCityOfItsMap_TheOutpostsAreNot()
        {
            foreach (string id in Ids)
            {
                MapDefinition map = MapCatalog.Get(id);
                MapLayout layout = MapLayouts.For(id);
                bool fullCity = map.Settlement == MapSettlementKind.City || map.Settlement == MapSettlementKind.RaceCity || map.Settlement == MapSettlementKind.Capital;
                Assert.AreEqual(fullCity, layout.SettlementIsReturnCity, $"{id}: only a city map respawns the hero in it");
                if (layout.SettlementIsReturnCity)
                {
                    Assert.AreEqual(map.ReturnCityId, layout.Town.CityId, $"{id}: the map's town is its return city");
                }
            }

            Assert.IsTrue(MapLayouts.For(MapLayouts.GarajonayId).SettlementIsReturnCity, "Copas de Garajonay: the elves' city");
        }

        [Test]
        public void Exits_LeadToBuiltMaps_ThatLeadBack_AndTheWalkChainStartsAtTheTugai()
        {
            foreach (string id in Ids)
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

            string[] chain = { MapCatalog.TugaiId, MapLayouts.BrejoId, MapLayouts.AmoreirasId, MapLayouts.GarajonayId, MapLayouts.MihrshahId };
            for (int i = 0; i + 1 < chain.Length; i++)
            {
                Assert.IsTrue(MapLayouts.For(chain[i]).Exits.Exists(e => e.Kind == MapExitKind.Map && e.TargetMapId == chain[i + 1]), $"{chain[i]} → {chain[i + 1]}");
                Assert.IsTrue(MapLayouts.For(chain[i + 1]).Exits.Exists(e => e.Kind == MapExitKind.Map && e.TargetMapId == chain[i]), $"{chain[i + 1]} → {chain[i]}");
            }

            Assert.IsEmpty(MapLayoutRules.Validate(MapLayouts.For(MapCatalog.TugaiId)), "the Tugai with its new south trail");
        }

        [Test]
        public void WildProps_KeepOffTrails_AndOffTheSettlementSolids()
        {
            foreach (string id in Ids)
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
        public void Water_BlocksExceptAtTheWalkways()
        {
            foreach (string id in Ids)
            {
                foreach (MapRiver river in MapLayouts.For(id).Rivers)
                {
                    Assert.IsNotEmpty(river.Crossings, $"{id}: every channel can be crossed");
                    foreach ((Vector2 center, float length, float yaw) in MapLayoutRules.RiverBlockers(river))
                    {
                        Assert.IsFalse(MapLayoutRules.OnCrossing(river, center), $"{id}: blocker on a crossing at {center}");
                    }

                    foreach (MapCrossing crossing in river.Crossings)
                    {
                        Assert.Less(MapLayoutRules.DistanceToPolyline(crossing.At, river.Points), 0.01f, $"{id}: crossing {crossing.At} on the water line");
                        Assert.IsTrue(MapLayoutRules.OnPath(MapLayouts.For(id), crossing.At, 0f), $"{id}: crossing {crossing.At} carries a trail");
                    }
                }
            }

            Assert.AreEqual(3, MapLayouts.For(MapLayouts.BrejoId).Rivers.Sum(r => r.Crossings.Count(c => c.IsBridge)), "Brejo: three log walkways");
            Assert.AreEqual(2, MapLayouts.For(MapLayouts.GarajonayId).Rivers.Count, "Garajonay: two cascading streams");
        }

        [Test]
        public void Garajonay_HasItsCanopy_PlatformsLinkedByRopeBridges_SolidOnTheGround()
        {
            CityLayout town = MapLayouts.For(MapLayouts.GarajonayId).Town;
            List<CityProp> trees = town.Props.Where(p => p.Kind == CityPropKind.CanopyTree).ToList();
            List<CityProp> bridges = town.Props.Where(p => p.Kind == CityPropKind.RopeBridge).ToList();
            Assert.GreaterOrEqual(trees.Count, 4, "giant laurels with platforms");
            Assert.AreEqual(trees.Count - 1, bridges.Count, "a rope bridge between neighbouring platforms");
            foreach (CityProp bridge in bridges)
            {
                Assert.AreEqual(trees[0].Scale, bridge.Scale, 1e-4f, "bridges at the platforms' height");
                float rim = TownKit.CanopyPlatformRadius * bridge.Scale;
                Assert.IsTrue(trees.Any(t => Vector2.Distance(t.Position, bridge.Position) <= rim), $"bridge start {bridge.Position} lands on a platform");
                Assert.IsTrue(trees.Any(t => Vector2.Distance(t.Position, bridge.End) <= rim), $"bridge end {bridge.End} lands on a platform");
            }

            foreach (CityProp tree in trees)
            {
                Assert.IsTrue(town.Solids.Any(s => s.Owner == nameof(CityPropKind.CanopyTree) && s.DistanceTo(tree.Position) <= 0f), $"canopy tree at {tree.Position} is solid");
                foreach (CityStreet street in town.Streets)
                {
                    Assert.Greater(CityGeometry.DistanceToRect(tree.Position, street.Bounds), TownKit.CanopyTrunkSolidRadius * tree.Scale, $"canopy tree at {tree.Position} stands off the street");
                }
            }

            var mesh = new TownMeshBuilder();
            TownKit.Prop(mesh, trees[0], town.Style);
            Assert.Greater(mesh.TriangleCount, 200, "the canopy tree is built");
            Assert.Less(mesh.TriangleCount, 2500, "the canopy tree fits the prop budget");
        }

        [Test]
        public void Mihrshah_IsAWalledChaharBagh_WithFourQuadrants_AndThePavilion()
        {
            MapLayout layout = MapLayouts.For(MapLayouts.MihrshahId);
            List<CityProp> props = layout.Town.Props;
            Assert.AreEqual(8, props.Count(p => p.Kind == CityPropKind.FenceRun && p.Scale > 2f), "the garden wall, four sides with a gate each");
            Assert.AreEqual(4, props.Count(p => p.Kind == CityPropKind.KarezChannel), "dry canals in a cross");
            Assert.AreEqual(8, props.Count(p => p.Kind == CityPropKind.Ruin), "the pavilion of columns");
            Assert.AreEqual(4, layout.Zones.Count, "one quadrant per spark");
            Assert.IsTrue(layout.Clearings.Contains(new Vector2(80f, 0f)), "the pavilion clearing (Saklas's arena)");
        }
    }
}
