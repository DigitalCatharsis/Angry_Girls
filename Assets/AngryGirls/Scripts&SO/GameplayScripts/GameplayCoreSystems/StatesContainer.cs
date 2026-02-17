using AYellowpaper.SerializedCollections;

namespace Angry_Girls
{
    #region enums
    public enum StateNames
    {
        NONE,
        Idle_General,
        Idle_HeadSpin,
        Idle_Floating_Air,
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

        Launch_Uppercut_Prep,
        Launch_SendFireball_Front,
        Launch_HeadSpin_Attack,
        Launch_SwordAttack_Attack,
        Launch_SwordAttack_Prep,
        Alternate_Uppercut_Prep,
        Alternate_SendFireball_Front,
        Alternate_HeadSpin_Attack_Prep,
        Alternate_HeadSpin_Attack,
        Alternate_SwordAttack_Prep,
        Alternate_SwordAttack_Attack,

        Launch_Uppercut_Finish,
        Alternate_Uppercut_Finish,

        HitReaction_1,
        HitReaction_2,
        HitReaction_3,
        HitReaction_4,
        HitReaction_5,

        Death_SweepFall,
        Falling_Idle_HandAverage
    }

    public enum Idle_States
    {
        NONE,
        Idle_General,
        Idle_HeadSpin,
        Idle_Floating_Air,
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
        Falling_Idle_HandAverage
    }

    public enum Landing_States
    {
        NONE,
        Landing_General,
        Landing_Rolling,
        Landing_SwordAttack,
    }

    public enum Attack_States
    {
        NONE,
        Launch_Uppercut_Prep,
        Launch_SendFireball_Front,
        Launch_HeadSpin_Attack,
        Launch_SwordAttack_Attack,
        Launch_SwordAttack_Prep,
        Alternate_Uppercut_Prep,
        Alternate_SendFireball_Front,
        Alternate_HeadSpin_Attack,
        Alternate_SwordAttack_Prep,
        Alternate_SwordAttack_Attack,
    }

    public enum AttackFinish_States
    {
        NONE,
        Launch_Uppercut_Finish,
        Alternate_Uppercut_Finish,
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

    /// <summary>
    /// Container for animation state dictionaries and hash management
    /// </summary>
    public static class StatesContainer
    {
        private static SerializedDictionary<Attack_States, int> _attackDictionary;
        private static SerializedDictionary<AttackFinish_States, int> _attackFinishDictionary;
        private static SerializedDictionary<StateNames, int> _stateNamesDictionary;
        private static SerializedDictionary<Idle_States, int> _idleDictionary;
        private static SerializedDictionary<Falling_States, int> _fallingDictionary;
        private static SerializedDictionary<Landing_States, int> _landingDictionary;
        private static SerializedDictionary<HitReaction_States, int> _hitReactionDictionary;
        private static SerializedDictionary<Death_States, int> _deathDictionary;

        public static SerializedDictionary<Attack_States, int> AttackDictionary
        {
            get
            {
                EnsureInitialized();
                return _attackDictionary;
            }
        }

        public static SerializedDictionary<AttackFinish_States, int> AttackFinishDictionary
        {
            get
            {
                EnsureInitialized();
                return _attackFinishDictionary;
            }
        }

        public static SerializedDictionary<StateNames, int> StateNamesDictionary
        {
            get
            {
                EnsureInitialized();
                return _stateNamesDictionary;
            }
        }

        public static SerializedDictionary<Idle_States, int> IdleDictionary
        {
            get
            {
                EnsureInitialized();
                return _idleDictionary;
            }
        }

        public static SerializedDictionary<Falling_States, int> FallingDictionary
        {
            get
            {
                EnsureInitialized();
                return _fallingDictionary;
            }
        }

        public static SerializedDictionary<Landing_States, int> LandingDictionary
        {
            get
            {
                EnsureInitialized();
                return _landingDictionary;
            }
        }

        public static SerializedDictionary<HitReaction_States, int> HitReactionDictionary
        {
            get
            {
                EnsureInitialized();
                return _hitReactionDictionary;
            }
        }

        public static SerializedDictionary<Death_States, int> DeathDictionary
        {
            get
            {
                EnsureInitialized();
                return _deathDictionary;
            }
        }

        // Flag for checking initialization
        private static bool _isInitialized = false;

        /// <summary>
        /// Static constructor
        /// </summary>
        static StatesContainer()
        {
            InitializeDictionaries();
        }

        /// <summary>
        /// Initialize all dictionaries
        /// </summary>
        private static void InitializeDictionaries()
        {
            if (_isInitialized) return;

            var hashManager = CoreManager.Instance.HashManager;

            _stateNamesDictionary = hashManager.CreateAndInitDictionary<StateNames>();
            _idleDictionary = hashManager.CreateAndInitDictionary<Idle_States>();
            _fallingDictionary = hashManager.CreateAndInitDictionary<Falling_States>();
            _attackDictionary = hashManager.CreateAndInitDictionary<Attack_States>();
            _attackFinishDictionary = hashManager.CreateAndInitDictionary<AttackFinish_States>();
            _landingDictionary = hashManager.CreateAndInitDictionary<Landing_States>();
            _hitReactionDictionary = hashManager.CreateAndInitDictionary<HitReaction_States>();
            _deathDictionary = hashManager.CreateAndInitDictionary<Death_States>();

            _isInitialized = true;
        }
        /// <summary>
        /// Checks initialization and initializes if necessary
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                InitializeDictionaries();
            }
        }

        /// <summary>
        /// Gets state name by hash value (for debugging)
        /// </summary>
        public static string GetStateNameByHash(int hash)
        {
            EnsureInitialized();
            var manager = CoreManager.Instance.HashManager;
            return manager.GetName(StateNamesDictionary, hash).ToString();
        }

        /// <summary>
        /// Reinitialize dictionaries (if necessary)
        /// </summary>
        public static void Reinitialize()
        {
            _isInitialized = false;
            InitializeDictionaries();
        }
    }
}