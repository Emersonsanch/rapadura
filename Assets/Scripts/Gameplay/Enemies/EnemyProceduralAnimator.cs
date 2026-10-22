using System;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Rig-free animation for enemy visuals. None of the enemy models has a skeleton (Wolf/Spider/Scorpion
    /// are static Meshy FBX, the rest are primitives — Meshy's rigging API only supports bipeds), so the
    /// whole body is posed procedurally on the visual root (the "EnemyVisual" child, never the collider
    /// root) as local offsets/rotations/squash on top of its rest pose:
    ///
    ///  * gait per <see cref="EnemyMotionArchetype"/> driven by the root's measured planar speed
    ///    (displacement / dt, so slows, roots and collisions are honoured): quadruped trot → gallop with
    ///    pitch rock and stretch/squash; arthropod low high-frequency skitter with yaw wiggle; flyer hover
    ///    bob + flutter + banking; heavy stomping steps (publishes <see cref="EnemyFootstepEvent"/> per footfall);
    ///  * behaviour poses: idle breathing/sniff, alert hop on aggro, telegraphed attack
    ///    (windup → lunge peaking at the damage moment → recover, timed by <see cref="EnemyController"/>'s
    ///    attack counters), hit recoil away from the attacker + squash (<see cref="DamageAppliedEvent"/>),
    ///    death topple during the first <see cref="EnemyMotionMath.ToppleDuration"/> s of
    ///    <see cref="EnemyDeathVfx"/>'s sink (that moves the root, this only the visual — they compose),
    ///    spawn pop-in rising from the ground.
    ///
    /// One LateUpdate per enemy, scaled time (frozen while paused), no per-frame allocations. The pure
    /// math lives in <see cref="EnemyMotionMath"/> (EditMode-tested).
    /// </summary>
    public class EnemyProceduralAnimator : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;

        [Tooltip("Global multiplier on every procedural pose (gait, idle, attack, recoil). 0 = rest pose only (topple/spawn still play).")]
        [Range(0f, 2f)]
        [SerializeField] private float _intensity = 1f;

        [Tooltip("How fast the measured speed follows the real displacement (higher = snappier gait changes).")]
        [SerializeField] private float _speedSmoothing = 10f;

        [Tooltip("Displacements faster than this (m/s) are teleports (spawn, pool reuse) and ignored.")]
        [SerializeField] private float _maxPlausibleSpeed = 25f;

        private const float PhaseWrap = 1000f;

        private Transform _root;
        private EnemyController _controller;
        private Health _health;
        private bool _initialized;

        // Rest pose (captured when the visual is assigned).
        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation;
        private Vector3 _restLocalScale = Vector3.one;
        private bool _hasRestPose;
        private Vector3 _bodyCenter;
        private Vector3 _bodyExtents = new Vector3(0.5f, 0.5f, 0.5f);
        private float _footHeight = -1f;
        private int _upAxis = 1;

        // Measurement.
        private Vector3 _lastRootPosition;
        private float _lastYaw;
        private float _speed;
        private float _acceleration;
        private float _yawRate;

        // Gait.
        private float _phase;
        private int _lastStep;
        private float _time;
        private float _seed;

        // Controller hand-off.
        private int _seenGeneration = int.MinValue;
        private int _seenAttack;
        private int _seenStrike;
        private int _seenCancel;
        private int _seenAlert;

        // Behaviour timers (negative = inactive).
        private EnemyAttackVisualPhase _attackPhase;
        private float _attackTime;
        private float _attackWindup;
        private EnemyPose _cancelPose;
        private float _cancelTime = -1f;
        private float _alertTime = -1f;
        private float _hitTime = -1f;
        private float _hitStrength;
        private Vector3 _hitDirection = Vector3.back;
        private bool _dead;
        private float _deathTime;
        private Vector3 _deathPush = Vector3.back;
        private float _spawnTime = -1f;

        private Action<DamageAppliedEvent> _onDamageApplied;

        public Transform VisualRoot => _visualRoot;

        /// <summary>Smoothed measured planar speed (m/s) driving the gait.</summary>
        public float MeasuredSpeed => _speed;

        /// <summary>Gait phase in cycles (continuous).</summary>
        public float GaitPhase => _phase;

        public EnemyAttackVisualPhase AttackPhase => _attackPhase;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            _onDamageApplied ??= OnDamageApplied;
            EventBus.Subscribe(_onDamageApplied);
            BeginLife();
        }

        private void OnDisable()
        {
            if (_onDamageApplied != null)
            {
                EventBus.Unsubscribe(_onDamageApplied);
            }

            RestoreRestPose();
        }

        private void LateUpdate()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>
        /// Re-targets which transform receives the poses — used by <see cref="EnemyVisualSwapper"/> after
        /// replacing the enemy's visual. The outgoing visual is put back at its rest pose first (it may be
        /// re-activated later); the rest pose and body bounds are recaptured from the new one. Null falls
        /// back to this transform.
        /// </summary>
        public void SetVisualRoot(Transform visualRoot)
        {
            EnsureInitialized();

            Transform next = visualRoot != null ? visualRoot : transform;
            if (_hasRestPose && _visualRoot != null && _visualRoot != next)
            {
                RestoreRestPose();
            }

            _visualRoot = next;
            CaptureRestPose();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _controller = GetComponentInParent<EnemyController>();
            // The root is the enemy itself (the transform that moves), never a pool/spawner container:
            // measuring the parent's motion left pooled enemies frozen in their rest pose.
            _root = _controller != null ? _controller.transform : transform;
            _health = _root.GetComponent<Health>();

            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }

            _seed = (Mathf.Abs(GetInstanceID()) % 1024) * 0.37f;
            _phase = _seed;
            _lastStep = EnemyMotionMath.StepIndex(_phase);
            CaptureRestPose();
            ResetMeasurement();
        }

        private void CaptureRestPose()
        {
            if (_visualRoot == null)
            {
                return;
            }

            _restLocalPosition = _visualRoot.localPosition;
            _restLocalRotation = _visualRoot.localRotation;
            _restLocalScale = _visualRoot.localScale;
            _hasRestPose = true;

            Vector3 upInVisual = Quaternion.Inverse(_restLocalRotation) * Vector3.up;
            float ax = Mathf.Abs(upInVisual.x), ay = Mathf.Abs(upInVisual.y), az = Mathf.Abs(upInVisual.z);
            _upAxis = ax > ay && ax > az ? 0 : (az > ay ? 2 : 1);

            Transform space = _visualRoot.parent != null ? _visualRoot.parent : _root;
            if (space != null && _visualRoot != _root && EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(space, _visualRoot, out Bounds bounds))
            {
                _bodyCenter = bounds.center;
                _bodyExtents = Vector3.Max(bounds.extents, new Vector3(0.05f, 0.05f, 0.05f));
                _footHeight = bounds.min.y;
            }
            else
            {
                _bodyCenter = _restLocalPosition;
                _bodyExtents = new Vector3(0.5f, 0.5f, 0.5f);
                _footHeight = _restLocalPosition.y - 0.5f;
            }
        }

        private void RestoreRestPose()
        {
            if (!_hasRestPose || _visualRoot == null || _visualRoot == _root)
            {
                return;
            }

            _visualRoot.SetLocalPositionAndRotation(_restLocalPosition, _restLocalRotation);
            _visualRoot.localScale = _restLocalScale;
        }

        private void ResetMeasurement()
        {
            if (_root == null)
            {
                return;
            }

            _lastRootPosition = _root.position;
            _lastYaw = _root.eulerAngles.y;
            _speed = 0f;
            _acceleration = 0f;
            _yawRate = 0f;
        }

        /// <summary>Fresh spawn / pool reuse: clear every reaction and play the pop-in rise.</summary>
        private void BeginLife()
        {
            ResetMeasurement();
            _attackPhase = EnemyAttackVisualPhase.None;
            _cancelTime = -1f;
            _alertTime = -1f;
            _hitTime = -1f;
            _dead = false;
            _deathTime = 0f;
            _spawnTime = 0f;

            if (_controller != null)
            {
                _seenGeneration = _controller.SpawnGeneration;
                _seenAttack = _controller.AttackSequence;
                _seenStrike = _controller.StrikeSequence;
                _seenCancel = _controller.AttackCancelSequence;
                _seenAlert = _controller.AlertSequence;
            }
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (_root == null || evt.Target != _root.gameObject || evt.FinalDamage <= 0f)
            {
                return;
            }

            bool hasSource = evt.Source != null && evt.Source != evt.Target;
            Vector3 sourcePosition = hasSource ? evt.Source.transform.position : Vector3.zero;
            _hitDirection = EnemyMotionMath.RecoilDirectionLocal(_root.position, _root.rotation, hasSource, sourcePosition);
            float maxHealth = _health != null ? Mathf.Max(1f, _health.MaxHealth) : 100f;
            _hitStrength = Mathf.Clamp(0.45f + 3f * evt.FinalDamage / maxHealth, 0.45f, 1f);
            _hitTime = 0f;
            if (!_dead)
            {
                _deathPush = _hitDirection;
            }
        }

        /// <summary>
        /// Core per-frame update with an explicit deltaTime (EditMode tests drive it directly; in play it
        /// runs from LateUpdate after the AI moved the root). deltaTime ≤ 0 (paused) holds the pose.
        /// </summary>
        public void Tick(float deltaTime)
        {
            EnsureInitialized();
            if (deltaTime <= 0f || _visualRoot == null || _root == null || _visualRoot == _root)
            {
                return;
            }

            SyncWithController();

            EnemyMotionProfile profile = _controller != null ? _controller.Motion : EnemyMotionProfile.Quadruped;
            float size = Mathf.Clamp(_bodyExtents.y * 2f / Mathf.Max(0.05f, profile.ReferenceHeight), 0.6f, 1.8f);

            Measure(deltaTime);
            _time += deltaTime;
            if (_time > PhaseWrap)
            {
                _time -= PhaseWrap;
            }

            if (_dead)
            {
                _deathTime += deltaTime;
            }

            float deathK = _dead ? EnemyMotionMath.Topple01(_deathTime) : 0f;
            float alive = 1f - Mathf.Clamp01(deathK);

            // --- Gait ---
            float speedFactor = EnemyMotionMath.SpeedFactor(profile, _speed);
            float gallop = EnemyMotionMath.GallopBlend(profile, _speed);
            _phase += EnemyMotionMath.StrideFrequency(profile, _speed, size) * deltaTime * alive;
            if (_phase > PhaseWrap)
            {
                _phase -= PhaseWrap;
                _lastStep -= (int)(PhaseWrap * 2f);
            }

            EnemyPose pose = EnemyMotionMath.SampleGait(profile, _phase, speedFactor, gallop);
            pose.Offset *= size;
            pose.Scale(alive);

            int step = EnemyMotionMath.StepIndex(_phase);
            if (step != _lastStep)
            {
                _lastStep = step;
                if (profile.Archetype == EnemyMotionArchetype.Heavy && speedFactor > 0.15f && !_dead)
                {
                    PublishFootstep(Mathf.Clamp01(0.4f + speedFactor * 0.6f));
                }
            }

            // Lean into acceleration / braking and bank into turns.
            float turnWeight = profile.Archetype == EnemyMotionArchetype.Flyer ? 1f : speedFactor;
            pose.Euler.x += Mathf.Clamp(_acceleration * 1.2f, -6f, 6f) * alive;
            pose.Euler.z += Mathf.Clamp(-_yawRate * 0.06f, -profile.BankDegrees, profile.BankDegrees) * turnWeight * alive;

            // --- Idle / alert ---
            EnemyPose idle = EnemyMotionMath.SampleIdle(profile, _time, _seed, speedFactor);
            idle.Scale(alive);
            pose.Add(idle);

            if (_alertTime >= 0f)
            {
                EnemyPose alert = EnemyMotionMath.SampleAlert(profile, _alertTime);
                alert.Offset *= size;
                pose.Add(alert);
                _alertTime += deltaTime;
                if (_alertTime >= EnemyMotionMath.AlertDuration)
                {
                    _alertTime = -1f;
                    if (profile.Archetype == EnemyMotionArchetype.Heavy && !_dead)
                    {
                        PublishFootstep(0.6f);
                    }
                }
            }

            // --- Attack ---
            pose.Add(TickAttack(profile, deltaTime));

            // --- Hit recoil ---
            if (_hitTime >= 0f)
            {
                pose.Add(EnemyMotionMath.SampleRecoil(profile, _hitDirection, _hitTime, _hitStrength));
                _hitTime += deltaTime;
                if (_hitTime >= profile.RecoilDuration)
                {
                    _hitTime = -1f;
                }
            }

            pose.Scale(_intensity);

            // --- Spawn rise (not scaled by intensity: it is a placement, not a flourish) ---
            if (_spawnTime >= 0f)
            {
                float rise = EnemyMotionMath.SpawnRise01(_spawnTime);
                float depth = _bodyExtents.y * 1.8f + profile.HoverHeight * size;
                pose.Offset.y -= (1f - rise) * depth;
                pose.Squash -= 0.15f * Mathf.Clamp01(1f - rise);
                _spawnTime += deltaTime;
                if (_spawnTime >= EnemyMotionMath.SpawnRiseDuration)
                {
                    _spawnTime = -1f;
                }
            }

            Apply(pose, profile, deathK);
        }

        private EnemyPose TickAttack(in EnemyMotionProfile profile, float deltaTime)
        {
            EnemyPose pose = default;
            if (_attackPhase != EnemyAttackVisualPhase.None)
            {
                _attackTime += deltaTime;
                pose = EnemyMotionMath.EvaluateAttack(profile, _attackPhase, _attackTime, _attackWindup);
                if (_attackPhase == EnemyAttackVisualPhase.Recover && _attackTime >= profile.RecoverDuration)
                {
                    _attackPhase = EnemyAttackVisualPhase.None;
                }
            }

            if (_cancelTime >= 0f)
            {
                EnemyPose fading = _cancelPose;
                fading.Scale(1f - EnemyMotionMath.SmoothStep01(_cancelTime / EnemyMotionMath.CancelBlendDuration));
                pose.Add(fading);
                _cancelTime += deltaTime;
                if (_cancelTime >= EnemyMotionMath.CancelBlendDuration)
                {
                    _cancelTime = -1f;
                }
            }

            return pose;
        }

        private void SyncWithController()
        {
            if (_controller == null)
            {
                return;
            }

            if (_controller.SpawnGeneration != _seenGeneration)
            {
                BeginLife();
            }

            EnemyMotionProfile profile = _controller.Motion;

            if (_controller.AlertSequence != _seenAlert)
            {
                _seenAlert = _controller.AlertSequence;
                _alertTime = 0f;
            }

            if (_controller.AttackSequence != _seenAttack)
            {
                _seenAttack = _controller.AttackSequence;
                _attackPhase = EnemyAttackVisualPhase.Windup;
                _attackTime = 0f;
                _attackWindup = Mathf.Max(0.05f, _controller.AttackWindupDuration);
                _cancelTime = -1f;
            }

            if (_controller.StrikeSequence != _seenStrike)
            {
                _seenStrike = _controller.StrikeSequence;
                _attackPhase = EnemyAttackVisualPhase.Recover;
                _attackTime = 0f;
                if (profile.Archetype == EnemyMotionArchetype.Heavy)
                {
                    PublishFootstep(1f);
                }
            }

            if (_controller.AttackCancelSequence != _seenCancel)
            {
                _seenCancel = _controller.AttackCancelSequence;
                StartCancelBlend(profile);
            }

            bool isDead = _health != null && _health.IsDead;
            if (isDead && !_dead)
            {
                _dead = true;
                _deathTime = 0f;
                _alertTime = -1f;
                StartCancelBlend(profile);
            }
            else if (!isDead && _dead)
            {
                BeginLife(); // revived without a ResetForSpawn (e.g. tests, healing a corpse)
            }
        }

        private void StartCancelBlend(in EnemyMotionProfile profile)
        {
            if (_attackPhase == EnemyAttackVisualPhase.None)
            {
                return;
            }

            _cancelPose = EnemyMotionMath.EvaluateAttack(profile, _attackPhase, _attackTime, _attackWindup);
            _cancelTime = 0f;
            _attackPhase = EnemyAttackVisualPhase.None;
        }

        private void Measure(float deltaTime)
        {
            Vector3 position = _root.position;
            Vector3 delta = position - _lastRootPosition;
            delta.y = 0f;
            _lastRootPosition = position;

            float raw = delta.magnitude / deltaTime;
            if (raw > _maxPlausibleSpeed || _dead)
            {
                raw = _dead ? 0f : _speed; // teleport (spawn/pool) or corpse: ignore
            }

            float yaw = _root.eulerAngles.y;
            float rawYawRate = Mathf.Clamp(Mathf.DeltaAngle(_lastYaw, yaw) / deltaTime, -720f, 720f);
            _lastYaw = yaw;

            float previous = _speed;
            _speed = Mathf.Lerp(_speed, raw, EnemyMotionMath.Damp(_speedSmoothing, deltaTime));
            _acceleration = Mathf.Lerp(_acceleration, (_speed - previous) / deltaTime, EnemyMotionMath.Damp(6f, deltaTime));
            _yawRate = Mathf.Lerp(_yawRate, rawYawRate, EnemyMotionMath.Damp(8f, deltaTime));
        }

        private void Apply(in EnemyPose pose, in EnemyMotionProfile profile, float deathK)
        {
            Quaternion r = Quaternion.Euler(pose.Euler);
            Vector3 position = _bodyCenter + r * (_restLocalPosition - _bodyCenter) + pose.Offset;
            Quaternion rotation = r * _restLocalRotation;

            float squash = Mathf.Clamp(pose.Squash, -0.3f, 0.3f);
            // Keep the feet planted while flattening/stretching about the visual's own pivot.
            position.y -= squash * (_restLocalPosition.y - _footHeight);

            if (_dead && deathK > 0f)
            {
                ApplyTopple(ref position, ref rotation, profile, deathK);
            }

            _visualRoot.SetLocalPositionAndRotation(position, rotation);

            Vector3 scale = _restLocalScale * (1f + 0.5f * squash);
            scale[_upAxis] = _restLocalScale[_upAxis] * (1f - squash);
            _visualRoot.localScale = scale;
        }

        private void ApplyTopple(ref Vector3 position, ref Quaternion rotation, in EnemyMotionProfile profile, float k)
        {
            float side = _deathPush.x >= 0f ? 1f : -1f;
            Quaternion topple;
            Vector3 pivot;
            float lift = 0f;

            switch (profile.Archetype)
            {
                case EnemyMotionArchetype.Arthropod:
                    // Flip belly-up with a little hop.
                    topple = Quaternion.AngleAxis(-side * 170f * k, Vector3.forward);
                    pivot = _bodyCenter;
                    lift = _bodyExtents.y * 0.6f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(_deathTime / EnemyMotionMath.ToppleDuration));
                    break;
                case EnemyMotionArchetype.Flyer:
                    // Wings stop: the hover gait fades with the topple (drops out of the air) while it rolls over.
                    topple = Quaternion.AngleAxis(-side * 110f * k, Vector3.forward);
                    pivot = _bodyCenter;
                    break;
                case EnemyMotionArchetype.Heavy:
                {
                    // Timber! Falls flat, pivoting on the edge it falls toward (away from the last hit).
                    float forward = _deathPush.z > 0f ? 1f : -1f;
                    topple = Quaternion.AngleAxis(forward * 80f * k, Vector3.right);
                    pivot = new Vector3(_bodyCenter.x, _footHeight, _bodyCenter.z + forward * _bodyExtents.z);
                    break;
                }
                default:
                    // Rolls onto its side over the edge it falls toward.
                    topple = Quaternion.AngleAxis(-side * 85f * k, Vector3.forward);
                    pivot = new Vector3(_bodyCenter.x + side * _bodyExtents.x, _footHeight, _bodyCenter.z);
                    break;
            }

            position = pivot + topple * (position - pivot);
            position.y += lift;
            rotation = topple * rotation;
        }

        private void PublishFootstep(float intensity)
        {
            Vector3 feet = _root.position;
            feet.y += _footHeight;
            EventBus.Publish(new EnemyFootstepEvent(_root.gameObject, feet, intensity));
        }
    }
}
