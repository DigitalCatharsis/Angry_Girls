using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages character launching mechanics and launch phase execution.
    /// </summary>
    public class LaunchManager : GameplayManagerClass
    {
        [SerializeField] private float _timeToWaitAfterAttackFinish = 1f;
        [SerializeField] private int _launchesBeforeFirstAlternate = 2;

        private CameraManager _cameraManager;
        private InputManager _inputManager;
        private GamePhaseFlowController _phaseFlowController;
        private StageManager _stageManager;
        private GameLogic _gameLogic;
        private GameplayCharactersManager _gameplayCharactersManager;

        private CharacterLauncher _characterLauncher;
        private bool _canPressAtCharacters;
        private bool _isLaunchAllowed;
        private bool _firstTurn = true;
        private int _launchCountThisStage;
        private int _currentStageIndex;

        private bool _isTheTurnFinished;
        private CControl _currentlyLaunchedCharacter;
        private CControl _lastLaunchedUnit;

        /// <summary>
        /// Gets the character that was launched most recently.
        /// </summary>
        public CControl LastLaunchedCharacter => _lastLaunchedUnit;

        /// <summary>
        /// Gets whether the current stage is still in its initial launch cycle.
        /// </summary>
        public bool IsFirstTurn => _firstTurn;

        /// <summary>
        /// Gets the number of launches already completed in the current stage launch cycle.
        /// </summary>
        public int LaunchCountThisStage => _launchCountThisStage;

        /// <summary>
        /// Gets the configured number of initial launches before the first Alternate phase.
        /// </summary>
        public int LaunchesBeforeFirstAlternate =>
            Mathf.Max(1, _launchesBeforeFirstAlternate);

        /// <summary>
        /// Gets the current launch candidate.
        /// </summary>
        public CControl GetCandidateToLaunch()
        {
            var characters =
                _gameplayCharactersManager.GetLaunchableCharacters();

            return characters.Count > 0
                ? characters[0]
                : null;
        }

        public override void Initialize()
        {
            isInitialized = true;

            _cameraManager =
                GameplayCoreManager.Instance.CameraManager;

            _inputManager =
                GameplayCoreManager.Instance.InputManager;

            _phaseFlowController =
                GameplayCoreManager.Instance.GamePhaseFlowController;

            _stageManager =
                GameplayCoreManager.Instance.StageManager;

            _gameLogic =
                GameplayCoreManager.Instance.GameLogic;

            _gameplayCharactersManager =
                GameplayCoreManager.Instance.GameplayCharactersManager;
        }

        /// <summary>
        /// Starts the launch phase.
        /// </summary>
        public void BeginLaunchPhase(System.Action onLaunchComplete)
        {
            StartCoroutine(
                BeginLaunchPhaseRoutine(onLaunchComplete));
        }

        private IEnumerator BeginLaunchPhaseRoutine(
            System.Action onLaunchComplete)
        {
            if (!isInitialized)
                yield break;

            _isTheTurnFinished = false;

            PrepareLaunchPhase();

            yield return WaitForLaunchCompletion();

            FinalizeLaunch(
                _currentlyLaunchedCharacter);

            _launchCountThisStage++;

            yield return HandlePostLaunchTransition(
                onLaunchComplete);
        }

        private void PrepareLaunchPhase()
        {
            var characters =
                _gameplayCharactersManager
                    .GetLaunchableCharacters();

            _characterLauncher =
                _stageManager.CurrentCharacterLauncher;

            if (_characterLauncher == null)
            {
                Debug.LogError(
                    "LaunchManager: Current CharacterLauncher is null.");

                _canPressAtCharacters = false;
                return;
            }

            PrepareLaunch(
                _characterLauncher,
                characters,
                _characterLauncher.UnitsTransforms);

            _cameraManager.MoveCameraTo(
                new Vector3(
                    Camera.main.transform.position.x,
                    Camera.main.transform.position.y,
                    _characterLauncher.transform.position.z),
                1f,
                false);

            _canPressAtCharacters = true;
        }

        private IEnumerator WaitForLaunchCompletion()
        {
            while (_isLaunchAllowed)
                yield return null;

            var candidate =
                GetCandidateToLaunch();

            while (candidate != null &&
                   !_isTheTurnFinished)
            {
                if (_currentlyLaunchedCharacter != null)
                {
                    _isTheTurnFinished =
                        _currentlyLaunchedCharacter
                            .hasFinishedLaunchingTurn;
                }

                yield return null;
            }
        }

        private void FinalizeLaunch(
            CControl character)
        {
            GameplayCoreManager.Instance
                .GameplayCharactersManager
                .CurrentlyAttackingUnit = character;
        }

        private IEnumerator HandlePostLaunchTransition(
            System.Action onLaunchComplete)
        {
            _lastLaunchedUnit =
                _currentlyLaunchedCharacter;

            _currentlyLaunchedCharacter = null;

            yield return new WaitForSeconds(
                _timeToWaitAfterAttackFinish);

            if (_firstTurn &&
                _launchCountThisStage <
                _launchesBeforeFirstAlternate)
            {
                _phaseFlowController.SwitchState(
                    GamePhaseNames.LaunchPhase);

                yield break;
            }

            if (_currentStageIndex !=
                _stageManager.CurrentStageIndex)
            {
                _currentStageIndex =
                    _stageManager.CurrentStageIndex;

                _phaseFlowController.SwitchState(
                    GamePhaseNames.LaunchPhase);

                yield break;
            }

            _firstTurn = false;
            _launchCountThisStage = 0;

            onLaunchComplete?.Invoke();
        }

        private void Update()
        {
            if (!isInitialized ||
                _gameLogic.GameOver ||
                !_canPressAtCharacters)
            {
                return;
            }

            HandleCharacterSelection();
            HandleLaunchCancellation();
            HandleLaunchExecution();
        }

        private void HandleCharacterSelection()
        {
            if (!_inputManager.IsPressed)
                return;

            var ray =
                Camera.main.ScreenPointToRay(
                    _inputManager.Position);

            var layerMask =
                LayerMask.GetMask("CharacterToLaunch");

            if (!Physics.Raycast(
                    ray,
                    out var hit,
                    Mathf.Infinity,
                    layerMask))
            {
                return;
            }

            var clickedCharacter =
                hit.collider.GetComponent<CControl>();

            var availableCharacters =
                _gameplayCharactersManager
                    .GetLaunchableCharacters();

            if (clickedCharacter == null ||
                !availableCharacters.Contains(
                    clickedCharacter))
            {
                return;
            }

            var index =
                availableCharacters.IndexOf(
                    clickedCharacter);

            _isLaunchAllowed =
                index == 0;
        }

        private void HandleLaunchCancellation()
        {
            if (Input.GetMouseButtonDown(1) &&
                _isLaunchAllowed)
            {
                _isLaunchAllowed = false;
                CancelAiming();
            }
        }

        private void HandleLaunchExecution()
        {
            if (!_isLaunchAllowed ||
                GetCandidateToLaunch() == null)
            {
                return;
            }

            if (_inputManager.IsHeld)
            {
                TryStartAiming(
                    GetCandidateToLaunch());
            }

            if (!_inputManager.IsReleased)
                return;

            if (TryExecuteLaunch(
                    GetCandidateToLaunch()))
            {
                LaunchCharacter(
                    GetCandidateToLaunch());
            }
            else
            {
                _isLaunchAllowed = false;
                CancelAiming();
            }
        }

        private void LaunchCharacter(
            CControl character)
        {
            _currentlyLaunchedCharacter = character;
            _isLaunchAllowed = false;
            _canPressAtCharacters = false;

            character.hasBeenLaunched = true;
            character.canCheckGlobalBehavior = true;
            character.canUseAbility = true;
            character.gameObject.layer =
                LayerMask.NameToLayer("Character");

            GameplayCoreManager.Instance
                .GameplayCharactersManager
                .CurrentlyAttackingUnit = character;

            GameplayCoreManager.Instance
                .GameplayCharactersManager
                .NotifyLaunchableCharactersChanged();

            _characterLauncher.LaunchUnit(
                character);

            _cameraManager.CameraFollowForRigidBody(
                character.CharacterMovement.Rigidbody);

            _cameraManager.ZoomOutCameraAfterLaunch();

            StartCoroutine(
                ControlUnitLaunch(character));
        }

        private IEnumerator ControlUnitLaunch(
            CControl control)
        {
            while (control.canUseAbility &&
                   !control.hasFinishedLaunchingTurn)
            {
                if (_inputManager.IsPressed)
                {
                    control.canUseAbility = false;
                    control.hasUsedAbility = true;
                    control.UnitPerformedAttack?.Invoke();
                }

                yield return null;
            }
        }

        /// <summary>
        /// Swaps the selected launchable character with the first character.
        /// </summary>
        public void TrySwapCharacterByIndex(
            int clickedIndex)
        {
            if (!_canPressAtCharacters ||
                clickedIndex <= 0)
            {
                return;
            }

            _gameplayCharactersManager
                .SwapWithFirst(clickedIndex);

            PrepareLaunch(
                _characterLauncher,
                _gameplayCharactersManager
                    .LaunchableCharacters,
                _characterLauncher.UnitsTransforms);
        }

        #region Execution service

        /// <summary>
        /// Prepares character launch positions.
        /// </summary>
        public void PrepareLaunch(
            CharacterLauncher launcher,
            List<CControl> characters,
            Transform[] positions)
        {
            if (launcher == null ||
                characters == null ||
                positions == null)
            {
                return;
            }

            _characterLauncher = launcher;

            UpdateCharacterPositions(
                characters,
                positions);
        }

        /// <summary>
        /// Starts aiming with the selected character.
        /// </summary>
        public bool TryStartAiming(
            CControl character)
        {
            if (character == null ||
                _characterLauncher == null)
            {
                return false;
            }

            _characterLauncher.AimingTheLaunch(
                character.gameObject);

            _cameraManager.CameraFollowForRigidBody(
                character.CharacterMovement.Rigidbody);

            return true;
        }

        /// <summary>
        /// Cancels current aiming.
        /// </summary>
        public void CancelAiming()
        {
            _characterLauncher?.CancelAiming();
        }

        /// <summary>
        /// Executes character launch when minimum distance is reached.
        /// </summary>
        public bool TryExecuteLaunch(
            CControl character)
        {
            if (character == null ||
                _characterLauncher == null)
            {
                return false;
            }

            if (!_characterLauncher
                    .IsLaunchDistanceSufficient())
            {
                return false;
            }

            _characterLauncher.LaunchUnit(
                character);

            _cameraManager.CameraFollowForRigidBody(
                character.CharacterMovement.Rigidbody);

            _cameraManager.ZoomOutCameraAfterLaunch();

            return true;
        }

        private void UpdateCharacterPositions(
            List<CControl> characters,
            Transform[] positions)
        {
            var count =
                Mathf.Min(
                    characters.Count,
                    positions.Length);

            for (var i = 0; i < count; i++)
            {
                if (characters[i] == null ||
                    positions[i] == null)
                {
                    continue;
                }

                characters[i]
                    .CharacterMovement
                    .Teleport(
                        positions[i].position);
            }
        }

        #endregion
    }
}