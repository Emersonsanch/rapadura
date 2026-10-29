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

    /// <summary>
    /// A single weighted entry in a biome's loot table, rolled by <c>EnemyLootRoller</c> each time an
    /// enemy of that biome dies: <see cref="Weight"/> picks the entry, then a quantity in
    /// [<see cref="QuantityMin"/>, <see cref="QuantityMax"/>] is granted (equipment always comes as one
    /// procedurally rolled piece, see <c>EnemyLootRoller</c>).
    /// </summary>
    [Serializable]
    public class BiomeLootEntry
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField] [Min(0)] private int _weight = 1;
        [SerializeField] [Min(1)] private int _quantityMin = 1;
        [SerializeField] [Min(1)] private int _quantityMax = 1;

        public ItemDefinition Item => _item;
        public int Weight => _weight;
        public int QuantityMin => Mathf.Max(1, _quantityMin);
        public int QuantityMax => Mathf.Max(QuantityMin, _quantityMax);

        public BiomeLootEntry()
        {
        }

        /// <summary>Plain-C# construction path for EditMode tests.</summary>
        public BiomeLootEntry(ItemDefinition item, int weight)
            : this(item, weight, 1, 1)
        {
        }

        /// <summary>Plain-C# construction path for EditMode tests, with a quantity range.</summary>
        public BiomeLootEntry(ItemDefinition item, int weight, int quantityMin, int quantityMax)
        {
            _item = item;
            _weight = weight;
            _quantityMin = quantityMin;
            _quantityMax = quantityMax;
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
        [Tooltip("Chance (0..1) that each loot roll of a common enemy of this biome yields an entry of the table. Bosses always succeed.")]
        [SerializeField] [Range(0f, 1f)] private float _lootDropChance = 0.75f;
        [Tooltip("Loot table rolls per common enemy killed.")]
        [SerializeField] [Min(0)] private int _lootRollsPerKill = 1;
        [Tooltip("Loot table rolls per boss killed (each one guaranteed).")]
        [SerializeField] [Min(0)] private int _bossLootRolls = 4;

        [Header("Weather Table (optional)")]
        [Tooltip("Weighted weather states for WeatherSystem. Leave empty to use WeatherTable's per-biome defaults.")]
        [SerializeField] private List<BiomeWeatherEntry> _weather = new List<BiomeWeatherEntry>();

        public BiomeType Type => _type;
        public string NameLocalizationKey => _nameLocalizationKey;
        public string NarrativeDescription => _narrativeDescription;

        public float MinTemperatureCelsius => _minTemperatureCelsius;
        public float MaxTemperatureCelsius => _maxTemperatureCelsius;

        public string AmbientMusicCueId => _ambientMusicCueId;

        public IReadOnlyList<BiomeEnemySpawnEntry> Enemies => _enemies;
        public IReadOnlyList<BiomeLootEntry> Loot => _loot;
        public float LootDropChance => Mathf.Clamp01(_lootDropChance);
        public int LootRollsPerKill => Mathf.Max(0, _lootRollsPerKill);
        public int BossLootRolls => Mathf.Max(0, _bossLootRolls);
        public IReadOnlyList<BiomeWeatherEntry> Weather => _weather;

        /// <summary>
        /// Picks one enemy from <see cref="Enemies"/> using weighted random selection. Deterministic
        /// for a given <paramref name="random"/> instance/seed, so callers (and tests) can reproduce
        /// a roll. Returns null if there are no entries or every entry has zero total weight.
        /// </summary>
        public EnemyDefinition RollEnemy(System.Random random)
        {
            return RollWeighted(_enemies, entry => entry.Weight, entry => entry.Enemy, random);
        }

        /// <summary>
        /// Same as <see cref="RollEnemy(System.Random)"/>, but only entries whose enemy passes
        /// <paramref name="isAllowed"/> take part (e.g. <c>DefeatedEnemyRegistry.CanSpawn</c>, so a
        /// defeated unique boss is never rolled again); weights are renormalized over what remains.
        /// A null filter behaves exactly like the unfiltered overload (same RNG consumption and
        /// result). Returns null if nothing passes the filter.
        /// </summary>
        public EnemyDefinition RollEnemy(System.Random random, Predicate<EnemyDefinition> isAllowed)
        {
            if (isAllowed == null)
            {
                return RollEnemy(random);
            }

            var allowed = new List<BiomeEnemySpawnEntry>(_enemies.Count);
            foreach (BiomeEnemySpawnEntry entry in _enemies)
            {
                if (entry != null && entry.Enemy != null && isAllowed(entry.Enemy))
                {
                    allowed.Add(entry);
                }
            }

            return RollWeighted(allowed, entry => entry.Weight, entry => entry.Enemy, random);
        }

        /// <summary>Picks one loot item from <see cref="Loot"/> using weighted random selection. See <see cref="RollEnemy(System.Random)"/>.</summary>
        public ItemDefinition RollLoot(System.Random random)
        {
            return RollWeighted(_loot, entry => entry.Weight, entry => entry.Item, random);
        }

        /// <summary>Same weighted pick as <see cref="RollLoot"/>, but returns the whole entry (quantity range included).</summary>
        public BiomeLootEntry RollLootEntry(System.Random random)
        {
            return RollWeighted(_loot, entry => entry != null && entry.Item != null ? entry.Weight : 0, entry => entry, random);
        }

        /// <summary>
        /// True if <paramref name="enemy"/> appears in this biome's spawn table (any weight). Used to find
        /// which biome's loot table an enemy drops from (<see cref="BiomeRegistry.FindBiomeOf"/>).
        /// </summary>
        public bool ContainsEnemy(EnemyDefinition enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            foreach (BiomeEnemySpawnEntry entry in _enemies)
            {
                if (entry != null && entry.Enemy == enemy)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>EditMode test hook for the loot roll settings (kept separate so SetDataForTests' signature is unchanged).</summary>
        public void SetLootRollsForTests(float lootDropChance, int lootRollsPerKill, int bossLootRolls)
        {
            _lootDropChance = lootDropChance;
            _lootRollsPerKill = lootRollsPerKill;
            _bossLootRolls = bossLootRolls;
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

        /// <summary>EditMode test hook for the optional weather table (kept separate so SetDataForTests' signature is unchanged).</summary>
        public void SetWeatherForTests(List<BiomeWeatherEntry> weather)
        {
            _weather = weather ?? new List<BiomeWeatherEntry>();
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
