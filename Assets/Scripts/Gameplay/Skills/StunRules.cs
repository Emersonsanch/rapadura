namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Optional companion to <see cref="BuffController"/>: lets the owner scale every stun it receives
    /// (<see cref="BuffController.ApplyStun"/> multiplies the duration by <see cref="StunDurationMultiplier"/>).
    /// Implemented by <c>EnemyController</c> so bosses take <see cref="StunRules.BossStunDurationMultiplier"/>.
    /// </summary>
    public interface IStunDurationModifier
    {
        /// <summary>Multiplier on incoming stun durations (1 = unchanged, 0.5 = half, 0 = immune).</summary>
        float StunDurationMultiplier { get; }
    }

    /// <summary>
    /// Stun tuning shared by skills and enemies (see Docs/Balance/README.md, "Atordoamento"):
    /// <code>
    /// applied = skill._stunDurationSeconds × (boss ? 0.5 : 1)      // never shortens a running stun
    /// </code>
    /// A stunned enemy stops moving, cancels a windup in progress (the next attack re-telegraphs from the
    /// start) and does not attack; a stunned hero ignores movement/attack input (PlayerController).
    /// </summary>
    public static class StunRules
    {
        /// <summary>Bosses take half the stun so three stunning heroes can't chain-lock them.</summary>
        public const float BossStunDurationMultiplier = 0.5f;

        /// <summary>The multiplier a regular enemy (1) or a boss (<see cref="BossStunDurationMultiplier"/>) applies.</summary>
        public static float MultiplierFor(bool isBoss) => isBoss ? BossStunDurationMultiplier : 1f;

        /// <summary>Duration × multiplier; non-positive duration or multiplier → 0 (no stun).</summary>
        public static float ScaleDuration(float durationSeconds, float multiplier)
        {
            if (durationSeconds <= 0f || multiplier <= 0f)
            {
                return 0f;
            }

            return durationSeconds * multiplier;
        }
    }
}
