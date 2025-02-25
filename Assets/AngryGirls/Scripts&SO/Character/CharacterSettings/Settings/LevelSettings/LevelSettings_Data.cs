using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Angry_Girls/Settings/LevelSettings")]
    public class LevelSettings_Data : ScriptableObject
    {
        [Header("BackgroundMusic")]
        public AudioSourceType audioSourceType = AudioSourceType.LevelMusic;
        public int audioClipEllementIndex;
    }
}