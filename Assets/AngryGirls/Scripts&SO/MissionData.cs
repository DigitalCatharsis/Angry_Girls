using System;
using System.Collections.Generic;

namespace Angry_Girls
{
    /// <summary>
    /// Runtime state of a single mission difficulty.
    /// </summary>
    [Serializable]
    public class MissionData
    {
        public MissionDifficulty missionDifficulty;
        public MissionRewardData rewardData;

        public bool isMissionAvailable;
        public bool isMissionCompleted;
        public bool isRewardReceived;

        public void ResetData()
        {
            isMissionAvailable = false;
            isMissionCompleted = false;
            isRewardReceived = false;
        }
    }

    /// <summary>
    /// Serializable data for one mission (all difficulties).
    /// </summary>
    [Serializable]
    public class MissionSaveData
    {
        public SceneType missionName;
        public List<DifficultyData> difficulties = new();
    }

    /// <summary>
    /// Serializable snapshot of a single difficulty's progress.
    /// </summary>
    [Serializable]
    public class DifficultyData
    {
        public MissionDifficulty difficulty;
        public bool isCompleted;
        public bool isAvailable;
        public bool isRewardReceived;
    }
}