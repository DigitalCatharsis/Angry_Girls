using System;
using UnityEngine;

namespace Angry_Girls
{
    public class SubComponentsController : MonoBehaviour
    {
        public static SubComponentsController Instance;
        
        private SubComponent<SubcomponentMediator_EventNames>[] _arrSubComponents;
        
        public void OnAwake()
        {
            _arrSubComponents = new SubComponent<SubcomponentMediator_EventNames>[Enum.GetNames(typeof(SubComponentType)).Length];
            _arrSubComponents = GetComponentsInChildren<SubComponent<SubcomponentMediator_EventNames>>();

            var control = GetComponentInParent<CControl>();
            foreach (var component in _arrSubComponents)
            {
                component.control = control;
                component.OnAwake();
            }

            Instance = this;
        }
        
        public void OnUpdate()
        {
            foreach (var component in _arrSubComponents)
            {
                component.OnUpdate();
            }
        }
        public void OnFixedUpdate()
        {
            foreach (var component in _arrSubComponents)
            {
                component.OnFixedUpdate();
            }
        }
        public void OnLateUpdate()
        {
            foreach (var component in _arrSubComponents)
            {
                component.OnLateUpdate();
            }
        }

        public void OnStart()
        {
            foreach (var component in _arrSubComponents)
            {
                component.OnStart();
            }
        }
        public void OnComponentEnable()
        {
            foreach (var component in _arrSubComponents)
            {
                component.OnComponentEnable();
            }
        }
        public void OnComponentLateUpdate()
        {
            foreach (var component in _arrSubComponents)
            {
                component.OnComponentEnable();
            }
        }
    }
}