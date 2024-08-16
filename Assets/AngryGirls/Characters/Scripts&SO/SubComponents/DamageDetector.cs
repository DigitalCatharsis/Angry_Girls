using UnityEditor.ShaderGraph;
using UnityEngine;

namespace Angry_Girls
{
    public class DamageProcessor : SubComponent
    {
        [Header("Setup")]
        [SerializeField] private float _health = 100f;

        public float currentHealth;

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.damageProcessor = this;

            currentHealth = _health;
        }

        public void CheckForDamage(Collider triggerCollider)
        {
            if (triggerCollider.gameObject.transform.GetComponent<VFX>() != null)
            {
                var vfx = triggerCollider.gameObject.transform.GetComponent<VFX>();

                ApplyDamage(vfx.projectileDamage);

                if (currentHealth <= 0)
                {
                    control.isDead = true;
                    return;
                }

                control.subComponentProcessor.animationProcessor.unitGotHit = true;
            }
            else
            {
                return;
            }
        }

        private void ApplyDamage(float damage)
        {
            currentHealth -= damage;
        }

        public void SetParamsAfterDeath()
        {
            control.subComponentProcessor.animationProcessor.airToGroundUnit_FinishedAbility = true;
            control.subComponentProcessor.launchLogic.hasFinishedLaunchingTurn = true;
            control.subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn = true;
            control.rigidBody.useGravity = true;
            //DeadBody
            control.gameObject.layer = 12;
        }

        public override void OnAwake()
        {
        }


        public override void OnFixedUpdate()
        {
        }

        public override void OnLateUpdate()
        {
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
            if (control.isDead)
            {
                SetParamsAfterDeath();
            }
        }
    }

}