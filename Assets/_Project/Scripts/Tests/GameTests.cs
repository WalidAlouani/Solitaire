using NUnit.Framework;
using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;

namespace Solitaire.Tests
{
    public class GameTests
    {
        // ===================================================================
        // Setup & Initial State
        // ===================================================================

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
        public void Constructor_Creates7Tableaus()
        {
            var game = new Game();
            Assert.AreEqual(7, game.Tableaus.Count);
        }

        [Test]
        public void Constructor_Creates4Foundations()
        {
            var game = new Game();
            Assert.AreEqual(4, game.Foundations.Count);
        }

        [Test]
        public void RecycleAndShuffleStock_ResetsAllPiles()
        {
            var game = new Game();
            game.RecycleAndShuffleStock();
            game.PopulateTableauPiles();

            // Move some cards around first
            game.DrawFromStock();
            Assert.IsTrue(game.Waste.Count > 0);

            // Reset
            game.RecycleAndShuffleStock();

            Assert.AreEqual(52, game.Stock.Count);
            Assert.AreEqual(0, game.Waste.Count);
            foreach (var t in game.Tableaus) Assert.AreEqual(0, t.Count);
            foreach (var f in game.Foundations) Assert.AreEqual(0, f.Count);
        }

        [Test]
        public void RecycleAndShuffleStock_AllCardsFaceDown()
        {
            var game = new Game();
            game.RecycleAndShuffleStock();

            var cards = game.Stock.GetCards();
            foreach (var card in cards)
            {
                Assert.IsFalse(card.IsFaceUp, $"Card {card.Suit} {card.Rank} should be face down after recycle.");
            }
        }

        // ===================================================================
        // Stock / Waste Cycling
        // ===================================================================

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
        public void DrawFromStock_WhenStockHasCards_MovesTopToWaste()
        {
            var game = new Game();
            game.RecycleAndShuffleStock();
            game.PopulateTableauPiles();

            int stockBefore = game.Stock.Count;
            game.DrawFromStock();

            Assert.AreEqual(stockBefore - 1, game.Stock.Count);
            Assert.AreEqual(1, game.Waste.Count);
            Assert.IsTrue(game.Waste.Peek().IsFaceUp);
        }

        [Test]
        public void DrawFromStock_WhenBothEmpty_DoesNothing()
        {
            var game = new Game();
            // Stock and Waste are both empty by default
            Assert.AreEqual(0, game.Stock.Count);
            Assert.AreEqual(0, game.Waste.Count);

            game.DrawFromStock();

            Assert.AreEqual(0, game.Stock.Count);
            Assert.AreEqual(0, game.Waste.Count);
        }

        [Test]
        public void DrawFromStock_WhenStockEmptyWasteHasCards_RecyclesWasteToStock()
        {
            var game = new Game();

            var card1 = new Card(Suit.Hearts, Rank.Ace, true);
            var card2 = new Card(Suit.Hearts, Rank.Two, true);
            game.Waste.Push(card1);
            game.Waste.Push(card2);

            Assert.AreEqual(0, game.Stock.Count);
            Assert.AreEqual(2, game.Waste.Count);

            game.DrawFromStock();

            Assert.AreEqual(2, game.Stock.Count);
            Assert.AreEqual(0, game.Waste.Count);
        }

        // ===================================================================
        // Card Movement
        // ===================================================================

