using System.Collections;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Lives in the combat scene (Main.unity). When the scene was entered from the world map with a
    /// pending <see cref="EncounterPayload"/> (see <see cref="SceneTransitionService"/>), it moves the
    /// player to the arena spawn, spawns each rolled enemy from <see cref="_enemyPrefab"/> at the
    /// configured spawn points (reusing <see cref="EnemyController.ResetForSpawn"/>, so combat is the
    /// existing real-time Hitbox/Health/SkillManager loop) and reports the outcome back:
    /// all spawned enemies dead → Victory, player dead → Defeat, player walks out of
    /// <see cref="_fleeRadius"/> (or <see cref="Flee"/> is called from UI) → Fled.
    ///
    /// Without a pending encounter (Main.unity played directly) it does nothing, so the existing
    /// standalone combat sandbox keeps working.
    /// </summary>
    public class EncounterArenaController : MonoBehaviour
    {
        private const string LogCategory = "EncounterArena";
        private const string PlayerTag = "Player";

        [Header("Spawning")]
        [Tooltip("Enemy prefab with EnemyController + Health (+ Hitbox). Reconfigured per rolled EnemyDefinition; its visual is swapped by EnemyVisualSwapper (definition model > placeholder > this prefab's own).")]
        [SerializeField] private GameObject _enemyPrefab;
        [Tooltip("Where rolled enemies appear (cycled if there are more enemies than points). Falls back to this transform.")]
        [SerializeField] private Transform[] _enemySpawnPoints = new Transform[0];
        [Tooltip("Optional patrol waypoints handed to spawned enemies.")]
        [SerializeField] private Transform[] _enemyWaypoints;
        [Tooltip("Optional: where the player is placed when the encounter starts.")]
        [SerializeField] private SpawnPoint _playerSpawn;

        [Header("Scene cleanup")]
        [Tooltip("Objects disabled while an encounter is active (e.g. the sandbox EnemySpawner or pre-placed test enemies).")]
        [SerializeField] private GameObject[] _disableDuringEncounter = new GameObject[0];

        [Header("Resolution")]
        [Tooltip("Player farther than this from the arena center (this transform) counts as fleeing. 0 disables distance fleeing.")]
        [SerializeField] [Min(0f)] private float _fleeRadius = 30f;
        [Tooltip("Seconds to wait after the outcome before returning to the map (lets death VFX/loot/XP play out).")]
        [SerializeField] [Min(0f)] private float _returnDelaySeconds = 1.5f;

        private readonly WaveTracker _tracker = new WaveTracker();
        private readonly HashSet<GameObject> _spawned = new HashSet<GameObject>();
        private SceneTransitionService _transition;
        private Transform _player;
        private bool _active;
        private bool _resolved;

        public bool IsEncounterActive => _active && !_resolved;
        public int AliveEnemies => _tracker.AliveCount;

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _transition) || !_transition.HasPendingEncounter)
            {
                return;
            }

            BeginArena(_transition.PendingEncounter);
        }

        private void Update()
        {
            if (!IsEncounterActive || _fleeRadius <= 0f || _player == null)
            {
                return;
            }

            Vector3 offset = _player.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > _fleeRadius * _fleeRadius)
            {
                Resolve(EncounterOutcome.Fled);
            }
        }

        /// <summary>Gives up on the current encounter (e.g. a "Fugir" HUD button).</summary>
        public void Flee()
        {
            Resolve(EncounterOutcome.Fled);
        }

        private void BeginArena(EncounterPayload payload)
        {
            foreach (GameObject toDisable in _disableDuringEncounter)
            {
                if (toDisable != null)
                {
                    toDisable.SetActive(false);
                }
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
            _player = playerObject != null ? playerObject.transform : null;

            if (_player != null && _playerSpawn != null)
            {
                MovePlayer(_player, _playerSpawn);
            }

            if (_enemyPrefab == null)
            {
                GameLogger.Error(LogCategory, "No enemy prefab assigned; cannot spawn the encounter. Returning to the map.");
                _active = true;
                Resolve(EncounterOutcome.Fled);
                return;
            }

            List<EnemyDefinition> enemies = payload.ResolveEnemies();
            FilterDefeatedUniqueEnemies(enemies);
            for (int i = 0; i < enemies.Count; i++)
            {
                SpawnEnemy(enemies[i], i);
            }

            _active = true;
            _tracker.StartWave(0, _spawned.Count);

            if (_spawned.Count == 0)
            {
                GameLogger.Warning(LogCategory, $"Encounter in {payload.biome} resolved to zero enemies; returning to the map.");
                Resolve(EncounterOutcome.Fled);
            }
        }

        private void SpawnEnemy(EnemyDefinition definition, int index)
        {
            Vector3 position = GetSpawnPosition(index);
            GameObject instance = Instantiate(_enemyPrefab, position, Quaternion.identity);
            instance.name = $"{_enemyPrefab.name}_{definition.name}_{index}";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetForSpawn(definition, position, _enemyWaypoints);
                if (_player != null)
                {
                    controller.Target = _player;
                }
            }
            else
            {
                GameLogger.Warning(LogCategory, $"Enemy prefab '{_enemyPrefab.name}' has no EnemyController; '{definition.name}' will not fight back.");
            }

            // The shared prefab only provides behaviour; the look comes from the definition
            // (its model, else a per-enemy placeholder, else the prefab's own visual).
            EnemyVisualSwapper.Apply(instance, definition);

            _spawned.Add(instance);
            EventBus.Publish(new EnemySpawnedEvent(instance));
        }

        /// <summary>
        /// Drops unique enemies already in <see cref="DefeatedEnemyRegistry"/> (the map's rolls are
        /// filtered too, but a payload can be stale or come from JSON). No registry = no filtering.
        /// </summary>
        private static void FilterDefeatedUniqueEnemies(List<EnemyDefinition> enemies)
        {
            if (!ServiceLocator.TryGet(out DefeatedEnemyRegistry defeated))
            {
                return;
            }

            int removed = enemies.RemoveAll(defeated.ShouldSkipSpawn);
            if (removed > 0)
            {
                GameLogger.Info(LogCategory, $"Skipped {removed} already-defeated boss(es) from the encounter.");
            }
        }

        private Vector3 GetSpawnPosition(int index)
        {
            if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
            {
                return transform.position;
            }

            Transform point = _enemySpawnPoints[index % _enemySpawnPoints.Length];
            return point != null ? point.position : transform.position;
        }

        private static void MovePlayer(Transform player, SpawnPoint spawn)
        {
            // CharacterController overrides transform writes while enabled, so toggle it around the teleport.
            CharacterController characterController = player.GetComponent<CharacterController>();
            bool wasEnabled = characterController != null && characterController.enabled;
            if (wasEnabled)
            {
                characterController.enabled = false;
            }

            player.SetPositionAndRotation(spawn.Position, spawn.Rotation);

            if (wasEnabled)
            {
                characterController.enabled = true;
            }
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (!IsEncounterActive || !_spawned.Contains(evt.Enemy))
            {
                return;
            }

            if (_tracker.RegisterDeath())
            {
                Resolve(EncounterOutcome.Victory);
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (IsEncounterActive)
            {
                Resolve(EncounterOutcome.Defeat);
            }
        }

        private void Resolve(EncounterOutcome outcome)
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;
            GameLogger.Info(LogCategory, $"Encounter resolved: {outcome}.");

            if (_returnDelaySeconds > 0f && isActiveAndEnabled)
            {
                StartCoroutine(CompleteAfterDelay(outcome));
            }
            else
            {
                _transition?.CompleteEncounter(outcome);
            }
        }

        private IEnumerator CompleteAfterDelay(EncounterOutcome outcome)
        {
            yield return new WaitForSeconds(_returnDelaySeconds);
            _transition?.CompleteEncounter(outcome);
        }
    }
}
