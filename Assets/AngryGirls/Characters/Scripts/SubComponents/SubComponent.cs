using UnityEngine;

namespace Angry_Girls
{
    public abstract class SubComponent : MonoBehaviour
    {
        public CharacterControl control;
        public abstract void OnAwake();
        public abstract void OnUpdate();
        public abstract void OnComponentEnable();
        public abstract void OnFixedUpdate(); 
        public abstract void OnStart();
        public abstract void OnLateUpdate();
    }
}
