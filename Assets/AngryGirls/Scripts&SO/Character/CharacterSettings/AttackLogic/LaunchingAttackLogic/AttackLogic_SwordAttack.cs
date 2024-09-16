using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_SwordAttack : AttackAbilityLogic
    {
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
            control.rigidBody.velocity = new Vector3(0, 10, 2);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (control.subComponentMediator.GetBottomContactPoint() != Vector3.zero)
            {
                control.transform.position = control.subComponentMediator.GetBottomContactPoint();
            }
            control.isAttacking = false;
            control.airToGroundUnit_FinishedAbility = true;
        }
    }
}