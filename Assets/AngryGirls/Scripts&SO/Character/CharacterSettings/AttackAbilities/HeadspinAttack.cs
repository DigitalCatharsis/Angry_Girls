using UnityEngine;

namespace Angry_Girls
{
    public class HeadspinAttack : AttackAbility
    {
        public HeadspinAttack(AttackAbilityData launchPrep, AttackAbilityData launchFinish, AttackAbilityData alternatePrep, AttackAbilityData alternateFinish) : base(launchPrep, launchFinish, alternatePrep, alternateFinish) { }
        //alernate
        private bool _haveShootedFirstTime = false;
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
            projectileManager.ProcessFireballs_HeadSpin(control, _firstShoot_ProjectileAngles, control.attackAbility.LaunchPrepData);
        }
        public override void OnLaunchPrepUpdate(CControl control)
        {
            base.OnLaunchPrepUpdate(control);
            PrepUpdate(control, control.attackAbility.LaunchPrepData);
        }
        public override void OnLaunchPrepExit(CControl control)
        {
            base.OnLaunchPrepExit(control);
            control.UnitCallsForStopAttack?.Invoke();
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
            if (control.CharacterMovement.Rigidbody.velocity.y <= -0.2f && !_haveShootedFirstTime)
            {
                PrepUpdate(control, control.attackAbility.AlternatePrepData);
            }
        }
        public override void OnAlternatePrepExit(CControl control)
        {
            base.OnAlternatePrepExit(control);
            control.CharacterMovement.ApplyRigidForce(control.attackAbility.AlternatePrepData.attackMovementForce, ForceMode.VelocityChange);
            projectileManager.ProcessFireballs_HeadSpin(control, _firstShoot_ProjectileAngles, control.attackAbility.AlternatePrepData);
            control.UnitCallsForStopAttack?.Invoke();
        }
        #endregion
        #region 

        private void PrepUpdate(CControl control,AttackAbilityData attackAbilityData)
        {
            //Second cast, second character move
            control.CharacterMovement.ApplyRigidForce(attackAbilityData.attackMovementForce, ForceMode.VelocityChange);
            projectileManager.ProcessFireballs_HeadSpin(control, _secondShoot_ProjectileAngles, attackAbilityData);
            control.isAttacking = false;
        }
        #endregion
    }
}