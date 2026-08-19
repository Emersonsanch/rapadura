using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates the four example BiomeDefinition assets (Floresta,
    /// Deserto, Montanha, Cavernas — see TODO.md Fase 8 "Biomas") under Assets/Resources/Biomes,
    /// so BiomeRegistry has real content to test against. Loot tables reference the example
    /// ItemDefinition assets created by <see cref="ItemDatabaseSeeder"/> (run that seeder first,
    /// or missing items are simply skipped with a warning). Enemy spawn tables are left empty here:
    /// no EnemyDefinition seeder exists yet in this project, so the intended enemy archetype per
    /// entry is documented in a comment (id/name) for whoever authors those assets next; wiring
    /// real EnemyDefinition references only needs a SerializedObject assignment once they exist.
    ///
    /// This is data only — actual Terrain/Scene content per biome is still blocked on opening the
    /// project in the Unity Editor (never done so far). Run via Rapadura > Seed Example Biomes.
    /// </summary>
    public static class BiomeSeeder
    {
        private const string BiomeFolder = "Assets/Resources/Biomes";
        private const string ItemFolder = "Assets/Resources/Items";

        private class LootSeed
        {
            public string itemId;
            public int weight;

            public LootSeed(string itemId, int weight)
            {
                this.itemId = itemId;
                this.weight = weight;
            }
        }

        private class EnemySeed
        {
            // No EnemyDefinition seeder exists yet; kept as documentation for future content authoring.
            public string commentedEnemyId;
            public int weight;

            public EnemySeed(string commentedEnemyId, int weight)
            {
                this.commentedEnemyId = commentedEnemyId;
                this.weight = weight;
            }
        }

        private class BiomeSeed
        {
            public BiomeType type;
            public string nameLocalizationKey;
            public string narrativeDescription;
            public float minTemperatureCelsius;
            public float maxTemperatureCelsius;
            public string ambientMusicCueId;
            public List<EnemySeed> enemies = new List<EnemySeed>();
            public List<LootSeed> loot = new List<LootSeed>();
        }

        [MenuItem("Rapadura/Seed Example Biomes")]
        public static void SeedBiomes()
        {
            if (!Directory.Exists(BiomeFolder))
            {
                Directory.CreateDirectory(BiomeFolder);
            }

            foreach (BiomeSeed seed in BuildSeeds())
            {
                CreateOrUpdateBiome(seed);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BiomeRegistry.Invalidate();
            Debug.Log("[BiomeSeeder] Example biomes created/updated under " + BiomeFolder);
        }

        private static void CreateOrUpdateBiome(BiomeSeed seed)
        {
            string path = Path.Combine(BiomeFolder, seed.type + ".asset");
            var biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>(path);

            if (biome == null)
            {
                biome = ScriptableObject.CreateInstance<BiomeDefinition>();
                AssetDatabase.CreateAsset(biome, path);
            }

            var so = new SerializedObject(biome);
            so.FindProperty("_type").enumValueIndex = (int)seed.type;
            so.FindProperty("_nameLocalizationKey").stringValue = seed.nameLocalizationKey;
            so.FindProperty("_narrativeDescription").stringValue = seed.narrativeDescription;
            so.FindProperty("_minTemperatureCelsius").floatValue = seed.minTemperatureCelsius;
            so.FindProperty("_maxTemperatureCelsius").floatValue = seed.maxTemperatureCelsius;
            so.FindProperty("_ambientMusicCueId").stringValue = seed.ambientMusicCueId;

            // Enemy spawn table: left empty pending a real EnemyDefinition seeder. Each intended
            // entry is documented above in BuildSeeds() as a commented enemy id + weight.
            SerializedProperty enemiesProp = so.FindProperty("_enemies");
            enemiesProp.ClearArray();

            SerializedProperty lootProp = so.FindProperty("_loot");
            lootProp.ClearArray();
            int index = 0;
            foreach (LootSeed lootSeed in seed.loot)
            {
                ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(Path.Combine(ItemFolder, lootSeed.itemId + ".asset"));
                if (item == null)
                {
                    Debug.LogWarning($"[BiomeSeeder] Item '{lootSeed.itemId}' not found under {ItemFolder} — run Rapadura > Seed Example Items first. Skipping loot entry for {seed.type}.");
                    continue;
                }

                lootProp.InsertArrayElementAtIndex(index);
                SerializedProperty entry = lootProp.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("_item").objectReferenceValue = item;
                entry.FindPropertyRelative("_weight").intValue = lootSeed.weight;
                index++;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(biome);
        }

        private static List<BiomeSeed> BuildSeeds()
        {
            return new List<BiomeSeed>
            {
                new BiomeSeed
                {
                    type = BiomeType.Forest,
                    nameLocalizationKey = "biome.forest.name",
                    narrativeDescription = "Mata densa e viva, cortada por trilhas de caça e riachos. O primeiro bioma que o jogador atravessa: perigoso o suficiente para ensinar combate e coleta, sem sufocar a exploração inicial.",
                    minTemperatureCelsius = 15f,
                    maxTemperatureCelsius = 26f,
                    ambientMusicCueId = "music.biome.forest",
                    enemies = new List<EnemySeed>
                    {
                        // Fauna hostil comum de mata (lobo/javali-like), spawn frequente.
                        new EnemySeed("enemy_forest_wolf", 5),
                        // Bandidos/batedores que usam a mata como esconderijo.
                        new EnemySeed("enemy_forest_bandit", 3),
                        // Variante rara/mini-boss da região.
                        new EnemySeed("enemy_forest_alpha_wolf", 1),
                    },
                    loot = new List<LootSeed>
                    {
                        new LootSeed("item_wood", 10),
                        new LootSeed("item_fiber", 8),
                        new LootSeed("item_herb", 6),
                        new LootSeed("item_cooked_meat", 3),
                        new LootSeed("item_wooden_sword", 1),
                    },
                },
                new BiomeSeed
                {
                    type = BiomeType.Desert,
                    nameLocalizationKey = "biome.desert.name",
                    narrativeDescription = "Dunas escaldantes de dia e geladas à noite, pontuadas por ruínas soterradas. Recompensa quem se prepara com itens de sobrevivência e pune quem entra despreparado.",
                    minTemperatureCelsius = 8f,
                    maxTemperatureCelsius = 42f,
                    ambientMusicCueId = "music.biome.desert",
                    enemies = new List<EnemySeed>
                    {
                        // Escorpiões/aracnídeos gigantes adaptados ao calor.
                        new EnemySeed("enemy_desert_scorpion", 5),
                        // Nômades hostis guardando as ruínas.
                        new EnemySeed("enemy_desert_raider", 3),
                        // Guardião de ruína, mini-boss.
                        new EnemySeed("enemy_desert_ruin_guardian", 1),
                    },
                    loot = new List<LootSeed>
                    {
                        new LootSeed("item_stone", 6),
                        new LootSeed("item_iron_ore", 8),
                        new LootSeed("item_gold_coin", 6),
                        new LootSeed("item_health_potion", 3),
                        new LootSeed("item_hunting_bow", 1),
                    },
                },
                new BiomeSeed
                {
                    type = BiomeType.Mountain,
                    nameLocalizationKey = "biome.mountain.name",
                    narrativeDescription = "Picos rochosos e passagens estreitas, ricas em minério mas hostis a quem não domina o terreno vertical. Ventos fortes e altitude testam a resistência do jogador.",
                    minTemperatureCelsius = -5f,
                    maxTemperatureCelsius = 12f,
                    ambientMusicCueId = "music.biome.mountain",
                    enemies = new List<EnemySeed>
                    {
                        // Predadores de altitude (grifo/águia-gigante-like).
                        new EnemySeed("enemy_mountain_harpy", 4),
                        // Golens/elementais de pedra guardando veios de minério.
                        new EnemySeed("enemy_mountain_stone_golem", 4),
                        // Chefe regional de uma passagem/mina.
                        new EnemySeed("enemy_mountain_warlord", 1),
                    },
                    loot = new List<LootSeed>
                    {
                        new LootSeed("item_stone", 10),
                        new LootSeed("item_iron_ore", 10),
                        new LootSeed("item_pickaxe", 2),
                        new LootSeed("item_leather_boots", 2),
                        new LootSeed("item_mana_potion", 3),
                    },
                },
                new BiomeSeed
                {
                    type = BiomeType.Caves,
                    nameLocalizationKey = "biome.caves.name",
                    narrativeDescription = "Rede subterrânea escura, úmida e silenciosa até que algo se mexe. O bioma mais vertical e claustrofóbico, com iluminação escassa e ameaças que dependem de emboscada.",
                    minTemperatureCelsius = 10f,
                    maxTemperatureCelsius = 16f,
                    ambientMusicCueId = "music.biome.caves",
                    enemies = new List<EnemySeed>
                    {
                        // Morcegos/insetos gigantes que atacam em bando.
                        new EnemySeed("enemy_cave_bat_swarm", 6),
                        // Aracnídeos cegos, emboscadores.
                        new EnemySeed("enemy_cave_blind_spider", 4),
                        // Criatura ancestral no fundo da caverna, mini-boss.
                        new EnemySeed("enemy_cave_dweller", 1),
                    },
                    loot = new List<LootSeed>
                    {
                        new LootSeed("item_iron_ore", 8),
                        new LootSeed("item_stone", 6),
                        new LootSeed("item_herb", 4),
                        new LootSeed("item_health_potion", 2),
                        new LootSeed("item_iron_sword", 1),
                    },
                },
            };
        }
    }
}
