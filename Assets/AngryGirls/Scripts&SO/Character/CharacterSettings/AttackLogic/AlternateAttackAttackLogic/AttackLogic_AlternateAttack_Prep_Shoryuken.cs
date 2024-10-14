using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_Prep_Shoryuken : AttackAbilityLogic
    {
        private GameObject _runningVFX;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = (new Vector3(0, 3.5f, 1.5f * control.transform.forward.z));
            _runningVFX = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.AttackAbility_Alternate.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);

        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1
                && control.isGrounded)
            {
                //TODO: fuck...
                //control.subComponentMediator.TEMP_SetShorukenLandingState();
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
            _runningVFX.GetComponentInChildren<VisualEffect>().Stop(); //TODO не забудь в остальных стейтах
        }
    }
}

