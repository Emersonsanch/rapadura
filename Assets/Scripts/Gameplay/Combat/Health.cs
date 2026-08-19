using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Generic vitals + damage-reception component for anything that is not the player
    /// (enemies, destructibles, future bosses). Mirrors the shape of <c>PlayerStats</c>'s health
    /// handling but is standalone so non-player entities don't need to drag in stamina/mana/XP.
    /// Implements <see cref="ICombatTarget"/> so hitboxes, skills and AI can all damage/heal it
    /// through the same contract regardless of concrete type.
    ///
    /// Owns invulnerability frames: after taking damage the target is briefly immune to further
    /// damage (<see cref="_invulnerabilityDuration"/>), which is standard game-feel to avoid a
    /// single overlapping hitbox instantly deleting a health bar across several trigger frames.
    /// </summary>
    public class Health : MonoBehaviour, ICombatTarget
    {
        [Header("Vitals")]
        [SerializeField] private float _maxHealth = 50f;
        [SerializeField] private float _defense = 0f;

        [Header("Balance")]
        [SerializeField] private DamageBalanceConfig _balanceConfig;

        [Header("Invulnerability")]
        [Tooltip("Seconds of damage immunity granted right after taking a hit.")]
        [SerializeField] private float _invulnerabilityDuration = 0.5f;

        private float _invulnerabilityTimer;

        /// <summary>Optional — absence means no resistance/immunity to any element (see <see cref="ElementResistance"/>).</summary>
        private ElementResistance _elementResistance;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => _maxHealth;
        public bool IsDead { get; private set; }
        public bool IsAlive => !IsDead;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;
        public Transform Transform => transform;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            _elementResistance = GetComponent<ElementResistance>();
        }

        private void Update()
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= Time.deltaTime;
            }
        }

        /// <summary>Grants temporary damage immunity, refreshing/extending any window already in progress.</summary>
        public void GrantInvulnerability(float duration)
        {
            _invulnerabilityTimer = Mathf.Max(_invulnerabilityTimer, duration);
        }

        /// <summary>Immediately ends any active invulnerability window.</summary>
        public void ClearInvulnerability()
        {
            _invulnerabilityTimer = 0f;
        }

        /// <summary>Applies a fully-resolved damage instance (post mitigation), honoring i-frames and raising events.</summary>
        public void ApplyDamage(DamageInfo info)
        {
            if (IsDead || IsInvulnerable || info.Amount <= 0f)
            {
                return;
            }

            float finalDamage = DamageCalculator.ComputeDamage(info, _defense, _balanceConfig, _elementResistance);
            SetHealth(CurrentHealth - finalDamage);
            GrantInvulnerability(_invulnerabilityDuration);

            EventBus.Publish(new DamageAppliedEvent(gameObject, info.Source, finalDamage, info.Element, info.IsCritical));

            if (CurrentHealth <= 0f)
            {
                Die(info.Source);
            }
        }

        /// <summary>ICombatTarget entry point — wraps the raw amount/element into a <see cref="DamageInfo"/> with no knockback.</summary>
        public void ApplyDamage(float amount, ElementType element, GameObject source)
        {
            ApplyDamage(DamageInfo.Simple(amount, element, source));
        }

        public void ApplyHealing(float amount, GameObject source)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            SetHealth(CurrentHealth + amount);
        }

        private void SetHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0f, _maxHealth);
        }

        private void Die(GameObject killer)
        {
            IsDead = true;
            EventBus.Publish(new CombatTargetDiedEvent(gameObject, killer));
        }
    }
}
