using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class CharacterManager : MonoBehaviour
    {
        public List<GameObject> playableCharacters = new List<GameObject>();
        public List<GameObject> enemyCharacters = new List<GameObject>();
        public GameObject GetPlaybleCharacter(GameObject character)
        {
            return playableCharacters.Find(x => x == character);
        }

        public GameObject GetEnemyCharacter(GameObject character)
        {
            return enemyCharacters.Find(x => x == character);
        }

        public GameObject GetCharacter(GameObject character)
        {
            if(GetPlaybleCharacter(character))
            {
                return character;
            }
            if(GetEnemyCharacter(character))
            {
                return character;
            }

            return null;
        }
    }
}