using UnityEngine;

namespace Angry_Girls
{
    public class HeadspinAttack : AttackAbility
    {
        public HeadspinAttack(AttackAbilityData launchPrep, AttackAbilityData launchFinish, AttackAbilityData alternatePrep, AttackAbilityData alternateFinish) : base(launchPrep, launchFinish, alternatePrep, alternateFinish) { }

        private bool _firstShotFired = false;
        private float _firstShotTime;
        private bool _exited = false;
        private float _delayBetweenShots = 0.5f; // can be made customizable 
                                                 // Angle arrays (can be left as is)
        private Vector3[] _firstShoot_ProjectileAngles =
            {
            new Vector3(170f,0,0),
            new Vector3(200f,0,0),
            new Vector3(230f,0,0),
            new Vector3(170f,-180f,0),
            new Vector3(200f,-180f,0),
            new Vector3(230f,-180f,0)
            };

        private Vector3[] _secondShoot_ProjectileAngles =
            {
            new Vector3(-205f,0,0),
            new Vector3(-225f,0,0),
            new Vector3(-270f,0,0),
            new Vector3(-305f,0,0),
            new Vector3(-325f,0,0),
            };
        #region launch
        public override void OnLaunchPrepEnter(CControl control)
        {
            base.OnLaunchPrepEnter(control);
            EnterPrep(control, control.attackAbility.LaunchPrepData);
        }

        public override void OnLaunchPrepUpdate(CControl control)
        {
            base.OnLaunchPrepUpdate(control);
            UpdatePrep(control, control.attackAbility.LaunchPrepData);
        }

        public override void OnLaunchPrepExit(CControl control)
        {
            base.OnLaunchPrepExit(control);
            ExitPrep(control);
        }
        #endregion

        #region Alternate
        public override void OnAlternatePrepEnter(CControl control)
        {
            base.OnAlternatePrepEnter(control);
            EnterPrep(control, control.attackAbility.AlternatePrepData);
        }

        public override void OnAlternatePrepUpdate(CControl control)
        {
            base.OnAlternatePrepUpdate(control);
            UpdatePrep(control, control.attackAbility.AlternatePrepData);
        }

        public override void OnAlternatePrepExit(CControl control)
        {
            base.OnAlternatePrepExit(control);
            ExitPrep(control);
        }
        #endregion

        #region Private Methods 
        private void EnterPrep(CControl control, AttackAbilityData data)
        {
            // Toss the character 
            control.CharacterMovement.ApplyRigidForce(data.attackMovementForce, ForceMode.VelocityChange);
            _firstShotFired = false;
            _exited = false;
        }

        private void UpdatePrep(CControl control, AttackAbilityData data)
        {
            if (_exited) return;

            float verticalVelocity = control.CharacterMovement.Rigidbody.velocity.y;

            // Wait for the peak altitude (velocity <= 0) for the first shot
            if (!_firstShotFired && verticalVelocity <= 0f)
            {
                projectileManager.ProcessFireballs_HeadSpin(control, _firstShoot_ProjectileAngles, data);
                _firstShotFired = true;
                _firstShotTime = Time.time;
            }
            // After the first shot, wait for the delay for the second
            else if (_firstShotFired && !_exited && Time.time >= _firstShotTime + _delayBetweenShots)
            {
                projectileManager.ProcessFireballs_HeadSpin(control, _secondShoot_ProjectileAngles, data);
                _exited = true;
                control.UnitCallsForStopAttack?.Invoke(); // end Prep
            }
        }

        private void ExitPrep(CControl control)
        {
        }
        #endregion
    }
}