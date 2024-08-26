using System;
using UnityEngine;

namespace Angry_Girls
{
    public abstract class SubComponent<T> : BaseMediatorComponent<T> where T : Enum
    {
        public CControl control;
        public abstract void OnAwake();
        public abstract void OnUpdate();
        public abstract void OnComponentEnable();
        public abstract void OnFixedUpdate(); 
        public abstract void OnStart();
        public abstract void OnLateUpdate();
    }
}
