using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum SubComponentType
    {
        collisionSpheres,
        animationProcessor,
        BlockingManager,
        boxColliderUpdater,
        groundDetector,
        launchLogic,
    }
    public class SubComponentProcessor : MonoBehaviour
    {
        private CharacterControl control;

        public SubComponent[] arrSubComponents;

        public GroundDetector groundDetector;
        public AnimationProcessor animationProcessor;
        public LaunchLogic launchLogic;
        public CollisionSpheres collisionSpheres;
        public BoxColliderUpdater boxColliderUpdater;
        public BlockingManager blockingManager;

        public void OnAwake()
        {
            control = GetComponentInParent<CharacterControl>();

            arrSubComponents = new SubComponent[Enum.GetNames(typeof(SubComponentType)).Length];
            arrSubComponents = GetComponentsInChildren<SubComponent>();

            foreach (var component in arrSubComponents)
            {
                component.control = control; 
                component.OnAwake();
            }
        }

        public void OnUpdate()
        {
            foreach (var component in arrSubComponents)
            {
                component.OnUpdate();
            }
        }
        public void OnFixedUpdate()
        {
            foreach (var component in arrSubComponents)
            {
                component.OnFixedUpdate();
            }
        }
        public void OnLateUpdate()
        {
            foreach (var component in arrSubComponents)
            {
                component.OnLateUpdate();
            }
        }

        public void OnStart()
        {
            foreach (var component in arrSubComponents)
            {
                component.OnStart();
            }
        }
        public void OnComponentEnable()
        {
            foreach (var component in arrSubComponents)
            {
                component.OnComponentEnable();
            }
        }
        public void OnComponentLateUpdate()
        {
            foreach (var component in arrSubComponents)
            {
                component.OnComponentEnable();
            }
        }
    }
}