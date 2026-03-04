using NUnit.Framework;
using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;

namespace Solitaire.Tests
{
    public class GameTests
    {
        [Test]
        public void RecycleAndPopulateTableauPiles_ValidatePilesInitialStates_Succeeds()
        {
            var game = new Game();
            var stockPile = game.Stock;
            var wastePile = game.Waste;

            Assert.AreEqual(0, stockPile.Count);
            game.RecycleAndShuffleStock();
            Assert.AreEqual(52, stockPile.Count);
            game.PopulateTableauPiles();
            Assert.AreEqual(24, stockPile.Count);

            Assert.AreEqual(0, wastePile.Count);

            for (int i = 0; i < game.Tableaus.Count; i++)
            {
                var tableau = game.Tableaus[i];
                Assert.AreEqual(tableau.Count, i + 1);
                Assert.True(tableau.Peek().IsFaceUp);
            }

            foreach (var foundation in game.Foundations)
            {
                Assert.AreEqual(0, foundation.Count);
            }
        }

        [Test]
        public void MoveAllCardsFromStockToWaste_AndRecycleWasteToStock_Succeeds()
        {
            var game = new Game();
            var stockPile = game.Stock;
            var wastePile = game.Waste;

            game.RecycleAndShuffleStock();
            game.PopulateTableauPiles();

            for (int i = 0; i < 24; i++)
            {
                var card = stockPile.Peek();
                Assert.False(card.IsFaceUp);
                var moved = game.TryMoveCard(card, wastePile);
                Assert.True(moved);
                Assert.True(card.IsFaceUp);
                Assert.AreEqual(24 - i - 1, stockPile.Count);
                Assert.AreEqual(i + 1, wastePile.Count);
            }

            Assert.True(stockPile.IsEmpty());

            game.DrawFromStock();

            Assert.AreEqual(24, stockPile.Count);
            Assert.AreEqual(0, wastePile.Count);
        }

