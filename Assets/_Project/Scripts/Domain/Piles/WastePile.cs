using Solitaire.Domain;
using Solitaire.Domain.Rules;

namespace Solitaire.Domain.Piles
{
    /// <summary>
    /// Represents the Waste pile in the Model.
    /// Rules: Cards can only be added from a drawable source (StockPile).
    /// Only the top card can be removed (to go to a Foundation or Tableau).
    /// </summary>
    public class WastePile : CardPile
    {
        public WastePile() : base(new WasteDropRule()) { }

        public WastePile(IDropRule customRule) : base(customRule) { }

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
