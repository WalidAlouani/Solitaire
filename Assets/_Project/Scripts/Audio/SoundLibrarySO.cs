using UnityEngine;

namespace Solitaire.Audio
{
    /// <summary>
    /// Central catalog of every sound in the game.
    /// One asset, referenced by anything that needs to play audio.
    /// Add new fields as new sounds are needed — no code changes in AudioService.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Solitaire/Audio/Sound Library")]
    public class SoundLibrarySO : ScriptableObject
    {
        [Header("Cards")]
        public SoundSO CardFlip;
        public SoundSO CardPlace;
        public SoundSO CardDeal;
        public SoundSO CardDraw;
        public SoundSO CardInvalidMove;

        [Header("Game")]
        public SoundSO WinFanfare;
        public SoundSO AutoCompleteStep;

        [Header("UI")]
        public SoundSO ButtonClick;
        public SoundSO ButtonHover;

        [Header("Music")]
        public AudioClip MenuMusic;
        public AudioClip GameMusic;
    }
}
