using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Drives enemy wave spawning through a pooled prefab. Publishes <see cref="WaveStartedEvent"/> when a
    /// wave begins and <see cref="WaveCompletedEvent"/> once every enemy spawned in that wave has died
    /// (tracked via <see cref="EnemyDiedEvent"/>), following the same EventBus-driven decoupling used
    /// elsewhere in the project (see CombatEvents/GameEvents).
    /// <para>
    /// A dead enemy counts towards the wave at once, but goes back to the pool only once its corpse is
    /// gone: <see cref="EnemyDeathVfx"/> sinks/fades it and deactivates it when done, so the release
    /// waits for that (or <see cref="_corpseReleaseTimeout"/> when nothing deactivates it). Releasing on
    /// the death event itself parked — SetActive(false) — the enemy instantly, skipping the death sink.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(EnemyPool))]
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private EnemyDefinition _bossDefinition;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private Transform[] _waypoints;
        [Tooltip("Swap each spawned enemy's visual to match its EnemyDefinition (model, else placeholder). See EnemyVisualSwapper.")]
        [SerializeField] private bool _applyDefinitionVisuals = true;
        [Tooltip("Longest a dead enemy waits for its death sink/fade (EnemyDeathVfx deactivates the corpse when done) before going back to the pool. Keep above EnemyDeathVfx's sink duration.")]
        [SerializeField] [Min(0f)] private float _corpseReleaseTimeout = 2f;

        private readonly struct PendingCorpse
        {
            public readonly GameObject Enemy;
            public readonly float ReleaseAt;

            public PendingCorpse(GameObject enemy, float releaseAt)
            {
                Enemy = enemy;
                ReleaseAt = releaseAt;
            }
        }

        private readonly List<PendingCorpse> _pendingCorpses = new List<PendingCorpse>();

        private EnemyPool _pool;
        private readonly WaveTracker _waveTracker = new WaveTracker();
        private readonly List<GameObject> _spawnedThisWave = new List<GameObject>();
        private int _nextWaveIndex;

        public WaveTracker CurrentWave => _waveTracker;

        /// <summary>Dead enemies waiting for their death sink to finish before going back to the pool.</summary>
        public int PendingCorpseCount => _pendingCorpses.Count;

        private void Awake()
        {
            _pool = GetComponent<EnemyPool>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            // Nothing will tick the pending list while disabled; don't strand corpses out of the pool.
            ReleaseFinishedCorpses(float.PositiveInfinity);
        }

        private void Update()
        {
            if (_pendingCorpses.Count > 0)
            {
                ReleaseFinishedCorpses(Time.time);
            }
        }

        /// <summary>
        /// Returns to the pool every pending corpse whose death sink is over (deactivated by
        /// <see cref="EnemyDeathVfx"/>) or whose timeout passed at <paramref name="now"/>. Destroyed ones
        /// are dropped. Called from Update; public so it can be driven directly.
        /// </summary>
        public void ReleaseFinishedCorpses(float now)
        {
            for (int i = _pendingCorpses.Count - 1; i >= 0; i--)
            {
                PendingCorpse corpse = _pendingCorpses[i];
                if (corpse.Enemy == null)
                {
                    _pendingCorpses.RemoveAt(i);
                    continue;
                }

                if (corpse.Enemy.activeSelf && now < corpse.ReleaseAt)
                {
                    continue;
                }

                _pendingCorpses.RemoveAt(i);
                if (_pool != null)
                {
                    _pool.Release(corpse.Enemy);
                }
            }
        }

        /// <summary>Spawns a regular wave of <paramref name="enemyCount"/> enemies distributed across the configured spawn points.</summary>
        public void StartWave(int enemyCount)
        {
            if (_pool == null || _definition == null || enemyCount <= 0)
            {
                GameLogger.Warning("Enemy", "EnemySpawner.StartWave called without a valid pool/definition/count.");
                return;
            }

            int waveIndex = _nextWaveIndex++;
            _waveTracker.StartWave(waveIndex, enemyCount);
            _spawnedThisWave.Clear();

            EventBus.Publish(new WaveStartedEvent(waveIndex, enemyCount));

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnOne(_definition, i);
            }
        }

        /// <summary>
        /// Spawns a single boss-configured enemy as its own one-enemy "wave". Returns null (no wave
        /// started) if that unique boss was already defeated according to <see cref="DefeatedEnemyRegistry"/>.
        /// </summary>
        public GameObject SpawnBoss()
        {
            if (_pool == null || _bossDefinition == null)
            {
                GameLogger.Warning("Enemy", "EnemySpawner.SpawnBoss called without a valid pool/boss definition.");
                return null;
            }

            if (ServiceLocator.TryGet(out DefeatedEnemyRegistry defeated) && defeated.ShouldSkipSpawn(_bossDefinition))
            {
                GameLogger.Info("Enemy", $"Boss '{_bossDefinition.name}' was already defeated; not respawning it.");
                return null;
            }

            int waveIndex = _nextWaveIndex++;
            _waveTracker.StartWave(waveIndex, 1);
            _spawnedThisWave.Clear();

            EventBus.Publish(new WaveStartedEvent(waveIndex, 1));

            return SpawnOne(_bossDefinition, 0);
        }

        private GameObject SpawnOne(EnemyDefinition definition, int spawnIndex)
        {
            Vector3 position = GetSpawnPosition(spawnIndex);
            // Spawn = Get + ResetForSpawn (health/AI/aggro hold) + visual swap, so a reused pooled
            // enemy always comes back as a fresh one.
            GameObject instance = _pool.Spawn(definition, position, Quaternion.identity, _waypoints, _applyDefinitionVisuals);
            if (instance == null)
            {
                GameLogger.Warning("Enemy", "EnemySpawner: the pool has no prefab configured; nothing spawned.");
                return null;
            }

            _spawnedThisWave.Add(instance);
            EventBus.Publish(new EnemySpawnedEvent(instance));
            return instance;
        }

        private Vector3 GetSpawnPosition(int index)
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                return transform.position;
            }

            Transform point = _spawnPoints[index % _spawnPoints.Length];
            return point != null ? point.position : transform.position;
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (!_spawnedThisWave.Remove(evt.Enemy))
            {
                return;
            }

            _pendingCorpses.Add(new PendingCorpse(evt.Enemy, Time.time + _corpseReleaseTimeout));

            bool waveJustCompleted = _waveTracker.RegisterDeath();
            if (waveJustCompleted)
            {
                EventBus.Publish(new WaveCompletedEvent(_waveTracker.WaveIndex));
            }
        }
    }
}
