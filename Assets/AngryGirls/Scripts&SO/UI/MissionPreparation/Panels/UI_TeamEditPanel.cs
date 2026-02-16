using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// UI panel for editing character equipment and viewing stats in mission preparation
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

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI _statsText;

        [Header("Prefabs")]
        [SerializeField] private GameObject _characterSlotPrefab;

        [Header("popup")]
        [SerializeField] private UI_EquipmentPopup _equipmentPopup;

        private CharactersManager _charactersManager;
        private CharacterProfile _selectedCharacter;
        private List<UI_CharacterSlot> _selectedSlots = new();
        private List<UI_CharacterSlot> _availableSlots = new();
        public void Refresh()
        {
            UpdateCharacterSlots();
        }

        public void Initialize(CoreManager coreManager)
        {
            _charactersManager = coreManager.CharactersManager;

            // Initialize UI slots once
            SpawnCharactersSlots();

            _charactersManager.OnDataChanged += UpdateCharacterSlots;
            _charactersManager.OnDataChanged += Refresh;

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

            // Create 10 slots for available characters (with scroll buffer)
            for (int i = 0; i < 10; i++)
            {
                var slotGO = Instantiate(_characterSlotPrefab, _availableCharactersContainer);
                var slot = slotGO.GetComponent<UI_CharacterSlot>();
                _availableSlots.Add(slot);
                SubscribeSlot(slot);
                slotGO.SetActive(true);
            }

            // Hide equipment panel until character is selected
            _equipmentSlotsPanel.SetActive(false);
        }

        private void SubscribeSlot(UI_CharacterSlot slot)
        {
            if (slot == null) return;

            slot.OnClicked += OnCharacterClicked;
            slot.OnPointerEntered += OnPointerEntered;
            slot.OnPointerExited += OnPointerExited;
        }

        private void OnCharacterClicked(UI_CharacterSlot slot)
        {
            OnCharacterSelected(slot, slot.Character);
        }

        private void OnPointerEntered(UI_CharacterSlot slot)
        {
            //_selectedCharacter = slot.Character;
            //UpdateStatsDisplay();
            if (slot.Character != null)
            {
                var baseStats = slot.Character.GetSettingsStats;
                var itemsStats = slot.Character.GetItemsStats;
                _statsText.text = CharactersStatsBase.GetColoredText(baseStats, itemsStats);
            }
        }

        private void OnPointerExited(UI_CharacterSlot slot)
        {
            // Возвращаем отображение статов выбранного персонажа (если он есть)
            if (_selectedCharacter != null)
            {
                var baseStats = _selectedCharacter.GetSettingsStats;
                var itemsStats = _selectedCharacter.GetItemsStats;
                _statsText.text = CharactersStatsBase.GetColoredText(baseStats, itemsStats);
            }
            else
            {
                _statsText.text = CharactersStatsBase.GetEmptyText();
            }
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
            var data = _charactersManager.CharactersData;

            // Update selected characters (compact populating)
            var compactSelected = data.SelectedCharactersPool.Where(p => p != null).ToArray();

            for (int i = 0; i < 6; i++)
            {
                if (i < compactSelected.Length)
                    _selectedSlots[i].SetCharacter(compactSelected[i], CharacterSlotType.Selected).Forget();
                else
                    _selectedSlots[i].Clear();
            }

            // Update available characters (compact populating)
            var compactAvailable = data.AvailableCharacterPool.Where(p => p != null).ToArray();

            for (int i = 0; i < compactAvailable.Length && i < _availableSlots.Count; i++)
            {
                _availableSlots[i].SetCharacter(compactAvailable[i], CharacterSlotType.Available).Forget();
            }

            for (int i = compactAvailable.Length; i < _availableSlots.Count; i++)
            {
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

            // Check if the selected character exists in the pools 
            bool exists = data.SelectedCharactersPool.Contains(_selectedCharacter) ||
            data.AvailableCharacterPool.Contains(_selectedCharacter);

            if (!exists)
            {
                _selectedCharacter = null;
                if (_equipmentSlotsPanel != null)
                {
                    _equipmentSlotsPanel.SetActive(false);
                }
                _statsText.text = "Character Stats:\n\n<color=gray>Select a character to view stats</color>";
            }
        }

        private void OnCharacterSelected(UI_CharacterSlot slot, CharacterProfile character)
        {
            UnsubscribeFromSelectedCharacter(_selectedCharacter);

            _selectedCharacter = character;
            SubscribeToSelectedCharacter(_selectedCharacter);

            if (character != null)
            {
                _equipmentSlotsPanel.SetActive(true);
                UpdateStatsDisplay();
                UpdateEquipmentSlotButtons();

                _weaponSlotButton.onClick.RemoveAllListeners();
                _weaponSlotButton.onClick.AddListener(() => ShowItemPopup(ItemType.Weapon, 0));

                _armorSlotButton.onClick.RemoveAllListeners();
                _armorSlotButton.onClick.AddListener(() => ShowItemPopup(ItemType.Armor, 0));

                _accessorySlot1Button.onClick.RemoveAllListeners();
                _accessorySlot1Button.onClick.AddListener(() => ShowItemPopup(ItemType.Accessory, 0));

                _accessorySlot2Button.onClick.RemoveAllListeners();
                _accessorySlot2Button.onClick.AddListener(() => ShowItemPopup(ItemType.Accessory, 1));
            }
            else
            {
                _equipmentSlotsPanel.SetActive(false);
                _statsText.text = "Character Stats:\n\n<color=gray>Select a character to view stats</color>";
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

            // Start a background task that waits until _equipmentPopup becomes inactive
            UniTask.Void(async () =>
            {
                // Wait until the popup's gameObject is deactivated
                await UniTask.WaitUntil(() => !_equipmentPopup.gameObject.activeSelf);

                // When the popup is closed (deactivated), call Refresh
                Refresh();
            });
        }

        private async void UpdateEquipmentSlotButtons()
        {
            if (_selectedCharacter == null) return;

            // Weapon slot
            await UpdateSlotButton(_weaponSlotButton, _selectedCharacter.Weapon, "Weapon");

            // Armor slot
            await UpdateSlotButton(_armorSlotButton, _selectedCharacter.Armor, "Armor");

            // Accessory slots
            await UpdateSlotButton(_accessorySlot1Button, _selectedCharacter.Accessory1, "Accessory 1");
            await UpdateSlotButton(_accessorySlot2Button, _selectedCharacter.Accessory2, "Accessory 2");
        }

        private async UniTask UpdateSlotButton(Button button, InventoryItem item, string defaultName)
        {
            if (button == null) return;

            var image = button.GetComponentInChildren<Image>();
            var text = button.GetComponentInChildren<TextMeshProUGUI>();

            if (item != null && item.ItemSettings != null)
            {
                // Item icon 
                if (image != null && !string.IsNullOrEmpty(item.ItemSettings.IconReference?.AssetGUID))
                {
                    var sprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(item.ItemSettings.IconReference.AssetGUID);
                    if (sprite != null)
                    {
                        image.sprite = sprite;
                        image.enabled = true;
                    }
                }

                // Item name
                if (text != null)
                {
                    text.text = item.ItemSettings.ItemName;
                }
            }
            else
            {
                // Default slot state
                if (image != null)
                {
                    image.sprite = null;
                    image.enabled = false;
                }

                if (text != null)
                {
                    text.text = defaultName;
                }
            }
        }

        private void UpdateStatsDisplay()
        {
            if (_selectedCharacter == null) return;

            var baseStats = _selectedCharacter.GetSettingsStats;
            var itemsStats = _selectedCharacter.GetItemsStats;

            _statsText.text = CharactersStatsBase.GetColoredText(baseStats, itemsStats);
        }

        private void ClearDisplay()
        {
            if (_selectedCharacter == null) return;

            _statsText.text = CharactersStatsBase.GetEmptyText();
            var stats = _selectedCharacter.GetSettingsStats;
        }

        //TODO WHY DOES NOT FUCKIN CALL?!?!?!?!
        private void OnDestroy()
        {
            if (_charactersManager != null)
                _charactersManager.OnDataChanged -= UpdateCharacterSlots;
            UnsubscribeFromSelectedCharacter(_selectedCharacter);

            foreach (var elem in _availableSlots) UnsubscribeSlot(elem);
            foreach (var elem in _selectedSlots) UnsubscribeSlot(elem);

        }
    }
}