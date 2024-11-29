using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum UnitLaunch_EventNames
    {
        Launch_Unit,
    }

    public enum NotifyContact_EventNames
    {
        CharacterCollider_Trigger_Enter,
    }

    public enum NotifyCollision_EventNames
    {
        Character_Reposition_ColliderSpheres,
    }

    public enum NotifyDamage_EventNames
    {
        CheckDamage,
        DamageRecived
    }

    public class SubComponentMediator : MonoBehaviour, IMediator<UnitLaunch_EventNames>
    {
        private CControl _control;
        private AnimationProcessor _animationProcessor;
        private DamageHandler _damageProcessor;
        private CollisionSpheres _collisionSpheres;
        private BoxColliderUpdater _boxColliderUpdater;
        private BlockingManager _blockingManager;
        private GroundDetector _groundDetector;

        public void OnAwake()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            _groundDetector = GetComponentInChildren<GroundDetector>();
            _animationProcessor = GetComponentInChildren<AnimationProcessor>();
            _collisionSpheres = GetComponentInChildren<CollisionSpheres>();
            _boxColliderUpdater = GetComponentInChildren<BoxColliderUpdater>();
            _blockingManager = GetComponentInChildren<BlockingManager>();
            _damageProcessor = GetComponentInChildren<DamageHandler>();
        }

        //replace? Todo:
        public void Notify(object sender, UnitLaunch_EventNames eventName)
        {
        }

        public void Notify(object sender, NotifyContact_EventNames eventName, Collider collider)
        {
            if (eventName == NotifyContact_EventNames.CharacterCollider_Trigger_Enter)
            {
                _damageProcessor.CheckForDamage(collider);
            }
        }
        public void Notify(object sender, NotifyCollision_EventNames eventName)
        {
            if (eventName == NotifyCollision_EventNames.Character_Reposition_ColliderSpheres)
            {
                _collisionSpheres.RepositionAllSpheres();
            }
        }

        public Vector3 Notify_GetBottomContactPoint(object sender)
        {
            return _groundDetector.bottomRaycastContactPoint;
        }
    }
}