using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.UI.MainMenu
{
    /// <summary>
    /// MonoBehaviour that lives on the MainMenu Canvas.
    /// Binds Unity UI buttons and forwards clicks as events
    /// to the <see cref="MainMenuPresenter"/>.
    /// </summary>
    public class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [Header("Primary Actions")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;

        [Header("Bottom Bar")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _infoButton;

        [Header("Scenes")]
        [SerializeField] private string _gameSceneName = "Game-Canvas";

        // --- IMainMenuView Events ---

        public event Action OnNewGameClicked;
        public event Action OnContinueClicked;
        public event Action OnSettingsClicked;
        public event Action OnLeaderboardClicked;
        public event Action OnInfoClicked;

        private MainMenuPresenter _presenter;

        private void Awake()
        {
            _presenter = new MainMenuPresenter(this, _gameSceneName);
        }

        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(HandleNewGame);
            _continueButton.onClick.AddListener(HandleContinue);
            _settingsButton.onClick.AddListener(HandleSettings);
            _leaderboardButton.onClick.AddListener(HandleLeaderboard);
            _infoButton.onClick.AddListener(HandleInfo);
        }

        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(HandleNewGame);
            _continueButton.onClick.RemoveListener(HandleContinue);
            _settingsButton.onClick.RemoveListener(HandleSettings);
            _leaderboardButton.onClick.RemoveListener(HandleLeaderboard);
            _infoButton.onClick.RemoveListener(HandleInfo);
        }

        private void OnDestroy()
        {
            _presenter.Dispose();
        }

        // --- IMainMenuView Methods ---

        public void SetContinueButtonInteractable(bool interactable)
        {
            _continueButton.interactable = interactable;
        }

        // --- Button Handlers ---

        private void HandleNewGame() => OnNewGameClicked?.Invoke();
        private void HandleContinue() => OnContinueClicked?.Invoke();
        private void HandleSettings() => OnSettingsClicked?.Invoke();
        private void HandleLeaderboard() => OnLeaderboardClicked?.Invoke();
        private void HandleInfo() => OnInfoClicked?.Invoke();
    }
}
