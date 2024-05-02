using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Angry_Girls/Settings/PhysicsSettings")]

    public class PhysicsSettings : ScriptableObject
    {
        public int defaultSolverIterations;
        public int defaultSolverVelocityIterations;

    }
}
