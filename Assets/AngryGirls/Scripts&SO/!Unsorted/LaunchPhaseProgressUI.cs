using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Displays the complete predicted future turn sequence.
    /// The queue is rebuilt only when gameplay state changes.
    /// </summary>
    public sealed class LaunchPhaseProgressUI
        : UI_GameplayManagersComponent
    {
        [Header("Scroll")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;

        [Header("Segment")]
        [SerializeField] private TurnOrderSegmentUI _segmentPrefab;

        [Header("Behaviour")]
        [SerializeField] private bool _resetScrollAfterRefresh = true;

        private readonly List<TurnOrderSegmentUI> _segmentPool = new();

        private GameplayCharactersManager _charactersManager;
        private LaunchManager _launchManager;
        private GamePhaseFlowController _phaseFlowController;
        private StageManager _stageManager;
        private SettingsManager _settingsManager;

        private TurnOrderQueue _queue;

        private bool _showTurnOrder;
        private bool _initialized;

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
                gameplayCore
                    .GameplayCharactersManager;

            _launchManager =
                gameplayCore
                    .LaunchManager;

            _phaseFlowController =
                gameplayCore
                    .GamePhaseFlowController;

            _stageManager =
                gameplayCore
                    .StageManager;

            _settingsManager =
                CoreManager.Instance
                    .SettingsManager;

            _queue =
                new TurnOrderQueue();

            SubscribeToEvents();

            _initialized = true;

            RefreshVisibility();
        }

        private void SubscribeToEvents()
        {
            if (_charactersManager != null)
            {
                _charactersManager
                    .OnLaunchableCharactersChanged +=
                    HandleGameplayOrderChanged;
            }

            if (_stageManager != null)
            {
                _stageManager
                    .TheStageIsSet +=
                    HandleStageChanged;
            }

            if (_phaseFlowController != null)
            {
                _phaseFlowController
                    .OnPhaseChanged +=
                    HandlePhaseChanged;
            }

            if (_settingsManager != null)
            {
                _settingsManager
                    .OnSettingsChanged +=
                    HandleSettingsChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_charactersManager != null)
            {
                _charactersManager
                    .OnLaunchableCharactersChanged -=
                    HandleGameplayOrderChanged;
            }

            if (_stageManager != null)
            {
                _stageManager
                    .TheStageIsSet -=
                    HandleStageChanged;
            }

            if (_phaseFlowController != null)
            {
                _phaseFlowController
                    .OnPhaseChanged -=
                    HandlePhaseChanged;
            }

            if (_settingsManager != null)
            {
                _settingsManager
                    .OnSettingsChanged -=
                    HandleSettingsChanged;
            }
        }

        /// <summary>
        /// Shows the queue and refreshes it.
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (!_initialized)
                return;

            RefreshVisibility();
        }

        /// <summary>
        /// Rebuilds the queue from current gameplay state.
        /// </summary>
        public void Refresh()
        {
            if (!_initialized ||
                !_showTurnOrder)
            {
                return;
            }

            BuildQueue();

            RenderQueue();
        }

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

            gameObject.SetActive(true);

            Refresh();
        }

        /// <summary>
        /// Builds a fresh linked-list representation of the current future turn sequence.
        /// </summary>
        private void BuildQueue()
        {
            _queue.Clear();

            if (_charactersManager == null ||
                _launchManager == null ||
                _phaseFlowController == null)
            {
                _queue.AddEnd();
                return;
            }

            var alivePlayers =
                GetAlivePlayers();

            var aliveEnemies =
                GetAliveEnemies();

            var launchablePlayers =
                _charactersManager
                    .GetLaunchableCharacters();

            var launchedPlayers =
                GetLaunchedPlayers(
                    alivePlayers);

            switch (
                _phaseFlowController
                    .CurrentGamePhaseState)
            {
                case GamePhaseNames.GameStartPhase:
                case GamePhaseNames.StageSetupPhase:
                case GamePhaseNames.LaunchPhase:

                    BuildLaunchPhaseQueue(
                        launchablePlayers,
                        launchedPlayers,
                        aliveEnemies);

                    break;

                case GamePhaseNames.AlternatePhase:

                    BuildAlternatePhaseQueue(
                        launchedPlayers,
                        aliveEnemies);

                    BuildFutureLaunchQueue(
                        launchablePlayers,
                        launchedPlayers,
                        aliveEnemies);

                    break;

                case GamePhaseNames.StageCompletePhase:
                case GamePhaseNames.VictoryPhase:
                case GamePhaseNames.DefeatPhase:
                default:
                    break;
            }

            _queue.AddEnd();
        }

        /// <summary>
        /// Builds the queue before and during LaunchPhase.
        /// First turn: two launches before the first Alternate.
        /// Every later launch is followed by an Alternate block.
        /// </summary>
        private void BuildLaunchPhaseQueue(
            List<CControl> launchablePlayers,
            List<CControl> launchedPlayers,
            List<CControl> aliveEnemies)
        {
            if (launchablePlayers.Count == 0)
            {
                BuildAlternateBlock(
                    launchedPlayers,
                    _launchManager.LastLaunchedCharacter,
                    aliveEnemies);

                return;
            }

            var simulatedLaunched =
                new List<CControl>(
                    launchedPlayers);

            var launchableIndex = 0;

            var launchesBeforeAlternate =
                _launchManager.IsFirstTurn
                    ? Mathf.Max(
                        1,
                        _launchManager
                            .LaunchesBeforeFirstAlternate
                        - _launchManager
                            .LaunchCountThisStage)
                    : 1;

            launchesBeforeAlternate =
                Mathf.Min(
                    launchesBeforeAlternate,
                    launchablePlayers.Count);

            CControl lastSimulatedLaunch =
                _launchManager
                    .LastLaunchedCharacter;

            for (var i = 0;
                 i < launchesBeforeAlternate;
                 i++)
            {
                var character =
                    launchablePlayers[
                        launchableIndex];

                launchableIndex++;

                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                _queue.AddLaunch(
                    character);

                simulatedLaunched.Add(
                    character);

                lastSimulatedLaunch =
                    character;
            }

            BuildAlternateBlock(
                simulatedLaunched,
                lastSimulatedLaunch,
                aliveEnemies);

            while (
                launchableIndex <
                launchablePlayers.Count)
            {
                var character =
                    launchablePlayers[
                        launchableIndex];

                launchableIndex++;

                if (character == null ||
                    character.isDead)
                {
                    continue;
                }

                _queue.AddLaunch(
                    character);

                simulatedLaunched.Add(
                    character);

                lastSimulatedLaunch =
                    character;

                BuildAlternateBlock(
                    simulatedLaunched,
                    lastSimulatedLaunch,
                    aliveEnemies);
            }
        }

        /// <summary>
        /// Builds the currently executing Alternate phase.
        /// </summary>
        private void BuildAlternatePhaseQueue(
            List<CControl> launchedPlayers,
            List<CControl> aliveEnemies)
        {
            BuildAlternateBlock(
                launchedPlayers,
                _launchManager.LastLaunchedCharacter,
                aliveEnemies);
        }

        /// <summary>
        /// Builds future launch cycles after the current Alternate phase.
        /// </summary>
        private void BuildFutureLaunchQueue(
            List<CControl> launchablePlayers,
            List<CControl> launchedPlayers,
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

                _queue.AddLaunch(
                    character);

                simulatedLaunched.Add(
                    character);

                BuildAlternateBlock(
                    simulatedLaunched,
                    character,
                    aliveEnemies);
            }
        }

        /// <summary>
        /// Adds one Alternate block.
        /// Players act first, then all alive enemies.
        /// The most recently launched player is excluded.
        /// </summary>
        private void BuildAlternateBlock(
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

                _queue.AddAlternate(
                    player);
            }

            foreach (var enemy in aliveEnemies)
            {
                if (enemy == null ||
                    enemy.isDead)
                {
                    continue;
                }

                _queue.AddAlternate(
                    enemy);
            }
        }

        private List<CControl> GetAlivePlayers()
        {
            var result =
                new List<CControl>();

            foreach (var character in
                     _charactersManager.PlayerCharacters)
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

        private List<CControl> GetAliveEnemies()
        {
            var result =
                new List<CControl>();

            foreach (var character in
                     _charactersManager.EnemyCharacters)
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

        private List<CControl> GetLaunchedPlayers(
            List<CControl> alivePlayers)
        {
            var result =
                new List<CControl>();

            foreach (var player in alivePlayers)
            {
                if (player.hasBeenLaunched)
                {
                    result.Add(player);
                }
            }

            return result;
        }

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

                segment.gameObject.SetActive(
                    false);

                _segmentPool.Add(
                    segment);
            }
        }

        private void RenderQueue()
        {
            if (_content == null ||
                _segmentPrefab == null)
            {
                return;
            }

            EnsureSegmentPool(
                _queue.Count);

            var index = 0;

            for (
                var node = _queue.First;
                node != null;
                node = node.Next)
            {
                var segment =
                    _segmentPool[index];

                if (segment == null)
                {
                    index++;
                    continue;
                }

                segment.gameObject.SetActive(
                    true);

                var action =
                    node.Value;

                switch (action.ActionType)
                {
                    case TurnOrderActionType.Launch:

                        segment.SetupCharacter(
                            action.Character,
                            true,
                            index == 0);

                        break;

                    case TurnOrderActionType.Alternate:

                        segment.SetupCharacter(
                            action.Character,
                            false,
                            index == 0);

                        break;

                    case TurnOrderActionType.End:

                        segment.SetupEnd();

                        break;
                }

                index++;
            }

            for (var i = index;
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

            if (_resetScrollAfterRefresh &&
                _scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();

                _scrollRect
                    .horizontalNormalizedPosition = 0f;
            }
        }

        private void HandleGameplayOrderChanged()
        {
            Refresh();
        }

        private void HandleStageChanged(
            int stageIndex)
        {
            RefreshVisibility();
        }

        private void HandlePhaseChanged(
            GamePhaseNames phase)
        {
            RefreshVisibility();
        }

        private void HandleSettingsChanged(
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
            UnsubscribeFromEvents();
        }
    }
}