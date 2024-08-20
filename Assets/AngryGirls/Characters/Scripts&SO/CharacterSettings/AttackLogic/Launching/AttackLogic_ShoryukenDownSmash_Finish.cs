using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_ShoryukenDownSmash_Finish : AttackAbilityLogic
    {
        private GameObject _vfx;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.groundAttackMovementSpeed;

            _vfx = Singleton.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken ,control.transform.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX(1, false, false, control.characterSettings.launchedAttackPrepAbility.attackDamage, false, true, owner: control.gameObject);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1)
            {
                control.isAttacking = false;
                control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn = true;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}