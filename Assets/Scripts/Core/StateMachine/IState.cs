namespace Rapadura.Core.StateMachine
{
    /// <summary>
    /// Contract for a single state used by <see cref="StateMachine{TContext}"/>.
    /// Implemented by concrete states such as PlayerIdleState, PlayerJumpState, NpcPatrolState, etc.
    /// </summary>
    /// <typeparam name="TContext">The object the state operates on (e.g. the player or an NPC).</typeparam>
    public interface IState<TContext>
    {
        /// <summary>Called once when the state machine transitions into this state.</summary>
        void Enter(TContext context);

        /// <summary>Called every frame while this state is active.</summary>
        void Tick(TContext context, float deltaTime);

        /// <summary>Called every physics step while this state is active.</summary>
        void FixedTick(TContext context, float fixedDeltaTime);

        /// <summary>Called once when the state machine transitions out of this state.</summary>
        void Exit(TContext context);
    }
}
