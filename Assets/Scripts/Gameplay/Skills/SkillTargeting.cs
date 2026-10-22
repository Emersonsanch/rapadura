using System.Collections.Generic;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>How a skill picks its aim point (derived from its data, never per-skill code).</summary>
    public enum SkillAimMode
    {
        /// <summary>Pure buff/heal (no damage, no debuffs) or Range 0 point-blank: centred on the caster.</summary>
        Self,

        /// <summary>AreaRadius &gt; 0: the area lands on the soft target (clamped to Range).</summary>
        Area,

        /// <summary>AreaRadius 0 single-target ("projectile") skill: hits the soft target directly.</summary>
        SingleTarget
    }

    /// <summary>Result of <see cref="SkillTargeting.ResolveAim"/>.</summary>
    public readonly struct SkillAim
    {
        public readonly Vector3 Point;
        public readonly ICombatTarget Target;
        public readonly SkillAimMode Mode;

        public SkillAim(Vector3 point, ICombatTarget target, SkillAimMode mode)
        {
            Point = point;
            Target = target;
            Mode = mode;
        }

        public bool HasTarget => Target != null;
    }

    /// <summary>
    /// Soft target selection for hotbar casts (playtest: skills aimed blindly at
    /// position + forward * Range missed anything closer than Range — 4 casts into a boss at 6m
    /// did 0). Picks the nearest living candidate inside a forward cone within reach, falling back
    /// to the nearest one within reach in any direction; with nothing in reach the cast keeps the
    /// old straight-ahead aim. Pure (no physics queries) so it is EditMode-testable.
    /// </summary>
    public static class SkillTargeting
    {
        /// <summary>Half-angle of the preferred forward cone, in degrees.</summary>
        public const float DefaultConeHalfAngle = 70f;

        /// <summary>Extra reach (m) for single-target skills so a target whose pivot sits just past Range — but whose body is inside it — still counts.</summary>
        public const float SingleTargetReachTolerance = 1f;

        public static SkillAimMode GetAimMode(SkillDefinition skill)
        {
            if (skill == null || skill.Range <= 0f || (skill.BaseDamage <= 0f && skill.Debuffs.Length == 0))
            {
                return SkillAimMode.Self;
            }

            return skill.AreaRadius > 0f ? SkillAimMode.Area : SkillAimMode.SingleTarget;
        }

        /// <summary>How far away a candidate may be for <paramref name="skill"/> to soft-target it.</summary>
        public static float GetTargetingReach(SkillDefinition skill)
        {
            switch (GetAimMode(skill))
            {
                case SkillAimMode.Area:
                    // The area can be placed at most Range away but still catches anything within its radius.
                    return skill.Range + skill.AreaRadius;
                case SkillAimMode.SingleTarget:
                    return skill.Range + SingleTargetReachTolerance;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Index of the best candidate in <paramref name="candidatePositions"/> (horizontal-plane
        /// distance), or -1 when none is within <paramref name="reach"/>.
        /// </summary>
        public static int SelectTargetIndex(Vector3 origin, Vector3 forward, float reach, float coneHalfAngle, IReadOnlyList<Vector3> candidatePositions)
        {
            if (candidatePositions == null || reach <= 0f)
            {
                return -1;
            }

            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
            bool hasForward = flatForward.sqrMagnitude > 0.0001f;
            float cosHalfAngle = Mathf.Cos(Mathf.Clamp(coneHalfAngle, 0f, 180f) * Mathf.Deg2Rad);
            float reachSqr = reach * reach;

            int bestInCone = -1;
            float bestInConeSqr = float.MaxValue;
            int bestAny = -1;
            float bestAnySqr = float.MaxValue;

            for (int i = 0; i < candidatePositions.Count; i++)
            {
                Vector3 offset = candidatePositions[i] - origin;
                offset.y = 0f;
                float sqr = offset.sqrMagnitude;

                if (sqr > reachSqr)
                {
                    continue;
                }

                if (sqr < bestAnySqr)
                {
                    bestAnySqr = sqr;
                    bestAny = i;
                }

                // A candidate standing on the caster counts as "in front".
                bool inCone = !hasForward || sqr < 0.0001f
                    || Vector3.Dot(flatForward.normalized, offset / Mathf.Sqrt(sqr)) >= cosHalfAngle;

                if (inCone && sqr < bestInConeSqr)
                {
                    bestInConeSqr = sqr;
                    bestInCone = i;
                }
            }

            return bestInCone >= 0 ? bestInCone : bestAny;
        }

        /// <summary>
        /// Aim for one cast of <paramref name="skill"/> from <paramref name="origin"/> facing
        /// <paramref name="forward"/>. Self skills aim at the caster; area/single-target skills aim at
        /// the soft target (area placement clamped to Range along the direction to it); with no
        /// target in reach the aim stays straight ahead at Range.
        /// </summary>
        public static SkillAim ResolveAim(SkillDefinition skill, Vector3 origin, Vector3 forward, IEnumerable<ICombatTarget> candidates,
            float coneHalfAngle = DefaultConeHalfAngle)
        {
            SkillAimMode mode = GetAimMode(skill);

            if (mode == SkillAimMode.Self)
            {
                return new SkillAim(origin, null, mode);
            }

            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
            flatForward = flatForward.sqrMagnitude > 0.0001f ? flatForward.normalized : Vector3.forward;
            Vector3 fallback = origin + flatForward * Mathf.Max(0f, skill.Range);

            var living = new List<ICombatTarget>();
            var positions = new List<Vector3>();

            if (candidates != null)
            {
                foreach (ICombatTarget candidate in candidates)
                {
                    if (!IsValid(candidate))
                    {
                        continue;
                    }

                    living.Add(candidate);
                    positions.Add(candidate.Transform.position);
                }
            }

            int index = SelectTargetIndex(origin, flatForward, GetTargetingReach(skill), coneHalfAngle, positions);

            if (index < 0)
            {
                return new SkillAim(fallback, null, mode);
            }

            ICombatTarget target = living[index];
            Vector3 targetPosition = positions[index];

            if (mode == SkillAimMode.Area)
            {
                Vector3 offset = targetPosition - origin;
                offset.y = 0f;
                float distance = offset.magnitude;

                if (distance > skill.Range && distance > 0.0001f)
                {
                    // Out of placement range but inside Range + radius: drop the area at max range
                    // toward the target so its edge still reaches it.
                    Vector3 clamped = origin + offset / distance * skill.Range;
                    clamped.y = targetPosition.y;
                    return new SkillAim(clamped, target, mode);
                }
            }

            return new SkillAim(targetPosition, target, mode);
        }

        private static bool IsValid(ICombatTarget candidate)
        {
            // Unity-null check first: a destroyed component behind the interface would throw on .Transform.
            if (candidate == null || (candidate is Object unityObject && unityObject == null))
            {
                return false;
            }

            return candidate.IsAlive && candidate.Transform != null;
        }
    }
}
