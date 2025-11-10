using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    public class WinState : IGameState
    {
        private GameManager gameManager;

        public WinState(GameManager gameManager, GamePresenter gamePresenter)
        {
            this.gameManager = gameManager;
        }

        public void Enter()
        {
            Debug.Log("Player won! Showing win screen...");
            // TODO: Trigger UI animation, save progress, etc.
        }

        public void Update() { }
        public void Exit() { }
    }
}
