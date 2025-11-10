using Solitaire.Domain;
using System;

public class Card : IEquatable<Card>
{
    public Suit Suit { get; private set; }
    public Rank Rank { get; private set; }

    public bool IsFaceUp => _isFaceUp;
    public Action<bool> OnFlipped;

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
        OnFlipped?.Invoke(isFaceUp);
    }

    public bool IsRed => Suit == Suit.Diamonds || Suit == Suit.Hearts;
    public bool IsBlack => Suit == Suit.Clubs || Suit == Suit.Spades;

    public bool Equals(Card other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Suit == other.Suit && Rank == other.Rank;
    }
}
