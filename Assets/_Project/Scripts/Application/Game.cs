using Solitaire.Application.Commands;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using Solitaire.Extensions;
using Solitaire.Infrastructure;
using System;
using System.Collections.Generic;

namespace Solitaire.Application
{
    public class Game
    {
        private const int CARDS_PER_FOUNDATION = 13;

        public StockPile Stock { get; private set; }
        public WastePile Waste { get; private set; }
        public List<TableauPile> Tableaus { get; private set; }
        public List<FoundationPile> Foundations { get; private set; }

        public event Action OnGameWon;
        public event Action<Card, CardPile> OnCardMoved;
        public event Action<Card> OnCardFlipped;

        private List<Card> _deck;
        private CommandManager _commandManager;
        private readonly Dictionary<Card, CardPile> _cardToPile = new Dictionary<Card, CardPile>();

        public Game()
        {
            Stock = new StockPile();
            Waste = new WastePile();

            Tableaus = new List<TableauPile>();
            for (int i = 0; i < 7; i++)
                Tableaus.Add(new TableauPile());

            Foundations = new List<FoundationPile>();
            for (int i = 0; i < 4; i++)
                Foundations.Add(new FoundationPile());

            _deck = DeckFactory.CreateDeck();

            // Subscribe with named method — no lambdas
            foreach (var card in _deck)
                card.OnFlipped += HandleCardFlipped;

            // Wire pile events
            foreach (var foundation in Foundations)
            {
                foundation.OnCardAddedEvent += HandleFoundationCardAdded;
                foundation.OnCardRemovedEvent += HandleCardRemoved;
            }

            foreach (var tableau in Tableaus)
            {
                tableau.OnCardAddedEvent += HandleCardAdded;
                tableau.OnCardRemovedEvent += HandleCardRemoved;
            }

            Stock.OnCardAddedEvent += HandleCardAdded;
            Stock.OnCardRemovedEvent += HandleCardRemoved;

            Waste.OnCardAddedEvent += HandleCardAdded;
            Waste.OnCardRemovedEvent += HandleCardRemoved;

            _commandManager = new CommandManager();
        }

        // --- Internal event handlers (named methods, no lambdas) ---

        private void HandleCardFlipped(Card card, bool isFaceUp)
        {
            OnCardFlipped?.Invoke(card);
        }

        private void HandleFoundationCardAdded(Card card)
        {
            UpdateCardPileMapping(card);
            OnCardMoved?.Invoke(card, FindPileForCard(card));

            if (CheckWin())
                OnGameWon?.Invoke();
        }

        private void HandleCardAdded(Card card)
        {
            UpdateCardPileMapping(card);
            OnCardMoved?.Invoke(card, FindPileForCard(card));
        }

        private void HandleCardRemoved(Card card)
        {
            // Mapping will be updated when the card is added to its new pile
        }

        // --- Public API ---

        public bool TryMoveCard(Card card, CardPile destination)
        {
            CardPile origin = FindPileForCard(card);

            if (origin is TableauPile tableauOrigin && destination is TableauPile tableauDestination)
            {
                var stackSize = TryMoveStack(card, tableauOrigin, tableauDestination);
                if (stackSize <= 0)
                    return false;

                _commandManager.ExecuteCmd(new MoveCommand(tableauOrigin, tableauDestination, stackSize));
                return true;
            }

            if (origin == null || !destination.CanAddCard(origin, card) || !origin.CanRemoveCard(card))
                return false;

            _commandManager.ExecuteCmd(new MoveCommand(origin, destination));
            return true;
        }

        public int TryMoveStack(Card topCard, TableauPile tableauOrigin, TableauPile tableauDestination)
        {
            if (!tableauOrigin.TryGetCardStack(topCard, out var cardStack))
                return 0;

            if (!tableauDestination.CanAddCard(tableauOrigin, cardStack[0]))
                return 0;

            return cardStack.Count;
        }

        public void DrawFromStock()
        {
            if (Stock.Count == 0)
            {
                if (Waste.Count == 0)
                    return;

                _commandManager.ExecuteCmd(new MoveReverseCommand(Waste, Stock, Waste.Count));
                return;
            }

            TryMoveCard(Stock.Peek(), Waste);
        }

        public CardPile FindPileForCard(Card card)
        {
            if (_cardToPile.TryGetValue(card, out var pile))
                return pile;

            // Fallback linear search — only needed if mapping is stale
            foreach (var p in Tableaus) if (p.Contains(card)) { _cardToPile[card] = p; return p; }
            foreach (var p in Foundations) if (p.Contains(card)) { _cardToPile[card] = p; return p; }
            if (Stock.Contains(card)) { _cardToPile[card] = Stock; return Stock; }
            if (Waste.Contains(card)) { _cardToPile[card] = Waste; return Waste; }
            return null;
        }

        public bool CheckWin()
        {
            for (int i = 0; i < Foundations.Count; i++)
            {
                if (Foundations[i].Count != CARDS_PER_FOUNDATION)
                    return false;
            }
            return true;
        }

        public void RecycleAndShuffleStock()
        {
            Stock.Clear();
            Waste.Clear();

            foreach (var tableau in Tableaus)
                tableau.Clear();

            foreach (var foundation in Foundations)
                foundation.Clear();

            _cardToPile.Clear();

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

        public void Undo()
        {
            if (_commandManager.CanUndo)
                _commandManager.Undo();
        }

        public void Redo()
        {
            if (_commandManager.CanRedo)
                _commandManager.Redo();
        }

        // --- Private helpers ---

        private void UpdateCardPileMapping(Card card)
        {
            // Find which pile actually contains this card now
            foreach (var p in Tableaus) if (p.Contains(card)) { _cardToPile[card] = p; return; }
            foreach (var p in Foundations) if (p.Contains(card)) { _cardToPile[card] = p; return; }
            if (Stock.Contains(card)) { _cardToPile[card] = Stock; return; }
            if (Waste.Contains(card)) { _cardToPile[card] = Waste; return; }
        }
    }
}