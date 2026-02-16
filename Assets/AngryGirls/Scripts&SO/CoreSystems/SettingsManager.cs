using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Settings data structure
    /// </summary>
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
    /// Manages game settings
    /// </summary>
    public sealed class SettingsManager
    {
        public event Action OnSettingsChanged;

        private SettingsData _currentSettingsData = new();

        /// <summary>
        /// Initialize settings from repository
        /// </summary>
        public void Init()
        {
            //if (Repository.TryGetData<SettingsData>(out var data))
            //{
            //    SetupSettings(data);
            //}
        }

        /// <summary>
        /// Apply new settings
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
        /// Set music volume
        /// </summary>
        public void SetupMusicVolume(float value)
        {
            _currentSettingsData.volumeMusic = Mathf.Clamp01(value);
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Set sounds volume
        /// </summary>
        public void SetupSoundsVolume(float value)
        {
            _currentSettingsData.volumeSounds = Mathf.Clamp01(value);
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Get current settings
        /// </summary>
        public SettingsData GetSettings()
        {
            return _currentSettingsData;
        }
    }
}