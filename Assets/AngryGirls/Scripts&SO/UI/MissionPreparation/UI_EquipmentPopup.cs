using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

namespace Angry_Girls
{
    /// <summary>
    /// Popup window for equipping items to a character slot.
    /// </summary>
    public class UI_EquipmentPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private GameObject _itemSlotPrefab; // UI_EquipmentSlot prefab
        [SerializeField] private TextMeshProUGUI _statsPreviewText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _confirmButton;

        private int _targetAccessorySlotIndex = 0;

        private CharacterProfile _targetCharacter;
        private ItemType _targetSlotType;
        private InventoryItem _previewItem;

        private void Start()
        {
            _cancelButton.onClick.AddListener(Hide);
            _confirmButton.onClick.AddListener(ConfirmEquipment);
        }

        /// <summary>
        /// Show popup for equipping items to specified slot.
        /// </summary>
        public void Show(CharacterProfile character, ItemType slotType, int accessorySlotIndex = 0)
        {
            _targetCharacter = character;
            _targetSlotType = slotType;
            _targetAccessorySlotIndex = accessorySlotIndex;
            _previewItem = null;

            gameObject.SetActive(true);
            RefreshItemsList();
            UpdateStatsPreview();
        }

        private void RefreshItemsList()
        {
            // Clear existing slots
            foreach (Transform child in _contentContainer)
                Destroy(child.gameObject);

            // Get compatible items from inventory
            var compatibleItems = CoreManager.Instance.InventoryManager
                .GetAllItems()
                .Where(item => item.ItemSettings.ItemType == _targetSlotType)
                .ToList();

            // Add item slots
            foreach (var item in compatibleItems)
            {
                var slotGO = Instantiate(_itemSlotPrefab, _contentContainer);
                var slot = slotGO.GetComponent<UI_EquipmentSlot>();
                slot.Initialize(OnItemSlotClicked);
                slot.SetItem(item);
            }

            // Add "Unequip" option if slot has item
            InventoryItem currentEquipped = GetCurrentEquippedItem();
            if (currentEquipped != null)
            {
                var unequipGO = Instantiate(_itemSlotPrefab, _contentContainer);
                var slot = unequipGO.GetComponent<UI_EquipmentSlot>();
                slot.Initialize(OnUnequipClicked);
                slot.SetLabel("Unequip");
            }
        }

        private void OnItemSlotClicked(UI_EquipmentSlot slot, InventoryItem item)
        {
            _previewItem = item;
            UpdateStatsPreview();
        }

        private void OnUnequipClicked(UI_EquipmentSlot slot, InventoryItem item)
        {
            _previewItem = null; // null means unequip
            UpdateStatsPreview();
        }

        private void UpdateStatsPreview()
        {
            if (_targetCharacter == null)
            {
                _statsPreviewText.text = "No character selected";
                return;
            }

            var currentStats = _targetCharacter.GetSettingsStats;
            CharactersStatsBase previewStats;

            if (_previewItem == null)
            {
                // Preview unequip: remove current item from slot
                previewStats = CalculateStatsWithoutCurrentItem();
            }
            else
            {
                // Preview equip: replace current item with preview item
                previewStats = CalculateStatsWithPreviewItem();
            }

            // Format with color coding for deltas
            string hpText = FormatStatChange(currentStats.health, previewStats.health, "HP");
            string atkText = FormatStatChange(currentStats.damage, previewStats.damage, "ATK");

            _statsPreviewText.text = $"{hpText}\n{atkText}";
        }


        private CharactersStatsBase CalculateStatsWithoutCurrentItem()
        {
            var stats = new CharactersStatsBase(_targetCharacter.CharacterSettings.characterStats);

            // TODOOOOOOOOO
            if (_targetSlotType != ItemType.Weapon && _targetCharacter.Weapon != null)
            {
                stats.damage += _targetCharacter.Weapon.ItemSettings.DamageBonus;
                stats.health += _targetCharacter.Weapon.ItemSettings.HealthBonus;
            }

            if (_targetSlotType != ItemType.Armor && _targetCharacter.Armor != null)
            {
                stats.damage += _targetCharacter.Armor.ItemSettings.DamageBonus;
                stats.health += _targetCharacter.Armor.ItemSettings.HealthBonus;
            }

            if (_targetSlotType != ItemType.Accessory && _targetCharacter.Accessory1 != null)
            {
                stats.damage += _targetCharacter.Accessory1.ItemSettings.DamageBonus;
                stats.health += _targetCharacter.Accessory1.ItemSettings.HealthBonus;
            }

            if (_targetSlotType != ItemType.Accessory && _targetCharacter.Accessory2 != null)
            {
                stats.damage += _targetCharacter.Accessory2.ItemSettings.DamageBonus;
                stats.health += _targetCharacter.Accessory2.ItemSettings.HealthBonus;
            }

            return stats;
        }

