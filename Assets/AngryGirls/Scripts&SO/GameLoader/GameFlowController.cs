using UnityEngine;
using System.Collections.Generic;

namespace Angry_Girls
{
    public class GameFlowController : MonoBehaviour
    {
        private Dictionary<GameState, IPhase> _phases;
        private IPhase _currentPhase;
        public GameState CurrentState { get; private set; }

        private void Start()
        {
            _phases = new Dictionary<GameState, IPhase>
            {
                { GameState.GameStart, new GameStartPhase(this) },
                { GameState.StageSetup, new StageSetupPhase(this) },
                { GameState.LaunchPhase, new LaunchPhase(this) },
                { GameState.AlternatePhase, new AlternatePhase(this) },
                { GameState.StageComplete, new StageCompletePhase(this) },
                { GameState.Victory, new VictoryPhase(this) },
                { GameState.Defeat, new DefeatPhase(this) },
            };

            SwitchState(GameState.GameStart);
        }

        public void SwitchState(GameState newState)
        {
            _currentPhase?.EndPhase();
            CurrentState = newState;
            Debug.Log($"Switched to state: {newState}");
            _currentPhase = _phases[newState];
            _currentPhase.StartPhase();
        }
    }

    public enum GameState
    {
        GameStart,
        StageSetup,
        LaunchPhase,
        AlternatePhase,
        StageComplete,
        Victory,
        Defeat
    }

    public interface IPhase
    {
        void StartPhase();
        void EndPhase();
    }

    public abstract class PhaseBase : IPhase
    {
        protected GameFlowController _gameFlow;
        public PhaseBase(GameFlowController controller) => _gameFlow = controller;
        public abstract void StartPhase();
        public virtual void EndPhase() { }
    }

    public class GameStartPhase : PhaseBase
    {
        public GameStartPhase(GameFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            Debug.Log("Game Start");
            _gameFlow.SwitchState(GameState.StageSetup);
        }
    }

    public class StageSetupPhase : PhaseBase
    {
        public StageSetupPhase(GameFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            GameLoader.Instance.stageManager.SetupCurrentStage();
            _gameFlow.SwitchState(GameState.LaunchPhase);
        }
    }

    public class LaunchPhase : PhaseBase
    {
        public LaunchPhase(GameFlowController controller) : base(controller) { }

        public override void StartPhase()
        {
            // Проверка победы ДО запуска
            var checker = new BattleResultChecker();

            if (checker.AllEnemiesDefeated())
            {
                _gameFlow.SwitchState(GameState.StageComplete);
                return;
            }

            // Проверка проигрыша ДО запуска
            if (!checker.AnyPlayersAlive())
            {
                _gameFlow.SwitchState(GameState.AlternatePhase);
                return;
            }

            // Проверка на одного персонажа
            var alivePlayers = GameLoader.Instance.characterManager.playableCharacters
                .FindAll(c => !c.isDead);

            int aliveCount = alivePlayers.Count;

            // Передаём в LaunchManager
            GameLoader.Instance.launchManager.BeginLaunchPhase(() =>
            {
                // После запуска проверяем победу
                if (checker.AllEnemiesDefeated())
                {
                    _gameFlow.SwitchState(GameState.StageComplete);
                    return;
                }

                // Если только один юнит — сразу в Alternate
                if (aliveCount <= 1 || GameLoader.Instance.launchManager.CharacterToLaunch != null)
                {
                    _gameFlow.SwitchState(GameState.AlternatePhase);
                    return;
                }

                // Иначе — повторный запуск (в случае двойного запуска)
                _gameFlow.SwitchState(GameState.LaunchPhase);
            });
        }
    }


    public class AlternatePhase : PhaseBase
    {
        public AlternatePhase(GameFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            GameLoader.Instance.turnManager.ExecuteAlternatePhase(() =>
            {
                var checker = new BattleResultChecker();
                if (checker.AllEnemiesDefeated())
                {
                    _gameFlow.SwitchState(GameState.StageComplete);
                }
                else if (!checker.AnyPlayersAlive())
                {
                    _gameFlow.SwitchState(GameState.Defeat);
                }
                else
                {
                    _gameFlow.SwitchState(GameState.LaunchPhase);
                }
            });
        }
    }

    public class StageCompletePhase : PhaseBase
    {
        public StageCompletePhase(GameFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            if (GameLoader.Instance.stageManager.HasNextStage())
            {
                GameLoader.Instance.stageManager.ProceedToNextStage();
                _gameFlow.SwitchState(GameState.StageSetup);
            }
            else
            {
                _gameFlow.SwitchState(GameState.Victory);
            }
        }
    }

    public class VictoryPhase : PhaseBase
    {
        public VictoryPhase(GameFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            GameLoader.Instance.gameLogic.ExecuteVictory();
        }
    }

    public class DefeatPhase : PhaseBase
    {
        public DefeatPhase(GameFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            GameLoader.Instance.gameLogic.ExecuteGameOver();
        }
    }

    public class BattleResultChecker
    {
        public bool AllEnemiesDefeated()
        {
            return GameLoader.Instance.characterManager.enemyCharacters.TrueForAll(c => c.isDead);
        }

        public bool AnyPlayersAlive()
        {
            return GameLoader.Instance.characterManager.playableCharacters.Exists(c => !c.isDead && !c.hasBeenLaunched);
        }
    }
}
