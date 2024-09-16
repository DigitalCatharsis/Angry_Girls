using UnityEngine;

namespace Angry_Girls
{
    public enum CharacterType
    {
        YBot_Original,
        YBot_Red,
        YBot_Green,
        YBot_Yellow,
        YBot_Blue,
    }
    public class CharacterFactory : MonoBehaviour, ICoreFactory<CharacterType> 
    {

        [SerializeField] private GameObject yBotBluePrefab;
        [SerializeField] private GameObject yBotOriginalrefab;
        [SerializeField] private GameObject yBotGreenPrefab;
        [SerializeField] private GameObject yBotYellowPrefab;
        [SerializeField] private GameObject yBotRedPrefab;

        private void Awake()
        {
            yBotBluePrefab = Resources.Load(CharacterType.YBot_Blue.ToString()) as GameObject;
            yBotOriginalrefab = Resources.Load(CharacterType.YBot_Original.ToString()) as GameObject;
            yBotRedPrefab = Resources.Load(CharacterType.YBot_Red.ToString()) as GameObject;
            yBotGreenPrefab = Resources.Load(CharacterType.YBot_Green.ToString()) as GameObject;
            yBotYellowPrefab = Resources.Load(CharacterType.YBot_Yellow.ToString()) as GameObject;
        }

        public PoolObject SpawnGameobject(CharacterType characterType, Vector3 position, Quaternion rotation)
        {
            switch (characterType)
            {
                case CharacterType.YBot_Original:
                    {
                        return (Instantiate(yBotOriginalrefab, position, rotation)).GetComponent<PoolObject>();
                    }
                case CharacterType.YBot_Yellow:
                    {
                        return (Instantiate(yBotYellowPrefab, position, rotation)).GetComponent<PoolObject>();
                    }
                case CharacterType.YBot_Green:
                    {
                        return (Instantiate(yBotGreenPrefab, position, rotation)).GetComponent<PoolObject>();
                    }
                case CharacterType.YBot_Red:
                    {
                        return (Instantiate(yBotRedPrefab, position, rotation)).GetComponent<PoolObject>();
                    }
                case CharacterType.YBot_Blue:
                    {
                        return (Instantiate(yBotBluePrefab, position, rotation)).GetComponent<PoolObject>();
                    }
                default:  //Интересно, как заткнуть эту дыру грамотно....
                    {
                        return null;
                    }
            }
        }
    }

}