        [Test]
        public void MoveCard_FromWasteToTableau_ValidMove_Succeeds()
        {
            var game = new Game();

            // Waste: Red 6 face up
            var wasteCard = new Card(Suit.Hearts, Rank.Six, true);
            game.Waste.SetCards(new List<Card> { wasteCard });

            // Tableau: Black 7 face up on top
            var tableauCard = new Card(Suit.Spades, Rank.Seven, true);
            var tableau = game.Tableaus[0];
            tableau.SetCards(new List<Card> { tableauCard });

            // Other tableaus empty
            for (int i = 1; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            // Move red 6 onto black 7
            bool moved = game.TryMoveCard(wasteCard, tableau);
            Assert.IsTrue(moved, "Card should be moved from waste to tableau.");
            Assert.AreEqual(wasteCard, tableau.Peek());
        }

        [Test]
        public void MoveCard_FromTableauToFoundation_ValidMove_Succeeds()
        {
            var game = new Game();

            // Tableau: Ace of Clubs face up
            var aceCard = new Card(Suit.Clubs, Rank.Ace, true);
            var tableau = game.Tableaus[0];
            tableau.SetCards(new List<Card> { aceCard });

            // Foundation for Clubs is empty
            var foundation = game.Foundations.Find(f => f.Suit == Suit.Clubs);
            foundation.SetCards(new List<Card>());

            // Other tableaus/foundations empty
            for (int i = 1; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());
            foreach (var f in game.Foundations)
                if (f.Suit != Suit.Clubs) f.SetCards(new List<Card>());

            bool moved = game.TryMoveCard(aceCard, foundation);
            Assert.IsTrue(moved, "Ace should be moved from tableau to foundation.");
            Assert.AreEqual(aceCard, foundation.Peek());
        }

        [Test]
        public void MoveCard_FromTableauToTableau_InvalidMove_Fails()
        {
            var game = new Game();

            // Tableau 1: Red 5 face up
            var card1 = new Card(Suit.Hearts, Rank.Five, true);
            var t1 = game.Tableaus[0];
            t1.SetCards(new List<Card> { card1 });

            // Tableau 2: Red 6 face up (same color, so move should fail)
            var card2 = new Card(Suit.Diamonds, Rank.Six, true);
            var t2 = game.Tableaus[1];
            t2.SetCards(new List<Card> { card2 });

            // Other tableaus empty
            for (int i = 2; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            bool moved = game.TryMoveCard(card1, t2);
            Assert.IsFalse(moved, "Invalid move should fail.");
        }

        [Test]
        public void MoveCard_FromWasteToFoundation_ValidMove_Succeeds()
        {
            var game = new Game();

            game.Waste.SetCards(new List<Card>
            {
                new Card(Suit.Spades, Rank.Ace, true),
                new Card(Suit.Spades, Rank.Two, true),
            });

            // Foundation for Spades is empty
            var foundation = game.Foundations[0];

            // Move Ace to foundation
            var wasteCard = game.Waste.Peek(); // Ace of Spades
            bool moved = game.TryMoveCard(wasteCard, foundation);
            Assert.IsTrue(moved, "Ace should be moved from waste to foundation.");
            Assert.AreEqual(wasteCard, foundation.Peek());

            wasteCard = game.Waste.Peek(); // Two of Spades
            moved = game.TryMoveCard(wasteCard, foundation);
            Assert.IsTrue(moved, "Two should be moved from waste to foundation.");
            Assert.AreEqual(wasteCard, foundation.Peek());
        }

        [Test]
        public void MoveMultipleCards_FromTableauToTableau_ValidMove_Succeeds()
        {
            var game = new Game();

            // Source tableau: Red 6, Black 5, Red 4 (all face up, bottom to top)
            var card1 = new Card(Suit.Diamonds, Rank.Ace, false);
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);
            var red4 = new Card(Suit.Diamonds, Rank.Four, true);
            var sourceTableau = game.Tableaus[0];

            sourceTableau.Push(card1);
            sourceTableau.Push(red6);
            sourceTableau.Push(black5);
            sourceTableau.Push(red4);

            // Destination tableau: Black 7 face up
            var card2 = new Card(Suit.Spades, Rank.Ace, false);
            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            var destTableau = game.Tableaus[1];
            destTableau.Push(card2);
            destTableau.Push(black7);

            // Simulate moving the stack starting from Red 6 (the bottom of the sequence)
            // This assumes your game logic supports moving a stack starting from a given card.
            // If not, you may need to extend your Game/TableauPile logic.

            bool moved = game.TryMoveCard(red6, destTableau);
            Assert.IsTrue(moved);

            // Validate destination tableau now contains: Black 7, Red 6, Black 5, Red 4 (bottom to top)
            var expectedOrder = new List<Card> { red4, black5, red6, black7, card2 };
            var actualOrder = destTableau.GetCards();
            Assert.AreEqual(expectedOrder.Count, actualOrder.Count);
            for (int i = 0; i < expectedOrder.Count; i++)
            {
                Assert.AreEqual(expectedOrder[i], actualOrder[i]);
            }

            Assert.AreEqual(1, sourceTableau.Count);
        }

        [Test]
        public void Undo_SingleMoveFromTableauToTableau_ReversesMove_Succeeds()
        {
            var game = new Game();

            // Setup: Black 7 and Red 6 in different tableaus
            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];

            t1.SetCards(new List<Card> { red6 });
            t2.SetCards(new List<Card> { black7 });

            for (int i = 2; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            // Move red 6 onto black 7
            bool moved = game.TryMoveCard(red6, t2);
            Assert.IsTrue(moved);
            Assert.AreEqual(0, t1.Count);
            Assert.AreEqual(2, t2.Count);
            Assert.AreEqual(red6, t2.Peek());

            // Undo the move
            game.Undo();

            // Verify state is reverted
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual(1, t2.Count);
            Assert.AreEqual(red6, t1.Peek());
            Assert.AreEqual(black7, t2.Peek());
        }

        [Test]
        public void Undo_MoveFromWasteToFoundation_ReversesMove_Succeeds()
        {
            var game = new Game();

            var aceSpades = new Card(Suit.Spades, Rank.Ace, true);
            game.Waste.SetCards(new List<Card> { aceSpades });

            var foundation = game.Foundations[0];
            foundation.SetCards(new List<Card>());

            // Move Ace to foundation
            bool moved = game.TryMoveCard(aceSpades, foundation);
            Assert.IsTrue(moved);
            Assert.AreEqual(0, game.Waste.Count);
            Assert.AreEqual(1, foundation.Count);

            // Undo the move
            game.Undo();

            // Verify state is reverted
            Assert.AreEqual(1, game.Waste.Count);
            Assert.AreEqual(0, foundation.Count);
            Assert.AreEqual(aceSpades, game.Waste.Peek());
        }

        [Test]
        public void Redo_AfterUndo_ReexecutesMove_Succeeds()
        {
            var game = new Game();

            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];

            t1.SetCards(new List<Card> { red6 });
            t2.SetCards(new List<Card> { black7 });

            for (int i = 2; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            // Move red 6 onto black 7
            bool moved = game.TryMoveCard(red6, t2);
            Assert.IsTrue(moved);
            Assert.AreEqual(red6, t2.Peek());

            // Undo
            game.Undo();
            Assert.AreEqual(red6, t1.Peek());

            // Redo
            game.Redo();
            Assert.AreEqual(red6, t2.Peek());
        }

        [Test]
        public void UndoMultipleMoves_AllMovesReversedInOrder_Succeeds()
        {
            var game = new Game();

            // Setup three tableaus with cards
            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Clubs, Rank.Five, true);

            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];
            var t3 = game.Tableaus[2];

            t1.SetCards(new List<Card> { red6 });
            t2.SetCards(new List<Card> { black7 });
            t3.SetCards(new List<Card> { black5 });

            for (int i = 3; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            // Move 1: red 6 onto black 7
            game.TryMoveCard(red6, t2);
            Assert.AreEqual(2, t2.Count);

            // Move 2: black 5 onto red 6
            game.TryMoveCard(black5, t2);
            Assert.AreEqual(3, t2.Count);

            // Undo Move 2
            game.Undo();
            Assert.AreEqual(2, t2.Count);
            Assert.AreEqual(black5, t3.Peek());

            // Undo Move 1
            game.Undo();
            Assert.AreEqual(1, t2.Count);
            Assert.AreEqual(red6, t1.Peek());
        }

