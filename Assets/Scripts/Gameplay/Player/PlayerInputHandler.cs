using Rapadura.Core.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Thin adapter over the New Input System's <see cref="PlayerInput"/> component.
    /// Exposes plain, per-frame input state (no events) so gameplay code can simply read
    /// values instead of wiring callbacks. Keyboard/mouse and gamepad come from the abstract actions
    /// in PlayerControls.inputactions; touch is tap/hold-to-move (MoveClick on
    /// &lt;Touchscreen&gt;/primaryTouch/press) plus the UI Toolkit HUD (HudController), which writes
    /// the Touch* properties below — an optional virtual joystick (<see cref="TouchMoveInput"/>), and
    /// Run/Jump/Atacar buttons. Deliberately no uGUI OnScreenStick/OnScreenButton: those feed a
    /// virtual Gamepad, which makes PlayerInput switch to the Gamepad scheme and unpair the
    /// Touchscreen (tap-to-move and pinch then stop working).
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

        /// <summary>Gameplay move vector: the Move action combined with <see cref="TouchMoveInput"/>, magnitude clamped to 1.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>
        /// Written by the HUD's virtual joystick (HudController), already dead-zoned and clamped to
        /// the unit circle; zero when released. Combined into <see cref="MoveInput"/> every frame, the
        /// same way <see cref="TouchRunHeld"/> is OR'd into <see cref="IsRunHeld"/>.
        /// </summary>
        public Vector2 TouchMoveInput { get; set; }
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

        /// <summary>
        /// Set by the HUD's "Atacar" button press (one-shot, like <see cref="TouchJumpRequested"/>):
        /// consumed the next Update into <see cref="AttackPressedThisFrame"/>. Only the pause gate
        /// applies to it — the "pointer over UI" gate exists to stop UI clicks from swinging, and
        /// this press IS a UI press.
        /// </summary>
        public bool TouchAttackRequested { get; set; }
        /// <summary>True the frame the click/tap-to-move button was pressed (MU Online-style point-and-click).</summary>
        public bool MoveClickPressedThisFrame { get; private set; }
        /// <summary>Current pointer (mouse/touch) position in screen space, for raycasting the click-to-move target.</summary>
        public Vector2 PointerPosition { get; private set; }

        /// <summary>
        /// True while the pointer that produced an accepted <see cref="MoveClickPressedThisFrame"/>
        /// (on the ground, not UI) is still held — <see cref="PlayerClickToMove"/> re-aims the walk at
        /// the pointer while this is set (MU-style hold-to-walk). Ends on release, pause, or a pinch.
        /// </summary>
        public bool IsMoveClickHeld { get; private set; }

        /// <summary>True the frame a second world finger turned the current hold/tap into a camera pinch.</summary>
        public bool MoveHoldCancelledByPinchThisFrame { get; private set; }

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

        /// <summary>
        /// <see cref="GatePresses(ref bool, ref bool, ref bool, ref bool, bool, bool)"/> plus the touch
        /// rules (EditMode-testable): the HUD Atacar press survives the pointer-over-UI gate (it is a UI
        /// press by design) but not a pause, and MoveClick is dropped while a camera pinch is latched
        /// (two world fingers, until every finger lifts — see <c>TouchGestureTracker</c>).
        /// </summary>
        public static void GatePresses(ref bool jump, ref bool interact, ref bool attack, ref bool moveClick, bool touchAttack, bool paused, bool pointerOverUi, bool pinchLatched)
        {
            GatePresses(ref jump, ref interact, ref attack, ref moveClick, paused, pointerOverUi);

            if (touchAttack && !paused)
            {
                attack = true;
            }

            if (pinchLatched)
            {
                moveClick = false;
            }
        }

        /// <summary>
        /// Pure hold-to-walk state step (EditMode-testable): an accepted press starts a hold; it lasts
        /// while the button/finger stays down, and ends on release, pause or pinch.
        /// </summary>
        public static bool NextMoveHoldState(bool wasHeld, bool acceptedPressThisFrame, bool stillPressed, bool paused, bool pinchLatched)
        {
            if (paused || pinchLatched || !stillPressed)
            {
                return false;
            }

            return acceptedPressThisFrame || wasHeld;
        }

        /// <summary>Pure combine of the Move action with the HUD joystick: summed, magnitude clamped to 1.</summary>
        public static Vector2 CombineMove(Vector2 actionMove, Vector2 touchMove)
        {
            return Vector2.ClampMagnitude(actionMove + touchMove, 1f);
        }

        private void Update()
        {
            MoveInput = CombineMove(_moveAction.ReadValue<Vector2>(), TouchMoveInput);
            LookInput = _lookAction.ReadValue<Vector2>();
            IsRunHeld = _runAction.IsPressed() || TouchRunHeld;
            IsCrouchHeld = _crouchAction.IsPressed();
            PointerPosition = _pointerPositionAction.ReadValue<Vector2>();

            bool jump = _jumpAction.WasPressedThisFrame() || TouchJumpRequested;
            TouchJumpRequested = false;
            bool interact = _interactAction.WasPressedThisFrame();
            bool attack = _attackAction.WasPressedThisFrame();
            bool touchAttack = TouchAttackRequested;
            TouchAttackRequested = false;
            bool moveClick = _moveClickAction.WasPressedThisFrame();
            bool paused = IsGameplayInputBlocked;
            // Picking UI panels isn't free — only ask when a pointer press actually happened.
            bool pointerOverUi = !paused && (attack || moveClick) && UiPointerQuery.IsPointerOverUi(PointerPosition);

            TouchGestureTracker.Refresh();
            bool pinchLatched = TouchGestureTracker.IsPinchLatched;
            GatePresses(ref jump, ref interact, ref attack, ref moveClick, touchAttack, paused, pointerOverUi, pinchLatched);

            bool wasHeld = IsMoveClickHeld;
            IsMoveClickHeld = NextMoveHoldState(wasHeld, moveClick, _moveClickAction.IsPressed(), paused, pinchLatched);
            MoveHoldCancelledByPinchThisFrame = wasHeld && pinchLatched;

            JumpPressedThisFrame = jump;
            InteractPressedThisFrame = interact;
            AttackPressedThisFrame = attack;
            MoveClickPressedThisFrame = moveClick;
        }
    }
}
