using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public class DamageHandler : SubComponent
    {
        public override void OnComponentEnable()
        {

        }
        public override void OnUpdate()
        {
        }

        public void CheckForDamage(Collider triggerCollider)
        {
            if (triggerCollider.gameObject.layer == LayerMask.NameToLayer("DeathZone"))
            {
                control.subComponentMediator.Notify_Dead(this);
                return;
            }

            //Does it VFX?
            var vfx = triggerCollider.gameObject.transform.GetComponent<VFX>();

            if (vfx == null)
            {
                return;
            }

            //aplly damage if no owner (damages everyone)
            if (vfx.vfxOwner == null && vfx.projectileDamage != 0)
            {
                control.subComponentMediator.Notify_DamageTaken(this, vfx, triggerCollider);
            }

            if (vfx.vfxOwner.GetComponent<CControl>() == null | vfx.vfxOwner.GetComponent<CControl>().playerOrAi == control.playerOrAi)
            {
                return;
            }
            else
            {
                control.subComponentMediator.Notify_DamageTaken(this, vfx, triggerCollider);
            }

            //are we dead?
            if (control.CurrentHealth <= 0)
            {
                control.subComponentMediator.Notify_Dead(this);
                return;
            }

            //should set bool for hit animation
            ColorDebugLog.Log(control.name + " has been hit by " + vfx.vfxOwner.name + " || " + vfx.GetVFXType().ToString() + " || " + "Damage: " + vfx.projectileDamage.ToString(), System.Drawing.KnownColor.Aquamarine);
            control.unitGotHit = true;
        }

        public void SetDeathParams()
        {
            ColorDebugLog.Log($"{control.name} called death", KnownColor.Yellow);
            control.isDead = true;
            control.FinishTurn();
            control.rigidBody.useGravity = true;
            control.rigidBody.isKinematic = false;


            var animator = control.wingsTransform.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

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