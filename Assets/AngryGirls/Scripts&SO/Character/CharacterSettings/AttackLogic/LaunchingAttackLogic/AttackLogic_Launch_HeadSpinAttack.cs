using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_HeadSpinAttack : AttackAbility
    {
        public AttackLogic_Launch_HeadSpinAttack(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private int _timesToRepeat_Attack_State = 2;
        private bool _haveShootedSecondTime = false;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);

            Vector3[] angles = {
                  new Vector3(170f,0,0),
                  new Vector3(200f,0,0),
                  new Vector3(230f,0,0),
                  new Vector3(170f,-180f,0),
                  new Vector3(200f,-180f,0),
                  new Vector3(230f,-180f,0)
            };

            GameplayCoreManager.Instance.ProjectileManager.ProcessFireballs_HeadSpin(control, angles);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (!_haveShootedSecondTime && stateInfo.normalizedTime >= _timesToRepeat_Attack_State)
            {
                Vector3[] angles = {
                  new Vector3(-205f,0,0),
                  new Vector3(-225f,0,0),
                  new Vector3(-270f,0,0),
                  new Vector3(-305f,0,0),
                  new Vector3(-325f,0,0),
            };

                //Second cast, second character move
                control.CharacterMovement.ApplyRigidForce(control.profile.CharacterSettings.AttackAbility_Launch.attackMovementForce, ForceMode.VelocityChange);
                GameplayCoreManager.Instance.ProjectileManager.ProcessFireballs_HeadSpin(control, angles);
                _haveShootedSecondTime = true;
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _haveShootedSecondTime = false;
            control.isAttacking = false;
        }
    }
}