using UnityEngine.SceneManagement;

namespace Solitaire.Core.StateMachine
{
    /// <summary>
    /// Displays the win screen and waits for the player to choose
    /// Play Again (restart via DealingState) or Main Menu (scene load).
    /// </summary>
    public class WinState : IGameState
    {
        private readonly IGameContext _context;

        public WinState(IGameContext context)
        {
            _context = context;
        }

        public void Enter()
        {
            _context.GameUI.ShowWinScreen();
            _context.GameUI.OnPlayAgainRequested += HandlePlayAgain;
            _context.GameUI.OnMainMenuRequested += HandleMainMenu;
        }

        public void Update() { }

        public void Exit()
        {
            _context.GameUI.OnPlayAgainRequested -= HandlePlayAgain;
            _context.GameUI.OnMainMenuRequested -= HandleMainMenu;
        }

        private void HandlePlayAgain()
        {
            _context.StateManager.ChangeState<DealingState>();
        }

        private void HandleMainMenu()
        {
            SceneManager.LoadScene(_context.MainMenuSceneName);
        }
    }
}
