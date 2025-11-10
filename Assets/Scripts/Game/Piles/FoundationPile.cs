using Solitaire.Domain;

public class FoundationPile : CardPile
{
    public Suit Suit { get; private set; }

    public FoundationPile(Suit suit)
    {
        Suit = suit;
    }

    public override bool CanAddCard(CardPile origin, Card card)
    {
        if (card.Suit != this.Suit)
            return false;

        if (Count == 0)
        {
            return card.Rank == Rank.Ace; // Only Aces on empty foundations
        }

        if (origin.Peek() != card)
            return false;

        // Must be same suit and one rank higher
        return card.Rank == Peek().Rank + 1;
    }

    public override bool CanRemoveCard(Card card)
    {
        // Right now, cards cannot be removed from foundation piles
        // Add the possibility later
        return false;
    }

    public override void OnCardAdded(Card card)
    {
        // When a card is added to the foundation, it should be face up
        card.SetFaceUp(true);
    }

    public override void OnCardRemoved(Card card)
    {
    }
}