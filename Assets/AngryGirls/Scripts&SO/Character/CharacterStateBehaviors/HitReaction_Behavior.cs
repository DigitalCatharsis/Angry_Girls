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
            //    _control.isAttacking = false;
            //    _control.isLanding = false;
            //    _control.unitGotHit = false;
            //}
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            //_control.unitGotHit = false;
        }
    }

}