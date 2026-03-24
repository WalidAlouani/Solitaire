using Solitaire.Domain.Piles;

namespace Solitaire.Domain.Rules
{
    /// <summary>
    /// Stock pile rule: never accepts cards through normal play.
    /// Cards are only added via recycling the waste pile (handled by commands, not rules).
    /// </summary>
    public class StockDropRule : IDropRule
    {
        public bool CanAddCard(CardPile pile, CardPile origin, Card card)
        {
            return false;
        }
    }
}
