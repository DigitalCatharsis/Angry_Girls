using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Persistent representation of a single inventory stack (or an equipped item).
    /// </summary>
    [Serializable]
    public class InventoryItemSaveData
    {
        public string itemSettingsUniqueId = "";

        /// <summary>
        /// Stack size. Defaults to 1 so saves created before quantities were persisted still load correctly.
        /// </summary>
        public int quantity = 1;

        public InventoryItemSaveData() { }

        public InventoryItemSaveData(string uniqueId, int quantity = 1)
        {
            itemSettingsUniqueId = uniqueId;
            this.quantity = quantity;
        }
    }

    /// <summary>
    /// A stack of identical items in the inventory (or a single equipped item).
    /// </summary>
    public class InventoryItem
    {
        public ItemSettings ItemSettings { get; }
        public int Quantity { get; set; } = 1;
        public bool IsEquipped { get; set; } = false;
        public int UpgradeLevel { get; set; } = 1;

        public InventoryItem(ItemSettings settings, int quantity = 1)
        {
            ItemSettings = settings;
            Quantity = Mathf.Max(1, quantity);
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
    /// Container for player's inventory items. Identical items are always kept as a single stack.
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

        /// <summary>
        /// Finds the stack that holds the given item, or null.
        /// </summary>
        public InventoryItem FindByUniqueId(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId)) return null;

            return _items.FirstOrDefault(i => i?.ItemSettings != null && i.ItemSettings.UniqueId == uniqueId);
        }

        /// <summary>
        /// Adds the given amount to the existing stack or creates a new one. Returns the resulting stack.
        /// </summary>
        public InventoryItem AddOrStack(ItemSettings itemSettings, int quantity)
        {
            if (itemSettings == null || quantity <= 0) return null;

            var existing = FindByUniqueId(itemSettings.UniqueId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                return existing;
            }

            var created = new InventoryItem(itemSettings, quantity);
            _items.Add(created);
            return created;
        }

        public UniTask UpdateFromSaveAsync(InventorySaveData saveData)
        {
            if (saveData == null)
            {
                throw new Exception("InventoryData save is null");
            }

            _items.Clear();

            if (saveData.items == null) return UniTask.CompletedTask;

            var repository = CoreManager.Instance.ItemSettingsRepository;

            foreach (var itemSaveData in saveData.items)
            {
                if (itemSaveData == null || string.IsNullOrEmpty(itemSaveData.itemSettingsUniqueId)) continue;

                var itemSettings = repository.GetItemByUniqueId(itemSaveData.itemSettingsUniqueId);

                if (itemSettings == null)
                {
                    Debug.LogWarning($"InventoryData.UpdateFromSave: ItemSettings not found for id {itemSaveData.itemSettingsUniqueId}");
                    continue;
                }

                // Merging also repairs saves that contain several entries of the same item.
                AddOrStack(itemSettings, Mathf.Max(1, itemSaveData.quantity));
            }

            return UniTask.CompletedTask;
        }

        public InventorySaveData ConvertToSaveData()
        {
            var saveData = new InventorySaveData { items = new List<InventoryItemSaveData>() };

            foreach (var item in _items)
            {
                if (item?.ItemSettings == null || item.Quantity <= 0) continue;

                saveData.items.Add(new InventoryItemSaveData(item.ItemSettings.UniqueId, item.Quantity));
            }

            return saveData;
        }
    }

    /// <summary>
    /// Manages player's inventory items
    /// </summary>
    public class InventoryManager : ISaveReinitManager<DefaultSaveTemplate, InventorySaveData, InventoryData>
    {
        /// <summary>
        /// Fraction of the purchase price paid when selling an item to the shop.
        /// </summary>
        private const float SellPriceFactor = 0.5f;

        private InventoryData _inventoryData = new();
        public InventoryData InventoryData => _inventoryData;

        public event Action OnDataChanged;

        /// <summary>
        /// Credits received for selling a single unit of the item.
        /// </summary>
        public static int GetSellPrice(ItemSettings itemSettings)
        {
            return itemSettings == null ? 0 : Mathf.RoundToInt(itemSettings.Price * SellPriceFactor);
        }

        public void ResetManagersData()
        {
            _inventoryData.ResetData();
            OnDataChanged?.Invoke();
        }

        public UniTask ReinitDataFromTemplateAsync(DefaultSaveTemplate template)
        {
            _inventoryData.ResetData();

            if (template?.startingInventory != null)
            {
                foreach (var itemTemplate in template.startingInventory)
                {
                    if (itemTemplate?.itemSettings == null) continue;

                    var quantity = Mathf.Max(1, itemTemplate.quantity);
                    _inventoryData.AddOrStack(itemTemplate.itemSettings, quantity);
                    Debug.Log($"InventoryManager: Added {quantity}x '{itemTemplate.itemSettings.ItemName}' to starting inventory");
                }
            }

            OnDataChanged?.Invoke();
            return UniTask.CompletedTask;
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

        #region Inventory logic

        /// <summary>
        /// Add items by ItemSettings. If the item already exists, the stack quantity is increased.
        /// </summary>
        public void AddItem(ItemSettings itemSettings, int quantity = 1)
        {
            if (itemSettings == null || quantity <= 0) return;

            _inventoryData.AddOrStack(itemSettings, quantity);
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// Remove the given amount (default one unit) from the stack that contains this item.
        /// Returns true when the requested amount was removed.
        /// </summary>
        public bool RemoveItem(InventoryItem item, int quantity = 1)
        {
            return item != null && RemoveItem(item.ItemSettings, quantity);
        }

        /// <summary>
        /// Remove the given amount (default one unit) of an item. The stack disappears when it reaches zero.
        /// </summary>
        public bool RemoveItem(ItemSettings itemSettings, int quantity = 1)
        {
            if (itemSettings == null || quantity <= 0) return false;

            var existingItem = _inventoryData.FindByUniqueId(itemSettings.UniqueId);
            if (existingItem == null || existingItem.Quantity < quantity) return false;

            existingItem.Quantity -= quantity;
            if (existingItem.Quantity <= 0)
            {
                _inventoryData.Items.Remove(existingItem);
            }

            OnDataChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Sell items to the shop for credits (default one unit). Returns the amount of credits received.
        /// </summary>
        public int SellItem(InventoryItem item, int quantity = 1)
        {
            if (item?.ItemSettings == null || quantity <= 0) return 0;

            var itemSettings = item.ItemSettings;
            var totalPrice = GetSellPrice(itemSettings) * quantity;

            if (!RemoveItem(itemSettings, quantity)) return 0;

            CoreManager.Instance.CreditsManager?.SetCredits(totalPrice);

            Debug.Log($"InventoryManager: Sold {quantity}x '{itemSettings.ItemName}' for {totalPrice} credits");
            return totalPrice;
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
            return _inventoryData.FindByUniqueId(uniqueId) != null;
        }

        /// <summary>
        /// Get quantity of item by UniqueId
        /// </summary>
        public int GetQuantity(string uniqueId)
        {
            return _inventoryData.FindByUniqueId(uniqueId)?.Quantity ?? 0;
        }
        #endregion
    }
}
