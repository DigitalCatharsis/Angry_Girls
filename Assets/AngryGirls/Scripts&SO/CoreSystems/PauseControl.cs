using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Controls game pause state
    /// </summary>
    public class PauseControl
    {
        public delegate void OnPauseChangedDelegate(bool isPaused);
        public event OnPauseChangedDelegate OnPauseChanged;

        private const float _savedTimeScale = 1f;
        [SerializeField] private bool _isPaused = false;
        public bool IsPaused => _isPaused;

        public void PauseGame()
        {
            Time.timeScale = 0;
            _isPaused = true;

            OnPauseChanged?.Invoke(_isPaused);
        }

        public void UnpauseGame()
        {
            Time.timeScale = _savedTimeScale;
            _isPaused = false;

            OnPauseChanged?.Invoke(_isPaused);
        }
    }
}