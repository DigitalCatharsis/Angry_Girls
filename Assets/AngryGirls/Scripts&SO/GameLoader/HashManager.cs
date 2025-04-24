using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
    [Serializable]
    public class HashManager : MonoBehaviour
    {
        public SerializedDictionary<T, int> CreateAndInitDictionary<T>(GameObject sender) where T : Enum
        {
            //Create
            var newDic = new SerializedDictionary<T, int>();

            //Init
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                newDic.Add((T)item, Animator.StringToHash(item.ToString()));
            }

            return newDic;
        }


        public int GetHash<T>(T enumType, SerializedDictionary<T, int> dictionary) where T : Enum
        {
            return dictionary[enumType];
        }

        public T GetName<T>(SerializedDictionary<T, int> dict, int val)
        {
            foreach (var pair in dict)
            {
                if (pair.Value == val)
                {
                    return pair.Key;
                }
            }
            return default;
        }

        //public void AddToDictionary<T>(SerializedDictionary<T, int> dict, T[] val)
        //{
        //    foreach (var item in val)
        //    {
        //        dict.Add((T)item, Animator.StringToHash(item.ToString()));
        //    }
        //}
    }
}