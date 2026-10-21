using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Data-driven configuration for an enemy archetype. A single <see cref="EnemyController"/>
    /// prefab can be reconfigured at spawn time by swapping the definition it references, which is
    /// how the "boss inicial" variant is produced (same controller, higher stats, <see cref="IsBoss"/> flag)
    /// without requiring a dedicated prefab or scene asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Rapadura/Enemies/Enemy Definition", fileName = "NewEnemyDefinition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _enemyName = "Enemy";
        [SerializeField] private bool _isBoss;

        [Header("Vitals")]
        [SerializeField] private float _maxHealth = 30f;
        [SerializeField] private float _contactDamage = 10f;

        [Header("Rewards")]
        [Tooltip("Experience granted to the killer's PlayerStats when this enemy dies. See EnemyRewardGranter.")]
        [SerializeField] private int _experienceReward = 10;

        [Header("Loot")]
        [Tooltip("Optional base item dropped into the killer's inventory. Leave null for no loot.")]
        [SerializeField] private ItemDefinition _lootItem;
        [Range(0f, 1f)]
        [SerializeField] private float _lootDropChance = 0.5f;
        [SerializeField] private int _lootQuantityMin = 1;
        [SerializeField] private int _lootQuantityMax = 1;

        [Header("Boss Bonuses (only used when Is Boss is set — see BossBonusRules)")]
        [Tooltip("Boss always drops its Loot Item, ignoring Loot Drop Chance.")]
        [SerializeField] private bool _bossGuaranteedLoot = true;
        [Tooltip("Optional equippable base item rolled through ProceduralItemGenerator (affixes) and given to the killer on top of the normal loot. Leave null for none.")]
        [SerializeField] private ItemDefinition _bossBonusDropItem;
        [Tooltip("Rarity the bonus drop is rolled at (drives how many affixes it gets).")]
        [SerializeField] private ItemRarity _bossBonusDropRarity = ItemRarity.Rare;
        [Tooltip("Multiplier applied to Experience Reward for bosses. 1 = no extra XP. Values below 1 are treated as 1.")]
        [Min(1f)]
        [SerializeField] private float _bossExperienceMultiplier = 1f;

        [Header("Movement")]
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _chaseSpeed = 4f;
        [SerializeField] private float _fleeSpeed = 5f;
        [SerializeField] private float _waypointArrivalThreshold = 0.25f;

        [Header("Detection")]
        [SerializeField] private float _detectionRadius = 8f;
        [SerializeField] private float _loseTargetRadius = 12f;

        [Header("Attack")]
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackCooldown = 1.2f;
        [SerializeField] private float _attackDamage = 8f;

        [Header("Flee")]
        [Range(0f, 1f)]
        [SerializeField] private float _fleeHealthThreshold = 0.2f;
        [SerializeField] private float _fleeSafeDistance = 10f;

        public string EnemyName => _enemyName;
        public bool IsBoss => _isBoss;

        public float MaxHealth => _maxHealth;
        public float ContactDamage => _contactDamage;
        public int ExperienceReward => _experienceReward;

        public ItemDefinition LootItem => _lootItem;
        public float LootDropChance => _lootDropChance;
        public int LootQuantityMin => _lootQuantityMin;
        public int LootQuantityMax => _lootQuantityMax;

        public bool BossGuaranteedLoot => _bossGuaranteedLoot;
        public ItemDefinition BossBonusDropItem => _bossBonusDropItem;
        public ItemRarity BossBonusDropRarity => _bossBonusDropRarity;
        public float BossExperienceMultiplier => _bossExperienceMultiplier;

        public float PatrolSpeed => _patrolSpeed;
        public float ChaseSpeed => _chaseSpeed;
        public float FleeSpeed => _fleeSpeed;
        public float WaypointArrivalThreshold => _waypointArrivalThreshold;

        public float DetectionRadius => _detectionRadius;
        public float LoseTargetRadius => _loseTargetRadius;

        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float AttackDamage => _attackDamage;

        public float FleeHealthThreshold => _fleeHealthThreshold;
        public float FleeSafeDistance => _fleeSafeDistance;
    }
}
