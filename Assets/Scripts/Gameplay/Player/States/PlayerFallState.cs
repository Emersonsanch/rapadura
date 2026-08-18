using Rapadura.Core.StateMachine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>
    /// Player is airborne and descending without having jumped (walked off a ledge).
    /// Distinct from <see cref="PlayerJumpState"/> so a different fall animation/fall-damage
    /// hook can be wired without touching jump logic.
    /// </summary>
    public class PlayerFallState : IState<PlayerController>
    {
        public void Enter(PlayerController context)
        {
            context.Animator.Play(PlayerAnimatorHashes.Fall);
        }

        public void Tick(PlayerController context, float deltaTime)
        {
            if (context.Motor.IsGrounded)
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
        }
    }
}
