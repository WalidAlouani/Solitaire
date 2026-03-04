using NUnit.Framework;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;

namespace Solitaire.Tests
{
    public class FoundationPileTests
    {
        private FoundationPile _foundation;
        private TableauPile _origin;

        [SetUp]
        public void SetUp()
        {
            _foundation = new FoundationPile();
            _origin = new TableauPile();
        }

        [Test]
        public void CanAddCard_EmptyFoundation_AceAllowed()
        {
            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            _origin.Push(ace);

            Assert.IsTrue(_foundation.CanAddCard(_origin, ace));
        }

        [Test]
        public void CanAddCard_EmptyFoundation_NonAceRejected()
        {
            var two = new Card(Suit.Hearts, Rank.Two, true);
            _origin.Push(two);

            Assert.IsFalse(_foundation.CanAddCard(_origin, two));
        }

        [Test]
        public void CanAddCard_SequentialSameSuit_Allowed()
        {
            var ace = new Card(Suit.Spades, Rank.Ace, true);
            _foundation.Push(ace);

            var two = new Card(Suit.Spades, Rank.Two, true);
            _origin.Push(two);

            Assert.IsTrue(_foundation.CanAddCard(_origin, two));
        }

        [Test]
        public void CanAddCard_WrongSuit_Rejected()
        {
            var ace = new Card(Suit.Spades, Rank.Ace, true);
            _foundation.Push(ace);

            var two = new Card(Suit.Hearts, Rank.Two, true);
            _origin.Push(two);

            Assert.IsFalse(_foundation.CanAddCard(_origin, two));
        }

        [Test]
        public void CanAddCard_SkippedRank_Rejected()
        {
            var ace = new Card(Suit.Spades, Rank.Ace, true);
            _foundation.Push(ace);

            var three = new Card(Suit.Spades, Rank.Three, true);
            _origin.Push(three);

            Assert.IsFalse(_foundation.CanAddCard(_origin, three));
        }

        [Test]
        public void CanAddCard_NotTopOfOrigin_Rejected()
        {
            var ace = new Card(Suit.Clubs, Rank.Ace, true);
            _foundation.Push(ace);

            var two = new Card(Suit.Clubs, Rank.Two, true);
            var three = new Card(Suit.Clubs, Rank.Three, true);
            _origin.Push(two);
            _origin.Push(three); // three is on top

            Assert.IsFalse(_foundation.CanAddCard(_origin, two)); // two is not top
        }

        [Test]
        public void OnCardAdded_FirstCard_SetsSuit()
        {
            var ace = new Card(Suit.Diamonds, Rank.Ace, true);
            _foundation.Push(ace);

            Assert.AreEqual(Suit.Diamonds, _foundation.Suit);
        }

        [Test]
        public void OnCardAdded_SetsFaceUp()
        {
            var ace = new Card(Suit.Hearts, Rank.Ace); // face down
            _foundation.Push(ace);

            Assert.IsTrue(ace.IsFaceUp);
        }

        [Test]
        public void OnCardAdded_FiresEvent()
        {
            Card eventCard = null;
            _foundation.OnCardAddedEvent += c => eventCard = c;

            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            _foundation.Push(ace);

            Assert.AreSame(ace, eventCard);
        }

        [Test]
        public void CanAddCard_DoesNotMutateSuit()
        {
            // Regression test: CanAddCard must not set Suit as a side effect
            var aceHearts = new Card(Suit.Hearts, Rank.Ace, true);
            _origin.Push(aceHearts);

            _foundation.CanAddCard(_origin, aceHearts);

            // Suit should still be default (not Hearts) because we only called CanAddCard, not Push
            Assert.AreNotEqual(Suit.Hearts, _foundation.Suit);
        }

        [Test]
        public void CanRemoveCard_TopCard_Allowed()
        {
            var ace = new Card(Suit.Spades, Rank.Ace, true);
            _foundation.Push(ace);

            Assert.IsTrue(_foundation.CanRemoveCard(ace));
        }

        [Test]
        public void CanRemoveCard_Empty_Rejected()
        {
            var card = new Card(Suit.Spades, Rank.Ace, true);
            Assert.IsFalse(_foundation.CanRemoveCard(card));
        }

        [Test]
        public void CanRemoveCard_NotTopCard_Rejected()
        {
            var ace = new Card(Suit.Spades, Rank.Ace, true);
            var two = new Card(Suit.Spades, Rank.Two, true);
            _foundation.Push(ace);
            _foundation.Push(two);

            Assert.IsFalse(_foundation.CanRemoveCard(ace)); // ace is buried
        }
    }
}