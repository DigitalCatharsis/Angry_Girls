using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_UppercutDownSmash_Finish : AttackAbilityLogic
    {
        public AttackLogic_Launch_UppercutDownSmash_Finish(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private bool _cameraShaked = false;

        private GameObject _vfx;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {

            _vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Uppercut ,control.rigidBody.position, Quaternion.identity);
            _vfx.GetComponent<VFX>().InitAndRunVFX_ByCustom(1, false, false,false, control.characterSettings.AttackAbility_Launch.attackDamage, knockbackValue: control.characterSettings.AttackAbility_Launch.enemyKnockbackValue, false, true, owner: control.gameObject);

        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 0.09 && control.isGrounded && _cameraShaked == false)
            {
                GameLoader.Instance.cameraManager.ShakeCamera();

                _cameraShaked = true;
            }

            if (stateInfo.normalizedTime >= 1 && control.isGrounded)
            {
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _vfx.GetComponent<VFX>().Dispose();
            control.isAttacking = false;
            _cameraShaked = false;
        }
    }
}