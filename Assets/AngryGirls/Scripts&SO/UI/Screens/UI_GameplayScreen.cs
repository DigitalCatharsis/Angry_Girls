using Cysharp.Threading.Tasks;
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
        [SerializeField] private UI_RewardPresentation _uiRewardPresentation;

        private UI_GameplayManagersComponent[] _uI_GameplayScreens;

        private bool _isInitialized;

        private GameLogic _gameLogic;

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
            _uI_GameplayScreens = GetComponentsInChildren<UI_GameplayManagersComponent>();
            foreach (var screen in _uI_GameplayScreens)
            {
                screen.Initialize();
            }
            //_scoreDisplay?.Initialize();
            //_charactersPanel?.Initialize();
            //_pauseMenu?.Initialize();
            //_tutorialSystem?.Initialize();
            //_gameResultUI?.Initialize();
            //_trajectoryCheatToggle?.Initialize();
            //_uiRewardPresentation?.Initialize();

            _gameLogic = GameplayCoreManager.Instance.GameLogic;
            _gameLogic.OnGameOver += _gameResultUI.ShowGameOver;
            _gameLogic.OnVictory += _gameResultUI.ShowVictory;
            _gameLogic.OnRewardStart += ShowReward;
        }

        public void ShowReward()
        {
            DisableAllUIButOne(_uiRewardPresentation);
            _uiRewardPresentation.ShowAndGrantRewardAsync(_scoreDisplay.GetScore()).Forget();
        }

        private void DisableAllUIButOne(UI_GameplayManagersComponent UIScreen)
        {
            foreach (var screen in _uI_GameplayScreens)
            {
                if (screen != UIScreen)
                {
                    screen.gameObject.SetActive(false);
                    Debug.Log("[UI_GameplayScreen]: Disabling " +  screen.gameObject.name);
                }
            }
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
    }
}