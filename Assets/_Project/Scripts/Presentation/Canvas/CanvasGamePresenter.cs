using DG.Tweening;
using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using Solitaire.Presentation.Canvas.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Presentation.Canvas
{
    /// <summary>
    /// Canvas/uGUI implementation of IGameUI.
    /// Handles card animations, drag-and-drop, and visual state.
    /// Flow decisions (dealing → playing → win) are driven by game states.
    /// </summary>
    public class CanvasGamePresenter : MonoBehaviour, IGameUI
    {
        [Header("View References")]
        [SerializeField] private CardSpawner _cardSpawner;

        [Header("Pile Views")]
        [SerializeField] private List<PileView> _tableauPileViews;
        [SerializeField] private List<PileView> _foundationPileViews;
        [SerializeField] private PileView _stockPileView;
        [SerializeField] private PileView _wastePileView;

        [Header("Win Screen")]
        [SerializeField] private WinScreenPopup _winScreenPopup;

        [Header("Auto-Complete")]
        [SerializeField] private Button _autoCompleteButton;
        [SerializeField] private GameSettingsSO _gameSettings;

        private Game _game;
        private ViewEventBus _viewEventBus;
        private bool _canInteract;
        private bool _stateAllowsInteraction;
        private bool _isAutoCompleting;

        // Active deal sequence (for cleanup)
        private Sequence _dealSequence;

        // Active pile punch tweens keyed by PileView (for cleanup)
        private readonly Dictionary<PileView, Sequence> _pilePunchTweens = new Dictionary<PileView, Sequence>();

        // --- IGameUI Events ---

        public event Action OnDealingComplete;
        public event Action OnPlayAgainRequested;
        public event Action OnMainMenuRequested;
        public event Action OnAutoCompleteRequested;

        public Game Game => _game;

        // --- Lifecycle ---

        private void OnDisable()
        {
            UnsubscribeFromViewEventBus();
            UnsubscribeFromGame();
            UnsubscribeFromWinScreen();
            UnsubscribeAutoCompleteButton();
        }

        private void OnDestroy()
        {
            _dealSequence?.Kill();
            foreach (var kvp in _pilePunchTweens)
                kvp.Value?.Kill();
            _pilePunchTweens.Clear();
        }

        // --- IGameUI Implementation ---

        public void StartGame(Game game)
        {
            UnsubscribeFromViewEventBus();
            UnsubscribeFromGame();

            _game = game;
            _isAutoCompleting = false;

            _viewEventBus = new ViewEventBus();
            _cardSpawner.SetEventBus(_viewEventBus);
            SubscribeToViewEventBus();

            BindPileViews();
            _cardSpawner.SpawnAllCards(_game, _stockPileView);
            _game.PopulateTableauPiles();

            _game.OnCardMoved += HandleCardMoved;
            _game.OnCardFlipped += HandleCardFlipped;

            PlayDealAnimation();
        }

        public void ShowWinScreen()
        {
            if (_winScreenPopup == null) return;
            _winScreenPopup.OnPlayAgainClicked += HandlePlayAgainClicked;
            _winScreenPopup.OnMainMenuClicked += HandleMainMenuClicked;
            _winScreenPopup.Show();
        }

        public void ShowAutoCompleteButton()
        {
            if (_autoCompleteButton == null) return;
            _autoCompleteButton.gameObject.SetActive(true);
            _autoCompleteButton.onClick.AddListener(HandleAutoCompleteClicked);
        }

        public void HideAutoCompleteButton()
        {
            if (_autoCompleteButton == null) return;
            _autoCompleteButton.onClick.RemoveListener(HandleAutoCompleteClicked);
            _autoCompleteButton.gameObject.SetActive(false);
        }

        public void RunAutoComplete()
        {
            _isAutoCompleting = true;
            SetAllCardsInteraction(false);
            StartCoroutine(AutoCompleteCoroutine());
        }

        public void SetInteractable(bool interactable)
        {
            _stateAllowsInteraction = interactable;
            _canInteract = interactable;
            _cardSpawner.SetAllCardsInteraction(interactable);
        }

        public void Cleanup()
        {
            StopAllCoroutines();
            _dealSequence?.Kill();
            foreach (var kvp in _pilePunchTweens)
                kvp.Value?.Kill();
            _pilePunchTweens.Clear();
            _isAutoCompleting = false;
            _stateAllowsInteraction = false;

            UnsubscribeFromViewEventBus();
            UnsubscribeFromGame();
            UnsubscribeFromWinScreen();
            UnsubscribeAutoCompleteButton();

            if (_winScreenPopup != null)
                _winScreenPopup.Hide();

            if (_autoCompleteButton != null)
                _autoCompleteButton.gameObject.SetActive(false);

            _cardSpawner.DestroyAllCards();
        }

        // --- View Event Bus ---

        private void SubscribeToViewEventBus()
        {
            if (_viewEventBus == null) return;
            _viewEventBus.OnCardClicked += HandleCardClicked;
            _viewEventBus.OnCardDroppedOnPiles += HandleCardDroppedOnPiles;
            _viewEventBus.OnCardDragFailed += HandleCardDragFailed;
            _viewEventBus.OnStockClicked += HandleStockClicked;
        }

        private void UnsubscribeFromViewEventBus()
        {
            if (_viewEventBus == null) return;
            _viewEventBus.OnCardClicked -= HandleCardClicked;
            _viewEventBus.OnCardDroppedOnPiles -= HandleCardDroppedOnPiles;
            _viewEventBus.OnCardDragFailed -= HandleCardDragFailed;
            _viewEventBus.OnStockClicked -= HandleStockClicked;
        }

        // --- Pile Binding ---

        private void BindPileViews()
        {
            for (int i = 0; i < _game.Tableaus.Count; i++)
            {
                _cardSpawner.RegisterPileMapping(_game.Tableaus[i], _tableauPileViews[i]);
                _tableauPileViews[i].Initialize(_game.Tableaus[i], _gameSettings.CanvasTableauOffsets.FaceUpOffset, _gameSettings.CanvasTableauOffsets.FaceDownOffset);
            }

            for (int i = 0; i < _game.Foundations.Count; i++)
            {
                _cardSpawner.RegisterPileMapping(_game.Foundations[i], _foundationPileViews[i]);
                _foundationPileViews[i].Initialize(_game.Foundations[i], 0f, 0f);
            }

            _cardSpawner.RegisterPileMapping(_game.Stock, _stockPileView);
            _stockPileView.Initialize(_game.Stock, _gameSettings.CanvasStockOffsets.FaceUpOffset, _gameSettings.CanvasStockOffsets.FaceDownOffset);

            _cardSpawner.RegisterPileMapping(_game.Waste, _wastePileView);
            _wastePileView.Initialize(_game.Waste, _gameSettings.CanvasWasteOffsets.FaceUpOffset, _gameSettings.CanvasWasteOffsets.FaceDownOffset);
        }

        // --- Deal Animation (DOTween Sequence) ---

        private void PlayDealAnimation()
        {
            _dealSequence?.Kill();
            _dealSequence = DOTween.Sequence();

            _dealSequence.AppendInterval(_gameSettings.DealStartDelay);

            foreach (var tableau in _game.Tableaus)
            {
                PileView pileView = _cardSpawner.GetPileView(tableau);
                List<Card> cardsInPile = tableau.GetCardsReverse();

                foreach (Card card in cardsInPile)
                {
                    CardView cardView = _cardSpawner.GetCardView(card);
                    bool shouldFlip = card.IsFaceUp;

                    _dealSequence.AppendInterval(_gameSettings.DealCardDelay);
                    _dealSequence.AppendCallback(() =>
                    {
                        cardView.AnimateMove(pileView, _gameSettings.DealCardDuration);
                        if (shouldFlip)
                            cardView.AnimateFlip(_gameSettings.CardFlipDuration);
                    });
                }
            }

            _dealSequence.AppendCallback(() => OnDealingComplete?.Invoke());
        }

        // --- View Event Handlers ---

        private void HandleCardClicked(CardView cardView)
        {
            if (!_canInteract || _isAutoCompleting) return;

            var pile = _game.FindPileForCard(cardView.Model);

            if (pile is StockPile)
            {
                _game.DrawFromStock();
                return;
            }

            // Try auto-move to foundation first
            foreach (var foundation in _game.Foundations)
            {
                if (foundation == pile) continue;
                if (_game.TryMoveCard(cardView.Model, foundation))
                {
                    // Punch effect on the foundation pile view
                    PileView foundationView = _cardSpawner.GetPileView(foundation);
                    AnimatePilePunch(foundationView);
                    return;
                }
            }

            // Then try auto-move to tableau
            foreach (var tableau in _game.Tableaus)
            {
                if (tableau == pile) continue;
                if (_game.TryMoveCard(cardView.Model, tableau))
                    return;
            }

            // No valid move found — shake the card
            cardView.AnimateShake();
        }

        private void HandleCardDroppedOnPiles(CardView cardView, List<PileView> pilesView)
        {
            if (!_canInteract || _isAutoCompleting) return;

            var success = false;
            var currentPile = _game.FindPileForCard(cardView.Model);

            foreach (var pileView in pilesView)
            {
                if (pileView.Model == currentPile) continue;
                success = _game.TryMoveCard(cardView.Model, pileView.Model);
                if (success)
                {
                    // Punch effect when dropping onto a foundation
                    if (pileView.Model is FoundationPile)
                        AnimatePilePunch(pileView);
                    break;
                }
            }

            if (!success)
                HandleCardDragFailed(cardView);
        }

        private void HandleCardDragFailed(CardView cardView)
        {
            CardPile originPile = _game.FindPileForCard(cardView.Model);
            HandleCardMoved(cardView.Model, originPile);
        }

        private void HandleStockClicked()
        {
            if (!_canInteract || _isAutoCompleting) return;
            _game.DrawFromStock();
        }

        public void HandleUndo()
        {
            if (!_canInteract || _isAutoCompleting) return;
            _game.Undo();
        }

        public void HandleRedo()
        {
            if (!_canInteract || _isAutoCompleting) return;
            _game.Redo();
        }

        // --- Model Event Handlers ---

        private void HandleCardMoved(Card card, CardPile newPile)
        {
            CardView cardView = _cardSpawner.GetCardView(card);
            PileView pileView = _cardSpawner.GetPileView(newPile);

            cardView.AnimateMove(pileView, _gameSettings.CardMoveDuration);
            SetAllCardsInteraction(false);

            cardView.OnCardMoveCompleted += OnCardMoveCompleted;
        }

        private void OnCardMoveCompleted(CardView cardView)
        {
            cardView.OnCardMoveCompleted -= OnCardMoveCompleted;
            if (_stateAllowsInteraction && !_isAutoCompleting)
                SetAllCardsInteraction(true);
        }

        private void HandleCardFlipped(Card card)
        {
            if (!_cardSpawner.TryGetCardView(card, out CardView cardView))
                return;

            cardView.AnimateFlip(_gameSettings.CardFlipDuration);
            SetAllCardsInteraction(false);

            cardView.OnCardFlipCompleted += OnCardFlipCompleted;
        }

        private void OnCardFlipCompleted(CardView cardView)
        {
            cardView.OnCardFlipCompleted -= OnCardFlipCompleted;
            if (_stateAllowsInteraction && !_isAutoCompleting)
                SetAllCardsInteraction(true);
        }

        private void SetAllCardsInteraction(bool interactable)
        {
            _canInteract = interactable;
            _cardSpawner.SetAllCardsInteraction(interactable);
        }

        // --- Pile Effects ---

        /// <summary>
        /// Quick punch-scale on a pile's visual children (Image, DottedOutline) only.
        /// CardsHolder is left untouched so card scales are not affected.
        /// </summary>
        private void AnimatePilePunch(PileView pileView)
        {
            // Kill any existing punch on this pile
            if (_pilePunchTweens.TryGetValue(pileView, out Sequence existing))
            {
                existing?.Kill();
                _pilePunchTweens.Remove(pileView);
            }

            pileView.ResetVisualsScale();

            // Capture for closure
            PileView capturedPile = pileView;
            float scale = 1f;

            var punchSeq = DOTween.Sequence();

            punchSeq.Append(
                DOTween.To(
                    () => scale,
                    x =>
                    {
                        scale = x;
                        capturedPile.SetVisualsScale(x);
                    },
                    1.12f, 0.1f
                ).SetEase(Ease.OutQuad)
            );
            punchSeq.Append(
                DOTween.To(
                    () => scale,
                    x =>
                    {
                        scale = x;
                        capturedPile.SetVisualsScale(x);
                    },
                    1f, 0.15f
                ).SetEase(Ease.InOutQuad)
            );

            // OnKill fires on both manual Kill() and natural completion
            punchSeq.OnKill(() =>
            {
                capturedPile.ResetVisualsScale();
                _pilePunchTweens.Remove(capturedPile);
            });

            _pilePunchTweens[pileView] = punchSeq;
        }

        // --- Auto-Complete ---

        private void HandleAutoCompleteClicked()
        {
            if (_isAutoCompleting) return;
            OnAutoCompleteRequested?.Invoke();
        }

        private IEnumerator AutoCompleteCoroutine()
        {
            while (_game.AutoCompleteStep())
            {
                yield return new WaitForSeconds(_gameSettings.AutoCompleteStepDelay);
            }
            _isAutoCompleting = false;
        }

        // --- Win Screen Handlers ---

        private void HandlePlayAgainClicked()
        {
            OnPlayAgainRequested?.Invoke();
        }

        private void HandleMainMenuClicked()
        {
            OnMainMenuRequested?.Invoke();
        }

        // --- Cleanup Helpers ---

        private void UnsubscribeFromGame()
        {
            if (_game != null)
            {
                _game.OnCardMoved -= HandleCardMoved;
                _game.OnCardFlipped -= HandleCardFlipped;
            }
        }

        private void UnsubscribeFromWinScreen()
        {
            if (_winScreenPopup != null)
            {
                _winScreenPopup.OnPlayAgainClicked -= HandlePlayAgainClicked;
                _winScreenPopup.OnMainMenuClicked -= HandleMainMenuClicked;
            }
        }

        private void UnsubscribeAutoCompleteButton()
        {
            if (_autoCompleteButton != null)
                _autoCompleteButton.onClick.RemoveListener(HandleAutoCompleteClicked);
        }
    }
}
