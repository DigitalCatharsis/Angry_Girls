using UnityEngine;
using System.Collections.Generic;

namespace Angry_Girls
{
    public enum GamePhaseNames
    {
        GameStartPhase,
        StageSetupPhase,
        LaunchPhase,
        AlternatePhase,
        StageCompletePhase,
        VictoryPhase,
        DefeatPhase
    }

    /// <summary>
    /// Controls game state flow and transitions between phases
    /// </summary>
    public class GamePhaseFlowController : GameplayManagerClass
    {
        private Dictionary<GamePhaseNames, IGamePhase> _gamePhases;
        private IGamePhase _currentGamePhase;
        public GamePhaseNames CurrentGamePhaseState { get; private set; }

        public override void Initialize()
        {

            _gamePhases = new Dictionary<GamePhaseNames, IGamePhase>
            {
                { GamePhaseNames.GameStartPhase, new GameStartGamePhase(this) },
                { GamePhaseNames.StageSetupPhase, new StageSetupGamePhase(this) },
                { GamePhaseNames.LaunchPhase, new LaunchGamePhase(this) },
                { GamePhaseNames.AlternatePhase, new AlternateGamePhase(this) },
                { GamePhaseNames.StageCompletePhase, new StageCompleteGamePhase(this) },
                { GamePhaseNames.VictoryPhase, new VictoryGamePhase(this) },
                { GamePhaseNames.DefeatPhase, new DefeatGamePhase(this) },
            };

            GameplayCoreManager.Instance.OnInitialized += LateInitialize;
            isInitialized = true;
        }

        private void LateInitialize()
        {
            GameplayCoreManager.Instance.OnInitialized -= LateInitialize;
            SwitchState(GamePhaseNames.GameStartPhase);
        }

        /// <summary>
        /// Transitions to new game state
        /// </summary>
        public void SwitchState(GamePhaseNames newPhase)
        {
            _currentGamePhase?.EndPhase();
            CurrentGamePhaseState = newPhase;
            Debug.Log($"Switched to gamephase: {newPhase}");
            _currentGamePhase = _gamePhases[newPhase];
            _currentGamePhase.StartPhase();
        }
    }


    /// <summary>
    /// Interface for game phase implementation
    /// </summary>
    public interface IGamePhase
    {
        void StartPhase();
        void EndPhase();
    }

    /// <summary>
    /// Base class for all game phases
    /// </summary>
    public abstract class PhaseBase : IGamePhase
    {
        protected GamePhaseFlowController _gameFlow;
        public PhaseBase(GamePhaseFlowController controller) => _gameFlow = controller;
        public abstract void StartPhase();
        public virtual void EndPhase() { }
    }

    /// <summary>
    /// Initial game start phase
    /// </summary>
    public class GameStartGamePhase : PhaseBase
    {
        public GameStartGamePhase(GamePhaseFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            Debug.Log("Game Start");
            _gameFlow.SwitchState(GamePhaseNames.StageSetupPhase);
        }
    }

    /// <summary>
    /// Stage setup and preparation phase
    /// </summary>
    public class StageSetupGamePhase : PhaseBase
    {
        public StageSetupGamePhase(GamePhaseFlowController controller) : base(controller) { }
        private StageManager _stageManager;

        public override void StartPhase()
        {
            if (_stageManager == null)
            {
                _stageManager = GameplayCoreManager.Instance.StageManager;
            }

            _stageManager.SetupCurrentStage();
            _gameFlow.SwitchState(GamePhaseNames.LaunchPhase);
        }
    }

    /// <summary>
    /// Character launching phase
    /// </summary>
    public class LaunchGamePhase : PhaseBase
    {
        public LaunchGamePhase(GamePhaseFlowController controller) : base(controller) { }

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
                _gameFlow.SwitchState(GamePhaseNames.StageCompletePhase);
                return;
            }

            if (!checker.AnyPlayersAlive())
            {
                _gameFlow.SwitchState(GamePhaseNames.AlternatePhase);
                return;
            }

            GameplayCoreManager.Instance.LaunchManager.BeginLaunchPhase(() =>
            {
                if (checker.AllEnemiesDefeated())
                {
                    _gameFlow.SwitchState(GamePhaseNames.StageCompletePhase);
                    return;
                }

                if (_charactersManager.GetAliveCharacters(PlayerOrAi.Player).Count <= 1 || GameplayCoreManager.Instance.LaunchManager.GetCandidateToLaunch() != null)
                {
                    _gameFlow.SwitchState(GamePhaseNames.AlternatePhase);
                    return;
                }

                _gameFlow.SwitchState(GamePhaseNames.LaunchPhase);
            });
        }
    }

    /// <summary>
    /// Alternate attack phase
    /// </summary>
    public class AlternateGamePhase : PhaseBase
    {
        public AlternateGamePhase(GamePhaseFlowController controller) : base(controller) { }

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
                        _gameFlow.SwitchState(GamePhaseNames.StageCompletePhase);
                    }
                    else
                    {
                        _gameFlow.SwitchState(GamePhaseNames.VictoryPhase);
                    }
                    return;
                }

                if (!checker.AnyLaunchableCharactersLeft())
                {
                    _gameFlow.SwitchState(GamePhaseNames.DefeatPhase);
                    return;
                }
                _gameFlow.SwitchState(GamePhaseNames.LaunchPhase);
            });
        }
    }
    public class StageCompleteGamePhase : PhaseBase
    {
        private StageManager _stageManager;
        public StageCompleteGamePhase(GamePhaseFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            if (_stageManager == null)
            {
                _stageManager = GameplayCoreManager.Instance.StageManager;
            }

            if (_stageManager.HasNextStage())
            {
                _stageManager.ProceedToNextStage();
                _gameFlow.SwitchState(GamePhaseNames.StageSetupPhase);
            }
            else
            {
                _gameFlow.SwitchState(GamePhaseNames.VictoryPhase);
            }
        }
    }

    /// <summary>
    /// Victory phase
    /// </summary>
    public class VictoryGamePhase : PhaseBase
    {
        public VictoryGamePhase(GamePhaseFlowController controller) : base(controller) { }
        public override void StartPhase()
        {
            GameplayCoreManager.Instance.GameLogic.ExecuteVictory();
        }
    }

    /// <summary>
    /// Defeat phase
    /// </summary>
    public class DefeatGamePhase : PhaseBase
    {
        public DefeatGamePhase(GamePhaseFlowController controller) : base(controller) { }
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