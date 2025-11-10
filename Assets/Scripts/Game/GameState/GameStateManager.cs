using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    public class GameStateManager : MonoBehaviour
    {
        private IGameState currentState;
        private readonly Dictionary<Type, IGameState> states = new();

        public void RegisterState(IGameState state)
        {
            states[state.GetType()] = state;
        }

        public void ChangeState<T>() where T : IGameState
        {
            if (currentState != null)
                currentState.Exit();

            if (states.TryGetValue(typeof(T), out var nextState))
            {
                currentState = nextState;
                currentState.Enter();
            }
            else
            {
                Debug.LogError($"State {typeof(T).Name} not registered!");
            }
        }

        private void Update()
        {
            currentState?.Update();
        }
    }
}
