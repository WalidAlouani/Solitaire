using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System;
using System.Collections;
using UnityEngine;

namespace Solitaire.Presentation
{
    /// <summary>
    /// Abstract base for all IGameUI implementations.
    /// Owns shared state (game model, interaction flags, auto-complete)
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
        protected bool _isAutoCompleting;

        // --- IGameUI Events ---

        public event Action OnDealingComplete;
        public event Action OnPlayAgainRequested;
        public event Action OnMainMenuRequested;
        public event Action OnAutoCompleteRequested;

        public Game Game => _game;

        // ==================================================================
        //  IGameUI — Template Method Implementations
        // ==================================================================

        public void StartGame(Game game)
        {
            UnsubscribeFromGame();
            BeforeStartGame();

            _game = game;
            _isAutoCompleting = false;

            SetupAndSpawn();

            _game.OnCardMoved += HandleCardMoved;
            _game.OnCardFlipped += HandleCardFlipped;

            StartDealAnimation();
        }

        public void Cleanup()
        {
            StopAllCoroutines();
            _isAutoCompleting = false;
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

        public void RunAutoComplete()
        {
            _isAutoCompleting = true;
            SetAllCardsInteraction(false);
            StartCoroutine(AutoCompleteCoroutine());
        }

        public abstract void ShowWinScreen();
        public abstract void ShowAutoCompleteButton();
        public abstract void HideAutoCompleteButton();

        // ==================================================================
        //  Abstract — Presenter-Specific Hooks
        // ==================================================================

        /// <summary>
        /// Called before _game is assigned. Unsubscribe from presenter-specific
        /// events, tear down previous state, etc.
        /// </summary>
        protected abstract void BeforeStartGame();

        /// <summary>
        /// Bind piles, spawn cards, call PopulateTableauPiles.
        /// _game is already set when this runs.
        /// </summary>
        protected abstract void SetupAndSpawn();

        /// <summary>
        /// Kick off the deal animation. Invoke <see cref="InvokeDealingComplete"/>
        /// when the animation finishes.
        /// </summary>
        protected abstract void StartDealAnimation();

        /// <summary>
        /// Presenter-specific cleanup: kill tweens, destroy views,
        /// unsubscribe from UI events, hide overlays.
        /// Flags and game event unsubscription are already handled by the base.
        /// </summary>
        protected abstract void CleanupPresenter();

        /// <summary>
        /// Toggle interaction on all card views.
        /// </summary>
        protected abstract void SetAllCardsInteraction(bool interactable);

        /// <summary>
        /// Animate a card moving to a new pile.
        /// </summary>
        protected abstract void HandleCardMoved(Card card, CardPile newPile);

        /// <summary>
        /// Animate a card flip.
        /// </summary>
        protected abstract void HandleCardFlipped(Card card);

        // ==================================================================
        //  Shared — Interaction Guards
        // ==================================================================

        protected bool CanAct => _canInteract && !_isAutoCompleting;

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
            if (_isAutoCompleting) return;
            OnAutoCompleteRequested?.Invoke();
        }

        // ==================================================================
        //  Shared — Event Invocators
        // ==================================================================

        protected void InvokeDealingComplete() => OnDealingComplete?.Invoke();
        protected void InvokePlayAgainRequested() => OnPlayAgainRequested?.Invoke();
        protected void InvokeMainMenuRequested() => OnMainMenuRequested?.Invoke();

        // ==================================================================
        //  Shared — Helpers
        // ==================================================================

        /// <summary>
        /// Re-enables card interaction after an animation completes,
        /// but only if the current game state still allows it.
        /// </summary>
        protected void RestoreInteractionIfAllowed()
        {
            if (_stateAllowsInteraction && !_isAutoCompleting)
                SetAllCardsInteraction(true);
        }

        private IEnumerator AutoCompleteCoroutine()
        {
            while (_game.AutoCompleteStep())
                yield return new WaitForSeconds(_gameSettings.AutoCompleteStepDelay);

            _isAutoCompleting = false;
        }

        protected void UnsubscribeFromGame()
        {
            if (_game == null) return;
            _game.OnCardMoved -= HandleCardMoved;
            _game.OnCardFlipped -= HandleCardFlipped;
        }
    }
}
