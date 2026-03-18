using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{
    /// <summary>
    /// Scene-local controller that manages background music for a specific level.
    /// Place one instance in each level scene. Assign LevelSettings in Inspector.
    /// </summary>
    public class LevelMusicController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Music settings for this level")]
        [SerializeField] private LevelSettings _levelSettings;

        private AudioManager _audioManager;
        private bool _isMusicPlaying = false;

        /// <summary>
        /// Initialize on scene load. Start background music.
        /// </summary>
        private void OnEnable()
        {
            if (CoreManager.Instance == null || CoreManager.Instance.AudioManager == null)
            {
                Debug.LogWarning("LevelMusicController: CoreManager or AudioManager not found.");
                return;
            }

            _audioManager = CoreManager.Instance.AudioManager;

            if (_levelSettings == null)
            {
                Debug.LogWarning($"LevelMusicController: No LevelSettings assigned in '{gameObject.name}'. Skipping music.");
                return;
            }

            if (_levelSettings.backgroundMusicData == null)
            {
                Debug.LogWarning($"LevelMusicController: No background music library in LevelSettings for '{gameObject.name}'. Skipping music.");
                return;
            }

            // Start background music with fade-in
            StartBackgroundMusicAsync().Forget();
        }

        ///// <summary>
        ///// Stop music on scene unload.
        ///// </summary>
        //private void OnDisable()
        //{
        //    if (!_isMusicPlaying) return;

        //    StopBackgroundMusicAsync();
        //}

        /// <summary>
        /// Start background music with optional fade-in.
        /// </summary>
        private async UniTaskVoid StartBackgroundMusicAsync()
        {
            if (_audioManager == null || _levelSettings == null) return;

            // Fade out any existing music first (smooth transition)
            await _audioManager.FadeOutMusicAsync(_levelSettings.fadeOutDuration);

            // Play new music
            _audioManager.PlayMusicAsync(_levelSettings.backgroundMusicData).Forget();

            _isMusicPlaying = true;

            // Fade in new music
            if (_levelSettings.fadeInDuration > 0f)
            {
                await _audioManager.FadeInMusicAsync(_levelSettings.fadeInDuration);
            }

            Debug.Log($"LevelMusicController: Started background music for '{_levelSettings.name}'");
        }

        /// <summary>
        /// Stop background music with optional fade-out.
        /// </summary>
        private void StopBackgroundMusicAsync()
        {
            if (_audioManager == null || _levelSettings == null) return;

            // Fade out before stopping
            if (_levelSettings.fadeOutDuration > 0f)
            {
                _audioManager.FadeOutMusicAsync(_levelSettings.fadeOutDuration).Forget();
            }

            _audioManager.StopMusic();
            _isMusicPlaying = false;

            Debug.Log($"LevelMusicController: Stopped background music for '{_levelSettings.name}'");
        }

        /// <summary>
        /// Play victory music (called on level completion).
        /// </summary>
        public void PlayVictoryMusic()
        {
            if (_levelSettings?.victoryMusicLibrary == null) return;
            _audioManager?.PlayMusicAsync(_levelSettings.victoryMusicLibrary, 0).Forget();
        }

        /// <summary>
        /// Play defeat music (called on level failure).
        /// </summary>
        public void PlayDefeatMusic()
        {
            if (_levelSettings?.defeatMusicLibrary == null) return;
            _audioManager?.PlayMusicAsync(_levelSettings.defeatMusicLibrary, 0).Forget();
        }
    }
}