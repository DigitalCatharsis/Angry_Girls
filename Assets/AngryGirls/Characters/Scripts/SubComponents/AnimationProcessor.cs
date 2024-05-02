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

    public enum LandingStates
    {
        A_Fall_Landing
    }
    public enum AttackStates
    {
        A_Shoryuken_DownSmash_Finish,
    }
    public enum AirbonedStates
    {
        A_Falling_Idle,
        A_Shoryuken_DownSmash_Prep,
    }

    public class AnimationProcessor : SubComponent
    {
        public CurrentStateData currentStateData = new();

        public bool isLanding = false;
        public bool isAttacking = false;
        public bool isGrounded = false;


        [SerializeField] private bool _isAttackStateOver = false;
        [SerializeField] private bool _isLandingStateOver = false;

        [SerializeField] private SerializedDictionary<StateNames, int> _stateNamesDictionary;
        [SerializeField] private SerializedDictionary<LandingStates, int> _landingNamesDictionary;
        [SerializeField] private SerializedDictionary<AttackStates, int> _attackNamesDictionary;
        [SerializeField] private SerializedDictionary<AirbonedStates, int> _airbonedNamesDictionary;

        public override void OnComponentEnable()
        {
            //Its me, lol
            control.subComponentProcessor.animationProcessor = this;

            //Init Dictionaries
            _stateNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<StateNames>(this.gameObject);
            _landingNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<LandingStates>(this.gameObject);
            _attackNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AttackStates>(this.gameObject);
            _airbonedNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AirbonedStates>(this.gameObject);

            //Init start Animation
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            currentStateData.hash = stateInfo.shortNameHash;
            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNamesDictionary, currentStateData.hash);
        }

        public override void OnFixedUpdate()
        {
            isLanding = CheckForLanding();
            _isAttackStateOver = IsAttackStateOver();
            _isLandingStateOver = IsLandingStateOver();
            //isGrounded updates in GroundDetector

            if (control.subComponentProcessor.blockingManager.IsFrontBlocked() == true)
            {
                control.rigidBody.velocity = new Vector3(0, control.rigidBody.velocity.y, 0);
            }

            //Grounded + Attack
            if (isGrounded == true && isAttacking == true)
            {
                control.animator.StopPlayback();
                ChangeAnimationState(_stateNamesDictionary[StateNames.A_Shoryuken_DownSmash_Finish], 0, 0f);
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
                    control.rigidBody.velocity = Vector3.zero;
                    ChangeAnimationState(_stateNamesDictionary[StateNames.A_Fall_Landing], 0, 0f);
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
                    ChangeAnimationState_CrossFadeInFixedTime(_stateNamesDictionary[StateNames.A_Idle], 0.25f);
                }
            }

            //Airboned + attack
            if (isGrounded == false && isAttacking == true)
            {
                if (_isAttackStateOver == false)
                {
                    return;
                }

                ChangeAnimationState_CrossFade(_stateNamesDictionary[StateNames.A_Shoryuken_DownSmash_Prep], 0.5f);
            }

            //Airboned
            if (isGrounded == false && isAttacking == false)
            {
                SetRotation();
                control.animator.StopPlayback();
                ChangeAnimationState_CrossFadeInFixedTime(_stateNamesDictionary[StateNames.A_Falling_Idle], 0.1f);
            }
        }

        private bool CheckForLanding()
        {
            if (_airbonedNamesDictionary.ContainsValue(currentStateData.hash) && isGrounded)
            {
                return true;
            }
            return false;
        }

        private bool IsAttackStateOver()
        {
            if (_attackNamesDictionary.ContainsValue(currentStateData.hash) && control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            {
                return false;
            }
            return true;
        }
        private bool IsLandingStateOver()
        {
            if (_landingNamesDictionary.ContainsValue(currentStateData.hash) && control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
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

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNamesDictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }
        private void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.CrossFadeInFixedTime(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNamesDictionary, newStateHash);
            currentStateData.hash = newStateHash;
        }
        private void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 0f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(_stateNamesDictionary, newStateHash);
            currentStateData.hash = newStateHash;
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
    }

    [Serializable]
    public class CurrentStateData
    {
        public StateNames currentStateName;
        public int hash;
    }
}