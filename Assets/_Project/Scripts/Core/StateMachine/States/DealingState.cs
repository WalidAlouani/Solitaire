using Solitaire.Presentation;

namespace Solitaire.Core.StateMachine
{
    public class DealingState : IGameState
    {
        private readonly GameManager _gameManager;
        private bool _hasDealt;

        public DealingState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            _hasDealt = false;
            _gameManager.GameUI.StartGame();
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
