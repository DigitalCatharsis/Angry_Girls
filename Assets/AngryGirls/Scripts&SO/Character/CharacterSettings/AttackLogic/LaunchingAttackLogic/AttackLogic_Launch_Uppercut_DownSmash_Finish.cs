using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_UppercutDownSmash_Finish : AttackAbilityLogic
    {
        public AttackLogic_Launch_UppercutDownSmash_Finish(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private bool _cameraShaked = false;

        private GameObject _projectile;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            var ability = control.CharacterSettings.AttackAbility_Launch;

            _projectile = GameplayCoreManager.Instance.ProjectileManager.SpawnDownSmash(control, ability);

            //    _vfx = GameplayCoreManager.Instance.ProjectileManager.SpawnProjectile
            //        (
            //        destroyOnCollision: false,
            //        spawnTransform: control.transform,
            //        originator: control.gameObject,
            //        destroyOnCharacterTrigger: false,
            //        attackDamageValue: ability.attackDamage,
            //        enemyKnockBackValue: ability.enemyKnockbackValue,
            //        enableCollider: false,
            //        enableTrigger: true,
            //        spawnSound: new Tuple<AudioSourceType, int>(AudioSourceType.SFX_Impact, 2),
            //        destroySound: null,
            //        layerMask: control.GetVfxLayermask(),
            //        vfxType: VFX_Type.VFX_Downsmash,
            //        vfxColor: Color.white,
            //        timeToLive: 1f,
            //        connectToOriginator: false,
            //        teamfire: false,
            //        deadbodyForceMultiplier: 0,
            //        deadbodyForceMode: ForceMode.Force
            //        );

            //    _vfx.transform.position = control.CharacterMovement.Rigidbody.position;
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 0.09 && control.CharacterMovement.IsGrounded && _cameraShaked == false)
            {
                GameplayCoreManager.Instance.CameraManager.ShakeCamera();

                _cameraShaked = true;
            }

            if (stateInfo.normalizedTime >= 1 && control.CharacterMovement.IsGrounded)
            {
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(_projectile);
            control.isAttacking = false;
            _cameraShaked = false;
        }
    }
}