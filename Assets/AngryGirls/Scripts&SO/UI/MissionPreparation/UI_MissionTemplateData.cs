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
        public async UniTaskVoid UpdateValues(Mission mission, MissionDifficulty difficulty)
        {
            _mission = mission;
            _missionData = _mission.GetData(difficulty);

            if (_missionNameText != null)
            {
                _missionNameText.text = _mission.missionName.ToString();
            }

            await LoadAndSetIconAsync();

            UpdateMissionStatusVisual(_missionData.isMissionAvailable, _missionData.isMissionCompleted);

            name = _mission.missionName.ToString();
        }

        private async UniTask LoadAndSetIconAsync()
        {
            if (_missionIcon != null && _mission.iconReference != null && !string.IsNullOrEmpty(_mission.iconReference.AssetGUID))
            {
                try
                {
                    var iconSprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(_mission.iconReference);

                    if (iconSprite != null && _missionIcon != null)
                    {
                        _missionIcon.sprite = iconSprite;
                        _missionIcon.enabled = true;
                    }
                    else
                    {
                        _missionIcon.enabled = false;
                        if (iconSprite == null)
                            Debug.LogWarning($"MissionTemplateData: Loaded icon sprite is null for AssetReference GUID '{_mission.iconReference.AssetGUID}' in mission {_mission.missionName}.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"MissionTemplateData: Failed to load icon for mission {_mission.missionName} from AssetReference '{_mission.iconReference?.AssetGUID}': {e.Message}");
                    if (_missionIcon != null) _missionIcon.enabled = false;
                }
            }
            else
            {
                if (_missionIcon != null) _missionIcon.enabled = false;
            }
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