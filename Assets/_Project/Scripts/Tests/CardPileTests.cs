using NUnit.Framework;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;

namespace Solitaire.Tests
{
    /// <summary>
    /// Tests for CardPile base class operations using TableauPile as the concrete type.
    /// </summary>
    public class CardPileTests
    {
        private TableauPile _pile;

        [SetUp]
        public void SetUp()
        {
            _pile = new TableauPile();
        }

        [Test]
        public void IsEmpty_NewPile_ReturnsTrue()
        {
            Assert.IsTrue(_pile.IsEmpty());
        }

        [Test]
        public void IsEmpty_AfterPush_ReturnsFalse()
        {
            _pile.Push(new Card(Suit.Hearts, Rank.Ace, true));
            Assert.IsFalse(_pile.IsEmpty());
        }

        [Test]
        public void Count_EmptyPile_ReturnsZero()
        {
            Assert.AreEqual(0, _pile.Count);
        }

        [Test]
        public void Count_AfterPushes_ReturnsCorrectCount()
        {
            _pile.Push(new Card(Suit.Hearts, Rank.Ace, true));
            _pile.Push(new Card(Suit.Spades, Rank.Two, true));
            Assert.AreEqual(2, _pile.Count);
        }

        [Test]
        public void Peek_EmptyPile_ReturnsNull()
        {
            Assert.IsNull(_pile.Peek());
        }

        [Test]
        public void Peek_ReturnsTopCard_DoesNotRemove()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            _pile.Push(card);

            Assert.AreEqual(card, _pile.Peek());
            Assert.AreEqual(1, _pile.Count); // still there
        }

        [Test]
        public void Pop_ReturnsTopCard_RemovesIt()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            _pile.Push(card);

            var popped = _pile.Pop();

            Assert.AreEqual(card, popped);
            Assert.AreEqual(0, _pile.Count);
        }

        [Test]
        public void Push_Pop_LIFO_Order()
        {
            var card1 = new Card(Suit.Hearts, Rank.Ace, true);
            var card2 = new Card(Suit.Spades, Rank.King, true);

            _pile.Push(card1);
            _pile.Push(card2);

            Assert.AreEqual(card2, _pile.Pop());
            Assert.AreEqual(card1, _pile.Pop());
        }

        [Test]
        public void Contains_CardInPile_ReturnsTrue()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            _pile.Push(card);

            Assert.IsTrue(_pile.Contains(card));
        }

        [Test]
        public void Contains_CardNotInPile_ReturnsFalse()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            Assert.IsFalse(_pile.Contains(card));
        }

        [Test]
        public void GetCards_ReturnsTopToBottomOrder()
        {
            var bottom = new Card(Suit.Hearts, Rank.Ace, true);
            var top = new Card(Suit.Spades, Rank.King, true);

            _pile.Push(bottom);
            _pile.Push(top);

            var cards = _pile.GetCards();
            Assert.AreEqual(2, cards.Count);
            Assert.AreSame(top, cards[0]);    // top first
            Assert.AreSame(bottom, cards[1]); // bottom second
        }

        [Test]
        public void GetCardsReverse_ReturnsBottomToTopOrder()
        {
            var bottom = new Card(Suit.Hearts, Rank.Ace, true);
            var top = new Card(Suit.Spades, Rank.King, true);

            _pile.Push(bottom);
            _pile.Push(top);

            var cards = _pile.GetCardsReverse();
            Assert.AreEqual(2, cards.Count);
            Assert.AreSame(bottom, cards[0]); // bottom first
            Assert.AreSame(top, cards[1]);    // top second
        }

        [Test]
        public void SetCards_ReplacesAllCards()
        {
            _pile.Push(new Card(Suit.Clubs, Rank.Three, true));

            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            var king = new Card(Suit.Spades, Rank.King, true);
            var newCards = new List<Card> { ace, king };

            _pile.SetCards(newCards);

            Assert.AreEqual(2, _pile.Count);
            // SetCards pushes in reverse: Push(king) then Push(ace), so ace is on top
            Assert.AreSame(ace, _pile.Peek());
        }

        [Test]
        public void SetCards_FirstElementBecomesTop()
        {
            var card1 = new Card(Suit.Hearts, Rank.Ace, true);
            var card2 = new Card(Suit.Spades, Rank.Two, true);
            var card3 = new Card(Suit.Diamonds, Rank.Three, true);

            _pile.SetCards(new List<Card> { card1, card2, card3 });

            // SetCards pushes in reverse order, so card1 (index 0) is pushed last = top
            Assert.AreSame(card1, _pile.Pop());
            Assert.AreSame(card2, _pile.Pop());
            Assert.AreSame(card3, _pile.Pop());
        }

        [Test]
        public void SetCards_EmptyList_ClearsPile()
        {
            _pile.Push(new Card(Suit.Hearts, Rank.Ace, true));
            _pile.SetCards(new List<Card>());

            Assert.IsTrue(_pile.IsEmpty());
        }

        [Test]
        public void Clear_RemovesAllCards()
        {
            _pile.Push(new Card(Suit.Hearts, Rank.Ace, true));
            _pile.Push(new Card(Suit.Spades, Rank.King, true));

            _pile.Clear();

            Assert.IsTrue(_pile.IsEmpty());
            Assert.AreEqual(0, _pile.Count);
        }

        [Test]
        public void GetCards_ReturnsCopy_NotReference()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            _pile.Push(card);

            var cards = _pile.GetCards();
            cards.Clear(); // mutate the returned list

            Assert.AreEqual(1, _pile.Count, "Modifying GetCards result should not affect the pile.");
        }
    }
}