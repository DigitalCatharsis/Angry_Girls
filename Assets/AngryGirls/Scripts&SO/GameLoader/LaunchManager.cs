using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private CharacterSelect characterSelectSO;
        [SerializeField] private CharacterLauncher _characterLauncher;

        [Space(10)]
        [Header("Conditions")]
        [SerializeField] private bool _isLaunchAllowed = false;
        [SerializeField] private bool _canPressAtCharacters = false;
        [Space(10)]
        [Header("Debug)")]
        [SerializeField] private List<GameObject> _charactersToLaunchLeft;
        [SerializeField] private List<GameObject> _launchedCharacters;

        private void Start()
        {
            _characterLauncher.InitLauncher();
            _charactersToLaunchLeft = SpawnAndGetCharacters(characterSelectSO.selectedCharacters);
            UpdateCharacterPositions(_charactersToLaunchLeft);
            SetLaunchableCharactersBehavior(_charactersToLaunchLeft);
            _canPressAtCharacters = true;
            GameLoader.Instance.cameraManager.CenterCameraAgainst(_characterLauncher.gameObject);
        }
        private void UpdateCharacterPositions(List<GameObject> charactersToLaunch)
        {
            for (var i = 0; i < charactersToLaunch.Count(); i++)
            {
                charactersToLaunch[i].transform.position = _characterLauncher.GetPositionTransforms()[i].position;
            }
        }

        private List<GameObject> SpawnAndGetCharacters(CharacterType[] selectedCharactersList)
        {
            var charList = new List<GameObject>();
            for (var i = 0; i < selectedCharactersList.Count(); i++)
            {
                charList.Add(GameLoader.Instance.poolManager.GetObject<CharacterType>
                    (selectedCharactersList[i], GameLoader.Instance.poolManager.characterPoolDictionary, Vector3.zero, Quaternion.identity).gameObject);
            }
            return charList;
        }

        private void SetLaunchableCharactersBehavior(List<GameObject> charactersToLaunchLeft)
        {
            foreach (var character in charactersToLaunchLeft)
            {
                character.GetComponent<CControl>().unitBehaviorIsStatic = false;
            }
        }
        private void Update()
        {
            //Нажали
            if (Input.GetMouseButtonDown(0) && _canPressAtCharacters)
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);

                //CharacterToLaunch layer
                int layerMask = 1 << 14;

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    if (_charactersToLaunchLeft.Contains(hit.collider.gameObject))
                    {
                        if (hit.collider.gameObject == _charactersToLaunchLeft[0])
                        {
                            //Launch
                            _isLaunchAllowed = true;
                        }
                        else
                        {
                            //Swap
                            SwapCharacters(_charactersToLaunchLeft.IndexOf(hit.collider.gameObject), 0);
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
            if (Input.GetMouseButton(0) && _isLaunchAllowed)
            {

                // Center camera on character collider center
                GameLoader.Instance.cameraManager.CenterCameraAgainst(_charactersToLaunchLeft[0]);

                _characterLauncher.AimingTheLaunch(_charactersToLaunchLeft[0]);
            }

            //Отпустили
            if (Input.GetMouseButtonUp(0) && _isLaunchAllowed)
            {
                Camera.main.orthographicSize /= 1.5f;
                _canPressAtCharacters = false;
                _characterLauncher.LaunchUnit(_charactersToLaunchLeft[0].GetComponent<CharacterControl>());
                _isLaunchAllowed = false;
            }

            //gameover
            if (_charactersToLaunchLeft.Count == 0 && _canPressAtCharacters)
            {
                ColorDebugLog.Log("GAME OVER", KnownColor.Cyan);
            }
        }

        public void Allow_CharacterPress()
        {
            _canPressAtCharacters = true;
        }

        private void SwapCharacters(int indexA, int indexB)
        {
            var tmp = _charactersToLaunchLeft[indexA];
            _charactersToLaunchLeft[indexA] = _charactersToLaunchLeft[indexB];
            _charactersToLaunchLeft[indexB] = tmp;
        }
        private void UpdateCharactersLists(GameObject launchedCharacter)
        {
            _charactersToLaunchLeft.Remove(launchedCharacter);
            _launchedCharacters.Add(launchedCharacter);
        }

        public void OnLaunchIsOver()
        {
            StartCoroutine(OnLaunchIsOver_Routine(GameLoader.Instance.cameraManager.SecondsCameraWaitsAfterAttack));
        }

        private IEnumerator OnLaunchIsOver_Routine(float secondsToWaitAfterAttack)
        {
            GameLoader.Instance.turnManager.AddCharacterToTurnList(_charactersToLaunchLeft[0]);

            yield return new WaitForSeconds(_charactersToLaunchLeft[0].GetComponent<CControl>().animator.GetCurrentAnimatorStateInfo(0).length);
            UpdateCharactersLists(_charactersToLaunchLeft[0]);
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
    }
}
