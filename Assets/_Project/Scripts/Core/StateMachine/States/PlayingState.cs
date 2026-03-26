namespace Solitaire.Core.StateMachine
{
    /// <summary>
    /// Active gameplay state. Enables card interaction, listens for
    /// game-won and auto-complete events, and drives transitions
    /// to WinState or AutoCompleteState.
    /// </summary>
    public class PlayingState : IGameState
    {
        private readonly IGameContext _context;

        public PlayingState(IGameContext context)
        {
            _context = context;
        }

        public void Enter()
        {
            _context.GameUI.SetInteractable(true);

            _context.Game.OnGameWon += HandleGameWon;
            _context.Game.OnAutoCompleteChanged += HandleAutoCompleteChanged;
            _context.GameUI.OnAutoCompleteRequested += HandleAutoCompleteRequested;

            // Auto-complete might already be available on enter (edge case)
            if (_context.Game.CanAutoComplete())
                _context.GameUI.ShowAutoCompleteButton();
        }

        public void Update() { }

        public void Exit()
        {
            _context.Game.OnGameWon -= HandleGameWon;
            _context.Game.OnAutoCompleteChanged -= HandleAutoCompleteChanged;
            _context.GameUI.OnAutoCompleteRequested -= HandleAutoCompleteRequested;

            _context.GameUI.HideAutoCompleteButton();
            _context.GameUI.SetInteractable(false);
        }

        private void HandleGameWon()
        {
            _context.StateManager.ChangeState<WinState>();
        }

        private void HandleAutoCompleteChanged(bool available)
        {
            if (available)
                _context.GameUI.ShowAutoCompleteButton();
            else
                _context.GameUI.HideAutoCompleteButton();
        }

        private void HandleAutoCompleteRequested()
        {
            _context.StateManager.ChangeState<AutoCompleteState>();
        }
    }
}
