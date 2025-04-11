using System;
using UnityEngine;

namespace Angry_Girls
{
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
        public Tuple<AudioSourceType, int> spawnSound;
        public Tuple<AudioSourceType, int> destroySound;
    }

    public class Projectile : MonoBehaviour
    {
        private ProjectileConfig _projectileConfig;

        public void OnDispose()
        {
            PlaySound(_projectileConfig.destroySound);
            GameLoader.Instance.interactionManager.Unregister(this.gameObject);
        }

        public void InitProjectile (ProjectileConfig config)
        {
            _projectileConfig = config;

            SetupColliders(config.enableCollider, config.enableTrigger);
            PlaySound(config.spawnSound);

            GameLoader.Instance.interactionManager.Register(this.gameObject, new InteractionConfig
            {
                type = InteractionMemberType.Projectile,
                ownerGO = this.gameObject,
                projectileConfig = config
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) { return; };

            GameLoader.Instance.interactionManager.HandleTrigger(gameObject, other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameLoader.Instance.interactionManager.HandleCollision(gameObject, collision);
        }

        private void PlaySound(Tuple<AudioSourceType, int> sound)
        {
            if (sound != null && sound.Item1 != AudioSourceType.None)
            {
                GameLoader.Instance.audioManager.PlayCustomSound(sound.Item1, sound.Item2);
            }
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
