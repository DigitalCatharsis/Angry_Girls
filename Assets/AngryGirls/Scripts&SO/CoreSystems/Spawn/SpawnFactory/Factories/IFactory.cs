using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Interface for factory classes
    /// </summary>
    public interface ICoreFactory<in T> where T : System.Enum
    {
        /// <summary>
        /// Spawns game object of specified type
        /// </summary>
        public PoolObject SpawnGameobject(T Type, Vector3 position, Quaternion rotation);
    }
}