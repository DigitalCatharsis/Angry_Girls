using DG.Tweening;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages projectile lifecycle, physics and collision logic.
    /// Separated from VFXManager to respect SRP.
    /// </summary>
    public class ProjectileManager : GameplayManagerClass
    {
        public override void Initialize()
        {
            isInitialized = true;
        }

        /// <summary>
        /// Spawns projectile based on character's current ability
        /// </summary>
        public GameObject SpawnByProjectileAbilityData(CControl control, AttackAbilityData ability)
        {
            var projectileConfig = new ProjectileConfig
            {
                VFXConfig = new VFXConfig
                {
                    spawnPosition = control.projectileSpawnTransform.position,
                    Vfxtype = ability.vfxType,
                    color = ability.vfxColor,
                    originator = control.gameObject,
                    timeToLive = ability.timeToLive,
                    layermask = control.GetVfxLayermask(),
                    connectToOriginator = ability.connectToOwner,
                },
                projectileSpawnTransform = control.projectileSpawnTransform,
                enemyKnockBackValue = ability.enemyKnockbackValue,
                destroyOnCollision = ability.destroyOnCollision,
                destroyOnCharacterTrigger = ability.destroyOnCharacterCollision,
                damage = control.profile.GetCurrentStats.damage,
                enableCollider = ability.enableCollider,
                enableTrigger = ability.enableTrigger,
                teamfire = ability.teamfire,
                spawnSound = new Tuple<AudioSourceType, int>(ability.spawnSourceType, ability.spawnIndex),
                destroySound = new Tuple<AudioSourceType, int>(ability.destroySourceType, ability.destoyIndex),
                deadbodyForceMultiplier = ability.deadbodyForceMultiplier,
                deadbodyForceMode = ability.deadbodyForceMode
            };

            var vfxGameobject = CoreManager.Instance.VFXManager.SpawnVFX(projectileConfig.VFXConfig);
            if (vfxGameobject != null)
            {
                vfxGameobject.GetComponent<Projectile>().InitProjectile(projectileConfig);
            }
            return vfxGameobject;
        }

        /// <summary>
        /// Spawns projectile with custom parameters
        /// </summary>
        public GameObject SpawnProjectile(
            Transform spawnTransform,
            VFX_Type vfxType,
            Color vfxColor,
            GameObject originator,
            LayerMask layerMask,
            float timeToLive,
            bool connectToOriginator,
            float enemyKnockBackValue,
            bool destroyOnCollision,
            bool destroyOnCharacterTrigger,
            float attackDamageValue,
            bool enableCollider,
            bool enableTrigger,
            bool teamfire,
            Tuple<AudioSourceType, int> spawnSound,
            Tuple<AudioSourceType, int> destroySound,
            float deadbodyForceMultiplier,
            ForceMode deadbodyForceMode)
        {
            var projectileConfig = new ProjectileConfig
            {
                VFXConfig = new VFXConfig
                {
                    Vfxtype = vfxType,
                    color = vfxColor,
                    originator = originator,
                    timeToLive = timeToLive,
                    layermask = layerMask,
                    connectToOriginator = connectToOriginator,
                },
                projectileSpawnTransform = spawnTransform,
                enemyKnockBackValue = enemyKnockBackValue,
                destroyOnCollision = destroyOnCollision,
                destroyOnCharacterTrigger = destroyOnCharacterTrigger,
                damage = attackDamageValue,
                enableCollider = enableCollider,
                enableTrigger = enableTrigger,
                teamfire = teamfire,
                spawnSound = spawnSound,
                destroySound = destroySound,
                deadbodyForceMultiplier = deadbodyForceMultiplier,
                deadbodyForceMode = deadbodyForceMode
            };

            var vfxGameobject = CoreManager.Instance.VFXManager.SpawnVFX(projectileConfig.VFXConfig);
            if (vfxGameobject != null)
            {
                vfxGameobject.GetComponent<Projectile>().InitProjectile(projectileConfig);
            }
            return vfxGameobject;
        }

        /// <summary>
        /// Cleanup projectile and return to pool
        /// </summary>
        public void DisposeProjectile(GameObject projectile)
        {
            if (projectile == null) return;

            var proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.OnDispose();
            }
            CoreManager.Instance.VFXManager.CallVFXDispose(projectile);
        }

        //private AttackAbilityData GetCurrentAbility(CControl control)
        //{
        //    return GameplayCoreManager.Instance.PhaseFlowController.CurrentGameState == GameState.AlternatePhase
        //        ? control.CharacterSettings.AttackAbility_Alternate
        //        : control.CharacterSettings.AttackAbility_Launch;
        //}

        #region projectile templates

        /// <summary>
        /// Processes fireballs for head spin attack with specified angles
        /// </summary>
        /// red
        public void ProcessFireballs_HeadSpin(CControl control, Vector3[] angles, AttackAbilityData attackAbilityData, float moveDuration = 1.5f)
        {
            var _impulseY = 7f;
            var _impulseZ = 5f;
            var _finalProjectileRotation = new Vector3(75f, 0, 0);

            for (var i = 0; i < angles.Length; i++)
            {
                var projectile = SpawnByProjectileAbilityData(control, attackAbilityData);
                if (projectile == null) continue;

                projectile.transform.position = control.projectileSpawnTransform.position;
                projectile.transform.rotation = Quaternion.Euler(angles[i]);

                var finalRotationDegree = _finalProjectileRotation;
                if (Mathf.Sign(projectile.transform.forward.z) < 0)
                {
                    finalRotationDegree.y += 180f;
                }

                var impulse = new Vector3(
                    0,
                    _impulseY * projectile.transform.forward.y,
                    _impulseZ * projectile.transform.forward.z
                );

                projectile.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);
                projectile.transform.DORotate(
                    endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * projectile.transform.forward.z),
                    duration: moveDuration,
                    mode: RotateMode.Fast
                );
            }
        }

        //green
        public void SendFireballFront(CControl control, Vector3 startPoint, Vector3 targetEuler, float attackAngleChangeValue, AttackAbilityData attackAbilityData, float rotationDuration = 1.5f)
        {
            // Spawn fireball
            var projectile = SpawnByProjectileAbilityData(control, attackAbilityData);

            // Set the initial position
            projectile.transform.position = startPoint;

            // Determine the initial rotation based on the control direction
            float initialYRotation = control.CharacterMovement.Rigidbody.transform.forward.z > 0 ? 0f : 180f;
            var initialRotation = Quaternion.Euler(
            attackAngleChangeValue * 4, // X-axis tilt
            initialYRotation, // Y-axis direction (0 or 180)
            0 // Z-axis tilt (not used)
            );

            // Set the initial direction taking into account the correction
            projectile.transform.rotation = initialRotation;

            // Define the target rotation angle only along the X axis
            var targetEulerAngles = new Vector3(
            targetEuler.x, // Final angle along the X axis
            initialYRotation, // Leave Y unchanged
            0 // Leave Z unchanged
            );

            // Apply rotation using DOTween (only on the X axis)
            projectile.transform.DORotate(targetEulerAngles, rotationDuration, RotateMode.Fast);

            // Set the impulse
            var impulse = new Vector3(
            0,
            attackAbilityData.projectileMovementSpeed.y * projectile.transform.forward.y - attackAngleChangeValue,
            attackAbilityData.projectileMovementSpeed.z * projectile.transform.forward.z
            );

            projectile.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);

            // Apply force to control.rigidBody 
            control.CharacterMovement.ApplyRigidForce(attackAbilityData.attackMovementForce * control.CharacterMovement.Rigidbody.transform.forward.z, ForceMode.VelocityChange);
        }

        public GameObject SpawnDownSmash(CControl control, AttackAbilityData ability)
        {
            var proj = GameplayCoreManager.Instance.ProjectileManager.SpawnProjectile
                (
                teamfire: false,
                spawnTransform: control.projectileSpawnTransform,
                originator: control.gameObject,
                layerMask: control.GetVfxLayermask(),
                vfxType: VFX_Type.PRE_Projectile_Downsmash,
                vfxColor: Color.white,
                timeToLive: 1f,
                connectToOriginator: false,
                destroyOnCollision: false,
                destroyOnCharacterTrigger: false,
                attackDamageValue: control.profile.GetCurrentStats.damage,
                enemyKnockBackValue: ability.enemyKnockbackValue,
                enableCollider: false,
                enableTrigger: true,
                spawnSound: new Tuple<AudioSourceType, int>(AudioSourceType.SFX_Impact, 2),
                destroySound: null,
                deadbodyForceMultiplier: 0,
                deadbodyForceMode: ForceMode.Force
                );

            proj.transform.position = control.transform.position;
            return proj;
        }

        #endregion
    }
}