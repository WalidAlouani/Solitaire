using NUnit.Framework;
using Solitaire.Domain;
using Solitaire.Infrastructure;
using System.Collections.Generic;

namespace Solitaire.Tests
{
    public class DeckFactoryTests
    {
        [Test]
        public void CreateDeck_Returns52Cards()
        {
            var deck = DeckFactory.CreateDeck();
            Assert.AreEqual(52, deck.Count);
        }

        [Test]
        public void CreateDeck_AllCardsUnique()
        {
            var deck = DeckFactory.CreateDeck();
            var seen = new HashSet<(Suit, Rank)>();

            foreach (var card in deck)
            {
                bool added = seen.Add((card.Suit, card.Rank));
                Assert.IsTrue(added, $"Duplicate card found: {card.Suit} {card.Rank}");
            }
        }

        [Test]
        public void CreateDeck_ContainsAllSuits()
        {
            var deck = DeckFactory.CreateDeck();
            var suits = new HashSet<Suit>();

            foreach (var card in deck)
                suits.Add(card.Suit);

            Assert.AreEqual(4, suits.Count);
            Assert.IsTrue(suits.Contains(Suit.Hearts));
            Assert.IsTrue(suits.Contains(Suit.Diamonds));
            Assert.IsTrue(suits.Contains(Suit.Clubs));
            Assert.IsTrue(suits.Contains(Suit.Spades));
        }

        [Test]
        public void CreateDeck_ContainsAllRanks()
        {
            var deck = DeckFactory.CreateDeck();
            var ranks = new HashSet<Rank>();

            foreach (var card in deck)
                ranks.Add(card.Rank);

            Assert.AreEqual(13, ranks.Count);
        }

        [Test]
        public void CreateDeck_AllCardsFaceDown()
        {
            var deck = DeckFactory.CreateDeck();

            foreach (var card in deck)
            {
                Assert.IsFalse(card.IsFaceUp, $"Card {card.Suit} {card.Rank} should be face down.");
            }
        }

        [Test]
        public void CreateDeck_13CardsPerSuit()
        {
            var deck = DeckFactory.CreateDeck();
            var suitCounts = new Dictionary<Suit, int>();

            foreach (var card in deck)
            {
                if (!suitCounts.ContainsKey(card.Suit))
                    suitCounts[card.Suit] = 0;
                suitCounts[card.Suit]++;
            }

            foreach (var kvp in suitCounts)
            {
                Assert.AreEqual(13, kvp.Value, $"Suit {kvp.Key} should have 13 cards.");
            }
        }
    }
}