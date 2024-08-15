using AYellowpaper.SerializedCollections;
using System;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace Angry_Girls
{
    #region enums
    public enum StateNames
    {
        NONE,
        A_AirbonedRolling,
        A_AirbonedRolling_Landing,
        A_Axe_Idle,
        A_Fall_Landing,
        A_Falling_Idle,
        A_Floating,
        A_HeadSpin_Attack,
        A_HitReaction,
        A_Idle,
        A_Idle_HeadSpin,
        A_ShootArrow,
        A_Shoryuken_DownSmash_Finish,
        A_Shoryuken_DownSmash_Prep,
        A_Shoryuken_Landing_Static,
        A_Shoryuken_Rise_Static,
        A_Shoryuken_Prep_Static,
        A_Sweep_Fall,
    }
    public enum Idle_States
    {
        NONE,
        A_Idle,
        A_Idle_HeadSpin,
        A_Floating,
        A_Axe_Idle,
    }
    public enum AirbonedFlying_States
    {
        NONE,
        A_Falling_Idle,
        A_AirbonedRolling,
    }
    public enum AttackPrep_States
    {
        NONE,
        A_Shoryuken_DownSmash_Prep,
        A_ShootArrow,
        A_HeadSpin_Attack,
    }
    public enum StaticAttack_States
    {
        NONE,
        A_Shoryuken_Prep_Static,
        A_Shoryuken_Landing_Static,
        A_Shoryuken_Rise_Static,
    }
    public enum AttackFinish_States
    {
        NONE,
        A_Shoryuken_DownSmash_Finish,
    }
    public enum Landing_States
    {
        NONE,
        A_Fall_Landing,
        A_AirbonedRolling_Landing,
        A_Idle_HeadSpin,
    }

    public enum HitReaction_States
    {
        NONE,
        A_HitReaction,
    }

    public enum Death_States
    {
        NONE,
        A_Sweep_Fall,
    }
    #endregion
    public class AnimationProcessor : SubComponent
    {
        public bool airToGroundUnit_FinishedAbility = false;
        public bool checkGlobalBehavior = false;
        public bool unitBehaviorIsStatic = true;
        public CurrentStateData currentStateData = new();
        //[SerializeField] private bool _isAttackStateOver = false;
        //[SerializeField] private bool _isLandingStateOver = false;

        public SerializedDictionary<AttackPrep_States, int> attackPrep_Dictionary;
        public SerializedDictionary<AttackFinish_States, int> attackFinish_Dictionary;
        public SerializedDictionary<StaticAttack_States, int> staticAttack_States_Dictionary;
        [SerializeField] private SerializedDictionary<StateNames, int> _stateNames_Dictionary;
        [SerializeField] private SerializedDictionary<Idle_States, int> _idle_Dictionary;
        [SerializeField] private SerializedDictionary<AirbonedFlying_States, int> _airbonedFlying_Dictionary;
        [SerializeField] private SerializedDictionary<Landing_States, int> _landingNames_Dictionary;


        //[SerializeField] private SerializedDictionary<HitReaction_States, int> _hitReaction_Dictionary;
        //[SerializeField] private SerializedDictionary<Death_States, int> _death_States_Dictionary;


        public override void OnComponentEnable()
        {
            //Its me, lol
            control.subComponentProcessor.animationProcessor = this;

            //Init Dictionaries
            _stateNames_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<StateNames>(this.gameObject);
            _idle_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<Idle_States>(this.gameObject);
            _airbonedFlying_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AirbonedFlying_States>(this.gameObject);
            attackPrep_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AttackPrep_States>(this.gameObject);
            attackFinish_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AttackFinish_States>(this.gameObject);
            _landingNames_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<Landing_States>(this.gameObject);
            staticAttack_States_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<StaticAttack_States>(this.gameObject);

            //_hitReaction_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<HitReaction_States>(this.gameObject);
            //_death_States_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<Death_States>(this.gameObject);

            //Init start Animation
            UpdateCurrentStateData_Value();
        }

        private void UpdateCurrentStateData_Value()
        {
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            currentStateData.hash = stateInfo.shortNameHash;
            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, currentStateData.hash);
        }

        public override void OnStart()
        {
            //for debug
            control.characterSettings.CheckForNoneValues(control);

            //have to disable Global behavior for launching units (done by default), untill they are launched, and enable for AI
            if (control.playerOrAi == PlayerOrAi.Ai)
            {
                checkGlobalBehavior = true;
            }

            //should start from idle when the game starts
            ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
        }

        public override void OnFixedUpdate()
        {
            //On Launching behavior phase
            if (Singleton.Instance.turnManager.currentPhase == CurrentPhase.LaunchingPhase
                //&& control.playerOrAi == PlayerOrAi.Player
                && control.subComponentProcessor.launchLogic.hasBeenLaunched == true)
            {
                LaunchingPhase_Logic();
            }

            //On Static behavior phase
            if (Singleton.Instance.turnManager.currentPhase == CurrentPhase.StaticPhase)
            {
                CheckUnit_StaticPhase();
            }

            CheckUnit_GlobalPhase();
        }

        private void CheckUnit_GlobalPhase()
        {
            if (checkGlobalBehavior == false)
            {
                return;
            }

            //Check just Airboned
            if (!control.isGrounded && !control.isAttacking)
            {
                Global_CheckAndProcess_AirbonedState();
            }

            //Check Landing
            if (control.isGrounded && !control.isAttacking)
            {
                Global_CheckAndProcess_Landing();
            }

            if (!control.isAttacking
                && !control.isLanding)
            {
                Global_CheckAndProcess_Idle();
            }
        }

        private bool UnitHaveFinishedTurn()
        {
            if (control.subComponentProcessor.animationProcessor.unitBehaviorIsStatic)
            {
                if (control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else  //LaunchingCharacter. Will be statick after turn finish
            {
                if (control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #region Launching behavior methods
        private void LaunchingPhase_Logic()
        {
            ////Check Idle
            //if (control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
            //{
            //    Launching_CheckAndProcess_Idle();
            //}

            //Check Ground attack (ground Unit Only)
            if (control.characterSettings.unitType == UnitType.Ground)
            {
                Launching_CheckAndProcess_GroundAttack();
            }

            //CheckAttackPrep
            if (control.subComponentProcessor.launchLogic.hasUsedAbility)
            {
                Launching_CheckAndProcess_AttackPrep();
            }

            ////Check just Airboned
            //if (!control.isGrounded && !control.isAttacking)
            //{
            //    Launching_CheckAndProcess_AirbonedState();
            //}

            ////Check Landing
            //if (control.isGrounded && !control.isAttacking)
            //{
            //    Launching_CheckAndProcess_Landing();
            //}
        }

        private void Global_CheckAndProcess_Landing()
        {
            //no landing phase for air units. The Launch is over
            if (control.characterSettings.unitType == UnitType.Air)
            {
                control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn = true;
                return;
            }

            if (IsLandingCondition())
            {
                //Этот Landing вызывает баг при попытке атаки сразу же во время запуска, так как отрабатывается раньше Attack == true.
                //Это не проблема, потому что в будущем сделаю минимальную длинну оттягивания для запуска, и проблема ммоентального Landing должна решиться
                control.isLanding = true; // эта срань не успеет отработать, если запихать в OnStateEnter
                ChangeAnimationState_CrossFadeInFixedTime(_landingNames_Dictionary[control.characterSettings.landing_State.animation], control.characterSettings.landing_State.transitionDuration);
            }
        }

        private void Global_CheckAndProcess_AirbonedState()
        {
            //Cant be airboned after finished attack for ground and Air unit for now...
            if (control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
            {
                if (control.characterSettings.unitType == UnitType.Ground || control.characterSettings.unitType == UnitType.Air)
                    return;
            }

            //everyones logic
            if (attackFinish_Dictionary.ContainsValue(currentStateData.hash))
            {
                return;
            }

            ChangeAnimationState_CrossFadeInFixedTime(_airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
        }

        private void Global_CheckAndProcess_Idle()
        {
            //Exctra condition for an air unit
            if (control.characterSettings.unitType == UnitType.Air)
            {
                if (control.isGrounded || control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
                {
                    ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                }

                return;
            }

            //everyone else
            if (control.isGrounded)
            {
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
            }
        }

        private bool Launching_CheckAndProcess_GroundAttack()
        {
            //Ground attack
            if (control.isAttacking
                && control.isGrounded
                && !control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
            {
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(attackFinish_Dictionary[control.characterSettings.attackFininsh_State.animation], transitionDuration: control.characterSettings.attackFininsh_State.transitionDuration);
                return true;
            }
            return false;
        }

        private void Launching_CheckAndProcess_AttackPrep()
        {
            //Airboned + Attack  (AttackPrep)
            if (!control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
            {
                //AirToGround unit personal condition
                if (control.characterSettings.unitType == UnitType.AirToGround)
                {
                    if (control.isGrounded
                        || control.isAttacking
                        || airToGroundUnit_FinishedAbility)
                    {
                        return;
                    }

                    if (attackPrep_Dictionary.ContainsValue(currentStateData.hash))
                    {
                        return;
                    }
                }

                //for ground unit
                if (attackFinish_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return;
                }

                //Everyones logic
                if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
                {
                    return;
                }

                control.isAttacking = true;
                ChangeAnimationState_CrossFade(attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation], control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
                return;
            }

            return;
        }
        #endregion

        #region Static behavior methods
        private void CheckUnit_StaticPhase()
        {
            if (!control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn
                && control.isAttacking)
            {
                Static_CheckAndProcessAttack();
            }
        }

        private void Static_CheckAndProcessAttack()
        {
            if (attackFinish_Dictionary.ContainsValue(currentStateData.hash))
            {
                return;
            }

            ChangeAnimationState_CrossFadeInFixedTime(staticAttack_States_Dictionary[control.characterSettings.staticAttackAbility.staticAttack_State.animation], transitionDuration: control.characterSettings.staticAttackAbility.attackTimeDuration);
        }
        #endregion

        #region Conditions
        //private bool IsLandingCondition()
        //{
        //    if ((_airbonedFlying_Dictionary.ContainsValue(currentStateData.hash) || attackPrep_Dictionary.ContainsValue(currentStateData.hash))
        //        && control.isGrounded)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        private bool IsLandingCondition()
        {
            if ((_airbonedFlying_Dictionary.ContainsValue(currentStateData.hash))
                && control.isGrounded)
            {
                return true;
            }
            return false;
        }

        public bool IsLauchingAttackStateOver(float attackAnimationRepeatRate)
        {
            bool airAttack = attackPrep_Dictionary.ContainsValue(currentStateData.hash);
            bool groundAttack = attackFinish_Dictionary.ContainsValue(currentStateData.hash);

            //Пока работает, но расчет normilized time в данном случае неверный.
            if ((airAttack || groundAttack)
                && (control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= attackAnimationRepeatRate))
            {
                return false;
            }
            return true;
        }
        public bool IsStaticAttackStateOver(float attackAnimationRepeatRate)
        {
            bool StaticAttack = staticAttack_States_Dictionary.ContainsValue(currentStateData.hash);

            //Пока работает, но расчет normilized time в данном случае неверный.
            if ((StaticAttack)
                && (control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= attackAnimationRepeatRate))
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Change State

        private void ChangeAnimationState_CrossFade(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.CrossFade(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }

        public void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, newStateHash) == StateNames.NONE)
            {
                return;
            }

            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.CrossFadeInFixedTime(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }

        public void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 1f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }

        #endregion

        public void SetRotation()
        {
            if (control.rigidBody.velocity.z > 0f)
            {
                control.transform.rotation = Quaternion.Euler(0, 0, 0);
                //return true;
            }

            if (control.rigidBody.velocity.z < 0f)
            {
                control.transform.rotation = Quaternion.Euler(0, 180, 0);
                //return false;
            }

            //return false;
        }

        public override void OnAwake()
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnLateUpdate()
        {
        }
    }

    [Serializable]
    public class CurrentStateData
    {
        public StateNames currentStateName;
        public int hash;
    }
}