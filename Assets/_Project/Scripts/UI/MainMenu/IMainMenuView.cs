using System;

namespace Solitaire.UI.MainMenu
{
    /// <summary>
    /// View contract for the Main Menu screen.
    /// The presenter subscribes to events; it never touches
    /// UnityEngine.UI types directly.
    /// </summary>
    public interface IMainMenuView
    {
        event Action OnNewGameClicked;
        event Action OnContinueClicked;
        event Action OnSettingsClicked;
        event Action OnLeaderboardClicked;
        event Action OnInfoClicked;

        /// <summary>
        /// Show or hide the Continue button (hidden when no saved game exists).
        /// </summary>
        void SetContinueButtonInteractable(bool interactable);
    }
}
