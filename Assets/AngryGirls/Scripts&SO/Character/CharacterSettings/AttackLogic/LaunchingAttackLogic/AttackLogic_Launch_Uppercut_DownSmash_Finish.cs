using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_UppercutDownSmash_Finish : AttackAbilityLogic
    {
        public AttackLogic_Launch_UppercutDownSmash_Finish(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private bool _cameraShaked = false;

        private GameObject _vfx;

        private AudioSource _auidioSource;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            var ability = control.characterSettings.AttackAbility_Launch;

            _vfx = GameLoader.Instance.VFXManager.SpawnByProjectileAbility
                (
                destroyOnCollision: false,
                spawnTramsform: control.transform,
                originator: control.gameObject,
                destroyOnCharacterTrigger: false,
                attackDamageValue: ability.attackDamage,
                enemyKnockBackValue: ability.enemyKnockbackValue,
                enableCollider: false,
                enableTrigger: true,
                spawnSound: new Tuple<AudioSourceType, int>(AudioSourceType.SFX_Impact, 2),
                destroySound: null,
                layerMask: control.GetVfxLayermask(),
                vfxType: VFX_Type.VFX_Downsmash,
                vfxColor: Color.white,
                timeToLive: 1f,
                connectToOriginator: false,
                teamfire: false
                );

            _vfx.transform.position = control.CharacterMovement.Rigidbody.position;
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
            GameLoader.Instance.VFXManager.CallVFXDispose(_vfx);
            control.isAttacking = false;
            _cameraShaked = false;
        }
    }
}