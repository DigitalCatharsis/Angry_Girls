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
        private IAssetProvider _assetProvider;

        public Action<ItemSettings> OnBuyRequested;
        public Action<InventoryItem> OnUseRequested;
        public Action<InventoryItem> OnSellRequested;

        public ItemSettings GetItemSettings() => _itemSettings;
        public InventoryItem GetInventoryItem() => _inventoryItem;
        public Button ActionButton => _actionButton;

        /// <summary>
        /// Initialize the slot with an asset provider.
        /// </summary>
        public void Initialize(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        /// <summary>
        /// Set up the slot for shop mode.
        /// </summary>
        public void SetupForShop(ItemSettings itemSettings)
        {
            _itemSettings = itemSettings;
            _inventoryItem = null;
            _mode = ItemSlotMode.Shop;

            Debug.Log($"ItemSlotUI: SetupForShop called for item '{itemSettings?.ItemName ?? "NULL"}'. ActionButton interactable: {_actionButton?.interactable ?? false}");

            UpdateUI();
        }

        /// <summary>
        /// Set up the slot for inventory mode.
        /// </summary>
        public void SetupForInventory(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
            _itemSettings = inventoryItem?.ItemSettings;
            _quantityText.text = inventoryItem?.Quantity.ToString() ?? "0";
            _mode = ItemSlotMode.Inventory;

            Debug.Log($"ItemSlotUI: SetupForInventory called for item '{inventoryItem?.ItemSettings.ItemName ?? "NULL"}'. ActionButton interactable: {_actionButton?.interactable ?? false}");

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
        private async void UpdateUI()
        {
            if (_itemSettings == null)
            {
                ClearUI();
                return;
            }

            if (_assetProvider != null && !string.IsNullOrEmpty(_itemSettings.IconReference?.AssetGUID))
            {
                var sprite = await _assetProvider.LoadSpriteAsync(_itemSettings.IconReference.AssetGUID);

                if (this == null)
                {
                    Debug.LogWarning($"ItemSlotUI: GameObject for item '{_itemSettings.ItemName}' was destroyed/returned to pool during sprite loading. Aborting UI update.");
                    return;
                }

                if (sprite != null)
                {
                    _iconImage.sprite = sprite;
                    _iconImage.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"ItemSlotUI: Could not load sprite for item '{_itemSettings.ItemName}' using GUID '{_itemSettings.IconReference.AssetGUID}'.");
                    _iconImage.enabled = false;
                }
            }
            else
            {
                if (this != null && _iconImage != null)
                {
                    _iconImage.enabled = false;
                }
            }

            if (this != null && _itemNameText != null)
            {
                _itemNameText.text = _itemSettings.ItemName;
            }

            if (this != null)
            {
                switch (_mode)
                {
                    case ItemSlotMode.Shop:
                        if (_priceText != null)
                        {
                            _priceText.text = _itemSettings.Price.ToString();
                            _priceText.gameObject.SetActive(true);
                        }
                        if (_quantityText != null)
                        {
                            _quantityText.gameObject.SetActive(false);
                        }
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
                            int sellPrice = Mathf.RoundToInt(_itemSettings.Price * 0.5f);
                            _priceText.text = $"Sell: {sellPrice}";
                            _priceText.gameObject.SetActive(true);
                        }
                        if (_quantityText != null)
                        {
                            _quantityText.gameObject.SetActive(true);
                        }
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
        }

        /// <summary>
        /// Cleanup when the slot is returned to the pool.
        /// </summary>
        protected override void OnDispose()
        {
            _itemSettings = null;
            _inventoryItem = null;
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

            Debug.Log($"ItemSlotUI: Returned to pool and disposed. GameObject: {gameObject.name}, Active: {gameObject.activeSelf}");
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