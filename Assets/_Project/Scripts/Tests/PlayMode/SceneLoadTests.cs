using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Solitaire.Tests.PlayMode
{
    /// <summary>
    /// Verifies that all game scenes load without errors
    /// and contain the expected root objects.
    /// </summary>
    public class SceneLoadTests
    {
        [UnityTest]
        public IEnumerator MainMenuScene_Loads_WithoutErrors()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null; // wait one frame for scene load

            var scene = SceneManager.GetActiveScene();
            Assert.AreEqual("MainMenu", scene.name);
            Assert.IsTrue(scene.isLoaded);
        }

        [UnityTest]
        public IEnumerator MainMenuScene_ContainsCanvas()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;

            var canvas = Object.FindObjectOfType<Canvas>();
            Assert.IsNotNull(canvas, "MainMenu scene should contain a Canvas.");
        }

        [UnityTest]
        public IEnumerator MainMenuScene_ContainsMainMenuController()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;

            var controller = Object.FindObjectOfType<UI.MainMenu.MainMenuController>();
            Assert.IsNotNull(controller, "MainMenu scene should contain a MainMenuController.");
        }

        [UnityTest]
        public IEnumerator GameCanvasScene_Loads_WithoutErrors()
        {
            SceneManager.LoadScene("Game-Canvas");
            yield return null;

            var scene = SceneManager.GetActiveScene();
            Assert.AreEqual("Game-Canvas", scene.name);
            Assert.IsTrue(scene.isLoaded);
        }

        [UnityTest]
        public IEnumerator GameCanvasScene_ContainsGameManager()
        {
            SceneManager.LoadScene("Game-Canvas");
            yield return null;

            var gm = Object.FindObjectOfType<Core.GameManager>();
            Assert.IsNotNull(gm, "Game-Canvas scene should contain a GameManager.");
        }

        [UnityTest]
        public IEnumerator GameCanvasScene_ContainsPresenter()
        {
            SceneManager.LoadScene("Game-Canvas");
            yield return null;

            var presenter = Object.FindObjectOfType<Presentation.GamePresenterBase>();
            Assert.IsNotNull(presenter, "Game-Canvas scene should contain a GamePresenterBase.");
        }

        [UnityTest]
        public IEnumerator GameCanvasScene_GameManagerHasUIReference()
        {
            SceneManager.LoadScene("Game-Canvas");
            yield return null;

            var gm = Object.FindObjectOfType<Core.GameManager>();
            Assert.IsNotNull(gm, "GameManager not found.");
            Assert.IsNotNull(gm.GameUI, "GameManager.GameUI should be wired to a presenter.");
        }
    }
}
