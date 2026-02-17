using AYellowpaper.SerializedCollections;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Angry_Girls
{
    public class AnimationProcessor : SubComponent
    {
        private AnimationPhaseMachine _phaseMachine;

        public override void OnStart()
        {
            if (control.playerOrAi == PlayerOrAi.Bot)
            {
                control.canCheckGlobalBehavior = true;
            }

            control.UnitHasBeenLaunched += ProcessLaunch;
            control.UnitHasPerformedLanding += PerformLanding;
            control.UnitPerformedAttack += ProcessAttack;
            control.UnitPerformedAttackFinish += ProcessAttackFinish;
            control.UnitCallsForStopAttack += CalculatePhaseAfterAttack;
            control.UnitCallsForStopAttackfiniss += ProcessIdle;

            InitializeStateMachine();
        }

        private void OnDestroy()
        {
            control.UnitHasBeenLaunched -= ProcessLaunch;
            control.UnitHasPerformedLanding -= PerformLanding;
            control.UnitPerformedAttack -= ProcessAttack;
            control.UnitPerformedAttackFinish -= ProcessAttackFinish;
            control.UnitCallsForStopAttack -= CalculatePhaseAfterAttack;
            control.UnitCallsForStopAttackfiniss -= ProcessIdle;
        }

        private void InitializeStateMachine()
        {
            var phases = new IAnimationPhase[]
            {
                new AnimationPhase_Idle(control),
                new AnimationPhase_Attack(control),
                new AnimationPhase_AttackFinish(control),
                new AnimationPhase_HitReaction(control),
                new AnimationPhase_Death(control),
                new AnimationPhase_Landing(control),
                new AnimationPhase_Airboned(control),
            };

            _phaseMachine = new AnimationPhaseMachine(phases);
            InitFirstState();
        }

        public override void OnFixedUpdate()
        {
            //ProcessPhaseBehavior();
            _phaseMachine.Update();
        }


        private void PerformLanding()
        {
            if (control.CharacterSettings.characterType == CharacterType.Player_YBot_Air_Green || control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Air_Green)
            {
                _phaseMachine.ChangePhase<AnimationPhase_Idle>(control.gameObject);
            }

            _phaseMachine.ChangePhase<AnimationPhase_Landing>(control.gameObject);
        }

        private void InitFirstState()
        {
            if (control.CharacterMovement.IsGrounded)
            {
                _phaseMachine.ChangePhase<AnimationPhase_Idle>(control.gameObject);
            }
            else
            {
                _phaseMachine.ChangePhase<AnimationPhase_Airboned>(control.gameObject);
            }
        }

        private void ProcessLaunch()
        {
            _phaseMachine.ChangePhase<AnimationPhase_Airboned>(control.gameObject);
        }

        private void ProcessIdle()
        {
            _phaseMachine.ChangePhase<AnimationPhase_Idle>(control.gameObject);
        }

        private void CalculatePhaseAfterAttack()
        {
            if (control.CharacterSettings.characterType == CharacterType.Player_YBot_Air_Green || control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Air_Green)
            {
                _phaseMachine.ChangePhase<AnimationPhase_Idle>(control.gameObject);
            }


            if (control.CharacterSettings.characterType == CharacterType.Player_YBot_Ground_Blue
                || control.CharacterSettings.characterType == CharacterType.Enemy_YBot_Ground_Blue
                || control.CharacterSettings.characterType == CharacterType.Player_YBot_Ground_Original)
            {
                _phaseMachine.ChangePhase<AnimationPhase_AttackFinish>(control.gameObject);
            }

            if (control.CharacterMovement.IsGrounded)
            {
                _phaseMachine.ChangePhase<AnimationPhase_Landing>(control.gameObject);
            }
            else
            {
                _phaseMachine.ChangePhase<AnimationPhase_Airboned>(control.gameObject);
            }
        }

        private void ProcessAttack()
        {
            _phaseMachine.ChangePhase<AnimationPhase_Attack>(control.gameObject);
        }

        private void ProcessAttackFinish()
        {
            _phaseMachine.ChangePhase<AnimationPhase_AttackFinish>(control.gameObject);
        }

        ///// <summary>
        ///// Processes behavior based on current game phase
        ///// </summary>
        //private void ProcessPhaseBehavior()
        //{
        //    var currentPhase = GameplayCoreManager.Instance.PhaseFlowController.CurrentState;

        //    if (currentPhase == GameState.LaunchPhase && control.hasBeenLaunched && !control.hasFinishedLaunchingTurn)
        //    {
        //        ProcessLaunchingPhase();
        //    }
        //    else if (currentPhase == GameState.AlternatePhase && !control.hasFinishedAlternateAttackTurn && control.isAttacking)
        //    {
        //        ProcessAlternatePhase();
        //    }

        //    ProcessGlobalBehavior();
        //}

        //private void ProcessLaunchingPhase()
        //{
        //    if (ShouldFinishGroundAttack())
        //    {
        //        _stateMachine.ChangeState<State_AttackFinish>(control.gameObject);
        //    }
        //    else if (ShouldStartAttackPrep())
        //    {
        //        _stateMachine.ChangeState<State_Attack>(control.gameObject);
        //    }
        //}

        //private void ProcessAlternatePhase()
        //{
        //    if (ShouldFinishGroundAttack())
        //    {
        //        _stateMachine.ChangeState<State_AttackFinish>(control.gameObject);
        //    }
        //    else if (control.canUseAbility)
        //    {
        //        _stateMachine.ChangeState<State_Attack>(control.gameObject);
        //    }
        //}

        ///// <summary>
        ///// Checks if ground unit attack should finish (attack animation ending and grounded)
        ///// </summary>
        //private bool ShouldFinishGroundAttack()
        //{
        //    var currentStateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

        //    if (control.CharacterSettings.unitType != UnitType.Ground) { return false; }

        //    if (_stateMachine.CurrentState is State_AttackFinish) { return false; }

        //    if (!control.isAttacking) { return false; }

        //    if (!control.CharacterMovement.IsGrounded) { return false; }

        //    return IsAnimationEnding(currentStateHash, StatesContainer.AttackDictionary);
        //}

        //private bool ShouldStartAttackPrep()
        //{
        //    return control.hasUsedAbility &&
        //           !control.isLanding &&
        //           !control.isAttacking &&
        //           control.canUseAbility;
        //}

        ////public void PlayDeathStateForNonRagdoll()
        ////{
        ////    _stateMachine.ChangeState<State_Death>(control.gameObject);
        ////}

        ///// <summary>
        ///// Processes global animation behavior independent of game phase
        ///// </summary>
        //private void ProcessGlobalBehavior()
        //{
        //    if (!control.canCheckGlobalBehavior || control.isDead) return;

        //    var currentStateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

        //    if (control.unitGotHit)
        //    {
        //        _stateMachine.ChangeState<State_HitReaction>(control.gameObject);
        //        return;
        //    }

        //    if (ShouldEnterLanding(currentStateHash))
        //    {
        //        _stateMachine.ChangeState<State_Landing>(control.gameObject);
        //        return;
        //    }
        //    if (ShouldEnterAirborne(currentStateHash))
        //    {
        //        _stateMachine.ChangeState<State_Airboned>(control.gameObject);
        //        return;
        //    }

        //    if (ShouldReturnToIdle(currentStateHash))
        //    {
        //        _stateMachine.ChangeState<State_Idle>(control.gameObject);
        //    }
        //}

        ///// <summary>
        ///// Determines if character should enter airborne state
        ///// </summary>
        //private bool ShouldEnterAirborne(int currentStateHash)
        //{
        //    if (_stateMachine.CurrentState is State_Airboned)
        //    {
        //        return false;
        //    }

        //    // AIR units: always airborne except when grounded and can attack
        //    if (control.CharacterSettings.unitType == UnitType.Air)
        //    {
        //        if (GameplayCoreManager.Instance.PhaseFlowController.CurrentState != GameState.LaunchPhase) { return false; }

        //        if (control.playerOrAi != PlayerOrAi.Player) { return false; }

        //        if (control.hasUsedAbility) { return false; }

        //        if (control.hasFinishedLaunchingTurn) { return false; }

        //        return true;
        //    }

        //    if (control.CharacterMovement.IsGrounded) { return false; }

        //    if (control.isAttacking) { return false; }

        //    if (control.isLanding) { return false; }

        //    // AirToGround and ground units: airborne when not grounded
        //    return true;
        //}

        ///// <summary>
        ///// Determines if character should enter landing state
        ///// </summary>
        //private bool ShouldEnterLanding(int currentStateHash)
        //{
        //    if (_stateMachine.CurrentState is State_Landing) { return false; }

        //    if (control.unitGotHit) { return false; }

        //    if (!control.CharacterMovement.IsGrounded) { return false; }

        //    if (control.CharacterSettings.unitType == UnitType.Air) return false;

        //    // AirToGround units: enter landing when attack ends and grounded
        //    if (control.CharacterSettings.unitType == UnitType.AirToGround)
        //    {
        //        if (_stateMachine.CurrentState is State_Attack
        //            && IsAnimationEnding(currentStateHash, StatesContainer.AttackDictionary)
        //            )
        //        {
        //            return true;
        //        }
        //    }

        //    if (_stateMachine.CurrentState is State_Airboned)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //private bool ShouldReturnToIdle(int currentStateHash)
        //{
        //    if (_stateMachine.CurrentState is State_Idle)
        //    {
        //        return false;
        //    }

        //    if (control.isAttacking) { return false; }

        //    // AIR units: return to idle when grounded or can't use ability
        //    if (control.CharacterSettings.unitType == UnitType.Air)
        //    {
        //        if (_stateMachine.CurrentState is State_HitReaction)
        //        {
        //            return IsAnimationEnding(currentStateHash, GameplayCoreManager.Instance.StatesContainer.hitReaction_Dictionary);
        //        }

        //        if (control.CharacterMovement.IsGrounded || !control.canUseAbility)
        //        {
        //            return true;
        //        }

        //        return false;
        //    }

        //    // Ground and AirToGround: return to idle when not landing, grounded
        //    if (control.isLanding) { return false; }

        //    if (!control.CharacterMovement.IsGrounded) { return false; }

        //    bool shouldTransitionFromAttackFinish = _stateMachine.CurrentState is State_AttackFinish && IsAnimationEnding(currentStateHash, StatesContainer.AttackFinishDictionary);

        //    bool shouldTransitionFromOtherStates = _stateMachine.CurrentState is State_HitReaction || _stateMachine.CurrentState is State_Landing;

        //    return shouldTransitionFromAttackFinish || shouldTransitionFromOtherStates;
        //}

        ///// <summary>
        ///// Checks if animation is ending based on normalized time
        ///// </summary>
        //private bool IsAnimationEnding<T>(int stateHash, SerializedDictionary<T, int> dict) where T : Enum
        //{
        //    if (dict.ContainsValue(stateHash))
        //    {
        //        return control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public override void OnUpdate() { }

        public override void OnComponentEnable() { }

        public override void OnLateUpdate() { }

        public override void OnAwake() { }
    }
}