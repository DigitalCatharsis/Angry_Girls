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


        private int _currentFrame;

        private void Awake()
        {
            // ������������ ����������� � ������� ����������
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
            Debug.Log(this.name + " has registered " + obj.name);
        }

        public void Unregister(GameObject obj)
        {
            _registeredObjects.Remove(obj);
            Debug.Log(this.name + " has deregistered " + obj.name);
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
                Debug.Log(this.name + " has deregistered " + obj.name + " (CleanUpForOwner)");
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
            if (Time.frameCount != _currentFrame)
            {
                _currentFrame = Time.frameCount;
                _processedPairs.Clear();
            }

            var pair = (data.source, data.target);

            if (_processedPairs.Contains(pair))
            {
                Debug.Log(this.name + " dont process interaction between " + data.source + " " + data.target);
                return;
            }

            _processedPairs.Add(pair);

            if (!_registeredObjects.TryGetValue(data.source, out var sourceConfig))
            {
                Debug.Log(this.name + " cant find " + data.source + " in registered objects. Decline processing interaction");
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
            Debug.Log(this.name + " is cleaning " + pair.Item1, pair.Item2);
            yield return new WaitForEndOfFrame();
            _processedPairs.Remove(pair);
            Debug.Log(this.name + " finished cleaning for " + pair.Item1, pair.Item2);
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
            if (sourceConfig.projectileConfig.VFXConfig.originator.GetComponent<CControl>().playerOrAi != PlayerOrAi.Character) { return; }

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

            // �� ������� ���� ����
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

            // �������������� ������� ��� ������� �����
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
            // ���������� ������� �����
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

            if (control != null && control.playerOrAi == PlayerOrAi.Character)
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

            if (!sourceControl.CharacterMovement.IsGrounded && !targetControl.IsAlly(sourceControl))
            {
                // 1. ������������ ��� ����
                sourceControl.CharacterMovement.HandleRepel(targetControl.CharacterMovement);

                // 2. ��������� �������� � ���� (���� ��� �����)
                if (data.collision == null) { return; }

                var targetRb = targetControl.CharacterMovement.Rigidbody;
                if (targetRb != null && targetRb.isKinematic == false)
                {
                    // ����������� � ������ ���� ���� �������� ������
                    if (data.collision.impulse.magnitude > 3f)
                    {
                        targetRb.velocity *= 0.2f; // ������� �������
                    }
                }
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
            // ������ ���������� true, ��� ��� ��� ���������� �� ���������
            return true;
        }

        public void Handle(InteractionData data, InteractionConfig config)
        {
        }
    }
}