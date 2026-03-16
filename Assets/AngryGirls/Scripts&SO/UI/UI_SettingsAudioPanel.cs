using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Audio settings category panel
    /// </summary>
    public class UI_SettingsAudioPanel : MonoBehaviour, ISettingsCategoryPanel
    {
        [Header("Audio Controls")]
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Button _muteAllButton;

        private SettingsManager _settingsManager;
        private bool _isInitializing = false;

        public SettingsCategory Category => SettingsCategory.Audio;

        public void Initialize(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            SetupListeners();
        }

        private void SetupListeners()
        {
            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

            if (_muteAllButton != null)
                _muteAllButton.onClick.AddListener(OnMuteAllPressed);
        }

        public void LoadValues()
        {
            _isInitializing = true;
            var settings = _settingsManager.GetCurrentSettings();

            if (_musicVolumeSlider != null)
                _musicVolumeSlider.value = settings.volumeMusic;

            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.value = settings.volumeSounds;

            _isInitializing = false;
        }

        public void SaveValues()
        {
            _settingsManager.SaveSettings();
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_isInitializing) return;
            _settingsManager.SetupMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_isInitializing) return;
            _settingsManager.SetupSoundsVolume(value);
        }

        private void OnMuteAllPressed()
        {
            _settingsManager.SetupMusicVolume(0f);
            _settingsManager.SetupSoundsVolume(0f);
            LoadValues();
        }

        private void OnDestroy()
        {
            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (_muteAllButton != null)
                _muteAllButton.onClick.RemoveListener(OnMuteAllPressed);
        }
    }
}