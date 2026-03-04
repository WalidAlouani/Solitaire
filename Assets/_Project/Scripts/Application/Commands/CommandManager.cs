using System.Collections.Generic;

namespace Solitaire.Application.Commands
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _undo = new Stack<ICommand>();
        private readonly Stack<ICommand> _redo = new Stack<ICommand>();

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void ExecuteCmd(ICommand cmd)
        {
            cmd.Execute();
            _undo.Push(cmd);
            _redo.Clear();
        }

        public void Undo()
        {
            if (_undo.Count == 0) return;
            var cmd = _undo.Pop();
            cmd.Undo();
            _redo.Push(cmd);
        }

        public void Redo()
        {
            if (_redo.Count == 0) return;
            var cmd = _redo.Pop();
            cmd.Execute();
            _undo.Push(cmd);
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
        }
    }
}