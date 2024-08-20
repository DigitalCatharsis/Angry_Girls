using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{

    public class AnimationProcessor : SubComponent
    {
        [Header("Debug")]
        [Header("Conditions")]
        public bool airToGroundUnit_FinishedAbility = false;
        public bool checkGlobalBehavior = false;
        public bool unitBehaviorIsStatic = true;
        public bool unitGotHit = false;

        [Header("State Data")]
        public CurrentStateData currentStateData = new();
        public override void OnComponentEnable()
        {
            //Its me, lol
            control.subComponentProcessor.animationProcessor = this;

            //Init start Animation
            UpdateCurrentStateData_Value();
        }

        private void UpdateCurrentStateData_Value()
        {
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            currentStateData.hash = stateInfo.shortNameHash;
            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(Singleton.Instance.statesDispatcher.stateNames_Dictionary, currentStateData.hash);
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
            ChangeAnimationState_CrossFadeInFixedTime(Singleton.Instance.statesDispatcher.idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
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


        #region Launching behavior methods
        private void LaunchingPhase_Logic()
        {
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
        }
        private bool Launching_CheckAndProcess_GroundAttack()
        {
            //Ground attack
            if (control.isAttacking
                && control.isGrounded
                && !control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
            {
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(Singleton.Instance.statesDispatcher.attackFinish_Dictionary[control.characterSettings.attackFininsh_State.animation], transitionDuration: control.characterSettings.attackFininsh_State.transitionDuration);
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

                    if (Singleton.Instance.statesDispatcher.attackPrep_Dictionary.ContainsValue(currentStateData.hash))
                    {
                        return;
                    }
                }

                //for ground unit
                if (Singleton.Instance.statesDispatcher.attackFinish_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return;
                }

                //Everyones logic
                if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
                {
                    return;
                }

                control.isAttacking = true;
                ChangeAnimationState_CrossFade(Singleton.Instance.statesDispatcher.attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation], control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
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
            if (Singleton.Instance.statesDispatcher.staticAttack_States_Dictionary.ContainsValue(currentStateData.hash))
            {
                return;
            }

            ChangeAnimationState_CrossFadeInFixedTime(Singleton.Instance.statesDispatcher.staticAttack_States_Dictionary[control.characterSettings.staticAttackAbility.staticAttack_State.animation], transitionDuration: control.characterSettings.staticAttackAbility.attackTimeDuration);
        }
        #endregion

        #region Global Behavior Methods

        private void CheckUnit_GlobalPhase()
        {
            if (checkGlobalBehavior == false)
            {
                return;
            }

            if (control.isDead)
            {
                Global_Process_Death();
                return;
            }

            if (unitGotHit)
            {
                Global_CheckAndProcess_HitReaction();
                return;
            }

            //Check just Airboned
            if (!control.isGrounded && !control.isAttacking && !control.isLanding)
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

        private void Global_Process_Death()
        {
            if (control.characterSettings.deathByAnimation == true)
            {
                var randomDeathAnimation = control.characterSettings.death_States[UnityEngine.Random.Range(0, control.characterSettings.death_States.Count)].animation; //0 = none            
                                                                                                                                                                        //no crossFade for instant animations changes at fast damage recive
                ChangeAnimationState(Singleton.Instance.statesDispatcher.death_States_Dictionary[randomDeathAnimation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                return;
            }
            else
            {
                //TODO: Implement
                //TriggerRagroll();
            }            
        }

        private void TriggerRagroll()
        {
            //change components layers from character to DeadBody to prevent unnessesary collisions.
            var bodypartsTransforms_Array = control.gameObject.GetComponentsInChildren<Transform>();
            foreach (var transform in bodypartsTransforms_Array)
            {
                transform.gameObject.layer = LayerMask.NameToLayer("DeadBody");
            }

            //turn off animator, avatar
            control.animator.enabled = false;
            control.animator.avatar = null;
        }

        private void Global_CheckAndProcess_HitReaction()
        {
            ////everyones logic
            //if (_hitReaction_Dictionary.ContainsValue(currentStateData.hash))
            //{
            //    if (control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= testFloat)
            //    {
            //        return;
            //    }
            //}


            var randomHitAnimation = control.characterSettings.hitReaction_States[UnityEngine.Random.Range(0, control.characterSettings.hitReaction_States.Count)].animation; //0 = none            
            //no crossFade for instant animations changes at fast damage recive
            ChangeAnimationState(Singleton.Instance.statesDispatcher.hitReaction_Dictionary[randomHitAnimation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
            unitGotHit = false;
            return;
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
                ChangeAnimationState_CrossFadeInFixedTime(Singleton.Instance.statesDispatcher.landingNames_Dictionary[control.characterSettings.landing_State.animation], control.characterSettings.landing_State.transitionDuration);
            }
        }

        private void Global_CheckAndProcess_AirbonedState()
        {
            //Cant be airboned after finished attack for ground and Air unit for now...
            if (!unitBehaviorIsStatic && control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
            {
                if (control.characterSettings.unitType == UnitType.Ground || control.characterSettings.unitType == UnitType.Air)
                    return;
            }

            //everyones logic
            if (Singleton.Instance.statesDispatcher.attackFinish_Dictionary.ContainsValue(currentStateData.hash))
            {
                return;
            }

            ChangeAnimationState_CrossFadeInFixedTime(Singleton.Instance.statesDispatcher.airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
        }

        private void Global_CheckAndProcess_Idle()
        {
            //Exctra condition for an air unit
            if (control.characterSettings.unitType == UnitType.Air)
            {
                if (control.isGrounded || control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn)
                {
                    ChangeAnimationState_CrossFadeInFixedTime(Singleton.Instance.statesDispatcher.idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                }

                return;
            }

            //everyone else
            if (control.isGrounded)
            {
                ChangeAnimationState_CrossFadeInFixedTime(Singleton.Instance.statesDispatcher.idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
            }
        }

        #endregion

        #region Conditions
        private bool IsLandingCondition()
        {
            if ((Singleton.Instance.statesDispatcher.airbonedFlying_Dictionary.ContainsValue(currentStateData.hash))
                && control.isGrounded)
            {
                return true;
            }
            return false;
        }

        public bool IsLauchingAttackStateOver(float attackAnimationRepeatRate)
        {
            bool airAttack = Singleton.Instance.statesDispatcher.attackPrep_Dictionary.ContainsValue(currentStateData.hash);
            bool groundAttack = Singleton.Instance.statesDispatcher.attackFinish_Dictionary.ContainsValue(currentStateData.hash);

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
            bool StaticAttack = Singleton.Instance.statesDispatcher.staticAttack_States_Dictionary.ContainsValue(currentStateData.hash);

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

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(Singleton.Instance.statesDispatcher.stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }

        public void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (Singleton.Instance.hashManager.GetName(Singleton.Instance.statesDispatcher.stateNames_Dictionary, newStateHash) == StateNames.NONE)
            {
                return;
            }

            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.CrossFadeInFixedTime(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(Singleton.Instance.statesDispatcher.stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }

        public void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 1f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(Singleton.Instance.statesDispatcher.stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }

        #endregion

        //TODO: implement. Its buged af
        public void SetRotation()
        {
            if (control.rigidBody.velocity.z > 0.00001f)
            {
                control.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            if (control.rigidBody.velocity.z < 0.00001f)
            {
                control.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
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