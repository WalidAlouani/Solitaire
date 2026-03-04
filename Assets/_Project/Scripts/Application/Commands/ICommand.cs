namespace Solitaire.Application.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}