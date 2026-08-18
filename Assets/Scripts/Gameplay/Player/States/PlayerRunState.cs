using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Player is grounded, moving, and sprinting — drains stamina while active.</summary>
    public class PlayerRunState : IState<PlayerController>
    {
        public void Enter(PlayerController context)
        {
            context.Animator.Play(PlayerAnimatorHashes.Run);
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

            if (context.Input.IsCrouchHeld)
            {
                context.StateMachine.ChangeState<PlayerSlideState>();
                return;
            }

            if (context.Input.MoveInput.sqrMagnitude <= 0.01f)
            {
                context.StateMachine.ChangeState<PlayerIdleState>();
                return;
            }

            if (!context.Input.IsRunHeld || !context.Stats.HasStamina())
            {
                context.StateMachine.ChangeState<PlayerWalkState>();
                return;
            }

            context.Stats.DrainStamina(deltaTime);
        }

        public void FixedTick(PlayerController context, float fixedDeltaTime)
        {
            context.Motor.Move(context.Input.MoveInput, isRunning: true, context.CameraTransform);
        }

        public void Exit(PlayerController context)
        {
        }
    }
}
