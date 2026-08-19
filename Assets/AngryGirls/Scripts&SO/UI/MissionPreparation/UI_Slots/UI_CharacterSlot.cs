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
        public async UniTask SetCharacter(CharacterProfile character, CharacterSlotType slotType)
        {
            Character = character;
            SlotType = slotType;

            SetSelected(false);

            if (character == null)
            {
                Clear();
                return;
            }

            var settings = character.CharacterSettings;

            if (settings != null && _characterPortrait != null)
            {
                var sprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(settings.portrait);

                if (this == null || _characterPortrait == null)
                    return;

                _characterPortrait.sprite = sprite;
                _characterPortrait.enabled = sprite != null;
            }

            UpdateSlotVisual();
        }

        /// <summary>
        /// Clears the character from the slot.
        /// </summary>
        public void Clear()
        {
            Character = null;
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

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }
    }
}