        [Test]
        public void RedoCleared_AfterNewMove_PreventsRedoOfOldMoves_Succeeds()
        {
            var game = new Game();

            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Clubs, Rank.Five, true);

            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];
            var t3 = game.Tableaus[2];

            t1.SetCards(new List<Card> { red6 });
            t2.SetCards(new List<Card> { black7 });
            t3.SetCards(new List<Card> { black5 });

            for (int i = 3; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            // Move red 6 onto black 7
            game.TryMoveCard(red6, t2);
            Assert.AreEqual(2, t2.Count);

            // Undo
            game.Undo();
            Assert.AreEqual(1, t2.Count);

            // Execute a new move (this should clear redo history)
            game.TryMoveCard(black5, t1);
            Assert.AreEqual(2, t1.Count);

            // Try to redo the old move - should not work because redo history was cleared
            game.Redo();
            Assert.AreEqual(1, t2.Count); // Should remain unchanged since there's nothing to redo
        }

        [Test]
        public void UndoAndRedo_WithCardFlipState_RestoresFlipState_Succeeds()
        {
            var game = new Game();

            // Setup: tableau with face-down card on bottom, face-up card on top
            var faceDownCard = new Card(Suit.Diamonds, Rank.Three, false);
            var faceUpCard = new Card(Suit.Hearts, Rank.Two, true);
            var destinationCard = new Card(Suit.Spades, Rank.Three, true);

            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];

            t1.SetCards(new List<Card> { faceUpCard, faceDownCard });
            t2.SetCards(new List<Card> { destinationCard });

            for (int i = 2; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            // Verify initial state
            Assert.False(faceDownCard.IsFaceUp);
            Assert.True(faceUpCard.IsFaceUp);

            // Move the face-up card
            game.TryMoveCard(faceUpCard, t2);
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual(2, t2.Count);

            // Undo - face-down card should still be face-down
            game.Undo();
            Assert.False(faceDownCard.IsFaceUp);
            Assert.AreEqual(2, t1.Count);

            // Redo - face-down card should still be face-down
            game.Redo();
            Assert.True(faceDownCard.IsFaceUp);
            Assert.AreEqual(1, t1.Count);
        }

        [Test]
        public void UndoRedo_MultipleCardsStack_PreservesStackOrder_Succeeds()
        {
            var game = new Game();

            // Source tableau: Red 6, Black 5, Red 4 (bottom to top)
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);
            var red4 = new Card(Suit.Diamonds, Rank.Four, true);

            // Destination: Black 7
            var black7 = new Card(Suit.Clubs, Rank.Seven, true);

            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];

            t1.SetCards(new List<Card> { red4, black5, red6 });
            t2.SetCards(new List<Card> { black7 });

            for (int i = 2; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            // Move stack: red6, black5, red4 onto black7
            game.TryMoveCard(red6, t2);
            Assert.AreEqual(0, t1.Count);
            Assert.AreEqual(4, t2.Count);

            var movedStack = t2.GetCards().GetRange(0, 3);
            Assert.AreEqual(red4, movedStack[0]);
            Assert.AreEqual(black5, movedStack[1]);
            Assert.AreEqual(red6, movedStack[2]);

            // Undo
            game.Undo();
            Assert.AreEqual(3, t1.Count);
            Assert.AreEqual(1, t2.Count);

            var restoredStack = t1.GetCards();
            Assert.AreEqual(red4, restoredStack[0]);
            Assert.AreEqual(black5, restoredStack[1]);
            Assert.AreEqual(red6, restoredStack[2]);

            // Redo
            game.Redo();
            Assert.AreEqual(0, t1.Count);
            Assert.AreEqual(4, t2.Count);
        }

        [Test]
        public void UndoRedo_DrawFromStock_CyclesWaste_Succeeds()
        {
            var game = new Game();

            // Setup stock with 3 face-down cards
            var card1 = new Card(Suit.Hearts, Rank.Ace, false);
            var card2 = new Card(Suit.Diamonds, Rank.King, false);
            var card3 = new Card(Suit.Clubs, Rank.Queen, false);

            game.Stock.SetCards(new List<Card> { card3, card2, card1 });
            game.Waste.SetCards(new List<Card>());

            // Draw card 3
            game.DrawFromStock();
            Assert.AreEqual(2, game.Stock.Count);
            Assert.AreEqual(1, game.Waste.Count);
            Assert.True(card3.IsFaceUp);

            // Draw card 2
            game.DrawFromStock();
            Assert.AreEqual(1, game.Stock.Count);
            Assert.AreEqual(2, game.Waste.Count);

            // Undo draw of card 2
            game.Undo();
            Assert.AreEqual(2, game.Stock.Count);
            Assert.AreEqual(1, game.Waste.Count);

            // Redo
            game.Redo();
            Assert.AreEqual(1, game.Stock.Count);
            Assert.AreEqual(2, game.Waste.Count);
        }
    }
}
