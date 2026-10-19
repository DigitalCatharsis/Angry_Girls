using System;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Display mode for the item slot.
    /// </summary>
    public enum ItemSlotMode
    {
        Shop,
        Inventory
    }

    /// <summary>
    /// UI slot for displaying and interacting with items in shop or inventory.
    /// Identical items are shown as a single slot with a quantity label.
    /// </summary>
    public class UI_ItemSlot : PoolObject
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMPro.TextMeshProUGUI _itemNameText;
        [SerializeField] private TMPro.TextMeshProUGUI _priceText;
        [SerializeField] private TMPro.TextMeshProUGUI _quantityText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TMPro.TextMeshProUGUI _actionButtonText;

        private ItemSettings _itemSettings;
        private InventoryItem _inventoryItem;
        private ItemSlotMode _mode;
        private int _shopQuantity = 1;

        public Action<ItemSettings> OnBuyRequested;
        public Action<InventoryItem> OnUseRequested;
        public Action<InventoryItem> OnSellRequested;

        public ItemSettings GetItemSettings() => _itemSettings;
        public InventoryItem GetInventoryItem() => _inventoryItem;
        public Button ActionButton => _actionButton;

        /// <summary>
        /// Set up the slot for shop mode. Quantity is the amount of identical items in stock.
        /// </summary>
        public void SetupForShop(ItemSettings itemSettings, int quantity = 1)
        {
            _itemSettings = itemSettings;
            _inventoryItem = null;
            _shopQuantity = Mathf.Max(1, quantity);
            _mode = ItemSlotMode.Shop;

            UpdateUI();
        }

        /// <summary>
        /// Set up the slot for inventory mode. The quantity is read from the inventory stack.
        /// </summary>
        public void SetupForInventory(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
            _itemSettings = inventoryItem?.ItemSettings;
            _mode = ItemSlotMode.Inventory;

            UpdateUI();
        }

        /// <summary>
        /// Set custom label text (used for special actions like "Unequip" in equipment popup).
        /// </summary>
        public void SetLabel(string label)
        {
            if (_itemNameText != null)
            {
                _itemNameText.text = label;
                _itemNameText.color = Color.gray;
            }

            if (_iconImage != null)
            {
                _iconImage.enabled = false;
            }

            if (_priceText != null)
            {
                _priceText.gameObject.SetActive(false);
            }

            if (_quantityText != null)
            {
                _quantityText.gameObject.SetActive(false);
            }

            if (_actionButton != null)
            {
                _actionButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Update the UI based on current mode and item data.
        /// </summary>
        private void UpdateUI()
        {
            if (_itemSettings == null)
            {
                ClearUI();
                return;
            }

            if (_iconImage != null)
            {
                _iconImage.sprite = _itemSettings.Icon;
                _iconImage.enabled = _itemSettings.Icon != null;
            }

            if (_itemNameText != null)
            {
                _itemNameText.text = _itemSettings.ItemName;
            }

            switch (_mode)
            {
                case ItemSlotMode.Shop:
                    if (_priceText != null)
                    {
                        _priceText.text = _itemSettings.Price.ToString();
                        _priceText.gameObject.SetActive(true);
                    }
                    SetQuantityLabel(_shopQuantity);
                    if (_actionButton != null && _actionButtonText != null)
                    {
                        _actionButtonText.text = "Buy";
                        _actionButton.onClick.RemoveAllListeners();
                        _actionButton.onClick.AddListener(() => OnBuyRequested?.Invoke(_itemSettings));
                        _actionButton.gameObject.SetActive(true);
                        _actionButton.interactable = true;
                    }
                    break;

                case ItemSlotMode.Inventory:
                    if (_priceText != null)
                    {
                        _priceText.text = $"Sell: {InventoryManager.GetSellPrice(_itemSettings)}";
                        _priceText.gameObject.SetActive(true);
                    }
                    SetQuantityLabel(_inventoryItem != null ? _inventoryItem.Quantity : 0);
                    if (_actionButton != null && _actionButtonText != null)
                    {
                        _actionButtonText.text = "Sell";
                        _actionButton.onClick.RemoveAllListeners();
                        _actionButton.onClick.AddListener(() => OnSellRequested?.Invoke(_inventoryItem));
                        _actionButton.gameObject.SetActive(true);
                        _actionButton.interactable = true;
                    }
                    break;
            }
        }

        private void SetQuantityLabel(int quantity)
        {
            if (_quantityText == null) return;

            _quantityText.text = $"x{quantity}";
            _quantityText.gameObject.SetActive(true);
        }

        /// <summary>
        /// Cleanup when the slot is returned to the pool.
        /// </summary>
        protected override void OnDispose()
        {
            _itemSettings = null;
            _inventoryItem = null;
            _shopQuantity = 1;
            _mode = ItemSlotMode.Shop;

            OnBuyRequested = null;
            OnUseRequested = null;
            OnSellRequested = null;

            ClearUI();

            if (_actionButton != null)
            {
                _actionButton.onClick.RemoveAllListeners();
                _actionButton.interactable = false;
            }
        }

        /// <summary>
        /// Return the object to the pool.
        /// </summary>
        protected override void ReturnToPool()
        {
            CoreManager.Instance.PoolManager.AddObject(UIObjectType.PRE_ItemSlot, CoreManager.Instance.PoolManager.UIObjectTypeDictionary, this);
        }

        /// <summary>
        /// Clear the UI components.
        /// </summary>
        private void ClearUI()
        {
            if (this != null && _iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }
            if (this != null && _itemNameText != null)
            {
                _itemNameText.text = "";
            }
            if (this != null && _priceText != null)
            {
                _priceText.text = "";
                _priceText.gameObject.SetActive(false);
            }
            if (this != null && _quantityText != null)
            {
                _quantityText.text = "";
                _quantityText.gameObject.SetActive(false);
            }
            if (this != null && _actionButton != null && _actionButtonText != null)
            {
                _actionButtonText.text = "";
                _actionButton.gameObject.SetActive(false);
                _actionButton.interactable = false;
            }
        }

        /// <summary>
        /// Highlight the slot.
        /// </summary>
        public void Highlight(bool isHighlighted)
        {
            if (this != null)
            {
                var image = GetComponent<Image>();
                if (image != null)
                {
                    image.color = isHighlighted ? Color.yellow : Color.white;
                }
            }
        }
    }
}
