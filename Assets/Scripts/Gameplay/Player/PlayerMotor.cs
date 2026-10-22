using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Low-level movement executor built on <see cref="CharacterController"/>. Knows nothing
    /// about input or states — it only exposes intent methods (Move, Jump, SetCrouching) and
    /// integrates gravity every physics step. Kept separate from PlayerController/states so the
    /// same motor could later drive NPCs or be swapped for a physics-based rig without touching
    /// state or input code.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float _walkSpeed = 2.5f;
        [SerializeField] private float _runSpeed = 5.5f;
        [SerializeField] private float _crouchSpeed = 1.4f;

        [Header("Acceleration (no instant start/stop)")]
        [Tooltip("Seconds to go from standing to run speed (walk speed is reached proportionally sooner).")]
        [SerializeField, Min(0.01f)] private float _accelerationTime = 0.16f;
        [Tooltip("Seconds to brake from run speed to a stop when input is released.")]
        [SerializeField, Min(0.01f)] private float _decelerationTime = 0.12f;
        [Tooltip("Acceleration multiplier by current/target speed (x 0..1): >1 at the start gives a snappy first step, <1 near the top eases into full speed.")]
        [SerializeField] private AnimationCurve _accelerationCurve = new AnimationCurve(
            new Keyframe(0f, 1.6f, 0f, -1.2f), new Keyframe(1f, 0.55f, -0.6f, 0f));
        [Tooltip("How fast (deg/s) the movement direction itself can bend — arcs instead of kinks.")]
        [SerializeField] private float _directionTurnSpeed = 900f;
        [Tooltip("Direction changes sharper than this (deg) snap instead of arcing (a reversal), keeping only part of the speed.")]
        [SerializeField, Range(90f, 180f)] private float _reversalAngle = 150f;
        [SerializeField, Range(0f, 1f)] private float _reversalSpeedKept = 0.35f;

        [Header("Turning (body facing)")]
        [Tooltip("Facing turn rate (deg/s) while moving at full speed.")]
        [SerializeField] private float _turnSpeedMoving = 720f;
        [Tooltip("Facing turn rate (deg/s) when (nearly) stopped — slightly snappier to turn in place.")]
        [SerializeField] private float _turnSpeedStopped = 1000f;

        [Header("Click-to-move / Hold-to-walk")]
        [Tooltip("Seconds the steering destination takes to catch up with hold-to-walk re-aims (0 = no smoothing). Removes zig-zag at each 0.15 s re-aim.")]
        [SerializeField, Min(0f)] private float _destinationSmoothing = 0.12f;
        [Tooltip("A new destination more than this many degrees away from the current heading is a new order and snaps (no smoothing).")]
        [SerializeField, Range(10f, 180f)] private float _destinationSnapAngle = 70f;
        [Tooltip("Braking (m/s²) used to ease into the click destination instead of stopping dead.")]
        [SerializeField, Min(0.1f)] private float _arrivalDeceleration = 10f;
        [Tooltip("Lowest speed while easing into the destination, so the last centimetres don't crawl.")]
        [SerializeField, Min(0f)] private float _minArrivalSpeed = 0.7f;
        [Tooltip("Already past the destination (moving away from it) within this distance counts as arrived — no tiny back-step.")]
        [SerializeField, Min(0f)] private float _passedArrivalTolerance = 0.45f;

        [Header("Jump & Gravity")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -18f;
        [SerializeField] private float _groundedStickForce = -2f;

        [Header("Crouch")]
        [SerializeField] private float _standingHeight = 1.8f;
        [SerializeField] private float _crouchingHeight = 1.0f;
        [SerializeField] private float _crouchTransitionSpeed = 8f;

        private CharacterController _controller;
        private float _verticalVelocity;
        private bool _isCrouching;
        private float _targetHeight;

        private float _currentSpeed;
        private Vector3 _moveDirection;
        private Vector3 _smoothedDestination;
        private bool _hasSmoothedDestination;

        public bool IsGrounded { get; private set; }

        /// <summary>Actual (accelerated, buffed) horizontal velocity this step — what the Animator and procedural layers read.</summary>
        public Vector3 HorizontalVelocity { get; private set; }

        /// <summary>Speed (m/s, buff multiplier included) the motor is currently accelerating toward; 0 with no move intent or while rooted.</summary>
        public float TargetHorizontalSpeed { get; private set; }

        /// <summary>Normalized direction the latest move request wanted to go (zero when there was none) — the procedural head-look leads turns with it.</summary>
        public Vector3 DesiredDirection { get; private set; }

        /// <summary>Base walk speed (m/s, before buffs) — the Animator blend maps it to 0.5.</summary>
        public float WalkSpeed => _walkSpeed;

        /// <summary>Base run speed (m/s, before buffs) — the Animator blend maps it to 1.</summary>
        public float RunSpeed => _runSpeed;

        public bool IsCrouching => _isCrouching;
        public float VerticalVelocity => _verticalVelocity;

        private BuffController _buffController;

        /// <summary>
        /// Sibling BuffController's movement multiplier (<see cref="BuffController.GetMoveSpeedMultiplier"/>:
        /// Passo do Vento +50%, dash +150%, slows, 0 while rooted), 1 without one. Applied to every
        /// horizontal move, so <see cref="HorizontalVelocity"/> — and the Animator "Speed" fed from it by
        /// <see cref="PlayerAnimatorSpeedDriver"/> — reflects the real, buffed speed.
        /// </summary>
        public float SpeedMultiplier
        {
            get
            {
                if (_buffController == null)
                {
                    _buffController = GetComponent<BuffController>();
                }

                return _buffController != null ? _buffController.GetMoveSpeedMultiplier() : 1f;
            }
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _targetHeight = _standingHeight;
            _controller.height = _standingHeight;
        }

        /// <summary>Edit Mode guard: AddComponent doesn't run Awake outside Play Mode (same as SkillManager).</summary>
        private CharacterController Controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller = GetComponent<CharacterController>();
                }

                return _controller;
            }
        }

        /// <summary>
        /// One physics step with no horizontal movement: integrates gravity (or the grounded stick
        /// force) and refreshes <see cref="IsGrounded"/>. Every grounded state that isn't moving calls
        /// this so the character can never hang in the air.
        /// </summary>
        public void ApplyGravityOnly()
        {
            // Not following a destination any more: the next click starts unsmoothed.
            _hasSmoothedDestination = false;
            MoveInDirection(Vector3.zero, 0f);
        }

        /// <summary>
        /// Teleports the character (CharacterController overrides transform writes while enabled, so it
        /// is toggled around the move) and resets vertical motion.
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            CharacterController controller = Controller;
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled)
            {
                controller.enabled = false;
            }

            transform.SetPositionAndRotation(position, rotation);

            if (wasEnabled)
            {
                controller.enabled = true;
            }

            _verticalVelocity = 0f;
            _currentSpeed = 0f;
            HorizontalVelocity = Vector3.zero;
            _hasSmoothedDestination = false;
        }

        /// <summary>
        /// Drops the character's feet (pivot) onto the ground right below it (probing from
        /// <paramref name="probeUp"/> above, so a spawn slightly sunk in also resolves), ignoring its
        /// own colliders and triggers. Returns false (no move) when nothing is below.
        /// </summary>
        public bool SnapToGround(LayerMask groundMask, float probeUp = 1f, float probeDown = 50f)
        {
            Physics.SyncTransforms();
            Vector3 position = transform.position;
            Vector3 origin = position + Vector3.up * probeUp;
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, probeUp + probeDown, groundMask, QueryTriggerInteraction.Ignore);

            float bestDistance = float.PositiveInfinity;
            float groundY = 0f;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = hit.distance;
                groundY = hit.point.y;
            }

            if (float.IsPositiveInfinity(bestDistance))
            {
                return false;
            }

            Teleport(new Vector3(position.x, groundY, position.z), transform.rotation);
            _verticalVelocity = _groundedStickForce;
            IsGrounded = true;
            return true;
        }

        private void Update()
        {
            UpdateCrouchHeight();
        }

        /// <summary>Moves the character on the XZ plane relative to the given camera-forward/right axes.</summary>
        public void Move(Vector2 moveInput, bool isRunning, Transform cameraTransform)
        {
            Move(moveInput, isRunning, cameraTransform, speedOverride: null);
        }

        /// <summary>
        /// Same as <see cref="Move(Vector2, bool, Transform)"/> but with an explicit speed,
        /// bypassing the walk/run/crouch speed table. Used by states like Slide that need a
        /// burst of speed decoupled from the current stance.
        /// </summary>
        public void Move(Vector2 moveInput, bool isRunning, Transform cameraTransform, float? speedOverride)
        {
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 desiredDirection = (forward * moveInput.y + right * moveInput.x);
            desiredDirection = Vector3.ClampMagnitude(desiredDirection, 1f);

            float speed = speedOverride ?? (_isCrouching ? _crouchSpeed : (isRunning ? _runSpeed : _walkSpeed));
            _hasSmoothedDestination = false;
            MoveInDirection(desiredDirection, speed);
        }

        /// <summary>
        /// Moves straight toward a world-space point (MU Online-style click-to-move), ignoring
        /// camera orientation entirely — the direction is just target minus current position,
        /// flattened to the XZ plane. Returns true once the character has arrived within
        /// <paramref name="arrivalThreshold"/> of the destination (caller should stop calling
        /// this and let the motor fall back to idle/gravity-only once true).
        /// </summary>
        public bool MoveToward(Vector3 destination, bool isRunning, float arrivalThreshold = 0.15f)
        {
            Vector3 position = transform.position;
            Vector3 toDestination = destination - position;
            toDestination.y = 0f;

            if (LocomotionMath.HasArrived(toDestination, HorizontalVelocity, arrivalThreshold, Mathf.Max(arrivalThreshold, _passedArrivalTolerance)))
            {
                // Arrived: the remaining (already eased-down) speed bleeds off through the normal
                // deceleration in the following gravity-only steps instead of a dead stop.
                _hasSmoothedDestination = false;
                MoveInDirection(Vector3.zero, 0f);
                return true;
            }

            float deltaTime = Time.deltaTime;

            // Hold-to-walk re-aims the destination every ~0.15 s: steer toward a low-passed copy so
            // the heading glides instead of kinking at each re-aim (a new order in a clearly
            // different direction snaps, see SmoothDestination).
            _smoothedDestination = _hasSmoothedDestination
                ? LocomotionMath.SmoothDestination(position, _smoothedDestination, destination, deltaTime, _destinationSmoothing, _destinationSnapAngle)
                : destination;
            _hasSmoothedDestination = true;

            Vector3 steer = _smoothedDestination - position;
            steer.y = 0f;
            if (steer.sqrMagnitude < 1e-6f)
            {
                steer = toDestination;
            }

            float distance = toDestination.magnitude;
            float speed = _isCrouching ? _crouchSpeed : (isRunning ? _runSpeed : _walkSpeed);

            // Ease into the point (v = √(2·a·d)) and never step past it in one frame.
            float cap = LocomotionMath.ArrivalSpeed(distance - arrivalThreshold, float.MaxValue, _arrivalDeceleration, _minArrivalSpeed);
            if (deltaTime > 0f)
            {
                cap = Mathf.Min(cap, distance / deltaTime);
            }

            MoveInDirection(steer.normalized, speed, cap);
            return false;
        }

        /// <summary>
        /// Shared movement integration: ramps speed (acceleration/deceleration), bends the movement
        /// direction at a bounded rate, turns the body at a bounded rate, then moves the
        /// CharacterController with gravity. <paramref name="speedCap"/> limits the final (buffed)
        /// speed — used for arrival braking.
        /// </summary>
        private void MoveInDirection(Vector3 desiredDirection, float speed, float speedCap = float.MaxValue)
        {
            CharacterController controller = Controller;
            if (controller == null || !controller.enabled)
            {
                return;
            }

            IsGrounded = controller.isGrounded;
            float deltaTime = Time.deltaTime;

            desiredDirection.y = 0f;
            bool hasIntent = desiredDirection.sqrMagnitude > 0.0001f && speed > 0f;
            float multiplier = SpeedMultiplier;

            if (multiplier <= 0f)
            {
                // Rooted: hard stop, no sliding out of a root.
                _currentSpeed = 0f;
                TargetHorizontalSpeed = 0f;
            }
            else
            {
                float inputMagnitude = Mathf.Clamp01(desiredDirection.magnitude);
                float target = hasIntent ? Mathf.Min(speed * multiplier * inputMagnitude, speedCap) : 0f;
                TargetHorizontalSpeed = target;

                if (hasIntent)
                {
                    Vector3 desired = desiredDirection.normalized;
                    bool reversal = _moveDirection.sqrMagnitude > 1e-6f && Vector3.Angle(_moveDirection, desired) > _reversalAngle;
                    _moveDirection = LocomotionMath.StepDirection(_moveDirection, desired, _directionTurnSpeed, deltaTime, _reversalAngle);

                    if (reversal)
                    {
                        _currentSpeed *= _reversalSpeedKept;
                    }
                }

                float referenceSpeed = Mathf.Max(_runSpeed * multiplier, target);
                float normalized = target > 0.001f ? Mathf.Clamp01(_currentSpeed / target) : 1f;
                float curveFactor = _accelerationCurve != null && _accelerationCurve.length > 0 ? _accelerationCurve.Evaluate(normalized) : 1f;
                float acceleration = LocomotionMath.AccelerationFor(referenceSpeed, _accelerationTime, curveFactor);
                float deceleration = LocomotionMath.AccelerationFor(referenceSpeed, _decelerationTime);

                // Arrival braking caps are hard limits (stepping past the destination looks worse
                // than a firm last brake), regular slow-downs ease.
                _currentSpeed = _currentSpeed > speedCap
                    ? speedCap
                    : LocomotionMath.StepSpeed(_currentSpeed, target, acceleration, deceleration, deltaTime);
            }

            if (_currentSpeed <= 0.0001f)
            {
                _currentSpeed = 0f;
            }

            HorizontalVelocity = _moveDirection * _currentSpeed;
            DesiredDirection = hasIntent ? desiredDirection.normalized : Vector3.zero;

            if (hasIntent && multiplier > 0f)
            {
                float normalizedSpeed = _runSpeed > 0f ? _currentSpeed / _runSpeed : 0f;
                float turnRate = LocomotionMath.TurnRate(_turnSpeedMoving, _turnSpeedStopped, normalizedSpeed);
                transform.rotation = LocomotionMath.StepFacing(transform.rotation, desiredDirection, turnRate, deltaTime);
            }

            ApplyGravity();

            Vector3 finalVelocity = HorizontalVelocity + Vector3.up * _verticalVelocity;
            controller.Move(finalVelocity * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (IsGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedStickForce;
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
        }

        /// <summary>Applies an instantaneous upward velocity to achieve the configured jump height.</summary>
        public void Jump()
        {
            if (!IsGrounded)
            {
                return;
            }

            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        public void SetCrouching(bool crouching)
        {
            _isCrouching = crouching;
            _targetHeight = crouching ? _crouchingHeight : _standingHeight;
        }

        private void UpdateCrouchHeight()
        {
            _controller.height = Mathf.Lerp(_controller.height, _targetHeight, _crouchTransitionSpeed * Time.deltaTime);
            _controller.center = new Vector3(0f, _controller.height / 2f, 0f);
        }
    }
}
