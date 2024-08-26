using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchManager : GameLoaderComponent
    {
        public static LaunchManager Instance;
        [Header("Setup")]
        [SerializeField] private CharacterSelect characterSelectSO;
        [SerializeField] private CharacterLauncher _characterLauncher;
        [SerializeField] private Transform _characterLauncherStartTransform;  //TODO: ? does it neccesery?

        [Space(10)]
        [Header("Conditions")]
        [SerializeField] private bool _isLaunchAllowed = false;
        [SerializeField] private bool _canPressAtCharacters = false;
        [Space(10)]
        [Header("Debug)")]
        [SerializeField] private List<GameObject> _charactersToLaunchLeft;
        [SerializeField] private List<GameObject> _launchedCharacters;

        public override void OnComponentEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _characterLauncher.InitLauncher();
            _charactersToLaunchLeft = SpawnAndGetCharacters(characterSelectSO.selectedCharacters);
            UpdateCharacterPositions(_charactersToLaunchLeft);
            SetLaunchableCharactersBehavior(_charactersToLaunchLeft);
            _canPressAtCharacters = true;
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
                //charList.Add(Instantiate(Resources.Load(selectedCharactersList[i].ToString())) as GameObject); //old
                charList.Add(GameLoader.Instance.poolManager.GetObject<CharacterType>
                    (selectedCharactersList[i], GameLoader.Instance.poolManager.characterPoolDictionary, Vector3.zero, Quaternion.identity));
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
                CameraManager.Instance.CenterCameraAgainst(_charactersToLaunchLeft[0].GetComponent<CharacterControl>().boxCollider);

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
            StartCoroutine(OnLaunchIsOver_Routine(CameraManager.Instance.SecondsCameraWaitsAfterAttack));
        }

        private IEnumerator OnLaunchIsOver_Routine(float secondsToWaitAfterAttack)
        {
            TurnManager.Instance.AddCharacterToTurnList(_charactersToLaunchLeft[0]);
            //GameLoader.Instance._turnManager.AddCharacterToTurnList(_charactersToLaunchLeft[0]);

            yield return new WaitForSeconds(_charactersToLaunchLeft[0].GetComponent<CharacterControl>().animator.GetCurrentAnimatorStateInfo(0).length);
            UpdateCharactersLists(_charactersToLaunchLeft[0]);
            yield return new WaitForSeconds(secondsToWaitAfterAttack);
            UpdateCharacterPositions(_charactersToLaunchLeft);

            if (TurnManager.Instance.CurrentTurn < 1)
            {
                CameraManager.Instance.ReturnCameraToStartPosition(1f);
                _canPressAtCharacters = true;
                TurnManager.Instance.IncrementCurentTurn();
                //GameLoader.Instance._turnManager._currentTurn++;
            }
            else
            {
                TurnManager.Instance.isLaunchingPhaseOver = true;
            }
        }
    }
}
