using System;
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
            foreach (var elem in playableCharacters)
            {
                if (elem == character)
                {
                    return elem;
                }
            }
            return null;
        }
        public GameObject GetEnemyCharacter(GameObject character)
        {
            foreach (var elem in enemyCharacters)
            {
                if (elem == character)
                {
                    return elem;
                }
            }
            return null;
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