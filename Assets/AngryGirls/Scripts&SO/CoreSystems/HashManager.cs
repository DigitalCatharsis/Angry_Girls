using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages animation hash creation and lookup
    /// </summary>
    [Serializable]
    public class HashManager
    {
        /// <summary>
        /// Creates and initializes dictionary for enum animation states
        /// </summary>
        public SerializedDictionary<T, int> CreateAndInitDictionary<T>(GameObject sender) where T : Enum
        {
            var newDic = new SerializedDictionary<T, int>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                newDic.Add((T)item, Animator.StringToHash(item.ToString()));
            }

            return newDic;
        }

        /// <summary>
        /// Gets hash value for enum type from dictionary
        /// </summary>
        public int GetHash<T>(T enumType, SerializedDictionary<T, int> dictionary) where T : Enum
        {
            return dictionary[enumType];
        }

        /// <summary>
        /// Gets enum name by hash value from dictionary
        /// </summary>
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
    }
}