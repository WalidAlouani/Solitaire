using Solitaire.Application;
using Solitaire.Core.StateMachine;
using Solitaire.Presentation;
using UnityEngine;

namespace Solitaire.Core
{
    /// <summary>
    /// Top-level orchestrator. Owns the Game model, the state machine,
    /// and a reference to whichever IGameUI implementation is active in the scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Assign any MonoBehaviour that implements IGameUI (CanvasGamePresenter or UIToolkitGamePresenter)")]
        [SerializeField] private MonoBehaviour _gameUI;

        [Header("Scenes")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        public GameStateManager StateManager { get; private set; }
        public IGameUI GameUI => _gameUI as IGameUI;
        public Game Game { get; private set; }
        public string MainMenuSceneName => _mainMenuSceneName;

        private void Awake()
        {
            if (_gameUI != null && !(_gameUI is IGameUI))
            {
                Debug.LogError($"GameManager: assigned UI object '{_gameUI.name}' does not implement IGameUI!");
                return;
            }

            RegisterGameStates();
        }

        private void Start()
        {
            StateManager.ChangeState<DealingState>();
        }

        /// <summary>
        /// Creates a fresh Game instance and shuffles the deck into the stock.
        /// PopulateTableauPiles() is NOT called here — the presenter must call it
        /// after spawning card views so the visual tree is ready for the deal animation.
        /// </summary>
        public Game CreateNewGame()
        {
            Game = new Game();
            Game.RecycleAndShuffleStock();
            return Game;
        }

        private void RegisterGameStates()
        {
            StateManager = gameObject.AddComponent<GameStateManager>();
            StateManager.RegisterState(new DealingState(this));
            StateManager.RegisterState(new PlayingState(this));
            StateManager.RegisterState(new WinState(this));
        }
    }
}
