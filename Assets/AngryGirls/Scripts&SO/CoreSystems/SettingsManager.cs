using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Settings categories for tab navigation
    /// </summary>
    public enum SettingsCategory
    {
        Audio = 0,
        Camera = 1,
        Graphics = 2,
        Gameplay = 3,
        Controls = 4,
        System = 5,
        All = 99,
    }

    public class SettingsSaveData
    {
        [Header("Audio")]
        public bool useCustomAudioSettings;
        [Range(0, 1)] public float volumeMusic;
        [Range(0, 1)] public float volumeSounds;

        [Header("Camera (User Override)")]
        [Tooltip("User can override platform default")]
        [Range(0, 1)] public bool useCustomCameraSettings;
        public float cameraMovementSpeed;

        public SettingsSaveData() { }

        public SettingsSaveData(bool useCustomAudioSettings, float volumeMusic, float volumeSounds, bool useCustomCameraSettings, float cameraMovementSpeed)
        {
            this.useCustomAudioSettings = useCustomAudioSettings;
            this.volumeMusic = volumeMusic;
            this.volumeSounds = volumeSounds;
            this.useCustomCameraSettings = useCustomCameraSettings;
            this.cameraMovementSpeed = cameraMovementSpeed;
        }

        public SettingsData ReinitToSettingsData()
        {
            return new SettingsData(useCustomAudioSettings, volumeMusic, volumeSounds, useCustomCameraSettings, cameraMovementSpeed);
        }
    }

    /// <summary>
    /// Settings data structure.
    /// </summary>
    [Serializable]
    public struct SettingsData
    {
        [Header("Audio")]
        public bool useCustomAudioSettings;
        [Range(0, 1)] public float volumeMusic;
        [Range(0, 1)] public float volumeSounds;

        [Header("Camera (User Override)")]
        [Tooltip("User can override platform default")]
        public bool useCustomCameraSettings;
        public float cameraMovementSpeed;

        public SettingsData(bool useCustomAudioSettings, float volumeMusic, float volumeSounds, bool useCustomCameraSettings, float cameraMovementSpeed)
        {
            this.useCustomAudioSettings = useCustomAudioSettings;
            this.volumeMusic = volumeMusic;
            this.volumeSounds = volumeSounds;
            this.useCustomCameraSettings = useCustomCameraSettings;
            this.cameraMovementSpeed = cameraMovementSpeed;
        }
    }

    /// <summary>
    /// Manages game settings.
    /// </summary>
    public sealed class SettingsManager
    {
        public Action<SettingsCategory> OnSettingsChanged;

        private SettingsData _currentSettingsData = new();

        private PlatformSettingsCatalog _platformSettingsCatalog;
        public SettingsData GetCurrentSettings() => _currentSettingsData;

        private SettingsSaveData _settingsSaveData = new();

        private PlatformProfile _platformProfile;

        /// <summary>
        /// Initialize with platform catalog reference
        /// </summary>
        public void Init(PlatformSettingsCatalog catalog)
        {
            _platformSettingsCatalog = catalog;
            _platformProfile = _platformSettingsCatalog.GetCurrentPlatformProfile();

            Repository.LoadState();

            var savedData = Repository.GetData<SettingsSaveData>();
            if (savedData != null)
            {
                SetupSettings(savedData.ReinitToSettingsData());
                Debug.Log("SettingsManager: Loaded settings from Repository.");
                return;
            }

            ApplyPlatformDefaults(SettingsCategory.All);
            Debug.Log("SettingsManager: Using platform defaults.");
        }

        /// <summary>
        /// Apply defaults from current platform profile
        /// </summary>
        public void ApplyPlatformDefaults(SettingsCategory settingsCategory)
        {
            if (_platformProfile == null)
            {
                _platformProfile = _platformSettingsCatalog.GetCurrentPlatformProfile();
            }

            switch (settingsCategory)
            {
                case SettingsCategory.All:
                    LoadDefaultCameraValues();
                    LoadDefaultAudioValues();
                    break;
                case SettingsCategory.Audio:
                    LoadDefaultAudioValues();
                    break;
                case SettingsCategory.Camera:
                    LoadDefaultCameraValues();
                    break;
                case SettingsCategory.Graphics:
                    break;
                case SettingsCategory.Gameplay:
                    break;
                case SettingsCategory.Controls:
                    break;
                case SettingsCategory.System:
                    break;
                default:
                    break;
            }
            OnSettingsChanged?.Invoke(settingsCategory);
        }

        private void LoadDefaultCameraValues()
        {
            _currentSettingsData.cameraMovementSpeed = _platformProfile.camera.movementSpeed;
            _currentSettingsData.useCustomCameraSettings = false;
        }

        private void LoadDefaultAudioValues()
        {
            _currentSettingsData.volumeMusic = _platformProfile.audio.volumeMusic;
            _currentSettingsData.volumeSounds = _platformProfile.audio.volumeSounds;
            _currentSettingsData.useCustomAudioSettings = false;
        }

        public void SaveSettings()
        {

            SetupSaveData(_currentSettingsData);
            Repository.SetData(_settingsSaveData);
            Repository.SaveState();
            Debug.Log("SettingsManager: Settings saved directly to Repository.");
        }

        private void SetupSaveData(SettingsData settingsData)
        {
            _settingsSaveData.volumeMusic = settingsData.volumeMusic;
            _settingsSaveData.volumeSounds = settingsData.volumeSounds;
            _settingsSaveData.useCustomAudioSettings = settingsData.useCustomAudioSettings;
            _settingsSaveData.cameraMovementSpeed = settingsData.cameraMovementSpeed;
            _settingsSaveData.useCustomCameraSettings = settingsData.useCustomCameraSettings;
        }

        public void SetupSettings(SettingsData settingsData)
        {
            _currentSettingsData = settingsData;
            OnSettingsChanged?.Invoke(SettingsCategory.All);
        }

        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetupMusicVolume(float value)
        {
            _currentSettingsData.volumeMusic = Mathf.Clamp01(value);
            _currentSettingsData.useCustomAudioSettings = true;
            OnSettingsChanged?.Invoke(SettingsCategory.Audio);
        }

        /// <summary>
        /// Set sounds volume.
        /// </summary>
        public void SetupSoundsVolume(float value)
        {
            _currentSettingsData.volumeSounds = Mathf.Clamp01(value);
            _currentSettingsData.useCustomAudioSettings = true;
            OnSettingsChanged?.Invoke(SettingsCategory.Audio);
        }

        public void SetupCameraMovementSpeed(float value)
        {
            _currentSettingsData.cameraMovementSpeed = Mathf.Clamp01(value);
            if (_currentSettingsData.cameraMovementSpeed == 0) { _currentSettingsData.cameraMovementSpeed += 0.1f; }
            _currentSettingsData.useCustomCameraSettings = true;
            OnSettingsChanged?.Invoke(SettingsCategory.Camera);
        }
    }
}