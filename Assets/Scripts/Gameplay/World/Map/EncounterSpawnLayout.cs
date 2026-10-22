using UnityEngine;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Pure placement math for <see cref="EncounterArenaController"/>: rolled enemies stand on an arc in
    /// front of the player, [<c>minRadius</c>, <c>maxRadius</c>] away (alternating between the two so a
    /// pack reads as a loose group, not a line), clamped inside the arena circle. Kept Unity-scene-free
    /// so the ring is EditMode testable; the controller adds the ground raycast.
    /// </summary>
    public static class EncounterSpawnLayout
    {
        public const float DefaultMinRadius = 6f;
        public const float DefaultMaxRadius = 9f;
        public const float DefaultArcDegrees = 120f;

        /// <summary>
        /// XZ position of enemy <paramref name="index"/> of <paramref name="count"/> around
        /// <paramref name="origin"/> (the player), spread over <paramref name="arcDegrees"/> centred on
        /// <paramref name="forward"/>. Y is the origin's. A single enemy goes straight ahead, halfway
        /// between the radii. A zero/vertical forward falls back to world +Z.
        /// </summary>
        public static Vector3 RingPoint(Vector3 origin, Vector3 forward, int index, int count,
            float minRadius = DefaultMinRadius, float maxRadius = DefaultMaxRadius, float arcDegrees = DefaultArcDegrees)
        {
            count = Mathf.Max(1, count);
            index = Mathf.Clamp(index, 0, count - 1);

            float inner = Mathf.Max(0f, Mathf.Min(minRadius, maxRadius));
            float outer = Mathf.Max(inner, Mathf.Max(minRadius, maxRadius));

            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = Vector3.forward;
            }

            flatForward.Normalize();

            float arc = Mathf.Clamp(arcDegrees, 0f, 360f);
            float angle;
            float radius;
            if (count == 1)
            {
                angle = 0f;
                radius = (inner + outer) * 0.5f;
            }
            else
            {
                // A full circle would put the first and last enemy on the same spot: divide by count then.
                float span = arc >= 359.9f ? arc * (count - 1) / count : arc;
                angle = -span * 0.5f + span * index / (count - 1);
                radius = index % 2 == 0 ? inner : outer;
            }

            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * flatForward;
            return origin + direction * radius;
        }

        /// <summary>
        /// Pulls <paramref name="point"/> back inside the arena circle (XZ only; Y kept). A non-positive
        /// <paramref name="arenaRadius"/> disables the clamp.
        /// </summary>
        public static Vector3 ClampToArena(Vector3 point, Vector3 arenaCenter, float arenaRadius)
        {
            if (arenaRadius <= 0f)
            {
                return point;
            }

            Vector3 offset = point - arenaCenter;
            offset.y = 0f;
            if (offset.sqrMagnitude <= arenaRadius * arenaRadius)
            {
                return point;
            }

            Vector3 clamped = arenaCenter + offset.normalized * arenaRadius;
            clamped.y = point.y;
            return clamped;
        }

        /// <summary>
        /// Root position that makes an enemy stand on <paramref name="groundPoint"/>. The shared enemy
        /// prefab's pivot is the centre of its capsule, not its feet: <paramref name="footHeight"/> is the
        /// (usually negative) local y of the feet — see <see cref="Rapadura.Gameplay.Enemies.EnemyVisualSwapper.GetFootHeight"/> —
        /// so the root goes <c>-footHeight</c> above the ground. Placing the root on the ground point
        /// itself buried the bottom half of every encounter enemy (models fully below the floor).
        /// </summary>
        public static Vector3 StandOnGround(Vector3 groundPoint, float footHeight)
        {
            groundPoint.y -= footHeight;
            return groundPoint;
        }

        /// <summary>Default minimum XZ gap between two encounter enemies (two 0.5 m-radius capsules plus room).</summary>
        public const float DefaultMinSpacing = 1.6f;

        /// <summary>
        /// Moves <paramref name="point"/> so it is at least <paramref name="minSpacing"/> (XZ) from every
        /// point in <paramref name="taken"/>, staying inside the arena circle. <see cref="ClampToArena"/>
        /// can squeeze arc points near the arena edge onto (almost) the same spot; this slides the point
        /// around <paramref name="pivot"/> (the ring origin, i.e. the player) in growing steps, alternating
        /// sides, re-clamping each try. Returns the first spaced candidate; if none is found the closest
        /// try to being spaced is returned. Y is kept.
        /// </summary>
        public static Vector3 KeepApart(Vector3 point, System.Collections.Generic.IReadOnlyList<Vector3> taken, float minSpacing,
            Vector3 pivot, Vector3 arenaCenter, float arenaRadius, int maxTries = 24)
        {
            if (taken == null || taken.Count == 0 || minSpacing <= 0f)
            {
                return point;
            }

            Vector3 best = point;
            float bestGap = NearestGap(point, taken);
            if (bestGap >= minSpacing)
            {
                return point;
            }

            Vector3 offset = point - pivot;
            offset.y = 0f;
            float radius = Mathf.Max(offset.magnitude, minSpacing);
            if (offset.sqrMagnitude < 0.0001f)
            {
                offset = Vector3.forward * radius;
            }

            float stepDegrees = Mathf.Rad2Deg * (minSpacing / radius) * 0.5f;
            for (int i = 1; i <= maxTries; i++)
            {
                float angle = stepDegrees * ((i + 1) / 2) * (i % 2 == 1 ? 1f : -1f);
                Vector3 candidate = pivot + Quaternion.AngleAxis(angle, Vector3.up) * offset;
                candidate.y = point.y;
                candidate = ClampToArena(candidate, arenaCenter, arenaRadius);

                float gap = NearestGap(candidate, taken);
                if (gap >= minSpacing)
                {
                    return candidate;
                }

                if (gap > bestGap)
                {
                    bestGap = gap;
                    best = candidate;
                }
            }

            return best;
        }

        private static float NearestGap(Vector3 point, System.Collections.Generic.IReadOnlyList<Vector3> taken)
        {
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < taken.Count; i++)
            {
                nearest = Mathf.Min(nearest, FlatDistance(point, taken[i]));
            }

            return nearest;
        }

        /// <summary>
        /// Whether a spawn ground raycast hit can be the floor: not the player and not any enemy (a
        /// collider under an <see cref="Rapadura.Gameplay.Enemies.EnemyController"/>).
        /// </summary>
        public static bool IsGroundCandidate(Collider collider, Transform player)
        {
            if (collider == null)
            {
                return false;
            }

            if (player != null && collider.transform.IsChildOf(player))
            {
                return false;
            }

            return collider.GetComponentInParent<Rapadura.Gameplay.Enemies.EnemyController>(true) == null;
        }

        /// <summary>XZ distance between two points (what the ring radii are measured in).</summary>
        public static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
