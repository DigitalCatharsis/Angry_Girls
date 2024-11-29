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
        public UIManager UIManager;
        public AttackLogicContainer attackLogicContainer;

        public CameraManager cameraManager;
        public LaunchHandler launchManager;
        public TurnManager turnManager;
        public GameLogic gameLogic;

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
            UIManager = GetComponentInChildren<UIManager>();
            attackLogicContainer = GetComponentInChildren<AttackLogicContainer>();
            gameLogic = GetComponentInChildren<GameLogic>();
        }
    }
}