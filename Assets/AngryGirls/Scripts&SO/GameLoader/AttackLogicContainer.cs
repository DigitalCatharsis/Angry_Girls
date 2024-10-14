using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogicContainer: MonoBehaviour
    {
        public void SetCharacterAttackLogic(CControl control)
        {
            if (control.characterSettings.AttackAbility_Launch != null)
            {
                //attackPrep
                switch (control.characterSettings.AttackAbility_Launch.attack_State.animation)
                {
                    case Attack_States.Launch_SendFireball_Front:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_SendFireball_Front();
                        break;
                    case Attack_States.Launch_Shoryuken_DownSmash_Prep:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_ShoryukenDownSmash_Prep();
                        break;
                    case Attack_States.Launch_HeadSpin_Attack:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_HeadSpinAttack();
                        break;
                    case Attack_States.Launch_SwordAttack_Prep:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_SwordAttack();
                        break;
                    default:
                        throw new Exception("No logic for state like " + control.characterSettings.AttackAbility_Launch.attack_State.animation.ToString());
                }
            }
            else
            {
                ColorDebugLog.Log("no Launch Attack Ability for " + control.name, System.Drawing.KnownColor.GreenYellow);
            }


            //attackFinish
            switch (control.characterSettings.attackFininsh_State.animation)
            {
                case AttackFinish_States.Launch_Shoryuken_DownSmash_Finish:
                    control.attackSystem_Data.Launch_AttackFinishLogic = new AttackLogic_Launch_ShoryukenDownSmash_Finish();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }

            //Alternate_Attack
            switch (control.characterSettings.AttackAbility_Alternate.attack_State.animation)
            {
                case Attack_States.Alternate_Shoryuken_Prep:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_AlternateAttack_Prep_Shoryuken();
                    control.attackSystem_Data.AlternateAttackLogic_Landing = new AttackLogic_AlternateAttack_Landing_Shoryuken();
                    break;
                case Attack_States.Alternate_SendFireball_Front:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_Launch_SendFireball_Front();
                    break;
                case Attack_States.Alternate_HeadSpin_Attack_Prep:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_AlternateAttack_HeadSpin_Prep();
                    control.attackSystem_Data.AlternateAttackLogic_Airboned = new AttackLogic_AlternateAttack_HeadSpin();
                    break;
                case Attack_States.Alternate_SwordAttack_Prep:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_AlternateAttack_SwordAttack();
                    break;
                default:
                    throw new Exception("No logic for state like " + control.characterSettings.AttackAbility_Alternate.attack_State.animation.ToString());
            }
        }
    }

    [Serializable]
    public class AttackSystem_Data
    {
        public AttackAbilityLogic Launch_AttackLogic;
        public AttackAbilityLogic Launch_AttackFinishLogic;

        public AttackAbilityLogic AlternateAttackLogic_Prep;
        public AttackAbilityLogic AlternateAttackLogic_Airboned;
        public AttackAbilityLogic AlternateAttackLogic_Landing;
        public AttackAbilityLogic AlternateAttackLogic_OnGround;
    }
}