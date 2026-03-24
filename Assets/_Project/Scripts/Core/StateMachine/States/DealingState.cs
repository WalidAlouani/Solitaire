namespace Solitaire.Core.StateMachine
{
    /// <summary>
    /// Entry state for each round. Creates a new game model,
    /// tells the UI to deal cards, and waits for the dealing
    /// animation to finish before transitioning to PlayingState.
    /// </summary>
    public class DealingState : IGameState
    {
        private readonly GameManager _gameManager;

        public DealingState(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void Enter()
        {
            _gameManager.GameUI.Cleanup();

            var game = _gameManager.CreateNewGame();

            _gameManager.GameUI.OnDealingComplete += HandleDealingComplete;
            _gameManager.GameUI.StartGame(game);
        }

        public void Update() { }

        public void Exit()
        {
            _gameManager.GameUI.OnDealingComplete -= HandleDealingComplete;
        }

        private void HandleDealingComplete()
        {
            _gameManager.StateManager.ChangeState<PlayingState>();
        }
    }
}
