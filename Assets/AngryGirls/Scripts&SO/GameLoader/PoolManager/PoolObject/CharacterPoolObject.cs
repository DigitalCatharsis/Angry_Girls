using UnityEngine;

namespace Angry_Girls
{
    public class CharacterPoolObject : MonoBehaviour, IPoolObject
    {
        public CharacterType poolObjectType;

        public void TurnOff()
        {
            this.transform.parent = null;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;

            ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.characterPoolDictionary[poolObjectType].Contains(this.gameObject))
            {
                GameLoader.Instance.poolManager.AddObject(poolObjectType, GameLoader.Instance.poolManager.characterPoolDictionary, this.gameObject);
            }
        }
    }
}