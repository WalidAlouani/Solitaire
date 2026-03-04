using Solitaire.Domain;

namespace Solitaire.Domain.Piles
{
    /// <summary>
    /// Represents the Stock pile in the Model.
    /// Rules: Cards can only be removed from the top (to go to the Waste).
    /// Cards cannot be added, except by recycling the Waste pile.
    /// </summary>
    public class StockPile : CardPile
    {
        public override bool CanAddCard(CardPile origin, Card card)
        {
            return false;
        }

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