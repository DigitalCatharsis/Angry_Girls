using System;

namespace Angry_Girls
{
    /// <summary>
    /// Data for a mission reward.
    /// </summary>
    [Serializable]
    public class MissionRewardData
    {
        public RewardType rewardType;

        // For Credits reward
        public int creditsAmount = 0;

        // For Item reward
        public ItemSettings itemSettings;
        public int itemQuantity = 1;

        // For Character reward
        public CharacterType characterType;

        /// <summary>
        /// Checks if the reward data is valid.
        /// </summary>
        public bool IsValid()
        {
            return rewardType switch
            {
                RewardType.Credits => creditsAmount > 0,
                RewardType.Item => itemSettings != null && itemQuantity > 0,
                RewardType.Character => characterType != CharacterType.NULL,
                RewardType.None => true,
                _ => false
            };
        }
    }
}