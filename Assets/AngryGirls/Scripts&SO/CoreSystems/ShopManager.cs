using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Save data for shop state including collection availability and removed items
    /// </summary>
    [Serializable]
    public class ShopSaveData
    {
        // Collection availability (Easy/Normal/Hard)
        public Dictionary<string, bool> collectionAvailability = new Dictionary<string, bool>();

        // Items temporarily removed from assortment since last refresh (by UniqueId)
        public HashSet<string> temporarilyRemovedItems = new HashSet<string>();

        // Last refresh timestamp (ticks)
        public long lastRefreshTimeTicks;
    }

    /// <summary>
    /// Container for shop runtime data
    /// </summary>
    [Serializable]
    public class ShopData : ISaveData<ShopData, ShopSaveData>
    {
        [SerializeField]
        private Dictionary<ShopAvailability, bool> _collectionAvailability =
            new Dictionary<ShopAvailability, bool>();

        [SerializeField] private HashSet<string> _temporarilyRemovedItems = new HashSet<string>();

        [SerializeField] private DateTime _lastRefreshTime = DateTime.MinValue;

        public Dictionary<ShopAvailability, bool> CollectionAvailability => _collectionAvailability;
        public HashSet<string> TemporarilyRemovedItems => _temporarilyRemovedItems;
        public DateTime LastRefreshTime => _lastRefreshTime;

        public void SetLastRefreshTime(DateTime dateTime)
        {
            _lastRefreshTime = dateTime;
        }

        public void ResetData()
        {
            _collectionAvailability.Clear();
            _temporarilyRemovedItems.Clear();
            _lastRefreshTime = DateTime.MinValue;
        }

        public async UniTask UpdateFromSaveAsync(ShopSaveData saveData)
        {
            _collectionAvailability.Clear();
            foreach (var kvp in saveData.collectionAvailability)
            {
                if (Enum.TryParse<ShopAvailability>(kvp.Key, out var availability))
                {
                    _collectionAvailability[availability] = kvp.Value;
                }
            }

            _temporarilyRemovedItems.Clear();
            if (saveData.temporarilyRemovedItems != null)
            {
                foreach (var id in saveData.temporarilyRemovedItems)
                {
                    _temporarilyRemovedItems.Add(id);
                }
            }

            _lastRefreshTime = saveData.lastRefreshTimeTicks > 0
                ? new DateTime(saveData.lastRefreshTimeTicks)
                : DateTime.MinValue;

            await UniTask.CompletedTask;
        }

        public ShopSaveData ConvertToSaveData()
        {
            var saveData = new ShopSaveData
            {
                collectionAvailability = new Dictionary<string, bool>(),
                temporarilyRemovedItems = new HashSet<string>(_temporarilyRemovedItems),
                lastRefreshTimeTicks = _lastRefreshTime.Ticks
            };

            foreach (var kvp in _collectionAvailability)
            {
                saveData.collectionAvailability[kvp.Key.ToString()] = kvp.Value;
            }

            return saveData;
        }
    }

    /// <summary>
    /// Manages shop operations including assortment generation and item purchasing
    /// </summary>
    public class ShopManager : ISaveReinitManager<DefaultSaveTemplate, ShopSaveData, ShopData>
    {
        [Header("Configuration")]
        [SerializeField] private ShopSettings _shopSettings;

        private ItemSettingsRepository _itemSettingsRepository;
        private InventoryManager _inventoryManager;
        private ShopData _shopData = new();

        private List<ItemSettings> _currentAssortment = new List<ItemSettings>();
        private List<ItemSettings> _easyItems = new List<ItemSettings>();
        private List<ItemSettings> _normalItems = new List<ItemSettings>();
        private List<ItemSettings> _hardItems = new List<ItemSettings>();

        public event Action OnDataChanged;

        public void Initialize(ItemSettingsRepository itemRepo, MissionsManager missionsMgr, InventoryManager inventoryMgr, ShopSettings shopSettings)
        {
            _itemSettingsRepository = itemRepo;
            _inventoryManager = inventoryMgr;
            _shopSettings = shopSettings;
        }

        public void ResetManagersData()
        {
            _shopData.ResetData();
            _currentAssortment.Clear();
            _easyItems.Clear();
            _normalItems.Clear();
            _hardItems.Clear();
        }

        public UniTask ReinitDataFromTemplateAsync(DefaultSaveTemplate template)
        {
            _shopData.ResetData();

            // Initialize collection availability from template
            _shopData.CollectionAvailability[ShopAvailability.Easy] = template.unlockEasyCollection;
            _shopData.CollectionAvailability[ShopAvailability.Normal] = template.unlockNormalCollection;
            _shopData.CollectionAvailability[ShopAvailability.Hard] = template.unlockHardCollection;

            // Initialize item pools from repository
            if (_itemSettingsRepository != null)
            {
                _easyItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Easy));
                _normalItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Normal));
                _hardItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Hard));
            }

            RefreshAssortment();
            OnDataChanged?.Invoke();
            return UniTask.CompletedTask;
        }

        public async UniTask ReinitDataFromSaveAsync(ShopSaveData saveData)
        {
            await _shopData.UpdateFromSaveAsync(saveData);

            // Rebuild item pools
            if (_itemSettingsRepository != null)
            {
                _easyItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Easy));
                _normalItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Normal));
                _hardItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Hard));
            }

            RefreshAssortment();
            OnDataChanged?.Invoke();
        }

        public ShopSaveData ConvertDataForSave()
        {
            return _shopData.ConvertToSaveData();
        }


        /// <summary>
        /// Request a manual refresh of the shop assortment (with cooldown check).
        /// </summary>
        public async UniTask<bool> RequestManualRefreshAsync()
        {
            // Check cooldown (e.g., 1 hour between manual refreshes)
            var timeSinceLastRefresh = DateTime.Now - _shopData.LastRefreshTime;
            if (timeSinceLastRefresh.TotalHours < 1.0)
            {
                Debug.Log($"ShopManager: Manual refresh on cooldown. Remaining: {60 - timeSinceLastRefresh.TotalMinutes:F0} minutes");
                return false;
            }

            await RefreshAssortmentAsync();
            return true;
        }

        /// <summary>
        /// Get the last time the shop assortment was refreshed.
        /// </summary>
        public DateTime LastRefreshTime => _shopData.LastRefreshTime;


        public async UniTask RefreshAssortmentAsync()
        {
            // Clear temporarily removed items on refresh
            _shopData.TemporarilyRemovedItems.Clear();
            _shopData.SetLastRefreshTime(DateTime.Now);
            RefreshAssortment();
            await UniTask.CompletedTask;
        }

        private void RefreshAssortment()
        {
            _currentAssortment.Clear();

            // Add items from available collections
            if (_shopData.CollectionAvailability.TryGetValue(ShopAvailability.Easy, out var easyUnlocked) && easyUnlocked)
            {
                AddRandomItemsFromPool(_easyItems, _shopSettings.EasyCollectionItemCount);
            }

            if (_shopData.CollectionAvailability.TryGetValue(ShopAvailability.Normal, out var normalUnlocked) && normalUnlocked)
            {
                AddRandomItemsFromPool(_normalItems, _shopSettings.NormalCollectionItemCountLow);
            }

            if (_shopData.CollectionAvailability.TryGetValue(ShopAvailability.Hard, out var hardUnlocked) && hardUnlocked)
            {
                AddRandomItemsFromPool(_hardItems, _shopSettings.HardCollectionItemCountHigh);
            }

            OnDataChanged?.Invoke();
        }

        private void AddRandomItemsFromPool(List<ItemSettings> pool, int count)
        {
            if (pool == null || pool.Count == 0) return;

            var availableItems = new List<ItemSettings>();
            foreach (var item in pool)
            {
                if (!_shopData.TemporarilyRemovedItems.Contains(item.UniqueId))
                {
                    availableItems.Add(item);
                }
            }

            if (availableItems.Count == 0) return;

            var randomIndices = GetUniqueRandomIndices(availableItems.Count, count);
            foreach (var index in randomIndices)
            {
                _currentAssortment.Add(availableItems[index]);
            }
        }

        private List<int> GetUniqueRandomIndices(int maxIndex, int count)
        {
            var indices = new List<int>();
            var available = new List<int>();
            for (int i = 0; i < maxIndex; i++) available.Add(i);

            count = Mathf.Min(count, maxIndex);
            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, available.Count);
                indices.Add(available[randomIndex]);
                available.RemoveAt(randomIndex);
            }
            return indices;
        }

        public bool TryPurchaseItem(ItemSettings itemSettings)
        {
            if (itemSettings == null || _inventoryManager == null) return false;

            int playerCredits = CoreManager.Instance.CreditsManager.GetCredits();
            if (playerCredits < itemSettings.Price) return false;

            // Deduct credits
            CoreManager.Instance.CreditsManager.SetCredits(-itemSettings.Price);

            // Add item to inventory
            _inventoryManager.AddItem(itemSettings);

            // Mark item as temporarily removed from assortment
            _shopData.TemporarilyRemovedItems.Add(itemSettings.UniqueId);

            // Refresh assortment to replace purchased item
            RefreshAssortment();

            return true;
        }

        public List<ItemSettings> GetCurrentAssortment() => _currentAssortment;

        public bool IsCollectionUnlocked(ShopAvailability availability)
        {
            return _shopData.CollectionAvailability.TryGetValue(availability, out var unlocked) && unlocked;
        }

        public void UnlockCollection(ShopAvailability availability)
        {
            _shopData.CollectionAvailability[availability] = true;
            RefreshAssortment();
            OnDataChanged?.Invoke();
        }
    }
}