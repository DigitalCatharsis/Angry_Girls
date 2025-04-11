using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public class DamageHandler : SubComponent
    {
        public void CheckForDamage(ProjectileConfig projectileConfig, InteractionData interactionData)
        {
            if (control.isDead) { return; }

            if (projectileConfig.damage == 0)
            {
                return;
            }

            control.subComponentMediator.Notify_DamageTaken(projectileConfig, interactionData);

            //are we dead?
            if (control.Health.CurrentHealth <= 0)
            {
                control.subComponentMediator.Notify_Dead();
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