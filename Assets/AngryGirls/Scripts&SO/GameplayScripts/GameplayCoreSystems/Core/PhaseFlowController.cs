using UnityEngine;
using System.Collections.Generic;

namespace Angry_Girls
{
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
    /// <summary>
    /// Controls game state flow and transitions between phases
    /// </summary>
    public class PhaseFlowController : GameplayManagerClass
    {
        private Dictionary<GameState, IPhase> _phases;
        private IPhase _currentPhase;
        public IPhase CurrentPhase => _currentPhase;
        public GameState CurrentGameState { get; private set; }

        public override void Initialize()
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

            GameplayCoreManager.Instance.OnInitialized += LateInitialize;
            isInitialized = true;

        }

        private void LateInitialize()
        {
            GameplayCoreManager.Instance.OnInitialized -= LateInitialize;
            SwitchState(GameState.GameStart);
        }

        /// <summary>
        /// Transitions to new game state
        /// </summary>
        public void SwitchState(GameState newState)
        {
            _currentPhase?.EndPhase();
            CurrentGameState = newState;
            Debug.Log($"Switched to state: {newState}");
            _currentPhase = _phases[newState];
            _currentPhase.StartPhase();
        }
    }


    /// <summary>
    /// Interface for game phase implementation
    /// </summary>
    public interface IPhase
    {
        void StartPhase();
        void EndPhase();
    }

    /// <summary>
    /// Base class for all game phases
    /// </summary>
    public abstract class PhaseBase : IPhase
    {
        protected PhaseFlowController _gameFlow;
        public PhaseBase(PhaseFlowController controller) => _gameFlow = controller;
        public abstract void StartPhase();
        public virtual void EndPhase() { }
    }

    /// <summary>
    /// Initial game start phase
    /// </summary>
    public class GameStartPhase : PhaseBase
    {
        public GameStartPhase(PhaseFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            Debug.Log("Game Start");
            _gameFlow.SwitchState(GameState.StageSetup);
        }
    }

    /// <summary>
    /// Stage setup and preparation phase
    /// </summary>
    public class StageSetupPhase : PhaseBase
    {
        public StageSetupPhase(PhaseFlowController controller) : base(controller) { }
        private StageManager _stageManager;

        public override void StartPhase()
        {
            if (_stageManager == null)
            {
                _stageManager = GameplayCoreManager.Instance.StageManager;
            }

            _stageManager.SetupCurrentStage();
            _gameFlow.SwitchState(GameState.LaunchPhase);
        }
    }

    /// <summary>
    /// Character launching phase
    /// </summary>
    public class LaunchPhase : PhaseBase
    {
        public LaunchPhase(PhaseFlowController controller) : base(controller) { }

        private GameplayCharactersManager _charactersManager;

        public override void StartPhase()
        {
            if (_charactersManager == null)
            {
                _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            }

            var checker = new BattleResultChecker(_charactersManager);

            if (checker.AllEnemiesDefeated())
            {
                _gameFlow.SwitchState(GameState.StageComplete);
                return;
            }

            if (!checker.AnyPlayersAlive())
            {
                _gameFlow.SwitchState(GameState.AlternatePhase);
                return;
            }

            GameplayCoreManager.Instance.LaunchManager.BeginLaunchPhase(() =>
            {
                if (checker.AllEnemiesDefeated())
                {
                    _gameFlow.SwitchState(GameState.StageComplete);
                    return;
                }

                if (_charactersManager.GetAliveCharacters(PlayerOrAi.Player).Count <= 1 || GameplayCoreManager.Instance.LaunchManager.GetCandidateToLaunch() != null)
                {
                    _gameFlow.SwitchState(GameState.AlternatePhase);
                    return;
                }

                _gameFlow.SwitchState(GameState.LaunchPhase);
            });
        }
    }

    /// <summary>
    /// Alternate attack phase
    /// </summary>
    public class AlternatePhase : PhaseBase
    {
        public AlternatePhase(PhaseFlowController controller) : base(controller) { }

        private TurnManager _turnManager;
        private GameplayCharactersManager _gameplayCharactersManager;

        public override void StartPhase()
        {
            if (_turnManager == null)
            {
                _turnManager = GameplayCoreManager.Instance.TurnManager;
            }

            if (_gameplayCharactersManager == null)
            {
                _gameplayCharactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            }

            _turnManager.ExecuteAlternatePhase(() =>
            {
                var checker = new BattleResultChecker(_gameplayCharactersManager);
                if (checker.AllEnemiesDefeated())
                {
                    if (GameplayCoreManager.Instance.StageManager.HasNextStage())
                    {
                        _gameFlow.SwitchState(GameState.StageComplete);
                    }
                    else
                    {
                        _gameFlow.SwitchState(GameState.Victory);
                    }
                    return;
                }

                if (!checker.AnyLaunchableCharactersLeft())
                {
                    _gameFlow.SwitchState(GameState.Defeat);
                    return;
                }
                _gameFlow.SwitchState(GameState.LaunchPhase);
            });
        }
    }
    public class StageCompletePhase : PhaseBase
    {
        private StageManager _stageManager;
        public StageCompletePhase(PhaseFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            if (_stageManager == null)
            {
                _stageManager = GameplayCoreManager.Instance.StageManager;
            }

            if (_stageManager.HasNextStage())
            {
                _stageManager.ProceedToNextStage();
                _gameFlow.SwitchState(GameState.StageSetup);
            }
            else
            {
                _gameFlow.SwitchState(GameState.Victory);
            }
        }
    }

    /// <summary>
    /// Victory phase
    /// </summary>
    public class VictoryPhase : PhaseBase
    {
        public VictoryPhase(PhaseFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            GameplayCoreManager.Instance.GameLogic.ExecuteVictory();
        }
    }

    /// <summary>
    /// Defeat phase
    /// </summary>
    public class DefeatPhase : PhaseBase
    {
        public DefeatPhase(PhaseFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            GameplayCoreManager.Instance.GameLogic.ExecuteGameOver();
        }
    }

    /// <summary>
    /// Checks battle results for win/lose conditions
    /// </summary>
    public class BattleResultChecker
    {
        private GameplayCharactersManager _charactersManager;

        public BattleResultChecker(GameplayCharactersManager manager)
        {
            _charactersManager = manager;
        }

        public bool AllEnemiesDefeated()
        {
            return _charactersManager.GetAliveCharacters(PlayerOrAi.Bot).Count <= 0;
        }

        public bool AnyPlayersAlive()
        {
            var launchable = _charactersManager.GetLaunchableCharacters();
            return launchable.Count > 0;
        }

        public bool AnyLaunchableCharactersLeft()
        {
            return _charactersManager.GetLaunchableCharacters().Count > 0;
        }
    }
}