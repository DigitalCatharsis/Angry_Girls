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
            if (!Singleton.Instance.poolManager.characterPoolDictionary[poolObjectType].Contains(this.gameObject))
            {
                Singleton.Instance.poolManager.AddObject(poolObjectType, Singleton.Instance.poolManager.characterPoolDictionary, this.gameObject);
            }
        }
    }
}