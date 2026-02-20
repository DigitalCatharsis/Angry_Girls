using System;
using UnityEngine;

namespace Angry_Girls
{
    public class GameplayCoreManager : MonoBehaviour
    {
        public static GameplayCoreManager Instance { get; private set; }
        public CameraManager CameraManager { get; private set; }
        public InteractionManager InteractionManager { get; private set; }
        public StageManager StageManager { get; private set; }
        public TurnManager TurnManager { get; private set; }
        public LaunchManager LaunchManager { get; private set; }
        public GameLogic GameLogic { get; private set; }
        public PhaseFlowController PhaseFlowController { get; private set; }
        public InputManager InputManager { get; private set; }
        public GameplayCharactersManager GameplayCharactersManager { get; private set; }
        public LaunchExecutionService LaunchExecutionService { get; private set; }
        public TurnOrderService TurnOrderService { get; private set; }

        public AttackAbilityManager AttackAbilityManager { get; private set; }

        public ProjectileManager ProjectileManager { get; private set; }

        public Action OnInitialized;

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeCoreSystems();
        }

        public void InitializeCoreSystems()
        {
            InputManager = InitializeSystem<InputManager>();
            CameraManager = InitializeSystem<CameraManager>();
            GameLogic = InitializeSystem<GameLogic>();

            LaunchExecutionService = InitializeSystem<LaunchExecutionService>();
            TurnOrderService = InitializeSystem<TurnOrderService>();
            GameplayCharactersManager = InitializeSystem<GameplayCharactersManager>();

            TurnManager = InitializeSystem<TurnManager>();
            StageManager = InitializeSystem<StageManager>();
            InteractionManager = InitializeSystem<InteractionManager>();
            LaunchManager = InitializeSystem<LaunchManager>();
            ProjectileManager = InitializeSystem<ProjectileManager>();

            AttackAbilityManager = InitializeSystem<AttackAbilityManager>();
            PhaseFlowController = InitializeSystem<PhaseFlowController>();

            Debug.Log("All gameplay systems initialized successfully");
            OnInitialized?.Invoke();
        }

        private T InitializeSystem<T>() where T : GameplayManagerClass
        {
            T system = GetComponentInChildren<T>();
            if (system == null)
            {
                system = gameObject.AddComponent<T>();
                Debug.Log($"Added missing component: {typeof(T).Name}");
            }
            system.Initialize();
            return system;
        }
    }
}