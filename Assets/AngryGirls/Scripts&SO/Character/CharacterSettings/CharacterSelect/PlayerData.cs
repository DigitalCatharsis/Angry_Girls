using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Angry_Girls/CharacterSettings/PlayerData")]
    public class PlayerData : ScriptableObject
    {        
        public SerializedDictionary<int, CharacterSettings> selectedCharacters = new();

        public SerializedDictionary<int, CharacterSettings> avaibleCharacterPool = new();
    }
}