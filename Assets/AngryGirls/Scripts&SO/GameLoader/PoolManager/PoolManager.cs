using AYellowpaper.SerializedCollections;
using Codice.CM.Common;
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

        // Счетчики для каждого типа объектов (автоматическая инициализация)
        private Dictionary<Enum, int> _instanceCounters = new Dictionary<Enum, int>();

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

                // Инициализация счетчика при первом обращении
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
                var obj = GameLoader.Instance.poolObjectLoader.InstantiatePrefab(objType, position, rotation);

                // Добавляем нумерацию только для новых объектов
                obj.gameObject.name = $"{obj.gameObject.name}_{GetNextInstanceNumber(objType)}";

                return obj;
            }
        }

        private int GetNextInstanceNumber<T>(T enumValue) where T : Enum
        {
            return ++_instanceCounters[enumValue];
        }

        #endregion

        public void AddObject<T>(T objType, Dictionary<T, List<PoolObject>> poolDictionary, PoolObject poolGameObject) where T : Enum
        {
            var listOfGameObjects = poolDictionary[(T)objType];
            listOfGameObjects.Add(poolGameObject);
            poolGameObject.gameObject.SetActive(false);
        }
    }
}