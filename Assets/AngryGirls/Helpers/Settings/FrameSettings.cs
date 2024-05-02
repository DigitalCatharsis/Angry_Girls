using UnityEngine;


namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/Settings/FrameSettings")]

    public class FrameSettings : ScriptableObject
    {
        [Range(0.01f,1f)]
        public float TimeScale;
        public int TargetFPS;

    }
}
