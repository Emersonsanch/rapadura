using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Feeds the "Speed" float parameter of a rigged character's <see cref="Animator"/> (Idle/Walk/Run
    /// blend, see Joaquim_AnimatorController) from <see cref="PlayerMotor.HorizontalVelocity"/> each
    /// frame. Purely a bridge — the actual movement decision lives in <see cref="PlayerMotor"/>/the
    /// state machine; this only reports how fast the character is currently moving so real leg
    /// animation (once a model has a Humanoid rig, e.g. via Meshy's rigging API + Mixamo-equivalent
    /// output) can react to it. Safe to leave on a non-rigged visual too — if no Animator is found it
    /// just does nothing every frame.
    /// </summary>
    public class PlayerAnimatorSpeedDriver : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Animator _animator;

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
        }

        private void Update()
        {
            if (_animator == null || _motor == null)
            {
                return;
            }

            _animator.SetFloat(SpeedParam, _motor.HorizontalVelocity.magnitude);
        }
    }
}
