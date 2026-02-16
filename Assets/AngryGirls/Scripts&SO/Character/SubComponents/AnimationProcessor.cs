using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AnimationProcessor : SubComponent
    {
        private AnimationStateMachine _stateMachine;
        private AnimationController _animationController;

        [ShowOnly][SerializeField] private string _probablyCurrentState;

        private void Update()
        {
            _probablyCurrentState = _stateMachine.CurrentState.ToString();
        }

        public override void OnStart()
        {
            control.CharacterSettings.CheckForNoneValues(control);

            if (control.playerOrAi == PlayerOrAi.Bot)
            {
                control.checkGlobalBehavior = true;
            }

            _animationController = new AnimationController
                (
                control.gameObject,
                control.animator,
                CoreManager.Instance.HashManager,
                GameplayCoreManager.Instance.StatesContainer
                );

            InitializeStateMachine();
        }

        private void InitializeStateMachine()
        {
            var states = new IAnimationState[]
            {
                new State_Idle(control, _animationController),
                new State_Attack(control, _animationController),
                new State_Attack(control, _animationController),
                new State_AttackFinish(control, _animationController),
                new State_HitReaction(control, _animationController),
                new State_Death(control, _animationController),
                new State_Landing(control, _animationController),
                new State_Airboned(control, _animationController),
            };

            _stateMachine = new AnimationStateMachine(control.gameObject, states);
            InitFirstState();
        }

        public void InitFirstState()
        {
            if (control.CharacterMovement.IsGrounded)
            {
                _stateMachine.ChangeState<State_Idle>(control.gameObject);
            }
            else
            {
                _stateMachine.ChangeState<State_Airboned>(control.gameObject);
            }
        }

        public override void OnFixedUpdate()
        {
            ProcessPhaseBehavior();
            _stateMachine.Update();
        }

        /// <summary>
        /// Processes behavior based on current game phase
        /// </summary>
        private void ProcessPhaseBehavior()
        {
            var currentPhase = GameplayCoreManager.Instance.GameFlowController.CurrentState;

            if (currentPhase == GameState.LaunchPhase && control.hasBeenLaunched && !control.hasFinishedLaunchingTurn)
            {
                ProcessLaunchingPhase();
            }
            else if (currentPhase == GameState.AlternatePhase && !control.hasFinishedAlternateAttackTurn && control.isAttacking)
            {
                ProcessAlternatePhase();
            }

            ProcessGlobalBehavior();
        }

        private void ProcessLaunchingPhase()
        {
            if (ShouldFinishGroundAttack())
            {
                _stateMachine.ChangeState<State_AttackFinish>(control.gameObject);
            }
            else if (ShouldStartAttackPrep())
            {
                _stateMachine.ChangeState<State_Attack>(control.gameObject);
            }
        }

        private void ProcessAlternatePhase()
        {
            if (ShouldFinishGroundAttack())
            {
                _stateMachine.ChangeState<State_AttackFinish>(control.gameObject);
            }
            else if (control.canUseAbility)
            {
                _stateMachine.ChangeState<State_Attack>(control.gameObject);
            }
        }

        /// <summary>
        /// Checks if ground unit attack should finish (attack animation ending and grounded)
        /// </summary>
        private bool ShouldFinishGroundAttack()
        {
            var currentStateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            if (control.CharacterSettings.unitType != UnitType.Ground) { return false; }

            if (_stateMachine.CurrentState is State_AttackFinish) { return false; }

            if (!control.isAttacking) { return false; }

            if (!control.CharacterMovement.IsGrounded) { return false; }

            return IsAnimationEnding(currentStateHash, GameplayCoreManager.Instance.StatesContainer.attack_Dictionary);
        }

        private bool ShouldStartAttackPrep()
        {
            return control.hasUsedAbility &&
                   !control.isLanding &&
                   !control.isAttacking &&
                   control.canUseAbility;
        }

        public void PlayDeathStateForNonRagdoll()
        {
            _stateMachine.ChangeState<State_Death>(control.gameObject);
        }

        /// <summary>
        /// Processes global animation behavior independent of game phase
        /// </summary>
        private void ProcessGlobalBehavior()
        {
            if (!control.checkGlobalBehavior || control.isDead) return;

            var currentStateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            if (control.unitGotHit)
            {
                _stateMachine.ChangeState<State_HitReaction>(control.gameObject);
                return;
            }

            if (ShouldEnterLanding(currentStateHash))
            {
                _stateMachine.ChangeState<State_Landing>(control.gameObject);
                return;
            }
            if (ShouldEnterAirborne(currentStateHash))
            {
                _stateMachine.ChangeState<State_Airboned>(control.gameObject);
                return;
            }

            if (ShouldReturnToIdle(currentStateHash))
            {
                _stateMachine.ChangeState<State_Idle>(control.gameObject);
            }
        }

        /// <summary>
        /// Determines if character should enter airborne state
        /// </summary>
        private bool ShouldEnterAirborne(int currentStateHash)
        {
            if (_stateMachine.CurrentState is State_Airboned)
            {
                return false;
            }

            // AIR units: always airborne except when grounded and can attack
            if (control.CharacterSettings.unitType == UnitType.Air)
            {
                if (GameplayCoreManager.Instance.GameFlowController.CurrentState != GameState.LaunchPhase) { return false; }

                if (control.playerOrAi != PlayerOrAi.Player) { return false; }

                if (control.hasUsedAbility) { return false; }

                if (control.hasFinishedLaunchingTurn) { return false; }

                return true;
            }

            if (control.CharacterMovement.IsGrounded) { return false; }

            if (control.isAttacking) { return false; }

            if (control.isLanding) { return false; }

            // AirToGround and ground units: airborne when not grounded
            return true;
        }

        /// <summary>
        /// Determines if character should enter landing state
        /// </summary>
        private bool ShouldEnterLanding(int currentStateHash)
        {
            if (_stateMachine.CurrentState is State_Landing) { return false; }

            if (control.unitGotHit) { return false; }

            if (!control.CharacterMovement.IsGrounded) { return false; }

            if (control.CharacterSettings.unitType == UnitType.Air) return false;

            // AirToGround units: enter landing when attack ends and grounded
            if (control.CharacterSettings.unitType == UnitType.AirToGround)
            {
                if (_stateMachine.CurrentState is State_Attack
                    && IsAnimationEnding(currentStateHash, GameplayCoreManager.Instance.StatesContainer.attack_Dictionary)
                    )
                {
                    return true;
                }
            }

            if (_stateMachine.CurrentState is State_Airboned)
            {
                return true;
            }

            return false;
        }

        private bool ShouldReturnToIdle(int currentStateHash)
        {
            if (_stateMachine.CurrentState is State_Idle)
            {
                return false;
            }

            if (control.isAttacking) { return false; }

            // AIR units: return to idle when grounded or can't use ability
            if (control.CharacterSettings.unitType == UnitType.Air)
            {
                if (_stateMachine.CurrentState is State_HitReaction)
                {
                    return IsAnimationEnding(currentStateHash, GameplayCoreManager.Instance.StatesContainer.hitReaction_Dictionary);
                }

                if (control.CharacterMovement.IsGrounded || !control.canUseAbility)
                {
                    return true;
                }

                return false;
            }

            // Ground and AirToGround: return to idle when not landing, grounded
            if (control.isLanding) { return false; }

            if (!control.CharacterMovement.IsGrounded) { return false; }

            bool shouldTransitionFromAttackFinish = _stateMachine.CurrentState is State_AttackFinish && IsAnimationEnding(currentStateHash, GameplayCoreManager.Instance.StatesContainer.attackFinish_Dictionary);

            bool shouldTransitionFromOtherStates = _stateMachine.CurrentState is State_HitReaction || _stateMachine.CurrentState is State_Landing;

            return shouldTransitionFromAttackFinish || shouldTransitionFromOtherStates;
        }

        /// <summary>
        /// Checks if animation is ending based on normalized time
        /// </summary>
        private bool IsAnimationEnding<T>(int stateHash, SerializedDictionary<T, int> dict) where T : Enum
        {
            if (dict.ContainsValue(stateHash))
            {
                return control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f;
            }
            else
            {
                return false;
            }
        }

        public override void OnUpdate() { }

        public override void OnComponentEnable() { }

        public override void OnLateUpdate() { }

        public override void OnAwake() { }
    }
}