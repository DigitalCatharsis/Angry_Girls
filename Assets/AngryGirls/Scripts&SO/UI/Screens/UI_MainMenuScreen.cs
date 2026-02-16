using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Angry_Girls
{
    /// <summary>
    /// Main menu screen with navigation buttons and settings.
    /// </summary>
    public class UI_MainMenuScreen : UI_UIScreen
    {
        [SerializeField] private GameObject _mainMenuPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        [Header("Settings Panel")]
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _soundsVolumeSlider;
        [SerializeField] private Button _closeSettingsButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject _confirmationDialog;
        [SerializeField] private Button _confirmNewGameButton;
        [SerializeField] private Button _cancelNewGameButton;

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            SetupButtons();
            SetupSettingsPanel();
            SetupConfirmationDialog();

            UpdateContinueButtonState();
        }

        private void SetupButtons()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(OnNewGamePressed);

            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinuePressed);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsPressed);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitPressed);
        }

        private void SetupSettingsPanel()
        {
            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);

                if (_musicVolumeSlider != null)
                {
                    _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                }

                if (_soundsVolumeSlider != null)
                {
                    _soundsVolumeSlider.onValueChanged.AddListener(OnSoundsVolumeChanged);
                }

                if (_closeSettingsButton != null)
                {
                    _closeSettingsButton.onClick.AddListener(OnCloseSettingsPressed);
                }
            }
        }

        private void SetupConfirmationDialog()
        {
            if (_confirmationDialog != null)
            {
                _confirmationDialog.SetActive(false);

                if (_confirmNewGameButton != null)
                {
                    _confirmNewGameButton.onClick.AddListener(OnConfirmNewGame);
                }

                if (_cancelNewGameButton != null)
                {
                    _cancelNewGameButton.onClick.AddListener(OnCancelNewGame);
                }
            }
        }

        public void UpdateContinueButtonState()
        {
            bool hasSaveData = Repository.LoadState();

            if (_continueButton != null)
            {
                _continueButton.interactable = hasSaveData;
                var textComponent = _continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = hasSaveData ? "Continue" : "No Save Data";
                }
            }
        }

        /// <inheritdoc/>
        public override void Show()
        {
            base.Show();
            UpdateContinueButtonState();
            LoadSettingsValues();
        }

        private void LoadSettingsValues()
        {
            var settings = CoreManager.Instance.SettingsManager.GetSettings();

            if (_musicVolumeSlider != null)
                _musicVolumeSlider.value = settings.volumeMusic;

            if (_soundsVolumeSlider != null)
                _soundsVolumeSlider.value = settings.volumeSounds;
        }

        #region Button Handlers

        private void OnNewGamePressed()
        {
            Debug.Log("New Game Button Pressed");
            ShowConfirmationDialog();
        }

        private void OnContinuePressed()
        {
            Debug.Log("Continue Button Pressed");

            GameStateManager.Instance.ContinueGame().Forget();
        }

        private void OnSettingsPressed()
        {
            Debug.Log("Settings Button Pressed");
            ShowSettingsPanel();
        }

        private void OnExitPressed()
        {
            Debug.Log("Exit Button Pressed");
            ExitGame();
        }
        #endregion

        #region Settings Handlers

        private void OnMusicVolumeChanged(float value)
        {
            Debug.Log($"Music Volume Changed: {value}");
            CoreManager.Instance.SettingsManager.SetupMusicVolume(value);
        }

        private void OnSoundsVolumeChanged(float value)
        {
            Debug.Log($"Sounds Volume Changed: {value}");
            CoreManager.Instance.SettingsManager.SetupSoundsVolume(value);
        }

        private void ShowSettingsPanel()
        {
            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(false);

            if (_settingsPanel != null)
            {
                LoadSettingsValues();
                _settingsPanel.SetActive(true);
            }
        }

        private void OnCloseSettingsPressed()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);

            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(true);
        }

        #endregion

        #region Confirmation Dialog Handlers

        private void ShowConfirmationDialog()
        {
            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(false);

            if (_confirmationDialog != null)
                _confirmationDialog.SetActive(true);
        }

        private void OnConfirmNewGame()
        {
            if (_confirmationDialog != null)
                _confirmationDialog.SetActive(false);

            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(true);

            _ = GameStateManager.Instance.NewGame();
        }

        private void OnCancelNewGame()
        {
            if (_confirmationDialog != null)
                _confirmationDialog.SetActive(false);

            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(true);
        }

        #endregion

        private void ExitGame()
        {
            Debug.Log("Exit Game");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.RemoveListener(OnNewGamePressed);

            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinuePressed);

            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettingsPressed);

            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(OnExitPressed);

            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

            if (_soundsVolumeSlider != null)
                _soundsVolumeSlider.onValueChanged.RemoveListener(OnSoundsVolumeChanged);

            if (_closeSettingsButton != null)
                _closeSettingsButton.onClick.RemoveListener(OnCloseSettingsPressed);

            if (_confirmNewGameButton != null)
                _confirmNewGameButton.onClick.RemoveListener(OnConfirmNewGame);

            if (_cancelNewGameButton != null)
                _cancelNewGameButton.onClick.RemoveListener(OnCancelNewGame);
        }
    }
}