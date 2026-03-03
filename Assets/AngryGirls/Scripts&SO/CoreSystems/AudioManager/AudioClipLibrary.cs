using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Collection of AudioClipData entries with stable lookup methods.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipLibrary", menuName = "Angry_Girls/Audio/AudioClipLibrary")]
    public class AudioClipLibrary : ScriptableObject
    {
        [Tooltip("List of audio clip entries with metadata")]
        public List<AudioClipData> clips;

        /// <summary>
        /// Get random clip entry from library.
        /// </summary>
        public AudioClipData GetRandomClipData()
        {
            if (clips == null || clips.Count == 0)
            {
                Debug.LogWarning($"AudioClipLibrary '{name}': No clips available.");
                return null;
            }
            return clips[Random.Range(0, clips.Count)];
        }

        ///// <summary>
        ///// Get clip by index (legacy, use only for initial setup).
        ///// </summary>
        //[System.Obsolete("Use GetClipByStableId() for stable references")]
        //public AudioClipData GetClipByIndex(int index)
        //{
        //    if (clips == null || index < 0 || index >= clips.Count)
        //    {
        //        Debug.LogWarning($"AudioClipLibrary '{name}': Index {index} out of range.");
        //        return null;
        //    }
        //    return clips[index];
        //}

        /// <summary>
        /// Get total number of clips.
        /// </summary>
        public int GetClipCount()
        {
            return clips?.Count ?? 0;
        }
    }
}