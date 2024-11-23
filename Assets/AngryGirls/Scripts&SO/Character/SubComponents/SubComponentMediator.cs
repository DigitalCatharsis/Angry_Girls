using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum SubcomponentMediator_EventNames
    {
        Launch_Unit,
        Character_Reposition_ColliderSpheres,
        Character_Check_For_Damage,
        CharacterCollider_Trigger_Enter,
        HealthBarGeneration,
    }

    public enum SubComponentType
    {
        CollisionSpheres,
        AnimationProcessor,
        BlockingManager,
        BoxColliderUpdater,
        LgroundDetector,
        LaunchLogic,
        DamageProcessor,
    }
    public class SubComponentMediator : MonoBehaviour, IMediator<SubcomponentMediator_EventNames>
    {
        private CControl _control;
        private AnimationProcessor _animationProcessor;
        private LaunchLogic _launchLogic;
        private DamageProcessor _damageProcessor;
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
            _launchLogic = GetComponentInChildren<LaunchLogic>();
            _collisionSpheres = GetComponentInChildren<CollisionSpheres>();
            _boxColliderUpdater = GetComponentInChildren<BoxColliderUpdater>();
            _blockingManager = GetComponentInChildren<BlockingManager>();
            _damageProcessor = GetComponentInChildren<DamageProcessor>();
        }

        public void Notify(object sender, SubcomponentMediator_EventNames eventName)
        {
            if (eventName == SubcomponentMediator_EventNames.Launch_Unit)
            {
                //ColorDebugLog.Log(_control.name + "'s" + " Subcomponent's Mediator reacts on " + SubcomponentMediator_EventNames.Launch_Unit + " and triggers following operations:", System.Drawing.KnownColor.ControlLightLight);
                _launchLogic.ProcessLaunch();
            }

            if (eventName == SubcomponentMediator_EventNames.Character_Reposition_ColliderSpheres)
            {
                _collisionSpheres.RepositionAllSpheres();
            }

            if (eventName == SubcomponentMediator_EventNames.Character_Reposition_ColliderSpheres)
            {
                _collisionSpheres.RepositionAllSpheres();
            }
        }

        //TODO ask Danya
        public void NotifyDamaged(object sender, SubcomponentMediator_EventNames eventName, Collider collider)
        {
            if (eventName == SubcomponentMediator_EventNames.CharacterCollider_Trigger_Enter)
            {
                _damageProcessor.CheckForDamage(collider);
            }
        }

        #region TEMP METHODS
        //Where the fuck should i place iT?!
        public void TEMP_SetHeadSpinState()
        {
            _animationProcessor.ChangeAnimationState(GameLoader.Instance.statesContainer.attack_Dictionary[Attack_States.Alternate_HeadSpin_Attack], 0, transitionDuration: 1);
        }
        //public void TEMP_SetShorukenLandingState()
        //{
        //    _animationProcessor.ChangeAnimationStateFixedTime(GameLoader.Instance.statesContainer.landingNames_Dictionary[Landing_States.Landing_Shoryuken], 0, transitionDuration: 1);
        //}
        public Vector3 GetBottomContactPoint()
        {
            return _groundDetector.bottomRaycastContactPoint;
        }
        #endregion
    }
}