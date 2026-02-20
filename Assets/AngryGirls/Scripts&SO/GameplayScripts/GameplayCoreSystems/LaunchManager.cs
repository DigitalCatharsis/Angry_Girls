using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages character launching mechanics and launch phase execution
    /// </summary>
    public class LaunchManager : GameplayManagerClass
    {
        [SerializeField] private float _timeToWaitAfterAttackFinish = 1f;
        [SerializeField] private int _launchesBeforeFirstAlternate = 2;

        private CameraManager _cameraManager;
        private InputManager _inputManager;
        private PhaseFlowController _phaseFlowController;
        private StageManager _stageManager;
        private GameLogic _gameLogic;
        private LaunchExecutionService _executionService;
        private GameplayCharactersManager _gameplayCharactersManager;

        private CharacterLauncher _characterLauncher;
        private bool _canPressAtCharacters = false;
        private bool _isLaunchAllowed = false;
        private bool _firstTurn = true;
        private int _launchCountThisStage = 0;
        private int _currentStageIndex = 0;

        private bool _isTheTurnFinished = false;
        private CControl _currentlyLaunchedCharacter;
        private CControl _lastLaunchedUnit;
        public CControl LastLaunchedCharacter => _lastLaunchedUnit;

        public CControl GetCandidateToLaunch() 
        {
            var chars =  _gameplayCharactersManager.GetLaunchableCharacters();
            if (chars.Count != 0)
            {
                return chars[0];
            }
            return null;
        }

        public override void Initialize()
        {
            isInitialized = true;
            GameplayCoreManager.Instance.OnInitialized += LateInitialize;
        }

        private void LateInitialize()
        {
            _cameraManager = GameplayCoreManager.Instance.CameraManager;
            _inputManager = GameplayCoreManager.Instance.InputManager;
            _phaseFlowController = GameplayCoreManager.Instance.PhaseFlowController;
            _stageManager = GameplayCoreManager.Instance.StageManager;
            _gameLogic = GameplayCoreManager.Instance.GameLogic;
            _executionService = GameplayCoreManager.Instance.LaunchExecutionService;
            _gameplayCharactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;

            GameplayCoreManager.Instance.OnInitialized -= LateInitialize;
        }

        public void BeginLaunchPhase(System.Action onLaunchComplete)
        {
            StartCoroutine(BeginLaunchPhaseRoutine(onLaunchComplete));
        }

        private IEnumerator BeginLaunchPhaseRoutine(System.Action onLaunchComplete)
        {
            if (!isInitialized) yield break;

            _isTheTurnFinished = false;

            PrepareLaunchPhase();
            yield return WaitForLaunchCompletion();

            FinalizeLaunch(_currentlyLaunchedCharacter);
            _launchCountThisStage++;

            yield return HandlePostLaunchTransition(onLaunchComplete);
        }

        private void PrepareLaunchPhase()
        {
            var characters = GameplayCoreManager.Instance.GameplayCharactersManager.GetLaunchableCharacters();

            _characterLauncher = _stageManager.CurrentCharacterLauncher;
            _executionService.PrepareLaunch(
                _characterLauncher,
                characters,
                _characterLauncher.UnitsTransforms
            );

            _cameraManager.MoveCameraTo(
                new Vector3(Camera.main.transform.position.x, _characterLauncher.transform.position.y, _characterLauncher.transform.position.z),
                1f,
                false
            );

            _canPressAtCharacters = true;
        }

        private IEnumerator WaitForLaunchCompletion()
        {
            while (_isLaunchAllowed)
                yield return null;


            var can = GetCandidateToLaunch();
            while (can != null && !_isTheTurnFinished)
            {
                if (_currentlyLaunchedCharacter != null) { _isTheTurnFinished = _currentlyLaunchedCharacter.hasFinishedLaunchingTurn; }
                yield return null;
            }

            Debug.Log("you are not supposed 2 b here");
        }


        //TODO: do i need this? is that bugfix?
        private void FinalizeLaunch(CControl character)
        {
            GameplayCoreManager.Instance.GameplayCharactersManager.CurrentlyAttackingUnit = character;
        }

        private IEnumerator HandlePostLaunchTransition(System.Action onLaunchComplete)
        {
            _lastLaunchedUnit = _currentlyLaunchedCharacter;
            _currentlyLaunchedCharacter = null;
            yield return new WaitForSeconds(_timeToWaitAfterAttackFinish);

            // First turn special case: allow 2 launches before alternate phase
            if (_firstTurn && _launchCountThisStage < _launchesBeforeFirstAlternate)
            {
                _phaseFlowController.SwitchState(GamePhaseState.LaunchPhaseState);
                yield break;
            }

            // Stage changed during launch
            if (_currentStageIndex != _stageManager.CurrentStageIndex)
            {
                _currentStageIndex = _stageManager.CurrentStageIndex;
                _phaseFlowController.SwitchState(GamePhaseState.LaunchPhaseState);
                yield break;
            }

            // Normal transition to next phase
            _firstTurn = false;
            _launchCountThisStage = 0;
            onLaunchComplete?.Invoke();
        }

        private void Update()
        {
            if (!isInitialized || _gameLogic.GameOver || !_canPressAtCharacters)
                return;

            HandleCharacterSelection();
            HandleLaunchCancellation();
            HandleLaunchExecution();
        }

        private void HandleCharacterSelection()
        {
            if (_inputManager.IsPressed)
            {
                Ray ray = Camera.main.ScreenPointToRay(_inputManager.Position);
                int layerMask = LayerMask.GetMask("CharacterToLaunch");

                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
                {
                    var clickedCharacter = hit.collider.GetComponent<CControl>();
                    var availableCharacters = _gameplayCharactersManager.GetLaunchableCharacters();

                    if (clickedCharacter != null && availableCharacters.Contains(clickedCharacter))
                    {
                        int index = availableCharacters.IndexOf(clickedCharacter);
                        //_selectionService.SelectCharacter(index);
                        _isLaunchAllowed = (index == 0);
                    }
                }
            }
        }

        private void HandleLaunchCancellation()
        {
            if (Input.GetMouseButtonDown(1) && _isLaunchAllowed)
            {
                _isLaunchAllowed = false;
                _executionService.CancelAiming();
            }
        }

        private void HandleLaunchExecution()
        {
            if (!_isLaunchAllowed || GetCandidateToLaunch() == null)
                return;

            if (_inputManager.IsHeld)
            {
                _executionService.TryStartAiming(GetCandidateToLaunch());
            }

            if (_inputManager.IsReleased)
            {
                if (_executionService.TryExecuteLaunch(GetCandidateToLaunch()))
                {
                    LaunchCharacter(GetCandidateToLaunch());
                }
                else
                {
                    _isLaunchAllowed = false;
                    _executionService.CancelAiming();
                }
            }
        }

        private void LaunchCharacter(CControl character)
        {
            _currentlyLaunchedCharacter = character;
            _isLaunchAllowed = false;
            _canPressAtCharacters = false;

            character.hasBeenLaunched = true;
            character.canCheckGlobalBehavior = true;
            character.canUseAbility = true;
            character.gameObject.layer = LayerMask.NameToLayer("Character");

            GameplayCoreManager.Instance.GameplayCharactersManager.CurrentlyAttackingUnit = character;
            GameplayCoreManager.Instance.GameplayCharactersManager.NotifyLaunchableCharactersChanged();

            _characterLauncher.LaunchUnit(character);
            _cameraManager.CameraFollowForRigidBody(character.CharacterMovement.Rigidbody);
            _cameraManager.ZoomOutCameraAfterLaunch();

            StartCoroutine(ControlUnitLaunch(character));
        }

        private IEnumerator ControlUnitLaunch(CControl control)
        {
            while (!control.hasUsedAbility && !control.hasFinishedLaunchingTurn)
            {
                if (_inputManager.IsPressed)
                {
                    control.hasUsedAbility = true;
                    control.UnitPerformedAttack?.Invoke();
                }
                yield return null;
            }
        }

        //TODO
        public void TrySwapCharacterByIndex(int clickedIndex)
        {
            if (!_canPressAtCharacters || clickedIndex <= 0) return;

            _gameplayCharactersManager.SwapWithFirst(clickedIndex);
            _executionService.PrepareLaunch(
                _characterLauncher,
                _gameplayCharactersManager.LaunchableCharacters,
            _characterLauncher.UnitsTransforms
            );
        }
    }
}