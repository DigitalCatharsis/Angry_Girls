using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchLogic : SubComponent
    {
        public bool hasBeenLaunched = false;
        public bool hasFinishedLaunch = false;
        public bool hasUsedAbility = false;
        //TODO: SO abilityType

        public override void OnAwake()
        {
        }
        public override void OnLateUpdate()
        {
            if (hasBeenLaunched == true && hasFinishedLaunch == false)
            {
                Singleton.Instance.ñameraManager.CenterCameraAgainst(control.boxCollider);
            }
        }

        public override void OnStart()
        {
        }

        public IEnumerator ProcessLaunch()
        {
            yield return StartCoroutine(OnLaunch_Routine());
        }

        private IEnumerator OnLaunch_Routine()
        {
            hasBeenLaunched = true;
            yield return new WaitForSeconds(0.1f);
            while (!control.subComponentProcessor.groundDetector.isGrounded)
            {
                CheckForAbilityUse();
                yield return null;
            }
            control.animator.SetBool(MainParameterType.IsAttacking.ToString(), false);
            hasFinishedLaunch = true;                       
            Singleton.Instance.launchManager.OnLaunchIsOver();            
        }        

        private void CheckForAbilityUse()
        {
            if (hasUsedAbility) 
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                //process ability
                hasUsedAbility = true;
                control.animator.SetBool(MainParameterType.IsAttacking.ToString(), true);
                ColorDebugLog.Log("Ability has been used", System.Drawing.KnownColor.Magenta);
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.launchLogic = this;
        }

        public override void OnFixedUpdate()
        {
        }


    }
}