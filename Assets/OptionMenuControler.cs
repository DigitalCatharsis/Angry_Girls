using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class OptionMenuControler : MonoBehaviour
    {
        [SerializeField] private GameSettings _gameSettings;

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _soundsSlider;

        private void OnEnable()
        {
            _gameSettings = GameLoader.Instance.gameLoaderSettingsContainer.gameSettings;

            _musicSlider.value = _gameSettings.volumeMusic;
            _soundsSlider.value = _gameSettings.volumeSounds;

            //Adds a listener to the main slider and invokes a method when the value changes.
            _musicSlider.onValueChanged.AddListener(delegate { UpdateGameSettingsMusicValue(); });
            _musicSlider.onValueChanged.AddListener(delegate { GameLoader.Instance.audioManager.UpdateAudioSourceVolume(AudioSourceType.LevelMusic, _musicSlider.value); });

            _soundsSlider.onValueChanged.AddListener(delegate { UpdateGameSettingsSoundsValue(); });
            _soundsSlider.onValueChanged.AddListener(delegate { GameLoader.Instance.audioManager.UpdateAudioSourceVolume(AudioSourceType.CharacterHit, _soundsSlider.value); });
        }

        private void UpdateGameSettingsMusicValue()
        {
            _gameSettings.volumeMusic = _musicSlider.value;
        }

        private void UpdateGameSettingsSoundsValue()
        {
            _gameSettings.volumeSounds = _soundsSlider.value;
        }
    }
}