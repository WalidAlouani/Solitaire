using UnityEngine;

namespace Solitaire.Audio
{
    /// <summary>
    /// Contract for audio playback. Implemented by <see cref="AudioPlayer"/>.
    /// The <see cref="AudioServiceSO"/> forwards all calls to the registered player.
    /// </summary>
    public interface IAudioPlayer
    {
        void PlaySFX(SoundSO sound);
        void PlayMusic(AudioClip clip, bool loop);
        void StopMusic();
        float SFXVolume { get; set; }
        float MusicVolume { get; set; }
    }
}
