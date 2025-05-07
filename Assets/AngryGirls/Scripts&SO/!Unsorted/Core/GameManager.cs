using System;
using UnityEngine;

namespace Angry_Girls
{
    public class GameManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private LaunchHandler _launchHandler;
        [SerializeField] private BattleController _battleController;
        [SerializeField] private PlayerData _playerData;

        private int _launchesCountInCurrentPhase;
        private GameState _currentState;

        private void Awake()
        {
            GameStateEvent.OnStateChanged += HandleStateChange;
        }

        private void Start()
        {
            GameStateEvent.Trigger(GameState.GameStart);
        }

        private void HandleStateChange(GameState newState)
        {
            _currentState = newState;

            switch (newState)
            {
                case GameState.GameStart:
                    InitializeGame();
                    break;

                case GameState.StageSetup:
                    SetupStage();
                    break;

                case GameState.LaunchPhase:
                    StartLaunchPhase();
                    break;

                case GameState.AlternatePhase:
                    StartAlternatePhase();
                    break;

                case GameState.StageComplete:
                    HandleStageCompletion();
                    break;

                case GameState.Victory:
                    ExecuteVictory();
                    break;

                case GameState.Defeat:
                    ExecuteDefeat();
                    break;
            }
        }

        private void InitializeGame()
        {
            // Первая инициализация игры
            _stageManager.SetupCurrentStage(
                onComplete: () => GameStateEvent.Trigger(GameState.StageSetup),
                selectedCharacters: _playerData.selectedCharacters,
                isInitialStage: true
            );
        }

        private void SetupStage()
        {
            _launchesCountInCurrentPhase = 0;
            GameStateEvent.Trigger(GameState.LaunchPhase);
        }

        private void StartLaunchPhase()
        {
            _launchHandler.BeginLaunchPhase();
            _launchHandler.OnCharacterLaunched += HandleCharacterLaunched;
        }

        private void HandleCharacterLaunched()
        {
            _launchesCountInCurrentPhase++;

            // Проверяем условия после каждого запуска
            if (!_battleController.HasActivePlayerCharacters())
            {
                GameStateEvent.Trigger(GameState.Defeat);
                return;
            }

            if (_launchesCountInCurrentPhase >= 2)
            {
                _launchHandler.OnCharacterLaunched -= HandleCharacterLaunched;
                GameStateEvent.Trigger(GameState.AlternatePhase);
            }
        }

        private void StartAlternatePhase()
        {
            _turnManager.StartAlternatePhase(OnAlternatePhaseComplete);
        }

        private void OnAlternatePhaseComplete()
        {
            if (_battleController.AllEnemiesDefeated())
            {
                GameStateEvent.Trigger(GameState.StageComplete);
            }
            else if (!_battleController.HasActivePlayerCharacters())
            {
                GameStateEvent.Trigger(GameState.Defeat);
            }
            else
            {
                // Возвращаемся к фазе запуска
                GameStateEvent.Trigger(GameState.LaunchPhase);
            }
        }

        private void HandleStageCompletion()
        {
            if (_stageManager.HasNextStage())
            {
                _stageManager.ProceedToNextStage();
            }
            else
            {
                GameStateEvent.Trigger(GameState.Victory);
            }
        }

        private void ExecuteVictory()
        {
            Debug.Log("VICTORY!");
            // Показать UI победы, сохранить прогресс и т.д.
        }

        private void ExecuteDefeat()
        {
            Debug.Log("DEFEAT!");
            // Показать UI поражения
        }

        public void PauseGame(bool pause)
        {
            GameStateEvent.Trigger(pause ? GameState.Paused : _currentState);
        }
    }
}