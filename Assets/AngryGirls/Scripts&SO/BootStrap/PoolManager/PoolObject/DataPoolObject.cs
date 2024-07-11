using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class DataPoolObject : MonoBehaviour, IPoolObject
    {
        public DataType poolObjectType;
        [SerializeField] float scheduledOffTime;
        private Coroutine offRoutine;

        private void OnEnable()                //Есть бага, когда объект не апдейтится, объект пула перестает деактивироваться. Это FailSafe
        {
            if (offRoutine != null)
            {
                StopCoroutine(offRoutine);
            }

            if (scheduledOffTime > 0f)
            {
                offRoutine = StartCoroutine(_ScheduledOff());
            }
        }

        public void TurnOff()
        {
            this.transform.parent = null;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
        }

        IEnumerator _ScheduledOff()
        {
            yield return new WaitForSeconds (scheduledOffTime);
            if (!Singleton.Instance.poolManager.dataPoolDictionary[poolObjectType].Contains(this.gameObject))
            {
                TurnOff();
                ReturnToPool();
            }
        }

        public void ReturnToPool()
        {
            if (!Singleton.Instance.poolManager.dataPoolDictionary[poolObjectType].Contains(this.gameObject))
            {
                Singleton.Instance.poolManager.AddObject(poolObjectType, Singleton.Instance.poolManager.dataPoolDictionary, this.gameObject);
            }
        }
    }
}