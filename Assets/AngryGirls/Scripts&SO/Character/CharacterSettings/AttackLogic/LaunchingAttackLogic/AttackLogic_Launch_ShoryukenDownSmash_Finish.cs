using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_ShoryukenDownSmash_Finish : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = true;
            control.rigidBody.velocity = Vector3.zero;

            var _vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken ,control.transform.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX(1, false, false,false, control.characterSettings.AttackAbility_Launch.attackDamage, false, true, owner: control.gameObject);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1)
            {
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //control.FinishTurn(2);
        }
    }
}