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

        /// <summary>
        /// Which "damage channel" of <see cref="Source"/> this instance belongs to (see <see cref="DamageChannel"/>):
        /// <see cref="DamageChannel.Default"/> for basic attacks/hitboxes, one per skill id, one per DoT.
        /// <see cref="Health"/> keys its anti-double-hit window by source + channel, so different skills
        /// cast back to back all land instead of the second being swallowed by the first's i-frames.
        /// </summary>
        public readonly int Channel;

        public DamageInfo(
            float amount,
            ElementType element,
            GameObject source,
            Vector3 hitPoint,
            Vector3 knockbackDirection,
            float knockbackForce,
            bool isCritical = false,
            int channel = DamageChannel.Default)
        {
            Amount = amount;
            Element = element;
            Source = source;
            HitPoint = hitPoint;
            KnockbackDirection = knockbackDirection.sqrMagnitude > 0f ? knockbackDirection.normalized : Vector3.zero;
            KnockbackForce = knockbackForce;
            IsCritical = isCritical;
            Channel = channel;
        }

        /// <summary>Convenience factory for damage that carries no knockback (e.g. poison ticks).</summary>
        public static DamageInfo Simple(float amount, ElementType element, GameObject source, int channel = DamageChannel.Default)
        {
            return new DamageInfo(amount, element, source, Vector3.zero, Vector3.zero, 0f, isCritical: false, channel: channel);
        }
    }

    /// <summary>
    /// Stable integer ids for <see cref="DamageInfo.Channel"/>. A channel groups the hits that must NOT
    /// stack on one target within <see cref="Health"/>'s short same-channel window (one cast's overlapping
    /// checks), while hits on different channels — basic attack vs. each skill vs. each DoT — always do.
    /// </summary>
    public static class DamageChannel
    {
        /// <summary>Basic attacks, weapon hitboxes and any caller that doesn't care about channels.</summary>
        public const int Default = 0;

        /// <summary>
        /// Deterministic FNV-1a hash of <paramref name="id"/> (e.g. a skill id). Allocation-free and
        /// never returns <see cref="Default"/>, so a named channel can't collide with the basic attack.
        /// </summary>
        public static int For(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Default;
            }

            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < id.Length; i++)
                {
                    hash ^= id[i];
                    hash *= 16777619u;
                }

                int channel = (int)hash;
                return channel == Default ? 1 : channel;
            }
        }

        /// <summary>Channel of a skill's direct hit.</summary>
        public static int ForSkill(string skillId) => For(skillId);

        /// <summary>Channel of a damage-over-time effect, distinct from the direct hit of the skill that applied it.</summary>
        public static int ForDamageOverTime(string sourceKey, StatType stat)
        {
            unchecked
            {
                int channel = For(sourceKey) * 31 + (int)stat + 7919;
                return channel == Default ? 2 : channel;
            }
        }
    }
}
