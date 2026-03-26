using UnityEngine;

namespace Solitaire.UI
{
    /// <summary>
    /// Marks a UI element as responsive to orientation changes.
    /// Instead of duplicating elements across portrait/landscape layouts,
    /// the element is reparented into the correct holder on each switch
    /// and stretched to fill its container.
    ///
    /// Each layout provides an empty RectTransform "holder" that defines
    /// the element's size and position for that orientation.
    /// The element's anchors are set to (0,0)→(1,1) with zero offsets
    /// so it always fills its current holder.
    ///
    /// Driven by <see cref="OrientationChanger"/> — do not call manually.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ResponsiveElement : MonoBehaviour
    {
        [Tooltip("The holder RectTransform inside the portrait layout.")]
        [SerializeField] private RectTransform _portraitParent;

        [Tooltip("The holder RectTransform inside the landscape layout.")]
        [SerializeField] private RectTransform _landscapeParent;

        private RectTransform _rect;

        /// <summary>
        /// Called by OrientationChanger when the orientation changes.
        /// Reparents this element into the correct holder and stretches to fill.
        /// </summary>
        public void ApplyOrientation(bool isPortrait)
        {
            if (_rect == null)
                _rect = GetComponent<RectTransform>();

            var target = isPortrait ? _portraitParent : _landscapeParent;
            if (target == null) return;

            _rect.SetParent(target, false);
            Stretch();
        }

        /// <summary>
        /// Sets anchors to fill the parent and zeroes all offsets.
        /// </summary>
        private void Stretch()
        {
            _rect.anchorMin = Vector2.zero;
            _rect.anchorMax = Vector2.one;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
