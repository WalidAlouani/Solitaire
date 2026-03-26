using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Audio
{
    /// <summary>
    /// Drop this on any GameObject with a <see cref="Button"/> to play a sound on click.
    /// No singleton needed — references the AudioServiceSO asset directly.
    ///
    /// Reusable across MainMenu, in-game UI, popups, etc.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private AudioServiceSO _audioService;
        [SerializeField] private SoundSO _clickSound;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            _audioService.PlaySFX(_clickSound);
        }
    }
}
