using System;
using System.Collections.Generic;

namespace Rapadura.Core.StateMachine
{
    /// <summary>
    /// Generic finite-state machine. Reused by any actor that needs discrete behavioral
    /// states (Player, NPC, enemies, interactive objects, doors, etc.) so state logic
    /// never lives inside a giant switch statement.
    /// </summary>
    /// <typeparam name="TContext">The object the states operate on.</typeparam>
    public class StateMachine<TContext>
    {
        private readonly TContext _context;
        private readonly Dictionary<Type, IState<TContext>> _registeredStates = new Dictionary<Type, IState<TContext>>();

        public IState<TContext> CurrentState { get; private set; }
        public event Action<IState<TContext>, IState<TContext>> OnStateChanged;

        public StateMachine(TContext context)
        {
            _context = context;
        }

        /// <summary>Registers a state instance so it can later be entered via <see cref="ChangeState{TState}"/>.</summary>
        public void RegisterState<TState>(TState state) where TState : IState<TContext>
        {
            _registeredStates[typeof(TState)] = state;
        }

        /// <summary>Transitions to a previously registered state, calling Exit/Enter as appropriate.</summary>
        public void ChangeState<TState>() where TState : IState<TContext>
        {
            if (!_registeredStates.TryGetValue(typeof(TState), out IState<TContext> nextState))
            {
                throw new InvalidOperationException($"State {typeof(TState).Name} was not registered before use.");
            }

            if (ReferenceEquals(CurrentState, nextState))
            {
                return;
            }

            IState<TContext> previousState = CurrentState;
            previousState?.Exit(_context);

            CurrentState = nextState;
            CurrentState.Enter(_context);

            OnStateChanged?.Invoke(previousState, CurrentState);
        }

        /// <summary>Returns true if the currently active state is of the given type.</summary>
        public bool IsInState<TState>() where TState : IState<TContext>
        {
            return CurrentState is TState;
        }

        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(_context, deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            CurrentState?.FixedTick(_context, fixedDeltaTime);
        }
    }
}
