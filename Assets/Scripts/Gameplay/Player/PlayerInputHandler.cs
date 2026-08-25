using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Thin adapter over the New Input System's <see cref="PlayerInput"/> component.
    /// Exposes plain, per-frame input state (no events) so gameplay code can simply read
    /// values instead of wiring callbacks. Works uniformly across keyboard/mouse, gamepad,
    /// and touch (virtual joystick via OnScreenStick + on-screen buttons), since all three
    /// control schemes are bound to the same abstract actions in PlayerControls.inputactions.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerInput _playerInput;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _runAction;
        private InputAction _crouchAction;
        private InputAction _interactAction;
        private InputAction _attackAction;
        private InputAction _moveClickAction;
        private InputAction _pointerPositionAction;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsRunHeld { get; private set; }
        public bool IsCrouchHeld { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }
        public bool InteractPressedThisFrame { get; private set; }
        public bool AttackPressedThisFrame { get; private set; }
        /// <summary>True the frame the click/tap-to-move button was pressed (MU Online-style point-and-click).</summary>
        public bool MoveClickPressedThisFrame { get; private set; }
        /// <summary>Current pointer (mouse/touch) position in screen space, for raycasting the click-to-move target.</summary>
        public Vector2 PointerPosition { get; private set; }

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();

            _moveAction = _playerInput.actions["Move"];
            _lookAction = _playerInput.actions["Look"];
            _jumpAction = _playerInput.actions["Jump"];
            _runAction = _playerInput.actions["Run"];
            _crouchAction = _playerInput.actions["Crouch"];
            _interactAction = _playerInput.actions["Interact"];
            _attackAction = _playerInput.actions["Attack"];
            _moveClickAction = _playerInput.actions["MoveClick"];
            _pointerPositionAction = _playerInput.actions["PointerPosition"];
        }

        private void Update()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            LookInput = _lookAction.ReadValue<Vector2>();
            IsRunHeld = _runAction.IsPressed();
            IsCrouchHeld = _crouchAction.IsPressed();
            JumpPressedThisFrame = _jumpAction.WasPressedThisFrame();
            InteractPressedThisFrame = _interactAction.WasPressedThisFrame();
            AttackPressedThisFrame = _attackAction.WasPressedThisFrame();
            MoveClickPressedThisFrame = _moveClickAction.WasPressedThisFrame();
            PointerPosition = _pointerPositionAction.ReadValue<Vector2>();
        }
    }
}
