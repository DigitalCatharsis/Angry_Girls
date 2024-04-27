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

        [SerializeField] private SerializedDictionary<StateNames, int> stateNamesDictionary;
        [SerializeField] private SerializedDictionary<LandingStates, int> landingNamesDictionary;
        [SerializeField] private SerializedDictionary<AttackStates, int> attackNamesDictionary;
        [SerializeField] private SerializedDictionary<AirbonedStates, int> airbonedNamesDictionary;

        public override void OnComponentEnable()
        {
            //Its me, lol
            control.subComponentProcessor.animationProcessor = this;

            //Init Dictionaries
            stateNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<StateNames>(this.gameObject);
            landingNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<LandingStates>(this.gameObject);
            attackNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AttackStates>(this.gameObject);
            airbonedNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<AirbonedStates>(this.gameObject);

            //Init start Animation
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            currentStateData.hash = stateInfo.shortNameHash;
            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(stateNamesDictionary, currentStateData.hash);
        }

        public override void OnFixedUpdate()
        {
            //isGrounded updates in GroundDetector
            isLanding = CheckForLanding();
            _isAttackStateOver = IsAttackStateOver();
            _isLandingStateOver = IsLandingStateOver();

            //GROUNDED
            if (isGrounded == true)
            {
                if (isAttacking == true)
                {
                    //GROUNDED-ATTACK
                    control.animator.StopPlayback();
                    ChangeAnimationState(stateNamesDictionary[StateNames.A_Shoryuken_DownSmash_Finish], 0, 0f);
                }
                else
                {
                    if (_isAttackStateOver == false)
                    {
                        return;
                    }

                    isAttacking = false;

                    //LANDING (Air state + Grounded)
                    if (isLanding == true)
                    {
                        //We are Landing
                        ChangeAnimationState(stateNamesDictionary[StateNames.A_Fall_Landing], 0, 0f);
                    }
                    else
                    {
                        if(_isLandingStateOver == false)
                        {
                            return;
                        }
                        //IDLE
                        control.rigidBody.velocity = Vector3.zero;
                        control.animator.StopPlayback();
                        ChangeAnimationState_CrossFadeInFixedTime(stateNamesDictionary[StateNames.A_Idle], 0.25f);
                    }
                }
            }
            else
            {
                //AIRBONED
                if (isAttacking == true)
                {
                    if (_isAttackStateOver == false)
                    {
                        return;
                    }

                    ChangeAnimationState_CrossFade(stateNamesDictionary[StateNames.A_Shoryuken_DownSmash_Prep], 0.5f);
                }
                else
                {
                    control.animator.StopPlayback();
                    ChangeAnimationState_CrossFadeInFixedTime(stateNamesDictionary[StateNames.A_Falling_Idle], 0.1f);
                }
            }
        }

        private bool CheckForLanding()
        {
            if (airbonedNamesDictionary.ContainsValue(currentStateData.hash) && isGrounded)
            {
                return true;
            }
            return false;
        }

        private bool IsAttackStateOver()
        {
            if (attackNamesDictionary.ContainsValue(currentStateData.hash) && control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            {
                return false;
            }
            return true;
        }
        private bool IsLandingStateOver()
        {
            if (landingNamesDictionary.ContainsValue(currentStateData.hash) && control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
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

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(stateNamesDictionary, newStateHash);
            currentStateData.hash = newStateHash;

            //control.subComponentProcessor.boxColliderUpdater.UpdateCollider();
        }
        private void ChangeAnimationState_CrossFadeInFixedTime(int newStateHash, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.CrossFadeInFixedTime(newStateHash, transitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(stateNamesDictionary, newStateHash);
            currentStateData.hash = newStateHash;
            //control.subComponentProcessor.boxColliderUpdater.UpdateCollider();
        }
        private void ChangeAnimationState(int newStateHash, int layer = 0, float transitionDuration = 0f)
        {
            if (currentStateData.hash == newStateHash)
            {
                return;
            }
            control.animator.Play(newStateHash, layer, transitionDuration);

            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(stateNamesDictionary, newStateHash);
            currentStateData.hash = newStateHash;
            //control.subComponentProcessor.boxColliderUpdater.UpdateCollider();
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

        //private void SetTurn()
        //{
        //    Debug.Log(control.rigidBody.velocity.z);

        //    if (control.rigidBody.velocity.z > 113f)
        //    {
        //        control.transform.rotation = Quaternion.Euler(0, 0, 0);
        //    }
        //    else if (control.rigidBody.velocity.z < 0.1f)
        //    {
        //        control.transform.rotation = Quaternion.Euler(0, 180, 0);
        //    }
        //}
    }

    [Serializable]
    public class CurrentStateData
    {
        public StateNames currentStateName;
        public int hash;
    }
}