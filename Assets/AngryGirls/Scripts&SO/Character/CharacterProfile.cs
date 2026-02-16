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
    }

    /// <summary>
    /// Contains character statistical data
    /// </summary>
    [Serializable]
    public class CharactersStatsBase
    {
        public CharactersStatsBase() { }
        public CharactersStatsBase(List<InventoryItem> items)
        {
            UpdateFromItemList(items);
        }

        public void UpdateFromItemList(List<InventoryItem> items)
        {
            health = 0;
            damage = 0;
            foreach (var item in items)
            {
                if (item == null) continue;
                health += item.GetTotalHealthBonus();
                damage += item.GetTotalDamageBonus();
            }
        }

        public CharactersStatsBase(CharactersStatsBase source)
        {
            health = source.health;
            damage = source.damage;
        }

        public static CharactersStatsBase operator +(CharactersStatsBase counter1, CharactersStatsBase counter2)
        {
            return new CharactersStatsBase { health = counter1.health + counter2.health, damage = counter1.damage + counter2.damage };
        }

        public float health;
        public float damage;

        public static string GetColoredText(CharactersStatsBase baseStats, CharactersStatsBase itemStats)
        {
            string healthText = FormatStat(baseStats.health, itemStats.health, "HP");
            string damageText = FormatStat(baseStats.damage, itemStats.damage, "Damage");

            return $"Character Stats:\n\n{healthText}\n{damageText}";
        }

        private static string FormatStat(float baseValue, float bonusValue, string statName)
        {
            float total = baseValue + bonusValue;
            string totalColored = $"<color=black>{total}</color>";

            if (bonusValue > 0)
            {
                string baseColored = $"<color=grey>{baseValue}</color>";
                string bonusColored = $"<color=green>{bonusValue}</color>";
                return $"{statName}: {totalColored} ({baseColored}+{bonusColored})";
            }
            else
            {
                return $"{statName}: {totalColored}";
            }
        }

        public static string GetEmptyText()
        {
            return
               $"Character Stats:\n\n" +
                $"HP: {""}\n" +
                $"Attack: {""}";
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

        public CharactersStatsBase GetSettingsStats => _characterSettings.characterStats;

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

            await LoadEquipmentFromSaveAsync(saveData);

            await UniTask.CompletedTask;
        }

        private async UniTask LoadEquipmentFromSaveAsync(CharacterProfileSaveData saveData)
        {
            Weapon = null;
            Armor = null;
            Accessory1 = null;
            Accessory2 = null;

            if (saveData.weaponInventoryItemSaveData != null)
            {
                var itemSettings = await CoreManager.Instance.AddressableAssetManager.LoadScriptableObjectAsync<ItemSettings>(saveData.weaponInventoryItemSaveData.itemSettingsUniqueId);
                Weapon = new InventoryItem(settings: itemSettings, 1);
            }

            if (saveData.armorInventoryItemSaveData != null)
            {
                var itemSettings = await CoreManager.Instance.AddressableAssetManager.LoadScriptableObjectAsync<ItemSettings>(saveData.armorInventoryItemSaveData.itemSettingsUniqueId);
                Armor = new InventoryItem(settings: itemSettings, 1);
            }

            if (saveData.accessory1InventoryItemSaveData != null)
            {
                var itemSettings = await CoreManager.Instance.AddressableAssetManager.LoadScriptableObjectAsync<ItemSettings>(saveData.accessory1InventoryItemSaveData.itemSettingsUniqueId);
                Accessory1 = new InventoryItem(settings: itemSettings, 1);
            }

            if (saveData.accessory2InventoryItemSaveData != null)
            {
                var itemSettings = await CoreManager.Instance.AddressableAssetManager.LoadScriptableObjectAsync<ItemSettings>(saveData.accessory2InventoryItemSaveData.itemSettingsUniqueId);
                Accessory2 = new InventoryItem(settings: itemSettings, 1);
            }
            UpdateItemsList();

        }
        public void ResetData()
        {
            _characterSettings = null;
            Weapon = null;
            Armor = null;
            Accessory1 = null;
            Accessory2 = null;
            UpdateItemsList();
        }



        public bool EquipItem(InventoryItem item, ItemType slotType, int accessorySlotIndex = 0)
        {
            if (item == null || !IsCompatible(item.ItemSettings.ItemType, slotType))
                return false;

            UnequipItem(slotType, accessorySlotIndex);

            switch (slotType)
            {
                case ItemType.Weapon:
                    Weapon = item;
                    break;
                case ItemType.Armor:
                    Armor = item;
                    break;
                case ItemType.Accessory:
                    if (accessorySlotIndex == 0)
                        Accessory1 = item;
                    else if (accessorySlotIndex == 1)
                        Accessory2 = item;
                    else
                        return false;
                    break;
                default:
                    return false;
            }

            CoreManager.Instance.InventoryManager.RemoveItem(item);
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