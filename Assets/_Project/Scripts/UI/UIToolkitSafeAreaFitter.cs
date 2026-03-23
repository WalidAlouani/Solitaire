using UnityEngine;
using UnityEngine.UIElements;

namespace Solitaire.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIToolkitSafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private string _safeAreaElementName = "SafeArea";

        private UIDocument _uiDocument;
        private VisualElement _safeAreaElement;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            var root = _uiDocument.rootVisualElement;

            root.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);

            // Schedule initial apply for next frame — handles the case where
            // the root geometry is already resolved before we register.
            root.schedule.Execute(InitialApply);
        }

        private void OnDisable()
        {
            if (_safeAreaElement != null)
                _safeAreaElement.UnregisterCallback<GeometryChangedEvent>(OnSafeAreaGeometryChanged);

            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
                _uiDocument.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
        }

        private void InitialApply()
        {
            TryFindSafeAreaElement();
            ApplySafeArea();
        }

        private void OnRootGeometryChanged(GeometryChangedEvent evt)
        {
            TryFindSafeAreaElement();
            ApplySafeArea();
        }

        private void OnSafeAreaGeometryChanged(GeometryChangedEvent evt)
        {
            ApplySafeArea();
        }

        private void TryFindSafeAreaElement()
        {
            if (_safeAreaElement != null) return;
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;

            _safeAreaElement = _uiDocument.rootVisualElement.Q<VisualElement>(_safeAreaElementName);

            if (_safeAreaElement != null)
                _safeAreaElement.RegisterCallback<GeometryChangedEvent>(OnSafeAreaGeometryChanged);
        }

        private void ApplySafeArea()
        {
            if (_safeAreaElement == null) return;

            var safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);

            if (safeArea == _lastSafeArea && screenSize == _lastScreenSize)
                return;

            _lastSafeArea = safeArea;
            _lastScreenSize = screenSize;

            if (Screen.width <= 0 || Screen.height <= 0) return;

            float leftPx = safeArea.x;
            float topPx = Screen.height - (safeArea.y + safeArea.height);
            float rightPx = Screen.width - (safeArea.x + safeArea.width);
            float bottomPx = safeArea.y;

            // Convert screen-space insets to panel-space
            float panelScale = (float)_uiDocument.panelSettings.referenceResolution.x / Screen.width;

            _safeAreaElement.style.paddingLeft = leftPx * panelScale;
            _safeAreaElement.style.paddingTop = topPx * panelScale;
            _safeAreaElement.style.paddingRight = rightPx * panelScale;
            _safeAreaElement.style.paddingBottom = bottomPx * panelScale;
        }
    }
}
