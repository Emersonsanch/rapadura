using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
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
            TickStun(Time.deltaTime);

            bool anyExpired = false;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveStatEffect effect = _activeEffects[i];
                bool shouldTick = effect.Tick(Time.deltaTime, out bool expired);

                if (shouldTick && _combatTarget != null)
                {
                    ApplyTick(effect);
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

        /// <summary>
        /// Applies (or refreshes/extends) a stun, preventing the owner from acting for the given duration
        /// (PlayerController and EnemyController both stop moving/attacking while <see cref="IsStunned"/>).
        /// The duration is first scaled by the owner's <see cref="IStunDurationModifier"/> when it has one
        /// (bosses: <see cref="StunRules.BossStunDurationMultiplier"/>, so they can't be stun-locked).
        /// A stun never shortens one already running. Returns the duration actually applied (0 = none).
        /// </summary>
        public float ApplyStun(float durationSeconds)
        {
            float scaled = StunRules.ScaleDuration(durationSeconds, ResolveStunMultiplier() * Rapadura.Gameplay.Combat.CombatModifierHooks.ResolveStunMultiplier(gameObject));
            if (scaled <= 0f)
            {
                return 0f;
            }

            _stunTimer = Mathf.Max(_stunTimer, scaled);
            EventBus.Publish(new StunAppliedEvent(gameObject, scaled));
            return scaled;
        }

        /// <summary>Counts the stun down without the rest of <c>Update</c> (EditMode tests, and Update itself).</summary>
        public void TickStun(float deltaTime)
        {
            if (_stunTimer > 0f && deltaTime > 0f)
            {
                _stunTimer -= deltaTime;
            }
        }

        private float ResolveStunMultiplier()
        {
            // Looked up per stun (rare event) instead of cached in Awake: EditMode tests never run Awake,
            // and an EnemyController's boss flag can change when a pooled instance is re-spawned.
            return TryGetComponent(out IStunDurationModifier modifier) ? modifier.StunDurationMultiplier : 1f;
        }

        /// <summary>Immediately ends any active stun (e.g. cleansed by a purify effect).</summary>
        public void ClearStun()
        {
            _stunTimer = 0f;
        }

        private void ApplyTick(ActiveStatEffect effect)
        {
            StatModifierDefinition definition = effect.Definition;

            if (definition.tickDamageOrHeal > 0f)
            {
                if (definition.isDebuff)
                {
                    // Source = whoever applied the debuff (falls back to self), on the effect's own channel:
                    // ticks used to come from the victim itself, so a DoT kill granted no XP/loot, and each
                    // tick opened a global i-frame window that swallowed the caster's next hit.
                    GameObject source = effect.Instigator != null ? effect.Instigator : gameObject;
                    OutgoingDamage.DeliverDamage(_combatTarget, definition.tickDamageOrHeal, ElementType.None, source, false, effect.DamageChannel);
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
        public void ApplyEffect(StatModifierDefinition definition, string sourceKey = null, GameObject instigator = null)
        {
            _activeEffects.RemoveAll(e => e.Definition.affectedStat == definition.affectedStat
                                          && e.Definition.isDebuff == definition.isDebuff
                                          && e.Definition.application == definition.application
                                          && e.SourceKey == sourceKey);
            _activeEffects.Add(new ActiveStatEffect(definition, sourceKey, instigator));
            OnModifiersChanged?.Invoke();
        }

        /// <param name="instigator">Who applied them (DoT tick damage source, see <see cref="ActiveStatEffect.Instigator"/>).</param>
        public void ApplyEffects(IEnumerable<StatModifierDefinition> definitions, string sourceKey = null, GameObject instigator = null)
        {
            foreach (StatModifierDefinition definition in definitions)
            {
                ApplyEffect(definition, sourceKey, instigator);
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

        /// <summary>Whether any active effect carries <paramref name="sourceKey"/> (e.g. a meal's effects survived a carried-state restore).</summary>
        public bool HasEffectsFromSource(string sourceKey)
        {
            if (string.IsNullOrEmpty(sourceKey))
            {
                return false;
            }

            foreach (ActiveStatEffect effect in _activeEffects)
            {
                if (effect.SourceKey == sourceKey)
                {
                    return true;
                }
            }

            return false;
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

        /// <summary>Lowest movement multiplier stacked slows can reach without a root (Congelado's -90% lands exactly here).</summary>
        public const float MinMoveSpeedMultiplier = 0.1f;

        /// <summary>Highest movement multiplier stacked haste can reach (Impulso / skill_dash's +150% lands exactly here).</summary>
        public const float MaxMoveSpeedMultiplier = 2.5f;

        /// <summary>
        /// True while a single MoveSpeed effect is -100% or lower (PercentAdditive value &lt;= -1): the
        /// owner is rooted and <see cref="GetMoveSpeedMultiplier"/> returns 0. Several partial slows
        /// stacking past -100% do NOT root — they stop at <see cref="MinMoveSpeedMultiplier"/> — so
        /// chaining two slows (Correntes -70% + Eclipse -50%) can't pin a boss in place.
        /// </summary>
        public bool IsRooted
        {
            get
            {
                foreach (ActiveStatEffect effect in _activeEffects)
                {
                    if (effect.Definition.affectedStat == StatType.MoveSpeed
                        && effect.Definition.application == ModifierApplication.PercentAdditive
                        && effect.Definition.value <= -1f)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Multiplier for the owner's movement speed (PlayerMotor walk/run/crouch/slide/click-to-move and
        /// EnemyController patrol/chase/flee): <c>GetModifiedValue(MoveSpeed, 1)</c> clamped to
        /// [<see cref="MinMoveSpeedMultiplier"/>, <see cref="MaxMoveSpeedMultiplier"/>], or 0 while
        /// <see cref="IsRooted"/>. Skills express MoveSpeed as PercentAdditive (-0.5 = half speed).
        /// MoveSpeed used to be applied by ~15 skills/affixes but read by nothing.
        /// </summary>
        public float GetMoveSpeedMultiplier()
        {
            return ClampMoveSpeedMultiplier(GetModifiedValue(StatType.MoveSpeed, 1f), IsRooted);
        }

        /// <summary>Pure clamp behind <see cref="GetMoveSpeedMultiplier"/> (rooted → 0).</summary>
        public static float ClampMoveSpeedMultiplier(float rawMultiplier, bool rooted)
        {
            return rooted ? 0f : Mathf.Clamp(rawMultiplier, MinMoveSpeedMultiplier, MaxMoveSpeedMultiplier);
        }

        /// <summary>Sum of the active <see cref="ModifierApplication.Flat"/> modifiers on a stat (e.g. armor points from affixes).</summary>
        public float GetFlatTotal(StatType stat)
        {
            return GetTotal(stat, ModifierApplication.Flat);
        }

        /// <summary>Sum of the active <see cref="ModifierApplication.PercentAdditive"/> modifiers on a stat (+0.25 = +25%).</summary>
        public float GetPercentTotal(StatType stat)
        {
            return GetTotal(stat, ModifierApplication.PercentAdditive);
        }

        private float GetTotal(StatType stat, ModifierApplication application)
        {
            float total = 0f;

            foreach (ActiveStatEffect effect in _activeEffects)
            {
                if (effect.Definition.affectedStat == stat && effect.Definition.application == application)
                {
                    total += effect.Definition.value;
                }
            }

            return total;
        }

        /// <summary>
        /// Sum of every active modifier value (flat AND percent) for a stat that is itself a 0-based
        /// fraction — <see cref="StatType.CriticalChance"/>, <see cref="StatType.CriticalDamage"/>,
        /// <see cref="StatType.AttackSpeed"/>, <see cref="StatType.Thorns"/>, <see cref="StatType.CooldownReduction"/>
        /// and <see cref="StatType.ResourceCostReduction"/>.
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
