using System;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Shared "attacker side" damage pipeline for the player's basic attack and skills, so both scale
    /// the same way:
    /// <code>
    /// damage   = base × levelMultiplier                    // ProgressionCurve damage_mult (1 by default)
    /// damage   = BuffController.GetModifiedValue(AttackDamage, damage)   // Poder Arcano, Berserk, combos, affixes
    /// chance   = clamp01(baseChance + Σ CriticalChance)    // Destreza (Flat) + affixes (PercentAdditive) both count
    /// isCrit   = roll01 &lt; chance                         // roll injected for deterministic tests
    /// damage  *= isCrit ? baseMultiplier + Σ CriticalDamage : 1
    /// </code>
    /// The target still applies its own defense/resistance (<see cref="DamageCalculator"/>); the crit
    /// is already folded into the amount, and <see cref="DeliverDamage"/> only forwards the flag so the
    /// hit FX/audio (<see cref="DamageAppliedEvent.IsCritical"/>) can react without multiplying twice.
    /// </summary>
    public static class OutgoingDamage
    {
        /// <summary>Default random source (Unity's global RNG, [0, 1]). Components expose an injectable override.</summary>
        public static readonly Func<float> UnityRandom = () => UnityEngine.Random.value;

        /// <summary>Critical chance (0..1) from the balance base plus every CriticalChance modifier.</summary>
        public static float GetCriticalChance(BuffController buffs, PlayerCombatBalance balance)
        {
            float chance = balance != null ? balance.BaseCriticalChance : 0f;

            if (buffs != null)
            {
                chance += buffs.GetAdditiveFractionTotal(StatType.CriticalChance);
            }

            return Mathf.Clamp01(chance);
        }

        /// <summary>Damage multiplier of a critical hit: the balance base plus every CriticalDamage modifier (never below 1).</summary>
        public static float GetCriticalMultiplier(BuffController buffs, PlayerCombatBalance balance)
        {
            float multiplier = balance != null ? balance.BaseCriticalMultiplier : PlayerCombatBalance.DefaultCriticalMultiplier;

            if (buffs != null)
            {
                multiplier += buffs.GetAdditiveFractionTotal(StatType.CriticalDamage);
            }

            return Mathf.Max(1f, multiplier);
        }

        /// <summary>
        /// Pure crit roll. <paramref name="roll01"/> is a uniform sample in [0, 1]; a chance of 0 never
        /// crits and a chance of 1 always does (even for a roll of exactly 1).
        /// </summary>
        public static bool RollCritical(float chance, float roll01)
        {
            if (chance <= 0f)
            {
                return false;
            }

            return chance >= 1f || roll01 < chance;
        }

        /// <summary>
        /// Runs <paramref name="baseDamage"/> through the full pipeline described on the class.
        /// <paramref name="random"/> null means <see cref="UnityRandom"/>; it is only sampled when the
        /// crit chance is above zero, so a build with no crit sources stays fully deterministic.
        /// </summary>
        public static float Resolve(float baseDamage, float levelMultiplier, BuffController buffs, PlayerCombatBalance balance,
            Func<float> random, out bool isCritical)
        {
            isCritical = false;

            if (baseDamage <= 0f)
            {
                return 0f;
            }

            float damage = baseDamage * Mathf.Max(0f, levelMultiplier);

            if (buffs != null)
            {
                damage = buffs.GetModifiedValue(StatType.AttackDamage, damage);
            }

            float chance = GetCriticalChance(buffs, balance);
            if (chance > 0f)
            {
                isCritical = RollCritical(chance, (random ?? UnityRandom)());
            }

            if (isCritical)
            {
                damage *= GetCriticalMultiplier(buffs, balance);
            }

            return Mathf.Max(0f, damage);
        }

        /// <summary>
        /// Hands a resolved amount to a target, forwarding the crit flag when the target can display it
        /// (<see cref="ICriticalAwareTarget"/>: <see cref="Health"/>, <c>PlayerStats</c>); any other
        /// <see cref="ICombatTarget"/> just gets the plain amount. <paramref name="channel"/>
        /// (<see cref="DamageChannel"/>) reaches targets that key their anti-double-hit window by it
        /// (<see cref="IChanneledDamageTarget"/>: <see cref="Health"/>), so a skill landing right after a
        /// basic attack or another skill is not swallowed.
        /// </summary>
        public static void DeliverDamage(ICombatTarget target, float amount, ElementType element, GameObject source, bool isCritical,
            int channel = DamageChannel.Default)
        {
            if (target == null)
            {
                return;
            }

            if (target is IChanneledDamageTarget channeled)
            {
                channeled.ApplyChanneledDamage(amount, element, source, isCritical, channel);
                return;
            }

            if (isCritical && target is ICriticalAwareTarget criticalAware)
            {
                criticalAware.ApplyResolvedCriticalDamage(amount, element, source);
                return;
            }

            target.ApplyDamage(amount, element, source);
        }
    }

    /// <summary>
    /// Optional companion to <see cref="ICombatTarget"/> for targets that can flag a hit as critical in
    /// their <see cref="DamageAppliedEvent"/>. The amount already includes the crit multiplier
    /// (<see cref="OutgoingDamage.Resolve"/>), so implementers must NOT multiply it again.
    /// </summary>
    public interface ICriticalAwareTarget
    {
        void ApplyResolvedCriticalDamage(float amount, ElementType element, GameObject source);
    }

    /// <summary>
    /// Optional companion to <see cref="ICombatTarget"/> for targets whose post-hit window is per damage
    /// channel (<see cref="DamageInfo.Channel"/>). Like <see cref="ICriticalAwareTarget"/>, the amount is
    /// already resolved (crit included) and must not be multiplied again.
    /// </summary>
    public interface IChanneledDamageTarget
    {
        void ApplyChanneledDamage(float amount, ElementType element, GameObject source, bool isCritical, int channel);
    }
}
