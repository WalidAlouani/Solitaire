using NUnit.Framework;
using Solitaire.Application.Commands;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;

namespace Solitaire.Tests
{
    public class MoveCommandTests
    {
        [Test]
        public void Execute_SingleCard_MovesFromOriginToDestination()
        {
            var from = new TableauPile();
            var to = new TableauPile();

            var king = new Card(Suit.Spades, Rank.King, true);
            from.Push(king);

            var cmd = new MoveCommand(from, to);
            cmd.Execute();

            Assert.AreEqual(0, from.Count);
            Assert.AreEqual(1, to.Count);
            Assert.AreEqual(king, to.Peek());
        }

        [Test]
        public void Undo_SingleCard_RestoresOriginalState()
        {
            var from = new TableauPile();
            var to = new TableauPile();

            var king = new Card(Suit.Spades, Rank.King, true);
            from.Push(king);

            var cmd = new MoveCommand(from, to);
            cmd.Execute();
            cmd.Undo();

            Assert.AreEqual(1, from.Count);
            Assert.AreEqual(0, to.Count);
            Assert.AreEqual(king, from.Peek());
        }

        [Test]
        public void Execute_MultipleCards_MovesStack()
        {
            var from = new TableauPile();
            var to = new TableauPile();

            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);
            var red4 = new Card(Suit.Diamonds, Rank.Four, true);

            from.Push(red6);
            from.Push(black5);
            from.Push(red4);

            var black7 = new Card(Suit.Clubs, Rank.Seven, true);
            to.Push(black7);

            var cmd = new MoveCommand(from, to, 3);
            cmd.Execute();

            Assert.AreEqual(0, from.Count);
            Assert.AreEqual(4, to.Count);
            Assert.AreEqual(red4, to.Peek()); // red4 should be on top
        }

        [Test]
        public void Undo_MultipleCards_RestoresStackOrder()
        {
            var from = new TableauPile();
            var to = new TableauPile();

            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);

            from.Push(red6);
            from.Push(black5);

            var black7 = new Card(Suit.Clubs, Rank.Seven, true);
            to.Push(black7);

            var cmd = new MoveCommand(from, to, 2);
            cmd.Execute();
            cmd.Undo();

            Assert.AreEqual(2, from.Count);
            Assert.AreEqual(1, to.Count);
            Assert.AreEqual(black5, from.Peek()); // black5 should be back on top
        }

        [Test]
        public void Undo_RestoresExposedCardFlipState()
        {
            var from = new TableauPile();
            var to = new TableauPile();

            var faceDown = new Card(Suit.Diamonds, Rank.King, false);
            var faceUp = new Card(Suit.Hearts, Rank.Queen, true);

            from.Push(faceDown);
            from.Push(faceUp);

            var king = new Card(Suit.Spades, Rank.King, true);
            to.Push(king);

            var cmd = new MoveCommand(from, to);
            cmd.Execute();

            // After move, faceDown is now exposed and auto-flipped by TableauPile.OnCardRemoved
            Assert.IsTrue(faceDown.IsFaceUp);

            cmd.Undo();

            // After undo, faceDown should be restored to its original flip state
            Assert.IsFalse(faceDown.IsFaceUp, "Exposed card should be restored to original face-down state on undo.");
        }
    }

    public class MoveReverseCommandTests
    {
        [Test]
        public void Execute_MovesAllCards_InReverseOrder()
        {
            var from = new WastePile();
            var to = new StockPile();

            var card1 = new Card(Suit.Hearts, Rank.Ace, true);
            var card2 = new Card(Suit.Hearts, Rank.Two, true);
            var card3 = new Card(Suit.Hearts, Rank.Three, true);

            from.Push(card1);
            from.Push(card2);
            from.Push(card3); // card3 is on top

            var cmd = new MoveReverseCommand(from, to, 3);
            cmd.Execute();

            Assert.AreEqual(0, from.Count);
            Assert.AreEqual(3, to.Count);
            // MoveReverseCommand pops from top (card3, card2, card1) and pushes in that order
            // So card1 ends up on top of stock
            Assert.AreEqual(card1, to.Peek());
        }

        [Test]
        public void Undo_RestoresOriginalOrder()
        {
            var from = new WastePile();
            var to = new StockPile();

            var card1 = new Card(Suit.Hearts, Rank.Ace, true);
            var card2 = new Card(Suit.Hearts, Rank.Two, true);
            var card3 = new Card(Suit.Hearts, Rank.Three, true);

            from.Push(card1);
            from.Push(card2);
            from.Push(card3);

            var cmd = new MoveReverseCommand(from, to, 3);
            cmd.Execute();
            cmd.Undo();

            Assert.AreEqual(3, from.Count);
            Assert.AreEqual(0, to.Count);
            Assert.AreEqual(card3, from.Peek()); // card3 should be back on top
        }

        [Test]
        public void Execute_SingleCard_Works()
        {
            var from = new WastePile();
            var to = new StockPile();

            var card = new Card(Suit.Spades, Rank.King, true);
            from.Push(card);

            var cmd = new MoveReverseCommand(from, to, 1);
            cmd.Execute();

            Assert.AreEqual(0, from.Count);
            Assert.AreEqual(1, to.Count);
        }

        [Test]
        public void Undo_DoesNotMutateMovedList()
        {
            // Regression: ensure calling Undo multiple times doesn't corrupt state
            var from = new WastePile();
            var to = new StockPile();

            var card1 = new Card(Suit.Hearts, Rank.Ace, true);
            var card2 = new Card(Suit.Hearts, Rank.Two, true);

            from.Push(card1);
            from.Push(card2);

            var cmd = new MoveReverseCommand(from, to, 2);
            cmd.Execute();

            Assert.AreEqual(0, from.Count);
            Assert.AreEqual(2, to.Count);

            cmd.Undo();

            Assert.AreEqual(2, from.Count);
            Assert.AreEqual(0, to.Count);
            Assert.AreEqual(card2, from.Peek());

            // Execute again should produce same result
            cmd.Execute();

            Assert.AreEqual(0, from.Count);
            Assert.AreEqual(2, to.Count);
        }
    }
}