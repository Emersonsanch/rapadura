using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Pure, physics-free helper functions for enemy AI decisions (detection radius, attack range,
    /// flee thresholds, waypoint arrival). Kept separate from <see cref="EnemyController"/> so the
    /// decision logic can be unit tested with plain Vector3/float inputs, without needing colliders,
    /// raycasts or a running scene.
    /// </summary>
    public static class EnemyDetectionUtility
    {
        /// <summary>Hero run speed (<c>PlayerMotor._runSpeed</c> default) that flee speeds are measured against.</summary>
        public const float HeroRunSpeed = 5.5f;

        /// <summary>Fleeing enemies never outrun more than this fraction of <see cref="HeroRunSpeed"/>, so a chasing hero catches them.</summary>
        public const float MaxFleeSpeedFractionOfHeroRun = 0.8f;

        /// <summary>Seconds an enemy keeps fleeing before it tires and turns back to fight (it won't flee again this life).</summary>
        public const float FleeMaxDurationSeconds = 3f;

        /// <summary>Highest speed an enemy may flee at (0.8 x hero run = 4.4).</summary>
        public static float MaxFleeSpeed => HeroRunSpeed * MaxFleeSpeedFractionOfHeroRun;

        /// <summary>
        /// The speed a flee actually runs at: the definition's, capped at <see cref="MaxFleeSpeed"/>.
        /// Bats (6.0), wolves (5.0) and scorpions (4.5) used to outrun a walking (2.5) or even running
        /// (5.5) hero, dragging fights out to 20+ s.
        /// </summary>
        public static float GetEffectiveFleeSpeed(float definitionFleeSpeed)
        {
            return Mathf.Clamp(definitionFleeSpeed, 0f, MaxFleeSpeed);
        }

        /// <summary>True once a flee has lasted <paramref name="maxDurationSeconds"/> (0 or less = never tires).</summary>
        public static bool IsFleeExhausted(float fleeElapsedSeconds, float maxDurationSeconds)
        {
            return maxDurationSeconds > 0f && fleeElapsedSeconds >= maxDurationSeconds;
        }

        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            Vector3 delta = a - b;
            return delta.sqrMagnitude;
        }

        public static bool IsWithinRadius(Vector3 origin, Vector3 target, float radius)
        {
            if (radius <= 0f)
            {
                return false;
            }

            return SqrDistance(origin, target) <= radius * radius;
        }

        public static bool HasArrivedAtWaypoint(Vector3 position, Vector3 waypoint, float threshold)
        {
            float safeThreshold = Mathf.Max(threshold, 0.01f);
            return SqrDistance(position, waypoint) <= safeThreshold * safeThreshold;
        }

        public static bool ShouldFlee(float currentHealth, float maxHealth, float fleeHealthThreshold01)
        {
            if (maxHealth <= 0f)
            {
                return false;
            }

            float ratio = Mathf.Clamp01(currentHealth / maxHealth);
            return ratio <= Mathf.Clamp01(fleeHealthThreshold01);
        }

        public static bool CanRecoverFromFlee(float currentHealth, float maxHealth, float fleeHealthThreshold01)
        {
            return !ShouldFlee(currentHealth, maxHealth, fleeHealthThreshold01);
        }

        public static bool IsFarEnoughToStopFleeing(Vector3 selfPosition, Vector3 threatPosition, float safeDistance)
        {
            return !IsWithinRadius(selfPosition, threatPosition, safeDistance);
        }

        public static bool IsTargetLost(Vector3 selfPosition, Vector3 targetPosition, float loseTargetRadius)
        {
            return !IsWithinRadius(selfPosition, targetPosition, loseTargetRadius);
        }

        public static bool IsWithinAttackRange(Vector3 selfPosition, Vector3 targetPosition, float attackRange)
        {
            return IsWithinRadius(selfPosition, targetPosition, attackRange);
        }

        /// <summary>Direction from origin to target flattened on the XZ plane, normalized. Returns Vector3.zero when the points coincide.</summary>
        public static Vector3 DirectionTowards(Vector3 origin, Vector3 target)
        {
            Vector3 delta = target - origin;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            return delta.normalized;
        }

        /// <summary>Direction from origin away from a threat, flattened on the XZ plane, normalized.</summary>
        public static Vector3 DirectionAwayFrom(Vector3 origin, Vector3 threat)
        {
            Vector3 towardsThreat = DirectionTowards(origin, threat);
            return towardsThreat == Vector3.zero ? Vector3.zero : -towardsThreat;
        }
    }
}
