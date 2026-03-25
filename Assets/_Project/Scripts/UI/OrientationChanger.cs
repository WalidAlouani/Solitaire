using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire.UI
{
    /// <summary>
    /// Switches between portrait and landscape layouts when the screen dimensions change.
    /// Uses OnRectTransformDimensionsChange (event-driven) instead of polling in Update.
    /// Must live on a GameObject with a RectTransform (e.g. a Canvas).
    /// </summary>
    public class OrientationChanger : UIBehaviour
    {
        [SerializeField] private GameObject _portraitLayout;
        [SerializeField] private GameObject _landscapeLayout;

        private bool _isPortrait;

        protected override void Start()
        {
            base.Start();
            _isPortrait = Screen.height >= Screen.width;
            ApplyLayout(_isPortrait);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            bool portrait = Screen.height >= Screen.width;
            if (portrait == _isPortrait) return;

            _isPortrait = portrait;
            ApplyLayout(_isPortrait);
        }

        private void ApplyLayout(bool portrait)
        {
            _portraitLayout.SetActive(portrait);
            _landscapeLayout.SetActive(!portrait);
        }
    }
}
