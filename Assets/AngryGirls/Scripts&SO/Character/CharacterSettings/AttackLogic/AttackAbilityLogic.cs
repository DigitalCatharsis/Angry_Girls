using UnityEngine;

namespace Angry_Girls
{
    public abstract class AttackAbilityLogic
    {
        protected AttackAbilityData _attackAbility;
        public AttackAbilityLogic(AttackAbilityData ability) 
        {
            _attackAbility = ability;
        }

        public virtual void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (control == null)
            {
                control = animator.transform.root.GetComponent<CControl>();
            }

            control.isAttacking = true;

            control.rigidBody.velocity = Vector3.zero;

            control.rigidBody.AddForce(new Vector3(0, _attackAbility.attackMovementForce.y,_attackAbility.attackMovementForce.z * control.transform.forward.z), ForceMode.VelocityChange);

        }

        public virtual void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
        public virtual void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}