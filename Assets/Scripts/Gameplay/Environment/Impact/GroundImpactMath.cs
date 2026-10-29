using UnityEngine;

namespace Rapadura.Gameplay.Environment.Impact
{
    /// <summary>How a prop reacts when the ground is struck nearby.</summary>
    public enum GroundImpactKind
    {
        None = 0,
        /// <summary>Big rock: rocks in place (a damped wobble about a horizontal axis) and settles.</summary>
        Boulder = 1,
        /// <summary>Pebble/twig: hops off the ground in a short arc, spins a little, lands slightly displaced.</summary>
        Pebble = 2,
    }

    /// <summary>One frame of reaction pose for a prop (local offsets on top of its rest pose).</summary>
    public readonly struct GroundImpactPose
    {
        public readonly Vector3 PositionOffset;
        /// <summary>Rotation to pre-multiply onto the rest rotation (world axes).</summary>
        public readonly Quaternion Rotation;
        public readonly bool Finished;

        public GroundImpactPose(Vector3 positionOffset, Quaternion rotation, bool finished)
        {
            PositionOffset = positionOffset;
            Rotation = rotation;
            Finished = finished;
        }

        public static readonly GroundImpactPose Rest = new GroundImpactPose(Vector3.zero, Quaternion.identity, true);
    }

    /// <summary>
    /// Pure math for ground impacts (EditMode-tested): how much of a strike reaches a prop at a
    /// distance, and the damped wobble/hop over time. Nothing here touches a Transform.
    /// </summary>
    public static class GroundImpactMath
    {
        /// <summary>Boulder wobble duration at kick 1 (seconds); shorter for weaker kicks.</summary>
        public const float BoulderDuration = 0.7f;
        /// <summary>Pebble hop duration at kick 1 (seconds).</summary>
        public const float PebbleDuration = 0.45f;
        /// <summary>Boulder wobble amplitude (degrees) at kick 1.</summary>
        public const float BoulderWobbleDegrees = 4f;
        /// <summary>Boulder wobble frequency (Hz).</summary>
        public const float BoulderWobbleHz = 7f;
        /// <summary>Pebble hop height (metres) at kick 1.</summary>
        public const float PebbleHopHeight = 0.18f;
        /// <summary>Pebble spin (degrees) over the hop at kick 1.</summary>
        public const float PebbleSpinDegrees = 70f;
        /// <summary>Pebble lateral skid (metres) at kick 1, away from the impact.</summary>
        public const float PebbleSkid = 0.12f;

        /// <summary>
        /// Share of a strike (0..1) felt at <paramref name="distance"/> from an impact of
        /// <paramref name="radius"/>: quadratic falloff, full at the centre, nothing at the edge or beyond.
        /// </summary>
        public static float Falloff(float distance, float radius)
        {
            if (radius <= 0f || distance >= radius)
            {
                return 0f;
            }

            float t = 1f - Mathf.Max(0f, distance) / radius;
            return t * t;
        }

        /// <summary>Kick strength (0..~1.5) a prop takes from a strike, including the per-kind sensitivity (pebbles fly, boulders barely budge).</summary>
        public static float Kick(float strikeStrength, float distance, float radius, GroundImpactKind kind)
        {
            float kick = Mathf.Max(0f, strikeStrength) * Falloff(distance, radius);
            return kind == GroundImpactKind.Pebble ? Mathf.Min(1.5f, kick * 1.25f) : Mathf.Min(1.5f, kick);
        }

        /// <summary>Total reaction length for a kick.</summary>
        public static float Duration(float kick, GroundImpactKind kind)
        {
            float baseDuration = kind == GroundImpactKind.Pebble ? PebbleDuration : BoulderDuration;
            return baseDuration * Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(kick));
        }

        /// <summary>
        /// Pose at <paramref name="elapsed"/> seconds into a reaction. <paramref name="awayXZ"/> is the
        /// normalised direction from the impact to the prop (the wobble axis and skid direction), and
        /// <paramref name="phase"/> (0..1) desynchronises neighbours.
        /// </summary>
        public static GroundImpactPose Evaluate(GroundImpactKind kind, float kick, float elapsed, Vector2 awayXZ, float phase)
        {
            if (kick <= 0f || kind == GroundImpactKind.None)
            {
                return GroundImpactPose.Rest;
            }

            float duration = Duration(kick, kind);
            if (elapsed >= duration)
            {
                return GroundImpactPose.Rest;
            }

            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 away = awayXZ.sqrMagnitude > 1e-6f ? new Vector3(awayXZ.x, 0f, awayXZ.y).normalized : Vector3.forward;
            // Rocking "toward then away" from the impact = rotating about the axis perpendicular to `away`.
            Vector3 axis = Vector3.Cross(Vector3.up, away);

            if (kind == GroundImpactKind.Pebble)
            {
                float arc = Mathf.Sin(t * Mathf.PI); // one hop
                float hop = PebbleHopHeight * kick * arc;
                float skid = PebbleSkid * kick * t;
                float spin = PebbleSpinDegrees * kick * t * (phase > 0.5f ? 1f : -1f);
                Quaternion rotation = Quaternion.AngleAxis(spin, axis) * Quaternion.AngleAxis(spin * 0.35f, Vector3.up);
                return new GroundImpactPose(Vector3.up * hop + away * skid, rotation, false);
            }

            float envelope = (1f - t) * (1f - t);
            float wobble = BoulderWobbleDegrees * kick * envelope *
                           Mathf.Sin((elapsed * BoulderWobbleHz + phase) * Mathf.PI * 2f);
            float settle = 0.01f * kick * envelope; // a hair of vertical jitter sells "heavy thing shifted"
            return new GroundImpactPose(Vector3.up * (settle * Mathf.Abs(Mathf.Sin(elapsed * BoulderWobbleHz * Mathf.PI * 2f))),
                Quaternion.AngleAxis(wobble, axis), false);
        }
    }
}
