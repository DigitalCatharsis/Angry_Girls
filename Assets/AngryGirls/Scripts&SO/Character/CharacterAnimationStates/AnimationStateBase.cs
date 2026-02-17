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
        public AnimationPhase_Idle(CControl control) : base(control) { }

        private GameplayCharactersManager _charactersManager;

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
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            return nextState is not AnimationPhase_AttackFinish && nextState is not AnimationPhase_Landing;
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
            if (_control.CharacterMovement.IsGrounded)
            {
                _control.UnitHasPerformedLanding?.Invoke();
            }
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            if (_control.CharacterSettings.characterType == CharacterType.Player_YBot_Air_Green || _control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Air_Green)
            {
                return nextState is not AnimationPhase_Landing && nextState is not AnimationPhase_AttackFinish;
            }

            return nextState is not AnimationPhase_Idle;
        }
    }

    public class AnimationPhase_Attack : AnimationStateBase
    {
        private AttackAbilityData _attackAbility;

        public AnimationPhase_Attack(CControl control) : base(control) { }

        public override void OnEnter()
        {
            _attackAbility = GameplayCoreManager.Instance.PhaseFlowController.CurrentState == GameState.LaunchPhase
                ? _control.profile.CharacterSettings.AttackAbility_Launch
                : _control.profile.CharacterSettings.AttackAbility_Alternate;

            _control.isAttacking = true;
            _control.canUseAbility = false;

            AnimationTransitioner.ChangeAnimationStateCrossFade(
                _control.animator,
                StatesContainer.AttackDictionary[_attackAbility.attack_State.animation],
                _attackAbility.attack_State.transitionDuration);
        }
        public override void OnExit()
        {
            _control.isAttacking = false;
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            //air 
            if (_control.CharacterSettings.characterType == CharacterType.Player_YBot_Air_Green || _control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Air_Green)
            {
                return nextState is not AnimationPhase_Landing && nextState is not AnimationPhase_Airboned;
            }

            //ground
            if (_control.CharacterSettings.characterType == CharacterType.Player_YBot_Ground_Blue || _control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Ground_Blue || _control.CharacterSettings.characterType == CharacterType.Player_YBot_Ground_Original)
            {
                return nextState is AnimationPhase_AttackFinish || nextState is AnimationPhase_HitReaction || nextState is AnimationPhase_Death;
            }

            return nextState is not AnimationPhase_Idle;

        }

    }

    public class AnimationPhase_AttackFinish : AnimationStateBase
    {
        public AnimationPhase_AttackFinish(CControl control)
            : base(control) { }

        public override void OnEnter()
        {
            var attackFinishState = _control.Get_AttackAbility().attackFininsh_State;
            AnimationTransitioner.ChangeAnimationStateFixedTime(
                _control.animator,
                StatesContainer.AttackFinishDictionary[attackFinishState.animation],
                attackFinishState.transitionDuration);

            _control.isAttacking = false;
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            return nextState is not AnimationPhase_Airboned && nextState is not AnimationPhase_Landing;
        }
    }


    public class AnimationPhase_HitReaction : AnimationStateBase
    {
        public AnimationPhase_HitReaction(CControl control)
            : base(control) { }

        public override void OnEnter()
        {
            ColorDebugLog.Log($"{_control.name} has entered {_control.GetCurrentAnimationName()}. Settings isAttacking to false. And unitGotHit to true (on enter hit animation)", System.Drawing.KnownColor.Aqua);
            //_control.isAttacking = false;
            _control.isLanding = false;


            var randomHitAnimation = _settings.GetRandomState(_settings.hitReaction_States).animation;
            AnimationTransitioner.ChangeAnimationState(
                _control.animator,
                StatesContainer.HitReactionDictionary[randomHitAnimation],
                transitionDuration: 0.1f);
        }

        public override void OnExit()
        {
            ColorDebugLog.Log($"{_control.name} has entered OnExit from" +
                $" {_control.GetCurrentAnimationName()}. Settings unitGotHit to false (on exit hit animation)", System.Drawing.KnownColor.Aqua);
            //_control.unitGotHit = false;
        }

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {

            if (_control.CharacterSettings.characterType == CharacterType.Player_YBot_Air_Green || _control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Air_Green)
            {
                return nextState is not AnimationPhase_Landing && nextState is not AnimationPhase_Airboned;
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

        public override bool CanTransitionTo(IAnimationPhase nextState)
        {
            return nextState is AnimationPhase_Idle
                || nextState is AnimationPhase_HitReaction
                || nextState is AnimationPhase_Death;
        }
    }
}