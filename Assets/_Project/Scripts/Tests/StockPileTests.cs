using NUnit.Framework;
using Solitaire.Domain;
using Solitaire.Domain.Piles;

namespace Solitaire.Tests
{
    public class StockPileTests
    {
        private StockPile _stock;

        [SetUp]
        public void SetUp()
        {
            _stock = new StockPile();
        }

        [Test]
        public void CanAddCard_AlwaysFalse()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            var origin = new TableauPile();
            origin.Push(card);

            Assert.IsFalse(_stock.CanAddCard(origin, card));
        }

        [Test]
        public void CanRemoveCard_TopCard_Allowed()
        {
            var card = new Card(Suit.Hearts, Rank.Ace);
            _stock.Push(card);

            Assert.IsTrue(_stock.CanRemoveCard(card));
        }

        [Test]
        public void CanRemoveCard_Empty_Rejected()
        {
            var card = new Card(Suit.Hearts, Rank.Ace);
            Assert.IsFalse(_stock.CanRemoveCard(card));
        }

        [Test]
        public void CanRemoveCard_NotTopCard_Rejected()
        {
            var bottom = new Card(Suit.Hearts, Rank.Ace);
            var top = new Card(Suit.Hearts, Rank.Two);
            _stock.Push(bottom);
            _stock.Push(top);

            Assert.IsFalse(_stock.CanRemoveCard(bottom));
        }

        [Test]
        public void OnCardAdded_SetsFaceDown()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true); // starts face up
            _stock.Push(card);

            Assert.IsFalse(card.IsFaceUp, "Stock pile should force cards face down.");
        }

        [Test]
        public void OnCardAdded_FiresEvent()
        {
            Card eventCard = null;
            _stock.OnCardAddedEvent += c => eventCard = c;

            var card = new Card(Suit.Hearts, Rank.Ace);
            _stock.Push(card);

            Assert.AreSame(card, eventCard);
        }

        [Test]
        public void OnCardRemoved_FiresEvent()
        {
            Card eventCard = null;
            _stock.OnCardRemovedEvent += c => eventCard = c;

            var card = new Card(Suit.Hearts, Rank.Ace);
            _stock.Push(card);
            _stock.Pop();

            Assert.AreSame(card, eventCard);
        }
    }
}