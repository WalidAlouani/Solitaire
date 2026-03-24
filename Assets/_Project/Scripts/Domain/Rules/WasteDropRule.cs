using Solitaire.Domain.Piles;

namespace Solitaire.Domain.Rules
{
    /// <summary>
    /// Waste pile rule: only accepts cards from a drawable source (e.g. StockPile).
    /// Uses the IDrawableSource marker interface instead of checking concrete types,
    /// following the Dependency Inversion Principle.
    /// </summary>
    public class WasteDropRule : IDropRule
    {
        public bool CanAddCard(CardPile pile, CardPile origin, Card card)
        {
            return origin is IDrawableSource;
        }
    }
}
