using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Interface for animation states in the state machine
    /// </summary>
    public interface IAnimationPhase
    {
        public void OnEnter();
        public void OnUpdate();
        public void OnExit();
        public bool CanTransitionTo(IAnimationPhase nextState);
    }

    /// <summary>
    /// Base class for all animation states
    /// </summary>
    public abstract class AnimationStateBase : IAnimationPhase
    {
        protected readonly CControl _control;
        protected readonly Animator _animator;
        protected readonly CharacterSettings _settings;
        protected readonly AttackAbilityData _attackAbilityData;

        protected AnimationStateBase(CControl control)
        {
            _control = control;
            _animator = control.animator;
            _settings = control.CharacterSettings;
        }

        /// <summary>
        /// Called when entering the state
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// Called every frame while in the state
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Called when exiting the state
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// Determines if transition to next state is allowed
        /// </summary>
        /// <param name="nextState">State to transition to</param>
        /// <returns>True if transition is allowed</returns>
        public virtual bool CanTransitionTo(IAnimationPhase nextState)
        {
            return true;
        }
    }

    public class AnimationPhase_Idle : AnimationStateBase
    {
        private bool _hasTriggeredTransition = false;
        private GameplayCharactersManager _charactersManager;

        public AnimationPhase_Idle(CControl control) : base(control) { }

        public override void OnEnter()
        {
            if (_charactersManager == null)
            {
                _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            }

            var idleState = _settings.GetRandomState(_settings.idle_States);
            AnimationTransitioner.ChangeAnimationStateCrossFade
                (
                _control.animator,
                   StatesContainer.IdleDictionary[idleState.animation],
                   idleState.transitionDuration
                );

            _hasTriggeredTransition = false;
        }

        public override void OnUpdate()
        {
            if (_control.gameObject.layer == LayerMask.NameToLayer("CharacterToLaunch"))
            {
                return;
            }

            if (_control.playerOrAi == PlayerOrAi.Player && _control.IsInIdleState())
            {
                TurnToTheClosestEnemy(PlayerOrAi.Bot);
            }
            else if (_control.playerOrAi == PlayerOrAi.Bot)
            {
                TurnToTheClosestEnemy(PlayerOrAi.Player);
            }

            if (_control.IsInIdleState() && _control.IsAnimationEnding(_control.animator, 0.95f) && !_hasTriggeredTransition)
            {
                _hasTriggeredTransition = true;

                var idleState = _settings.GetRandomState(_settings.idle_States);
                AnimationTransitioner.ChangeAnimationStateCrossFade
                    (
                    _control.animator,
                       StatesContainer.IdleDictionary[idleState.animation],
                       idleState.transitionDuration
                    );
            }
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            if (_control.CharacterSettings.unitType == UnitType.Air && _control.hasBeenLaunched)
            {
                return nextState is not AnimationPhase_Landing && nextState is not AnimationPhase_Airboned;
            }

            return nextState is not AnimationPhase_Landing;
        }

        private void TurnToTheClosestEnemy(PlayerOrAi typeOfUnitToTurn)
        {
            float closestDistance = 9999f;
            var collection = new List<CControl>();

            collection = _charactersManager.GetAliveCharacters(typeOfUnitToTurn);

            foreach (var character in collection)
            {
                var distance = _control.CharacterMovement.Rigidbody.position.z -
                               character.CharacterMovement.Rigidbody.position.z;

                if (Math.Abs(closestDistance) > Math.Abs(distance))
                {
                    closestDistance = distance;
                }
            }

            if (closestDistance > 0)
            {
                _control.CharacterMovement.SetRotation(Quaternion.Euler(0, 180, 0));
            }
            else if (closestDistance < 0)
            {
                _control.CharacterMovement.SetRotation(Quaternion.Euler(0, 0, 0));
            }
        }
    }

    public class AnimationPhase_Death : AnimationStateBase
    {
        public AnimationPhase_Death(CControl control) : base(control) { }

        public override void OnEnter()
        {
            var randomDeathAnimation = _settings.GetRandomState(_settings.death_States).animation;
            AnimationTransitioner.ChangeAnimationState(
                _control.animator,
                StatesContainer.DeathDictionary[randomDeathAnimation],
                transitionDuration: 0.1f);
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            //cant change state, lol
            return false;
        }
    }

    /// <summary>
    /// Represents airborne state for character animation
    /// </summary>
    public class AnimationPhase_Airboned : AnimationStateBase
    {
        public AnimationPhase_Airboned(CControl control)
            : base(control) { }

        public override void OnEnter()
        {
            AnimationTransitioner.ChangeAnimationStateCrossFade(
                _control.animator,
                StatesContainer.FallingDictionary[_settings.airbonedFlying_States.animation],
                _settings.airbonedFlying_States.transitionDuration);
        }

        public override void OnUpdate()
        {
            //if (_control.CharacterMovement.IsGrounded)
            //{
            //    _control.UnitHasPerformedLanding?.Invoke();
            //}
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            if (_control.CharacterSettings.characterType == CharacterType.Player_YBot_Air_Green || _control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Air_Green)
            {
                return nextState is not AnimationPhase_Landing;
            }

            return nextState is not AnimationPhase_Idle && nextState is not AnimationPhase_Airboned;
        }
    }

    public class AnimationPhase_Attack : AnimationStateBase
    {
        private AttackAbilityData _abilityData;
        private Action<CControl> _onEnterDelegate;
        private Action<CControl> _onUpdateDelegate;
        private Action<CControl> _onExitDelegate;

        public AnimationPhase_Attack(CControl control) : base(control) { }

        private GamePhaseFlowController _phaseFlowController;

        public override void OnEnter()
        {
            _control.canUseAbility = false;

            if (_phaseFlowController == null)
            {
                _phaseFlowController = GameplayCoreManager.Instance.GamePhaseFlowController;
            }


            //get data
            if (_phaseFlowController.CurrentGamePhaseState == GamePhaseNames.LaunchPhase)
            {
                _abilityData = _control.attackAbility.LaunchPrepData;
                _onEnterDelegate += _control.attackAbility.OnLaunchPrepEnter;
                _onUpdateDelegate += _control.attackAbility.OnLaunchPrepUpdate;
                _onExitDelegate += _control.attackAbility.OnLaunchPrepExit;


            }
            else if (_phaseFlowController.CurrentGamePhaseState == GamePhaseNames.AlternatePhase)
            {
                _abilityData = _control.attackAbility.AlternatePrepData;
                _onEnterDelegate += _control.attackAbility.OnAlternatePrepEnter;
                _onUpdateDelegate += _control.attackAbility.OnAlternatePrepUpdate;
                _onExitDelegate += _control.attackAbility.OnAlternatePrepExit;
            }

            _control.CharacterMovement.ResetVelocity();
            _control.CharacterMovement.ApplyRigidForce(new Vector3(0, _abilityData.attackMovementForce.y, _abilityData.attackMovementForce.z * _control.transform.forward.z), ForceMode.VelocityChange);

            //process animation
            AnimationTransitioner.ChangeAnimationStateCrossFade(
                _control.animator,
                StatesContainer.AttackDictionary[_abilityData.attack_State.animation],
                _abilityData.attack_State.transitionDuration);

            //process On method
            _onEnterDelegate.Invoke(_control);
        }

        public override void OnUpdate()
        {
            _onUpdateDelegate.Invoke(_control);
        }

        public override void OnExit()
        {
            _onExitDelegate?.Invoke(_control);

            //Unsubscribe
            var enterSubscribedList = _onEnterDelegate.GetInvocationList();
            foreach (var sub in enterSubscribedList)
            {
                _onEnterDelegate -= sub as Action<CControl>;
            }

            var updateSubscribedList = _onUpdateDelegate.GetInvocationList();
            foreach (var sub in updateSubscribedList)
            {
                _onUpdateDelegate -= sub as Action<CControl>;
            }
            var exitSubscribedList = _onExitDelegate.GetInvocationList();
            foreach (var sub in exitSubscribedList)
            {
                _onExitDelegate -= sub as Action<CControl>;
            }

            //finish turn
            _control.FinishTurn();
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            //air 
            if (_control.CharacterSettings.unitType == UnitType.Air)
            {
                return nextState is not AnimationPhase_Landing && nextState is not AnimationPhase_Airboned;
            }

            //ground
            if (_control.CharacterSettings.unitType == UnitType.Ground)
            {
                return nextState is AnimationPhase_HitReaction || nextState is AnimationPhase_Death || nextState is AnimationPhase_Idle;
            }

            return nextState is not AnimationPhase_Idle;

        }

    }

    public class AnimationPhase_HitReaction : AnimationStateBase
    {
        public AnimationPhase_HitReaction(CControl control)
            : base(control) { }

        //private bool _hasFinishedByTimer = false;

        public override void OnEnter()
        {
            //_hasFinishedByTimer = false;
            var randomHitAnimation = _settings.GetRandomState(_settings.hitReaction_States).animation;
            AnimationTransitioner.ChangeAnimationState(
                _control.animator,
                StatesContainer.HitReactionDictionary[randomHitAnimation],
                transitionDuration: 0.1f,
                dontChangeOnSameAnimation: false);

            Debug.Log($"{_control.name} entered HitReaction");
        }

        public override void OnUpdate()
        {
            if (_control.IsAnimationEnding(_control.animator, 0.95f) && StatesContainer.HitReactionDictionary.ContainsValue(_control.GetCurrentAnimationHash()))
            {
                //_hasFinishedByTimer = true;
                _control.UnitHasFinishedHitReaction?.Invoke();
            }
        }

        public override void OnExit()
        {
            Debug.Log($"{_control.name} exited HitReaction");
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            if (_control.CharacterSettings.unitType == UnitType.Air)
            {
                return nextState is AnimationPhase_HitReaction || nextState is AnimationPhase_Death || nextState is AnimationPhase_Idle;
            }

            return nextState is not AnimationPhase_Landing;
        }
    }

    public class AnimationPhase_Landing : AnimationStateBase
    {
        public AnimationPhase_Landing(CControl control)
            : base(control) { }

        public override void OnEnter()
        {
            AnimationTransitioner.ChangeAnimationStateCrossFade(
                _control.animator,
               StatesContainer.LandingDictionary[_settings.landing_State.animation],
               _settings.landing_State.transitionDuration);
        }

        public override void OnUpdate()
        {
            if (_control.IsAnimationEnding(_control.animator, 0.95f))
            {
                _control.UnitHasFinishedLanding?.Invoke();
            }
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            return nextState is AnimationPhase_Idle
                || nextState is AnimationPhase_HitReaction
                || nextState is AnimationPhase_Death
                || nextState is AnimationPhase_Attack;
        }
    }
}