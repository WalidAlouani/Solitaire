using UnityEngine.SceneManagement;

namespace Solitaire.Core.StateMachine
{
    /// <summary>
    /// Displays the win screen and waits for the player to choose
    /// Play Again (restart via DealingState) or Main Menu (scene load).
    /// </summary>
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
            _gameManager.GameUI.OnPlayAgainRequested += HandlePlayAgain;
            _gameManager.GameUI.OnMainMenuRequested += HandleMainMenu;
        }

        public void Update() { }

        public void Exit()
        {
            _gameManager.GameUI.OnPlayAgainRequested -= HandlePlayAgain;
            _gameManager.GameUI.OnMainMenuRequested -= HandleMainMenu;
        }

        private void HandlePlayAgain()
        {
            _gameManager.StateManager.ChangeState<DealingState>();
        }

        private void HandleMainMenu()
        {
            SceneManager.LoadScene(_gameManager.MainMenuSceneName);
        }
    }
}
