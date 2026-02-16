using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Toggles full trajectory visualization cheat mode
    /// </summary>
    public class TrajectoryCheatToggle : UI_GameplayManagersComponent
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private Sprite _enabledSprite;
        [SerializeField] private Sprite _disabledSprite;

        private bool _isCheatModeActive = false;
        private CharacterLauncher _characterLauncher;

        public override void Initialize()
        {
            base.Initialize();

            _characterLauncher = GameplayCoreManager.Instance.StageManager.CurrentCharacterLauncher;

            if (_toggleButton != null)
            {
                _toggleButton.onClick.AddListener(ToggleCheatMode);
                UpdateButtonVisual();
            }
        }

        private void ToggleCheatMode()
        {
            _isCheatModeActive = !_isCheatModeActive;
            _characterLauncher?.SetCheatTrajectoryMode(_isCheatModeActive);
            UpdateButtonVisual();
        }

        private void UpdateButtonVisual()
        {
            if (_toggleButton == null) return;

            var image = _toggleButton.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = _isCheatModeActive ? _enabledSprite : _disabledSprite;
            }
        }
    }
}