using UnityEngine;

namespace Angry_Girls
{
    public class DamageProcessor : SubComponent<SubcomponentMediator_EventNames>
    {
        public override void OnComponentEnable()
        {
        }
        public override void OnUpdate()
        {
            if (control.isDead)
            {
                SetParamsAfterDeath();
            }
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
                OnDamageTaken(vfx.projectileDamage, triggerCollider, vfx);
            }

            //no team fire
            if (vfx.vfxOwner.GetComponent<CControl>().playerOrAi == control.playerOrAi)
            {
                return;
            }
            else
            {
                OnDamageTaken(vfx.projectileDamage, triggerCollider, vfx);
            }

            //are we dead?
            if (control.currentHealth <= 0)
            {
                control.isDead = true;
                return;
            }

            //should set bool for hit animation
            ColorDebugLog.Log(control.name + " has been hit by " + vfx.vfxOwner.name + " || " + vfx.GetVFXType().ToString(), System.Drawing.KnownColor.Aquamarine);
            control.unitGotHit = true;
        }

        private void OnDamageTaken(float damage, Collider triggerCollider, VFX vfx)
        {
            control.currentHealth -= damage;
            var contactpoint = triggerCollider.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            var previewVfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_TestOnHitEffect, contactpoint, Quaternion.identity);
            previewVfx.GetComponent<Test_ShowDamageAmount>().ShowDamage(vfx.projectileDamage);
            GameLoader.Instance.audioManager.PlayRandomSound(AudioSourceType.CharacterHit);
        }

        public void SetParamsAfterDeath()
        {
            control.airToGroundUnit_FinishedAbility = true;
            control.hasFinishedLaunchingTurn = true;
            control.hasFinishedStaticAttackTurn = true;
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


    }

}