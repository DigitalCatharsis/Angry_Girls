using UnityEngine;

namespace Angry_Girls
{
    public class SwordSpinAttack : AttackAbility
    {
        public SwordSpinAttack(AttackAbilityData launchPrep, AttackAbilityData launchFinish, AttackAbilityData alternatePrep, AttackAbilityData alternateFinish) : base(launchPrep, launchFinish, alternatePrep, alternateFinish) { }
        private float _timeInCurrentLoop;
        private int _timesToRepeat_Attack_State = 6;
        private GameObject _projectile;
        private bool _hasEnteredAttackState = false;

        // Sound settings: normalizedTime moments when they should play (e.g., 0.0 - start, 0.5 - middle)
        private readonly float[] _soundTriggers = { 0.0f, 0.5f }; // Can be changed to {0.0f, 0.3f, 0.7f}, etc.
        private bool[] _hasPlayedSoundInThisCycle; // Tracks which sounds have already played

        #region Launch
        public override void OnLaunchPrepEnter(CControl control)
        {
            base.OnLaunchPrepEnter(control);
            PrepEnter(control, control.attackAbility.LaunchPrepData);
        }
        public override void OnLaunchPrepUpdate(CControl control)
        {
            base.OnLaunchPrepUpdate(control);
            UpdateEnter(control);
        }
        public override void OnLaunchPrepExit(CControl control)
        {
            base.OnLaunchPrepExit(control);
        }
        #endregion

        #region Alternate
        public override void OnAlternatePrepEnter(CControl control)
        {
            base.OnLaunchPrepEnter(control);
            PrepEnter(control, control.attackAbility.AlternatePrepData);
        }
        public override void OnAlternatePrepUpdate(CControl control)
        {
            base.OnAlternatePrepUpdate(control);
            UpdateEnter(control);
        }


        public override void OnAlternatePrepExit(CControl control)
        {
            base.OnAlternatePrepExit(control);
            PrepExit(control);
        }
        #endregion

        #region private
        private void PrepEnter(CControl control, AttackAbilityData attackAbilityData)
        {
            _projectile = projectileManager.SpawnByProjectileAbilityData(control, attackAbilityData);

            if (!_hasEnteredAttackState)
            {
                audioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                _hasEnteredAttackState = true;
            }

            // Initialize the array for tracking sounds in this cycle
            _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
        }
        private void UpdateEnter(CControl control)
        {
            var stateInfo = control.GetAnimatorStateInfo();
            float normalizedTime = stateInfo.normalizedTime % 1; // Current position in the cycle [0, 1)

            // Check all sound triggers
            for (int i = 0; i < _soundTriggers.Length; i++)
            {
                // If the trigger has passed and the sound has not yet played
                if (normalizedTime >= _soundTriggers[i] && !_hasPlayedSoundInThisCycle[i])
                {
                    audioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                    _hasPlayedSoundInThisCycle[i] = true;
                }
            }

            // If the loop has completed (normalizedTime jumped from ~1.0 to 0.0)
            if (normalizedTime < _timeInCurrentLoop)
            {
                // Reset flags for a new loop
                _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
            }

            _timeInCurrentLoop = normalizedTime;

            // Check if the entire animation has completed
            if (stateInfo.normalizedTime >= _timesToRepeat_Attack_State)
            {
                _timeInCurrentLoop = 0f;
                _hasEnteredAttackState = false;
                projectileManager.DisposeProjectile(_projectile);
                control.UnitCallsForStopAttack?.Invoke();
            }
        }
        private void PrepExit(CControl control)
        {
            if (_projectile != null)
                GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(_projectile);
            control.UnitCallsForStopAttack?.Invoke();
        }
        #endregion
    }
}