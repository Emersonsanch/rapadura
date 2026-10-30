using System;
using UnityEngine;

namespace Rapadura.Gameplay.Locomotion
{
    /// <summary>Tuning of a <see cref="LegGaitSolver"/>. Distances in metres; the "fraction" fields are of each leg's length.</summary>
    [Serializable]
    public struct LegGaitSettings
    {
        public LegGaitStyle Style;

        [Tooltip("Metres covered per gait cycle at low speed.")]
        public float StrideLength;
        [Tooltip("Extra stride (fraction) at FullSpeed: faster = longer strides, not only faster steps.")]
        public float StrideGrowth;
        public float FullSpeed;
        [Tooltip("Gait cycles per second while moving (clamped).")]
        public float MinCadence;
        public float MaxCadence;
        [Tooltip("Cadence used to settle the feet after a stop and to step while turning in place.")]
        public float IdleCadence;
        [Tooltip("Turning faster than this (deg/s) while standing makes the feet step round.")]
        public float TurnStepYawRate;

        [Tooltip("Quadrupeds: walk → trot by this speed, trot → gallop between the two gallop speeds (m/s). Bipeds: walk → run by this speed.")]
        public float TrotSpeed;
        public float GallopStartSpeed;
        public float GallopFullSpeed;

        [Tooltip("Swing apex, fraction of the leg length.")]
        public float StepHeightFraction;
        [Tooltip("Displacement from the rest target worth a step in the leg's phase window (fraction of the leg).")]
        public float MinStepFraction;
        [Tooltip("Displacement beyond which a foot steps whatever its phase (fraction of the leg).")]
        public float ReachFraction;
        public float MinSwingTime;

        [Tooltip("Body bob (m), roll and pitch (deg) produced by the lifted feet.")]
        public float BobAmplitude;
        public float RollDegrees;
        public float PitchDegrees;

        public static LegGaitSettings Default(LegGaitStyle style)
        {
            switch (style)
            {
                case LegGaitStyle.Arthropod:
                    return new LegGaitSettings
                    {
                        Style = style, StrideLength = 0.45f, StrideGrowth = 0.6f, FullSpeed = 4f,
                        MinCadence = 2.5f, MaxCadence = 6.5f, IdleCadence = 4f, TurnStepYawRate = 25f,
                        StepHeightFraction = 0.3f, MinStepFraction = 0.1f, ReachFraction = 0.4f, MinSwingTime = 0.06f,
                        BobAmplitude = 0.015f, RollDegrees = 1.5f, PitchDegrees = 1.5f,
                    };
                case LegGaitStyle.Biped:
                    return new LegGaitSettings
                    {
                        Style = style, StrideLength = 1.4f, StrideGrowth = 0.25f, FullSpeed = 3f,
                        MinCadence = 0.55f, MaxCadence = 2.2f, IdleCadence = 0.9f, TurnStepYawRate = 20f, TrotSpeed = 3f,
                        StepHeightFraction = 0.18f, MinStepFraction = 0.1f, ReachFraction = 0.5f, MinSwingTime = 0.18f,
                        BobAmplitude = 0.06f, RollDegrees = 3f, PitchDegrees = 1.5f,
                    };
                default:
                    return new LegGaitSettings
                    {
                        Style = LegGaitStyle.Quadruped, StrideLength = 1.0f, StrideGrowth = 0.4f, FullSpeed = 4.5f,
                        MinCadence = 1.2f, MaxCadence = 3.4f, IdleCadence = 1.8f, TurnStepYawRate = 30f,
                        TrotSpeed = 1.4f, GallopStartSpeed = 2.8f, GallopFullSpeed = 3.9f,
                        StepHeightFraction = 0.22f, MinStepFraction = 0.12f, ReachFraction = 0.5f, MinSwingTime = 0.08f,
                        BobAmplitude = 0.04f, RollDegrees = 2f, PitchDegrees = 7f,
                    };
            }
        }
    }

    /// <summary>
    /// Ground query used by <see cref="LegGaitSolver"/>: drop a ray at <paramref name="point"/> from
    /// <paramref name="up"/> above to <paramref name="down"/> below; false = no ground (or no budget).
    /// </summary>
    public delegate bool LegGroundProbe(Vector3 point, float up, float down, out Vector3 ground, out Vector3 normal);

    /// <summary>
    /// Per-leg state machine of the procedural locomotion, in world space and free of transforms (so it
    /// is EditMode-simulated): every foot is either <b>planted</b> (its world position never changes —
    /// no foot sliding, by construction) or <b>swinging</b> along an arc to a predicted landing. A gait
    /// clock advances at the cadence for the current speed; each leg's phase (clock + gait offset) opens a
    /// swing window once per cycle, and the foot lifts in it if it has drifted from its rest target
    /// (body-relative point under the hip). Beyond reach it steps right away (turns, shoves, lunges).
    /// Standing still the clock stops once every foot has settled, but keeps ticking at the idle cadence
    /// to step round while turning in place. Lifted feet also produce the body bob/roll/pitch so the body
    /// moves with its footfalls. Teleports (pool reuse, spawn) re-plant every foot.
    /// </summary>
    public sealed class LegGaitSolver
    {
        public const int MaxLegs = 8;
        private const float ClockWrap = 1000f;

