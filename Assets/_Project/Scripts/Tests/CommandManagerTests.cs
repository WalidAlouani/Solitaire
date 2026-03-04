using NUnit.Framework;
using Solitaire.Application.Commands;
using Solitaire.Domain;
using Solitaire.Domain.Piles;

namespace Solitaire.Tests
{
    public class CommandManagerTests
    {
        private CommandManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new CommandManager();
        }

        [Test]
        public void Initial_CanUndo_IsFalse()
        {
            Assert.IsFalse(_manager.CanUndo);
        }

        [Test]
        public void Initial_CanRedo_IsFalse()
        {
            Assert.IsFalse(_manager.CanRedo);
        }

        [Test]
        public void ExecuteCmd_CanUndo_BecomesTrue()
        {
            var from = new TableauPile();
            var to = new TableauPile();
            var card = new Card(Suit.Spades, Rank.King, true);
            from.Push(card);

            _manager.ExecuteCmd(new MoveCommand(from, to));

            Assert.IsTrue(_manager.CanUndo);
        }

        [Test]
        public void ExecuteCmd_ClearsRedoStack()
        {
            var from = new TableauPile();
            var to = new TableauPile();
            var card = new Card(Suit.Spades, Rank.King, true);
            from.Push(card);

            _manager.ExecuteCmd(new MoveCommand(from, to));
            _manager.Undo();
            Assert.IsTrue(_manager.CanRedo);

            // New command should clear redo
            var card2 = new Card(Suit.Hearts, Rank.King, true);
            from.Push(card2);
            _manager.ExecuteCmd(new MoveCommand(from, to));

            Assert.IsFalse(_manager.CanRedo);
        }

        [Test]
        public void Undo_AfterExecute_CanRedo_BecomesTrue()
        {
            var from = new TableauPile();
            var to = new TableauPile();
            var card = new Card(Suit.Spades, Rank.King, true);
            from.Push(card);

            _manager.ExecuteCmd(new MoveCommand(from, to));
            _manager.Undo();

            Assert.IsTrue(_manager.CanRedo);
            Assert.IsFalse(_manager.CanUndo);
        }

        [Test]
        public void Undo_WhenEmpty_DoesNothing()
        {
            Assert.DoesNotThrow(() => _manager.Undo());
        }

        [Test]
        public void Redo_WhenEmpty_DoesNothing()
        {
            Assert.DoesNotThrow(() => _manager.Redo());
        }

        [Test]
        public void Clear_ResetsUndoAndRedo()
        {
            var from = new TableauPile();
            var to = new TableauPile();
            var card = new Card(Suit.Spades, Rank.King, true);
            from.Push(card);

            _manager.ExecuteCmd(new MoveCommand(from, to));
            _manager.Undo();

            Assert.IsTrue(_manager.CanRedo);

            _manager.Clear();

            Assert.IsFalse(_manager.CanUndo);
            Assert.IsFalse(_manager.CanRedo);
        }

        [Test]
        public void Redo_ReExecutesCommand()
        {
            var from = new TableauPile();
            var to = new TableauPile();
            var card = new Card(Suit.Spades, Rank.King, true);
            from.Push(card);

            _manager.ExecuteCmd(new MoveCommand(from, to));
            Assert.AreEqual(1, to.Count);

            _manager.Undo();
            Assert.AreEqual(0, to.Count);

            _manager.Redo();
            Assert.AreEqual(1, to.Count);
            Assert.IsTrue(_manager.CanUndo);
        }
    }
}