using DG.Tweening;
using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using Solitaire.Presentation.Canvas.UI;
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
    public class CanvasGamePresenter : GamePresenterBase
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

        private ViewEventBus _viewEventBus;

        // Active deal sequence (for cleanup)
        private Sequence _dealSequence;

        // Active pile punch tweens keyed by PileView (for cleanup)
        private readonly Dictionary<PileView, Sequence> _pilePunchTweens = new Dictionary<PileView, Sequence>();

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

        // ==================================================================
        //  GamePresenterBase Overrides
        // ==================================================================

        protected override void BeforeStartGame()
        {
            UnsubscribeFromViewEventBus();
        }

        protected override void SetupAndSpawn()
        {
            _viewEventBus = new ViewEventBus();
            _cardSpawner.SetEventBus(_viewEventBus);
            SubscribeToViewEventBus();

            BindPileViews();
            _cardSpawner.SpawnAllCards(_game, _stockPileView);
            _game.PopulateTableauPiles();
        }

        protected override void StartDealAnimation()
        {
            PlayDealAnimation();
        }

        protected override void CleanupPresenter()
        {
            _dealSequence?.Kill();
            foreach (var kvp in _pilePunchTweens)
                kvp.Value?.Kill();
            _pilePunchTweens.Clear();

            UnsubscribeFromViewEventBus();
            UnsubscribeFromWinScreen();
            UnsubscribeAutoCompleteButton();

            if (_winScreenPopup != null)
                _winScreenPopup.Hide();

            if (_autoCompleteButton != null)
                _autoCompleteButton.gameObject.SetActive(false);

            _cardSpawner.DestroyAllCards();
        }

        protected override void SetAllCardsInteraction(bool interactable)
        {
            _canInteract = interactable;
            _cardSpawner.SetAllCardsInteraction(interactable);
        }

        public override void ShowWinScreen()
        {
            if (_winScreenPopup == null) return;
            _winScreenPopup.OnPlayAgainClicked += HandlePlayAgainClicked;
            _winScreenPopup.OnMainMenuClicked += HandleMainMenuClicked;
            _winScreenPopup.Show();
        }

        public override void ShowAutoCompleteButton()
        {
            if (_autoCompleteButton == null) return;
            _autoCompleteButton.gameObject.SetActive(true);
            _autoCompleteButton.onClick.AddListener(HandleAutoCompleteClicked);
        }

        public override void HideAutoCompleteButton()
        {
            if (_autoCompleteButton == null) return;
            _autoCompleteButton.onClick.RemoveListener(HandleAutoCompleteClicked);
            _autoCompleteButton.gameObject.SetActive(false);
        }

        // ==================================================================
        //  Model Event Handlers
        // ==================================================================

        protected override void HandleCardMoved(Card card, CardPile newPile)
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
            RestoreInteractionIfAllowed();
        }

        protected override void HandleCardFlipped(Card card)
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
            RestoreInteractionIfAllowed();
        }

        // ==================================================================
        //  View Event Bus
        // ==================================================================

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

            _dealSequence.AppendCallback(() => InvokeDealingComplete());
        }

        // --- View Event Handlers ---

        private void HandleCardClicked(CardView cardView)
        {
            if (!CanAct) return;

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
            if (!CanAct) return;

            var success = false;
            var currentPile = _game.FindPileForCard(cardView.Model);

            foreach (var pileView in pilesView)
            {
                if (pileView.Model == currentPile) continue;
                success = _game.TryMoveCard(cardView.Model, pileView.Model);
                if (success)
                {
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

        // --- Pile Effects ---

        private void AnimatePilePunch(PileView pileView)
        {
            if (_pilePunchTweens.TryGetValue(pileView, out Sequence existing))
            {
                existing?.Kill();
                _pilePunchTweens.Remove(pileView);
            }

            pileView.ResetVisualsScale();

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

            punchSeq.OnKill(() =>
            {
                capturedPile.ResetVisualsScale();
                _pilePunchTweens.Remove(capturedPile);
            });

            _pilePunchTweens[pileView] = punchSeq;
        }

        // --- Win Screen Handlers ---

        private void HandlePlayAgainClicked()
        {
            InvokePlayAgainRequested();
        }

        private void HandleMainMenuClicked()
        {
            InvokeMainMenuRequested();
        }

        // --- Cleanup Helpers ---

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
