using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "ChracterSelect", menuName = "Angry_Girls/CharacterSettings/CharacterSelect")]
    public class CharacterSelect : ScriptableObject
    {
        public CharacterType[] selectedCharacters = new CharacterType[4];
    }
}