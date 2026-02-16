using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Abstract base factory for creating game objects
    /// </summary>
    public abstract class BaseFactory<T> : MonoBehaviour, ICoreFactory<T> where T : System.Enum
    {
        /// <summary>
        /// Dictionary mapping enum types to prefab loading functions
        /// </summary>
        protected abstract Dictionary<T, Func<GameObject>> Prefabs { get; }

        /// <summary>
        /// Spawns game object of specified type at given position and rotation
        /// </summary>
        public PoolObject SpawnGameobject(T type, Vector3 position, Quaternion rotation)
        {
            if (!Prefabs.TryGetValue(type, out var prefabFunc))
            {
                Debug.LogError($"Unknown type: {type}");
                return null;
            }

            var prefab = prefabFunc.Invoke();
            if (prefab == null)
            {
                Debug.LogError($"Failed to load resource for {type}");
                return null;
            }

            return Instantiate(prefab, position, rotation)?.GetComponent<PoolObject>();
        }
    }
}