using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Static_Prep_Shoryuken : AttackAbilityLogic
    {
        private GameObject runningVFX;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //runningVFX = Singleton.Instance.VFXManager.SpawnVFX(
            //    parentsTransform: control.gameObject.transform,
            //    vfx_Type: control.characterSettings.staticAttackAbility.AttackVFX.GetComponent<VFXPoolObject>().poolObjectType,
            //    VFXColor: control.subComponentProcessor.attackSystem.VFX_Color,
            //    spawnPosition: control.subComponentProcessor.attackSystem.projectileSpawnTransform.position,
            //    spawnRotation: Quaternion.identity,
            //    timeToLive: control.characterSettings.staticAttackAbility.timeToLive,
            //    isTimeToLiveIsNormilizedTime: true,
            //    destroyOnCollision: true,
            //    VFXDamage: control.characterSettings.launchedAttackPrepAbility.attackDamage,
            //    enableCollider: false,
            //    enableTrigger: true
            //    );

            //Второй вариант с перегрузкой
            //runningVFX = Singleton.Instance.VFXManager.SpawnVFX(control, control.characterSettings.staticAttackAbility.AttackVFX.GetComponent<VFXPoolObject>().poolObjectType);
        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1
                && control.isGrounded)
            {
                var ap = control.subComponentProcessor.animationProcessor;
                //TODO: fuck...
                ap.ChangeAnimationState(ap.staticAttack_States_Dictionary[StaticAttack_States.A_Shoryuken_Landing_Static], 0, transitionDuration: control.characterSettings.idle_State.transitionDuration);
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}

