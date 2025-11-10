using UnityEngine;
using System.Collections.Generic;
using Solitaire.Core.StateMachine;

namespace Solitaire.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Presenters")]
        [SerializeField] private GamePresenter _gamePresenter;

        public GameStateManager StateManager { get; private set; }

        private void Awake()
        {
            RegisterGameStates();
        }

        private void Start()
        {
            StateManager.ChangeState<DealingState>();
        }

        private void RegisterGameStates()
        {
            StateManager = gameObject.AddComponent<GameStateManager>();
            StateManager.RegisterState(new DealingState(this, _gamePresenter));
            StateManager.RegisterState(new PlayingState(this, _gamePresenter));
            StateManager.RegisterState(new WinState(this, _gamePresenter));
        }
    }
}

