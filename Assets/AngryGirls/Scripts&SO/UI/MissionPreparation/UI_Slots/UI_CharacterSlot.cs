using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Defines the context in which a character slot is displayed.
    /// </summary>
    public enum CharacterSlotType
    {
        Selected,
        Available
    }

    /// <summary>
    /// UI slot for displaying a character and its current selection state.
    /// </summary>
    public class UI_CharacterSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image _characterPortrait;
        [SerializeField] private Button _button;
        [SerializeField] private TMPro.TextMeshProUGUI _levelText;
        [Tooltip("Optional container of the level label, it is hidden together with the label.")]
        [SerializeField] private GameObject _levelBadge;

        [Header("Selection")]
        [SerializeField] private GameObject _selectionHighlight;

        public event Action<UI_CharacterSlot> OnClicked;
        public event Action<UI_CharacterSlot> OnPointerEntered;
        public event Action<UI_CharacterSlot> OnPointerExited;

        public CharacterProfile Character { get; private set; }
        public CharacterSlotType SlotType { get; private set; }

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);

            SetSelected(false);
        }

        /// <summary>
        /// Sets character data and updates the slot visuals.
        /// </summary>
        public UniTask SetCharacter(CharacterProfile character, CharacterSlotType slotType)
        {
            UnsubscribeFromCharacter();

            Character = character;
            SlotType = slotType;

            SetSelected(false);

            if (character == null)
            {
                Clear();
                return UniTask.CompletedTask;
            }

            character.OnProgressChanged += RefreshLevelLabel;
            RefreshLevelLabel();

            var portrait = character.CharacterSettings != null ? character.CharacterSettings.portrait : null;

            if (_characterPortrait != null)
            {
                _characterPortrait.sprite = portrait;
                _characterPortrait.enabled = portrait != null;
            }

            UpdateSlotVisual();
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Clears the character from the slot.
        /// </summary>
        public void Clear()
        {
            UnsubscribeFromCharacter();
            Character = null;
            RefreshLevelLabel();
            SetSelected(false);

            if (_characterPortrait != null)
            {
                _characterPortrait.sprite = null;
                _characterPortrait.enabled = false;
            }

            UpdateSlotVisual();
        }

        /// <summary>
        /// Updates the selected state of the slot.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(isSelected);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEntered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExited?.Invoke(this);
        }

        private void HandleClick()
        {
            OnClicked?.Invoke(this);
        }

        private void UpdateSlotVisual()
        {
            if (_button != null)
                _button.interactable = Character != null;
        }

        private void RefreshLevelLabel()
        {
            if (_levelText == null)
                return;

            var badge = _levelBadge != null ? _levelBadge : _levelText.gameObject;
            badge.SetActive(Character != null);
            if (Character != null)
                _levelText.text = $"Lv.{Character.Level}";
        }

        private void UnsubscribeFromCharacter()
        {
            if (Character != null)
                Character.OnProgressChanged -= RefreshLevelLabel;
        }

        private void OnDestroy()
        {
            UnsubscribeFromCharacter();

            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }
    }
}