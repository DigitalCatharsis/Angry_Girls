using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Displays the complete predicted future action sequence.
    /// One segment represents exactly one future action.
    /// </summary>
    public sealed class LaunchPhaseProgressUI
        : UI_GameplayManagersComponent
    {
        private enum PlannedActionType
        {
            Launch,
            Alternate,
            End
        }

        private sealed class PlannedAction
        {
            public PlannedActionType Type;
            public CControl Character;

            public static PlannedAction CreateLaunch(
                CControl character)
            {
                return new PlannedAction
                {
                    Type = PlannedActionType.Launch,
                    Character = character
                };
            }

            public static PlannedAction CreateAlternate(
                CControl character)
            {
                return new PlannedAction
                {
                    Type = PlannedActionType.Alternate,
                    Character = character
                };
            }

            public static PlannedAction CreateEnd()
            {
                return new PlannedAction
                {
                    Type = PlannedActionType.End,
                    Character = null
                };
            }
        }

        [Header("Scroll")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;

        [Header("Segment")]
        [SerializeField] private TurnOrderSegmentUI _segmentPrefab;

        [Header("Visibility")]
        [SerializeField] private bool _hideWhenGameIsOver = true;

        private readonly List<TurnOrderSegmentUI> _segmentPool = new();

        private GameplayCharactersManager _charactersManager;
        private LaunchManager _launchManager;
        private GamePhaseFlowController _phaseFlowController;
        private StageManager _stageManager;
        private SettingsManager _settingsManager;

        private bool _showTurnOrder;
        private bool _initialized;

        private int _lastStateHash;

        public override void Initialize()
        {
            base.Initialize();

            var gameplayCore =
                GameplayCoreManager.Instance;

            if (gameplayCore == null)
            {
                Debug.LogError(
                    "LaunchPhaseProgressUI: GameplayCoreManager is null.",
                    this);

                return;
            }

            _charactersManager =
                gameplayCore.GameplayCharactersManager;

            _launchManager =
                gameplayCore.LaunchManager;

            _phaseFlowController =
                gameplayCore.GamePhaseFlowController;

            _stageManager =
                gameplayCore.StageManager;

            _settingsManager =
                CoreManager.Instance.SettingsManager;

            if (_charactersManager != null)
            {
                _charactersManager
                    .OnLaunchableCharactersChanged +=
                    OnGameplayStateChanged;
            }

            if (_stageManager != null)
            {
                _stageManager.TheStageIsSet +=
                    OnStageChanged;
            }

            if (_settingsManager != null)
            {
                _settingsManager.OnSettingsChanged +=
                    OnSettingsChanged;
            }

            if (_scrollRect != null)
            {
                _scrollRect.horizontal = true;
                _scrollRect.vertical = false;
            }

            _initialized = true;

            RefreshVisibility();
        }

        /// <summary>
        /// Shows the turn order UI and refreshes its content.
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (!_initialized)
                return;

            RefreshVisibility();
        }

        private void LateUpdate()
        {
            if (!_initialized ||
                !_showTurnOrder ||
                _phaseFlowController == null)
            {
                return;
            }

            if (_hideWhenGameIsOver &&
                IsGameFinished())
            {
                return;
            }

            var actions =
                BuildTurnSequence();

            var stateHash =
                CalculateStateHash(actions);

            if (stateHash != _lastStateHash)
            {
                ApplySequence(actions);
            }
        }

        /// <summary>
        /// Updates UI visibility from gameplay settings.
        /// </summary>
        private void RefreshVisibility()
        {
            if (!_initialized ||
                _settingsManager == null)
            {
                return;
            }

            _showTurnOrder =
                _settingsManager
                    .GetCurrentSettings()
                    .showTurnOrder;

            if (!_showTurnOrder)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_hideWhenGameIsOver &&
                IsGameFinished())
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            Refresh();
        }

        /// <summary>
        /// Rebuilds and renders the complete future sequence.
        /// </summary>
        public void Refresh()
        {
            if (!_initialized ||
                !_showTurnOrder)
            {
                return;
            }

            var actions =
                BuildTurnSequence();

            ApplySequence(actions);
        }

        /// <summary>
        /// Builds the predicted sequence from the explicit player and enemy collections.
        /// </summary>
        private List<PlannedAction> BuildTurnSequence()
        {
            var result =
                new List<PlannedAction>();

            if (_charactersManager == null ||
                _launchManager == null ||
                _phaseFlowController == null)
            {
                result.Add(
                    PlannedAction.CreateEnd());

                return result;
            }

            var alivePlayers =
                GetAliveCharacters(
                    _charactersManager.PlayerCharacters);

            var aliveEnemies =
                GetAliveCharacters(
                    _charactersManager.EnemyCharacters);

            var launchedPlayers =
                new List<CControl>();

            var launchablePlayers =
                new List<CControl>();

            foreach (var player in alivePlayers)
            {
                if (player.hasBeenLaunched)
                {
                    launchedPlayers.Add(player);
                }
                else
                {
                    launchablePlayers.Add(player);
                }
            }

            switch (
                _phaseFlowController.CurrentGamePhaseState)
            {
                case GamePhaseNames.GameStartPhase:
                case GamePhaseNames.StageSetupPhase:
                    BuildInitialLaunchSequence(
                        result,
                        launchedPlayers,
                        launchablePlayers,
                        aliveEnemies);

                    break;

                case GamePhaseNames.LaunchPhase:
                    BuildLaunchPhaseSequence(
                        result,
                        launchedPlayers,
                        launchablePlayers,
                        aliveEnemies);

                    break;

                case GamePhaseNames.AlternatePhase:
                    BuildAlternatePhaseSequence(
                        result,
                        launchedPlayers,
                        _launchManager.LastLaunchedCharacter,
                        aliveEnemies);

                    BuildFutureLaunchSequence(
                        result,
                        launchedPlayers,
                        launchablePlayers,
                        aliveEnemies);

                    break;

                case GamePhaseNames.StageCompletePhase:
                case GamePhaseNames.VictoryPhase:
                case GamePhaseNames.DefeatPhase:
                    break;
            }

            result.Add(
                PlannedAction.CreateEnd());

            return result;
        }

        /// <summary>
        /// Builds the initial launch cycle.
        /// Two launches happen before the first Alternate phase.
        /// </summary>
        private void BuildInitialLaunchSequence(
            List<PlannedAction> result,
            List<CControl> launchedPlayers,
            List<CControl> launchablePlayers,
            List<CControl> aliveEnemies)
        {
            var simulatedLaunched =
                new List<CControl>(
                    launchedPlayers);

            var launchCount =
                Mathf.Min(
                    2,
                    launchablePlayers.Count);

            for (var i = 0;
                 i < launchCount;
                 i++)
            {
                var character =
                    launchablePlayers[i];

                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                result.Add(
                    PlannedAction.CreateLaunch(
                        character));

                simulatedLaunched.Add(
                    character);
            }

            if (launchCount > 0)
            {
                var lastLaunch =
                    launchablePlayers[
                        launchCount - 1];

                BuildAlternatePhaseSequence(
                    result,
                    simulatedLaunched,
                    lastLaunch,
                    aliveEnemies);
            }

            for (var i = launchCount;
                 i < launchablePlayers.Count;
                 i++)
            {
                var character =
                    launchablePlayers[i];

                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                result.Add(
                    PlannedAction.CreateLaunch(
                        character));

                simulatedLaunched.Add(
                    character);

                BuildAlternatePhaseSequence(
                    result,
                    simulatedLaunched,
                    character,
                    aliveEnemies);
            }
        }

        /// <summary>
        /// Builds the remaining launch sequence of the current stage.
        /// </summary>
        private void BuildLaunchPhaseSequence(
            List<PlannedAction> result,
            List<CControl> launchedPlayers,
            List<CControl> launchablePlayers,
            List<CControl> aliveEnemies)
        {
            var simulatedLaunched =
                new List<CControl>(
                    launchedPlayers);

            var lastLaunched =
                _launchManager.LastLaunchedCharacter;

            var initialLaunchesRemaining =
                _launchManager.IsFirstTurn
                    ? Mathf.Max(
                        0,
                        _launchManager
                            .LaunchesBeforeFirstAlternate
                        - _launchManager
                            .LaunchCountThisStage)
                    : 1;

            for (var i = 0;
                 i < initialLaunchesRemaining &&
                 i < launchablePlayers.Count;
                 i++)
            {
                var character =
                    launchablePlayers[i];

                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                result.Add(
                    PlannedAction.CreateLaunch(
                        character));

                simulatedLaunched.Add(
                    character);

                lastLaunched =
                    character;
            }

            if (initialLaunchesRemaining >
                    0 &&
                simulatedLaunched.Count >
                    launchedPlayers.Count)
            {
                BuildAlternatePhaseSequence(
                    result,
                    simulatedLaunched,
                    lastLaunched,
                    aliveEnemies);
            }

            var processedCount =
                initialLaunchesRemaining;

            for (var i = processedCount;
                 i < launchablePlayers.Count;
                 i++)
            {
                var character =
                    launchablePlayers[i];

                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                result.Add(
                    PlannedAction.CreateLaunch(
                        character));

                simulatedLaunched.Add(
                    character);

                BuildAlternatePhaseSequence(
                    result,
                    simulatedLaunched,
                    character,
                    aliveEnemies);
            }
        }

        /// <summary>
        /// Builds one Alternate block.
        /// Launched players except the most recently launched player
        /// are followed by every living enemy.
        /// </summary>
        private void BuildAlternatePhaseSequence(
            List<PlannedAction> result,
            List<CControl> launchedPlayers,
            CControl lastLaunched,
            List<CControl> aliveEnemies)
        {
            foreach (var player in launchedPlayers)
            {
                if (player == null ||
                    player.isDead ||
                    player == lastLaunched)
                {
                    continue;
                }

                result.Add(
                    PlannedAction.CreateAlternate(
                        player));
            }

            foreach (var enemy in aliveEnemies)
            {
                if (enemy == null ||
                    enemy.isDead)
                {
                    continue;
                }

                result.Add(
                    PlannedAction.CreateAlternate(
                        enemy));
            }
        }

        /// <summary>
        /// Adds future launch and Alternate cycles.
        /// </summary>
        private void BuildFutureLaunchSequence(
            List<PlannedAction> result,
            List<CControl> launchedPlayers,
            List<CControl> launchablePlayers,
            List<CControl> aliveEnemies)
        {
            var simulatedLaunched =
                new List<CControl>(
                    launchedPlayers);

            foreach (var character in launchablePlayers)
            {
                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                result.Add(
                    PlannedAction.CreateLaunch(
                        character));

                simulatedLaunched.Add(
                    character);

                BuildAlternatePhaseSequence(
                    result,
                    simulatedLaunched,
                    character,
                    aliveEnemies);
            }
        }

        private List<CControl> GetAliveCharacters(
            IEnumerable<CControl> source)
        {
            var result =
                new List<CControl>();

            if (source == null)
                return result;

            foreach (var character in source)
            {
                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                result.Add(character);
            }

            return result;
        }

        /// <summary>
        /// Creates enough UI segments for the current sequence.
        /// </summary>
        private void EnsureSegmentPool(
            int requiredCount)
        {
            if (_content == null ||
                _segmentPrefab == null)
            {
                return;
            }

            while (_segmentPool.Count <
                   requiredCount)
            {
                var segment =
                    Instantiate(
                        _segmentPrefab,
                        _content);

                if (segment != null)
                    segment.gameObject.SetActive(
                        false);

                _segmentPool.Add(
                    segment);
            }
        }

        /// <summary>
        /// Applies the sequence to the reusable UI segment pool.
        /// </summary>
        private void ApplySequence(
            List<PlannedAction> actions)
        {
            if (_content == null ||
                _segmentPrefab == null)
            {
                return;
            }

            EnsureSegmentPool(
                actions.Count);

            for (var i = 0;
                 i < actions.Count;
                 i++)
            {
                var segment =
                    _segmentPool[i];

                if (segment == null)
                    continue;

                segment.gameObject.SetActive(
                    true);

                var action =
                    actions[i];

                switch (action.Type)
                {
                    case PlannedActionType.Launch:
                        segment.SetupCharacter(
                            action.Character,
                            true,
                            i == 0);
                        break;

                    case PlannedActionType.Alternate:
                        segment.SetupCharacter(
                            action.Character,
                            false,
                            i == 0);
                        break;

                    case PlannedActionType.End:
                        segment.SetupEnd();
                        break;
                }
            }

            for (var i = actions.Count;
                 i < _segmentPool.Count;
                 i++)
            {
                if (_segmentPool[i] != null)
                {
                    _segmentPool[i]
                        .gameObject
                        .SetActive(false);
                }
            }

            _lastStateHash =
                CalculateStateHash(actions);

            NormalizeScrollPosition();
        }

        private int CalculateStateHash(
            List<PlannedAction> actions)
        {
            unchecked
            {
                var hash = 17;

                if (_phaseFlowController != null)
                {
                    hash =
                        hash * 31 +
                        (int)_phaseFlowController
                            .CurrentGamePhaseState;
                }

                if (_launchManager != null)
                {
                    hash =
                        hash * 31 +
                        (_launchManager.IsFirstTurn
                            ? 1
                            : 0);

                    hash =
                        hash * 31 +
                        _launchManager
                            .LaunchCountThisStage;

                    var lastLaunched =
                        _launchManager
                            .LastLaunchedCharacter;

                    hash =
                        hash * 31 +
                        (lastLaunched != null
                            ? lastLaunched
                                .GetInstanceID()
                            : 0);
                }

                foreach (var action in actions)
                {
                    hash =
                        hash * 31 +
                        (int)action.Type;

                    hash =
                        hash * 31 +
                        (action.Character != null
                            ? action.Character
                                .GetInstanceID()
                            : 0);
                }

                return hash;
            }
        }

        private void NormalizeScrollPosition()
        {
            if (_scrollRect == null)
                return;

            Canvas.ForceUpdateCanvases();

            _scrollRect.horizontalNormalizedPosition =
                0f;
        }

        private bool IsGameFinished()
        {
            if (GameplayCoreManager.Instance == null ||
                GameplayCoreManager.Instance.GameLogic == null)
            {
                return false;
            }

            return GameplayCoreManager.Instance
                .GameLogic
                .GameOver;
        }

        private void OnGameplayStateChanged()
        {
            if (!_initialized ||
                !_showTurnOrder)
            {
                return;
            }

            Refresh();
        }

        private void OnStageChanged(
            int stageIndex)
        {
            RefreshVisibility();
        }

        private void OnSettingsChanged(
            SettingsCategory category)
        {
            if (category != SettingsCategory.Gameplay &&
                category != SettingsCategory.All)
            {
                return;
            }

            RefreshVisibility();
        }

        private void OnDestroy()
        {
            if (_charactersManager != null)
            {
                _charactersManager
                    .OnLaunchableCharactersChanged -=
                    OnGameplayStateChanged;
            }

            if (_stageManager != null)
            {
                _stageManager.TheStageIsSet -=
                    OnStageChanged;
            }

            if (_settingsManager != null)
            {
                _settingsManager.OnSettingsChanged -=
                    OnSettingsChanged;
            }
        }
    }
}