using System;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Locomotion;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Rig-free animation for enemy visuals. None of the enemy models has a skeleton (Wolf/Spider/Scorpion
    /// are static Meshy FBX, the rest are primitives — Meshy's rigging API only supports bipeds), so the
    /// whole body is posed procedurally on the visual root (the "EnemyVisual" child, never the collider
    /// root) as local offsets/rotations/squash on top of its rest pose. The feel comes from the archetype's
    /// <see cref="EnemyMotionProfile"/> scaled for the real body (<see cref="EnemyMotionProfile.ScaledForBody"/>:
    /// bigger or boss = heavier, slower springs and eases):
    ///
    ///  * gait per <see cref="EnemyMotionArchetype"/> driven by the root's measured planar speed
    ///    (displacement / dt, so slows, roots and collisions are honoured), eased in/out by a critically
    ///    damped spring: quadruped trot → gallop with pitch rock and stretch/squash; arthropod low
    ///    high-frequency skitter with yaw wiggle; flyer hover bob + flutter + banking; heavy stomping steps
    ///    (publishes <see cref="EnemyFootstepEvent"/> per footfall);
    ///  * inertia: lean and surge springs follow acceleration and turns (lean into starts, lean back to
    ///    brake, rear back and crouch braking into sharp turns, overshoot forward after a stop);
    ///  * behaviour poses: idle breathing + personality (wolf sniffs and looks around, arthropods twitch,
    ///    separate leg objects flick, bats drift and tilt, heavies heave), alert hop on aggro, telegraphed
    ///    attack timed by <see cref="EnemyController"/>'s counters (anticipation crouch with squash → stretched
    ///    lunge peaking exactly at the damage moment → follow-through → recovery; the gameplay windup is
    ///    only read, never changed), hit spring kick away from the attacker (<see cref="DamageAppliedEvent"/>),
    ///    death topple + impact bounce during the first moments of <see cref="EnemyDeathVfx"/>'s sink (that
    ///    moves the root, this only the visual — they compose), spawn emerge with overshoot.
    ///
    /// Squash is volume-preserving and capped at <see cref="MotionFeelMath.MaxSquash"/>. With the
    /// accessibility option "Reduzir Movimento" squash, overlap (spring overshoot), bob, bounce and
    /// overshoot are scaled by <see cref="MotionScale"/>; the attack telegraph and the topple stay readable.
    /// One LateUpdate per enemy, scaled time (frozen while paused), no per-frame allocations. The pure
    /// math lives in <see cref="EnemyMotionMath"/> (EditMode-tested).
    ///
    /// Models built from jointed parts (<c>Leg_*</c> → <c>Shin_*</c> → <c>Foot_*</c>, see
    /// <see cref="ProceduralLegLocomotion"/>) get real legs on top of this: planted feet stepping in gait
    /// order with knee IK, the body bob/roll/pitch taken from the footfalls instead of the sine gait, heavy
    /// stomps published per actual landing, and tail/ear/head/abdomen follow-through. Far from the camera
    /// (<c>_legsLodDistance</c>), off screen, while emerging or dead the legs blend back to the rest pose
    /// and the cheap whole-body gait takes over again.
    /// </summary>
    public class EnemyProceduralAnimator : MonoBehaviour
    {
        private const int MaxLegs = 8;

        [SerializeField] private Transform _visualRoot;

        [Tooltip("Global multiplier on every procedural pose (gait, idle, attack, recoil). 0 = rest pose only (topple/spawn still play).")]
        [Range(0f, 2f)]
        [SerializeField] private float _intensity = 1f;

        [Tooltip("How fast the measured speed follows the real displacement (higher = snappier gait changes).")]
        [SerializeField] private float _speedSmoothing = 10f;

        [Tooltip("Displacements faster than this (m/s) are teleports (spawn, pool reuse) and ignored.")]
        [SerializeField] private float _maxPlausibleSpeed = 25f;

        [Tooltip("Multiplier on squash, overlap, bob, bounces and overshoot with the accessibility option \"Reduzir Movimento\" on.")]
        [SerializeField, Range(0f, 1f)] private float _reducedMotionScale = AccessibilitySettings.ReducedMotionScale;

        [Tooltip("Step jointed legs (Leg_/Shin_/Foot_ parts) with planted feet and knee IK. Off = whole-body gait only.")]
        [SerializeField] private bool _proceduralLegs = true;

        [Tooltip("Beyond this distance (m) from the camera the legs rest and the cheap whole-body gait is used.")]
        [SerializeField] private float _legsLodDistance = 24f;

        [Tooltip("Ignore the distance/visibility LOD of the legs (cutscenes, photo mode, tests).")]
        [SerializeField] private bool _alwaysAnimateLegs;

        private const float LegsBlendTime = 0.25f;
        private const float LegsLodHysteresis = 3f;

        private const float PhaseWrap = 1000f;

        private Transform _root;
        private EnemyController _controller;
        private Health _health;
        private bool _initialized;
        private AccessibilitySettings _accessibility;
        private float _accessibilityRetry;

        // Rest pose (captured when the visual is assigned).
        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation;
        private Vector3 _restLocalScale = Vector3.one;
        private bool _hasRestPose;
        private Vector3 _bodyCenter;
        private Vector3 _bodyExtents = new Vector3(0.5f, 0.5f, 0.5f);
        private float _footHeight = -1f;
        private int _upAxis = 1;
        private readonly Transform[] _legs = new Transform[MaxLegs];
        private readonly Quaternion[] _legRest = new Quaternion[MaxLegs];
        private int _legCount;
        private ProceduralLegLocomotion _legRig;
        private ProceduralEnemyRig _partRig;
        private Renderer _bodyRenderer;
        private float _legWeight;
        private bool _legsLive;

        // Measurement.
        private Vector3 _lastRootPosition;
        private float _lastYaw;
        private float _speed;
        private Vector3 _velocity;
        private float _acceleration;
        private float _yawRate;

        // Eased gait speed and inertia springs.
        private float _easedSpeed;
        private float _easedSpeedVelocity;
        private Vector2 _lean;
        private Vector2 _leanVelocity;
        private float _surge;
        private float _surgeVelocity;

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

        // Behaviour timers (negative = inactive) and reaction springs.
        private EnemyAttackVisualPhase _attackPhase;
        private float _attackTime;
        private float _attackWindup;
        private EnemyPose _cancelPose;
        private float _cancelTime = -1f;
        private float _alertTime = -1f;
        private Vector2 _hitOffset;
        private Vector2 _hitOffsetVelocity;
        private Vector2 _hitTilt;
        private Vector2 _hitTiltVelocity;
        private float _squashSpring;
        private float _squashVelocity;
        private bool _pendingHit;
        private float _pendingHitStrength;
        private Vector3 _hitDirection = Vector3.back;
        private bool _dead;
        private float _deathTime;
        private Vector3 _deathPush = Vector3.back;
        private float _spawnTime = -1f;

        private Action<DamageAppliedEvent> _onDamageApplied;

        public Transform VisualRoot => _visualRoot;

        /// <summary>Smoothed measured planar speed (m/s) of the root.</summary>
        public float MeasuredSpeed => _speed;

        /// <summary>Gait speed (m/s) after the profile's ease (lags <see cref="MeasuredSpeed"/> on starts and stops; heavier = slower).</summary>
        public float EasedSpeed => _easedSpeed;

        /// <summary>Gait phase in cycles (continuous).</summary>
        public float GaitPhase => _phase;

        public EnemyAttackVisualPhase AttackPhase => _attackPhase;

        /// <summary>Separate leg objects found under the visual (named leg/perna/pata), twitched while idle (0 when they step instead).</summary>
        public int LegCount => _legCount;

        /// <summary>The stepping-legs driver on the current visual (null for single-mesh models).</summary>
        public ProceduralLegLocomotion LegRig => _legRig;

        /// <summary>0..1 blend of the stepping legs (0 = whole-body gait: far, off screen, emerging or dead).</summary>
        public float LegWeight => _legWeight;

        /// <summary>Skip the distance/visibility LOD of the legs (cutscenes, photo mode, tests).</summary>
        public bool AlwaysAnimateLegs
        {
            get => _alwaysAnimateLegs;
            set => _alwaysAnimateLegs = value;
        }

        /// <summary>1, or the reduced-motion scale while the accessibility "Reduzir Movimento" option is on.</summary>
        public float MotionScale => AccessibilitySettings.MotionScaleFor(_accessibility, _reducedMotionScale);

        /// <summary>The archetype profile scaled for this visual's real size (what the poses use).</summary>
        public EnemyMotionProfile CurrentProfile
        {
            get
            {
                EnemyMotionProfile profile = _controller != null ? _controller.Motion : EnemyMotionProfile.Quadruped;
                return profile.ScaledForBody(SizeScale(profile), _controller != null && _controller.IsBoss);
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            _onDamageApplied ??= OnDamageApplied;
            EventBus.Subscribe(_onDamageApplied);
            _accessibilityRetry = 0f;
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

            CaptureLegs();
            BindLegRig();
        }

        /// <summary>
        /// Binds (adding it once, on the visual) the stepping-legs driver when the visual has jointed legs or
        /// follow-through parts. Parts the model's own <see cref="ProceduralEnemyRig"/> already animates are
        /// left to it. Stepping legs replace the idle leg twitch. Allocates only here.
        /// </summary>
        private void BindLegRig()
        {
            _legWeight = 0f;
            _legsLive = false;
            _partRig = null;
            _bodyRenderer = null;
            _legRig = null;
            if (_visualRoot == null || _visualRoot == _root)
            {
                return;
            }

            _partRig = _visualRoot.GetComponent<ProceduralEnemyRig>();
            _bodyRenderer = _visualRoot.GetComponentInChildren<Renderer>();
            _legRig = _visualRoot.GetComponent<ProceduralLegLocomotion>();
            if (_legRig == null && HasLocomotionParts(_visualRoot))
            {
                _legRig = _visualRoot.gameObject.AddComponent<ProceduralLegLocomotion>();
            }

            if (_legRig == null)
            {
                return;
            }

            _legRig.SelfDriven = false;
            _legRig.Bind(_root, _visualRoot, _footHeight, _partRig != null ? ProceduralEnemyRig.PartsAnimatedBy(_partRig.Kind) : null);
            if (_legRig.HasLegs)
            {
                _legCount = 0;
            }
        }

        private static bool HasLocomotionParts(Transform visual)
        {
            foreach (Transform t in visual.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;
                if (n.StartsWith(ProceduralLegLocomotion.HipPrefix) || n == "Tail" || n == "Abdomen")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Separate leg objects (only if the model has them: today's Meshy enemies are one mesh). Allocates once, here.</summary>
        private void CaptureLegs()
        {
            _legCount = 0;
            if (_visualRoot == null || _visualRoot == _root)
            {
                return;
            }

            foreach (Transform child in _visualRoot.GetComponentsInChildren<Transform>(true))
            {
                if (_legCount >= MaxLegs)
                {
                    break;
                }

                if (child != _visualRoot && IsLegName(child.name))
                {
                    _legs[_legCount] = child;
                    _legRest[_legCount] = child.localRotation;
                    _legCount++;
                }
            }
        }

        /// <summary>Leg object naming (English/Portuguese): "leg", "perna", "pata".</summary>
        public static bool IsLegName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return false;
            }

            string s = objectName.ToLowerInvariant();
            return s.Contains("leg") || s.Contains("perna") || s.Contains("pata");
        }

        private void RestoreRestPose()
        {
            if (!_hasRestPose || _visualRoot == null || _visualRoot == _root)
            {
                return;
            }

            _visualRoot.SetLocalPositionAndRotation(_restLocalPosition, _restLocalRotation);
            _visualRoot.localScale = _restLocalScale;
            RestLegRig();
            for (int i = 0; i < _legCount; i++)
            {
                if (_legs[i] != null)
                {
                    _legs[i].localRotation = _legRest[i];
                }
            }
        }

        private void RestLegRig()
        {
            _legWeight = 0f;
            _legsLive = false;
            if (_legRig != null)
            {
                _legRig.RestoreRest();
            }
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
            _velocity = Vector3.zero;
            _acceleration = 0f;
            _yawRate = 0f;
            _easedSpeed = 0f;
            _easedSpeedVelocity = 0f;
            _lean = Vector2.zero;
            _leanVelocity = Vector2.zero;
            _surge = 0f;
            _surgeVelocity = 0f;
        }

        /// <summary>Fresh spawn / pool reuse: clear every reaction and play the emerge.</summary>
        private void BeginLife()
        {
            ResetMeasurement();
            _attackPhase = EnemyAttackVisualPhase.None;
            _cancelTime = -1f;
            _alertTime = -1f;
            _hitOffset = Vector2.zero;
            _hitOffsetVelocity = Vector2.zero;
            _hitTilt = Vector2.zero;
            _hitTiltVelocity = Vector2.zero;
            _squashSpring = 0f;
            _squashVelocity = 0f;
            _pendingHit = false;
            _dead = false;
            _deathTime = 0f;
            _spawnTime = 0f;
            RestLegRig(); // the feet are re-planted where the enemy actually stands once the emerge ends

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
            _pendingHitStrength = Mathf.Max(_pendingHit ? _pendingHitStrength : 0f, Mathf.Clamp(0.45f + 3f * evt.FinalDamage / maxHealth, 0.45f, 1f));
            _pendingHit = true; // kicked on the next Tick, with the profile scaled for this body
            if (!_dead)
            {
                _deathPush = _hitDirection;
            }
        }

        private float SizeScale(in EnemyMotionProfile profile)
        {
            return Mathf.Clamp(_bodyExtents.y * 2f / Mathf.Max(0.05f, profile.ReferenceHeight), 0.6f, 1.8f);
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

            ResolveAccessibility(deltaTime);
            SyncWithController();

            EnemyMotionProfile archetype = _controller != null ? _controller.Motion : EnemyMotionProfile.Quadruped;
            float size = SizeScale(archetype);
            EnemyMotionProfile profile = archetype.ScaledForBody(size, _controller != null && _controller.IsBoss);
            float motion = MotionScale;

            Measure(deltaTime);
            _time += deltaTime;
            if (_time > PhaseWrap)
            {
                _time -= PhaseWrap;
            }

            float previousDeathTime = _deathTime;
            if (_dead)
            {
                _deathTime += deltaTime;
            }

            float deathK = _dead ? EnemyMotionMath.Topple01(_deathTime) : 0f;
            float alive = 1f - Mathf.Clamp01(deathK);

            // --- Eased gait speed: ease-in on start, ease-out on stop (critically damped, heavier = slower) ---
            MotionFeelMath.StepSpring(ref _easedSpeed, ref _easedSpeedVelocity, _speed, MotionFeelMath.EaseFrequency(profile.GaitEaseTime), 1f, deltaTime);
            float speed = Mathf.Max(0f, _easedSpeed);

            // --- Gait ---
            float speedFactor = EnemyMotionMath.SpeedFactor(profile, speed);
            float gallop = EnemyMotionMath.GallopBlend(profile, speed);
            _phase += EnemyMotionMath.StrideFrequency(profile, speed, size) * deltaTime * alive;
            if (_phase > PhaseWrap)
            {
                _phase -= PhaseWrap;
                _lastStep -= (int)(PhaseWrap * 2f);
            }

            EnemyPose pose = EnemyMotionMath.SampleGait(profile, _phase, speedFactor, gallop);
            float hover = profile.Archetype == EnemyMotionArchetype.Flyer ? profile.HoverHeight : 0f;
            pose.Offset.y = hover + (pose.Offset.y - hover) * motion; // bob reduced, hover height kept
            pose.Squash *= motion;
            pose.Offset *= size;

            // --- Stepping legs: the body rides its footfalls instead of the sine gait ---
            if (AdvanceLegs(profile, deltaTime, size, gallop, motion))
            {
                EnemyPose stepped = default;
                stepped.Offset.y = _legRig.BodyBob + profile.MoveCrouch * speedFactor * size - EnemyLegGait.KneeBendDrop * _legRig.MeanLegLength;
                stepped.Euler.x = _legRig.BodySway.x;
                stepped.Euler.y = pose.Euler.y * 0.35f;
                stepped.Euler.z = _legRig.BodySway.y;
                pose.Scale(1f - _legWeight);
                stepped.Scale(_legWeight);
                pose.Add(stepped);
            }

            pose.Scale(alive);

            int step = EnemyMotionMath.StepIndex(_phase);
            if (step != _lastStep)
            {
                _lastStep = step;
                if (profile.Archetype == EnemyMotionArchetype.Heavy && speedFactor > 0.15f && !_dead && _legWeight < 0.5f)
                {
                    PublishFootstep(Mathf.Clamp01(0.4f + speedFactor * 0.6f));
                }
            }

            if (_legsLive && _legRig.Landings > 0 && profile.Archetype == EnemyMotionArchetype.Heavy && !_dead && _legWeight >= 0.5f)
            {
                // A stomp per real footfall, where the foot came down; the body settles into it.
                float strength = _legRig.LandingStrength;
                PublishFootstep(_legRig.LandingPoint, Mathf.Clamp01(0.35f + 0.65f * Mathf.Max(strength, speedFactor)));
                KickSquash(profile, 0.02f * strength * motion);
            }

            // --- Inertia: lean into starts, lean back to brake, bank into turns; springs add the follow-through ---
            Vector2 leanTarget = EnemyMotionMath.LeanTarget(profile, _acceleration, _yawRate, speedFactor) * alive;
            MotionFeelMath.StepSpring(ref _lean, ref _leanVelocity, leanTarget, profile.LeanSpringHz, profile.LeanSpringDamping, deltaTime, Vector2.zero);
            float surgeTarget = EnemyMotionMath.SurgeTarget(profile, _acceleration) * size * alive;
            MotionFeelMath.StepSpring(ref _surge, ref _surgeVelocity, surgeTarget, profile.LeanSpringHz, profile.LeanSpringDamping, deltaTime);
            Vector2 lean = leanTarget + (_lean - leanTarget) * motion; // overlap (overshoot) reduced, the lean itself kept
            pose.Euler.x += lean.x;
            pose.Euler.z += lean.y;
            pose.Offset.z += (surgeTarget + (_surge - surgeTarget) * motion);

            float brake = EnemyMotionMath.TurnBrake01(profile, _yawRate, speedFactor) * alive;
            pose.Squash += 0.03f * brake * motion; // plants and crouches into the pivot
            pose.Offset.y -= 0.02f * brake * size;

            // --- Idle / alert ---
            EnemyPose idle = EnemyMotionMath.SampleIdle(profile, _time, _seed, speedFactor);
            idle.Offset *= size * motion;
            idle.Squash *= motion;
            idle.Scale(alive);
            pose.Add(idle);

            if (_alertTime >= 0f)
            {
                EnemyPose alert = EnemyMotionMath.SampleAlert(profile, _alertTime);
                alert.Offset *= size;
                alert.Squash *= motion;
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

            // --- Attack (the telegraph itself is gameplay information: only its squash is reduced) ---
            EnemyPose attack = TickAttack(profile, deltaTime);
            attack.Squash *= motion;
            pose.Add(attack);

            // --- Hit spring kick, squash wobble ---
            TickReactions(profile, deltaTime, previousDeathTime);
            pose.Offset += new Vector3(_hitOffset.x, 0f, _hitOffset.y) * motion;
            pose.Euler.x += _hitTilt.x * motion;
            pose.Euler.z += _hitTilt.y * motion;
            pose.Squash += _squashSpring * motion;

            pose.Scale(_intensity);

            // --- Spawn emerge (not scaled by intensity: it is a placement, not a flourish) ---
            if (_spawnTime >= 0f)
            {
                float rise = EnemyMotionMath.SpawnRise01(_spawnTime, profile.SpawnOvershoot * motion);
                float depth = _bodyExtents.y * 1.8f + profile.HoverHeight * size;
                pose.Offset.y -= (1f - rise) * depth;
                // Stretched while pushing out of the ground...
                pose.Squash -= MotionFeelMath.MaxSquash * Mathf.Sin(Mathf.PI * Mathf.Clamp01(_spawnTime / EnemyMotionMath.SpawnRiseDuration)) * motion;
                _spawnTime += deltaTime;
                if (_spawnTime >= EnemyMotionMath.SpawnRiseDuration)
                {
                    _spawnTime = -1f;
                    KickSquash(profile, 0.04f); // ...and a squash wobble as the weight settles
                }
            }

            Apply(pose, profile, deathK, size, motion);
            SolveLegs(deltaTime, alive, motion);
            ApplyLegs(profile, speedFactor, alive, motion);
            if (_partRig != null)
            {
                _partRig.MoveSpeed01 = speedFactor * alive;
            }
        }

        /// <summary>
        /// LOD + blend of the stepping legs, then the feet step for this frame (before the body is posed).
        /// Returns true while the legs contribute to the body pose (weight above 0).
        /// </summary>
        private bool AdvanceLegs(in EnemyMotionProfile profile, float deltaTime, float size, float gallop, float motion)
        {
            if (_legRig == null)
            {
                return false;
            }

            bool wanted = WantLegs();
            if (wanted && !_legsLive)
            {
                _legsLive = true;
                _legRig.ResetState(); // wake-up (emerge ended, back in range): plant the feet where the enemy stands
            }

            _legWeight = Mathf.MoveTowards(_legWeight, wanted ? 1f : 0f, deltaTime / LegsBlendTime);
            if (!_legsLive)
            {
                return false;
            }

            if (_legRig.HasLegs)
            {
                _legRig.Settings = EnemyLegGait.SettingsFor(profile, _legRig.DetectedStyle, _legRig.MeanLegLength, size, gallop);
            }

            _legRig.Advance(deltaTime, _dead ? Vector3.zero : _velocity, _dead ? 0f : _yawRate, motion);
            return _legRig.HasLegs && _legWeight > 0f;
        }

        private void SolveLegs(float deltaTime, float alive, float motion)
        {
            if (!_legsLive)
            {
                return;
            }

            _legRig.Solve(deltaTime, _legWeight * alive, motion * Mathf.Clamp01(_intensity));
            if (_legWeight <= 0f && !WantLegs())
            {
                RestLegRig(); // fully faded out: back to the cheap path, legs at rest
            }
        }

        /// <summary>Legs step only near the camera, on screen, alive and out of the ground (hysteresis on the distance).</summary>
        private bool WantLegs()
        {
            if (!_proceduralLegs || _legRig == null || _dead || _spawnTime >= 0f || _intensity <= 0f)
            {
                return false;
            }

            if (_alwaysAnimateLegs)
            {
                return true;
            }

            float lod = _legsLive ? _legsLodDistance + LegsLodHysteresis : _legsLodDistance;
            if (ProceduralLegLocomotion.SqrDistanceToCamera(_root.position) > lod * lod)
            {
                return false;
            }

            return _bodyRenderer == null || _bodyRenderer.isVisible;
        }

        private void TickReactions(in EnemyMotionProfile profile, float deltaTime, float previousDeathTime)
        {
            if (_pendingHit)
            {
                _pendingHit = false;
                EnemyHitKick kick = EnemyMotionMath.HitKick(profile, _hitDirection, _pendingHitStrength);
                _hitOffsetVelocity += kick.OffsetVelocity;
                _hitTiltVelocity += kick.TiltVelocity;
                _squashVelocity += kick.SquashVelocity;
            }

            if (_dead)
            {
                // Impact: the body hits the ground at the end of the topple (squash), and again after the first bounce.
                float impact = EnemyMotionMath.ToppleDuration;
                float second = impact + EnemyMotionMath.FirstBounceDuration;
                if (previousDeathTime < impact && _deathTime >= impact)
                {
                    KickSquash(profile, MotionFeelMath.MaxSquash);
                }
                else if (previousDeathTime < second && _deathTime >= second)
                {
                    KickSquash(profile, MotionFeelMath.MaxSquash * EnemyMotionMath.SecondBounceHeight);
                }
            }

            MotionFeelMath.StepSpring(ref _hitOffset, ref _hitOffsetVelocity, Vector2.zero, profile.HitSpringHz, profile.HitSpringDamping, deltaTime, Vector2.zero);
            MotionFeelMath.StepSpring(ref _hitTilt, ref _hitTiltVelocity, Vector2.zero, profile.HitSpringHz, profile.HitSpringDamping, deltaTime, Vector2.zero);
            MotionFeelMath.StepSpring(ref _squashSpring, ref _squashVelocity, 0f, profile.SquashSpringHz, profile.SquashSpringDamping, deltaTime);
        }

        private void KickSquash(in EnemyMotionProfile profile, float peak)
        {
            _squashVelocity += MotionFeelMath.SpringVelocityForPeak(peak, profile.SquashSpringHz, profile.SquashSpringDamping);
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

            EnemyMotionProfile profile = CurrentProfile;

            if (_controller.AlertSequence != _seenAlert)
            {
                _seenAlert = _controller.AlertSequence;
                _alertTime = 0f;
            }

            if (_controller.AttackSequence != _seenAttack)
            {
                _seenAttack = _controller.AttackSequence;
                // A new windup while the last recovery still plays: fade that out instead of popping to the coil.
                StartCancelBlend(profile);
                _attackPhase = EnemyAttackVisualPhase.Windup;
                _attackTime = 0f;
                // The gameplay windup, read as-is: the lunge peaks exactly when EnemyAttackState deals damage.
                _attackWindup = Mathf.Max(0.05f, _controller.AttackWindupDuration);
                EventBus.Publish(new EnemyAttackWindupEvent(_root.gameObject, _root.position, _attackWindup));
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

        private void ResolveAccessibility(float deltaTime)
        {
            if (_accessibility != null)
            {
                return;
            }

            // Not registered yet (boot order, tests): look again about once a second, never every frame.
            _accessibilityRetry -= deltaTime;
            if (_accessibilityRetry <= 0f)
            {
                _accessibilityRetry = 1f;
                ServiceLocator.TryGet(out _accessibility);
            }
        }

        private void Measure(float deltaTime)
        {
            Vector3 position = _root.position;
            Vector3 delta = position - _lastRootPosition;
            delta.y = 0f;
            _lastRootPosition = position;

            float raw = delta.magnitude / deltaTime;
            Vector3 rawVelocity = delta / deltaTime;
            if (raw > _maxPlausibleSpeed || _dead)
            {
                raw = _dead ? 0f : _speed; // teleport (spawn/pool) or corpse: ignore
                rawVelocity = _dead ? Vector3.zero : _velocity;
            }

            _velocity = Vector3.Lerp(_velocity, rawVelocity, EnemyMotionMath.Damp(_speedSmoothing, deltaTime));

            float yaw = _root.eulerAngles.y;
            float rawYawRate = Mathf.Clamp(Mathf.DeltaAngle(_lastYaw, yaw) / deltaTime, -720f, 720f);
            _lastYaw = yaw;

            float previous = _speed;
            _speed = Mathf.Lerp(_speed, raw, EnemyMotionMath.Damp(_speedSmoothing, deltaTime));
            _acceleration = Mathf.Lerp(_acceleration, (_speed - previous) / deltaTime, EnemyMotionMath.Damp(6f, deltaTime));
            _yawRate = Mathf.Lerp(_yawRate, rawYawRate, EnemyMotionMath.Damp(8f, deltaTime));
        }

        private void Apply(in EnemyPose pose, in EnemyMotionProfile profile, float deathK, float size, float motion)
        {
            Quaternion r = Quaternion.Euler(pose.Euler);
            Vector3 position = _bodyCenter + r * (_restLocalPosition - _bodyCenter) + pose.Offset;
            Quaternion rotation = r * _restLocalRotation;

            // Volume-preserving squash (+ = flatten), capped like the heroes'; the feet stay planted.
            Vector3 squash = MotionFeelMath.SquashScale(-pose.Squash);
            float stretch = squash.y - 1f;
            position.y += stretch * (_restLocalPosition.y - _footHeight);

            if (_dead && deathK > 0f)
            {
                ApplyTopple(ref position, ref rotation, profile, deathK);
                position.y += EnemyMotionMath.ImpactBounce01(_deathTime - EnemyMotionMath.ToppleDuration) * profile.DeathBounce * size * motion;
            }

            _visualRoot.SetLocalPositionAndRotation(position, rotation);

            Vector3 scale = _restLocalScale * squash.x;
            scale[_upAxis] = _restLocalScale[_upAxis] * squash.y;
            _visualRoot.localScale = scale;
        }

        /// <summary>Separate legs: flick on their own rhythm while idle, scurry with the gait while moving (arthropods only).</summary>
        private void ApplyLegs(in EnemyMotionProfile profile, float speedFactor, float alive, float motion)
        {
            if (_legCount == 0 || profile.Archetype != EnemyMotionArchetype.Arthropod || _legsLive)
            {
                return;
            }

            float rest = 1f - speedFactor;
            float k = alive * _intensity * motion;
            for (int i = 0; i < _legCount; i++)
            {
                Transform leg = _legs[i];
                if (leg == null)
                {
                    continue;
                }

                float twitch = EnemyMotionMath.LegTwitch(_time, i, _seed) * profile.IdleFidgetDegrees * 2f * rest;
                float scurry = Mathf.Sin((_phase * 2f + i * 0.5f) * Mathf.PI * 2f) * 15f * speedFactor;
                leg.localRotation = _legRest[i] * Quaternion.AngleAxis((twitch + scurry) * k, Vector3.right);
            }
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
            PublishFootstep(feet, intensity);
        }

        private void PublishFootstep(Vector3 position, float intensity)
        {
            EventBus.Publish(new EnemyFootstepEvent(_root.gameObject, position, intensity));
        }
    }
}
