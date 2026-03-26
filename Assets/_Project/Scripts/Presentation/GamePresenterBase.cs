using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System;
using UnityEngine;

namespace Solitaire.Presentation
{
    /// <summary>
    /// Abstract base for all IGameUI implementations.
    /// Owns shared state (game model, interaction flags)
    /// and provides template methods for the presenter lifecycle.
    /// Subclasses implement only the visual/animation-specific logic.
    /// </summary>
    public abstract class GamePresenterBase : MonoBehaviour, IGameUI
    {
        [Header("Settings")]
        [SerializeField] protected GameSettingsSO _gameSettings;

        protected Game _game;
        protected bool _canInteract;
        protected bool _stateAllowsInteraction;

        // --- IGameUI Events ---

        public event Action OnDealingComplete;
        public event Action OnPlayAgainRequested;
        public event Action OnMainMenuRequested;
        public event Action OnAutoCompleteRequested;

        // --- Presenter Events (for audio, haptics, analytics, etc.) ---

        public event Action OnDealCardAnimated;
        public event Action OnInvalidMoveAttempted;
        public event Action OnAutoCompleteStepPerformed;

        // --- Properties ---

        public Game Game => _game;
        public GameSettingsSO GameSettings => _gameSettings;

        // ==================================================================
        //  IGameUI — Template Method Implementations
        // ==================================================================

        public void StartGame(Game game)
        {
            UnsubscribeFromGame();
            BeforeStartGame();

            _game = game;

            SetupAndSpawn();

            _game.OnCardMoved += HandleCardMoved;
            _game.OnCardFlipped += HandleCardFlipped;

            StartDealAnimation();
        }

        public void Cleanup()
        {
            StopAllCoroutines();
            _stateAllowsInteraction = false;

            UnsubscribeFromGame();
            CleanupPresenter();
        }

        public void SetInteractable(bool interactable)
        {
            _stateAllowsInteraction = interactable;
            _canInteract = interactable;
            SetAllCardsInteraction(interactable);
        }

        public abstract void ShowWinScreen();
        public abstract void ShowAutoCompleteButton();
        public abstract void HideAutoCompleteButton();

        // ==================================================================
        //  Abstract — Presenter-Specific Hooks
        // ==================================================================

        protected abstract void BeforeStartGame();
        protected abstract void SetupAndSpawn();
        protected abstract void StartDealAnimation();
        protected abstract void CleanupPresenter();
        protected abstract void SetAllCardsInteraction(bool interactable);
        protected abstract void HandleCardMoved(Card card, CardPile newPile);
        protected abstract void HandleCardFlipped(Card card);

        // ==================================================================
        //  Shared — Interaction Guards
        // ==================================================================

        protected bool CanAct => _canInteract;

        protected void HandleStockClicked()
        {
            if (!CanAct) return;
            _game.DrawFromStock();
        }

        public void HandleUndo()
        {
            if (!CanAct) return;
            _game.Undo();
        }

        public void HandleRedo()
        {
            if (!CanAct) return;
            _game.Redo();
        }

        protected void HandleAutoCompleteClicked()
        {
            OnAutoCompleteRequested?.Invoke();
        }

        // ==================================================================
        //  Shared — Event Invocators
        // ==================================================================

        protected void InvokeDealingComplete() => OnDealingComplete?.Invoke();
        protected void InvokePlayAgainRequested() => OnPlayAgainRequested?.Invoke();
        protected void InvokeMainMenuRequested() => OnMainMenuRequested?.Invoke();
        protected void InvokeDealCardAnimated() => OnDealCardAnimated?.Invoke();
        protected void InvokeInvalidMoveAttempted() => OnInvalidMoveAttempted?.Invoke();
        protected void InvokeAutoCompleteStepPerformed() => OnAutoCompleteStepPerformed?.Invoke();

        // ==================================================================
        //  Shared — Helpers
        // ==================================================================

        protected void RestoreInteractionIfAllowed()
        {
            if (_stateAllowsInteraction)
                SetAllCardsInteraction(true);
        }

        protected void UnsubscribeFromGame()
        {
            if (_game == null) return;
            _game.OnCardMoved -= HandleCardMoved;
            _game.OnCardFlipped -= HandleCardFlipped;
        }
    }
}
