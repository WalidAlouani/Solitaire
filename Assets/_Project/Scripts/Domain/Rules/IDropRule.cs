using Solitaire.Domain.Piles;

namespace Solitaire.Domain.Rules
{
    /// <summary>
    /// Strategy interface for pile card-acceptance rules.
    /// Each pile type has a default rule, but custom rules can be injected
    /// for testing or "house rules" variants.
    /// </summary>
    public interface IDropRule
    {
        bool CanAddCard(CardPile pile, CardPile origin, Card card);
    }
}
