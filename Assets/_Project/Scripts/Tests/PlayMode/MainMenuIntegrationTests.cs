using System.Collections;
using NUnit.Framework;
using Solitaire.UI.MainMenu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Solitaire.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for the Main Menu screen.
    /// Verifies button wiring, interactability, and scene transitions.
    /// </summary>
    public class MainMenuIntegrationTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;
        }

        // ===================================================================
        // Button Existence & Wiring
        // ===================================================================

        [UnityTest]
        public IEnumerator AllButtons_ExistInScene()
        {
            yield return null;

            // Count active buttons only (excludes inactive Horizontal layout)
            var buttons = Object.FindObjectsOfType<Button>(false);
            Assert.IsTrue(buttons.Length >= 5,
                $"MainMenu should have at least 5 active buttons, found {buttons.Length}.");
        }

        [UnityTest]
        public IEnumerator NewGameButton_ExistsAndIsInteractable()
        {
            yield return null;

            var btn = FindActiveButtonByName("Btn_NewGame");
            Assert.IsNotNull(btn, "Btn_NewGame should exist as an active button in MainMenu scene.");
            Assert.IsTrue(btn.interactable, "New Game button should be interactable.");
        }

        [UnityTest]
        public IEnumerator ContinueButton_ExistsAndIsDisabled()
        {
            // Wait for Start() to run and disable the button
            yield return null;
            yield return null;

            var btn = FindActiveButtonByName("Btn_Continue");
            Assert.IsNotNull(btn, "Btn_Continue should exist as an active button in MainMenu scene.");
            Assert.IsFalse(btn.interactable,
                "Continue button should be disabled (no save data).");
        }

        [UnityTest]
        public IEnumerator SettingsButton_ExistsAndIsInteractable()
        {
            yield return null;

            var btn = FindActiveButtonByName("Btn_Settings");
            Assert.IsNotNull(btn, "Btn_Settings should exist as an active button in MainMenu scene.");
            Assert.IsTrue(btn.interactable, "Settings button should be interactable.");
        }

        [UnityTest]
        public IEnumerator LeaderboardButton_ExistsAndIsInteractable()
        {
            yield return null;

            var btn = FindActiveButtonByName("Btn_Leaderboard");
            Assert.IsNotNull(btn, "Btn_Leaderboard should exist as an active button in MainMenu scene.");
            Assert.IsTrue(btn.interactable, "Leaderboard button should be interactable.");
        }

        [UnityTest]
        public IEnumerator InfoButton_ExistsAndIsInteractable()
        {
            yield return null;

            var btn = FindActiveButtonByName("Btn_Info");
            Assert.IsNotNull(btn, "Btn_Info should exist as an active button in MainMenu scene.");
            Assert.IsTrue(btn.interactable, "Info button should be interactable.");
        }

        // ===================================================================
        // Scene Transitions
        // ===================================================================

        [UnityTest]
        public IEnumerator NewGameButton_Click_LoadsGameScene()
        {
            yield return null;

            var btn = FindActiveButtonByName("Btn_NewGame");
            Assert.IsNotNull(btn, "Btn_NewGame not found.");

            // Simulate button click
            btn.onClick.Invoke();

            // Wait for scene to fully load (LoadScene is not instant in Play Mode)
            float timeout = 5f;
            float elapsed = 0f;
            while (elapsed < timeout)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;

                if (SceneManager.GetActiveScene().name == "Game-Canvas")
                    break;
            }

            Assert.AreEqual("Game-Canvas", SceneManager.GetActiveScene().name,
                "Clicking New Game should load the Game-Canvas scene.");
        }

        // ===================================================================
        // Controller Wiring
        // ===================================================================

        [UnityTest]
        public IEnumerator MainMenuController_AttachedToActiveObject()
        {
            yield return null;

            // Search only active controllers
            var controllers = Object.FindObjectsOfType<MainMenuController>(false);
            Assert.IsTrue(controllers.Length > 0,
                "At least one active MainMenuController should exist in the scene.");
        }

        // ===================================================================
        // Helpers
        // ===================================================================

        /// <summary>
        /// Finds a button by GameObject name, searching only active objects.
        /// This avoids finding duplicate buttons on the inactive layout (Horizontal).
        /// </summary>
        private Button FindActiveButtonByName(string name)
        {
            var buttons = Object.FindObjectsOfType<Button>(false);
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name == name)
                    return btn;
            }
            return null;
        }
    }
}
