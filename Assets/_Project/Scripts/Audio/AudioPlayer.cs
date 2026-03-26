using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Solitaire.Audio
{
    /// <summary>
    /// Runtime audio playback engine. Persists across scenes via DontDestroyOnLoad.
    /// Registers itself with the <see cref="AudioServiceSO"/> on Awake so all scripts
    /// can play audio through the SO asset without any singleton or static reference.
    ///
    /// Pre-allocates a pool of AudioSources to avoid runtime allocations.
    /// Handles SFX (pooled, one-shot) and music (dual-source crossfade).
    /// </summary>
    public class AudioPlayer : MonoBehaviour, IAudioPlayer
    {
        [Header("Service")]
        [SerializeField] private AudioServiceSO _audioService;

        [Header("Mixer (optional)")]
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private AudioMixerGroup _musicMixerGroup;

        [Header("SFX Pool")]
        [SerializeField, Range(2, 16)] private int _poolSize = 6;

        [Header("Music")]
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;
        [SerializeField, Range(0f, 2f)] private float _crossfadeDuration = 1f;

        private AudioSource[] _sfxPool;
        private int _nextSourceIndex;
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private Coroutine _crossfadeCoroutine;
        private float _sfxVolume = 1f;

        // Cooldown tracking: SoundSO instance ID -> last play time
        private readonly Dictionary<int, float> _cooldowns = new Dictionary<int, float>();

        // --- Lifecycle ---

        private void Awake()
        {
            if (_audioService == null)
            {
                Debug.LogError("[AudioPlayer] _audioService is NULL! Assign the AudioServiceSO asset.");
                return;
            }

            // Prevent duplicates when returning to a scene that has another AudioPlayer
            if (_audioService.IsReady)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            _audioService.RegisterPlayer(this);

            InitSFXPool();
            InitMusicSources();
        }

        private void OnDestroy()
        {
            if (_audioService != null)
                _audioService.UnregisterPlayer(this);
        }

        // --- IAudioPlayer: SFX ---

        public void PlaySFX(SoundSO sound)
        {
            if (sound == null || sound.Clip == null) return;

            // Cooldown check
            if (sound.Cooldown > 0f)
            {
                int id = sound.GetInstanceID();
                if (_cooldowns.TryGetValue(id, out float lastTime))
                {
                    if (Time.unscaledTime - lastTime < sound.Cooldown)
                        return;
                }
                _cooldowns[id] = Time.unscaledTime;
            }

            var source = GetNextSource();
            source.clip = sound.Clip;
            source.volume = sound.RandomVolume * _sfxVolume;
            source.pitch = sound.RandomPitch;
            source.Play();
        }

        // --- IAudioPlayer: Music ---

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);

            if (clip == null)
            {
                _musicSourceA.Stop();
                _musicSourceB.Stop();
                return;
            }

            // Swap: B becomes outgoing, A becomes incoming
            (_musicSourceA, _musicSourceB) = (_musicSourceB, _musicSourceA);

            _musicSourceA.clip = clip;
            _musicSourceA.loop = loop;
            _musicSourceA.volume = 0f;
            _musicSourceA.Play();

            _crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine());
        }

        public void StopMusic()
        {
            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
                _crossfadeCoroutine = null;
            }

            _musicSourceA.Stop();
            _musicSourceB.Stop();
        }

        // --- IAudioPlayer: Volume ---

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                if (_sfxMixerGroup != null)
                    _sfxMixerGroup.audioMixer.SetFloat("SFXVolume", LinearToDb(_sfxVolume));
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (_musicMixerGroup != null)
                    _musicMixerGroup.audioMixer.SetFloat("MusicVolume", LinearToDb(_musicVolume));
                else if (_musicSourceA != null)
                    _musicSourceA.volume = _musicVolume;
            }
        }

        // --- Initialization ---

        private void InitSFXPool()
        {
            _sfxPool = new AudioSource[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                if (_sfxMixerGroup != null)
                    source.outputAudioMixerGroup = _sfxMixerGroup;
                _sfxPool[i] = source;
            }
        }

        private void InitMusicSources()
        {
            _musicSourceA = CreateMusicSource("Music_A");
            _musicSourceB = CreateMusicSource("Music_B");
        }

        private AudioSource CreateMusicSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            if (_musicMixerGroup != null)
                source.outputAudioMixerGroup = _musicMixerGroup;
            return source;
        }

        // --- Pool ---

        private AudioSource GetNextSource()
        {
            var source = _sfxPool[_nextSourceIndex];
            _nextSourceIndex = (_nextSourceIndex + 1) % _sfxPool.Length;
            return source;
        }

        // --- Crossfade ---

        private IEnumerator CrossfadeCoroutine()
        {
            float elapsed = 0f;
            float duration = _crossfadeDuration > 0f ? _crossfadeDuration : 0.01f;
            float startVolumeB = _musicSourceB.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                _musicSourceA.volume = Mathf.Lerp(0f, _musicVolume, t);
                _musicSourceB.volume = Mathf.Lerp(startVolumeB, 0f, t);

                yield return null;
            }

            _musicSourceA.volume = _musicVolume;
            _musicSourceB.Stop();
            _musicSourceB.volume = 0f;
            _crossfadeCoroutine = null;
        }

        // --- Helpers ---

        private static float LinearToDb(float linear)
        {
            return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        }
    }
}
