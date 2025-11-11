using Solitaire.Domain;
using Solitaire.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

public class Game
{
    public StockPile Stock { get; private set; }
    public WastePile Waste { get; private set; }
    public List<TableauPile> Tableaus { get; private set; }
    public List<FoundationPile> Foundations { get; private set; }

    // The Presenter subscribes to these
    public event Action OnGameWon;
    public event Action<Card, CardPile> OnCardMoved;
    public event Action<Card> OnCardFlipped;

    private List<Card> _deck;
    private int cardsPerFoundation => Enum.GetValues(typeof(Rank)).Length;

    public Game()
    {
        Stock = new StockPile();
        Waste = new WastePile();

        Tableaus = new List<TableauPile>();
        for (int i = 0; i < 7; i++)
        {
            Tableaus.Add(new TableauPile());
        }

        Foundations = new List<FoundationPile>();
        for (int i = 0; i < 4; i++)
        {
            Foundations.Add(new FoundationPile());
        }

        _deck = DeckFactory.CreateDeck();

        // Subscribe to each card's flip event
        foreach (var card in _deck)
        {
            card.OnFlipped += (isFaceUp) =>
            {
                OnCardFlipped?.Invoke(card);
            };
        }
    }

    public void Deal()
    {
        RecycleAndSuffleStock();
        PopulateTableauPiles();
    }

    public bool TryMoveCard(Card card, CardPile destination)
    {
        CardPile origin = FindPileForCard(card);

        if (origin is TableauPile tableauOrigin && destination is TableauPile tableauDestination)
        {
            return TryMoveStack(card, tableauOrigin, tableauDestination);
        }

        if (origin == null || !destination.CanAddCard(origin, card) || !origin.CanRemoveCard(card))
        {
            return false;
        }

        MoveCard(card, destination, origin);

        return true;
    }

    private void MoveCard(Card card, CardPile destination, CardPile origin)
    {
        // Perform the move
        origin.Pop();
        destination.Push(card);

        origin.OnCardRemoved(card);
        destination.OnCardAdded(card);

        OnCardMoved?.Invoke(card, destination);

        // Check for win condition
        if (CheckWin())
        {
            OnGameWon?.Invoke();
        }
    }

    public bool TryMoveStack(Card topCard, TableauPile tableauOrigin, TableauPile tableauDestination)
    {
        if (!tableauOrigin.TryGetCardStack(topCard, out var cardStack))
            return false;

        // Check if the destination can accept the bottom card of the stack
        if (!tableauDestination.CanAddCard(tableauOrigin, cardStack[0]))
            return false;

        foreach (var card in cardStack)
        {
            MoveCard(card, tableauDestination, tableauOrigin);
        }

        return true;
    }

    public void DrawFromStock()
    {
        if (Stock.Count == 0)
        {
            if (Waste.Count == 0)
                return;

            var cards = Waste.GetCards();

            foreach (var card in cards)
            {
                MoveCard(card, Stock, Waste);
            }

            return;
        }

        TryMoveCard(Stock.Peek(), Waste);
    }

    public CardPile FindPileForCard(Card card)
    {
        foreach (var p in Tableaus) if (p.GetCards().Contains(card)) return p;
        foreach (var p in Foundations) if (p.GetCards().Contains(card)) return p;
        if (Stock.GetCards().Contains(card)) return Stock;
        if (Waste.GetCards().Contains(card)) return Waste;
        return null;
    }

    public bool CheckWin()
    {
        return Foundations.All(f => f.Count == cardsPerFoundation);
    }

    public void RecycleAndSuffleStock()
    {
        Stock.Clear();
        Waste.Clear();

        foreach (var tableau in Tableaus)
            tableau.Clear();

        foreach (var foundation in Foundations)
            foundation.Clear();

        foreach (var card in _deck)
            card.SetFaceUp(false);

        _deck.Shuffle();
        foreach (var card in _deck)
            Stock.Push(card);
    }

    public void PopulateTableauPiles()
    {
        for (int i = 0; i < Tableaus.Count; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                var card = Stock.Pop();
                Tableaus[i].Push(card);
                if (j == i)
                    card.SetFaceUp(true);
            }
        }
    }
}
