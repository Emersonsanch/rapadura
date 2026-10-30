using UnityEngine;

namespace Rapadura.Gameplay.Locomotion
{
    /// <summary>Which footfall pattern a legged body uses (see <see cref="LegGaitMath.PhaseOffset"/>).</summary>
    public enum LegGaitStyle
    {
        /// <summary>Four legs: walk (lateral sequence) → trot (diagonal pairs) → transverse gallop, blended by speed.</summary>
        Quadruped = 0,

        /// <summary>Six or eight legs: alternating tetrapod/tripod groups with a back-to-front ripple.</summary>
        Arthropod = 1,

        /// <summary>Two legs, half a cycle apart; long stance (heavy stomp).</summary>
        Biped = 2,
    }

    /// <summary>
    /// Pure, allocation-free math of the procedural leg locomotion (EditMode-tested): per-leg phase
    /// offsets and duty factors per gait, the speed-based walk → trot → gallop blend, stride length and
    /// cadence from speed, the step trigger, landing prediction, the swing arc and an analytic two-bone
    /// IK. <see cref="LegGaitSolver"/> runs the per-leg state machine on top of it and
    /// <see cref="ProceduralLegLocomotion"/> applies it to transforms. Nothing here knows about enemies,
    /// so a pet or a prop can reuse it.
    ///
    /// Leg order convention (what <see cref="ProceduralLegLocomotion"/> sorts its chains into): pairs
    /// front to back, left before right, i.e. leg 2k = left of pair k, 2k + 1 = right of pair k. For a
    /// quadruped that is 0 FL, 1 FR, 2 HL, 3 HR.
    /// </summary>
    public static class LegGaitMath
    {
        /// <summary>Fraction of the cycle each foot is on the ground.</summary>
        public const float WalkDuty = 0.68f;
        public const float TrotDuty = 0.5f;
        public const float GallopDuty = 0.38f;
        public const float ArthropodDuty = 0.55f;
        public const float BipedDuty = 0.62f;
        /// <summary>Biped duty at a full run (the "trot" blend doubles as walk → run for two legs).</summary>
        public const float BipedRunDuty = 0.42f;

        /// <summary>Back-to-front delay (cycles) between neighbouring pairs of an arthropod group (metachronal ripple).</summary>
        public const float ArthropodRipple = 0.05f;

        // Quadruped offsets (cycles) per leg: FL, FR, HL, HR.
        // Walk: lateral sequence HL → FL → HR → FR, a quarter cycle apart.
        private static readonly float[] WalkOffsets = { 0.25f, 0.75f, 0f, 0.5f };
        // Trot: diagonal pairs move together (FL + HR, FR + HL), half a cycle apart.
        private static readonly float[] TrotOffsets = { 0.25f, 0.75f, 0.75f, 0.25f };
        // Transverse gallop: fore pair close together, hind pair close together, half a cycle apart.
        private static readonly float[] GallopOffsets = { 0.25f, 0.35f, 0.75f, 0.85f };

        /// <summary>x − floor(x): 0..1.</summary>
        public static float Frac(float x) => x - Mathf.Floor(x);

        /// <summary>Interpolates two phases (cycles) along the shorter way round the circle.</summary>
        public static float CircularLerp(float a, float b, float t)
        {
            float d = Mathf.Repeat(b - a + 0.5f, 1f) - 0.5f;
            return Frac(a + d * Mathf.Clamp01(t));
        }

        /// <summary>Shortest signed phase difference b − a, in −0.5..0.5 cycles.</summary>
        public static float PhaseDelta(float a, float b) => Mathf.Repeat(b - a + 0.5f, 1f) - 0.5f;

