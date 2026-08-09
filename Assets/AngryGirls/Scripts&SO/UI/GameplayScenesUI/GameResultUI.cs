using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Manages victory and game over UI screens.
    /// Handles reward granting, coin conversion, and reward presentation.
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
        private RewardService _rewardService;

        public override void Initialize()
        {
            base.Initialize();

            _missionsManager = CoreManager.Instance.MissionsManager;

            // Initialize RewardService with all required dependencies
            _rewardService = new RewardService(
                CoreManager.Instance.InventoryManager,
                CoreManager.Instance.CharactersManager,
                CoreManager.Instance.CreditsManager,
                CoreManager.Instance.ItemSettingsRepository,
                CoreManager.Instance.CharacterSettingsCatalogSO
            );

            if (_rewardPresentation != null)
                _rewardPresentation.Initialize(CoreManager.Instance.AddressableAssetManager);

            // Button bindings
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => RestartLevel().Forget());

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(() => ReturnToMainMenu().Forget());

            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => ReturnToMissionPreparation().Forget());

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

        /// <summary>
        /// Returns to mission preparation after victory.
        /// Grants reward (only once), converts collected coins to credits,
        /// shows reward presentation, then saves and navigates.
        /// </summary>
        private async UniTask ReturnToMissionPreparation()
        {
            var missionData = _missionsManager.GetMissionData(
                _missionsManager.CurrentMission,
                _missionsManager.CurrentDifficulty);

            RewardGrantResult rewardResult = null;
            var rewardAlreadyReceived = missionData.isRewardReceived;

            if (!missionData.isRewardReceived)
            {
                rewardResult = await _rewardService.GrantRewardAsync(missionData.rewardData);

                if (rewardResult.isSuccess)
                {
                    _missionsManager.CompleteCurrentMission();
                }
            }

            // Convert collected coins to credits
            int collectedCoins = GetCollectedCoinsScore();
            if (collectedCoins > 0)
            {
                CoreManager.Instance.CreditsManager.SetCredits(collectedCoins);
            }

            CoreManager.Instance.SaveLoadManager.SaveGame();

            // Hide ALL gameplay UI before showing reward
            if (rewardResult != null && _rewardPresentation != null)
            {
                HideAllGameplayUI();
                Hide();

                var presentationData = BuildPresentationData(rewardResult, collectedCoins);
                await _rewardPresentation.ShowRewardAsync(presentationData);
            }

            await NavigationManager.NavigateToScene(SceneType.MissionPreparation);
        }

        /// <summary>
        /// Hide all gameplay UI elements before showing reward presentation.
        /// </summary>
        private void HideAllGameplayUI()
        {
            // Hide victory/game over panels
            HideAllPanels();

            // Hide score display
            var scoreDisplay = FindObjectOfType<ScoreDisplay>();
            if (scoreDisplay != null) scoreDisplay.Hide();

            // Hide trajectory cheat button
            var trajectoryCheat = FindObjectOfType<TrajectoryCheatToggle>();
            if (trajectoryCheat != null) trajectoryCheat.Hide();

            // Hide tutorial
            var tutorial = FindObjectOfType<TutorialSystem>();
            if (tutorial != null) tutorial.Hide();

            // Hide character panel
            var characterPanel = FindObjectOfType<UI_GameplayCharactersPanel>();
            if (characterPanel != null) characterPanel.Hide();

            // Hide pause menu
            var pauseMenu = FindObjectOfType<PauseMenu>();
            if (pauseMenu != null) pauseMenu.Hide();
        }

        /// <summary>
        /// Gets the score of coins collected during the current mission.
        /// Reads from ScoreDisplay component in the gameplay scene.
        /// </summary>
        private int GetCollectedCoinsScore()
        {
            var scoreDisplay = FindObjectOfType<ScoreDisplay>();
            if (scoreDisplay != null)
            {
                return scoreDisplay.GetScore();
            }
            return 0;
        }

        /// <summary>
        /// Builds the data package for reward presentation display.
        /// Includes reward result, collected coins, and character XP stubs.
        /// </summary>
        private RewardPresentationData BuildPresentationData(
            RewardGrantResult rewardResult, int collectedCoins)
        {
            var data = new RewardPresentationData
            {
                rewardResult = rewardResult,
                collectedCoinsScore = collectedCoins,
                characterEntries = new List<CharacterRewardEntry>()
            };

            // Get selected characters from CharactersManager
            var charactersData = CoreManager.Instance.CharactersManager.CharactersData;
            if (charactersData != null)
            {
                foreach (var character in charactersData.SelectedCharactersPool)
                {
                    if (character == null || character.CharacterSettings == null) continue;

                    data.characterEntries.Add(new CharacterRewardEntry
                    {
                        characterSettings = character.CharacterSettings,
                        xpGained = Random.Range(50, 200),
                        currentLevel = Random.Range(1, 10),
                        currentXp = Random.Range(100, 500),
                        xpForNextLevel = 1000
                    });
                }
            }

            return data;
        }

        public override void Hide()
        {
            base.Hide();
            HideAllPanels();
        }
    }
}