        private struct Leg
        {
            public Vector3 RestLocal;   // foot rest position in body space (body = root, yaw only)
            public float Sole;          // foot point height above the ground
            public float Length;        // leg length (m)
            public float MaxReach;      // geometric limit of the drift from the rest target (m)
            public int Side;            // -1 left, +1 right
            public int Front;           // +1 front half, -1 hind half, 0 middle / biped

            public bool Swinging;
            public Vector3 Planted;
            public Vector3 From;
            public Vector3 To;
            public float SwingTime;
            public float SwingDuration;
            public bool Reprobed;
            public int LastCycle;
            public Vector3 Normal;
        }

        private readonly Leg[] _legs = new Leg[MaxLegs];
        private int _count;
        private float _clock;
        private float _groundLocalY;
        private bool _planted;

        public LegGaitSettings Settings = LegGaitSettings.Default(LegGaitStyle.Quadruped);

        /// <summary>Optional ground query; null = flat ground at the body's feet plane.</summary>
        public LegGroundProbe Probe;

        public int LegCount => _count;
        public float Clock => _clock;
        public float Cadence { get; private set; }
        public float Duty { get; private set; } = LegGaitMath.WalkDuty;
        public float Trot { get; private set; }
        public float Gallop { get; private set; }

        /// <summary>Body sway targets from the lifted feet (m / deg), recomputed every <see cref="Step"/>.</summary>
        public float Bob { get; private set; }
        public float Roll { get; private set; }
        public float Pitch { get; private set; }

        /// <summary>Feet that touched down during the last <see cref="Step"/>, and the strongest landing (0..1).</summary>
        public int Landings { get; private set; }
        public float LandingStrength { get; private set; }
        public Vector3 LandingPoint { get; private set; }

        public bool IsSwinging(int leg) => _legs[leg].Swinging;
        public Vector3 PlantedPosition(int leg) => _legs[leg].Planted;
        public Vector3 GroundNormal(int leg) => _legs[leg].Normal;
        public float LegLength(int leg) => _legs[leg].Length;
        public int Side(int leg) => _legs[leg].Side;

        public void Clear()
        {
            _count = 0;
            _planted = false;
        }

        /// <summary>
        /// Adds a leg (call in the leg order convention of <see cref="LegGaitMath"/>). <paramref name="restLocal"/>
        /// is the foot point in body space, <paramref name="groundLocalY"/> the body-space height of the
        /// ground under the feet (so the sole height is their difference). <paramref name="maxReach"/>
        /// caps how far the planted foot may drift from its rest target before stepping (what the leg's
        /// geometry allows with the hip at its riding height; see <see cref="ProceduralLegLocomotion"/>).
        /// </summary>
        public void AddLeg(Vector3 restLocal, float groundLocalY, float length, int side, int front, float maxReach = float.MaxValue)
        {
            if (_count >= MaxLegs)
            {
                return;
            }

            _groundLocalY = groundLocalY;
            _legs[_count] = new Leg
            {
                RestLocal = restLocal,
                Sole = Mathf.Max(0f, restLocal.y - groundLocalY),
                Length = Mathf.Max(0.02f, length),
                MaxReach = Mathf.Max(0.02f, maxReach),
                Side = side,
                Front = front,
                Normal = Vector3.up,
            };
            _count++;
            _planted = false;
        }

        public Vector3 RestTarget(int leg, Vector3 bodyPosition, Quaternion bodyRotation) => bodyPosition + bodyRotation * _legs[leg].RestLocal;

        public float Reach(int leg) => Mathf.Min(_legs[leg].Length * Mathf.Max(0.05f, Settings.ReachFraction), _legs[leg].MaxReach);

        /// <summary>Re-plants every foot on its rest target (spawn, pool reuse, teleport, LOD wake-up).</summary>
        public void Reset(Vector3 bodyPosition, Quaternion bodyRotation)
        {
            Cadence = 0f;
            Bob = Roll = Pitch = 0f;
            Landings = 0;
            for (int i = 0; i < _count; i++)
            {
                ref Leg leg = ref _legs[i];
                leg.Swinging = false;
                leg.Planted = Ground(ref leg, RestTarget(i, bodyPosition, bodyRotation));
                // The current cycle's window stays available: a planted foot only uses it once it drifts.
                leg.LastCycle = Mathf.FloorToInt(_clock + LegGaitMath.PhaseOffset(Settings.Style, i, _count, Trot, Gallop)) - 1;
            }

            _planted = true;
        }

