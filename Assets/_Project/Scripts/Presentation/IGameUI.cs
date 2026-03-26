using Solitaire.Application;
using System;

namespace Solitaire.Presentation
{
    /// <summary>
    /// Common interface for game UI implementations.
    /// Allows GameManager and game states to work with both
    /// Canvas (uGUI) and UI Toolkit presentations.
    /// States subscribe to events to drive transitions;
    /// they call methods to trigger visual changes.
    /// </summary>
    public interface IGameUI
    {
        // --- Events reported to states ---

        event Action OnDealingComplete;
        event Action OnPlayAgainRequested;
        event Action OnMainMenuRequested;
        event Action OnAutoCompleteRequested;

        // --- Settings ---

        GameSettingsSO GameSettings { get; }

        // --- Methods called by states ---

        /// <summary>
        /// Set up visuals for the given game model and begin the dealing animation.
        /// Fires OnDealingComplete when the animation finishes.
        /// </summary>
        void StartGame(Game game);

        /// <summary>
        /// Display the win screen overlay. Win screen buttons fire
        /// OnPlayAgainRequested / OnMainMenuRequested.
        /// </summary>
        void ShowWinScreen();

        void ShowAutoCompleteButton();
        void HideAutoCompleteButton();

        /// <summary>
        /// Master interaction toggle controlled by states.
        /// When false, no card clicks or drags are processed, and
        /// animation-completion handlers will not re-enable interaction.
        /// </summary>
        void SetInteractable(bool interactable);

        /// <summary>
        /// Tear down all card views, hide overlays, stop coroutines.
        /// Called before each new deal.
        /// </summary>
        void Cleanup();
    }
}
