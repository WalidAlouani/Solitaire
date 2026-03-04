using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    public class PlayingState : IGameState
    {
        private readonly GameManager _gameManager;

        public PlayingState(GameManager manager)
        {
            _gameManager = manager;
        }

        public void Enter()
        {
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }
}