using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class for managing all game events.
/// </summary>
public static class GameEvents
{
    // --- Input / View Events (View -> Presenter) ---

    // Fired by CardView when it's clicked (not dragged)
    public static event Action<CardView> OnCardClicked;
    public static void RaiseCardClicked(CardView cardView) => OnCardClicked?.Invoke(cardView);

    // Fired by PileView when a card is dropped on it
    public static event Action<CardView, PileView> OnCardDroppedOnPile;
    public static void RaiseCardDroppedOnPile(CardView cardView, PileView pileView) => OnCardDroppedOnPile?.Invoke(cardView, pileView);

    // Fired by CardView when dropped on multiple piles (for UGUI compatibility)
    public static event Action<CardView, List<PileView>> OnCardDroppedOnPiles;
    public static void RaiseCardDroppedOnPiles(CardView cardView, List<PileView> pilesView) => OnCardDroppedOnPiles?.Invoke(cardView, pilesView);

    // Fired by CardView when a drag ends on an invalid location
    public static event Action<CardView> OnCardDragFailed;
    public static void RaiseCardDragFailed(CardView cardView) => OnCardDragFailed?.Invoke(cardView);

    // Fired by a View (e.g. StockPileView) when the stock is clicked
    public static event Action OnStockClicked;
    public static void RaiseStockClicked() => OnStockClicked?.Invoke();

    // Fired by a View when the waste pile is clicked (to recycle)
    public static event Action OnWasteClicked;
    public static void RaiseWasteClicked() => OnWasteClicked?.Invoke();

    // --- Game Logic / Model Events (Presenter -> View) ---

    // Fired by the Presenter when a card's model state changes
    public static event Action<Card, PileView, Vector2> OnCardMoveAnimated;
    public static void RaiseCardMoveAnimated(Card card, PileView newPile, Vector2 targetPos) => OnCardMoveAnimated?.Invoke(card, newPile, targetPos);

    // Fired by the Presenter when a card's face-up status changes
    public static event Action<Card> OnCardFlipped;
    public static void RaiseCardFlipped(Card card) => OnCardFlipped?.Invoke(card);

    // --- Drag State Management ---
    // Flag to track if a UGUI drop was successful, to prevent OnCardDragFailed
    public static bool WasDropSuccessfulThisFrame = false;
}
