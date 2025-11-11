
/// <summary>
/// Represents the Stock pile in the Model.
/// Rules: Cards can only be removed from the top (to go to the Waste).
/// Cards cannot be added, except by recycling the Waste pile.
/// </summary>
public class StockPile : CardPile
{
    /// <summary>
    /// Can you add a card to the stock?
    /// </summary>
    /// <returns>False. Cards are only added via dealing or recycling.</returns>
    public override bool CanAddCard(CardPile origin, Card card)
    {
        return false;
    }

    /// <summary>
    /// Can you remove a card from the stock?
    /// </summary>
    /// <param name="card">The card to remove.</param>
    /// <returns>True only if it's the top card.</returns>
    public override bool CanRemoveCard(Card card)
    {
        if (IsEmpty())
            return false;

        // You can only remove the top card of the stock
        return card == Peek();
    }

    public override void OnCardAdded(Card card)
    {
        // card in stock should always be face down
        card.SetFaceUp(false);
    }

    public override void OnCardRemoved(Card card)
    {
    }
}