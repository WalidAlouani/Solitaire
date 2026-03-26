using System;
using System.Collections.Generic;

namespace Solitaire.Core.StateMachine
{
    public class GameStateManager
    {
        private IGameState _currentState;
        private readonly Dictionary<Type, IGameState> _states = new();

        public IGameState CurrentState => _currentState;

        public void RegisterState(IGameState state)
        {
            _states[state.GetType()] = state;
        }

        public bool ChangeState<T>() where T : IGameState
        {
            if (!_states.TryGetValue(typeof(T), out var nextState))
                return false;

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
            return true;
        }

        public void Tick()
        {
            _currentState?.Update();
        }

        public bool IsInState<T>() where T : IGameState
        {
            return _currentState is T;
        }
    }
}
