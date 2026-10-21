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

        /// <summary>XZ distance between two points (what the ring radii are measured in).</summary>
        public static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
