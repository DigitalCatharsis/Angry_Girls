using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [Serializable]
    public class Serializable_SubList<T> : List<T>
    {
        [HideInInspector] public string name;
        public List<T> subList = new();
    }

    [Serializable]
    public class Serializable_List<T>
    {
        public List<Serializable_SubList<T>> instance = new();

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

