using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        public MissionData missionDataEasy;
        public MissionData missionDataNormal;
        public MissionData missionDataHard;

        [Header("Resources")]
        public AssetReferenceT<Sprite> iconReference;
        public AssetReferenceT<Sprite> previewReference;

        public void ResetData()
        {
            missionDataEasy?.ResetData();
            missionDataNormal?.ResetData();
            missionDataHard?.ResetData();
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