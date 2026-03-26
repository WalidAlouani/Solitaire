using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Solitaire.UI.MainMenu
{
    /// <summary>
    /// Single controller for the Main Menu screen.
    /// Binds buttons directly to their actions — no need for
    /// a separate presenter/interface for a simple menu.
    /// </summary>
    public class MainMenuController : MonoBehaviour
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

        private void Start()
        {
            // TODO: check for saved game data and toggle continue button
            _continueButton.interactable = false;
        }

        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
            _settingsButton.onClick.AddListener(OnSettings);
            _leaderboardButton.onClick.AddListener(OnLeaderboard);
            _infoButton.onClick.AddListener(OnInfo);
        }

        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(OnNewGame);
            _continueButton.onClick.RemoveListener(OnContinue);
            _settingsButton.onClick.RemoveListener(OnSettings);
            _leaderboardButton.onClick.RemoveListener(OnLeaderboard);
            _infoButton.onClick.RemoveListener(OnInfo);
        }

        private void OnNewGame()
        {
            SceneManager.LoadScene(_gameSceneName);
        }

        private void OnContinue()
        {
            // TODO: load saved game state, then transition to game scene
            Debug.Log("[MainMenu] Continue — not implemented yet");
        }

        private void OnSettings()
        {
            // TODO: open settings panel/popup
            Debug.Log("[MainMenu] Settings — not implemented yet");
        }

        private void OnLeaderboard()
        {
            // TODO: open leaderboard panel/popup
            Debug.Log("[MainMenu] Leaderboard — not implemented yet");
        }

        private void OnInfo()
        {
            // TODO: open info/about panel/popup
            Debug.Log("[MainMenu] Info — not implemented yet");
        }
    }
}
