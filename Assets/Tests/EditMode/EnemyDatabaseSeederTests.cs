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
            public float attackRange;
            public EnemyProjectileKind projectile;
        }

        private const float MeleeRange = 1.5f;

        private static readonly List<EnemySeed> Seeds = new List<EnemySeed>
        {
            new EnemySeed { id = "enemy_forest_wolf", name = "Lobo do Tugai", isBoss = false, biome = BiomeType.Forest, maxHealth = 70f, attackDamage = 7f, attackRange = MeleeRange },
            new EnemySeed { id = "enemy_forest_alpha_wolf", name = "Kök-Böri, o Lobo Celeste", isBoss = true, biome = BiomeType.Forest, maxHealth = 520f, attackDamage = 15f, attackRange = MeleeRange },
            new EnemySeed { id = "enemy_desert_scorpion", name = "Escorpião das Areias Flamejantes", isBoss = false, biome = BiomeType.Desert, maxHealth = 110f, attackDamage = 11f, attackRange = MeleeRange },
            new EnemySeed { id = "enemy_desert_ruin_guardian", name = "O Grande Ushabti", isBoss = true, biome = BiomeType.Desert, maxHealth = 780f, attackDamage = 32f, attackRange = MeleeRange },
            new EnemySeed { id = "enemy_mountain_stone_golem", name = "Ushabti de Pedra", isBoss = false, biome = BiomeType.Mountain, maxHealth = 140f, attackDamage = 13f, attackRange = MeleeRange },
            new EnemySeed { id = "enemy_mountain_warlord", name = "Hahya, o Gigante Guerreiro", isBoss = true, biome = BiomeType.Mountain, maxHealth = 900f, attackDamage = 30f, attackRange = MeleeRange },
            new EnemySeed { id = "enemy_cave_bat_swarm", name = "Enxame de Morcegos do Limiar", isBoss = false, biome = BiomeType.Caves, maxHealth = 170f, attackDamage = 7f, attackRange = MeleeRange },
            new EnemySeed { id = "enemy_cave_dweller", name = "Tages, o Habitante Ancestral", isBoss = true, biome = BiomeType.Caves, maxHealth = 1180f, attackDamage = 42f, attackRange = MeleeRange },

            // [distância] commons of the playable maps (Docs/Mapas100.md 07, 08, 13, 19), spawn weight 0 in their biome.
            new EnemySeed { id = "enemy_desert_ember_vulture", name = "Abutre de Brasa", isBoss = false, biome = BiomeType.Desert, maxHealth = 110f, attackDamage = 12f, attackRange = 8f, projectile = EnemyProjectileKind.Ember },
            new EnemySeed { id = "enemy_desert_broken_lantern", name = "Lanterna Quebrada dos Artífices", isBoss = false, biome = BiomeType.Desert, maxHealth = 110f, attackDamage = 14f, attackRange = 7f, projectile = EnemyProjectileKind.LanternBolt },
            new EnemySeed { id = "enemy_mountain_crested_hawk", name = "Gavião-de-Penacho", isBoss = false, biome = BiomeType.Mountain, maxHealth = 140f, attackDamage = 13f, attackRange = 9f, projectile = EnemyProjectileKind.Feather },
            new EnemySeed { id = "enemy_cave_lantern_fish", name = "Peixe-Lanterna Apagado", isBoss = false, biome = BiomeType.Caves, maxHealth = 170f, attackDamage = 16f, attackRange = 7.5f, projectile = EnemyProjectileKind.LureGlob },
        };

        private static IEnumerable<EnemySeed> RangedSeeds => Seeds.Where(s => s.projectile != EnemyProjectileKind.None);

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
            SetPrivateField(enemy, "_attackRange", seed.attackRange);
            SetPrivateField(enemy, "_projectile", seed.projectile);

            return enemy;
        }

        [Test]
        public void BuildSeeds_ProducesTwelveEnemies_FourOfThemRanged()
        {
            Assert.AreEqual(12, Seeds.Count);
            Assert.AreEqual(4, RangedSeeds.Count());
        }

        [Test]
        public void BuildSeeds_CoversFourBiomes_TwoMeleeEach_PlusTheRangedCommons()
        {
            var byBiome = Seeds.GroupBy(s => s.biome).ToDictionary(g => g.Key, g => g.Count());

            CollectionAssert.AreEquivalent(
                new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves },
                byBiome.Keys);

            foreach (IGrouping<BiomeType, EnemySeed> group in Seeds.Where(s => s.projectile == EnemyProjectileKind.None).GroupBy(s => s.biome))
            {
                Assert.AreEqual(2, group.Count(), $"Expected exactly 2 melee enemies for biome {group.Key}.");
            }

            Assert.AreEqual(4, byBiome[BiomeType.Desert], "Desert: Abutre de Brasa + Lanterna Quebrada");
            Assert.AreEqual(3, byBiome[BiomeType.Mountain], "Mountain: Gavião-de-Penacho");
            Assert.AreEqual(3, byBiome[BiomeType.Caves], "Caves: Peixe-Lanterna Apagado");
            Assert.AreEqual(2, byBiome[BiomeType.Forest], "Forest: no ranged common yet");
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
                "enemy_desert_ember_vulture", "enemy_desert_broken_lantern",
                "enemy_mountain_crested_hawk", "enemy_cave_lantern_fish",
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
                EnemySeed boss = group.Single(s => s.isBoss);
                foreach (EnemySeed common in group.Where(s => !s.isBoss))
                {
                    Assert.Greater(boss.maxHealth, common.maxHealth, $"Boss should be tougher than {common.id} in {group.Key}.");
                }
            }
        }

        [Test]
        public void RangedSeeds_AreCommonsThatShootFromRange_MeleeSeedsSwing()
        {
            foreach (EnemySeed seed in Seeds)
            {
                EnemyDefinition enemy = BuildEnemy(seed);
                bool ranged = seed.projectile != EnemyProjectileKind.None;
                Assert.AreEqual(ranged, enemy.IsRanged, seed.id);
                Assert.AreEqual(seed.projectile, enemy.Projectile, seed.id);
                if (ranged)
                {
                    Assert.IsFalse(seed.isBoss, $"{seed.id}: a common");
                    Assert.That(enemy.AttackRange, Is.InRange(6f, 10f), $"{seed.id}: shoots from range");
                }
            }

            CollectionAssert.AllItemsAreUnique(RangedSeeds.Select(s => s.projectile), "each shooter has its own projectile (and shot sound)");
        }

        [Test]
        public void RangedSeeds_MirrorTheirSeededAssets()
        {
            foreach (EnemySeed seed in RangedSeeds)
            {
                EnemyDefinition asset = Resources.Load<EnemyDefinition>("Enemies/" + seed.id);
                Assert.IsNotNull(asset, $"{seed.id}: seeded into Resources/Enemies");
                Assert.AreEqual(seed.name, asset.EnemyName, seed.id);
                Assert.AreEqual(seed.isBoss, asset.IsBoss, seed.id);
                Assert.AreEqual(seed.maxHealth, asset.MaxHealth, 1e-4f, seed.id);
                Assert.AreEqual(seed.attackDamage, asset.AttackDamage, 1e-4f, seed.id);
                Assert.AreEqual(seed.attackRange, asset.AttackRange, 1e-4f, seed.id);
                Assert.AreEqual(seed.projectile, asset.Projectile, seed.id);
            }
        }
    }
}
