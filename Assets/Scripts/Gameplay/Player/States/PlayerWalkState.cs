using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Player is grounded and moving, either walking or running depending on input.</summary>
    public class PlayerWalkState : IState<PlayerController>
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

            if (!context.HasMoveIntent)
            {
                context.StateMachine.ChangeState<PlayerIdleState>();
                return;
            }

            // Click-to-move ignores crouch/run modifiers (there's no manual key held while
            // walking toward a clicked point) — those only apply to manual WASD/joystick input.
            if (context.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                if (context.Input.IsCrouchHeld)
                {
                    context.StateMachine.ChangeState<PlayerCrouchState>();
                    return;
                }

                if (context.Input.IsRunHeld && context.Stats.HasStamina())
                {
                    context.StateMachine.ChangeState<PlayerRunState>();
                }
            }
        }

        public void FixedTick(PlayerController context, float fixedDeltaTime)
        {
            if (context.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                context.Motor.Move(context.Input.MoveInput, isRunning: false, context.CameraTransform);
                return;
            }

            if (context.ClickToMove != null && context.ClickToMove.HasDestination)
            {
                // Chasing a clicked enemy that is far away (WantsAutoRun) moves at run speed.
                bool arrived = context.Motor.MoveToward(context.ClickToMove.Destination, isRunning: context.ClickToMove.WantsAutoRun);

                if (arrived)
                {
                    context.ClickToMove.ClearDestination();
                }

                return;
            }

            // No move this step (intent just ended): still integrate gravity / refresh grounding.
            context.Motor.ApplyGravityOnly();
        }

        public void Exit(PlayerController context)
        {
        }
    }
}
