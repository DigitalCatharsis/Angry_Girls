using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class AttackLogic_ShoryukenDownSmash_Prep : AttackAbilityLogic
    {
        private GameObject _vfx;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.launchedAttackPrepAbility.attackPrepMovementSpeed;


            _vfx = Singleton.Instance.VFXManager.SpawnVFX(control, control.characterSettings.launchedAttackPrepAbility.AttackVFX.GetComponent<VFX>().GetVFXType());
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _vfx.GetComponentInChildren<VisualEffect>().Stop(); //TODO не забудь в остальных стейтах
        }
    }
}