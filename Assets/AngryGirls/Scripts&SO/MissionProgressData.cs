using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Save container for all missions.
    /// </summary>
    [Serializable]
    public class MissionsProgressSaveData
    {
        public MissionSaveData[] missions;
    }

    [Serializable]
    public class MissionProgressData : ISaveData<MissionProgressData, MissionsProgressSaveData>
    {
        [SerializeField]
        private Dictionary<SceneType, Dictionary<MissionDifficulty, MissionData>> _progressData = new();

        public MissionData GetMissionData(SceneType missionName, MissionDifficulty difficulty)
        {
            if (_progressData.TryGetValue(missionName, out var dict) &&
                dict.TryGetValue(difficulty, out var data))
                return data;

            return new MissionData { isMissionAvailable = false, isMissionCompleted = false };
        }

        public void SetMissionData(SceneType missionName, MissionDifficulty difficulty, MissionData data)
        {
            if (!_progressData.ContainsKey(missionName))
                _progressData[missionName] = new Dictionary<MissionDifficulty, MissionData>();

            _progressData[missionName][difficulty] = data;
        }

        public void ResetData()
        {
            _progressData.Clear();
        }

        /// <summary>
        /// Fill progress with values from repository template.
        /// Availability, completion and reward-received flags are read from template.
        /// Reward data is copied as static reference.
        /// </summary>
        public void InitializeFromTemplate(MissionsRepository repository)
        {
            _progressData.Clear();
            var difficulties = Enum.GetValues(typeof(MissionDifficulty));
            var allMissionNames = repository.GetAllMissionsNames();

            foreach (var missionName in allMissionNames)
            {
                var mission = repository.GetMissionBySceneType(missionName);
                var dict = new Dictionary<MissionDifficulty, MissionData>();

                foreach (MissionDifficulty diff in difficulties)
                {
                    var templateData = mission?.GetData(diff);
                    dict[diff] = new MissionData
                    {
                        missionDifficulty = diff,
                        isMissionAvailable = templateData?.isMissionAvailable ?? false,
                        isMissionCompleted = templateData?.isMissionCompleted ?? false,
                        isRewardReceived = templateData?.isRewardReceived ?? false,
                        rewardData = templateData?.rewardData
                    };
                }
                _progressData[missionName] = dict;
            }
        }

        public void RestoreRewardDataFromRepository(MissionsRepository repository)
        {
            foreach (var kvp in _progressData)
            {
                var missionId = kvp.Key;
                var mission = repository.GetMissionBySceneType(missionId);
                if (mission == null) continue;

                foreach (var diffKvp in kvp.Value)
                {
                    var templateData = mission.GetData(diffKvp.Key);
                    if (templateData != null)
                    {
                        diffKvp.Value.rewardData = templateData.rewardData;
                    }
                }
            }
        }

        public MissionsProgressSaveData ConvertToSaveData()
        {
            var missionList = new List<MissionSaveData>();

            foreach (var kvp in _progressData)
            {
                var msd = new MissionSaveData { missionName = kvp.Key };
                foreach (var diffKvp in kvp.Value)
                {
                    msd.difficulties.Add(new DifficultyData
                    {
                        difficulty = diffKvp.Key,
                        isCompleted = diffKvp.Value.isMissionCompleted,
                        isAvailable = diffKvp.Value.isMissionAvailable,
                        isRewardReceived = diffKvp.Value.isRewardReceived
                    });
                }
                missionList.Add(msd);
            }

            return new MissionsProgressSaveData { missions = missionList.ToArray() };
        }

        public async UniTask UpdateFromSaveAsync(MissionsProgressSaveData saveData)
        {
            _progressData.Clear();
            if (saveData?.missions == null) return;

            foreach (var msd in saveData.missions)
            {
                var dict = new Dictionary<MissionDifficulty, MissionData>();
                if (msd.difficulties != null)
                {
                    foreach (var dsd in msd.difficulties)
                    {
                        dict[dsd.difficulty] = new MissionData
                        {
                            isMissionAvailable = dsd.isAvailable,
                            isMissionCompleted = dsd.isCompleted,
                            isRewardReceived = dsd.isRewardReceived
                        };
                    }
                }
                _progressData[msd.missionName] = dict;
            }

            await UniTask.CompletedTask;
        }

        public float GetCompletionPercentageForDifficulty(MissionDifficulty difficulty, int totalCount)
        {
            var completed = 0;
            foreach (var dict in _progressData.Values)
            {
                if (dict.TryGetValue(difficulty, out var data) && data.isMissionCompleted)
                    completed++;
            }
            return totalCount > 0 ? (float)completed / totalCount * 100f : 0f;
        }

        public int GetTotalMissionsForDifficulty(MissionDifficulty difficulty)
        {
            var count = 0;
            foreach (var dict in _progressData.Values)
            {
                if (dict.ContainsKey(difficulty)) count++;
            }
            return count;
        }

        public int GetCompletedMissionsForDifficulty(MissionDifficulty difficulty)
        {
            var count = 0;
            foreach (var dict in _progressData.Values)
            {
                if (dict.TryGetValue(difficulty, out var data) && data.isMissionCompleted)
                    count++;
            }
            return count;
        }
    }
}