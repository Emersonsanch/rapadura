using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Player is crouched — reduced height, reduced speed, cannot run or jump.</summary>
    public class PlayerCrouchState : IState<PlayerController>
    {
        public void Enter(PlayerController context)
        {
            context.Animator.Play(PlayerAnimatorHashes.Crouch);
            context.Motor.SetCrouching(true);
        }

        public void Tick(PlayerController context, float deltaTime)
        {
            if (!context.Motor.IsGrounded)
            {
                context.StateMachine.ChangeState<PlayerFallState>();
                return;
            }

            if (!context.Input.IsCrouchHeld)
            {
                context.StateMachine.ChangeState<PlayerIdleState>();
            }
        }

        public void FixedTick(PlayerController context, float fixedDeltaTime)
        {
            context.Motor.Move(context.Input.MoveInput, isRunning: false, context.CameraTransform);
        }

        public void Exit(PlayerController context)
        {
            context.Motor.SetCrouching(false);
        }
    }
}
