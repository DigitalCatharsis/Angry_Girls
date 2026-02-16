using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Applies frame and physics settings at runtime
    /// </summary>
    public class Settings : MonoBehaviour
    {
        [SerializeField] private bool debug;

        public FrameSettings frameSettings;
        public PhysicsSettings physicsSettings;

        private float PastTimeScale = 0f;

        private void Awake()
        {
            //Frames
            if (debug)
            {
                Debug.Log("targetFrameRate: " + frameSettings.TargetFPS);
            }
            Application.targetFrameRate = frameSettings.TargetFPS;

            //Physics
            if (debug)
            {
                Debug.Log("Default Solver Iterations: " + physicsSettings.defaultSolverIterations);
            }
            Physics.defaultSolverIterations = physicsSettings.defaultSolverIterations;

            if (debug)
            {
                Debug.Log("Default Solver Velocity Iterations: " + physicsSettings.defaultSolverIterations);
            }
            Physics.defaultSolverVelocityIterations = physicsSettings.defaultSolverIterations;
        }

        private void LateUpdate()
        {
            if (PastTimeScale != frameSettings.TimeScale)
            {
                PastTimeScale = frameSettings.TimeScale;
                Time.timeScale = frameSettings.TimeScale;
            }
        }
    }
}