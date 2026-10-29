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
        // No contact damage on purpose: the old _contactDamage field was seeded but never read, and
        // wiring it would add untelegraphed hits on top of the windup-based attack (EnemyAttackState),
        // see Docs/Balance/README.md §2. Assets still carrying the YAML key just ignore it.

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

        [Header("Visual (see EnemyVisualSwapper)")]
        [Tooltip("Optional model/prefab instantiated as this enemy's 'EnemyVisual' child under the shared enemy prefab (e.g. Assets/Models/Enemies/Scorpion/Scorpion.fbx). Leave null to use the placeholder below (or the shared prefab's own visual if the placeholder is disabled).")]
        [SerializeField] private GameObject _visualPrefab;
        [Tooltip("Local position of the visual under the enemy root (Meshy models sit at y = -0.5 under the 2 m enemy capsule).")]
        [SerializeField] private Vector3 _visualLocalPosition = new Vector3(0f, -0.5f, 0f);
        [Tooltip("Local rotation of the visual. Meshy FBX exports need -90 on X to stand upright.")]
        [SerializeField] private Vector3 _visualLocalEulerAngles = new Vector3(-90f, 0f, 0f);
        [Tooltip("Uniform scale of the visual (Meshy FBX exports are in centimetres: ~0.0145 matches the scene's Wolf/Spider).")]
        [Min(0.0001f)]
        [SerializeField] private float _visualScale = 0.0145f;

        [Header("Placeholder Visual (used when Visual Prefab is null)")]
        [Tooltip("Build a primitive placeholder so model-less enemies are still distinguishable. Off = keep the shared prefab's own visual.")]
        [SerializeField] private bool _usePlaceholderVisual = true;
        [SerializeField] private EnemyPlaceholderShape _placeholderShape = EnemyPlaceholderShape.Capsule;
        [Tooltip("Body tint. Fully transparent (alpha 0, the default) = auto color derived from the asset name, so every enemy still differs.")]
        [SerializeField] private Color _placeholderColor = new Color(0f, 0f, 0f, 0f);
        [Tooltip("Body scale of the primitive. It is placed so its base rests at the bottom of the enemy's capsule collider.")]
        [SerializeField] private Vector3 _placeholderScale = Vector3.one;

        [Header("Movement")]
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _chaseSpeed = 4f;
        [SerializeField] private float _fleeSpeed = 5f;
        [SerializeField] private float _waypointArrivalThreshold = 0.25f;
        [Tooltip("Body plan driving acceleration/turning and the procedural gait/attack poses (EnemyProceduralAnimator). Auto = guessed from the asset/enemy name (wolf → Quadruped, spider/scorpion → Arthropod, bat → Flyer, golem/guardian/warlord/dweller → Heavy).")]
        [SerializeField] private EnemyMotionArchetype _motionArchetype = EnemyMotionArchetype.Auto;

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
        public int ExperienceReward => _experienceReward;

        public ItemDefinition LootItem => _lootItem;
        public float LootDropChance => _lootDropChance;
        public int LootQuantityMin => _lootQuantityMin;
        public int LootQuantityMax => _lootQuantityMax;

        public bool BossGuaranteedLoot => _bossGuaranteedLoot;
        public ItemDefinition BossBonusDropItem => _bossBonusDropItem;
        public ItemRarity BossBonusDropRarity => _bossBonusDropRarity;
        public float BossExperienceMultiplier => _bossExperienceMultiplier;

        public GameObject VisualPrefab => _visualPrefab;
        public Vector3 VisualLocalPosition => _visualLocalPosition;
        public Vector3 VisualLocalEulerAngles => _visualLocalEulerAngles;
        public float VisualScale => _visualScale;

        public bool UsePlaceholderVisual => _usePlaceholderVisual;
        public EnemyPlaceholderShape PlaceholderShape => _placeholderShape;
        public Color PlaceholderColor => _placeholderColor;
        public Vector3 PlaceholderScale => _placeholderScale;

        public float PatrolSpeed => _patrolSpeed;
        public float ChaseSpeed => _chaseSpeed;
        public float FleeSpeed => _fleeSpeed;
        public float WaypointArrivalThreshold => _waypointArrivalThreshold;

        /// <summary>The serialized value (may be <see cref="EnemyMotionArchetype.Auto"/>).</summary>
        public EnemyMotionArchetype AuthoredMotionArchetype => _motionArchetype;

        /// <summary>Concrete archetype: the authored one, or the name-based default when Auto.</summary>
        public EnemyMotionArchetype MotionArchetype => EnemyMotionArchetypes.Resolve(this);

        public float DetectionRadius => _detectionRadius;
        public float LoseTargetRadius => _loseTargetRadius;

        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float AttackDamage => _attackDamage;

        public float FleeHealthThreshold => _fleeHealthThreshold;
        public float FleeSafeDistance => _fleeSafeDistance;
    }
}
