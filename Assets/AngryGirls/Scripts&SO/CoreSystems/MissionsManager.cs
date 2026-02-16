using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages mission states and progression
    /// </summary>
    public class MissionsManager : MonoBehaviour, ISaveReinitManager<DefaultSaveTemplate, MissionsSaveData, MissionsData>
    {
        private MissionsData _missionsData = new();
        public MissionsData MissionsData => _missionsData;

        public event Action OnDataChanged;

        private MissionsRepository _missionsRepository;

        public void Initialize(DefaultSaveTemplate template)
        {
            _missionsRepository = template.missionsRepository;
        }

        public void ResetManagersData()
        {
            _missionsData.ResetData();
            OnDataChanged?.Invoke();
        }

        public UniTask ReinitDataFromTemplateAsync(DefaultSaveTemplate template)
        {
            _missionsRepository = template.missionsRepository;
            _missionsData.ResetData();

            int missionCount = _missionsRepository.GetMissionCount();
            for (int i = 0; i < missionCount; i++)
            {
                var mission = _missionsRepository.GetMission(i);
                var difficultyDataDict = new Dictionary<string, MissionData>();

                if (mission.missionDifficultyDatas != null)
                {
                    foreach (var kvp in mission.missionDifficultyDatas)
                    {
                        string difficultyStr = kvp.Key.ToString();
                        difficultyDataDict[difficultyStr] = kvp.Value;
                    }
                }
                _missionsData.MissionStates[i] = difficultyDataDict;
            }

            OnDataChanged?.Invoke();
            return UniTask.CompletedTask;
        }

        public async UniTask ReinitDataFromSaveAsync(MissionsSaveData saveData)
        {
            await _missionsData.UpdateFromSaveAsync(saveData);
            OnDataChanged?.Invoke();
        }

        public MissionsSaveData ConvertDataForSave()
        {
            return _missionsData.ConvertToSaveData();
        }

        #region mission stuff itself
        /// <summary>
        /// Mark a specific mission as completed
        /// </summary>
        public void CompleteMission(int stageIndex, MissionDifficulty difficulty)
        {
            var missionData = GetMissionData(stageIndex, difficulty) ?? new MissionData();
            missionData.isMissionCompleted = true;
            missionData.isMissionAvailable = false;
            missionData.isRewardRecived = false; // reward pending

            SetMissionData(stageIndex, difficulty, missionData);

            // Grant reward immediately (simplified logic)
            if (missionData.reward == RewardType.Credits)
            {
                var creditsManager = CoreManager.Instance.CreditsManager;
                creditsManager?.SetCredits(100);
            }
            // TODO: Character and Item rewards require additional logic (not implemented yet)
        }

        /// <summary>
        /// Get completion percentage for a difficulty level
        /// </summary>
        public float GetCompletionPercentageForDifficulty(MissionDifficulty difficulty)
        {
            int totalMissionsForDifficulty = 0;
            int completedMissionsForDifficulty = 0;

            if (_missionsRepository == null) return 0f;

            int missionCount = _missionsRepository.GetMissionCount();
            string difficultyStr = difficulty.ToString();

            for (int i = 0; i < missionCount; i++)
            {
                if (_missionsData.MissionStates.TryGetValue(i, out var difficultyDataDict))
                {
                    if (difficultyDataDict.ContainsKey(difficultyStr))
                    {
                        totalMissionsForDifficulty++;
                        if (difficultyDataDict[difficultyStr].isMissionCompleted)
                        {
                            completedMissionsForDifficulty++;
                        }
                    }
                }
            }

            if (totalMissionsForDifficulty > 0)
            {
                return ((float)completedMissionsForDifficulty / totalMissionsForDifficulty) * 100f;
            }
            return 0f;
        }

        /// <summary>
        /// Get mission data for specific mission and difficulty
        /// </summary>
        public MissionData GetMissionData(int missionIndex, MissionDifficulty difficulty)
        {
            string difficultyStr = difficulty.ToString();

            if (_missionsData.MissionStates.TryGetValue(missionIndex, out var difficultyDataDict))
            {
                if (difficultyDataDict.TryGetValue(difficultyStr, out var missionData))
                {
                    return missionData;
                }
                Debug.LogWarning($"MissionsManager: Mission index {missionIndex} does not have data for difficulty {difficulty}.");

                return new MissionData
                {
                    reward = RewardType.None,
                    isMissionAvailable = false,
                    isMissionCompleted = false,
                    isRewardRecived = false
                };
            }
            Debug.LogWarning($"MissionsManager: Mission index {missionIndex} is out of range.");
            return new MissionData();
        }

        /// <summary>
        /// Update mission data for specific mission and difficulty
        /// </summary>
        public void SetMissionData(int missionIndex, MissionDifficulty difficulty, MissionData newData)
        {
            string difficultyStr = difficulty.ToString();

            if (_missionsData.MissionStates.TryGetValue(missionIndex, out var difficultyDataDict))
            {
                difficultyDataDict[difficultyStr] = newData;

                if (newData.isMissionCompleted || newData.isRewardRecived)
                {
                    OnDataChanged?.Invoke();
                }
            }
            else
            {
                // Create new difficulty dictionary for this mission
                var newDict = new Dictionary<string, MissionData>
                {
                    { difficultyStr, newData }
                };
                _missionsData.MissionStates[missionIndex] = newDict;

                if (newData.isMissionCompleted || newData.isRewardRecived)
                {
                    OnDataChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Get total number of missions for a difficulty
        /// </summary>
        public int GetTotalMissionsForDifficulty(MissionDifficulty difficulty)
        {
            if (_missionsRepository == null) return 0;

            int count = 0;
            int missionCount = _missionsRepository.GetMissionCount();
            string difficultyStr = difficulty.ToString();

            for (int i = 0; i < missionCount; i++)
            {
                if (_missionsData.MissionStates.ContainsKey(i) &&
                    _missionsData.MissionStates[i].ContainsKey(difficultyStr))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Get number of completed missions for a difficulty
        /// </summary>
        public int GetCompletedMissionsForDifficulty(MissionDifficulty difficulty)
        {
            if (_missionsRepository == null) return 0;

            int count = 0;
            int missionCount = _missionsRepository.GetMissionCount();
            string difficultyStr = difficulty.ToString();

            for (int i = 0; i < missionCount; i++)
            {
                if (_missionsData.MissionStates.TryGetValue(i, out var difficultyDataDict))
                {
                    if (difficultyDataDict.ContainsKey(difficultyStr) &&
                        difficultyDataDict[difficultyStr].isMissionCompleted)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Get total number of missions
        /// </summary>
        public int GetMissionCount()
        {
            return _missionsRepository?.GetMissionCount() ?? 0;
        }

        /// <summary>
        /// Reset all mission progress
        /// </summary>
        public void ResetAllProgress()
        {
            _missionsData.ResetData();
            Debug.Log($"MissionsManager: All mission progress reset.");
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// Get all missions
        /// </summary>
        public IReadOnlyList<Mission> GetMissions() => _missionsRepository?.Missions ?? new List<Mission>();
        #endregion
    }
}