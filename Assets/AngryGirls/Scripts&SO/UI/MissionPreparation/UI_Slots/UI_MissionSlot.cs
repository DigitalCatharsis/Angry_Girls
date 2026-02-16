using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Angry_Girls
{
    /// <summary>
    /// UI slot for displaying a mission.
    /// </summary>
    public class UI_MissionSlot : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _missionIcon;
        [SerializeField] private TextMeshProUGUI _missionNameText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _completedOverlay;
        [SerializeField] private Image _lockedOverlay;

        private Mission _mission;
        private Action<Mission> _onClickCallback;

        /// <summary>
        /// Initialize the slot with mission data and click callback.
        /// </summary>
        public void Initialize(Mission mission, Action<Mission> onClickCallback)
        {
            _mission = mission;
            _onClickCallback = onClickCallback;

            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// Update the display for the current difficulty.
        /// </summary>
        public async UniTaskVoid UpdateDisplay(MissionDifficulty currentDifficulty)
        {
            if (_mission.missionName == SceneType.None)
            {
                gameObject.SetActive(false);
                return;
            }

            var missionData = _mission.GetData(currentDifficulty);

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
                    Debug.LogWarning($"MissionSlot: Loaded sprite is null for AssetReference '{_mission.iconReference.AssetGUID}' in mission {_mission.missionName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"MissionSlot: Failed to load icon for mission {_mission.missionName} from AssetReference '{_mission.iconReference?.AssetGUID}' (name: {_mission.iconReference?.SubObjectName}: {e.Message}");
                _missionIcon.enabled = false;
            }

            if (_missionNameText != null)
            {
                _missionNameText.text = _mission.missionName.ToString() + "_" + currentDifficulty;
            }

            UpdateVisualState(missionData.isMissionAvailable, missionData.isMissionCompleted);
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

        /// <summary>
        /// Get the mission associated with this slot.
        /// </summary>
        public Mission GetMission() => _mission;

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}