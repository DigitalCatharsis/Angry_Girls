using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class CharacterManager : MonoBehaviour
    {
        public List<CControl> playableCharacters = new List<CControl>();
        public List<CControl> enemyCharacters = new List<CControl>();
        public CControl GetPlaybleCharacter(CControl character)
        {
            return playableCharacters.Find(x => x == character);
        }

        public CControl GetEnemyCharacter(CControl character)
        {
            return enemyCharacters.Find(x => x == character);
        }

        public CControl GetCharacter(CControl character)
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