using System.Collections.Generic;

namespace Angry_Girls
{
    /// <summary>
    /// Data package for reward presentation screen.
    /// </summary>
    public class RewardPresentationData
    {
        public RewardGrantResult rewardResult;
        public int collectedCoinsScore;
        public List<CharacterRewardEntry> characterEntries = new();
    }

    /// <summary>
    /// Single character entry for reward presentation: experience gained and the progress before and after.
    /// </summary>
    [System.Serializable]
    public class CharacterRewardEntry
    {
        public CharacterSettings characterSettings;
        public int xpGained;
        public bool survived;

        // State after the experience was granted
        public int currentLevel;
        public int currentXp;
        public int xpForNextLevel;

        // State before the experience was granted
        public int previousLevel;
        public int previousXp;
        public int previousXpForNextLevel;

        // Stat growth caused by the level ups
        public float healthGain;
        public float damageGain;

        public bool LeveledUp => currentLevel > previousLevel;
    }
}