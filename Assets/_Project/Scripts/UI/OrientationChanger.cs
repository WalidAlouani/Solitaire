using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire.UI
{
    /// <summary>
    /// Switches between portrait and landscape layouts when the screen dimensions change.
    /// Uses OnRectTransformDimensionsChange (event-driven) instead of polling in Update.
    /// Must live on a GameObject with a RectTransform (e.g. a Canvas).
    ///
    /// Drives <see cref="ResponsiveElement"/>s: instead of duplicating UI elements
    /// across both layouts, each element is reparented into the correct holder
    /// on each orientation switch.
    /// </summary>
    public class OrientationChanger : UIBehaviour
    {
        [SerializeField] private GameObject _portraitLayout;
        [SerializeField] private GameObject _landscapeLayout;

        [Tooltip("Elements that move between portrait/landscape holders on orientation change.")]
        [SerializeField] private List<ResponsiveElement> _responsiveElements = new List<ResponsiveElement>();

        private bool _isPortrait;

        public bool IsPortrait => _isPortrait;

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
            // Activate both layouts temporarily so holders are available for reparenting
            _portraitLayout.SetActive(true);
            _landscapeLayout.SetActive(true);

            // Move responsive elements into their target holders
            for (int i = 0; i < _responsiveElements.Count; i++)
            {
                if (_responsiveElements[i] != null)
                    _responsiveElements[i].ApplyOrientation(portrait);
            }

            // Deactivate the inactive layout
            _portraitLayout.SetActive(portrait);
            _landscapeLayout.SetActive(!portrait);
        }
    }
}
