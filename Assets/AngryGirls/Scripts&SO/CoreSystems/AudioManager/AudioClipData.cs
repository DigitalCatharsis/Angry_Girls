using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Wrapper for AudioClip with stable identifier and per-clip metadata.
    /// Create via: Right-click in Project > Angry_Girls/Audio/AudioClipData
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipData", menuName = "Angry_Girls/Audio/AudioClipData")]
    public class AudioClipData : ScriptableObject
    {
        [Header("Core")]
        [Tooltip("The actual audio clip asset")]
        public AudioClip clip;

        [Header("Playback Overrides (optional)")]
        [Tooltip("Override volume for this clip (1 = use AudioManager default)")]
        [Range(0f, 2f)]
        public float volumeOverride = 1f;

        [Tooltip("Minimum random pitch (1 = no variation)")]
        [Range(0.5f, 2f)]
        public float minPitch = 1f;

        [Tooltip("Maximum random pitch (1 = no variation)")]
        [Range(0.5f, 2f)]
        public float maxPitch = 1f;

        [Tooltip("Category for volume/pause routing (if not set in parent SoundData)")]
        public AudioCategory fallbackCategory = AudioCategory.SFX;

        [Header("Debug")]
        [Tooltip("Notes for designers (not used at runtime)")]
        [TextArea]
        public string developerNote;

        /// <summary>
        /// Gets effective volume (override or default).
        /// </summary>
        public float GetEffectiveVolume(float defaultVolume)
        {
            return Mathf.Approximately(volumeOverride, 1f) ? defaultVolume : volumeOverride;
        }

        /// <summary>
        /// Gets effective pitch range.
        /// </summary>
        public (float min, float max) GetPitchRange()
        {
            return (minPitch, maxPitch);
        }
    }
}