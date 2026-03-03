using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Configuration data for projectiles
    /// </summary>
    [Serializable]
    public class ProjectileConfig
    {
        public Transform projectileSpawnTransform;
        public VFXConfig VFXConfig;
        public bool destroyOnCollision;
        public bool destroyOnCharacterTrigger;
        public bool teamfire;
        public float damage;
        public bool enableCollider;
        public bool enableTrigger;
        public float enemyKnockBackValue;
        public AudioClipData[] spawnSound;
        public AudioClipData[] destroySound;
        public float deadbodyForceMultiplier = 1f;
        public ForceMode deadbodyForceMode;
    }

    /// <summary>
    /// Represents a projectile that can be launched
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private ProjectileConfig _projectileConfig;

        /// <summary>
        /// Cleanup when projectile is disposed
        /// </summary>
        public void OnDispose()
        {
            PlaySound(_projectileConfig.destroySound?[0]);
            GameplayCoreManager.Instance.InteractionManager.Unregister(this.gameObject);
        }

        /// <summary>
        /// Initialize projectile with configuration
        /// </summary>
        public void InitProjectile(ProjectileConfig config)
        {
            _projectileConfig = config;

            GameplayCoreManager.Instance.InteractionManager.Register(this.gameObject, new InteractionConfig
            {
                type = InteractionMemberType.Projectile,
                ownerGO = this.gameObject,
                projectileConfig = config
            });

            SetupColliders(config.enableCollider, config.enableTrigger);
            PlaySound(config.spawnSound?[0]);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) { return; }
            ;

            GameplayCoreManager.Instance.InteractionManager.HandleTrigger(gameObject, other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameplayCoreManager.Instance.InteractionManager.HandleCollision(gameObject, collision);
        }

        private void PlaySound(AudioClipData data)
        {
            CoreManager.Instance.AudioManager.PlayClipData(data, data.fallbackCategory, false);
        }

        private void SetupColliders(bool enableCollider, bool enableTrigger)
        {
            foreach (var collider in GetComponents<Collider>())
            {
                collider.enabled = collider.isTrigger ? enableTrigger : enableCollider;
            }
        }
    }
}