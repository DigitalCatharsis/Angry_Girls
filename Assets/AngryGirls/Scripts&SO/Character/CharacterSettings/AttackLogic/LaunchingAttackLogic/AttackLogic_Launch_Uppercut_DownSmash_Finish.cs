//using UnityEngine;

//namespace Angry_Girls
//{
//    public class AttackLogic_Launch_UppercutDownSmash_Finish : AttackAbility
//    {
//        public AttackLogic_Launch_UppercutDownSmash_Finish(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

//        private bool _cameraShaked = false;

//        private GameObject _projectile;

//        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
//        {
//            var ability = control.CharacterSettings.AttackAbility_Launch;

//            _projectile = GameplayCoreManager.Instance.ProjectileManager.SpawnDownSmash(control, ability);
//        }

//        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
//        {
//            if (stateInfo.normalizedTime >= 0.09 && control.CharacterMovement.IsGrounded && _cameraShaked == false)
//            {
//                GameplayCoreManager.Instance.CameraManager.ShakeCamera();

//                _cameraShaked = true;
//            }

//            if (stateInfo.normalizedTime >= 1 && control.CharacterMovement.IsGrounded)
//            {
//                control.UnitCallsForStopAttackfinish?.Invoke();
//            }
//        }

//        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
//        {
//            GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(_projectile);
//            _cameraShaked = false;
//        }
//    }
//}