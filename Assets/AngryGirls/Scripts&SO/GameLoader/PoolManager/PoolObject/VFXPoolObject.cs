using UnityEngine;

namespace Angry_Girls
{
    public class VFXPoolObject : MonoBehaviour, IPoolObject
    {
        public VFX_Type poolObjectType;
        //[SerializeField] float scheduledOffTime;
        //private Coroutine offRoutine;

        private void OnEnable()                //Есть бага, когда объект не апдейтится, объект пула перестает деактивироваться. Это FailSafe
        {
            //if (offRoutine != null)
            //{
            //    StopCoroutine(offRoutine);
            //}

            //if (scheduledOffTime > 0f)
            //{
            //    offRoutine = StartCoroutine(_ScheduledOff());
            //}
        }

        private void OnDisable()
        {
            ReturnToPool();
        }

        public void TurnOff()
        {
            //this.transform.parent = null;
            //this.transform.position = Vector3.zero;
            //this.transform.rotation = Quaternion.identity;
        }

        //IEnumerator _ScheduledOff()
        //{
        //    yield return new WaitForSeconds(scheduledOffTime);
        //    if (!Singleton.Instance.poolManager.vfxPoolDictionary[poolObjectType].Contains(this.gameObject))
        //    {
        //        TurnOff();
        //        ReturnToPool();
        //    }
        //}

        public void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.vfxPoolDictionary[poolObjectType].Contains(this.gameObject))
            {
                GameLoader.Instance.poolManager.AddObject(poolObjectType, GameLoader.Instance.poolManager.vfxPoolDictionary, this.gameObject);
            }
        }
    }
}