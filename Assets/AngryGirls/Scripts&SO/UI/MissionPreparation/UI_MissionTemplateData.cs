using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Angry_Girls
{
    /// <summary>
    /// Template for mission data display.
    /// </summary>
    public class UI_MissionTemplateData : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI _missionNameText;
        [SerializeField] private Image _missionIcon;
        [SerializeField] private Image _completedOverlay;
        [SerializeField] private Image _lockedOverlay;
        [SerializeField] private Button _button;

        private Mission _mission = new Mission();
        private MissionData _missionData = new MissionData();
        public SceneType MissionName => _mission.missionName;

        /// <summary>
        /// Update the display with mission data for a specific difficulty.
        /// </summary>
        public void UpdateValues(Mission mission, MissionDifficulty difficulty)
        {
            _mission = mission;
            _missionData = _mission.GetData(difficulty);

            if (_missionNameText != null)
            {
                _missionNameText.text = _mission.missionName.ToString();
            }

            ApplyIcon();

            UpdateMissionStatusVisual(_missionData.isMissionAvailable, _missionData.isMissionCompleted);

            name = _mission.missionName.ToString();
        }

        private void ApplyIcon()
        {
            if (_missionIcon == null) return;

            var sprite = _mission != null ? _mission.icon : null;
            _missionIcon.sprite = sprite;
            _missionIcon.enabled = sprite != null;
        }

        private void UpdateMissionStatusVisual(bool isAvailable, bool isCompleted)
        {
            if (_lockedOverlay != null)
                _lockedOverlay.gameObject.SetActive(!isAvailable);

            if (_completedOverlay != null)
                _completedOverlay.gameObject.SetActive(isCompleted);

            if (_button != null)
                _button.interactable = isAvailable;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }
    }
}