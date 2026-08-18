using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Data-driven definition of a single skill/ability. Every gameplay-facing property a
    /// designer needs to tune lives here as a ScriptableObject asset, so new skills can be
    /// created and balanced entirely from the Unity Editor (or the custom Skill Editor window)
    /// without touching code. SkillManager reads these assets at cast time; it never hardcodes
    /// per-skill behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "Rapadura/Skills/Skill Definition", order = 0)]
    public class SkillDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _skillId = "skill_new";
        [SerializeField] private string _displayName = "New Skill";
        [TextArea(2, 5)]
        [SerializeField] private string _description = string.Empty;
        [SerializeField] private Sprite _icon;

        [Header("Classification")]
        [SerializeField] private SkillType _skillType = SkillType.Active;
        [SerializeField] private SkillCategory _category = SkillCategory.Offensive;
        [SerializeField] private ElementType _element = ElementType.None;

        [Header("Costs & Timing")]
        [SerializeField] private float _cooldownSeconds = 3f;
        [SerializeField] private float _manaCost = 10f;
        [SerializeField] private float _energyCost = 0f;
        [SerializeField] private float _castTimeSeconds = 0f;
        [SerializeField] private float _recoverySeconds = 0.2f;

        [Header("Power")]
        [SerializeField] private float _baseDamage = 0f;
        [SerializeField] private float _baseHealing = 0f;
        [SerializeField] private float _range = 5f;
        [SerializeField] private float _areaRadius = 0f;
        [SerializeField] private float _damagePerLevel = 0f;
        [SerializeField] private float _healingPerLevel = 0f;

        [Header("Status Effects")]
        [SerializeField] private StatModifierDefinition[] _buffs = System.Array.Empty<StatModifierDefinition>();
        [SerializeField] private StatModifierDefinition[] _debuffs = System.Array.Empty<StatModifierDefinition>();

        [Header("Presentation")]
        [SerializeField] private string _animationTrigger = string.Empty;
        [SerializeField] private AudioClip _castSound;
        [SerializeField] private GameObject _castParticlesPrefab;
        [SerializeField] private GameObject _impactVfxPrefab;

        [Header("Progression")]
        [SerializeField] private int _maxLevel = 5;
        [SerializeField] private int _experienceToUnlock = 0;
        [SerializeField] private SkillRequirement _requirement = new SkillRequirement();

        public string SkillId => _skillId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;

        public SkillType SkillType => _skillType;
        public SkillCategory Category => _category;
        public ElementType Element => _element;

        public float CooldownSeconds => _cooldownSeconds;
        public float ManaCost => _manaCost;
        public float EnergyCost => _energyCost;
        public float CastTimeSeconds => _castTimeSeconds;
        public float RecoverySeconds => _recoverySeconds;

        public float BaseDamage => _baseDamage;
        public float BaseHealing => _baseHealing;
        public float Range => _range;
        public float AreaRadius => _areaRadius;

        public StatModifierDefinition[] Buffs => _buffs;
        public StatModifierDefinition[] Debuffs => _debuffs;

        public string AnimationTrigger => _animationTrigger;
        public AudioClip CastSound => _castSound;
        public GameObject CastParticlesPrefab => _castParticlesPrefab;
        public GameObject ImpactVfxPrefab => _impactVfxPrefab;

        public int MaxLevel => _maxLevel;
        public int ExperienceToUnlock => _experienceToUnlock;
        public SkillRequirement Requirement => _requirement;

        /// <summary>Damage this skill deals when cast at the given skill level (1-based).</summary>
        public float GetDamageForLevel(int level)
        {
            return _baseDamage + _damagePerLevel * Mathf.Max(0, level - 1);
        }

        /// <summary>Healing this skill provides when cast at the given skill level (1-based).</summary>
        public float GetHealingForLevel(int level)
        {
            return _baseHealing + _healingPerLevel * Mathf.Max(0, level - 1);
        }
    }
}
