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
        [SerializeField] private HealthBarManager healthBarManager;
        [SerializeField] private ScoreDisplay scoreDisplay;
        [SerializeField] private UI_GameplayCharactersPanel charactersPanel;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private TutorialSystem tutorialSystem;
        [SerializeField] private GameResultUI gameResultUI;
        [SerializeField] private TrajectoryCheatToggle trajectoryCheatToggle;

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
            healthBarManager?.Initialize();
            scoreDisplay?.Initialize();
            charactersPanel?.Initialize();
            pauseMenu?.Initialize();
            tutorialSystem?.Initialize();
            gameResultUI?.Initialize();
            trajectoryCheatToggle?.Initialize();

            var gameLogic = GameplayCoreManager.Instance.GameLogic;
            gameLogic.OnGameOver += gameResultUI.ShowGameOver;
            gameLogic.OnVictory += gameResultUI.ShowVictory;
        }

        public override void Show()
        {
            base.Show();
            if (_isInitialized)
            {
                healthBarManager?.Show();
                scoreDisplay?.Show();
                charactersPanel?.Show();
                pauseMenu?.Show();
                gameResultUI?.Hide(); 
            }
        }

        public void UpdateScore(int value) => scoreDisplay?.AddScore(value);
        public void ShowTutorial() => tutorialSystem?.Show();
        public void TogglePause() => pauseMenu?.TogglePause();
    }
}