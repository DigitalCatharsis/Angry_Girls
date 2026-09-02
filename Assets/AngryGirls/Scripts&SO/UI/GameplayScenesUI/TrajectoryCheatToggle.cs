using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Toggles full trajectory visualization cheat mode
    /// and keeps it synchronized with the current stage launcher.
    /// </summary>
    public class TrajectoryCheatToggle : UI_GameplayManagersComponent
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private Sprite _enabledSprite;
        [SerializeField] private Sprite _disabledSprite;

        private bool _isCheatModeActive;
        private CharacterLauncher _characterLauncher;
        private StageManager _stageManager;

        public override void Initialize()
        {
            base.Initialize();

            _stageManager =
                GameplayCoreManager.Instance?.StageManager;

            if (_stageManager != null)
            {
                _stageManager.TheStageIsSet +=
                    OnStageChanged;
            }

            ResolveCurrentLauncher();

            if (_toggleButton != null)
            {
                _toggleButton.onClick.AddListener(
                    ToggleCheatMode);

                UpdateButtonVisual();
            }
        }

        private void ToggleCheatMode()
        {
            _isCheatModeActive =
                !_isCheatModeActive;

            ApplyCheatModeToCurrentLauncher();
            UpdateButtonVisual();
        }

        private void OnStageChanged(
            int stageIndex)
        {
            ResolveCurrentLauncher();
            ApplyCheatModeToCurrentLauncher();
        }

        private void ResolveCurrentLauncher()
        {
            _characterLauncher =
                _stageManager?
                    .CurrentCharacterLauncher;
        }

        private void ApplyCheatModeToCurrentLauncher()
        {
            if (_characterLauncher == null)
                return;

            _characterLauncher.SetCheatTrajectoryMode(
                _isCheatModeActive);
        }

        private void UpdateButtonVisual()
        {
            if (_toggleButton == null)
                return;

            var image =
                _toggleButton.GetComponent<Image>();

            if (image == null)
                return;

            image.sprite =
                _isCheatModeActive
                    ? _enabledSprite
                    : _disabledSprite;
        }

        private void OnDestroy()
        {
            if (_toggleButton != null)
            {
                _toggleButton.onClick.RemoveListener(
                    ToggleCheatMode);
            }

            if (_stageManager != null)
            {
                _stageManager.TheStageIsSet -=
                    OnStageChanged;
            }
        }
    }
}