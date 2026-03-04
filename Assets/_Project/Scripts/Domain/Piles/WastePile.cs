using Solitaire.Domain;

namespace Solitaire.Domain.Piles
{
    /// <summary>
    /// Represents the Waste pile in the Model.
    /// Rules: Cards can only be added from the Stock.
    /// Only the top card can be removed (to go to a Foundation or Tableau).
    /// </summary>
    public class WastePile : CardPile
    {
        public override bool CanAddCard(CardPile origin, Card card)
        {
            return origin is StockPile;
        }

        public override bool CanRemoveCard(Card card)
        {
            if (IsEmpty())
                return false;

            return card == Peek();
        }

        public override void OnCardAdded(Card card)
        {
            card.SetFaceUp(true);
            RaiseCardAdded(card);
        }

        public override void OnCardRemoved(Card card)
        {
            RaiseCardRemoved(card);
        }
    }
}