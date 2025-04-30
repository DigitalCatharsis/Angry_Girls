using UnityEditorInternal;
using UnityEngine;

namespace Angry_Girls
{
    public class SubComponentMediator : MonoBehaviour
    {
        private CControl _control;
        private AnimationProcessor _animationProcessor;

        public void OnAwake()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            _control = GetComponentInParent<CControl>();
            _animationProcessor = GetComponentInChildren<AnimationProcessor>();
        }
        public void Notyfy_CheckForDamage(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            if (projectileConfig.damage == 0) { return; }

            _control.unitGotHit = true;
            _control.CharacterMovement.ApplyKnockbackFromEnemy(interactionData.target, projectileConfig.enemyKnockBackValue);
            _control.Health.ApplyDamage(projectileConfig.damage);

            GameLoader.Instance.gameLogic_UIManager.UpdateHealthBarValueAndVision(_control);
            GameLoader.Instance.VFXManager.ShowDamageNumbers(interactionData.targetCollider, projectileConfig.VFXConfig.originator, projectileConfig.damage);
            GameLoader.Instance.audioManager.PlayRandomSound(AudioSourceType.CharacterHit);

            if (_control.Health.CurrentHealth <= 0)
            {
                GameLoader.Instance.gameLogic_UIManager.DisableHealthBar(_control);
                _control.SetDeathParams();

                if (_control.characterSettings.deathByAnimation)
                {
                    _animationProcessor.PlayDeathStateForNonRagdoll();
                }
                else
                {
                    Vector3 hitPoint = GetHitPoint(interactionData);
                    Vector3 forceDirection = GetProjectileForceDirection(interactionData);
                    Vector3 force = forceDirection * Mathf.Abs(projectileConfig.deadbodyForceMultiplier);

                    // Учитываем знак множителя
                    if (projectileConfig.deadbodyForceMultiplier < 0)
                        force = -force;

                    _control.Ragdoll.ProcessRagdoll(
                        rigidbody: _control.CharacterMovement.Rigidbody,
                        forceValue: force,
                        forceApplyPosition: hitPoint,
                        forceMode: projectileConfig.deadbodyForceMode);
                }
            }
        }
        private Vector3 GetProjectileForceDirection(InteractionData interactionData)
        {
            // 1. Пробуем получить направление скорости из Rigidbody
            Rigidbody projectileRb = interactionData.source.GetComponent<Rigidbody>();
            if (projectileRb != null && projectileRb.velocity.sqrMagnitude > 0.1f)
            {
                return projectileRb.velocity.normalized;
            }

            // 2. Если нет Rigidbody, пробуем определить направление по вращению
            if (interactionData.source.transform.up.sqrMagnitude > 0.1f)
            {
                return interactionData.source.transform.up;
            }

            // 3. Fallback: направление от projectile к цели
            Vector3 projectileToTarget = interactionData.target.transform.position - interactionData.source.transform.position;
            if (projectileToTarget.sqrMagnitude > 0.1f)
            {
                return projectileToTarget.normalized;
            }

            // 4. Ultimate fallback: вверх с небольшим рандомом
            return (Vector3.up + Random.insideUnitSphere * 0.3f).normalized;
        }

        private Vector3 GetHitPoint(InteractionData interactionData)
        {
            // Если было столкновение (Collision), берём точку из contacts[0]
            if (interactionData.physicType == InteractionPhysicType.Collision && interactionData.collision != null)
            {
                return interactionData.collision.contacts[0].point;
            }
            // Если был триггер (Trigger), используем ближайшую точку на коллайдере цели
            else if (interactionData.targetCollider != null)
            {
                return interactionData.targetCollider.ClosestPoint(interactionData.source.transform.position);
            }
            // Fallback: если данных нет, используем центр объекта
            else
            {
                return interactionData.target.transform.position;
            }
        }
        public void NotifyDeathZoneContact()
        {
            _control.Health.ApplyDamage(_control.Health.CurrentHealth);
        }
    }
}