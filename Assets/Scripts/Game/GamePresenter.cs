using System;
using System.Collections;
using System.Collections.Generic;
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

    private bool _canInteract = false;

    public Game Game => game;

    void OnEnable()
    {
        // Listen to UI events
        GameEvents.OnCardClicked += HandleCardClicked;
        GameEvents.OnCardDroppedOnPiles += HandleCardDroppedOnPiles;
        GameEvents.OnCardDragFailed += HandleCardDragFailed;
        GameEvents.OnStockClicked += HandleStockClicked;
    }

    void OnDisable()
    {
        // Stop listening to UI events
        GameEvents.OnCardClicked -= HandleCardClicked;
        GameEvents.OnCardDroppedOnPiles -= HandleCardDroppedOnPiles;
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

        game.RecycleAndSuffleStock();

        SpawnAndPlaceAllCards();

        game.PopulateTableauPiles();

        // All cards are spawned. Now update their positions.
        StartCoroutine(RefreshAllPileLayouts());

        // Listen to Model events
        game.OnCardMoved += HandleCardMoved;
        game.OnCardFlipped += HandleCardFlipped;
        game.OnGameWon += HandleGameWon;
    }

    private void SpawnAndPlaceAllCards()
    {
        // 1. Stock
        var stockPile = game.Stock.GetCardsReverse();

        foreach (Card card in stockPile)
        {
            SpawnCard(card, stockPileView);
        }
    }

    private CardView SpawnCard(Card cardModel, PileView pileView)
    {
        CardView cardView = Instantiate(cardPrefab, pileView.CardsHolder.transform);

        cardView.Initialize(cardModel);
        cardView.TopLevelCanvasTransform = topLevelCanvas;

        cardViewMap.Add(cardModel, cardView);

        pileView.ParentToPile(cardView);
        Vector2 nextPos = new Vector2(0, pileView.GetCardPosition(cardModel));
        cardView.GetComponent<RectTransform>().anchoredPosition = nextPos;

        return cardView;
    }

    private IEnumerator RefreshAllPileLayouts()
    {
        yield return new WaitForSeconds(1f);

        // This is the "magic" that stacks cards correctly
        foreach (var tableau in game.Tableaus)
        {
            PileView pileView = pileViewMap[tableau];

            List<Card> cardsInPile = tableau.GetCardsReverse();

            foreach (Card card in cardsInPile)
            {
                yield return new WaitForSeconds(0.1f);
                CardView cardView = cardViewMap[card];
                cardView.AnimateMove(pileView);
                if (card.IsFaceUp)
                    cardView.AnimateFlip();
            }
        }

        SetAllCardsInteraction(true);
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
            if (foundation == pile)
                continue; // Skip same pile

            if (game.TryMoveCard(cardView.Model, foundation))
                return;
        }

        // Try auto-move to tableau
        foreach (var tableau in game.Tableaus)
        {
            if (tableau == pile)
                continue; // Skip same pile

            if (game.TryMoveCard(cardView.Model, tableau))
                return;
        }
    }

    private void HandleCardDroppedOnPiles(CardView cardView, List<PileView> pilesView)
    {
        var success = false;
        var currentPile = game.FindPileForCard(cardView.Model);

        foreach (var pileView in pilesView)
        {
            if (pileView.Model == currentPile)
                continue; // Skip same pile

            success = game.TryMoveCard(cardView.Model, pileView.Model);
            if (success)
                break;
        }

        if (!success)
        {
            // Move failed validation. Send it back.
            HandleCardDragFailed(cardView);
        }
    }

    private void HandleCardDragFailed(CardView cardView)
    {
        // Drag failed. Animate back to original pile.
        CardPile originPile = game.FindPileForCard(cardView.Model);
        PileView originView = pileViewMap[originPile];

        HandleCardMoved(cardView.Model, originPile);
    }

    public void HandleStockClicked()
    {
        if (!_canInteract)
            return;

        game.DrawFromStock();
    }

    public void HandleUndo() 
    {
        if (!_canInteract)
            return;

        game.Undo();
    }

    public void HandleRedo() 
    {
        if (!_canInteract)
            return;

        game.Redo();
    }

    // --- Event Handler (Listens to MODEL) ---

    private void HandleCardMoved(Card card, CardPile newPile)
    {
        CardView cardView = cardViewMap[card];
        PileView pileView = pileViewMap[newPile];

        cardView.AnimateMove(pileView);
        SetAllCardsInteraction(false);

        cardView.OnCardMoveCompleted += CardMoveCompleted;
    }

    private void CardMoveCompleted(CardView cardView)
    {
        cardView.OnCardMoveCompleted -= CardMoveCompleted;
        SetAllCardsInteraction(true);
    }

    private void HandleCardFlipped(Card card)
    {
        if (!cardViewMap.TryGetValue(card, out CardView cardView))
            return;

        cardView.AnimateFlip();
        SetAllCardsInteraction(false);

        cardView.OnCardFlipCompleted += CardFlipCompleted;
    }

    private void CardFlipCompleted(CardView cardView)
    {
        cardView.OnCardFlipCompleted -= CardFlipCompleted;
        SetAllCardsInteraction(true);
    }

    private void SetAllCardsInteraction(bool interactable)
    {
        _canInteract = interactable;
        foreach (var kvp in cardViewMap)
        {
            kvp.Value.SetInteractable(interactable);
        }
    }

    private void HandleGameWon()
    {
        Debug.Log("GAME WON!");
        SetAllCardsInteraction(false);
        // TODO: Show Win Screen
    }
}
