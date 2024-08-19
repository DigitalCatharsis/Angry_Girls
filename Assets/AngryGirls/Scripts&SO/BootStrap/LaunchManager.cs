using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private CharacterSelect characterSelectSO;
        [SerializeField] private CharacterLauncher characterLauncher;
        [SerializeField] private Transform characterLauncherStartTransform;

        [Space(10)]
        [Header("Conditions")]
        public bool isLaunchAllowed = false;
        public bool canPressAtCharacters = false;

        [Space(10)]
        [Header("Debug)")]
        public List<GameObject> charactersToLaunchLeft;
        public List<GameObject> launchedCharacters;

        //Characters
        [SerializeField] public CharacterType[] SelectedCharactersList => characterSelectSO.selectedCharacters;

        private void Start()
        {
            characterLauncher.InitLauncher();
            charactersToLaunchLeft = characterLauncher.SpawnAndGetCharacters(SelectedCharactersList);
            characterLauncher.UpdateCharacterPositions(charactersToLaunchLeft);
            characterLauncher.SetLaunchableCharactersBehavior(charactersToLaunchLeft);
            canPressAtCharacters = true;
        }

        private void Update()
        {
            //Íàæàëè
            if (Input.GetMouseButtonDown(0) && canPressAtCharacters)
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);

                //CharacterToLaunch layer
                int layerMask = 1 << 14;

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    if (charactersToLaunchLeft.Contains(hit.collider.gameObject))
                    {
                        if (hit.collider.gameObject == charactersToLaunchLeft[0])
                        {
                            //Launch
                            isLaunchAllowed = true;
                        }
                        else
                        {
                            //Swap
                            SwapCharacters(charactersToLaunchLeft.IndexOf(hit.collider.gameObject), 0);
                            characterLauncher.UpdateCharacterPositions(charactersToLaunchLeft);
                            isLaunchAllowed = false;
                        }
                    }
                    else
                    {
                        isLaunchAllowed = false;
                    }
                }
            }

            //Äåðæèì
            if (Input.GetMouseButton(0) && isLaunchAllowed)
            {

                // Center camera on character collider center
                Singleton.Instance.ñameraManager.CenterCameraAgainst(charactersToLaunchLeft[0].GetComponent<CharacterControl>().boxCollider);

                characterLauncher.AimingTheLaunch(charactersToLaunchLeft[0]);
            }

            //Îòïóñòèëè
            if (Input.GetMouseButtonUp(0) && Singleton.Instance.launchManager.isLaunchAllowed)
            {
                Camera.main.orthographicSize /= 1.5f;
                canPressAtCharacters = false;
                characterLauncher.LaunchUnit(charactersToLaunchLeft[0].GetComponent<CharacterControl>());
                isLaunchAllowed = false;
            }

            //gameover
            if (charactersToLaunchLeft.Count == 0 && canPressAtCharacters)
            {
                ColorDebugLog.Log("GAME OVER", KnownColor.Cyan);
            }
        }

        public void SwapCharacters(int indexA, int indexB)
        {
            var tmp = charactersToLaunchLeft[indexA];
            charactersToLaunchLeft[indexA] = charactersToLaunchLeft[indexB];
            charactersToLaunchLeft[indexB] = tmp;
        }
        public void UpdateCharactersLists(GameObject launchedCharacter)
        {
            charactersToLaunchLeft.Remove(launchedCharacter);
            launchedCharacters.Add(launchedCharacter);
        }

        public void OnLaunchIsOver()
        {
            StartCoroutine(OnLaunchIsOver_Routine(Singleton.Instance.ñameraManager.secondsCameraWaitsAfterAttack));
        }

        private IEnumerator OnLaunchIsOver_Routine(float secondsToWaitAfterAttack)
        {
            Singleton.Instance.turnManager.AddCharacterToTurnList(charactersToLaunchLeft[0]);

            yield return new WaitForSeconds(charactersToLaunchLeft[0].GetComponent<CharacterControl>().animator.GetCurrentAnimatorStateInfo(0).length);
            UpdateCharactersLists(charactersToLaunchLeft[0]);
            yield return new WaitForSeconds(secondsToWaitAfterAttack);
            characterLauncher.UpdateCharacterPositions(charactersToLaunchLeft);

            if (Singleton.Instance.turnManager.currentTurn < 1)
            {
                Singleton.Instance.ñameraManager.ReturnCameraToStartPosition(1f);
                canPressAtCharacters = true;
                Singleton.Instance.turnManager.currentTurn++;
            }
            else
            {
                Singleton.Instance.turnManager.isLaunchingPhaseOver = true;
            }
        }
    }
}
