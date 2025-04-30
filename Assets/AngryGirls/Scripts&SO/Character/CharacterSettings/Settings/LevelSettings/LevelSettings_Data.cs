using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Angry_Girls/Settings/LevelSettings")]
    public class LevelSettings_Data : ScriptableObject
    {
        [Header("BackgroundMusic")]
        public AudioSourceType audioSourceType = AudioSourceType.LevelMusic;
        public int audioClipEllementIndex;

        [Header("Gamelogic")]
        public StageData[] stages;
    }

    [Serializable]
    public struct StageData
    {
        public int stageIndex;
        public Transform chratacterLauncherTransform;
        public Transform[] enemyToSpawnTransform;
        public List<GameObject> charactersToSpawn;
        public List<GameObject> enemyToSpawn;
    }
}