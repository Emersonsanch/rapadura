using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Data-driven knobs for the player's outgoing damage that used to be serialized straight into the
    /// scene (<c>PlayerBasicAttack._damage</c> lives in <c>Main.unity</c>) or didn't exist at all
    /// (critical hits). Same pattern as <see cref="Rapadura.Gameplay.Player.ProgressionCurve"/>: the
    /// shipped asset at <c>Assets/Resources/Balance/PlayerCombatBalance.asset</c> is picked up
    /// automatically through <see cref="LoadDefault"/>; when it's missing, every consumer falls back to
    /// its own inspector value / the defaults below, so deleting the asset reverts the rebalance.
    /// See <c>Docs/Balance/README.md</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerCombatBalance", menuName = "Rapadura/Balance/Player Combat Balance")]
    public class PlayerCombatBalance : ScriptableObject
    {
        /// <summary>Path (relative to a Resources folder) of the shipped asset.</summary>
        public const string ResourcesPath = "Balance/PlayerCombatBalance";

        /// <summary>Critical multiplier used when no asset exists (matches <see cref="DamageBalanceConfig"/>'s default).</summary>
        public const float DefaultCriticalMultiplier = 1.5f;

        [Header("Basic Attack (PlayerBasicAttack)")]
        [Tooltip("Base damage per basic-attack hit, before AttackDamage buffs, the level damage multiplier and crits. Overrides PlayerBasicAttack's own inspector value.")]
        [Min(0f)]
        [SerializeField] private float _basicAttackDamage = 15f;

        [Header("Critical Hits (basic attack + skills)")]
        [Tooltip("Critical chance (0..1) every character has before Destreza / CriticalChance modifiers.")]
        [Range(0f, 1f)]
        [SerializeField] private float _baseCriticalChance = 0f;

        [Tooltip("Damage multiplier of a critical hit before CriticalDamage modifiers (each +0.1 of CriticalDamage adds +0.1x).")]
        [Min(1f)]
        [SerializeField] private float _baseCriticalMultiplier = DefaultCriticalMultiplier;

        public float BasicAttackDamage => _basicAttackDamage;
        public float BaseCriticalChance => _baseCriticalChance;
        public float BaseCriticalMultiplier => _baseCriticalMultiplier;

        /// <summary>Loads the shipped asset from Resources, or null if it doesn't exist. Callers cache the result.</summary>
        public static PlayerCombatBalance LoadDefault()
        {
            return Resources.Load<PlayerCombatBalance>(ResourcesPath);
        }

#if UNITY_EDITOR
        /// <summary>Editor/test-only: builds an in-memory instance with explicit values.</summary>
        public static PlayerCombatBalance CreateForTests(float basicAttackDamage, float baseCriticalChance, float baseCriticalMultiplier)
        {
            var balance = CreateInstance<PlayerCombatBalance>();
            balance._basicAttackDamage = basicAttackDamage;
            balance._baseCriticalChance = baseCriticalChance;
            balance._baseCriticalMultiplier = baseCriticalMultiplier;
            return balance;
        }
#endif
    }
}
