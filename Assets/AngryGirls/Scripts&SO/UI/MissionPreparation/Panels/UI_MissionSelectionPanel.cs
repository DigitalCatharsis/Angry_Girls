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
        [SerializeField] private TextMeshProUGUI _missionReward;
        [SerializeField] private Button _difficultyEasyButton;
        [SerializeField] private Button _difficultyNormalButton;
        [SerializeField] private Button _difficultyHardButton;

        private List<UI_MissionSlot> _missionSlots = new List<UI_MissionSlot>();
        private Mission _selectedMission;
        private MissionDifficulty _selectedDifficulty = MissionDifficulty.Easy;
        private MissionsManager _missionsManager;

        /// <summary>
        /// Initialize the panel.
        /// </summary>
        public void Initialize(CoreManager coremanger)
        {
            _missionsManager = coremanger.MissionsManager;
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
            _selectedMission = default;
            UpdateMissionList();
            UpdateDifficultyButtonsVisual();
        }

        private void SetDefaultPreview()
        {
            if (_missionPreview != null) { _missionPreview.sprite = default; }
            ;
            Color tmp = Color.white;
            tmp.a = 0.25f;
            _missionPreview.color = tmp;
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
            if (missions == null) { return; }
            ;

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
            if (_difficultyEasyButton != null)
            {
                var image = _difficultyEasyButton.GetComponent<Image>();
                if (image != null)
                    image.color = (_selectedDifficulty == MissionDifficulty.Easy) ? Color.yellow : Color.white;
            }

            if (_difficultyNormalButton != null)
            {
                var image = _difficultyNormalButton.GetComponent<Image>();
                if (image != null)
                    image.color = (_selectedDifficulty == MissionDifficulty.Normal) ? Color.yellow : Color.white;
            }

            if (_difficultyHardButton != null)
            {
                var image = _difficultyHardButton.GetComponent<Image>();
                if (image != null)
                    image.color = (_selectedDifficulty == MissionDifficulty.Hard) ? Color.yellow : Color.white;
            }
        }

        private void OnMissionSlotClicked(Mission mission)
        {
            _selectedMission = mission;
            UpdatePreviewWindowContent(_selectedMission).Forget();
            UpdateMissionDetailsAndPreviewWindows(_selectedMission, _selectedDifficulty);
        }

        private void UpdateMissionDetailsAndPreviewWindows(Mission mission, MissionDifficulty difficulty)
        {
            var missionData = mission.GetData(difficulty);

            if (_missionDescription != null)
            {
                _missionDescription.text = $"Mission: {mission.missionName}\nDifficulty: {difficulty}";
            }

            if (_missionReward != null)
            {
                _missionReward.text = $"Reward: {missionData.reward}";
            }
        }

        private async UniTask UpdatePreviewWindowContent(Mission mission)
        {
            try
            {
                var previewSprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(mission.previewReference);
                if (previewSprite != null)
                {
                    _missionPreview.sprite = previewSprite;
                    Color tmp = Color.white;
                    tmp.a = 1f;
                    _missionPreview.color = tmp;
                }
                else
                {
                    SetDefaultPreview();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"MissionSelectionPanel: Failed to load preview for mission {mission.missionName}: {e.Message}");
                _missionPreview.sprite = default;
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