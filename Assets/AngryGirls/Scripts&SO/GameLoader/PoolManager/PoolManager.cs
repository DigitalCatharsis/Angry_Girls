using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class PoolManager : MonoBehaviour
    {
        [SerializedDictionary("CharacterType", "Values")]
        public SerializedDictionary<CharacterType, List<PoolObject>> characterPoolDictionary = new SerializedDictionary<CharacterType, List<PoolObject>>();

        [SerializedDictionary("VFXType", "Values")]
        public SerializedDictionary<VFX_Type, List<PoolObject>> vfxPoolDictionary = new SerializedDictionary<VFX_Type, List<PoolObject>>();

        #region SetupDictionary
        private void SetUpDictionary<T>(Dictionary<T, List<PoolObject>> poolDictionary)
        {
            T[] arr = Enum.GetValues(typeof(T)) as T[];

            foreach (T p in arr)
            {
                if (!poolDictionary.ContainsKey(p))
                {
                    poolDictionary.Add(p, new List<PoolObject>());
                }
            }
        }
        #endregion

        #region GetObjectFromPool
        public PoolObject GetObject<T>(T objType, Dictionary<T, List<PoolObject>> poolDictionary, Vector3 position, Quaternion rotation)
        {
            return ObjectGetter(poolDictionary, objType, position, rotation);
        }

        private PoolObject ObjectGetter<T>(Dictionary<T, List<PoolObject>> poolDictionary, T objType, Vector3 position, Quaternion rotation)
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

                obj.Init_PoolObject();
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                var obj = GameLoader.Instance.poolObjectLoader.InstantiatePrefab(objType, position, rotation);
                obj.Init_PoolObject();
                return obj;
            }
        }
        #endregion

        public void AddObject<T>(T objType, Dictionary<T, List<PoolObject>> poolDictionary, PoolObject poolGameObject)
        {
            var listOfGameObjects = poolDictionary[(T)objType];
            listOfGameObjects.Add(poolGameObject);
            poolGameObject.gameObject.SetActive(false);
        }
    }
}