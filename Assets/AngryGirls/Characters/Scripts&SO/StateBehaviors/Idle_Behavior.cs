using UnityEngine;

namespace Angry_Girls
{
    public class Idle_Behavior : StateMachineBehaviour
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
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }
    }
}