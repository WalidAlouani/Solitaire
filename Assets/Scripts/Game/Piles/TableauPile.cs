using Solitaire.Domain;
using System.Collections.Generic;

public class TableauPile : CardPile
{
    // A tableau pile can have multiple cards removed at once
    public bool TryGetCardStack(Card topCard, out List<Card> cardsStack)
    {
        cardsStack = null;
        var allCards = GetCardsReverse();

        int index = allCards.IndexOf(topCard);
        if (index == -1)
            return false;

        // Only allocate if all cards are face up
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
            return card.Rank == Rank.King; // Only Kings on empty tableaus
        }

        Card topCard = Peek();
        // Must be alternating color and one rank lower
        return card.IsRed == topCard.IsBlack && card.Rank == topCard.Rank - 1;
    }

    public override bool CanRemoveCard(Card card)
    {
        // Can only remove if it's face up
        return card.IsFaceUp;
    }

    public override void OnCardAdded(Card card)
    {
        OnCardAddedEvent?.Invoke(card);
    }

    public override void OnCardRemoved(Card card)
    {
        // If there are cards left, ensure the new top card is face up
        if (Count > 0)
        {
            Peek().SetFaceUp(true);
        }
    }
}