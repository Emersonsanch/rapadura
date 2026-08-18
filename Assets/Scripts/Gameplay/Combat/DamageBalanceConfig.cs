using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Data-driven balance knobs for <see cref="DamageCalculator"/>. Living as a ScriptableObject
    /// asset means designers can retune the armor curve, minimum-damage floor and crit multiplier
    /// without touching code or rebuilding the game — the same "balanceável via dados externos"
    /// approach already used by <see cref="Rapadura.Gameplay.Skills.SkillDefinition"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultDamageBalance", menuName = "Rapadura/Combat/Damage Balance Config", order = 0)]
    public class DamageBalanceConfig : ScriptableObject
    {
        [Header("Armor Curve")]
        [Tooltip("Higher values make defense less effective (diminishing returns constant in the mitigation formula).")]
        [SerializeField] private float _armorConstant = 100f;

        [Header("Floors & Multipliers")]
        [Tooltip("A hit can never deal less than this, so high-defense targets are never fully immune to chip damage.")]
        [SerializeField] private float _minimumDamage = 1f;
        [SerializeField] private float _criticalMultiplier = 1.5f;

        public float ArmorConstant => _armorConstant;
        public float MinimumDamage => _minimumDamage;
        public float CriticalMultiplier => _criticalMultiplier;
    }
}
