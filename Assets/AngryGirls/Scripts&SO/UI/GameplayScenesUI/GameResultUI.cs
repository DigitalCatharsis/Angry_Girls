using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Manages victory and game over UI screens
    /// </summary>
    public class GameResultUI : UI_GameplayManagersComponent
    {
        [SerializeField] private UI_RewardPresentation _rewardPresentation;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _gameOverPanel;

        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _continueButton;

        private MissionsManager _missionsManager;
        private StageManager _stageManager;
        private RewardService _rewardService;

        public override void Initialize()
        {
            base.Initialize();

            _missionsManager = CoreManager.Instance.MissionsManager;
            _stageManager = GameplayCoreManager.Instance.StageManager;

            // Initialize RewardService
            _rewardService = new RewardService(
                CoreManager.Instance.InventoryManager,
                CoreManager.Instance.CharactersManager,
                CoreManager.Instance.CreditsManager,
                CoreManager.Instance.ItemSettingsRepository,
                CoreManager.Instance.CharacterSettingsCatalogSO
            );

            if (_rewardPresentation != null)
                _rewardPresentation.Initialize();

            //buttons
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(() => RestartLevel().Forget());
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(() => ReturnToMainMenu().Forget());
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(() => ReturnToMissionPreparation().Forget());
            }

            Hide();
        }

        public void ShowVictory()
        {
            HideAllPanels();
            _victoryPanel.SetActive(true);
            _mainMenuButton.gameObject.SetActive(true);
            _restartButton.gameObject.SetActive(true);
            _continueButton.gameObject.SetActive(true);
            Show();
        }

        public void ShowGameOver()
        {
            HideAllPanels();
            _gameOverPanel.SetActive(true);
            _mainMenuButton.gameObject.SetActive(true);
            _restartButton.gameObject.SetActive(true);
            Show();
        }

        private void HideAllPanels()
        {
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        private async UniTask RestartLevel()
        {
            await NavigationManager.NavigateToLastMission();
        }

        private async UniTask ReturnToMainMenu()
        {
            await NavigationManager.NavigateToScene(SceneType.MainMenuScene);
        }

        private async UniTask ReturnToMissionPreparationAsync()
        {
            await NavigationManager.NavigateToScene(SceneType.MissionPreparation);
        }

        private async UniTask ReturnToMissionPreparation()
        {
            // Grant reward BEFORE returning to menu
            var currentStageIndex = _stageManager.CurrentStageIndex;
            var difficulty = NavigationManager.GetLastDifficulty();

            var missionData = _missionsManager.GetMissionData(currentStageIndex, difficulty);

            if (missionData.isMissionCompleted && !missionData.isRewardReceived)
            {
                var rewardResult = await _rewardService.GrantRewardAsync(missionData.rewardData);

                if (rewardResult.isSuccess)
                {
                    // Mark reward as received
                    missionData.isRewardReceived = true;
                    _missionsManager.SetMissionData(currentStageIndex, difficulty, missionData);

                    // Show reward presentation
                    if (_rewardPresentation != null)
                    {
                        _rewardPresentation.ShowRewardAsync(rewardResult).Forget();
                        await UniTask.WaitUntil(() => !_rewardPresentation.gameObject.activeSelf);
                    }
                }
            }

            await NavigationManager.NavigateToScene(SceneType.MissionPreparation);
        }

        public override void Hide()
        {
            base.Hide();
            HideAllPanels();
        }
    }
}