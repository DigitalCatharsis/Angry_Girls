using Cysharp.Threading.Tasks;
using System;
using System.Linq;
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
        private readonly CharacterSettingsCatalogSO _characterCatalog;

        public RewardService(
            InventoryManager inventoryManager,
            CharactersManager charactersManager,
            CreditsManager creditsManager,
            CharacterSettingsCatalogSO characterCatalog)
        {
            _inventoryManager = inventoryManager;
            _charactersManager = charactersManager;
            _creditsManager = creditsManager;
            _characterCatalog = characterCatalog;
        }

        /// <summary>
        /// Grant reward to player. Returns reward result info.
        /// </summary>
        public async UniTask<RewardGrantResult> GrantRewardAsync(MissionRewardData rewardData, int collectedCoins, bool isObtained)
        {
            if (!rewardData.IsValid())
            {
                Debug.LogWarning("RewardService: Invalid reward data");
                return new RewardGrantResult { isSuccess = false, errorMessage = "Invalid reward data" };
            }

            try
            {
                // Convert collected coins to credits
                if (collectedCoins > 0)
                {
                    CoreManager.Instance.CreditsManager.SetCredits(collectedCoins);
                }

                switch (rewardData.rewardType)
                {
                    case RewardType.Credits:
                        return GrantCredits(rewardData.creditsAmount, isObtained);

                    case RewardType.Item:
                        return GrantItem(rewardData.itemSettings, rewardData.itemQuantity, isObtained);

                    case RewardType.Character:
                        return GrantCharacter(rewardData.characterType, isObtained);

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

        private RewardGrantResult GrantCredits(int amount, bool isObtained)
        {
            if (!isObtained)
                _creditsManager.SetCredits(amount);

            return new RewardGrantResult
            {
                isSuccess = true,
                rewardType = RewardType.Credits,
                creditsAmount = amount,
                message = $"+{amount} Credits"
            };
        }

        private RewardGrantResult GrantItem(ItemSettings itemSettings, int quantity, bool isObtained)
        {
            if (itemSettings == null)
            {
                return new RewardGrantResult { isSuccess = false, errorMessage = "Item not found" };
            }

            if (!isObtained)
            {
                _inventoryManager.AddItem(itemSettings, quantity);
            }

            return new RewardGrantResult
            {
                isSuccess = true,
                rewardType = RewardType.Item,
                itemSettings = itemSettings,
                quantity = quantity,
                message = $"+{quantity}x {itemSettings.ItemName}"
            };
        }

        private RewardGrantResult GrantCharacter(CharacterType characterType, bool isObtained)
        {
            var settings = _characterCatalog.GetByType(characterType);
            if (settings == null)
            {
                return new RewardGrantResult { isSuccess = false, errorMessage = "Character not found" };
            }

            if (!isObtained)
            {
                var profile = new CharacterProfile(settings);

                // A new character joins close to the level of the roster, so it is not useless on arrival.
                var charactersData = _charactersManager.CharactersData;
                var roster = charactersData.SelectedCharactersPool.Concat(charactersData.AvailableCharacterPool);
                profile.SetLevel(CoreManager.Instance.ExperienceService.GetRecruitLevel(roster));

                _charactersManager.AddCharacterToAvailablePool(profile);
            }

            return new RewardGrantResult
            {
                isSuccess = true,
                rewardType = RewardType.Character,
                characterSettings = settings,
                isDuplicate = false,
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