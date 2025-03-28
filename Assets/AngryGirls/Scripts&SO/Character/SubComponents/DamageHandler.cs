using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public class DamageHandler : SubComponent
    {
        public void CheckForDamage(Collider triggerCollider)
        {
            if (control.isDead) { return; }

            var vfx = triggerCollider.gameObject.transform.GetComponent<VFX>();

            if (vfx == null) { return; }

            //apply damage if no owner (damages everyone)
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
            if (control.Health.CurrentHealth <= 0)
            {
                control.subComponentMediator.Notify_Dead(this);
                return;
            }

            control.unitGotHit = true;
        }

        public void SetDeathParams()
        {
            ColorDebugLog.Log($"{control.name} called death", KnownColor.Yellow);
            control.isDead = true;
            control.FinishTurn();
            control.CharacterMovement.Rigidbody.useGravity = true;
            control.CharacterMovement.Rigidbody.isKinematic = false;
            //control.CharacterMovement.Rigidbody.constraints = RigidbodyConstraints.None;


            var animator = control.wingsTransform.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

            control.gameObject.layer = 12;
        }

        public override void OnAwake() { }
        public override void OnFixedUpdate() { }
        public override void OnLateUpdate() { }
        public override void OnStart() { }
        public override void OnComponentEnable() { }
        public override void OnUpdate() { }
    }
}