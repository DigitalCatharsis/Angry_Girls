using UnityEngine;

namespace Angry_Girls
{
    public enum DataType
    {
        AttackCondition,
    }

    public class DataFactory : MonoBehaviour, ICoreFactory<DataType>
    {
        [SerializeField] private GameObject attackCondition;

        private void Awake()
        {
            attackCondition = Resources.Load(DataType.AttackCondition.ToString()) as GameObject;
        }

        public GameObject SpawnGameobject(DataType VFX_Type, Vector3 position, Quaternion rotation)
        {
            switch (VFX_Type)
            {
                case DataType.AttackCondition:
                    {
                        return Instantiate(attackCondition, position, rotation);
                    }
                default:
                    {
                        Debug.Log("Cant instantiate " + VFX_Type);
                        return null;
                    }
            }
        }
    }
}