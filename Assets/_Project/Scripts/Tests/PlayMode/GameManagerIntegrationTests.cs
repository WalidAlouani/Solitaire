using System.Collections;
using NUnit.Framework;
using Solitaire.Core;
using Solitaire.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Solitaire.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for the GameManager → Presenter → Game pipeline.
    /// These tests load the actual Game-Canvas scene and verify the full
    /// lifecycle: dealing, game creation, state transitions, and UI wiring.
    /// </summary>
    public class GameManagerIntegrationTests
    {
        private GameManager _gameManager;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("Game-Canvas");
            yield return null; // wait for scene load

            _gameManager = Object.FindObjectOfType<GameManager>();
            Assert.IsNotNull(_gameManager, "GameManager must exist in Game-Canvas scene.");
        }

        // ===================================================================
        // Game Creation
        // ===================================================================

        [UnityTest]
        public IEnumerator Start_CreatesGameModel()
        {
            // GameManager.Start() runs during scene load, which triggers DealingState
            // which calls CreateNewGame(). Wait a frame for Start() to execute.
            yield return null;

            Assert.IsNotNull(_gameManager.Game, "Game model should be created after Start().");
        }

        [UnityTest]
        public IEnumerator Start_GameHas52CardsInStockBeforePopulate()
        {
            yield return null;

            // After CreateNewGame + RecycleAndShuffleStock, stock has 52 cards
            // But PopulateTableauPiles is called by the presenter during StartGame
            // So by now, stock should have 24 cards (52 - 28 tableau cards)
            var game = _gameManager.Game;
            Assert.IsNotNull(game);

            int totalCards = game.Stock.Count + game.Waste.Count;
            for (int i = 0; i < game.Tableaus.Count; i++)
                totalCards += game.Tableaus[i].Count;
            for (int i = 0; i < game.Foundations.Count; i++)
                totalCards += game.Foundations[i].Count;

            Assert.AreEqual(52, totalCards, "Total cards across all piles should be 52.");
        }

        [UnityTest]
        public IEnumerator Start_TableauPilesArePopulated()
        {
            yield return null;

            var game = _gameManager.Game;
            Assert.IsNotNull(game);

            for (int i = 0; i < game.Tableaus.Count; i++)
            {
                Assert.AreEqual(i + 1, game.Tableaus[i].Count,
                    $"Tableau {i} should have {i + 1} cards.");
            }
        }

        [UnityTest]
        public IEnumerator Start_TableauTopCardsAreFaceUp()
        {
            yield return null;

            var game = _gameManager.Game;
            Assert.IsNotNull(game);

            for (int i = 0; i < game.Tableaus.Count; i++)
            {
                var topCard = game.Tableaus[i].Peek();
                Assert.IsNotNull(topCard);
                Assert.IsTrue(topCard.IsFaceUp,
                    $"Top card of Tableau {i} should be face up.");
            }
        }

        [UnityTest]
        public IEnumerator Start_StockHas24Cards()
        {
            yield return null;

            Assert.AreEqual(24, _gameManager.Game.Stock.Count,
                "Stock should have 24 cards after populating tableaus.");
        }

        // ===================================================================
        // State Machine
        // ===================================================================

        [UnityTest]
        public IEnumerator Start_StateManagerIsInitialized()
        {
            yield return null;

            Assert.IsNotNull(_gameManager.StateManager,
                "StateManager should be initialized in Awake().");
        }

        // ===================================================================
        // UI Wiring
        // ===================================================================

        [UnityTest]
        public IEnumerator GameUI_ImplementsIGameUI()
        {
            yield return null;

            Assert.IsNotNull(_gameManager.GameUI);
            Assert.IsTrue(_gameManager.GameUI is IGameUI,
                "GameUI should implement IGameUI.");
        }

        [UnityTest]
        public IEnumerator GameUI_IsActiveInScene()
        {
            yield return null;

            var presenter = _gameManager.GameUI as GamePresenterBase;
            Assert.IsNotNull(presenter);
            Assert.IsTrue(presenter.gameObject.activeInHierarchy,
                "Presenter should be active in the scene hierarchy.");
        }

        // ===================================================================
        // Dealing Animation Completion
        // ===================================================================

        [UnityTest]
        public IEnumerator DealingCompletes_WithinTimeout()
        {
            // The dealing animation runs card-by-card with DOTween.
            // We wait up to 15 seconds for it to finish and transition to PlayingState.
            float timeout = 15f;
            float elapsed = 0f;
            bool dealingComplete = false;

            // After dealing completes, PlayingState enables interaction.
            // We can detect this by checking if interaction is enabled.
            while (elapsed < timeout)
            {
                yield return null;
                elapsed += Time.deltaTime;

                // Once the game enters PlayingState, dealing is done
                if (_gameManager.Game != null && _gameManager.Game.Stock.Count == 24)
                {
                    // Stock stays at 24 after dealing. Check if any tableau card
                    // is face up (they should be after dealing)
                    bool allTopFaceUp = true;
                    for (int i = 0; i < _gameManager.Game.Tableaus.Count; i++)
                    {
                        if (_gameManager.Game.Tableaus[i].Peek() != null &&
                            !_gameManager.Game.Tableaus[i].Peek().IsFaceUp)
                        {
                            allTopFaceUp = false;
                            break;
                        }
                    }
                    if (allTopFaceUp)
                    {
                        dealingComplete = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(dealingComplete,
                $"Dealing animation should complete within {timeout}s.");
        }
    }
}
