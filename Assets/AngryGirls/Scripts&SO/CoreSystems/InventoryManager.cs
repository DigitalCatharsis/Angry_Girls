using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    [Serializable]
    public class InventoryItemSaveData
    {
        public string itemSettingsUniqueId = "";

        public InventoryItemSaveData() { }

        public InventoryItemSaveData(string uniqueId)
        {
            itemSettingsUniqueId = uniqueId;
        }
    }

    public class InventoryItem
    {
        public ItemSettings ItemSettings { get; }
        public int Quantity { get; set; } = 1;
        public bool IsEquipped { get; set; } = false;
        public int UpgradeLevel { get; set; } = 1;

        public InventoryItem(ItemSettings settings, int quantity = 1)
        {
            ItemSettings = settings;
        }

        public float GetTotalHealthBonus() => ItemSettings.HealthBonus;
        public float GetTotalDamageBonus() => ItemSettings.DamageBonus;
    }

    /// <summary>
    /// Save data container for all inventory items
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        public List<InventoryItemSaveData> items = new();
    }

    /// <summary>
    /// Container for player's inventory items
    /// </summary>
    [Serializable]
    public class InventoryData : ISaveData<InventoryData, InventorySaveData>
    {
        [SerializeField] private List<InventoryItem> _items = new();

        public List<InventoryItem> Items => _items;

        public void ResetData()
        {
            _items.Clear();
        }

        public async UniTask UpdateFromSaveAsync(InventorySaveData saveData)
        {
            if (saveData == null)
            {
                throw new Exception("InventoryData save is null");
            }

            _items.Clear();

            if (saveData?.items == null) return;

            foreach (var itemSaveData in saveData.items)
            {
                if (string.IsNullOrEmpty(itemSaveData.itemSettingsUniqueId)) continue;

                var itemSettings = await CoreManager.Instance.AddressableAssetManager
                    .LoadScriptableObjectAsync<ItemSettings>(itemSaveData.itemSettingsUniqueId);

                if (itemSettings != null)
                {
                    var inventoryItem = new InventoryItem(itemSettings);
                    _items.Add(inventoryItem);
                }
                else
                {
                    Debug.LogWarning($"InventoryData.UpdateFromSave: Failed to load ItemSettings for GUID {itemSaveData.itemSettingsUniqueId}");
                }
            }
        }

        public InventorySaveData ConvertToSaveData()
        {
            var saveData = new InventorySaveData { items = new List<InventoryItemSaveData>() };

            foreach (var item in _items)
            {
                if (item?.ItemSettings != null)
                {
                    saveData.items.Add(new InventoryItemSaveData
                    {
                        itemSettingsUniqueId = CoreManager.Instance.AddressableAssetManager.GetAssetGuid(item.ItemSettings),
                    });
                }
            }

            return saveData;
        }
    }

    /// <summary>
    /// Manages player's inventory items
    /// </summary>
    public class InventoryManager : ISaveReinitManager<DefaultSaveTemplate, InventorySaveData, InventoryData>
    {
        private InventoryData _inventoryData = new();
        public InventoryData InventoryData => _inventoryData;

        public event Action OnDataChanged;

        public void ResetManagersData()
        {
            _inventoryData.ResetData();
            OnDataChanged?.Invoke();
        }

        public async UniTask ReinitDataFromTemplateAsync(DefaultSaveTemplate template)
        {
            _inventoryData.ResetData();

            if (template?.startingInventory == null) return;

            foreach (var itemTemplate in template.startingInventory)
            {
                if (itemTemplate?.itemSettingsReference == null) continue;

                var itemSettings = await CoreManager.Instance.AddressableAssetManager.LoadScriptableObjectAsync(itemTemplate.itemSettingsReference);

                if (itemSettings != null)
                {
                    var inventoryItem = new InventoryItem(itemSettings, itemTemplate.quantity);
                    _inventoryData.Items.Add(inventoryItem);
                    Debug.Log($"InventoryManager: Added {itemTemplate.quantity}x '{itemSettings.ItemName}' to starting inventory");
                }
                else
                {
                    Debug.LogWarning($"InventoryManager: Failed to load ItemSettings for starting inventory item");
                }
            }

            OnDataChanged?.Invoke();
        }

        public async UniTask ReinitDataFromSaveAsync(InventorySaveData saveData)
        {
            await _inventoryData.UpdateFromSaveAsync(saveData);
            OnDataChanged?.Invoke();
        }

        public InventorySaveData ConvertDataForSave()
        {
            return _inventoryData.ConvertToSaveData();
        }


        #region Existing inventory logic (preserved)

        /// <summary>
        /// Add item by ItemSettings. If item already exists, increase quantity.
        /// </summary>
        public void AddItem(ItemSettings itemSettings, int quantity = 1)
        {
            if (itemSettings == null) return;

            var existingItem = _inventoryData.Items.FirstOrDefault(i =>
                i.ItemSettings.UniqueId == itemSettings.UniqueId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _inventoryData.Items.Add(new InventoryItem(itemSettings, quantity));
            }

            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// Remove item from inventory. Returns true if item was removed.
        /// </summary>
        public bool RemoveItem(InventoryItem item)
        {
            if (item == null) return false;

            var existingItem = _inventoryData.Items.FirstOrDefault(i =>
                i.ItemSettings.UniqueId == item.ItemSettings.UniqueId);

            if (existingItem != null)
            {
                existingItem.Quantity -= item.Quantity;
                if (existingItem.Quantity <= 0)
                {
                    _inventoryData.Items.Remove(existingItem);
                }
                OnDataChanged?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sell item to shop for credits. Returns amount of credits received.
        /// </summary>
        public int SellItem(InventoryItem item)
        {
            if (item == null || item.ItemSettings == null) return 0;

            // Calculate sell price (50% of buy price)
            int sellPrice = Mathf.RoundToInt(item.ItemSettings.Price * 0.5f);

            // Remove item from inventory
            if (RemoveItem(item))
            {
                // Add credits to player
                var creditsManager = CoreManager.Instance.CreditsManager;
                creditsManager?.SetCredits(sellPrice);

                Debug.Log($"InventoryManager: Sold '{item.ItemSettings.ItemName}' for {sellPrice} credits");
                return sellPrice;
            }

            return 0;
        }

        /// <summary>
        /// Get all items in inventory
        /// </summary>
        public List<InventoryItem> GetAllItems()
        {
            return new List<InventoryItem>(_inventoryData.Items);
        }

        /// <summary>
        /// Get items filtered by type
        /// </summary>
        public List<InventoryItem> GetItemsByType(ItemType type)
        {
            return _inventoryData.Items
                .Where(item => item.ItemSettings.ItemType == type)
                .ToList();
        }

        /// <summary>
        /// Check if inventory contains item by UniqueId
        /// </summary>
        public bool ContainsItem(string uniqueId)
        {
            return _inventoryData.Items.Any(item => item.ItemSettings.UniqueId == uniqueId);
        }

        /// <summary>
        /// Get quantity of item by UniqueId
        /// </summary>
        public int GetQuantity(string uniqueId)
        {
            var item = _inventoryData.Items.FirstOrDefault(i => i.ItemSettings.UniqueId == uniqueId);
            return item?.Quantity ?? 0;
        }
        #endregion
    }
}