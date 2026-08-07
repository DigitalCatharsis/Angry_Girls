using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Repository for mission data.
    /// </summary>
    [CreateAssetMenu(fileName = "MissionRepository", menuName = "Angry_Girls/MissionRepository")]
    public class MissionsRepository : ScriptableObject
    {
        [SerializeField] private Mission[] _missions;
        public IReadOnlyList<Mission> Missions => Array.AsReadOnly(_missions);

        public IEnumerable<SceneType> GetAllMissionsNames()
        {
            return Missions.Select(m => m.missionName);
        }
        public Mission GetMissionBySceneType(SceneType sceneType)
        {
            return Missions.FirstOrDefault(m => m.missionName == sceneType);
        }

        /// <summary>
        /// Get total number of missions.
        /// </summary>
        public int GetMissionCount() => _missions?.Length ?? 0;

        /// <summary>
        /// Returns zero-based index of the mission in the repository array.
        /// Returns -1 when the mission is not found.
        /// </summary>
        public int GetMissionIndex(SceneType sceneType)
        {
            if (_missions == null) return -1;

            for (int i = 0; i < _missions.Length; i++)
            {
                if (_missions[i] != null && _missions[i].missionName == sceneType)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns the mission that follows the given one in repository order,
        /// or null when the given mission is the last one.
        /// </summary>
        public Mission GetNextMission(SceneType currentMission)
        {
            int index = GetMissionIndex(currentMission);
            if (index < 0 || index + 1 >= (_missions?.Length ?? 0))
                return null;

            return _missions[index + 1];
        }
    }
}