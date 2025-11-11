using Solitaire.Domain;

public class FoundationPile : CardPile
{
    public Suit Suit { get; private set; }

    public override bool CanAddCard(CardPile origin, Card card)
    {
        if (IsEmpty())
        {
            Suit = card.Suit;
            return card.Rank == Rank.Ace; // Only Aces on empty foundations
        }

        if (card.Suit != this.Suit)
            return false;

        if (origin.Peek() != card)
            return false;

        // Must be same suit and one rank higher
        return card.Rank == Peek().Rank + 1;
    }

    public override bool CanRemoveCard(Card card)
    {
        if (IsEmpty())
            return false;

        // You can only remove the top card of the foundation
        return card == Peek();
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