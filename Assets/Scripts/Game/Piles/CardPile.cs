using System.Collections.Generic;

public abstract class CardPile
{
    protected Stack<Card> cards = new Stack<Card>();

    public bool IsEmpty() => cards.Count == 0;
    public int Count => cards.Count;
    public Card Peek() => cards.Count > 0 ? cards.Peek() : null;
    public Card Pop() => cards.Pop();
    public void Push(Card card) => cards.Push(card);
    public void Clear() => cards.Clear();

    public List<Card> GetCards() => new List<Card>(cards); // Return a copy

    /// <summary>
    /// Sets the pile's cards to the given list, with the last card in the list becoming the top of the pile.
    /// </summary>
    /// <param name="newCards">Cards in bottom-to-top order.</param>
    public void SetCards(List<Card> newCards)
    {
        cards.Clear();
        for (int i = newCards.Count - 1; i >= 0; i--)
        {
            cards.Push(newCards[i]);
        }
    }

    // Abstract methods to enforce rules in derived classes
    // This is the core of the rules engine!
    public abstract bool CanAddCard(CardPile origin, Card card);
    public abstract bool CanRemoveCard(Card card);
    public abstract void OnCardAdded(Card card);
    public abstract void OnCardRemoved(Card card);
}