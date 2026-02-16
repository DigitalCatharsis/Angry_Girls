using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Angry_Girls
{
    [System.Serializable]
    public class InventoryItemTemplate
    {
        public AssetReferenceT<ItemSettings> itemSettingsReference;
        //TODO: implement
        public int quantity = 1;
    }

    [CreateAssetMenu(fileName = "DefaultSaveTemplate", menuName = "Angry_Girls/DefaultSaveTemplate")]
    public class DefaultSaveTemplate : ScriptableObject
    {
        [Header("Starting Credits")]
        [Tooltip("Initial amount of credits for a new game")]
        public int startingCredits = 100;

        [Header("Selected Characters (pre-picked for mission, max 6)")]
        [Tooltip("First 6 non-NULL entries will be placed in selected slots")]
        public List<CharacterType> selectedCharacters = new List<CharacterType>();

        [Header("Available Characters (pool to choose from)")]
        public List<CharacterType> availableCharacters = new List<CharacterType>();

        [Header("Starting Inventory")]
        [Tooltip("Items that will be added to player inventory on new game")]
        public List<InventoryItemTemplate> startingInventory = new List<InventoryItemTemplate>();

        [Header("Shop Collections")]
        public bool unlockEasyCollection = true;
        public bool unlockNormalCollection = false;
        public bool unlockHardCollection = false;

        [Header("Missions")]
        public MissionsRepository missionsRepository;
    }
}