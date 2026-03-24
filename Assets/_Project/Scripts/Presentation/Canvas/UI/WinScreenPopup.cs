using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Presentation.Canvas.UI
{
    /// <summary>
    /// Win screen popup with DOTween fade-in + scale animation.
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
        private Sequence _showSequence;
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
            _showSequence?.Kill();

            if (!_initialized) return;
            _playAgainButton.onClick.RemoveListener(HandlePlayAgain);
            _mainMenuButton.onClick.RemoveListener(HandleMainMenu);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Initialize();

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _panelTransform.localScale = Vector3.one * 0.85f;

            _showSequence?.Kill();
            _showSequence = DOTween.Sequence();

            float alpha = 0f;
            float scale = 0.85f;

            _showSequence.AppendInterval(_delayBeforeShow);

            // Fade in
            _showSequence.Append(
                DOTween.To(
                    () => alpha,
                    x => { alpha = x; _canvasGroup.alpha = x; },
                    1f, _fadeDuration
                ).SetEase(Ease.OutCubic)
            );

            // Scale up (joined with fade)
            _showSequence.Join(
                DOTween.To(
                    () => scale,
                    x => { scale = x; _panelTransform.localScale = Vector3.one * x; },
                    1f, _fadeDuration
                ).SetEase(Ease.OutBack)
            );

            _showSequence.OnComplete(() =>
            {
                _canvasGroup.alpha = 1f;
                _panelTransform.localScale = Vector3.one;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
        }

        public void Hide()
        {
            _showSequence?.Kill();
            gameObject.SetActive(false);
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
