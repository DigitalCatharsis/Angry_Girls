using UnityEngine;

namespace Angry_Girls
{
    public class Landing_Behavior : StateMachineBehaviour
    {
        private CControl _control;

        //Air units does not land
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CControl>();
            }

            _control.rigidBody.velocity = _control.characterSettings.landingMovementSpeed;
            _control.isLanding = true;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control.characterSettings.unitType == UnitType.AirToGround)
            {
                if (stateInfo.normalizedTime >= 1)
                {
                    _control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn = true;
                }
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _control.isLanding = false;
            _control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn = true;
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