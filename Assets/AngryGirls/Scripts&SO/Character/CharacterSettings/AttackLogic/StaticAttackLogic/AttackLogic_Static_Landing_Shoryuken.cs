using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Static_Landing_Shoryuken : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //control.isAttacking = false;
            //Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
    }

}