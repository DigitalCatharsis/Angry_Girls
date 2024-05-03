using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchLogic : SubComponent
    {
        public bool hasBeenLaunched = false;
        public bool hasFinishedLaunch = false;
        public bool hasUsedAbility = false;

        public override void OnAwake()
        {
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
            var temp = control;

            hasBeenLaunched = true;
            Camera.main.orthographicSize -= (Camera.main.orthographicSize / 5f);  //TODO: replace
            yield return new WaitForSeconds(0.1f);
            while (!hasUsedAbility)
            {
                if (hasFinishedLaunch)
                {
                    break;
                }

                CheckForAbilityUse();
                yield return null;
            }

            while (!hasFinishedLaunch)
            {
                yield return null;
            }
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
            if (hasBeenLaunched == true && hasFinishedLaunch == false)
            {
                Singleton.Instance.ñameraManager.CenterCameraAgainst(control.boxCollider);
            }
        }

        public override void OnLateUpdate()
        {
        }
    }
}