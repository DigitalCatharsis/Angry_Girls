using UnityEngine;

namespace Angry_Girls
{
    public enum AttackTypes
    {
        A_Shoryuken_DownSmash_Finish,
    }
    public class AttackSystem : SubComponent
    {
        private BoxCollider attackTriggerCollider;

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.attackSystem = this;
            attackTriggerCollider = GetComponent<BoxCollider>();
        }
        public override void OnUpdate()
        {

        }

        public void TryProcessAttack()
        {            
            EnableAttackTrigger();
        }

        public void DisableAttackTrigger()
        {
            attackTriggerCollider.center = new Vector3(0f, 0.56f, 0f);
            attackTriggerCollider.size = new Vector3(0f, 0f, 0f);
        }
        public void EnableAttackTrigger()
        {
            attackTriggerCollider.center = new Vector3(4.470348e-08f, 0.4552352f, 0.06810474f);
            attackTriggerCollider.size = new Vector3(0.2461494f, 0.942835f, 1.462443f);
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