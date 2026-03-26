using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using Solitaire.Presentation;
using UnityEngine;

namespace Solitaire.Audio
{
    /// <summary>
    /// Bridges game model events and presenter events to audio playback.
    /// Lives in the Game-Canvas scene alongside the GameManager.
    ///
    /// Subscribes to:
    ///   - <see cref="Game"/> events: card moved, card flipped, game won
    ///   - <see cref="GamePresenterBase"/> events: deal card animated, invalid move, auto-complete step
    ///
    /// All sounds go through <see cref="AudioServiceSO"/> — no direct coupling to the audio player.
    /// </summary>
    public class GameAudioHandler : MonoBehaviour
    {
        [Header("Service")]
        [SerializeField] private AudioServiceSO _audioService;

        [Header("Sound Library")]
        [SerializeField] private SoundLibrarySO _sounds;

        [Header("Music")]
        [SerializeField] private bool _playMusicOnStart = true;

        private Game _game;
        private GamePresenterBase _presenter;

        // --- Public API ---

        /// <summary>
        /// Subscribe to a game instance's events. Call after each new game is created.
        /// </summary>
        public void BindGame(Game game)
        {
            UnbindGame();
            _game = game;

            _game.OnCardMoved += HandleCardMoved;
            _game.OnCardFlipped += HandleCardFlipped;
            _game.OnGameWon += HandleGameWon;
        }

        /// <summary>
        /// Subscribe to presenter events. Call once during scene setup.
        /// </summary>
        public void BindPresenter(GamePresenterBase presenter)
        {
            UnbindPresenter();
            _presenter = presenter;

            _presenter.OnDealCardAnimated += HandleDealCardAnimated;
            _presenter.OnInvalidMoveAttempted += HandleInvalidMoveAttempted;
            _presenter.OnAutoCompleteStepPerformed += HandleAutoCompleteStep;
        }

        public void UnbindGame()
        {
            if (_game == null) return;

            _game.OnCardMoved -= HandleCardMoved;
            _game.OnCardFlipped -= HandleCardFlipped;
            _game.OnGameWon -= HandleGameWon;
            _game = null;
        }

        public void UnbindPresenter()
        {
            if (_presenter == null) return;

            _presenter.OnDealCardAnimated -= HandleDealCardAnimated;
            _presenter.OnInvalidMoveAttempted -= HandleInvalidMoveAttempted;
            _presenter.OnAutoCompleteStepPerformed -= HandleAutoCompleteStep;
            _presenter = null;
        }

        // --- Music ---

        public void StartGameMusic()
        {
            if (_sounds.GameMusic != null)
                _audioService.PlayMusic(_sounds.GameMusic);
        }

        public void StartMenuMusic()
        {
            if (_sounds.MenuMusic != null)
                _audioService.PlayMusic(_sounds.MenuMusic);
        }

        // --- Lifecycle ---

        private void Start()
        {
            if (_playMusicOnStart && _sounds.GameMusic != null)
                _audioService.PlayMusic(_sounds.GameMusic);
        }

        private void OnDestroy()
        {
            UnbindGame();
            UnbindPresenter();
        }

        // --- Game Event Handlers ---

        private void HandleCardMoved(Card card, CardPile pile)
        {
            if (pile is FoundationPile)
                _audioService.PlaySFX(_sounds.CardPlace);
            else if (pile is WastePile)
                _audioService.PlaySFX(_sounds.CardDraw);
            else
                _audioService.PlaySFX(_sounds.CardPlace);
        }

        private void HandleCardFlipped(Card card)
        {
            _audioService.PlaySFX(_sounds.CardFlip);
        }

        private void HandleGameWon()
        {
            _audioService.PlaySFX(_sounds.WinFanfare);
        }

        // --- Presenter Event Handlers ---

        private void HandleDealCardAnimated()
        {
            _audioService.PlaySFX(_sounds.CardDeal);
        }

        private void HandleInvalidMoveAttempted()
        {
            _audioService.PlaySFX(_sounds.CardInvalidMove);
        }

        private void HandleAutoCompleteStep()
        {
            _audioService.PlaySFX(_sounds.AutoCompleteStep);
        }
    }
}
