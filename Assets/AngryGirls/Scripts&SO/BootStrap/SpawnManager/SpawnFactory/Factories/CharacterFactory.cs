using UnityEngine;

namespace Angry_Girls
{
    //TODO: HashManager
    public enum CharacterType
    {
        YBot_Blue,
        YBot_Red,
        YBot_Green,
        YBot_Yellow,
    }
    public class CharacterFactory : MonoBehaviour, ICoreFactory<CharacterType> 
    {

        [SerializeField] private GameObject yBotBluePrefab;
        [SerializeField] private GameObject yBotRedPrefab;
        [SerializeField] private GameObject yBotGreenPrefab;
        [SerializeField] private GameObject yBotYellowPrefab;

        private void Awake()
        {
            yBotBluePrefab = Resources.Load(CharacterType.YBot_Blue.ToString()) as GameObject;
            yBotRedPrefab = Resources.Load(CharacterType.YBot_Red.ToString()) as GameObject;
            yBotGreenPrefab = Resources.Load(CharacterType.YBot_Green.ToString()) as GameObject;
            yBotYellowPrefab = Resources.Load(CharacterType.YBot_Yellow.ToString()) as GameObject;
        }

        public GameObject SpawnGameobject(CharacterType characterType, Vector3 position, Quaternion rotation)
        {
            switch (characterType)
            {
                case CharacterType.YBot_Blue:
                    {
                        return Instantiate(yBotBluePrefab, position, rotation);
                    }
                case CharacterType.YBot_Yellow:
                    {
                        return Instantiate(yBotYellowPrefab, position, rotation);
                    }
                case CharacterType.YBot_Green:
                    {
                        return Instantiate(yBotGreenPrefab, position, rotation);
                    }
                case CharacterType.YBot_Red:
                    {
                        return Instantiate(yBotRedPrefab, position, rotation);
                    }
                default:  //Интересно, как заткнуть эту дыру грамотно....
                    {
                        return null;
                    }
            }
        }
    }

}
