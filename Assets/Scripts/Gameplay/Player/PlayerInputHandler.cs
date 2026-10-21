using Rapadura.Core.Systems;
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

        /// <summary>Number of Skill1..SkillN actions in PlayerControls.inputactions (one per hotbar slot).</summary>
        public const int SkillActionCount = 5;
        private readonly InputAction[] _skillActions = new InputAction[SkillActionCount];

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsRunHeld { get; private set; }

        /// <summary>
        /// Set/cleared by an on-screen touch button (see HudController's run button) since there's
        /// no physical "held" key to bind on mobile. OR'd into <see cref="IsRunHeld"/> every frame —
        /// keyboard/gamepad Run keeps working unchanged alongside it.
        /// </summary>
        public bool TouchRunHeld { get; set; }
        public bool IsCrouchHeld { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }

        /// <summary>
        /// Set by an on-screen touch button's press event (see HudController's jump button).
        /// Unlike <see cref="TouchRunHeld"/> this is a one-shot request, consumed (cleared) the
        /// same frame it's read into <see cref="JumpPressedThisFrame"/> — mirroring
        /// WasPressedThisFrame's single-frame-true semantics for the physical Jump action.
        /// </summary>
        public bool TouchJumpRequested { get; set; }
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

            // FindAction (not the indexer) so an older/custom actions asset without Skill1..5
            // just leaves the hotbar keys unbound instead of throwing in Awake.
            for (int i = 0; i < SkillActionCount; i++)
            {
                _skillActions[i] = _playerInput.actions.FindAction("Skill" + (i + 1), throwIfNotFound: false);
            }
        }

        /// <summary>
        /// True the frame the hotbar key for <paramref name="slotIndex"/> (0-based: Skill1 = 0) was
        /// pressed. Queried straight from the action rather than cached in <see cref="Update"/>, so
        /// the result is correct regardless of script execution order relative to the reader
        /// (see <c>SkillHotbar</c>). Touch hotbar buttons call <c>SkillHotbar.TryCastSlot</c> directly.
        /// </summary>
        public bool WasSkillPressedThisFrame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SkillActionCount)
            {
                return false;
            }

            // A modal (shop, inventory, dialogue...) or the pause menu is up: 1-5 must not cast behind it.
            if (IsGameplayInputBlocked)
            {
                return false;
            }

            InputAction action = _skillActions[slotIndex];
            return action != null && action.WasPressedThisFrame();
        }

        /// <summary>Gameplay is frozen (GameplayPause modal owner, pause menu, or any timeScale-0 pause).</summary>
        private static bool IsGameplayInputBlocked => GameplayPause.IsPaused || Time.timeScale == 0f;

        /// <summary>
        /// Pure gate applied to the raw one-shot presses each frame (EditMode-testable): a pause drops
        /// every press, and a pointer over UI drops the two that share the left mouse button with UI
        /// clicks (Attack, MoveClick) — otherwise clicking Buy, an inventory slot or the HUD swung an
        /// attack.
        /// </summary>
        public static void GatePresses(ref bool jump, ref bool interact, ref bool attack, ref bool moveClick, bool paused, bool pointerOverUi)
        {
            if (paused)
            {
                jump = interact = attack = moveClick = false;
                return;
            }

            if (pointerOverUi)
            {
                attack = false;
                moveClick = false;
            }
        }

        private void Update()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            LookInput = _lookAction.ReadValue<Vector2>();
            IsRunHeld = _runAction.IsPressed() || TouchRunHeld;
            IsCrouchHeld = _crouchAction.IsPressed();
            PointerPosition = _pointerPositionAction.ReadValue<Vector2>();

            bool jump = _jumpAction.WasPressedThisFrame() || TouchJumpRequested;
            TouchJumpRequested = false;
            bool interact = _interactAction.WasPressedThisFrame();
            bool attack = _attackAction.WasPressedThisFrame();
            bool moveClick = _moveClickAction.WasPressedThisFrame();
            bool paused = IsGameplayInputBlocked;
            // Picking UI panels isn't free — only ask when a pointer press actually happened.
            bool pointerOverUi = !paused && (attack || moveClick) && UiPointerQuery.IsPointerOverUi(PointerPosition);
            GatePresses(ref jump, ref interact, ref attack, ref moveClick, paused, pointerOverUi);

            JumpPressedThisFrame = jump;
            InteractPressedThisFrame = interact;
            AttackPressedThisFrame = attack;
            MoveClickPressedThisFrame = moveClick;
        }
    }
}
