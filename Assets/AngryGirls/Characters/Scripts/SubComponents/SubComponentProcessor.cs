using UnityEngine;

namespace Angry_Girls
{
    public class SubComponentProcessor : MonoBehaviour
    {
        public GroundDetector groundDetector;
        public AnimationProcessor animationProcessor;
        public LaunchLogic launchLogic;

        private void Awake()
        {
            groundDetector = GetComponentInChildren<GroundDetector>();
            animationProcessor = GetComponentInChildren<AnimationProcessor>();
            launchLogic = GetComponentInChildren<LaunchLogic>();
        }
    }
}