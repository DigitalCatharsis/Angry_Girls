using UnityEngine;

namespace Angry_Girls
{
    public class AnimationProcessor : SubComponent
    {
        private AnimationStateMachine _stateMachine;
        private AnimationController _animationController;

        public override void OnStart()
        {
            control.characterSettings.CheckForNoneValues(control);

            if (control.playerOrAi == PlayerOrAi.Ai)
            {
                control.checkGlobalBehavior = true;
            }

            _animationController = new AnimationController(
                control.animator,
                GameLoader.Instance.hashManager,
                GameLoader.Instance.statesContainer);

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

            _stateMachine = new AnimationStateMachine(states);
            _stateMachine.ChangeState<State_Idle>();
        }


        public override void OnFixedUpdate()
        {
            ProcessPhaseBehavior();
            _stateMachine.Update();
        }

        private void ProcessPhaseBehavior()
        {
            var currentPhase = GameLoader.Instance.turnManager.CurrentPhase;

            if (currentPhase == CurrentPhase.LaunchingPhase && control.hasBeenLaunched && !control.hasFinishedLaunchingTurn)
            {
                ProcessLaunchingPhase();
            }
            else if (currentPhase == CurrentPhase.AlternatePhase && !control.hasFinishedAlternateAttackTurn && control.isAttacking)
            {
                ProcessAlternatePhase();
            }

            ProcessGlobalBehavior();
        }

        private void ProcessLaunchingPhase()
        {
            if (ShouldFinishGroundAttack())
            {
                _stateMachine.ChangeState<State_AttackFinish>();
            }
            else if (ShouldStartAttackPrep())
            {
                _stateMachine.ChangeState<State_Attack>();
            }
        }
        private void ProcessAlternatePhase()
        {
            if (ShouldFinishGroundAttack())
            {
                _stateMachine.ChangeState<State_AttackFinish>();
            }
            // Обработка основной атаки
            else if (control.canUseAbility)
            {
                _stateMachine.ChangeState<State_Attack>();
            }
        }
        private bool ShouldFinishGroundAttack()
        {
            return control.characterSettings.unitType == UnitType.Ground &&
                   control.isAttacking &&
                   control.isGrounded &&
                   (GameLoader.Instance.statesContainer.attack_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash) ||
                   GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash));
        }

        private bool ShouldStartAttackPrep()
        {
            return control.hasUsedAbility &&
                   !control.isLanding &&
                   !control.isAttacking &&
                   control.canUseAbility;
        }

        public void SetDeath()
        {
            if (control.characterSettings.deathByAnimation)
            {
                _stateMachine.ChangeState<State_Death>();
                control.FinishTurn(0);
            }
        }

        private void ProcessGlobalBehavior()
        {
            if (!control.checkGlobalBehavior || control.isDead) return;

            var currentStateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var states = GameLoader.Instance.statesContainer;

            if (control.unitGotHit)
            {
                _stateMachine.ChangeState<State_HitReaction>();
                return;
            }

            if (ShouldEnterAirborne(currentStateHash, states))
            {
                _stateMachine.ChangeState<State_Airborned>();
                return;
            }

            if (ShouldEnterLanding(currentStateHash, states))
            {
                _stateMachine.ChangeState<State_Landing>();
                return;
            }

            if (ShouldReturnToIdle(currentStateHash, states))
            {
                _stateMachine.ChangeState<State_Idle>();
            }
        }

        private bool ShouldEnterAirborne(int currentStateHash, StatesContainer states)
        {
            // AIR
            if (control.characterSettings.unitType == UnitType.Air)
            {
                return GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase &&
                       control.playerOrAi == PlayerOrAi.Player &&
                       !control.hasUsedAbility &&
                       !control.hasFinishedLaunchingTurn;
            }

            //AirToGround
            return !control.isGrounded &&
                   !control.isAttacking &&
                   !control.isLanding &&
                   !states.attackFinish_Dictionary.ContainsValue(currentStateHash);
        }

        private bool ShouldEnterLanding(int currentStateHash, StatesContainer states)
        {
            if (control.characterSettings.unitType == UnitType.Air) return false;

            return control.isGrounded &&
                   !control.unitGotHit &&
                   (states.airbonedFlying_Dictionary.ContainsValue(currentStateHash) ||
                    (control.characterSettings.unitType == UnitType.AirToGround &&
                     states.attack_Dictionary.ContainsValue(currentStateHash) &&
                     IsAnimationEnding(currentStateHash)));
        }

        private bool ShouldReturnToIdle(int currentStateHash, StatesContainer states)
        {
            // If already in idle
            if (states.idle_Dictionary.ContainsValue(currentStateHash))
            {
                if (_stateMachine.CurrentState is not State_Idle)
                {
                    //Bugfix after restart level
                    _stateMachine.ChangeState<State_Idle>();
                }

                return false;
            }

            // Air
            if (control.characterSettings.unitType == UnitType.Air)
            {
                return (control.isGrounded || !control.canUseAbility) &&
                       !control.isAttacking &&
                       (states.hitReaction_Dictionary.ContainsValue(currentStateHash) ||
                        IsAnimationEnding(currentStateHash));
            }

            // Air2Ground
            bool shouldTransitionFromAttackFinish = states.attackFinish_Dictionary.ContainsValue(currentStateHash) &&
                                                  IsAnimationEnding(currentStateHash);

            bool shouldTransitionFromOtherStates = states.hitReaction_Dictionary.ContainsValue(currentStateHash) ||
                                                  states.landingNames_Dictionary.ContainsValue(currentStateHash);

            return control.isGrounded &&
                   !control.isAttacking &&
                   !control.isLanding &&
                   (shouldTransitionFromAttackFinish || shouldTransitionFromOtherStates);
        }

        private bool IsAnimationEnding(int stateHash)
        {
            var stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.shortNameHash == stateHash && stateInfo.normalizedTime >= 0.95f;
        }

        public override void OnUpdate() { }

        public override void OnComponentEnable() { }

        public override void OnLateUpdate() { }

        public override void OnAwake() { }
    }
}