using UnityEngine;

namespace Angry_Girls
{
    public class StaticAttack_Behavior_OnGround : StateMachineBehaviour
    {
        private CharacterControl _control;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CharacterControl>();
            }

            _control.subComponentProcessor.attackSystem.staticAttackLogic_OnGround.OnStateEnter(_control, animator, stateInfo);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.subComponentProcessor.attackSystem.staticAttackLogic_OnGround.OnStateUpdate(_control, animator, stateInfo);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.subComponentProcessor.attackSystem.staticAttackLogic_OnGround.OnStateExit(_control, animator, stateInfo);
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }

}