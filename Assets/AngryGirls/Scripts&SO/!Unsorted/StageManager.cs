using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static Codice.Client.BaseCommands.Import.Commit;

namespace Angry_Girls
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private List<StageData> _stages;
        private int _currentStageIndex = 0;
        private CharacterLauncher _currentCharacterLauncher;

        private List<CControl> _survivingCharacters = new();

        public void SetupCurrentStage()
        {
            ClearCurrentScene();

            var stage = _stages[_currentStageIndex];

            _currentCharacterLauncher = SpawnCharacterLauncher(stage).GetComponent<CharacterLauncher>();
            _currentCharacterLauncher.InitLauncher();

            SpawnEnemies(stage);

            if (_currentStageIndex == 0)
            {
                SpawnInitialCharacters();
            }
            else
            {
                RespawnSurvivingCharacters();
            }

            GameLoader.Instance.cameraManager.ReturnCameraToStartPosition(1f);
        }

        public GameObject SpawnCharacterLauncher(StageData stage)
        {
            return Instantiate(stage.characterLauncherData.characterLauncher, stage.characterLauncherData.spawnTransform) as GameObject;
        }

        public void ProceedToNextStage()
        {
            _survivingCharacters = GameLoader.Instance.characterManager.playableCharacters
                .FindAll(c => !c.isDead);

            _currentStageIndex++;
        }

        public bool HasNextStage()
        {
            return _currentStageIndex + 1 < _stages.Count;
        }

        private void SpawnInitialCharacters()
        {
            List<CControl> spawned = new();

            for (int i =0; i < GameLoader.Instance.levelSettings.playerData.selectedCharacters.Count(); i++)
            {
                var character = GameLoader.Instance.levelSettings.playerData.selectedCharacters[i];

                if (character == null) continue;

                spawned.Add(SpawnUnit(character, _currentCharacterLauncher.UnitsTransforms[i].position, Quaternion.identity).GetComponent<CControl>());
            }

            foreach (var character in spawned)
            {
                GameLoader.Instance.characterManager.playableCharacters.Add(character);
            }
        }

        private PoolObject SpawnUnit(CharacterSettings characterSettings, Vector3 spawnPosition, Quaternion rotation)
        {
                return GameLoader.Instance.poolManager.GetObject(characterSettings.characterType, spawnPosition, rotation);
        }

        private void RespawnSurvivingCharacters()
        {
            GameLoader.Instance.characterManager.playableCharacters.Clear();
            foreach (var c in _survivingCharacters)
            {
                GameLoader.Instance.characterManager.playableCharacters.Add(c);
            }
        }

        private void SpawnEnemies(StageData stage)
        {
            var characterSettings = new List<CharacterSettings>();

            for (int i = 0; i < stage.enemies.Count;  i++)
            {
                var unit = SpawnUnit(stage.enemies[i].CharacterSettings, stage.enemies[i].spawnTransform.position, Quaternion.identity);
                GameLoader.Instance.characterManager.enemyCharacters.Add(unit.GetComponent<CControl>());
            }
        }

        private void ClearCurrentScene()
        {
            GameLoader.Instance.characterManager.enemyCharacters.Clear();
            GameLoader.Instance.characterManager.playableCharacters.RemoveAll(c => c.isDead);
        }
    }

    [System.Serializable]
    public class StageData
    {
        public List<EnemyData> enemies;
        public CharacterLauncherData characterLauncherData;
    }

    [System.Serializable]
    public class EnemyData
    {
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