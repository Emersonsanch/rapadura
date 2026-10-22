using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Core.StateMachine;
using Rapadura.Gameplay.Player.States;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Serializable snapshot of the player's world position/rotation used by the save system.
    /// </summary>
    [System.Serializable]
    public class PlayerTransformSaveState
    {
        public float posX, posY, posZ;
        public float rotY;
    }

    /// <summary>
    /// Top-level orchestrator for the player character. Owns references to the supporting
    /// components (input, motor, stats, animator, camera) and drives the finite-state machine
    /// that decides which behavior is active each frame. Individual states contain the actual
    /// movement logic; this class only wires dependencies and forwards Unity callbacks.
    /// </summary>
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour, ISaveable, ISceneLocalSaveable
    {
        // ISceneLocalSaveable: the saved position belongs to the scene it was captured in — carrying
        // it into the next Main.unity load would override the encounter arena's player spawn.
        [SerializeField] private Animator _animator;
        [SerializeField] private Rapadura.Gameplay.Cameras.CameraController _playerCamera;
        [SerializeField] private Transform _cameraLookTransform;

        [Tooltip("Run the states' movement step (FixedTick) every rendered frame instead of on the fixed physics step. CharacterController has no interpolation, so moving it at 50 Hz on a 60/120 Hz screen made the hero visibly judder against the smoothly-following camera. Off = legacy FixedUpdate movement.")]
        [SerializeField] private bool _moveEveryFrame = true;

        public PlayerInputHandler Input { get; private set; }
        public PlayerMotor Motor { get; private set; }
        public PlayerStats Stats { get; private set; }
        public PlayerClickToMove ClickToMove { get; private set; }
        public Animator Animator => _animator;
        public Transform CameraTransform => _cameraLookTransform != null ? _cameraLookTransform : Camera.main != null ? Camera.main.transform : transform;
        public StateMachine<PlayerController> StateMachine { get; private set; }

        /// <summary>Optional — not every prefab needs crowd-control (see <see cref="IsStunned"/>). Cached the same
        /// way SkillManager/ComboTracker/InventoryManager already look it up (GetComponent, no RequireComponent).</summary>
        private BuffController _buffController;

        /// <summary>True while the player is stunned and should not process movement/attack input this frame.</summary>
        public bool IsStunned => _buffController != null && _buffController.IsStunned;

        /// <summary>True if the player wants to be moving this frame — either manual (WASD/joystick) input,
        /// or a pending click-to-move destination (see <see cref="ClickToMove"/>). States use this instead of
        /// reading <c>Input.MoveInput</c> directly so both input styles drive the same Idle/Walk/Run transitions.</summary>
        public bool HasMoveIntent => Input.MoveInput.sqrMagnitude > 0.01f || (ClickToMove != null && ClickToMove.HasDestination);

        public string SaveKey => "PlayerTransform";

        private void Awake()
        {
            Input = GetComponent<PlayerInputHandler>();
            Motor = GetComponent<PlayerMotor>();
            Stats = GetComponent<PlayerStats>();
            ClickToMove = GetComponent<PlayerClickToMove>();
            _buffController = GetComponent<BuffController>();

            // Plays the hero's one-shot clips (attack/cast/hit/death) when the controller has them;
            // a no-op otherwise. Added here so no scene wiring is needed.
            if (GetComponent<PlayerActionAnimator>() == null)
            {
                gameObject.AddComponent<PlayerActionAnimator>();
            }

            BuildStateMachine();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.Register(this);
                saveManager.Register(Stats);
            }

            if (_playerCamera != null)
            {
                _playerCamera.SetPivot(_cameraLookTransform != null ? _cameraLookTransform : transform);
            }

            StateMachine.ChangeState<PlayerIdleState>();
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.Unregister(this);
                saveManager.Unregister(Stats);
            }
        }

        private void BuildStateMachine()
        {
            StateMachine = new StateMachine<PlayerController>(this);
            StateMachine.RegisterState(new PlayerIdleState());
            StateMachine.RegisterState(new PlayerWalkState());
            StateMachine.RegisterState(new PlayerRunState());
            StateMachine.RegisterState(new PlayerJumpState());
            StateMachine.RegisterState(new PlayerFallState());
            StateMachine.RegisterState(new PlayerSlideState());
            StateMachine.RegisterState(new PlayerCrouchState());
        }

        private void Update()
        {
            // Central stun guard: while stunned, the player cannot act (no movement/attack state
            // transitions or per-frame state logic). Camera look still works so a stunned player
            // isn't also blind, matching common ARPG stun conventions (e.g. Diablo/PoE) where
            // camera/UI stays responsive but character action does not.
            if (!IsStunned)
            {
                StateMachine.Tick(Time.deltaTime);

                if (_moveEveryFrame)
                {
                    // PlayerMotor integrates with Time.deltaTime, so the same FixedTick code is
                    // frame-rate independent here too (see _moveEveryFrame).
                    StateMachine.FixedTick(Time.deltaTime);
                }
            }

            if (_playerCamera != null && Input.LookInput.sqrMagnitude > 0.0001f)
            {
                bool isTouch = UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.press.isPressed;
                _playerCamera.ApplyLook(Input.LookInput, isTouch);
            }
        }

        private void FixedUpdate()
        {
            if (IsStunned || _moveEveryFrame)
            {
                return;
            }

            StateMachine.FixedTick(Time.fixedDeltaTime);
        }

        public object CaptureState()
        {
            Vector3 position = transform.position;

            return new PlayerTransformSaveState
            {
                posX = position.x,
                posY = position.y,
                posZ = position.z,
                rotY = transform.eulerAngles.y
            };
        }

        public void RestoreState(object state)
        {
            if (state is not PlayerTransformSaveState saveState)
            {
                return;
            }

            transform.position = new Vector3(saveState.posX, saveState.posY, saveState.posZ);
            transform.rotation = Quaternion.Euler(0f, saveState.rotY, 0f);
        }
    }
}
