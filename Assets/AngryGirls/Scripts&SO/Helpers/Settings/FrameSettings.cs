using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// ScriptableObject for frame rate and time scale settings
    /// </summary>
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/Settings/FrameSettings")]
    public class FrameSettings : ScriptableObject
    {
        [Range(0.01f, 4f)]
        public float TimeScale = 1.0f;
        public int TargetFPS = 200;
    }
}