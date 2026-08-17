using Cysharp.Threading.Tasks;
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
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _gameOverPanel;

        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _continueButton;

        private MissionsManager _missionsManager;

        public override void Initialize()
        {
            base.Initialize();

            _missionsManager = CoreManager.Instance.MissionsManager;

            // Button bindings
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => RestartLevel().Forget());

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(() => ReturnToMainMenu().Forget());

            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => GameplayCoreManager.Instance.GameLogic.ExecuteReward());

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

        public override void Hide()
        {
            base.Hide();
            HideAllPanels();
        }
    }
}