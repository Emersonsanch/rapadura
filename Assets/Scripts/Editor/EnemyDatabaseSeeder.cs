using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates a starter set of example EnemyDefinition assets under
    /// Assets/Resources/Enemies, covering the four example biomes already seeded by
    /// <see cref="BiomeSeeder"/> (Floresta/Bosque do Zarafshan, Deserto/Areias Flamejantes, Montanha/Serra de Kaggen, Cavernas/Grutas do Limiar — see TODO.md Fase 8
    /// "Biomas"), with one common enemy and one boss-flagged mini-boss per biome. The enemy ids
    /// used here match the ones <see cref="BiomeSeeder"/> already documents by comment (e.g.
    /// "enemy_forest_wolf"), so re-running <c>Rapadura &gt; Seed Example Biomes</c> after this
    /// seeder is free to wire real references instead of leaving the spawn table empty.
    ///
    /// Safe to re-run: existing assets with a matching enemy id are updated in place. Optionally,
    /// after creating the EnemyDefinition assets, this seeder also tries to populate the spawn
    /// table of the matching BiomeDefinition asset under Assets/Resources/Biomes (if that asset
    /// already exists there) — if it can't be found via AssetDatabase, that biome is simply
    /// skipped with a warning, no exception.
    ///
    /// Run via Rapadura > Seed Example Enemies.
    /// </summary>
    public static class EnemyDatabaseSeeder
    {
        private const string EnemyFolder = "Assets/Resources/Enemies";
        private const string BiomeFolder = "Assets/Resources/Biomes";

        private class EnemySeed
        {
            public string id;
            public string name;
            public bool isBoss;
            public BiomeType biome;
            public int spawnWeight;

            public float maxHealth;

            public float patrolSpeed;
            public float chaseSpeed;
            public float fleeSpeed;
            public float waypointArrivalThreshold = 0.25f;

            public float detectionRadius;
            public float loseTargetRadius;

            public float attackRange;
            public float attackCooldown;
            public float attackDamage;

            public float fleeHealthThreshold = 0.2f;
            public float fleeSafeDistance = 10f;
        }

        [MenuItem("Rapadura/Seed Example Enemies")]
        public static void SeedEnemies()
        {
            if (!Directory.Exists(EnemyFolder))
            {
                Directory.CreateDirectory(EnemyFolder);
            }

            List<EnemySeed> seeds = BuildSeeds();
            var createdByBiome = new Dictionary<BiomeType, List<(EnemyDefinition enemy, int weight)>>();

            foreach (EnemySeed seed in seeds)
            {
                EnemyDefinition enemy = CreateOrUpdateEnemy(seed);

                if (!createdByBiome.TryGetValue(seed.biome, out List<(EnemyDefinition, int)> list))
                {
                    list = new List<(EnemyDefinition, int)>();
                    createdByBiome[seed.biome] = list;
                }

                list.Add((enemy, seed.spawnWeight));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TryPopulateBiomeSpawnTables(createdByBiome);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemyDatabaseSeeder] Example enemies created/updated under " + EnemyFolder);
        }

        private static EnemyDefinition CreateOrUpdateEnemy(EnemySeed seed)
        {
            string path = Path.Combine(EnemyFolder, seed.id + ".asset");
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);

            if (enemy == null)
            {
                enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(enemy, path);
            }

            var so = new SerializedObject(enemy);
            so.FindProperty("_enemyName").stringValue = seed.name;
            so.FindProperty("_isBoss").boolValue = seed.isBoss;

            so.FindProperty("_maxHealth").floatValue = seed.maxHealth;

            so.FindProperty("_patrolSpeed").floatValue = seed.patrolSpeed;
            so.FindProperty("_chaseSpeed").floatValue = seed.chaseSpeed;
            so.FindProperty("_fleeSpeed").floatValue = seed.fleeSpeed;
            so.FindProperty("_waypointArrivalThreshold").floatValue = seed.waypointArrivalThreshold;

            so.FindProperty("_detectionRadius").floatValue = seed.detectionRadius;
            so.FindProperty("_loseTargetRadius").floatValue = seed.loseTargetRadius;

            so.FindProperty("_attackRange").floatValue = seed.attackRange;
            so.FindProperty("_attackCooldown").floatValue = seed.attackCooldown;
            so.FindProperty("_attackDamage").floatValue = seed.attackDamage;

            so.FindProperty("_fleeHealthThreshold").floatValue = seed.fleeHealthThreshold;
            so.FindProperty("_fleeSafeDistance").floatValue = seed.fleeSafeDistance;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(enemy);

            return enemy;
        }

        /// <summary>
        /// Best-effort: looks up each biome's BiomeDefinition asset (BiomeSeeder's naming
        /// convention, "{BiomeType}.asset") under Assets/Resources/Biomes and, if found, appends
        /// this seeder's enemies to its spawn table. If the biome asset doesn't exist yet (the
        /// project hasn't run Rapadura > Seed Example Biomes, or the folder is missing), the biome
        /// is skipped with a warning — this step never throws.
        /// </summary>
        private static void TryPopulateBiomeSpawnTables(Dictionary<BiomeType, List<(EnemyDefinition enemy, int weight)>> createdByBiome)
        {
            if (!AssetDatabase.IsValidFolder(BiomeFolder))
            {
                Debug.LogWarning($"[EnemyDatabaseSeeder] Biome folder '{BiomeFolder}' not found — run Rapadura > Seed Example Biomes first if you want spawn tables wired automatically. Skipping.");
                return;
            }

            foreach (KeyValuePair<BiomeType, List<(EnemyDefinition enemy, int weight)>> kvp in createdByBiome)
            {
                string path = Path.Combine(BiomeFolder, kvp.Key + ".asset");
                var biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>(path);

                if (biome == null)
                {
                    Debug.LogWarning($"[EnemyDatabaseSeeder] BiomeDefinition asset '{path}' not found — skipping spawn table wiring for {kvp.Key}.");
                    continue;
                }

                var so = new SerializedObject(biome);
                SerializedProperty enemiesProp = so.FindProperty("_enemies");

                foreach ((EnemyDefinition enemy, int weight) in kvp.Value)
                {
                    bool alreadyPresent = false;
                    for (int i = 0; i < enemiesProp.arraySize; i++)
                    {
                        SerializedProperty existing = enemiesProp.GetArrayElementAtIndex(i).FindPropertyRelative("_enemy");
                        if (existing.objectReferenceValue == enemy)
                        {
                            alreadyPresent = true;
                            break;
                        }
                    }

                    if (alreadyPresent)
                    {
                        continue;
                    }

                    int index = enemiesProp.arraySize;
                    enemiesProp.InsertArrayElementAtIndex(index);
                    SerializedProperty entry = enemiesProp.GetArrayElementAtIndex(index);
                    entry.FindPropertyRelative("_enemy").objectReferenceValue = enemy;
                    entry.FindPropertyRelative("_weight").intValue = weight;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(biome);
            }
        }

        private static List<EnemySeed> BuildSeeds()
        {
            return new List<EnemySeed>
            {
                // Floresta (Bosque do Zarafshan)
                new EnemySeed
                {
                    id = "enemy_forest_wolf",
                    name = "Lobo do Tugai",
                    isBoss = false,
                    biome = BiomeType.Forest,
                    spawnWeight = 5,
                    maxHealth = 70f,
                    patrolSpeed = 2f,
                    chaseSpeed = 4.5f,
                    fleeSpeed = 5f,
                    detectionRadius = 8f,
                    loseTargetRadius = 12f,
                    attackRange = 1.3f,
                    attackCooldown = 1f,
                    attackDamage = 7f,
                },
                new EnemySeed
                {
                    id = "enemy_forest_alpha_wolf",
                    name = "Kök-Böri, o Lobo Celeste",
                    isBoss = true,
                    biome = BiomeType.Forest,
                    spawnWeight = 1,
                    maxHealth = 520f,
                    patrolSpeed = 2.2f,
                    chaseSpeed = 5.5f,
                    fleeSpeed = 0f,
                    detectionRadius = 12f,
                    loseTargetRadius = 18f,
                    attackRange = 1.8f,
                    attackCooldown = 2.4f,
                    attackDamage = 15f,
                    fleeHealthThreshold = 0f,
                },

                // Deserto (Areias Flamejantes)
                new EnemySeed
                {
                    id = "enemy_desert_scorpion",
                    name = "Escorpião das Areias Flamejantes",
                    isBoss = false,
                    biome = BiomeType.Desert,
                    spawnWeight = 5,
                    maxHealth = 110f,
                    patrolSpeed = 1.6f,
                    chaseSpeed = 3.5f,
                    fleeSpeed = 4.5f,
                    detectionRadius = 7f,
                    loseTargetRadius = 10f,
                    attackRange = 1.4f,
                    attackCooldown = 1.3f,
                    attackDamage = 11f,
                },
                new EnemySeed
                {
                    id = "enemy_desert_ruin_guardian",
                    name = "O Grande Ushabti",
                    isBoss = true,
                    biome = BiomeType.Desert,
                    spawnWeight = 1,
                    maxHealth = 780f,
                    patrolSpeed = 1.8f,
                    chaseSpeed = 3.8f,
                    fleeSpeed = 0f,
                    detectionRadius = 14f,
                    loseTargetRadius = 20f,
                    attackRange = 2f,
                    attackCooldown = 3.3f,
                    attackDamage = 32f,
                    fleeHealthThreshold = 0f,
                },

                // Montanha (Serra de Kaggen)
                new EnemySeed
                {
                    id = "enemy_mountain_stone_golem",
                    name = "Ushabti de Pedra",
                    isBoss = false,
                    biome = BiomeType.Mountain,
                    spawnWeight = 4,
                    maxHealth = 140f,
                    patrolSpeed = 1.2f,
                    chaseSpeed = 2.5f,
                    fleeSpeed = 0f,
                    detectionRadius = 6f,
                    loseTargetRadius = 9f,
                    attackRange = 1.7f,
                    attackCooldown = 1.6f,
                    attackDamage = 13f,
                    fleeHealthThreshold = 0f,
                },
                new EnemySeed
                {
                    id = "enemy_mountain_warlord",
                    name = "Hahya, o Gigante Guerreiro",
                    isBoss = true,
                    biome = BiomeType.Mountain,
                    spawnWeight = 1,
                    maxHealth = 900f,
                    patrolSpeed = 2f,
                    chaseSpeed = 4f,
                    fleeSpeed = 0f,
                    detectionRadius = 13f,
                    loseTargetRadius = 19f,
                    attackRange = 2.2f,
                    attackCooldown = 2.6f,
                    attackDamage = 30f,
                    fleeHealthThreshold = 0f,
                },

                // Cavernas (Grutas do Limiar)
                new EnemySeed
                {
                    id = "enemy_cave_bat_swarm",
                    name = "Enxame de Morcegos do Limiar",
                    isBoss = false,
                    biome = BiomeType.Caves,
                    spawnWeight = 6,
                    maxHealth = 170f,
                    patrolSpeed = 2.5f,
                    chaseSpeed = 5f,
                    fleeSpeed = 6f,
                    detectionRadius = 9f,
                    loseTargetRadius = 13f,
                    attackRange = 1.5f,
                    attackCooldown = 0.7f,
                    attackDamage = 7f,
                },
                new EnemySeed
                {
                    id = "enemy_cave_dweller",
                    name = "Tages, o Habitante Ancestral",
                    isBoss = true,
                    biome = BiomeType.Caves,
                    spawnWeight = 1,
                    maxHealth = 1180f,
                    patrolSpeed = 1.5f,
                    chaseSpeed = 3.2f,
                    fleeSpeed = 0f,
                    detectionRadius = 10f,
                    loseTargetRadius = 15f,
                    attackRange = 1.9f,
                    attackCooldown = 3f,
                    attackDamage = 42f,
                    fleeHealthThreshold = 0f,
                },
            };
        }
    }
}
