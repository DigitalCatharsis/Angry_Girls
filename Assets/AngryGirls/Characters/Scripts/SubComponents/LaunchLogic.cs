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

        private void LateUpdate()
        {
            if (hasBeenLaunched == true && hasFinishedLaunch == false)
            {
                CameraManager.Instance.CenterCameraAgainst(Control.Boxcollider);
            }
        }

        public IEnumerator ProcessLaunch()
        {
            yield return StartCoroutine(OnLaunch_Routine());
        }

        private IEnumerator OnLaunch_Routine()
        {
            hasBeenLaunched = true;
            yield return new WaitForSeconds(0.1f);
            while (Control.SubComponentProcessor.groundDetector.IsAirboned)
            {
                CheckForAbilityUse();
                yield return null;
            }
            CameraManager.Instance.ReturnCameraToStartPosition(1f);
            hasFinishedLaunch = true;           
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
                Control.Animator.SetBool(MainParameterType.IsAttacking.ToString(), true);
                ColorDebugLog.Log("Ability has been used", System.Drawing.KnownColor.Magenta);
            }
        }

    }
}