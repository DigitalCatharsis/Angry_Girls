using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Angry_Girls
{
    public class Attack_Fininsh_Behavior : StateMachineBehaviour
    {
        private CharacterControl _control;

        //ONLY FOR GROUND

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CharacterControl>();
            }

            _control.isAttacking = true;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.isAttacking = false;
            _control.subComponentProcessor.attackSystem.DisableAttackTrigger();
            _control.subComponentProcessor.launchLogic.hasFinishedLaunch = true;
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