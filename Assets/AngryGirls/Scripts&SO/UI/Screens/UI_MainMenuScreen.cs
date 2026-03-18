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
        [SerializeField] private Button _closeSettingsButton;

        private SettingsManager _settingsManager;

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
            _settingsManager = CoreManager.Instance.SettingsManager;

            SetupButtons();
            SetupSettingsPanel();
            UpdateContinueButtonState();


            _settingsPanel.GetComponent<UI_SettingsMenu>().Initialize(this.gameObject);
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

                if (_closeSettingsButton != null)
                {
                    _closeSettingsButton.onClick.AddListener(OnCloseSettingsPressed);
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
        public override void Show()
        {
            base.Show();
            UpdateContinueButtonState();
        }

        #region Button Handlers

        private void OnNewGamePressed()
        {
            Debug.Log("New Game Button Pressed");
            if (Repository.LoadState()) 
            {
                UIManager.Instance.ShowConfirmation
                    ("Are you sure? All previous saves will be deleted.", 
                    yesAction:() => { OnConfirmNewGame(); }, 
                    noAction: () => { return; }
                    );
            }
            else
            {
                OnConfirmNewGame();
            }
            
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

        private void ShowSettingsPanel()
        {
            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(false);

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(true);
            }
        }

        private void OnCloseSettingsPressed()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);

            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(true);

            _settingsManager.SaveSettings();
        }

        #endregion

        #region Confirmation Dialog Handlers


        private void OnConfirmNewGame()
        {
            if (_mainMenuPanel != null)
                _mainMenuPanel.SetActive(true);

            GameStateManager.Instance.NewGame().Forget();
        }

        #endregion

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

            if (_closeSettingsButton != null)
                _closeSettingsButton.onClick.RemoveListener(OnCloseSettingsPressed);
        }
    }
}