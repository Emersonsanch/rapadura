using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// A single weighted entry in an enemy spawn table: how likely <see cref="Enemy"/> is to be
    /// picked relative to the other entries in the same <see cref="BiomeDefinition"/>. Weights are
    /// not required to sum to any particular total — <see cref="BiomeDefinition.RollEnemy"/> and
    /// <see cref="BiomeDefinition.RollLoot"/> normalize against the sum of whatever is configured.
    /// </summary>
    [Serializable]
    public class BiomeEnemySpawnEntry
    {
        [SerializeField] private EnemyDefinition _enemy;
        [SerializeField] [Min(0)] private int _weight = 1;

        public EnemyDefinition Enemy => _enemy;
        public int Weight => _weight;

        public BiomeEnemySpawnEntry()
        {
        }

        /// <summary>Plain-C# construction path for EditMode tests.</summary>
        public BiomeEnemySpawnEntry(EnemyDefinition enemy, int weight)
        {
            _enemy = enemy;
            _weight = weight;
        }
    }

    /// <summary>A single weighted entry in a biome's typical loot table.</summary>
    [Serializable]
    public class BiomeLootEntry
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField] [Min(0)] private int _weight = 1;

        public ItemDefinition Item => _item;
        public int Weight => _weight;

        public BiomeLootEntry()
        {
        }

        /// <summary>Plain-C# construction path for EditMode tests.</summary>
        public BiomeLootEntry(ItemDefinition item, int weight)
        {
            _item = item;
            _weight = weight;
        }
    }

    /// <summary>
    /// Data-driven configuration for a biome. Does not itself contain a Scene, Terrain or any
    /// authored geometry (those require the Unity Editor, which this project has never been
    /// opened in — see TODO.md Fase 8 "Biomas"); this asset is the source of truth that future
    /// terrain/Scene authoring, spawners and ambient audio will read from: its typical enemies
    /// (weighted spawn table), typical loot (weighted drop table), ambient music cue and narrative
    /// tone.
    /// </summary>
    [CreateAssetMenu(menuName = "Rapadura/World/Biome Definition", fileName = "NewBiomeDefinition")]
    public class BiomeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private BiomeType _type = BiomeType.Forest;
        [Tooltip("Localization key resolved through LocalizationManager, e.g. \"biome.forest.name\".")]
        [SerializeField] private string _nameLocalizationKey = "biome.forest.name";
        [TextArea(2, 6)]
        [Tooltip("Short narrative description (GDD tone), used as a fallback/editor label until localized flavor text exists.")]
        [SerializeField] private string _narrativeDescription = string.Empty;

        [Header("Climate")]
        [SerializeField] private float _minTemperatureCelsius = 15f;
        [SerializeField] private float _maxTemperatureCelsius = 25f;

        [Header("Ambient Audio")]
        [Tooltip("Cue id resolved through AudioCueDatabase / AudioManager.PlayOneShot, or a direct AudioClip if PlayMusic is used instead.")]
        [SerializeField] private string _ambientMusicCueId = string.Empty;

        [Header("Spawn Table — Enemies")]
        [SerializeField] private List<BiomeEnemySpawnEntry> _enemies = new List<BiomeEnemySpawnEntry>();

        [Header("Loot Table — Items")]
        [SerializeField] private List<BiomeLootEntry> _loot = new List<BiomeLootEntry>();

        public BiomeType Type => _type;
        public string NameLocalizationKey => _nameLocalizationKey;
        public string NarrativeDescription => _narrativeDescription;

        public float MinTemperatureCelsius => _minTemperatureCelsius;
        public float MaxTemperatureCelsius => _maxTemperatureCelsius;

        public string AmbientMusicCueId => _ambientMusicCueId;

        public IReadOnlyList<BiomeEnemySpawnEntry> Enemies => _enemies;
        public IReadOnlyList<BiomeLootEntry> Loot => _loot;

        /// <summary>
        /// Picks one enemy from <see cref="Enemies"/> using weighted random selection. Deterministic
        /// for a given <paramref name="random"/> instance/seed, so callers (and tests) can reproduce
        /// a roll. Returns null if there are no entries or every entry has zero total weight.
        /// </summary>
        public EnemyDefinition RollEnemy(System.Random random)
        {
            return RollWeighted(_enemies, entry => entry.Weight, entry => entry.Enemy, random);
        }

        /// <summary>Picks one loot item from <see cref="Loot"/> using weighted random selection. See <see cref="RollEnemy"/>.</summary>
        public ItemDefinition RollLoot(System.Random random)
        {
            return RollWeighted(_loot, entry => entry.Weight, entry => entry.Item, random);
        }

        /// <summary>Plain-C# construction path for EditMode tests, which can't rely on AssetDatabase/CreateAsset (mirrors DialogueDefinition.SetDataForTests).</summary>
        public void SetDataForTests(
            BiomeType type,
            string nameLocalizationKey,
            string narrativeDescription,
            float minTemperatureCelsius,
            float maxTemperatureCelsius,
            string ambientMusicCueId,
            List<BiomeEnemySpawnEntry> enemies,
            List<BiomeLootEntry> loot)
        {
            _type = type;
            _nameLocalizationKey = nameLocalizationKey;
            _narrativeDescription = narrativeDescription;
            _minTemperatureCelsius = minTemperatureCelsius;
            _maxTemperatureCelsius = maxTemperatureCelsius;
            _ambientMusicCueId = ambientMusicCueId;
            _enemies = enemies ?? new List<BiomeEnemySpawnEntry>();
            _loot = loot ?? new List<BiomeLootEntry>();
        }

        private static TResult RollWeighted<TEntry, TResult>(
            IReadOnlyList<TEntry> entries,
            Func<TEntry, int> weightSelector,
            Func<TEntry, TResult> resultSelector,
            System.Random random)
            where TResult : class
        {
            if (entries == null || entries.Count == 0 || random == null)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                totalWeight += Mathf.Max(0, weightSelector(entries[i]));
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = random.Next(totalWeight);
            int cumulative = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                cumulative += Mathf.Max(0, weightSelector(entries[i]));
                if (roll < cumulative)
                {
                    return resultSelector(entries[i]);
                }
            }

            return resultSelector(entries[entries.Count - 1]);
        }
    }
}
