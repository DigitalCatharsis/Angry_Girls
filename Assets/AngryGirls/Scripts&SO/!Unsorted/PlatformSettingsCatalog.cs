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

    /// <summary>
    /// Platform-specific camera configuration
    /// </summary>
    [Serializable]
    public class CameraPlatformSettings
    {
        [Tooltip("Camera panning speed (units per second)")]
        public float movementSpeed = 0.5f;

        [Tooltip("Zoom sensitivity multiplier")]
        public float zoomSensitivity = 7.0f;

        [Tooltip("Minimum orthographic size")]
        public float minZoom = 1f;

        [Tooltip("Maximum orthographic size")]
        public float maxZoom = 10f;

        [Tooltip("Touch drag deadzone in pixels")]
        public float touchDeadzone = 10f;
    }

    /// <summary>
    /// Platform-specific input configuration
    /// </summary>
    [Serializable]
    public class InputPlatformSettings
    {
        [Tooltip("Swipe detection threshold")]
        public float swipeThreshold = 50f;

        [Tooltip("Long press duration in seconds")]
        public float longPressDuration = 0.5f;

        [Tooltip("UI button scale multiplier for touch")]
        public float uiScaleMultiplier = 1.0f;
    }

    /// <summary>
    /// Platform-specific graphics configuration
    /// </summary>
    [Serializable]
    public class GraphicsPlatformSettings
    {
        [Tooltip("Target frames per second")]
        public int targetFPS = 60;

        [Tooltip("Unity quality level index")]
        public int qualityLevel = 3;

        [Tooltip("Maximum active particles")]
        public int maxParticles = 500;

        [Tooltip("Shadow distance in units")]
        public float shadowDistance = 50f;
    }

    /// <summary>
    /// Complete platform configuration profile
    /// </summary>
    [Serializable]
    public class PlatformProfile
    {
        public TargetPlatform platform;
        public CameraPlatformSettings camera = new();
        public InputPlatformSettings input = new();
        public GraphicsPlatformSettings graphics = new();
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