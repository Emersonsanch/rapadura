using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Tests
{
    public class BiomeRegistryTests
    {
        private readonly List<ScriptableObject> _createdAssets = new List<ScriptableObject>();

        [TearDown]
        public void TearDown()
        {
            BiomeRegistry.Invalidate();

            foreach (ScriptableObject asset in _createdAssets)
            {
                if (asset != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            _createdAssets.Clear();
        }

        private BiomeDefinition MakeBiome(
            BiomeType type,
            List<BiomeEnemySpawnEntry> enemies = null,
            List<BiomeLootEntry> loot = null)
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            biome.SetDataForTests(
                type,
                $"biome.{type.ToString().ToLowerInvariant()}.name",
                $"Descrição narrativa de teste para {type}.",
                minTemperatureCelsius: 0f,
                maxTemperatureCelsius: 20f,
                ambientMusicCueId: $"music.biome.{type.ToString().ToLowerInvariant()}",
                enemies: enemies,
                loot: loot);

            _createdAssets.Add(biome);
            return biome;
        }

        private EnemyDefinition MakeEnemy(string name)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            _createdAssets.Add(enemy);
            return enemy;
        }

        private ItemDefinition MakeItem(string id)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _createdAssets.Add(item);
            return item;
        }

        private void SeedAllFourBiomes()
        {
            BiomeRegistry.SetForTests(new[]
            {
                MakeBiome(BiomeType.Forest),
                MakeBiome(BiomeType.Desert),
                MakeBiome(BiomeType.Mountain),
                MakeBiome(BiomeType.Caves),
            });
        }

        [Test]
        public void GetAll_ReturnsExactlyFourBiomes()
        {
            SeedAllFourBiomes();

            Assert.AreEqual(4, BiomeRegistry.GetAll().Count());
        }

        [TestCase(BiomeType.Forest)]
        [TestCase(BiomeType.Desert)]
        [TestCase(BiomeType.Mountain)]
        [TestCase(BiomeType.Caves)]
        public void Get_ReturnsBiomeWithMatchingType(BiomeType type)
        {
            SeedAllFourBiomes();

            BiomeDefinition biome = BiomeRegistry.Get(type);

            Assert.IsNotNull(biome);
            Assert.AreEqual(type, biome.Type);
        }

        [Test]
        public void TryGet_UnknownType_ReturnsFalse()
        {
            BiomeRegistry.SetForTests(new[] { MakeBiome(BiomeType.Forest) });

            bool found = BiomeRegistry.TryGet(BiomeType.Caves, out BiomeDefinition biome);

            Assert.IsFalse(found);
            Assert.IsNull(biome);
        }

        [Test]
        public void Get_UnknownType_ReturnsNull()
        {
            BiomeRegistry.SetForTests(Array.Empty<BiomeDefinition>());

            Assert.IsNull(BiomeRegistry.Get(BiomeType.Mountain));
        }

        // ------------------------------------------------------------------
        // Weighted spawn/loot table rolls.
        // ------------------------------------------------------------------

        [Test]
        public void RollEnemy_IsDeterministic_ForSameSeed()
        {
            EnemyDefinition common = MakeEnemy("Common");
            EnemyDefinition rare = MakeEnemy("Rare");

            BiomeDefinition biome = MakeBiome(BiomeType.Forest, enemies: new List<BiomeEnemySpawnEntry>
            {
                new BiomeEnemySpawnEntry(common, 9),
                new BiomeEnemySpawnEntry(rare, 1),
            });

            var results1 = new List<EnemyDefinition>();
            var random1 = new System.Random(1234);
            for (int i = 0; i < 50; i++)
            {
                results1.Add(biome.RollEnemy(random1));
            }

            var results2 = new List<EnemyDefinition>();
            var random2 = new System.Random(1234);
            for (int i = 0; i < 50; i++)
            {
                results2.Add(biome.RollEnemy(random2));
            }

            CollectionAssert.AreEqual(results1, results2);
        }

        [Test]
        public void RollEnemy_RespectsWeights_OverManySamples()
        {
            EnemyDefinition common = MakeEnemy("Common");
            EnemyDefinition rare = MakeEnemy("Rare");

            BiomeDefinition biome = MakeBiome(BiomeType.Forest, enemies: new List<BiomeEnemySpawnEntry>
            {
                new BiomeEnemySpawnEntry(common, 9),
                new BiomeEnemySpawnEntry(rare, 1),
            });

            var random = new System.Random(42);
            int commonCount = 0;
            const int sampleCount = 2000;

            for (int i = 0; i < sampleCount; i++)
            {
                if (biome.RollEnemy(random) == common)
                {
                    commonCount++;
                }
            }

            // Expected ~90% common; allow generous slack to keep the test non-flaky.
            double ratio = commonCount / (double)sampleCount;
            Assert.Greater(ratio, 0.8);
            Assert.Less(ratio, 0.98);
        }

        [Test]
        public void RollEnemy_NoEntries_ReturnsNull()
        {
            BiomeDefinition biome = MakeBiome(BiomeType.Caves, enemies: new List<BiomeEnemySpawnEntry>());

            Assert.IsNull(biome.RollEnemy(new System.Random(1)));
        }

        [Test]
        public void RollEnemy_AllZeroWeights_ReturnsNull()
        {
            EnemyDefinition enemy = MakeEnemy("ZeroWeight");
            BiomeDefinition biome = MakeBiome(BiomeType.Caves, enemies: new List<BiomeEnemySpawnEntry>
            {
                new BiomeEnemySpawnEntry(enemy, 0),
            });

            Assert.IsNull(biome.RollEnemy(new System.Random(1)));
        }

        [Test]
        public void RollLoot_IsDeterministic_ForSameSeed()
        {
            ItemDefinition wood = MakeItem("item_wood");
            ItemDefinition sword = MakeItem("item_sword");

            BiomeDefinition biome = MakeBiome(BiomeType.Forest, loot: new List<BiomeLootEntry>
            {
                new BiomeLootEntry(wood, 8),
                new BiomeLootEntry(sword, 2),
            });

            var random1 = new System.Random(777);
            var results1 = Enumerable.Range(0, 30).Select(_ => biome.RollLoot(random1)).ToList();

            var random2 = new System.Random(777);
            var results2 = Enumerable.Range(0, 30).Select(_ => biome.RollLoot(random2)).ToList();

            CollectionAssert.AreEqual(results1, results2);
        }

        [Test]
        public void RollLoot_OnlyReturnsConfiguredItems()
        {
            ItemDefinition wood = MakeItem("item_wood");
            ItemDefinition stone = MakeItem("item_stone");

            BiomeDefinition biome = MakeBiome(BiomeType.Mountain, loot: new List<BiomeLootEntry>
            {
                new BiomeLootEntry(wood, 1),
                new BiomeLootEntry(stone, 1),
            });

            var random = new System.Random(5);
            for (int i = 0; i < 50; i++)
            {
                ItemDefinition rolled = biome.RollLoot(random);
                Assert.That(rolled == wood || rolled == stone);
            }
        }
    }
}