        /// <summary>Current foot point of <paramref name="leg"/> (planted, or along its swing arc).</summary>
        public Vector3 FootPosition(int leg)
        {
            ref Leg l = ref _legs[leg];
            if (!l.Swinging)
            {
                return l.Planted;
            }

            float t = l.SwingDuration > 0f ? l.SwingTime / l.SwingDuration : 1f;
            float distance = LegGaitMath.PlanarDistance(l.From, l.To);
            float height = l.Length * Settings.StepHeightFraction * Mathf.Clamp(distance / Mathf.Max(0.01f, Reach(leg)), 0.35f, 1f);
            Vector3 from = l.From, to = l.To;
            float top = Mathf.Max(from.y, to.y);
            Vector3 p = LegGaitMath.SwingPosition(from, to, t, height);
            p.y += (top - Mathf.Lerp(from.y, to.y, LegGaitMath.SwingTravel(t))) * LegGaitMath.SwingLift(t); // clear a step up
            return p;
        }

        /// <summary>
        /// Advances the gait by <paramref name="deltaTime"/> for a body at <paramref name="bodyPosition"/>/<paramref name="bodyRotation"/>
        /// moving at <paramref name="velocity"/> (world, m/s) and turning at <paramref name="yawRateDeg"/>.
        /// </summary>
        public void Step(float deltaTime, Vector3 bodyPosition, Quaternion bodyRotation, Vector3 velocity, float yawRateDeg)
        {
            Landings = 0;
            LandingStrength = 0f;
            if (_count == 0 || deltaTime <= 0f)
            {
                return;
            }

            if (!_planted)
            {
                Reset(bodyPosition, bodyRotation);
            }

            LegGaitSettings s = Settings;
            velocity.y = 0f;
            float speed = velocity.magnitude;

            float trot = 0f, gallop = 0f;
            if (s.Style == LegGaitStyle.Quadruped && _count == 4)
            {
                LegGaitMath.QuadrupedBlend(speed, s.TrotSpeed, s.GallopStartSpeed, s.GallopFullSpeed, out trot, out gallop);
            }
            else if (s.Style == LegGaitStyle.Biped || _count <= 2)
            {
                LegGaitMath.QuadrupedBlend(speed, s.TrotSpeed, 0f, 0f, out trot, out _); // walk → run
            }

            Trot = trot;
            Gallop = gallop;
            float duty = LegGaitMath.DutyFactor(s.Style, _count, trot, gallop);
            Duty = duty;

            // Stride grows with speed but never asks a stance longer than the legs can reach.
            float reach = float.MaxValue;
            bool unsettled = false;
            bool teleported = false;
            for (int i = 0; i < _count; i++)
            {
                reach = Mathf.Min(reach, Reach(i));
                if (!_legs[i].Swinging)
                {
                    float drift = LegGaitMath.PlanarDistance(_legs[i].Planted, RestTarget(i, bodyPosition, bodyRotation));
                    unsettled |= drift >= MinStep(i);
                    teleported |= drift > _legs[i].Length * 4f;
                }
            }

            if (teleported)
            {
                Reset(bodyPosition, bodyRotation);
                return;
            }

            float stride = Mathf.Min(LegGaitMath.StrideLength(s.StrideLength, speed, s.FullSpeed, s.StrideGrowth), 1.8f * reach / Mathf.Max(0.2f, duty));
            float cadence = LegGaitMath.Cadence(speed, stride, s.MinCadence, s.MaxCadence);
            bool turning = Mathf.Abs(yawRateDeg) >= s.TurnStepYawRate;
            if (cadence < s.IdleCadence && (turning || (unsettled && speed < 0.05f)))
            {
                cadence = s.IdleCadence;
            }

            Cadence = cadence;
            _clock += cadence * deltaTime;
            if (_clock > ClockWrap)
            {
                _clock -= ClockWrap;
                for (int i = 0; i < _count; i++)
                {
                    _legs[i].LastCycle -= (int)ClockWrap;
                }
            }

            float stanceTime = duty / Mathf.Max(0.1f, Mathf.Max(cadence, s.IdleCadence));
            int swinging = 0;
            for (int i = 0; i < _count; i++)
            {
                swinging += _legs[i].Swinging ? 1 : 0;
            }

            for (int i = 0; i < _count; i++)
            {
                ref Leg leg = ref _legs[i];
                Vector3 rest = RestTarget(i, bodyPosition, bodyRotation);
                float maxLead = Reach(i) * 0.8f;

                if (leg.Swinging)
                {
                    leg.SwingTime += deltaTime;
                    float t = leg.SwingTime / Mathf.Max(1e-4f, leg.SwingDuration);
                    if (t < 0.6f)
                    {
                        // Keep aiming where the hip will be: the body may speed up, slow down or turn mid-swing.
                        Vector3 aim = LegGaitMath.PredictLanding(bodyPosition, rest, velocity, yawRateDeg, leg.SwingDuration - leg.SwingTime, stanceTime, maxLead);
                        float y = leg.To.y;
                        leg.To = new Vector3(aim.x, y, aim.z);
                        if (!leg.Reprobed && t >= 0.4f)
                        {
                            leg.Reprobed = true;
                            leg.To = Ground(ref leg, new Vector3(aim.x, rest.y, aim.z));
                        }
                    }

                    if (t >= 1f)
                    {
                        Land(ref leg, i);
                        swinging--;
                    }

                    continue;
                }

                float u = _clock + LegGaitMath.PhaseOffset(s.Style, i, _count, trot, gallop);
                int cycle = Mathf.FloorToInt(u);
                float local = u - cycle;
                bool window = cadence > 0f && local >= duty && leg.LastCycle != cycle;
                if (window)
                {
                    leg.LastCycle = cycle; // one chance per cycle: a settled foot waits for the next
                }

                float displacement = LegGaitMath.PlanarDistance(leg.Planted, rest);
                // Beyond reach: step now, keeping half the feet down — unless it is badly left behind.
                bool urgent = displacement >= Reach(i) && (swinging < Mathf.Max(1, _count / 2) || displacement >= Reach(i) * 1.6f);
                if (!LegGaitMath.ShouldStep(displacement, MinStep(i), window, urgent ? 0f : float.MaxValue))
                {
                    continue;
                }

                float rate = Mathf.Max(cadence, s.IdleCadence);
                float duration = window ? (1f - local) / Mathf.Max(0.1f, cadence) : (1f - duty) / Mathf.Max(0.1f, rate);
                leg.SwingDuration = Mathf.Max(s.MinSwingTime, duration);
                leg.SwingTime = 0f;
                leg.Swinging = true;
                leg.Reprobed = false;
                leg.From = leg.Planted;
                Vector3 landing = LegGaitMath.PredictLanding(bodyPosition, rest, velocity, yawRateDeg, leg.SwingDuration, stanceTime, maxLead);
                leg.To = Ground(ref leg, new Vector3(landing.x, rest.y, landing.z));
                swinging++;
            }

            ComputeSway();
        }

