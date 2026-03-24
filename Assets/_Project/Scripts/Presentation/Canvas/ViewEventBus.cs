using System;
using System.Collections.Generic;

namespace Solitaire.Presentation.Canvas
{
    /// <summary>
    /// Instance-based event bus for view-layer communication (View → Presenter).
    /// Owned by GamePresenter and injected into CardViews via CardSpawner.
    /// Replaces the old static GameEvents class — no manual cleanup needed,
    /// the bus is garbage-collected when the presenter is destroyed.
    /// </summary>
    public class ViewEventBus
    {
        public event Action<CardView> OnCardClicked;
        public event Action<CardView, List<PileView>> OnCardDroppedOnPiles;
        public event Action<CardView> OnCardDragFailed;
        public event Action OnStockClicked;

        public void RaiseCardClicked(CardView cardView)
            => OnCardClicked?.Invoke(cardView);

        public void RaiseCardDroppedOnPiles(CardView cardView, List<PileView> piles)
            => OnCardDroppedOnPiles?.Invoke(cardView, piles);

        public void RaiseCardDragFailed(CardView cardView)
            => OnCardDragFailed?.Invoke(cardView);

        public void RaiseStockClicked()
            => OnStockClicked?.Invoke();
    }
}
