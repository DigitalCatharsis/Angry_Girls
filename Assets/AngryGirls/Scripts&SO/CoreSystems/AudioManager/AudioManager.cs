using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Legacy Sound ID enum. Preserved for backward compatibility with existing callers.
    /// </summary>
    public enum AudioSourceType
    {
        None = 0,
        CharacterHit = 1,
        LevelMusic = 2,
        Coins = 3,
        SFX_Impact = 4,
    }

    /// <summary>
    /// Standard audio categories for volume and pause control.
    /// </summary>
    public enum AudioCategory
    {
        None = 0,
        Music = 1,
        SFX = 2,
        UI = 3,
        Ambient = 4
    }

    /// <summary>
    /// Configuration for a single sound slot.
    /// No AudioSource required in Inspector (managed dynamically).
    /// </summary>
    [Serializable]
    public class SoundData
    {
        [Tooltip("Library containing clips for this sound")]
        public AudioClipLibrary audioClipLibrary;

        [Tooltip("Category for volume and pause control (Music vs SFX)")]
        public AudioCategory category = AudioCategory.SFX;
    }

    /// <summary>
    /// Manages audio playback, volume settings, and pause states.
    /// Creates AudioSources dynamically at runtime.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<AudioSourceType, SoundData> _soundDataDict = new();

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private float _minRandomPitchDiff = 0.3f;
        private float _targetMusicVolume = 1f;
        private bool _isMusicInitialized = false;

        /// <summary>
        /// Initialize audio manager. Creates internal AudioSources.
        /// Must be called after CoreManager initializes SettingsManager.
        /// </summary>
        public void Init()
        {
            if (CoreManager.Instance == null)
            {
                Debug.LogError("AudioManager: CoreManager not found.");
                return;
            }

            CreateDynamicAudioSources();
            SubscribeToEvents();
            UpdateVolumesFromSettings();
            _isMusicInitialized = true;
        }

        /// <summary>
        /// Creates internal AudioSources for Music and SFX.
        /// </summary>
        private void CreateDynamicAudioSources()
        {
            // Music Source (Looping)
            var musicObj = new GameObject("Dynamic_MusicSource");
            musicObj.transform.SetParent(transform);
            _musicSource = musicObj.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f; // 2D

            // SFX Source (One-Shot)
            var sfxObj = new GameObject("Dynamic_SFXSource");
            sfxObj.transform.SetParent(transform);
            _sfxSource = sfxObj.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.spatialBlend = 0f; // 2D
        }

        /// <summary>
        /// Subscribes to Settings and Pause events.
        /// </summary>
        private void SubscribeToEvents()
        {
            CoreManager.Instance.SettingsManager.OnSettingsChanged += UpdateVolumesFromSettings;
            CoreManager.Instance.PauseControl.OnPauseChanged += HandlePauseChanged;
        }

        /// <summary>
        /// Handles game pause state changes.
        /// Common pattern: Music pauses on game pause, SFX may continue or pause.
        /// </summary>
        private void HandlePauseChanged(bool isPaused)
        {
            if (isPaused)
            {
                _musicSource.Pause();
                _sfxSource.Pause();
            }
            else
            {
                _musicSource.UnPause();
                _sfxSource.UnPause();
            }
        }

        /// <summary>
        /// Updates volume for all sources based on SettingsManager data.
        /// </summary>
        private void UpdateVolumesFromSettings()
        {
            if (CoreManager.Instance == null || CoreManager.Instance.SettingsManager == null) return;

            var settings = CoreManager.Instance.SettingsManager.GetSettings();

            // Store target volume for fade operations
            _targetMusicVolume = settings.volumeMusic;
            _musicSource.volume = _targetMusicVolume;

            // Apply SFX Volume
            _sfxSource.volume = settings.volumeSounds;
        }

        /// <summary>
        /// Plays music from specified library with optional crossfade.
        /// </summary>
        public async UniTask PlayMusicAsync(AudioClipData data, int clipIndex = -1, float crossfadeDuration = 0f)
        {
            if (data == null || _musicSource == null) return;

            if (crossfadeDuration > 0f && _musicSource.isPlaying)
            {
                // Fade out current music
                await FadeOutMusicAsync(crossfadeDuration);
            }

            _musicSource.clip = data.clip;
            _musicSource.Play();

            if (crossfadeDuration > 0f)
            {
                // Fade in new music
                await FadeInMusicAsync(crossfadeDuration);
            }
        }

        /// <summary>
        /// Stops music playback.
        /// </summary>
        public void StopMusic()
        {
            if (_musicSource == null) return;
            _musicSource.Stop();
            _musicSource.clip = null;
        }

        /// <summary>
        /// Fades in music volume over specified duration.
        /// Compatible with base DOTween + UniTask.
        /// </summary>
        public async UniTask FadeInMusicAsync(float duration)
        {
            if (!_isMusicInitialized || _musicSource == null) return;

            _musicSource.volume = 0f;
            float elapsed = 0f;
            float startVolume = 0f;
            float targetVolume = _targetMusicVolume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                await UniTask.Yield();
            }
            _musicSource.volume = targetVolume;
        }

        /// <summary>
        /// Fades out music volume over specified duration.
        /// Compatible with base DOTween + UniTask (no DOTweenModuleUniTask required).
        /// </summary>
        public async UniTask FadeOutMusicAsync(float duration)
        {
            if (!_isMusicInitialized || _musicSource == null) return;

            float elapsed = 0f;
            float startVolume = _musicSource.volume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                await UniTask.Yield();
            }
            _musicSource.volume = 0f;
        }

        /// <summary>
        /// Plays a random clip from the specified sound library.
        /// Routes to Music or SFX source based on Category.
        /// </summary>
        public void PlayRandomSound(AudioSourceType type, bool randomPitch = false)
        {
            if (!_soundDataDict.TryGetValue(type, out var soundData))
            {
                Debug.LogWarning($"AudioManager: SoundData not found for type {type}");
                return;
            }

            if (soundData.audioClipLibrary == null)
            {
                Debug.LogWarning($"AudioManager: Missing Library for type {type}");
                return;
            }

            var clipData = soundData.audioClipLibrary.GetRandomClipData();
            PlayClipData(clipData, soundData.category, randomPitch);
        }

        /// <summary>
        /// Plays clip using AudioClipData metadata.
        /// </summary>
        public void PlayClipData(AudioClipData clipData, AudioCategory category, bool randomPitch)
        {
            if (clipData == null || clipData.clip == null) return;

            var source = GetSourceForCategory(category);
            if (source == null) return;

            // Apply per-clip volume as MULTIPLIER, not replacement
            float baseVolume = source.volume; // Already set from SettingsManager
            float effectiveVolume = baseVolume * clipData.volumeOverride;

            if (category == AudioCategory.Music || category == AudioCategory.Ambient)
            {
                source.clip = clipData.clip;
                source.volume = effectiveVolume; // Apply for music
                source.Play();
            }
            else
            {
                // For OneShot, pass volume as parameter
                source.PlayOneShot(clipData.clip, effectiveVolume);
            }

            // Apply pitch
            if (randomPitch)
            {
                var (min, max) = clipData.GetPitchRange();
                source.pitch = UnityEngine.Random.Range(min, max);
            }
            else
            {
                source.pitch = 1f;
            }
        }

        ///// <summary>
        ///// Plays a specific clip by index from the specified sound library.
        ///// </summary>
        //public void PlayCustomSound(AudioSourceType type, int index = 0, bool randomPitch = false)
        //{
        //    if (!_soundDataDict.TryGetValue(type, out var soundData))
        //    {
        //        Debug.LogWarning($"AudioManager: SoundData not found for type {type}");
        //        return;
        //    }

        //    if (soundData.audioClipLibrary == null)
        //    {
        //        Debug.LogWarning($"AudioManager: Missing Library for type {type}");
        //        return;
        //    }

        //    // Use AudioClipData instead of raw AudioClip
        //    var clipData = soundData.audioClipLibrary.GetClipByIndex(index);
        //    PlayClipData(clipData, soundData.category, randomPitch);
        //}

        ///// <summary>
        ///// Internal helper to play clip on correct source with correct volume.
        ///// </summary>
        //private void PlayClip(AudioClip clip, AudioCategory category, bool randomPitch)
        //{
        //    if (clip == null) return;

        //    AudioSource source = GetSourceForCategory(category);
        //    if (source == null) return;

        //    SetupPitch(source, randomPitch);

        //    if (category == AudioCategory.Music || category == AudioCategory.Ambient)
        //    {
        //        source.clip = clip;
        //        source.Play();
        //    }
        //    else
        //    {
        //        source.PlayOneShot(clip, 1f);
        //    }
        //}

        /// <summary>
        /// Returns the correct AudioSource based on category.
        /// </summary>
        private AudioSource GetSourceForCategory(AudioCategory category)
        {
            switch (category)
            {
                case AudioCategory.Music:
                case AudioCategory.Ambient:
                    return _musicSource;
                case AudioCategory.SFX:
                case AudioCategory.UI:
                default:
                    return _sfxSource;
            }
        }

        /// <summary>
        /// Applies random pitch variation if enabled.
        /// </summary>
        private void SetupPitch(AudioSource source, bool randomPitch)
        {
            if (randomPitch)
            {
                source.pitch = UnityEngine.Random.Range(1f - _minRandomPitchDiff, 1f + _minRandomPitchDiff);
            }
            else
            {
                source.pitch = 1f;
            }
        }
    }
}