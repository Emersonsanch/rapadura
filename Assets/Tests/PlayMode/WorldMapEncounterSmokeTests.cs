using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Managers;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Player;
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

            // Pick the hero the way the game does: on WorldMap.unity, through the title flow when
            // GameFrontEndController is in the scene (Novo jogo -> hero -> map; the map controller
            // stays disabled, not even Started, until then), else directly. Choosing it up front also
            // makes the arena scene skip its own select screen, so time is not frozen there.
            // SelectHero also re-enables the picker (if WorldMap has one) so it closes itself and
            // releases its GameplayPause — the picker doesn't react to a hero chosen through the API.
            GameFlowService flow = GetService<GameFlowService>();
            yield return SelectHero(CharacterId.Lavine, requireCharacterSelect: false);
            Assert.AreEqual(GameFlowState.World, flow.State, "Choosing a hero did not hand control to the world map.");
            Assert.AreEqual(1f, Time.timeScale, "World map still paused after the hero was chosen.");

            WorldMapController map = FindInScene<WorldMapController>();
            Assert.IsNotNull(map, "WorldMap.unity has no WorldMapController.");
            yield return WaitUntilOrFail(() => map.isActiveAndEnabled && map.Progress != null,
                "WorldMapController to be enabled and started once a hero is chosen");
            Assert.IsNotNull(map.Encounters, "WorldMapController built no RandomEncounterSystem.");

            SceneTransitionService transition = GetService<SceneTransitionService>();
            WorldMapProgress progress = GetService<WorldMapProgress>();
            Assert.AreSame(progress, map.Progress, "WorldMapController is not using the GameManager's WorldMapProgress (nothing it does would be saved).");

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

            // Unexpected Error/Exception logs already fail the test (LogAssert default).
        }

        /// <summary>
        /// Regression: Main.unity's GameManager object also carries HitStopController, CombatFxManager,
        /// EnemyRewardGranter, EnemyDeathVfx and EnemyLootGranter. Reached from WorldMap (whose
        /// GameManager persists), SingletonBehaviour used to Destroy that whole duplicate object — so
        /// encounters granted no XP/loot and had no damage numbers, death VFX or hit-stop. Starts from
        /// WorldMap through the real flow, kills an enemy and expects the XP to arrive.
        /// </summary>
        [UnityTest]
        [Timeout(180000)]
        public IEnumerator WorldMap_Encounter_KillGrantsExperience_SceneCombatServicesSurviveDuplicateGameManager()
        {
            yield return LoadScene(WorldMapSceneName);
            GameManager persistent = GameManager.Instance;
            Assert.IsNotNull(persistent, "WorldMap.unity has no GameManager.");

            yield return SelectHero(CharacterId.Lavine, requireCharacterSelect: false);

            WorldMapController map = FindInScene<WorldMapController>();
            Assert.IsNotNull(map, "WorldMap.unity has no WorldMapController.");
            yield return WaitUntilOrFail(() => map.isActiveAndEnabled && map.Progress != null,
                "WorldMapController to be enabled and started once a hero is chosen");

            SceneTransitionService transition = GetService<SceneTransitionService>();
            BiomeDefinition biome = BiomeRegistry.Get(map.Progress.CurrentRegion);
            Assert.IsNotNull(biome);
            EncounterPayload payload = map.Encounters.BuildPayload(biome);
            Assert.IsNotNull(payload, "Current region rolled no enemies.");

            var spawnedObjects = new List<GameObject>();
            int experienceEvents = 0;
            System.Action<EnemySpawnedEvent> onSpawned = e => spawnedObjects.Add(e.Enemy);
            System.Action<PlayerExperienceChangedEvent> onExperience = _ => experienceEvents++;
            EventBus.Subscribe(onSpawned);
            EventBus.Subscribe(onExperience);
            try
            {
                Assert.IsTrue(transition.BeginEncounter(payload), "SceneTransitionService refused the encounter.");
                yield return WaitUntilOrFail(() => SceneManager.GetActiveScene().name == MainSceneName && SceneManager.GetActiveScene().isLoaded,
                    "Main.unity to load for the encounter", SceneLoadTimeoutSeconds);
                yield return WaitFrames(5);

                EncounterArenaController arena = null;
                yield return WaitUntilOrFail(() =>
                {
                    arena = Object.FindFirstObjectByType<EncounterArenaController>();
                    return arena != null && arena.IsEncounterActive;
                }, "EncounterArenaController to start the encounter");

                // Main's duplicate GameManager is gone, the persistent one (and its services) is not.
                Assert.IsTrue(persistent != null, "The persistent GameManager was destroyed when Main.unity loaded.");
                Assert.AreSame(persistent, GameManager.Instance);
                Assert.AreEqual(1, FindAllInScene<GameManager>().Length, "Main.unity's duplicate GameManager component was not removed.");
                Assert.IsTrue(Rapadura.Core.DI.ServiceLocator.TryGet(out Rapadura.Save.SaveManager _), "Removing the duplicate GameManager tore down the live services.");

                // ...but the per-scene combat components sharing its GameObject survived.
                Assert.IsNotNull(FindInScene<EnemyRewardGranter>(), "EnemyRewardGranter died with Main.unity's duplicate GameManager.");
                Assert.IsTrue(FindInScene<EnemyRewardGranter>().isActiveAndEnabled);
                Assert.IsNotNull(FindInScene<EnemyLootGranter>(), "EnemyLootGranter died with Main.unity's duplicate GameManager.");
                Assert.IsNotNull(FindInScene<EnemyDeathVfx>(), "EnemyDeathVfx died with Main.unity's duplicate GameManager.");
                Assert.IsNotNull(FindInScene<HitStopController>(), "HitStopController died with Main.unity's duplicate GameManager.");
                Assert.IsNotNull(FindInScene<CombatFxManager>(), "CombatFxManager died with Main.unity's duplicate GameManager.");

                EnemyController target = spawnedObjects
                    .Where(go => go != null)
                    .Select(go => go.GetComponent<EnemyController>())
                    .FirstOrDefault(e => e != null && e.Definition != null && BossBonusRules.GetExperienceReward(e.Definition) > 0);
                Assert.IsNotNull(target, "No spawned enemy grants experience.");

                GameObject player = FindPlayer();
                PlayerStats stats = player.GetComponent<PlayerStats>();
                Assert.IsNotNull(stats);
                int levelBefore = stats.Level;
                int experienceBefore = stats.CurrentExperience;
                experienceEvents = 0;

                target.Health.ClearInvulnerability();
                target.Health.ApplyDamage(1_000_000f, ElementType.Physical, player);
                Assert.IsTrue(target.Health.IsDead);
                yield return WaitFrames(2);

                Assert.Greater(experienceEvents, 0, "Killing an enemy published no PlayerExperienceChangedEvent (EnemyRewardGranter not listening).");
                Assert.IsTrue(stats.Level > levelBefore || stats.CurrentExperience > experienceBefore,
                    $"Killing '{target.Definition.name}' granted no experience (level {levelBefore}->{stats.Level}, xp {experienceBefore}->{stats.CurrentExperience}).");
            }
            finally
            {
                EventBus.Unsubscribe(onSpawned);
                EventBus.Unsubscribe(onExperience);
            }
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
