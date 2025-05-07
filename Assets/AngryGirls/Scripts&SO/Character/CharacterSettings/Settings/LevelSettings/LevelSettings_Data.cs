using System.Collections.Generic;
using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

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
        public Transform characterLauncherTransform;
        public SerializedDictionary<CharacterType, Transform> enemiesToSpawn;
    }
}