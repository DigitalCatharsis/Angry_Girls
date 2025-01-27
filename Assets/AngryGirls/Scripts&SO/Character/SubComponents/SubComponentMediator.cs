using UnityEngine;

namespace Angry_Girls
{
    public class SubComponentMediator : MonoBehaviour/*, IMediator<UnitLaunch_EventNames>*/
    {
        private CControl _control;
        private AnimationProcessor _animationProcessor;
        private DamageHandler _damageHandler;
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
            _damageHandler = GetComponentInChildren<DamageHandler>();
        }

        public void Notify_DamageTaken(object sender, VFX vFX, Collider triggerCollider)
        {
            _control.ApplyKnockback(triggerCollider.gameObject, vFX.projectileKnockBack);
            _control.UpdateHealth(-vFX.projectileDamage);
            GameLoader.Instance.gameLogic_UIManager.UpdateHealthBarValueAndVision(_control);
            GameLoader.Instance.VFXManager.ShowDamageNumbers(triggerCollider, _control, vFX.projectileDamage);
            GameLoader.Instance.audioManager.PlayRandomSound(AudioSourceType.CharacterHit);
        }

        public void Notify_Dead(object sender)
        {            
            GameLoader.Instance.gameLogic_UIManager.DisableHealthBar(_control);
            _damageHandler.SetDeathParams();
        }

        public void Notify_CollissionCheck(object sender, Collider collider)
        {
            _damageHandler.CheckForDamage(collider);
        }

        public void Notify_UpdatingColliderSpheres(object sender)
        {
            _collisionSpheres.RepositionAllSpheres();
        }
    }
}