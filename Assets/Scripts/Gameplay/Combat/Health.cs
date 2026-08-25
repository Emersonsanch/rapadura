using Rapadura.Core.Events;
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
        private bool _initialized;

        /// <summary>Optional — absence means no resistance/immunity to any element (see <see cref="ElementResistance"/>).</summary>
        private ElementResistance _elementResistance;

        private float _currentHealth;

        public float CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return _currentHealth;
            }
            private set => _currentHealth = value;
        }

        public float MaxHealth => _maxHealth;
        public bool IsDead { get; private set; }
        public bool IsAlive => !IsDead;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;
        public Transform Transform => transform;

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Guards <see cref="ApplyDamage(DamageInfo)"/>/<see cref="ApplyHealing"/> against being
        /// called before <see cref="Awake"/> has run — in Edit Mode (outside Play Mode),
        /// <c>AddComponent&lt;Health&gt;()</c> does NOT call <c>Awake</c> synchronously the way
        /// Play Mode always does, which left <see cref="CurrentHealth"/> at its default 0 for any
        /// EditMode test that added the component and damaged/healed it in the same call (0
        /// clamped to [0, max] never actually decreases, so "damage reduces health" assertions
        /// failed). Uses an explicit flag rather than re-running on every call so it doesn't
        /// clobber <see cref="CurrentHealth"/> back to full after real damage has already been
        /// dealt (0 health post-lethal-hit is a valid, meaningful state, not "uninitialized").
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
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
            EnsureInitialized();

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
            EnsureInitialized();

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
