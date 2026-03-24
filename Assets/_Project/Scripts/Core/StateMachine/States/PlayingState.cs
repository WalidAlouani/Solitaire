namespace Solitaire.Core.StateMachine
{
    /// <summary>
    /// Active gameplay state. Enables card interaction, listens for
    /// game-won and auto-complete events, and drives transitions
    /// to WinState when the player completes all foundations.
    /// </summary>
    public class PlayingState : IGameState
    {
        private readonly GameManager _gameManager;

        public PlayingState(GameManager manager)
        {
            _gameManager = manager;
        }

        public void Enter()
        {
            _gameManager.GameUI.SetInteractable(true);

            _gameManager.Game.OnGameWon += HandleGameWon;
            _gameManager.Game.OnAutoCompleteChanged += HandleAutoCompleteChanged;
            _gameManager.GameUI.OnAutoCompleteRequested += HandleAutoCompleteRequested;

            // Auto-complete might already be available on enter (edge case)
            if (_gameManager.Game.CanAutoComplete())
                _gameManager.GameUI.ShowAutoCompleteButton();
        }

        public void Update() { }

        public void Exit()
        {
            _gameManager.Game.OnGameWon -= HandleGameWon;
            _gameManager.Game.OnAutoCompleteChanged -= HandleAutoCompleteChanged;
            _gameManager.GameUI.OnAutoCompleteRequested -= HandleAutoCompleteRequested;

            _gameManager.GameUI.HideAutoCompleteButton();
            _gameManager.GameUI.SetInteractable(false);
        }

        private void HandleGameWon()
        {
            _gameManager.StateManager.ChangeState<WinState>();
        }

        private void HandleAutoCompleteChanged(bool available)
        {
            if (available)
                _gameManager.GameUI.ShowAutoCompleteButton();
            else
                _gameManager.GameUI.HideAutoCompleteButton();
        }

        private void HandleAutoCompleteRequested()
        {
            _gameManager.GameUI.HideAutoCompleteButton();
            _gameManager.GameUI.RunAutoComplete();
        }
    }
}
