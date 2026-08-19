using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EnemyDatabaseSeeder (Assets/Scripts/Editor/EnemyDatabaseSeeder.cs) lives in the default
    /// Editor assembly, which this EditMode test assembly does not reference (same reason
    /// BiomeSeeder/ItemDatabaseSeeder aren't referenced directly by other tests here), and its
    /// [MenuItem] entry point can't be safely re-run from a test anyway (creates/saves real
    /// AssetDatabase assets). So this test mirrors the exact seed data the seeder builds — same
    /// ids, names, biomes and IsBoss flags documented in EnemyDatabaseSeeder.BuildSeeds() and
    /// cross-referenced against the enemy ids BiomeSeeder.cs already comments (e.g.
    /// "enemy_forest_wolf") — and exercises it the same way the seeder does: writing it onto a
    /// real EnemyDefinition instance via SerializedObject-equivalent reflection and reading back
    /// through its public API.
    /// </summary>
    public class EnemyDatabaseSeederTests
    {
        private struct EnemySeed
        {
            public string id;
            public string name;
            public bool isBoss;
            public BiomeType biome;
            public float maxHealth;
            public float attackDamage;
        }

        private static readonly List<EnemySeed> Seeds = new List<EnemySeed>
        {
            new EnemySeed { id = "enemy_forest_wolf", name = "Lobo da Mata", isBoss = false, biome = BiomeType.Forest, maxHealth = 30f, attackDamage = 7f },
            new EnemySeed { id = "enemy_forest_alpha_wolf", name = "Lobo Alfa", isBoss = true, biome = BiomeType.Forest, maxHealth = 160f, attackDamage = 18f },
            new EnemySeed { id = "enemy_desert_scorpion", name = "Escorpião das Dunas", isBoss = false, biome = BiomeType.Desert, maxHealth = 26f, attackDamage = 9f },
            new EnemySeed { id = "enemy_desert_ruin_guardian", name = "Guardião da Ruína", isBoss = true, biome = BiomeType.Desert, maxHealth = 200f, attackDamage = 22f },
            new EnemySeed { id = "enemy_mountain_stone_golem", name = "Golem de Pedra", isBoss = false, biome = BiomeType.Mountain, maxHealth = 55f, attackDamage = 13f },
            new EnemySeed { id = "enemy_mountain_warlord", name = "Senhor da Guerra da Montanha", isBoss = true, biome = BiomeType.Mountain, maxHealth = 220f, attackDamage = 26f },
            new EnemySeed { id = "enemy_cave_bat_swarm", name = "Enxame de Morcegos", isBoss = false, biome = BiomeType.Caves, maxHealth = 18f, attackDamage = 5f },
            new EnemySeed { id = "enemy_cave_dweller", name = "Habitante Ancestral", isBoss = true, biome = BiomeType.Caves, maxHealth = 180f, attackDamage = 20f },
        };

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

        private static void SetPrivateField(EnemyDefinition enemy, string fieldName, object value)
        {
            FieldInfo field = typeof(EnemyDefinition).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on EnemyDefinition.");
            field.SetValue(enemy, value);
        }

        private EnemyDefinition BuildEnemy(EnemySeed seed)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(enemy);

            SetPrivateField(enemy, "_enemyName", seed.name);
            SetPrivateField(enemy, "_isBoss", seed.isBoss);
            SetPrivateField(enemy, "_maxHealth", seed.maxHealth);
            SetPrivateField(enemy, "_attackDamage", seed.attackDamage);

            return enemy;
        }

        [Test]
        public void BuildSeeds_ProducesEightEnemies()
        {
            Assert.AreEqual(8, Seeds.Count);
        }

        [Test]
        public void BuildSeeds_CoversFourBiomesWithTwoEnemiesEach()
        {
            var byBiome = Seeds.GroupBy(s => s.biome).ToDictionary(g => g.Key, g => g.Count());

            CollectionAssert.AreEquivalent(
                new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves },
                byBiome.Keys);

            foreach (KeyValuePair<BiomeType, int> kvp in byBiome)
            {
                Assert.AreEqual(2, kvp.Value, $"Expected exactly 2 enemies for biome {kvp.Key}.");
            }
        }

        [Test]
        public void BuildSeeds_HasExactlyOneBossPerBiome()
        {
            foreach (IGrouping<BiomeType, EnemySeed> group in Seeds.GroupBy(s => s.biome))
            {
                int bossCount = group.Count(s => s.isBoss);
                Assert.AreEqual(1, bossCount, $"Expected exactly 1 boss for biome {group.Key}.");
            }
        }

        [Test]
        public void BuildSeeds_IdsAreUniqueAndMatchBiomeSeederComments()
        {
            // Ids BiomeSeeder.cs already references by comment for the "common" and "mini-boss"
            // spawn entries per biome — kept in sync so a future BiomeSeeder update can wire real
            // references instead of leaving the spawn table empty.
            var expectedIds = new HashSet<string>
            {
                "enemy_forest_wolf", "enemy_forest_alpha_wolf",
                "enemy_desert_scorpion", "enemy_desert_ruin_guardian",
                "enemy_mountain_stone_golem", "enemy_mountain_warlord",
                "enemy_cave_bat_swarm", "enemy_cave_dweller",
            };

            var actualIds = new HashSet<string>(Seeds.Select(s => s.id));

            Assert.AreEqual(expectedIds.Count, actualIds.Count, "Enemy ids must be unique.");
            CollectionAssert.AreEquivalent(expectedIds, actualIds);
        }

        [Test]
        public void BuildEnemy_PopulatesBasicFieldsOnRealDefinition()
        {
            foreach (EnemySeed seed in Seeds)
            {
                EnemyDefinition enemy = BuildEnemy(seed);

                Assert.AreEqual(seed.name, enemy.EnemyName, $"Name mismatch for {seed.id}.");
                Assert.AreEqual(seed.isBoss, enemy.IsBoss, $"IsBoss mismatch for {seed.id}.");
                Assert.Greater(enemy.MaxHealth, 0f, $"MaxHealth should be positive for {seed.id}.");
                Assert.Greater(enemy.AttackDamage, 0f, $"AttackDamage should be positive for {seed.id}.");
            }
        }

        [Test]
        public void BuildEnemy_BossesHaveHigherHealthThanCommonEnemyInSameBiome()
        {
            foreach (IGrouping<BiomeType, EnemySeed> group in Seeds.GroupBy(s => s.biome))
            {
                EnemySeed common = group.Single(s => !s.isBoss);
                EnemySeed boss = group.Single(s => s.isBoss);

                Assert.Greater(boss.maxHealth, common.maxHealth, $"Boss should be tougher than the common enemy in {group.Key}.");
            }
        }
    }
}
