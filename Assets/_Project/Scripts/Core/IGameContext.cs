using Solitaire.Application;
using Solitaire.Core.StateMachine;
using Solitaire.Presentation;

namespace Solitaire.Core
{
    /// <summary>
    /// Abstraction over GameManager that game states depend on.
    /// Keeps states decoupled from MonoBehaviour so they can be
    /// tested with pure C# mocks — no GameObjects required.
    /// </summary>
    public interface IGameContext
    {
        GameStateManager StateManager { get; }
        IGameUI GameUI { get; }
        Game Game { get; }
        string MainMenuSceneName { get; }

        Game CreateNewGame();
    }
}
