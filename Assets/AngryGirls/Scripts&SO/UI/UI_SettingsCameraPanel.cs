using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Angry_Girls
{
    /// <summary>
    /// Camera settings category panel
    /// </summary>
    public class UI_SettingsCameraPanel : MonoBehaviour, ISettingsCategoryPanel
    {
        [Header("Camera Controls")]
        [SerializeField] private Slider _movementSpeedSlider;
        [SerializeField] private Button _resetButton;
        [SerializeField] private TextMeshProUGUI _platformLabel;

        private SettingsManager _settingsManager;
        private PlatformSettingsCatalog _platformCatalog;
        private bool _isInitializing = false;

        public SettingsCategory Category => SettingsCategory.Camera;

        public void Initialize(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _platformCatalog = CoreManager.Instance.PlatformSettingsCatalog;
            
            SetupListeners();
            UpdatePlatformLabel();
        }

        private void SetupListeners()
        {
            if (_movementSpeedSlider != null)
                _movementSpeedSlider.onValueChanged.AddListener(OnMovementSpeedChanged);

            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetPressed);
        }

        private void UpdatePlatformLabel()
        {
            if (_platformLabel != null && _platformCatalog != null)
            {
                var profile = _platformCatalog.GetCurrentPlatformProfile();
                if (profile != null)
                {
                    _platformLabel.text = $"Platform: {profile.platform}";
                }
            }
        }

        public void LoadValues()
        {
            _isInitializing = true;
            var settings = _settingsManager.GetCurrentSettings();

            _movementSpeedSlider.value = settings.cameraMovementSpeed;

            _isInitializing = false;
        }

        public void SaveValues()
        {
            _settingsManager.SaveSettings();
        }

        private void OnMovementSpeedChanged(float value)
        {
            if (_isInitializing) return;
            _settingsManager.SetupCameraMovementSpeed(value);
        }

        private void OnResetPressed()
        {
            var settings = _settingsManager.GetCurrentSettings();
            settings.useCustomCameraSettings = false;

            if (_platformCatalog != null)
            {
                var profile = _platformCatalog.GetCurrentPlatformProfile();
                settings.cameraMovementSpeed = profile.camera.movementSpeed;
            }

            _settingsManager.ApplyPlatformDefaults(SettingsCategory.Camera);
            LoadValues();
            UIManager.Instance?.ShowNotification("Camera settings reset", 0.5f);
        }

        private void OnDestroy()
        {
            if (_movementSpeedSlider != null)
                _movementSpeedSlider.onValueChanged.RemoveListener(OnMovementSpeedChanged);
            if (_resetButton != null)
                _resetButton.onClick.RemoveListener(OnResetPressed);
        }
    }
}