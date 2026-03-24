using Solitaire.Domain;
using Solitaire.Domain.Rules;

namespace Solitaire.Domain.Piles
{
    public class FoundationPile : CardPile
    {
        public Suit Suit { get; private set; }

        public FoundationPile() : base(new FoundationDropRule()) { }

        public FoundationPile(IDropRule customRule) : base(customRule) { }

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
