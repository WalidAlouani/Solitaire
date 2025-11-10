/// <summary>
/// Represents the Waste pile in the Model.
/// Rules: Cards can only be added from the Stock.
/// Only the top card can be removed (to go to a Foundation or Tableau).
/// </summary>
public class WastePile : CardPile
{
    /// <summary>
    /// Can you add a card to the waste pile?
    /// </summary>
    /// <returns>True. The waste pile's job is to accept cards.</returns>
    public override bool CanAddCard(CardPile origin, Card card)
    {
        // The waste pile can always receive a card (from the stock)
        // We don't need to check *where* it came from, 
        // as the Game.cs (Presenter/Controller) logic will handle that.
        // This method just answers "am I allowed to receive this card?"
        return origin is StockPile;
    }

    /// <summary>
    /// Can you remove a card from the waste pile?
    /// </summary>
    /// <param name="card">The card to remove.</param>
    /// <returns>True only if it's the top card.</returns>
    public override bool CanRemoveCard(Card card)
    {
        if (Count == 0)
            return false;

        // You can only remove the top card of the waste
        return card == Peek();
    }

    public override void OnCardAdded(Card card)
    {
        // card in stock should always be face up
        card.SetFaceUp(true);
    }

    public override void OnCardRemoved(Card card)
    {
    }
}