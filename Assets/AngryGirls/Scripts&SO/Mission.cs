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