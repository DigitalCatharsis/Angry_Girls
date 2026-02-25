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
            //control.isAttacking = true;
            control.CharacterMovement.ResetVelocity();
            control.CharacterMovement.ApplyRigidForce(new Vector3(0, LaunchPrepData.attackMovementForce.y, LaunchPrepData.attackMovementForce.z * control.transform.forward.z), ForceMode.VelocityChange);
        }
        public virtual void OnAlternatePrepUpdate(CControl control) { }
        public virtual void OnAlternatePrepExit(CControl control) { }
        #endregion
    }
}