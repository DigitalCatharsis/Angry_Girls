using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Settings data structure.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        [Header("Audio")]
        [Range(0, 1)] public float volumeMusic = 0.5f;
        [Range(0, 1)] public float volumeSounds = 0.5f;

        [Header("Camera (User Override)")]
        [Tooltip("User can override platform default")]
        public bool useCustomCameraSettings = false;
        public float cameraMovementSpeed = 0.5f;
        public float cameraZoomSensitivity = 7.0f;

        [Header("Graphics")]
        public bool useCustomGraphicsSettings = false;
        public int targetFPS = 60;
        public int qualityLevel = 3;
        public bool vfxHighQuality = true;

        [Header("Gameplay")]
        public bool enableTutorial = true;
        public bool showDamageNumbers = true;
        public float turnTimerSeconds = 30f;

        [Header("Controls")]
        public bool useTouchControls = false;
        public float swipeThreshold = 50f;
        public float touchDeadzone = 10f;

        [Header("System")]
        public string languageCode = "en";
        public TargetPlatform lastKnownPlatform;
        public string settingsVersion = "1.0";
    }

    /// <summary>
    /// Manages game settings.
    /// </summary>
    public sealed class SettingsManager
    {
        public Action OnSettingsChanged;

        private SettingsData _currentSettingsData = new();
        private PlatformSettingsCatalog _platformCatalog;

        public SettingsData GetSettings() => _currentSettingsData;

        /// <summary>
        /// Initialize with platform catalog reference
        /// </summary>
        public void Init(PlatformSettingsCatalog catalog = null)
        {
            _platformCatalog = catalog;

            if (Repository.LoadState())
            {
                var savedData = Repository.GetData<SettingsData>();
                if (savedData != null)
                {
                    SetupSettings(savedData);
                    Debug.Log("SettingsManager: Loaded settings from Repository.");
                    return;
                }
            }

            ApplyPlatformDefaults();
            Debug.Log("SettingsManager: Using platform defaults.");
        }

        /// <summary>
        /// Apply defaults from current platform profile
        /// </summary>
        private void ApplyPlatformDefaults()
        {
            if (_platformCatalog == null)
            {
                Debug.LogWarning("SettingsManager: PlatformCatalog not assigned, using hardcoded defaults");
                return;
            }

            var profile = _platformCatalog.GetCurrentPlatformProfile();
            if (profile == null) return;

            _currentSettingsData.cameraMovementSpeed = profile.camera.movementSpeed;
            _currentSettingsData.cameraZoomSensitivity = profile.camera.zoomSensitivity;
            _currentSettingsData.targetFPS = profile.graphics.targetFPS;
            _currentSettingsData.qualityLevel = profile.graphics.qualityLevel;
            _currentSettingsData.lastKnownPlatform = profile.platform;
        }

        /// <summary>
        /// Apply settings to runtime systems
        /// </summary>
        public void ApplyRuntimeSettings()
        {
            // FPS
            if (!_currentSettingsData.useCustomGraphicsSettings && _platformCatalog != null)
            {
                var profile = _platformCatalog.GetCurrentPlatformProfile();
                if (profile != null)
                {
                    Application.targetFrameRate = profile.graphics.targetFPS;
                    QualitySettings.SetQualityLevel(profile.graphics.qualityLevel);
                }
            }
            else
            {
                Application.targetFrameRate = _currentSettingsData.targetFPS;
                QualitySettings.SetQualityLevel(_currentSettingsData.qualityLevel);
            }

            // Audio volumes are applied via AudioManager subscription
        }

        public void SaveSettings()
        {
            Repository.SetData(_currentSettingsData);
            Repository.SaveState();
            Debug.Log("SettingsManager: Settings saved directly to Repository.");
        }

        /// <summary>
        /// Apply new settings externally (e.g., from UI sliders or SaveLoadManager).
        /// </summary>
        public void SetupSettings(SettingsData settingsData)
        {
            if (settingsData != null)
            {
                _currentSettingsData = settingsData;
            }
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetupMusicVolume(float value)
        {
            _currentSettingsData.volumeMusic = Mathf.Clamp01(value);
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Set sounds volume.
        /// </summary>
        public void SetupSoundsVolume(float value)
        {
            _currentSettingsData.volumeSounds = Mathf.Clamp01(value);
            OnSettingsChanged?.Invoke();
        }

        public void SetupCameraMovementSpeed(float value)
        {
            _currentSettingsData.cameraMovementSpeed = Mathf.Max(0.1f, value);
            _currentSettingsData.useCustomCameraSettings = true;
            OnSettingsChanged?.Invoke();
        }

        public void SetupCameraZoomSensitivity(float value)
        {
            _currentSettingsData.cameraZoomSensitivity = Mathf.Max(1f, value);
            _currentSettingsData.useCustomCameraSettings = true;
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Reset to platform defaults
        /// </summary>
        public void ResetToPlatformDefaults()
        {
            ApplyPlatformDefaults();
            _currentSettingsData.useCustomCameraSettings = false;
            _currentSettingsData.useCustomGraphicsSettings = false;
            OnSettingsChanged?.Invoke();
        }
    }
}