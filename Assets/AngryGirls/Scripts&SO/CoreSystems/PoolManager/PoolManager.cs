using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{
    /// <summary>
    /// Manages object pooling system for different object types
    /// </summary>
    public class PoolManager
    {
        [SerializedDictionary("CharacterType", "Values")]
        public SerializedDictionary<CharacterType, List<PoolObject>> characterPoolDictionary = new();

        [SerializedDictionary("VFXType", "Values")]
        public SerializedDictionary<VFX_Type, List<PoolObject>> vfxPoolDictionary = new();

        [SerializedDictionary("UIObjectType", "Values")]
        public SerializedDictionary<UIObjectType, List<PoolObject>> UIObjectTypeDictionary = new();

        private Dictionary<Enum, int> _instanceCounters = new Dictionary<Enum, int>();

        public PoolManager()
        {        
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Scene change handler
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Clear pools when a scene changes
            ClearAllPools();
        }

        /// <summary>
        /// Clears all pools of null objects
        /// </summary>
        public void ClearAllPools()
        {
            ClearDictionary(characterPoolDictionary);
            ClearDictionary(vfxPoolDictionary);
            ClearDictionary(UIObjectTypeDictionary);
        }

        /// <summary>
        /// Clears a specific dictionary of null objects
        /// </summary>
        private void ClearDictionary<T>(Dictionary<T, List<PoolObject>> dictionary) where T : notnull
        {
            if (dictionary == null) return;

            foreach (var key in dictionary.Keys)
            {
                var list = dictionary[key];
                if (list == null) continue;

                // Remove all null objects
                list.RemoveAll(obj => obj == null || obj.gameObject == null);
            }
        }

        #region SetupDictionary
        private void SetUpDictionary<T>(Dictionary<T, List<PoolObject>> poolDictionary) where T : Enum
        {
            T[] arr = Enum.GetValues(typeof(T)) as T[];

            foreach (T type in arr)
            {
                if (!poolDictionary.ContainsKey(type))
                {
                    poolDictionary.Add(type, new List<PoolObject>());
                }

                if (!_instanceCounters.ContainsKey(type))
                {
                    _instanceCounters.Add(type, 0);
                }
            }
        }
        #endregion

        #region GetObjectFromPool
        public PoolObject GetObject<T>(T objType, Vector3 position, Quaternion rotation) where T : Enum
        {
            var poolDictionary = GetDictionaryByType<T>();
            return ObjectGetter(poolDictionary, objType, position, rotation);
        }

        public SerializedDictionary<T, List<PoolObject>> GetDictionaryByType<T>() where T : Enum
        {
            if (typeof(T) == typeof(CharacterType))
            {
                return characterPoolDictionary as SerializedDictionary<T, List<PoolObject>>;
            }
            else if (typeof(T) == typeof(VFX_Type))
            {
                return vfxPoolDictionary as SerializedDictionary<T, List<PoolObject>>;
            }
            else if (typeof(T) == typeof(UIObjectType))
            {
                return UIObjectTypeDictionary as SerializedDictionary<T, List<PoolObject>>;
            }

            throw new ArgumentException($"Unsupported enum type: {typeof(T)}");
        }

        private PoolObject ObjectGetter<T>(Dictionary<T, List<PoolObject>> poolDictionary, T objType, Vector3 position, Quaternion rotation) where T : Enum
        {
            if (poolDictionary.Count == 0)
            {
                SetUpDictionary(poolDictionary);
            }

            List<PoolObject> list = poolDictionary[objType];

            if (list.Count > 0)
            {
                var obj = list[0];
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                list.RemoveAt(0);

                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                var obj = CoreManager.Instance.PoolObjectLoader.InstantiatePrefab(objType, position, rotation);

                obj.gameObject.name = $"{obj.gameObject.name}_{GetNextInstanceNumber(objType)}";

                return obj;
            }
        }

        private int GetNextInstanceNumber<T>(T enumValue) where T : Enum
        {
            return ++_instanceCounters[enumValue];
        }

        #endregion

        /// <summary>
        /// Adds object back to pool for reuse
        /// </summary>
        public void AddObject<T>(T objType, Dictionary<T, List<PoolObject>> poolDictionary, PoolObject poolGameObject) where T : Enum
        {
            var listOfGameObjects = poolDictionary[(T)objType];
            listOfGameObjects.Add(poolGameObject);
            poolGameObject.gameObject.SetActive(false);
        }
    }
}