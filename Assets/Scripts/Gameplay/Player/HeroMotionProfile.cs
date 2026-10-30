using System;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Per-hero "how this body moves": weight/inertia, acceleration and braking, turn rates, idle
    /// rhythm, attack anticipation/recovery, jump squash and the springiness of the secondary
    /// overlap. Read by <see cref="PlayerMotor"/> (gameplay feel), <see cref="PlayerProceduralAnimator"/>
    /// (the additive layers) and <see cref="PlayerFootIK"/>. Code defaults per hero come from
    /// <see cref="For"/>; an optional <see cref="HeroMotionProfileAsset"/> on the
    /// <see cref="PlayerVisualEntry"/> overrides them, and the live copy on the procedural animator is
    /// editable in the Inspector during Play Mode. Numbers are documented in <c>Docs/Animation.md</c>.
    /// </summary>
    [Serializable]
    public class HeroMotionProfile
    {
        [Tooltip("Free text, shown in the Inspector.")]
        public string label = "Default";

        [Header("Weight")]
        [Tooltip("1 = average. Heavier bodies dip deeper on landings, lag more in the overlap and flinch less.")]
        [Range(0.5f, 2f)] public float weight = 1f;

        [Header("Locomotion (PlayerMotor)")]
        [Tooltip("Seconds standing → run speed.")]
        [Min(0.01f)] public float accelerationTime = 0.16f;
        [Tooltip("Seconds run speed → stop.")]
        [Min(0.01f)] public float decelerationTime = 0.12f;
        [Tooltip("Acceleration ramp: multiplier by current/target speed (x 0..1). >1 early = strong first push; <1 late = eases into top speed.")]
        public AnimationCurve accelerationCurve = new AnimationCurve(new Keyframe(0f, 1.6f, 0f, -1.2f), new Keyframe(1f, 0.55f, -0.6f, 0f));
        [Tooltip("How fast (deg/s) the movement direction bends.")]
        public float directionTurnSpeed = 900f;
        [Tooltip("Facing turn rate (deg/s) at full speed / standing.")]
        public float turnSpeedMoving = 720f;
        public float turnSpeedStopped = 1000f;
        [Tooltip("Direction changes sharper than this (deg) brake before pivoting.")]
        [Range(60f, 180f)] public float turnBrakeAngle = 110f;
        [Tooltip("Fraction of speed kept at the sharpest braked turn.")]
        [Range(0f, 1f)] public float turnBrakeSpeedKept = 0.6f;

        [Header("Body lean / bob")]
        [Tooltip("Forward pitch (deg) by forward acceleration (x = m/s², sign kept).")]
        public AnimationCurve leanByAcceleration = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(8f, 5f), new Keyframe(30f, 12f));
        [Tooltip("Multiplier on the roll into turns.")]
        public float turnLeanGain = 0.8f;
        [Tooltip("Extra pelvis bob (m) per step on top of the clip (humanoid with foot IK; rigid bodies use it as their whole bob).")]
        [Range(0f, 0.08f)] public float bobAmplitude = 0.025f;
        [Tooltip("Torso counter-rotation (deg) against the legs at full stride.")]
        public float torsoCounterTwist = 5f;
        [Tooltip("Extra arm swing (deg) opposite the legs at full stride.")]
        public float armSwingDegrees = 8f;
        [Tooltip("Stride of the walk/run clips this hero takes (PlayerAnimatorSpeedDriver): < 1 = shorter, quicker steps for the same ground speed, " +
                 "but the planted foot slides by (1 − value). 1 = exact stride matching. The child cadence itself comes from the clip (Mocap_WalkKid, ~200 steps/min).")]
        [Range(0.8f, 1.2f)] public float strideScale = 1f;

        [Header("Idle (never fully still)")]
        [Tooltip("Breaths per second.")]
        public float breathRate = 0.28f;
        public float breathDegrees = 1.6f;
        [Tooltip("Weight shift foot to foot: hip roll (deg) and lateral hip shift (m), one full left-right cycle every period (s).")]
        public float weightShiftDegrees = 1.2f;
        public float weightShiftHip = 0.015f;
        [Min(0.5f)] public float weightShiftPeriod = 5f;
        [Tooltip("Micro head/shoulder motion (deg, noise).")]
        public float fidgetDegrees = 1f;
        [Tooltip("Noise speed of the micro motion (higher = restless).")]
        public float fidgetRate = 0.6f;
        [Tooltip("Average seconds between idle glances; 0 = never.")]
        public float glanceInterval = 5f;
        public float glanceYaw = 35f;
        public float glanceHold = 0.9f;

        [Header("Attack (anticipation → impact → recovery)")]
        [Tooltip("Arm swing (0..1 of attackArmDegrees) over the attack axis: 0..0.5 anticipation ending exactly on the damage frame (StrikeDelay), 0.5..1 follow-through + recovery.")]
        public AnimationCurve attackArmCurve = MotionFeelMath.DefaultAttackArmCurve();
        [Tooltip("Seconds of follow-through + recovery after the impact.")]
        [Min(0.05f)] public float attackRecoverTime = 0.28f;
        public float attackArmDegrees = 110f;
        public float attackTwistDegrees = 22f;
        [Tooltip("Whole-body lunge (m) at the impact (rig mode uses 40 %).")]
        public float attackLunge = 0.3f;

        [Header("Jump / landing")]
        [Tooltip("Visual-only crouch on takeoff (squash fraction). The jump itself is never delayed.")]
        [Range(0f, 0.06f)] public float takeoffSquash = 0.04f;
        [Tooltip("Max squash fraction on a hard landing.")]
        [Range(0f, 0.06f)] public float landingSquash = 0.06f;
        [Tooltip("Landing squash (0..1 of the max) by fall speed (x = m/s).")]
        public AnimationCurve landingSquashByFallSpeed = new AnimationCurve(new Keyframe(2f, 0f), new Keyframe(5f, 0.55f), new Keyframe(10f, 1f));
        [Tooltip("Pelvis dip (m) on a hard landing (knees absorb through foot IK).")]
        [Range(0f, 0.25f)] public float landingDip = 0.1f;
        [Tooltip("Knee tuck (m) of the feet in the air (foot IK).")]
        [Range(0f, 0.3f)] public float airTuck = 0.12f;

        [Header("Secondary overlap (springs)")]
        [Tooltip("Natural frequency (Hz) of the head/arm/extra-bone springs: low = loose and heavy, high = tight.")]
        [Min(0.2f)] public float springFrequency = 2.8f;
        [Tooltip("Damping ratio: < 1 overshoots (follow-through), 1 = no overshoot.")]
        [Range(0.1f, 1.5f)] public float springDamping = 0.55f;
        [Tooltip("Degrees of lag per m/s² of body acceleration.")]
        public float overlapGain = 1f;
        [Tooltip("Procedural overlap layer on/off for this hero: head/spine/arm spring lag, turn follow-through and the extra hip bob. Off = only the clip, lean and foot IK.")]
        public bool proceduralOverlap = true;
        [Tooltip("Turn follow-through: share (0..1) of the physical yaw lag of head/neck/upper spine when a turn starts or stops (they trail, then overshoot).")]
        [Range(0f, 1f)] public float turnFollowThrough = 0.35f;
        [Tooltip("Largest yaw lag (deg) of the head on a turn.")]
        [Range(0f, 20f)] public float turnLagMaxDegrees = 8f;

        [Header("Hit")]
        [Tooltip("Multiplier on the flinch/impact reaction.")]
        public float flinchScale = 1f;

        /// <summary>Deep copy (curves included) so Inspector tweaks on a live profile never edit the source asset.</summary>
        public HeroMotionProfile Clone()
        {
            var copy = (HeroMotionProfile)MemberwiseClone();
            copy.accelerationCurve = CopyCurve(accelerationCurve);
            copy.leanByAcceleration = CopyCurve(leanByAcceleration);
            copy.attackArmCurve = CopyCurve(attackArmCurve);
            copy.landingSquashByFallSpeed = CopyCurve(landingSquashByFallSpeed);
            return copy;
        }

        private static AnimationCurve CopyCurve(AnimationCurve curve)
        {
            return curve != null ? new AnimationCurve(curve.keys) { preWrapMode = curve.preWrapMode, postWrapMode = curve.postWrapMode } : null;
        }

        /// <summary>Neutral profile (the values the motor/animator used before profiles existed).</summary>
        public static HeroMotionProfile Default => new HeroMotionProfile();

        /// <summary>
        /// Code defaults per hero (personality). Joaquim heavy and grounded, Maria calm and smooth,
        /// Maithe light and quick (archer), Ícaro fidgety, Lavine restless/nervous (youngest).
        /// </summary>
        public static HeroMotionProfile For(CharacterId id)
        {
            var p = new HeroMotionProfile();
            switch (id)
            {
                case CharacterId.Joaquim:
                    p.label = "Joaquim — pesado, firme";
                    p.weight = 1.35f;
                    p.accelerationTime = 0.24f;
                    p.decelerationTime = 0.18f;
                    p.accelerationCurve = Ramp(1.25f, 0.6f);
                    p.directionTurnSpeed = 620f;
                    p.turnSpeedMoving = 520f;
                    p.turnSpeedStopped = 760f;
                    p.turnBrakeAngle = 100f;
                    p.turnBrakeSpeedKept = 0.45f;
                    p.leanByAcceleration = Lean(9f);
                    p.turnLeanGain = 0.6f;
                    p.bobAmplitude = 0.035f;
                    p.torsoCounterTwist = 4f;
                    p.armSwingDegrees = 6f;
                    p.breathRate = 0.22f;
                    p.breathDegrees = 2f;
                    p.weightShiftDegrees = 1.6f;
                    p.weightShiftHip = 0.02f;
                    p.weightShiftPeriod = 6.5f;
                    p.fidgetDegrees = 0.8f;
                    p.fidgetRate = 0.4f;
                    p.glanceInterval = 7f;
                    p.glanceYaw = 25f;
                    p.glanceHold = 1.2f;
                    p.attackRecoverTime = 0.34f;
                    p.attackArmDegrees = 105f;
                    p.attackTwistDegrees = 26f;
                    p.attackLunge = 0.25f;
                    p.takeoffSquash = 0.05f;
                    p.landingSquash = 0.06f;
                    p.landingDip = 0.14f;
                    p.airTuck = 0.1f;
                    p.springFrequency = 2.2f;
                    p.springDamping = 0.55f;
                    p.overlapGain = 1.4f;
                    p.turnFollowThrough = 0.45f;
                    p.turnLagMaxDegrees = 10f;
                    p.flinchScale = 0.75f;
                    break;

                case CharacterId.Maria:
                    p.label = "Maria — calma, suave";
                    p.weight = 1f;
                    p.accelerationTime = 0.18f;
                    p.decelerationTime = 0.14f;
                    p.accelerationCurve = Ramp(1.4f, 0.6f);
                    p.directionTurnSpeed = 760f;
                    p.turnSpeedMoving = 640f;
                    p.turnSpeedStopped = 900f;
                    p.turnBrakeAngle = 105f;
                    p.turnBrakeSpeedKept = 0.55f;
                    p.leanByAcceleration = Lean(9f);
                    p.turnLeanGain = 0.7f;
                    p.bobAmplitude = 0.022f;
                    p.torsoCounterTwist = 4f;
                    p.armSwingDegrees = 5f;
                    p.breathRate = 0.24f;
                    p.breathDegrees = 1.4f;
                    p.weightShiftDegrees = 1f;
                    p.weightShiftHip = 0.012f;
                    p.weightShiftPeriod = 7.5f;
                    p.fidgetDegrees = 0.6f;
                    p.fidgetRate = 0.35f;
                    p.glanceInterval = 6f;
                    p.glanceYaw = 30f;
                    p.glanceHold = 1.3f;
                    p.attackRecoverTime = 0.3f;
                    p.attackArmDegrees = 95f;
                    p.attackTwistDegrees = 18f;
                    p.attackLunge = 0.2f;
                    p.takeoffSquash = 0.035f;
                    p.landingSquash = 0.045f;
                    p.landingDip = 0.09f;
                    p.airTuck = 0.12f;
                    p.springFrequency = 2.6f;
                    p.springDamping = 0.75f;
                    p.overlapGain = 0.9f;
                    p.turnFollowThrough = 0.3f;
                    p.turnLagMaxDegrees = 7f;
                    p.flinchScale = 1f;
                    break;

                case CharacterId.Maithe:
                    p.label = "Maithe — leve, ágil";
                    p.weight = 0.75f;
                    p.accelerationTime = 0.12f;
                    p.decelerationTime = 0.09f;
                    p.accelerationCurve = Ramp(1.9f, 0.5f);
                    p.directionTurnSpeed = 1080f;
                    p.turnSpeedMoving = 840f;
                    p.turnSpeedStopped = 1150f;
                    p.turnBrakeAngle = 120f;
                    p.turnBrakeSpeedKept = 0.75f;
                    p.leanByAcceleration = Lean(13f);
                    p.turnLeanGain = 0.95f;
                    p.bobAmplitude = 0.03f;
                    p.torsoCounterTwist = 6f;
                    p.armSwingDegrees = 8f;
                    p.breathRate = 0.3f;
                    p.breathDegrees = 1.4f;
                    p.weightShiftDegrees = 1f;
                    p.weightShiftHip = 0.012f;
                    p.weightShiftPeriod = 4.5f;
                    p.fidgetDegrees = 1f;
                    p.fidgetRate = 0.6f;
                    p.glanceInterval = 4f;
                    p.glanceYaw = 40f;
                    p.glanceHold = 0.8f;
                    p.attackRecoverTime = 0.22f;
                    p.attackArmDegrees = 110f;
                    p.attackTwistDegrees = 20f;
                    p.attackLunge = 0.32f;
                    p.takeoffSquash = 0.04f;
                    p.landingSquash = 0.05f;
                    p.landingDip = 0.07f;
                    p.airTuck = 0.16f;
                    p.springFrequency = 3.4f;
                    p.springDamping = 0.5f;
                    p.overlapGain = 1f;
                    p.turnFollowThrough = 0.25f;
                    p.turnLagMaxDegrees = 6f;
                    p.flinchScale = 1.2f;
                    break;

                case CharacterId.Icaro:
                    p.label = "Ícaro — inquieto, elétrico";
                    p.weight = 0.9f;
                    p.accelerationTime = 0.14f;
                    p.decelerationTime = 0.11f;
                    p.accelerationCurve = Ramp(1.7f, 0.55f);
                    p.directionTurnSpeed = 960f;
                    p.turnSpeedMoving = 760f;
                    p.turnSpeedStopped = 1080f;
                    p.turnBrakeAngle = 110f;
                    p.turnBrakeSpeedKept = 0.65f;
                    p.leanByAcceleration = Lean(11f);
                    p.turnLeanGain = 0.85f;
                    p.bobAmplitude = 0.028f;
                    p.torsoCounterTwist = 5f;
                    p.armSwingDegrees = 9f;
                    p.breathRate = 0.3f;
                    p.breathDegrees = 1.6f;
                    p.weightShiftDegrees = 1.4f;
                    p.weightShiftHip = 0.016f;
                    p.weightShiftPeriod = 4f;
                    p.fidgetDegrees = 1.8f;
                    p.fidgetRate = 0.9f;
                    p.glanceInterval = 3.5f;
                    p.glanceYaw = 45f;
                    p.glanceHold = 0.6f;
                    p.attackRecoverTime = 0.26f;
                    p.attackArmDegrees = 115f;
                    p.attackTwistDegrees = 22f;
                    p.attackLunge = 0.28f;
                    p.takeoffSquash = 0.04f;
                    p.landingSquash = 0.055f;
                    p.landingDip = 0.09f;
                    p.airTuck = 0.14f;
                    p.springFrequency = 3.2f;
                    p.springDamping = 0.45f;
                    p.overlapGain = 1.2f;
                    p.turnFollowThrough = 0.35f;
                    p.turnLagMaxDegrees = 8f;
                    p.flinchScale = 1.1f;
                    break;

                case CharacterId.Lavine:
                    p.label = "Lavine — nervosa, a mais nova";
                    p.weight = 0.8f;
                    p.accelerationTime = 0.13f;
                    p.decelerationTime = 0.1f;
                    p.accelerationCurve = Ramp(1.8f, 0.5f);
                    p.directionTurnSpeed = 1000f;
                    p.turnSpeedMoving = 800f;
                    p.turnSpeedStopped = 1100f;
                    p.turnBrakeAngle = 110f;
                    p.turnBrakeSpeedKept = 0.65f;
                    p.leanByAcceleration = Lean(12f);
                    p.turnLeanGain = 0.9f;
                    p.bobAmplitude = 0.03f;
                    p.torsoCounterTwist = 5f;
                    p.armSwingDegrees = 10f;
                    p.breathRate = 0.36f;
                    p.breathDegrees = 1.8f;
                    p.weightShiftDegrees = 1.8f;
                    p.weightShiftHip = 0.018f;
                    p.weightShiftPeriod = 3.2f;
                    p.fidgetDegrees = 2.2f;
                    p.fidgetRate = 1.1f;
                    p.glanceInterval = 2.8f;
                    p.glanceYaw = 50f;
                    p.glanceHold = 0.5f;
                    p.attackRecoverTime = 0.24f;
                    p.attackArmDegrees = 120f;
                    p.attackTwistDegrees = 24f;
                    p.attackLunge = 0.26f;
                    p.takeoffSquash = 0.045f;
                    p.landingSquash = 0.055f;
                    p.landingDip = 0.08f;
                    p.airTuck = 0.16f;
                    p.springFrequency = 3f;
                    p.springDamping = 0.4f;
                    p.overlapGain = 1.3f;
                    p.turnFollowThrough = 0.4f;
                    p.turnLagMaxDegrees = 9f;
                    p.flinchScale = 1.3f;
                    break;
            }

            return p;
        }

        private static AnimationCurve Ramp(float start, float end)
        {
            return new AnimationCurve(new Keyframe(0f, start, 0f, -(start - end) * 1.1f), new Keyframe(1f, end, -(start - end) * 0.5f, 0f));
        }

        private static AnimationCurve Lean(float maxDegrees)
        {
            return new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(8f, maxDegrees * 0.45f), new Keyframe(30f, maxDegrees));
        }
    }
}
