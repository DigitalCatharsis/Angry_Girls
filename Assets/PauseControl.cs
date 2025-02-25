using UnityEngine;

namespace Angry_Girls
{
    public class PauseControl : MonoBehaviour
    {
        public delegate void OnPauseChanged(bool isPaused);
        public event OnPauseChanged onPauseChanged;

        private const float _savedTimeScale = 1f;
        [SerializeField] private bool _isPaused = false;
        public bool IsPaused => _isPaused;
        public void PauseGame()
        {
            Time.timeScale = 0;
            _isPaused = true;

            onPauseChanged?.Invoke(_isPaused);
        }

        public void UnpauseGame()
        {
            Time.timeScale = _savedTimeScale;
            _isPaused = false;

            onPauseChanged?.Invoke(_isPaused);
        }
    }
}