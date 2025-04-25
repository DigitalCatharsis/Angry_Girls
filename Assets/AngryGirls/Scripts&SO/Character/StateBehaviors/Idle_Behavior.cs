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

            //no need in characterToLaunch
            if (_control.gameObject.layer == 14)
            {
                return;
            }

            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.LaunchingPhase)
            {
                //Ai does not attack in launch phase
                if (_control.playerOrAi == PlayerOrAi.Bot)
                {
                    return;
                }
                //already launched unit does not attack in launch phase
                if (_control.playerOrAi == PlayerOrAi.Character && _control.hasFinishedLaunchingTurn)
                {
                    return;
                }
            }
                        
            if (_control.CheckAttackFinishCondition())
            {
                _control.FinishTurn();
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