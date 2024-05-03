using System.Collections;
using System.Collections.Generic;
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
            attackTriggerCollider = GetComponent<BoxCollider>();
        }
        public override void OnUpdate()
        {
            var test = control.subComponentProcessor.animationProcessor.groundedAttack_Dictionary[GroundedAttack_States.A_Shoryuken_DownSmash_Finish];

            if (control.subComponentProcessor.animationProcessor.IsAttackStateOver() == false)
            {
                if (control.subComponentProcessor.animationProcessor.groundedAttack_Dictionary[GroundedAttack_States.A_Shoryuken_DownSmash_Finish] 
                    == control.subComponentProcessor.animationProcessor.currentStateData.hash)
                {
                    ProceedDownSmashFinish_Attack();
                }
            }
            else
            {
                attackTriggerCollider.center = new Vector3(0, 0.56f, 0);
                attackTriggerCollider.size = new Vector3(0, 0, 0);
            }
        }

        private void ProceedDownSmashFinish_Attack()
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