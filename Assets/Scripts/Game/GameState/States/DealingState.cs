using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    public class DealingState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly GamePresenter _gamePresenter;

        public DealingState(GameManager gameManager, GamePresenter gamePresenter)
        {
            _gameManager = gameManager;
            _gamePresenter = gamePresenter;
        }

        public void Enter()
        {
            Debug.Log("Entering Dealing State...");
            _gamePresenter.StartGame();
        }

        public void Update()
        {
            // Once dealing is complete, we can transition to the playing state.
            // In a real game, this might wait for animations to finish.
            _gameManager.StateManager.ChangeState<PlayingState>();
        }

        public void Exit()
        {
            Debug.Log("Exiting Dealing State...");
        }
    }
}
