using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum AttackTypes
    {
        A_Shoryuken_DownSmash_Finish,
    }
    public class AttackSystem : SubComponent
    {
        [Header("Setup")]
        public Transform projectileSpawnPosition;


        private BoxCollider _attackTriggerCollider;
        public AttackAbilityLogic attackPrepLogic;
        public AttackAbilityLogic attackFinishLogic;

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.attackSystem = this;
            _attackTriggerCollider = GetComponent<BoxCollider>();

            InitAttackLogic();
        }

        private void InitAttackLogic()
        {
            //attackPrep
            switch (control.characterSettings.attackPrepAbility.attackPrep_State.animation)
            {
                case AttackPrep_States.A_ShootArrow:
                    attackPrepLogic = new AttackLogic_ShootArrow();
                    break;
                case AttackPrep_States.A_Shoryuken_DownSmash_Prep:
                    attackPrepLogic = new AttackLogic_ShoryukenDownSmash_Prep();
                    break;
                case AttackPrep_States.A_HeadSpin_Attack:
                    attackPrepLogic = new AttackLogic_HeadSpinAttack();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }

            //attackFinish
            switch (control.characterSettings.attackFininsh_State.animation)
            {
                case AttackFinish_States.A_Shoryuken_DownSmash_Finish:
                    attackFinishLogic = new AttackLogic_ShoryukenDownSmash_Finish();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }
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
            _attackTriggerCollider.center = new Vector3(0f, 0.56f, 0f);
            _attackTriggerCollider.size = new Vector3(0f, 0f, 0f);
        }
        public void EnableAttackTrigger()
        {
            _attackTriggerCollider.center = new Vector3(4.470348e-08f, 0.4552352f, 0.06810474f);
            _attackTriggerCollider.size = new Vector3(0.2461494f, 0.942835f, 1.462443f);
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