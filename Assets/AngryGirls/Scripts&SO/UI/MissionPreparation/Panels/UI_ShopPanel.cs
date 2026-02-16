using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Panel for shop and inventory operations.
    /// </summary>
    public class UI_ShopPanel : MonoBehaviour, IUIPanel
    {
        [Header("UI References")]
        [SerializeField] private Transform _itemsContainer;

        [Header("Filter Buttons")]
        [SerializeField] private Button _allButton;
        [SerializeField] private Button _weaponsButton;
        [SerializeField] private Button _armorButton;
        [SerializeField] private Button _accessoriesButton;

        [Header("Purchase Mode Buttons")]
        [SerializeField] private Button _shoppingButton;
        [SerializeField] private Button _sellingButton;

        [Header("Account Display")]
        [SerializeField] private TextMeshProUGUI _accountCreditsText;

        [Header("Selected Item Details")]
        [SerializeField] private Image _selectedItemIcon;
        [SerializeField] private TextMeshProUGUI _selectedItemNameText;
        [SerializeField] private TextMeshProUGUI _selectedItemRarityText;
        [SerializeField] private TextMeshProUGUI _selectedItemLevelText;
        [SerializeField] private TextMeshProUGUI _selectedItemHpText;
        [SerializeField] private TextMeshProUGUI _selectedItemAttackText;
        [SerializeField] private TextMeshProUGUI _selectedItemDefenceText;
        [SerializeField] private TextMeshProUGUI _selectedItemEvadeText;
        [SerializeField] private TextMeshProUGUI _selectedItemCritText;
        [SerializeField] private TextMeshProUGUI _selectedItemPriceText;

        [Header("Action Buttons")]
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _refreshButton;

        private IAssetProvider _assetProvider;
        private ShopManager _shopManager;
        private InventoryManager _inventoryManager;
        private CreditsManager _creditsManager;

        private List<PoolObject> _activeItemSlots = new List<PoolObject>();

        private ItemSettings _selectedItemSettings;
        private InventoryItem _selectedInventoryItem;

        private PurchaseMode _currentPurchaseMode = PurchaseMode.Shopping;
        private ItemType _currentFilter = ItemType.None;

        private Dictionary<PurchaseMode, Button> _modeButtons = new Dictionary<PurchaseMode, Button>();
        private Dictionary<ItemType, Button> _filterButtons = new Dictionary<ItemType, Button>();

        [SerializeField] private int _refreshPrice = 500;

        private enum PurchaseMode
        {
            Shopping,
            Selling
        }

        /// <summary>
        /// Initialize the shop panel with required dependencies.
        /// </summary>
        public void Initialize(CoreManager coreManager)
        {
            _assetProvider = coreManager.AddressableAssetManager;
            _shopManager = coreManager.ShopManager;
            _inventoryManager = coreManager.InventoryManager;
            _creditsManager = coreManager.CreditsManager;

            _creditsManager.OnDataChanged += UpdateAccountDisplay;

            SetupButtons();
            UpdateAccountDisplay();

            SetPurchaseMode(PurchaseMode.Shopping, true);
            SetFilter(ItemType.None, true);
        }

        private void OnDestroy()
        {
            if (_creditsManager != null)
            {
                _creditsManager.OnDataChanged -= UpdateAccountDisplay;
            }

            ClearAllItemSlots();
        }

        private void UpdatePurchaseButtonText()
        {
            if (_purchaseButton != null)
            {
                var textComponent = _purchaseButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textComponent != null)
                {
                    string buttonText = _currentPurchaseMode == PurchaseMode.Shopping ? "Buy" : "Sell";
                    textComponent.text = buttonText;
                    ColorDebugLog.Log($"ShopPanel: UpdatePurchaseButtonText - set to '{buttonText}' for mode {_currentPurchaseMode}.", System.Drawing.KnownColor.Goldenrod);
                }
                else
                {
                    ColorDebugLog.Log($"ShopPanel: UpdatePurchaseButtonText - TextMeshProUGUI component not found on _purchaseButton or its children.", System.Drawing.KnownColor.Red);
                }
            }
        }

        private void SetupButtons()
        {
            if (_allButton != null) { _filterButtons[ItemType.None] = _allButton; _allButton.onClick.AddListener(() => SetFilter(ItemType.None)); }
            if (_weaponsButton != null) { _filterButtons[ItemType.Weapon] = _weaponsButton; _weaponsButton.onClick.AddListener(() => SetFilter(ItemType.Weapon)); }
            if (_armorButton != null) { _filterButtons[ItemType.Armor] = _armorButton; _armorButton.onClick.AddListener(() => SetFilter(ItemType.Armor)); }
            if (_accessoriesButton != null) { _filterButtons[ItemType.Accessory] = _accessoriesButton; _accessoriesButton.onClick.AddListener(() => SetFilter(ItemType.Accessory)); }

            if (_shoppingButton != null) { _modeButtons[PurchaseMode.Shopping] = _shoppingButton; _shoppingButton.onClick.AddListener(() => SetPurchaseMode(PurchaseMode.Shopping)); }
            if (_sellingButton != null) { _modeButtons[PurchaseMode.Selling] = _sellingButton; _sellingButton.onClick.AddListener(() => SetPurchaseMode(PurchaseMode.Selling)); }

            if (_purchaseButton != null) _purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
            if (_refreshButton != null) _refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        private void SetPurchaseMode(PurchaseMode mode, bool isInitialization = false)
        {
            ColorDebugLog.Log($"ShopPanel: SetPurchaseMode called. Mode: {mode}, Initialization: {isInitialization}", System.Drawing.KnownColor.Goldenrod);

            _currentPurchaseMode = mode;
            if (!isInitialization)
            {
                DeactivateAllModeButtons();
            }
            ActivateModeButton(mode);
            Refresh();

            UpdatePurchaseButtonText();

            if (_refreshButton != null)
            {
                _refreshButton.gameObject.SetActive(mode == PurchaseMode.Shopping);
                ColorDebugLog.Log($"ShopPanel: Refresh button {(mode == PurchaseMode.Shopping ? "enabled" : "disabled")} for mode {mode}.", System.Drawing.KnownColor.Goldenrod);
            }
        }

        private void SetFilter(ItemType type, bool isInitialization = false)
        {
            ColorDebugLog.Log($"ShopPanel: SetFilter called. Type: {type}, Initialization: {isInitialization}", System.Drawing.KnownColor.Goldenrod);

            _currentFilter = type;
            if (!isInitialization)
            {
                DeactivateAllFilterButtons();
            }
            ActivateFilterButton(type);
            Refresh();
        }

        private void ActivateModeButton(PurchaseMode mode)
        {
            if (_modeButtons.TryGetValue(mode, out var button) && button != null)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.yellow;
                }
            }
        }

        private void DeactivateAllModeButtons()
        {
            foreach (var button in _modeButtons.Values)
            {
                if (button != null)
                {
                    var image = button.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = Color.white;
                    }
                }
            }
        }

        private void ActivateFilterButton(ItemType type)
        {
            if (_filterButtons.TryGetValue(type, out var button) && button != null)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.yellow;
                }
            }
        }

        private void DeactivateAllFilterButtons()
        {
            foreach (var button in _filterButtons.Values)
            {
                if (button != null)
                {
                    var image = button.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = Color.white;
                    }
                }
            }
        }

        /// <summary>
        /// Refresh the panel display.
        /// </summary>
        public void Refresh()
        {
            ColorDebugLog.Log("ShopPanel: Refresh called.", System.Drawing.KnownColor.Goldenrod);
            UpdateItemsDisplay();
            UpdateAccountDisplay();
            ClearSelectedItemDetails();
            UpdatePurchaseButtonText();
        }

        private void UpdateItemsDisplay()
        {
            ColorDebugLog.Log("ShopPanel: UpdateItemsDisplay called. Clearing all slots.", System.Drawing.KnownColor.Goldenrod);
            ClearAllItemSlots();

            List<ItemSettings> itemsToDisplay = new();
            List<InventoryItem> inventoryItemsToDisplay = new();

            if (_currentPurchaseMode == PurchaseMode.Shopping)
            {
                var currentAssortment = _shopManager.GetCurrentAssortment();
                if (currentAssortment != null)
                {
                    itemsToDisplay = currentAssortment.ToList();
                    ColorDebugLog.Log($"ShopPanel: Shopping mode. Got {itemsToDisplay.Count} items from ShopManager.", System.Drawing.KnownColor.Goldenrod);
                }
            }
            else if (_currentPurchaseMode == PurchaseMode.Selling)
            {
                var allItems = _inventoryManager.GetAllItems();
                inventoryItemsToDisplay = allItems.ToList();
                ColorDebugLog.Log($"ShopPanel: Selling mode. Got {inventoryItemsToDisplay.Count} items from InventoryManager.", System.Drawing.KnownColor.Goldenrod);
            }

            if (_currentFilter != ItemType.None)
            {
                if (_currentPurchaseMode == PurchaseMode.Shopping)
                {
                    itemsToDisplay = itemsToDisplay.Where(item => item.ItemType == _currentFilter).ToList();
                    ColorDebugLog.Log($"ShopPanel: Applied filter '{_currentFilter}' to shopping items. Remaining: {itemsToDisplay.Count}", System.Drawing.KnownColor.Goldenrod);
                }
                else
                {
                    inventoryItemsToDisplay = inventoryItemsToDisplay.Where(item => item.ItemSettings.ItemType == _currentFilter).ToList();
                    ColorDebugLog.Log($"ShopPanel: Applied filter '{_currentFilter}' to selling items. Remaining: {inventoryItemsToDisplay.Count}", System.Drawing.KnownColor.Goldenrod);
                }
            }

            int targetCount = _currentPurchaseMode == PurchaseMode.Shopping ? itemsToDisplay.Count : inventoryItemsToDisplay.Count;
            ColorDebugLog.Log($"ShopPanel: Target count for slots: {targetCount}", System.Drawing.KnownColor.Goldenrod);

            if (_currentPurchaseMode == PurchaseMode.Shopping)
            {
                foreach (var itemSettings in itemsToDisplay)
                {
                    var slotObject = CoreManager.Instance.PoolManager.GetObject<UIObjectType>(UIObjectType.PRE_ItemSlot, Vector3.zero, Quaternion.identity);
                    if (slotObject == null)
                    {
                        ColorDebugLog.Log("ShopPanel: Failed to get ItemSlotUI from PoolManager.", System.Drawing.KnownColor.Red);
                        continue;
                    }

                    var slotInstance = slotObject as UI_ItemSlot;
                    if (slotInstance == null)
                    {
                        ColorDebugLog.Log("ShopPanel: PoolObject is not of type ItemSlotUI.", System.Drawing.KnownColor.Red);
                        CoreManager.Instance.PoolManager.AddObject(UIObjectType.PRE_ItemSlot, CoreManager.Instance.PoolManager.UIObjectTypeDictionary, slotObject);
                        continue;
                    }

                    slotInstance.transform.SetParent(_itemsContainer, false);
                    var localPos = slotInstance.transform.localPosition;
                    localPos.z = 0f;
                    slotInstance.transform.localPosition = localPos;
                    slotInstance.transform.localRotation = Quaternion.identity;

                    slotInstance.Initialize(_assetProvider);
                    slotInstance.OnBuyRequested = (settings) => OnItemSlotClicked(settings, null);
                    slotInstance.SetupForShop(itemSettings);

                    _activeItemSlots.Add(slotObject);
                    ColorDebugLog.Log($"ShopPanel: Spawned ItemSlotUI for item '{itemSettings.ItemName}'. Active slots now: {_activeItemSlots.Count}", System.Drawing.KnownColor.Goldenrod);
                }
            }
            else
            {
                foreach (var inventoryItem in inventoryItemsToDisplay)
                {
                    var slotObject = CoreManager.Instance.PoolManager.GetObject<UIObjectType>(UIObjectType.PRE_ItemSlot, Vector3.zero, Quaternion.identity);
                    if (slotObject == null)
                    {
                        ColorDebugLog.Log("ShopPanel: Failed to get ItemSlotUI from PoolManager.", System.Drawing.KnownColor.Red);
                        continue;
                    }

                    var slotInstance = slotObject as UI_ItemSlot;
                    if (slotInstance == null)
                    {
                        ColorDebugLog.Log("ShopPanel: PoolObject is not of type ItemSlotUI.", System.Drawing.KnownColor.Red);
                        CoreManager.Instance.PoolManager.AddObject(UIObjectType.PRE_ItemSlot, CoreManager.Instance.PoolManager.UIObjectTypeDictionary, slotObject);
                        continue;
                    }

                    slotInstance.transform.SetParent(_itemsContainer, false);
                    var localPos = slotInstance.transform.localPosition;
                    localPos.z = 0f;
                    slotInstance.transform.localPosition = localPos;
                    slotInstance.transform.localRotation = Quaternion.identity;

                    slotInstance.Initialize(_assetProvider);
                    slotInstance.OnSellRequested = (item) => OnItemSlotClicked(null, item);
                    slotInstance.SetupForInventory(inventoryItem);

                    _activeItemSlots.Add(slotObject);
                    ColorDebugLog.Log($"ShopPanel: Spawned ItemSlotUI for inventory item '{inventoryItem.ItemSettings.ItemName}'. Active slots now: {_activeItemSlots.Count}", System.Drawing.KnownColor.Goldenrod);
                }
            }
        }

        private void OnItemSlotClicked(ItemSettings itemSettings, InventoryItem inventoryItem)
        {
            ColorDebugLog.Log($"ShopPanel: OnItemSlotClicked called. ItemSettings: {itemSettings?.ItemName ?? "NULL"}, InventoryItem: {inventoryItem?.ItemSettings.ItemName ?? "NULL"}", System.Drawing.KnownColor.Goldenrod);

            ClearItemHighlights();

            if (_currentPurchaseMode == PurchaseMode.Shopping && itemSettings != null)
            {
                _selectedItemSettings = itemSettings;
                _selectedInventoryItem = null;
                var clickedSlot = _activeItemSlots.FirstOrDefault(slot => (slot as UI_ItemSlot)?.GetItemSettings() == itemSettings);
                (clickedSlot as UI_ItemSlot)?.Highlight(true);
                UpdateSelectedItemDetails(itemSettings, null);
            }
            else if (_currentPurchaseMode == PurchaseMode.Selling && inventoryItem != null)
            {
                _selectedInventoryItem = inventoryItem;
                _selectedItemSettings = null;
                var clickedSlot = _activeItemSlots.FirstOrDefault(slot => (slot as UI_ItemSlot)?.GetInventoryItem() == inventoryItem);
                (clickedSlot as UI_ItemSlot)?.Highlight(true);
                UpdateSelectedItemDetails(inventoryItem.ItemSettings, inventoryItem);
            }
        }

        private void ClearItemHighlights()
        {
            ColorDebugLog.Log("ShopPanel: ClearItemHighlights called.", System.Drawing.KnownColor.Goldenrod);
            foreach (var slot in _activeItemSlots)
            {
                (slot as UI_ItemSlot)?.Highlight(false);
            }
        }

        private void UpdateSelectedItemDetails(ItemSettings itemSettings, InventoryItem inventoryItem = null)
        {
            ColorDebugLog.Log($"ShopPanel: UpdateSelectedItemDetails STARTED for item '{itemSettings?.ItemName ?? "NULL"}'", System.Drawing.KnownColor.Goldenrod);

            if (itemSettings == null)
            {
                ClearSelectedItemDetails();
                return;
            }

            LoadAndDisplayIcon(itemSettings);

            if (_selectedItemNameText != null)
            {
                _selectedItemNameText.text = $"Name: {itemSettings.ItemName}";
            }

            if (_selectedItemRarityText != null)
            {
                _selectedItemRarityText.text = $"Rarity: {itemSettings.ItemRarity}";
            }

            if (_selectedItemLevelText != null)
            {
                int level = inventoryItem?.UpgradeLevel ?? 1;
                _selectedItemLevelText.text = $"Level: {level}";
            }

            if (_selectedItemHpText != null)
            {
                float hpBonus = inventoryItem != null ? inventoryItem.GetTotalHealthBonus() : itemSettings.HealthBonus;
                _selectedItemHpText.text = $"HP: {hpBonus}";
            }

            if (_selectedItemAttackText != null)
            {
                float attackBonus = inventoryItem != null ? inventoryItem.GetTotalDamageBonus() : itemSettings.DamageBonus;
                _selectedItemAttackText.text = $"Attack: {attackBonus}";
            }

            if (_selectedItemDefenceText != null)
            {
                _selectedItemDefenceText.text = $"Defence: 0";
            }

            if (_selectedItemEvadeText != null)
            {
                _selectedItemEvadeText.text = $"Evade: 0";
            }

            if (_selectedItemCritText != null)
            {
                _selectedItemCritText.text = $"Crit: 0";
            }

            if (_selectedItemPriceText != null)
            {
                int price = itemSettings.Price;
                if (_currentPurchaseMode == PurchaseMode.Selling)
                {
                    price = Mathf.RoundToInt(price * 0.5f);
                    if (inventoryItem != null)
                    {
                        price *= inventoryItem.Quantity;
                    }
                }
                _selectedItemPriceText.text = $"Item price: {price}";
            }

            UpdatePurchaseButtonText();
        }

        private async void LoadAndDisplayIcon(ItemSettings itemSettings)
        {
            ColorDebugLog.Log($"ShopPanel: LoadAndDisplayIcon called for item '{itemSettings.ItemName}'", System.Drawing.KnownColor.Goldenrod);

            if (_selectedItemIcon == null || _assetProvider == null || string.IsNullOrEmpty(itemSettings.IconReference?.AssetGUID))
            {
                if (_selectedItemIcon != null)
                {
                    _selectedItemIcon.enabled = false;
                }
                ColorDebugLog.Log($"ShopPanel: LoadAndDisplayIcon - conditions not met for item '{itemSettings.ItemName}'. Icon disabled.", System.Drawing.KnownColor.Goldenrod);
                return;
            }

            var sprite = await _assetProvider.LoadSpriteAsync(itemSettings.IconReference.AssetGUID);
            if (sprite != null)
            {
                _selectedItemIcon.sprite = sprite;
                _selectedItemIcon.enabled = true;
                ColorDebugLog.Log($"ShopPanel: LoadAndDisplayIcon - loaded sprite for item '{itemSettings.ItemName}'.", System.Drawing.KnownColor.Goldenrod);
            }
            else
            {
                ColorDebugLog.Log($"ShopPanel: LoadAndDisplayIcon - could not load sprite for item '{itemSettings.ItemName}' using GUID '{itemSettings.IconReference.AssetGUID}'.", System.Drawing.KnownColor.Orange);
                _selectedItemIcon.enabled = false;
            }
        }

        private void ClearSelectedItemDetails()
        {
            ColorDebugLog.Log("ShopPanel: ClearSelectedItemDetails called.", System.Drawing.KnownColor.Goldenrod);
            if (_selectedItemIcon != null)
            {
                _selectedItemIcon.sprite = null;
                _selectedItemIcon.enabled = false;
            }
            if (_selectedItemNameText != null) _selectedItemNameText.text = "";
            if (_selectedItemRarityText != null) _selectedItemRarityText.text = "";
            if (_selectedItemLevelText != null) _selectedItemLevelText.text = "";
            if (_selectedItemHpText != null) _selectedItemHpText.text = "";
            if (_selectedItemAttackText != null) _selectedItemAttackText.text = "";
            if (_selectedItemDefenceText != null) _selectedItemDefenceText.text = "";
            if (_selectedItemEvadeText != null) _selectedItemEvadeText.text = "";
            if (_selectedItemCritText != null) _selectedItemCritText.text = "";
            if (_selectedItemPriceText != null) _selectedItemPriceText.text = "";
        }

        private void UpdateAccountDisplay()
        {
            var credits = _creditsManager.GetCredits();

            if (_accountCreditsText != null)
            {
                _accountCreditsText.text = $"Account: {credits}";
                ColorDebugLog.Log($"ShopPanel: UpdateAccountDisplay - updated to {credits} credits.", System.Drawing.KnownColor.Goldenrod);
            }
        }

        private void OnPurchaseButtonClicked()
        {
            ColorDebugLog.Log("ShopPanel: OnPurchaseButtonClicked called.", System.Drawing.KnownColor.Goldenrod);

            if (_currentPurchaseMode == PurchaseMode.Shopping)
            {
                if (_selectedItemSettings != null)
                {
                    ColorDebugLog.Log($"ShopPanel: Attempting to buy item '{_selectedItemSettings.ItemName}'.", System.Drawing.KnownColor.Goldenrod);
                    _shopManager.TryPurchaseItem(_selectedItemSettings);
                    _selectedItemSettings = null;
                    ClearItemHighlights();
                    Refresh();
                }
                else
                {
                    ColorDebugLog.Log("ShopPanel: No item selected for purchase.", System.Drawing.KnownColor.Goldenrod);
                }
            }
            else if (_currentPurchaseMode == PurchaseMode.Selling)
            {
                if (_selectedInventoryItem != null)
                {
                    ColorDebugLog.Log($"ShopPanel: Attempting to sell item '{_selectedInventoryItem.ItemSettings.ItemName}'.", System.Drawing.KnownColor.Goldenrod);
                    _inventoryManager.SellItem(_selectedInventoryItem);
                    _selectedInventoryItem = null;
                    ClearItemHighlights();
                    Refresh();
                }
                else
                {
                    ColorDebugLog.Log("ShopPanel: No item selected for sale.", System.Drawing.KnownColor.Goldenrod);
                }
            }
        }

        //TODO: fix refresh logic
        private async void OnRefreshButtonClicked()
        {
            ColorDebugLog.Log("ShopPanel: OnRefreshButtonClicked called.", System.Drawing.KnownColor.Magenta);

            if (_shopManager != null)
            {
                await _shopManager.RequestManualRefreshAsync();
                _creditsManager.SetCredits(-_refreshPrice);
                Refresh();
            }
        }

        private void ClearAllItemSlots()
        {
            ColorDebugLog.Log($"ShopPanel: ClearAllItemSlots called. Returning {_activeItemSlots.Count} slots to pool.", System.Drawing.KnownColor.Goldenrod);

            foreach (var slotObject in _activeItemSlots)
            {
                if (slotObject != null)
                {
                    var slotInstance = slotObject as UI_ItemSlot;
                    if (slotInstance != null)
                    {
                        var actionButton = slotInstance.ActionButton;
                        if (actionButton != null)
                        {
                            actionButton.onClick.RemoveAllListeners();
                        }
                    }
                    CoreManager.Instance.PoolManager.AddObject(UIObjectType.PRE_ItemSlot, CoreManager.Instance.PoolManager.UIObjectTypeDictionary, slotObject);
                    ColorDebugLog.Log($"ShopPanel: Returned slot to pool. Slot object: {slotObject?.name ?? "NULL"}.", System.Drawing.KnownColor.Goldenrod);
                }
            }
            _activeItemSlots.Clear();
            ColorDebugLog.Log("ShopPanel: Active item slots list cleared.", System.Drawing.KnownColor.Goldenrod);
        }
    }
}