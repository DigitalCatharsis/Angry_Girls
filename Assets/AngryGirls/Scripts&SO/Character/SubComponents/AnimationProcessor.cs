using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
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

            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.idle_Dictionary[idleState.animation], transitionDuration: idleState.transitionDuration);
        }

        public override void OnFixedUpdate()
        {
            //On Launching behavior phase
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase
                && control.hasBeenLaunched == true)
            {
                LaunchingPhase_Logic();
            }

            //On Alternate behavior phase
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.AlternatePhase)
            {
                CheckUnit_AlternatePhase();
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
                CheckAndProcess_GroundAttackFinish();
            }

            //CheckAttackPrep
            if (control.hasUsedAbility
                && !control.isLanding)
            {
                Launching_CheckAndProcess_AttackPrep();
            }
        }
        private void CheckAndProcess_GroundAttackFinish()
        {
            //Ground attack
            if (control.isAttacking && control.isGrounded)
            {
                control.animator.StopPlayback();
                ChangeAnimationStateFixedTime(GameLoader.Instance.statesContainer.attackFinish_Dictionary[control.Get_AttackAbility().attackFininsh_State.animation], transitionDuration: control.Get_AttackAbility().attackFininsh_State.transitionDuration);
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
                if (GameLoader.Instance.statesContainer.attack_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
                {
                    return;
                }
            }

            //for ground unit
            if (GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return;
            }

            //Everyones logic
            if (IsLauchingAttackStateOver(control.characterSettings.AttackAbility_Launch.timesToRepeat_Attack_State) == false)
            {
                return;
            }

            control.isAttacking = true;
            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.attack_Dictionary[control.characterSettings.AttackAbility_Launch.attack_State.animation], control.characterSettings.AttackAbility_Launch.attack_State.transitionDuration);
            return;
        }
        #endregion

        #region Alternate behavior methods
        private void CheckUnit_AlternatePhase()
        {
            if (!control.hasFinishedAlternateAttackTurn
                && control.isAttacking)
            {
                //Check Ground attack (ground Unit Only)
                if (control.characterSettings.unitType == UnitType.Ground
                    && GameLoader.Instance.statesContainer.attack_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
                {
                    CheckAndProcess_GroundAttackFinish();
                    return;
                }

                Alternate_CheckAndProcessAttack();
            }
        }

        private void Alternate_CheckAndProcessAttack()
        {
            if (GameLoader.Instance.statesContainer.attack_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash) 
                || GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return;
            }

            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.attack_Dictionary[control.characterSettings.AttackAbility_Alternate.attack_State.animation], transitionDuration: control.characterSettings.AttackAbility_Alternate.attackTimeDuration);
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

            //if idle turn to enemy
            if (GameLoader.Instance.statesContainer.idle_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                if (control.hasFinishedLaunchingTurn)
                {

                    if (control.playerOrAi == PlayerOrAi.Player)
                    {
                        TurnToTheClosestEnemy(PlayerOrAi.Ai);
                    }
                    else
                    {
                        TurnToTheClosestEnemy(PlayerOrAi.Player);
                    }
                }
            }

            //Check just Airboned
            if (!control.isGrounded && !control.isAttacking && !control.isLanding)
            {
                Global_CheckAndProcess_AirbonedState();
            }

            //Check Landing
            if (control.isGrounded)
            {
                if (control.characterSettings.unitType != UnitType.AirToGround)
                {
                    if (control.isAttacking)
                    {
                        return;
                    }
                }

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
                ChangeAnimationState(GameLoader.Instance.statesContainer.death_States_Dictionary[randomDeathAnimation], transitionDuration: 0.1f);
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
            ChangeAnimationState(GameLoader.Instance.statesContainer.hitReaction_Dictionary[randomHitAnimation], transitionDuration: 0.1f);
            return;
        }

        private bool Global_CheckAndProcess_Landing()
        {
            if (control.unitGotHit)
            {
                return false;
            }

            //no landing phase for air units. The Launch is over
            if (control.characterSettings.unitType == UnitType.Air)
            {
                control.FinishTurn();
                //control.hasFinishedLaunchingTurn = true;
                return false;
            }

            if (GameLoader.Instance.statesContainer.landingNames_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash)
                || GameLoader.Instance.statesContainer.idle_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash)
                || GameLoader.Instance.statesContainer.hitReaction_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return false;
            }

            //no landing phase for air units. The Launch is over
            if (control.characterSettings.unitType == UnitType.AirToGround
                && (!control.isAttacking)
                && control.isGrounded
                )

            {
                ChangeAnimationState_CrossFadeInFixedTime(
                    GameLoader.Instance.statesContainer.landingNames_Dictionary[control.characterSettings.landing_State.animation],
                    control.characterSettings.landing_State.transitionDuration);

                return true;
            }

            if (control.characterSettings.unitType != UnitType.AirToGround
                && (GameLoader.Instance.statesContainer.airbonedFlying_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
                && control.isGrounded)
            {
                ChangeAnimationState_CrossFadeInFixedTime(
                    GameLoader.Instance.statesContainer.landingNames_Dictionary[control.characterSettings.landing_State.animation],
                    control.characterSettings.landing_State.transitionDuration);

                return true;
            }

            return false;
        }

        private void Global_CheckAndProcess_AirbonedState()
        {
            //Cant be airboned after finished attack for ground and Air unit for now...
            if (!control.unitBehaviorIsAlternate && control.hasFinishedLaunchingTurn)
            {
                if (/*control.characterSettings.unitType == UnitType.Ground ||*/ control.characterSettings.unitType == UnitType.Air)
                    return;
            }

            //personal ground unit condition
            if (GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
            {
                return;
            }

            //everyones logic
            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], transitionDuration: control.characterSettings.airbonedFlying_States.transitionDuration);

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
            bool airAttack = GameLoader.Instance.statesContainer.attack_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
            bool groundAttack = GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);

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

            if (GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, newStateHash) == StateNames.NONE)
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

            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, newStateHash);
            control.currentStateData.hash = newStateHash;
        }



        public void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 1f)
        {
            if (control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);

            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, newStateHash);
            control.currentStateData.hash = newStateHash;
        }
        public void ChangeAnimationStateFixedTime(int newStateHash, int layer = 0, float transitionDuration = 0f)
        {
            if (control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == newStateHash)
            {
                return;
            }
            control.animator.PlayInFixedTime(newStateHash, layer, transitionDuration);

            control.currentStateData.currentStateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, newStateHash);
            control.currentStateData.hash = newStateHash;
        }

        #endregion


        private void TurnToTheClosestEnemy(PlayerOrAi playerOrAi)
        {
            float closestDistance = 9999f;
            var collection = new List<GameObject>();

            if (playerOrAi == PlayerOrAi.Player)
            {
                collection = GameLoader.Instance.characterManager.playableCharacters;
            }
            else
            {
                collection = GameLoader.Instance.characterManager.enemyCharacters;
            }

            foreach (var character in collection)
            {
                if (character.GetComponent<CControl>().isDead) continue;

                var distacne = transform.position.z - character.transform.position.z;

                if (Math.Abs(closestDistance) > Math.Abs(distacne))
                {
                    closestDistance = distacne;
                }
            }

            if (closestDistance > 0)
            {
                control.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (closestDistance < 0)
            {
                control.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        private void ToIdle()
        {
            var test = GameLoader.Instance.statesContainer.stateNames_Dictionary;
            var hash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            if (GameLoader.Instance.statesContainer.idle_Dictionary.ContainsValue(hash))
            {
                return;
            }
            if ((control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9))
            {
                return;
            }

            var idleState = control.characterSettings.idle_States[UnityEngine.Random.Range(0, control.characterSettings.idle_States.Count)];

            ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.idle_Dictionary[idleState.animation], transitionDuration: idleState.transitionDuration);
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