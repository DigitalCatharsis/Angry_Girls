using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Angry_Girls
{
    public enum CharacterSlotType { Selected, Available }

    public class UI_CharacterSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image _characterPortrait;
        [SerializeField] private Button _button;

        // События
        public event Action<UI_CharacterSlot> OnClicked;
        public event Action<UI_CharacterSlot> OnPointerEntered;
        public event Action<UI_CharacterSlot> OnPointerExited;

        public CharacterProfile Character { get; private set; }
        public CharacterSlotType SlotType { get; private set; }

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(() => OnClicked?.Invoke(this));
        }

        public async UniTask SetCharacter(CharacterProfile character, CharacterSlotType slotType)
        {
            Character = character;
            SlotType = slotType;

            if (character != null)
            {
                var settings = character.CharacterSettings;

                if (settings != null && _characterPortrait != null)
                {
                    _characterPortrait.sprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(settings.portrait);
                    _characterPortrait.enabled = true;
                }

            }
            else
            {
                Clear();
            }

            UpdateSlotVisual();
        }

        public void Clear()
        {
            Character = null;
            if (_characterPortrait != null)
            {
                _characterPortrait.sprite = null;
                _characterPortrait.enabled = false;
            }
            UpdateSlotVisual();
        }

        public void OnPointerEnter(PointerEventData eventData) => OnPointerEntered?.Invoke(this);
        public void OnPointerExit(PointerEventData eventData) => OnPointerExited?.Invoke(this);

        private void UpdateSlotVisual()
        {
            if (_button != null)
                _button.interactable = Character != null;
        }
    }
}