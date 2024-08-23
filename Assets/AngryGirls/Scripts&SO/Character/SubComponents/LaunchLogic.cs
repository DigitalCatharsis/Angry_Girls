using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchLogic : SubComponent
    {
        public override void OnFixedUpdate()
        {
            if (control.hasBeenLaunched == true && control.hasFinishedLaunchingTurn == false)
            {
                Singleton.Instance.�ameraManager.CenterCameraAgainst(control.boxCollider);
            }
        }
        public void ProcessLaunch()
        {
            ColorDebugLog.Log(control.name + this.name + " proceed ProcessLaunch", System.Drawing.KnownColor.ControlLightLight);            
            StartCoroutine(Launch());
        }

        private IEnumerator Launch()
        {
            yield return StartCoroutine(OnLaunch_Routine());
        }

        private IEnumerator OnLaunch_Routine()
        {
            control.hasBeenLaunched = true;

            //Changing layer from CharacterToLaunch to Character
            int characterLayer = LayerMask.NameToLayer("Character");
            transform.root.gameObject.layer = characterLayer;

            control.checkGlobalBehavior = true;
            control.hasFinishedLaunchingTurn = false;
            Camera.main.orthographicSize -= (Camera.main.orthographicSize / 5f);  //TODO: replace
            yield return new WaitForSeconds(0.1f);
            while (!control.hasUsedAbility)
            {
                if (control.hasFinishedLaunchingTurn)
                {
                    break;
                }

                CheckForAbilityUse();
                yield return null;
            }

            while (!control.hasFinishedLaunchingTurn)
            {
                yield return null;
            }
            Singleton.Instance.launchManager.OnLaunchIsOver();
        }

        private void CheckForAbilityUse()
        {
            if (control.hasUsedAbility)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                //process ability
                control.hasUsedAbility = true;
                ColorDebugLog.Log("Ability has been used", System.Drawing.KnownColor.Magenta);
            }
        }

        public override void OnUpdate()
        {
        }
        public override void OnComponentEnable()
        {            
        }
        public override void OnLateUpdate()
        {
        }
        public override void OnAwake()
        {
        }
        public override void OnStart()
        {
        }
    }
}