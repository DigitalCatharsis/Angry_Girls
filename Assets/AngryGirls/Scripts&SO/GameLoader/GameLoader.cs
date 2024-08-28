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
        public StatesDispatcher statesDispatcher;
        public VFXManager VFXManager;
        public AudioManager audioManager;

        public CameraManager cameraManager;
        public LaunchManager launchManager;
        public TurnManager turnManager;

        public GameLoaderMediator gameLoaderMediator;

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
            launchManager = GetComponentInChildren<LaunchManager>();
            hashManager = GetComponentInChildren<HashManager>();
            spawnManager = GetComponentInChildren<SpawnManager>();
            turnManager = GetComponentInChildren<TurnManager>();
            poolManager = GetComponentInChildren<PoolManager>();
            poolObjectLoader = GetComponentInChildren<PoolObjectLoader>();
            VFXManager = GetComponentInChildren<VFXManager>();
            statesDispatcher = GetComponentInChildren<StatesDispatcher>();
            audioManager = GetComponentInChildren<AudioManager>();

            gameLoaderMediator = new GameLoaderMediator(cameraManager, launchManager);
        }
    }
}