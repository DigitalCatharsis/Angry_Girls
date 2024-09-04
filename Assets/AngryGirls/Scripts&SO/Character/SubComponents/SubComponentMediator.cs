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
    }

    public enum SubComponentType
    {
        CollisionSpheres,
        AnimationProcessor,
        BlockingManager,
        BoxColliderUpdater,
        LgroundDetector,
        LaunchLogic,
        CharacterMovement,
        AttackSystem,
        DamageProcessor,
    }
    public class SubComponentMediator : MonoBehaviour, IMediator<SubcomponentMediator_EventNames>
    {
        private CControl _control;

        private SubComponent<SubcomponentMediator_EventNames>[] _arrSubComponents;

        private AnimationProcessor _animationProcessor;
        private LaunchLogic _launchLogic;
        private AttackSystem _attackSystem;
        private DamageProcessor _damageProcessor;
        private CollisionSpheres _collisionSpheres;
        private BoxColliderUpdater _boxColliderUpdater;
        private BlockingManager _blockingManager;
        private GroundDetector _groundDetector;
        private CharacterMovement _characterMovement;

        public void OnAwake()
        {
            _control = GetComponentInParent<CControl>();
            InitComponents();

            _arrSubComponents = new SubComponent<SubcomponentMediator_EventNames>[Enum.GetNames(typeof(SubComponentType)).Length];
            _arrSubComponents = GetComponentsInChildren<SubComponent<SubcomponentMediator_EventNames>>();

            foreach (var component in _arrSubComponents)
            {
                component.control = _control;
                component.OnAwake();
            }
        }

        private void InitComponents()
        {
            _groundDetector = GetComponentInChildren<GroundDetector>();
            _animationProcessor = GetComponentInChildren<AnimationProcessor>();
            _launchLogic = GetComponentInChildren<LaunchLogic>();
            _collisionSpheres = GetComponentInChildren<CollisionSpheres>();
            _boxColliderUpdater = GetComponentInChildren<BoxColliderUpdater>();
            _blockingManager = GetComponentInChildren<BlockingManager>();
            _characterMovement = GetComponentInChildren<CharacterMovement>();
            _attackSystem = GetComponentInChildren<AttackSystem>();
            _damageProcessor = GetComponentInChildren<DamageProcessor>();
        }

        public void Notify(object sender, SubcomponentMediator_EventNames eventName)
        {
            if (eventName == SubcomponentMediator_EventNames.Launch_Unit)
            {
                ColorDebugLog.Log(_control.name + "'s" + " Subcomponent's Mediator reacts on " + SubcomponentMediator_EventNames.Launch_Unit + " and triggers following operations:", System.Drawing.KnownColor.ControlLightLight);
                _launchLogic.ProcessLaunch();
            }

            if (eventName == SubcomponentMediator_EventNames.Character_Reposition_ColliderSpheres)
            {
                //ColorDebugLog.Log(_control.name + "'s" + " Subcomponent's Mediator reacts on " + SubcomponentMediator_EventNames.Reposition_ColliderSpheres + " and triggers following operations:", System.Drawing.KnownColor.ControlLightLight);
                _collisionSpheres.RepositionAllSpheres();
            }
        }


        //TODO ask Danya
        public void CheckForDamage(object sender, SubcomponentMediator_EventNames eventName, Collider collider)
        {
            if (eventName == SubcomponentMediator_EventNames.CharacterCollider_Trigger_Enter)
            {
                //ColorDebugLog.Log(_control.name + "'s" + " Subcomponent's Mediator reacts on " + SubcomponentMediator_EventNames.Trigger_Enter + " and triggers following operations:", System.Drawing.KnownColor.ControlLightLight);
                _damageProcessor.CheckForDamage(collider);
            }
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

        #region TEMP METHODS
        //Where the fuck should i place iT?!
        public void TEMP_SetHeadSpinState()
        {
            _animationProcessor.ChangeAnimationState(GameLoader.Instance.statesDispatcher.staticAttack_States_Dictionary[StaticAttack_States.A_HeadSpin_Attack_Static], 0, transitionDuration: 1);
        }
        public void TEMP_SetShorukenLandingState()
        {
            _animationProcessor.ChangeAnimationState(GameLoader.Instance.statesDispatcher.staticAttack_States_Dictionary[StaticAttack_States.A_Shoryuken_Landing_Static], 0, transitionDuration: 1);
        }
        #endregion
    }
}