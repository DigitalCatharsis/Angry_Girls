
using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public abstract class AttackAbility
    {
        public AttackAbilityData LaunchPrepData { get; private set; }
        public AttackAbilityData LaunchFinishData { get; private set; }
        public AttackAbilityData AlternatePrepData { get; private set; }
        public AttackAbilityData AlternateFinishData { get; private set; }

        protected ProjectileManager projectileManager;
        protected VFXManager vFXManager;
        protected CameraManager cameraManager;

        protected AttackAbility(
            AttackAbilityData launchPrep,
            AttackAbilityData launchFinish,
            AttackAbilityData alternatePrep,
            AttackAbilityData alternateFinish)
        {
            LaunchPrepData = launchPrep;
            LaunchFinishData = launchFinish;
            AlternatePrepData = alternatePrep;
            AlternateFinishData = alternateFinish;

            projectileManager = GameplayCoreManager.Instance.ProjectileManager;
            vFXManager = CoreManager.Instance.VFXManager;
            cameraManager = GameplayCoreManager.Instance.CameraManager;
        }

        #region
        // Launch Prep
        public virtual void OnLaunchPrepEnter(CControl control)
        {
            //control.isAttacking = true;
            control.CharacterMovement.ResetVelocity();
            control.CharacterMovement.ApplyRigidForce(new Vector3(0, LaunchPrepData.attackMovementForce.y, LaunchPrepData.attackMovementForce.z * control.transform.forward.z), ForceMode.VelocityChange);
        }
        public virtual void OnLaunchPrepUpdate(CControl control) { }
        public virtual void OnLaunchPrepExit(CControl control) { }
        #endregion

        #region alternate
        // Alternate Prep
        public virtual void OnAlternatePrepEnter(CControl control)
        {
            //control.isAttacking = true;
            control.CharacterMovement.ResetVelocity();
            control.CharacterMovement.ApplyRigidForce(new Vector3(0, LaunchPrepData.attackMovementForce.y, LaunchPrepData.attackMovementForce.z * control.transform.forward.z), ForceMode.VelocityChange);
        }

        public virtual void OnAlternatePrepUpdate(CControl control) { }
        public virtual void OnAlternatePrepExit(CControl control) { }

        // Launch Finish
        public virtual void OnLaunchFinishEnter(CControl control)
        {
            //control.isAttacking = true;
            control.CharacterMovement.ResetVelocity();
            control.CharacterMovement.ApplyRigidForce(new Vector3(0, LaunchFinishData.attackMovementForce.y, LaunchFinishData.attackMovementForce.z * control.transform.forward.z), ForceMode.VelocityChange);
        }
        public virtual void OnLaunchFinishUpdate(CControl control) { }
        public virtual void OnLaunchFinishExit(CControl control) { }

        // Alternate Finish
        public virtual void OnAlternateFinishEnter(CControl control)
        {
            //control.isAttacking = true;
            control.CharacterMovement.ResetVelocity();
            control.CharacterMovement.ApplyRigidForce(new Vector3(0, AlternatePrepData.attackMovementForce.y, AlternatePrepData.attackMovementForce.z * control.transform.forward.z), ForceMode.VelocityChange);
        }
        public virtual void OnAlternateFinishUpdate(CControl control) { }
        public virtual void OnAlternateFinishExit(CControl control)
        {
            //control.isAttacking = true;
            control.CharacterMovement.ResetVelocity();
            control.CharacterMovement.ApplyRigidForce(new Vector3(0, AlternateFinishData.attackMovementForce.y, AlternateFinishData.attackMovementForce.z * control.transform.forward.z), ForceMode.VelocityChange);
        }
        #endregion
    }
}