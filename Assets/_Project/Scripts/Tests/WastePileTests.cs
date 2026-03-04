using NUnit.Framework;
using Solitaire.Domain;
using Solitaire.Domain.Piles;

namespace Solitaire.Tests
{
    public class WastePileTests
    {
        private WastePile _waste;

        [SetUp]
        public void SetUp()
        {
            _waste = new WastePile();
        }

        [Test]
        public void CanAddCard_FromStockPile_Allowed()
        {
            var stock = new StockPile();
            var card = new Card(Suit.Hearts, Rank.Ace);
            stock.Push(card);

            Assert.IsTrue(_waste.CanAddCard(stock, card));
        }

        [Test]
        public void CanAddCard_FromTableauPile_Rejected()
        {
            var tableau = new TableauPile();
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            tableau.Push(card);

            Assert.IsFalse(_waste.CanAddCard(tableau, card));
        }

        [Test]
        public void CanAddCard_FromFoundationPile_Rejected()
        {
            var foundation = new FoundationPile();
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            foundation.Push(card);

            Assert.IsFalse(_waste.CanAddCard(foundation, card));
        }

        [Test]
        public void CanRemoveCard_TopCard_Allowed()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            _waste.Push(card);

            Assert.IsTrue(_waste.CanRemoveCard(card));
        }

        [Test]
        public void CanRemoveCard_Empty_Rejected()
        {
            var card = new Card(Suit.Hearts, Rank.Ace, true);
            Assert.IsFalse(_waste.CanRemoveCard(card));
        }

        [Test]
        public void CanRemoveCard_NotTopCard_Rejected()
        {
            var bottom = new Card(Suit.Hearts, Rank.Ace, true);
            var top = new Card(Suit.Hearts, Rank.Two, true);
            _waste.Push(bottom);
            _waste.Push(top);

            Assert.IsFalse(_waste.CanRemoveCard(bottom));
        }

        [Test]
        public void OnCardAdded_SetsFaceUp()
        {
            var card = new Card(Suit.Hearts, Rank.Ace); // face down
            _waste.Push(card);

            Assert.IsTrue(card.IsFaceUp, "Waste pile should force cards face up.");
        }

        [Test]
        public void OnCardAdded_FiresEvent()
        {
            Card eventCard = null;
            _waste.OnCardAddedEvent += c => eventCard = c;

            var card = new Card(Suit.Hearts, Rank.Ace);
            _waste.Push(card);

            Assert.AreSame(card, eventCard);
        }

        [Test]
        public void OnCardRemoved_FiresEvent()
        {
            Card eventCard = null;
            _waste.OnCardRemovedEvent += c => eventCard = c;

            var card = new Card(Suit.Hearts, Rank.Ace, true);
            _waste.Push(card);
            _waste.Pop();

            Assert.AreSame(card, eventCard);
        }
    }
}