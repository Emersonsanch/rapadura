using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="EnemyPool"/> on top of <see cref="Rapadura.Core.Pooling.GenericObjectPool{T}"/>:
    /// a reused (previously killed) enemy must come back as a fresh spawn — full health, no aggro hold,
    /// no leftover i-frames, the new definition's visual, and the colliders/faded renderers that
    /// <see cref="EnemyDeathVfx"/> touched restored — and <see cref="EnemyPool.Clear"/> must destroy
    /// every instance the pool created.
    /// </summary>
    public class EnemyPoolTests
    {
        private readonly List<Object> _created = new List<Object>();
        private GameObject _prefab;
        private EnemyPool _pool;
        private Transform _container;

        [SetUp]
        public void SetUp()
        {
            _prefab = new GameObject("EnemyPrefab_PoolTest");
            _prefab.SetActive(false);
            _prefab.AddComponent<Health>();
            _prefab.AddComponent<EnemyController>();
            CapsuleCollider capsule = _prefab.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = EnemyVisualSwapper.VisualName;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(_prefab.transform, false);
            _created.Add(_prefab);

            _container = new GameObject("EnemyPoolContainer_Test").transform;
            _created.Add(_container.gameObject);

            var poolObject = new GameObject("EnemyPool_Test");
            _created.Add(poolObject);
            _pool = poolObject.AddComponent<EnemyPool>();
            _pool.Configure(_prefab, _container);
        }

        [TearDown]
        public void TearDown()
        {
            if (_pool != null)
            {
                _pool.Clear();
            }

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private EnemyDefinition MakeEnemy(string id, float maxHealth, bool usePlaceholder = true)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = id;
            SetField(enemy, "_maxHealth", maxHealth);
            SetField(enemy, "_usePlaceholderVisual", usePlaceholder);
            _created.Add(enemy);
            return enemy;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        /// <summary>What a kill + <see cref="EnemyDeathVfx"/> leave on a corpse, then back into the pool.</summary>
        private void KillAndRelease(GameObject enemy)
        {
            EnemyController controller = enemy.GetComponent<EnemyController>();
            controller.HoldAggro(30f);
            enemy.GetComponent<Health>().ApplyDamage(100000f, default(ElementType), null);

            foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0f));
            foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>(true))
            {
                renderer.SetPropertyBlock(block);
            }

            _pool.Release(enemy);
        }

        [Test]
        public void Spawn_ReusesReleasedInstance_InsteadOfInstantiating()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_pool_wolf", 40f);
            GameObject first = _pool.Spawn(wolf, Vector3.zero, Quaternion.identity);
            _pool.Release(first);

            GameObject second = _pool.Spawn(wolf, Vector3.one, Quaternion.identity);

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _pool.CreatedCount);
            Assert.AreEqual(1, _pool.ActiveCount);
            Assert.AreEqual(0, _pool.InactiveCount);
            Assert.IsTrue(second.activeSelf);
            Assert.AreEqual(Vector3.one, second.transform.position);
        }

        [Test]
        public void Spawn_ReusedDeadEnemy_ComesBackWithFullHealthAndNoAggroHold()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_pool_wolf", 40f);
            EnemyDefinition boar = MakeEnemy("enemy_pool_boar", 75f);
            GameObject enemy = _pool.Spawn(wolf, Vector3.zero, Quaternion.identity);
            EnemyController controller = enemy.GetComponent<EnemyController>();
            Health health = enemy.GetComponent<Health>();
            int generationBefore = controller.SpawnGeneration;

            KillAndRelease(enemy);
            Assert.IsTrue(health.IsDead, "Precondition: the pooled corpse is dead.");
            Assert.IsTrue(controller.IsHoldingAggro, "Precondition: the corpse still holds its aggro.");

            GameObject reused = _pool.Spawn(boar, Vector3.zero, Quaternion.identity);

            Assert.AreSame(enemy, reused);
            Assert.IsFalse(health.IsDead);
            Assert.AreEqual(75f, health.MaxHealth, 0.001f);
            Assert.AreEqual(75f, health.CurrentHealth, 0.001f);
            Assert.IsFalse(health.IsInvulnerable, "Post-hit i-frames from the last life must not carry over.");
            Assert.IsFalse(controller.IsHoldingAggro);
            Assert.AreEqual(0f, controller.AggroHoldRemaining);
            Assert.AreSame(boar, controller.Definition);
            Assert.Greater(controller.SpawnGeneration, generationBefore, "EnemyDeathVfx uses the generation to stop sinking a re-spawned enemy.");
        }

        [Test]
        public void Spawn_ReusedEnemy_GetsTheNewDefinitionsVisual()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_pool_wolf", 40f);
            EnemyDefinition scorpion = MakeEnemy("enemy_pool_scorpion", 30f);
            GameObject enemy = _pool.Spawn(wolf, Vector3.zero, Quaternion.identity);
            Assert.AreSame(wolf, enemy.GetComponentInChildren<EnemyVisualMarker>().Source);

            _pool.Release(enemy);
            _pool.Spawn(scorpion, Vector3.zero, Quaternion.identity);

            EnemyVisualMarker[] markers = enemy.GetComponentsInChildren<EnemyVisualMarker>(true);
            Assert.AreEqual(1, markers.Length, "The old placeholder must be discarded, not stacked.");
            Assert.AreSame(scorpion, markers[0].Source);
        }

        [Test]
        public void Spawn_WithoutVisuals_LeavesThePrefabVisualAlone()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_pool_wolf", 40f);

            GameObject enemy = _pool.Spawn(wolf, Vector3.zero, Quaternion.identity, applyVisuals: false);

            Assert.IsNull(enemy.GetComponentInChildren<EnemyVisualMarker>(true));
        }

        [Test]
        public void Get_ReusedCorpse_RestoresCollidersAndClearsFadePropertyBlocks()
        {
            GameObject enemy = _pool.Spawn(MakeEnemy("enemy_pool_wolf", 40f), Vector3.zero, Quaternion.identity, applyVisuals: false);
            KillAndRelease(enemy);

            GameObject reused = _pool.Get(Vector3.zero, Quaternion.identity);

            Assert.AreSame(enemy, reused);
            Assert.IsTrue(reused.GetComponent<CapsuleCollider>().enabled);
            foreach (Renderer renderer in reused.GetComponentsInChildren<Renderer>(true))
            {
                Assert.IsFalse(renderer.HasPropertyBlock(), $"{renderer.name} kept the death fade.");
            }
        }

        [Test]
        public void Get_ReusedCorpse_KeepsCollidersThePrefabHadDisabled()
        {
            var trigger = new GameObject("DisabledTrigger");
            trigger.transform.SetParent(_prefab.transform, false);
            SphereCollider sphere = trigger.AddComponent<SphereCollider>();
            sphere.enabled = false;

            GameObject enemy = _pool.Get(Vector3.zero, Quaternion.identity);
            KillAndRelease(enemy);
            GameObject reused = _pool.Get(Vector3.zero, Quaternion.identity);

            Assert.IsTrue(reused.GetComponent<CapsuleCollider>().enabled);
            Assert.IsFalse(reused.GetComponentInChildren<SphereCollider>(true).enabled);
        }

        [Test]
        public void Release_ParksInstanceInactiveUnderTheContainer()
        {
            GameObject enemy = _pool.Get(Vector3.zero, Quaternion.identity);
            enemy.transform.SetParent(null);

            _pool.Release(enemy);

            Assert.IsFalse(enemy.activeSelf);
            Assert.AreSame(_container, enemy.transform.parent);
            Assert.AreEqual(1, _pool.InactiveCount);
            Assert.AreEqual(0, _pool.ActiveCount);
        }

        [Test]
        public void Get_SkipsParkedInstancesDestroyedOutsideThePool()
        {
            GameObject a = _pool.Get(Vector3.zero, Quaternion.identity);
            _pool.Release(a);
            Object.DestroyImmediate(a);

            GameObject fresh = _pool.Get(Vector3.zero, Quaternion.identity);

            Assert.IsTrue(fresh != null);
            Assert.IsTrue(fresh.activeSelf);
        }

        [Test]
        public void Clear_DestroysActiveAndParkedInstances()
        {
            _pool.Prewarm(2);
            GameObject active = _pool.Get(Vector3.zero, Quaternion.identity);
            GameObject alsoActive = _pool.Get(Vector3.zero, Quaternion.identity);
            alsoActive.transform.SetParent(null); // e.g. reparented somewhere else in the scene
            GameObject parked = _pool.Get(Vector3.zero, Quaternion.identity);
            _pool.Release(parked);
            Assert.AreEqual(3, _pool.CreatedCount);

            _pool.Clear();

            Assert.IsTrue(active == null);
            Assert.IsTrue(alsoActive == null);
            Assert.IsTrue(parked == null);
            Assert.AreEqual(0, _container.childCount);
            Assert.AreEqual(0, _pool.CreatedCount);
            Assert.AreEqual(0, _pool.ActiveCount);
            Assert.AreEqual(0, _pool.InactiveCount);
        }

        [Test]
        public void Clear_ThenGet_StartsAFreshPool()
        {
            GameObject old = _pool.Get(Vector3.zero, Quaternion.identity);
            _pool.Clear();

            GameObject fresh = _pool.Get(Vector3.zero, Quaternion.identity);

            Assert.IsTrue(old == null);
            Assert.IsTrue(fresh != null);
            Assert.AreEqual(1, _pool.CreatedCount);
            Assert.AreEqual(1, _pool.ActiveCount);
        }

        [Test]
        public void Configure_WithADifferentPrefab_DestroysInstancesOfTheOldOne()
        {
            GameObject old = _pool.Get(Vector3.zero, Quaternion.identity);
            var otherPrefab = new GameObject("OtherEnemyPrefab_Test");
            otherPrefab.SetActive(false);
            _created.Add(otherPrefab);

            _pool.Configure(otherPrefab, _container);

            Assert.IsTrue(old == null);
            Assert.AreEqual(0, _pool.CreatedCount);
        }

        [Test]
        public void Get_WithoutPrefab_ReturnsNull()
        {
            var poolObject = new GameObject("EmptyEnemyPool_Test");
            _created.Add(poolObject);
            EnemyPool empty = poolObject.AddComponent<EnemyPool>();

            Assert.IsNull(empty.Get(Vector3.zero, Quaternion.identity));
            Assert.IsNull(empty.Spawn(null, Vector3.zero, Quaternion.identity));
        }
    }
}
