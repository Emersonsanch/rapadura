using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Body plan used to pick an enemy's movement feel (acceleration/turning in
    /// <see cref="EnemyController"/>) and its procedural gait/poses (<see cref="EnemyProceduralAnimator"/>).
    /// Serialized as int on <see cref="EnemyDefinition"/> — append only.
    /// </summary>
    public enum EnemyMotionArchetype
    {
        /// <summary>Not authored: resolved from the asset/enemy name (see <see cref="EnemyMotionArchetypes.DefaultFor"/>).</summary>
        Auto = 0,
        /// <summary>Four-legged runner (wolf): trot → gallop, pitch rock, stretch/squash.</summary>
        Quadruped = 1,
        /// <summary>Many-legged, low and fast (spider, scorpion): high-frequency skitter, yaw wiggle.</summary>
        Arthropod = 2,
        /// <summary>Hovering flyer (bats): sine hover, wing flutter roll, banks into turns.</summary>
        Flyer = 3,
        /// <summary>Big and slow (golem, guardian, warlord, cave dweller): heavy stomping steps.</summary>
        Heavy = 4,
    }

    /// <summary>Resolves <see cref="EnemyMotionArchetype.Auto"/> to a concrete archetype.</summary>
    public static class EnemyMotionArchetypes
    {
        /// <summary>The definition's authored archetype, or the name-based default when it is Auto. Null → Quadruped.</summary>
        public static EnemyMotionArchetype Resolve(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return EnemyMotionArchetype.Quadruped;
            }

            EnemyMotionArchetype authored = definition.AuthoredMotionArchetype;
            return authored != EnemyMotionArchetype.Auto
                ? authored
                : DefaultFor(definition.name, definition.EnemyName, definition.IsBoss);
        }

        /// <summary>
        /// Name-based default (asset id first, then display name; Portuguese and English keywords).
        /// Unknown names: bosses are Heavy, everything else Quadruped.
        /// </summary>
        public static EnemyMotionArchetype DefaultFor(string assetName, string displayName, bool isBoss)
        {
            EnemyMotionArchetype found = FromKeywords(assetName);
            if (found == EnemyMotionArchetype.Auto)
            {
                found = FromKeywords(displayName);
            }

            if (found != EnemyMotionArchetype.Auto)
            {
                return found;
            }

            return isBoss ? EnemyMotionArchetype.Heavy : EnemyMotionArchetype.Quadruped;
        }

        private static EnemyMotionArchetype FromKeywords(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return EnemyMotionArchetype.Auto;
            }

            string s = value.ToLowerInvariant();
            if (s.Contains("spider") || s.Contains("aranha") || s.Contains("scorpion") || s.Contains("escorpi"))
            {
                return EnemyMotionArchetype.Arthropod;
            }

            if (s.Contains("wolf") || s.Contains("lobo"))
            {
                return EnemyMotionArchetype.Quadruped;
            }

            if (s.Contains("_bat") || s.Contains("bat_") || s.StartsWith("bat") || s.Contains("morcego"))
            {
                return EnemyMotionArchetype.Flyer;
            }

            if (s.Contains("golem") || s.Contains("guardian") || s.Contains("guardi") || s.Contains("warlord")
                || s.Contains("senhor da guerra") || s.Contains("dweller") || s.Contains("habitante"))
            {
                return EnemyMotionArchetype.Heavy;
            }

            return EnemyMotionArchetype.Auto;
        }
    }

    /// <summary>
    /// Per-archetype tuning for movement (controller) and procedural animation (animator). Plain data;
    /// one shared instance per archetype via <see cref="For"/>. Distances are metres for a body of
    /// <see cref="ReferenceHeight"/>; the animator scales gait distances by the visual's real height and
    /// the feel (springs, eases, kicks) by <see cref="ScaledForBody"/>. The gameplay fields (movement
    /// rates and <see cref="BaseWindup"/>) are never scaled: <see cref="EnemyController.Motion"/> always
    /// hands out the archetype's own values.
    /// </summary>
    public struct EnemyMotionProfile
    {
        public EnemyMotionArchetype Archetype;

        // --- Locomotion (EnemyController) ---
        /// <summary>m/s² used while speeding up / steering.</summary>
        public float Acceleration;
        /// <summary>m/s² used while slowing down (also the coast when no state is moving the enemy).</summary>
        public float Deceleration;
        /// <summary>SmoothDampAngle time for heading changes (s).</summary>
        public float TurnSmoothTime;
        /// <summary>Max angular speed (deg/s).</summary>
        public float MaxTurnSpeed;

        // --- Gait (EnemyProceduralAnimator) ---
        /// <summary>Metres covered per full gait cycle at walk speed.</summary>
        public float StrideLength;
        public float MinStrideHz;
        public float MaxStrideHz;
        /// <summary>Gait cycles/s when standing still (flyers keep flapping; 0 for ground walkers).</summary>
        public float IdleStrideHz;
        /// <summary>Speed (m/s) at which the gait reaches full intensity.</summary>
        public float FullGaitSpeed;
        /// <summary>Quadruped trot → gallop blend speeds (m/s).</summary>
        public float GallopStartSpeed;
        public float GallopFullSpeed;

        public float BobAmplitude;
        public float GallopBobAmplitude;
        public float PitchAmplitude;
        public float GallopPitchAmplitude;
        public float RollAmplitude;
        public float YawAmplitude;
        public float SquashAmplitude;
        /// <summary>Crouch (m, negative = lower) while moving at full gait — arthropods hug the ground.</summary>
        public float MoveCrouch;
        /// <summary>Visual hover height above the collider's feet (flyers only).</summary>
        public float HoverHeight;
        public float BankDegrees;
        public float ReferenceHeight;

        // --- Attack (EnemyAttackState timing + animator pose) ---
        /// <summary>Wanted windup (telegraph) before the strike; clamped by <see cref="EnemyMotionMath.ResolveWindup"/>. Gameplay: never scaled.</summary>
        public float BaseWindup;
        /// <summary>Lunge travel time, taken from the end of the windup (visual only); the lunge peaks exactly at the damage moment.</summary>
        public float StrikeDuration;
        public float RecoverDuration;
        public float WindupLeanDegrees;
        public float StrikeLeanDegrees;
        public float WindupPullBack;
        /// <summary>Vertical offset at full coil: negative = crouch (ground bodies), positive = rise (flyers).</summary>
        public float WindupRise;
        public float LungeDistance;
        /// <summary>Vertical offset at lunge peak (flyers dip toward the target: negative).</summary>
        public float StrikeDrop;
        /// <summary>Squash (fraction, ≤ <see cref="MotionFeelMath.MaxSquash"/>) at full coil: the anticipation crouch.</summary>
        public float AnticipationSquash;
        /// <summary>Stretch (fraction) at the lunge peak.</summary>
        public float StrikeStretch;
        /// <summary>Follow-through: how far (fraction of the lunge) the body carries on past the peak before recovering.</summary>
        public float FollowThrough;

        // --- Reactions ---
        /// <summary>Peak shove (m) of the hit spring kick at full strength.</summary>
        public float RecoilDistance;
        /// <summary>Peak tilt (deg) of the hit spring kick at full strength.</summary>
        public float RecoilDegrees;
        public float AlertHop;
        public float AlertRearDegrees;
        /// <summary>Height (m at reference size) of the first bounce when the corpse hits the ground.</summary>
        public float DeathBounce;
        /// <summary>Ease-out-back constant of the spawn rise (0 = no overshoot, 1.7 = classic back-out).</summary>
        public float SpawnOvershoot;

        // --- Feel (animator only; scaled per body by ScaledForBody) ---
        /// <summary>Relative body mass (1 = reference wolf); multiplied by size³ in <see cref="ScaledForBody"/>.</summary>
        public float Mass;
        /// <summary>Time (s) the visual gait takes to ease into / out of a speed change (start, stop).</summary>
        public float GaitEaseTime;
        /// <summary>Body lean / surge spring (follows acceleration and turns, overshoots on stops).</summary>
        public float LeanSpringHz;
        public float LeanSpringDamping;
        /// <summary>Lean (deg) per m/s² of acceleration, clamped to <see cref="MaxAccelLean"/>.</summary>
        public float LeanPerAcceleration;
        public float MaxAccelLean;
        /// <summary>Body lag/surge (m) per m/s² of acceleration: trails the collider on starts, carries on past it on stops.</summary>
        public float SurgePerAcceleration;
        public float MaxSurge;
        /// <summary>Rear-back pitch (deg) while braking into a sharp turn.</summary>
        public float TurnBrakeLean;
        /// <summary>Squash wobble spring (kicked by hits, impacts, landings).</summary>
        public float SquashSpringHz;
        public float SquashSpringDamping;
        /// <summary>Hit spring (kick away from the attacker, overshoots back).</summary>
        public float HitSpringHz;
        public float HitSpringDamping;

        // --- Idle personality ---
        public float BreathHz;
        public float BreathSquash;
        /// <summary>Look-around glances (<see cref="MotionFeelMath.IdleGlanceYaw"/>): interval (s), max yaw (deg), hold (s).</summary>
        public float IdleLookInterval;
        public float IdleLookDegrees;
        public float IdleLookHold;
        /// <summary>Archetype fidget (deg): wolf sniff dip, arthropod (leg) twitch, bat head tilt, heavy shoulder roll.</summary>
        public float IdleFidgetDegrees;

        public static readonly EnemyMotionProfile Quadruped = new EnemyMotionProfile
        {
            Archetype = EnemyMotionArchetype.Quadruped,
            Acceleration = 16f, Deceleration = 20f, TurnSmoothTime = 0.1f, MaxTurnSpeed = 480f,
            StrideLength = 1.1f, MinStrideHz = 1.3f, MaxStrideHz = 3.2f, IdleStrideHz = 0f, FullGaitSpeed = 4.5f,
            GallopStartSpeed = 2.6f, GallopFullSpeed = 3.9f,
            BobAmplitude = 0.05f, GallopBobAmplitude = 0.13f, PitchAmplitude = 2.5f, GallopPitchAmplitude = 9f,
            RollAmplitude = 2.5f, YawAmplitude = 2f, SquashAmplitude = 0.045f, MoveCrouch = 0f,
            HoverHeight = 0f, BankDegrees = 10f, ReferenceHeight = 0.9f,
            BaseWindup = 0.42f, StrikeDuration = 0.12f, RecoverDuration = 0.3f,
            WindupLeanDegrees = 12f, StrikeLeanDegrees = 10f, WindupPullBack = 0.15f, WindupRise = -0.05f,
            LungeDistance = 0.5f, StrikeDrop = 0f, AnticipationSquash = 0.05f, StrikeStretch = 0.05f, FollowThrough = 0.15f,
            RecoilDistance = 0.22f, RecoilDegrees = 14f, AlertHop = 0.18f, AlertRearDegrees = 14f,
            DeathBounce = 0.08f, SpawnOvershoot = 1.7f,
            Mass = 1f, GaitEaseTime = 0.18f, LeanSpringHz = 2.6f, LeanSpringDamping = 0.45f,
            LeanPerAcceleration = 1.2f, MaxAccelLean = 7f, SurgePerAcceleration = 0.01f, MaxSurge = 0.06f, TurnBrakeLean = 8f,
            SquashSpringHz = 4f, SquashSpringDamping = 0.35f, HitSpringHz = 3.2f, HitSpringDamping = 0.38f,
            BreathHz = 0.45f, BreathSquash = 0.02f, IdleLookInterval = 4.5f, IdleLookDegrees = 22f, IdleLookHold = 0.9f, IdleFidgetDegrees = 6f,
        };

        public static readonly EnemyMotionProfile Arthropod = new EnemyMotionProfile
        {
            Archetype = EnemyMotionArchetype.Arthropod,
            Acceleration = 26f, Deceleration = 30f, TurnSmoothTime = 0.06f, MaxTurnSpeed = 600f,
            StrideLength = 0.45f, MinStrideHz = 3f, MaxStrideHz = 9f, IdleStrideHz = 0f, FullGaitSpeed = 4f,
            GallopStartSpeed = 0f, GallopFullSpeed = 0f,
            BobAmplitude = 0.018f, GallopBobAmplitude = 0f, PitchAmplitude = 1.5f, GallopPitchAmplitude = 0f,
            RollAmplitude = 1.5f, YawAmplitude = 6f, SquashAmplitude = 0.03f, MoveCrouch = -0.035f,
            HoverHeight = 0f, BankDegrees = 4f, ReferenceHeight = 0.5f,
            BaseWindup = 0.4f, StrikeDuration = 0.1f, RecoverDuration = 0.25f,
            WindupLeanDegrees = 18f, StrikeLeanDegrees = 12f, WindupPullBack = 0.1f, WindupRise = -0.03f,
            LungeDistance = 0.42f, StrikeDrop = 0f, AnticipationSquash = 0.04f, StrikeStretch = 0.04f, FollowThrough = 0.1f,
            RecoilDistance = 0.2f, RecoilDegrees = 12f, AlertHop = 0.12f, AlertRearDegrees = 18f,
            DeathBounce = 0.06f, SpawnOvershoot = 2f,
            Mass = 0.4f, GaitEaseTime = 0.08f, LeanSpringHz = 4.5f, LeanSpringDamping = 0.6f,
            LeanPerAcceleration = 0.8f, MaxAccelLean = 5f, SurgePerAcceleration = 0.005f, MaxSurge = 0.03f, TurnBrakeLean = 4f,
            SquashSpringHz = 6f, SquashSpringDamping = 0.4f, HitSpringHz = 4.5f, HitSpringDamping = 0.42f,
            BreathHz = 0.6f, BreathSquash = 0.012f, IdleLookInterval = 3f, IdleLookDegrees = 10f, IdleLookHold = 0.25f, IdleFidgetDegrees = 7f,
        };

        public static readonly EnemyMotionProfile Flyer = new EnemyMotionProfile
        {
            Archetype = EnemyMotionArchetype.Flyer,
            Acceleration = 10f, Deceleration = 8f, TurnSmoothTime = 0.2f, MaxTurnSpeed = 300f,
            StrideLength = 2.5f, MinStrideHz = 1.6f, MaxStrideHz = 3f, IdleStrideHz = 1.6f, FullGaitSpeed = 5f,
            GallopStartSpeed = 0f, GallopFullSpeed = 0f,
            BobAmplitude = 0.12f, GallopBobAmplitude = 0f, PitchAmplitude = 10f, GallopPitchAmplitude = 0f,
            RollAmplitude = 7f, YawAmplitude = 3f, SquashAmplitude = 0.035f, MoveCrouch = 0f,
            HoverHeight = 0.45f, BankDegrees = 25f, ReferenceHeight = 0.5f,
            BaseWindup = 0.36f, StrikeDuration = 0.12f, RecoverDuration = 0.3f,
            WindupLeanDegrees = 15f, StrikeLeanDegrees = 25f, WindupPullBack = 0.2f, WindupRise = 0.3f,
            LungeDistance = 0.6f, StrikeDrop = -0.45f, AnticipationSquash = 0.035f, StrikeStretch = 0.05f, FollowThrough = 0.2f,
            RecoilDistance = 0.3f, RecoilDegrees = 20f, AlertHop = 0.3f, AlertRearDegrees = 10f,
            DeathBounce = 0.12f, SpawnOvershoot = 1.4f,
            Mass = 0.25f, GaitEaseTime = 0.3f, LeanSpringHz = 1.6f, LeanSpringDamping = 0.35f,
            LeanPerAcceleration = 2f, MaxAccelLean = 14f, SurgePerAcceleration = 0.02f, MaxSurge = 0.1f, TurnBrakeLean = 0f,
            SquashSpringHz = 3.5f, SquashSpringDamping = 0.3f, HitSpringHz = 2.6f, HitSpringDamping = 0.3f,
            BreathHz = 0.3f, BreathSquash = 0.015f, IdleLookInterval = 5f, IdleLookDegrees = 12f, IdleLookHold = 0.6f, IdleFidgetDegrees = 5f,
        };

        public static readonly EnemyMotionProfile Heavy = new EnemyMotionProfile
        {
            Archetype = EnemyMotionArchetype.Heavy,
            Acceleration = 6f, Deceleration = 10f, TurnSmoothTime = 0.25f, MaxTurnSpeed = 140f,
            StrideLength = 1.6f, MinStrideHz = 0.6f, MaxStrideHz = 1.6f, IdleStrideHz = 0f, FullGaitSpeed = 3f,
            GallopStartSpeed = 0f, GallopFullSpeed = 0f,
            BobAmplitude = 0.05f, GallopBobAmplitude = 0f, PitchAmplitude = 3f, GallopPitchAmplitude = 0f,
            RollAmplitude = 4f, YawAmplitude = 2f, SquashAmplitude = 0.05f, MoveCrouch = 0f,
            HoverHeight = 0f, BankDegrees = 3f, ReferenceHeight = 2f,
            BaseWindup = 0.65f, StrikeDuration = 0.16f, RecoverDuration = 0.45f,
            WindupLeanDegrees = 14f, StrikeLeanDegrees = 16f, WindupPullBack = 0.12f, WindupRise = -0.06f,
            LungeDistance = 0.35f, StrikeDrop = -0.05f, AnticipationSquash = 0.05f, StrikeStretch = 0.035f, FollowThrough = 0.08f,
            RecoilDistance = 0.08f, RecoilDegrees = 6f, AlertHop = 0.06f, AlertRearDegrees = 8f,
            DeathBounce = 0.05f, SpawnOvershoot = 1.1f,
            Mass = 4f, GaitEaseTime = 0.4f, LeanSpringHz = 1.3f, LeanSpringDamping = 0.6f,
            LeanPerAcceleration = 1f, MaxAccelLean = 5f, SurgePerAcceleration = 0.015f, MaxSurge = 0.08f, TurnBrakeLean = 5f,
            SquashSpringHz = 2.4f, SquashSpringDamping = 0.45f, HitSpringHz = 2.2f, HitSpringDamping = 0.5f,
            BreathHz = 0.25f, BreathSquash = 0.03f, IdleLookInterval = 7f, IdleLookDegrees = 15f, IdleLookHold = 1.4f, IdleFidgetDegrees = 1.5f,
        };

        public static EnemyMotionProfile For(EnemyMotionArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyMotionArchetype.Arthropod: return Arthropod;
                case EnemyMotionArchetype.Flyer: return Flyer;
                case EnemyMotionArchetype.Heavy: return Heavy;
                default: return Quadruped;
            }
        }

        /// <summary>
        /// Inertia factor of a body <paramref name="sizeScale"/> times the archetype's reference height
        /// (clamped 0.5..2): √size, ×1.15 for bosses. A pendulum's period grows with √length, so springs
        /// slow down and eases lengthen by this; kicks shrink by it.
        /// </summary>
        public static float InertiaFor(float sizeScale, bool isBoss = false)
        {
            return Mathf.Sqrt(Mathf.Clamp(sizeScale, 0.5f, 2f)) * (isBoss ? 1.15f : 1f);
        }

        /// <summary>
        /// Copy with the visual feel scaled for the real body: bigger (or a boss) = more mass, slower
        /// springs and eases, longer strike travel and recovery, smaller shoves and less spawn overshoot.
        /// Never touches the gameplay fields (movement rates, <see cref="BaseWindup"/>); the strike always
        /// ends exactly at the gameplay windup (<see cref="EnemyMotionMath.EvaluateAttack"/>).
        /// </summary>
        public EnemyMotionProfile ScaledForBody(float sizeScale, bool isBoss = false)
        {
            EnemyMotionProfile p = this;
            float size = Mathf.Clamp(sizeScale, 0.5f, 2f);
            float inertia = InertiaFor(sizeScale, isBoss);
            float kick = 1f / inertia;
            float slower = Mathf.Clamp(inertia, 0.85f, 1.25f);

            p.Mass = Mass * size * size * size * (isBoss ? 1.5f : 1f);
            p.GaitEaseTime = GaitEaseTime * inertia;
            p.LeanSpringHz = LeanSpringHz * kick;
            p.SquashSpringHz = SquashSpringHz * kick;
            p.HitSpringHz = HitSpringHz * kick;
            p.StrikeDuration = StrikeDuration * slower;
            p.RecoverDuration = RecoverDuration * slower;
            p.RecoilDistance = RecoilDistance * kick;
            p.RecoilDegrees = RecoilDegrees * kick;
            p.SpawnOvershoot = SpawnOvershoot * kick;
            p.BreathHz = BreathHz * kick;
            p.IdleLookInterval = IdleLookInterval * inertia;
            p.IdleLookHold = IdleLookHold * inertia;
            return p;
        }
    }

    /// <summary>Additive local pose of an enemy visual: translation (parent/root space), Euler (pitch X, yaw Y, roll Z) and squash (+ flatten, - stretch).</summary>
    public struct EnemyPose
    {
        public Vector3 Offset;
        public Vector3 Euler;
        public float Squash;

        public void Add(in EnemyPose other)
        {
            Offset += other.Offset;
            Euler += other.Euler;
            Squash += other.Squash;
        }

        public void Scale(float k)
        {
            Offset *= k;
            Euler *= k;
            Squash *= k;
        }
    }

    /// <summary>Which part of the attack the visual is playing.</summary>
    public enum EnemyAttackVisualPhase
    {
        None = 0,
        /// <summary>Anticipation (crouch, pull back, squash, tremble) then, in its last <see cref="EnemyMotionProfile.StrikeDuration"/>, the stretched lunge, peaking at the damage moment.</summary>
        Windup = 1,
        /// <summary>After the damage moment: follow-through past the peak, then back to rest.</summary>
        Recover = 2,
    }

    /// <summary>Initial velocities a hit gives the animator's hit springs (see <see cref="EnemyMotionMath.HitKick"/>).</summary>
    public struct EnemyHitKick
    {
        /// <summary>Planar shove velocity (local X, local Z), m/s.</summary>
        public Vector2 OffsetVelocity;
        /// <summary>Tilt velocity (pitch, roll), deg/s.</summary>
        public Vector2 TiltVelocity;
        /// <summary>Squash velocity (fraction/s, + = flatten).</summary>
        public float SquashVelocity;
    }

    /// <summary>
    /// Pure, allocation-free math behind enemy movement feel and procedural animation. Static so
    /// EditMode tests can verify it without a scene; angles are degrees, phases are gait cycles.
    /// Springs and squash come from <see cref="MotionFeelMath"/> (shared with the heroes).
    /// </summary>
    public static class EnemyMotionMath
    {
        /// <summary>Shortest windup any enemy attack gets, so the player can read and react to it.</summary>
        public const float MinTelegraph = 0.35f;
        /// <summary>Time kept free after the strike inside one cooldown (visual recover).</summary>
        public const float MinRecoverWindow = 0.25f;
        public const float ToppleDuration = 0.4f;
        public const float SpawnRiseDuration = 0.45f;
        public const float AlertDuration = 0.35f;
        public const float CancelBlendDuration = 0.2f;
        /// <summary>Fraction of the coil (pre-strike part of the windup) spent gathering into the crouch; the rest holds and trembles.</summary>
        public const float AnticipationGather = 0.6f;
        /// <summary>Fraction of the recovery spent carrying on past the lunge peak (follow-through).</summary>
        public const float FollowThroughPortion = 0.2f;
        /// <summary>Death impact bounces: first hop, second hop (s) and the second's relative height.</summary>
        public const float FirstBounceDuration = 0.24f;
        public const float SecondBounceDuration = 0.14f;
        public const float SecondBounceHeight = 0.3f;

        private const float TwoPi = Mathf.PI * 2f;

        // ------------------------------------------------------------------ attack timing

        /// <summary>
        /// Windup actually used: <paramref name="baseWindup"/> clamped to at least <see cref="MinTelegraph"/>
        /// and, when possible, short enough to leave <see cref="MinRecoverWindow"/> of the cooldown, so the
        /// strike-to-strike interval stays exactly <paramref name="attackCooldown"/>.
        /// </summary>
        public static float ResolveWindup(float baseWindup, float attackCooldown)
        {
            float max = Mathf.Max(MinTelegraph, attackCooldown - MinRecoverWindow);
            return Mathf.Clamp(baseWindup, MinTelegraph, max);
        }

        /// <summary>
        /// Seconds (from the windup start) at which the lunge starts: the strike always takes the last
        /// <see cref="EnemyMotionProfile.StrikeDuration"/> (at most half) of the gameplay
        /// <paramref name="windup"/>, so the visual can never move the damage moment.
        /// </summary>
        public static float StrikeStartTime(in EnemyMotionProfile p, float windup)
        {
            float strike = Mathf.Min(Mathf.Max(0f, p.StrikeDuration), windup * 0.5f);
            return Mathf.Max(0.0001f, windup - strike);
        }

        /// <summary>
        /// Attack pose at <paramref name="t"/> seconds into <paramref name="phase"/>.
        /// Windup: anticipation (crouch or rise, pull back, lean back, squash) gathered over the first
        /// <see cref="AnticipationGather"/> of the coil, held with a tremble until
        /// <see cref="StrikeStartTime"/>, then the lunge accelerates (ease-in) into the stretched peak at
        /// exactly <paramref name="windup"/> (the damage moment) and holds there until the strike is
        /// reported. Recover: follow-through past the peak, then eased back with a slight settle.
        /// </summary>
        public static EnemyPose EvaluateAttack(in EnemyMotionProfile p, EnemyAttackVisualPhase phase, float t, float windup)
        {
            EnemyPose pose = default;
            if (phase == EnemyAttackVisualPhase.None)
            {
                return pose;
            }

            if (phase == EnemyAttackVisualPhase.Recover)
            {
                float r = p.RecoverDuration > 0f ? Mathf.Clamp01(t / p.RecoverDuration) : 1f;
                float w = RecoverWeight(r, p.FollowThrough);
                EnemyPose peak = StrikePeak(p);
                pose.Offset = peak.Offset * w;
                pose.Euler = peak.Euler * w;
                // The stretch relaxes out of the strike while the weight lands back: a small squash mid-recovery.
                pose.Squash = peak.Squash * Mathf.Clamp01(w) * (1f - r) + 0.5f * p.AnticipationSquash * Mathf.Sin(Mathf.PI * r);
                return pose;
            }

            float coilEnd = StrikeStartTime(p, windup);
            if (t < coilEnd)
            {
                float u = t / coilEnd;
                pose = Coiled(p);
                pose.Scale(SmoothStep01(u / AnticipationGather));

                // Readable "about to strike" tremble over the last third of the coil.
                float late = Mathf.Clamp01((u - 0.65f) / 0.35f);
                pose.Euler.y = Mathf.Sin(t * TwoPi * 16f) * 2f * late;
                return pose;
            }

            float strike = windup - coilEnd;
            float k = strike > 0f ? Mathf.Clamp01((t - coilEnd) / strike) : 1f;
            k *= k; // ease-in: fastest at the contact, the follow-through carries the speed on
            EnemyPose coiled = Coiled(p);
            EnemyPose top = StrikePeak(p);
            pose.Offset = Vector3.LerpUnclamped(coiled.Offset, top.Offset, k);
            pose.Euler = Vector3.LerpUnclamped(coiled.Euler, top.Euler, k);
            pose.Squash = Mathf.LerpUnclamped(coiled.Squash, top.Squash, k);
            return pose;
        }

        /// <summary>Full anticipation pose (end of the gather).</summary>
        public static EnemyPose Coiled(in EnemyMotionProfile p)
        {
            return new EnemyPose
            {
                Offset = new Vector3(0f, p.WindupRise, -p.WindupPullBack),
                Euler = new Vector3(-p.WindupLeanDegrees, 0f, 0f),
                Squash = p.AnticipationSquash,
            };
        }

        /// <summary>Pose at the lunge peak (the damage moment): extended and stretched.</summary>
        public static EnemyPose StrikePeak(in EnemyMotionProfile p)
        {
            return new EnemyPose
            {
                Offset = new Vector3(0f, p.StrikeDrop, p.LungeDistance),
                Euler = new Vector3(p.StrikeLeanDegrees, 0f, 0f),
                Squash = -p.StrikeStretch,
            };
        }

        /// <summary>
        /// Weight of the strike peak during recovery (<paramref name="r"/> 0..1): 1 at the start (no pop),
        /// rising to 1 + <paramref name="followThrough"/> over <see cref="FollowThroughPortion"/>, then
        /// eased to exactly 0 with a slight dip below (settles back past rest).
        /// </summary>
        public static float RecoverWeight(float r, float followThrough)
        {
            r = Mathf.Clamp01(r);
            float ft = Mathf.Max(0f, followThrough);
            if (r < FollowThroughPortion)
            {
                return 1f + ft * Mathf.Sin(0.5f * Mathf.PI * r / FollowThroughPortion);
            }

            float u = (r - FollowThroughPortion) / (1f - FollowThroughPortion);
            return (1f + ft) * (1f - SmoothStep01(u)) - 0.5f * ft * Mathf.Sin(Mathf.PI * u) * u;
        }

        // ------------------------------------------------------------------ gait

        /// <summary>
        /// Gait cycles per second at <paramref name="speed"/> m/s: speed / stride (stride lengthened by
        /// the gallop blend and <paramref name="sizeScale"/>), clamped to the profile's range. At rest it
        /// is <see cref="EnemyMotionProfile.IdleStrideHz"/> (0 for walkers, wing beat for flyers).
        /// </summary>
        public static float StrideFrequency(in EnemyMotionProfile p, float speed, float sizeScale = 1f)
        {
            if (speed < 0.05f)
            {
                return p.IdleStrideHz;
            }

            float stride = Mathf.Max(0.05f, p.StrideLength * Mathf.Max(0.1f, sizeScale) * (1f + 0.2f * GallopBlend(p, speed)));
            float hz = Mathf.Clamp(speed / stride, p.MinStrideHz, p.MaxStrideHz);
            return Mathf.Max(hz, p.IdleStrideHz);
        }

        /// <summary>0 = trot, 1 = full gallop (quadrupeds only; others always 0).</summary>
        public static float GallopBlend(in EnemyMotionProfile p, float speed)
        {
            if (p.Archetype != EnemyMotionArchetype.Quadruped || p.GallopFullSpeed <= p.GallopStartSpeed)
            {
                return 0f;
            }

            return SmoothStep01((speed - p.GallopStartSpeed) / (p.GallopFullSpeed - p.GallopStartSpeed));
        }

        /// <summary>0..1 gait intensity from speed.</summary>
        public static float SpeedFactor(in EnemyMotionProfile p, float speed)
        {
            return p.FullGaitSpeed > 0f ? Mathf.Clamp01(speed / p.FullGaitSpeed) : 0f;
        }

        /// <summary>Heavy footfall index: two steps per gait cycle. A change of index = one stomp.</summary>
        public static int StepIndex(float phaseCycles)
        {
            return Mathf.FloorToInt(phaseCycles * 2f);
        }

        /// <summary>
        /// Locomotion pose for <paramref name="phaseCycles"/> (continuous cycle counter) at
        /// <paramref name="speedFactor"/> (0..1) and <paramref name="gallop"/> (0..1). Distances are
        /// for a reference-size body; multiply by size outside.
        /// </summary>
        public static EnemyPose SampleGait(in EnemyMotionProfile p, float phaseCycles, float speedFactor, float gallop)
        {
            EnemyPose pose = default;
            float ph = phaseCycles - Mathf.Floor(phaseCycles);
            float a = ph * TwoPi;
            float sinA = Mathf.Sin(a);
            float sin2A = Mathf.Sin(2f * a);
            float k = Mathf.Clamp01(speedFactor);

            switch (p.Archetype)
            {
                case EnemyMotionArchetype.Arthropod:
                {
                    // Many small steps: double-frequency bounce + a third harmonic for a jittery skitter.
                    float bounce = 0.5f - 0.5f * Mathf.Cos(2f * a) + 0.35f * Mathf.Sin(3f * a + 1.3f);
                    pose.Offset.y = (p.BobAmplitude * bounce + p.MoveCrouch) * k;
                    pose.Euler.x = p.PitchAmplitude * sin2A * k;
                    pose.Euler.y = p.YawAmplitude * sinA * k;
                    pose.Euler.z = p.RollAmplitude * Mathf.Sin(2f * a + 0.7f) * k;
                    pose.Squash = p.SquashAmplitude * Mathf.Sin(2f * a) * k;
                    break;
                }

                case EnemyMotionArchetype.Flyer:
                {
                    // Hover bob at the wing-beat rate even at rest; flutter roll at 3x; nose down when flying.
                    pose.Offset.y = p.HoverHeight + p.BobAmplitude * sinA;
                    pose.Euler.x = p.PitchAmplitude * k;
                    pose.Euler.z = p.RollAmplitude * Mathf.Sin(3f * a);
                    pose.Euler.y = p.YawAmplitude * Mathf.Sin(a * 0.5f + 0.4f);
                    pose.Squash = p.SquashAmplitude * Mathf.Sin(3f * a);
                    break;
                }

                case EnemyMotionArchetype.Heavy:
                {
                    // Two steps per cycle: slow lift arc then a sharp stomp dip right at foot contact (s = 0).
                    float s = ph * 2f;
                    s -= Mathf.Floor(s);
                    float impact = Mathf.Pow(1f - s, 6f);
                    pose.Offset.y = (p.BobAmplitude * Mathf.Sin(Mathf.PI * s) - p.BobAmplitude * 0.8f * impact) * k;
                    pose.Euler.x = p.PitchAmplitude * Mathf.Pow(1f - s, 4f) * k;
                    pose.Euler.z = p.RollAmplitude * sinA * k;
                    pose.Euler.y = p.YawAmplitude * sinA * k;
                    pose.Squash = p.SquashAmplitude * impact * k;
                    break;
                }

                default:
                {
                    // Trot: two diagonal-pair bounces per cycle, light rock. Gallop: one big bound per cycle,
                    // rocking-horse pitch, stretch in the air / squash on landing.
                    float g = Mathf.Clamp01(gallop);
                    float trotY = p.BobAmplitude * (0.5f - 0.5f * Mathf.Cos(2f * a));
                    float gallopY = p.GallopBobAmplitude * Mathf.Max(0f, sinA);
                    float trotPitch = p.PitchAmplitude * sin2A;
                    float gallopPitch = p.GallopPitchAmplitude * Mathf.Cos(a);
                    pose.Offset.y = Mathf.Lerp(trotY, gallopY, g) * k;
                    pose.Euler.x = Mathf.Lerp(trotPitch, gallopPitch, g) * k;
                    pose.Euler.z = p.RollAmplitude * sinA * (1f - 0.6f * g) * k;
                    pose.Euler.y = p.YawAmplitude * sinA * k;
                    pose.Squash = -p.SquashAmplitude * sinA * g * k;
                    break;
                }
            }

            return pose;
        }

        // ------------------------------------------------------------------ inertia (start / stop / turn)

        /// <summary>
        /// 0..1 "braking into a sharp turn": the heading swings faster than ~90 °/s while still moving.
        /// Drives the rear-back lean and the planted crouch of a pivot. Flyers bank instead (0).
        /// </summary>
        public static float TurnBrake01(in EnemyMotionProfile p, float yawRateDegPerSec, float speedFactor)
        {
            if (p.Archetype == EnemyMotionArchetype.Flyer || p.TurnBrakeLean <= 0f)
            {
                return 0f;
            }

            return SmoothStep01((Mathf.Abs(yawRateDegPerSec) - 90f) / 270f) * Mathf.Clamp01(speedFactor * 2f);
        }

        /// <summary>
        /// Target of the lean spring (pitch +nose down, roll +top left, degrees): leans into acceleration,
        /// back when braking (stops and sharp turns), and banks into turns. The spring (underdamped) adds
        /// the follow-through: a small forward tip after a stop, a sway back after a turn.
        /// </summary>
        public static Vector2 LeanTarget(in EnemyMotionProfile p, float acceleration, float yawRateDegPerSec, float speedFactor)
        {
            float pitch = Mathf.Clamp(acceleration * p.LeanPerAcceleration, -p.MaxAccelLean, p.MaxAccelLean);
            pitch -= p.TurnBrakeLean * TurnBrake01(p, yawRateDegPerSec, speedFactor);
            float turnWeight = p.Archetype == EnemyMotionArchetype.Flyer ? 1f : Mathf.Clamp01(speedFactor);
            float roll = Mathf.Clamp(-yawRateDegPerSec * 0.06f, -p.BankDegrees, p.BankDegrees) * turnWeight;
            return new Vector2(pitch, roll);
        }

        /// <summary>Target of the surge spring (m along local Z): the body trails behind on starts and carries forward on stops.</summary>
        public static float SurgeTarget(in EnemyMotionProfile p, float acceleration)
        {
            return -Mathf.Clamp(acceleration * p.SurgePerAcceleration, -p.MaxSurge, p.MaxSurge);
        }

        // ------------------------------------------------------------------ idle

        /// <summary>
        /// Idle life, faded out by <paramref name="speedFactor"/>: breathing for everyone, glances
        /// (<see cref="MotionFeelMath.IdleGlanceYaw"/>) and a per-archetype fidget: the wolf sniffs the
        /// ground and perks up while it looks around, arthropods twitch, bats drift and tilt their head,
        /// heavies heave and roll their shoulders. <paramref name="time"/> in seconds, desynced by
        /// <paramref name="seed"/>.
        /// </summary>
        public static EnemyPose SampleIdle(in EnemyMotionProfile p, float time, float seed, float speedFactor)
        {
            EnemyPose pose = default;
            float rest = 1f - Mathf.Clamp01(speedFactor);
            if (rest <= 0f)
            {
                return pose;
            }

            float t = time + seed;
            int glanceSeed = Mathf.RoundToInt(seed * 100f);
            float breath = Mathf.Sin(t * TwoPi * Mathf.Max(0.01f, p.BreathHz));
            float look = MotionFeelMath.IdleGlanceYaw(time + seed * 0.1f, p.IdleLookInterval, p.IdleLookHold, p.IdleLookDegrees, glanceSeed);
            float lookK = p.IdleLookDegrees > 0f ? Mathf.Abs(look) / p.IdleLookDegrees : 0f;
            pose.Squash = p.BreathSquash * breath;
            pose.Euler.y = look;

            switch (p.Archetype)
            {
                case EnemyMotionArchetype.Arthropod:
                    // Nervous: quick yaw twitches and a jittery squash blip.
                    pose.Euler.y += p.IdleFidgetDegrees * Pulse(t / 2.3f, 12f);
                    pose.Squash += 0.015f * Pulse(t / 1.7f + 0.3f, 16f);
                    break;
                case EnemyMotionArchetype.Flyer:
                    // The hover gait already flaps; add a slow drift and a curious head tilt with the glance.
                    pose.Offset.y = 0.04f * Mathf.Sin(t * TwoPi * 0.23f);
                    pose.Euler.z = p.IdleFidgetDegrees * Mathf.Sin(t * TwoPi * 0.17f) + 0.5f * look;
                    break;
                case EnemyMotionArchetype.Heavy:
                    // Slow chest heave and a shoulder roll; the glance is a slow head-and-torso turn.
                    pose.Offset.y = 0.012f * breath;
                    pose.Euler.z = p.IdleFidgetDegrees * Mathf.Sin(t * TwoPi * 0.15f);
                    break;
                default:
                    // Sniff: the head dips toward the ground; perks up (nose high) while looking around.
                    pose.Euler.x = p.IdleFidgetDegrees * Pulse(t / 3.7f, 8f) * (1f - lookK) - 3f * lookK;
                    break;
            }

            pose.Scale(rest);
            return pose;
        }

        /// <summary>
        /// 0..1 twitch of leg <paramref name="legIndex"/> (separate leg objects only): each leg flicks on
        /// its own jittered period (1.6–3.8 s), deterministic from <paramref name="seed"/>.
        /// </summary>
        public static float LegTwitch(float time, int legIndex, float seed)
        {
            int s = Mathf.RoundToInt(seed * 100f);
            float period = 1.6f + 2.2f * MotionFeelMath.Hash01(legIndex, s);
            float offset = MotionFeelMath.Hash01(legIndex * 7 + 3, s);
            return Pulse(time / period + offset, 14f);
        }

        /// <summary>Alert on aggro: quick hop + rear-up, 0 outside [0, <see cref="AlertDuration"/>].</summary>
        public static EnemyPose SampleAlert(in EnemyMotionProfile p, float t)
        {
            EnemyPose pose = default;
            if (t < 0f || t >= AlertDuration)
            {
                return pose;
            }

            float e = Mathf.Sin(Mathf.PI * t / AlertDuration);
            pose.Offset.y = p.AlertHop * e;
            pose.Euler.x = -p.AlertRearDegrees * e;
            pose.Squash = -0.05f * e;
            return pose;
        }

        // ------------------------------------------------------------------ hit / death / spawn

        /// <summary>
        /// Planar direction (in the enemy's local space) the body is pushed by a hit: away from the
        /// source. No source (DoT) or a source on top of the enemy → straight back (local -Z).
        /// </summary>
        public static Vector3 RecoilDirectionLocal(Vector3 enemyPosition, Quaternion enemyRotation, bool hasSource, Vector3 sourcePosition)
        {
            if (hasSource)
            {
                Vector3 away = enemyPosition - sourcePosition;
                away.y = 0f;
                if (away.sqrMagnitude > 0.0001f)
                {
                    Vector3 local = Quaternion.Inverse(enemyRotation) * away;
                    local.y = 0f;
                    if (local.sqrMagnitude > 0.0001f)
                    {
                        return local.normalized;
                    }
                }
            }

            return Vector3.back;
        }

        /// <summary>
        /// Spring kick of a hit along <paramref name="pushLocal"/>: velocities that make the hit springs
        /// (<see cref="EnemyMotionProfile.HitSpringHz"/>, underdamped) peak at
        /// <see cref="EnemyMotionProfile.RecoilDistance"/> / <see cref="EnemyMotionProfile.RecoilDegrees"/>
        /// × <paramref name="strength"/> away from the attacker (top tilted away), swing back past rest and settle.
        /// </summary>
        public static EnemyHitKick HitKick(in EnemyMotionProfile p, Vector3 pushLocal, float strength)
        {
            float k = Mathf.Clamp01(strength);
            float shove = MotionFeelMath.SpringVelocityForPeak(p.RecoilDistance * k, p.HitSpringHz, p.HitSpringDamping);
            float tilt = MotionFeelMath.SpringVelocityForPeak(p.RecoilDegrees * k, p.HitSpringHz, p.HitSpringDamping);
            return new EnemyHitKick
            {
                OffsetVelocity = new Vector2(pushLocal.x, pushLocal.z) * shove,
                TiltVelocity = new Vector2(pushLocal.z, -pushLocal.x) * tilt,
                SquashVelocity = MotionFeelMath.SpringVelocityForPeak(0.05f * k, p.SquashSpringHz, p.SquashSpringDamping),
            };
        }

        /// <summary>Topple progress: accelerating fall reaching 1 at <see cref="ToppleDuration"/>, then a small settle bounce.</summary>
        public static float Topple01(float t)
        {
            if (t <= 0f)
            {
                return 0f;
            }

            if (t < ToppleDuration)
            {
                float k = t / ToppleDuration;
                return k * k;
            }

            float b = (t - ToppleDuration) / 0.2f;
            return b < 1f ? 1f - 0.07f * Mathf.Sin(Mathf.PI * b) : 1f;
        }

        /// <summary>
        /// Height (0..1 of <see cref="EnemyMotionProfile.DeathBounce"/>) of the corpse <paramref name="t"/> s
        /// after it hits the ground: a parabolic hop, a smaller second one, then 0 for good (the sink takes over).
        /// </summary>
        public static float ImpactBounce01(float t)
        {
            if (t <= 0f)
            {
                return 0f;
            }

            if (t < FirstBounceDuration)
            {
                float u = t / FirstBounceDuration;
                return 4f * u * (1f - u);
            }

            t -= FirstBounceDuration;
            if (t < SecondBounceDuration)
            {
                float u = t / SecondBounceDuration;
                return SecondBounceHeight * 4f * u * (1f - u);
            }

            return 0f;
        }

        /// <summary>Spawn rise: 0 = fully sunk, 1 = at rest, overshooting above rest by <paramref name="overshoot"/> (ease-out-back; 0 = plain ease-out).</summary>
        public static float SpawnRise01(float t, float overshoot = 1.7f)
        {
            if (t >= SpawnRiseDuration)
            {
                return 1f;
            }

            float k = Mathf.Clamp01(t / SpawnRiseDuration) - 1f;
            float c = Mathf.Max(0f, overshoot);
            return 1f + (c + 1f) * k * k * k + c * k * k;
        }

        /// <summary>Speed cap that lets an enemy brake (at <paramref name="deceleration"/>) to a stop over <paramref name="remainingDistance"/>.</summary>
        public static float ArriveSpeed(float speed, float remainingDistance, float deceleration)
        {
            if (speed <= 0f)
            {
                return 0f;
            }

            float brake = Mathf.Sqrt(2f * Mathf.Max(0f, deceleration) * Mathf.Max(0f, remainingDistance)) + 0.5f;
            return Mathf.Min(speed, brake);
        }

        /// <summary>Frame-rate independent exponential smoothing factor.</summary>
        public static float Damp(float sharpness, float deltaTime)
        {
            return 1f - Mathf.Exp(-sharpness * deltaTime);
        }

        public static float SmoothStep01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        /// <summary>Periodic narrow bump (period 1 in <paramref name="cycles"/>), 0..1.</summary>
        private static float Pulse(float cycles, float sharpness)
        {
            return Mathf.Pow(Mathf.Max(0f, Mathf.Sin(cycles * TwoPi)), sharpness);
        }
    }
}
