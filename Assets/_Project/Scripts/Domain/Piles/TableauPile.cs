using Solitaire.Domain;
using System.Collections.Generic;

namespace Solitaire.Domain.Piles
{
    public class TableauPile : CardPile
    {
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

        public override bool CanAddCard(CardPile origin, Card card)
        {
            if (IsEmpty())
            {
                return card.Rank == Rank.King;
            }

            Card topCard = Peek();
            return card.IsRed == topCard.IsBlack && card.Rank == topCard.Rank - 1;
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