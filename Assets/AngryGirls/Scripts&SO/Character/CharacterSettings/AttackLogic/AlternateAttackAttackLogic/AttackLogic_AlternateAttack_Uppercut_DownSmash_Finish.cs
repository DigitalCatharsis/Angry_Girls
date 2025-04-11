using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Alternate_UppercutDownSmash_Finish : AttackAbilityLogic
    {
        public AttackLogic_Alternate_UppercutDownSmash_Finish(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private bool _cameraShaked = false;

        private GameObject _projectile;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            var ability = control.characterSettings.AttackAbility_Alternate;
            _projectile = GameLoader.Instance.VFXManager.SpawnByProjectileAbility
                (
                teamfire: false,
                spawnTramsform: control.transform,
                originator: control.gameObject,
                layerMask: control.GetVfxLayermask(),
                vfxType: VFX_Type.VFX_Downsmash,
                vfxColor: Color.white,
                timeToLive: 1f,
                connectToOriginator: false,
                destroyOnCollision: false,
                destroyOnCharacterTrigger: false,
                attackDamageValue: control.characterSettings.AttackAbility_Alternate.attackDamage,
                enemyKnockBackValue: ability.enemyKnockbackValue,
                enableCollider: false,
                enableTrigger: true,
                spawnSound: new Tuple<AudioSourceType, int>(AudioSourceType.SFX_Impact, 2),
                destroySound: null
                );


            _projectile.transform.position = control.CharacterMovement.Rigidbody.position;
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 0.14 && control.isGrounded && _cameraShaked == false)
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
            GameLoader.Instance.VFXManager.CallVFXDispose(_projectile);
            control.isAttacking = false;
            _cameraShaked = false;
        }
    }
}