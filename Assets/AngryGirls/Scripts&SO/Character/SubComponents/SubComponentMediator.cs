using UnityEngine;

namespace Angry_Girls
{
    public class SubComponentMediator : MonoBehaviour
    {
        private CControl _control;
        private AnimationProcessor _animationProcessor;
        private DamageHandler _damageHandler;
        private BoxColliderUpdater _boxColliderUpdater;
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
            _boxColliderUpdater = GetComponentInChildren<BoxColliderUpdater>();
            _damageHandler = GetComponentInChildren<DamageHandler>();
        }
        public void Notyfy_CheckForDamage(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            _damageHandler.CheckForDamage(projectileConfig, interactionData);
        }

        public void Notify_DamageTaken(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            _control.CharacterMovement.ApplyKnockbackFromEnemy(interactionData.target, projectileConfig.enemyKnockBackValue);
            _control.Health.ApplyDamage(projectileConfig.damage);
            GameLoader.Instance.gameLogic_UIManager.UpdateHealthBarValueAndVision(_control);
            GameLoader.Instance.VFXManager.ShowDamageNumbers(interactionData.targetCollider, projectileConfig.VFXConfig.originator, projectileConfig.damage);
            GameLoader.Instance.audioManager.PlayRandomSound(AudioSourceType.CharacterHit);
        }
        public void NotifyDeathZoneContact(object sender)
        {
            _control.Health.ApplyDamage(_control.Health.CurrentHealth);
            Notify_Dead();
        }

        public void Notify_Dead()
        {            
            GameLoader.Instance.gameLogic_UIManager.DisableHealthBar(_control);
            _damageHandler.SetDeathParams();
            _animationProcessor.SetDeath();
            _control.isDead = true;
        }
    }
}