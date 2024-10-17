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

            Debug.Log("Entering Idle Behavior");
            if (_control.animator.GetNextAnimatorStateInfo(0).shortNameHash == GameLoader.Instance.hashManager.GetHash(Idle_States.Idle_Floating_Air, GameLoader.Instance.statesContainer.idle_Dictionary)) 
            {
                Debug.Log(_control.name + " is floating");
                var vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Flame, _control.projectileSpawnTransform.position, Quaternion.identity);
                vfx.transform.parent = _control.projectileSpawnTransform;
                vfx.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
            else
            {
                Debug.Log(
                    GameLoader.Instance.hashManager.
                    GetName<Idle_States>(GameLoader.Instance.statesContainer.idle_Dictionary, _control.animator.GetNextAnimatorStateInfo(0).shortNameHash).ToString());
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