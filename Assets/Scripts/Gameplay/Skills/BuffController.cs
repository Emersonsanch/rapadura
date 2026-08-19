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
                }
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

        /// <summary>Applies a new buff/debuff, replacing any existing instance of the same stat+debuff-flag combo.</summary>
        public void ApplyEffect(StatModifierDefinition definition, string sourceKey = null)
        {
            _activeEffects.RemoveAll(e => e.Definition.affectedStat == definition.affectedStat && e.Definition.isDebuff == definition.isDebuff);
            _activeEffects.Add(new ActiveStatEffect(definition, sourceKey));
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

            _activeEffects.RemoveAll(e => e.SourceKey == sourceKey);
        }

        public void ClearAllEffects()
        {
            _activeEffects.Clear();
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
    }
}
