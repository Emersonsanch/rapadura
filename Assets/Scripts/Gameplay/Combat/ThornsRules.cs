using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Optional companion to <see cref="ICombatTarget"/> for targets that can be hit in melee and so can
    /// reflect <see cref="StatType.Thorns"/>. Returns the damage actually taken (after resistance/defense,
    /// 0 when blocked by i-frames or already dead), which is what thorns reflect.
    /// <c>PlayerStats</c> implements it; <c>EnemyController.ApplyDamageToTarget</c> (every enemy attack is
    /// melee today) routes through it.
    /// </summary>
    public interface IMeleeDamageTarget
    {
        float ApplyMeleeDamage(float amount, ElementType element, GameObject attacker);
    }

    /// <summary>
    /// Espinhos (<see cref="StatType.Thorns"/>): a share of the melee damage taken goes back to the attacker.
    /// <code>
    /// fraction  = clamp(Σ Thorns, 0, MaxFraction)         // Flat and PercentAdditive both count (0.1 = 10%)
    /// reflected = damageTaken × fraction                    // damageTaken is post-defense
    /// </code>
    /// The reflected hit is delivered on its own <see cref="Channel"/> (so it never eats the hero's next
    /// basic-attack window on the enemy), credits the thorns owner as the source (kill XP/loot), and is
    /// never melee: it goes through <see cref="OutgoingDamage.DeliverDamage"/>, not
    /// <see cref="IMeleeDamageTarget"/>, so it cannot reflect again. A re-entrancy guard
    /// (<see cref="IsReflecting"/>) also drops any thorns triggered while a reflection is being delivered,
    /// so even two thorns users hitting each other can't ping-pong.
    /// </summary>
    public static class ThornsRules
    {
        /// <summary>Most of the damage taken that can be reflected, whatever the gear rolls.</summary>
        public const float MaxFraction = 0.5f;

        /// <summary>Damage channel of reflected hits (distinct from basic attacks and every skill).</summary>
        public static readonly int Channel = DamageChannel.For("thorns");

        private static bool _reflecting;

        /// <summary>True while a reflected hit is being delivered (nested thorns are ignored).</summary>
        public static bool IsReflecting => _reflecting;

        /// <summary>Σ Thorns of <paramref name="buffs"/> clamped to [0, <see cref="MaxFraction"/>]; 0 without buffs.</summary>
        public static float GetFraction(BuffController buffs)
        {
            return buffs != null ? Mathf.Clamp(buffs.GetAdditiveFractionTotal(StatType.Thorns), 0f, MaxFraction) : 0f;
        }

        /// <summary>Pure: <paramref name="damageTaken"/> × clamped <paramref name="fraction"/> (never negative).</summary>
        public static float ComputeReflected(float damageTaken, float fraction)
        {
            if (damageTaken <= 0f || fraction <= 0f)
            {
                return 0f;
            }

            return damageTaken * Mathf.Min(fraction, MaxFraction);
        }

        /// <summary>
        /// Reflects <paramref name="damageTaken"/> × the victim's thorns back at <paramref name="attacker"/>.
        /// No-op (returns 0) without thorns, without a live attacker that is an <see cref="ICombatTarget"/>,
        /// when the attacker is the victim itself, or while another reflection is in flight.
        /// </summary>
        public static float TryReflect(GameObject victim, GameObject attacker, float damageTaken)
        {
            if (_reflecting || victim == null || attacker == null || attacker == victim || damageTaken <= 0f)
            {
                return 0f;
            }

            float reflected = ComputeReflected(damageTaken, GetFraction(victim.GetComponent<BuffController>()));
            if (reflected <= 0f)
            {
                return 0f;
            }

            ICombatTarget attackerTarget = attacker.GetComponentInParent<ICombatTarget>();
            if (attackerTarget == null || !attackerTarget.IsAlive)
            {
                return 0f;
            }

            _reflecting = true;
            try
            {
                OutgoingDamage.DeliverDamage(attackerTarget, reflected, ElementType.Physical, victim, false, Channel);
            }
            finally
            {
                _reflecting = false;
            }

            return reflected;
        }
    }
}
