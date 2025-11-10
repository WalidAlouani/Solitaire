using Solitaire.Domain;
using System.Collections.Generic;
using System.Linq;

public class TableauPile : CardPile
{
    // A tableau pile can have multiple cards removed at once
    public bool TryGetCardStack(Card topCard, out List<Card> cardsStack)
    {
        cardsStack = null;
        var allCards = GetCards();
        allCards.Reverse(); // bottom-to-top order

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
        if (Count == 0)
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
        // When a card is added to the tableau, it should be face up
        card.SetFaceUp(true);
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