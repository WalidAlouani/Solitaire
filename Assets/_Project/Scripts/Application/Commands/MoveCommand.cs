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
            
            // Track the card that will be exposed after we pop _count cards
            // We need to peek at the card that's _count positions from the top
            var allCards = _from.GetCards();
            if (allCards.Count > _count)
            {
                _exposedCard = allCards[_count];
                _exposedCardFlipStateBeforeMove = _exposedCard.IsFaceUp;
            }
            else
            {
                _exposedCard = null;
            }

            var tmp = new List<Card>();
            for (int i = 0; i < _count; i++) 
                tmp.Add(_from.Pop());
            tmp.Reverse();

            foreach (var c in tmp) 
            { 
                _to.Push(c); 
                _moved.Add(c);
            }
        }

        public void Undo()
        {        
            // Restore the exposed card to its original flip state
            if (_exposedCard != null)
            {
                _exposedCard.SetFaceUp(_exposedCardFlipStateBeforeMove);
            }

            // Pop moved items from to (assumes they are on top)
            for (int i = 0; i < _moved.Count; i++) 
                _to.Pop();

            foreach (var c in _moved)
            {
                _from.Push(c);
            }
        }
    }
}