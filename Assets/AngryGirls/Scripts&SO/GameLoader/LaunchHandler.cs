using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchHandler : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private PlayerData characterSelectSO;
        [SerializeField] private CharacterLauncher _characterLauncher;

        [Space(10)]
        [Header("Conditions")]
        [SerializeField] private bool _isLaunchAllowed = false;
        [SerializeField] private bool _canPressAtCharacters = false;
        [Space(10)]
        [Header("Debug)")]
        [SerializeField] private List<CControl> _charactersToLaunchLeft;
        [SerializeField] private List<CControl> _launchedCharacters;

        private CControl CharacterToLaunch { get => _charactersToLaunchLeft[0]; }

        private void Start()
        {
            _characterLauncher.InitLauncher();
            _charactersToLaunchLeft = SpawnAndGetCharacters(characterSelectSO.selectedCharacters);
            UpdateCharacterPositions(_charactersToLaunchLeft);
            SetLaunchableCharactersBehavior(_charactersToLaunchLeft);
            _canPressAtCharacters = true;
            GameLoader.Instance.cameraManager.ReturnCameraToStartPosition(1f);
        }

        private List<CControl> SpawnAndGetCharacters(CharacterSettings[] selectedCharactersList)
        {
            var charList = new List<CControl>();

            foreach (var character in selectedCharactersList)
            {
                if (character == null) 
                {
                    continue;
                }
                charList.Add(GameLoader.Instance.poolManager.GetObject<CharacterType>
                    (character.characterType, GameLoader.Instance.poolManager.characterPoolDictionary, Vector3.zero, Quaternion.identity).GetComponent<CControl>());
            }
            return charList;
        }
        private void UpdateCharacterPositions(List<CControl> charactersToLaunch)
        {
            var transforms = _characterLauncher.GetPositionTransforms();

            for (var i = 0; i < charactersToLaunch.Count(); i++)
            {
                charactersToLaunch[i].rigidBody.MovePosition(transforms[i].position);
            }
        }
        private void SetLaunchableCharactersBehavior(List<CControl> charactersToLaunchLeft)
        {
            foreach (var character in charactersToLaunchLeft)
            {
                character.GetComponent<CControl>().unitBehaviorIsAlternate = false;
            }
        }

        private void Update()
        {
            if (GameLoader.Instance.gameLogic.GameOver)
            {
                return;
            }

            #region ButtonReaction (Except applyAttack)
            //Нажали
            if (GameLoader.Instance.inputManager.IsPressed && _canPressAtCharacters)
            {
                Ray ray = Camera.main.ScreenPointToRay(GameLoader.Instance.inputManager.Position);

                //CharacterToLaunch layer
                int layerMask = 1 << 14;

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    var characterCcontrol = hit.collider.gameObject.GetComponent<CControl>();

                    if (_charactersToLaunchLeft.Contains(characterCcontrol))
                    {
                        if (hit.collider == CharacterToLaunch.boxCollider)
                        {
                            // Center camera on character collider center
                            GameLoader.Instance.cameraManager.CameraFollowForRigidBody(CharacterToLaunch.rigidBody);
                            //Launch
                            _isLaunchAllowed = true;
                        }
                        else
                        {
                            //Swap
                            SwapCharacters(_charactersToLaunchLeft.IndexOf(characterCcontrol), 0);
                            UpdateCharacterPositions(_charactersToLaunchLeft);
                            _isLaunchAllowed = false;
                        }
                    }
                    else
                    {
                        _isLaunchAllowed = false;
                    }
                }
            }

            //Держим
            if (GameLoader.Instance.inputManager.IsHeld && _isLaunchAllowed)
            {
                _characterLauncher.AimingTheLaunch(CharacterToLaunch.gameObject);
            }

            //Отпустили
            if (GameLoader.Instance.inputManager.IsReleased && _isLaunchAllowed)
            {
                Camera.main.orthographicSize /= 1.5f;
                _canPressAtCharacters = false;

                ColorDebugLog.Log(CharacterToLaunch.name + this.name + " proceed ProcessLaunch", System.Drawing.KnownColor.ControlLightLight);
                _characterLauncher.LaunchUnit(CharacterToLaunch.GetComponent<CControl>());
                _isLaunchAllowed = false;

                GameLoader.Instance.cameraManager.CameraFollowForRigidBody(CharacterToLaunch.GetComponent<Rigidbody>());
                StartCoroutine(ControlUnitLaunch(CharacterToLaunch.GetComponent<CControl>()));
            }

            #endregion ButtonReaction (Except applyAttack)

            //TODO: replace to proper class. gameover
            if (_charactersToLaunchLeft.Count == 0 && _canPressAtCharacters)
            {
                GameLoader.Instance.gameLogic.ExecuteGameOver();
            }
        }

        private void SwapCharacters(int indexA, int indexB)
        {
            var tmp = _charactersToLaunchLeft[indexA];
            _charactersToLaunchLeft[indexA] = _charactersToLaunchLeft[indexB];
            _charactersToLaunchLeft[indexB] = tmp;
        }

        #region unitLaunchControl

        private IEnumerator ControlUnitLaunch(CControl control)
        {
            control.hasBeenLaunched = true;

            //Changing layer from CharacterToLaunch to Character
            int characterLayer = LayerMask.NameToLayer("Character");
            control.transform.root.gameObject.layer = characterLayer;

            control.checkGlobalBehavior = true;
            control.hasFinishedLaunchingTurn = false;
            control.canUseAbility = true;

            GameLoader.Instance.cameraManager.ZoomOutCameraAfterLaunch();

            //Camera follow
            //GameLoader.Instance.cameraManager.FollowCamera(control.gameObject);

            while (!control.hasUsedAbility)
            {
                if (control.hasFinishedLaunchingTurn)
                {
                    break;
                }

                CheckForAbilityUse(control);
                yield return null;
            }

            ////Camera follow
            //GameLoader.Instance.cameraManager.FollowCamera(control.gameObject);

            while (!control.hasFinishedLaunchingTurn)
            {

                yield return null;
            }

            GameLoader.Instance.launchManager.OnLaunchIsOver();
        }

        private void CheckForAbilityUse(CControl control)
        {
            if (control.hasUsedAbility)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                //process ability
                control.hasUsedAbility = true;
                ColorDebugLog.Log("Ability has been used", System.Drawing.KnownColor.Magenta);
            }
        }

        public void OnLaunchIsOver()
        {
            StartCoroutine(OnLaunchIsOver_Routine(GameLoader.Instance.cameraManager.SecondsCameraWaitsAfterAttack));
        }

        private IEnumerator OnLaunchIsOver_Routine(float secondsToWaitAfterAttack)
        {
            GameLoader.Instance.turnManager.AddCharacterToTurnList(CharacterToLaunch);

            yield return new WaitForSeconds(CharacterToLaunch.GetComponent<CControl>().animator.GetCurrentAnimatorStateInfo(0).length);
            UpdateCharactersLists(CharacterToLaunch);
            yield return new WaitForSeconds(secondsToWaitAfterAttack);
            UpdateCharacterPositions(_charactersToLaunchLeft);

            if (GameLoader.Instance.turnManager.CurrentTurn < 1)
            {
                GameLoader.Instance.cameraManager.ReturnCameraToStartPosition(1f);
                _canPressAtCharacters = true;
                GameLoader.Instance.turnManager.IncrementCurentTurn();
            }
            else
            {
                GameLoader.Instance.turnManager.IsLaunchingPhaseOver = true;
            }
        }

        private void UpdateCharactersLists(CControl launchedCharacter)
        {
            _charactersToLaunchLeft.Remove(launchedCharacter);
            _launchedCharacters.Add(launchedCharacter);
        }
        #endregion UnitLaunchControl

        public void Allow_CharacterPress()
        {
            _canPressAtCharacters = true;
        }
    }
}
