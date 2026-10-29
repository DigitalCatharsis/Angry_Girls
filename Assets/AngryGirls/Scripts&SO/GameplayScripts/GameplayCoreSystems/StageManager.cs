using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages stage progression and spawn configuration.
    /// Delegates character management to GameplayCharactersManager.
    /// </summary>
    public class StageManager : GameplayManagerClass
    {
        [SerializeField] private List<StageData> _stages;
        private int _currentStageIndex = 0;
        private CharacterLauncher _currentCharacterLauncher;
        //private TurnManager _turnManager;
        private GameplayCharactersManager _charactersManager;

        // Stages that are active on the selected difficulty. Stage indices always refer to this list.
        private readonly List<StageData> _activeStages = new();
        private MissionDifficulty _difficulty = MissionDifficulty.Normal;
        private int _enemyLevel = 1;
        private DifficultyTuning _difficultyTuning = new();

        public int CurrentStageIndex => _currentStageIndex;
        public int StageCount => _activeStages.Count;
        public int EnemyLevel => _enemyLevel;
        public MissionDifficulty Difficulty => _difficulty;
        public CharacterLauncher CurrentCharacterLauncher => _currentCharacterLauncher;

        public Action<int> TheStageIsSet;
        public Action ClearScene;
        public Action<CControl> UnitSpawned;

        public override void Initialize()
        {
            //_turnManager = GameplayCoreManager.Instance.TurnManager;
            _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;

            ResolveMissionSettings();
            BuildActiveStages();

            isInitialized = true;
        }

        /// <summary>
        /// Reads the selected mission and difficulty: they define the enemy level and the stage setup.
        /// </summary>
        private void ResolveMissionSettings()
        {
            var core = CoreManager.Instance;
            var missions = core.MissionsManager;

            _difficulty = missions.CurrentDifficulty;
            _enemyLevel = missions.GetCurrentEnemyLevel();
            _difficultyTuning = core.ProgressionConfig.GetDifficulty(_difficulty);

            Debug.Log($"StageManager: mission {missions.CurrentMission}, difficulty {_difficulty}, enemy level {_enemyLevel}");
        }

        /// <summary>
        /// Keeps only the stages that are enabled for the selected difficulty.
        /// </summary>
        private void BuildActiveStages()
        {
            _activeStages.Clear();

            foreach (var stage in _stages)
            {
                if (stage == null || stage.minDifficulty > _difficulty)
                    continue;

                _activeStages.Add(stage);
            }

            if (_activeStages.Count == 0)
            {
                Debug.LogError($"StageManager: no stages are available for difficulty {_difficulty}.", this);
                return;
            }

            var mission = CoreManager.Instance.MissionsManager.GetMission(CoreManager.Instance.MissionsManager.CurrentMission);
            var configuredCount = mission != null ? mission.GetStageCount(_difficulty) : 0;
            if (configuredCount > 0 && configuredCount != _activeStages.Count)
            {
                Debug.LogWarning($"StageManager: mission data says {configuredCount} stages for {_difficulty}, the scene has {_activeStages.Count}.", this);
            }
        }

        public void SetupCurrentStage()
        {
            ClearCurrentScene();
            
            var stage = _activeStages[_currentStageIndex];
            _currentCharacterLauncher = SpawnCharacterLauncher(stage).GetComponent<CharacterLauncher>();
            _currentCharacterLauncher.InitLauncher();

            SpawnEnemies(stage);

            // Only spawn initial player characters on first stage
            if (_currentStageIndex == 0)
            {
                _charactersManager.SpawnInitialPlayerCharacters(_currentCharacterLauncher.UnitsTransforms);
                foreach (var unit in _charactersManager.PlayerCharacters)
                {
                    UnitSpawned?.Invoke(unit);
                }
            }

            TheStageIsSet?.Invoke(_currentStageIndex);
        }

        public void ProceedToNextStage()
        {
            _charactersManager.ReinitSurvivingCharacters();
            _currentStageIndex++;
        }

        public bool HasNextStage()
        {
            return _currentStageIndex + 1 < _activeStages.Count;
        }

        private void SpawnEnemies(StageData stage)
        {
            foreach (var enemyData in stage.enemies)
            {
                if (enemyData == null || enemyData.CharacterSettings == null || enemyData.spawnTransform == null)
                {
                    Debug.LogWarning("StageManager: Skipping enemy with missing settings or spawn transform.", this);
                    continue;
                }

                // Enemies that belong to a higher difficulty only are not spawned here.
                if (enemyData.minDifficulty > _difficulty)
                    continue;

                var characterProfile = new CharacterProfile(enemyData.CharacterSettings);
                characterProfile.ConfigureAsEnemy(
                    _enemyLevel,
                    _difficultyTuning.enemyHealthMultiplier,
                    _difficultyTuning.enemyDamageMultiplier);
                characterProfile.ModifyBonusStats(enemyData.BonusStats);

                var character = _charactersManager.SpawnEnemy(characterProfile, enemyData.spawnTransform);
                if (character != null)
                    UnitSpawned?.Invoke(character);
            }
        }

        private void ClearCurrentScene()
        {
            ClearScene?.Invoke();
            //_turnManager.ClearTurnList();
            CurrentCharacterLauncher?.gameObject.SetActive(false);
        }

        public GameObject SpawnCharacterLauncher(StageData stage)
        {
            return Instantiate(stage.characterLauncherData.characterLauncher, stage.characterLauncherData.spawnTransform);
        }
    }

    [System.Serializable]
    public class StageData
    {
        [Tooltip("The stage exists on this difficulty and on all harder ones.")]
        public MissionDifficulty minDifficulty = MissionDifficulty.Easy;
        public List<EnemyStageData> enemies;
        public CharacterLauncherData characterLauncherData;
    }

    [System.Serializable]
    public class EnemyStageData
    {
        [Tooltip("The enemy spawns on this difficulty and on all harder ones.")]
        public MissionDifficulty minDifficulty = MissionDifficulty.Easy;
        public CharactersStatsBase BonusStats;
        public CharacterSettings CharacterSettings;
        public Transform spawnTransform;
    }

    [System.Serializable]
    public class CharacterLauncherData
    {
        public GameObject characterLauncher;
        public Transform spawnTransform;
    }
}
