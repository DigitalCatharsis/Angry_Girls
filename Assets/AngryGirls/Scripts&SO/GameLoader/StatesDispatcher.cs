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
        A_ShootArrow,
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
    }
    public enum Idle_States
    {
        NONE,
        A_Idle,
        A_Idle_HeadSpin,
        A_Floating,
        A_Axe_Idle,
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
        A_ShootArrow,
        A_HeadSpin_Attack,
    }
    public enum StaticAttack_States
    {
        NONE,
        A_Shoryuken_Prep_Static,
        A_Shoryuken_Landing_Static,
        A_Shoryuken_Rise_Static,
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
    public class StatesDispatcher : GameLoaderComponent
    {
        public static StatesDispatcher Instance;

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

        public override void OnComponentEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Awake()
        {
            //Init Dictionaries
            stateNames_Dictionary = HashManager.Instance.CreateAndInitDictionary<StateNames>(this.gameObject);
            idle_Dictionary = HashManager.Instance.CreateAndInitDictionary<Idle_States>(this.gameObject);
            airbonedFlying_Dictionary = HashManager.Instance.CreateAndInitDictionary<AirbonedFlying_States>(this.gameObject);
            attackPrep_Dictionary = HashManager.Instance.CreateAndInitDictionary<AttackPrep_States>(this.gameObject);
            attackFinish_Dictionary = HashManager.Instance.CreateAndInitDictionary<AttackFinish_States>(this.gameObject);
            landingNames_Dictionary = HashManager.Instance.CreateAndInitDictionary<Landing_States>(this.gameObject);
            staticAttack_States_Dictionary = HashManager.Instance.CreateAndInitDictionary<StaticAttack_States>(this.gameObject);
            hitReaction_Dictionary = HashManager.Instance.CreateAndInitDictionary<HitReaction_States>(this.gameObject);
            death_States_Dictionary = HashManager.Instance.CreateAndInitDictionary<Death_States>(this.gameObject);
        }
    }
}