using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum AudioSourceType
    {
        None = 0,
        CharacterHit =1,
        LevelMusic =2,
        Coins =3 ,
        SFX_Impact =4,
    }

    public class AudioManager : MonoBehaviour
    {

        private float _minRandomDiff = 0.3f;

        [SerializeField] private SerializedDictionary<AudioSourceType, SoundData> _soundDataDict = new();
        private GameSettings _gameSettings;

        private void Awake()
        {
            _gameSettings = GameLoader.Instance.gameLoaderSettingsContainer.gameSettings;
            UpdateAudioSourceVolumeFromSettings();
        }

        private void Start()
        {


            if (GameLoader.Instance.pauseControl != null)
            {
                GameLoader.Instance.pauseControl.onPauseChanged += PauseUnpauseMusic;
            }
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

        public void UpdateAudioSourceVolumeFromSettings()
        {
            foreach (var kvp in _soundDataDict)
            {
                // Получаем источник звука
                var source = kvp.Value.audioSource;

                // Проверяем тип источника звука
                switch (kvp.Key)
                {
                    case (AudioSourceType.LevelMusic):
                        source.volume = _gameSettings.volumeMusic;
                        break;
                    default:
                        source.volume = _gameSettings.volumeSounds;
                        break;
                }
            }
        }

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

        public void PlayCustomSound(AudioSourceType type, int index = 0, bool randomPitch = false)
        {
            var audiosource = _soundDataDict[type].audioSource;
            var audioclipCollection = _soundDataDict[type].audioClipCollection;

            if (randomPitch)
            {
                audiosource.pitch = (UnityEngine.Random.Range(audiosource.volume - _minRandomDiff, audiosource.volume));
            }

            //Debug.Log(audiosource.volume + "  " + _gameSettings.volumeMusic);
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