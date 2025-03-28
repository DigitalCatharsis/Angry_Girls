using UnityEngine;

namespace Angry_Girls
{
    //ONLY FOR GROUND
    public class Attack_Fininsh_Behavior : StateMachineBehaviour
    {
        private CControl _control;
        private AttackAbilityLogic _attackabilityLogic;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_control == null)
            {
                _control = animator.transform.root.GetComponent<CControl>();
            }
            _attackabilityLogic = _control.Get_AttackFinish_AttackAbilityLogic();

            _attackabilityLogic.OnStateEnter(_control, animator, stateInfo);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _attackabilityLogic.OnStateUpdate(_control, animator, stateInfo);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _attackabilityLogic.OnStateExit(_control, animator, stateInfo);
        }
    }
}