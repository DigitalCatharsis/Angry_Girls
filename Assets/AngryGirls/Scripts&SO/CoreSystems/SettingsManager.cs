using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Settings categories for tab navigation.
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

    /// <summary>
    /// Serialized settings data.
    /// </summary>
    public class SettingsSaveData
    {
        [Header("Audio")]
        public bool useCustomAudioSettings;

        [Range(0f, 1f)]
        public float volumeMusic;

        [Range(0f, 1f)]
        public float volumeSounds;

        [Header("Camera")]
        public bool useCustomCameraSettings;
        public float cameraMovementSpeed;

        [Header("Gameplay")]
        public bool? showTurnOrder;

        public SettingsSaveData()
        {
        }

        public SettingsSaveData(
            bool useCustomAudioSettings,
            float volumeMusic,
            float volumeSounds,
            bool useCustomCameraSettings,
            float cameraMovementSpeed,
            bool showTurnOrder = true)
        {
            this.useCustomAudioSettings =
                useCustomAudioSettings;

            this.volumeMusic =
                volumeMusic;

            this.volumeSounds =
                volumeSounds;

            this.useCustomCameraSettings =
                useCustomCameraSettings;

            this.cameraMovementSpeed =
                cameraMovementSpeed;

            this.showTurnOrder =
                showTurnOrder;
        }

        /// <summary>
        /// Converts serialized settings into runtime settings.
        /// </summary>
        public SettingsData ReinitToSettingsData()
        {
            var resolvedShowTurnOrder =
                showTurnOrder ?? true;

            return new SettingsData(
                useCustomAudioSettings,
                volumeMusic,
                volumeSounds,
                useCustomCameraSettings,
                cameraMovementSpeed,
                resolvedShowTurnOrder);
        }
    }

    /// <summary>
    /// Runtime settings data.
    /// </summary>
    [Serializable]
    public struct SettingsData
    {
        [Header("Audio")]
        public bool useCustomAudioSettings;

        [Range(0f, 1f)]
        public float volumeMusic;

        [Range(0f, 1f)]
        public float volumeSounds;

        [Header("Camera")]
        public bool useCustomCameraSettings;
        public float cameraMovementSpeed;

        [Header("Gameplay")]
        public bool showTurnOrder;

        public SettingsData(
            bool useCustomAudioSettings,
            float volumeMusic,
            float volumeSounds,
            bool useCustomCameraSettings,
            float cameraMovementSpeed,
            bool showTurnOrder = true)
        {
            this.useCustomAudioSettings =
                useCustomAudioSettings;

            this.volumeMusic =
                volumeMusic;

            this.volumeSounds =
                volumeSounds;

            this.useCustomCameraSettings =
                useCustomCameraSettings;

            this.cameraMovementSpeed =
                cameraMovementSpeed;

            this.showTurnOrder =
                showTurnOrder;
        }
    }

    /// <summary>
    /// Manages game settings.
    /// </summary>
    public sealed class SettingsManager
    {
        public Action<SettingsCategory> OnSettingsChanged;

        private SettingsData _currentSettingsData =
            new SettingsData();

        private SettingsSaveData _settingsSaveData =
            new SettingsSaveData();

        private PlatformSettingsCatalog _platformSettingsCatalog;
        private PlatformProfile _platformProfile;

        /// <summary>
        /// Gets current runtime settings.
        /// </summary>
        public SettingsData GetCurrentSettings() =>
            _currentSettingsData;

        /// <summary>
        /// Initializes settings from save data or platform defaults.
        /// </summary>
        public void Init(
            PlatformSettingsCatalog catalog)
        {
            if (catalog == null)
            {
                Debug.LogError(
                    "SettingsManager: PlatformSettingsCatalog is null.");

                return;
            }

            _platformSettingsCatalog = catalog;

            _platformProfile =
                _platformSettingsCatalog
                    .GetCurrentPlatformProfile();

            if (_platformProfile == null)
            {
                Debug.LogError(
                    "SettingsManager: Current platform profile is null.");

                return;
            }

            Repository.LoadState();

            var savedData =
                Repository.GetData<SettingsSaveData>();

            if (savedData != null)
            {
                SetupSettings(
                    savedData.ReinitToSettingsData());

                Debug.Log(
                    "SettingsManager: Loaded settings from Repository.");

                return;
            }

            ApplyPlatformDefaults(
                SettingsCategory.All);

            Debug.Log(
                "SettingsManager: Using platform defaults.");
        }

        /// <summary>
        /// Applies platform defaults to the selected category.
        /// </summary>
        public void ApplyPlatformDefaults(
            SettingsCategory settingsCategory)
        {
            if (_platformProfile == null)
            {
                if (_platformSettingsCatalog == null)
                    return;

                _platformProfile =
                    _platformSettingsCatalog
                        .GetCurrentPlatformProfile();

                if (_platformProfile == null)
                    return;
            }

            switch (settingsCategory)
            {
                case SettingsCategory.All:
                    LoadDefaultCameraValues();
                    LoadDefaultAudioValues();
                    LoadDefaultGameplayValues();
                    break;

                case SettingsCategory.Audio:
                    LoadDefaultAudioValues();
                    break;

                case SettingsCategory.Camera:
                    LoadDefaultCameraValues();
                    break;

                case SettingsCategory.Gameplay:
                    LoadDefaultGameplayValues();
                    break;

                case SettingsCategory.Graphics:
                case SettingsCategory.Controls:
                case SettingsCategory.System:
                default:
                    break;
            }

            OnSettingsChanged?.Invoke(
                settingsCategory);
        }

        private void LoadDefaultCameraValues()
        {
            _currentSettingsData.cameraMovementSpeed =
                _platformProfile.camera.movementSpeed;

            _currentSettingsData.useCustomCameraSettings =
                false;
        }

        private void LoadDefaultAudioValues()
        {
            _currentSettingsData.volumeMusic =
                _platformProfile.audio.volumeMusic;

            _currentSettingsData.volumeSounds =
                _platformProfile.audio.volumeSounds;

            _currentSettingsData.useCustomAudioSettings =
                false;
        }

        private void LoadDefaultGameplayValues()
        {
            _currentSettingsData.showTurnOrder =
                true;
        }

        /// <summary>
        /// Saves current settings to the repository.
        /// </summary>
        public void SaveSettings()
        {
            SetupSaveData(
                _currentSettingsData);

            Repository.SetData(
                _settingsSaveData);

            Repository.SaveState();

            Debug.Log(
                "SettingsManager: Settings saved directly to Repository.");
        }

        private void SetupSaveData(
            SettingsData settingsData)
        {
            _settingsSaveData.volumeMusic =
                settingsData.volumeMusic;

            _settingsSaveData.volumeSounds =
                settingsData.volumeSounds;

            _settingsSaveData.useCustomAudioSettings =
                settingsData.useCustomAudioSettings;

            _settingsSaveData.cameraMovementSpeed =
                settingsData.cameraMovementSpeed;

            _settingsSaveData.useCustomCameraSettings =
                settingsData.useCustomCameraSettings;

            _settingsSaveData.showTurnOrder =
                settingsData.showTurnOrder;
        }

        /// <summary>
        /// Applies runtime settings immediately.
        /// </summary>
        public void SetupSettings(
            SettingsData settingsData)
        {
            _currentSettingsData =
                settingsData;

            OnSettingsChanged?.Invoke(
                SettingsCategory.All);
        }

        /// <summary>
        /// Sets music volume.
        /// </summary>
        public void SetupMusicVolume(
            float value)
        {
            _currentSettingsData.volumeMusic =
                Mathf.Clamp01(value);

            _currentSettingsData.useCustomAudioSettings =
                true;

            OnSettingsChanged?.Invoke(
                SettingsCategory.Audio);
        }

        /// <summary>
        /// Sets sound volume.
        /// </summary>
        public void SetupSoundsVolume(
            float value)
        {
            _currentSettingsData.volumeSounds =
                Mathf.Clamp01(value);

            _currentSettingsData.useCustomAudioSettings =
                true;

            OnSettingsChanged?.Invoke(
                SettingsCategory.Audio);
        }

        /// <summary>
        /// Sets camera movement speed.
        /// </summary>
        public void SetupCameraMovementSpeed(
            float value)
        {
            _currentSettingsData.cameraMovementSpeed =
                Mathf.Clamp01(value);

            if (_currentSettingsData.cameraMovementSpeed <= 0f)
                _currentSettingsData.cameraMovementSpeed = 0.1f;

            _currentSettingsData.useCustomCameraSettings =
                true;

            OnSettingsChanged?.Invoke(
                SettingsCategory.Camera);
        }

        /// <summary>
        /// Sets turn order visibility.
        /// </summary>
        public void SetupShowTurnOrder(
            bool value)
        {
            _currentSettingsData.showTurnOrder =
                value;

            OnSettingsChanged?.Invoke(
                SettingsCategory.Gameplay);
        }
    }
}