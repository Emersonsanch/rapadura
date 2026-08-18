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
