using UnityEngine;

namespace Angry_Girls
{
    public class SubComponentProcessor : MonoBehaviour
    {
        public GroundDetector GroundDetector;
        public AnimationProcessor AnimationProcessor;

        private void Awake()
        {
            GroundDetector = GetComponentInChildren<GroundDetector>();
            AnimationProcessor = GetComponentInChildren<AnimationProcessor>();
        }
    }
}