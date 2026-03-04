using Solitaire.Presentation;
using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    public class DealingState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly GamePresenter _gamePresenter;
        private bool _hasDealt;

        public DealingState(GameManager gameManager, GamePresenter gamePresenter)
        {
            _gameManager = gameManager;
            _gamePresenter = gamePresenter;
        }

        public void Enter()
        {
            _hasDealt = false;
            _gamePresenter.StartGame();
        }

        public void Update()
        {
            // Transition once — guard prevents re-entering PlayingState every frame
            if (!_hasDealt)
            {
                _hasDealt = true;
                _gameManager.StateManager.ChangeState<PlayingState>();
            }
        }

        public void Exit()
        {
        }
    }
}