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
        [SerializeField] private float _rotationSmoothing = 12f;

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

        public bool IsGrounded { get; private set; }
        public Vector3 HorizontalVelocity { get; private set; }
        public bool IsCrouching => _isCrouching;
        public float VerticalVelocity => _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _targetHeight = _standingHeight;
            _controller.height = _standingHeight;
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
            Vector3 toDestination = destination - transform.position;
            toDestination.y = 0f;

            if (toDestination.magnitude <= arrivalThreshold)
            {
                MoveInDirection(Vector3.zero, 0f);
                return true;
            }

            float speed = _isCrouching ? _crouchSpeed : (isRunning ? _runSpeed : _walkSpeed);
            MoveInDirection(toDestination.normalized, speed);
            return false;
        }

        /// <summary>Shared movement integration: rotates toward, accelerates along, and physically moves along a world-space direction.</summary>
        private void MoveInDirection(Vector3 desiredDirection, float speed)
        {
            IsGrounded = _controller.isGrounded;

            HorizontalVelocity = desiredDirection * speed;

            if (desiredDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSmoothing * Time.deltaTime);
            }

            ApplyGravity();

            Vector3 finalVelocity = HorizontalVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(finalVelocity * Time.deltaTime);
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
