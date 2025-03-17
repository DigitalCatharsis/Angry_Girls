using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_Shoryuken_DownSmash_Finish : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.AddForce(new Vector3(0, -Math.Abs(control.characterSettings.AttackAbility_Alternate.attackMovementSpeed.y), 0), ForceMode.VelocityChange);

            var _vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken, control.rigidBody.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX_ByCustom(1, false, false, false, control.characterSettings.AttackAbility_Alternate.attackDamage, knockbackValue: control.characterSettings.AttackAbility_Alternate.knockback, false, true, owner: control.gameObject);
            GameLoader.Instance.cameraManager.ShakeCamera();
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1 && control.isGrounded)
            {
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
    }

}