using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    public class PlayingState : IGameState
    {
        private readonly GameManager gameManager;

        public PlayingState(GameManager manager, GamePresenter gamePresenter)
        {
            gameManager = manager;
        }

        public void Enter()
        {
            Debug.Log("Entering Playing State...");
        }

        public void Update()
        {
            // Check win conditions or listen for player input.
        }

        public void Exit()
        {
            Debug.Log("Exiting Playing State...");
        }
    }
}
