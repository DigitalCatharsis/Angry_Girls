using UnityEngine;

namespace Angry_Girls
{
    public interface ICoreFactory<in T>  where T : System.Enum
    {
        public GameObject SpawnGameobject(T Type, Vector3 position, Quaternion rotation);
    }
}
