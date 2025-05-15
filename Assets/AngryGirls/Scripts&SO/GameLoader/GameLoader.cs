using UnityEngine;

namespace Angry_Girls
{
    public class GameLoader : MonoBehaviour
    {
        public static GameLoader Instance { get; private set; }

        public PoolManager poolManager;
        public PoolObjectLoader poolObjectLoader;
        public CharacterManager characterManager;
        public LaunchManager launchManager;
        public TurnManager turnManager;
        public GameLogic gameLogic;
        public GameLogic_UIManager gameLogic_UIManager;
        public CameraManager cameraManager;
        public InputManager inputManager;
        public InteractionManager interactionManager;
        public AttackLogicContainer attackLogicContainer;
        public StatesContainer statesContainer;
        public LevelSettings levelSettings;
        public HashManager hashManager;
        public AudioManager audioManager;
        public VFXManager VFXManager;
        public StageManager stageManager;
        public GameFlowController gameFlowController;
        public GameLoader_Settings_Container gameLoaderSettingsContainer;
        public PauseControl pauseControl;
        public MyExtentions myExtentions;

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            poolManager = GetComponentInChildren<PoolManager>();
            poolObjectLoader = GetComponentInChildren<PoolObjectLoader>();
            characterManager = GetComponentInChildren<CharacterManager>();
            launchManager = GetComponentInChildren<LaunchManager>();
            turnManager = GetComponentInChildren<TurnManager>();
            gameLogic = GetComponentInChildren<GameLogic>();
            gameLogic_UIManager = GetComponentInChildren<GameLogic_UIManager>();
            cameraManager = GetComponentInChildren<CameraManager>();
            inputManager = GetComponentInChildren<InputManager>();
            interactionManager = GetComponentInChildren<InteractionManager>();
            attackLogicContainer = GetComponentInChildren<AttackLogicContainer>();
            statesContainer = GetComponentInChildren<StatesContainer>();
            levelSettings = GetComponentInChildren<LevelSettings>();
            hashManager = GetComponentInChildren<HashManager>();
            audioManager = GetComponentInChildren<AudioManager>();
            VFXManager = GetComponentInChildren<VFXManager>();
            stageManager = GetComponentInChildren<StageManager>();
            gameFlowController = GetComponentInChildren<GameFlowController>();
            gameLoaderSettingsContainer = GetComponentInChildren<GameLoader_Settings_Container>();
            pauseControl = GetComponentInChildren<PauseControl>();
            myExtentions = GetComponentInChildren<MyExtentions>();
        }
    }

    public static class LaunchHandlerExtensions
    {
        public static void BeginLaunchPhase(this LaunchManager handler, System.Action onComplete)
        {
            handler.StartCoroutine(handler.BeginLaunchPhaseRoutine(onComplete));
        }
    }

    public static class TurnManagerExtensions
    {
        public static void ExecuteAlternatePhase(this TurnManager manager, System.Action onComplete)
        {
            manager.StartCoroutine(manager.AlternatePhaseRoutine(onComplete));
        }
    }
}
