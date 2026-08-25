using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Player is grounded, moving, and sprinting — drains stamina while active.</summary>
    public class PlayerRunState : IState<PlayerController>
    {
        public void Enter(PlayerController context)
        {
            // See PlayerIdleState.Enter — the rigged AnimatorControllers auto-transition via the
            // "Speed" float (PlayerAnimatorSpeedDriver), an explicit Play() here fought that.
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

            if (!context.HasMoveIntent)
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
            if (context.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                context.Motor.Move(context.Input.MoveInput, isRunning: true, context.CameraTransform);
                return;
            }

            if (context.ClickToMove != null && context.ClickToMove.HasDestination)
            {
                bool arrived = context.Motor.MoveToward(context.ClickToMove.Destination, isRunning: true);

                if (arrived)
                {
                    context.ClickToMove.ClearDestination();
                }
            }
        }

        public void Exit(PlayerController context)
        {
        }
    }
}
