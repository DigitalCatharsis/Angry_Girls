using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Angry_Girls
{
    #region enums
    public enum StateNames
    {
        NONE,
        A_AirbonedRolling,
        A_AirbonedRolling_Landing,
        A_Axe_Idle,
        A_Fall_Landing,
        A_Falling_Idle,
        A_Floating,
        A_HeadSpin_Attack,
        A_HitReaction,
        A_Idle,
        A_Idle_HeadSpin,
        A_SendFireball_Front,
        A_Shoryuken_DownSmash_Finish,
        A_Shoryuken_DownSmash_Prep,
        A_Shoryuken_Landing_Static,
        A_Shoryuken_Rise_Static,
        A_Shoryuken_Prep_Static,
        A_Sweep_Fall,
        A_HitReaction_Gut,
        A_HitReaction_HeadHit,
        A_HitReaction_HeadHit2,
        A_HitReaction_Left,
        A_SendFireball_Front_Static,
        A_HeadSpin_Attack_Static,
        A_HeadSpin_Attack_Prep_Static,
        A_NinjaIdle_1,
        A_NinjaIdle_2,
        A_NinjaIdle_3,
        A_NinjaIdle_4,
        A_SwordIdle_1,
        A_SwordIdle_2,            
        A_SwordIdle_3,
        A_SwordIdle_4,
        A_SwordIdle_5,
        A_SwordIdle_6,
            
    }
    public enum Idle_States
    {
        NONE,
        A_Idle,
        A_Idle_HeadSpin,
        A_Floating,
        A_Axe_Idle,
        A_NinjaIdle_1,
        A_NinjaIdle_2,
        A_NinjaIdle_3,
        A_NinjaIdle_4,
        A_SwordIdle_1,
        A_SwordIdle_2,
        A_SwordIdle_3,
        A_SwordIdle_4,
        A_SwordIdle_5,
        A_SwordIdle_6,
    }
    public enum AirbonedFlying_States
    {
        NONE,
        A_Falling_Idle,
        A_AirbonedRolling,
    }
    public enum AttackPrep_States
    {
        NONE,
        A_Shoryuken_DownSmash_Prep,
        A_SendFireball_Front,
        A_HeadSpin_Attack,
    }
    public enum StaticAttack_States
    {
        NONE,
        A_Shoryuken_Prep_Static,
        A_Shoryuken_Landing_Static,
        A_Shoryuken_Rise_Static,
        A_SendFireball_Front_Static,
        A_HeadSpin_Attack_Static,
        A_HeadSpin_Attack_Prep_Static
    }
    public enum AttackFinish_States
    {
        NONE,
        A_Shoryuken_DownSmash_Finish,
    }
    public enum Landing_States
    {
        NONE,
        A_Fall_Landing,
        A_AirbonedRolling_Landing,
        A_Idle_HeadSpin,
    }

    public enum HitReaction_States
    {
        NONE,
        A_HitReaction,
        A_HitReaction_Gut,
        A_HitReaction_HeadHit,
        A_HitReaction_HeadHit2,
        A_HitReaction_Left,
    }

    public enum Death_States
    {
        NONE,
        A_Sweep_Fall,
    }
    #endregion
    public class StatesDispatcher : MonoBehaviour
    {

        [Header("State Dictionaries")]
        public SerializedDictionary<AttackPrep_States, int> attackPrep_Dictionary;
        public SerializedDictionary<AttackFinish_States, int> attackFinish_Dictionary;
        public SerializedDictionary<StaticAttack_States, int> staticAttack_States_Dictionary;
        public SerializedDictionary<StateNames, int> stateNames_Dictionary;
        public SerializedDictionary<Idle_States, int> idle_Dictionary;
        public SerializedDictionary<AirbonedFlying_States, int> airbonedFlying_Dictionary;
        public SerializedDictionary<Landing_States, int> landingNames_Dictionary;
        public SerializedDictionary<HitReaction_States, int> hitReaction_Dictionary;
        public SerializedDictionary<Death_States, int> death_States_Dictionary;


        private void Awake()
        {
            //Init Dictionaries
            stateNames_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<StateNames>(this.gameObject);
            idle_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Idle_States>(this.gameObject);
            airbonedFlying_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<AirbonedFlying_States>(this.gameObject);
            attackPrep_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<AttackPrep_States>(this.gameObject);
            attackFinish_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<AttackFinish_States>(this.gameObject);
            landingNames_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Landing_States>(this.gameObject);
            staticAttack_States_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<StaticAttack_States>(this.gameObject);
            hitReaction_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<HitReaction_States>(this.gameObject);
            death_States_Dictionary = GameLoader.Instance.hashManager.CreateAndInitDictionary<Death_States>(this.gameObject);
        }
    }
}