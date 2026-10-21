using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// The full WorldMap ⇄ Main encounter loop on the real scenes: roll an encounter with the map's
    /// own <see cref="RandomEncounterSystem"/> for the current region, hand it to
    /// <see cref="SceneTransitionService.BeginEncounter"/> (exactly what WorldMapController does when
    /// a step roll hits), check Main.unity's arena spawned every rolled enemy wearing its own visual,
    /// kill them all and expect to land back on the map with the victory recorded.
    /// </summary>
    [Category("Smoke")]
    public class WorldMapEncounterSmokeTests : SmokeTestBase
    {
        [UnityTest]
        [Timeout(180000)]
        public IEnumerator WorldMap_ForcedEncounter_SpawnsRolledEnemies_AndVictoryReturnsToMap()
        {
            yield return LoadScene(WorldMapSceneName);

            WorldMapController map = FindInScene<WorldMapController>();
            Assert.IsNotNull(map, "WorldMap.unity has no WorldMapController.");
            Assert.IsNotNull(map.Encounters, "WorldMapController built no RandomEncounterSystem.");

            SceneTransitionService transition = GetService<SceneTransitionService>();
            WorldMapProgress progress = GetService<WorldMapProgress>();
            Assert.AreSame(progress, map.Progress, "WorldMapController is not using the GameManager's WorldMapProgress (nothing it does would be saved).");

            // The hero is normally picked on Main.unity's character select; choosing it up front makes
            // the arena scene skip that screen (as on any re-entry) so time isn't frozen at 0.
            GetService<CharacterSelectionState>().Select(CharacterId.Lavine);

            BiomeType region = progress.CurrentRegion;
            BiomeDefinition biome = BiomeRegistry.Get(region);
            Assert.IsNotNull(biome, $"No BiomeDefinition registered for the current region {region}.");

            EncounterPayload payload = map.Encounters.BuildPayload(biome);
            Assert.IsNotNull(payload, $"Biome {region} rolled no enemies (empty/filtered enemy table).");
            Assert.Greater(payload.EnemyCount, 0);

            List<EnemyDefinition> rolled = payload.ResolveEnemies();
            Assert.AreEqual(payload.EnemyCount, rolled.Count, "Some rolled enemy ids don't resolve to EnemyDefinition assets.");

            int victoriesBefore = progress.GetVictories(region);

            var spawnedObjects = new List<GameObject>();
            System.Action<EnemySpawnedEvent> onSpawned = e => spawnedObjects.Add(e.Enemy);
            EventBus.Subscribe(onSpawned);
            try
            {
                yield return RunEncounter(transition, progress, payload, rolled, region, victoriesBefore, spawnedObjects);
            }
            finally
            {
                EventBus.Unsubscribe(onSpawned);
            }

            LogAssert.NoUnexpectedReceived();
        }

        private static IEnumerator RunEncounter(SceneTransitionService transition, WorldMapProgress progress, EncounterPayload payload,
            List<EnemyDefinition> rolled, BiomeType region, int victoriesBefore, List<GameObject> spawnedObjects)
        {
            Assert.IsTrue(transition.BeginEncounter(payload), "SceneTransitionService refused the encounter.");
            Assert.IsTrue(transition.HasPendingEncounter);

            // --- Main.unity arena ----------------------------------------------------------------
            yield return WaitUntilOrFail(() => SceneManager.GetActiveScene().name == MainSceneName && SceneManager.GetActiveScene().isLoaded,
                "Main.unity to load for the encounter", SceneLoadTimeoutSeconds);
            yield return WaitFrames(5);

            EncounterArenaController arena = null;
            yield return WaitUntilOrFail(() =>
            {
                arena = Object.FindFirstObjectByType<EncounterArenaController>();
                return arena != null && arena.IsEncounterActive;
            }, "EncounterArenaController to start the encounter");

            Assert.AreEqual(1f, Time.timeScale, "Time is frozen during the encounter (character select shown again despite a chosen hero?).");
            Assert.AreEqual(rolled.Count, arena.AliveEnemies, "Arena is tracking a different number of enemies than were rolled.");

            List<EnemyController> spawned = spawnedObjects
                .Where(go => go != null)
                .Select(go => go.GetComponent<EnemyController>())
                .Where(e => e != null)
                .ToList();
            Assert.AreEqual(rolled.Count, spawned.Count, "Arena did not publish EnemySpawnedEvent (with an EnemyController) for every rolled enemy.");

            CollectionAssert.AreEquivalent(rolled, spawned.Select(e => e.Definition).ToList(), "Spawned enemies don't carry the rolled EnemyDefinitions.");

            foreach (EnemyController enemy in spawned)
            {
                EnemyDefinition definition = enemy.Definition;
                Assert.IsTrue(enemy.gameObject.activeInHierarchy, $"Spawned '{definition.name}' is inactive.");
                Assert.IsNotNull(enemy.Health, $"Spawned '{definition.name}' has no Health.");
                Assert.IsTrue(enemy.Health.IsAlive, $"Spawned '{definition.name}' is already dead.");

                EnemyVisualSource expectedSource = EnemyVisualSwapper.ResolveSource(definition);
                if (expectedSource != EnemyVisualSource.SharedPrefab)
                {
                    EnemyVisualMarker marker = enemy.GetComponentsInChildren<EnemyVisualMarker>(true).FirstOrDefault(m => m.Source == definition);
                    Assert.IsNotNull(marker, $"Spawned '{definition.name}' is missing its own visual ({expectedSource}) — still wearing the shared prefab's model.");
                    Assert.AreEqual(expectedSource, marker.Kind);
                    Assert.IsTrue(marker.gameObject.activeInHierarchy, $"'{definition.name}' visual is inactive.");
                    Assert.IsTrue(marker.GetComponentsInChildren<Renderer>(true).Any(r => r.enabled && r.gameObject.activeInHierarchy),
                        $"'{definition.name}' visual has no enabled Renderer — the enemy is invisible.");
                }
                else
                {
                    Assert.IsTrue(enemy.GetComponentsInChildren<Renderer>(false).Any(r => r.enabled),
                        $"'{definition.name}' (shared prefab visual) has no enabled Renderer — the enemy is invisible.");
                }
            }

            // --- Win ------------------------------------------------------------------------------
            GameObject player = FindPlayer();
            foreach (EnemyController enemy in spawned)
            {
                enemy.Health.ClearInvulnerability();
                enemy.Health.ApplyDamage(1_000_000f, ElementType.Physical, player);
                Assert.IsTrue(enemy.Health.IsDead, $"'{enemy.name}' survived 1,000,000 damage.");
            }

            Assert.IsFalse(arena.IsEncounterActive, "Killing every spawned enemy did not resolve the encounter.");

            // --- Back on the map --------------------------------------------------------------------
            yield return WaitUntilOrFail(() => SceneManager.GetActiveScene().name == WorldMapSceneName && SceneManager.GetActiveScene().isLoaded,
                "the return to WorldMap.unity after victory", SceneLoadTimeoutSeconds);
            yield return WaitFrames(5);

            Assert.IsFalse(transition.HasPendingEncounter, "Encounter still pending after returning to the map.");
            Assert.AreEqual(EncounterOutcome.Victory, transition.LastOutcome, "Encounter was not resolved as a Victory.");
            Assert.AreEqual(victoriesBefore + 1, progress.GetVictories(region), $"Victory in {region} was not recorded in WorldMapProgress.");

            WorldMapController mapAgain = FindInScene<WorldMapController>();
            Assert.IsNotNull(mapAgain, "WorldMap.unity reloaded without a WorldMapController.");
            Assert.AreSame(progress, mapAgain.Progress);
        }
    }
}
