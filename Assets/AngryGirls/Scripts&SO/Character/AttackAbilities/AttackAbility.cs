using UnityEngine;

namespace Angry_Girls
{
    public abstract class AttackAbility
    {
        public AttackAbilityData LaunchPrepData { get; private set; }
        public AttackAbilityData AlternatePrepData { get; private set; }
        protected ProjectileManager projectileManager;
        protected VFXManager vFXManager;
        protected CameraManager cameraManager;
        protected AudioManager audioManager;

        protected AttackAbility(AttackAbilityData launchPrep,AttackAbilityData alternatePrep)
        {
            LaunchPrepData = launchPrep;
            AlternatePrepData = alternatePrep;

            projectileManager = GameplayCoreManager.Instance.ProjectileManager;
            vFXManager = CoreManager.Instance.VFXManager;
            cameraManager = GameplayCoreManager.Instance.CameraManager;
            audioManager = CoreManager.Instance.AudioManager;
        }

        #region Launch Prep
        // Launch Prep
        public virtual void OnLaunchPrepEnter(CControl control)
        {
        }
        public virtual void OnLaunchPrepUpdate(CControl control) { }
        public virtual void OnLaunchPrepExit(CControl control) { }
        #endregion

        #region Alternate
        // Alternate Prep
        public virtual void OnAlternatePrepEnter(CControl control)
        {
            if (control == null || control.CharacterMovement == null)
                return;

            control.CharacterMovement.ResetVelocity();

            if (AlternatePrepData != null)
            {
                control.CharacterMovement.ApplyRigidForce(
                    AlternatePrepData.GetAttackMovementForceXY(control.transform),
                    ForceMode.VelocityChange);
            }
        }
        public virtual void OnAlternatePrepUpdate(CControl control) { }
        public virtual void OnAlternatePrepExit(CControl control) { }
        #endregion
    }
}
