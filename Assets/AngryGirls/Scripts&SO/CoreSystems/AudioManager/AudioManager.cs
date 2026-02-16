using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum AudioSourceType
    {
        None = 0,
        CharacterHit = 1,
        LevelMusic = 2,
        Coins = 3,
        SFX_Impact = 4,
    }

    /// <summary>
    /// Manages audio playback and volume settings
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private float _minRandomDiff = 0.3f;

        [SerializeField] private SerializedDictionary<AudioSourceType, SoundData> _soundDataDict = new();

        public void Init()
        {
            CoreManager.Instance.SettingsManager.OnSettingsChanged += UpdateAudioSourceVolumeFromSettings;
            CoreManager.Instance.PauseControl.OnPauseChanged += PauseUnpauseMusic;
            UpdateAudioSourceVolumeFromSettings();
        }

        private void PauseUnpauseMusic(bool isPaused)
        {
            if (isPaused)
            {
                foreach (var soundData in _soundDataDict.Values)
                {
                    soundData.audioSource.Pause();
                }
            }
            else
            {
                foreach (var soundData in _soundDataDict.Values)
                {
                    soundData.audioSource.UnPause();
                }
            }
        }

        /// <summary>
        /// Updates volume for specific audio source type
        /// </summary>
        public void UpdateAudioSourceVolume(AudioSourceType audioSourceType, float volumeValue)
        {
            if (_soundDataDict.ContainsKey(audioSourceType))
            {
                _soundDataDict[audioSourceType].audioSource.volume = volumeValue;
            }
            else
            {
                ColorDebugLog.Log("wrong Audiosource key", System.Drawing.KnownColor.DarkRed);
            }
        }

        /// <summary>
        /// Updates all audio sources volume from settings
        /// </summary>
        public void UpdateAudioSourceVolumeFromSettings()
        {
            foreach (var kvp in _soundDataDict)
            {
                var source = kvp.Value.audioSource;

                switch (kvp.Key)
                {
                    case (AudioSourceType.LevelMusic):
                        source.volume = CoreManager.Instance.SettingsManager.GetSettings().volumeMusic;
                        break;
                    default:
                        source.volume = CoreManager.Instance.SettingsManager.GetSettings().volumeSounds;
                        break;
                }
            }
        }

        /// <summary>
        /// Plays random sound from audio clip collection
        /// </summary>
        public void PlayRandomSound(AudioSourceType type, bool randomPitch = false)
        {
            var audiosource = _soundDataDict[type].audioSource;
            var audioclipCollection = _soundDataDict[type].audioClipCollection;

            if (randomPitch)
            {
                audiosource.pitch = (UnityEngine.Random.Range(audiosource.volume - _minRandomDiff, audiosource.volume));
            }
            audiosource.PlayOneShot(GetRandomAudioClip(audioclipCollection), 1);
        }

        /// <summary>
        /// Plays specific sound by index
        /// </summary>
        public void PlayCustomSound(AudioSourceType type, int index = 0, bool randomPitch = false)
        {
            var audiosource = _soundDataDict[type].audioSource;
            var audioclipCollection = _soundDataDict[type].audioClipCollection;

            if (randomPitch)
            {
                audiosource.pitch = (UnityEngine.Random.Range(audiosource.volume - _minRandomDiff, audiosource.volume));
            }

            audiosource.PlayOneShot(audioclipCollection.audioClips[index], 1);
        }

        private AudioClip GetRandomAudioClip(SoundClipsCollection clipsCollection)
        {
            var randomIndex = UnityEngine.Random.Range(0, (clipsCollection.audioClips.Count));
            var randomValue = clipsCollection.audioClips[randomIndex];
            return randomValue;
        }

        private AudioClip GetCusomAudioClip(SoundClipsCollection clipsCollection, int index)
        {
            var value = clipsCollection.audioClips[index];
            return value;
        }
    }

    [Serializable]
    public class SoundData
    {
        public AudioSource audioSource;
        public SoundClipsCollection audioClipCollection;
    }
}