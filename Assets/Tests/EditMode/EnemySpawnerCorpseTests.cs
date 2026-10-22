using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="EnemySpawner"/>'s corpse handling: a dead pooled enemy counts for
    /// the wave immediately but is only parked (SetActive(false)) once <see cref="EnemyDeathVfx"/>'s
    /// sink/fade deactivated it, or after the timeout — releasing it on the death event itself cut the
    /// death animation.
    /// </summary>
    public class EnemySpawnerCorpseTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void Invoke(object target, string method, params object[] args)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        }

        private EnemySpawner MakeSpawner(out EnemyPool pool)
        {
            var prefab = new GameObject("EnemyPrefab_CorpseTest");
            prefab.SetActive(false);
            prefab.AddComponent<Health>();
            prefab.AddComponent<EnemyController>();
            _created.Add(prefab);

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.name = "enemy_corpse_test";
            _created.Add(definition);

            var spawnerObject = new GameObject("Spawner_CorpseTest");
            _created.Add(spawnerObject);
            EnemySpawner spawner = spawnerObject.AddComponent<EnemySpawner>();
            pool = spawnerObject.GetComponent<EnemyPool>();
            pool.Configure(prefab);
            SetField(spawner, "_definition", definition);
            SetField(spawner, "_applyDefinitionVisuals", false);
            SetField(spawner, "_corpseReleaseTimeout", 2f);

            // EditMode doesn't run Awake for non-ExecuteAlways components; it only caches the pool.
            Invoke(spawner, "Awake");
            return spawner;
        }

        private static GameObject TheOnlyLiveEnemy(EnemyPool pool)
        {
            foreach (EnemyController controller in pool.GetComponentsInChildren<EnemyController>(true))
            {
                if (controller.gameObject.activeSelf)
                {
                    return controller.gameObject;
                }
            }

            return null;
        }

        [Test]
        public void EnemyDied_CountsForTheWaveAtOnce_ButKeepsTheCorpseOutOfThePoolWhileItSinks()
        {
            EnemySpawner spawner = MakeSpawner(out EnemyPool pool);
            spawner.StartWave(1);
            GameObject enemy = TheOnlyLiveEnemy(pool);
            Assert.IsNotNull(enemy);

            Invoke(spawner, "OnEnemyDied", new EnemyDiedEvent(enemy, false));

            Assert.IsTrue(spawner.CurrentWave.IsComplete, "the death must complete the wave right away");
            Assert.IsTrue(enemy.activeSelf, "the corpse must stay visible for the death sink");
            Assert.AreEqual(1, spawner.PendingCorpseCount);
            Assert.AreEqual(1, pool.ActiveCount);

            spawner.ReleaseFinishedCorpses(Time.time + 0.5f); // mid-sink
            Assert.IsTrue(enemy.activeSelf);
            Assert.AreEqual(1, spawner.PendingCorpseCount);

            enemy.SetActive(false); // EnemyDeathVfx finished and deactivated it
            spawner.ReleaseFinishedCorpses(Time.time + 0.6f);

            Assert.AreEqual(0, spawner.PendingCorpseCount);
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(1, pool.InactiveCount);
        }

        [Test]
        public void EnemyDied_WithoutADeathSink_IsReleasedAfterTheTimeout()
        {
            EnemySpawner spawner = MakeSpawner(out EnemyPool pool);
            spawner.StartWave(1);
            GameObject enemy = TheOnlyLiveEnemy(pool);

            Invoke(spawner, "OnEnemyDied", new EnemyDiedEvent(enemy, false));
            spawner.ReleaseFinishedCorpses(Time.time + 1.9f);
            Assert.AreEqual(1, spawner.PendingCorpseCount);

            spawner.ReleaseFinishedCorpses(Time.time + 2.1f);

            Assert.AreEqual(0, spawner.PendingCorpseCount);
            Assert.IsFalse(enemy.activeSelf);
            Assert.AreEqual(1, pool.InactiveCount);
        }

        [Test]
        public void DestroyedCorpse_IsDroppedFromThePendingList()
        {
            EnemySpawner spawner = MakeSpawner(out EnemyPool pool);
            spawner.StartWave(1);
            GameObject enemy = TheOnlyLiveEnemy(pool);
            Invoke(spawner, "OnEnemyDied", new EnemyDiedEvent(enemy, false));

            Object.DestroyImmediate(enemy);
            spawner.ReleaseFinishedCorpses(Time.time);

            Assert.AreEqual(0, spawner.PendingCorpseCount);
        }
    }
}
