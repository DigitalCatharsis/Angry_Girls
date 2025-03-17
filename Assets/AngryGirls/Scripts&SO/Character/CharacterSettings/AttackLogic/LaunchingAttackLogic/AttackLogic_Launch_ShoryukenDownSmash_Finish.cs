using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_ShoryukenDownSmash_Finish : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isLanding = false;
            control.isAttacking = true;
            control.rigidBody.velocity = Vector3.zero;

            var _vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken ,control.rigidBody.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX_ByCustom(1, false, false,false, control.characterSettings.AttackAbility_Launch.attackDamage, knockbackValue: control.characterSettings.AttackAbility_Launch.knockback, false, true, owner: control.gameObject);

            GameLoader.Instance.cameraManager.ShakeCamera();
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