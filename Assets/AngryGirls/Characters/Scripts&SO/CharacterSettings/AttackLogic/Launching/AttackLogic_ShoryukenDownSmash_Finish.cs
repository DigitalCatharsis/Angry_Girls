using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_ShoryukenDownSmash_Finish : AttackAbilityLogic
    {
        public override void OnStateEnter(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = true;
            control.rigidBody.velocity = control.characterSettings.groundAttackMovementSpeed;

            //TODO: vfx spawner
            Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity);
        }
        public override void OnStateUpdate(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1)
            {
                control.isAttacking = false;
                control.subComponentProcessor.launchLogic.hasFinishedaunLaunchingTurn = true;
            }
        }

        public override void OnStateExit(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}