using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Orchestrates all gameplay UI components
    /// </summary>
    public class UI_GameplayScreen : UI_UIScreen
    {
        [Header("UI Components")]
        [SerializeField] private ScoreDisplay _scoreDisplay;
        [SerializeField] private UI_GameplayCharactersPanel _charactersPanel;
        [SerializeField] private PauseMenu _pauseMenu;
        [SerializeField] private TutorialSystem _tutorialSystem;
        [SerializeField] private GameResultUI _gameResultUI;
        [SerializeField] private TrajectoryCheatToggle _trajectoryCheatToggle;

        private bool _isInitialized;

        public override void Initialize()
        {
            if (_isInitialized) return;

            StartCoroutine(WaitForGameplayCore());
        }

        private IEnumerator WaitForGameplayCore()
        {
            while (GameplayCoreManager.Instance == null)
                yield return null;

            InitializeComponents();
            _isInitialized = true;
        }

        private void InitializeComponents()
        {
            _scoreDisplay?.Initialize();
            _charactersPanel?.Initialize();
            _pauseMenu?.Initialize();
            _tutorialSystem?.Initialize();
            _gameResultUI?.Initialize();
            _trajectoryCheatToggle?.Initialize();

            var gameLogic = GameplayCoreManager.Instance.GameLogic;
            gameLogic.OnGameOver += _gameResultUI.ShowGameOver;
            gameLogic.OnVictory += _gameResultUI.ShowVictory;
        }

        public override void Show()
        {
            base.Show();
            if (_isInitialized)
            {
                _scoreDisplay?.Show();
                _charactersPanel?.Show();
                _pauseMenu?.Show();
                _gameResultUI?.Hide(); 
            }
        }

        public void UpdateScore(int value) => _scoreDisplay?.AddScore(value);
        public void ShowTutorial() => _tutorialSystem?.Show();
        public void TogglePause() => _pauseMenu?.TogglePause();
    }
}