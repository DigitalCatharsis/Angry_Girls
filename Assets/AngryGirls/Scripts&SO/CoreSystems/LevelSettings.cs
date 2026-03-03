using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Music configuration for a specific level or scene.
    /// Assign to LevelMusicController in Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Angry_Girls/Audio/LevelSettings")]
    public class LevelSettings : ScriptableObject
    {
        [Header("Background Music")]
        [Tooltip("Music library played during level gameplay")]
        public AudioClipData backgroundMusicData;

        [Header("Optional: Victory Music")]
        [Tooltip("Music played on level completion (optional)")]
        public AudioClipData victoryMusicLibrary;

        [Header("Optional: Defeat Music")]
        [Tooltip("Music played on level failure (optional)")]
        public AudioClipData defeatMusicLibrary;

        [Header("Settings")]
        [Tooltip("Fade in duration on scene load (seconds)")]
        public float fadeInDuration = 1f;

        [Tooltip("Fade out duration on scene unload (seconds)")]
        public float fadeOutDuration = 0.5f;
    }
}