using Solitaire.Application;
using Solitaire.Audio;
using Solitaire.Core.StateMachine;
using Solitaire.Presentation;
using UnityEngine;

namespace Solitaire.Core
{
    /// <summary>
    /// Top-level orchestrator. Owns the Game model, the state machine,
    /// and a reference to whichever IGameUI implementation is active in the scene.
    /// Implements IGameContext so states depend on the interface, not this MonoBehaviour.
    /// </summary>
    public class GameManager : MonoBehaviour, IGameContext
    {
        [Header("UI")]
        [SerializeField] private GamePresenterBase _gameUI;

        [Header("Audio")]
        [SerializeField] private GameAudioHandler _audioHandler;

        [Header("Scenes")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        public GameStateManager StateManager { get; private set; }
        public IGameUI GameUI => _gameUI;
        public Game Game { get; private set; }
        public string MainMenuSceneName => _mainMenuSceneName;
        public GameAudioHandler AudioHandler => _audioHandler;

        private void Awake()
        {
            RegisterGameStates();

            if (_audioHandler != null)
                _audioHandler.BindPresenter(_gameUI);
        }

        private void Start()
        {
            StateManager.ChangeState<DealingState>();
        }

        private void Update()
        {
            StateManager.Tick();
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

            if (_audioHandler != null)
                _audioHandler.BindGame(Game);

            return Game;
        }

        private void RegisterGameStates()
        {
            StateManager = new GameStateManager();
            StateManager.RegisterState(new DealingState(this));
            StateManager.RegisterState(new PlayingState(this));
            StateManager.RegisterState(new AutoCompleteState(this));
            StateManager.RegisterState(new WinState(this));
        }
    }
}
