using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_ShoryukenDownSmash_Prep : AttackAbilityLogic
    {
        private GameObject _vfx;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.launchedAttackPrepAbility.attackPrepMovementSpeed;

            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.launchedAttackPrepAbility.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
            //_vfx.GetComponent<VFX>().InitAndRunVFX(control.characterSettings.launchedAttackPrepAbility, control.gameObject);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _vfx.GetComponent<VFX>().Dispose();
        }
    }
}