        private CharactersStatsBase CalculateStatsWithPreviewItem()
        {
            var stats = new CharactersStatsBase(_targetCharacter.CharacterSettings.characterStats);

            // Weapon
            if (_targetSlotType == ItemType.Weapon && _previewItem != null)
            {
                stats.damage += _previewItem.ItemSettings.DamageBonus;
                stats.health += _previewItem.ItemSettings.HealthBonus;
            }
            else if (_targetCharacter.Weapon != null)
            {
                stats.damage += _targetCharacter.Weapon.ItemSettings.DamageBonus;
                stats.health += _targetCharacter.Weapon.ItemSettings.HealthBonus;
            }

            // Armor
            if (_targetSlotType == ItemType.Armor && _previewItem != null)
            {
                stats.damage += _previewItem.ItemSettings.DamageBonus;
                stats.health += _previewItem.ItemSettings.HealthBonus;
            }
            else if (_targetCharacter.Armor != null)
            {
                stats.damage += _targetCharacter.Armor.ItemSettings.DamageBonus;
                stats.health += _targetCharacter.Armor.ItemSettings.HealthBonus;
            }

            // Accessories
            if (_targetSlotType == ItemType.Accessory && _previewItem != null)
            {
                // Replace first non-null accessory slot
                if (_targetCharacter.Accessory1 == null)
                {
                    stats.damage += _previewItem.ItemSettings.DamageBonus;
                    stats.health += _previewItem.ItemSettings.HealthBonus;
                }
                else if (_targetCharacter.Accessory2 == null)
                {
                    stats.damage += _previewItem.ItemSettings.DamageBonus;
                    stats.health += _previewItem.ItemSettings.HealthBonus;
                }
                // If both slots full, we'd need swap logic (future enhancement)
            }
            else
            {
                if (_targetCharacter.Accessory1 != null)
                {
                    stats.damage += _targetCharacter.Accessory1.ItemSettings.DamageBonus;
                    stats.health += _targetCharacter.Accessory1.ItemSettings.HealthBonus;
                }
                if (_targetCharacter.Accessory2 != null)
                {
                    stats.damage += _targetCharacter.Accessory2.ItemSettings.DamageBonus;
                    stats.health += _targetCharacter.Accessory2.ItemSettings.HealthBonus;
                }
            }

            return stats;
        }

        private string FormatStatChange(float oldValue, float newValue, string label)
        {
            float delta = newValue - oldValue;
            string sign = delta >= 0 ? "+" : "";

            if (delta > 0)
                return $"<color=green>{label}: {Mathf.Round(newValue)} ({sign}{Mathf.Round(delta)})</color>";
            else if (delta < 0)
                return $"<color=red>{label}: {Mathf.Round(newValue)} ({sign}{Mathf.Round(delta)})</color>";
            else
                return $"{label}: {Mathf.Round(newValue)}";
        }

        private InventoryItem GetCurrentEquippedItem()
        {
            return _targetSlotType switch
            {
                ItemType.Weapon => _targetCharacter.Weapon,
                ItemType.Armor => _targetCharacter.Armor,
                ItemType.Accessory => _targetCharacter.Accessory1 ?? _targetCharacter.Accessory2,
                _ => null
            };
        }

        private void ConfirmEquipment()
        {
            if (_previewItem == null)
            {
                // Unequip from SPECIFIC slot
                _targetCharacter.UnequipItem(_targetSlotType, _targetAccessorySlotIndex);
            }
            else
            {
                // Equip to SPECIFIC slot
                _targetCharacter.EquipItem(_previewItem, _targetSlotType, _targetAccessorySlotIndex);
            }

            Hide();
        }

        private void Hide()
        {
            _previewItem = null;
            gameObject.SetActive(false);
        }
    }
}