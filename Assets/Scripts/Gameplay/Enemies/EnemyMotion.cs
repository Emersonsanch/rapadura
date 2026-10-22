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
    /// <see cref="ReferenceHeight"/>; the animator scales gait distances by the visual's real height.
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
        /// <summary>Wanted windup (telegraph) before the strike; clamped by <see cref="EnemyMotionMath.ResolveWindup"/>.</summary>
        public float BaseWindup;
        /// <summary>Lunge travel time; the lunge peaks exactly at the damage moment.</summary>
        public float StrikeDuration;
        public float RecoverDuration;
        public float WindupLeanDegrees;
        public float StrikeLeanDegrees;
        public float WindupPullBack;
        public float WindupRise;
        public float LungeDistance;
        /// <summary>Vertical offset at lunge peak (flyers dip toward the target: negative).</summary>
        public float StrikeDrop;

        // --- Reactions ---
        public float RecoilDistance;
        public float RecoilDegrees;
        public float RecoilDuration;
        public float AlertHop;
        public float AlertRearDegrees;

        public static readonly EnemyMotionProfile Quadruped = new EnemyMotionProfile
        {
            Archetype = EnemyMotionArchetype.Quadruped,
            Acceleration = 16f, Deceleration = 20f, TurnSmoothTime = 0.1f, MaxTurnSpeed = 480f,
            StrideLength = 1.1f, MinStrideHz = 1.3f, MaxStrideHz = 3.2f, IdleStrideHz = 0f, FullGaitSpeed = 4.5f,
            GallopStartSpeed = 2.6f, GallopFullSpeed = 3.9f,
            BobAmplitude = 0.05f, GallopBobAmplitude = 0.13f, PitchAmplitude = 2.5f, GallopPitchAmplitude = 9f,
            RollAmplitude = 2.5f, YawAmplitude = 2f, SquashAmplitude = 0.07f, MoveCrouch = 0f,
            HoverHeight = 0f, BankDegrees = 10f, ReferenceHeight = 0.9f,
            BaseWindup = 0.42f, StrikeDuration = 0.12f, RecoverDuration = 0.3f,
            WindupLeanDegrees = 12f, StrikeLeanDegrees = 10f, WindupPullBack = 0.15f, WindupRise = 0.05f,
            LungeDistance = 0.5f, StrikeDrop = 0f,
            RecoilDistance = 0.22f, RecoilDegrees = 14f, RecoilDuration = 0.3f, AlertHop = 0.18f, AlertRearDegrees = 14f,
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
            WindupLeanDegrees = 18f, StrikeLeanDegrees = 12f, WindupPullBack = 0.1f, WindupRise = 0.04f,
            LungeDistance = 0.42f, StrikeDrop = 0f,
            RecoilDistance = 0.2f, RecoilDegrees = 12f, RecoilDuration = 0.25f, AlertHop = 0.12f, AlertRearDegrees = 18f,
        };

        public static readonly EnemyMotionProfile Flyer = new EnemyMotionProfile
        {
            Archetype = EnemyMotionArchetype.Flyer,
            Acceleration = 10f, Deceleration = 8f, TurnSmoothTime = 0.2f, MaxTurnSpeed = 300f,
            StrideLength = 2.5f, MinStrideHz = 1.6f, MaxStrideHz = 3f, IdleStrideHz = 1.6f, FullGaitSpeed = 5f,
            GallopStartSpeed = 0f, GallopFullSpeed = 0f,
            BobAmplitude = 0.12f, GallopBobAmplitude = 0f, PitchAmplitude = 10f, GallopPitchAmplitude = 0f,
            RollAmplitude = 7f, YawAmplitude = 3f, SquashAmplitude = 0.05f, MoveCrouch = 0f,
            HoverHeight = 0.45f, BankDegrees = 25f, ReferenceHeight = 0.5f,
            BaseWindup = 0.36f, StrikeDuration = 0.12f, RecoverDuration = 0.3f,
            WindupLeanDegrees = 15f, StrikeLeanDegrees = 25f, WindupPullBack = 0.2f, WindupRise = 0.3f,
            LungeDistance = 0.6f, StrikeDrop = -0.45f,
            RecoilDistance = 0.3f, RecoilDegrees = 20f, RecoilDuration = 0.3f, AlertHop = 0.3f, AlertRearDegrees = 10f,
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
            WindupLeanDegrees = 14f, StrikeLeanDegrees = 16f, WindupPullBack = 0.12f, WindupRise = 0.1f,
            LungeDistance = 0.35f, StrikeDrop = -0.05f,
            RecoilDistance = 0.08f, RecoilDegrees = 6f, RecoilDuration = 0.35f, AlertHop = 0.06f, AlertRearDegrees = 8f,
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
        /// <summary>Windup (lean back, coil) then, in its last <see cref="EnemyMotionProfile.StrikeDuration"/>, the lunge — peaking at the damage moment.</summary>
        Windup = 1,
        /// <summary>After the damage moment: back from the lunge peak to rest.</summary>
        Recover = 2,
    }

    /// <summary>
    /// Pure, allocation-free math behind enemy movement feel and procedural animation. Static so
    /// EditMode tests can verify it without a scene; angles are degrees, phases are gait cycles.
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
        /// Attack pose at <paramref name="t"/> seconds into <paramref name="phase"/>. Windup: lean back /
        /// coil until <c>windup - StrikeDuration</c>, then lunge to the peak at exactly <paramref name="windup"/>
        /// (the damage moment), held there until the strike is reported. Recover: peak → rest.
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
                pose = StrikePeak(p);
                pose.Scale(1f - SmoothStep01(r));
                return pose;
            }

            float strike = Mathf.Min(p.StrikeDuration, windup * 0.5f);
            float coilEnd = Mathf.Max(0.0001f, windup - strike);
            if (t < coilEnd)
            {
                float e = SmoothStep01(t / coilEnd);
                pose.Offset = new Vector3(0f, p.WindupRise * e, -p.WindupPullBack * e);
                pose.Euler = new Vector3(-p.WindupLeanDegrees * e, 0f, 0f);
                pose.Squash = 0.08f * e;

                // Readable "about to strike" tremble over the last third of the coil.
                float late = Mathf.Clamp01((t / coilEnd - 0.65f) / 0.35f);
                pose.Euler.y = Mathf.Sin(t * TwoPi * 16f) * 2f * late;
                return pose;
            }

            float k = strike > 0f ? Mathf.Clamp01((t - coilEnd) / strike) : 1f;
            k = 1f - (1f - k) * (1f - k) * (1f - k); // ease-out cubic
            EnemyPose coiled = default;
            coiled.Offset = new Vector3(0f, p.WindupRise, -p.WindupPullBack);
            coiled.Euler = new Vector3(-p.WindupLeanDegrees, 0f, 0f);
            coiled.Squash = 0.08f;
            EnemyPose peak = StrikePeak(p);
            pose.Offset = Vector3.LerpUnclamped(coiled.Offset, peak.Offset, k);
            pose.Euler = Vector3.LerpUnclamped(coiled.Euler, peak.Euler, k);
            pose.Squash = Mathf.LerpUnclamped(coiled.Squash, peak.Squash, k);
            return pose;
        }

        /// <summary>Pose at the lunge peak (the damage moment).</summary>
        public static EnemyPose StrikePeak(in EnemyMotionProfile p)
        {
            return new EnemyPose
            {
                Offset = new Vector3(0f, p.StrikeDrop, p.LungeDistance),
                Euler = new Vector3(p.StrikeLeanDegrees, 0f, 0f),
                Squash = -0.1f,
            };
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

        /// <summary>Idle breathing / sniff / twitch, faded out by <paramref name="speedFactor"/>. <paramref name="time"/> in seconds, desynced by <paramref name="seed"/>.</summary>
        public static EnemyPose SampleIdle(in EnemyMotionProfile p, float time, float seed, float speedFactor)
        {
            EnemyPose pose = default;
            float rest = 1f - Mathf.Clamp01(speedFactor);
            if (rest <= 0f)
            {
                return pose;
            }

            float t = time + seed;
            switch (p.Archetype)
            {
                case EnemyMotionArchetype.Arthropod:
                    pose.Squash = 0.015f * Mathf.Sin(t * TwoPi * 0.6f);
                    pose.Euler.y = 7f * Pulse(t / 2.3f, 12f);
                    break;
                case EnemyMotionArchetype.Flyer:
                    break; // the hover gait already animates it at rest
                case EnemyMotionArchetype.Heavy:
                    pose.Squash = 0.03f * Mathf.Sin(t * TwoPi * 0.3f);
                    pose.Euler.z = 1.5f * Mathf.Sin(t * TwoPi * 0.15f);
                    break;
                default:
                    pose.Squash = 0.02f * Mathf.Sin(t * TwoPi * 0.45f);
                    pose.Euler.x = 6f * Pulse(t / 3.7f, 8f); // sniff: head dips toward the ground
                    pose.Euler.y = 4f * Mathf.Sin(t * TwoPi * 0.12f);
                    break;
            }

            pose.Scale(rest);
            return pose;
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
            pose.Squash = -0.08f * e;
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

        /// <summary>Recoil intensity: snaps to 1 within 0.04 s, then decays to exactly 0 at <paramref name="duration"/>.</summary>
        public static float RecoilEnvelope(float t, float duration)
        {
            if (t < 0f || duration <= 0f || t >= duration)
            {
                return 0f;
            }

            const float attack = 0.04f;
            if (t < attack)
            {
                return t / attack;
            }

            float u = (t - attack) / Mathf.Max(0.0001f, duration - attack);
            return Mathf.Exp(-u * 3.5f) * (1f - u);
        }

        /// <summary>Recoil pose for a push along <paramref name="pushLocal"/>: shoved away, top tilted away, short squash.</summary>
        public static EnemyPose SampleRecoil(in EnemyMotionProfile p, Vector3 pushLocal, float t, float strength)
        {
            EnemyPose pose = default;
            float env = RecoilEnvelope(t, p.RecoilDuration) * Mathf.Clamp01(strength);
            if (env <= 0f)
            {
                return pose;
            }

            pose.Offset = new Vector3(pushLocal.x, 0f, pushLocal.z) * (p.RecoilDistance * env);
            pose.Euler = new Vector3(pushLocal.z * p.RecoilDegrees * env, 0f, -pushLocal.x * p.RecoilDegrees * env);
            pose.Squash = 0.12f * env;
            return pose;
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

        /// <summary>Spawn rise: 0 = fully sunk, 1 = at rest, with a small overshoot (ease-out-back).</summary>
        public static float SpawnRise01(float t)
        {
            if (t >= SpawnRiseDuration)
            {
                return 1f;
            }

            float k = Mathf.Clamp01(t / SpawnRiseDuration) - 1f;
            const float c = 1.7f;
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
