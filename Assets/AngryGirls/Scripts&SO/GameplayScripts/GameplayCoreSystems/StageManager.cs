using AYellowpaper.SerializedCollections;
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
        private TurnManager _turnManager;
        private GameplayCharactersManager _charactersManager;

        public int CurrentStageIndex => _currentStageIndex;
        public CharacterLauncher CurrentCharacterLauncher => _currentCharacterLauncher;

        public override void Initialize()
        {
            _turnManager = GameplayCoreManager.Instance.TurnManager;
            _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            isInitialized = true;
        }

        public void SetupCurrentStage()
        {
            ClearCurrentScene();
            
            var stage = _stages[_currentStageIndex];
            _currentCharacterLauncher = SpawnCharacterLauncher(stage).GetComponent<CharacterLauncher>();
            _currentCharacterLauncher.InitLauncher();

            SpawnEnemies(stage);

            // Only spawn initial player characters on first stage
            if (_currentStageIndex == 0)
            {
                _charactersManager.SpawnInitialPlayerCharacters(_currentCharacterLauncher.UnitsTransforms);
            }
        }

        public void ProceedToNextStage()
        {
            _charactersManager.ReinitSurvivingCharacters();
            _currentStageIndex++;
        }

        public bool HasNextStage()
        {
            return _currentStageIndex + 1 < _stages.Count;
        }

        private void SpawnEnemies(StageData stage)
        {
            foreach (var enemyData in stage.enemies)
            {
                var CharData = new CharacterProfile(enemyData.CharacterSettings);
                CharData.ModifyBonusStats(enemyData.BonusStats);
                var character = _charactersManager.SpawnEnemy(CharData, enemyData.spawnTransform.position);
            }
        }

        private void ClearCurrentScene()
        {
            _turnManager.ClearTurnList();
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
        public List<EnemyStageData> enemies;
        public CharacterLauncherData characterLauncherData;
    }

    [System.Serializable]
    public class EnemyStageData
    {
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