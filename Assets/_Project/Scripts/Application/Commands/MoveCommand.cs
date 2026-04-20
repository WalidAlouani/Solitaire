using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;

namespace Solitaire.Application.Commands
{
    public class MoveCommand : ICommand
    {
        private readonly CardPile _from;
        private readonly CardPile _to;
        private readonly int _count;
        private readonly List<Card> _moved = new List<Card>();
        private Card _exposedCard;
        private bool _exposedCardFlipStateBeforeMove;

        public MoveCommand(CardPile from, CardPile to, int count = 1)
        {
            _from = from;
            _to = to;
            _count = count;
        }

        public void Execute()
        {
            _moved.Clear();

            // Find the card that will be exposed after popping _count cards.
            // Uses the struct enumerator — zero allocation.
            _exposedCard = PeekAt(_from, _count);
            if (_exposedCard != null)
                _exposedCardFlipStateBeforeMove = _exposedCard.IsFaceUp;

            if (_count == 1)
            {
                // Fast path: single-card move — no temp list, no reverse
                var card = _from.Pop();
                _to.Push(card);
                _moved.Add(card);
            }
            else
            {
                // Multi-card: pop into _moved, reverse in-place, then push
                for (int i = 0; i < _count; i++)
                    _moved.Add(_from.Pop());
                _moved.Reverse();

                foreach (var c in _moved)
                    _to.Push(c);
            }
        }

        public void Undo()
        {
            // Restore the exposed card to its original flip state
            if (_exposedCard != null)
                _exposedCard.SetFaceUp(_exposedCardFlipStateBeforeMove);

            // Pop moved items from _to (assumes they are on top)
            for (int i = 0; i < _moved.Count; i++)
                _to.Pop();

            foreach (var c in _moved)
                _from.Push(c);
        }

        /// <summary>
        /// Peeks at the card at the given depth (0 = top) using the struct enumerator.
        /// Returns null if the pile has fewer than (index + 1) cards.
        /// </summary>
        private static Card PeekAt(CardPile pile, int index)
        {
            int i = 0;
            foreach (var card in pile)
            {
                if (i == index) return card;
                i++;
            }
            return null;
        }
    }
}
