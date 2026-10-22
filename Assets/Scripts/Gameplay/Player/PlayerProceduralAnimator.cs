using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Code-driven animation layers for the hero visual, composed every LateUpdate (after the
    /// Animator) on top of whatever the Animator produced. Everything is additive, short and
    /// interruptible, and uses scaled time (pausing freezes it). Two modes, picked automatically
    /// per visual (re-detected on <see cref="SetVisualRoot"/>):
    ///
    /// <para><b>Rig mode</b> (the Meshy-rigged heroes: Humanoid avatar bones, or Meshy/Mixamo bone
    /// names Hips/Spine02/Spine01/Spine/neck/Head/LeftArm/...): the Walk/Run clips own the legs; this
    /// adds on the spine/neck/head/arm bones:</para>
    /// <list type="bullet">
    /// <item>lean into acceleration (pitch) and into turns (roll), from the real motor velocity;</item>
    /// <item>head look toward the pursued attack target, else leading the movement direction;</item>
    /// <item>idle breathing / weight shift on the chest (there is no idle clip);</item>
    /// <item>melee strike (arm whip + torso twist, alternating arms) on <see cref="PlayerAttackPerformedEvent"/>;</item>
    /// <item>cast pose (both arms raise, chest arches) on <see cref="SkillCastEvent"/>;</item>
    /// <item>hit flinch (+ emissive flash) on <see cref="DamageAppliedEvent"/>.</item>
    /// </list>
    ///
    /// <para><b>Rigid mode</b> (no rig — Maithe's static model, the placeholder capsule): the same
    /// cues as whole-body motion on <see cref="_visualRoot"/>: stride-matched step bob, sway, lean,
    /// squash-stretch on footfalls, breathing, lunge, cast hop, recoil.</para>
    ///
    /// <para>Both modes: death collapse (fall around the feet + sink, no ragdoll) while
    /// <see cref="PlayerStats.IsDead"/>, blending back out on revive. Whole-body rotations pivot
    /// at the player's feet, so models with offset pivots don't swing around.</para>
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class PlayerProceduralAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Cosmetic child transform to animate. Never the Player root (that's owned by CharacterController).")]
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Renderer _renderer;

        [Header("Locomotion (rigid mode)")]
        [SerializeField] private float _bobAmplitude = 0.06f;
        [SerializeField] private float _leanDegrees = 8f;
        [SerializeField] private float _poseSmoothing = 12f;
        [Tooltip("Metres per step of the rigid step bob (cadence = speed / step length, so it never 'skates').")]
        [SerializeField, Min(0.1f)] private float _stepLength = 0.6f;
        [SerializeField, Range(0f, 0.2f)] private float _footfallSquash = 0.05f;
        [SerializeField] private float _swayDegrees = 3f;

        [Header("Lean (both modes)")]
        [SerializeField] private float _accelLeanGain = 1.2f;
        [SerializeField] private float _turnLeanGain = 0.8f;
        [SerializeField] private float _maxLeanPitch = 12f;
        [SerializeField] private float _maxLeanRoll = 10f;
        [Tooltip("Extra forward lean (deg) at run speed (rig mode).")]
        [SerializeField] private float _runLeanDegrees = 4f;

        [Header("Idle (no idle clip yet)")]
        [SerializeField] private float _breathDegrees = 1.6f;
        [SerializeField] private float _breathRate = 0.28f;
        [SerializeField] private float _breathScale = 0.012f;
        [SerializeField] private float _weightShiftDegrees = 1.2f;

        [Header("Head Look (rig mode)")]
        [SerializeField] private float _headLookMaxYaw = 65f;
        [SerializeField] private float _headLookSharpness = 8f;
        [SerializeField] private float _headLookTargetRange = 14f;

        [Header("Attack Swing")]
        [SerializeField] private float _attackLungeDistance = 0.3f;
        [SerializeField] private float _attackSquash = 0.15f;
        [SerializeField] private float _attackDuration = 0.18f;
        [SerializeField] private AnimationCurve _attackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("Rig mode strike length (s): whip, chop, recover. Shorter than the 0.5 s attack cooldown.")]
        [SerializeField, Min(0.05f)] private float _strikeDuration = 0.36f;
        [SerializeField] private float _strikeArmDegrees = 115f;
        [SerializeField] private float _strikeTwistDegrees = 22f;

        [Header("Skill Cast")]
        [SerializeField] private float _castArmDegrees = 100f;
        [SerializeField] private float _castArchDegrees = 7f;
        [SerializeField] private float _castRise = 0.1f;
        [SerializeField] private float _castHold = 0.16f;
        [SerializeField] private float _castRelease = 0.26f;
        [Tooltip("Rigid mode hop height (m) on cast.")]
        [SerializeField] private float _castHop = 0.08f;

        [Header("Hit Reaction")]
        [SerializeField] private float _hitShakeMagnitude = 0.1f;
        [SerializeField] private float _hitShakeDuration = 0.25f;
        [SerializeField] private float _hitFlashDuration = 0.2f;
        [SerializeField] private Color _hitFlashColor = new Color(1f, 0.15f, 0.15f);
        [SerializeField] private float _flinchDegrees = 11f;
        [SerializeField] private float _flinchDuration = 0.28f;

        [Header("Death (no ragdoll)")]
        [SerializeField] private float _deathFallDegrees = 80f;
        [SerializeField, Min(0.05f)] private float _deathFallDuration = 0.6f;
        [SerializeField] private float _deathSinkDelay = 1.1f;
        [SerializeField] private float _deathSinkDepth = 0.25f;
        [SerializeField, Min(0.05f)] private float _deathSinkDuration = 1.4f;
        [SerializeField, Min(0.05f)] private float _reviveBlendTime = 0.3f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        /// <summary>One bone the rig layers write additively; remembers what it wrote to detect an Animator that didn't overwrite it (no accumulation).</summary>
        private sealed class Bone
        {
            public Transform Transform;
            public Quaternion LastBase;
            public Quaternion LastWritten;
            public bool HasWritten;
        }

        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation;
        private Vector3 _restLocalScale;

        private Bone _spineLow;
        private Bone _chest;
        private Bone _upperChest;
        private Bone _neck;
        private Bone _head;
        private Bone _leftArm;
        private Bone _rightArm;
        private Bone _leftForeArm;
        private Bone _rightForeArm;
        private Bone[] _bones = new Bone[0];
        private bool _rigMode;

        private Renderer[] _renderers = new Renderer[0];
        private bool _flashApplied;
        private MaterialPropertyBlock _propertyBlock;

        private PlayerStats _stats;
        private PlayerClickToMove _clickToMove;

        // Kinematics
        private float _prevSpeed;
        private float _prevYaw;
        private bool _hasPrevKinematics;
        private float _accelSmoothed;
        private float _yawRateSmoothed;
        private float _leanPitch;
        private float _leanRoll;
        private float _currentSpeedFactor;
        private float _bobPhase;
        private float _idleClock;
        private float _headYaw;

        // Timers (-1 = inactive; count up from 0)
        private float _attackTime = -1f;
        private float _castTime = -1f;
        private float _hitTime = -1f;
        private float _hitFlashTimer = -1f;
        private bool _strikeLeft;
        private Vector3 _hitShakeSeed;
        private float _flinchRollSign = 1f;

        // Death
        private bool _wasDead;
        private float _deathTime = -1f;
        private float _deathFall;
        private float _deathSink;

        /// <summary>True when bone layers are active (a rig was found on the visual).</summary>
        public bool IsRigMode => _rigMode;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _stats = GetComponent<PlayerStats>();
            _clickToMove = GetComponent<PlayerClickToMove>();

            if (_visualRoot == null)
            {
                Debug.LogWarning($"[PlayerProceduralAnimator] No visual root assigned on {name}; procedural animation disabled.", this);
                enabled = false;
                return;
            }

            if (_motor == null)
            {
                _motor = GetComponentInParent<PlayerMotor>();
            }

            CaptureVisual(_visualRoot);
        }

        /// <summary>Re-targets which transform receives the procedural motion — used by <see cref="PlayerVisualSwapper"/> after swapping the visual model. Recaptures the rest pose and re-detects the rig.</summary>
        public void SetVisualRoot(Transform visualRoot)
        {
            ClearFlash();

            // Leave the previous root (e.g. the scene's PlayerVisual container) at its rest pose,
            // not frozen mid-lean/shake.
            if (_visualRoot != null && _visualRoot != visualRoot)
            {
                _visualRoot.localPosition = _restLocalPosition;
                _visualRoot.localRotation = _restLocalRotation;
                _visualRoot.localScale = _restLocalScale;
            }

            _visualRoot = visualRoot;
            CaptureVisual(visualRoot);

            if (visualRoot != null && !enabled)
            {
                enabled = true;
            }
        }

        private void CaptureVisual(Transform visualRoot)
        {
            _bones = new Bone[0];
            _rigMode = false;
            _renderers = new Renderer[0];

            if (visualRoot == null)
            {
                return;
            }

            _restLocalPosition = visualRoot.localPosition;
            _restLocalRotation = visualRoot.localRotation;
            _restLocalScale = visualRoot.localScale;

            _renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (_renderers.Length == 0 && _renderer != null)
            {
                _renderers = new[] { _renderer };
            }

            FindBones(visualRoot);
        }

        private void FindBones(Transform root)
        {
            Animator animator = root.GetComponentInChildren<Animator>();
            bool human = animator != null && animator.isHuman && animator.avatar != null;

            _spineLow = MakeBone(root, animator, human, HumanBodyBones.Spine, "Spine02", "Spine", "mixamorig:Spine");
            _chest = MakeBone(root, animator, human, HumanBodyBones.Chest, "Spine01", "Spine1", "mixamorig:Spine1");
            _upperChest = MakeBone(root, animator, human, HumanBodyBones.UpperChest, "Spine2", "mixamorig:Spine2");
            if (!human && _upperChest == null)
            {
                // Meshy order is Hips → Spine02 → Spine01 → Spine → neck: "Spine" is the top segment.
                Transform top = FindDeep(root, "Spine");
                if (top != null && (_spineLow == null || top != _spineLow.Transform))
                {
                    _upperChest = NewBone(top);
                }
            }

            _neck = MakeBone(root, animator, human, HumanBodyBones.Neck, "neck", "Neck", "mixamorig:Neck");
            _head = MakeBone(root, animator, human, HumanBodyBones.Head, "Head", "mixamorig:Head");
            _leftArm = MakeBone(root, animator, human, HumanBodyBones.LeftUpperArm, "LeftArm", "mixamorig:LeftArm");
            _rightArm = MakeBone(root, animator, human, HumanBodyBones.RightUpperArm, "RightArm", "mixamorig:RightArm");
            _leftForeArm = MakeBone(root, animator, human, HumanBodyBones.LeftLowerArm, "LeftForeArm", "mixamorig:LeftForeArm");
            _rightForeArm = MakeBone(root, animator, human, HumanBodyBones.RightLowerArm, "RightForeArm", "mixamorig:RightForeArm");

            // Parent-before-child order: deltas are applied in world space, so children inherit them.
            var list = new System.Collections.Generic.List<Bone>();
            foreach (Bone bone in new[] { _spineLow, _chest, _upperChest, _neck, _head, _leftArm, _rightArm, _leftForeArm, _rightForeArm })
            {
                if (bone != null && !list.Exists(b => b.Transform == bone.Transform))
                {
                    list.Add(bone);
                }
            }

            _bones = list.ToArray();
            _rigMode = (_spineLow != null || _chest != null) && (_head != null || _neck != null);
        }

        private static Bone MakeBone(Transform root, Animator animator, bool human, HumanBodyBones humanBone, params string[] names)
        {
            if (human)
            {
                Transform mapped = animator.GetBoneTransform(humanBone);
                if (mapped != null)
                {
                    return NewBone(mapped);
                }
            }

            foreach (string boneName in names)
            {
                Transform found = FindDeep(root, boneName);
                if (found != null)
                {
                    return NewBone(found);
                }
            }

            return null;
        }

        private static Bone NewBone(Transform t)
        {
            return new Bone { Transform = t, LastBase = t.localRotation, LastWritten = t.localRotation };
        }

        private static Transform FindDeep(Transform root, string boneName)
        {
            if (string.Equals(root.name, boneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), boneName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            ClearFlash();
        }

        private bool IsDead => _stats != null && _stats.IsDead;

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (evt.Target != gameObject || IsDead)
            {
                return;
            }

            _hitTime = 0f;
            _hitFlashTimer = _hitFlashDuration;
            _hitShakeSeed = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.3f, 0.3f), Random.Range(-1f, 1f)).normalized;
            _flinchRollSign = Random.value < 0.5f ? -1f : 1f;
        }

        private void OnAttackPerformed(PlayerAttackPerformedEvent evt)
        {
            if (evt.Attacker != gameObject || IsDead)
            {
                return;
            }

            _attackTime = 0f;
            _strikeLeft = !_strikeLeft;
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt.Caster != gameObject || IsDead)
            {
                return;
            }

            _castTime = 0f;
        }

        private void LateUpdate()
        {
            if (_visualRoot == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            TickKinematics(deltaTime);
            TickTimers(deltaTime);
            TickDeath(deltaTime);

            if (_rigMode)
            {
                ApplyBoneLayers();
            }

            ComposeVisualPose();
            ApplyHitFlash();
        }

        // ------------------------------------------------------------------
        // Ticking
        // ------------------------------------------------------------------

        private void TickKinematics(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            float speed = _motor != null ? _motor.HorizontalVelocity.magnitude : 0f;
            float yaw = transform.eulerAngles.y;

            if (!_hasPrevKinematics)
            {
                _prevSpeed = speed;
                _prevYaw = yaw;
                _hasPrevKinematics = true;
            }

            float accel = (speed - _prevSpeed) / deltaTime;
            float yawRate = Mathf.DeltaAngle(_prevYaw, yaw) / deltaTime;
            _prevSpeed = speed;
            _prevYaw = yaw;

            float k = LocomotionMath.ExpBlend(10f, deltaTime);
            _accelSmoothed = Mathf.Lerp(_accelSmoothed, accel, k);
            _yawRateSmoothed = Mathf.Lerp(_yawRateSmoothed, yawRate, k);

            Vector2 lean = LocomotionMath.LeanAngles(_accelSmoothed, _yawRateSmoothed, speed, _accelLeanGain, _turnLeanGain, _maxLeanPitch, _maxLeanRoll);
            float poseK = LocomotionMath.ExpBlend(_poseSmoothing, deltaTime);
            _leanPitch = Mathf.Lerp(_leanPitch, lean.x, poseK);
            _leanRoll = Mathf.Lerp(_leanRoll, lean.y, poseK);

            float runSpeed = _motor != null && _motor.RunSpeed > 0f ? _motor.RunSpeed : 5.5f;
            _currentSpeedFactor = Mathf.Lerp(_currentSpeedFactor, Mathf.Clamp01(speed / runSpeed), poseK);

            bool isMoving = _motor != null && _motor.IsGrounded && speed > 0.15f;
            if (isMoving)
            {
                _bobPhase += LocomotionMath.StepPhaseAdvance(speed, _stepLength, deltaTime);
            }
            else
            {
                // Ease toward the nearest rest point instead of snapping mid-stride.
                _bobPhase = Mathf.Lerp(_bobPhase, Mathf.Round(_bobPhase / Mathf.PI) * Mathf.PI, poseK);
            }

            _idleClock += deltaTime;
            TickHeadLook(deltaTime);
        }

        private void TickHeadLook(float deltaTime)
        {
            float targetYaw = 0f;
            Vector3 lookDirection = Vector3.zero;

            ICombatTarget target = _clickToMove != null ? _clickToMove.AttackTarget : null;
            if (target != null && !(target is Object unityObject && unityObject == null) && target.IsAlive)
            {
                Vector3 toTarget = target.Transform.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude < _headLookTargetRange * _headLookTargetRange)
                {
                    lookDirection = toTarget;
                }
            }

            if (lookDirection.sqrMagnitude < 1e-4f && _motor != null)
            {
                lookDirection = _motor.DesiredDirection;
            }

            if (lookDirection.sqrMagnitude > 1e-4f)
            {
                targetYaw = Mathf.Clamp(Vector3.SignedAngle(transform.forward, lookDirection, Vector3.up), -_headLookMaxYaw, _headLookMaxYaw);
            }

            _headYaw = Mathf.Lerp(_headYaw, targetYaw, LocomotionMath.ExpBlend(_headLookSharpness, deltaTime));
        }

        private void TickTimers(float deltaTime)
        {
            float strikeLength = _rigMode ? _strikeDuration : _attackDuration;
            Advance(ref _attackTime, deltaTime, strikeLength);
            Advance(ref _castTime, deltaTime, LocomotionMath.EnvelopeLength(_castRise, _castHold, _castRelease));
            Advance(ref _hitTime, deltaTime, Mathf.Max(_flinchDuration, _hitShakeDuration));

            if (_hitFlashTimer >= 0f)
            {
                _hitFlashTimer -= deltaTime;
                if (_hitFlashTimer < 0f)
                {
                    _hitFlashTimer = -1f;
                }
            }
        }

        private static void Advance(ref float timer, float deltaTime, float length)
        {
            if (timer < 0f)
            {
                return;
            }

            timer += deltaTime;
            if (timer > length)
            {
                timer = -1f;
            }
        }

        private void TickDeath(float deltaTime)
        {
            bool dead = IsDead;

            if (dead && !_wasDead)
            {
                _deathTime = 0f;
                _attackTime = _castTime = -1f;
            }

            _wasDead = dead;

            if (dead)
            {
                _deathTime += deltaTime;
                _deathFall = Mathf.Max(_deathFall, LocomotionMath.FallProgress(_deathTime, _deathFallDuration));
                _deathSink = Mathf.Clamp01((_deathTime - _deathSinkDelay) / _deathSinkDuration);
                _deathSink = Mathf.SmoothStep(0f, 1f, _deathSink);
            }
            else
            {
                _deathTime = -1f;
                float step = deltaTime / _reviveBlendTime;
                _deathFall = Mathf.MoveTowards(_deathFall, 0f, step);
                _deathSink = Mathf.MoveTowards(_deathSink, 0f, step);
            }
        }

        // ------------------------------------------------------------------
        // Rig layers
        // ------------------------------------------------------------------

        private void ApplyBoneLayers()
        {
            // Restore each bone's animated base (or last base when the Animator didn't write it).
            foreach (Bone bone in _bones)
            {
                if (bone.Transform == null)
                {
                    continue;
                }

                Quaternion current = bone.Transform.localRotation;
                Quaternion baseRotation = bone.HasWritten && current == bone.LastWritten ? bone.LastBase : current;
                bone.LastBase = baseRotation;
                bone.Transform.localRotation = baseRotation;
            }

            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            Vector3 up = Vector3.up;

            float idle = 1f - _currentSpeedFactor;
            float breath = Mathf.Sin(_idleClock * Mathf.PI * 2f * _breathRate) * _breathDegrees * idle;
            float weightShift = Mathf.Sin(_idleClock * Mathf.PI * 2f * _breathRate * 0.37f) * _weightShiftDegrees * idle;

            float strikeU = _attackTime >= 0f ? _attackTime / _strikeDuration : -1f;
            float strikeArm = LocomotionMath.AttackArmSwing(strikeU);
            float strikeTwist = LocomotionMath.AttackTwist(strikeU) * _strikeTwistDegrees * (_strikeLeft ? -1f : 1f);

            float cast = _castTime >= 0f ? LocomotionMath.Envelope(_castTime, _castRise, _castHold, _castRelease) : 0f;
            float flinch = _hitTime >= 0f ? LocomotionMath.Envelope(_hitTime, 0.04f, 0f, Mathf.Max(0.01f, _flinchDuration - 0.04f)) : 0f;

            float alive = 1f - _deathFall;
            float pitch = (_leanPitch + _currentSpeedFactor * _runLeanDegrees) * alive
                          - _castArchDegrees * cast
                          - _flinchDegrees * flinch
                          + 18f * _deathFall;
            float roll = (_leanRoll + weightShift) * alive + _flinchDegrees * 0.5f * _flinchRollSign * flinch;

            // Torso: lean + twist distributed down the spine.
            RotateChain(_spineLow, 0.35f, pitch, roll, strikeTwist * 0.3f, right, forward, up);
            RotateChain(_chest, 0.35f, pitch + breath * 0.5f, roll, strikeTwist * 0.35f, right, forward, up);
            RotateChain(_upperChest, 0.3f, pitch + breath, roll, strikeTwist * 0.35f, right, forward, up);

            // Head: look (yaw), counter part of the lean so the eyes stay level, flinch jerk, death droop.
            float headYaw = _headYaw * alive - strikeTwist * 0.5f;
            float headPitch = -pitch * 0.4f * alive - _flinchDegrees * 0.6f * flinch + 22f * _deathFall;
            RotateChain(_neck, 0.4f, headPitch, 0f, headYaw, right, forward, up);
            RotateChain(_head, 0.6f, headPitch, 0f, headYaw, right, forward, up);

            // Arms: strike (one arm, alternating) and cast (both raise).
            float strikeDegrees = strikeArm * _strikeArmDegrees;
            float leftRaise = cast * _castArmDegrees + (_strikeLeft ? strikeDegrees : 0f);
            float rightRaise = cast * _castArmDegrees + (_strikeLeft ? 0f : strikeDegrees);
            RotateArm(_leftArm, leftRaise, right);
            RotateArm(_rightArm, rightRaise, right);
            RotateArm(_leftForeArm, leftRaise * 0.25f, right);
            RotateArm(_rightForeArm, rightRaise * 0.25f, right);

            foreach (Bone bone in _bones)
            {
                if (bone.Transform != null)
                {
                    bone.LastWritten = bone.Transform.localRotation;
                    bone.HasWritten = true;
                }
            }
        }

        private static void RotateChain(Bone bone, float weight, float pitch, float roll, float yaw, Vector3 right, Vector3 forward, Vector3 up)
        {
            if (bone == null || bone.Transform == null)
            {
                return;
            }

            Quaternion delta = Quaternion.AngleAxis(yaw * weight, up)
                               * Quaternion.AngleAxis(pitch * weight, right)
                               * Quaternion.AngleAxis(-roll * weight, forward);
            bone.Transform.rotation = delta * bone.Transform.rotation;
        }

        private static void RotateArm(Bone bone, float raiseDegrees, Vector3 right)
        {
            if (bone == null || bone.Transform == null || Mathf.Abs(raiseDegrees) < 0.01f)
            {
                return;
            }

            // Negative about the character's right axis swings a hanging arm forward/up.
            bone.Transform.rotation = Quaternion.AngleAxis(-raiseDegrees, right) * bone.Transform.rotation;
        }

        // ------------------------------------------------------------------
        // Whole-body pose (both modes; richer in rigid mode)
        // ------------------------------------------------------------------

        private void ComposeVisualPose()
        {
            Transform parent = _visualRoot.parent;
            Vector3 rightP = ToParentDirection(parent, transform.right);
            Vector3 forwardP = ToParentDirection(parent, transform.forward);
            Vector3 upP = ToParentDirection(parent, Vector3.up);
            Vector3 pivot = parent != null ? parent.InverseTransformPoint(transform.position) : transform.position;

            float pitch = 0f;
            float roll = 0f;
            float yaw = 0f;
            Vector3 scale = Vector3.one;
            Vector3 worldOffset = Vector3.zero;
            float alive = 1f - _deathFall;

            if (!_rigMode)
            {
                float stepWave = Mathf.Sin(_bobPhase);
                float hop = Mathf.Abs(stepWave);
                float footfall = 1f - hop; // 1 at contact
                worldOffset += Vector3.up * (hop * _bobAmplitude * (0.3f + _currentSpeedFactor) * Mathf.Clamp01(_currentSpeedFactor * 4f));

                pitch += (_currentSpeedFactor * _leanDegrees + _leanPitch) * alive;
                roll += (_leanRoll + stepWave * _swayDegrees * _currentSpeedFactor) * alive;

                float squash = footfall * _footfallSquash * _currentSpeedFactor;
                float idle = 1f - _currentSpeedFactor;
                float breath = Mathf.Sin(_idleClock * Mathf.PI * 2f * _breathRate) * _breathScale * idle;
                roll += Mathf.Sin(_idleClock * Mathf.PI * 2f * _breathRate * 0.37f) * _weightShiftDegrees * idle;
                scale = new Vector3(1f + squash * 0.5f, 1f - squash + breath, 1f + squash * 0.5f);

                if (_castTime >= 0f)
                {
                    float cast = LocomotionMath.Envelope(_castTime, _castRise, _castHold, _castRelease);
                    worldOffset += Vector3.up * (_castHop * cast);
                    pitch -= _castArchDegrees * cast;
                    scale.y *= 1f + 0.06f * cast;
                }

                if (_hitTime >= 0f)
                {
                    float flinch = LocomotionMath.Envelope(_hitTime, 0.04f, 0f, Mathf.Max(0.01f, _flinchDuration - 0.04f));
                    pitch -= _flinchDegrees * 0.6f * flinch;
                }
            }

            if (_attackTime >= 0f)
            {
                float length = _rigMode ? _strikeDuration : _attackDuration;
                float progress = Mathf.Clamp01(_attackTime / length);
                float curveValue = _attackCurve != null ? _attackCurve.Evaluate(progress) : progress;
                float wave = Mathf.Sin(curveValue * Mathf.PI);
                worldOffset += transform.forward * (wave * _attackLungeDistance * (_rigMode ? 0.4f : 1f));

                if (!_rigMode)
                {
                    float squash = wave * _attackSquash;
                    scale = Vector3.Scale(scale, new Vector3(1f + squash, 1f - squash * 0.6f, 1f + squash));
                    yaw += LocomotionMath.AttackTwist(progress) * _strikeTwistDegrees * (_strikeLeft ? -1f : 1f);
                }
            }

            if (_hitTime >= 0f && _hitTime < _hitShakeDuration)
            {
                float shake = 1f - _hitTime / _hitShakeDuration;
                worldOffset += _hitShakeSeed * (_hitShakeMagnitude * shake * (_rigMode ? 0.5f : 1f));
            }

            if (_deathFall > 0f || _deathSink > 0f)
            {
                // Collapse backward around the feet, lift a little so the body lies on (not in) the
                // ground, then sink.
                pitch -= _deathFallDegrees * _deathFall;
                worldOffset += Vector3.up * (0.08f * _deathFall - _deathSinkDepth * _deathSink);
            }

            Quaternion rotation = Quaternion.AngleAxis(yaw, upP)
                                  * Quaternion.AngleAxis(pitch, rightP)
                                  * Quaternion.AngleAxis(-roll, forwardP);

            Vector3 fromPivot = _restLocalPosition - pivot;
            fromPivot = Vector3.Scale(fromPivot, scale);
            Vector3 offset = parent != null ? parent.InverseTransformVector(worldOffset) : worldOffset;

            _visualRoot.localPosition = pivot + rotation * fromPivot + offset;
            _visualRoot.localRotation = rotation * _restLocalRotation;
            _visualRoot.localScale = Vector3.Scale(_restLocalScale, scale);
        }

        private static Vector3 ToParentDirection(Transform parent, Vector3 worldDirection)
        {
            if (parent == null)
            {
                return worldDirection;
            }

            Vector3 local = parent.InverseTransformDirection(worldDirection);
            return local.sqrMagnitude > 1e-8f ? local.normalized : worldDirection;
        }

        // ------------------------------------------------------------------
        // Hit flash
        // ------------------------------------------------------------------

        private void ApplyHitFlash()
        {
            if (_hitFlashTimer < 0f)
            {
                ClearFlash();
                return;
            }

            float intensity = _hitFlashTimer / _hitFlashDuration;
            foreach (Renderer target in _renderers)
            {
                if (target == null)
                {
                    continue;
                }

                target.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(EmissionColorId, _hitFlashColor * intensity);
                target.SetPropertyBlock(_propertyBlock);
            }

            _flashApplied = true;
        }

        private void ClearFlash()
        {
            if (!_flashApplied)
            {
                return;
            }

            foreach (Renderer target in _renderers)
            {
                if (target != null)
                {
                    target.SetPropertyBlock(null);
                }
            }

            _flashApplied = false;
        }
    }
}
