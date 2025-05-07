using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private StageData[] _stages;
        [SerializeField] private float _characterSpawnOffset = 1f;
        [SerializeField] private GameObject _characterLauncherPrefab;

        private int _currentStageIndex = 0;
        private List<CControl> _survivingCharacters = new();
        private GameObject _currentCharacterLauncher;
        private PoolManager _poolManager;
        // Для других систем
        public List<CControl> GetSurvivingCharacters() => new(_survivingCharacters);
        public int CurrentStageIndex => _currentStageIndex;

        public bool HasNextStage() => _currentStageIndex < _stages.Length - 1;
        private void Awake()
        {
            _poolManager = GameLoader.Instance.poolManager;

            // Лучше загружать префаб один раз при старте
            if (_characterLauncherPrefab == null)
                _characterLauncherPrefab = Resources.Load<GameObject>("CharacterLauncher");
        }

        public void SetupCurrentStage(
            Action onComplete,
            CharacterSettings[] selectedCharacters,
            bool isInitialStage = false)
        {
            CleanupPreviousStage();

            if (isInitialStage)
            {
                InitializeFirstStage(selectedCharacters);
            }
            else
            {
                PrepareSurvivingCharacters();
            }

            SpawnEnemies();
            PositionCharacters();

            onComplete?.Invoke();
        }

        private void CleanupPreviousStage()
        {
            if (_currentCharacterLauncher != null)
            {
                Destroy(_currentCharacterLauncher);
            }

            // Спавним новый лаунчер для текущей стадии
            _currentCharacterLauncher = Instantiate(
                _characterLauncherPrefab,
                _stages[_currentStageIndex].characterLauncherTransform.position,
                Quaternion.identity
            );
        }

        private void InitializeFirstStage(CharacterSettings[] selectedCharacters)
        {
            _currentStageIndex = 0;
            _survivingCharacters.Clear();

            foreach (var characterSettings in selectedCharacters)
            {
                if (characterSettings == null) continue;

                var character = SpawnCharacter(
                    characterSettings.characterType,
                    Vector3.zero // Позиция будет установлена в PositionCharacters
                );

                _survivingCharacters.Add(character.GetComponent<CControl>());
            }
        }

        private void PrepareSurvivingCharacters()
        {
            // Удаляем мертвых персонажей
            _survivingCharacters.RemoveAll(character =>
                character == null || character.isDead);
        }

        private void SpawnEnemies()
        {
            foreach (var enemy in _stages[_currentStageIndex].enemiesToSpawn)
            {
                var enemyObj = SpawnCharacter(
                    enemy.Key,
                    enemy.Value.position
                );

                _survivingCharacters.Add(enemyObj.GetComponent<CControl>());
            }
        }

        private GameObject SpawnCharacter(CharacterType type, Vector3 position)
        {
            return _poolManager.GetObject(
                type,
                _poolManager.characterPoolDictionary,
                position,
                Quaternion.identity
            ).gameObject;
        }

        private void PositionCharacters()
        {
            float offset = 0f;

            foreach (var character in _survivingCharacters)
            {
                if (character.isDead) continue;

                var newPosition = new Vector3(
                    _currentCharacterLauncher.transform.position.x,
                    _currentCharacterLauncher.transform.position.y,
                    _currentCharacterLauncher.transform.position.z - offset
                );

                character.transform.position = newPosition;
                offset += _characterSpawnOffset;
            }
        }

        public void ProceedToNextStage()
        {
            _currentStageIndex++;
            GameStateEvent.Trigger(GameState.StageSetup);
        }
    }
}