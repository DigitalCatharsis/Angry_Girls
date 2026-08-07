using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Angry_Girls
{
    /// <summary>
    /// Panel for selecting missions and difficulty.
    /// Uses runtime progress data from MissionsManager instead of static repository data.
    /// Subscribes to MissionsManager.OnDataChanged for reactive UI updates.
    /// </summary>
    public class UI_MissionSelectionPanel : MonoBehaviour, IUIPanel
    {
        [Header("UI References")]
        [SerializeField] private Transform _missionsContainer;
        [SerializeField] private GameObject _missionSlotPrefab;
        [SerializeField] private Image _missionPreview;
        [SerializeField] private TextMeshProUGUI _missionDescription;

        [Header("Reward Display")]
        [SerializeField] private TextMeshProUGUI _rewardNameText;
        [SerializeField] private Image _rewardIconImage;
        [SerializeField] private Sprite _defaultCreditsIcon;

        /// <summary>
        /// Checkmark displayed over the reward icon when the reward was already claimed.
        /// Place as a child of the reward icon in the prefab hierarchy.
        /// </summary>
        [SerializeField] private GameObject _rewardReceivedCheckmark;

        [Header("Difficulty Buttons")]
        [SerializeField] private Button _difficultyEasyButton;
        [SerializeField] private Button _difficultyNormalButton;
        [SerializeField] private Button _difficultyHardButton;

        private List<UI_MissionSlot> _missionSlots = new List<UI_MissionSlot>();
        private Mission _selectedMission;
        private MissionDifficulty _selectedDifficulty = MissionDifficulty.Easy;
        private MissionsManager _missionsManager;
        private IAssetProvider _assetProvider;

        /// <summary>
        /// Flag to guard against callbacks after destruction.
        /// </summary>
        private bool _isDestroyed = false;

        public MissionDifficulty SelectedDifficulty => _selectedDifficulty;

        /// <summary>
        /// Initialize the panel. Restores last selected difficulty and subscribes to data changes.
        /// </summary>
        public void Initialize(CoreManager coreManager)
        {
            _missionsManager = coreManager.MissionsManager;
            _assetProvider = coreManager.AddressableAssetManager;

            // Restore last selected difficulty from previous session
            _selectedDifficulty = NavigationManager.GetLastDifficulty();

            // Subscribe to runtime data changes for reactive UI updates
            _missionsManager.OnDataChanged += OnMissionsDataChanged;

            SetupDifficultyButtons();
            CreateMissionSlots();
            Refresh();
        }

        /// <summary>
        /// Callback invoked when MissionsManager data changes.
        /// Updates mission list and reward display if a mission is selected.
        /// </summary>
        private void OnMissionsDataChanged()
        {
            if (_isDestroyed) return;

            UpdateMissionList();

            if (_selectedMission != null)
            {
                UpdateMissionDetailsAndRewardDisplay(_selectedMission, _selectedDifficulty);
            }
        }

        /// <summary>
        /// Refresh the panel display.
        /// </summary>
        public void Refresh()
        {
            if (_isDestroyed) return;

            SetDefaultPreview();
            ClearRewardDisplay();
            _selectedMission = default;
            UpdateMissionList();
            UpdateDifficultyButtonsVisual();
        }

        private void SetDefaultPreview()
        {
            if (_missionPreview != null)
            {
                _missionPreview.sprite = default;
            }
            _missionPreview.color = new Color(1f, 1f, 1f, 0.25f);
        }

        private void ClearRewardDisplay()
        {
            if (_rewardNameText != null)
                _rewardNameText.text = "Reward: -";

            if (_rewardIconImage != null)
            {
                _rewardIconImage.sprite = null;
                _rewardIconImage.enabled = false;
            }

            if (_rewardReceivedCheckmark != null)
                _rewardReceivedCheckmark.SetActive(false);
        }

        private void SetupDifficultyButtons()
        {
            if (_difficultyEasyButton != null)
                _difficultyEasyButton.onClick.AddListener(OnEasyClicked);

            if (_difficultyNormalButton != null)
                _difficultyNormalButton.onClick.AddListener(OnNormalClicked);

            if (_difficultyHardButton != null)
                _difficultyHardButton.onClick.AddListener(OnHardClicked);
        }

        private void OnEasyClicked() => SetDifficulty(MissionDifficulty.Easy);
        private void OnNormalClicked() => SetDifficulty(MissionDifficulty.Normal);
        private void OnHardClicked() => SetDifficulty(MissionDifficulty.Hard);

        private void CreateMissionSlots()
        {
            if (_missionsContainer == null || _missionSlotPrefab == null) return;

            foreach (var slot in _missionSlots)
            {
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            }
            _missionSlots.Clear();

            var missions = _missionsManager?.GetMissionsFromRepository();
            if (missions != null)
            {
                foreach (var mission in missions)
                {
                    var slotGO = Instantiate(_missionSlotPrefab, _missionsContainer);
                    var missionSlot = slotGO.GetComponent<UI_MissionSlot>();
                    if (missionSlot != null)
                    {
                        // Pass MissionsManager so slot can query runtime progress data
                        missionSlot.Initialize(mission, _missionsManager, OnMissionSlotClicked);
                        _missionSlots.Add(missionSlot);
                    }
                }
            }
        }

        private void UpdateMissionList()
        {
            var missions = _missionsManager?.GetMissionsFromRepository();
            if (missions == null) return;

            for (int i = 0; i < _missionSlots.Count && i < missions.Count; i++)
            {
                _missionSlots[i].UpdateDisplay(_selectedDifficulty).Forget();
            }
        }

        /// <summary>
        /// Sets difficulty and persists it for future sessions.
        /// </summary>
        private void SetDifficulty(MissionDifficulty difficulty)
        {
            _selectedDifficulty = difficulty;

            // Persist selected difficulty for next session
            NavigationManager.SetLastDifficulty(difficulty);

            UpdateMissionList();
            UpdateDifficultyButtonsVisual();

            if (_selectedMission != null)
            {
                UpdateMissionDetailsAndRewardDisplay(_selectedMission, _selectedDifficulty);
            }
        }

        private void UpdateDifficultyButtonsVisual()
        {
            UpdateButtonColor(_difficultyEasyButton, MissionDifficulty.Easy);
            UpdateButtonColor(_difficultyNormalButton, MissionDifficulty.Normal);
            UpdateButtonColor(_difficultyHardButton, MissionDifficulty.Hard);
        }

        private void UpdateButtonColor(Button button, MissionDifficulty difficulty)
        {
            if (button != null)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                    image.color = (_selectedDifficulty == difficulty) ? Color.yellow : Color.white;
            }
        }

        private void OnMissionSlotClicked(Mission mission)
        {
            _selectedMission = mission;
            UpdatePreviewWindowContent(_selectedMission).Forget();
            UpdateMissionDetailsAndRewardDisplay(_selectedMission, _selectedDifficulty);
        }

        /// <summary>
        /// Updates mission details and reward display using RUNTIME data from MissionsManager.
        /// </summary>
        private void UpdateMissionDetailsAndRewardDisplay(Mission mission, MissionDifficulty difficulty)
        {
            // Use runtime progress data, not static repository data
            var missionData = _missionsManager.GetMissionData(mission.missionName, difficulty);

            if (_missionDescription != null)
            {
                _missionDescription.text =
                    $"Mission: {mission.missionName}\nDifficulty: {difficulty}";
            }

            // Show checkmark if reward was already claimed
            if (_rewardReceivedCheckmark != null)
                _rewardReceivedCheckmark.SetActive(missionData.isRewardReceived);

            UpdateRewardDisplay(missionData);
        }

        /// <summary>
        /// Updates reward icon and text from runtime mission data.
        /// </summary>
        private void UpdateRewardDisplay(MissionData missionData)
        {
            if (_rewardNameText == null || _rewardIconImage == null)
                return;

            var rewardData = missionData.rewardData;

            if (rewardData == null)
            {
                _rewardNameText.text = "Reward: -";
                _rewardIconImage.enabled = false;
                return;
            }

            switch (rewardData.rewardType)
            {
                case RewardType.Credits:
                    _rewardNameText.text = $"Reward: +{rewardData.creditsAmount} Credits";
                    LoadRewardIcon(_defaultCreditsIcon);
                    break;

                case RewardType.Item:
                    LoadItemRewardAsync(rewardData.assetReference.AssetGUID);
                    break;

                case RewardType.Character:
                    LoadCharacterRewardAsync(rewardData.characterType);
                    break;

                case RewardType.None:
                default:
                    _rewardNameText.text = "Reward: -";
                    _rewardIconImage.enabled = false;
                    break;
            }
        }

        private void LoadRewardIcon(Sprite sprite)
        {
            if (sprite != null)
            {
                _rewardIconImage.sprite = sprite;
                _rewardIconImage.enabled = true;
            }
            else
            {
                _rewardIconImage.enabled = false;
            }
        }

        private async void LoadItemRewardAsync(string assetGuid)
        {
            if (_isDestroyed) return;

            var itemSettings = await _assetProvider.LoadScriptableObjectAsync<ItemSettings>(assetGuid);
            if (_isDestroyed) return;

            if (itemSettings != null)
            {
                _rewardNameText.text = $"Reward: {itemSettings.ItemName}";

                if (itemSettings.IconReference != null && !string.IsNullOrEmpty(itemSettings.IconReference.AssetGUID))
                {
                    var sprite = await _assetProvider.LoadSpriteAsync(itemSettings.IconReference);
                    if (_isDestroyed) return;
                    LoadRewardIcon(sprite);
                }
                else
                {
                    _rewardIconImage.enabled = false;
                }
            }
        }

        private async void LoadCharacterRewardAsync(CharacterType characterType)
        {
            if (_isDestroyed) return;

            var settings = CoreManager.Instance.CharacterSettingsCatalogSO.GetByType(characterType);
            if (settings != null)
            {
                _rewardNameText.text = $"Reward: {settings.name}";

                if (settings.portrait != null && !string.IsNullOrEmpty(settings.portrait.AssetGUID))
                {
                    var sprite = await _assetProvider.LoadSpriteAsync(settings.portrait);
                    if (_isDestroyed) return;
                    LoadRewardIcon(sprite);
                }
                else
                {
                    _rewardIconImage.enabled = false;
                }
            }
        }

        private async UniTask UpdatePreviewWindowContent(Mission mission)
        {
            if (_isDestroyed) return;

            try
            {
                var previewSprite = await _assetProvider.LoadSpriteAsync(mission.previewReference);
                if (_isDestroyed) return;

                if (previewSprite != null)
                {
                    _missionPreview.sprite = previewSprite;
                    _missionPreview.color = Color.white;
                }
                else
                {
                    SetDefaultPreview();
                }
            }
            catch (System.Exception e)
            {
                if (_isDestroyed) return;
                Debug.LogError($"UI_MissionSelectionPanel: Failed to load preview for mission {mission.missionName}: {e.Message}");
                SetDefaultPreview();
            }
        }

        /// <summary>
        /// Check if a mission is selected.
        /// </summary>
        public bool IsMissionSelected()
        {
            return _selectedMission != null && _selectedMission.missionName != SceneType.None;
        }

        /// <summary>
        /// Get the selected mission.
        /// </summary>
        public Mission GetSelectedMission()
        {
            return _selectedMission;
        }

        /// <summary>
        /// Unsubscribe from all events on destruction.
        /// Uses method group references for correct unsubscription.
        /// </summary>
        private void OnDestroy()
        {
            _isDestroyed = true;

            if (_missionsManager != null)
                _missionsManager.OnDataChanged -= OnMissionsDataChanged;

            if (_difficultyEasyButton != null)
                _difficultyEasyButton.onClick.RemoveListener(OnEasyClicked);

            if (_difficultyNormalButton != null)
                _difficultyNormalButton.onClick.RemoveListener(OnNormalClicked);

            if (_difficultyHardButton != null)
                _difficultyHardButton.onClick.RemoveListener(OnHardClicked);
        }
    }
}