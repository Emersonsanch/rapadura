using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The maps that exist only as zones of a continuous region (09–12, 15–18, 21–24): built in the catalog, reachable by the
    /// Barcas de Luz (arrival at their post's dock inside the right overworld), the visited docks saved with the cities (old
    /// saves migrated), and the Ossuário's portal in the Grutas do Limiar starting the dungeon run and bringing the hero back.
    /// </summary>
    public class OverworldTravelTests
    {
        private static readonly string[] OverworldOnlyMaps =
        {
            "map_009_karez", "map_010_dunas", "map_011_tambor_rubro", "map_012_namrael",
            "map_015_abrigos", "map_016_pedra_seca", "map_017_napirasu", "map_018_mazans",
            "map_021_pedras_dragao", "map_022_mercado_tuneis", "map_023_canion_roda", "map_024_mahaway",
        };

        private readonly List<Object> _objects = new List<Object>();

        private sealed class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> Loaded = new List<string>();

            public void LoadScene(string sceneName) => Loaded.Add(sceneName);
        }

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
            OverworldProgress.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _objects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _objects.Clear();
            OverworldProgress.Clear();
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        // ------------------------------------------------------------------
        // Catalog and the barca
        // ------------------------------------------------------------------

        [Test]
        public void OverworldOnlyMaps_AreBuilt_AndLiveInTheirRegionsOverworld()
        {
            foreach (string id in OverworldOnlyMaps)
            {
                MapDefinition map = MapCatalog.Get(id);
                Assert.IsNotNull(map, id);
                Assert.IsTrue(map.IsBuilt, $"{id}: built (a zone of its region)");
                Assert.IsFalse(MapLayouts.Has(id), $"{id}: no hand-made layout");
                Assert.IsTrue(MapLayouts.IsPlayable(id), $"{id}: playable");
                Assert.IsTrue(MapLayouts.IsOverworldOnly(id), id);

                OverworldLayout world = OverworldLayouts.ForMap(id);
                Assert.IsNotNull(world, id);
                Assert.AreEqual(map.Region, world.Region, $"{id}: the overworld of its own region");
                Assert.IsTrue(world.ZonesOfMap(id).Any(), $"{id}: at least one zone");
            }

            CollectionAssert.IsSubsetOf(OverworldOnlyMaps, MapCatalog.Built.Select(m => m.Id).ToArray());
            Assert.IsFalse(MapCatalog.Get("map_025_taa_pwani").IsBuilt, "region 5 is not a continuous region yet");
        }

        [Test]
        public void Barca_ToAnOverworldOnlyMap_LandsAtItsPostsDock()
        {
            foreach (string id in OverworldOnlyMaps)
            {
                OverworldLayout world = OverworldLayouts.ForMap(id);
                OverworldSettlement post = world.SettlementOf(id);
                Assert.IsNotNull(post, $"{id}: its post");
                Assert.IsTrue(post.HasDock, $"{id}: the post has a Cais da Barca");
                Assert.AreEqual(id, OverworldRules.ZoneAt(world, post.Offset)?.MapIdOrOwner, $"{id}: the post stands in the map's own area");

                // Map mode's arrival (no border, no exit): the post's dock, inside the region.
                Vector2 at = MapModeController.ArrivalPoint(world, world.World, id, null, null, out Vector2 facing);
                Assert.AreEqual(post.DockArrival, at, id);
                Assert.AreEqual(post.DockArrivalFacing, facing, id);
                Assert.IsTrue(world.Bounds.Contains(at), id);
            }
        }

        [Test]
        public void Barca_WindowRow_OpensOverworldOnlyMaps_AndRefusesUnvisitedDocksOfTheSameRegion()
        {
            MapDefinition karez = MapCatalog.Get("map_009_karez");
            OverworldLayout areias = OverworldLayouts.ForMap(karez.Id);
            int level = karez.WarpLevelRequirement;

            // From the island map (or another region): open, like every built map.
            MapTravelRow fromOutside = FastTravelWindowController.BuildMapRow(karez, level, null, 9999, false, null, false, _ => true, null);
            Assert.AreEqual(MapAccessCheck.Ok, fromOutside.Check);
            Assert.IsTrue(fromOutside.CanTravel);

            // Walking the Areias, the dock of 09 was never reached on foot: closed with the padlock.
            System.Func<string, bool> unvisited = mapId => MapModeController.IsUnvisitedStop(areias, mapId);
            MapTravelRow refused = FastTravelWindowController.BuildMapRow(karez, level, null, 9999, false, "map_007_caravanas", false, _ => true, null, unvisited);
            Assert.AreEqual(MapAccessCheck.DockNotVisited, refused.Check);
            Assert.IsTrue(refused.IsLocked);
            Assert.IsFalse(refused.CanTravel);
            Assert.AreEqual("Ande até esse cais uma vez antes de pedir a barca", refused.Status);

            // Once reached, the barca links it.
            OverworldProgress.MarkVisited(karez.Id);
            MapTravelRow open = FastTravelWindowController.BuildMapRow(karez, level, null, 9999, false, "map_007_caravanas", false, _ => true, null, unvisited);
            Assert.AreEqual(MapAccessCheck.Ok, open.Check);

            // Level still comes first (a padlock for the level, not for the dock).
            MapTravelRow low = FastTravelWindowController.BuildMapRow(karez, 1, null, 9999, false, "map_007_caravanas", false, _ => true, null, unvisited);
            Assert.AreEqual(MapAccessCheck.LevelTooLow, low.Check);

            // Another region's maps are not this overworld's stops; the Vila (01) is always open.
            Assert.IsFalse(MapModeController.IsUnvisitedStop(areias, MapCatalog.TugaiId));
            Assert.IsFalse(MapModeController.IsUnvisitedStop(OverworldLayouts.ForMap(MapCatalog.PomarId), MapCatalog.PomarId));
            Assert.IsFalse(MapModeController.IsUnvisitedStop(null, karez.Id));
        }

        // ------------------------------------------------------------------
        // Save / load of the visited docks
        // ------------------------------------------------------------------

        [Test]
        public void VisitedDocks_SurviveSaveAndLoad()
        {
            var cities = new CityProgress();
            OverworldProgress.MarkVisited("map_009_karez");
            OverworldProgress.MarkVisited("map_022_mercado_tuneis");

            var state = (CitySaveState)cities.CaptureState();
            CollectionAssert.AreEqual(new[] { "map_009_karez", "map_022_mercado_tuneis" }, state.visitedDocks);
            Assert.AreEqual(CityProgress.DocksSchema, state.docksSchema);

            string json = JsonUtility.ToJson(state);
            OverworldProgress.Clear();
            Assert.IsFalse(OverworldProgress.IsVisited("map_009_karez"));

            var loaded = new CityProgress();
            loaded.RestoreState(JsonUtility.FromJson<CitySaveState>(json));
            Assert.IsTrue(OverworldProgress.IsVisited("map_009_karez"));
            Assert.IsTrue(OverworldProgress.IsVisited("map_022_mercado_tuneis"));
            Assert.IsFalse(OverworldProgress.IsVisited("map_010_dunas"));
            Assert.IsFalse(OverworldProgress.IsVisited(MapCatalog.TugaiId), "a new-schema save is not migrated: only what it lists");

            // A save replaces the docks (no leak from the session before), unknown ids are dropped.
            OverworldProgress.MarkVisited("map_010_dunas");
            var other = new CitySaveState { visitedDocks = new List<string> { "map_015_abrigos", "map_999_nope", null }, docksSchema = CityProgress.DocksSchema };
            loaded.RestoreState(other);
            CollectionAssert.AreEqual(new[] { "map_015_abrigos" }, OverworldProgress.VisitedMapIds);

            // A new game forgets them.
            loaded.ResetForNewSession();
            Assert.IsEmpty(OverworldProgress.VisitedMapIds);
            Assert.IsTrue(OverworldProgress.IsVisited(MapCatalog.PomarId), "the Vila's dock is always open");
        }

        [Test]
        public void OldSave_WithoutDocks_GetsTheVilaOfEveryVisitedCityRegion()
        {
            // A save written before the docks were saved: no visitedDocks, no docksSchema.
            string oldJson = "{\"visitedRegions\":[" + (int)BiomeType.Desert + "," + (int)BiomeType.Caves + "],\"shopIds\":[],\"shopPeriods\":[],\"bornaisBought\":0}";
            CitySaveState old = JsonUtility.FromJson<CitySaveState>(oldJson);
            Assert.AreEqual(0, old.docksSchema);
            Assert.IsEmpty(old.visitedDocks);

            OverworldProgress.MarkVisited("map_018_mazans");
            var cities = new CityProgress();
            cities.RestoreState(old);

            // Forest (the start, no map bound), Desert and Caves were reached: their Vilas' docks (01, 07 Qocho, 19 Lamparinas).
            CollectionAssert.AreEquivalent(new[] { MapCatalog.PomarId, "map_007_caravanas", "map_019_lago_limiar" }, OverworldProgress.VisitedMapIds);
            Assert.IsFalse(OverworldProgress.IsVisited("map_018_mazans"), "the loaded save replaces the session's docks");

            // Each migrated dock is the Vila (return city) of its region's overworld.
            foreach (string id in OverworldProgress.VisitedMapIds)
            {
                OverworldLayout world = OverworldLayouts.ForMap(id);
                Assert.IsNotNull(world, id);
                Assert.AreSame(world.Vila, world.SettlementOf(id), $"{id}: the region's Vila");
            }

            // Saved again, it is a new-schema save.
            Assert.AreEqual(CityProgress.DocksSchema, ((CitySaveState)cities.CaptureState()).docksSchema);
            CollectionAssert.IsEmpty(CityProgress.MigratedDocks(null));
            CollectionAssert.AreEqual(new[] { "map_013_rondaveis" }, CityProgress.MigratedDocks(new[] { BiomeType.Mountain, BiomeType.Mountain }).ToArray());
        }

        // ------------------------------------------------------------------
        // The Ossuário's portal
        // ------------------------------------------------------------------

        [Test]
        public void OssuaryPortal_IsAnExitOfTheLimiar_AndTheHeroComesBackOutOfIt()
        {
            OverworldLayout limiar = OverworldLayouts.Get(OverworldLayouts.LimiarId);
            MapExitSpec portal = limiar.World.Exits.Find(e => e.Id == MapModeController.OssuaryExitId);
            Assert.IsNotNull(portal, "the barred portal of the Ossuário");
            Assert.AreEqual(MapExitKind.WorldMap, portal.Kind);
            Assert.AreEqual(DungeonRules.Region, MapCatalog.LegacyBiomeOf(limiar.Region), "the dungeon's region is the Limiar");

            string mapThere = OverworldRules.ZoneAt(limiar, portal.Arrival)?.MapIdOrOwner;
            Assert.IsNotNull(mapThere, "the portal stands in an area of the region");
            Vector2 back = MapModeController.ArrivalPoint(limiar, limiar.World, mapThere, null, portal.Id, out Vector2 facing);
            Assert.AreEqual(portal.Arrival, back, "coming out of the run: in front of the portal");
            Assert.AreEqual(portal.ArrivalFacing, facing);
        }

        [Test]
        public void DungeonEnteredFromAMap_ReturnsToThePortal_UnlessDefeated()
        {
            var loader = new FakeSceneLoader();
            var transitions = new SceneTransitionService(new WorldMapProgress(BiomeType.Forest), loader);

            Assert.IsTrue(transitions.BeginEncounterFromMap(DungeonPayload(), "map_020_tages", MapModeController.OssuaryExitId));
            Assert.AreEqual("map_020_tages", transitions.EncounterReturnMap);
            Assert.IsFalse(transitions.HasPendingMapVisit, "the map visit pauses during the run");
            Assert.IsTrue(transitions.CompleteEncounter(EncounterOutcome.Victory));
            Assert.AreEqual(transitions.CombatSceneName, loader.Loaded.Last(), "back to map mode, not the island map");
            Assert.AreEqual("map_020_tages", transitions.PendingMapVisit);
            Assert.AreEqual(MapModeController.OssuaryExitId, transitions.PendingMapArrivalExit);
            Assert.IsNull(transitions.EncounterReturnMap);

            // Walking out (fled) also comes back to the portal.
            Assert.IsTrue(transitions.BeginEncounterFromMap(DungeonPayload(), "map_020_tages", MapModeController.OssuaryExitId));
            Assert.IsNull(transitions.PendingMapArrivalExit, "a new encounter clears the old arrival");
            Assert.IsTrue(transitions.CompleteEncounter(EncounterOutcome.Fled));
            Assert.AreEqual("map_020_tages", transitions.PendingMapVisit);

            // A defeat goes the usual way (the island map, then the return city).
            Assert.IsTrue(transitions.BeginEncounterFromMap(DungeonPayload(), "map_020_tages", MapModeController.OssuaryExitId));
            Assert.IsTrue(transitions.CompleteEncounter(EncounterOutcome.Defeat));
            Assert.AreEqual(transitions.WorldMapSceneName, loader.Loaded.Last());
            Assert.IsFalse(transitions.HasPendingMapVisit);

            // The island map's button (plain BeginEncounter) still ends on the island map.
            Assert.IsTrue(transitions.BeginEncounter(DungeonPayload()));
            Assert.IsTrue(transitions.CompleteEncounter(EncounterOutcome.Victory));
            Assert.AreEqual(transitions.WorldMapSceneName, loader.Loaded.Last());
        }

        private EncounterPayload DungeonPayload()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = "enemy_test_ossuary_guard";
            _objects.Add(enemy);
            EncounterPayload payload = EncounterPayload.Create(DungeonRules.Region, new[] { enemy }, seed: 7);
            payload.dungeonId = DungeonRules.OssuaryId;
            return payload;
        }
    }
}
