using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for gating spawns on <see cref="DefeatedEnemyRegistry"/>: a defeated unique
    /// boss is skipped by <see cref="DefeatedEnemyRegistry.ShouldSkipSpawn"/>, the filtered
    /// <see cref="BiomeDefinition.RollEnemy(System.Random, System.Predicate{EnemyDefinition})"/>,
    /// <see cref="RandomEncounterSystem.EnemyFilter"/> and <see cref="EnemySpawner.SpawnBoss"/>,
    /// while regular enemies keep respawning.
    /// </summary>
    public class DefeatedBossSpawnTests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        // ------------------------------------------------------------------
        // Helpers.
        // ------------------------------------------------------------------

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private EnemyDefinition MakeEnemy(string id, bool isBoss)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = id;
            SetField(enemy, "_isBoss", isBoss);
            _created.Add(enemy);
            return enemy;
        }

        private BiomeDefinition MakeBiome(params (EnemyDefinition enemy, int weight)[] enemies)
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            var entries = new List<BiomeEnemySpawnEntry>();
            foreach ((EnemyDefinition enemy, int weight) in enemies)
            {
                entries.Add(new BiomeEnemySpawnEntry(enemy, weight));
            }

            biome.SetDataForTests(BiomeType.Forest, "biome.forest.name", string.Empty, 15f, 25f, string.Empty, entries, null);
            _created.Add(biome);
            return biome;
        }

        // ------------------------------------------------------------------
        // DefeatedEnemyRegistry gate.
        // ------------------------------------------------------------------

        [Test]
        public void ShouldSkipSpawn_OnlyForDefeatedBosses()
        {
            var registry = new DefeatedEnemyRegistry();
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", isBoss: true);
            EnemyDefinition guardian = MakeEnemy("enemy_desert_ruin_guardian", isBoss: true);
            EnemyDefinition wolf = MakeEnemy("enemy_forest_wolf", isBoss: false);

            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(alpha));
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(wolf));

            Assert.IsTrue(registry.ShouldSkipSpawn(alpha));
            Assert.IsFalse(registry.CanSpawn(alpha));

            Assert.IsFalse(registry.ShouldSkipSpawn(guardian), "Undefeated bosses still spawn.");
            Assert.IsTrue(registry.CanSpawn(guardian));

            Assert.IsFalse(registry.ShouldSkipSpawn(wolf), "Regular enemies always respawn, even if recorded.");
            Assert.IsTrue(registry.CanSpawn(wolf));

            Assert.IsFalse(registry.ShouldSkipSpawn(null));
            Assert.IsFalse(registry.CanSpawn(null));
        }

        [Test]
        public void ShouldSkipSpawn_SurvivesSaveRestore()
        {
            var registry = new DefeatedEnemyRegistry();
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", isBoss: true);
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(alpha));

            var restored = new DefeatedEnemyRegistry();
            restored.RestoreState(registry.CaptureState());

            Assert.IsTrue(restored.ShouldSkipSpawn(alpha));
        }

        // ------------------------------------------------------------------
        // BiomeDefinition.RollEnemy(random, filter).
        // ------------------------------------------------------------------

        [Test]
        public void FilteredRoll_NeverReturnsDefeatedBoss()
        {
            var registry = new DefeatedEnemyRegistry();
            EnemyDefinition wolf = MakeEnemy("enemy_forest_wolf", isBoss: false);
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", isBoss: true);
            BiomeDefinition forest = MakeBiome((wolf, 1), (alpha, 50));
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(alpha));

            var random = new System.Random(7);
            for (int i = 0; i < 200; i++)
            {
                Assert.AreSame(wolf, forest.RollEnemy(random, registry.CanSpawn));
            }
        }

        [Test]
        public void FilteredRoll_NullFilter_MatchesUnfilteredRoll()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_forest_wolf", isBoss: false);
            EnemyDefinition spider = MakeEnemy("enemy_forest_spider", isBoss: false);
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", isBoss: true);
            BiomeDefinition forest = MakeBiome((wolf, 5), (spider, 3), (alpha, 1));

            var a = new System.Random(42);
            var b = new System.Random(42);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreSame(forest.RollEnemy(a), forest.RollEnemy(b, null));
            }
        }

        [Test]
        public void FilteredRoll_NothingAllowed_ReturnsNull()
        {
            var registry = new DefeatedEnemyRegistry();
            EnemyDefinition dweller = MakeEnemy("enemy_cave_dweller", isBoss: true);
            BiomeDefinition caves = MakeBiome((dweller, 1));
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(dweller));

            Assert.IsNull(caves.RollEnemy(new System.Random(1), registry.CanSpawn));
        }

        // ------------------------------------------------------------------
        // RandomEncounterSystem.
        // ------------------------------------------------------------------

        [Test]
        public void EncounterSystem_WithRegistryFilter_NeverRollsDefeatedBoss()
        {
            var registry = new DefeatedEnemyRegistry();
            EnemyDefinition wolf = MakeEnemy("enemy_forest_wolf", isBoss: false);
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", isBoss: true);
            BiomeDefinition forest = MakeBiome((wolf, 1), (alpha, 10));
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(alpha));

            var settings = new RandomEncounterSettings(EncounterStepMode.Distance, 1f, 0.5f, 0, 1f, 0f, 1f, 2, 3);
            var system = new RandomEncounterSystem(settings, 123) { EnemyFilter = registry.CanSpawn };

            for (int i = 0; i < 50; i++)
            {
                EncounterPayload payload = system.BuildPayload(forest);
                Assert.IsNotNull(payload);
                CollectionAssert.DoesNotContain(payload.enemyIds, alpha.name);
            }
        }

        [Test]
        public void EncounterSystem_OnlyDefeatedBossInBiome_ProducesNoEncounter()
        {
            var registry = new DefeatedEnemyRegistry();
            EnemyDefinition dweller = MakeEnemy("enemy_cave_dweller", isBoss: true);
            BiomeDefinition caves = MakeBiome((dweller, 1));

            var settings = new RandomEncounterSettings(EncounterStepMode.Distance, 1f, 0.5f, 0, 1f, 0f, 1f);
            var system = new RandomEncounterSystem(settings, 5) { EnemyFilter = registry.CanSpawn };

            Assert.IsNotNull(system.BuildPayload(caves), "Before the boss is defeated it can still be rolled.");

            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(dweller));

            Assert.IsNull(system.BuildPayload(caves));
            Assert.IsFalse(system.RollStep(caves, out EncounterPayload payload));
            Assert.IsNull(payload);
        }

        // ------------------------------------------------------------------
        // EnemySpawner.SpawnBoss.
        // ------------------------------------------------------------------

        private EnemySpawner MakeSpawner(EnemyDefinition boss)
        {
            var prefab = new GameObject("EnemyPrefab_Test");
            prefab.SetActive(false);
            _created.Add(prefab);

            var spawnerObject = new GameObject("Spawner_Test");
            _created.Add(spawnerObject);
            EnemySpawner spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawnerObject.GetComponent<EnemyPool>().Configure(prefab);
            SetField(spawner, "_bossDefinition", boss);
            SetField(spawner, "_applyDefinitionVisuals", false);

            // EditMode doesn't run Awake for non-ExecuteAlways components; it only caches the pool.
            typeof(EnemySpawner).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(spawner, null);
            return spawner;
        }

        [Test]
        public void SpawnBoss_DefeatedBoss_IsSkipped()
        {
            var registry = new DefeatedEnemyRegistry();
            ServiceLocator.Register(registry);
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", isBoss: true);
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(alpha));
            EnemySpawner spawner = MakeSpawner(alpha);

            Assert.IsNull(spawner.SpawnBoss());
            Assert.IsFalse(spawner.CurrentWave.IsActive, "No wave should start for a skipped boss.");
        }

        [Test]
        public void SpawnBoss_UndefeatedBoss_Spawns()
        {
            ServiceLocator.Register(new DefeatedEnemyRegistry());
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", isBoss: true);
            EnemySpawner spawner = MakeSpawner(alpha);

            GameObject boss = spawner.SpawnBoss();
            if (boss != null)
            {
                _created.Add(boss);
            }

            Assert.IsNotNull(boss);
            Assert.AreEqual(1, spawner.CurrentWave.TotalSpawned);
        }
    }
}
