using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Gameplay settings category panel.
    /// </summary>
    public sealed class UI_SettingsGameplayPanel
        : MonoBehaviour,
          ISettingsCategoryPanel
    {
        [Header("Gameplay Controls")]
        [SerializeField] private Toggle _showTurnOrderToggle;

        private SettingsManager _settingsManager;
        private bool _isInitializing;

        public SettingsCategory Category =>
            SettingsCategory.Gameplay;

        /// <summary>
        /// Initializes the gameplay settings panel.
        /// </summary>
        public void Initialize(
            SettingsManager settingsManager)
        {
            _settingsManager =
                settingsManager;

            if (_showTurnOrderToggle != null)
            {
                _showTurnOrderToggle.onValueChanged
                    .AddListener(
                        OnShowTurnOrderChanged);
            }

            LoadValues();
        }

        /// <summary>
        /// Loads current gameplay settings into the UI.
        /// </summary>
        public void LoadValues()
        {
            if (_settingsManager == null ||
                _showTurnOrderToggle == null)
            {
                return;
            }

            _isInitializing = true;

            _showTurnOrderToggle.isOn =
                _settingsManager
                    .GetCurrentSettings()
                    .showTurnOrder;

            _isInitializing = false;
        }

        /// <summary>
        /// Saves current settings.
        /// </summary>
        public void SaveValues()
        {
            _settingsManager?.SaveSettings();
        }

        private void OnShowTurnOrderChanged(
            bool value)
        {
            if (_isInitializing ||
                _settingsManager == null)
            {
                return;
            }

            _settingsManager.SetupShowTurnOrder(
                value);
        }

        private void OnDestroy()
        {
            if (_showTurnOrderToggle != null)
            {
                _showTurnOrderToggle.onValueChanged
                    .RemoveListener(
                        OnShowTurnOrderChanged);
            }
        }
    }
}