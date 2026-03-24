using Solitaire.Domain;
using Solitaire.Domain.Rules;
using System.Collections.Generic;

namespace Solitaire.Domain.Piles
{
    public class TableauPile : CardPile
    {
        public TableauPile() : base(new TableauDropRule()) { }

        public TableauPile(IDropRule customRule) : base(customRule) { }

        public bool TryGetCardStack(Card topCard, out List<Card> cardsStack)
        {
            cardsStack = null;
            var allCards = GetCardsReverse();

            int index = allCards.IndexOf(topCard);
            if (index == -1)
                return false;

            for (int i = index; i < allCards.Count; i++)
            {
                if (!allCards[i].IsFaceUp)
                    return false;
            }

            cardsStack = allCards.GetRange(index, allCards.Count - index);
            return true;
        }

        public override bool CanRemoveCard(Card card)
        {
            return card.IsFaceUp;
        }

        public override void OnCardAdded(Card card)
        {
            RaiseCardAdded(card);
        }

        public override void OnCardRemoved(Card card)
        {
            if (Count > 0)
                Peek().SetFaceUp(true);

            RaiseCardRemoved(card);
        }
    }
}
