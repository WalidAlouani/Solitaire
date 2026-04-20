using NUnit.Framework;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;

namespace Solitaire.Tests
{
    public class TableauPileTests
    {
        private TableauPile _tableau;
        private TableauPile _origin;

        [SetUp]
        public void SetUp()
        {
            _tableau = new TableauPile();
            _origin = new TableauPile();
        }

        // --- CanAddCard ---

        [Test]
        public void CanAddCard_EmptyTableau_KingAllowed()
        {
            var king = new Card(Suit.Spades, Rank.King, true);
            _origin.Push(king);

            Assert.IsTrue(_tableau.CanAddCard(_origin, king));
        }

        [Test]
        public void CanAddCard_EmptyTableau_NonKingRejected()
        {
            var queen = new Card(Suit.Spades, Rank.Queen, true);
            _origin.Push(queen);

            Assert.IsFalse(_tableau.CanAddCard(_origin, queen));
        }

        [Test]
        public void CanAddCard_AlternatingColor_OneLessRank_Allowed()
        {
            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            _tableau.Push(black7);

            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            _origin.Push(red6);

            Assert.IsTrue(_tableau.CanAddCard(_origin, red6));
        }

        [Test]
        public void CanAddCard_SameColor_Rejected()
        {
            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            _tableau.Push(black7);

            var black6 = new Card(Suit.Clubs, Rank.Six, true);
            _origin.Push(black6);

            Assert.IsFalse(_tableau.CanAddCard(_origin, black6));
        }

        [Test]
        public void CanAddCard_WrongRank_Rejected()
        {
            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            _tableau.Push(black7);

            var red5 = new Card(Suit.Hearts, Rank.Five, true);
            _origin.Push(red5);

            Assert.IsFalse(_tableau.CanAddCard(_origin, red5));
        }

        // --- CanRemoveCard ---

        [Test]
        public void CanRemoveCard_FaceUpCard_Allowed()
        {
            var card = new Card(Suit.Hearts, Rank.Five, true);
            Assert.IsTrue(_tableau.CanRemoveCard(card));
        }

        [Test]
        public void CanRemoveCard_FaceDownCard_Rejected()
        {
            var card = new Card(Suit.Hearts, Rank.Five, false);
            Assert.IsFalse(_tableau.CanRemoveCard(card));
        }

        // --- OnCardRemoved auto-flip ---

        [Test]
        public void OnCardRemoved_ExposesNextCard_FlipsFaceUp()
        {
            var bottom = new Card(Suit.Diamonds, Rank.King, false);
            var top = new Card(Suit.Spades, Rank.Queen, true);

            _tableau.Push(bottom);
            _tableau.Push(top);

            Assert.IsFalse(bottom.IsFaceUp);

            _tableau.Pop(); // remove top

            Assert.IsTrue(bottom.IsFaceUp, "Card below should auto-flip face up when exposed.");
        }

        [Test]
        public void OnCardRemoved_EmptyAfterPop_NoError()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            _tableau.Push(card);

            Assert.DoesNotThrow(() => _tableau.Pop());
            Assert.IsTrue(_tableau.IsEmpty());
        }

        // --- TryGetCardStack ---

        [Test]
        public void TryGetCardStack_SingleFaceUpCard_ReturnsStack()
        {
            var card = new Card(Suit.Hearts, Rank.Five, true);
            _tableau.Push(card);

            bool result = _tableau.TryGetCardStack(card, out var stack);

            Assert.IsTrue(result);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(card, stack[0]);
        }

        [Test]
        public void TryGetCardStack_MultipleFaceUpCards_ReturnsFullStack()
        {
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);
            var red4 = new Card(Suit.Diamonds, Rank.Four, true);

            _tableau.Push(red6);
            _tableau.Push(black5);
            _tableau.Push(red4);

            bool result = _tableau.TryGetCardStack(red6, out var stack);

            Assert.IsTrue(result);
            Assert.AreEqual(3, stack.Count);
            Assert.AreEqual(red6, stack[0]);
            Assert.AreEqual(black5, stack[1]);
            Assert.AreEqual(red4, stack[2]);
        }

        [Test]
        public void TryGetCardStack_FaceDownCardInStack_ReturnsFalse()
        {
            var faceDown = new Card(Suit.Hearts, Rank.Six, false);
            var faceUp = new Card(Suit.Spades, Rank.Five, true);

            _tableau.Push(faceDown);
            _tableau.Push(faceUp);

            bool result = _tableau.TryGetCardStack(faceDown, out var stack);

            Assert.IsFalse(result);
            Assert.IsNull(stack);
        }

        [Test]
        public void TryGetCardStack_CardNotInPile_ReturnsFalse()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);

            bool result = _tableau.TryGetCardStack(card, out var stack);

            Assert.IsFalse(result);
            Assert.IsNull(stack);
        }

        [Test]
        public void TryGetCardStack_PartialStack_FromMiddle()
        {
            var faceDown = new Card(Suit.Clubs, Rank.King, false);
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);

            _tableau.Push(faceDown);
            _tableau.Push(red6);
            _tableau.Push(black5);

            // Get stack starting from red6 (skipping faceDown)
            bool result = _tableau.TryGetCardStack(red6, out var stack);

            Assert.IsTrue(result);
            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual(red6, stack[0]);
            Assert.AreEqual(black5, stack[1]);
        }

        // --- Events ---

        [Test]
        public void OnCardAdded_FiresEvent()
        {
            Card eventCard = null;
            _tableau.OnCardAddedEvent += (c, p) => eventCard = c;

            var card = new Card(Suit.Hearts, Rank.Five, true);
            _tableau.Push(card);

            Assert.AreSame(card, eventCard);
        }

        [Test]
        public void OnCardRemoved_FiresEvent()
        {
            Card eventCard = null;
            _tableau.OnCardRemovedEvent += (c, p) => eventCard = c;

            var card = new Card(Suit.Hearts, Rank.Five, true);
            _tableau.Push(card);
            _tableau.Pop();

            Assert.AreSame(card, eventCard);
        }
    }
}