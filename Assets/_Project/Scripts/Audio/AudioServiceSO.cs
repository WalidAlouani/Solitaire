using UnityEngine;

namespace Solitaire.Audio
{
    /// <summary>
    /// ScriptableObject that acts as the public API for all audio playback.
    /// Scripts reference this asset via [SerializeField] — no singleton, no static access.
    ///
    /// At runtime, an <see cref="AudioPlayer"/> registers itself here.
    /// All calls are forwarded to the registered player.
    ///
    /// Usage:
    ///   [SerializeField] private AudioServiceSO _audio;
    ///   _audio.PlaySFX(someSound);
    /// </summary>
    [CreateAssetMenu(fileName = "AudioService", menuName = "Solitaire/Audio/Audio Service")]
    public class AudioServiceSO : ScriptableObject
    {
        private IAudioPlayer _player;

        /// <summary>
        /// Called by AudioPlayer on Awake to register itself as the active player.
        /// </summary>
        public void RegisterPlayer(IAudioPlayer player)
        {
            _player = player;
        }

        /// <summary>
        /// Called by AudioPlayer on OnDestroy to unregister.
        /// </summary>
        public void UnregisterPlayer(IAudioPlayer player)
        {
            if (_player == player)
                _player = null;
        }

        public bool IsReady => _player != null;

        // --- SFX ---

        public void PlaySFX(SoundSO sound)
        {
            if (_player != null && sound != null)
                _player.PlaySFX(sound);
        }

        // --- Music ---

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_player != null)
                _player.PlayMusic(clip, loop);
        }

        public void StopMusic()
        {
            _player?.StopMusic();
        }

        // --- Volume ---

        public float SFXVolume
        {
            get => _player?.SFXVolume ?? 1f;
            set { if (_player != null) _player.SFXVolume = value; }
        }

        public float MusicVolume
        {
            get => _player?.MusicVolume ?? 1f;
            set { if (_player != null) _player.MusicVolume = value; }
        }

        // --- Editor Play Mode Cleanup ---

#if UNITY_EDITOR
        /// <summary>
        /// Clears the stale player reference when exiting Play Mode.
        /// Prevents IsReady returning true after domain reload is skipped
        /// (Enter Play Mode Settings with Reload Domain disabled).
        /// </summary>
        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                _player = null;
        }
#endif
    }
}
