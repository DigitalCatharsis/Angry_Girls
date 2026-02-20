using UnityEngine;

namespace Angry_Girls
{
    public class FireballAttack : AttackAbility
    {
        public FireballAttack(AttackAbilityData launchPrep, AttackAbilityData launchFinish, AttackAbilityData alternatePrep, AttackAbilityData alternateFinish) : base(launchPrep, launchFinish, alternatePrep, alternateFinish) { }

        private float _timeInCurrentLoop;
        private bool _fireballSentThisLoop = false;
        private int _timesToRepeat_Attack_State = 3;
        private float _spawnProjectile_TransitionOffset = 0.4f;
        private float _attackAngleChangeValue = 0f;

        #region Launch
        public override void OnLaunchPrepEnter(CControl control)
        {
            base.OnLaunchPrepEnter(control);
            PrepEnter(control);
        }
        public override void OnLaunchPrepUpdate(CControl control)
        {
            base.OnLaunchPrepUpdate(control);
            PrepUpdate(control, control.attackAbility.LaunchPrepData);
        }
        public override void OnLaunchPrepExit(CControl control)
        {
            base.OnLaunchPrepExit(control);
            PrepExit(control);
        }
        #endregion

        #region Alternate
        public override void OnAlternatePrepEnter(CControl control)
        {
            base.OnAlternatePrepEnter(control);
            PrepEnter(control);
        }
        public override void OnAlternatePrepUpdate(CControl control)
        {
            base.OnAlternatePrepUpdate(control);
            PrepUpdate(control, control.attackAbility.AlternatePrepData);
        }
        public override void OnAlternatePrepExit(CControl control)
        {
            base.OnAlternatePrepExit(control);
            PrepExit(control);
        }
        #endregion

        #region Private
        private void PrepEnter(CControl control)
        {
            control.CharacterMovement.Rigidbody.isKinematic = false;

            _timeInCurrentLoop = 0f;
            _fireballSentThisLoop = false;

            control.CharacterMovement.Rigidbody.useGravity = false;
            _attackAngleChangeValue = 0f;
        }
        private void PrepUpdate(CControl control, AttackAbilityData attackAbilityData)
        {
            _timeInCurrentLoop += Time.deltaTime;

            // Determine the target rotation along the X and Y axes
            var targetEuler = control.CharacterMovement.Rigidbody.transform.forward.z > 0
            ? new Vector3(45, 0, 0) // Rotate downwards
            : new Vector3(45, 180, 0); // Rotate upward

            var stateInfo = control.GetAnimatorStateInfo();
            // Check for 40% in the CURRENT animation loop
            if (!_fireballSentThisLoop && _timeInCurrentLoop / stateInfo.length >= _spawnProjectile_TransitionOffset)
            {

                projectileManager.SendFireballFront(control, control.projectileSpawnTransform.position, targetEuler, _attackAngleChangeValue, attackAbilityData);
                _fireballSentThisLoop = true;
                _attackAngleChangeValue += 4;
            }

            // Check for the end of ALL animation loops
            if (stateInfo.normalizedTime >= _timesToRepeat_Attack_State * stateInfo.length + 0.8f)
            {
                control.UnitCallsForStopAttack();
                _timeInCurrentLoop = 0f;
            }

            //Reset the flag at the end of the loop
            if (_timeInCurrentLoop >= stateInfo.length)
            {
                _timeInCurrentLoop -= stateInfo.length;
                _fireballSentThisLoop = false;
            }
        }
        private void PrepExit(CControl control)
        {
            _attackAngleChangeValue = 0f;
            control.CharacterMovement.ResetVelocity();
            control.CharacterMovement.Rigidbody.isKinematic = true;
        }
        #endregion
    }
}