        [Test]
        public void MoveCard_FromWasteToTableau_ValidMove_Succeeds()
        {
            var game = new Game();

            var wasteCard = new Card(Suit.Hearts, Rank.Six, true);
            game.Waste.SetCards(new List<Card> { wasteCard });

            var tableauCard = new Card(Suit.Spades, Rank.Seven, true);
            var tableau = game.Tableaus[0];
            tableau.SetCards(new List<Card> { tableauCard });

            for (int i = 1; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            bool moved = game.TryMoveCard(wasteCard, tableau);
            Assert.IsTrue(moved, "Card should be moved from waste to tableau.");
            Assert.AreEqual(wasteCard, tableau.Peek());
        }

        [Test]
        public void MoveCard_FromTableauToFoundation_ValidMove_Succeeds()
        {
            var game = new Game();

            var aceCard = new Card(Suit.Clubs, Rank.Ace, true);
            var tableau = game.Tableaus[0];
            tableau.SetCards(new List<Card> { aceCard });

            var foundation = game.Foundations.Find(f => f.Suit == Suit.Clubs);
            if (foundation == null) foundation = game.Foundations[0];
            foundation.SetCards(new List<Card>());

            for (int i = 1; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());
            foreach (var f in game.Foundations)
                if (f != foundation) f.SetCards(new List<Card>());

            bool moved = game.TryMoveCard(aceCard, foundation);
            Assert.IsTrue(moved, "Ace should be moved from tableau to foundation.");
            Assert.AreEqual(aceCard, foundation.Peek());
        }

        [Test]
        public void MoveCard_FromTableauToTableau_InvalidMove_Fails()
        {
            var game = new Game();

            var card1 = new Card(Suit.Hearts, Rank.Five, true);
            var t1 = game.Tableaus[0];
            t1.SetCards(new List<Card> { card1 });

            var card2 = new Card(Suit.Diamonds, Rank.Six, true);
            var t2 = game.Tableaus[1];
            t2.SetCards(new List<Card> { card2 });

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

            var foundation = game.Foundations[0];

            var wasteCard = game.Waste.Peek();
            bool moved = game.TryMoveCard(wasteCard, foundation);
            Assert.IsTrue(moved, "Ace should be moved from waste to foundation.");
            Assert.AreEqual(wasteCard, foundation.Peek());

            wasteCard = game.Waste.Peek();
            moved = game.TryMoveCard(wasteCard, foundation);
            Assert.IsTrue(moved, "Two should be moved from waste to foundation.");
            Assert.AreEqual(wasteCard, foundation.Peek());
        }

        [Test]
        public void MoveCard_InvalidDestination_ReturnsFalse()
        {
            var game = new Game();

            // Try to move a non-King to an empty tableau
            var card = new Card(Suit.Hearts, Rank.Five, true);
            game.Waste.SetCards(new List<Card> { card });

            var emptyTableau = game.Tableaus[0];
            emptyTableau.SetCards(new List<Card>());

            bool moved = game.TryMoveCard(card, emptyTableau);
            Assert.IsFalse(moved);
        }

        [Test]
        public void MoveCard_FromFoundationToTableau_ValidMove_Succeeds()
        {
            var game = new Game();

            // Put Ace then Two of Hearts on a foundation
            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            var two = new Card(Suit.Hearts, Rank.Two, true);
            var foundation = game.Foundations[0];
            foundation.Push(ace);
            foundation.Push(two);

            // Put a black Three on tableau
            var black3 = new Card(Suit.Spades, Rank.Three, true);
            var tableau = game.Tableaus[0];
            tableau.SetCards(new List<Card> { black3 });

            // Move Two of Hearts from foundation back to tableau
            bool moved = game.TryMoveCard(two, tableau);
            Assert.IsTrue(moved, "Should be able to move top foundation card to valid tableau.");
            Assert.AreEqual(two, tableau.Peek());
            Assert.AreEqual(ace, foundation.Peek());
        }

        // ===================================================================
        // Multi-Card Stack Moves
        // ===================================================================

        [Test]
        public void MoveMultipleCards_FromTableauToTableau_ValidMove_Succeeds()
        {
            var game = new Game();

            var card1 = new Card(Suit.Diamonds, Rank.Ace, false);
            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);
            var red4 = new Card(Suit.Diamonds, Rank.Four, true);
            var sourceTableau = game.Tableaus[0];

            sourceTableau.Push(card1);
            sourceTableau.Push(red6);
            sourceTableau.Push(black5);
            sourceTableau.Push(red4);

            var card2 = new Card(Suit.Spades, Rank.Ace, false);
            var black7 = new Card(Suit.Spades, Rank.Seven, true);
            var destTableau = game.Tableaus[1];
            destTableau.Push(card2);
            destTableau.Push(black7);

            bool moved = game.TryMoveCard(red6, destTableau);
            Assert.IsTrue(moved);

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
        public void TryMoveStack_InvalidDestination_ReturnsZero()
        {
            var game = new Game();

            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var t1 = game.Tableaus[0];
            t1.Push(red6);

            // Same color card on destination - invalid
            var red7 = new Card(Suit.Diamonds, Rank.Seven, true);
            var t2 = game.Tableaus[1];
            t2.Push(red7);

            int stackSize = game.TryMoveStack(red6, t1, t2);
            Assert.AreEqual(0, stackSize);
        }

        [Test]
        public void TryMoveStack_CardNotInPile_ReturnsZero()
        {
            var game = new Game();

            var card = new Card(Suit.Hearts, Rank.Six, true);
            var t1 = game.Tableaus[0]; // empty
            var t2 = game.Tableaus[1];

            int stackSize = game.TryMoveStack(card, t1, t2);
            Assert.AreEqual(0, stackSize);
        }

        // ===================================================================
        // FindPileForCard
        // ===================================================================

        [Test]
        public void FindPileForCard_CardInStock_ReturnsStock()
        {
            var game = new Game();
            game.RecycleAndShuffleStock();

            var stockCard = game.Stock.Peek();
            var pile = game.FindPileForCard(stockCard);

            Assert.AreSame(game.Stock, pile);
        }

        [Test]
        public void FindPileForCard_CardInWaste_ReturnsWaste()
        {
            var game = new Game();
            game.RecycleAndShuffleStock();
            game.PopulateTableauPiles();
            game.DrawFromStock(); // moves top stock card to waste

            var wasteCard = game.Waste.Peek();
            var pile = game.FindPileForCard(wasteCard);

            Assert.AreSame(game.Waste, pile);
        }

        [Test]
        public void FindPileForCard_CardInTableau_ReturnsTableau()
        {
            var game = new Game();
            game.RecycleAndShuffleStock();
            game.PopulateTableauPiles();

            var tableauCard = game.Tableaus[3].Peek();
            var pile = game.FindPileForCard(tableauCard);

            Assert.AreSame(game.Tableaus[3], pile);
        }

        [Test]
        public void FindPileForCard_UnknownCard_ReturnsNull()
        {
            var game = new Game();
            var unknownCard = new Card(Suit.Hearts, Rank.Ace, true);

            var pile = game.FindPileForCard(unknownCard);
            Assert.IsNull(pile);
        }

        // ===================================================================
        // CheckWin
        // ===================================================================

        [Test]
        public void CheckWin_EmptyFoundations_ReturnsFalse()
        {
            var game = new Game();
            Assert.IsFalse(game.CheckWin());
        }

        [Test]
        public void CheckWin_PartialFoundations_ReturnsFalse()
        {
            var game = new Game();
            game.Foundations[0].Push(new Card(Suit.Spades, Rank.Ace, true));
            Assert.IsFalse(game.CheckWin());
        }

        [Test]
        public void CheckWin_AllFoundationsFull_ReturnsTrue()
        {
            var game = new Game();
            FillAllFoundations(game);
            Assert.IsTrue(game.CheckWin());
        }

        [Test]
        public void OnGameWon_Fires_WhenLastCardMovedToFoundation()
        {
            var game = new Game();
            bool wonFired = false;
            game.OnGameWon += () => wonFired = true;

            // Fill 3 foundations completely and the 4th up to Queen
            var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            for (int s = 0; s < 3; s++)
            {
                for (int r = (int)Rank.Ace; r <= (int)Rank.King; r++)
                    game.Foundations[s].Push(new Card(suits[s], (Rank)r, true));
            }

            for (int r = (int)Rank.Ace; r <= (int)Rank.Queen; r++)
                game.Foundations[3].Push(new Card(Suit.Clubs, (Rank)r, true));

            // Place the last King on a tableau
            var lastKing = new Card(Suit.Clubs, Rank.King, true);
            game.Tableaus[0].SetCards(new List<Card> { lastKing });

            // Clear other tableaus to avoid interference
            for (int i = 1; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            Assert.IsFalse(wonFired);

            // Move the last king to its foundation
            bool moved = game.TryMoveCard(lastKing, game.Foundations[3]);
            Assert.IsTrue(moved);
            Assert.IsTrue(wonFired, "OnGameWon should fire when all foundations are complete.");
        }

        [Test]
        public void OnCardMoved_Fires_WhenCardMoved()
        {
            var game = new Game();
            Card movedCard = null;
            CardPile movedToPile = null;
            game.OnCardMoved += (card, pile) => { movedCard = card; movedToPile = pile; };

            var ace = new Card(Suit.Hearts, Rank.Ace, true);
            game.Tableaus[0].SetCards(new List<Card> { ace });

            game.TryMoveCard(ace, game.Foundations[0]);

            Assert.IsNotNull(movedCard);
            Assert.AreEqual(ace, movedCard);
        }

        [Test]
        public void OnCardFlipped_Fires_WhenCardFlips()
        {
            var game = new Game();
            game.RecycleAndShuffleStock();
            game.PopulateTableauPiles();

            // Put a face-down card under a face-up card in a tableau
            Card flippedCard = null;
            game.OnCardFlipped += card => flippedCard = card;

            // Find a tableau with at least 2 cards (the face-down one will auto-flip on removal)
            var tableau = game.Tableaus[1]; // has 2 cards: 1 face-down + 1 face-up
            var topCard = tableau.Peek();

            // Move top card to an empty foundation if it's an ace, or to another tableau
            // Simplify: just pop and check
            var faceDownCard = tableau.GetCardsReverse()[0]; // bottom card is face-down
            Assert.IsFalse(faceDownCard.IsFaceUp);

            // Remove top card to trigger auto-flip
            tableau.Pop();

            Assert.IsNotNull(flippedCard, "OnCardFlipped should fire when a card is flipped.");
            Assert.AreSame(faceDownCard, flippedCard);
        }

        // ===================================================================
        // Undo / Redo
        // ===================================================================

        [Test]
        public void Undo_SingleMoveFromTableauToTableau_ReversesMove_Succeeds()
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

            bool moved = game.TryMoveCard(red6, t2);
            Assert.IsTrue(moved);
            Assert.AreEqual(0, t1.Count);
            Assert.AreEqual(2, t2.Count);
            Assert.AreEqual(red6, t2.Peek());

            game.Undo();

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

            bool moved = game.TryMoveCard(aceSpades, foundation);
            Assert.IsTrue(moved);
            Assert.AreEqual(0, game.Waste.Count);
            Assert.AreEqual(1, foundation.Count);

            game.Undo();

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

            bool moved = game.TryMoveCard(red6, t2);
            Assert.IsTrue(moved);
            Assert.AreEqual(red6, t2.Peek());

            game.Undo();
            Assert.AreEqual(red6, t1.Peek());

            game.Redo();
            Assert.AreEqual(red6, t2.Peek());
        }

        [Test]
        public void UndoMultipleMoves_AllMovesReversedInOrder_Succeeds()
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

            game.TryMoveCard(red6, t2);
            Assert.AreEqual(2, t2.Count);

            game.TryMoveCard(black5, t2);
            Assert.AreEqual(3, t2.Count);

            game.Undo();
            Assert.AreEqual(2, t2.Count);
            Assert.AreEqual(black5, t3.Peek());

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

            game.TryMoveCard(red6, t2);
            Assert.AreEqual(2, t2.Count);

            game.Undo();
            Assert.AreEqual(1, t2.Count);

            game.TryMoveCard(black5, t1);
            Assert.AreEqual(2, t1.Count);

            game.Redo();
            Assert.AreEqual(1, t2.Count);
        }

