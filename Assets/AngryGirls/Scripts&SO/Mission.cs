using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Difficulty levels for missions
    /// </summary>
    [Serializable]
    public enum MissionDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
    }

    /// <summary>
    /// Types of rewards for missions
    /// </summary>
    public enum RewardType
    {
        Character = 1,
        Credits = 2,
        Item = 3,
        None = 0,
    }
    /// <summary>
    /// Static definition of a mission (scenario, rewards, etc.).
    /// </summary>
    [Serializable]
    public class Mission
    {
        [Header("General")]
        public SceneType missionName;

        [Tooltip("Human readable name shown in the mission UI. Falls back to the scene name when empty.")]
        public string displayName;

        [TextArea(2, 5)]
        [Tooltip("Short briefing shown in the mission details panel.")]
        public string description;

        public MissionData missionDataEasy;
        public MissionData missionDataNormal;
        public MissionData missionDataHard;

        [Header("Balance")]
        [Tooltip("Level of the enemies on the Easy difficulty. The other difficulties add their level offset.")]
        [Min(1)] public int baseEnemyLevel = 1;

        [Tooltip("Multiplier of the experience reward of this mission.")]
        [Min(0.1f)] public float xpMultiplier = 1f;

        [Tooltip("Number of stages on each difficulty. Used for UI only, 0 hides the value. Must match the stage setup of the scene.")]
        [Min(0)] public int stagesEasy;
        [Min(0)] public int stagesNormal;
        [Min(0)] public int stagesHard;

        [Header("Resources")]
        public Sprite icon;
        public Sprite preview;

        public void ResetData()
        {
            missionDataEasy?.ResetData();
            missionDataNormal?.ResetData();
            missionDataHard?.ResetData();
        }

        /// <summary>
        /// Returns the display name, or the scene name when no display name is configured.
        /// </summary>
        public string GetDisplayName()
        {
            return string.IsNullOrWhiteSpace(displayName) ? missionName.ToString() : displayName;
        }

        /// <summary>
        /// Number of stages configured for UI on the given difficulty, 0 when unknown.
        /// </summary>
        public int GetStageCount(MissionDifficulty diff)
        {
            return diff switch
            {
                MissionDifficulty.Easy => stagesEasy,
                MissionDifficulty.Normal => stagesNormal,
                MissionDifficulty.Hard => stagesHard,
                _ => 0
            };
        }

        public MissionData GetData(MissionDifficulty diff)
        {
            return diff switch
            {
                MissionDifficulty.Easy => missionDataEasy,
                MissionDifficulty.Normal => missionDataNormal,
                MissionDifficulty.Hard => missionDataHard,
                _ => null
            };
        }
    }
}