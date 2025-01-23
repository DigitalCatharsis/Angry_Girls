using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public abstract class BaseFactory<T> : MonoBehaviour, ICoreFactory<T> where T : System.Enum
    {
        protected abstract Dictionary<T, Func<GameObject>> Prefabs { get; }

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