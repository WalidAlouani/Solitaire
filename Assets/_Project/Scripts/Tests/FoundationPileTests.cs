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

        // --- CanAddCard ---

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
        public void CanAddCard_LowerRank_Rejected()
        {
            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            var two = new Card(Suit.Hearts, Rank.Two, true);
            _foundation.Push(ace);
            _foundation.Push(two);

            // Try to add an Ace again (lower rank)
            var anotherAce = new Card(Suit.Hearts, Rank.Ace, true);
            _origin.Push(anotherAce);

            Assert.IsFalse(_foundation.CanAddCard(_origin, anotherAce));
        }

        [Test]
        public void CanAddCard_KingOnQueen_Allowed()
        {
            // Build foundation up to Queen
            for (int r = (int)Rank.Ace; r <= (int)Rank.Queen; r++)
                _foundation.Push(new Card(Suit.Diamonds, (Rank)r, true));

            var king = new Card(Suit.Diamonds, Rank.King, true);
            _origin.Push(king);

            Assert.IsTrue(_foundation.CanAddCard(_origin, king));
        }

        // --- OnCardAdded ---

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
            _foundation.OnCardAddedEvent += (c, p) => eventCard = c;

            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            _foundation.Push(ace);

            Assert.AreSame(ace, eventCard);
        }

        [Test]
        public void CanAddCard_DoesNotMutateSuit()
        {
            var aceHearts = new Card(Suit.Hearts, Rank.Ace, true);
            _origin.Push(aceHearts);

            _foundation.CanAddCard(_origin, aceHearts);

            Assert.AreNotEqual(Suit.Hearts, _foundation.Suit);
        }

        // --- CanRemoveCard ---

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

        // --- Full Sequence ---

        [Test]
        public void FullSequence_AceToKing_AllAccepted()
        {
            var suit = Suit.Hearts;

            for (int r = (int)Rank.Ace; r <= (int)Rank.King; r++)
            {
                var card = new Card(suit, (Rank)r, true);
                _origin.SetCards(new List<Card> { card });

                Assert.IsTrue(_foundation.CanAddCard(_origin, card),
                    $"{(Rank)r} of {suit} should be accepted on foundation.");

                _foundation.Push(card);
            }

            Assert.AreEqual(13, _foundation.Count);
        }

        [Test]
        public void FullSequence_AfterKing_NoMoreCardsAccepted()
        {
            var suit = Suit.Clubs;

            // Fill foundation completely
            for (int r = (int)Rank.Ace; r <= (int)Rank.King; r++)
                _foundation.Push(new Card(suit, (Rank)r, true));

            // No card of any rank should be accepted now
            // (There's no rank above King, but we can test that another Ace is rejected)
            var extraAce = new Card(suit, Rank.Ace, true);
            _origin.Push(extraAce);

            Assert.IsFalse(_foundation.CanAddCard(_origin, extraAce));
        }

        [Test]
        public void Suit_AfterRemovingLastCard_RetainsSuit()
        {
            var ace = new Card(Suit.Diamonds, Rank.Ace, true);
            _foundation.Push(ace);
            Assert.AreEqual(Suit.Diamonds, _foundation.Suit);

            _foundation.Pop();

            // Suit should still be set (implementation detail: suit persists)
            Assert.AreEqual(Suit.Diamonds, _foundation.Suit);
        }

        [Test]
        public void OnCardRemoved_FiresEvent()
        {
            Card eventCard = null;
            _foundation.OnCardRemovedEvent += (c, p) => eventCard = c;

            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            _foundation.Push(ace);
            _foundation.Pop();

            Assert.AreSame(ace, eventCard);
        }
    }
}
