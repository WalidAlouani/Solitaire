using UnityEngine;

namespace Solitaire.Audio
{
    /// <summary>
    /// Data container for a single sound effect.
    /// Create one asset per distinct sound (e.g. SFX_CardFlip, SFX_ButtonClick).
    /// Volume and pitch ranges add subtle variation so repeated plays don't feel robotic.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSound", menuName = "Solitaire/Audio/Sound")]
    public class SoundSO : ScriptableObject
    {
        [Header("Clip")]
        [SerializeField] private AudioClip _clip;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float _volumeMin = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _volumeMax = 1f;

        [Header("Pitch")]
        [SerializeField, Range(0.5f, 2f)] private float _pitchMin = 0.95f;
        [SerializeField, Range(0.5f, 2f)] private float _pitchMax = 1.05f;

        [Header("Playback")]
        [Tooltip("Minimum seconds between plays. 0 = no limit.")]
        [SerializeField, Range(0f, 1f)] private float _cooldown;

        public AudioClip Clip => _clip;
        public float RandomVolume => Random.Range(_volumeMin, _volumeMax);
        public float RandomPitch => Random.Range(_pitchMin, _pitchMax);
        public float Cooldown => _cooldown;
    }
}
