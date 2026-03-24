using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Presentation.Canvas
{
    /// <summary>
    /// Responsible for spawning CardView instances and maintaining model-to-view mappings.
    /// Extracted from GamePresenter to follow Single Responsibility Principle.
    /// </summary>
    public class CardSpawner : MonoBehaviour
    {
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private Transform _topLevelCanvas;
        [SerializeField] private CardThemeSO _cardTheme;

        private readonly Dictionary<Card, CardView> _cardViewMap = new Dictionary<Card, CardView>();
        private readonly Dictionary<CardPile, PileView> _pileViewMap = new Dictionary<CardPile, PileView>();

        private ViewEventBus _eventBus;

        public IReadOnlyDictionary<Card, CardView> CardViewMap => _cardViewMap;
        public IReadOnlyDictionary<CardPile, PileView> PileViewMap => _pileViewMap;

        public void SetEventBus(ViewEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void RegisterPileMapping(CardPile model, PileView view)
        {
            _pileViewMap[model] = view;
        }

        public CardView SpawnCard(Card cardModel, PileView pileView)
        {
            CardView cardView = Instantiate(_cardPrefab, pileView.CardsHolder.transform);

            cardView.Initialize(cardModel, _cardTheme);
            cardView.SetEventBus(_eventBus);
            cardView.TopLevelCanvasTransform = _topLevelCanvas;

            _cardViewMap[cardModel] = cardView;

            pileView.ParentToPile(cardView);
            Vector2 nextPos = new Vector2(0, pileView.GetCardPosition(cardModel));
            cardView.GetComponent<RectTransform>().anchoredPosition = nextPos;

            return cardView;
        }

        public void SpawnAllCards(Game game, PileView stockPileView)
        {
            var stockPile = game.Stock.GetCardsReverse();

            foreach (Card card in stockPile)
            {
                SpawnCard(card, stockPileView);
            }
        }

        public bool TryGetCardView(Card card, out CardView cardView)
        {
            return _cardViewMap.TryGetValue(card, out cardView);
        }

        public bool TryGetPileView(CardPile pile, out PileView pileView)
        {
            return _pileViewMap.TryGetValue(pile, out pileView);
        }

        public CardView GetCardView(Card card) => _cardViewMap[card];
        public PileView GetPileView(CardPile pile) => _pileViewMap[pile];

        public void SetAllCardsInteraction(bool interactable)
        {
            foreach (var kvp in _cardViewMap)
            {
                kvp.Value.SetInteractable(interactable);
            }
        }

        /// <summary>
        /// Destroys all spawned CardView GameObjects and clears all mappings.
        /// Used when restarting the game.
        /// </summary>
        public void DestroyAllCards()
        {
            foreach (var kvp in _cardViewMap)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }

            _cardViewMap.Clear();
            _pileViewMap.Clear();
        }

        public void Clear()
        {
            _cardViewMap.Clear();
            _pileViewMap.Clear();
        }
    }
}
