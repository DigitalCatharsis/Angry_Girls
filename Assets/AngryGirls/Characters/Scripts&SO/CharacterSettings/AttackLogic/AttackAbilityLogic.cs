using UnityEngine;

namespace Angry_Girls
{
    public abstract class AttackAbilityLogic
    {
        public virtual void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }

        public virtual void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
        public virtual void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}