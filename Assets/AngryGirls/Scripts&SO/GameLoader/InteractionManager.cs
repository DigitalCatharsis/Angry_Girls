using System;
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

    public struct InteractionConfig
    {
        public GameObject ownerGO;
        public InteractionMemberType type;
        public ProjectileConfig projectileConfig;
    }

    public struct InteractionData
    {
        public GameObject source;
        public GameObject target;
        public InteractionPhysicType physicType;
        public Collider targetCollider;
        public Collision collision;
    }

    public interface IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig);
        public void Handle(InteractionData data, InteractionConfig sourceConfig);
    }

    public class InteractionManager : MonoBehaviour
    {
        private Dictionary<GameObject, InteractionConfig> _registeredObjects = new Dictionary<GameObject, InteractionConfig>();
        private List<IInteractionHandler> _handlers = new List<IInteractionHandler>();
        private IInteractionHandler _defaultHandler = new DefaultInteractionHandler();
        private HashSet<(GameObject, GameObject)> _processedPairs = new HashSet<(GameObject, GameObject)>();

        private void Awake()
        {
            // Регистрируем обработчики в порядке приоритета
            AddHandler(new ProjectileCharacter_TriggerHandler());
            AddHandler(new ProjectilePickable_TriggerHandler());
            AddHandler(new Projectile_CollisionHandler());
            AddHandler(new CharacterPickable_TriggerHandler());
            AddHandler(new CharacterCharacter_CollisionHandler());
            AddHandler(new ProjectileDeathzone_TriggerHandler());
            AddHandler(new CharacterDeathzone_TriggerHandler());
        }

        public void AddHandler(IInteractionHandler handler)
        {
            _handlers.Add(handler);
        }

        public void Register(GameObject obj, InteractionConfig config)
        {
            _registeredObjects[obj] = config;
        }

        public void Unregister(GameObject obj)
        {
            _registeredObjects.Remove(obj);
        }

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

        private void ProcessInteraction(InteractionData data)
        {
            var pair = (data.source, data.target);

            if (_processedPairs.Contains(pair))
            {
                return;
            }

            _processedPairs.Add(pair);

            if (!_registeredObjects.TryGetValue(data.source, out var sourceConfig))
            {
                return;
            }

            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(data, sourceConfig))
                {
                    handler.Handle(data, sourceConfig);
                    break;
                }
            }

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

    public class ProjectilePickable_TriggerHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return sourceConfig.type == InteractionMemberType.Projectile &&
                   data.physicType == InteractionPhysicType.Trigger &&
                   data.target.GetComponentInParent<IPickable>() != null
                   ;
        }

        public void Handle(InteractionData data, InteractionConfig sourceConfig)
        {
            if (sourceConfig.projectileConfig.VFXConfig.originator.GetComponent<CControl>().playerOrAi != PlayerOrAi.Player) { return; }

            var tar = data.target.GetComponent<IPickable>();
            if (tar != null) { tar.OnPickUp(); };
        }
    }

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

            // Не наносим урон себе
            if (targetControl.gameObject == config.ownerGO)
                return;

            if (!config.projectileConfig.teamfire && targetControl.IsAlly(config.projectileConfig.VFXConfig.originator.GetComponent<CControl>()))
            {
                return;
            }

            if (config.projectileConfig.destroyOnCharacterTrigger)
            {
                GameLoader.Instance.VFXManager.CallVFXDispose(projectile.gameObject);
            }

            var mediator = targetControl.GetComponentInChildren<SubComponentMediator>();
            if (mediator != null)
            {
                mediator.Notyfy_CheckForDamage(config.projectileConfig, data);
            }
        }
    }

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

            // Дополнительные эффекты при сильном ударе
            if (data.collision.impulse.magnitude > 2f)
            {
                SpawnImpactEffect(data.collision.contacts[0].point);
            }

            if (config.projectileConfig.destroyOnCollision)
            {

                GameLoader.Instance.VFXManager.CallVFXDispose(projectile.gameObject);
            }
        }

        private void SpawnImpactEffect(Vector3 position)
        {
            // Реализация эффекта удара
        }
    }

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

            if (control != null && control.playerOrAi == PlayerOrAi.Player)
            {
                pickable.OnPickUp();
            }
        }
    }

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

            if (!sourceControl.isGrounded && !targetControl.IsAlly(sourceControl))
            {
                sourceControl.CharacterMovement.HandleRepel(targetControl.CharacterMovement);
            }
        }
    }

    public class ProjectileDeathzone_TriggerHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            return data.source.GetComponent<Projectile>() != null && data.target.layer == LayerMask.NameToLayer("DeathZone");
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
            GameLoader.Instance.VFXManager.CallVFXDispose(data.source);
        }
    }

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

    public class DefaultInteractionHandler : IInteractionHandler
    {
        public bool CanHandle(InteractionData data, InteractionConfig sourceConfig)
        {
            // Всегда возвращаем true, так как это обработчик по умолчанию
            return true;
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
        }
    }
}