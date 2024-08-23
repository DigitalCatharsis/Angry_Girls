using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

namespace Angry_Girls
{
        //ONLY FOR GROUND
    public class Attack_Fininsh_Behavior : StateMachineBehaviour
    {
        private CharacterControl _control;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CharacterControl>();
            }

            _control.attackSystem_Data.attackFinishLogic.OnStateEnter(_control, animator, stateInfo);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.attackSystem_Data.attackFinishLogic.OnStateUpdate(_control, animator, stateInfo);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.attackSystem_Data.attackFinishLogic.OnStateExit(_control, animator, stateInfo);
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