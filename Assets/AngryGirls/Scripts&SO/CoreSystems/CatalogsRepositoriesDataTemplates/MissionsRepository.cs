using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Get mission by index.
        /// </summary>
        public Mission GetMission(int index)
        {
            if (index >= 0 && index < _missions.Length)
            {
                return _missions[index];
            }
            Debug.LogError($"MissionRepository: Index {index} is out of range.");
            return default;
        }

        /// <summary>
        /// Get total number of missions.
        /// </summary>
        public int GetMissionCount() => _missions?.Length ?? 0;
    }
}