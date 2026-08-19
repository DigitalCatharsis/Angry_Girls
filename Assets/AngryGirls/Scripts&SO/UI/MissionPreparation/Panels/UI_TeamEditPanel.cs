// ===== UI_TeamEditPanel.cs =====

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// UI panel for editing character equipment and viewing character stats
    /// during mission preparation.
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

        [Tooltip("Final stat color. Used for the total value.")]
        [SerializeField] private Color _statsTotalColor = Color.black;

        [Tooltip("Base stat color. Used for the character base value.")]
        [SerializeField] private Color _statsBaseColor = Color.grey;

        [Tooltip("Equipment bonus color. Used for equipment bonuses.")]
        [SerializeField] private Color _statsBonusColor = Color.green;

        [Header("Prefabs")]
        [SerializeField] private GameObject _characterSlotPrefab;

        [Header("Popup")]
        [SerializeField] private UI_EquipmentPopup _equipmentPopup;

        private CharactersManager _charactersManager;
        private CharacterProfile _selectedCharacter;

        private readonly List<UI_CharacterSlot> _selectedSlots = new();
        private readonly List<UI_CharacterSlot> _availableSlots = new();

        private bool _isDestroyed;

        /// <summary>
        /// Refreshes character slots and all dependent UI.
        /// </summary>
        public void Refresh()
        {
            if (_isDestroyed)
                return;

            UpdateCharacterSlots();

            if (_selectedCharacter != null)
                UpdateStatsDisplay();
        }

        /// <summary>
        /// Initializes the team edit panel and subscribes to character data changes.
        /// </summary>
        public void Initialize(CoreManager coreManager)
        {
            if (coreManager == null)
            {
                Debug.LogError($"{nameof(UI_TeamEditPanel)}: CoreManager is null.");
                return;
            }

            _charactersManager = coreManager.CharactersManager;

            if (_charactersManager == null)
            {
                Debug.LogError($"{nameof(UI_TeamEditPanel)}: CharactersManager is null.");
                return;
            }

            SpawnCharactersSlots();

            _charactersManager.OnDataChanged += OnCharactersDataChanged;

            UpdateCharacterSlots();
        }

        /// <summary>
        /// Handles character data changes from CharactersManager.
        /// </summary>
        private void OnCharactersDataChanged()
        {
            if (_isDestroyed)
                return;

            UpdateCharacterSlots();
        }

        /// <summary>
        /// Creates the character slot instances for selected and available characters.
        /// </summary>
        private void SpawnCharactersSlots()
        {
            if (_characterSlotPrefab == null)
            {
                Debug.LogError($"{nameof(UI_TeamEditPanel)}: Character slot prefab is not assigned.");
                return;
            }

            if (_selectedCharactersContainer == null || _availableCharactersContainer == null)
            {
                Debug.LogError($"{nameof(UI_TeamEditPanel)}: Character slot containers are not assigned.");
                return;
            }

            for (var i = 0; i < 6; i++)
            {
                var slotGO = Instantiate(_characterSlotPrefab, _selectedCharactersContainer);
                var slot = slotGO.GetComponent<UI_CharacterSlot>();

                if (slot == null)
                {
                    Debug.LogError($"{nameof(UI_TeamEditPanel)}: Character slot prefab has no UI_CharacterSlot.");
                    Destroy(slotGO);
                    continue;
                }

                _selectedSlots.Add(slot);
                SubscribeSlot(slot);
                slotGO.SetActive(true);
            }

            for (var i = 0; i < 10; i++)
            {
                var slotGO = Instantiate(_characterSlotPrefab, _availableCharactersContainer);
                var slot = slotGO.GetComponent<UI_CharacterSlot>();

                if (slot == null)
                {
                    Debug.LogError($"{nameof(UI_TeamEditPanel)}: Character slot prefab has no UI_CharacterSlot.");
                    Destroy(slotGO);
                    continue;
                }

                _availableSlots.Add(slot);
                SubscribeSlot(slot);
                slotGO.SetActive(true);
            }

            if (_equipmentSlotsPanel != null)
                _equipmentSlotsPanel.SetActive(false);
        }

        /// <summary>
        /// Subscribes a character slot to all required UI events.
        /// </summary>
        private void SubscribeSlot(UI_CharacterSlot slot)
        {
            if (slot == null)
                return;

            slot.OnClicked += OnCharacterClicked;
            slot.OnPointerEntered += OnPointerEntered;
            slot.OnPointerExited += OnPointerExited;
        }

        /// <summary>
        /// Unsubscribes a character slot from all UI events.
        /// </summary>
        private void UnsubscribeSlot(UI_CharacterSlot slot)
        {
            if (slot == null)
                return;

            slot.OnClicked -= OnCharacterClicked;
            slot.OnPointerEntered -= OnPointerEntered;
            slot.OnPointerExited -= OnPointerExited;
        }

        /// <summary>
        /// Synchronizes character slots with the current CharactersManager data.
        /// </summary>
        private void UpdateCharacterSlots()
        {
            if (_isDestroyed || _charactersManager == null)
                return;

            var data = _charactersManager.CharactersData;

            if (data == null)
                return;

            var compactSelected = data.SelectedCharactersPool
                .Where(character => character != null)
                .ToArray();

            for (var i = 0; i < _selectedSlots.Count; i++)
            {
                var slot = _selectedSlots[i];

                if (slot == null)
                    continue;

                if (i < compactSelected.Length)
                    slot.SetCharacter(compactSelected[i], CharacterSlotType.Selected).Forget();
                else
                    slot.Clear();
            }

            var compactAvailable = data.AvailableCharacterPool
                .Where(character => character != null)
                .ToArray();

            for (var i = 0; i < _availableSlots.Count; i++)
            {
                var slot = _availableSlots[i];

                if (slot == null)
                    continue;

                if (i < compactAvailable.Length)
                    slot.SetCharacter(compactAvailable[i], CharacterSlotType.Available).Forget();
                else
                    slot.Clear();
            }

            ValidateCurrentSelection(data);
            UpdateSelectionHighlight();

            if (_selectedCharacter != null)
                UpdateEquipmentSlotButtons();
        }

        /// <summary>
        /// Ensures that the currently selected character still exists in the runtime data.
        /// </summary>
        private void ValidateCurrentSelection(CharactersData data)
        {
            if (_selectedCharacter == null || data == null)
            {
                UpdateSelectionHighlight();
                return;
            }

            var exists =
                data.SelectedCharactersPool.Contains(_selectedCharacter) ||
                data.AvailableCharacterPool.Contains(_selectedCharacter);

            if (exists)
                return;

            UnsubscribeFromSelectedCharacter(_selectedCharacter);
            _selectedCharacter = null;

            if (_equipmentSlotsPanel != null)
                _equipmentSlotsPanel.SetActive(false);

            if (_statsText != null)
                _statsText.text = CharactersStatsBase.GetEmptyText();

            UpdateSelectionHighlight();
        }

        /// <summary>
        /// Updates the visual highlight so only the active character is highlighted
        /// across both selected and available character lists.
        /// </summary>
        private void UpdateSelectionHighlight()
        {
            foreach (var slot in _selectedSlots)
            {
                if (slot == null)
                    continue;

                var isSelected = _selectedCharacter != null &&
                                 slot.Character == _selectedCharacter;

                slot.SetSelected(isSelected);
            }

            foreach (var slot in _availableSlots)
            {
                if (slot == null)
                    continue;

                var isSelected = _selectedCharacter != null &&
                                 slot.Character == _selectedCharacter;

                slot.SetSelected(isSelected);
            }
        }

        private void OnCharacterClicked(UI_CharacterSlot slot)
        {
            if (_isDestroyed || slot == null || slot.Character == null)
                return;

            OnCharacterSelected(slot, slot.Character);
        }

        private void OnPointerEntered(UI_CharacterSlot slot)
        {
            if (_isDestroyed || slot == null || slot.Character == null || _statsText == null)
                return;

            var baseStats = slot.Character.GetSettingsStats;
            var itemsStats = slot.Character.GetItemsStats;

            _statsText.text = CharactersStatsBase.GetColoredText(
                baseStats,
                itemsStats,
                _statsTotalColor,
                _statsBaseColor,
                _statsBonusColor);
        }

        private void OnPointerExited(UI_CharacterSlot slot)
        {
            if (_isDestroyed || _statsText == null)
                return;

            UpdateStatsDisplay();
        }

        /// <summary>
        /// Selects a character as the active target for equipment editing.
        /// </summary>
        private void OnCharacterSelected(UI_CharacterSlot slot, CharacterProfile character)
        {
            UnsubscribeFromSelectedCharacter(_selectedCharacter);

            _selectedCharacter = character;

            SubscribeToSelectedCharacter(_selectedCharacter);
            UpdateSelectionHighlight();

            if (_selectedCharacter == null)
            {
                if (_equipmentSlotsPanel != null)
                    _equipmentSlotsPanel.SetActive(false);

                if (_statsText != null)
                    _statsText.text = CharactersStatsBase.GetEmptyText();

                return;
            }

            if (_equipmentSlotsPanel != null)
                _equipmentSlotsPanel.SetActive(true);

            UpdateStatsDisplay();
            UpdateEquipmentSlotButtons();
            ConfigureEquipmentButtons();
        }

        /// <summary>
        /// Configures equipment slot button callbacks for the currently selected character.
        /// </summary>
        private void ConfigureEquipmentButtons()
        {
            ConfigureEquipmentButton(_weaponSlotButton, ItemType.Weapon, 0);
            ConfigureEquipmentButton(_armorSlotButton, ItemType.Armor, 0);
            ConfigureEquipmentButton(_accessorySlot1Button, ItemType.Accessory, 0);
            ConfigureEquipmentButton(_accessorySlot2Button, ItemType.Accessory, 1);
        }

        private void ConfigureEquipmentButton(Button button, ItemType itemType, int accessorySlotIndex)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowItemPopup(itemType, accessorySlotIndex));
        }

        private void SubscribeToSelectedCharacter(CharacterProfile character)
        {
            if (character != null)
                character.OnEquipmentChanged += OnSelectedCharacterEquipmentChanged;
        }

        private void UnsubscribeFromSelectedCharacter(CharacterProfile character)
        {
            if (character != null)
                character.OnEquipmentChanged -= OnSelectedCharacterEquipmentChanged;
        }

        private void OnSelectedCharacterEquipmentChanged()
        {
            if (_isDestroyed)
                return;

            UpdateStatsDisplay();
            UpdateEquipmentSlotButtons();
        }

        private void ShowItemPopup(ItemType slotType, int accessorySlotIndex)
        {
            if (_selectedCharacter == null)
                return;

            if (_equipmentPopup == null)
            {
                Debug.LogError($"{nameof(UI_TeamEditPanel)}: Equipment popup is not assigned.");
                return;
            }

            _equipmentPopup.Show(_selectedCharacter, slotType, accessorySlotIndex);

            UniTask.Void(async () =>
            {
                await UniTask.WaitUntil(() =>
                    _equipmentPopup == null ||
                    !_equipmentPopup.gameObject.activeSelf);

                if (_isDestroyed)
                    return;

                Refresh();
            });
        }

        private async void UpdateEquipmentSlotButtons()
        {
            if (_selectedCharacter == null || _isDestroyed)
                return;

            await UpdateSlotButton(
                _weaponSlotButton,
                _selectedCharacter.Weapon,
                "Weapon");

            if (_isDestroyed)
                return;

            await UpdateSlotButton(
                _armorSlotButton,
                _selectedCharacter.Armor,
                "Armor");

            if (_isDestroyed)
                return;

            await UpdateSlotButton(
                _accessorySlot1Button,
                _selectedCharacter.Accessory1,
                "Accessory 1");

            if (_isDestroyed)
                return;

            await UpdateSlotButton(
                _accessorySlot2Button,
                _selectedCharacter.Accessory2,
                "Accessory 2");
        }

        private async UniTask UpdateSlotButton(
            Button button,
            InventoryItem item,
            string defaultName)
        {
            if (button == null || _isDestroyed)
                return;

            var image = button.GetComponentInChildren<Image>();
            var text = button.GetComponentInChildren<TextMeshProUGUI>();

            if (item != null && item.ItemSettings != null)
            {
                if (image != null &&
                    !string.IsNullOrEmpty(item.ItemSettings.IconReference?.AssetGUID))
                {
                    var sprite = await CoreManager.Instance.AddressableAssetManager
                        .LoadSpriteAsync(item.ItemSettings.IconReference.AssetGUID);

                    if (_isDestroyed)
                        return;

                    if (sprite != null)
                    {
                        image.sprite = sprite;
                        image.enabled = true;
                    }
                }

                if (text != null)
                    text.text = item.ItemSettings.ItemName;

                return;
            }

            if (image != null)
            {
                image.sprite = _defaultItemSlotSprite;
                image.enabled = _defaultItemSlotSprite != null;
            }

            if (text != null)
                text.text = defaultName;
        }

        /// <summary>
        /// Refreshes the displayed stats using inspector-configured colors.
        /// </summary>
        private void UpdateStatsDisplay()
        {
            if (_selectedCharacter == null || _isDestroyed || _statsText == null)
                return;

            var baseStats = _selectedCharacter.GetSettingsStats;
            var itemsStats = _selectedCharacter.GetItemsStats;

            _statsText.text = CharactersStatsBase.GetColoredText(
                baseStats,
                itemsStats,
                _statsTotalColor,
                _statsBaseColor,
                _statsBonusColor);
        }

        /// <summary>
        /// Properly unsubscribes from all runtime events.
        /// </summary>
        private void OnDestroy()
        {
            _isDestroyed = true;

            if (_charactersManager != null)
                _charactersManager.OnDataChanged -= OnCharactersDataChanged;

            UnsubscribeFromSelectedCharacter(_selectedCharacter);

            foreach (var slot in _availableSlots)
                UnsubscribeSlot(slot);

            foreach (var slot in _selectedSlots)
                UnsubscribeSlot(slot);

            _availableSlots.Clear();
            _selectedSlots.Clear();
        }
    }
}