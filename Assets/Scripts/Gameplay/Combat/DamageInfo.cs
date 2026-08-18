using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Immutable description of a single damage instance travelling from a hitbox (or any other
    /// damage source, e.g. a skill) to a hurtbox/<see cref="ICombatTarget"/>. Carries everything
    /// the damage formula and game-feel systems need — the raw amount, its element, where/how it
    /// hit, and whether it should knock the target back — without either side needing to know
    /// about the other's concrete type.
    /// </summary>
    public readonly struct DamageInfo
    {
        /// <summary>Raw damage amount before defense/resistance is applied by <see cref="DamageCalculator"/>.</summary>
        public readonly float Amount;
        public readonly ElementType Element;
        public readonly bool IsCritical;

        /// <summary>GameObject responsible for the damage (attacker, projectile owner, trap owner...). May be null.</summary>
        public readonly GameObject Source;

        /// <summary>World-space point of impact, used for VFX and as the knockback origin.</summary>
        public readonly Vector3 HitPoint;

        /// <summary>Normalized direction the target should be pushed in. Zero vector means no knockback.</summary>
        public readonly Vector3 KnockbackDirection;

        /// <summary>Magnitude of the knockback impulse, in the same units <see cref="KnockbackReceiver"/> expects.</summary>
        public readonly float KnockbackForce;

        public DamageInfo(
            float amount,
            ElementType element,
            GameObject source,
            Vector3 hitPoint,
            Vector3 knockbackDirection,
            float knockbackForce,
            bool isCritical = false)
        {
            Amount = amount;
            Element = element;
            Source = source;
            HitPoint = hitPoint;
            KnockbackDirection = knockbackDirection.sqrMagnitude > 0f ? knockbackDirection.normalized : Vector3.zero;
            KnockbackForce = knockbackForce;
            IsCritical = isCritical;
        }

        /// <summary>Convenience factory for damage that carries no knockback (e.g. poison ticks).</summary>
        public static DamageInfo Simple(float amount, ElementType element, GameObject source)
        {
            return new DamageInfo(amount, element, source, Vector3.zero, Vector3.zero, 0f);
        }
    }
}