        public static float SmoothStep01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        /// <summary>
        /// Quadruped gait blends from speed: <paramref name="trot"/> eases in from half
        /// <paramref name="trotSpeed"/> to <paramref name="trotSpeed"/>, <paramref name="gallop"/> from
        /// <paramref name="gallopStart"/> to <paramref name="gallopFull"/> (≤ start disables the gallop).
        /// </summary>
        public static void QuadrupedBlend(float speed, float trotSpeed, float gallopStart, float gallopFull, out float trot, out float gallop)
        {
            float half = Mathf.Max(0.01f, trotSpeed * 0.5f);
            trot = SmoothStep01((speed - half) / half);
            gallop = gallopFull > gallopStart ? SmoothStep01((speed - gallopStart) / (gallopFull - gallopStart)) : 0f;
        }

        /// <summary>Phase offset (cycles, 0..1) of quadruped leg 0..3 (FL, FR, HL, HR) for the walk/trot/gallop blend.</summary>
        public static float QuadrupedOffset(int leg, float trot, float gallop)
        {
            int i = Mathf.Clamp(leg, 0, 3);
            float walkToTrot = CircularLerp(WalkOffsets[i], TrotOffsets[i], trot);
            return CircularLerp(walkToTrot, GallopOffsets[i], gallop);
        }

        /// <summary>
        /// Alternating groups for many legs: pair k's left leg is in group (k mod 2), its right leg in
        /// the other, so every leg's neighbours (across and along) step half a cycle away (8 legs:
        /// alternating tetrapod, 6 legs: tripod). Hind pairs lead by <see cref="ArthropodRipple"/>.
        /// </summary>
        public static float ArthropodOffset(int leg, int legCount)
        {
            int pairs = Mathf.Max(1, (legCount + 1) / 2);
            int pair = leg / 2;
            int side = leg % 2;
            int group = (pair + side) % 2;
            return Frac(group * 0.5f + (pairs - 1 - pair) * ArthropodRipple);
        }

        /// <summary>Phase offset (cycles) of <paramref name="leg"/> of <paramref name="legCount"/> for the style and quadruped blends.</summary>
        public static float PhaseOffset(LegGaitStyle style, int leg, int legCount, float trot = 0f, float gallop = 0f)
        {
            if (legCount <= 2 || style == LegGaitStyle.Biped)
            {
                return (leg % 2) * 0.5f;
            }

            if (style == LegGaitStyle.Quadruped && legCount == 4)
            {
                return QuadrupedOffset(leg, trot, gallop);
            }

            return ArthropodOffset(leg, legCount);
        }

        /// <summary>Duty factor (fraction of the cycle on the ground) for the style and blends (bipeds: <paramref name="trot"/> = walk → run).</summary>
        public static float DutyFactor(LegGaitStyle style, int legCount, float trot = 0f, float gallop = 0f)
        {
            if (legCount <= 2 || style == LegGaitStyle.Biped)
            {
                return Mathf.Lerp(BipedDuty, BipedRunDuty, trot);
            }

            if (style == LegGaitStyle.Quadruped && legCount == 4)
            {
                return Mathf.Lerp(Mathf.Lerp(WalkDuty, TrotDuty, trot), GallopDuty, gallop);
            }

            return ArthropodDuty;
        }

        /// <summary>Stride (m per cycle): <paramref name="baseStride"/> lengthened by up to <paramref name="growth"/> × at <paramref name="fullSpeed"/>.</summary>
        public static float StrideLength(float baseStride, float speed, float fullSpeed, float growth)
        {
            float k = fullSpeed > 0f ? Mathf.Clamp01(speed / fullSpeed) : 0f;
            return Mathf.Max(0.02f, baseStride * (1f + Mathf.Max(0f, growth) * k));
        }

        /// <summary>Gait cycles per second at <paramref name="speed"/> m/s for a <paramref name="stride"/>, clamped; 0 when standing.</summary>
        public static float Cadence(float speed, float stride, float minHz, float maxHz)
        {
            if (speed < 0.05f)
            {
                return 0f;
            }

            return Mathf.Clamp(speed / Mathf.Max(0.02f, stride), minHz, Mathf.Max(minHz, maxHz));
        }

