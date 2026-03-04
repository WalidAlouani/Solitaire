using UnityEngine;

namespace Solitaire.UI
{
    public class OrientationChanger : MonoBehaviour
    {
        [SerializeField] private GameObject _portraitLayout;
        [SerializeField] private GameObject _landscapeLayout;

        private ScreenOrientation _lastOrientation;

        private void Start()
        {
            _lastOrientation = Screen.orientation;
            HandleOrientationChange(_lastOrientation);
        }

        private void Update()
        {
            if (Screen.orientation != _lastOrientation)
            {
                HandleOrientationChange(Screen.orientation);
                _lastOrientation = Screen.orientation;
            }
        }

        private void HandleOrientationChange(ScreenOrientation newOrientation)
        {
            switch (newOrientation)
            {
                case ScreenOrientation.Portrait:
                case ScreenOrientation.PortraitUpsideDown:
                    _portraitLayout.SetActive(true);
                    _landscapeLayout.SetActive(false);
                    break;

                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                    _portraitLayout.SetActive(false);
                    _landscapeLayout.SetActive(true);
                    break;
            }
        }
    }
}