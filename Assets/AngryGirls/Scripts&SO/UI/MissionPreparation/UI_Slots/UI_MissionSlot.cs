using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class UI_MissionSlot : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _missionIcon;
        [SerializeField] private TextMeshProUGUI _missionNameText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _completedOverlay;
        [SerializeField] private Image _lockedOverlay;

        private Mission _mission;
        private MissionsManager _missionsManager;
        private Action<Mission> _onClickCallback;

        public void Initialize(Mission mission, MissionsManager missionsManager, Action<Mission> onClickCallback)
        {
            _mission = mission;
            _missionsManager = missionsManager;
            _onClickCallback = onClickCallback;

            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// Update display using RUNTIME progress data from MissionsManager.
        /// </summary>
        public async UniTaskVoid UpdateDisplay(MissionDifficulty currentDifficulty)
        {
            if (_mission == null || _mission.missionName == SceneType.None)
            {
                gameObject.SetActive(false);
                return;
            }

            var progressData = _missionsManager.GetMissionData(_mission.missionName, currentDifficulty);

            await LoadMissionIconAsync();

            if (_missionNameText != null)
            {
                _missionNameText.text = $"{_mission.missionName}_{currentDifficulty}";
            }

            UpdateVisualState(progressData.isMissionAvailable, progressData.isMissionCompleted);
        }

        private async UniTask LoadMissionIconAsync()
        {
            if (_missionIcon == null) return;

            try
            {
                var sprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(_mission.iconReference);
                if (sprite != null)
                {
                    _missionIcon.sprite = sprite;
                    _missionIcon.enabled = true;
                }
                else
                {
                    _missionIcon.enabled = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"UI_MissionSlot: Failed to load icon for {_mission.missionName}: {e.Message}");
                _missionIcon.enabled = false;
            }
        }

        private void UpdateVisualState(bool isAvailable, bool isCompleted)
        {
            if (_lockedOverlay != null)
                _lockedOverlay.gameObject.SetActive(!isAvailable);

            if (_completedOverlay != null)
                _completedOverlay.gameObject.SetActive(isCompleted);

            if (_button != null)
                _button.interactable = isAvailable;
        }

        private void OnButtonClicked()
        {
            _onClickCallback?.Invoke(_mission);
        }

        public Mission GetMission() => _mission;

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}