namespace Solitaire.Core.StateMachine
{
    public class WinState : IGameState
    {
        private readonly GameManager _gameManager;

        public WinState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            _gameManager.GameUI.ShowWinScreen();
        }

        public void Update() { }
        public void Exit() { }
    }
}
