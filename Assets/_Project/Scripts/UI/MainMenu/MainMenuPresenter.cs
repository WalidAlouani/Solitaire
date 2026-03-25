using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Solitaire.UI.MainMenu
{
    /// <summary>
    /// Pure C# presenter for the Main Menu screen.
    /// Subscribes to <see cref="IMainMenuView"/> events and handles navigation.
    /// No MonoBehaviour dependency — easy to test in isolation.
    /// </summary>
    public class MainMenuPresenter : IDisposable
    {
        private readonly IMainMenuView _view;
        private readonly string _gameSceneName;

        public MainMenuPresenter(IMainMenuView view, string gameSceneName)
        {
            _view = view;
            _gameSceneName = gameSceneName;

            _view.OnNewGameClicked += HandleNewGame;
            _view.OnContinueClicked += HandleContinue;
            _view.OnSettingsClicked += HandleSettings;
            _view.OnLeaderboardClicked += HandleLeaderboard;
            _view.OnInfoClicked += HandleInfo;

            // TODO: check for saved game data and toggle continue button
            _view.SetContinueButtonInteractable(false);
        }

        public void Dispose()
        {
            _view.OnNewGameClicked -= HandleNewGame;
            _view.OnContinueClicked -= HandleContinue;
            _view.OnSettingsClicked -= HandleSettings;
            _view.OnLeaderboardClicked -= HandleLeaderboard;
            _view.OnInfoClicked -= HandleInfo;
        }

        private void HandleNewGame()
        {
            SceneManager.LoadScene(_gameSceneName);
        }

        private void HandleContinue()
        {
            // TODO: load saved game state, then transition to game scene
            Debug.Log("[MainMenu] Continue — not implemented yet");
        }

        private void HandleSettings()
        {
            // TODO: open settings panel/popup
            Debug.Log("[MainMenu] Settings — not implemented yet");
        }

        private void HandleLeaderboard()
        {
            // TODO: open leaderboard panel/popup
            Debug.Log("[MainMenu] Leaderboard — not implemented yet");
        }

        private void HandleInfo()
        {
            // TODO: open info/about panel/popup
            Debug.Log("[MainMenu] Info — not implemented yet");
        }
    }
}