        [Test]
        public void UndoAndRedo_WithCardFlipState_RestoresFlipState_Succeeds()
        {
            var game = new Game();

            var faceDownCard = new Card(Suit.Diamonds, Rank.Three, false);
            var faceUpCard = new Card(Suit.Hearts, Rank.Two, true);
            var destinationCard = new Card(Suit.Spades, Rank.Three, true);

            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];

            t1.SetCards(new List<Card> { faceUpCard, faceDownCard });
            t2.SetCards(new List<Card> { destinationCard });

            for (int i = 2; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            Assert.False(faceDownCard.IsFaceUp);
            Assert.True(faceUpCard.IsFaceUp);

            game.TryMoveCard(faceUpCard, t2);
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual(2, t2.Count);

            game.Undo();
            Assert.False(faceDownCard.IsFaceUp);
            Assert.AreEqual(2, t1.Count);

            game.Redo();
            Assert.True(faceDownCard.IsFaceUp);
            Assert.AreEqual(1, t1.Count);
        }

        [Test]
        public void UndoRedo_MultipleCardsStack_PreservesStackOrder_Succeeds()
        {
            var game = new Game();

            var red6 = new Card(Suit.Hearts, Rank.Six, true);
            var black5 = new Card(Suit.Spades, Rank.Five, true);
            var red4 = new Card(Suit.Diamonds, Rank.Four, true);

            var black7 = new Card(Suit.Clubs, Rank.Seven, true);

            var t1 = game.Tableaus[0];
            var t2 = game.Tableaus[1];

            t1.SetCards(new List<Card> { red4, black5, red6 });
            t2.SetCards(new List<Card> { black7 });

            for (int i = 2; i < game.Tableaus.Count; i++)
                game.Tableaus[i].SetCards(new List<Card>());

            game.TryMoveCard(red6, t2);
            Assert.AreEqual(0, t1.Count);
            Assert.AreEqual(4, t2.Count);

            var movedStack = t2.GetCards().GetRange(0, 3);
            Assert.AreEqual(red4, movedStack[0]);
            Assert.AreEqual(black5, movedStack[1]);
            Assert.AreEqual(red6, movedStack[2]);

            game.Undo();
            Assert.AreEqual(3, t1.Count);
            Assert.AreEqual(1, t2.Count);

            var restoredStack = t1.GetCards();
            Assert.AreEqual(red4, restoredStack[0]);
            Assert.AreEqual(black5, restoredStack[1]);
            Assert.AreEqual(red6, restoredStack[2]);

            game.Redo();
            Assert.AreEqual(0, t1.Count);
            Assert.AreEqual(4, t2.Count);
        }

