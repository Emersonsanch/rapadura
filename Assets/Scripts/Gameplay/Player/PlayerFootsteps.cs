using Rapadura.Core.Events;
using Rapadura.Gameplay.Player.States;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Publishes <see cref="PlayerFootstepEvent"/> each time one of the hero's feet plants while walking or
    /// running (sound: <c>hero.step</c> through SfxEventRouter; dust: <see cref="PlayerFootstepDust"/>). Added
    /// to the player at runtime by <see cref="PlayerProceduralAnimator"/>, which re-binds it on every visual swap.
    /// The plant timing, best source first:
    /// <list type="number">
    /// <item><b>Baked</b>: the rebuilt controller carries <c>FootPlantL</c>/<c>FootPlantR</c>/<c>FootStrides</c>
    /// (normalized time of each plant in the Locomotion state, measured from the clips by
    /// <c>Rapadura/Animation/Rebuild Hero Animators</c>, which also phase-aligns Walk/Jog/Run so these hold for
    /// the whole blend). Exact, no animation events needed (so no "no receiver" errors on other copies).</item>
    /// <item><b>Feet</b>: no baked phases (old controller): a heel strike at each extreme of the animated stride
    /// spread (<see cref="FootstepMath.StrideExtremum"/>).</item>
    /// <item><b>Rigid</b>: no rig (Maithe): every π of the procedural step phase.</item>
    /// </list>
    /// Runs after the Animator and the procedural layer (LateUpdate, order 110), no allocations.
    /// </summary>
    [DefaultExecutionOrder(110)]
    [DisallowMultipleComponent]
    public class PlayerFootsteps : MonoBehaviour
    {
        public enum StepSource
        {
            None = 0,
            Baked = 1,
            Feet = 2,
            Rigid = 3,
        }

        [Tooltip("No footsteps below this ground speed (m/s): shuffling in place stays quiet.")]
        [SerializeField, Min(0f)] private float _minSpeed = 0.4f;
        [Tooltip("Never two footsteps closer than this (s), whatever the source.")]
        [SerializeField, Min(0f)] private float _minInterval = 0.1f;
        [Tooltip("Add a PlayerFootstepDust (small dust puff on harder steps).")]
        [SerializeField] private bool _dust = true;

        private PlayerMotor _motor;
        private PlayerStats _stats;
        private PlayerProceduralAnimator _procedural;
        private Animator _animator;
        private Transform _leftFoot;
        private Transform _rightFoot;

        private RuntimeAnimatorController _cachedController;
        private bool _hasBaked;
        private float _plantLeft = -1f;
        private float _plantRight = -1f;
        private int _strides = 1;
        private float _previousNormalized = -1f;

        private FootstepMath.StrideTracker _tracker;
        private float _previousStepPhase;
        private bool _hasStepPhase;
        private float _sinceStep = 10f;

        /// <summary>Footsteps published so far (tests/debug).</summary>
        public int StepCount { get; private set; }

        /// <summary>Where the last frame's timing came from.</summary>
        public StepSource Source { get; private set; }

        private void Awake()
        {
            _motor = GetComponentInParent<PlayerMotor>();
            _stats = GetComponentInParent<PlayerStats>();
            _procedural = GetComponent<PlayerProceduralAnimator>();
            if (_dust && GetComponent<PlayerFootstepDust>() == null)
            {
                gameObject.AddComponent<PlayerFootstepDust>();
            }
        }

        /// <summary>Re-targets the visual: its Animator (may be null) and its foot bones (null on rig-less bodies).</summary>
        public void Bind(PlayerMotor motor, Animator animator, Transform leftFoot, Transform rightFoot)
        {
            if (motor != null)
            {
                _motor = motor;
            }

            _animator = animator;
            _leftFoot = leftFoot;
            _rightFoot = rightFoot;
            _cachedController = null;
            _hasBaked = false;
            _previousNormalized = -1f;
            _tracker = default;
            _hasStepPhase = false;
        }

        private void CacheParameters()
        {
            _cachedController = _animator.runtimeAnimatorController;
            _hasBaked = false;
            _plantLeft = _plantRight = -1f;
            _strides = 1;
            _previousNormalized = -1f;
            bool hasLeft = false;
            bool hasRight = false;
            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                if (parameter.type != AnimatorControllerParameterType.Float)
                {
                    continue;
                }

                if (parameter.nameHash == PlayerAnimatorHashes.FootPlantLeft)
                {
                    _plantLeft = parameter.defaultFloat;
                    hasLeft = true;
                }
                else if (parameter.nameHash == PlayerAnimatorHashes.FootPlantRight)
                {
                    _plantRight = parameter.defaultFloat;
                    hasRight = true;
                }
                else if (parameter.nameHash == PlayerAnimatorHashes.FootStrides)
                {
                    _strides = Mathf.Clamp(Mathf.RoundToInt(parameter.defaultFloat), 1, 4);
                }
            }

            _hasBaked = hasLeft && hasRight && (_plantLeft >= 0f || _plantRight >= 0f);
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f || _motor == null)
            {
                return;
            }

            _sinceStep += deltaTime;
            int foot = -1;

            bool animated = _animator != null && _animator.isActiveAndEnabled && _animator.runtimeAnimatorController != null;
            if (animated && _cachedController != _animator.runtimeAnimatorController)
            {
                CacheParameters();
            }

            if (animated && _hasBaked)
            {
                Source = StepSource.Baked;
                foot = BakedFoot();
            }
            else if (_leftFoot != null && _rightFoot != null)
            {
                Source = StepSource.Feet;
                float spread = Vector3.Dot(_leftFoot.position - _rightFoot.position, transform.forward);
                foot = FootstepMath.StrideExtremum(ref _tracker, spread);
            }
            else if (_procedural != null)
            {
                Source = StepSource.Rigid;
                float phase = _procedural.StepPhase;
                if (_hasStepPhase)
                {
                    int fall = FootstepMath.RigidFootfall(_previousStepPhase, phase);
                    foot = fall < 0 ? -1 : (fall & 1);
                }

                _previousStepPhase = phase;
                _hasStepPhase = true;
            }
            else
            {
                Source = StepSource.None;
            }

            if (foot < 0 || _sinceStep < _minInterval || !_motor.IsGrounded || (_stats != null && _stats.IsDead))
            {
                return;
            }

            float speed = _motor.HorizontalVelocity.magnitude;
            if (speed < _minSpeed)
            {
                return;
            }

            bool left = foot == 0;
            Transform footBone = left ? _leftFoot : _rightFoot;
            Vector3 root = transform.position;
            Vector3 position = footBone != null
                ? new Vector3(footBone.position.x, root.y, footBone.position.z)
                : root + transform.right * (left ? -0.12f : 0.12f);
            float weight = _procedural != null && _procedural.Profile != null ? _procedural.Profile.weight : 1f;

            _sinceStep = 0f;
            StepCount++;
            EventBus.Publish(new PlayerFootstepEvent(gameObject, position, left, FootstepMath.Intensity(speed, _motor.RunSpeed, weight)));
        }

        private int BakedFoot()
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (_animator.IsInTransition(0))
            {
                AnimatorStateInfo next = _animator.GetNextAnimatorStateInfo(0);
                if (next.shortNameHash == PlayerAnimatorHashes.LocomotionState)
                {
                    info = next;
                }
            }

            if (info.shortNameHash != PlayerAnimatorHashes.LocomotionState)
            {
                _previousNormalized = -1f;
                return -1;
            }

            float now = info.normalizedTime;
            float before = _previousNormalized;
            _previousNormalized = now;
            if (before < 0f || now <= before)
            {
                return -1;
            }

            if (FootstepMath.CrossedPhase(before, now, _plantLeft, _strides))
            {
                return 0;
            }

            return FootstepMath.CrossedPhase(before, now, _plantRight, _strides) ? 1 : -1;
        }
    }
}
