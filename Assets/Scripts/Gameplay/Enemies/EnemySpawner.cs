using System.Collections.Generic;
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
    /// </summary>
    [RequireComponent(typeof(EnemyPool))]
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private EnemyDefinition _bossDefinition;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private Transform[] _waypoints;

        private EnemyPool _pool;
        private readonly WaveTracker _waveTracker = new WaveTracker();
        private readonly List<GameObject> _spawnedThisWave = new List<GameObject>();
        private int _nextWaveIndex;

        public WaveTracker CurrentWave => _waveTracker;

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

        /// <summary>Spawns a single boss-configured enemy as its own one-enemy "wave".</summary>
        public GameObject SpawnBoss()
        {
            if (_pool == null || _bossDefinition == null)
            {
                GameLogger.Warning("Enemy", "EnemySpawner.SpawnBoss called without a valid pool/boss definition.");
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
            GameObject instance = _pool.Get(position, Quaternion.identity);

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetForSpawn(definition, position, _waypoints);
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

            if (_pool != null)
            {
                _pool.Release(evt.Enemy);
            }

            bool waveJustCompleted = _waveTracker.RegisterDeath();
            if (waveJustCompleted)
            {
                EventBus.Publish(new WaveCompletedEvent(_waveTracker.WaveIndex));
            }
        }
    }
}
