using NUnit.Framework;
using Solitaire.Domain;

namespace Solitaire.Tests
{
    public class CardTests
    {
        [Test]
        public void Constructor_DefaultFaceDown_IsFaceUpFalse()
        {
            var card = new Card(Suit.Hearts, Rank.Ace);
            Assert.IsFalse(card.IsFaceUp);
        }

        [Test]
        public void Constructor_FaceUpTrue_IsFaceUpTrue()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            Assert.IsTrue(card.IsFaceUp);
        }

        [Test]
        public void SetFaceUp_ChangesState_FiresEvent()
        {
            var card = new Card(Suit.Spades, Rank.King);
            Card firedCard = null;
            bool firedState = false;

            card.OnFlipped += (c, state) => { firedCard = c; firedState = state; };
            card.SetFaceUp(true);

            Assert.IsTrue(card.IsFaceUp);
            Assert.AreSame(card, firedCard);
            Assert.IsTrue(firedState);
        }

        [Test]
        public void SetFaceUp_SameState_DoesNotFireEvent()
        {
            var card = new Card(Suit.Spades, Rank.King, true);
            bool eventFired = false;

            card.OnFlipped += (c, state) => eventFired = true;
            card.SetFaceUp(true); // Already face up

            Assert.IsFalse(eventFired);
        }

        [Test]
        public void IsRed_HeartsAndDiamonds_ReturnsTrue()
        {
            Assert.IsTrue(new Card(Suit.Hearts, Rank.Ace).IsRed);
            Assert.IsTrue(new Card(Suit.Diamonds, Rank.King).IsRed);
        }

        [Test]
        public void IsBlack_SpadesAndClubs_ReturnsTrue()
        {
            Assert.IsTrue(new Card(Suit.Spades, Rank.Ace).IsBlack);
            Assert.IsTrue(new Card(Suit.Clubs, Rank.King).IsBlack);
        }

        [Test]
        public void IsRed_BlackSuits_ReturnsFalse()
        {
            Assert.IsFalse(new Card(Suit.Spades, Rank.Five).IsRed);
            Assert.IsFalse(new Card(Suit.Clubs, Rank.Five).IsRed);
        }

        [Test]
        public void Equals_SameSuitAndRank_ReturnsTrue()
        {
            var a = new Card(Suit.Hearts, Rank.Seven);
            var b = new Card(Suit.Hearts, Rank.Seven);
            Assert.IsTrue(a.Equals(b));
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Equals_DifferentSuit_ReturnsFalse()
        {
            var a = new Card(Suit.Hearts, Rank.Seven);
            var b = new Card(Suit.Spades, Rank.Seven);
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentRank_ReturnsFalse()
        {
            var a = new Card(Suit.Hearts, Rank.Seven);
            var b = new Card(Suit.Hearts, Rank.Eight);
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var card = new Card(Suit.Hearts, Rank.Ace);
            Assert.IsFalse(card.Equals(null));
        }

        [Test]
        public void GetHashCode_EqualCards_SameHash()
        {
            var a = new Card(Suit.Diamonds, Rank.Queen);
            var b = new Card(Suit.Diamonds, Rank.Queen);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentCards_DifferentHash()
        {
            var a = new Card(Suit.Diamonds, Rank.Queen);
            var b = new Card(Suit.Clubs, Rank.King);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equals_ObjectOverload_WorksCorrectly()
        {
            var a = new Card(Suit.Hearts, Rank.Three);
            object b = new Card(Suit.Hearts, Rank.Three);
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_NonCardObject_ReturnsFalse()
        {
            var card = new Card(Suit.Hearts, Rank.Ace);
            Assert.IsFalse(card.Equals("not a card"));
        }
    }
}