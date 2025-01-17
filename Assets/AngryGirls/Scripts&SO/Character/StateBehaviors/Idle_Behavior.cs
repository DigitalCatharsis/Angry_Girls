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

            if (_control.playerOrAi == PlayerOrAi.Ai)
            {
                return;
            }

            //no need in characterToLaunch
            if (_control.gameObject.layer == 14)
            {
                return;
            }

            if (_control.CheckAttackFinishCondition())
            {
                _control.FinishTurn(2f);
            }


            if (_control.characterSettings.unitType == UnitType.Air)
            {
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