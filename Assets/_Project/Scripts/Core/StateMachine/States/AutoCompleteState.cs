using UnityEngine;

namespace Solitaire.Core.StateMachine
{
    /// <summary>
    /// Runs the auto-complete sequence: repeatedly moves the lowest-ranked
    /// eligible card from tableaus to foundations until the game is won.
    ///
    /// Uses a timer in Update() to space out steps — no coroutine needed.
    /// Transitions to <see cref="WinState"/> when no more moves are available.
    /// </summary>
    public class AutoCompleteState : IGameState
    {
        private readonly IGameContext _context;
        private float _stepTimer;

        public AutoCompleteState(IGameContext context)
        {
            _context = context;
        }

        public void Enter()
        {
            _context.GameUI.SetInteractable(false);
            _context.GameUI.HideAutoCompleteButton();

            // Fire the first step immediately
            _stepTimer = 0f;
        }

        public void Update()
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer > 0f)
                return;

            if (_context.Game.AutoCompleteStep())
            {
                _stepTimer = _context.GameUI.GameSettings.AutoCompleteStepDelay;
            }
            else
            {
                // No more moves — should be a win
                if (_context.Game.CheckWin())
                    _context.StateManager.ChangeState<WinState>();
                else
                    _context.StateManager.ChangeState<PlayingState>();
            }
        }

        public void Exit() { }
    }
}
