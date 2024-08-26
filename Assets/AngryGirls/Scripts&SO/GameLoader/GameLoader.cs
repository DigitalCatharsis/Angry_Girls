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

        private CharacterManager _characterManager;
        private HashManager _hashManager;
        private StatesDispatcher _statesDispatcher;
        public VFXManager VFXManager;

        private CameraManager _ñameraManager;
        private LaunchManager _launchManager;
        private TurnManager _turnManager;

        public GameLoaderMediator gameLoaderMediator;

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            _characterManager = GetComponentInChildren<CharacterManager>();
            myExtentions = GetComponentInChildren<MyExtentions>();
            _ñameraManager = GetComponentInChildren<CameraManager>();
            _launchManager = GetComponentInChildren<LaunchManager>();
            _hashManager = GetComponentInChildren<HashManager>();
            spawnManager = GetComponentInChildren<SpawnManager>();
            _turnManager = GetComponentInChildren<TurnManager>();
            poolManager = GetComponentInChildren<PoolManager>();
            poolObjectLoader = GetComponentInChildren<PoolObjectLoader>();
            VFXManager = GetComponentInChildren<VFXManager>();
            _statesDispatcher = GetComponentInChildren<StatesDispatcher>();

            foreach (var component in GetComponentsInChildren<GameLoaderComponent>())
            {
                component.OnComponentEnable();
            }

            gameLoaderMediator = new GameLoaderMediator(_ñameraManager, _launchManager, _turnManager);
        }
    }
}

public abstract class GameLoaderComponent : MonoBehaviour
{
    public abstract void OnComponentEnable();
}