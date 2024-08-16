using UnityEngine;

namespace Angry_Girls
{
    public class HitReaction_Behavior : StateMachineBehaviour
    {
        private CControl _control;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CControl>();
            }

        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //No need in this, cause of no crossfade
            //if (stateInfo.normalizedTime >= 1)
            //{
            //    _control.subComponentProcessor.animationProcessor.unitGotHit = false;
            //}
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.subComponentProcessor.animationProcessor.unitGotHit = false;
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