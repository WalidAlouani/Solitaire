using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Presentation.Canvas.UI
{
    /// <summary>
    /// Win screen popup with fade-in animation.
    /// The GameObject must be set INACTIVE in the scene hierarchy.
    /// Call Show() to reveal with animation, Hide() to dismiss.
    /// </summary>
    public class WinScreenPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _delayBeforeShow = 0.5f;

        public event Action OnPlayAgainClicked;
        public event Action OnMainMenuClicked;

        private RectTransform _panelTransform;
        private bool _initialized;

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _panelTransform = GetComponent<RectTransform>();
            _playAgainButton.onClick.AddListener(HandlePlayAgain);
            _mainMenuButton.onClick.AddListener(HandleMainMenu);
        }

        private void OnDestroy()
        {
            if (!_initialized) return;
            _playAgainButton.onClick.RemoveListener(HandlePlayAgain);
            _mainMenuButton.onClick.RemoveListener(HandleMainMenu);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Initialize();
            StartCoroutine(AnimateIn());
        }

        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        private IEnumerator AnimateIn()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _panelTransform.localScale = Vector3.one * 0.85f;

            yield return new WaitForSeconds(_delayBeforeShow);

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _fadeDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                _canvasGroup.alpha = eased;
                _panelTransform.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, eased);

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _panelTransform.localScale = Vector3.one;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void HandlePlayAgain()
        {
            OnPlayAgainClicked?.Invoke();
        }

        private void HandleMainMenu()
        {
            OnMainMenuClicked?.Invoke();
        }
    }
}
