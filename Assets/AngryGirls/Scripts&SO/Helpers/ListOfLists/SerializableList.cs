using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Serializable sublist with a name field for identification
    /// </summary>
    [Serializable]
    public class Serializable_SubList<T> : List<T>
    {
        [HideInInspector] public string name;
        public List<T> subList = new();
    }

    /// <summary>
    /// Serializable list containing multiple named sublists
    /// </summary>
    [Serializable]
    public class Serializable_List<T>
    {
        public List<Serializable_SubList<T>> instance = new();

        /// <summary>
        /// Retrieves a sublist by its name
        /// </summary>
        /// <param name="sublistName">Name of the sublist to find</param>
        /// <returns>The found sublist or null</returns>
        public Serializable_SubList<T> GetSublistByName(string sublistName)
        {
            foreach (var sublist in instance)
            {
                var name = sublistName.Replace(typeof(Serializable_List<T>).Namespace.ToString() + ".", "");
                if (sublist.name == name)
                {
                    return sublist;
                }
            }

            return null;
        }
    }
}