using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_SwordAttack : AttackAbilityLogic
    {
        private GameObject _vfx;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
            //rotation
            control.rigidBody.velocity = new Vector3(0, 10, 2);

            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.launchedAttackPrepAbility.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
            //_vfx.GetComponent<VFX>().InitAndRunVFX(control.characterSettings.launchedAttackPrepAbility, control.gameObject);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if(control.isGrounded)
            {
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (control.subComponentMediator.GetBottomContactPoint() != Vector3.zero)
            {
                control.transform.position = control.subComponentMediator.GetBottomContactPoint();
            }
            control.isAttacking = false;
            control.hasFinishedStaticAttackTurn = true;
            _vfx.GetComponent<VFX>().Dispose();
        }
    }
}