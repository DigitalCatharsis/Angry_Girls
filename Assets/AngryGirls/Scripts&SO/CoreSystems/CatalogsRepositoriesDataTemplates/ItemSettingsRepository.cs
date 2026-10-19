using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Repository that holds and categorizes all ItemSettings in the game.
    /// Provides lookups by unique id and by ShopAvailability. Items are referenced directly,
    /// so initialization is synchronous.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemSettingsRepository", menuName = "Angry_Girls/ItemSettingsRepository")]
    public class ItemSettingsRepository : ScriptableObject
    {
        [Header("Item Settings")]
        [SerializeField] private List<ItemSettings> _items = new();

        private readonly Dictionary<string, ItemSettings> _itemSettingsById = new();
        private readonly Dictionary<ShopAvailability, List<ItemSettings>> _itemsByAvailability = new();

        private bool _isInitialized;

        public void OnEnable()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Builds the lookup tables. Safe to call multiple times.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            _itemSettingsById.Clear();
            _itemsByAvailability.Clear();

            foreach (ShopAvailability availability in System.Enum.GetValues(typeof(ShopAvailability)))
            {
                _itemsByAvailability[availability] = new List<ItemSettings>();
            }

            foreach (var itemSettings in _items)
            {
                if (itemSettings == null)
                {
                    Debug.LogWarning($"{nameof(ItemSettingsRepository)}: Null entry found in the items list.");
                    continue;
                }

                if (string.IsNullOrEmpty(itemSettings.UniqueId))
                {
                    Debug.LogError($"{nameof(ItemSettingsRepository)}: Item '{itemSettings.name}' has no UniqueId. Re-save the asset in the editor.");
                    continue;
                }

                if (_itemSettingsById.ContainsKey(itemSettings.UniqueId))
                {
                    Debug.LogWarning($"{nameof(ItemSettingsRepository)}: Duplicate UniqueId for item '{itemSettings.name}', skipping.");
                    continue;
                }

                _itemSettingsById[itemSettings.UniqueId] = itemSettings;
                _itemsByAvailability[itemSettings.ShopAvailability].Add(itemSettings);
            }

            _isInitialized = true;
            Debug.Log($"{nameof(ItemSettingsRepository)}: Initialized with {_itemSettingsById.Count} items.");
        }

        /// <summary>
        /// Gets all ItemSettings categorized by their ShopAvailability.
        /// </summary>
        public List<ItemSettings> GetItemsByAvailability(ShopAvailability availability)
        {
            Initialize();

            return _itemsByAvailability.TryGetValue(availability, out var items)
                ? new List<ItemSettings>(items)
                : new List<ItemSettings>();
        }

        /// <summary>
        /// Gets an ItemSettings instance by its unique id, or null when not found.
        /// </summary>
        public ItemSettings GetItemByUniqueId(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId)) return null;

            Initialize();

            _itemSettingsById.TryGetValue(uniqueId, out var itemSettings);
            return itemSettings;
        }

        /// <summary>
        /// Gets all registered ItemSettings.
        /// </summary>
        public List<ItemSettings> GetAllItems()
        {
            Initialize();

            return new List<ItemSettings>(_itemSettingsById.Values);
        }
    }
}
