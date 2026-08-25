using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Player is grounded and not receiving meaningful movement input.</summary>
    public class PlayerIdleState : IState<PlayerController>
    {
        public void Enter(PlayerController context)
        {
            // No explicit Animator.Play() here: the rigged AnimatorControllers (see
            // PlayerAnimatorSpeedDriver) already auto-transition Idle/Walk/Run purely from the
            // "Speed" float parameter. Calling Play() here used to fight that — forcing a state
            // then having the controller's own Speed-driven condition immediately transition
            // away from it — which showed up as the character visually freezing mid-animation
            // while still moving (the C# state machine and the Animator's internal state drifted
            // out of sync since Play() only fires once, on Enter).
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

            if (context.HasMoveIntent)
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
