using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Player is grounded and not receiving meaningful movement input.</summary>
    public class PlayerIdleState : IState<PlayerController>
    {
        public void Enter(PlayerController context)
        {
            context.Animator.Play(PlayerAnimatorHashes.Idle);
        }

        public void Tick(PlayerController context, float deltaTime)
        {
            if (context.Input.JumpPressedThisFrame && context.Motor.IsGrounded)
            {
                context.StateMachine.ChangeState<PlayerJumpState>();
                return;
            }

            if (!context.Motor.IsGrounded)
            {
                context.StateMachine.ChangeState<PlayerFallState>();
                return;
            }

            if (context.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                context.StateMachine.ChangeState<PlayerWalkState>();
                return;
            }

            if (context.Input.IsCrouchHeld)
            {
                context.StateMachine.ChangeState<PlayerCrouchState>();
            }
        }

        public void FixedTick(PlayerController context, float fixedDeltaTime)
        {
        }

        public void Exit(PlayerController context)
        {
        }
    }
}
