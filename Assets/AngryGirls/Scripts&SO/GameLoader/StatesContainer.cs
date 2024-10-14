using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Angry_Girls
{
    #region enums
    public enum StateNames
    {
        NONE,
        Idle_General,
        Idle_HeadSpin,
        Idle_Floating, 
        Idle_Axe,
        Idle_Ninja_1,
        Idle_Ninja_2,
        Idle_Ninja_3,
        Idle_Ninja_4,
        Idle_Sword_1,
        Idle_Sword_2,
        Idle_Sword_3,
        Idle_Sword_4,
        Idle_Sword_5,
        Idle_Sword_6,

        Falling_General,
        Falling_Rolling,

        Landing_General,
        Landing_Rolling,
        Landing_SwordAttack,

        Launch_Shoryuken_DownSmash_Prep,
        Launch_SendFireball_Front,
        Launch_HeadSpin_Attack,
        Launch_SwordAttack_Attack,
        Launch_SwordAttack_Prep,
        Alternate_Shoryuken_Prep,
        Alternate_Shoryuken_Rise,
        Alternate_SendFireball_Front,
        Alternate_HeadSpin_Attack_Prep,
        Alternate_HeadSpin_Attack,
        Alternate_SwordAttack_Prep,
        Alternate_SwordAttack_Attack,
        Alternate_Shoryuken_Landing,

        Launch_Shoryuken_DownSmash_Finish,

        HitReaction_1,
        HitReaction_2,
        HitReaction_3,
        HitReaction_4,
        HitReaction_5,

        Death_SweepFall,
    }
    public enum Idle_States
    {
        NONE,
        Idle_General,
        Idle_HeadSpin,
        Idle_Floating,
        Idle_Axe,
        Idle_Ninja_1,
        Idle_Ninja_2,
        Idle_Ninja_3,
        Idle_Ninja_4,
        Idle_Sword_1,
        Idle_Sword_2,
        Idle_Sword_3,
        Idle_Sword_4,
        Idle_Sword_5,
        Idle_Sword_6,
    }
    public enum Falling_States
    {
        NONE,
        Falling_General,
        Falling_Rolling,
    }

    public enum Landing_States
    {
        NONE,
        Landing_General,
        Landing_Rolling,
        Landing_SwordAttack,
        Landing_Shoryuken,
    }

    public enum Attack_States
    {
        NONE,
        Launch_Shoryuken_DownSmash_Prep,
        Launch_SendFireball_Front,
        Launch_HeadSpin_Attack,
        Launch_SwordAttack_Attack,
        Launch_SwordAttack_Prep,
        Alternate_Shoryuken_Prep,
        Alternate_Shoryuken_Rise,
        Alternate_SendFireball_Front,
        Alternate_HeadSpin_Attack_Prep,
        Alternate_HeadSpin_Attack,
        Alternate_SwordAttack_Prep,
        Alternate_SwordAttack_Attack,
        Alternate_Shoryuken_Landing,  //:TODO should not be here. Rework entire Shoryken
    }
    public enum AttackFinish_States
    {
        NONE,
        Launch_Shoryuken_DownSmash_Finish,  // TODO: Static?
    }


    public enum HitReaction_States
    {
        NONE,
        HitReaction_1,
        HitReaction_2,
        HitReaction_3,
        HitReaction_4,
        HitReaction_5,
    }

    public enum Death_States
    {
        NONE,
        Death_SweepFall,
    }
    #endregion
    public class StatesContainer : MonoBehaviour
    {

        [Header("State Dictionaries")]
        public SerializedDictionary<Attack_States, int> attack_Dictionary;
        public SerializedDictionary<AttackFinish_States, int> attackFinish_Dictionary;
        public SerializedDictionary<StateNames, int> stateNames_Dictionary;
        public SerializedDictionary<Idle_States, int> idle_Dictionary;
        public SerializedDictionary<Falling_States, int> airbonedFlying_Dictionary;
        public SerializedDictionary<Landing_States, int> landingNames_Dictionary;
        public SerializedDictionary<HitReaction_States, int> hitReaction_Dictionary;
        public SerializedDictionary<Death_States, int> death_States_Dictionary;


        private void Awake()
        {
            //Init Dictionaries
            stateNames_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<StateNames>(this.gameObject);
            idle_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Idle_States>(this.gameObject);
            airbonedFlying_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Falling_States>(this.gameObject);
            attack_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Attack_States>(this.gameObject);
            attackFinish_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<AttackFinish_States>(this.gameObject);
            landingNames_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Landing_States>(this.gameObject);
            hitReaction_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<HitReaction_States>(this.gameObject);
            death_States_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Death_States>(this.gameObject);
        }
    }
}