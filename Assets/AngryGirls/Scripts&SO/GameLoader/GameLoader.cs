using UnityEngine;

namespace Angry_Girls
{
    public class GameLoader : MonoBehaviour
    {
        public static GameLoader Instance { get; private set; }
        public MyExtentions myExtentions;
        public SpawnManager spawnManager;
        public PoolManager poolManager;
        public PoolObjectLoader poolObjectLoader;

        public CharacterManager characterManager;
        public HashManager hashManager; 
        public StatesContainer statesContainer;
        public VFXManager VFXManager;
        public AudioManager audioManager;
        public GameLogic_UIManager gameLogic_UIManager;
        public AttackLogicContainer attackLogicContainer;

        public CameraManager cameraManager;
        public LaunchHandler launchManager;
        public TurnManager turnManager;
        public GameLogic gameLogic;
        public CameraSizer cameraSizer;
        public GameLoader_Settings_Container gameLoaderSettingsContainer;
        public PauseControl pauseControl;

        public InputManager inputManager;

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            characterManager = GetComponentInChildren<CharacterManager>();
            myExtentions = GetComponentInChildren<MyExtentions>();
            cameraManager = GetComponentInChildren<CameraManager>();
            launchManager = GetComponentInChildren<LaunchHandler>();
            hashManager = GetComponentInChildren<HashManager>();
            spawnManager = GetComponentInChildren<SpawnManager>();
            turnManager = GetComponentInChildren<TurnManager>();
            poolManager = GetComponentInChildren<PoolManager>();
            poolObjectLoader = GetComponentInChildren<PoolObjectLoader>();
            VFXManager = GetComponentInChildren<VFXManager>();
            statesContainer = GetComponentInChildren<StatesContainer>();
            audioManager = GetComponentInChildren<AudioManager>();
            gameLogic_UIManager = GetComponentInChildren<GameLogic_UIManager>();
            attackLogicContainer = GetComponentInChildren<AttackLogicContainer>();
            gameLogic = GetComponentInChildren<GameLogic>();
            cameraSizer = GetComponentInChildren<CameraSizer>();
            inputManager = GetComponentInChildren<InputManager>();
            gameLoaderSettingsContainer = GetComponentInChildren<GameLoader_Settings_Container>();
            pauseControl = GetComponentInChildren<PauseControl>();
        }
    }
}