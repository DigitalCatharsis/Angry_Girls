using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum MainParameterType
    {
        IsGrounded,
        IsAttacking
    }
    public enum StateNames
    {
        A_Idle,
        A_Falling_Idle,
        A_Shoryuken_DownSmash_Finish,
        A_Shoryuken_DownSmash_Prep,
        A_Fall_Landing
    }
    public enum GroundIdle_States
    {
        A_Idle,
    }
    public enum AirbonedIdle_States
    {
        A_Falling_Idle,
    }
    public enum AirbonedAttack_States
    {
        A_Shoryuken_DownSmash_Prep,
    }
    public enum GroundedAttack_States
    {
        A_Shoryuken_DownSmash_Finish,
    }
    public enum Landing_States
    {
        A_Fall_Landing,
    }

    public class AnimationProcessor : SubComponent
    {
        public CurrentStateData currentStateData = new();

        public bool isLanding = false;
        public bool isAttacking = false;
        public bool isGrounded = false;


        [SerializeField] private bool _isAttackStateOver = false;
        [SerializeField] private bool _isLandingStateOver = false;

        [SerializeField] private SerializedDictionary<StateNames, int> _stateNames_Dictionary;
        [SerializeField] private SerializedDictionary<GroundIdle_States, int> _groundIdle_Dictionary;
        [SerializeField] private SerializedDictionary<AirbonedIdle_States, int> _airbonedIdle_Dictionary;
        [SerializeField] private SerializedDictionary<AirbonedAttack_States, int> _airbonedAttack_Dictionary;
        [SerializeField] private SerializedDictionary<GroundedAttack_States, int> _groundedAttack_Dictionary;
        [SerializeField] private SerializedDictionary<Landing_States, int> _landingNames_Dictionary;

        public override void OnComponentEnable()
        {
            //Its me, lol
            control.subComponentProcessor.animationProcessor = this;

            //Init Dictionaries
            _stateNames_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<StateNames>(this.gameObject);
            _groundIdle_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<GroundIdle_States>(this.gameObject);
            _airbonedIdle_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AirbonedIdle_States>(this.gameObject);
            _airbonedAttack_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AirbonedAttack_States>(this.gameObject);
            _groundedAttack_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<GroundedAttack_States>(this.gameObject);
            _landingNames_Dictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<Landing_States>(this.gameObject);

            //Init start Animation
            UpdateCurrentState();
        }

        private void UpdateCurrentState()
        {
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            currentStateData.hash = stateInfo.shortNameHash;
            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, currentStateData.hash);
        }

        public override void OnFixedUpdate()
        {
            isLanding = CheckForLanding();
            _isAttackStateOver = IsAttackStateOver();
            _isLandingStateOver = IsLandingStateOver();
            //isGrounded updates in GroundDetector

            if (control.subComponentProcessor.blockingManager.IsFrontBlocked() == true)
            {
                control.rigidBody.velocity = control.characterSettings.atHittingObstacle_Speed;
            }

            //Grounded + Attack
            if (isGrounded == true && isAttacking == true)
            {
                control.animator.StopPlayback();
                ChangeAnimationState(_groundedAttack_Dictionary[control.characterSettings.groundedAttack_State], 0, 0f);
                control.rigidBody.velocity = control.characterSettings.groundAttackMovementSpeed;
            }

            //Grounded
            if (isGrounded == true && isAttacking == false)
            {
                //Is in attack state?
                if (_isAttackStateOver == false)
                {
                    return;
                }

                //LANDING
                if (isLanding == true)
                {
                    control.rigidBody.velocity = control.characterSettings.landingAttackMovementSpeed;
                    ChangeAnimationState(_landingNames_Dictionary[control.characterSettings.landing_State], 0, 0f);
                }

                //IDLE
                if (isLanding == false)
                {
                    if (_isLandingStateOver == false)
                    {
                        return;
                    }
                    control.rigidBody.velocity = Vector3.zero;
                    control.animator.StopPlayback();
                    ChangeAnimationState_CrossFadeInFixedTime(_groundIdle_Dictionary[control.characterSettings.idle_State], 0.25f);
                }
            }

            //Airboned + attack
            if (isGrounded == false && isAttacking == true)
            {
                if (_isAttackStateOver == false)
                {
                    return;
                }

                ChangeAnimationState_CrossFade(_airbonedAttack_Dictionary[control.characterSettings.airbonedAttack_State], 0.5f);
            }

            //Airboned
            if (isGrounded == false && isAttacking == false)
            {
                SetRotation();
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_airbonedIdle_Dictionary[control.characterSettings.airbonedIdle_State], 0.1f);
            }
        }

        private bool CheckForLanding()
        {
            if ((_airbonedIdle_Dictionary.ContainsValue(currentStateData.hash) || _airbonedAttack_Dictionary.ContainsValue(currentStateData.hash)) 
                && isGrounded)
            {
                return true;
            }
            return false;
        }

        private bool IsAttackStateOver()
        {
            if ((_airbonedAttack_Dictionary.ContainsValue(currentStateData.hash) || _groundedAttack_Dictionary.ContainsValue(currentStateData.hash))
                && control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            {
                return false;
            }
            return true;
        }
        private bool IsLandingStateOver()
        {
            if (_landingNames_Dictionary.ContainsValue(currentStateData.hash) 
                && control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            {
                return false;
            }
            return true;
        }

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
        private void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.CrossFadeInFixedTime(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }
        private void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 0f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNames_Dictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }

        public override void OnAwake()
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnStart()
        {
        }

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