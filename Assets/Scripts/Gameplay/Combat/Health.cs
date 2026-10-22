using System.Collections.Generic;
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
    /// Anti-double-hit windows are per damage <b>channel</b> (attacker + <see cref="DamageInfo.Channel"/>),
    /// not global: after a hit, only that same source's same attack/skill is ignored for
    /// <see cref="_sameChannelHitWindow"/> seconds, which stops one cast's overlapping checks counting
    /// twice. A global 0.5 s window used to equal the basic-attack cooldown, so any skill cast within
    /// 0.5 s of a hit (or a DoT tick) dealt nothing — ~40% of ranged heroes' skill casts were wasted.
    /// Whole-target immunity is still available explicitly via <see cref="GrantInvulnerability"/>;
    /// the player's own post-hit i-frames live in <c>PlayerStats</c>.
    /// </summary>
    public class Health : MonoBehaviour, ICombatTarget, ICriticalAwareTarget, IChanneledDamageTarget
    {
        [Header("Vitals")]
        [SerializeField] private float _maxHealth = 50f;
        [SerializeField] private float _defense = 0f;

        [Header("Balance")]
        [SerializeField] private DamageBalanceConfig _balanceConfig;

        [Header("Invulnerability")]
        [Tooltip("Seconds the SAME source's SAME damage channel (basic attack / one skill / one DoT) is ignored after it lands. Other channels and other attackers still hit.")]
        [SerializeField] private float _sameChannelHitWindow = 0.2f;

        /// <summary>Explicit whole-target immunity (<see cref="GrantInvulnerability"/>); hits never start it.</summary>
        private float _invulnerabilityTimer;

        private struct ChannelWindow
        {
            public int SourceId;
            public int Channel;
            public float Remaining;
        }

        private readonly List<ChannelWindow> _channelWindows = new List<ChannelWindow>(4);
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
        /// <summary>Whole-target immunity from <see cref="GrantInvulnerability"/> (not from regular hits — see <see cref="IsChannelOnCooldown"/>).</summary>
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
            TickInvulnerability(Time.deltaTime);
        }

        /// <summary>Advances the whole-target and per-channel windows (Update drives it; tests call it directly).</summary>
        public void TickInvulnerability(float deltaTime)
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= deltaTime;
            }

            for (int i = _channelWindows.Count - 1; i >= 0; i--)
            {
                ChannelWindow window = _channelWindows[i];
                window.Remaining -= deltaTime;

                if (window.Remaining <= 0f)
                {
                    _channelWindows.RemoveAt(i);
                }
                else
                {
                    _channelWindows[i] = window;
                }
            }
        }

        /// <summary>True while <paramref name="source"/>'s <paramref name="channel"/> hit this target too recently to land again.</summary>
        public bool IsChannelOnCooldown(GameObject source, int channel)
        {
            return FindChannelWindow(SourceIdOf(source), channel) >= 0;
        }

        private static int SourceIdOf(GameObject source)
        {
            return source != null ? source.GetInstanceID() : 0;
        }

        private int FindChannelWindow(int sourceId, int channel)
        {
            for (int i = 0; i < _channelWindows.Count; i++)
            {
                if (_channelWindows[i].SourceId == sourceId && _channelWindows[i].Channel == channel)
                {
                    return i;
                }
            }

            return -1;
        }

        private void StartChannelWindow(int sourceId, int channel)
        {
            if (_sameChannelHitWindow <= 0f)
            {
                return;
            }

            _channelWindows.Add(new ChannelWindow { SourceId = sourceId, Channel = channel, Remaining = _sameChannelHitWindow });
        }

        /// <summary>Grants temporary damage immunity, refreshing/extending any window already in progress.</summary>
        public void GrantInvulnerability(float duration)
        {
            _invulnerabilityTimer = Mathf.Max(_invulnerabilityTimer, duration);
        }

        /// <summary>Immediately ends any active invulnerability window, whole-target and per-channel.</summary>
        public void ClearInvulnerability()
        {
            _invulnerabilityTimer = 0f;
            _channelWindows.Clear();
        }

        /// <summary>Applies a fully-resolved damage instance (post mitigation), honoring i-frames and raising events.</summary>
        public void ApplyDamage(DamageInfo info)
        {
            ApplyDamageInternal(info, info.IsCritical);
        }

        /// <summary>
        /// <see cref="ICriticalAwareTarget"/>: the attacker already multiplied <paramref name="amount"/> by
        /// its crit multiplier (<see cref="OutgoingDamage.Resolve"/>), so the hit is computed as a normal
        /// one and only the published event is flagged critical (for the crit FX/SFX).
        /// </summary>
        public void ApplyResolvedCriticalDamage(float amount, ElementType element, GameObject source)
        {
            ApplyDamageInternal(DamageInfo.Simple(amount, element, source), displayAsCritical: true);
        }

        /// <summary>
        /// <see cref="IChanneledDamageTarget"/>: an already-resolved amount (crit folded in by
        /// <see cref="OutgoingDamage.Resolve"/>) tagged with the attacker's damage channel.
        /// </summary>
        public void ApplyChanneledDamage(float amount, ElementType element, GameObject source, bool isCritical, int channel)
        {
            ApplyDamageInternal(DamageInfo.Simple(amount, element, source, channel), displayAsCritical: isCritical);
        }

        private void ApplyDamageInternal(DamageInfo info, bool displayAsCritical)
        {
            EnsureInitialized();

            if (IsDead || IsInvulnerable || info.Amount <= 0f)
            {
                return;
            }

            int sourceId = SourceIdOf(info.Source);
            if (FindChannelWindow(sourceId, info.Channel) >= 0)
            {
                return;
            }

            float finalDamage = DamageCalculator.ComputeDamage(info, _defense, _balanceConfig, _elementResistance);
            SetHealth(CurrentHealth - finalDamage);
            StartChannelWindow(sourceId, info.Channel);

            EventBus.Publish(new DamageAppliedEvent(gameObject, info.Source, finalDamage, info.Element, displayAsCritical));

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

        /// <summary>
        /// Reconfigures max health and fully revives at that new max — used by
        /// <see cref="Rapadura.Gameplay.Enemies.EnemyController"/> to apply a data-driven
        /// <see cref="Rapadura.Gameplay.Enemies.EnemyDefinition"/>'s stats onto a shared
        /// prefab/controller (e.g. turning a common enemy into a tougher "boss" variant) without
        /// reflection into private fields.
        /// </summary>
        public void ConfigureMaxHealth(float maxHealth)
        {
            EnsureInitialized();
            _maxHealth = maxHealth;
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        private void Die(GameObject killer)
        {
            IsDead = true;
            EventBus.Publish(new CombatTargetDiedEvent(gameObject, killer));
        }
    }
}
