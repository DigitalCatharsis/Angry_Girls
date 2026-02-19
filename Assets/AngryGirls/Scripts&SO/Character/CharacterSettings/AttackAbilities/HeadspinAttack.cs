using UnityEngine;

namespace Angry_Girls
{
    public class HeadspinAttack : AttackAbility
    {
        public HeadspinAttack(AttackAbilityData launchPrep, AttackAbilityData launchFinish, AttackAbilityData alternatePrep, AttackAbilityData alternateFinish) : base(launchPrep, launchFinish, alternatePrep, alternateFinish) { }

        private int _timesToRepeat_Attack_State = 2;
        private bool _haveShootedSecondTime = false;

        //alternate
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

        #region Launch
        public override void OnLaunchPrepEnter(CControl control)
        {
            base.OnLaunchPrepEnter(control);
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
        public override void OnLaunchPrepUpdate(CControl control)
        {
            base.OnLaunchPrepUpdate(control);
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

        public override void OnLaunchPrepExit(CControl control)
        {
            base.OnLaunchPrepExit(control);

            _haveShootedSecondTime = false;
            control.isAttacking = false;
        }
        #endregion

        #region Alternate
        public override void OnAlternatePrepEnter(CControl control)
        {
            base.OnAlternatePrepEnter(control);
        }

        public override void OnAlternatePrepUpdate(CControl control)
        {
            base.OnAlternatePrepUpdate(control);
            //control.CheckAttackFinishCondition();

            if (control.CharacterMovement.Rigidbody.velocity.y <= -0.2f && !_haveShootedFirstTime)
            {
                //Second cast, second character move
                control.CharacterMovement.ApplyRigidForce(control.CharacterSettings.AttackAbility_Alternate.attackMovementForce, ForceMode.VelocityChange);
                GameplayCoreManager.Instance.ProjectileManager.ProcessFireballs_HeadSpin(control, _firstShoot_ProjectileAngles);
                _haveShootedFirstTime = true;
                control.isAttacking = false;
            }
        }
        public override void OnAlternatePrepExit(CControl control)
        {
            base.OnAlternatePrepExit(control);
            control.CharacterMovement.ApplyRigidForce(control.CharacterSettings.AttackAbility_Alternate.attackMovementForce, ForceMode.VelocityChange);
            GameplayCoreManager.Instance.ProjectileManager.ProcessFireballs_HeadSpin(control, _firstShoot_ProjectileAngles);
            _haveShootedFirstTime = false;
            control.isAttacking = false;
        }
        #endregion
    }
}