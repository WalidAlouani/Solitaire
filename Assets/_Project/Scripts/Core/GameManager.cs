using Solitaire.Core.StateMachine;
using Solitaire.Presentation;
using UnityEngine;

namespace Solitaire.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Assign any MonoBehaviour that implements IGameUI (SolitaireUIManager or GamePresenter)")]
        [SerializeField] private MonoBehaviour _gameUI;

        public GameStateManager StateManager { get; private set; }

        public IGameUI GameUI => _gameUI as IGameUI;

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

        private void RegisterGameStates()
        {
            StateManager = gameObject.AddComponent<GameStateManager>();
            StateManager.RegisterState(new DealingState(this));
            StateManager.RegisterState(new PlayingState(this));
            StateManager.RegisterState(new WinState(this));
        }
    }
}
