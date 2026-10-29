using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Code-driven animation layers for the hero visual, composed every LateUpdate (after the
    /// Animator) on top of whatever the Animator produced. Everything is additive, short and
    /// interruptible, and uses scaled time (pausing freezes it). How a hero moves — weight, idle
    /// rhythm, attack timing, springiness — comes from its <see cref="HeroMotionProfile"/>
    /// (<see cref="SetMotionProfile"/>, called by <see cref="PlayerVisualSwapper"/>). Two modes, picked
    /// automatically per visual (re-detected on <see cref="SetVisualRoot"/>):
    ///
    /// <para><b>Rig mode</b> (the Meshy-rigged heroes: Humanoid avatar bones, or Meshy/Mixamo bone
    /// names Hips/Spine02/Spine01/Spine/neck/Head/LeftArm/...): the Walk/Run clips own the legs; this
    /// adds on the spine/neck/head/arm bones:</para>
    /// <list type="bullet">
    /// <item>lean into acceleration (profile curve) and into turns, the chest leading the turn;</item>
    /// <item>torso counter-rotation and extra arm swing opposite the legs, read from the animated feet;</item>
    /// <item>head look toward the pursued attack target, else leading the movement direction;</item>
    /// <item>idle that is never still: breathing (chest + shoulders), weight shift foot to foot (hip
    /// roll + lateral hip shift through <see cref="PlayerFootIK"/>), micro head/shoulder noise,
    /// occasional glances;</item>
    /// <item>secondary overlap: damped springs make head, neck and arms (and any hair/cape/weapon
    /// bones the model has) lag behind the body's accelerations and overshoot on stops/landings;</item>
    /// <item>melee strike with anticipation that lands exactly on <see cref="PlayerBasicAttack.StrikeDelay"/>,
    /// follow-through and recovery (profile curve), alternating arms, on <see cref="PlayerAttackPerformedEvent"/>;</item>
    /// <item>cast pose (both arms raise, chest arches) on <see cref="SkillCastEvent"/>;</item>
    /// <item>hit impact: a spring kick away from the attacker + flinch (+ emissive flash) on <see cref="DamageAppliedEvent"/>.</item>
    /// </list>
    /// Humanoid rigs also get <see cref="PlayerFootIK"/> (ground-following feet, pinned feet while
    /// standing/turning, knee tuck in the air, pelvis bob/dip/weight shift with the feet planted).
    ///
    /// <para><b>Rigid mode</b> (no rig — Maithe's static model, the placeholder capsule): the same
    /// cues as whole-body motion on <see cref="_visualRoot"/>: stride-matched step bob, sway, lean,
    /// squash-stretch on footfalls, breathing, lunge, cast hop, recoil.</para>
    ///
    /// <para>Both modes: subtle volume-preserving squash &amp; stretch (takeoff crouch → stretch, air
    /// stretch, landing squash by fall speed; ≤ <see cref="MotionFeelMath.MaxSquash"/>, halved with
    /// "Reduzir Flashes"), death collapse (fall around the feet + settle bounce + sink, no ragdoll)
    /// while <see cref="PlayerStats.IsDead"/>, blending back out on revive. Whole-body rotations pivot
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

        [Header("Motion profile (live copy — tweak in Play Mode)")]
        [SerializeField] private HeroMotionProfile _profile = HeroMotionProfile.Default;
        [Tooltip("Add PlayerFootIK to humanoid visuals (needs IK Pass on the controller's base layer — Rebuild Hero Animators).")]
        [SerializeField] private bool _footIK = true;

        [Header("Locomotion (rigid mode)")]
        [SerializeField] private float _bobAmplitude = 0.06f;
        [SerializeField] private float _leanDegrees = 8f;
        [SerializeField] private float _poseSmoothing = 12f;
        [Tooltip("Metres per step of the rigid step bob (cadence = speed / step length, so it never 'skates').")]
        [SerializeField, Min(0.1f)] private float _stepLength = 0.6f;
        [SerializeField, Range(0f, 0.06f)] private float _footfallSquash = 0.04f;
        [SerializeField] private float _swayDegrees = 3f;

        [Header("Lean (both modes)")]
        [SerializeField] private float _maxLeanPitch = 12f;
        [SerializeField] private float _maxLeanRoll = 10f;
        [Tooltip("Extra forward lean (deg) at run speed (rig mode).")]
        [SerializeField] private float _runLeanDegrees = 4f;
        [Tooltip("Chest yaw (deg) leading toward the wanted direction at a 90° turn.")]
        [SerializeField] private float _turnLeadDegrees = 12f;

        [Header("Idle (rigid mode breathing scale)")]
        [SerializeField] private float _breathScale = 0.012f;

        [Header("Head Look (rig mode)")]
        [SerializeField] private float _headLookMaxYaw = 65f;
        [SerializeField] private float _headLookSharpness = 8f;
        [SerializeField] private float _headLookTargetRange = 14f;

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
        [Tooltip("Degrees the overlap spring is kicked away from the attacker on a hit (× profile flinch scale / weight).")]
        [SerializeField] private float _hitKickDegrees = 14f;

        [Header("Squash & stretch")]
        [Tooltip("Spring of the whole-body squash: low damping = a little bounce after landings (follow-through).")]
        [SerializeField] private float _squashFrequency = 4f;
        [SerializeField] private float _squashDamping = 0.45f;
        [Tooltip("Multiplier on squash and overlap with the accessibility option \"Reduzir Flashes\" on.")]
        [SerializeField, Range(0f, 1f)] private float _reducedMotionScale = 0.5f;

        [Header("Death (no ragdoll)")]
        [SerializeField] private float _deathFallDegrees = 80f;
        [SerializeField, Min(0.05f)] private float _deathFallDuration = 0.6f;
        [SerializeField] private float _deathSinkDelay = 1.1f;
        [SerializeField] private float _deathSinkDepth = 0.25f;
        [SerializeField, Min(0.05f)] private float _deathSinkDuration = 1.4f;
        [SerializeField, Min(0.05f)] private float _reviveBlendTime = 0.3f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        /// <summary>Name fragments of non-standard bones/props that get springy overlap (hair, cloth, tails, held props).</summary>
        private static readonly string[] SecondaryKeywords =
        {
            "hair", "pony", "braid", "bang", "cape", "cloak", "scarf", "ribbon", "skirt", "tail", "hood",
            "weapon", "sword", "shield", "bow", "staff", "wand", "quiver", "prop",
        };

        private const int MaxSecondaryBones = 12;

        /// <summary>One bone the rig layers write additively; remembers what it wrote to detect an Animator that didn't overwrite it (no accumulation).</summary>
        private sealed class Bone
        {
            public Transform Transform;
            public Quaternion LastBase;
            public Quaternion LastWritten;
            public bool HasWritten;
            public float Depth; // secondary bones: chain depth below the first keyword bone (1 = root) — deeper lags more
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
        private Bone[] _secondary = new Bone[0];
        private Transform _leftFoot;
        private Transform _rightFoot;
        private bool _rigMode;
        private PlayerFootIK _footIk;

        private Renderer[] _renderers = new Renderer[0];
        private bool _flashApplied;
        private MaterialPropertyBlock _propertyBlock;

        private SkinnedMeshRenderer _blinkRenderer;
        private int _blinkShape = -1;
        private float _nextBlink = 3f;
        private float _blinkTime = -1f;

        private PlayerStats _stats;
        private PlayerActionAnimator _actions;
        private PlayerClickToMove _clickToMove;
        private PlayerBasicAttack _basicAttack;
        private AccessibilitySettings _accessibility;

        // Kinematics
        private float _prevSpeed;
        private float _prevYaw;
        private Vector3 _prevVelocity;
        private bool _hasPrevKinematics;
        private float _accelSmoothed;
        private float _yawRateSmoothed;
        private Vector2 _planarAccel; // (right, forward) m/s², smoothed
        private float _leanPitch;
        private float _leanRoll;
        private float _currentSpeedFactor;
        private float _bobPhase;
        private float _idleClock;
        private float _standTime;
        private float _headYaw;
        private float _turnLead;
        private float _stride;
        private float _strideReference = 0.2f;
        private int _seed;

        // Air / ground
        private bool _wasGrounded = true;
        private float _lastAirVelocity;
        private float _squash;
        private float _squashVelocity;
        private float _dip;
        private float _dipVelocity;

        // Overlap springs (pitch, roll in degrees)
        private Vector2 _overlap;
        private Vector2 _overlapVelocity;

        // Timers (-1 = inactive; count up from 0)
        private float _attackTime = -1f;
        private float _attackStrikeDelay = HeroActionTiming.StrikeDelay;
        private float _attackCarryArm;
        private float _attackCarryTwist;
        private float _attackCarryTime = -1f;
        private float _castTime = -1f;
        private float _hitTime = -1f;
        private float _hitFlashTimer = -1f;
        private bool _strikeLeft;
        private Vector3 _hitShakeSeed;
        private float _flinchRollSign = 1f;

        private bool _restCaptured;

        // Death
        private bool _wasDead;
        private float _deathTime = -1f;
        private float _deathFall;
        private float _deathSink;
        private bool _deathImpactDone;

        /// <summary>True when bone layers are active (a rig was found on the visual).</summary>
        public bool IsRigMode => _rigMode;

        /// <summary>The live profile (a copy; edits don't touch the source asset).</summary>
        public HeroMotionProfile Profile => _profile;

        /// <summary>Current whole-body squash/stretch amount (+ = taller) before the accessibility scale — for tests/debugging.</summary>
        public float Squash => _squash;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _stats = GetComponent<PlayerStats>();
            _clickToMove = GetComponent<PlayerClickToMove>();
            _basicAttack = GetComponent<PlayerBasicAttack>();
            _seed = GetInstanceID();
            if (_profile == null)
            {
                _profile = HeroMotionProfile.Default;
            }

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

        /// <summary>
        /// Switches the hero's motion personality: keeps a private copy (tweakable in the Inspector), pushes
        /// the locomotion part (acceleration/braking/turning) to the <see cref="PlayerMotor"/>. Null = neutral defaults.
        /// </summary>
        public void SetMotionProfile(HeroMotionProfile profile)
        {
            _profile = (profile ?? HeroMotionProfile.Default).Clone();

            if (_motor == null)
            {
                _motor = GetComponentInParent<PlayerMotor>();
            }

            if (_motor != null)
            {
                _motor.ApplyMotionProfile(_profile);
            }
        }

        /// <summary>Re-targets which transform receives the procedural motion — used by <see cref="PlayerVisualSwapper"/> after swapping the visual model. Recaptures the rest pose and re-detects the rig.</summary>
        public void SetVisualRoot(Transform visualRoot)
        {
            ClearFlash();

            // Leave the previous root (e.g. the scene's PlayerVisual container) at its rest pose,
            // not frozen mid-lean/shake. Only if that pose was actually captured: the swapper can
            // call this before Awake (hero restored from carried state on scene load), and the
            // default rest scale is zero — that collapsed the whole hero to nothing.
            if (_restCaptured && _visualRoot != null && _visualRoot != visualRoot)
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
            _secondary = new Bone[0];
            _rigMode = false;
            _renderers = new Renderer[0];
            _footIk = null;
            _leftFoot = _rightFoot = null;
            _blinkRenderer = null;
            _blinkShape = -1;

            if (visualRoot == null)
            {
                return;
            }

            _restLocalPosition = visualRoot.localPosition;
            _restLocalRotation = visualRoot.localRotation;
            _restLocalScale = visualRoot.localScale;
            _restCaptured = true;

            _renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (_renderers.Length == 0 && _renderer != null)
            {
                _renderers = new[] { _renderer };
            }

            FindBones(visualRoot);
            FindSecondaryBones(visualRoot);
            FindBlinkShape();
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
            _leftFoot = MakeBone(root, animator, human, HumanBodyBones.LeftFoot, "LeftFoot", "mixamorig:LeftFoot")?.Transform;
            _rightFoot = MakeBone(root, animator, human, HumanBodyBones.RightFoot, "RightFoot", "mixamorig:RightFoot")?.Transform;

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

            if (human && _footIK)
            {
                _footIk = animator.GetComponent<PlayerFootIK>();
                if (_footIk == null)
                {
                    _footIk = animator.gameObject.AddComponent<PlayerFootIK>();
                }

                _footIk.Initialize(_motor, transform);
            }
        }

        /// <summary>
        /// Hair/cloth/tail/prop transforms (by name) that get spring overlap. None of today's Meshy rigs has
        /// any (standard 24-bone humanoid only), so this is for future models; skinned-mesh owners are
        /// skipped (their transform doesn't move vertices).
        /// </summary>
        private void FindSecondaryBones(Transform root)
        {
            var list = new System.Collections.Generic.List<Bone>();
            CollectSecondary(root, root, 0, list);
            _secondary = list.ToArray();
        }

        private void CollectSecondary(Transform root, Transform t, int depth, System.Collections.Generic.List<Bone> list)
        {
            if (list.Count >= MaxSecondaryBones)
            {
                return;
            }

            int childDepth = depth;
            if (t != root && t.GetComponent<SkinnedMeshRenderer>() == null && t.GetComponent<Animator>() == null && !IsCoreBone(t))
            {
                if (depth > 0 || NameMatches(t.name))
                {
                    childDepth = depth + 1;
                    list.Add(new Bone { Transform = t, LastBase = t.localRotation, LastWritten = t.localRotation, Depth = childDepth });
                }
            }

            for (int i = 0; i < t.childCount; i++)
            {
                CollectSecondary(root, t.GetChild(i), childDepth, list);
            }
        }

        private bool IsCoreBone(Transform t)
        {
            foreach (Bone bone in _bones)
            {
                if (bone.Transform == t)
                {
                    return true;
                }
            }

            return t == _leftFoot || t == _rightFoot;
        }

        private static bool NameMatches(string boneName)
        {
            if (boneName.IndexOf("elbow", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            foreach (string keyword in SecondaryKeywords)
            {
                if (boneName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Eye blink only if a mesh actually has a blink/eyes-closed blendshape (the current Meshy heroes have none).</summary>
        private void FindBlinkShape()
        {
            foreach (Renderer r in _renderers)
            {
                if (r is SkinnedMeshRenderer skinned && skinned.sharedMesh != null)
                {
                    Mesh mesh = skinned.sharedMesh;
                    for (int i = 0; i < mesh.blendShapeCount; i++)
                    {
                        string shape = mesh.GetBlendShapeName(i);
                        if (shape.IndexOf("blink", System.StringComparison.OrdinalIgnoreCase) >= 0
                            || shape.IndexOf("eyesclosed", System.StringComparison.OrdinalIgnoreCase) >= 0
                            || shape.IndexOf("eyeclose", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _blinkRenderer = skinned;
                            _blinkShape = i;
                            return;
                        }
                    }
                }
            }
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
            ServiceLocator.TryGet(out _accessibility);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            ClearFlash();
        }

        private bool IsDead => _stats != null && _stats.IsDead;

        /// <summary>1, or the reduced-motion scale while the accessibility "Reduzir Flashes" option is on.</summary>
        private float MotionScale => _accessibility != null && _accessibility.ReduceFlashEffects ? _reducedMotionScale : 1f;

        private float SpringFrequency => Mathf.Max(0.2f, _profile.springFrequency);

        /// <summary>The Animator plays real attack/cast clips: the procedural arm whip and cast pose stand down (lean, head look, flinch and flash stay).</summary>
        private bool ClipActions
        {
            get
            {
                if (_actions == null)
                {
                    _actions = GetComponent<PlayerActionAnimator>();
                }

                return _actions != null && _actions.HasClipActions;
            }
        }

        /// <summary>The Animator plays a real death clip: no procedural topple, only the delayed sink into the ground.</summary>
        private bool ClipDeath
        {
            get
            {
                if (_actions == null)
                {
                    _actions = GetComponent<PlayerActionAnimator>();
                }

                return _actions != null && _actions.HasDeathClip;
            }
        }

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

            // Physical impact: kick the overlap spring away from the attacker (from the front = snap back,
            // from the right = roll left); the spring's overshoot is the inertia of the recovery.
            Vector3 push = -transform.forward;
            if (evt.Source != null)
            {
                Vector3 away = transform.position - evt.Source.transform.position;
                away.y = 0f;
                if (away.sqrMagnitude > 1e-4f)
                {
                    push = away.normalized;
                }
            }

            float kick = _hitKickDegrees * _profile.flinchScale / Mathf.Max(0.5f, _profile.weight);
            float omega = 2f * Mathf.PI * SpringFrequency;
            _overlapVelocity += new Vector2(Vector3.Dot(push, transform.forward), Vector3.Dot(push, transform.right)) * (kick * omega);
            _squashVelocity -= 0.02f * omega * _profile.flinchScale;
        }

        private void OnAttackPerformed(PlayerAttackPerformedEvent evt)
        {
            if (evt.Attacker != gameObject || IsDead)
            {
                return;
            }

            // Restarting mid-swing: fade out what the previous swing was showing instead of popping.
            if (_attackTime >= 0f)
            {
                float x = MotionFeelMath.AttackCurveX(_attackTime, _attackStrikeDelay, _profile.attackRecoverTime);
                _attackCarryArm = AttackArm(x);
                _attackCarryTwist = MotionFeelMath.AttackTwistAt(x) * (_strikeLeft ? -1f : 1f);
                _attackCarryTime = 0f;
            }

            // The anticipation ends exactly when PlayerBasicAttack lands the damage.
            _attackStrikeDelay = _basicAttack != null ? _basicAttack.StrikeDelay : HeroActionTiming.StrikeDelay;
            _attackTime = 0f;
            _strikeLeft = !_strikeLeft;
        }

        private float AttackArm(float x)
        {
            if (x < 0f)
            {
                return 0f;
            }

            AnimationCurve curve = _profile.attackArmCurve;
            return curve != null && curve.length > 0 ? curve.Evaluate(x) : LocomotionMath.AttackArmSwing(x);
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
            TickAirborne(deltaTime);
            TickTimers(deltaTime);
            TickDeath(deltaTime);
            TickSprings(deltaTime);

            if (_rigMode)
            {
                ApplyBoneLayers();
                ApplySecondaryBones();
            }

            ComposeVisualPose();
            DriveFootIK();
            TickBlink(deltaTime);
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

            Vector3 velocity = _motor != null ? _motor.HorizontalVelocity : Vector3.zero;
            float speed = velocity.magnitude;
            float yaw = transform.eulerAngles.y;

            if (!_hasPrevKinematics)
            {
                _prevSpeed = speed;
                _prevYaw = yaw;
                _prevVelocity = velocity;
                _hasPrevKinematics = true;
            }

            float accel = (speed - _prevSpeed) / deltaTime;
            float yawRate = Mathf.DeltaAngle(_prevYaw, yaw) / deltaTime;
            Vector3 accelWorld = Vector3.ClampMagnitude((velocity - _prevVelocity) / deltaTime, 40f);
            _prevSpeed = speed;
            _prevYaw = yaw;
            _prevVelocity = velocity;

            float k = LocomotionMath.ExpBlend(10f, deltaTime);
            _accelSmoothed = Mathf.Lerp(_accelSmoothed, accel, k);
            _yawRateSmoothed = Mathf.Lerp(_yawRateSmoothed, yawRate, k);
            Vector2 planar = new Vector2(Vector3.Dot(accelWorld, transform.right), Vector3.Dot(accelWorld, transform.forward));
            _planarAccel = Vector2.Lerp(_planarAccel, planar, LocomotionMath.ExpBlend(12f, deltaTime));

            // Pitch from the profile's lean-by-acceleration curve (sign kept), roll into turns from physics.
            Vector2 lean = LocomotionMath.LeanAngles(_accelSmoothed, _yawRateSmoothed, speed, 1f, _profile.turnLeanGain, _maxLeanPitch, _maxLeanRoll);
            AnimationCurve leanCurve = _profile.leanByAcceleration;
            float pitch = leanCurve != null && leanCurve.length > 0
                ? Mathf.Sign(_accelSmoothed) * leanCurve.Evaluate(Mathf.Abs(_accelSmoothed))
                : lean.x;
            pitch = Mathf.Clamp(pitch, -_maxLeanPitch, _maxLeanPitch);

            float poseK = LocomotionMath.ExpBlend(_poseSmoothing, deltaTime);
            _leanPitch = Mathf.Lerp(_leanPitch, pitch, poseK);
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

            // Turn lead: the chest starts toward where the hero wants to go before the hips get there.
            float turnAngle = _motor != null ? MotionFeelMath.SignedTurnAngle(transform.forward, _motor.DesiredDirection) : 0f;
            _turnLead = Mathf.Lerp(_turnLead, Mathf.Clamp(turnAngle / 90f, -1f, 1f) * _turnLeadDegrees, poseK);

            TickStride(poseK);

            _idleClock += deltaTime;
            TickHeadLook(deltaTime, speed);
        }

        /// <summary>Stride phase from the animated feet (rig) or the rigid step phase — drives counter-rotation/arm swing.</summary>
        private void TickStride(float k)
        {
            float target;
            if (_rigMode && _leftFoot != null && _rightFoot != null)
            {
                float raw = Vector3.Dot(_leftFoot.position - _rightFoot.position, transform.forward);
                // Self-calibrating stride length: tracks the largest recent separation, slowly forgets it.
                _strideReference = Mathf.Max(Mathf.Max(Mathf.Abs(raw) * 0.5f, _strideReference - 0.05f * Time.deltaTime), 0.05f);
                target = MotionFeelMath.StridePhase(_leftFoot.position, _rightFoot.position, transform.forward, _strideReference * 2f);
            }
            else
            {
                target = Mathf.Sin(_bobPhase);
            }

            _stride = Mathf.Lerp(_stride, target, Mathf.Max(k, 0.5f));
        }

        private void TickHeadLook(float deltaTime, float speed)
        {
            float targetYaw = 0f;
            Vector3 lookDirection = Vector3.zero;
            bool hasTarget = false;

            ICombatTarget target = _clickToMove != null ? _clickToMove.AttackTarget : null;
            if (target != null && !(target is Object unityObject && unityObject == null) && target.IsAlive)
            {
                Vector3 toTarget = target.Transform.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude < _headLookTargetRange * _headLookTargetRange)
                {
                    lookDirection = toTarget;
                    hasTarget = true;
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

            // Standing around with nothing to look at: the occasional glance.
            bool standing = !hasTarget && speed < 0.1f && _attackTime < 0f && _castTime < 0f && !IsDead;
            _standTime = standing ? _standTime + deltaTime : 0f;
            if (standing)
            {
                targetYaw += MotionFeelMath.IdleGlanceYaw(_standTime, _profile.glanceInterval, _profile.glanceHold, _profile.glanceYaw, _seed);
            }

            _headYaw = Mathf.Lerp(_headYaw, targetYaw, LocomotionMath.ExpBlend(_headLookSharpness, deltaTime));
        }

        /// <summary>Takeoff (visual crouch → stretch, never delaying the jump), air stretch, landing squash/dip by fall speed.</summary>
        private void TickAirborne(float deltaTime)
        {
            if (_motor == null || deltaTime <= 0f)
            {
                return;
            }

            bool grounded = _motor.IsGrounded;
            float vertical = _motor.VerticalVelocity;
            float omega = 2f * Mathf.PI * _squashFrequency;
            float heavy = Mathf.Max(0.5f, _profile.weight);

            if (_wasGrounded && !grounded && vertical > 0.5f)
            {
                // Takeoff: the crouch is shown as an instant compression the spring releases into the
                // stretch over the first few frames (≈ 60 ms) — the motor already left the ground.
                _squash = -_profile.takeoffSquash;
                _squashVelocity = 0f;
                _overlapVelocity.x += 6f * omega / heavy;
            }
            else if (!_wasGrounded && grounded)
            {
                float fall = -Mathf.Min(0f, _lastAirVelocity);
                float amount = MotionFeelMath.LandingSquash(fall, _profile.landingSquashByFallSpeed, _profile.landingSquash);
                float fraction = _profile.landingSquash > 0f ? amount / _profile.landingSquash : 0f;
                _squashVelocity -= amount * omega * 1.4f;
                _dipVelocity -= fraction * _profile.landingDip * 2f * Mathf.PI * 3f * 1.3f;
                _overlapVelocity.x += fraction * 18f * heavy * 2f * Mathf.PI * SpringFrequency;
            }

            if (!grounded)
            {
                _lastAirVelocity = vertical;
            }

            _wasGrounded = grounded;
        }

        private void TickSprings(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            bool airborne = _motor != null && !_motor.IsGrounded;
            float squashTarget = airborne ? MotionFeelMath.AirStretch(_motor.VerticalVelocity, 0.03f) : 0f;
            MotionFeelMath.StepSpring(ref _squash, ref _squashVelocity, squashTarget, _squashFrequency, _squashDamping, deltaTime);
            _squash = Mathf.Clamp(_squash, -MotionFeelMath.MaxSquash, MotionFeelMath.MaxSquash);

            // Pelvis dip: landings and the plant of a braked turn.
            float brake = _motor != null ? _motor.TurnBrake : 0f;
            MotionFeelMath.StepSpring(ref _dip, ref _dipVelocity, -0.03f * brake * _profile.weight, 3f, 0.6f, deltaTime);
            _dip = Mathf.Clamp(_dip, -0.25f, 0.05f);

            // Overlap: lag behind the body's planar acceleration (steady lag = gain · a degrees), overshoot on stops.
            float omega = 2f * Mathf.PI * SpringFrequency;
            float gain = _profile.overlapGain * Mathf.Max(0.5f, _profile.weight) * omega * omega;
            Vector2 drive = new Vector2(-_planarAccel.y, -_planarAccel.x) * gain;
            MotionFeelMath.StepSpring(ref _overlap, ref _overlapVelocity, Vector2.zero, SpringFrequency, _profile.springDamping, deltaTime, drive);
            _overlap = Vector2.ClampMagnitude(_overlap, 25f);
        }

        private void TickTimers(float deltaTime)
        {
            Advance(ref _attackTime, deltaTime, MotionFeelMath.AttackLength(_attackStrikeDelay, _profile.attackRecoverTime));
            Advance(ref _attackCarryTime, deltaTime, 0.08f);
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
                _deathImpactDone = false;
            }

            _wasDead = dead;

            if (dead)
            {
                _deathTime += deltaTime;
                if (!(_rigMode && ClipDeath))
                {
                    _deathFall = Mathf.Max(_deathFall, LocomotionMath.FallProgress(_deathTime, _deathFallDuration));
                }

                if (!_deathImpactDone && _deathTime >= _deathFallDuration)
                {
                    // Body hits the ground: a squash and a head/arm bounce settle it (inertia, not a freeze).
                    _deathImpactDone = true;
                    float omega = 2f * Mathf.PI * _squashFrequency;
                    _squashVelocity -= 0.05f * omega;
                    _overlapVelocity.x -= 20f * 2f * Mathf.PI * SpringFrequency;
                }

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
        // Attack sampling (shared by both modes)
        // ------------------------------------------------------------------

        /// <summary>Current attack-curve axis position (−1 = no swing), arm (0..1) and twist (−1..1), the restart carry faded in.</summary>
        private float SampleAttack(out float arm, out float twist)
        {
            float x = _attackTime >= 0f ? MotionFeelMath.AttackCurveX(_attackTime, _attackStrikeDelay, _profile.attackRecoverTime) : -1f;
            arm = AttackArm(x);
            twist = MotionFeelMath.AttackTwistAt(x) * (_strikeLeft ? -1f : 1f);

            if (_attackCarryTime >= 0f)
            {
                float carry = 1f - Mathf.SmoothStep(0f, 1f, _attackCarryTime / 0.08f);
                arm = Mathf.Max(arm, _attackCarryArm * carry);
                twist += _attackCarryTwist * carry;
            }

            return x;
        }

        // ------------------------------------------------------------------
        // Rig layers
        // ------------------------------------------------------------------

        private void ApplyBoneLayers()
        {
            RestoreBases(_bones);

            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            Vector3 up = Vector3.up;

            float motion = MotionScale;
            float idle = 1f - _currentSpeedFactor;
            float moving = Mathf.Clamp01(_currentSpeedFactor * 3f);
            float breathWave = Mathf.Sin(_idleClock * Mathf.PI * 2f * _profile.breathRate);
            float breath = breathWave * _profile.breathDegrees * Mathf.Lerp(0.4f, 1f, idle);
            float shiftWave = Mathf.Sin(_idleClock * Mathf.PI * 2f / Mathf.Max(0.5f, _profile.weightShiftPeriod));
            float weightShift = shiftWave * _profile.weightShiftDegrees * idle;

            // Micro motion: low-frequency noise on head and shoulders (never perfectly still).
            float noiseT = _idleClock * _profile.fidgetRate;
            float fidgetYaw = (Mathf.PerlinNoise(noiseT, _seed * 0.001f) - 0.5f) * 2f * _profile.fidgetDegrees;
            float fidgetPitch = (Mathf.PerlinNoise(_seed * 0.001f, noiseT + 7.3f) - 0.5f) * 2f * _profile.fidgetDegrees;
            float shoulderFidget = (Mathf.PerlinNoise(noiseT * 0.7f + 3.1f, 11.7f) - 0.5f) * 2f * _profile.fidgetDegrees;

            // With real attack/cast clips on the Animator, the bone-level strike and cast poses would
            // fight the clip (double swing); keep only a trace of torso twist for punch.
            float procedural = ClipActions ? 0f : 1f;
            SampleAttack(out float strikeArm, out float strikeTwistUnit);
            strikeArm *= procedural;
            float strikeTwist = strikeTwistUnit * _profile.attackTwistDegrees * Mathf.Max(procedural, 0.25f);

            float cast = (_castTime >= 0f ? LocomotionMath.Envelope(_castTime, _castRise, _castHold, _castRelease) : 0f) * procedural;
            float flinch = (_hitTime >= 0f ? LocomotionMath.Envelope(_hitTime, 0.04f, 0f, Mathf.Max(0.01f, _flinchDuration - 0.04f)) : 0f) * _profile.flinchScale;

            float alive = 1f - _deathFall;
            Vector2 overlap = _overlap * motion;
            float counter = -_stride * _profile.torsoCounterTwist * moving;
            float pitch = (_leanPitch + _currentSpeedFactor * _runLeanDegrees) * alive
                          - _castArchDegrees * cast
                          - _flinchDegrees * flinch
                          + 18f * _deathFall;
            float roll = (_leanRoll + weightShift) * alive + _flinchDegrees * 0.5f * _flinchRollSign * flinch;
            float torsoYaw = strikeTwist + _turnLead * alive;

            // Torso: lean + twist distributed down the spine; counter-rotation grows toward the chest; the
            // upper chest takes a bit of the overlap (it rides on the spine like the head does).
            RotateChain(_spineLow, 0.35f, pitch, roll, torsoYaw * 0.3f - counter * 0.3f, right, forward, up);
            RotateChain(_chest, 0.35f, pitch + breath * 0.5f, roll, torsoYaw * 0.35f + counter * 0.4f, right, forward, up);
            RotateChain(_upperChest, 0.3f, pitch + breath + overlap.x * 0.3f, roll + overlap.y * 0.3f, torsoYaw * 0.35f + counter * 0.6f, right, forward, up);

            // Head: look (yaw), counter part of the lean so the eyes stay level, overlap lag, fidget,
            // flinch jerk, death droop.
            float headYaw = (_headYaw + fidgetYaw * idle) * alive - torsoYaw * 0.5f - counter * 0.5f;
            float headPitch = -pitch * 0.4f * alive + overlap.x * 0.8f + fidgetPitch * idle * alive - _flinchDegrees * 0.6f * flinch + 22f * _deathFall;
            float headRoll = overlap.y * 0.8f;
            RotateChain(_neck, 0.4f, headPitch, headRoll, headYaw, right, forward, up);
            RotateChain(_head, 0.6f, headPitch, headRoll, headYaw, right, forward, up);

            // Arms: strike (one arm, alternating), cast (both raise), swing opposite the legs, breathing
            // shoulders, overlap lag (accelerating forward swings them back, stopping throws them forward).
            float strikeDegrees = strikeArm * _profile.attackArmDegrees;
            float swing = _stride * _profile.armSwingDegrees * moving;
            float shared = cast * _castArmDegrees + overlap.x * 1.2f + (breathWave * 0.6f + shoulderFidget * 0.5f) * idle * alive;
            float leftRaise = shared - swing + (_strikeLeft ? strikeDegrees : 0f);
            float rightRaise = shared + swing + (_strikeLeft ? 0f : strikeDegrees);
            RotateArm(_leftArm, leftRaise, right);
            RotateArm(_rightArm, rightRaise, right);
            RotateArm(_leftForeArm, leftRaise * 0.25f + Mathf.Abs(overlap.x) * 0.4f, right);
            RotateArm(_rightForeArm, rightRaise * 0.25f + Mathf.Abs(overlap.x) * 0.4f, right);

            MarkWritten(_bones);
        }

        /// <summary>Hair/cape/prop bones (if any): rotate with the overlap lag, deeper links more (a chain whips).</summary>
        private void ApplySecondaryBones()
        {
            if (_secondary.Length == 0)
            {
                return;
            }

            RestoreBases(_secondary);
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            Vector2 overlap = _overlap * MotionScale;
            float bob = Mathf.Abs(Mathf.Sin(_bobPhase)) * _currentSpeedFactor * 4f;
            foreach (Bone bone in _secondary)
            {
                if (bone.Transform == null)
                {
                    continue;
                }

                float weight = 0.6f + 0.4f * bone.Depth;
                Quaternion delta = Quaternion.AngleAxis((overlap.x * 1.5f + bob) * weight, right) * Quaternion.AngleAxis(-overlap.y * 1.5f * weight, forward);
                bone.Transform.rotation = delta * bone.Transform.rotation;
            }

            MarkWritten(_secondary);
        }

        private static void RestoreBases(Bone[] bones)
        {
            // Restore each bone's animated base (or last base when the Animator didn't write it).
            foreach (Bone bone in bones)
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
        }

        private static void MarkWritten(Bone[] bones)
        {
            foreach (Bone bone in bones)
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

        /// <summary>Hands the pelvis motion (weight shift, extra step bob, landing/turn dip) and the air tuck to the foot IK.</summary>
        private void DriveFootIK()
        {
            if (_footIk == null)
            {
                return;
            }

            float alive = 1f - _deathFall;
            float idle = 1f - _currentSpeedFactor;
            float shiftWave = Mathf.Sin(_idleClock * Mathf.PI * 2f / Mathf.Max(0.5f, _profile.weightShiftPeriod));
            float moving = Mathf.Clamp01(_currentSpeedFactor * 3f);

            // Lowest at full stride spread (double support), highest passing the feet — the physical walk bob.
            float bob = -Mathf.Abs(_stride) * _profile.bobAmplitude * moving;
            Vector3 offset = transform.right * (shiftWave * _profile.weightShiftHip * idle)
                             + Vector3.up * (bob + _dip * MotionScale);
            _footIk.BodyOffset = offset * alive;

            float rising = _motor != null && !_motor.IsGrounded ? 1f : 0f;
            _footIk.AirTuck = _profile.airTuck * rising * alive;
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
            float stretch = _squash * MotionScale;
            Vector3 worldOffset = Vector3.zero;
            float alive = 1f - _deathFall;
            float x = SampleAttack(out _, out float twistUnit);

            if (!_rigMode)
            {
                float stepWave = Mathf.Sin(_bobPhase);
                float hop = Mathf.Abs(stepWave);
                float footfall = 1f - hop; // 1 at contact
                float bobScale = Mathf.Lerp(0.8f, 1.2f, Mathf.InverseLerp(0.5f, 1.5f, _profile.weight));
                worldOffset += Vector3.up * (hop * _bobAmplitude * bobScale * (0.3f + _currentSpeedFactor) * Mathf.Clamp01(_currentSpeedFactor * 4f));

                pitch += (_currentSpeedFactor * _leanDegrees + _leanPitch) * alive + _overlap.x * 0.3f * MotionScale;
                roll += (_leanRoll + stepWave * _swayDegrees * _currentSpeedFactor) * alive + _overlap.y * 0.3f * MotionScale;
                yaw += -stepWave * _profile.torsoCounterTwist * 0.5f * _currentSpeedFactor * alive;

                float idle = 1f - _currentSpeedFactor;
                stretch += -footfall * _footfallSquash * _currentSpeedFactor;
                stretch += Mathf.Sin(_idleClock * Mathf.PI * 2f * _profile.breathRate) * _breathScale * idle;
                roll += Mathf.Sin(_idleClock * Mathf.PI * 2f / Mathf.Max(0.5f, _profile.weightShiftPeriod)) * _profile.weightShiftDegrees * idle;
                yaw += _turnLead * 0.3f * alive;

                if (_castTime >= 0f)
                {
                    float cast = LocomotionMath.Envelope(_castTime, _castRise, _castHold, _castRelease);
                    worldOffset += Vector3.up * (_castHop * cast);
                    pitch -= _castArchDegrees * cast;
                    stretch += 0.04f * cast;
                }

                if (_hitTime >= 0f)
                {
                    float flinch = LocomotionMath.Envelope(_hitTime, 0.04f, 0f, Mathf.Max(0.01f, _flinchDuration - 0.04f));
                    pitch -= _flinchDegrees * 0.6f * flinch * _profile.flinchScale;
                }

                if (x >= 0f)
                {
                    // Anticipation crouch (squash) while winding up, stretch through the impact.
                    float lunge = MotionFeelMath.AttackLungeAt(x);
                    stretch += lunge < 0f ? lunge * 0.12f : lunge * 0.03f;
                    yaw += twistUnit * _profile.attackTwistDegrees;
                }
            }

            if (x >= 0f)
            {
                float lunge = MotionFeelMath.AttackLungeAt(x);
                worldOffset += transform.forward * (lunge * _profile.attackLunge * (_rigMode ? 0.4f : 1f));
            }

            if (_hitTime >= 0f && _hitTime < _hitShakeDuration)
            {
                float shake = 1f - _hitTime / _hitShakeDuration;
                worldOffset += _hitShakeSeed * (_hitShakeMagnitude * shake * (_rigMode ? 0.5f : 1f) * _profile.flinchScale);
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

            // Volume-preserving squash & stretch, pivoting at the feet so they stay on the ground.
            Vector3 scale = MotionFeelMath.SquashScale(stretch);
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
        // Face (only with a blink blendshape)
        // ------------------------------------------------------------------

        private void TickBlink(float deltaTime)
        {
            if (_blinkRenderer == null || _blinkShape < 0)
            {
                return;
            }

            _nextBlink -= deltaTime;
            if (_blinkTime < 0f && _nextBlink <= 0f)
            {
                _blinkTime = 0f;
                _nextBlink = Random.Range(2.5f, 5.5f);
            }

            float weight = 0f;
            if (_blinkTime >= 0f)
            {
                _blinkTime += deltaTime;
                weight = LocomotionMath.Envelope(_blinkTime, 0.05f, 0.03f, 0.08f);
                if (_blinkTime > 0.16f)
                {
                    _blinkTime = -1f;
                }
            }

            _blinkRenderer.SetBlendShapeWeight(_blinkShape, (IsDead ? 1f : weight) * 100f);
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
