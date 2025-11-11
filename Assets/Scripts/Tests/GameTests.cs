using NUnit.Framework;
using Solitaire.Domain;
using System.Collections.Generic;

public class GameTests
{
    [Test]
    public void RecycleAndPopulateTableauPiles_ValidatePilesInitialStates_Succeeds()
    {
        var game = new Game();
        var stockPile = game.Stock;
        var wastePile = game.Waste;

        Assert.AreEqual(0, stockPile.Count);
        game.RecycleAndSuffleStock();
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

        game.Deal();

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
}