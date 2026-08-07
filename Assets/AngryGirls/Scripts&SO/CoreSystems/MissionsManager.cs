using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Angry_Girls
{
    public class MissionsManager : MonoBehaviour, ISaveReinitManager<DefaultSaveTemplate, MissionsProgressSaveData, MissionProgressData>
    {
        public event Action OnDataChanged;

        private MissionsRepository _missionsRepository;
        private MissionProgressData _missionProgressData = new();

        public SceneType CurrentMission { get; private set; }
        public MissionDifficulty CurrentDifficulty { get; private set; } = MissionDifficulty.Normal;

        public void Initialize(DefaultSaveTemplate template)
        {
            _missionsRepository = template.missionsRepository;
        }

        public void ResetManagersData()
        {
            _missionProgressData.ResetData();
            CurrentMission = SceneType.None;
            OnDataChanged?.Invoke();
        }

        public UniTask ReinitDataFromTemplateAsync(DefaultSaveTemplate template)
        {
            if (_missionsRepository == null)
                _missionsRepository = template.missionsRepository;

            _missionProgressData.InitializeFromTemplate(_missionsRepository);
            CurrentMission = SceneType.None;
            OnDataChanged?.Invoke();
            return UniTask.CompletedTask;
        }

        public async UniTask ReinitDataFromSaveAsync(MissionsProgressSaveData saveData)
        {
            await _missionProgressData.UpdateFromSaveAsync(saveData);
            _missionProgressData.RestoreRewardDataFromRepository(_missionsRepository);

            CurrentMission = SceneType.None;
            OnDataChanged?.Invoke();
        }

        public MissionsProgressSaveData ConvertDataForSave() =>
            _missionProgressData.ConvertToSaveData();

        public void SetCurrentMission(SceneType missionId, MissionDifficulty difficulty)
        {
            CurrentMission = missionId;
            CurrentDifficulty = difficulty;
        }

        public void CompleteCurrentMission()
        {
            if (CurrentMission == SceneType.None)
            {
                Debug.LogWarning("MissionsManager: Current mission is None. No active mission to complete.");
                return;
            }

            CompleteMission(CurrentMission, CurrentDifficulty);
            CurrentMission = SceneType.None;
        }

        /// <summary>
        /// Mark mission as completed and unlock the next mission of the same difficulty.
        /// Mission stays available for replay; reward can be claimed only once.
        /// </summary>
        public void CompleteMission(SceneType missionId, MissionDifficulty difficulty)
        {
            var data = _missionProgressData.GetMissionData(missionId, difficulty);
            data.isMissionCompleted = true;
            data.isRewardReceived = true;
            _missionProgressData.SetMissionData(missionId, difficulty, data);

            UnlockNextMission(missionId, difficulty);

            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// Unlocks the next mission in repository order for the given difficulty.
        /// Does nothing when the completed mission is the last one.
        /// </summary>
        private void UnlockNextMission(SceneType completedMissionId, MissionDifficulty difficulty)
        {
            if (_missionsRepository == null) return;

            var nextMission = _missionsRepository.GetNextMission(completedMissionId);
            if (nextMission == null) return;

            var nextData = _missionProgressData.GetMissionData(nextMission.missionName, difficulty);

            if (!nextData.isMissionAvailable)
            {
                nextData.isMissionAvailable = true;
                _missionProgressData.SetMissionData(nextMission.missionName, difficulty, nextData);
            }
        }

        public MissionData GetMissionData(SceneType missionId, MissionDifficulty difficulty) =>
            _missionProgressData.GetMissionData(missionId, difficulty);

        public void SetMissionData(SceneType missionId, MissionDifficulty difficulty, MissionData newData)
        {
            _missionProgressData.SetMissionData(missionId, difficulty, newData);
            OnDataChanged?.Invoke();
        }

        public float GetCompletionPercentageForDifficulty(MissionDifficulty difficulty) =>
            _missionProgressData.GetCompletionPercentageForDifficulty(difficulty, GetMissionCount());

        public int GetTotalMissionsForDifficulty(MissionDifficulty difficulty) =>
            _missionProgressData.GetTotalMissionsForDifficulty(difficulty);

        public int GetCompletedMissionsForDifficulty(MissionDifficulty difficulty) =>
            _missionProgressData.GetCompletedMissionsForDifficulty(difficulty);

        public int GetMissionCount() =>
            _missionsRepository?.GetMissionCount() ?? 0;

        public void ResetAllProgress()
        {
            _missionProgressData.ResetData();
            Debug.Log("MissionsManager: All mission progress reset.");
            OnDataChanged?.Invoke();
        }

        public IReadOnlyList<Mission> GetMissionsFromRepository() =>
            _missionsRepository?.Missions;
    }
}