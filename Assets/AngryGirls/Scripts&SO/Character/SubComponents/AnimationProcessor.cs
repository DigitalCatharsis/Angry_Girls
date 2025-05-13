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
            control.characterSettings.CheckForNoneValues(control);

            if (control.playerOrAi == PlayerOrAi.Bot)
            {
                control.checkGlobalBehavior = true;
            }

            _animationController = new AnimationController
                (
                control.gameObject,
                control.animator,
                GameLoader.Instance.hashManager,
                GameLoader.Instance.statesContainer
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
                new State_Airborned(control, _animationController),
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
                _stateMachine.ChangeState<State_Airborned>(control.gameObject);
            }
        }

        public override void OnFixedUpdate()
        {
            ProcessPhaseBehavior();
            _stateMachine.Update();
        }

        private void ProcessPhaseBehavior()
        {
            var currentPhase = GameLoader.Instance.gameFlowController.CurrentState;

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
            // Обработка основной атаки
            else if (control.canUseAbility)
            {
                _stateMachine.ChangeState<State_Attack>(control.gameObject);
            }
        }
        private bool ShouldFinishGroundAttack()
        {
            var currentStateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            if (control.characterSettings.unitType != UnitType.Ground) { return false; }

            if (_stateMachine.CurrentState is State_AttackFinish) { return false; }

            if (!control.isAttacking) { return false; }

            if (!control.CharacterMovement.IsGrounded) { return false; }

            return IsAnimationEnding(currentStateHash, GameLoader.Instance.statesContainer.attack_Dictionary);
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
                _stateMachine.ChangeState<State_Airborned>(control.gameObject);
                return;
            }


            if (ShouldReturnToIdle(currentStateHash))
            {
                _stateMachine.ChangeState<State_Idle>(control.gameObject);
            }
        }

        private bool ShouldEnterAirborne(int currentStateHash)
        {
            if (_stateMachine.CurrentState is State_Airborned)
            {
                return false;
            }

            // AIR
            if (control.characterSettings.unitType == UnitType.Air)
            {
                if (GameLoader.Instance.gameFlowController.CurrentState != GameState.LaunchPhase) { return false; }

                if (control.playerOrAi != PlayerOrAi.Character) { return false; }

                if (control.hasUsedAbility) { return false; }

                if (control.hasFinishedLaunchingTurn) { return false; }

                return true;
            }

            if (control.CharacterMovement.IsGrounded) { return false; }

            if (control.isAttacking) { return false; }

            if (control.isLanding) { return false; }

            //if (_stateMachine.CurrentState is State_Attack) { return false; }

            //AirToGround and ground
            return true;
        }

        private bool ShouldEnterLanding(int currentStateHash)
        {
            if (_stateMachine.CurrentState is State_Landing) { return false; }

            if (control.unitGotHit) { return false; }

            if (!control.CharacterMovement.IsGrounded) { return false; }

            if (control.characterSettings.unitType == UnitType.Air) return false;

            if (control.characterSettings.unitType == UnitType.AirToGround)
            {
                if (_stateMachine.CurrentState is State_Attack 
                    && IsAnimationEnding(currentStateHash, GameLoader.Instance.statesContainer.attack_Dictionary)
                    )
                {
                    ColorDebugLog.Log(
                        $"{control.name} shouldEnterLanding return true. Fields: " +
                        $"\nisgrounded = {control.CharacterMovement.IsGrounded}" +
                        $"\ncurrentAnimation: {control.GetCurrentAnimationName()}" +
                        $"\nisAnimationEnding:{IsAnimationEnding(currentStateHash, GameLoader.Instance.statesContainer.attack_Dictionary)}" +
                        $"\n CurrentState: {_stateMachine.CurrentState}" +
                        $"\ncolliding with {control.detectedGroundObject[0].name}"
                        , System.Drawing.KnownColor.Yellow);
                    return true;
                }
            }

            if (_stateMachine.CurrentState is State_Airborned)
            {
                return true;
            }

            return false;
        }

        private bool ShouldReturnToIdle(int currentStateHash)
        {
            // If already in idle
            if (_stateMachine.CurrentState is State_Idle)
            {
                return false;
            }

            //everyone
            if (control.isAttacking) { return false; }

            // Air
            if (control.characterSettings.unitType == UnitType.Air)
            {
                if (_stateMachine.CurrentState is State_HitReaction)
                {
                    return IsAnimationEnding(currentStateHash, GameLoader.Instance.statesContainer.hitReaction_Dictionary);
                }

                if (control.CharacterMovement.IsGrounded || !control.canUseAbility)
                {
                    return true;
                }

                return false;
            }

            //Ground and Air2Ground
            if (control.isLanding) { return false; }

            if (!control.CharacterMovement.IsGrounded) { return false; }

            bool shouldTransitionFromAttackFinish = _stateMachine.CurrentState is State_AttackFinish && IsAnimationEnding(currentStateHash, GameLoader.Instance.statesContainer.attackFinish_Dictionary);

            bool shouldTransitionFromOtherStates = _stateMachine.CurrentState is State_HitReaction || _stateMachine.CurrentState is State_Landing;

            return shouldTransitionFromAttackFinish || shouldTransitionFromOtherStates;
        }

        private bool IsAnimationEnding<T>(int stateHash, SerializedDictionary<T, int> dict) where T : Enum
        {
            if (dict.ContainsValue(stateHash))
            {
                return control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f;
            }
            else
            {
                return false;
                //throw new Exception(dict.ToString() +" does not contain such value as " + stateHash +". Supposed to be " + GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, stateHash));
            }
        }

        public override void OnUpdate() { }

        public override void OnComponentEnable() { }

        public override void OnLateUpdate() { }

        public override void OnAwake() { }
    }
}