        /// <summary>
        /// A planted foot lifts when it is far enough from its rest target and its phase window is open
        /// (gait order), or — whatever the phase — when it is beyond reach (turning, shoves, lunges).
        /// </summary>
        public static bool ShouldStep(float displacement, float minStep, bool windowOpen, float urgentDistance)
        {
            return displacement >= urgentDistance || (windowOpen && displacement >= minStep);
        }

        /// <summary>
        /// Where a step should land: the rest target as it will be when the foot lands (carried by the body
        /// velocity and turned by the yaw rate), plus half a stance of travel so the foot passes under the
        /// hip mid-stance. The lead is clamped to <paramref name="maxLead"/> and the turn to ±45°.
        /// </summary>
        public static Vector3 PredictLanding(Vector3 bodyPosition, Vector3 restTarget, Vector3 velocity, float yawRateDeg,
            float timeToLand, float stanceTime, float maxLead)
        {
            velocity.y = 0f;
            float ahead = Mathf.Max(0f, timeToLand);
            float turn = Mathf.Clamp(yawRateDeg * (ahead + stanceTime * 0.5f), -45f, 45f);
            Vector3 fromBody = restTarget - bodyPosition;
            Vector3 rest = bodyPosition + velocity * ahead + Quaternion.AngleAxis(turn, Vector3.up) * fromBody;
            return rest + Vector3.ClampMagnitude(velocity * (stanceTime * 0.5f), Mathf.Max(0f, maxLead));
        }

        /// <summary>Lift (0..1) along a swing at progress <paramref name="t"/>: up quickly, settles onto the ground.</summary>
        public static float SwingLift(float t)
        {
            t = Mathf.Clamp01(t);
            return Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.8f));
        }

        /// <summary>Foot position during a swing: eased travel from <paramref name="from"/> to <paramref name="to"/> plus the lift arc.</summary>
        public static Vector3 SwingPosition(Vector3 from, Vector3 to, float t, float height)
        {
            float travel = SmoothStep01(t);
            Vector3 p = Vector3.LerpUnclamped(from, to, travel);
            p.y += height * SwingLift(t);
            return p;
        }

        /// <summary>
        /// Analytic two-bone IK: places the joint so that |joint − root| = <paramref name="upper"/> and
        /// |end − joint| = <paramref name="lower"/> with the end on the root → target line, as close to the
        /// target as the chain allows, bending toward <paramref name="pole"/> (a direction). Returns true
        /// when the target is reachable (end == target).
        /// </summary>
        public static bool SolveTwoBone(Vector3 root, Vector3 target, float upper, float lower, Vector3 pole, out Vector3 joint, out Vector3 end)
        {
            upper = Mathf.Max(1e-4f, upper);
            lower = Mathf.Max(1e-4f, lower);
            Vector3 toTarget = target - root;
            float distance = toTarget.magnitude;
            Vector3 dir = distance > 1e-6f ? toTarget / distance : Vector3.down;

            float maxReach = upper + lower;
            float minReach = Mathf.Abs(upper - lower);
            bool reachable = distance <= maxReach && distance >= minReach;
            float d = Mathf.Clamp(distance, minReach + 1e-5f, Mathf.Max(minReach + 1e-5f, maxReach - 1e-5f));

            float cosA = Mathf.Clamp((upper * upper + d * d - lower * lower) / (2f * upper * d), -1f, 1f);
            float sinA = Mathf.Sqrt(Mathf.Max(0f, 1f - cosA * cosA));

            Vector3 bend = pole - dir * Vector3.Dot(pole, dir);
            if (bend.sqrMagnitude < 1e-10f)
            {
                bend = Vector3.Cross(dir, Mathf.Abs(dir.x) < 0.9f ? Vector3.right : Vector3.forward);
            }

            bend.Normalize();
            joint = root + dir * (upper * cosA) + bend * (upper * sinA);
            end = root + dir * d;
            return reachable;
        }

        /// <summary>Horizontal (xz) distance between two points.</summary>
        public static float PlanarDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x, dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
