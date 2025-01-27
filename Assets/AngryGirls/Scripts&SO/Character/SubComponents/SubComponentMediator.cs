using UnityEngine;

namespace Angry_Girls
{
    public enum Subcomponents
    {
    }

    public enum NotifyContact_EventNames
    {
        CharacterCollider_Trigger_Enter,
    }

    public enum NotifyCollision_EventNames
    {
        Character_Reposition_ColliderSpheres,
    }

    public enum NotifyDamage_EventNames
    {
        CheckDamage,
        DamageRecived
    }

    public class SubComponentMediator : MonoBehaviour/*, IMediator<UnitLaunch_EventNames>*/
    {
        private CControl _control;
        private AnimationProcessor _animationProcessor;
        private DamageHandler _damageProcessor;
        private CollisionSpheres _collisionSpheres;
        private BoxColliderUpdater _boxColliderUpdater;
        private BlockingManager _blockingManager;
        private GroundDetector _groundDetector;

        public void OnAwake()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            _control = GetComponentInParent<CControl>();
            _groundDetector = GetComponentInChildren<GroundDetector>();
            _animationProcessor = GetComponentInChildren<AnimationProcessor>();
            _collisionSpheres = GetComponentInChildren<CollisionSpheres>();
            _boxColliderUpdater = GetComponentInChildren<BoxColliderUpdater>();
            _blockingManager = GetComponentInChildren<BlockingManager>();
            _damageProcessor = GetComponentInChildren<DamageHandler>();
        }

        //replace? Todo:
        public void Notify_DamageTaken(object sender, VFX vFX, Collider triggerCollider)
        {
            _control.ApplyKnockback(triggerCollider.gameObject, vFX.projectileKnockBack);
            _control.UpdateHealth(-vFX.projectileDamage);
            GameLoader.Instance.UIManager.UpdateHealthBarValueAndVision(_control);
            GameLoader.Instance.VFXManager.ShowDamageNumbers(triggerCollider, _control, vFX.projectileDamage);
            GameLoader.Instance.audioManager.PlayRandomSound(AudioSourceType.CharacterHit);
        }

        public void Notify_Dead(object sender, Subcomponents eventName)
        {
            _control.isDead = true;
            GameLoader.Instance.UIManager.RemoveHealthBar(_control);
        }

        public void Notify(object sender, NotifyContact_EventNames eventName, Collider collider)
        {
            if (eventName == NotifyContact_EventNames.CharacterCollider_Trigger_Enter)
            {
                _damageProcessor.CheckForDamage(collider);
            }
        }
        public void Notify(object sender, NotifyCollision_EventNames eventName)
        {
            if (eventName == NotifyCollision_EventNames.Character_Reposition_ColliderSpheres)
            {
                _collisionSpheres.RepositionAllSpheres();
            }
        }

        public Vector3 Notify_GetBottomContactPoint(object sender)
        {
            return _groundDetector.bottomRaycastContactPoint;
        }
    }
}