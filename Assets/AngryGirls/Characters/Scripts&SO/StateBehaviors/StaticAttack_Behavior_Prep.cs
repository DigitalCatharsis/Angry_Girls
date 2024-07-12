using UnityEngine;

namespace Angry_Girls
{
    public class StaticAttack_Behavior_Prep : StateMachineBehaviour
    {
        private CControl _control;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CControl>();
            }

            _control.subComponentProcessor.attackSystem.staticAttackLogic_Prep.OnStateEnter(_control, animator, stateInfo);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.subComponentProcessor.attackSystem.staticAttackLogic_Prep.OnStateUpdate(_control, animator, stateInfo);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.subComponentProcessor.attackSystem.staticAttackLogic_Prep.OnStateExit(_control, animator, stateInfo);
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