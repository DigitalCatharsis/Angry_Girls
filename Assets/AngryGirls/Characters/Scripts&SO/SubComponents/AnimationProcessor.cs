using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
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
        A_Shoryuken_Static_Prep,
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
        A_Shoryuken_Static_Prep,
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

    public class AnimationProcessor : SubComponent
    {
        public bool airToGroundFinishedAbility = false;
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
        public override void OnStart()
        {
            control.characterSettings.CheckForNoneValues(control);
            ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
        }

        private void UpdateCurrentStateData_Value()
        {
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            currentStateData.hash = stateInfo.shortNameHash;
            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, currentStateData.hash);
        }

        public override void OnFixedUpdate()
        {
            if (Singleton.Instance.turnManager.currentPhase == CurrentPhase.LaunchingPhase
                && control.playerOrAi == PlayerOrAi.Player
                && control.subComponentProcessor.launchLogic.hasBeenLaunched == true)
            {
                CheckUnit_LaunchingPhase();
                //switch (control.characterSettings.unitType)
                //{
                //    case UnitType.Air:
                //        CheckAirUnit_LaunchingPhase();
                //        break;
                //    case UnitType.AirToGround:
                //        CheckAirToGround_LaunchingPhase();
                //        break;
                //    case UnitType.Ground:
                //        CheckUnit_LaunchingPhase();
                //        break;
                //}
            }

            if (Singleton.Instance.turnManager.currentPhase == CurrentPhase.StaticAttackingPhase)
            {
                switch (control.characterSettings.unitType)
                {
                    case UnitType.Air:
                        CheckAirUnit_Static();
                        break;
                    case UnitType.AirToGround:
                        CheckAirToGround_Static();
                        break;
                    case UnitType.Ground:
                        CheckGroundUnit_Static();
                        break;
                }
            }
        }

        #region Launching Check
        private void CheckAirUnit_LaunchingPhase()
        {
            //Idle
            if (control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
            {
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                return;
            }

            //Airboned
            if (!control.isGrounded
                && !control.isAttacking)
            {
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
            }

            //Airboned + Attack  (AttackPrep)
            if (control.subComponentProcessor.launchLogic.hasUsedAbility)
            {
                if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
                {
                    return;
                }

                ChangeAnimationState_CrossFadeInFixedTime(attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation], control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
            }

            //Airboned at ground. No attack
            if (control.isGrounded)
            {
                control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn = true;
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
            }
        }

        private void CheckAirToGround_LaunchingPhase()
        {
            //Idle
            if (control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn
                && control.isGrounded)
            {
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                return;
            }

            //Airboned + Attack  (AttackPrep)
            if (control.subComponentProcessor.launchLogic.hasUsedAbility
                && !control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn
                && !control.isGrounded
                && !control.isAttacking
                && !airToGroundFinishedAbility)
            {
                if (attackPrep_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return;
                }

                control.animator.StopPlayback();
                ChangeAnimationState_CrossFade(attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation], control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
                return;
            }

            //Just Airboned
            if (!control.isGrounded
                && !control.isAttacking)
            {
                if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
                {
                    return;
                }

                if (_airbonedFlying_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return;
                }
                //control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
            }

            //Landing
            if (control.isGrounded && !control.isAttacking)
            {
                if (IsLandingCondition())
                {
                    ChangeAnimationState_CrossFadeInFixedTime(_landingNames_Dictionary[control.characterSettings.landing_State.animation], control.characterSettings.landing_State.transitionDuration);
                }
            }
        }

        private void CheckUnit_LaunchingPhase()
        {
            //Check Idle
            CheckAndProcced_Idle();

            //Check Ground attack (ground Unit Only)
            if (control.characterSettings.unitType == UnitType.Ground)
            {
                CheckAndProcess_GroundAttack();
            }

            //CheckAttackPrep
            if (control.subComponentProcessor.launchLogic.hasUsedAbility)
            {
                CheckAndProcess_AttackPrep();
            }

            //Check just Airboned
            if (!control.isGrounded && !control.isAttacking)
            {
                CheckAndProcess_AirbonedState();
            }

            //Check Landing
            if (control.isGrounded && !control.isAttacking)
            {
                CheckAndProcess_Landing();
            }



            // isAirboned
            // landing

            ////Idle
            //if (control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn
            //    && control.isGrounded)
            //{
            //    ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
            //    return;
            //}
        }

        private void CheckAndProcess_Landing()
        {
            //no landing phase for air units
            if (control.characterSettings.unitType == UnitType.Air)
            {
                return;
            }

            if (IsLandingCondition())
            {
                ChangeAnimationState_CrossFadeInFixedTime(_landingNames_Dictionary[control.characterSettings.landing_State.animation], control.characterSettings.landing_State.transitionDuration);
            }
        }

        private void CheckAndProcess_AirbonedState()
        {
            //Cant be airboned after finished attack for ground unit for now...
            if (control.characterSettings.unitType == UnitType.Ground
                && control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
            {
                return;
            }

            //everyones logic
            if (attackFinish_Dictionary.ContainsValue(currentStateData.hash))
            {
                return;
            }

            ChangeAnimationState_CrossFadeInFixedTime(_airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
        }
        #endregion


        private bool CheckAndProcced_Idle()
        {
            if (control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
            {
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                return true;
            }

            //Exctra condition for an air unit
            if (control.characterSettings.unitType == UnitType.Air)
            {
                if (control.isGrounded || control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
                {
                    ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                    return true;
                }
            }

            return false;
        }
        private bool CheckAndProcess_GroundAttack()
        {
            //Ground attack
            if (control.isAttacking
                && control.isGrounded
                && !control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
            {
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(attackFinish_Dictionary[control.characterSettings.attackFininsh_State.animation], transitionDuration: control.characterSettings.attackFininsh_State.transitionDuration);
                return true;
            }
            return false;
        }

        private bool CheckAndProcess_AttackPrep()
        {
            //Airboned + Attack  (AttackPrep)
            if (!control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
            {
                if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
                {
                    return false;
                }

                if (attackFinish_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return false;
                }

                ChangeAnimationState_CrossFade(attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation], control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
                control.isAttacking = true;
                return true;
            }

            return false;
        }

        #region Static Check

        //TODO not implemented
        private void CheckAirUnit_Static()
        {
            //Idle
            if (control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
            {
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                return;
            }

            //Airboned
            if (!control.isGrounded
                && !control.isAttacking)
            {
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
            }

            //Airboned + Attack  (AttackPrep)
            if (control.subComponentProcessor.launchLogic.hasUsedAbility)
            {
                if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
                {
                    return;
                }

                ChangeAnimationState_CrossFadeInFixedTime(attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation], control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
            }

            //Airboned at ground. No attack
            if (control.isGrounded)
            {
                control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn = true;
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
            }
        }

        //TODO not implemented
        private void CheckAirToGround_Static()
        {
            //Idle
            if (control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn
                && control.isGrounded)
            {
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                return;
            }

            //Airboned + Attack  (AttackPrep)
            if (control.subComponentProcessor.launchLogic.hasUsedAbility
                && !control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn
                && !control.isGrounded
                && !control.isAttacking
                && !airToGroundFinishedAbility)
            {
                if (attackPrep_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return;
                }

                control.animator.StopPlayback();
                ChangeAnimationState_CrossFade(attackPrep_Dictionary[control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation], control.characterSettings.launchedAttackPrepAbility.attackPrep_State.transitionDuration);
                return;
            }

            //Just Airboned
            if (!control.isGrounded
                && !control.isAttacking)
            {
                if (IsLauchingAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
                {
                    return;
                }

                if (_airbonedFlying_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return;
                }
                //control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
            }

            //Landing
            if (control.isGrounded && !control.isAttacking)
            {
                if (IsLandingCondition())
                {
                    ChangeAnimationState_CrossFadeInFixedTime(_landingNames_Dictionary[control.characterSettings.landing_State.animation], control.characterSettings.landing_State.transitionDuration);
                }
            }
        }

        private void CheckGroundUnit_Static()
        {
            //Idle
            if (control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn
                && control.isGrounded)
            {
                ChangeAnimationState_CrossFadeInFixedTime(_idle_Dictionary[control.characterSettings.idle_State.animation], transitionDuration: control.characterSettings.idle_State.transitionDuration);
                return;
            }

            //Just Airboned
            if (!control.isGrounded
                && !control.isAttacking
                && !control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn)
            {
                if (attackFinish_Dictionary.ContainsValue(currentStateData.hash))
                {
                    return;
                }

                ChangeAnimationState_CrossFadeInFixedTime(_airbonedFlying_Dictionary[control.characterSettings.airbonedFlying_States.animation], control.characterSettings.airbonedFlying_States.transitionDuration);
            }

            //Static attack
            if (control.isAttacking
                && !control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn)
            {
                //prevert from repeating attackPrep
                if (staticAttack_States_Dictionary.ContainsValue(control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash))
                {
                    return;
                }

                if (staticAttack_States_Dictionary.ContainsValue(control.animator.GetNextAnimatorStateInfo(0).shortNameHash))
                {
                    return;
                }

                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(staticAttack_States_Dictionary[control.characterSettings.staticAttackAbility.staticAttack_State.animation], transitionDuration: control.characterSettings.staticAttackAbility.attackTimeDuration);
                //ChangeAnimationState(Singleton.Instance.hashManager.GetHash<StaticAttack_States>(StaticAttack_States.A_Shoryuken_Static, _staticAttack_States_Dictionary),0, control.characterSettings.staticAttackAbility.attackTimeDuration);
            }

            ////Airboned + Attack  (AttackPrep)
            //if (control.isAttacking
            //    && !control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn)
            //{
            //    if (IsStaticAttackStateOver(control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State) == false)
            //    {
            //        return;
            //    }

            //    ChangeAnimationState_CrossFadeInFixedTime(
            //        newStateHash: _staticAttack_States_Dictionary[control.characterSettings.staticAttackAbility.staticAttack_State.animation], 
            //        transitionDuration: control.characterSettings.staticAttackAbility.attackTimeDuration);
            //}

            //Landing
            if (control.isGrounded && !control.isAttacking)
            {
                if (IsLandingCondition())
                {
                    ChangeAnimationState_CrossFadeInFixedTime(_landingNames_Dictionary[control.characterSettings.landing_State.animation], control.characterSettings.landing_State.transitionDuration);
                }
            }
        }
        #endregion

        #region Conditions
        private bool IsLandingCondition()
        {
            if ((_airbonedFlying_Dictionary.ContainsValue(currentStateData.hash) || attackPrep_Dictionary.ContainsValue(currentStateData.hash))
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

            //ѕока работает, но расчет normilized time в данном случае неверный.
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

            //ѕока работает, но расчет normilized time в данном случае неверный.
            if ((StaticAttack)
                && (control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= attackAnimationRepeatRate))
            {
                return false;
            }
            return true;
        }
        private bool IsIdleStateOver()
        {
            if (_idle_Dictionary.ContainsValue(currentStateData.hash)
                && control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
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