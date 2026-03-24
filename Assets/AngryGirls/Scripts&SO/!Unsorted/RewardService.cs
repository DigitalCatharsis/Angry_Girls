using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Handles reward granting logic. Separated from MissionsManager for SRP.
    /// </summary>
    public class RewardService
    {
        private readonly InventoryManager _inventoryManager;
        private readonly CharactersManager _charactersManager;
        private readonly CreditsManager _creditsManager;
        private readonly ItemSettingsRepository _itemSettingsRepository;
        private readonly CharacterSettingsCatalogSO _characterCatalog;

        public RewardService(
            InventoryManager inventoryManager,
            CharactersManager charactersManager,
            CreditsManager creditsManager,
            ItemSettingsRepository itemSettingsRepository,
            CharacterSettingsCatalogSO characterCatalog)
        {
            _inventoryManager = inventoryManager;
            _charactersManager = charactersManager;
            _creditsManager = creditsManager;
            _itemSettingsRepository = itemSettingsRepository;
            _characterCatalog = characterCatalog;
        }

        /// <summary>
        /// Grant reward to player. Returns reward result info.
        /// </summary>
        public async UniTask<RewardGrantResult> GrantRewardAsync(MissionRewardData rewardData)
        {
            if (!rewardData.IsValid())
            {
                Debug.LogWarning("RewardService: Invalid reward data");
                return new RewardGrantResult { isSuccess = false, errorMessage = "Invalid reward data" };
            }

            try
            {
                switch (rewardData.rewardType)
                {
                    case RewardType.Credits:
                        return GrantCredits(rewardData.creditsAmount);

                    case RewardType.Item:
                        return await GrantItemAsync(rewardData.itemSettingsUniqueId, rewardData.itemQuantity);

                    case RewardType.Character:
                        return GrantCharacter(rewardData.characterType);

                    case RewardType.None:
                        return new RewardGrantResult { isSuccess = true, rewardType = RewardType.None };

                    default:
                        return new RewardGrantResult { isSuccess = false, errorMessage = "Unknown reward type" };
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"RewardService: Failed to grant reward: {ex.Message}");
                return new RewardGrantResult { isSuccess = false, errorMessage = ex.Message };
            }
        }

        private RewardGrantResult GrantCredits(int amount)
        {
            _creditsManager.SetCredits(amount);
            return new RewardGrantResult
            {
                isSuccess = true,
                rewardType = RewardType.Credits,
                creditsAmount = amount,
                message = $"+{amount} Credits"
            };
        }

        private async UniTask<RewardGrantResult> GrantItemAsync(string uniqueId, int quantity)
        {
            var itemSettings = _itemSettingsRepository.GetItemByUniqueId(uniqueId);
            if (itemSettings == null)
            {
                return new RewardGrantResult { isSuccess = false, errorMessage = "Item not found" };
            }

            _inventoryManager.AddItem(itemSettings, quantity);
            return new RewardGrantResult
            {
                isSuccess = true,
                rewardType = RewardType.Item,
                itemSettings = itemSettings,
                quantity = quantity,
                message = $"+{quantity}x {itemSettings.ItemName}"
            };
        }

        private RewardGrantResult GrantCharacter(CharacterType characterType)
        {
            var settings = _characterCatalog.GetByType(characterType);
            if (settings == null)
            {
                return new RewardGrantResult { isSuccess = false, errorMessage = "Character not found" };
            }

            // Allow duplicates - create new profile
            var profile = new CharacterProfile(settings);
            _charactersManager.AddCharacterToAvailablePool(profile);

            return new RewardGrantResult
            {
                isSuccess = true,
                rewardType = RewardType.Character,
                characterSettings = settings,
                isDuplicate = false, // Duplicates allowed
                message = $"New Character: {settings.name}"
            };
        }
    }

    /// <summary>
    /// Result of reward granting operation
    /// </summary>
    public class RewardGrantResult
    {
        public bool isSuccess;
        public RewardType rewardType;
        public string message;
        public bool isDuplicate;
        public string errorMessage;

        // Reward details
        public int creditsAmount;
        public ItemSettings itemSettings;
        public int quantity;
        public CharacterSettings characterSettings;
    }
}