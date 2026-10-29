using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Pure, allocation-free math behind the hero's "weight and personality" layers (Disney
    /// principles: anticipation, follow-through/overlap, ease, squash &amp; stretch, secondary
    /// action), kept out of the MonoBehaviours so it is EditMode-testable: damped springs for
    /// overlap, the turn-brake rule, volume-preserving squash, landing/takeoff squash, the attack
    /// anticipation timeline, idle glances and the foot-IK ground offsets. Also used by the enemies'
    /// <c>EnemyProceduralAnimator</c> (springs, squash, glances). Companion of
    /// <see cref="LocomotionMath"/> (which owns the gameplay-side locomotion numbers).
    /// </summary>
    public static class MotionFeelMath
    {
        /// <summary>Largest integration step (s) of <see cref="StepSpring(ref float, ref float, float, float, float, float, float)"/>; longer frames are sub-stepped so a hitch can never blow the spring up.</summary>
        public const float MaxSpringStep = 1f / 120f;

        /// <summary>Hard cap on squash &amp; stretch (fraction of height): the brief asks for subtle (≤ 6 %).</summary>
        public const float MaxSquash = 0.06f;

        // ------------------------------------------------------------------
        // Springs (overlap / follow-through)
        // ------------------------------------------------------------------

        /// <summary>
        /// One step of a damped spring pulling <paramref name="position"/> toward <paramref name="target"/>
        /// (x'' = ω²(target − x) − 2ζω x' + <paramref name="externalAcceleration"/>), semi-implicit
        /// Euler sub-stepped at ≤ <see cref="MaxSpringStep"/>: unconditionally stable for the
        /// frequencies used here, frame-rate independent. ζ &lt; 1 overshoots (follow-through),
        /// ζ = 1 is critically damped. <paramref name="frequencyHz"/> ≤ 0 snaps to the target.
        /// </summary>
        public static float StepSpring(ref float position, ref float velocity, float target, float frequencyHz, float dampingRatio, float deltaTime, float externalAcceleration = 0f)
        {
            if (deltaTime <= 0f)
            {
                return position;
            }

            if (frequencyHz <= 0f)
            {
                position = target;
                velocity = 0f;
                return position;
            }

            float omega = 2f * Mathf.PI * frequencyHz;
            float stiffness = omega * omega;
            float damping = 2f * Mathf.Max(0f, dampingRatio) * omega;

            // A 0.5 s hitch is at most 60 sub-steps; anything longer is clamped (the pose would be stale anyway).
            float remaining = Mathf.Min(deltaTime, 0.5f);
            while (remaining > 1e-6f)
            {
                float h = Mathf.Min(MaxSpringStep, remaining);
                float accel = stiffness * (target - position) - damping * velocity + externalAcceleration;
                velocity += accel * h;
                position += velocity * h;
                remaining -= h;
            }

            if (float.IsNaN(position) || float.IsInfinity(position) || float.IsNaN(velocity) || float.IsInfinity(velocity))
            {
                position = target;
                velocity = 0f;
            }

            return position;
        }

        /// <summary>2D (pitch, roll) version of <see cref="StepSpring(ref float, ref float, float, float, float, float, float)"/>.</summary>
        public static Vector2 StepSpring(ref Vector2 position, ref Vector2 velocity, Vector2 target, float frequencyHz, float dampingRatio, float deltaTime, Vector2 externalAcceleration)
        {
            float x = position.x, vx = velocity.x, y = position.y, vy = velocity.y;
            StepSpring(ref x, ref vx, target.x, frequencyHz, dampingRatio, deltaTime, externalAcceleration.x);
            StepSpring(ref y, ref vy, target.y, frequencyHz, dampingRatio, deltaTime, externalAcceleration.y);
            position = new Vector2(x, y);
            velocity = new Vector2(vx, vy);
            return position;
        }

        /// <summary>
        /// Initial velocity to give a spring resting at its target so its first excursion, integrated by
        /// <see cref="StepSpring(ref float, ref float, float, float, float, float, float)"/>, peaks at
        /// <paramref name="peak"/> (same units): a "kick" whose size reads the same whatever the spring's
        /// stiffness or damping. The response is linear in the velocity, so a unit kick is integrated with
        /// the very same sub-steps (≤ 1.5 s, allocation-free) and its peak measured: the semi-implicit
        /// integrator damps a little more than the analytic spring, and this stays exact for it.
        /// <paramref name="frequencyHz"/> ≤ 0 → 0 (a snapping spring never moves).
        /// </summary>
        public static float SpringVelocityForPeak(float peak, float frequencyHz, float dampingRatio)
        {
            if (frequencyHz <= 0f || peak == 0f)
            {
                return 0f;
            }

            float x = 0f, v = 1f, best = 0f;
            for (int i = 0; i < 180 && v > 0f; i++)
            {
                StepSpring(ref x, ref v, 0f, frequencyHz, dampingRatio, MaxSpringStep);
                best = Mathf.Max(best, x);
            }

            return peak / Mathf.Max(1e-6f, best);
        }

        /// <summary>Spring frequency (Hz) of a critically damped spring that covers ~90 % of a step in <paramref name="easeTime"/> s (ease-in/out without overshoot). ≤ 0 → 0 (snap).</summary>
        public static float EaseFrequency(float easeTime)
        {
            // Step response 1 − (1 + ωt)e^(−ωt) reaches 0.9 at ωt ≈ 3.89.
            return easeTime > 0f ? 3.89f / (2f * Mathf.PI * easeTime) : 0f;
        }

        // ------------------------------------------------------------------
        // Turn braking
        // ------------------------------------------------------------------

        /// <summary>
        /// Target-speed multiplier while the movement direction still has to swing through
        /// <paramref name="angleDegrees"/>: 1 below <paramref name="brakeAngle"/> (normal arcing turns),
        /// easing down to <paramref name="speedKept"/> at <paramref name="reversalAngle"/> (where the
        /// motor's own reversal snap takes over). A sharp turn therefore brakes briefly, plants and
        /// pivots instead of whipping round at full speed. <paramref name="brakeAngle"/> ≥ reversal disables it.
        /// </summary>
        public static float TurnBrakeFactor(float angleDegrees, float brakeAngle, float reversalAngle, float speedKept)
        {
            if (brakeAngle >= reversalAngle || angleDegrees <= brakeAngle)
            {
                return 1f;
            }

            float u = Mathf.Clamp01((angleDegrees - brakeAngle) / (reversalAngle - brakeAngle));
            return Mathf.Lerp(1f, Mathf.Clamp01(speedKept), Mathf.SmoothStep(0f, 1f, u));
        }

        /// <summary>True when a direction change is sharp enough to brake (and the hero moves fast enough for it to matter).</summary>
        public static bool ShouldTurnBrake(float angleDegrees, float brakeAngle, float speed, float minSpeed)
        {
            return angleDegrees > brakeAngle && speed > minSpeed;
        }

        // ------------------------------------------------------------------
        // Squash & stretch
        // ------------------------------------------------------------------

        /// <summary>
        /// Volume-preserving scale for a squash/stretch amount <paramref name="stretch"/> (+ = taller,
        /// − = squashed), clamped to ±<paramref name="maxAmount"/>: y = 1 + s, x = z = 1/√(1 + s),
        /// so x·y·z = 1.
        /// </summary>
        public static Vector3 SquashScale(float stretch, float maxAmount = MaxSquash)
        {
            float s = Mathf.Clamp(stretch, -Mathf.Abs(maxAmount), Mathf.Abs(maxAmount));
            float side = 1f / Mathf.Sqrt(1f + s);
            return new Vector3(side, 1f + s, side);
        }

        /// <summary>
        /// Landing squash (0..<paramref name="maxSquash"/>, positive = amount to squash) for a touchdown at
        /// <paramref name="fallSpeed"/> m/s (sign ignored), shaped by the profile curve (x = fall speed, y =
        /// 0..1 of the max). No curve: linear from 2 to 10 m/s.
        /// </summary>
        public static float LandingSquash(float fallSpeed, AnimationCurve curve, float maxSquash)
        {
            float speed = Mathf.Abs(fallSpeed);
            float fraction = curve != null && curve.length > 0 ? curve.Evaluate(speed) : Mathf.InverseLerp(2f, 10f, speed);
            return Mathf.Clamp01(fraction) * Mathf.Clamp(maxSquash, 0f, MaxSquash);
        }

        /// <summary>Airborne stretch from vertical speed: elongates on the way up and down, neutral at the apex (arc reads as motion).</summary>
        public static float AirStretch(float verticalSpeed, float maxStretch)
        {
            return Mathf.Clamp(Mathf.Abs(verticalSpeed) / 8f, 0f, 1f) * Mathf.Clamp(maxStretch, 0f, MaxSquash);
        }

        // ------------------------------------------------------------------
        // Attack timeline (anticipation → impact → follow-through → recovery)
        // ------------------------------------------------------------------

        /// <summary>
        /// Maps seconds since the attack input to the profile's attack-curve axis: [0, 0.5] is the
        /// anticipation and lands exactly on 0.5 at <paramref name="strikeDelay"/> (the frame
        /// <see cref="PlayerBasicAttack"/> applies damage), (0.5, 1] the follow-through and recovery over
        /// <paramref name="recoverTime"/>. Returns -1 before the input and past the end.
        /// </summary>
        public static float AttackCurveX(float t, float strikeDelay, float recoverTime)
        {
            if (t < 0f)
            {
                return -1f;
            }

            if (strikeDelay > 0f && t <= strikeDelay)
            {
                return 0.5f * t / strikeDelay;
            }

            float after = t - Mathf.Max(0f, strikeDelay);
            if (recoverTime <= 0f || after > recoverTime)
            {
                return -1f;
            }

            return 0.5f + 0.5f * after / recoverTime;
        }

        /// <summary>Total length (s) of the attack timeline.</summary>
        public static float AttackLength(float strikeDelay, float recoverTime)
        {
            return Mathf.Max(0f, strikeDelay) + Mathf.Max(0f, recoverTime);
        }

        /// <summary>
        /// Torso twist (−1..1) along the attack-curve axis: winds back through the anticipation
        /// (peak at 0.4), whips through the impact (−1 just after 0.5 — follow-through), settles by 1.
        /// </summary>
        public static float AttackTwistAt(float x)
        {
            if (x <= 0f || x >= 1f)
            {
                return 0f;
            }

            if (x < 0.4f)
            {
                return Mathf.SmoothStep(0f, 1f, x / 0.4f);
            }

            if (x < 0.56f)
            {
                return Mathf.Lerp(1f, -1f, Mathf.SmoothStep(0f, 1f, (x - 0.4f) / 0.16f));
            }

            return Mathf.Lerp(-1f, 0f, Mathf.SmoothStep(0f, 1f, (x - 0.56f) / 0.44f));
        }

        /// <summary>
        /// Whole-body lunge (−0.3..1 of the lunge distance) along the attack-curve axis: a small
        /// pull-back as anticipation, the push peaking just after the impact, eased return.
        /// </summary>
        public static float AttackLungeAt(float x)
        {
            if (x <= 0f || x >= 1f)
            {
                return 0f;
            }

            if (x < 0.4f)
            {
                return -0.3f * Mathf.SmoothStep(0f, 1f, x / 0.4f);
            }

            if (x < 0.55f)
            {
                return Mathf.Lerp(-0.3f, 1f, Mathf.SmoothStep(0f, 1f, (x - 0.4f) / 0.15f));
            }

            return Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, (x - 0.55f) / 0.45f));
        }

        /// <summary>Default arm curve for the attack (x per <see cref="AttackCurveX"/>, y = fraction of the arm angle): cock high, chop through on 0.5, swing slightly past, recover.</summary>
        public static AnimationCurve DefaultAttackArmCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.18f, 0.25f),
                new Keyframe(0.4f, 1f, 0f, 0f),
                new Keyframe(0.5f, 0.35f, -8f, -8f),
                new Keyframe(0.6f, 0.12f, 0f, 0f),
                new Keyframe(1f, 0f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Idle
        // ------------------------------------------------------------------

        /// <summary>
        /// Head yaw (degrees) of the occasional idle glance after <paramref name="idleTime"/> seconds
        /// standing: every ~<paramref name="interval"/> s (jittered ±30 % per glance by a hash of its
        /// index and <paramref name="seed"/>) the head turns to a random side by up to
        /// <paramref name="maxYaw"/>, holds <paramref name="holdTime"/> s and comes back (eased).
        /// Deterministic (no Random), 0 for the first interval.
        /// </summary>
        public static float IdleGlanceYaw(float idleTime, float interval, float holdTime, float maxYaw, int seed)
        {
            if (interval <= 0f || idleTime < interval || maxYaw == 0f)
            {
                return 0f;
            }

            // Glance k starts at k·interval + jitter(k), k ≥ 1.
            int k = Mathf.FloorToInt(idleTime / interval);
            float start = k * interval + Hash01(k, seed) * 0.3f * interval;
            float local = idleTime - start;
            if (local < 0f)
            {
                // Before this one's jittered start: maybe still in the previous glance.
                if (k <= 1)
                {
                    return 0f;
                }

                k -= 1;
                start = k * interval + Hash01(k, seed) * 0.3f * interval;
                local = idleTime - start;
            }

            const float turn = 0.25f;
            float length = turn * 2f + Mathf.Max(0f, holdTime);
            if (local < 0f || local > length)
            {
                return 0f;
            }

            float side = Hash01(k * 31 + 7, seed) < 0.5f ? -1f : 1f;
            float amount = Mathf.Lerp(0.45f, 1f, Hash01(k * 17 + 3, seed));
            float envelope = LocomotionMath.Envelope(local, turn, holdTime, turn);
            return side * amount * maxYaw * envelope;
        }

        /// <summary>Cheap integer hash → [0, 1).</summary>
        public static float Hash01(int a, int b)
        {
            unchecked
            {
                uint h = (uint)a * 374761393u + (uint)b * 668265263u;
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        // ------------------------------------------------------------------
        // Gait phase from the feet
        // ------------------------------------------------------------------

        /// <summary>
        /// Stride phase (−1..1) read from the animated feet: + when the left foot is ahead of the
        /// right along <paramref name="forward"/>, normalised by <paramref name="strideReference"/> (half
        /// the usual foot separation). Drives torso counter-rotation and the arm swing opposite the legs.
        /// </summary>
        public static float StridePhase(Vector3 leftFoot, Vector3 rightFoot, Vector3 forward, float strideReference)
        {
            if (strideReference <= 1e-4f)
            {
                return 0f;
            }

            forward.y = 0f;
            if (forward.sqrMagnitude < 1e-8f)
            {
                return 0f;
            }

            return Mathf.Clamp(Vector3.Dot(leftFoot - rightFoot, forward.normalized) / strideReference, -1f, 1f);
        }

        // ------------------------------------------------------------------
        // Foot IK
        // ------------------------------------------------------------------

        /// <summary>
        /// Vertical correction (m) for one foot: how far the ground under the foot is above (+) or below (−)
        /// the ground under the character root, clamped to ±<paramref name="maxOffset"/>. The IK target is
        /// the animated foot moved by this, so the clip's own lift is preserved (no stuck feet) while the
        /// sole follows slopes/steps. No hit → 0.
        /// </summary>
        public static float FootGroundOffset(bool hasHit, float groundY, float rootY, float maxOffset)
        {
            if (!hasHit)
            {
                return 0f;
            }

            return Mathf.Clamp(groundY - rootY, -Mathf.Abs(maxOffset), Mathf.Abs(maxOffset));
        }

        /// <summary>
        /// Pelvis (body) drop for a pair of foot offsets: the hips go down by the lower foot's negative
        /// offset so that foot can reach the ground (the other leg bends to its higher target); never up —
        /// lifting the hips would hyper-extend the legs.
        /// </summary>
        public static float PelvisOffset(float leftOffset, float rightOffset)
        {
            return Mathf.Min(0f, Mathf.Min(leftOffset, rightOffset));
        }

        /// <summary>
        /// Whether a locked (planted) foot must be released to step: it drifted more than
        /// <paramref name="maxDrift"/> metres from where the animation wants it, or the body turned more
        /// than <paramref name="maxTwistDegrees"/> away from the planted foot's heading.
        /// </summary>
        public static bool ShouldReleaseFootLock(Vector3 lockedPosition, Vector3 animatedPosition, float twistDegrees, float maxDrift, float maxTwistDegrees)
        {
            Vector3 drift = animatedPosition - lockedPosition;
            drift.y = 0f;
            return drift.sqrMagnitude > maxDrift * maxDrift || Mathf.Abs(twistDegrees) > maxTwistDegrees;
        }

        /// <summary>Foot lift (m) of an in-place re-plant step at progress <paramref name="u"/> (0..1): a sine arc.</summary>
        public static float StepArc(float u, float height)
        {
            return u <= 0f || u >= 1f ? 0f : Mathf.Sin(u * Mathf.PI) * height;
        }

        // ------------------------------------------------------------------
        // Animator parameters
        // ------------------------------------------------------------------

        /// <summary>
        /// The <c>VerticalSpeed</c> Animator parameter: the real vertical speed while airborne; on the
        /// ground the last airborne (touchdown) speed is held for <paramref name="holdTime"/> s so the
        /// Fall → Land transition can read how hard the landing was, then 0 (the grounded stick force is
        /// not "falling").
        /// </summary>
        public static float AnimatorVerticalSpeed(bool grounded, float verticalVelocity, float touchdownVelocity, float timeSinceLanding, float holdTime)
        {
            if (!grounded)
            {
                return verticalVelocity;
            }

            return timeSinceLanding <= holdTime ? touchdownVelocity : 0f;
        }

        /// <summary>Signed angle (deg, + = right) from the facing to the wanted direction on the ground plane; 0 without a direction.</summary>
        public static float SignedTurnAngle(Vector3 facing, Vector3 desired)
        {
            facing.y = 0f;
            desired.y = 0f;
            if (facing.sqrMagnitude < 1e-8f || desired.sqrMagnitude < 1e-8f)
            {
                return 0f;
            }

            return Vector3.SignedAngle(facing, desired, Vector3.up);
        }
    }
}
