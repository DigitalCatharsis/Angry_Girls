using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchManager : MonoBehaviour
    {
        private int _currentStageIndex = 0;
        private CharacterLauncher _characterLauncher;

        private List<CControl> _charactersToLaunchLeft = new();

        private bool _canPressAtCharacters = false;
        private bool _isLaunchAllowed = false;
        private bool _firstTurn = true;
        private int _launchCountThisStage = 0;
        private const float _timeToWaitAfterAttackFinish = 1f;

        private const int _launchesBeforeFirstAlternate = 2;
        public CControl CharacterToLaunch => _charactersToLaunchLeft.Count > 0 ? _charactersToLaunchLeft[0] : null;

        private void InitHandler()
        {
            _charactersToLaunchLeft.Clear();
            _charactersToLaunchLeft.AddRange(GameLoader.Instance.characterManager.playableCharacters);
            _charactersToLaunchLeft.RemoveAll(c => c.isDead);
            _charactersToLaunchLeft.RemoveAll(c => c.hasBeenLaunched);
            _characterLauncher = GameLoader.Instance.stageManager.CurrentCharacterLauncher;
        }

        public IEnumerator BeginLaunchPhaseRoutine(System.Action onLaunchComplete)
        {
            InitHandler();
            GameLoader.Instance.cameraManager.MoveCameraTo(new Vector3(Camera.main.transform.position.x, _characterLauncher.transform.position.y, _characterLauncher.transform.position.z), 0.5f, true);

            UpdateCharacterPositions(_charactersToLaunchLeft);
            _canPressAtCharacters = true;

            // Ждём запуска
            while (_isLaunchAllowed)
                yield return null;

            // Ожидаем окончания приземления
            while (!CharacterToLaunch.hasFinishedLaunchingTurn)
                yield return null;

            GameLoader.Instance.turnManager.currentAttackingUnit = null;

            yield return new WaitForSeconds(_timeToWaitAfterAttackFinish);


            // Добавим в очередь хода
            GameLoader.Instance.turnManager.AddCharacterToTurnList(CharacterToLaunch);

            // Удалим из списка запуска
            _charactersToLaunchLeft.Remove(CharacterToLaunch);

            // Переход к следующей фазе (по логике)
            _launchCountThisStage++;

            if (_firstTurn && _launchCountThisStage < _launchesBeforeFirstAlternate)
            {
                GameLoader.Instance.gameFlowController.SwitchState(GameState.LaunchPhase);
            }
            else if (_currentStageIndex != GameLoader.Instance.stageManager.CurrentStageIndex)
            {
                _currentStageIndex = GameLoader.Instance.stageManager.CurrentStageIndex;
                GameLoader.Instance.gameFlowController.SwitchState(GameState.LaunchPhase);

            }
            else
            {
                _firstTurn = false;
                _launchCountThisStage = 0;
                onLaunchComplete?.Invoke();
            }
        }

        private void Update()
        {
            if (!_canPressAtCharacters || GameLoader.Instance.gameLogic.GameOver)
                return;

            if (GameLoader.Instance.inputManager.IsPressed)
                TrySelectCharacter();

            if (!_isLaunchAllowed)
            {
                return;
            }

            if (GameLoader.Instance.inputManager.IsHeld && CharacterToLaunch != null)
            {
                _characterLauncher.AimingTheLaunch(CharacterToLaunch.gameObject);
                GameLoader.Instance.cameraManager.CameraFollowForRigidBody(CharacterToLaunch.CharacterMovement.Rigidbody);
            }

            if (GameLoader.Instance.inputManager.IsReleased && CharacterToLaunch != null && _isLaunchAllowed)
            {
                LaunchCharacter(CharacterToLaunch);
            }
        }

        private void TrySelectCharacter()
        {
            Ray ray = Camera.main.ScreenPointToRay(GameLoader.Instance.inputManager.Position);
            int layerMask = LayerMask.GetMask("CharacterToLaunch");

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
                return;

            var clickedCharacter = hit.collider.GetComponent<CControl>();
            if (clickedCharacter == null || !_charactersToLaunchLeft.Contains(clickedCharacter)) return;

            if (clickedCharacter != CharacterToLaunch)
            {
                SwapCharacters(_charactersToLaunchLeft.IndexOf(clickedCharacter), 0);
                UpdateCharacterPositions(_charactersToLaunchLeft);
            }
            else
            {
                _isLaunchAllowed = true;
            }
        }

        private void LaunchCharacter(CControl character)
        {
            _isLaunchAllowed = false;
            _canPressAtCharacters = false;

            character.hasBeenLaunched = true;
            character.checkGlobalBehavior = true;
            character.canUseAbility = true;

            //Changing layer from CharacterToLaunch to Character
            int characterLayer = LayerMask.NameToLayer("Character");
            character.transform.root.gameObject.layer = characterLayer;

            GameLoader.Instance.turnManager.currentAttackingUnit = character;

            _characterLauncher.LaunchUnit(character);
            GameLoader.Instance.cameraManager.CameraFollowForRigidBody(character.CharacterMovement.Rigidbody);
            GameLoader.Instance.cameraManager.ZoomOutCameraAfterLaunch();

            StartCoroutine(ControlUnitLaunch(character));
        }

        private IEnumerator ControlUnitLaunch(CControl control)
        {
            while (!control.hasUsedAbility)
            {
                if (control.hasFinishedLaunchingTurn)
                {
                    break;
                }

                CheckForAbilityUse(control);
                yield return null;
            }
        }

        private void CheckForAbilityUse(CControl control)
        {
            if (control.hasUsedAbility)
            {
                return;
            }

            if (GameLoader.Instance.inputManager.IsPressed)
            {
                //process ability
                control.hasUsedAbility = true;
                //ColorDebugLog.Log("Ability has been used", System.Drawing.KnownColor.Magenta);
            }
        }
        private void UpdateCharacterPositions(List<CControl> characters)
        {
            var transforms = _characterLauncher.UnitsTransforms;
            for (int i = 0; i < characters.Count && i < transforms.Length; i++)
            {
                characters[i].CharacterMovement.Teleport(transforms[i].position);
            }
        }
        private void SwapCharacters(int indexA, int indexB)
        {
            var temp = _charactersToLaunchLeft[indexA];
            _charactersToLaunchLeft[indexA] = _charactersToLaunchLeft[indexB];
            _charactersToLaunchLeft[indexB] = temp;
        }
    }
}
