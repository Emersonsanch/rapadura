using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Humanoid foot planting, on the rigged hero's <see cref="Animator"/> GameObject (added at
    /// runtime by <see cref="PlayerProceduralAnimator"/>; needs "IK Pass" on the base layer, which
    /// <c>Rapadura/Animation/Rebuild Hero Animators</c> turns on for Humanoid controllers —
    /// without it <see cref="OnAnimatorIK"/> is simply never called and nothing changes).
    /// <list type="bullet">
    /// <item><b>Ground:</b> each foot's IK goal is the animated foot moved by how far the ground under it
    /// is above/below the ground under the root (one short ray per foot), so the clip's own lift is kept
    /// (stride matching still owns sliding) while the sole follows slopes and steps; the sole aligns to
    /// the ground normal; the pelvis drops by the lower foot's offset so it can reach.</item>
    /// <item><b>Body offset:</b> the procedural layer's hip motion (idle weight shift, extra step bob,
    /// landing dip) is applied to the pelvis here while the feet keep their targets — the knees bend
    /// instead of the feet sinking or floating.</item>
    /// <item><b>Foot lock:</b> standing (and turning in place) the planted feet are pinned in world space;
    /// when the body twists/drifts too far from a pinned foot it takes a short arced re-plant step, one
    /// foot at a time — no skating while pivoting.</item>
    /// <item><b>Air:</b> the feet tuck up by <see cref="AirTuck"/> (knees bend through the jump arc).</item>
    /// </list>
    /// Scaled time, no allocations (non-alloc raycasts into a fixed buffer).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerFootIK : MonoBehaviour
    {
        [Tooltip("Layers the feet stand on. Triggers, CharacterControllers and rigidbodies (other creatures) are always ignored.")]
        [SerializeField] private LayerMask _groundMask = Physics.DefaultRaycastLayers;
        [Tooltip("Ray starts this far above the root and reaches this far below it (m).")]
        [SerializeField] private float _rayUp = 0.5f;
        [SerializeField] private float _rayDown = 0.6f;
        [Tooltip("Largest ground height difference (m) a foot adapts to.")]
        [SerializeField] private float _maxOffset = 0.35f;
        [Tooltip("Smoothing rate of the ground offsets/pelvis (1/s).")]
        [SerializeField] private float _offsetSharpness = 18f;

        [Header("Foot lock (standing / turning in place)")]
        [Tooltip("Below this ground speed (m/s) the feet are pinned.")]
        [SerializeField] private float _lockSpeed = 0.15f;
        [Tooltip("A pinned foot re-plants when the animation wants it this far away (m)…")]
        [SerializeField] private float _lockMaxDrift = 0.16f;
        [Tooltip("…or when the body turned this far (deg) since it was planted.")]
        [SerializeField] private float _lockMaxTwist = 40f;
        [SerializeField, Min(0.05f)] private float _stepDuration = 0.18f;
        [SerializeField] private float _stepHeight = 0.06f;
        [Tooltip("Seconds to blend a pinned foot back to the animation when walking off.")]
        [SerializeField, Min(0.01f)] private float _lockBlendTime = 0.12f;

        private struct Foot
        {
            public AvatarIKGoal Goal;
            public float Offset;
            public Vector3 Normal;
            public bool Locked;
            public float LockWeight;
            public Vector3 LockPosition;
            public Quaternion LockRotation;
            public float LockYaw;
            public float StepTime; // < 0 = not stepping
            public Vector3 StepFrom;
            public Quaternion StepFromRotation;
            public float RestHeight; // lowest animated ankle height above the root seen (slowly forgotten)
        }

        private readonly RaycastHit[] _hits = new RaycastHit[8];
        private Animator _animator;
        private PlayerMotor _motor;
        private Transform _owner;
        private Foot _left;
        private Foot _right;
        private float _pelvis;
        private float _groundWeight = 1f;
        private float _tuck;

        /// <summary>Extra world-space pelvis offset from the procedural layer (weight shift, bob, landing dip). Feet stay put.</summary>
        public Vector3 BodyOffset { get; set; }

        /// <summary>Metres the feet tuck up while airborne (set by the procedural layer from the profile and the jump arc).</summary>
        public float AirTuck { get; set; }

        /// <summary>True once OnAnimatorIK has run at least once (the controller has IK Pass on).</summary>
        public bool IsActive { get; private set; }

        public void Initialize(PlayerMotor motor, Transform owner)
        {
            _motor = motor;
            _owner = owner != null ? owner : transform;
            _animator = GetComponent<Animator>();
            _left = new Foot { Goal = AvatarIKGoal.LeftFoot, Normal = Vector3.up, StepTime = -1f, RestHeight = float.PositiveInfinity };
            _right = new Foot { Goal = AvatarIKGoal.RightFoot, Normal = Vector3.up, StepTime = -1f, RestHeight = float.PositiveInfinity };
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != 0)
            {
                return;
            }

            if (_animator == null)
            {
                Initialize(GetComponentInParent<PlayerMotor>(), transform.root);
            }

            if (_animator == null || !_animator.isHuman)
            {
                return;
            }

            if (_motor == null)
            {
                _motor = GetComponentInParent<PlayerMotor>();
            }

            IsActive = true;
            float deltaTime = Time.deltaTime;
            bool grounded = _motor == null || _motor.IsGrounded;
            float speed = _motor != null ? _motor.HorizontalVelocity.magnitude : 0f;
            float rootY = _owner.position.y;
            float k = LocomotionMath.ExpBlend(_offsetSharpness, deltaTime);

            _groundWeight = Mathf.MoveTowards(_groundWeight, grounded ? 1f : 0f, deltaTime / 0.08f);
            _tuck = Mathf.Lerp(_tuck, grounded ? 0f : AirTuck, k);

            Vector3 leftAnimated = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Vector3 rightAnimated = _animator.GetIKPosition(AvatarIKGoal.RightFoot);
            Quaternion leftRotation = _animator.GetIKRotation(AvatarIKGoal.LeftFoot);
            Quaternion rightRotation = _animator.GetIKRotation(AvatarIKGoal.RightFoot);

            ProbeGround(ref _left, leftAnimated, rootY, grounded, k);
            ProbeGround(ref _right, rightAnimated, rootY, grounded, k);

            _pelvis = Mathf.Lerp(_pelvis, MotionFeelMath.PelvisOffset(_left.Offset, _right.Offset) * _groundWeight, k);
            _animator.bodyPosition += Vector3.up * _pelvis + BodyOffset;

            bool lockFeet = grounded && speed < _lockSpeed;
            float yaw = _owner.eulerAngles.y;
            bool otherStepping = _left.StepTime >= 0f || _right.StepTime >= 0f;

            Apply(ref _left, leftAnimated, leftRotation, lockFeet && IsPlanted(ref _left, leftAnimated.y - rootY, deltaTime), yaw, ref otherStepping, deltaTime);
            Apply(ref _right, rightAnimated, rightRotation, lockFeet && IsPlanted(ref _right, rightAnimated.y - rootY, deltaTime), yaw, ref otherStepping, deltaTime);
        }

        /// <summary>A foot only gets pinned once it is down (near the lowest ankle height seen) — never mid-swing in the air.</summary>
        private static bool IsPlanted(ref Foot foot, float height, float deltaTime)
        {
            foot.RestHeight = float.IsPositiveInfinity(foot.RestHeight) ? height : Mathf.Min(foot.RestHeight + 0.01f * deltaTime, height);
            return foot.Locked || height <= foot.RestHeight + 0.03f;
        }

        private void ProbeGround(ref Foot foot, Vector3 animated, float rootY, bool grounded, float k)
        {
            float target = 0f;
            Vector3 normal = Vector3.up;

            if (grounded)
            {
                Vector3 origin = new Vector3(animated.x, rootY + _rayUp, animated.z);
                int count = Physics.RaycastNonAlloc(origin, Vector3.down, _hits, _rayUp + _rayDown, _groundMask, QueryTriggerInteraction.Ignore);
                float best = float.PositiveInfinity;
                float groundY = 0f;
                for (int i = 0; i < count; i++)
                {
                    RaycastHit hit = _hits[i];
                    if (hit.distance >= best || hit.collider is CharacterController || hit.rigidbody != null || hit.collider.transform.IsChildOf(_owner))
                    {
                        continue;
                    }

                    best = hit.distance;
                    groundY = hit.point.y;
                    normal = hit.normal;
                }

                target = MotionFeelMath.FootGroundOffset(!float.IsPositiveInfinity(best), groundY, rootY, _maxOffset);
            }

            foot.Offset = Mathf.Lerp(foot.Offset, target, k);
            foot.Normal = Vector3.Slerp(foot.Normal, normal, k).normalized;
        }

        private void Apply(ref Foot foot, Vector3 animated, Quaternion animatedRotation, bool lockFeet, float yaw, ref bool someoneStepping, float deltaTime)
        {
            Vector3 position = animated + Vector3.up * (foot.Offset * _groundWeight + _tuck);
            Quaternion rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(Vector3.up, foot.Normal), _groundWeight) * animatedRotation;

            if (lockFeet)
            {
                if (!foot.Locked)
                {
                    foot.Locked = true;
                    foot.LockPosition = position;
                    foot.LockRotation = rotation;
                    foot.LockYaw = yaw;
                    foot.StepTime = -1f;
                }

                if (foot.StepTime >= 0f)
                {
                    // Arced re-plant toward where the animation wants the foot now.
                    foot.StepTime += deltaTime;
                    float u = Mathf.Clamp01(foot.StepTime / _stepDuration);
                    float eased = Mathf.SmoothStep(0f, 1f, u);
                    Vector3 stepped = Vector3.Lerp(foot.StepFrom, position, eased) + Vector3.up * MotionFeelMath.StepArc(u, _stepHeight);
                    Quaternion steppedRotation = Quaternion.Slerp(foot.StepFromRotation, rotation, eased);
                    if (u >= 1f)
                    {
                        foot.StepTime = -1f;
                        foot.LockPosition = position;
                        foot.LockRotation = rotation;
                        foot.LockYaw = yaw;
                    }

                    position = stepped;
                    rotation = steppedRotation;
                }
                else
                {
                    float twist = Mathf.DeltaAngle(foot.LockYaw, yaw);
                    if (!someoneStepping && MotionFeelMath.ShouldReleaseFootLock(foot.LockPosition, position, twist, _lockMaxDrift, _lockMaxTwist))
                    {
                        foot.StepTime = 0f;
                        foot.StepFrom = foot.LockPosition;
                        foot.StepFromRotation = foot.LockRotation;
                        someoneStepping = true;
                    }

                    position = Vector3.Lerp(position, foot.LockPosition, foot.LockWeight);
                    rotation = Quaternion.Slerp(rotation, foot.LockRotation, foot.LockWeight);
                }

                foot.LockWeight = Mathf.MoveTowards(foot.LockWeight, 1f, deltaTime / _lockBlendTime);
            }
            else
            {
                if (foot.Locked)
                {
                    foot.Locked = false;
                    foot.StepTime = -1f;
                }

                foot.LockWeight = Mathf.MoveTowards(foot.LockWeight, 0f, deltaTime / _lockBlendTime);
                if (foot.LockWeight > 0f)
                {
                    position = Vector3.Lerp(position, foot.LockPosition, foot.LockWeight);
                    rotation = Quaternion.Slerp(rotation, foot.LockRotation, foot.LockWeight);
                }
            }

            _animator.SetIKPositionWeight(foot.Goal, 1f);
            _animator.SetIKRotationWeight(foot.Goal, _groundWeight);
            _animator.SetIKPosition(foot.Goal, position);
            _animator.SetIKRotation(foot.Goal, rotation);
        }
    }
}
