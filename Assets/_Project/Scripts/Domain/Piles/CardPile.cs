using Solitaire.Domain;
using System;
using System.Collections.Generic;

namespace Solitaire.Domain.Piles
{
    public abstract class CardPile
    {
        private readonly Stack<Card> _cards = new Stack<Card>();

        public bool IsEmpty() => _cards.Count == 0;
        public int Count => _cards.Count;
        public Card Peek() => _cards.Count > 0 ? _cards.Peek() : null;

        public Card Pop()
        {
            var card = _cards.Pop();
            OnCardRemoved(card);
            return card;
        }

        public void Push(Card card)
        {
            _cards.Push(card);
            OnCardAdded(card);
        }

        public void Clear() => _cards.Clear();

        /// <summary>
        /// Checks if this pile contains the given card without allocating a new list.
        /// </summary>
        public bool Contains(Card card) => _cards.Contains(card);

        /// <summary>
        /// Returns a copy of the cards (top of stack first).
        /// </summary>
        public List<Card> GetCards() => new List<Card>(_cards);

        /// <summary>
        /// Returns a copy of the cards in bottom-to-top order (reverse of stack order).
        /// </summary>
        public List<Card> GetCardsReverse()
        {
            var list = new List<Card>(_cards.Count);
            foreach (var card in _cards)
                list.Add(card);

            // Reverse in-place to get bottom-to-top order (avoids LINQ allocation)
            list.Reverse();
            return list;
        }

        /// <summary>
        /// Sets the pile's cards to the given list, with the first card becoming the bottom.
        /// </summary>
        public void SetCards(List<Card> newCards)
        {
            _cards.Clear();
            for (int i = newCards.Count - 1; i >= 0; i--)
                _cards.Push(newCards[i]);
        }

        /// <summary>
        /// Provides read-only access to iterate over cards without allocating.
        /// </summary>
        public Stack<Card>.Enumerator GetEnumerator() => _cards.GetEnumerator();

        public event Action<Card> OnCardAddedEvent;
        public event Action<Card> OnCardRemovedEvent;

        public abstract bool CanAddCard(CardPile origin, Card card);
        public abstract bool CanRemoveCard(Card card);

        public abstract void OnCardAdded(Card card);
        public abstract void OnCardRemoved(Card card);

        protected void RaiseCardAdded(Card card) => OnCardAddedEvent?.Invoke(card);
        protected void RaiseCardRemoved(Card card) => OnCardRemovedEvent?.Invoke(card);
    }
}