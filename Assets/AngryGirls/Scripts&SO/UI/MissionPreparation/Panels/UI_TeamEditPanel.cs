using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// UI panel for editing character equipment and viewing stats in mission preparation.
    /// Properly unsubscribes from all events on destroy.
    /// </summary>
    public class UI_TeamEditPanel : MonoBehaviour, IUIPanel
    {
        [Header("UI Containers")]
        [SerializeField] private Transform _selectedCharactersContainer;
        [SerializeField] private Transform _availableCharactersContainer;

        [Header("Equipment Slots")]
        [SerializeField] private GameObject _equipmentSlotsPanel;
        [SerializeField] private Button _weaponSlotButton;
        [SerializeField] private Button _armorSlotButton;
        [SerializeField] private Button _accessorySlot1Button;
        [SerializeField] private Button _accessorySlot2Button;
        [SerializeField] private Sprite _defaultItemSlotSprite;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI _statsText;

        [Header("Prefabs")]
        [SerializeField] private GameObject _characterSlotPrefab;

        [Header("Popup")]
        [SerializeField] private UI_EquipmentPopup _equipmentPopup;

        private CharactersManager _charactersManager;
        private CharacterProfile _selectedCharacter;
        private List<UI_CharacterSlot> _selectedSlots = new();
        private List<UI_CharacterSlot> _availableSlots = new();

        private bool _isDestroyed = false;

        public void Refresh()
        {
            if (_isDestroyed) return;
            UpdateCharacterSlots();
        }

        public void Initialize(CoreManager coreManager)
        {
            _charactersManager = coreManager.CharactersManager;

            SpawnCharactersSlots();

            // Subscribe to data changes - use single handler
            _charactersManager.OnDataChanged += OnCharactersDataChanged;

            UpdateCharacterSlots();
        }

        /// <summary>
        /// Single handler for CharactersManager.OnDataChanged.
        /// </summary>
        private void OnCharactersDataChanged()
        {
            if (_isDestroyed) return;
            UpdateCharacterSlots();
        }

        private void SpawnCharactersSlots()
        {
            // Create 6 slots for selected characters
            for (int i = 0; i < 6; i++)
            {
                var slotGO = Instantiate(_characterSlotPrefab, _selectedCharactersContainer);
                var slot = slotGO.GetComponent<UI_CharacterSlot>();
                _selectedSlots.Add(slot);
                SubscribeSlot(slot);
                slotGO.SetActive(true);
            }

            // Create 10 slots for available characters
            for (int i = 0; i < 10; i++)
            {
                var slotGO = Instantiate(_characterSlotPrefab, _availableCharactersContainer);
                var slot = slotGO.GetComponent<UI_CharacterSlot>();
                _availableSlots.Add(slot);
                SubscribeSlot(slot);
                slotGO.SetActive(true);
            }

            // Hide equipment panel until character is selected
            if (_equipmentSlotsPanel != null)
                _equipmentSlotsPanel.SetActive(false);
        }

        private void SubscribeSlot(UI_CharacterSlot slot)
        {
            if (slot == null) return;
            slot.OnClicked += OnCharacterClicked;
            slot.OnPointerEntered += OnPointerEntered;
            slot.OnPointerExited += OnPointerExited;
        }

        private void UnsubscribeSlot(UI_CharacterSlot slot)
        {
            if (slot == null) return;
            slot.OnClicked -= OnCharacterClicked;
            slot.OnPointerEntered -= OnPointerEntered;
            slot.OnPointerExited -= OnPointerExited;
        }

        private void UpdateCharacterSlots()
        {
            if (_isDestroyed) return;
            if (_charactersManager == null) return;

            var data = _charactersManager.CharactersData;
            if (data == null) return;

            // Update selected characters (compact populating)
            var compactSelected = data.SelectedCharactersPool.Where(p => p != null).ToArray();
            for (int i = 0; i < 6; i++)
            {
                if (_selectedSlots[i] == null) continue;

                if (i < compactSelected.Length)
                    _selectedSlots[i].SetCharacter(compactSelected[i], CharacterSlotType.Selected).Forget();
                else
                    _selectedSlots[i].Clear();
            }

            // Update available characters (compact populating)
            var compactAvailable = data.AvailableCharacterPool.Where(p => p != null).ToArray();
            for (int i = 0; i < compactAvailable.Length && i < _availableSlots.Count; i++)
            {
                if (_availableSlots[i] == null) continue;
                _availableSlots[i].SetCharacter(compactAvailable[i], CharacterSlotType.Available).Forget();
            }
            for (int i = compactAvailable.Length; i < _availableSlots.Count; i++)
            {
                if (_availableSlots[i] == null) continue;
                _availableSlots[i].Clear();
            }

            ValidateCurrentSelection(data);

            // Update equipment buttons if a character is selected
            if (_selectedCharacter != null)
            {
                UpdateEquipmentSlotButtons();
            }
        }

        private void ValidateCurrentSelection(CharactersData data)
        {
            if (_selectedCharacter == null) return;

            bool exists = data.SelectedCharactersPool.Contains(_selectedCharacter) ||
                          data.AvailableCharacterPool.Contains(_selectedCharacter);

            if (!exists)
            {
                UnsubscribeFromSelectedCharacter(_selectedCharacter);
                _selectedCharacter = null;
                if (_equipmentSlotsPanel != null)
                    _equipmentSlotsPanel.SetActive(false);
                if (_statsText != null)
                    _statsText.text = CharactersStatsBase.GetEmptyText();
            }
        }

        private void OnCharacterClicked(UI_CharacterSlot slot)
        {
            if (_isDestroyed || slot == null) return;
            OnCharacterSelected(slot, slot.Character);
        }

        private void OnPointerEntered(UI_CharacterSlot slot)
        {
            if (_isDestroyed || slot == null) return;
            if (slot.Character != null && _statsText != null)
            {
                var baseStats = slot.Character.GetSettingsStats;
                var itemsStats = slot.Character.GetItemsStats;
                _statsText.text = CharactersStatsBase.GetColoredText(baseStats, itemsStats);
            }
        }

        private void OnPointerExited(UI_CharacterSlot slot)
        {
            if (_isDestroyed) return;
            if (_selectedCharacter != null && _statsText != null)
            {
                var baseStats = _selectedCharacter.GetSettingsStats;
                var itemsStats = _selectedCharacter.GetItemsStats;
                _statsText.text = CharactersStatsBase.GetColoredText(baseStats, itemsStats);
            }
            else if (_statsText != null)
            {
                _statsText.text = CharactersStatsBase.GetEmptyText();
            }
        }

        private void OnCharacterSelected(UI_CharacterSlot slot, CharacterProfile character)
        {
            UnsubscribeFromSelectedCharacter(_selectedCharacter);
            _selectedCharacter = character;
            SubscribeToSelectedCharacter(_selectedCharacter);

            if (character != null)
            {
                if (_equipmentSlotsPanel != null)
                    _equipmentSlotsPanel.SetActive(true);
                UpdateStatsDisplay();
                UpdateEquipmentSlotButtons();

                if (_weaponSlotButton != null)
                {
                    _weaponSlotButton.onClick.RemoveAllListeners();
                    _weaponSlotButton.onClick.AddListener(() => ShowItemPopup(ItemType.Weapon, 0));
                }
                if (_armorSlotButton != null)
                {
                    _armorSlotButton.onClick.RemoveAllListeners();
                    _armorSlotButton.onClick.AddListener(() => ShowItemPopup(ItemType.Armor, 0));
                }
                if (_accessorySlot1Button != null)
                {
                    _accessorySlot1Button.onClick.RemoveAllListeners();
                    _accessorySlot1Button.onClick.AddListener(() => ShowItemPopup(ItemType.Accessory, 0));
                }
                if (_accessorySlot2Button != null)
                {
                    _accessorySlot2Button.onClick.RemoveAllListeners();
                    _accessorySlot2Button.onClick.AddListener(() => ShowItemPopup(ItemType.Accessory, 1));
                }
            }
            else
            {
                if (_equipmentSlotsPanel != null)
                    _equipmentSlotsPanel.SetActive(false);
                if (_statsText != null)
                    _statsText.text = CharactersStatsBase.GetEmptyText();
            }
        }

        private void SubscribeToSelectedCharacter(CharacterProfile character)
        {
            if (character != null)
            {
                character.OnEquipmentChanged += OnSelectedCharacterEquipmentChanged;
            }
        }

        private void UnsubscribeFromSelectedCharacter(CharacterProfile character)
        {
            if (character != null)
            {
                character.OnEquipmentChanged -= OnSelectedCharacterEquipmentChanged;
            }
        }

        private void OnSelectedCharacterEquipmentChanged()
        {
            if (_isDestroyed) return;
            UpdateStatsDisplay();
            UpdateEquipmentSlotButtons();
        }

        private void ShowItemPopup(ItemType slotType, int accessorySlotIndex)
        {
            if (_equipmentPopup == null)
            {
                Debug.LogError("UI_EquipmentPopup not assigned in inspector!");
                return;
            }

            _equipmentPopup.Show(_selectedCharacter, slotType, accessorySlotIndex);

            UniTask.Void(async () =>
            {
                await UniTask.WaitUntil(() => !_equipmentPopup.gameObject.activeSelf);
                if (!_isDestroyed)
                    Refresh();
            });
        }

        private async void UpdateEquipmentSlotButtons()
        {
            if (_selectedCharacter == null || _isDestroyed) return;

            await UpdateSlotButton(_weaponSlotButton, _selectedCharacter.Weapon, "Weapon");
            if (_isDestroyed) return;

            await UpdateSlotButton(_armorSlotButton, _selectedCharacter.Armor, "Armor");
            if (_isDestroyed) return;

            await UpdateSlotButton(_accessorySlot1Button, _selectedCharacter.Accessory1, "Accessory 1");
            if (_isDestroyed) return;

            await UpdateSlotButton(_accessorySlot2Button, _selectedCharacter.Accessory2, "Accessory 2");
        }

        private async UniTask UpdateSlotButton(Button button, InventoryItem item, string defaultName)
        {
            if (button == null || _isDestroyed) return;

            var image = button.GetComponentInChildren<Image>();
            var text = button.GetComponentInChildren<TextMeshProUGUI>();

            if (item != null && item.ItemSettings != null)
            {
                if (image != null && !string.IsNullOrEmpty(item.ItemSettings.IconReference?.AssetGUID))
                {
                    var sprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(item.ItemSettings.IconReference.AssetGUID);
                    if (_isDestroyed) return;
                    if (sprite != null)
                    {
                        image.sprite = sprite;
                        image.enabled = true;
                    }
                }

                if (text != null)
                {
                    text.text = item.ItemSettings.ItemName;
                }
            }
            else
            {
                if (image != null)
                {
                    image.sprite = _defaultItemSlotSprite;
                }
                if (text != null)
                {
                    text.text = defaultName;
                }
            }
        }

        private void UpdateStatsDisplay()
        {
            if (_selectedCharacter == null || _isDestroyed) return;
            if (_statsText == null) return;

            var baseStats = _selectedCharacter.GetSettingsStats;
            var itemsStats = _selectedCharacter.GetItemsStats;
            _statsText.text = CharactersStatsBase.GetColoredText(baseStats, itemsStats);
        }

        /// <summary>
        /// Properly unsubscribe from ALL events on destroy.
        /// </summary>
        private void OnDestroy()
        {
            _isDestroyed = true;

            // Unsubscribe from CharactersManager - SINGLE handler
            if (_charactersManager != null)
            {
                _charactersManager.OnDataChanged -= OnCharactersDataChanged;
            }

            // Unsubscribe from selected character
            UnsubscribeFromSelectedCharacter(_selectedCharacter);

            // Unsubscribe all slots
            foreach (var elem in _availableSlots)
                UnsubscribeSlot(elem);
            foreach (var elem in _selectedSlots)
                UnsubscribeSlot(elem);

            _availableSlots.Clear();
            _selectedSlots.Clear();
        }
    }
}