using Solitaire.Domain;

namespace Solitaire.Domain.Piles
{
    public class FoundationPile : CardPile
    {
        public Suit Suit { get; private set; }

        public override bool CanAddCard(CardPile origin, Card card)
        {
            if (IsEmpty())
            {
                return card.Rank == Rank.Ace; // Only Aces on empty foundations
            }

            if (card.Suit != Suit)
                return false;

            if (origin.Peek() != card)
                return false;

            // Must be same suit and one rank higher
            return card.Rank == Peek().Rank + 1;
        }

        public override bool CanRemoveCard(Card card)
        {
            if (IsEmpty())
                return false;

            return card == Peek();
        }

        public override void OnCardAdded(Card card)
        {
            if (Count == 1)
                Suit = card.Suit;

            card.SetFaceUp(true);
            RaiseCardAdded(card);
        }

        public override void OnCardRemoved(Card card)
        {
            RaiseCardRemoved(card);
        }
    }
}