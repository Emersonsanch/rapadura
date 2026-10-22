using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Pure, allocation-free math behind the hero's fluid locomotion and animation layering, kept
    /// out of the MonoBehaviours so it is EditMode-testable without a scene: speed ramps
    /// (acceleration/deceleration), velocity-direction and facing turn rates, click-to-move
    /// destination smoothing and arrival braking, Animator blend/stride-matched playback speed,
    /// and the short envelopes/curves the procedural layers (lean, swing, cast, flinch) use.
    /// </summary>
    public static class LocomotionMath
    {
        /// <summary>Standard gravity (m/s²) used to turn accelerations into physically plausible lean angles.</summary>
        public const float Gravity = 9.81f;

        // ------------------------------------------------------------------
        // Speed / direction / facing
        // ------------------------------------------------------------------

        /// <summary>
        /// Moves <paramref name="current"/> speed toward <paramref name="target"/>, using
        /// <paramref name="acceleration"/> (m/s²) when speeding up and <paramref name="deceleration"/>
        /// when slowing down — never an instant start/stop.
        /// </summary>
        public static float StepSpeed(float current, float target, float acceleration, float deceleration, float deltaTime)
        {
            float rate = target > current ? acceleration : deceleration;
            return Mathf.MoveTowards(current, target, Mathf.Max(0f, rate) * Mathf.Max(0f, deltaTime));
        }

        /// <summary>
        /// Acceleration (m/s²) needed to go from 0 to <paramref name="referenceSpeed"/> in
        /// <paramref name="timeToReach"/> seconds, scaled by an optional curve factor (the motor's
        /// acceleration curve evaluated at the current normalized speed: a strong first push that
        /// eases into top speed).
        /// </summary>
        public static float AccelerationFor(float referenceSpeed, float timeToReach, float curveFactor = 1f)
        {
            if (timeToReach <= 0f)
            {
                return float.PositiveInfinity;
            }

            return Mathf.Max(0f, referenceSpeed) / timeToReach * Mathf.Max(0f, curveFactor);
        }

        /// <summary>
        /// Rotates the horizontal movement direction toward <paramref name="desired"/> by at most
        /// <paramref name="maxDegreesPerSecond"/> (arcs instead of instant kinks). A near-reversal
        /// (angle above <paramref name="snapAngle"/>) snaps — arcing through 180° would run sideways.
        /// Returns a normalized direction (or zero when both inputs are zero).
        /// </summary>
        public static Vector3 StepDirection(Vector3 current, Vector3 desired, float maxDegreesPerSecond, float deltaTime, float snapAngle = 150f)
        {
            current.y = 0f;
            desired.y = 0f;

            if (desired.sqrMagnitude < 1e-8f)
            {
                return current.sqrMagnitude < 1e-8f ? Vector3.zero : current.normalized;
            }

            desired.Normalize();

            if (current.sqrMagnitude < 1e-8f || Vector3.Angle(current, desired) > snapAngle)
            {
                return desired;
            }

            Vector3 rotated = Vector3.RotateTowards(current.normalized, desired, maxDegreesPerSecond * Mathf.Deg2Rad * Mathf.Max(0f, deltaTime), 0f);
            rotated.y = 0f;
            return rotated.sqrMagnitude < 1e-8f ? desired : rotated.normalized;
        }

        /// <summary>Facing turn rate (deg/s): <paramref name="stoppedRate"/> at rest, blending to <paramref name="movingRate"/> at full speed (turning in place is a bit snappier).</summary>
        public static float TurnRate(float movingRate, float stoppedRate, float normalizedSpeed)
        {
            return Mathf.Lerp(stoppedRate, movingRate, Mathf.Clamp01(normalizedSpeed));
        }

        /// <summary>Rotates <paramref name="current"/> toward facing <paramref name="direction"/> (flattened) at <paramref name="degreesPerSecond"/>.</summary>
        public static Quaternion StepFacing(Quaternion current, Vector3 direction, float degreesPerSecond, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 1e-8f)
            {
                return current;
            }

            Quaternion target = Quaternion.LookRotation(direction.normalized, Vector3.up);
            return Quaternion.RotateTowards(current, target, Mathf.Max(0f, degreesPerSecond) * Mathf.Max(0f, deltaTime));
        }

        // ------------------------------------------------------------------
        // Click-to-move / hold-to-walk
        // ------------------------------------------------------------------

        /// <summary>
        /// Low-pass filters the click/hold destination so the steering direction glides between the
        /// 0.15 s hold-to-walk re-aims instead of kinking at each one (zig-zag). Snaps straight to
        /// <paramref name="raw"/> when there is no meaningful previous value, when the new point is
        /// in a clearly different direction (a genuinely new click) or <paramref name="smoothingTime"/> ≤ 0.
        /// Exponential, so it is frame-rate independent.
        /// </summary>
        public static Vector3 SmoothDestination(Vector3 position, Vector3 smoothed, Vector3 raw, float deltaTime, float smoothingTime, float snapAngleDegrees)
        {
            if (smoothingTime <= 0f)
            {
                return raw;
            }

            Vector3 toRaw = raw - position;
            toRaw.y = 0f;
            Vector3 toSmoothed = smoothed - position;
            toSmoothed.y = 0f;

            if (toRaw.sqrMagnitude < 1e-4f || toSmoothed.sqrMagnitude < 1e-4f || Vector3.Angle(toRaw, toSmoothed) > snapAngleDegrees)
            {
                return raw;
            }

            float t = 1f - Mathf.Exp(-Mathf.Max(0f, deltaTime) / smoothingTime);
            return Vector3.Lerp(smoothed, raw, t);
        }

        /// <summary>
        /// Highest speed that can still brake to a stop over <paramref name="distance"/> at
        /// <paramref name="deceleration"/> (v = √(2·a·d)), clamped to [<paramref name="minSpeed"/>,
        /// <paramref name="maxSpeed"/>] — the hero eases into the destination instead of stopping dead.
        /// </summary>
        public static float ArrivalSpeed(float distance, float maxSpeed, float deceleration, float minSpeed)
        {
            if (maxSpeed <= 0f)
            {
                return 0f;
            }

            float braking = Mathf.Sqrt(2f * Mathf.Max(0f, deceleration) * Mathf.Max(0f, distance));
            return Mathf.Clamp(braking, Mathf.Min(minSpeed, maxSpeed), maxSpeed);
        }

        /// <summary>
        /// Arrival test with a stop-distance tolerance: inside <paramref name="threshold"/>, or already
        /// past the point (moving away from it) while within <paramref name="passedTolerance"/> — so a
        /// hero who overshoots by a hair doesn't turn around for a tiny back-step.
        /// </summary>
        public static bool HasArrived(Vector3 toDestination, Vector3 velocity, float threshold, float passedTolerance)
        {
            toDestination.y = 0f;
            float distance = toDestination.magnitude;

            if (distance <= threshold)
            {
                return true;
            }

            velocity.y = 0f;
            return distance <= passedTolerance && velocity.sqrMagnitude > 0.01f && Vector3.Dot(toDestination, velocity) < 0f;
        }

        // ------------------------------------------------------------------
        // Animator
        // ------------------------------------------------------------------

        /// <summary>Normalized locomotion blend: 0 idle → 0.5 at walk speed → 1 at run speed (and above).</summary>
        public static float SpeedToBlend(float speed, float walkSpeed, float runSpeed)
        {
            if (speed <= 0f || walkSpeed <= 0f)
            {
                return 0f;
            }

            if (speed <= walkSpeed)
            {
                return 0.5f * speed / walkSpeed;
            }

            return Mathf.Clamp01(0.5f + 0.5f * (speed - walkSpeed) / Mathf.Max(0.01f, runSpeed - walkSpeed));
        }

        /// <summary>
        /// Ground speed (m/s) the blended clips "walk at" for a given blend value at playback 1: the
        /// idle child is static, so 0..0.5 scales the walk stride down, 0.5..1 crossfades walk→run.
        /// </summary>
        public static float BlendNaturalSpeed(float blend, float walkClipSpeed, float runClipSpeed)
        {
            blend = Mathf.Clamp01(blend);
            return blend <= 0.5f
                ? walkClipSpeed * (blend / 0.5f)
                : Mathf.Lerp(walkClipSpeed, runClipSpeed, (blend - 0.5f) / 0.5f);
        }

        /// <summary>
        /// Animator playback multiplier that makes the feet cover exactly the ground the body does
        /// (no foot sliding): actual / natural clip speed, clamped. 1 when (nearly) standing still.
        /// </summary>
        public static float StridePlaybackSpeed(float actualSpeed, float naturalSpeed, float minPlayback, float maxPlayback)
        {
            if (actualSpeed < 0.05f || naturalSpeed < 0.05f)
            {
                return 1f;
            }

            return Mathf.Clamp(actualSpeed / naturalSpeed, minPlayback, maxPlayback);
        }

        /// <summary>Critically-damped smoothing of an Animator parameter (same family as Animator.SetFloat's dampTime), testable and frame-rate independent.</summary>
        public static float DampParameter(float current, float target, ref float velocity, float smoothTime, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return current;
            }

            if (smoothTime <= 0f)
            {
                velocity = 0f;
                return target;
            }

            return Mathf.SmoothDamp(current, target, ref velocity, smoothTime, Mathf.Infinity, deltaTime);
        }

        /// <summary>Exponential smoothing factor for a frame (1 - e^(-rate·dt)).</summary>
        public static float ExpBlend(float rate, float deltaTime)
        {
            return 1f - Mathf.Exp(-Mathf.Max(0f, rate) * Mathf.Max(0f, deltaTime));
        }

        // ------------------------------------------------------------------
        // Procedural layers
        // ------------------------------------------------------------------

        /// <summary>
        /// Body lean (degrees) a runner adopts: x = forward pitch from forward acceleration
        /// (atan(a/g)), y = roll into the turn from centripetal acceleration (ω·v, atan(ω·v/g);
        /// positive yaw rate = turning right = leaning right). Gains scale, max clamps.
        /// </summary>
        public static Vector2 LeanAngles(float forwardAcceleration, float yawRateDegrees, float speed, float accelGain, float turnGain, float maxPitch, float maxRoll)
        {
            float pitch = Mathf.Atan2(forwardAcceleration, Gravity) * Mathf.Rad2Deg * accelGain;
            float centripetal = yawRateDegrees * Mathf.Deg2Rad * Mathf.Max(0f, speed);
            float roll = Mathf.Atan2(centripetal, Gravity) * Mathf.Rad2Deg * turnGain;
            return new Vector2(Mathf.Clamp(pitch, -maxPitch, maxPitch), Mathf.Clamp(roll, -maxRoll, maxRoll));
        }

        /// <summary>Phase advance (radians) of a rigid-body step bob: one π per step of <paramref name="stepLength"/> metres, so the bob cadence matches the ground speed.</summary>
        public static float StepPhaseAdvance(float speed, float stepLength, float deltaTime)
        {
            if (stepLength <= 0f)
            {
                return 0f;
            }

            return Mathf.PI * Mathf.Max(0f, speed) / stepLength * Mathf.Max(0f, deltaTime);
        }

        /// <summary>
        /// Attack/hold/release envelope (0..1) at time <paramref name="t"/> since the trigger: smooth
        /// rise over <paramref name="attack"/>, flat <paramref name="hold"/>, smooth fall over
        /// <paramref name="release"/>; 0 before 0 and after the end.
        /// </summary>
        public static float Envelope(float t, float attack, float hold, float release)
        {
            if (t < 0f)
            {
                return 0f;
            }

            if (t < attack)
            {
                return Mathf.SmoothStep(0f, 1f, t / attack);
            }

            t -= Mathf.Max(0f, attack);
            if (t < hold)
            {
                return 1f;
            }

            t -= Mathf.Max(0f, hold);
            if (t < release)
            {
                return 1f - Mathf.SmoothStep(0f, 1f, t / release);
            }

            return 0f;
        }

        /// <summary>Total length of an <see cref="Envelope"/>.</summary>
        public static float EnvelopeLength(float attack, float hold, float release)
        {
            return Mathf.Max(0f, attack) + Mathf.Max(0f, hold) + Mathf.Max(0f, release);
        }

        /// <summary>
        /// Arm swing (0..1 of the configured arm angle, 1 = raised high) of the procedural melee
        /// strike at normalized time <paramref name="u"/> (0..1). <see cref="PlayerBasicAttack"/>
        /// raises its event the instant damage lands, so the strike is front-loaded: a whip up over
        /// the first 12 %, the chop down through the target until 35 %, then an eased recovery.
        /// </summary>
        public static float AttackArmSwing(float u)
        {
            if (u <= 0f || u >= 1f)
            {
                return 0f;
            }

            if (u < 0.12f)
            {
                return Mathf.SmoothStep(0f, 1f, u / 0.12f);
            }

            if (u < 0.35f)
            {
                return Mathf.Lerp(1f, 0.45f, Mathf.SmoothStep(0f, 1f, (u - 0.12f) / 0.23f));
            }

            return Mathf.Lerp(0.45f, 0f, Mathf.SmoothStep(0f, 1f, (u - 0.35f) / 0.65f));
        }

        /// <summary>Torso twist (−1..1) through the strike: wind back while the arm rises, whip through on the chop, settle.</summary>
        public static float AttackTwist(float u)
        {
            if (u <= 0f || u >= 1f)
            {
                return 0f;
            }

            if (u < 0.12f)
            {
                return Mathf.SmoothStep(0f, 1f, u / 0.12f);
            }

            if (u < 0.35f)
            {
                return Mathf.Lerp(1f, -1f, Mathf.SmoothStep(0f, 1f, (u - 0.12f) / 0.23f));
            }

            return Mathf.Lerp(-1f, 0f, Mathf.SmoothStep(0f, 1f, (u - 0.35f) / 0.65f));
        }

        // ------------------------------------------------------------------
        // Clip analysis (used by the Rebuild Hero Animators editor tool)
        // ------------------------------------------------------------------

        /// <summary>
        /// Ground speed of a looping gait clip, from one foot's position relative to the hips sampled
        /// at a fixed <paramref name="sampleInterval"/> over the cycle (model space, y up). While a
        /// foot is planted (within the lowest <paramref name="stanceFraction"/> of its height range)
        /// it slides backward relative to the hips at exactly the ground speed — whether the clip is
        /// in-place or translates the hips. Returns the mean horizontal speed over stance samples, or
        /// 0 when it can't tell.
        /// </summary>
        public static float EstimateStanceSpeed(Vector3[] footRelativeToHips, float sampleInterval, float stanceFraction = 0.2f)
        {
            if (footRelativeToHips == null || footRelativeToHips.Length < 3 || sampleInterval <= 0f)
            {
                return 0f;
            }

            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            foreach (Vector3 p in footRelativeToHips)
            {
                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            float limit = minY + Mathf.Max(1e-5f, maxY - minY) * Mathf.Clamp01(stanceFraction);
            float total = 0f;
            int count = 0;

            for (int i = 0; i + 1 < footRelativeToHips.Length; i++)
            {
                Vector3 a = footRelativeToHips[i];
                Vector3 b = footRelativeToHips[i + 1];
                if (a.y > limit || b.y > limit)
                {
                    continue;
                }

                Vector2 delta = new Vector2(b.x - a.x, b.z - a.z);
                total += delta.magnitude / sampleInterval;
                count++;
            }

            return count > 0 ? total / count : 0f;
        }

        /// <summary>
        /// Index of the most "standing" sample of a walk cycle to freeze as the idle pose: both feet
        /// low (planted) and close together horizontally. Inputs are per-sample foot positions
        /// relative to the hips (model space).
        /// </summary>
        public static int ChooseStandingSample(Vector3[] leftFoot, Vector3[] rightFoot)
        {
            if (leftFoot == null || rightFoot == null || leftFoot.Length == 0 || leftFoot.Length != rightFoot.Length)
            {
                return 0;
            }

            float minY = float.PositiveInfinity;
            float maxSeparation = 1e-5f;
            for (int i = 0; i < leftFoot.Length; i++)
            {
                minY = Mathf.Min(minY, Mathf.Min(leftFoot[i].y, rightFoot[i].y));
                maxSeparation = Mathf.Max(maxSeparation, HorizontalDistance(leftFoot[i], rightFoot[i]));
            }

            int best = 0;
            float bestScore = float.PositiveInfinity;
            for (int i = 0; i < leftFoot.Length; i++)
            {
                float lift = Mathf.Max(leftFoot[i].y, rightFoot[i].y) - minY;
                float separation = HorizontalDistance(leftFoot[i], rightFoot[i]);
                float score = lift / maxSeparation * 2f + separation / maxSeparation;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            return best;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            return new Vector2(a.x - b.x, a.z - b.z).magnitude;
        }

        /// <summary>Death collapse progress (0..1): gravity-like ease-in (t²) over <paramref name="duration"/>.</summary>
        public static float FallProgress(float t, float duration)
        {
            if (duration <= 0f)
            {
                return t >= 0f ? 1f : 0f;
            }

            float u = Mathf.Clamp01(t / duration);
            return u * u;
        }
    }
}
