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
    /// Single character entry for reward presentation (stub for XP system).
    /// </summary>
    [System.Serializable]
    public class CharacterRewardEntry
    {
        public CharacterSettings characterSettings;
        public int xpGained;
        public int currentLevel;
        public int currentXp;
        public int xpForNextLevel;
    }
}