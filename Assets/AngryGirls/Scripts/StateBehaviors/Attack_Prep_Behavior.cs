using UnityEngine;

namespace Angry_Girls
{
    public class Attack_Prep_Behavior : StateMachineBehaviour
    {
        private CharacterControl _control;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CharacterControl>();
            }

            _control.isAttacking = true;

            if (_control.characterSettings.unitType == UnitType.Air)
            {
                _control.rigidBody.useGravity = false;
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control.characterSettings.unitType == UnitType.Air)
            {
                _control.isAttacking = false;
                _control.subComponentProcessor.launchLogic.hasFinishedLaunch = true;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control.characterSettings.unitType == UnitType.Ground)
            {
                //will go to fall in AnimationProcessor
            }
            if (_control.characterSettings.unitType == UnitType.AirToGround)
            {
                _control.isAttacking = false;
                //will go to fall in AnimationProcessor
            }

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