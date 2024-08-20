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
            //Does it VFX?
            var vfx = triggerCollider.gameObject.transform.GetComponent<VFX>();

            if (vfx == null)
            {
                return;
            }

            //aplly damage if no owner (damages everyone)
            if (vfx.vfxOwner == null && vfx.projectileDamage != 0)
            {
                ApplyDamage(vfx.projectileDamage);
            }

            //no team fire
            if (vfx.vfxOwner.GetComponent<CControl>().playerOrAi == control.playerOrAi)
            {
                return;
            }
            else
            {
                ApplyDamage(vfx.projectileDamage);
            }

            //are we dead?
            if (currentHealth <= 0)
            {
                control.isDead = true;
                return;
            }

            //should set bool for hit animation
            ColorDebugLog.Log(control.name + " has been hit by " + vfx.vfxOwner.name + " || " + vfx.GetVFXType().ToString(), System.Drawing.KnownColor.Aquamarine);
            control.subComponentProcessor.animationProcessor.unitGotHit = true;
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