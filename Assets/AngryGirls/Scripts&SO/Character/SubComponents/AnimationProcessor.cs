using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AnimationProcessor : SubComponent<SubcomponentMediator_EventNames>
    {
        public override void OnComponentEnable()
        {
            //Init start Animation
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            control.currentStateData.hash = stateInfo.shortNameHash;
            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
        }

        public override void OnStart()
        {
            //for debug
            control.characterSettings.CheckForNoneValues(control);

            //have to disable Global behavior for launching units (done by default), untill they are launched, and enable for AI
            if (control.playerOrAi == PlayerOrAi.Ai)
            {
                control.checkGlobalBehavior = true;
            }

            //should start from idle when the game starts
            var idleState = control.characterSettings.idle_States[UnityEngine.Random.Range(0, control.characterSettings.idle_States.Count)];

            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesDispatcher.idle_Dictionary[idleState.animation], transitionDuration: idleState.transitionDuration);
        }

        public override void OnFixedUpdate()
        {
            //On Launching behavior phase
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase
                && control.hasBeenLaunched == true)
            {
                LaunchingPhase_Logic();
            }

            //On Static behavior phase
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.StaticPhase)
            {
                CheckUnit_StaticPhase();
            }

            CheckUnit_GlobalPhase();
        }


        #region Launching behavior methods
        private void LaunchingPhase_Logic()
        {
            if (control.hasFinishedLaunchingTurn)
            {
                return;
            }

            //Check Ground attack (ground Unit Only)
            if (control.characterSettings.unitType == UnitType.Ground)
            {
                Launching_CheckAndProcess_GroundAttack();
            }

            //CheckAttackPrep
            if (control.hasUsedAbility
                && !control.isLanding)
            {
                Launching_CheckAndProcess_AttackPrep();
            }
        }
        private void Launching_CheckAndProcess_GroundAttack()
        {
            //Ground attack
            if (control.isAttacking && control.isGrounded)
            {
                control.animator.StopPlayback();
                ChangeAnimationStateFixedTime(GameLoader.Instance.statesDispatcher.attackFinish_Dictionary[control.characterSettings.attackFininsh_State.animation], transitionDuration: control.characterSettings.attackFininsh_State.transitionDuration);
            }
        }

        private void Launching_CheckAndProcess_AttackPrep()
        {
            //AirToGround unit personal condition
            if (control.characterSettings.unitType == UnitType.AirToGround)
            {
                if (control.isGrounded
                    || control.isAttacking
                    || control.airToGroundUnit_FinishedAbility)
                {
                    return;
                }

                //if (GameLoader.Instance.statesDispatcher.attackPrep_Dictionary.ContainsValue(control.currentStateData.hash))
                if (GameLoader.Instance.statesDispatcher.attackPrep_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
                {
                    return;
                }
            }

            //for ground unit
            if (GameLoader.Instance.statesDispatcher.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return;
            }

            //Everyones logic
            if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
            {
                return;
            }

            control.isAttacking = true;
            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesDispatcher.attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation],control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
            return;
        }
        #endregion

        #region Static behavior methods
        private void CheckUnit_StaticPhase()
        {
            if (!control.hasFinishedStaticAttackTurn
                && control.isAttacking)
            {
                Static_CheckAndProcessAttack();
            }
        }

        private void Static_CheckAndProcessAttack()
        {
            if (GameLoader.Instance.statesDispatcher.staticAttack_States_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return;
            }

            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesDispatcher.staticAttack_States_Dictionary[control.characterSettings.staticAttackAbility.staticAttack_State.animation], transitionDuration: control.characterSettings.staticAttackAbility.attackTimeDuration);
        }
        #endregion

        #region Global Behavior Methods

        private void CheckUnit_GlobalPhase()
        {
            if (control.checkGlobalBehavior == false)
            {
                return;
            }

            if (control.isDead)
            {
                Global_Process_Death();
                return;
            }

            if (control.unitGotHit)
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
                if (Global_CheckAndProcess_Landing() == true)
                {
                    //без return не успеет перейти в State Behavior
                    return;
                }
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
                ChangeAnimationState(GameLoader.Instance.statesDispatcher.death_States_Dictionary[randomDeathAnimation], transitionDuration: 0.1f);
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
            var randomHitAnimation = control.characterSettings.hitReaction_States[UnityEngine.Random.Range(0, control.characterSettings.hitReaction_States.Count)].animation;
            //no crossFade for instant animations changes at fast damage recive
            ChangeAnimationState(GameLoader.Instance.statesDispatcher.hitReaction_Dictionary[randomHitAnimation], transitionDuration: 0.1f);
            control.unitGotHit = false;
            return;
        }

        private bool Global_CheckAndProcess_Landing()
        {
            //no landing phase for air units. The Launch is over
            if (control.characterSettings.unitType == UnitType.Air)
            {
                control.hasFinishedLaunchingTurn = true;
                return false;
            }

            if ((GameLoader.Instance.statesDispatcher.airbonedFlying_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
                && control.isGrounded
                && control.isAttacking == false)
            {
                ChangeAnimationState_CrossFadeInFixedTime(
                    GameLoader.Instance.statesDispatcher.landingNames_Dictionary[control.characterSettings.landing_State.animation],
                    control.characterSettings.landing_State.transitionDuration);

                return true;
            }

            return false;
        }

        private void Global_CheckAndProcess_AirbonedState()
        {
            //Cant be airboned after finished attack for ground and Air unit for now...
            if (!control.unitBehaviorIsStatic && control.hasFinishedLaunchingTurn)
            {
                if (control.characterSettings.unitType == UnitType.Ground || control.characterSettings.unitType == UnitType.Air)
                    return;
            }

            //personal ground unit condition
            if (GameLoader.Instance.statesDispatcher.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return;
            }

            //everyones logic
            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesDispatcher.airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], transitionDuration: control.characterSettings.airbonedFlying_States.transitionDuration);

            //ChangeAnimationStateFixedTime(GameLoader.Instance.statesDispatcher.airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], transitionDuration: control.characterSettings.airbonedFlying_States.transitionDuration);
        }

        private void Global_CheckAndProcess_Idle()
        {
            //Exctra condition for an air unit
            if (control.characterSettings.unitType == UnitType.Air)
            {
                if (control.isGrounded || control.hasFinishedLaunchingTurn)
                {
                    ToIdle();
                }

                return;
            }

            //everyone else
            if (control.isGrounded)
            {
                ToIdle();
            }
        }

        #endregion

        #region Conditions
        public bool IsLauchingAttackStateOver(float attackAnimationRepeatRate)
        {
            bool airAttack = GameLoader.Instance.statesDispatcher.attackPrep_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
            bool groundAttack = GameLoader.Instance.statesDispatcher.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);

            //Пока работает, но расчет normilized time в данном случае неверный.
            if ((airAttack || groundAttack)
                && (control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= attackAnimationRepeatRate))
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Change State
        public void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0)
        {            
            var shortNameHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            if (GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, newStateHash) == StateNames.NONE)
            {
                return;
            }

            if (shortNameHash == newStateHash || control.animator.IsInTransition(layer))
            {
                return;
            }

            //if (control.name == "YBot_Green(Clone)")
            //{
            //    ColorDebugLog.Log(GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, shortNameHash).ToString() 
            //        + " -> " 
            //        + GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, newStateHash).ToString(), KnownColor.Yellow);

            //    ColorDebugLog.Log("ShortNameHash: " + GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, shortNameHash).ToString(), System.Drawing.KnownColor.Yellow);
            //    ColorDebugLog.Log("newStateHash: " + GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, newStateHash).ToString(), System.Drawing.KnownColor.Yellow);
            //    ColorDebugLog.Log("===================================", KnownColor.Window);
            //}

            control.animator.CrossFadeInFixedTime(newStateHash, fixedTransitionDuration: transitionDuration, layer: layer, fixedTimeOffset: 0f, normalizedTransitionTime: 0.0f);

            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, newStateHash);
            control.currentStateData.hash = newStateHash;
        }



        public void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 1f)
        {
            if (control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);

            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, newStateHash);
            control.currentStateData.hash = newStateHash;
        }
        public void ChangeAnimationStateFixedTime(int newStateHash, int layer = 0, float transitionDuration = 0f)
        {
            if (control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == newStateHash)
            {
                return;
            }
            control.animator.PlayInFixedTime(newStateHash, layer, transitionDuration);

            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesDispatcher.stateNames_Dictionary, newStateHash);
            control.currentStateData.hash = newStateHash;
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

        private void ToIdle()
        {
            var test = GameLoader.Instance.statesDispatcher.stateNames_Dictionary;
            var hash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            if (GameLoader.Instance.statesDispatcher.idle_Dictionary.ContainsValue(hash))
            {
                return;
            }
            if ((control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9))
            {
                return;
            }

            var idleState = control.characterSettings.idle_States[UnityEngine.Random.Range(0, control.characterSettings.idle_States.Count)];

            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesDispatcher.idle_Dictionary[idleState.animation], transitionDuration: idleState.transitionDuration);
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