        private float MinStep(int leg) => _legs[leg].Length * Mathf.Max(0.01f, Settings.MinStepFraction);

        private void Land(ref Leg leg, int index)
        {
            leg.Swinging = false;
            float travel = LegGaitMath.PlanarDistance(leg.From, leg.To);
            leg.Planted = leg.To;
            Landings++;
            float strength = Mathf.Clamp01(travel / Mathf.Max(0.01f, Reach(index) * 1.5f));
            if (strength >= LandingStrength)
            {
                LandingStrength = strength;
                LandingPoint = leg.Planted;
            }
        }

        /// <summary>Ground-snaps <paramref name="point"/> (a rest-height point) for <paramref name="leg"/>: sole height above the hit.</summary>
        private Vector3 Ground(ref Leg leg, Vector3 point)
        {
            leg.Normal = Vector3.up;
            if (Probe != null)
            {
                float range = leg.Length;
                Vector3 floor = point - Vector3.up * leg.Sole;
                if (Probe(floor, range, range, out Vector3 ground, out Vector3 normal) && Mathf.Abs(ground.y - floor.y) <= range * 0.75f)
                {
                    leg.Normal = normal;
                    return new Vector3(point.x, ground.y + leg.Sole, point.z);
                }
            }

            return point;
        }

        /// <summary>Lifted feet unload the body: it rises mid-swing, rolls toward the stance side and pitches with the front/hind feet.</summary>
        private void ComputeSway()
        {
            float lift = 0f, roll = 0f, pitch = 0f;
            for (int i = 0; i < _count; i++)
            {
                ref Leg leg = ref _legs[i];
                if (!leg.Swinging)
                {
                    continue;
                }

                float l = LegGaitMath.SwingLift(leg.SwingTime / Mathf.Max(1e-4f, leg.SwingDuration));
                lift += l;
                roll += leg.Side * l;
                pitch -= leg.Front * l;
            }

            float norm = Mathf.Max(1f, _count * (1f - Duty));
            float k = Cadence > 0f ? 1f : 0f;
            Bob = Settings.BobAmplitude * (lift / norm - 0.5f) * k;
            Roll = Settings.RollDegrees * Mathf.Clamp(roll / norm, -1f, 1f);
            float pitchScale = Settings.Style == LegGaitStyle.Quadruped ? Mathf.Lerp(0.25f, 1f, Gallop) : 1f;
            Pitch = Settings.PitchDegrees * pitchScale * Mathf.Clamp(pitch / norm, -1f, 1f);
        }
    }
}
