using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class AttackLogic_Static_Prep_Shoryuken : AttackAbilityLogic
    {
        private GameObject _runningVFX;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //Первый вариант
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
            _runningVFX = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.staticAttackAbility.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
            _runningVFX.GetComponent<VFX>().InitAndRunVFX(control.characterSettings.launchedAttackPrepAbility, control.gameObject);

        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1
                && control.isGrounded)
            {
                //TODO: fuck...
                control.subComponentMediator.TEMP_SetShorukenLandingState();
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _runningVFX.GetComponentInChildren<VisualEffect>().Stop(); //TODO не забудь в остальных стейтах
        }
    }
}

