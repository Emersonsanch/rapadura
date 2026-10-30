using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Pure, physics-free steering math for enemies (Docs/Animation.md §7.3): obstacle avoidance (slide
    /// along a probed wall/trunk instead of pushing into it), separation between neighbours (no two
    /// creatures occupying the same spot), slow-down when something is right ahead, and a per-creature
    /// pace so a pack never moves in lock-step. <see cref="EnemyController"/> feeds it the probe result
    /// and the neighbour positions; nothing here touches transforms or physics, so it is unit-testable.
    /// </summary>
    public static class EnemySteering
    {
        /// <summary>Default radius multiplier: creatures keep (ownRadius + otherRadius) × this between centres.</summary>
        public const float DefaultSeparationScale = 1.35f;

        /// <summary>Default spread of <see cref="PaceMultiplier"/> (±10 %).</summary>
        public const float DefaultPaceSpread = 0.1f;

        /// <summary>
        /// How far ahead to probe for obstacles: at least <paramref name="minDistance"/>, growing with speed
        /// (stopping distance at <paramref name="deceleration"/> plus a little reaction margin), capped.
        /// </summary>
        public static float LookAhead(float speed, float deceleration, float minDistance = 0.6f, float maxDistance = 2.5f)
        {
            speed = Mathf.Max(0f, speed);
            float stopping = deceleration > 0.01f ? speed * speed / (2f * deceleration) : speed * 0.5f;
            return Mathf.Clamp(minDistance + stopping + speed * 0.15f, minDistance, maxDistance);
        }

        /// <summary>
        /// Direction to steer around an obstacle hit while heading <paramref name="desired"/>: slides along the
        /// obstacle surface, on the side closer to the desired heading (and, for a head-on hit, the side the
        /// creature already leans to via <paramref name="preferRight"/>). The slide gets stronger the closer
        /// the hit (<paramref name="hitDistance"/> / <paramref name="lookAhead"/>). Returns the (normalized)
        /// steered direction; <paramref name="desired"/> itself when the hit is not blocking.
        /// </summary>
        public static Vector3 AvoidanceDirection(Vector3 desired, Vector3 hitNormal, float hitDistance, float lookAhead, bool preferRight)
        {
            desired.y = 0f;
            if (desired.sqrMagnitude < 1e-6f)
            {
                return Vector3.zero;
            }

            desired.Normalize();
            hitNormal.y = 0f;
            if (hitNormal.sqrMagnitude < 1e-6f || lookAhead <= 0f)
            {
                return desired;
            }

            hitNormal.Normalize();

            // Moving away from (or along) the surface: nothing to avoid.
            float into = -Vector3.Dot(desired, hitNormal);
            if (into <= 0.05f)
            {
                return desired;
            }

            // Tangent of the surface, on the side that keeps most of the desired heading.
            Vector3 tangent = Vector3.ProjectOnPlane(desired, hitNormal);
            if (tangent.sqrMagnitude < 1e-4f)
            {
                // Head-on: pick a side.
                tangent = Vector3.Cross(Vector3.up, hitNormal) * (preferRight ? -1f : 1f);
            }

            tangent.Normalize();

            // Urgency: 0 at the probe's far end, 1 touching (smoothstep, so a far hit only nudges);
            // heading straight in always gets at least a small bend, so the slide starts before contact.
            float urgency = Mathf.Clamp01(1f - hitDistance / lookAhead);
            urgency = urgency * urgency * (3f - 2f * urgency);
            float weight = Mathf.Clamp01(Mathf.Max(urgency, 0.3f * into));

            Vector3 steered = Vector3.Slerp(desired, tangent, weight);
            steered.y = 0f;
            return steered.sqrMagnitude > 1e-6f ? steered.normalized : tangent;
        }

        /// <summary>
        /// Speed factor (<paramref name="minFactor"/>..1) when an obstacle is <paramref name="hitDistance"/> ahead within
        /// <paramref name="lookAhead"/>: ease off near things instead of grinding into them. 1 with no hit (negative distance).
        /// </summary>
        public static float ObstacleSlowdown(float hitDistance, float lookAhead, float minFactor = 0.45f)
        {
            if (hitDistance < 0f || lookAhead <= 0f || hitDistance >= lookAhead)
            {
                return 1f;
            }

            float t = Mathf.Clamp01(hitDistance / lookAhead);
            return Mathf.Lerp(minFactor, 1f, t);
        }

        /// <summary>
        /// Adds this neighbour's push to <paramref name="accumulator"/>: away from it, growing linearly from 0 at
        /// <paramref name="desiredDistance"/> to 1 when the centres coincide (then a fixed sideways push, so two
        /// creatures spawned on the same point still part). Returns true when the neighbour counted.
        /// </summary>
        public static bool AccumulateSeparation(ref Vector3 accumulator, Vector3 self, Vector3 other, float desiredDistance)
        {
            Vector3 away = self - other;
            away.y = 0f;
            float distance = away.magnitude;
            if (desiredDistance <= 0f || distance >= desiredDistance)
            {
                return false;
            }

            if (distance < 1e-4f)
            {
                accumulator += Vector3.right;
                return true;
            }

            float strength = 1f - distance / desiredDistance;
            accumulator += away / distance * strength;
            return true;
        }

        /// <summary>
        /// Final heading: the (already obstacle-steered) <paramref name="direction"/> bent by the separation push.
        /// Separation never fully overrides the goal (capped at <paramref name="maxSeparationWeight"/> of the
        /// result), so a chase still closes in — it just does so without stacking. Normalized; zero input → zero.
        /// </summary>
        public static Vector3 Combine(Vector3 direction, Vector3 separation, float separationGain = 1f, float maxSeparationWeight = 0.75f)
        {
            direction.y = 0f;
            separation.y = 0f;
            if (direction.sqrMagnitude < 1e-6f)
            {
                return separation.sqrMagnitude > 1e-6f ? separation.normalized : Vector3.zero;
            }

            direction.Normalize();
            if (separation.sqrMagnitude < 1e-6f || separationGain <= 0f)
            {
                return direction;
            }

            // A push of magnitude p against a unit direction weighs p / (1 + p) of the result:
            // cap p so that weight never exceeds maxSeparationWeight (0.75 → p ≤ 3).
            float w = Mathf.Clamp(maxSeparationWeight, 0f, 0.95f);
            float maxPush = w / (1f - w);
            Vector3 push = Vector3.ClampMagnitude(separation * separationGain, maxPush);
            Vector3 result = direction + push;
            result.y = 0f;
            return result.sqrMagnitude > 1e-6f ? result.normalized : direction;
        }

        /// <summary>
        /// Deterministic per-creature pace in [1 − spread, 1 + spread] from a seed (instance id, spawn index):
        /// the same enemy always walks at the same pace, but a pack never moves perfectly in sync.
        /// </summary>
        public static float PaceMultiplier(int seed, float spread = DefaultPaceSpread)
        {
            spread = Mathf.Clamp(spread, 0f, 0.5f);
            uint h = unchecked((uint)seed * 2654435761u);
            h ^= h >> 15;
            h *= 2246822519u;
            h ^= h >> 13;
            float unit = (h & 0xFFFFFF) / (float)0xFFFFFF; // 0..1
            return 1f + (unit * 2f - 1f) * spread;
        }

        /// <summary>Which way a creature sidesteps a head-on obstacle: stable per seed, so it never dithers between sides.</summary>
        public static bool PrefersRight(int seed)
        {
            return (unchecked((uint)seed * 2654435761u) & 0x10000u) != 0u;
        }
    }
}
