using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Angry_Girls
{
    /// <summary>
    /// Repository that holds and categorizes all ItemSettings in the game.
    /// Provides methods to retrieve items based on their ShopAvailability.
    /// Requires asynchronous initialization to load assets via Addressables.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemSettingsRepository", menuName = "Angry_Girls/ItemSettingsRepository")]
    public class ItemSettingsRepository : ScriptableObject
    {
        [Header("Item Settings References")]
        [SerializeField] private List<AssetReferenceT<ItemSettings>> _itemSettingsReferences = new();

        private Dictionary<string, ItemSettings> _itemSettingsById = new();
        private Dictionary<ShopAvailability, List<ItemSettings>> _itemsByAvailability = new();

        private IAssetProvider _assetProvider;

        [SerializeField] private bool _isInitialized = false;

        public void OnEnable()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Initialize the repository asynchronously using the provided asset provider.
        /// </summary>
        public async UniTask InitializeAsync(IAssetProvider provider)
        {
            try
            {
                if (_isInitialized) return;

                _assetProvider = provider;

                if (_assetProvider == null)
                {
                    Debug.LogError($"{nameof(ItemSettingsRepository)}: IAssetProvider is not set.");
                    return;
                }

                _itemSettingsById.Clear();
                _itemsByAvailability.Clear();

                foreach (ShopAvailability availability in System.Enum.GetValues(typeof(ShopAvailability)))
                {
                    _itemsByAvailability[availability] = new List<ItemSettings>();
                }

                var loadTasks = new List<UniTask<ItemSettings>>();

                foreach (var reference in _itemSettingsReferences)
                {
                    if (reference != null && reference.RuntimeKeyIsValid())
                    {
                        var loadTask = LoadItemSetting(reference.AssetGUID);
                        loadTasks.Add(loadTask);
                    }
                    else
                    {
                        Debug.LogWarning($"ItemSettingsRepository: Invalid or null AssetReference found in list: {reference?.RuntimeKey}");
                    }
                }

                var loadedItems = await UniTask.WhenAll(loadTasks);

                foreach (var itemSettings in loadedItems)
                {
                    if (itemSettings != null)
                    {
                        var assetID = CoreManager.Instance.AddressableAssetManager.GetAssetGuid(itemSettings);
                        itemSettings.UniqueId = assetID;
                        _itemSettingsById[itemSettings.UniqueId] = itemSettings;
                        _itemsByAvailability[itemSettings.ShopAvailability].Add(itemSettings);
                    }
                    else
                    {
                        Debug.LogWarning($"ItemSettingsRepository: Loaded ItemSettings is null despite valid reference key.");
                    }
                }

                _isInitialized = true;
                Debug.Log($"ItemSettingsRepository: Initialized ASYNCHRONOUSLY with {_itemSettingsById.Count} items using AssetProvider.");

            }
            catch
            {

            }
        }

        private async UniTask<ItemSettings> LoadItemSetting(string assetGuid)
        {
            var loadedAsset = await _assetProvider.LoadScriptableObjectAsync<ItemSettings>(assetGuid);
            return loadedAsset;
        }

        /// <summary>
        /// Gets all ItemSettings categorized by their ShopAvailability.
        /// </summary>
        /// <param name="availability">The ShopAvailability category.</param>
        /// <returns>A list of ItemSettings in the specified category. Returns an empty list if category is invalid or not initialized.</returns>
        public List<ItemSettings> GetItemsByAvailability(ShopAvailability availability)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("ItemSettingsRepository: GetItemsByAvailability called before initialization.");
                return new List<ItemSettings>();
            }

            if (_itemsByAvailability.TryGetValue(availability, out var items))
            {
                return new List<ItemSettings>(items);
            }
            return new List<ItemSettings>();
        }

        /// <summary>
        /// Gets an ItemSettings instance by its unique ID.
        /// </summary>
        /// <param name="uniqueId">The unique ID of the ItemSettings.</param>
        /// <returns>The ItemSettings instance, or null if not found or not initialized.</returns>
        public ItemSettings GetItemByUniqueId(string uniqueId)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("ItemSettingsRepository: GetItemByUniqueId called before initialization.");
                return null;
            }

            _itemSettingsById.TryGetValue(uniqueId, out var itemSettings);
            return itemSettings;
        }

        /// <summary>
        /// Gets all ItemSettings instances currently loaded.
        /// </summary>
        /// <returns>A list of all ItemSettings, or an empty list if not initialized.</returns>
        public List<ItemSettings> GetAllItems()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("ItemSettingsRepository: GetAllItems called before initialization.");
                return new List<ItemSettings>();
            }

            return new List<ItemSettings>(_itemSettingsById.Values);
        }
    }
}