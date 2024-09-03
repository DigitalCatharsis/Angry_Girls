using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackSystem : SubComponent<SubcomponentMediator_EventNames>
    {
        //Logic
        //public AttackAbilityLogic attackPrepLogic;
        //public AttackAbilityLogic attackFinishLogic;

        //public AttackAbilityLogic staticAttackLogic_Prep;
        //public AttackAbilityLogic staticAttackLogic_Landing;
        //public AttackAbilityLogic staticAttackLogic_OnGround;
        public override void OnComponentEnable()
        {
            InitAttackLogic();
        }

        private void InitAttackLogic()
        {
            //attackPrep
            switch (control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation)
            {
                case AttackPrep_States.A_SendFireball_Front:
                    control.attackSystem_Data.attackPrepLogic = new AttackLogic_SendFireball_Front();
                    break;
                case AttackPrep_States.A_Shoryuken_DownSmash_Prep:
                    control.attackSystem_Data.attackPrepLogic = new AttackLogic_ShoryukenDownSmash_Prep();
                    break;
                case AttackPrep_States.A_HeadSpin_Attack:
                    control.attackSystem_Data.attackPrepLogic = new AttackLogic_HeadSpinAttack();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }

            //attackFinish
            switch (control.characterSettings.attackFininsh_State.animation)
            {
                case AttackFinish_States.A_Shoryuken_DownSmash_Finish:
                    control.attackSystem_Data.attackFinishLogic = new AttackLogic_ShoryukenDownSmash_Finish();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }

            //StaticAttack
            switch (control.characterSettings.staticAttackAbility.staticAttack_State.animation)
            {
                case StaticAttack_States.A_Shoryuken_Prep_Static:
                    control.attackSystem_Data.staticAttackLogic_Prep = new AttackLogic_Static_Prep_Shoryuken();
                    control.attackSystem_Data.staticAttackLogic_Landing = new AttackLogic_Static_Landing_Shoryuken();
                    control.attackSystem_Data.staticAttackLogic_OnGround = new AttackLogic_Static_OnGround_Shoryuken_Rise();
                    break;
                case StaticAttack_States.A_SendFireball_Front_Static:
                    control.attackSystem_Data.staticAttackLogic_Prep = new AttackLogic_SendFireball_Front();
                    break;
                case StaticAttack_States.A_HeadSpin_Attack_Static:
                    control.attackSystem_Data.staticAttackLogic_Prep = new AttackLogic_Static_HeadSpin();
                    break;
                default:
                    throw new Exception("No logic for state like " + control.characterSettings.staticAttackAbility.staticAttack_State.animation.ToString());
            }
        }

        public override void OnUpdate()
        {
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

    [Serializable]
    public class AttackSystem_Data
    {
        public AttackAbilityLogic attackPrepLogic;
        public AttackAbilityLogic attackFinishLogic;

        public AttackAbilityLogic staticAttackLogic_Prep;
        public AttackAbilityLogic staticAttackLogic_Landing;
        public AttackAbilityLogic staticAttackLogic_OnGround;
    }
}