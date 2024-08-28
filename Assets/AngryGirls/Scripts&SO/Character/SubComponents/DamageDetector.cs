using UnityEngine;

namespace Angry_Girls
{
    public class DamageProcessor : SubComponent<SubcomponentMediator_EventNames>
    {
        [Header("Setup")]
        [SerializeField] private float _health = 100f;

        public override void OnComponentEnable()
        {
            control.currentHealth = _health;
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
                ApplyDamage(vfx.projectileDamage);
                var contactpoint = triggerCollider.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                var previewVfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_TestOnHitEffect, contactpoint, Quaternion.identity);
                previewVfx.GetComponent<Test_ShowDamageAmount>().ShowDamage(vfx.projectileDamage);
                GameLoader.Instance.audioManager.PlayRandomSound(AudioSourceType.CharacterHit);
            }

            //no team fire
            if (vfx.vfxOwner.GetComponent<CControl>().playerOrAi == control.playerOrAi)
            {
                return;
            }
            else
            {
                ApplyDamage(vfx.projectileDamage);
                var contactpoint = triggerCollider.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                var previewVfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_TestOnHitEffect, contactpoint, Quaternion.identity);
                previewVfx.GetComponent<Test_ShowDamageAmount>().ShowDamage(vfx.projectileDamage);
                GameLoader.Instance.audioManager.PlayRandomSound(AudioSourceType.CharacterHit);
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

        private void ApplyDamage(float damage)
        {
            control.currentHealth -= damage;
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