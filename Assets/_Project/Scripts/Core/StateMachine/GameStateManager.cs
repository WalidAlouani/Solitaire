using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    public class GameStateManager : MonoBehaviour
    {
        private IGameState _currentState;
        private readonly Dictionary<Type, IGameState> _states = new();

        public void RegisterState(IGameState state)
        {
            _states[state.GetType()] = state;
        }

        public void ChangeState<T>() where T : IGameState
        {
            if (_currentState != null)
                _currentState.Exit();

            if (_states.TryGetValue(typeof(T), out var nextState))
            {
                _currentState = nextState;
                _currentState.Enter();
            }
            else
            {
                Debug.LogError($"State {typeof(T).Name} not registered!");
            }
        }

        private void Update()
        {
            _currentState?.Update();
        }
    }
}