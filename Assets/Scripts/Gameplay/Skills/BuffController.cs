using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Tracks every buff/debuff currently active on a character, ticks their durations and
    /// damage/heal-over-time effects, and exposes the aggregated stat modifiers so other
    /// systems (PlayerMotor for move speed, combat math for attack/defense) can query the
    /// character's effective stats without knowing anything about skills.
    /// Attach alongside any component that implements <see cref="ICombatTarget"/>.
    /// </summary>
    public class BuffController : MonoBehaviour
    {
        private readonly List<ActiveStatEffect> _activeEffects = new List<ActiveStatEffect>();
        private ICombatTarget _combatTarget;
        private float _stunTimer;

        /// <summary>True while a stun effect is active — consumed by <see cref="Rapadura.Gameplay.Player.PlayerController"/>
        /// (and any other actor with a BuffController) to block movement/attack input processing for the frame.</summary>
        public bool IsStunned => _stunTimer > 0f;

        /// <summary>Seconds of stun remaining, clamped to zero (never negative).</summary>
        public float StunTimeRemaining => Mathf.Max(0f, _stunTimer);

        /// <summary>
        /// Raised whenever the set of active modifiers changes (applied, removed, cleared or expired),
        /// so derived values cached elsewhere — e.g. <c>PlayerStats</c> clamping current health/mana
        /// when a MaxHealth/MaxMana bonus goes away — can refresh without polling every frame.
        /// </summary>
        public event Action OnModifiersChanged;

        private void Awake()
        {
            _combatTarget = GetComponent<ICombatTarget>();
        }

        private void Update()
        {
            if (_stunTimer > 0f)
            {
                _stunTimer -= Time.deltaTime;
            }

            bool anyExpired = false;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveStatEffect effect = _activeEffects[i];
                bool shouldTick = effect.Tick(Time.deltaTime, out bool expired);

                if (shouldTick && _combatTarget != null)
                {
                    ApplyTick(effect.Definition);
                }

                if (expired)
                {
                    _activeEffects.RemoveAt(i);
                    anyExpired = true;
                }
            }

            if (anyExpired)
            {
                OnModifiersChanged?.Invoke();
            }
        }

        /// <summary>Applies (or refreshes/extends) a stun, preventing the owner from acting for the given duration.</summary>
        public void ApplyStun(float durationSeconds)
        {
            if (durationSeconds <= 0f)
            {
                return;
            }

            _stunTimer = Mathf.Max(_stunTimer, durationSeconds);
        }

        /// <summary>Immediately ends any active stun (e.g. cleansed by a purify effect).</summary>
        public void ClearStun()
        {
            _stunTimer = 0f;
        }

        private void ApplyTick(StatModifierDefinition definition)
        {
            if (definition.tickDamageOrHeal > 0f)
            {
                if (definition.isDebuff)
                {
                    _combatTarget.ApplyDamage(definition.tickDamageOrHeal, ElementType.None, gameObject);
                }
                else
                {
                    _combatTarget.ApplyHealing(definition.tickDamageOrHeal, gameObject);
                }
            }
        }

        /// <summary>
        /// Applies a new buff/debuff, replacing any existing instance of the same stat+debuff-flag combo
        /// <b>from the same source</b> (recasting a skill refreshes its own buff instead of stacking).
        /// Effects from different sources stack: before, the replacement ignored the source, so e.g.
        /// equipping an <c>affix_vital</c> item silently wiped the Vitality attribute's MaxHealth bonus
        /// (<c>AttributeSet</c>, source "attributes") and casting an AttackDamage buff permanently
        /// erased Poder Arcano's — they share the stat but not the source. The modifier's
        /// <see cref="ModifierApplication"/> is part of the identity too: a Flat and a Percent bonus on
        /// the same stat from one source are different effects, not a refresh of each other.
        /// </summary>
        public void ApplyEffect(StatModifierDefinition definition, string sourceKey = null)
        {
            _activeEffects.RemoveAll(e => e.Definition.affectedStat == definition.affectedStat
                                          && e.Definition.isDebuff == definition.isDebuff
                                          && e.Definition.application == definition.application
                                          && e.SourceKey == sourceKey);
            _activeEffects.Add(new ActiveStatEffect(definition, sourceKey));
            OnModifiersChanged?.Invoke();
        }

        public void ApplyEffects(IEnumerable<StatModifierDefinition> definitions, string sourceKey = null)
        {
            foreach (StatModifierDefinition definition in definitions)
            {
                ApplyEffect(definition, sourceKey);
            }
        }

        /// <summary>Removes every effect tagged with the given source key (e.g. all stat bonuses from one unequipped item).</summary>
        public void RemoveEffectsBySource(string sourceKey)
        {
            if (string.IsNullOrEmpty(sourceKey))
            {
                return;
            }

            if (_activeEffects.RemoveAll(e => e.SourceKey == sourceKey) > 0)
            {
                OnModifiersChanged?.Invoke();
            }
        }

        /// <summary>
        /// Removes every effect whose source key starts with <paramref name="sourceKeyPrefix"/> — e.g.
        /// "equip_Chest/" drops an unequipped item's base modifiers and rolled affixes, which live under
        /// their own sub-keys so they can't replace each other.
        /// </summary>
        public void RemoveEffectsBySourcePrefix(string sourceKeyPrefix)
        {
            if (string.IsNullOrEmpty(sourceKeyPrefix))
            {
                return;
            }

            if (_activeEffects.RemoveAll(e => e.SourceKey != null && e.SourceKey.StartsWith(sourceKeyPrefix, StringComparison.Ordinal)) > 0)
            {
                OnModifiersChanged?.Invoke();
            }
        }

        public void ClearAllEffects()
        {
            if (_activeEffects.Count == 0)
            {
                return;
            }

            _activeEffects.Clear();
            OnModifiersChanged?.Invoke();
        }

        /// <summary>Sums every active flat + percent modifier for the given stat, applied to a base value.</summary>
        public float GetModifiedValue(StatType stat, float baseValue)
        {
            float flatTotal = 0f;
            float percentTotal = 0f;

            foreach (ActiveStatEffect effect in _activeEffects)
            {
                if (effect.Definition.affectedStat != stat)
                {
                    continue;
                }

                if (effect.Definition.application == ModifierApplication.Flat)
                {
                    flatTotal += effect.Definition.value;
                }
                else
                {
                    percentTotal += effect.Definition.value;
                }
            }

            return (baseValue + flatTotal) * (1f + percentTotal);
        }

        /// <summary>
        /// Sum of every active modifier value (flat AND percent) for a stat that is itself a 0-based
        /// fraction — <see cref="StatType.CriticalChance"/>, <see cref="StatType.CriticalDamage"/>.
        /// <see cref="GetModifiedValue"/> would drop the percent ones for these: (0 + flat) × (1 + percent)
        /// ignores a percent bonus on a zero base, and the procedural affixes (<c>AffixDatabaseSeeder</c>)
        /// roll crit as PercentAdditive while Destreza applies it as Flat. Here "+3%" means +0.03 either way.
        /// </summary>
        public float GetAdditiveFractionTotal(StatType stat)
        {
            float total = 0f;

            foreach (ActiveStatEffect effect in _activeEffects)
            {
                if (effect.Definition.affectedStat == stat)
                {
                    total += effect.Definition.value;
                }
            }

            return total;
        }
    }
}
