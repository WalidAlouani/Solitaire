using Solitaire.Domain;
using Solitaire.Domain.Rules;

namespace Solitaire.Domain.Piles
{
    /// <summary>
    /// Represents the Stock pile in the Model.
    /// Rules: Cards can only be removed from the top (to go to the Waste).
    /// Cards cannot be added, except by recycling the Waste pile.
    /// Implements IDrawableSource so WasteDropRule can verify origin
    /// without depending on the concrete StockPile type.
    /// </summary>
    public class StockPile : CardPile, IDrawableSource
    {
        public StockPile() : base(new StockDropRule()) { }

        public StockPile(IDropRule customRule) : base(customRule) { }

        public override bool CanRemoveCard(Card card)
        {
            if (IsEmpty())
                return false;

            return card == Peek();
        }

        public override void OnCardAdded(Card card)
        {
            card.SetFaceUp(false);
            RaiseCardAdded(card);
        }

        public override void OnCardRemoved(Card card)
        {
            RaiseCardRemoved(card);
        }
    }
}
