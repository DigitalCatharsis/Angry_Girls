using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public enum InteractionMemberType
    {
        Character,
        Projectile,
        Pickable
    }

    public enum InteractionPhysicType
    {
        Trigger,
        Collision
    }

    /// <summary>
    /// Configuration for interaction source
    /// </summary>
    public struct InteractionConfig
    {
        public GameObject ownerGO;
        public InteractionMemberType type;
        public ProjectileConfig projectileConfig;
    }

    /// <summary>
    /// Data for interaction between objects
    /// </summary>
    public struct InteractionData
    {
        public GameObject source;
        public GameObject target;
        public InteractionPhysicType physicType;
        public Collider targetCollider;
        public Collision collision;
    }

    /// <summary>
    /// Interface for handling interactions between objects
    /// </summary>
    public interface IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig);
        public void Handle(InteractionData data, InteractionConfig sourceConfig);
    }

    /// <summary>
    /// Manages interactions between game objects (collisions and triggers)
    /// </summary>
    public class InteractionManager : GameplayManagerClass
    {
        private Dictionary<GameObject, InteractionConfig> _registeredObjects = new Dictionary<GameObject, InteractionConfig>();
        private List<IInteractionHandler> _handlers = new List<IInteractionHandler>();
        private IInteractionHandler _defaultHandler = new DefaultInteractionHandler();
        private HashSet<(GameObject, GameObject)> _processedPairs = new HashSet<(GameObject, GameObject)>();

        private int _currentFrame;

        public override void Initialize()
        {
            // Register all interaction handlers
            AddHandler(new ProjectileCharacter_TriggerHandler());
            AddHandler(new ProjectilePickable_TriggerHandler());
            AddHandler(new Projectile_CollisionHandler());
            AddHandler(new CharacterPickable_TriggerHandler());
            AddHandler(new CharacterCharacter_CollisionHandler());
            AddHandler(new ProjectileDeathzone_TriggerHandler());
            AddHandler(new CharacterDeathzone_TriggerHandler());
            isInitialized = true;
        }

        public void AddHandler(IInteractionHandler handler)
        {
            _handlers.Add(handler);
        }

        /// <summary>
        /// Registers object for interaction handling
        /// </summary>
        public void Register(GameObject obj, InteractionConfig config)
        {
            _registeredObjects[obj] = config;
        }

        /// <summary>
        /// Unregisters object from interaction handling
        /// </summary>
        public void Unregister(GameObject obj)
        {
            _registeredObjects.Remove(obj);
        }

        /// <summary>
        /// Cleans up all objects owned by specified owner
        /// </summary>
        public void CleanUpForOwner(GameObject owner)
        {
            var toRemove = new List<GameObject>();
            foreach (var entry in _registeredObjects)
            {
                if (entry.Value.ownerGO == owner)
                {
                    toRemove.Add(entry.Key);
                }
            }

            foreach (var obj in toRemove)
            {
                _registeredObjects.Remove(obj);
            }
        }

        public void HandleTrigger(GameObject source, Collider other)
        {
            var data = new InteractionData
            {
                source = source,
                target = other.gameObject,
                physicType = InteractionPhysicType.Trigger,
                targetCollider = other,
                collision = null
            };
            ProcessInteraction(data);
        }

        public void HandleCollision(GameObject source, Collision collision)
        {
            var data = new InteractionData
            {
                source = source,
                target = collision.gameObject,
                physicType = InteractionPhysicType.Collision,
                targetCollider = null,
                collision = collision
            };
            ProcessInteraction(data);
        }

        /// <summary>
        /// Processes interaction between objects, prevents duplicate processing in same frame
        /// </summary>
        private void ProcessInteraction(InteractionData data)
        {
            if (Time.frameCount != _currentFrame)
            {
                _currentFrame = Time.frameCount;
                _processedPairs.Clear();
            }

            var pair = (data.source, data.target);

            // Prevent duplicate processing in same frame
            if (_processedPairs.Contains(pair))
            {
                return;
            }

            _processedPairs.Add(pair);

            if (!_registeredObjects.TryGetValue(data.source, out var sourceConfig))
            {
                Debug.Log(this.name + " cant find " + data.source + " in registered objects. Decline processing interaction");
                return;
            }

            // Try to find appropriate handler
            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(data, sourceConfig))
                {
                    handler.Handle(data, sourceConfig);
                    break;
                }
            }

            // Fallback to default handler
            if (_defaultHandler.CanHandle(data, sourceConfig))
            {
                _defaultHandler.Handle(data, sourceConfig);
            }

            StartCoroutine(ClearProcessedPair(pair));
        }

        private IEnumerator ClearProcessedPair((GameObject, GameObject) pair)
        {
            yield return new WaitForEndOfFrame();
            _processedPairs.Remove(pair);
        }
    }

    /// <summary>
    /// Handles projectile picking up pickable items
    /// </summary>
    public class ProjectilePickable_TriggerHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return sourceConfig.type == InteractionMemberType.Projectile &&
                   data.physicType == InteractionPhysicType.Trigger &&
                   data.target.GetComponentInParent<IPickable>() != null;
        }

        public void Handle(InteractionData data, InteractionConfig sourceConfig)
        {
            // Only player projectiles can pick up items
            if (sourceConfig.projectileConfig.VFXConfig.originator.GetComponent<CControl>().playerOrAi != PlayerOrAi.Player) { return; }

            var tar = data.target.GetComponent<IPickable>();
            if (tar != null) { tar.OnPickUp(); }
            ;
        }
    }

    /// <summary>
    /// Handles projectile hitting characters
    /// </summary>
    public class ProjectileCharacter_TriggerHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return sourceConfig.type == InteractionMemberType.Projectile &&
                   data.physicType == InteractionPhysicType.Trigger &&
                   data.target.GetComponentInParent<CControl>() != null;
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            var projectile = data.source.GetComponent<Projectile>();
            var targetControl = data.target.GetComponentInParent<CControl>();

            // Don't damage owner
            if (targetControl.gameObject == config.ownerGO)
                return;

            // Check teamfire - don't damage allies if teamfire is disabled
            if (!config.projectileConfig.teamfire && targetControl.IsAlly(config.projectileConfig.VFXConfig.originator.GetComponent<CControl>()))
            {
                return;
            }

            // Destroy projectile if configured
            if (config.projectileConfig.destroyOnCharacterTrigger)
            {
                GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(projectile.gameObject);
            }

            // Apply damage to character
            var control = targetControl.GetComponentInChildren<CControl>();
            if (control != null)
            {
                control.UnitGotHit?.Invoke(config.projectileConfig, data);
            }
        }
    }

    /// <summary>
    /// Handles projectile collisions with environment
    /// </summary>
    public class Projectile_CollisionHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return sourceConfig.type == InteractionMemberType.Projectile &&
                   data.physicType == InteractionPhysicType.Collision;
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            var projectile = data.source.GetComponent<Projectile>();
            if (projectile == null)
                return;

            // Spawn impact effect on strong collisions
            if (data.collision.impulse.magnitude > 2f)
            {
                // TODO: Implement impact effect spawning
                // SpawnImpactEffect(data.collision.contacts[0].point);
            }

            // Destroy projectile if configured
            if (config.projectileConfig.destroyOnCollision)
            {
                GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(projectile.gameObject);
                //CoreManager.Instance.VFXManager.CallVFXDispose(projectile.gameObject);
            }
        }

        private void SpawnImpactEffect(Vector3 position)
        {
            // TODO: Implement impact effect
        }
    }

    /// <summary>
    /// Handles characters picking up items
    /// </summary>
    public class CharacterPickable_TriggerHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return sourceConfig.type == InteractionMemberType.Character &&
                   data.target.GetComponent<IPickable>() != null;
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            var pickable = data.target.GetComponent<IPickable>();
            var control = data.source.GetComponent<CControl>();

            // Only player characters can pick up items
            if (control != null && control.playerOrAi == PlayerOrAi.Player)
            {
                pickable.OnPickUp();
            }
        }
    }

    /// <summary>
    /// Handles character-character collisions with repulsion logic
    /// </summary>
    public class CharacterCharacter_CollisionHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return sourceConfig.type == InteractionMemberType.Character &&
                   data.target.GetComponent<CControl>() != null;
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            var sourceControl = data.source.GetComponent<CControl>();
            var targetControl = data.target.GetComponent<CControl>();

            if (sourceControl == null || targetControl == null ||
                sourceControl.isDead || targetControl.isDead)
                return;

            // Apply repulsion when characters overlap in air (not allies)
            if (!sourceControl.CharacterMovement.IsGrounded && !targetControl.IsAlly(sourceControl))
            {
                // 1. Apply repulsion between characters
                sourceControl.CharacterMovement.HandleRepel(targetControl.CharacterMovement);

                // 2. Reduce velocity of target on strong collisions
                if (data.collision == null) { return; }

                var targetRb = targetControl.CharacterMovement.Rigidbody;
                if (targetRb != null && targetRb.isKinematic == false)
                {
                    // Reduce velocity on strong impacts
                    if (data.collision.impulse.magnitude > 3f)
                    {
                        targetRb.velocity *= 0.2f;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles projectiles entering deathzone
    /// </summary>
    public class ProjectileDeathzone_TriggerHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return data.source.GetComponent<Projectile>() != null && data.target.layer == LayerMask.NameToLayer("DeathZone");
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(data.source);
            //CoreManager.Instance.VFXManager.CallVFXDispose(data.source);
        }
    }

    /// <summary>
    /// Handles characters entering deathzone
    /// </summary>
    public class CharacterDeathzone_TriggerHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return data.source.GetComponent<CControl>() != null && data.target.layer == LayerMask.NameToLayer("DeathZone");
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            data.source.GetComponent<CControl>().subComponentMediator.NotifyDeathZoneContact();
        }
    }

    /// <summary>
    /// Default handler for unhandled interactions
    /// </summary>
    public class DefaultInteractionHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return true;
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            // Do nothing for unhandled interactions
        }        
    }
}