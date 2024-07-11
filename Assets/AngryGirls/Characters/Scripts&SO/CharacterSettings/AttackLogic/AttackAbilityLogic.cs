using UnityEngine;

namespace Angry_Girls
{
    public abstract class AttackAbilityLogic
    {
        public virtual void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public virtual void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
        public virtual void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}