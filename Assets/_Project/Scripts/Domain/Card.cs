using System;

namespace Solitaire.Domain
{
    public class Card : IEquatable<Card>
    {
        public Suit Suit { get; private set; }
        public Rank Rank { get; private set; }

        public bool IsFaceUp => _isFaceUp;

        /// <summary>
        /// Fired when the card's face-up state changes. Passes the card itself and the new face-up state.
        /// </summary>
        public event Action<Card, bool> OnFlipped;

        private bool _isFaceUp;

        public Card(Suit suit, Rank rank, bool isFaceUp = false)
        {
            Suit = suit;
            Rank = rank;
            SetFaceUp(isFaceUp);
        }

        public void SetFaceUp(bool isFaceUp)
        {
            if (_isFaceUp == isFaceUp)
                return;

            _isFaceUp = isFaceUp;
            OnFlipped?.Invoke(this, isFaceUp);
        }

        public bool IsRed => Suit == Suit.Diamonds || Suit == Suit.Hearts;
        public bool IsBlack => Suit == Suit.Clubs || Suit == Suit.Spades;

        public bool Equals(Card other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Suit == other.Suit && Rank == other.Rank;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Card other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Suit, Rank);
        }
    }
}