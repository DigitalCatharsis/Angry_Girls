using System.Collections.Generic;

namespace Angry_Girls

{
    [System.Serializable]
    public class SaveData
    {
        public int credits;
        public List<CharacterProgress> characters;
        public List<string> unlockedItems;
    }

    [System.Serializable]
    public class CharacterProgress
    {
        public string characterID;
        public int level;
        public float bonusHealth;
        public float bonusDamage;
        public List<string> equippedItems;
    }
}