        [Test]
        public void UndoRedo_DrawFromStock_CyclesWaste_Succeeds()
        {
            var game = new Game();

            var card1 = new Card(Suit.Hearts, Rank.Ace, false);
            var card2 = new Card(Suit.Diamonds, Rank.King, false);
            var card3 = new Card(Suit.Clubs, Rank.Queen, false);

            game.Stock.SetCards(new List<Card> { card3, card2, card1 });
            game.Waste.SetCards(new List<Card>());

            game.DrawFromStock();
            Assert.AreEqual(2, game.Stock.Count);
            Assert.AreEqual(1, game.Waste.Count);
            Assert.True(card3.IsFaceUp);

            game.DrawFromStock();
            Assert.AreEqual(1, game.Stock.Count);
            Assert.AreEqual(2, game.Waste.Count);

            game.Undo();
            Assert.AreEqual(2, game.Stock.Count);
            Assert.AreEqual(1, game.Waste.Count);

            game.Redo();
            Assert.AreEqual(1, game.Stock.Count);
            Assert.AreEqual(2, game.Waste.Count);
        }

        [Test]
        public void Undo_WhenNoMoves_DoesNotThrow()
        {
            var game = new Game();
            Assert.DoesNotThrow(() => game.Undo());
        }

        [Test]
        public void Redo_WhenNoMoves_DoesNotThrow()
        {
            var game = new Game();
            Assert.DoesNotThrow(() => game.Redo());
        }

        // ===================================================================
        // Helpers
        // ===================================================================

        private static void FillAllFoundations(Game game)
        {
            var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            for (int s = 0; s < 4; s++)
            {
                for (int r = (int)Rank.Ace; r <= (int)Rank.King; r++)
                    game.Foundations[s].Push(new Card(suits[s], (Rank)r, true));
            }
        }
    }
}
