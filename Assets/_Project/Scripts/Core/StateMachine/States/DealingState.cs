namespace Solitaire.Core.StateMachine
{
    /// <summary>
    /// Entry state for each round. Creates a new game model,
    /// tells the UI to deal cards, and waits for the dealing
    /// animation to finish before transitioning to PlayingState.
    /// </summary>
    public class DealingState : IGameState
    {
        private readonly IGameContext _context;

        public DealingState(IGameContext context)
        {
            _context = context;
        }

        public void Enter()
        {
            _context.GameUI.Cleanup();

            var game = _context.CreateNewGame();

            _context.GameUI.OnDealingComplete += HandleDealingComplete;
            _context.GameUI.StartGame(game);
        }

        public void Update() { }

        public void Exit()
        {
            _context.GameUI.OnDealingComplete -= HandleDealingComplete;
        }

        private void HandleDealingComplete()
        {
            _context.StateManager.ChangeState<PlayingState>();
        }
    }
}
