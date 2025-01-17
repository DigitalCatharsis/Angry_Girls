using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_Shoryuken_DownSmash_Finish : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = new Vector3(0, -Math.Abs(control.characterSettings.AttackAbility_Alternate.attackMovementSpeed.y), 0);

            var _vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX(1, false, false, false, control.characterSettings.AttackAbility_Alternate.attackDamage, false, true, owner: control.gameObject);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1 && control.isGrounded)
            {
                control.FinishTurn(2);
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
    }

}