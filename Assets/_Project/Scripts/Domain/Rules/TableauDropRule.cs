using Solitaire.Domain.Piles;

namespace Solitaire.Domain.Rules
{
    /// <summary>
    /// Standard Klondike tableau rule:
    /// Empty pile accepts only Kings.
    /// Non-empty pile accepts cards of opposite color and one rank lower.
    /// </summary>
    public class TableauDropRule : IDropRule
    {
        public bool CanAddCard(CardPile pile, CardPile origin, Card card)
        {
            if (pile.IsEmpty())
                return card.Rank == Rank.King;

            Card topCard = pile.Peek();
            return card.IsRed != topCard.IsRed && card.Rank == topCard.Rank - 1;
        }
    }
}
