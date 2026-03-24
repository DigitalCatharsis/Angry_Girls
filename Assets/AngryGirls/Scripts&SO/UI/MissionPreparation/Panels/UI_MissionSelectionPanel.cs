using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Angry_Girls
{
    /// <summary>
    /// Panel for selecting missions and difficulty.
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

        [Header("Difficulty Buttons")]
        [SerializeField] private Button _difficultyEasyButton;
        [SerializeField] private Button _difficultyNormalButton;
        [SerializeField] private Button _difficultyHardButton;

        private List<UI_MissionSlot> _missionSlots = new List<UI_MissionSlot>();
        private Mission _selectedMission;
        private MissionDifficulty _selectedDifficulty = MissionDifficulty.Easy;
        private MissionsManager _missionsManager;
        private IAssetProvider _assetProvider;
        public MissionDifficulty SelectedDifficulty => _selectedDifficulty;

        /// <summary>
        /// Initialize the panel.
        /// </summary>
        public void Initialize(CoreManager coreManager)
        {
            _missionsManager = coreManager.MissionsManager;
            _assetProvider = coreManager.AddressableAssetManager;

            SetupDifficultyButtons();
            CreateMissionSlots();
            Refresh();
        }

        /// <summary>
        /// Refresh the panel display.
        /// </summary>
        public void Refresh()
        {
            SetDefaultPreview();
            ClearRewardDisplay();
            _selectedMission = default;
            UpdateMissionList();
            UpdateDifficultyButtonsVisual();
        }

        private void SetDefaultPreview()
        {
            if (_missionPreview != null) { _missionPreview.sprite = default; };
            _missionPreview.color = new Color(1f, 1f, 1f, 0.25f);
            //Color tmp = Color.white;
            //tmp.a = 0.25f;
            //_missionPreview.color = tmp;
        }
        private void ClearRewardDisplay()
        {
            if (_rewardNameText != null)
                _rewardNameText.text = "Reward: —";

            if (_rewardIconImage != null)
            {
                _rewardIconImage.sprite = null;
                _rewardIconImage.enabled = false;
            }
        }
        private void SetupDifficultyButtons()
        {
            if (_difficultyEasyButton != null)
                _difficultyEasyButton.onClick.AddListener(() => SetDifficulty(MissionDifficulty.Easy));

            if (_difficultyNormalButton != null)
                _difficultyNormalButton.onClick.AddListener(() => SetDifficulty(MissionDifficulty.Normal));

            if (_difficultyHardButton != null)
                _difficultyHardButton.onClick.AddListener(() => SetDifficulty(MissionDifficulty.Hard));
        }

        private void CreateMissionSlots()
        {
            if (_missionsContainer == null || _missionSlotPrefab == null) return;

            foreach (var slot in _missionSlots)
            {
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            }
            _missionSlots.Clear();

            var missions = _missionsManager?.GetMissions();
            if (missions != null)
            {
                foreach (var mission in missions)
                {
                    var slotGO = Instantiate(_missionSlotPrefab, _missionsContainer);
                    var missionSlot = slotGO.GetComponent<UI_MissionSlot>();
                    if (missionSlot != null)
                    {
                        missionSlot.Initialize(mission, OnMissionSlotClicked);
                        _missionSlots.Add(missionSlot);
                    }
                }
            }
        }

        private void UpdateMissionList()
        {
            var missions = _missionsManager?.GetMissions();
            if (missions == null) { return; };

            for (int i = 0; i < _missionSlots.Count && i < missions.Count; i++)
            {
                _missionSlots[i].UpdateDisplay(_selectedDifficulty).Forget();
            }
        }

        private void SetDifficulty(MissionDifficulty difficulty)
        {
            _selectedDifficulty = difficulty;
            Refresh();
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

        private void UpdateMissionDetailsAndRewardDisplay(Mission mission, MissionDifficulty difficulty)
        {
            var missionData = mission.GetData(difficulty);

            if (_missionDescription != null)
            {
                _missionDescription.text = $"Mission: {mission.missionName}\nDifficulty: {difficulty}";
            }

            // Update reward display
            UpdateRewardDisplay(missionData.rewardData);
        }

        private void UpdateRewardDisplay(MissionRewardData rewardData)
        {
            if (_rewardNameText == null || _rewardIconImage == null)
                return;

            switch (rewardData.rewardType)
            {
                case RewardType.Credits:
                    _rewardNameText.text = $"Reward: +{rewardData.creditsAmount} Credits";
                    LoadRewardIcon(_defaultCreditsIcon);
                    break;

                case RewardType.Item:
                    LoadItemRewardAsync(rewardData.itemSettingsUniqueId);
                    break;

                case RewardType.Character:
                    LoadCharacterRewardAsync(rewardData.characterType);
                    break;

                case RewardType.None:
                default:
                    _rewardNameText.text = "Reward: —";
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

        private async void LoadItemRewardAsync(string uniqueId)
        {
            var itemSettings = await _assetProvider.LoadScriptableObjectAsync<ItemSettings>(uniqueId);
            if (itemSettings != null)
            {
                _rewardNameText.text = $"Reward: {itemSettings.ItemName}";

                if (itemSettings.IconReference != null && !string.IsNullOrEmpty(itemSettings.IconReference.AssetGUID))
                {
                    var sprite = await _assetProvider.LoadSpriteAsync(itemSettings.IconReference);
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
            var settings = CoreManager.Instance.CharacterSettingsCatalogSO.GetByType(characterType);
            if (settings != null)
            {
                _rewardNameText.text = $"Reward: {settings.name}";

                if (settings.portrait != null && !string.IsNullOrEmpty(settings.portrait.AssetGUID))
                {
                    var sprite = await _assetProvider.LoadSpriteAsync(settings.portrait);
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
            try
            {
                var previewSprite = await _assetProvider.LoadSpriteAsync(mission.previewReference);
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
                Debug.LogError($"MissionSelectionPanel: Failed to load preview for mission {mission.missionName}: {e.Message}");
                SetDefaultPreview();
            }
        }

        /// <summary>
        /// Check if a mission is selected.
        /// </summary>
        public bool IsMissionSelected()
        {
            return _selectedMission.missionName != SceneType.None;
        }

        /// <summary>
        /// Get the selected mission.
        /// </summary>
        public Mission GetSelectedMission()
        {
            return _selectedMission;
        }

        private void OnDestroy()
        {
            if (_difficultyEasyButton != null)
                _difficultyEasyButton.onClick.RemoveListener(() => SetDifficulty(MissionDifficulty.Easy));

            if (_difficultyNormalButton != null)
                _difficultyNormalButton.onClick.RemoveListener(() => SetDifficulty(MissionDifficulty.Normal));

            if (_difficultyHardButton != null)
                _difficultyHardButton.onClick.RemoveListener(() => SetDifficulty(MissionDifficulty.Hard));
        }
    }
}