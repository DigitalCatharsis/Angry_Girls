using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Orchestrates all gameplay UI components.
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
        [SerializeField] private LaunchPhaseProgressUI _launchPhaseProgressUI;
        [SerializeField] private UI_RewardPresentation _uiRewardPresentation;

        private UI_GameplayManagersComponent[] _uI_GameplayScreens;

        private bool _isInitialized;

        private GameLogic _gameLogic;

        public override void Initialize()
        {
            if (_isInitialized)
                return;

            StartCoroutine(
                WaitForGameplayCore());
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
            _uI_GameplayScreens =
                GetComponentsInChildren<
                    UI_GameplayManagersComponent>(
                    true);

            foreach (var screen in _uI_GameplayScreens)
            {
                if (screen == null)
                    continue;

                screen.Initialize();
            }

            _gameLogic =
                GameplayCoreManager.Instance.GameLogic;

            if (_gameLogic == null)
                return;

            if (_gameResultUI != null)
            {
                _gameLogic.OnGameOver +=
                    _gameResultUI.ShowGameOver;

                _gameLogic.OnVictory +=
                    _gameResultUI.ShowVictory;
            }

            _gameLogic.OnRewardStart +=
                ShowReward;
        }

        public void ShowReward()
        {
            if (_uiRewardPresentation == null)
                return;

            DisableAllUIButOne(
                _uiRewardPresentation);

            if (_scoreDisplay != null)
            {
                _uiRewardPresentation
                    .ShowAndGrantRewardAsync(
                        _scoreDisplay.GetScore())
                    .Forget();
            }
        }

        private void DisableAllUIButOne(
            UI_GameplayManagersComponent uiScreen)
        {
            if (_uI_GameplayScreens == null)
                return;

            foreach (var screen in _uI_GameplayScreens)
            {
                if (screen == null ||
                    screen == uiScreen)
                {
                    continue;
                }

                screen.gameObject.SetActive(false);
            }
        }

        public override void Show()
        {
            base.Show();

            if (!_isInitialized)
                return;

            _scoreDisplay?.Show();
            _charactersPanel?.Show();
            _pauseMenu?.Show();
            _launchPhaseProgressUI?.Show();

            _gameResultUI?.Hide();
        }

        private void OnDestroy()
        {
            if (_gameLogic == null)
                return;

            if (_gameResultUI != null)
            {
                _gameLogic.OnGameOver -=
                    _gameResultUI.ShowGameOver;

                _gameLogic.OnVictory -=
                    _gameResultUI.ShowVictory;
            }

            _gameLogic.OnRewardStart -=
                ShowReward;
        }
    }
}