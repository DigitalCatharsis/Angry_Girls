using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Target platforms for configuration profiles
    /// </summary>
    public enum TargetPlatform
    {
        Windows,
        WebGL,
        Android,
        iOS,
        Default
    }

    [Serializable]
    public class AudioPlatformSettngs
    {
        [Range(0, 1)] public float volumeMusic = 0.5f;
        [Range(0, 1)] public float volumeSounds = 0.5f;
    }

    /// <summary>
    /// Platform-specific camera configuration
    /// </summary>
    [Serializable]
    public class CameraPlatformSettings
    {
        [Tooltip("Camera panning speed (units per second)")]
        [Range(0, 1)] public float movementSpeed = 0.5f;
    }

    /// <summary>
    /// Complete platform configuration profile
    /// </summary>
    [Serializable]
    public class PlatformProfile
    {
        public TargetPlatform platform;
        public AudioPlatformSettngs audio = new();
        public CameraPlatformSettings camera = new();
    }

    /// <summary>
    /// ScriptableObject container for all platform profiles
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformSettingsCatalog", menuName = "Angry_Girls/Settings/PlatformSettingsCatalog")]
    public class PlatformSettingsCatalog : ScriptableObject
    {
        [SerializeField] private PlatformProfile[] _profiles;

        /// <summary>
        /// Get profile for current runtime platform
        /// </summary>
        public PlatformProfile GetCurrentPlatformProfile()
        {
            var target = GetTargetPlatformFromRuntime();
            return GetProfileForPlatform(target);
        }

        /// <summary>
        /// Get profile for specific platform
        /// </summary>
        public PlatformProfile GetProfileForPlatform(TargetPlatform platform)
        {
            if (_profiles == null) return null;

            var profile = Array.Find(_profiles, p => p.platform == platform);
            if (profile != null) return profile;

            // Fallback to Default
            return Array.Find(_profiles, p => p.platform == TargetPlatform.Default);
        }

        private TargetPlatform GetTargetPlatformFromRuntime()
        {
        #if UNITY_ANDROID
            return TargetPlatform.Android;
        #elif UNITY_WEBGL
                return TargetPlatform.WebGL;
        #elif UNITY_STANDALONE_WIN
                return TargetPlatform.Windows;
        #elif UNITY_IOS
                return TargetPlatform.iOS;
        #else
                return TargetPlatform.Default;
        #endif
        }
    }
}