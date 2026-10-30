using System.Collections.Generic;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The special ability of a mount (Docs/Mounts.md §12.2): one per species, its own cooldown. Data as asset like
    /// <c>SkillDefinition</c>; the starters are also built in code (<see cref="MountCatalog"/>) so baking the assets
    /// (menu "Rapadura/Mounts/Bake Starter Mount Assets") is optional. What each <see cref="MountAbilityKind"/> does is
    /// <c>MountAbilityExecutor</c>'s business; the generic parameters below are read by it and by the server checks of
    /// §9.2 (<see cref="ServerMaxDistance"/>...).
    /// </summary>
    [CreateAssetMenu(fileName = "NewMountAbility", menuName = "Rapadura/Mounts/Mount Ability Definition", order = 1)]
    public class MountAbilityDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _abilityId = "mount_ability_new";
        [SerializeField] private string _nameKey = "mount.ability.new.name";
        [SerializeField] private string _descriptionKey = "mount.ability.new.desc";
        [SerializeField] private Sprite _icon;
        [SerializeField] private MountAbilityKind _kind = MountAbilityKind.Dash;

        [Header("Parameters")]
        [SerializeField, Min(0f)] private float _cooldownSeconds = 12f;
        [SerializeField, Min(0f)] private float _durationSeconds;
        [SerializeField, Min(0f)] private float _distance;
        [SerializeField, Min(0f)] private float _radius;
        [SerializeField] private float _magnitude;
        [SerializeField] private List<StatModifierDefinition> _statModifiers = new List<StatModifierDefinition>();

        [Header("Rules")]
        [Tooltip("Always false (Docs/Mounts.md §2.5): no ability works inside a city.")]
        [SerializeField] private bool _allowedInSafeZone;
        [Tooltip("0.5 for crowd control (StunRules), 0 for fear and knockback (bosses ignore them).")]
        [SerializeField, Range(0f, 1f)] private float _bossMultiplier;

        [Header("Server validation (§9.2)")]
        [SerializeField, Min(0f)] private float _serverMaxDistance;
        [SerializeField, Min(0f)] private float _serverMaxDuration;
        [SerializeField, Min(0f)] private float _serverMaxAltitude;

        public string AbilityId => _abilityId;
        public string NameKey => _nameKey;
        public string DescriptionKey => _descriptionKey;
        public Sprite Icon => _icon;
        public MountAbilityKind Kind => _kind;
        public float CooldownSeconds => _cooldownSeconds;
        public float DurationSeconds => _durationSeconds;
        public float Distance => _distance;
        public float Radius => _radius;
        public float Magnitude => _magnitude;
        public IReadOnlyList<StatModifierDefinition> StatModifiers => _statModifiers;
        public bool AllowedInSafeZone => _allowedInSafeZone;
        public float BossMultiplier => _bossMultiplier;
        public float ServerMaxDistance => _serverMaxDistance;
        public float ServerMaxDuration => _serverMaxDuration;
        public float ServerMaxAltitude => _serverMaxAltitude;

        /// <summary>Code-built ability (the starter catalog, tests). Lives in memory only.</summary>
        public static MountAbilityDefinition CreateRuntime(string abilityId, MountAbilityKind kind, float cooldownSeconds,
            float durationSeconds = 0f, float distance = 0f, float radius = 0f, float magnitude = 0f, float bossMultiplier = 0f)
        {
            var ability = CreateInstance<MountAbilityDefinition>();
            ability.name = abilityId;
            ability.hideFlags = HideFlags.DontUnloadUnusedAsset;
            ability.Configure(abilityId, kind, cooldownSeconds, durationSeconds, distance, radius, magnitude, bossMultiplier);
            return ability;
        }

        /// <summary>Fills every field from code (runtime catalog and the editor baker).</summary>
        public void Configure(string abilityId, MountAbilityKind kind, float cooldownSeconds, float durationSeconds, float distance,
            float radius, float magnitude, float bossMultiplier)
        {
            _abilityId = abilityId;
            _nameKey = "mount.ability." + abilityId + ".name";
            _descriptionKey = "mount.ability." + abilityId + ".desc";
            _kind = kind;
            _cooldownSeconds = Mathf.Max(0f, cooldownSeconds);
            _durationSeconds = Mathf.Max(0f, durationSeconds);
            _distance = Mathf.Max(0f, distance);
            _radius = Mathf.Max(0f, radius);
            _magnitude = magnitude;
            _bossMultiplier = Mathf.Clamp01(bossMultiplier);
            _allowedInSafeZone = false;
            _serverMaxDistance = _distance;
            _serverMaxDuration = _durationSeconds;
            _serverMaxAltitude = 0f;
        }
    }
}
