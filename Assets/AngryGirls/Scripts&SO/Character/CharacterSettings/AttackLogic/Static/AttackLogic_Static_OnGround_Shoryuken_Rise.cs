using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Static_OnGround_Shoryuken_Rise : AttackAbilityLogic
    {
        private GameObject _vfx;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

            _vfx = Singleton.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX(
                1, 
                false, 
                destroyOnCollision: false, 
                control.characterSettings.launchedAttackPrepAbility.attackDamage, 
                enableCollider: false, 
                enableTrigger: true, //TODO: бага в анимации из-за урона самому себе
                owner: control.gameObject
                ); 
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1)
            {
                control.isAttacking = false;
                control.hasFinishedStaticAttackTurn = true;
            }
        }
        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
    }
}