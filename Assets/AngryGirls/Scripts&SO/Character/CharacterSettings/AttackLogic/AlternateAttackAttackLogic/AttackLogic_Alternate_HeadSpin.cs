using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Alternate_HeadSpin : AttackAbility
    {
        public AttackLogic_Alternate_HeadSpin(AttackAbilityData attackAbilityData): base(attackAbilityData) { }
        private bool _haveShootedFirstTime = false;


       private Vector3[] _firstShoot_ProjectileAngles = {
                  new Vector3(170f,0,0),
                  new Vector3(200f,0,0),
                  new Vector3(230f,0,0),
                  new Vector3(170f,-180f,0),
                  new Vector3(200f,-180f,0),
                  new Vector3(230f,-180f,0)
            };

        private Vector3[] _secondShoot_ProjectileAngles = {
                  new Vector3(-205f,0,0),
                  new Vector3(-225f,0,0),
                  new Vector3(-270f,0,0),
                  new Vector3(-305f,0,0),
                  new Vector3(-325f,0,0),
            };

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            //control.CheckAttackFinishCondition();

            if (control.CharacterMovement.Rigidbody.velocity.y <= - 0.2f && !_haveShootedFirstTime)            
            {
                //Second cast, second character move
                control.CharacterMovement.ApplyRigidForce(control.CharacterSettings.AttackAbility_Alternate.attackMovementForce, ForceMode.VelocityChange);
                GameplayCoreManager.Instance.ProjectileManager.ProcessFireballs_HeadSpin(control, _firstShoot_ProjectileAngles);
                _haveShootedFirstTime = true;
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.CharacterMovement.ApplyRigidForce(control.CharacterSettings.AttackAbility_Alternate.attackMovementForce, ForceMode.VelocityChange);
            GameplayCoreManager.Instance.ProjectileManager.ProcessFireballs_HeadSpin(control, _firstShoot_ProjectileAngles);
            _haveShootedFirstTime = false;
            control.isAttacking = false;
        }
    }
}