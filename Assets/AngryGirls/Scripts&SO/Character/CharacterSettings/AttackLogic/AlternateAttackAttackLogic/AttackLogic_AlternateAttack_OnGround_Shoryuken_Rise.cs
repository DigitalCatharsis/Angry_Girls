using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_OnGround_Shoryuken_Rise : AttackAbilityLogic
    {
        private GameObject _vfx;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

            _vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken, control.transform.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX(
                1, 
                false, 
                destroyOnCollision: false, 
                destroyOnCharacterCollision: false, 
                control.characterSettings.AttackAbility_Launch.attackDamage, 
                enableCollider: false, 
                enableTrigger: true, 
                owner: control.gameObject
                ); 
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 1)
            {
                control.FinishTurn();
                //control.isAttacking = false;
                //control.hasFinishedAlternateAttackTurn = true;
            }
        }
        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        }
    }
}