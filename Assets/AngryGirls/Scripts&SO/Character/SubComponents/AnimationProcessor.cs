using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Angry_Girls
{
    public class AnimationProcessor : SubComponent<UnitLaunch_EventNames>
    {
        public override void OnComponentEnable()
        {
            //Init start Animation
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
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

            ////Everyones logic
            //if (IsLauchingAttackStateOver(control.characterSettings.AttackAbility_Launch.timesToRepeat_Attack_State) == false)
            //{
            //    return;
            //}

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
            var hash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            if (control.checkGlobalBehavior == false)
            {
                return;
            }

            //DEAD
            if (control.isDead)
            {
                if (control.characterSettings.deathByAnimation == true)
                {
                    var randomDeathAnimation = control.characterSettings.death_States[UnityEngine.Random.Range(0, control.characterSettings.death_States.Count)].animation; //0 = none            
                                                                                                                                                                            //no crossFade for instant animations changes at fast damage recive
                    ChangeAnimationState(GameLoader.Instance.statesContainer.death_States_Dictionary[randomDeathAnimation], transitionDuration: 0.1f);
                }
                else
                {
                    //TODO: Implement
                    //TriggerRagroll();
                }
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

            //GOT HIT
            if (control.unitGotHit)
            {
                var randomHitAnimation = control.characterSettings.hitReaction_States[UnityEngine.Random.Range(0, control.characterSettings.hitReaction_States.Count)].animation;
                ChangeAnimationState(GameLoader.Instance.statesContainer.hitReaction_Dictionary[randomHitAnimation], transitionDuration: 0.1f);

                if (control.isAttacking)
                {
                    control.FinishTurn();
                }
                return;
            }

            //CHECK AIRBONED
            if (!control.isGrounded
                && !control.isAttacking
                && !control.isLanding)
            {
                //Cant be airboned after finished attack for ground and Air unit for now...
                if (!control.unitBehaviorIsAlternate && control.hasFinishedLaunchingTurn)
                {
                    if (control.characterSettings.unitType == UnitType.Air)
                        return;
                }

                //personal ground unit condition
                if (GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(hash))
                {
                    return;
                }

                //everyones logic
                ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], transitionDuration: control.characterSettings.airbonedFlying_States.transitionDuration);
            }

            //CHECK LANDING
            if (control.isGrounded
                && !control.unitGotHit
                && control.characterSettings.unitType != UnitType.Air
                && !GameLoader.Instance.statesContainer.landingNames_Dictionary.ContainsValue(hash)
                && !GameLoader.Instance.statesContainer.idle_Dictionary.ContainsValue(hash))
            {
                //airToGround unit condition
                if (control.characterSettings.unitType == UnitType.AirToGround)
                {
                    ChangeAnimationState_CrossFadeInFixedTime(
                        GameLoader.Instance.statesContainer.landingNames_Dictionary[control.characterSettings.landing_State.animation],
                        control.characterSettings.landing_State.transitionDuration);
                    return;
                }

                //Ground unit condition
                if (GameLoader.Instance.statesContainer.airbonedFlying_Dictionary.ContainsValue(hash))
                {
                    ChangeAnimationState_CrossFadeInFixedTime(
                        GameLoader.Instance.statesContainer.landingNames_Dictionary[control.characterSettings.landing_State.animation],
                        control.characterSettings.landing_State.transitionDuration);

                    return ;
                }
            }

            //CHECK IDLE
            if (control.isGrounded && !control.isAttacking && !control.isLanding)
            {
                if (GameLoader.Instance.statesContainer.idle_Dictionary.ContainsValue(hash))
                {
                    return;
                }

                //Exctra condition for an air unit
                if (control.characterSettings.unitType == UnitType.Air)
                {
                    if (control.isGrounded || control.hasFinishedLaunchingTurn)
                    {
                        var idleState = control.characterSettings.idle_States[UnityEngine.Random.Range(0, control.characterSettings.idle_States.Count)];
                        ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.idle_Dictionary[idleState.animation], transitionDuration: idleState.transitionDuration);
                        return;
                    }
                }

                //everyone else
                if (GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(hash)
                        || GameLoader.Instance.statesContainer.hitReaction_Dictionary.ContainsValue(hash)
                        || GameLoader.Instance.statesContainer.landingNames_Dictionary.ContainsValue(hash))
                {
                    var idleState = control.characterSettings.idle_States[UnityEngine.Random.Range(0, control.characterSettings.idle_States.Count)];
                    ChangeAnimationState_CrossFadeInFixedTime(GameLoader.Instance.statesContainer.idle_Dictionary[idleState.animation], transitionDuration: idleState.transitionDuration);
                    return;
                }
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

        #endregion

        #region Conditions
        public bool IsLauchingAttackStateOver(float attackAnimationRepeatRate)
        {
            bool airAttack = GameLoader.Instance.statesContainer.attack_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
            bool groundAttack = GameLoader.Instance.statesContainer.attackFinish_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash);

            //���� ��������, �� ������ normilized time � ������ ������ ��������.
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
            control.animator.CrossFadeInFixedTime(newStateHash, fixedTransitionDuration: transitionDuration, layer: layer, fixedTimeOffset: 0f, normalizedTransitionTime: 0.0f);
        }



        public void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 1f)
        {
            if (control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);


        }
        public void ChangeAnimationStateFixedTime(int newStateHash, int layer = 0, float transitionDuration = 0f)
        {
            if (control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == newStateHash)
            {
                return;
            }
            control.animator.PlayInFixedTime(newStateHash, layer, transitionDuration);


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
}