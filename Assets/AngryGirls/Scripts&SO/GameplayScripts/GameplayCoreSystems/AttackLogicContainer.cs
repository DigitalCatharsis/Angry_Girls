using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogicContainer : MonoBehaviour
    {
        public void SetCharacterAttackLogic(CControl control)
        {
            if (control.CharacterSettings.AttackAbility_Launch != null)
            {
                //attackPrep
                switch (control.CharacterSettings.AttackAbility_Launch.attack_State.animation)
                {
                    case Attack_States.Launch_SendFireball_Front:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_SendFireball_Front(control.CharacterSettings.AttackAbility_Launch);
                        break;
                    case Attack_States.Launch_Uppercut_Prep:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_Uppercut_Prep(control.CharacterSettings.AttackAbility_Launch);
                        break;
                    case Attack_States.Launch_HeadSpin_Attack:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_HeadSpinAttack(control.CharacterSettings.AttackAbility_Launch);
                        break;
                    case Attack_States.Launch_SwordAttack_Prep:
                        control.attackSystem_Data.Launch_AttackLogic = new AttackLogic_Launch_SwordAttack(control.CharacterSettings.AttackAbility_Launch);
                        break;
                    default:
                        throw new Exception("No logic for state like " + control.CharacterSettings.AttackAbility_Launch.attack_State.animation.ToString());
                }
            }

            ///////////////////////////////attackFinish (for ground units)
            //for launch
            if (control.CharacterSettings.AttackAbility_Launch != null)
            {
                switch (control.CharacterSettings.AttackAbility_Launch.attackFininsh_State.animation)
                {
                    case AttackFinish_States.Launch_Uppercut_Finish:
                        control.attackSystem_Data.launch_AttackFinishLogic = new AttackLogic_Launch_UppercutDownSmash_Finish(control.CharacterSettings.AttackAbility_Launch);
                        break;
                    //default:
                    //    throw new Exception("No attackFinish logic for " + control.characterSettings.AttackAbility_Launch.attackFininsh_State);
                }
            }

            //for alternate
            switch (control.CharacterSettings.AttackAbility_Alternate.attackFininsh_State.animation)
            {
                case AttackFinish_States.Alternate_Uppercut_Finish:
                    control.attackSystem_Data.alternate_AttackFinishLogic = new AttackLogic_Alternate_UppercutDownSmash_Finish(control.CharacterSettings.AttackAbility_Alternate);
                    break;
                //default:
                //    throw new Exception("No logic for state like " + control.characterSettings.AttackAbility_Alternate.attackFininsh_State);
            }
            /////////////////////////////////////////////////
            ///

            //Alternate_Attack
            switch (control.CharacterSettings.AttackAbility_Alternate.attack_State.animation)
            {
                case Attack_States.Alternate_Uppercut_Prep:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_Alternate_Uppercut_Prep(control.CharacterSettings.AttackAbility_Alternate);
                    break;
                case Attack_States.Alternate_SendFireball_Front:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_Alternate_SendFireball_Front(control.CharacterSettings.AttackAbility_Alternate);
                    break;
                case Attack_States.Alternate_HeadSpin_Attack:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_Alternate_HeadSpin(control.CharacterSettings.AttackAbility_Alternate);
                    break;
                case Attack_States.Alternate_SwordAttack_Prep:
                    control.attackSystem_Data.AlternateAttackLogic_Prep = new AttackLogic_Alternate_SwordAttack(control.CharacterSettings.AttackAbility_Alternate);
                    break;
                default:
                    throw new Exception("No Alternate_Attack logic for " + control.name);
            }
        }
    }

    [Serializable]
    public class AttackSystem_Data
    {
        public AttackAbilityLogic Launch_AttackLogic;

        public AttackAbilityLogic AlternateAttackLogic_Prep;
        public AttackAbilityLogic AlternateAttackLogic_Airboned;
        public AttackAbilityLogic AlternateAttackLogic_Landing;
        public AttackAbilityLogic AlternateAttackLogic_OnGround;

        public AttackAbilityLogic launch_AttackFinishLogic;
        public AttackAbilityLogic alternate_AttackFinishLogic;
    }
}