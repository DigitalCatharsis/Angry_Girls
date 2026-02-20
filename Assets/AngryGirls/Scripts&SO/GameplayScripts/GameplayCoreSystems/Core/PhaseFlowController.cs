using UnityEngine;
using System.Collections.Generic;

namespace Angry_Girls
{
    public enum GamePhaseState
    {
        GameStartState,
        StageSetupState,
        LaunchPhaseState,
        AlternatePhaseState,
        StageCompleteState,
        VictoryState,
        DefeatState
    }

    /// <summary>
    /// Controls game state flow and transitions between phases
    /// </summary>
    public class PhaseFlowController : GameplayManagerClass
    {
        private Dictionary<GamePhaseState, IPhase> _phases;
        private IPhase _currentPhase;
        public IPhase CurrentPhase => _currentPhase;
        public GamePhaseState CurrentGameState { get; private set; }

        public override void Initialize()
        {

            _phases = new Dictionary<GamePhaseState, IPhase>
            {
                { GamePhaseState.GameStartState, new GameStartPhase(this) },
                { GamePhaseState.StageSetupState, new StageSetupPhase(this) },
                { GamePhaseState.LaunchPhaseState, new LaunchPhase(this) },
                { GamePhaseState.AlternatePhaseState, new AlternatePhase(this) },
                { GamePhaseState.StageCompleteState, new StageCompletePhase(this) },
                { GamePhaseState.VictoryState, new VictoryPhase(this) },
                { GamePhaseState.DefeatState, new DefeatPhase(this) },
            };

            GameplayCoreManager.Instance.OnInitialized += LateInitialize;
            isInitialized = true;

        }

        private void LateInitialize()
        {
            GameplayCoreManager.Instance.OnInitialized -= LateInitialize;
            SwitchState(GamePhaseState.GameStartState);
        }

        /// <summary>
        /// Transitions to new game state
        /// </summary>
        public void SwitchState(GamePhaseState newState)
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
            _gameFlow.SwitchState(GamePhaseState.StageSetupState);
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
            _gameFlow.SwitchState(GamePhaseState.LaunchPhaseState);
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
                _gameFlow.SwitchState(GamePhaseState.StageCompleteState);
                return;
            }

            if (!checker.AnyPlayersAlive())
            {
                _gameFlow.SwitchState(GamePhaseState.AlternatePhaseState);
                return;
            }

            GameplayCoreManager.Instance.LaunchManager.BeginLaunchPhase(() =>
            {
                if (checker.AllEnemiesDefeated())
                {
                    _gameFlow.SwitchState(GamePhaseState.StageCompleteState);
                    return;
                }

                if (_charactersManager.GetAliveCharacters(PlayerOrAi.Player).Count <= 1 || GameplayCoreManager.Instance.LaunchManager.GetCandidateToLaunch() != null)
                {
                    _gameFlow.SwitchState(GamePhaseState.AlternatePhaseState);
                    return;
                }

                _gameFlow.SwitchState(GamePhaseState.LaunchPhaseState);
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
                        _gameFlow.SwitchState(GamePhaseState.StageCompleteState);
                    }
                    else
                    {
                        _gameFlow.SwitchState(GamePhaseState.VictoryState);
                    }
                    return;
                }

                if (!checker.AnyLaunchableCharactersLeft())
                {
                    _gameFlow.SwitchState(GamePhaseState.DefeatState);
                    return;
                }
                _gameFlow.SwitchState(GamePhaseState.LaunchPhaseState);
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
                _gameFlow.SwitchState(GamePhaseState.StageSetupState);
            }
            else
            {
                _gameFlow.SwitchState(GamePhaseState.VictoryState);
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