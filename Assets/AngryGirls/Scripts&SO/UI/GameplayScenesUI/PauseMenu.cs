using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Pause menu with game control options
    /// </summary>
    public class PauseMenu : UI_GameplayManagersComponent
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _genericPanel;

        [Header("Buttons")]
        [SerializeField] private Button _returnButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _quitMissionButton;
        [SerializeField] private Button _quitGameButton;
        [SerializeField] private Button _closeOptionsButton;

        private PauseControl _pauseControl;
        private bool _isPaused;

        private SettingsManager _settingsManager;
        private AudioManager _audioManager;

        public override void Initialize()
        {
            base.Initialize();
            
            _settingsManager = CoreManager.Instance.SettingsManager;
            _pauseControl = new PauseControl();
            _pauseControl.OnPauseChanged += OnPauseChanged;
            _audioManager = CoreManager.Instance.AudioManager;

            SetupButtons();
            _settingsPanel.GetComponent<UI_SettingsMenu>().Initialize(_genericPanel);
            HideSettingsPanel();
        }

        private void SetupButtons()
        {
            _returnButton.onClick.AddListener(OnReturnPressed);
            _restartButton.onClick.AddListener(() => OnRestartPressed().Forget());
            _optionsButton.onClick.AddListener(OnSettingsPressed);
            _quitMissionButton.onClick.AddListener(() => OnQuitMissionPressed().Forget());
            _quitGameButton.onClick.AddListener(OnQuitGamePressed);
            _closeOptionsButton.onClick.AddListener(OnCloseOptionsPressed);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !_isPaused)
            {
                Execute();
            }
        }

        public void Execute()
        {
            TogglePause();
            ShowGenericPanel();
        }

        public void TogglePause()
        {
            if (_isPaused)
                _pauseControl.UnpauseGame();
            else
                _pauseControl.PauseGame();
        }

        private void OnPauseChanged(bool isPaused)
        {
            _isPaused = isPaused;
            _genericPanel.SetActive(isPaused);

            if (isPaused)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        }

        private void OnReturnPressed() => _pauseControl.UnpauseGame();
        private async UniTaskVoid OnRestartPressed()
        {
            await NavigationManager.NavigateToLastMission();
        }
        private void OnSettingsPressed()
        {
            ShowSettingsPanel();
            HideGenericPanel();
            //_settingsPanel.GetComponent<UI_SettingsMenu>().LoadCategoryValues();
        }

        private async UniTaskVoid OnQuitMissionPressed() 
        {
            await NavigationManager.NavigateToScene(SceneType.MissionPreparation); 
        }
        private void OnQuitGamePressed()
        {
            _settingsManager.SaveSettings();
            ExitGame();
        }

        private void ExitGame()
        {
            _settingsManager.SaveSettings();
            Debug.Log("Exit Game");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnCloseOptionsPressed()
        {
            HideSettingsPanel();
            ShowGenericPanel();
        }
        private void ShowSettingsPanel() => _settingsPanel.SetActive(true);
        private void ShowGenericPanel() => _genericPanel.SetActive(true);
        private void HideSettingsPanel() => _settingsPanel.SetActive(false);
        private void HideGenericPanel() => _genericPanel.SetActive(false);

        private void OnDestroy()
        {
            _returnButton.onClick.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
        }
    }
}