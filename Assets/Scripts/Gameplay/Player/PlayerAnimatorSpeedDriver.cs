using Rapadura.Gameplay.Player.States;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Bridges <see cref="PlayerMotor.HorizontalVelocity"/> to the rigged hero's <see cref="Animator"/>.
    ///
    /// With a rebuilt controller (menu <c>Rapadura/Animation/Rebuild Hero Animators</c>: one
    /// "Locomotion" 1D blend tree Idle 0 → Walk 0.5 → Run 1) it drives:
    /// <list type="bullet">
    /// <item><c>MoveBlend</c>: normalized speed (walk speed = 0.5, run speed = 1), critically damped
    /// so starts/stops/walk↔run crossfade smoothly.</item>
    /// <item><c>MoveSpeedMul</c>: the state's playback multiplier, chosen so the blended stride covers
    /// exactly the ground the body moves (no foot sliding) — from each clip's measured ground speed,
    /// baked by the editor tool into the <c>WalkClipSpeed</c>/<c>RunClipSpeed</c> parameter defaults
    /// (model units/s, scaled here by the model's world scale), else the tunable fallbacks below.</item>
    /// </list>
    /// <item><c>Speed</c> (m/s, damped by <see cref="_speedDampTime"/>), <c>TurnAngle</c> (signed facing →
    /// wanted direction, damped), <c>IsGrounded</c> and <c>VerticalSpeed</c> (holding the touchdown
    /// speed briefly so the controller's Fall → Land can tell a hard landing) — the rebuilt
    /// controller's Jump/Fall/Land transitions read these; a clip can drop into those states later.</item>
    /// The legacy <c>Speed</c> float (raw m/s) is still written whenever the controller has it, so
    /// the old 3-state controllers keep working until the rebuild runs. Safe on a visual without an
    /// Animator (does nothing).
    /// </summary>
    public class PlayerAnimatorSpeedDriver : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Animator _animator;

        [Header("Blend")]
        [Tooltip("Smoothing time (s) of the normalized MoveBlend parameter.")]
        [SerializeField, Min(0f)] private float _blendSmoothTime = 0.1f;
        [Tooltip("Smoothing time (s) of the stride-matching playback multiplier.")]
        [SerializeField, Min(0f)] private float _playbackSmoothTime = 0.08f;

        [Header("Stride matching")]
        [Tooltip("Clamp of the playback multiplier, so a haste/slow never plays the legs absurdly fast/slow.")]
        [SerializeField] private float _minPlayback = 0.6f;
        [SerializeField] private float _maxPlayback = 1.9f;
        [Tooltip("Fallback ground speed (world m/s) of the Walk clip at playback 1 when the controller has no baked WalkClipSpeed.")]
        [SerializeField, Min(0.05f)] private float _fallbackWalkClipSpeed = 2.5f;
        [Tooltip("Fallback ground speed (world m/s) of the Run clip at playback 1 when the controller has no baked RunClipSpeed.")]
        [SerializeField, Min(0.05f)] private float _fallbackRunClipSpeed = 5.5f;
        [Tooltip("Hand-tuning multiplier on the clip ground speeds (>1 = longer stride = slower legs).")]
        [SerializeField, Min(0.1f)] private float _strideTuning = 1f;

        [Header("State parameters")]
        [Tooltip("Animator dampTime (s) of Speed.")]
        [SerializeField, Min(0f)] private float _speedDampTime = 0.1f;
        [Tooltip("Animator dampTime (s) of TurnAngle.")]
        [SerializeField, Min(0f)] private float _turnDampTime = 0.12f;
        [Tooltip("Animator dampTime (s) of VerticalSpeed while airborne.")]
        [SerializeField, Min(0f)] private float _verticalDampTime = 0.05f;
        [Tooltip("Seconds VerticalSpeed holds the touchdown speed after landing.")]
        [SerializeField, Min(0f)] private float _landingHold = 0.12f;

        [Tooltip("The motor owns movement: clips that translate the body must not also move the model (root motion).")]
        [SerializeField] private bool _disableRootMotion = true;

        private RuntimeAnimatorController _cachedController;
        private bool _hasSpeed;
        private bool _hasBlend;
        private bool _hasMultiplier;
        private bool _hasTurn;
        private bool _hasGrounded;
        private bool _hasVertical;
        private bool _wasGrounded = true;
        private float _airVelocity;
        private float _sinceLanding = 10f;
        private float _bakedWalkClipSpeed;
        private float _bakedRunClipSpeed;

        private float _blend;
        private float _blendVelocity;
        private float _playback = 1f;
        private float _playbackVelocity;

        /// <summary>Current (damped) MoveBlend value — exposed for debugging/tests.</summary>
        public float CurrentBlend => _blend;

        /// <summary>Current stride-matching playback multiplier.</summary>
        public float CurrentPlayback => _playback;

        private void Awake()
        {
            if (_motor == null)
            {
                _motor = GetComponentInParent<PlayerMotor>();
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        /// <summary>Re-targets which Animator gets the Speed parameter — used by <see cref="PlayerVisualSwapper"/> after swapping the visual model.</summary>
        public void SetAnimator(Animator animator)
        {
            _animator = animator;
            _cachedController = null;
            _blend = 0f;
            _blendVelocity = 0f;
            _playback = 1f;
            _playbackVelocity = 0f;
        }

        private void CacheParameters()
        {
            _cachedController = _animator.runtimeAnimatorController;
            _hasSpeed = _hasBlend = _hasMultiplier = _hasTurn = _hasGrounded = _hasVertical = false;
            _bakedWalkClipSpeed = _bakedRunClipSpeed = 0f;

            if (_disableRootMotion)
            {
                _animator.applyRootMotion = false;
            }

            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                int hash = parameter.nameHash;
                if (parameter.type == AnimatorControllerParameterType.Bool && hash == PlayerAnimatorHashes.IsGrounded)
                {
                    _hasGrounded = true;
                    continue;
                }

                if (parameter.type != AnimatorControllerParameterType.Float)
                {
                    continue;
                }

                if (hash == PlayerAnimatorHashes.TurnAngle)
                {
                    _hasTurn = true;
                }
                else if (hash == PlayerAnimatorHashes.VerticalSpeed)
                {
                    _hasVertical = true;
                }
                else if (hash == PlayerAnimatorHashes.Speed)
                {
                    _hasSpeed = true;
                }
                else if (hash == PlayerAnimatorHashes.MoveBlend)
                {
                    _hasBlend = true;
                }
                else if (hash == PlayerAnimatorHashes.MoveSpeedMultiplier)
                {
                    _hasMultiplier = true;
                }
                else if (hash == PlayerAnimatorHashes.WalkClipSpeed)
                {
                    _bakedWalkClipSpeed = parameter.defaultFloat;
                }
                else if (hash == PlayerAnimatorHashes.RunClipSpeed)
                {
                    _bakedRunClipSpeed = parameter.defaultFloat;
                }
            }
        }

        private void UpdateStateParameters(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            bool grounded = _motor.IsGrounded;
            float vertical = _motor.VerticalVelocity;
            if (!grounded)
            {
                _airVelocity = vertical;
                _sinceLanding = 0f;
            }
            else if (_wasGrounded)
            {
                _sinceLanding += deltaTime;
            }

            _wasGrounded = grounded;

            if (_hasGrounded)
            {
                _animator.SetBool(PlayerAnimatorHashes.IsGrounded, grounded);
            }

            if (_hasVertical)
            {
                float value = MotionFeelMath.AnimatorVerticalSpeed(grounded, vertical, _airVelocity, _sinceLanding, _landingHold);
                if (grounded)
                {
                    // Not damped on the ground: the held touchdown speed must be readable on the landing frame.
                    _animator.SetFloat(PlayerAnimatorHashes.VerticalSpeed, value);
                }
                else
                {
                    _animator.SetFloat(PlayerAnimatorHashes.VerticalSpeed, value, _verticalDampTime, deltaTime);
                }
            }

            if (_hasTurn)
            {
                float turn = MotionFeelMath.SignedTurnAngle(_motor.transform.forward, _motor.DesiredDirection);
                _animator.SetFloat(PlayerAnimatorHashes.TurnAngle, turn, _turnDampTime, deltaTime);
            }
        }

        private void Update()
        {
            if (_animator == null || _motor == null || _animator.runtimeAnimatorController == null)
            {
                return;
            }

            if (_cachedController != _animator.runtimeAnimatorController)
            {
                CacheParameters();
            }

            float speed = _motor.HorizontalVelocity.magnitude;
            float deltaTime = Time.deltaTime;

            if (_hasSpeed)
            {
                // Rebuilt controllers read a damped Speed; the legacy 3-state ones keep the raw value they transition on.
                if (_hasGrounded)
                {
                    _animator.SetFloat(PlayerAnimatorHashes.Speed, speed, _speedDampTime, deltaTime);
                }
                else
                {
                    _animator.SetFloat(PlayerAnimatorHashes.Speed, speed);
                }
            }

            UpdateStateParameters(deltaTime);

            if (!_hasBlend)
            {
                return;
            }

            float targetBlend = LocomotionMath.SpeedToBlend(speed, _motor.WalkSpeed, _motor.RunSpeed);
            _blend = LocomotionMath.DampParameter(_blend, targetBlend, ref _blendVelocity, _blendSmoothTime, deltaTime);
            _animator.SetFloat(PlayerAnimatorHashes.MoveBlend, _blend);

            if (!_hasMultiplier)
            {
                return;
            }

            // Baked clip speeds are in model space: convert with the model's world scale.
            float worldScale = Mathf.Abs(_animator.transform.lossyScale.y);
            float walkClip = (_bakedWalkClipSpeed > 0f ? _bakedWalkClipSpeed * worldScale : _fallbackWalkClipSpeed) * _strideTuning;
            float runClip = (_bakedRunClipSpeed > 0f ? _bakedRunClipSpeed * worldScale : _fallbackRunClipSpeed) * _strideTuning;

            float natural = LocomotionMath.BlendNaturalSpeed(_blend, walkClip, runClip);
            float targetPlayback = LocomotionMath.StridePlaybackSpeed(speed, natural, _minPlayback, _maxPlayback);
            _playback = LocomotionMath.DampParameter(_playback, targetPlayback, ref _playbackVelocity, _playbackSmoothTime, deltaTime);
            _animator.SetFloat(PlayerAnimatorHashes.MoveSpeedMultiplier, _playback);
        }
    }
}
