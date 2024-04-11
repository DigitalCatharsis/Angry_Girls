using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class CorutinesManager : Singleton<CorutinesManager>
    {
        [SerializedDictionary("Owner", "Corutine")]
        [SerializeField] private SerializedDictionary<GameObject, List<string>> _corutinesDictionary = new();

        public void AddValueToDictionary(GameObject owner, string corutineName)
        {
            if (!_corutinesDictionary.ContainsKey(owner))
            {
                _corutinesDictionary.Add(owner, new List<string>());
            }
            _corutinesDictionary[owner].Add(corutineName);
        }

        public void RemoveKeyFromDictionary(GameObject owner)
        {
            if (!_corutinesDictionary.ContainsKey(owner))
            {
                return;
            }
            _corutinesDictionary.Remove(owner);

        }

        public void RemoveValueFromDictionary(GameObject owner, string corutineName)
        {
            if (!_corutinesDictionary.ContainsKey(owner))
            {
                return;
            }
            _corutinesDictionary[owner].Remove(corutineName);
        }
    }
}