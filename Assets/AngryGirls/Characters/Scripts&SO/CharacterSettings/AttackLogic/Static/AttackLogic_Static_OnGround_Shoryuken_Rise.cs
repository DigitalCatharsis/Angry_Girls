using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Static_OnGround_Shoryuken_Rise : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            var poolManager = Singleton.Instance.poolManager;
            poolManager.GetObject(VFX_Type.VFX_Shouryken, poolManager.vfxPoolDictionary, control.transform.position, Quaternion.identity);
            //TODO: vfx spawner 
            //Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity); // OLD!
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1)
            {
                control.isAttacking = false;
                control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn = true;
            }
        }
        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
    }
}