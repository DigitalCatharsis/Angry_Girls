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
        Idle,
        Fall,
        Shoryuken_DownSmash_Finish,
        Shoryuken_DownSmash_Prep,
        Shoryuken_Start,
        Fall_Landing,
    }

    public enum LandingStates
    {
        Fall_Landing
    }

    public class AnimationProcessor : SubComponent
    {
        public CurrentStateData currentStateData = new();

        public bool isLanding = false;

        [SerializeField] private SerializedDictionary<StateNames, int> stateNamesDictionary;
        [SerializeField] private SerializedDictionary<LandingStates, int> landingNamesDictionary;

        public override void OnUpdate()
        {
            isLanding = CheckForLanding();

            if (isLanding == true )
            {
                if (control.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.3f)
                control.transform.position = control.subComponentProcessor.groundDetector.landingPosition;
            }

            if (control.subComponentProcessor.groundDetector.isGrounded)
            {
                control.animator.SetBool(MainParameterType.IsGrounded.ToString(), true);
                SetTurn();
            }
            else
            {
                control.animator.SetBool(MainParameterType.IsGrounded.ToString(), false);
            }
        }

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.animationProcessor = this;
        }

        public override void OnAwake()
        {
        }
        public override void OnFixedUpdate()
        {
            //Kepp CurrentStateUpdated
            if (currentStateData.hash != control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash)
            {
                UpdateCurrentState();
            }
        }

        public override void OnLateUpdate()
        {
        }

        public override void OnStart()
        {
            stateNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<StateNames>(this.gameObject);
            landingNamesDictionary = Singleton.Instance.hashManager.CreateAndInitDictionary<LandingStates>(this.gameObject);
            UpdateCurrentState();
        }

        private void UpdateCurrentState()
        {
            AnimatorStateInfo stateInfo = control.animator.GetCurrentAnimatorStateInfo(0);
            currentStateData.hash = stateInfo.shortNameHash;
            currentStateData.currentStateName = Singleton.Instance.hashManager.GetName(stateNamesDictionary, currentStateData.hash);
        }

        private void SetTurn()
        {
            if (control.rigidBody.velocity.z > 0.1f)
            {
                control.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (control.rigidBody.velocity.z < 0.1f)
            {
                control.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        private bool CheckForLanding()
        {
            if (landingNamesDictionary.ContainsValue(currentStateData.hash))
            {
                return true;
            }

            return false;

        }
    }

    [Serializable]
    public class CurrentStateData
    {
        public StateNames currentStateName;
        public int hash;
    }
}