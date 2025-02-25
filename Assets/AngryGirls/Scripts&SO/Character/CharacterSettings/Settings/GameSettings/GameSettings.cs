using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Angry_Girls/GameSettings/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Range(0, 1)]
        public float volumeMusic = 1;
        [Range(0, 1)]
        public float volumeSounds = 1;
    }
}