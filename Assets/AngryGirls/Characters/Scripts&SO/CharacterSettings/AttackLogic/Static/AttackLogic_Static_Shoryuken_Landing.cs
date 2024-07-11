using Angry_Girls;
using UnityEngine;

public class AttackLogic_Static_Shoryuken_Landing : AttackAbilityLogic
{
    public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
        control.isAttacking = false;
        //Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity);
    }

    public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
    {

    }
    public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
    }
}
