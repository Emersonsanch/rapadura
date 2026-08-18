using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Data-driven description of a skill combo: an ordered sequence of skills that, if cast one
    /// after another with no more than <see cref="WindowSeconds"/> between each consecutive pair,
    /// grants a bonus. Entirely designer-authored — <see cref="ComboTracker"/> contains no
    /// per-combo special-casing, so new combos never require a code change.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCombo", menuName = "Rapadura/Skills/Combo Definition", order = 2)]
    public class ComboDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _comboId = "combo_new";
        [SerializeField] private string _displayName = "New Combo";
        [TextArea(2, 4)]
        [SerializeField] private string _description = string.Empty;

        [Header("Sequence")]
        [Tooltip("Skills must be cast in this exact order.")]
        [SerializeField] private SkillDefinition[] _sequence = System.Array.Empty<SkillDefinition>();
        [Tooltip("Maximum seconds allowed between two consecutive casts in the sequence before progress resets.")]
        [SerializeField] private float _windowSeconds = 2.5f;

        [Header("Bonus")]
        [SerializeField] private ComboBonusType _bonusType = ComboBonusType.DamageMultiplier;
        [Tooltip("Percent value for DamageMultiplier/CostReduction (e.g. 0.25 = +25% damage or -25% cost). Unused for UnlockSkill.")]
        [SerializeField] private float _bonusValue = 0.25f;
        [Tooltip("How long the DamageMultiplier/CostReduction buff lasts once the combo completes. Unused for UnlockSkill.")]
        [SerializeField] private float _bonusDurationSeconds = 5f;
        [Tooltip("Skill instantly learned for free when this combo completes. Only used when Bonus Type is UnlockSkill.")]
        [SerializeField] private SkillDefinition _unlockSkill;

        public string ComboId => _comboId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public SkillDefinition[] Sequence => _sequence;
        public float WindowSeconds => _windowSeconds;
        public ComboBonusType BonusType => _bonusType;
        public float BonusValue => _bonusValue;
        public float BonusDurationSeconds => _bonusDurationSeconds;
        public SkillDefinition UnlockSkill => _unlockSkill;
    }
}
