using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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

    [Serializable]
    public class MissionRewardData
    {
        public RewardType rewardType;

        // For Credits reward
        public int creditsAmount = 100;

        // For Item reward
        public AssetReference itemSettingsUniqueId;
        public int itemQuantity = 1;

        // For Character reward
        public CharacterType characterType;

        // Validation
        public bool IsValid()
        {
            return rewardType switch
            {
                RewardType.Credits => creditsAmount > 0,
                RewardType.Item => !string.IsNullOrEmpty(itemSettingsUniqueId),
                RewardType.Character => characterType != CharacterType.NULL,
                RewardType.None => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// Save data for a single mission state
    /// </summary>
    [Serializable]
    public class MissionSaveData
    {
        public MissionRewardData rewardData;
        public bool isMissionAvailable = false;
        public bool isMissionCompleted = false;
        public bool isRewardReceived = false; // intentional typo to match existing code
    }

    /// <summary>
    /// Save data container for all mission states
    /// Preserves existing hierarchical structure: stageIndex -> difficulty -> mission data
    /// </summary>
    [Serializable]
    public class MissionsSaveData
    {
        public Dictionary<int, Dictionary<string, MissionSaveData>> missionStates = new();
    }

    /// <summary>
    /// Data class for mission information
    /// </summary>
    [Serializable]
    public class MissionData : ISaveData<MissionData, MissionSaveData>
    {
        public MissionRewardData rewardData;
        public bool isMissionAvailable;
        public bool isMissionCompleted;
        public bool isRewardReceived;

        public MissionSaveData ConvertToSaveData()
        {
            return new MissionSaveData
            {
                rewardData = this.rewardData,
                isMissionAvailable = this.isMissionAvailable,
                isMissionCompleted = this.isMissionCompleted,
                isRewardReceived = this.isRewardReceived
            };
        }

        public void ResetData()
        {
            this.rewardData = null;
            this.isMissionAvailable = false;
            this.isMissionCompleted = false;
            this.isRewardReceived = false;
        }

        public UniTask UpdateFromSaveAsync(MissionSaveData saveData)
        {
            if (saveData == null)
            {
                throw new Exception("MissionsData save is null");
            }

            this.rewardData = saveData.rewardData;
            this.isMissionAvailable = saveData.isMissionAvailable;
            this.isMissionCompleted = saveData.isMissionCompleted;
            this.isRewardReceived = saveData.isRewardReceived;
            return UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// Container for all mission data organized by stage and difficulty
    /// </summary>
    [Serializable]
    public class MissionsData : ISaveData<MissionsData, MissionsSaveData>
    {
        [SerializeField] private Dictionary<int, Dictionary<string, MissionData>> _missionStates = new();

        public Dictionary<int, Dictionary<string, MissionData>> MissionStates => _missionStates;

        public MissionsSaveData ConvertToSaveData()
        {
            var saveData = new MissionsSaveData
            {
                missionStates = new Dictionary<int, Dictionary<string, MissionSaveData>>()
            };

            foreach (var stageEntry in _missionStates)
            {
                var stageIndex = stageEntry.Key;
                var difficulties = stageEntry.Value;
                var difficultyDict = new Dictionary<string, MissionSaveData>();

                foreach (var difficultyEntry in difficulties)
                {
                    var difficulty = difficultyEntry.Key;
                    var missionData = difficultyEntry.Value;
                    difficultyDict[difficulty] = missionData.ConvertToSaveData();
                }

                saveData.missionStates[stageIndex] = difficultyDict;
            }

            return saveData;
        }

        public async UniTask UpdateFromSaveAsync(MissionsSaveData dto)
        {
            _missionStates.Clear();

            if (dto?.missionStates == null) return;

            foreach (var stageEntry in dto.missionStates)
            {
                var stageIndex = stageEntry.Key;
                var difficultyDict = stageEntry.Value;
                var missionDict = new Dictionary<string, MissionData>();

                foreach (var difficultyEntry in difficultyDict)
                {
                    var difficulty = difficultyEntry.Key;
                    var saveData = difficultyEntry.Value;
                    var missionData = new MissionData();
                    await missionData.UpdateFromSaveAsync(saveData);
                    missionDict[difficulty] = missionData;
                }

                _missionStates[stageIndex] = missionDict;
            }
        }

        public void ResetData()
        {
            _missionStates.Clear();
        }
    }

    /// <summary>
    /// Represents a mission with difficulty-specific data
    /// </summary>
    [Serializable]
    public struct Mission
    {
        [Header("General")]
        public SceneType missionName;

        [Header("Resources")]
        public AssetReferenceT<Sprite> iconReference;
        public AssetReferenceT<Sprite> previewReference;

        [Header("Difficulty Data")]
        public SerializedDictionary<MissionDifficulty, MissionData> missionDifficultyDatas;

        /// <summary>
        /// Get mission data for a specific difficulty
        /// </summary>
        public MissionData GetData(MissionDifficulty diff)
        {
            if (missionDifficultyDatas != null && missionDifficultyDatas.TryGetValue(diff, out var data))
            {
                return data;
            }

            return new MissionData
            {
                rewardData = null,
                isMissionAvailable = false,
                isMissionCompleted = false,
                isRewardReceived = false
            };
        }

        /// <summary>
        /// Set mission data for a specific difficulty
        /// </summary>
        public void SetData(MissionDifficulty diff, MissionData data)
        {
            if (missionDifficultyDatas == null)
                missionDifficultyDatas = new SerializedDictionary<MissionDifficulty, MissionData>();
            missionDifficultyDatas[diff] = data;
        }
    }
}