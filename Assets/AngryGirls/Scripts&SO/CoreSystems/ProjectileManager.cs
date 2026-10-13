using DG.Tweening;
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
                spawnSound = ability.spawnAudioData,
                destroySound = ability.destroyAudioData,
                deadbodyForceMultiplier = ability.deadbodyForceMultiplier,
                deadbodyForceMode = ability.deadbodyForceMode,
                deadbodyDirectionMode = ability.deadbodyDirectionMode,
                deadbodyVerticalBias = ability.deadbodyVerticalBias
            };

            var vfxGameobject = CoreManager.Instance.VFXManager.SpawnVFX(projectileConfig.VFXConfig);
            if (vfxGameobject != null)
            {
                var proj = vfxGameobject.GetComponent<Projectile>();
                proj.InitProjectile(projectileConfig);
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
            AudioClipData[] spawnSound,
            AudioClipData[] destroySound,
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

            var alignment = projectile.GetComponent<XYProjectileFlightAlignment>();
            if (alignment != null)
                alignment.Stop();

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
            const float impulseY = 7f;
            const float impulseHorizontal = 5f;

            if (control == null || control.projectileSpawnTransform == null || angles == null || attackAbilityData == null)
                return;

            for (int i = 0; i < angles.Length; i++)
            {
                var projectile = SpawnByProjectileAbilityData(control, attackAbilityData);
                if (projectile == null)
                    continue;

                // Existing serialized angle presets were authored in the legacy YZ plane.
                // Convert their forward vector once into the XY gameplay plane.
                Vector3 legacyDirection = Quaternion.Euler(angles[i]) * Vector3.forward;
                Vector3 direction = new Vector3(legacyDirection.z, legacyDirection.y, 0f).normalized;

                if (direction.sqrMagnitude <= 0.0001f)
                {
                    DisposeProjectile(projectile);
                    continue;
                }

                var rigidbody = projectile.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    DisposeProjectile(projectile);
                    continue;
                }

                projectile.transform.DOKill();
                projectile.transform.position = control.projectileSpawnTransform.position;
                projectile.transform.rotation = Quaternion.LookRotation(direction, Vector3.forward);

                ConfigureXYProjectileRigidbody(rigidbody);
                rigidbody.useGravity = true;

                var impulse = new Vector3(
                    impulseHorizontal * direction.x,
                    impulseY * direction.y,
                    0f);

                rigidbody.AddForce(impulse, ForceMode.VelocityChange);

                var alignment = projectile.GetComponent<XYProjectileFlightAlignment>();
                if (alignment == null)
                    alignment = projectile.AddComponent<XYProjectileFlightAlignment>();

                alignment.Begin(rigidbody, direction);
            }
        }

        /// <summary>
        /// Launches a fireball forward on the XY gameplay plane.
        /// Projectile speed can come from current X data or legacy Z data.
        /// </summary>
        public void SendFireballFront(CControl control, Vector3 startPoint, Vector3 targetEuler, float attackAngleChangeValue, AttackAbilityData attackAbilityData, float rotationDuration = 1.5f)
        {
            if (control == null || attackAbilityData == null)
                return;

            var projectile = SpawnByProjectileAbilityData(control, attackAbilityData);
            if (projectile == null)
                return;

            var rigidbody = projectile.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                DisposeProjectile(projectile);
                return;
            }

            projectile.transform.position = startPoint;

            float facingX = control.CharacterMovement.Rigidbody.transform.forward.x;
            if (Mathf.Abs(facingX) <= 0.001f)
                facingX = control.playerOrAi == PlayerOrAi.Player ? 1f : -1f;

            float facingSign = Mathf.Sign(facingX);
            float initialAngle = attackAngleChangeValue * 4f;
            Vector3 initialDirection = new Vector3(
                facingSign * Mathf.Cos(initialAngle * Mathf.Deg2Rad),
                -Mathf.Sin(initialAngle * Mathf.Deg2Rad),
                0f).normalized;

            projectile.transform.DOKill();
            projectile.transform.rotation = Quaternion.LookRotation(initialDirection, Vector3.forward);

            ConfigureXYProjectileRigidbody(rigidbody);

            float targetAngle = targetEuler.x;
            Vector3 targetDirection = new Vector3(
                facingSign * Mathf.Cos(targetAngle * Mathf.Deg2Rad),
                -Mathf.Sin(targetAngle * Mathf.Deg2Rad),
                0f).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.forward);
            projectile.transform.DORotateQuaternion(targetRotation, rotationDuration).SetEase(Ease.Linear);

            var impulse = new Vector3(
                attackAbilityData.GetProjectileHorizontalSpeed() * initialDirection.x,
                attackAbilityData.projectileMovementSpeed.y * initialDirection.y - attackAngleChangeValue,
                0f);

            rigidbody.AddForce(impulse, ForceMode.VelocityChange);

            // Apply the configured character recoil on XY rather than along the old Z axis.
            control.CharacterMovement.ApplyRigidForce(
                attackAbilityData.GetAttackMovementForceXY(control.transform),
                ForceMode.VelocityChange);
        }

        private static void ConfigureXYProjectileRigidbody(Rigidbody rigidbody)
        {
            if (rigidbody == null)
                return;

            rigidbody.constraints =
                (rigidbody.constraints &
                 ~(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY)) |
                RigidbodyConstraints.FreezePositionZ;

            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        public GameObject SpawnDownSmash(CControl control, AttackAbilityData data ,float enemyKnockBackValue)
        {
            var proj = SpawnProjectile
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
                enemyKnockBackValue: enemyKnockBackValue,
                enableCollider: false,
                enableTrigger: true,
                spawnSound: data.spawnAudioData,
                destroySound: data.destroyAudioData,
                deadbodyForceMultiplier: 0,
                deadbodyForceMode: ForceMode.Force
                );

            proj.transform.position = control.transform.position;
            return proj;
        }

        #endregion
    }
}
