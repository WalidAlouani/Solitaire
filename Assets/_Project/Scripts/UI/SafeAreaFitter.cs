using UnityEngine;

namespace Solitaire.UI
{
    /// <summary>
    /// Adjusts a RectTransform to fit within the device's safe area,
    /// preventing UI from being hidden behind notches, dynamic islands, or rounded corners.
    /// Attach to a full-screen RectTransform that acts as the root content container.
    /// 
    /// Reacts to resolution/orientation changes via OnRectTransformDimensionsChange
    /// instead of polling every frame in Update.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rectTransform == null) return;
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            var safeArea = Screen.safeArea;

            if (safeArea == _lastSafeArea) return;
            _lastSafeArea = safeArea;

            if (Screen.width <= 0 || Screen.height <= 0) return;

            _rectTransform.anchorMin = new Vector2(
                safeArea.x / Screen.width,
                safeArea.y / Screen.height
            );
            _rectTransform.anchorMax = new Vector2(
                (safeArea.x + safeArea.width) / Screen.width,
                (safeArea.y + safeArea.height) / Screen.height
            );
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Returns the current safe area insets in pixels (left, top, right, bottom).
        /// Useful for UI Toolkit code that needs to apply padding based on the safe area.
        /// </summary>
        public static (float left, float top, float right, float bottom) GetSafeAreaInsets()
        {
            var safeArea = Screen.safeArea;
            float left = safeArea.x;
            float top = Screen.height - (safeArea.y + safeArea.height);
            float right = Screen.width - (safeArea.x + safeArea.width);
            float bottom = safeArea.y;
            return (left, top, right, bottom);
        }
    }
}