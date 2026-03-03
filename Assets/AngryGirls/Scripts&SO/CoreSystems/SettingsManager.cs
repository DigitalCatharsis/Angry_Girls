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
        [Range(0, 1)]
        public float volumeMusic;
        [Range(0, 1)]
        public float volumeSounds;

        public SettingsData()
        {
            volumeMusic = 0.5f;
            volumeSounds = 0.5f;
        }
    }

    /// <summary>
    /// Manages game settings.
    /// </summary>
    public sealed class SettingsManager
    {
        public event Action OnSettingsChanged;
        private SettingsData _currentSettingsData = new();

        public void SaveSettings()
        {
            Repository.SetData(_currentSettingsData);
            Repository.SaveState();
            Debug.Log("SettingsManager: Settings saved directly to Repository.");
        }

        /// <summary>
        /// Initialize settings. No Repository load here - handled by SaveLoadManager.
        /// Defaults are provided by SettingsData constructor.
        /// </summary>
        public void Init()
        {
            // Try load from Repository immediately on app start
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
            Debug.Log("SettingsManager: Using default settings.");
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

        /// <summary>
        /// Get current settings data.
        /// Used by SaveLoadManager save delegate.
        /// </summary>
        public SettingsData GetSettings()
        {
            return _currentSettingsData;
        }
    }
}