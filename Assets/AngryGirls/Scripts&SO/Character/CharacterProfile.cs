using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [Serializable]
    public class CharacterProfileSaveData
    {
        public CharacterType characterType = CharacterType.NULL;
        public InventoryItemSaveData weaponInventoryItemSaveData;
        public InventoryItemSaveData armorInventoryItemSaveData;
        public InventoryItemSaveData accessory1InventoryItemSaveData;
        public InventoryItemSaveData accessory2InventoryItemSaveData;

        /// <summary>Character level. Defaults to 1 so saves created before the progression system still load.</summary>
        public int level = 1;

        /// <summary>Experience collected inside the current level.</summary>
        public int experience = 0;
    }

    /// <summary>
    /// Contains character statistical data and formatting helpers for UI presentation.
    /// </summary>
    [Serializable]
    public class CharactersStatsBase
    {
        public CharactersStatsBase()
        {
        }

        public CharactersStatsBase(List<InventoryItem> items)
        {
            UpdateFromItemList(items);
        }

        /// <summary>
        /// Recalculates aggregated equipment bonuses.
        /// </summary>
        public void UpdateFromItemList(List<InventoryItem> items)
        {
            health = 0f;
            damage = 0f;

            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                health += item.GetTotalHealthBonus();
                damage += item.GetTotalDamageBonus();
            }
        }

        public CharactersStatsBase(CharactersStatsBase source)
        {
            if (source == null)
                return;

            health = source.health;
            damage = source.damage;
        }

        public static CharactersStatsBase operator +(CharactersStatsBase counter1, CharactersStatsBase counter2)
        {
            if (counter1 == null)
                return new CharactersStatsBase(counter2);

            if (counter2 == null)
                return new CharactersStatsBase(counter1);

            return new CharactersStatsBase
            {
                health = counter1.health + counter2.health,
                damage = counter1.damage + counter2.damage
            };
        }

        public float health;
        public float damage;

        /// <summary>
        /// Formats character stats using the legacy default colors.
        /// Kept for backward compatibility with existing callers.
        /// </summary>
        public static string GetColoredText(
            CharactersStatsBase baseStats,
            CharactersStatsBase itemStats)
        {
            return GetColoredText(
                baseStats,
                itemStats,
                Color.black,
                Color.grey,
                Color.green);
        }

        /// <summary>
        /// Formats character stats using configurable UI colors.
        /// </summary>
        public static string GetColoredText(
            CharactersStatsBase baseStats,
            CharactersStatsBase itemStats,
            Color totalColor,
            Color baseColor,
            Color bonusColor,
            string levelLine = null)
        {
            baseStats ??= new CharactersStatsBase();
            itemStats ??= new CharactersStatsBase();

            var healthText = FormatStat(
                baseStats.health,
                itemStats.health,
                "HP",
                totalColor,
                baseColor,
                bonusColor);

            var damageText = FormatStat(
                baseStats.damage,
                itemStats.damage,
                "Damage",
                totalColor,
                baseColor,
                bonusColor);

            var levelBlock = string.IsNullOrEmpty(levelLine) ? string.Empty : $"{levelLine}\n";

            return $"Character Stats:\n\n{levelBlock}{healthText}\n{damageText}";
        }

        /// <summary>
        /// Builds a single stat line with total, base and equipment bonus values.
        /// </summary>
        private static string FormatStat(
            float baseValue,
            float bonusValue,
            string statName,
            Color totalColor,
            Color baseColor,
            Color bonusColor)
        {
            var total = baseValue + bonusValue;

            var totalColored = Colorize(total, totalColor);

            if (Mathf.Approximately(bonusValue, 0f))
                return $"{statName}: {totalColored}";

            var baseColored = Colorize(baseValue, baseColor);
            var bonusColored = Colorize(bonusValue, bonusColor);

            var sign = bonusValue > 0f ? "+" : string.Empty;

            return $"{statName}: {totalColored} ({baseColored}{sign}{bonusColored})";
        }


        /// <summary>
        /// Converts a Unity Color into a TextMeshPro rich-text color tag.
        /// </summary>
        private static string Colorize(float value, Color color)
        {
            var htmlColor = ColorUtility.ToHtmlStringRGBA(color);
            return $"<color=#{htmlColor}>{Mathf.RoundToInt(value)}</color>";
        }

        /// <summary>
        /// Returns the empty character stats representation.
        /// </summary>
        public static string GetEmptyText()
        {
            return
                "Character Stats:\n\n" +
                "HP: \n" +
                "Attack: ";
        }
    }


    /// <summary>
    /// The data for a specific instance of the player character before it is loaded into the scene.
    /// Holds runtime-modifiable data (equipment) and references immutable CharacterSettings.
    /// </summary>
    [Serializable]
    public class CharacterProfile : ISaveData<CharacterProfile, CharacterProfileSaveData>
    {
        public CharacterProfile() { }
        public CharacterProfile(CharacterSettings settings)
        {
            _characterSettings = settings;
        }

        private CharacterSettings _characterSettings;
        public CharacterSettings CharacterSettings => _characterSettings;

        // Equipment slots
        public InventoryItem Weapon { get; private set; }
        public InventoryItem Armor { get; private set; }
        public InventoryItem Accessory1 { get; private set; }
        public InventoryItem Accessory2 { get; private set; }
        private CharactersStatsBase _cachedItemsStats;
        private List<InventoryItem> _cacheditemsList = new(4);

        public event Action OnEquipmentChanged;
        public CharactersStatsBase GetItemsStats
        {
            get
            {
                if (_cachedItemsStats == null)
                {
                    _cacheditemsList = new List<InventoryItem>(4) { Weapon, Armor, Accessory1, Accessory2 };
                    _cachedItemsStats = new CharactersStatsBase(_cacheditemsList);
                    return _cachedItemsStats;
                }
                else
                {
                    _cachedItemsStats.UpdateFromItemList(_cacheditemsList);
                    return (_cachedItemsStats);
                }
            }
        }

        #region Progression

        private float _healthMultiplier = 1f;
        private float _damageMultiplier = 1f;

        /// <summary>Current level, starts from 1.</summary>
        public int Level { get; private set; } = 1;

        /// <summary>Experience collected inside the current level.</summary>
        public int Experience { get; private set; }

        public event Action OnProgressChanged;

        private static ProgressionConfig Config => CoreManager.Instance != null ? CoreManager.Instance.ProgressionConfig : null;

        public int MaxLevel => Config != null ? Config.MaxLevel : 1;
        public bool IsMaxLevel => Level >= MaxLevel;

        /// <summary>Experience needed to reach the next level, zero on the maximum level.</summary>
        public int XpToNextLevel => Config != null ? Config.GetXpToNextLevel(Level) : 0;

        /// <summary>
        /// Base stats of the character scaled to its level (and to the difficulty multipliers for enemies).
        /// </summary>
        public CharactersStatsBase GetSettingsStats
        {
            get
            {
                var baseStats = _characterSettings != null ? _characterSettings.characterStats : null;
                var config = Config;

                // Progression is not configured: behave like a plain level 1 character.
                if (config == null)
                    return new CharactersStatsBase(baseStats);

                return config.CalculateLevelStats(baseStats, Level, _healthMultiplier, _damageMultiplier);
            }
        }

        /// <summary>
        /// Adds experience and performs all level ups it pays for. Returns the number of gained levels.
        /// </summary>
        public int AddExperience(int amount)
        {
            var config = Config;
            if (amount <= 0 || config == null || IsMaxLevel)
                return 0;

            var gainedLevels = 0;
            long pool = (long)Experience + amount;

            while (Level < config.MaxLevel)
            {
                var needed = config.GetXpToNextLevel(Level);
                if (pool < needed)
                    break;

                pool -= needed;
                Level++;
                gainedLevels++;
            }

            Experience = Level >= config.MaxLevel ? 0 : (int)pool;
            OnProgressChanged?.Invoke();
            return gainedLevels;
        }

        /// <summary>
        /// Sets the level directly and clears the experience of the level.
        /// </summary>
        public void SetLevel(int level)
        {
            var config = Config;
            Level = config != null ? config.ClampLevel(level) : Mathf.Max(1, level);
            Experience = 0;
            OnProgressChanged?.Invoke();
        }

        /// <summary>
        /// Prepares the profile of an enemy: level plus the extra difficulty multipliers.
        /// </summary>
        public void ConfigureAsEnemy(int level, float healthMultiplier, float damageMultiplier)
        {
            SetLevel(level);
            _healthMultiplier = Mathf.Max(0.01f, healthMultiplier);
            _damageMultiplier = Mathf.Max(0.01f, damageMultiplier);
        }

        /// <summary>
        /// Short level description for UI, for example "Level: 5 (XP 120 / 450)".
        /// </summary>
        public string GetLevelSummaryText()
        {
            return IsMaxLevel
                ? $"Level: {Level} (MAX)"
                : $"Level: {Level} (XP {Experience} / {XpToNextLevel})";
        }

        /// <summary>
        /// Restores saved progress and repairs values that cannot be valid (corrupted or edited saves).
        /// </summary>
        private void RestoreProgress(int savedLevel, int savedExperience)
        {
            var config = Config;
            Level = config != null ? config.ClampLevel(savedLevel) : Mathf.Max(1, savedLevel);

            var xpLimit = config != null ? config.GetXpToNextLevel(Level) : 0;
            Experience = xpLimit > 0 ? Mathf.Clamp(savedExperience, 0, xpLimit - 1) : 0;

            _healthMultiplier = 1f;
            _damageMultiplier = 1f;
        }

        #endregion

        public CharactersStatsBase GetCurrentStats
        {
            get
            {
                return GetItemsStats + GetSettingsStats + BonusStats;
            }
            private set { }
        }

        public CharactersStatsBase BonusStats { get; private set; } = new CharactersStatsBase();

        private void UpdateItemsList()
        {
            _cacheditemsList.Clear();
            _cacheditemsList.TryAdd(Weapon);
            _cacheditemsList.TryAdd(Armor);
            _cacheditemsList.TryAdd(Accessory1);
            _cacheditemsList.TryAdd(Accessory2);

            OnEquipmentChanged?.Invoke();
        }

        public void ModifyBonusStats(CharactersStatsBase extraStats)
        {
            BonusStats += extraStats;
        }

        public void UpdateCharactersSettingsManually(CharacterSettings settings)
        {
            _characterSettings = settings;
        }

        public CharacterProfileSaveData ConvertToSaveData()
        {
            return new CharacterProfileSaveData
            {
                characterType = _characterSettings?.characterType ?? CharacterType.NULL,
                level = Level,
                experience = Experience,
                weaponInventoryItemSaveData = Weapon != null && Weapon.ItemSettings != null
                    ? new InventoryItemSaveData(Weapon.ItemSettings.UniqueId)
                    : null,
                armorInventoryItemSaveData = Armor != null && Armor.ItemSettings != null
                    ? new InventoryItemSaveData(Armor.ItemSettings.UniqueId)
                    : null,
                accessory1InventoryItemSaveData = Accessory1 != null && Accessory1.ItemSettings != null
                    ? new InventoryItemSaveData(Accessory1.ItemSettings.UniqueId)
                    : null,
                accessory2InventoryItemSaveData = Accessory2 != null && Accessory2.ItemSettings != null
                    ? new InventoryItemSaveData(Accessory2.ItemSettings.UniqueId)
                    : null
            };
        }

        public async UniTask UpdateFromSaveAsync(CharacterProfileSaveData saveData)
        {
            if (saveData == null)
            {
                throw new Exception("CharacterProfile save is null");
            }

            if (saveData.characterType != CharacterType.NULL)
            {
                _characterSettings = CoreManager.Instance.CharacterSettingsCatalogSO.GetByType(saveData.characterType);
            }
            else
            {
                _characterSettings = null;
                Debug.LogWarning("CharacterProfile.UpdateFromSaveAsync: characterType is NULL");
            }

            if (_characterSettings == null)
            {
                Debug.LogWarning($"CharacterProfile.UpdateFromSaveAsync: Failed to load settings for type {saveData.characterType}");
            }

            RestoreProgress(saveData.level, saveData.experience);
            LoadEquipmentFromSave(saveData);

            await UniTask.CompletedTask;
        }

        private void LoadEquipmentFromSave(CharacterProfileSaveData saveData)
        {
            Weapon = CreateEquippedItem(saveData.weaponInventoryItemSaveData);
            Armor = CreateEquippedItem(saveData.armorInventoryItemSaveData);
            Accessory1 = CreateEquippedItem(saveData.accessory1InventoryItemSaveData);
            Accessory2 = CreateEquippedItem(saveData.accessory2InventoryItemSaveData);

            UpdateItemsList();
        }

        /// <summary>
        /// Resolves a saved equipment entry into a single equipped item, or null when it cannot be resolved.
        /// </summary>
        private static InventoryItem CreateEquippedItem(InventoryItemSaveData saveData)
        {
            if (saveData == null || string.IsNullOrEmpty(saveData.itemSettingsUniqueId)) return null;

            var itemSettings = CoreManager.Instance.ItemSettingsRepository.GetItemByUniqueId(saveData.itemSettingsUniqueId);
            if (itemSettings == null)
            {
                Debug.LogWarning($"CharacterProfile: Equipped ItemSettings not found for id {saveData.itemSettingsUniqueId}");
                return null;
            }

            return new InventoryItem(itemSettings, 1) { IsEquipped = true };
        }
        public void ResetData()
        {
            _characterSettings = null;
            Weapon = null;
            Armor = null;
            Accessory1 = null;
            Accessory2 = null;
            Level = 1;
            Experience = 0;
            _healthMultiplier = 1f;
            _damageMultiplier = 1f;
            UpdateItemsList();
        }



        /// <summary>
        /// Equips exactly one unit of the given inventory stack into the slot.
        /// The previously equipped item returns to the inventory as a single unit.
        /// </summary>
        public bool EquipItem(InventoryItem item, ItemType slotType, int accessorySlotIndex = 0)
        {
            if (item?.ItemSettings == null || !IsCompatible(item.ItemSettings.ItemType, slotType))
                return false;

            if (slotType == ItemType.Accessory && accessorySlotIndex != 0 && accessorySlotIndex != 1)
                return false;

            if (slotType != ItemType.Weapon && slotType != ItemType.Armor && slotType != ItemType.Accessory)
                return false;

            var inventory = CoreManager.Instance.InventoryManager;
            var itemSettings = item.ItemSettings;

            // Validate before touching the slot so a failed equip never leaves the character half-changed.
            if (inventory.GetQuantity(itemSettings.UniqueId) < 1)
                return false;

            UnequipItem(slotType, accessorySlotIndex);

            // Take exactly one unit out of the stack, the rest of the stack stays in the inventory.
            if (!inventory.RemoveItem(itemSettings, 1))
            {
                UpdateItemsList();
                return false;
            }

            var equippedItem = new InventoryItem(itemSettings, 1)
            {
                IsEquipped = true,
                UpgradeLevel = item.UpgradeLevel
            };

            switch (slotType)
            {
                case ItemType.Weapon:
                    Weapon = equippedItem;
                    break;
                case ItemType.Armor:
                    Armor = equippedItem;
                    break;
                case ItemType.Accessory:
                    if (accessorySlotIndex == 0)
                        Accessory1 = equippedItem;
                    else
                        Accessory2 = equippedItem;
                    break;
            }

            UpdateItemsList();
            return true;
        }

        public void UnequipItem(ItemType slotType, int accessorySlotIndex = 0)
        {
            InventoryItem item = null;

            switch (slotType)
            {
                case ItemType.Weapon:
                    item = Weapon;
                    Weapon = null;
                    break;
                case ItemType.Armor:
                    item = Armor;
                    Armor = null;
                    break;
                case ItemType.Accessory:
                    if (accessorySlotIndex == 0)
                    {
                        item = Accessory1;
                        Accessory1 = null;
                    }
                    else if (accessorySlotIndex == 1)
                    {
                        item = Accessory2;
                        Accessory2 = null;
                    }
                    break;
            }

            if (item != null)
            {
                CoreManager.Instance.InventoryManager.AddItem(item.ItemSettings);
            }

            UpdateItemsList();
        }

        private bool IsCompatible(ItemType itemType, ItemType slotType)
        {
            return itemType == slotType;
        }
    }
}