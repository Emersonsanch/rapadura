using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>
    /// Short crouched speed burst triggered by crouching while running. Locks movement speed
    /// for a fixed duration (decoupled from the walk/run/crouch table) then falls back to
    /// Crouch or Idle depending on whether the crouch input is still held.
    /// </summary>
    public class PlayerSlideState : IState<PlayerController>
    {
        private const float Duration = 0.5f;
        private const float SlideSpeed = 7.5f;

        private float _elapsed;

        public void Enter(PlayerController context)
        {
            context.Animator.Play(PlayerAnimatorHashes.Slide);
            context.Motor.SetCrouching(true);
            _elapsed = 0f;
        }

        public void Tick(PlayerController context, float deltaTime)
        {
            _elapsed += deltaTime;

            if (!context.Motor.IsGrounded)
            {
                context.StateMachine.ChangeState<PlayerFallState>();
                return;
            }

            if (_elapsed >= Duration)
            {
                if (context.Input.IsCrouchHeld)
                {
                    context.StateMachine.ChangeState<PlayerCrouchState>();
                }
                else
                {
                    context.StateMachine.ChangeState<PlayerIdleState>();
                }
            }
        }

        public void FixedTick(PlayerController context, float fixedDeltaTime)
        {
            context.Motor.Move(context.Input.MoveInput, isRunning: false, context.CameraTransform, SlideSpeed);
        }

        public void Exit(PlayerController context)
        {
            context.Motor.SetCrouching(false);
        }
    }
}
