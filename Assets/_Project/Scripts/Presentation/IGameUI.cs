namespace Solitaire.Presentation
{
    /// <summary>
    /// Common interface for game UI implementations.
    /// Allows GameManager/States to work with both
    /// Canvas (uGUI) and UI Toolkit presentations.
    /// </summary>
    public interface IGameUI
    {
        void StartGame();
        void ShowWinScreen();
        void RestartGame();
    }
}
