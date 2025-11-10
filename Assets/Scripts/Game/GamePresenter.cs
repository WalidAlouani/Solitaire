using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePresenter : MonoBehaviour
{
    [Header("Model")]
    private Game game;

    [Header("View References (Canvas)")]
    [SerializeField] private Transform topLevelCanvas; // For re-parenting during drag
    [SerializeField] private CardView cardPrefab;

    [Header("Pile Views")]
    [SerializeField] private List<PileView> tableauPileViews;
    [SerializeField] private List<PileView> foundationPileViews;
    [SerializeField] private PileView stockPileView;
    [SerializeField] private PileView wastePileView;

    // Mappings to connect Model and View
    private Dictionary<Card, CardView> cardViewMap = new Dictionary<Card, CardView>();
    private Dictionary<CardPile, PileView> pileViewMap = new Dictionary<CardPile, PileView>();

    public Game Game => game;

    void OnEnable()
    {
        // Listen to UI events
        GameEvents.OnCardClicked += HandleCardClicked;
        GameEvents.OnCardDroppedOnPile += HandleCardDroppedOnPile;
        GameEvents.OnCardDragFailed += HandleCardDragFailed;
        GameEvents.OnStockClicked += HandleStockClicked;
    }

    void OnDisable()
    {
        // Stop listening to UI events
        GameEvents.OnCardClicked -= HandleCardClicked;
        GameEvents.OnCardDroppedOnPile -= HandleCardDroppedOnPile;
        GameEvents.OnCardDragFailed -= HandleCardDragFailed;
        GameEvents.OnStockClicked -= HandleStockClicked;

        // Stop listening to Model events
        if (game != null)
        {
            game.OnCardMoved -= HandleCardMoved;
            game.OnCardFlipped -= HandleCardFlipped;
            game.OnGameWon -= HandleGameWon;
        }
    }

    public void StartGame()
    {
        game = new Game();

        // Listen to Model events
        game.OnCardMoved += HandleCardMoved;
        game.OnCardFlipped += HandleCardFlipped;
        game.OnGameWon += HandleGameWon;

        // Bind Model piles to View piles
        for (int i = 0; i < game.Tableaus.Count; i++)
        {
            pileViewMap.Add(game.Tableaus[i], tableauPileViews[i]);
            tableauPileViews[i].Initialize(game.Tableaus[i]);
        }

        for (int i = 0; i < game.Foundations.Count; i++)
        {
            pileViewMap.Add(game.Foundations[i], foundationPileViews[i]);
            foundationPileViews[i].Initialize(game.Foundations[i]);
        }

        pileViewMap.Add(game.Stock, stockPileView);
        stockPileView.Initialize(game.Stock);

        pileViewMap.Add(game.Waste, wastePileView);
        wastePileView.Initialize(game.Waste);

        game.Deal();

        SpawnAndPlaceAllCards();
    }

    private void SpawnAndPlaceAllCards()
    {
        // 1. Stock
        foreach (Card card in game.Stock.GetCards())
        {
            SpawnCard(card, stockPileView);
        }

        // 2. Tableaus
        foreach (TableauPile tableau in game.Tableaus)
        {
            PileView pileView = pileViewMap[tableau];
            foreach (Card card in tableau.GetCards().AsReadOnly().Reverse()) // Must reverse stack
            {
                SpawnCard(card, pileView);
            }
        }

        // All cards are spawned. Now update their positions.
        RefreshAllPileLayouts();
    }

    private CardView SpawnCard(Card cardModel, PileView pileView)
    {
        CardView cardView = Instantiate(cardPrefab, pileView.CardsHolder);

        cardView.Initialize(cardModel);
        cardView.TopLevelCanvasTransform = topLevelCanvas;

        cardViewMap.Add(cardModel, cardView);
        return cardView;
    }

    private void RefreshAllPileLayouts()
    {
        // This is the "magic" that stacks cards correctly
        foreach (var tableau in game.Tableaus)
        {
            PileView pileView = pileViewMap[tableau];

            // GetCards() returns a list from bottom-of-stack to top
            List<Card> cardsInPile = tableau.GetCards();

            // We must reverse to stack from bottom-up visually
            cardsInPile.Reverse();

            foreach (Card card in cardsInPile)
            {
                CardView cardView = cardViewMap[card];
                cardView.transform.SetParent(pileView.CardsHolder); // Ensure parent
                Vector2 nextPos = new Vector2(0, pileView.GetCardPosition(card));
                cardView.GetComponent<RectTransform>().anchoredPosition = nextPos;
                cardView.transform.SetAsLastSibling(); // Puts top card... on top
            }
        }
    }

    // --- Event Handler (Listens to VIEW) ---

    private void HandleCardClicked(CardView cardView)
    {
        var pile = game.FindPileForCard(cardView.Model);

        if (pile is StockPile stockPile)
        {
            game.DrawFromStock();
            return;
        }

        // Try auto-move to foundation
        foreach (var foundation in game.Foundations)
        {
            if (game.TryMoveCard(cardView.Model, foundation))
            {
                return;
            }
        }

        // Try auto-move to tableau
        foreach (var tableau in game.Tableaus)
        {
            if (game.TryMoveCard(cardView.Model, tableau))
            {
                return;
            }
        }
    }

    private void HandleCardDroppedOnPile(CardView cardView, PileView pileView)
    {
        // A drop happened. Validate it with the Model.
        bool success = game.TryMoveCard(cardView.Model, pileView.Model);

        if (!success)
        {
            // Move failed validation. Send it back.
            HandleCardDragFailed(cardView);
        }
        // If success, the game.OnCardMoved event will fire
        // and HandleCardMoved will position it correctly.
    }

    private void HandleCardDragFailed(CardView cardView)
    {
        // Drag failed. Animate back to original pile.
        CardPile originPile = game.FindPileForCard(cardView.Model);
        PileView originView = pileViewMap[originPile];

        // We just re-call HandleCardMoved with the *current* model state
        HandleCardMoved(cardView.Model, originPile);
    }

    public void HandleStockClicked()
    {
        game.DrawFromStock();
    }

    // --- Event Handler (Listens to MODEL) ---

    private void HandleCardMoved(Card card, CardPile newPile)
    {
        CardView cardView = cardViewMap[card];
        PileView pileView = pileViewMap[newPile];

        var targetPos = pileView.GetNextCardPosition();

        cardView.AnimateMove(pileView.CardsHolder, new Vector2(0, targetPos));
    }

    private void HandleCardFlipped(Card card)
    {
        if (!cardViewMap.TryGetValue(card, out CardView cardView))
        {
            return;
        }

        cardView.AnimateFlip();
    }

    private void HandleGameWon()
    {
        Debug.Log("GAME WON!");
        // TODO: Show Win Screen
    }
}
