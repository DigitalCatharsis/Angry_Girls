using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
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

            _control.rigidBody.velocity = _control.characterSettings.groundAttackMovementSpeed;
            _control.isAttacking = true;

            //TODO: vfx spawner
            var vfx = Instantiate(Resources.Load(VFX_Type.VFX_Shouryken.ToString())) as GameObject;
            vfx.gameObject.transform.position = _control.transform.position;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime >= 1)
            {
                _control.isAttacking = false;
                _control.subComponentProcessor.launchLogic.hasFinishedTurn = true;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
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