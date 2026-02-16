using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Angry_Girls
{
    /// <summary>
    /// Types of items.
    /// </summary>
    public enum ItemType
    {
        None = 0,
        Weapon = 1,
        Armor = 2,
        Accessory = 3,
    }

    /// <summary>
    /// Rarity levels of items.
    /// </summary>
    public enum ItemRarity
    {
        None = 0,
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5
    }

    /// <summary>
    /// Categories for shop generation logic.
    /// </summary>
    public enum ShopAvailability
    {
        None = 0,
        Easy = 1,
        Normal = 2,
        Hard = 3
    }

    /// <summary>
    /// Template for all items of the same type. Contains basic, fixed characteristics.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemSettings", menuName = "Angry_Girls/ItemSettings")]
    public class ItemSettings : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _itemName;
        public string ItemName => _itemName;

        [SerializeField] private ItemRarity _itemRarity;
        public ItemRarity ItemRarity => _itemRarity;

        [SerializeField] private ItemType _itemType;
        public ItemType ItemType => _itemType;

        [Header("Stats")]
        [SerializeField] private int _basePrice = 100;
        public int Price => _basePrice;

        [SerializeField] private float _healthBonus = 0f;
        public float HealthBonus => _healthBonus;

        [SerializeField] private float _damageBonus = 0f;
        public float DamageBonus => _damageBonus;

        [Header("Shop")]
        [SerializeField] private ShopAvailability _shopAvailability = ShopAvailability.None;
        public ShopAvailability ShopAvailability => _shopAvailability;
        public bool IsAvailableInShop => _shopAvailability != ShopAvailability.None;

        public string UniqueId { get; set; }

        [Header("Upgrades")]
        [SerializeField] private bool _isUpgradable = false;
        public bool IsUpgradable => _isUpgradable;

        [SerializeField] private int _maxUpgradeLevel = 5;
        public int MaxUpgradeLevel => _maxUpgradeLevel;

        [SerializeField] private int _baseUpgradeCost = 100;
        public int BaseUpgradeCost => _baseUpgradeCost;

        [SerializeField] private float _healthBonusPerLevel = 10f;
        public float HealthBonusPerLevel => _healthBonusPerLevel;

        [SerializeField] private float _damageBonusPerLevel = 5f;
        public float DamageBonusPerLevel => _damageBonusPerLevel;

        [Header("Visual")]
        [SerializeField] private AssetReferenceT<Sprite> _iconReference;
        public AssetReferenceT<Sprite> IconReference => _iconReference;
    }
}