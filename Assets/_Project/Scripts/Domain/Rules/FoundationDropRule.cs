using Solitaire.Domain.Piles;

namespace Solitaire.Domain.Rules
{
    /// <summary>
    /// Standard Klondike foundation rule:
    /// Empty pile accepts only Aces.
    /// Non-empty pile accepts same suit, one rank higher, and only the top card of the origin pile.
    /// </summary>
    public class FoundationDropRule : IDropRule
    {
        public bool CanAddCard(CardPile pile, CardPile origin, Card card)
        {
            if (pile.IsEmpty())
                return card.Rank == Rank.Ace;

            Card topCard = pile.Peek();

            if (card.Suit != topCard.Suit)
                return false;

            if (origin.Peek() != card)
                return false;

            return card.Rank == topCard.Rank + 1;
        }
    }
}
