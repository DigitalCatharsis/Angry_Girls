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
        public Dictionary<string, bool> collectionAvailability = new();

        public List<string> currentAssortiment = new();

        // Last refresh timestamp (ticks)
        public long lastRefreshTimeTicks;
    }

    /// <summary>
    /// Container for shop runtime data
    /// </summary>
    [Serializable]
    public class ShopData : ISaveData<ShopData, ShopSaveData>
    {
        [SerializeField] private Dictionary<ShopAvailability, bool> _collectionAvailability = new();
        [SerializeField] private List<string> _currentItemIds = new(); // current product range identifiers

        [SerializeField] private DateTime _lastRefreshTime = DateTime.MinValue;
        public Dictionary<ShopAvailability, bool> CollectionAvailability => _collectionAvailability;
        public IReadOnlyList<string> CurrentItemIds => _currentItemIds;
        public DateTime LastRefreshTime => _lastRefreshTime;

        public void SetLastRefreshTime(DateTime dateTime)
        {
            _lastRefreshTime = dateTime;
        }
        
        public void SetCurrentAssortment(List<ItemSettings> assortment)
        {
            _currentItemIds.Clear();
            foreach (var item in assortment)
            {
                if (item != null && !string.IsNullOrEmpty(item.UniqueId))
                    _currentItemIds.Add(item.UniqueId);
                else
                    Debug.LogWarning($"ShopData: попытка добавить предмет без UniqueId: {item?.name}");
            }
        }

        public void ResetData()
        {
            _collectionAvailability.Clear();
            _currentItemIds.Clear();
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

            _currentItemIds.Clear();
            if (saveData.currentAssortiment != null)
            {
                foreach (var id in saveData.currentAssortiment)
                {
                    if (!string.IsNullOrEmpty(id))
                        _currentItemIds.Add(id);
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
                lastRefreshTimeTicks = _lastRefreshTime.Ticks,
                currentAssortiment = new List<string>(_currentItemIds)
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
        private CreditsManager _creditsManager;
        private ShopData _shopData = new();

        private List<ItemSettings> _currentAssortment = new();
        private List<ItemSettings> _easyItems = new();
        private List<ItemSettings> _normalItems = new();
        private List<ItemSettings> _hardItems = new();

        public event Action OnDataChanged;

        public void Initialize()
        {
            _itemSettingsRepository = CoreManager.Instance.ItemSettingsRepository;
            _inventoryManager = CoreManager.Instance.InventoryManager;
            _shopSettings = CoreManager.Instance.ShopSettings;
            _creditsManager = CoreManager.Instance.CreditsManager;
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
            RebuildItemPools();

            RefreshAssortment();
            OnDataChanged?.Invoke();
            return UniTask.CompletedTask;
        }

        public async UniTask ReinitDataFromSaveAsync(ShopSaveData saveData)
        {
            await _shopData.UpdateFromSaveAsync(saveData);

            // Convert saved IDs back to ItemSettings
            _currentAssortment.Clear();
            if (_itemSettingsRepository != null)
            {
                foreach (string id in _shopData.CurrentItemIds)
                {
                    var item = _itemSettingsRepository.GetItemByUniqueId(id);
                    if (item != null)
                        _currentAssortment.Add(item);
                    else
                        Debug.LogError($"ShopManager: Item with UniqueId '{id}' not found in repository. Skipping.");
                }
            }

            RebuildItemPools();

            //RefreshAssortment();
            OnDataChanged?.Invoke();
        }
        private void RebuildItemPools()
        {
            if (_itemSettingsRepository == null) return;
            _easyItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Easy));
            _normalItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Normal));
            _hardItems = new List<ItemSettings>(_itemSettingsRepository.GetItemsByAvailability(ShopAvailability.Hard));
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
            await RefreshAssortmentAsync();
            return true;
        }

        private bool CheckRefreshTimer()
        {
            // Check cooldown (e.g., 1 hour between manual refreshes)
            var timeSinceLastRefresh = DateTime.Now - _shopData.LastRefreshTime;
            if (timeSinceLastRefresh.TotalHours < 1.0)
            {
                Debug.Log($"ShopManager: Manual refresh on cooldown. Remaining: {60 - timeSinceLastRefresh.TotalMinutes:F0} minutes");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get the last time the shop assortment was refreshed.
        /// </summary>
        public DateTime LastRefreshTime => _shopData.LastRefreshTime;


        public async UniTask RefreshAssortmentAsync()
        {
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

            // Synchronize with ShopData
            _shopData.SetCurrentAssortment(_currentAssortment);

            OnDataChanged?.Invoke();
        }

        private void AddRandomItemsFromPool(List<ItemSettings> pool, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, pool.Count);
                _currentAssortment.Add(pool[randomIndex]);
            }
        }

        //private List<int> GetUniqueRandomIndices(int maxIndex, int count)
        //{
        //    var indices = new List<int>();
        //    var available = new List<int>();
        //    for (int i = 0; i < maxIndex; i++) available.Add(i);

        //    count = Mathf.Min(count, maxIndex);
        //    for (int i = 0; i < count; i++)
        //    {
        //        int randomIndex = UnityEngine.Random.Range(0, available.Count);
        //        indices.Add(available[randomIndex]);
        //        available.RemoveAt(randomIndex);
        //    }
        //    return indices;
        //}

        public bool TryPurchaseItem(ItemSettings itemSettings)
        {
            int playerCredits = _creditsManager.GetCredits();

            if (playerCredits < itemSettings.Price) return false;

            // Deduct credits
            _creditsManager.SetCredits(-itemSettings.Price);

            // Add item to inventory
            _inventoryManager.AddItem(itemSettings);

            if (!_currentAssortment.Remove(itemSettings))
            {
                Debug.LogWarning($"ShopManager: Item {itemSettings.name} not found in current assortment.");
            }

            // Synchronize with ShopData (update saved IDs)
            _shopData.SetCurrentAssortment(_currentAssortment);

            // Notify UI that assortment changed
            OnDataChanged?.Invoke();

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