using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using Solitaire.Presentation.Canvas.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Solitaire.Presentation.Canvas
{
    /// <summary>
    /// Coordinates between the Game model and the visual representation.
    /// Handles input routing and animation triggering.
    /// Card spawning and view mappings are delegated to CardSpawner.
    /// </summary>
    public class GamePresenter : MonoBehaviour, IGameUI
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
        [SerializeField] private float _autoCompleteStepDelay = 0.15f;

        [Header("Scenes")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        private Game _game;
        private ViewEventBus _viewEventBus;
        private bool _canInteract;
        private bool _isAutoCompleting;

        public Game Game => _game;

        // --- Lifecycle ---

        private void OnDisable()
        {
            UnsubscribeFromViewEventBus();
            UnsubscribeFromGame();
            UnsubscribeFromWinScreen();
            UnsubscribeAutoCompleteButton();
        }

        // --- IGameUI ---

        public void StartGame()
        {
            UnsubscribeFromViewEventBus();
            UnsubscribeFromGame();

            if (_winScreenPopup != null)
                _winScreenPopup.Hide();

            HideAutoCompleteButton();
            _isAutoCompleting = false;

            _game = new Game();

            _viewEventBus = new ViewEventBus();
            _cardSpawner.SetEventBus(_viewEventBus);
            SubscribeToViewEventBus();

            BindPileViews();

            _game.RecycleAndShuffleStock();
            _cardSpawner.SpawnAllCards(_game, _stockPileView);
            _game.PopulateTableauPiles();

            StartCoroutine(RefreshAllPileLayouts());

            _game.OnCardMoved += HandleCardMoved;
            _game.OnCardFlipped += HandleCardFlipped;
            _game.OnGameWon += HandleGameWon;
            _game.OnAutoCompleteChanged += HandleAutoCompleteChanged;
        }

        public void ShowWinScreen()
        {
            if (_winScreenPopup == null) return;

            _winScreenPopup.OnPlayAgainClicked += HandlePlayAgainClicked;
            _winScreenPopup.OnMainMenuClicked += HandleMainMenuClicked;
            _winScreenPopup.Show();
        }

        public void RestartGame()
        {
            StopAllCoroutines();
            _isAutoCompleting = false;
            UnsubscribeFromWinScreen();

            _cardSpawner.DestroyAllCards();
            StartGame();
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
                _tableauPileViews[i].Initialize(_game.Tableaus[i]);
            }

            for (int i = 0; i < _game.Foundations.Count; i++)
            {
                _cardSpawner.RegisterPileMapping(_game.Foundations[i], _foundationPileViews[i]);
                _foundationPileViews[i].Initialize(_game.Foundations[i]);
            }

            _cardSpawner.RegisterPileMapping(_game.Stock, _stockPileView);
            _stockPileView.Initialize(_game.Stock);

            _cardSpawner.RegisterPileMapping(_game.Waste, _wastePileView);
            _wastePileView.Initialize(_game.Waste);
        }

        private IEnumerator RefreshAllPileLayouts()
        {
            yield return new WaitForSeconds(1f);

            foreach (var tableau in _game.Tableaus)
            {
                PileView pileView = _cardSpawner.GetPileView(tableau);
                List<Card> cardsInPile = tableau.GetCardsReverse();

                foreach (Card card in cardsInPile)
                {
                    yield return new WaitForSeconds(0.1f);
                    CardView cardView = _cardSpawner.GetCardView(card);
                    cardView.AnimateMove(pileView);
                    if (card.IsFaceUp)
                        cardView.AnimateFlip();
                }
            }

            SetAllCardsInteraction(true);
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
                    return;
            }

            // Then try auto-move to tableau
            foreach (var tableau in _game.Tableaus)
            {
                if (tableau == pile) continue;
                if (_game.TryMoveCard(cardView.Model, tableau))
                    return;
            }
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
                if (success) break;
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

            cardView.AnimateMove(pileView);
            SetAllCardsInteraction(false);

            cardView.OnCardMoveCompleted += OnCardMoveCompleted;
        }

        private void OnCardMoveCompleted(CardView cardView)
        {
            cardView.OnCardMoveCompleted -= OnCardMoveCompleted;
            if (!_isAutoCompleting)
                SetAllCardsInteraction(true);
        }

        private void HandleCardFlipped(Card card)
        {
            if (!_cardSpawner.TryGetCardView(card, out CardView cardView))
                return;

            cardView.AnimateFlip();
            SetAllCardsInteraction(false);

            cardView.OnCardFlipCompleted += OnCardFlipCompleted;
        }

        private void OnCardFlipCompleted(CardView cardView)
        {
            cardView.OnCardFlipCompleted -= OnCardFlipCompleted;
            if (!_isAutoCompleting)
                SetAllCardsInteraction(true);
        }

        private void SetAllCardsInteraction(bool interactable)
        {
            _canInteract = interactable;
            _cardSpawner.SetAllCardsInteraction(interactable);
        }

        private void HandleGameWon()
        {
            SetAllCardsInteraction(false);
            HideAutoCompleteButton();
            _isAutoCompleting = false;
            ShowWinScreen();
        }

        // --- Auto-Complete ---

        private void HandleAutoCompleteChanged(bool available)
        {
            if (available && !_isAutoCompleting)
                ShowAutoCompleteButton();
            else if (!available)
                HideAutoCompleteButton();
        }

        private void HandleAutoCompleteClicked()
        {
            if (_isAutoCompleting) return;
            HideAutoCompleteButton();
            _isAutoCompleting = true;
            SetAllCardsInteraction(false);
            StartCoroutine(RunAutoComplete());
        }

        private IEnumerator RunAutoComplete()
        {
            while (_game.AutoCompleteStep())
            {
                yield return new WaitForSeconds(_autoCompleteStepDelay);
            }
        }

        // --- Win Screen Handlers ---

        private void HandlePlayAgainClicked()
        {
            RestartGame();
        }

        private void HandleMainMenuClicked()
        {
            UnsubscribeFromViewEventBus();
            UnsubscribeFromGame();
            UnsubscribeFromWinScreen();
            UnsubscribeAutoCompleteButton();
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        // --- Cleanup Helpers ---

        private void UnsubscribeFromGame()
        {
            if (_game != null)
            {
                _game.OnCardMoved -= HandleCardMoved;
                _game.OnCardFlipped -= HandleCardFlipped;
                _game.OnGameWon -= HandleGameWon;
                _game.OnAutoCompleteChanged -= HandleAutoCompleteChanged;
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
