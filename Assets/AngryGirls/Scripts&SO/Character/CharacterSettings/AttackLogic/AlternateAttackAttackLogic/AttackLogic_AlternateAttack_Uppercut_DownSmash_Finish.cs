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
            var ability = control.profile.CharacterSettings.AttackAbility_Alternate;
            _projectile = GameplayCoreManager.Instance.ProjectileManager.SpawnDownSmash(control, ability);

            _projectile.transform.position = control.CharacterMovement.Rigidbody.position;
        }



        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= 0.14 && control.CharacterMovement.IsGrounded && _cameraShaked == false)
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