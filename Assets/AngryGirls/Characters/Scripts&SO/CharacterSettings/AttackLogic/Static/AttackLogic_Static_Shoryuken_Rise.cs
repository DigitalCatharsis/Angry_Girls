using Angry_Girls;
using UnityEngine;

public class AttackLogic_Static_Shoryuken_Rise : AttackAbilityLogic
{
    public override void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
        //TODO: vfx spawner
        Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity);
    }

    public override void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
        if (stateInfo.normalizedTime >= 1)
        {
            control.isAttacking = false;
            control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn = true;
        }
    }
    public override void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
    }
}
