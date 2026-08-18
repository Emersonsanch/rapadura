using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Player is airborne, either from a jump or falling off a ledge.</summary>
    public class PlayerJumpState : IState<PlayerController>
    {
        public void Enter(PlayerController context)
        {
            context.Animator.Play(PlayerAnimatorHashes.Jump);

            if (context.Motor.IsGrounded)
            {
                context.Motor.Jump();
            }
        }

        public void Tick(PlayerController context, float deltaTime)
        {
            if (context.Motor.IsGrounded)
            {
                context.StateMachine.ChangeState<PlayerIdleState>();
                return;
            }

            if (context.Motor.VerticalVelocity < 0f)
            {
                context.StateMachine.ChangeState<PlayerFallState>();
            }
        }

        public void FixedTick(PlayerController context, float fixedDeltaTime)
        {
            context.Motor.Move(context.Input.MoveInput, isRunning: context.Input.IsRunHeld, context.CameraTransform);
        }

        public void Exit(PlayerController context)
        {
        }
    }
}
