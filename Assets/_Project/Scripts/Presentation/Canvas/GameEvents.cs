using Solitaire.Domain;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Presentation.Canvas
{
    /// <summary>
    /// Static event bus for view-layer communication.
    /// Call ClearAllSubscribers() on scene unload to prevent leaks.
    /// </summary>
    public static class GameEvents
    {
        // --- Input / View Events (View -> Presenter) ---

        public static event Action<CardView> OnCardClicked;
        public static void RaiseCardClicked(CardView cardView) => OnCardClicked?.Invoke(cardView);

        public static event Action<CardView, PileView> OnCardDroppedOnPile;
        public static void RaiseCardDroppedOnPile(CardView cardView, PileView pileView) => OnCardDroppedOnPile?.Invoke(cardView, pileView);

        public static event Action<CardView, List<PileView>> OnCardDroppedOnPiles;
        public static void RaiseCardDroppedOnPiles(CardView cardView, List<PileView> pilesView) => OnCardDroppedOnPiles?.Invoke(cardView, pilesView);

        public static event Action<CardView> OnCardDragFailed;
        public static void RaiseCardDragFailed(CardView cardView) => OnCardDragFailed?.Invoke(cardView);

        public static event Action OnStockClicked;
        public static void RaiseStockClicked() => OnStockClicked?.Invoke();

        public static event Action OnWasteClicked;
        public static void RaiseWasteClicked() => OnWasteClicked?.Invoke();

        // --- Game Logic / Model Events (Presenter -> View) ---

        public static event Action<Card, PileView, Vector2> OnCardMoveAnimated;
        public static void RaiseCardMoveAnimated(Card card, PileView newPile, Vector2 targetPos) => OnCardMoveAnimated?.Invoke(card, newPile, targetPos);

        public static event Action<Card> OnCardFlipped;
        public static void RaiseCardFlipped(Card card) => OnCardFlipped?.Invoke(card);

        // --- Drag State Management ---

        private static bool _wasDropSuccessfulThisFrame;

        public static bool WasDropSuccessfulThisFrame
        {
            get => _wasDropSuccessfulThisFrame;
            set => _wasDropSuccessfulThisFrame = value;
        }

        /// <summary>
        /// Clears all static event subscribers. Call on scene unload to prevent memory leaks.
        /// </summary>
        public static void ClearAllSubscribers()
        {
            OnCardClicked = null;
            OnCardDroppedOnPile = null;
            OnCardDroppedOnPiles = null;
            OnCardDragFailed = null;
            OnStockClicked = null;
            OnWasteClicked = null;
            OnCardMoveAnimated = null;
            OnCardFlipped = null;
        }
    }
}