using Angry_Girls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchHandler : MonoBehaviour
{
    [SerializeField] private CharacterLauncher _characterLauncher;
    [SerializeField] private bool _canPressAtCharacters = false;

    [SerializeField] private List<CControl> _charactersToLaunchLeft = new();
    [SerializeField] private List<CControl> _launchedCharacters = new();

    private CControl CharacterToLaunch => _charactersToLaunchLeft.Count > 0 ? _charactersToLaunchLeft[0] : null;

    private void Start()
    {
        //_characterLauncher.InitLauncher();
        GameLoader.Instance.cameraManager.ReturnCameraToStartPosition(1f);
    }

    public IEnumerator BeginLaunchPhaseRoutine(System.Action onLaunchComplete)
    {
        UpdateCharacterPositions(_charactersToLaunchLeft);
        SetLaunchableCharactersBehavior(_charactersToLaunchLeft);
        _canPressAtCharacters = true;

        while (_charactersToLaunchLeft.Count > 0)
        {
            yield return null;
        }

        onLaunchComplete?.Invoke();
    }

    private void UpdateCharacterPositions(List<CControl> charactersToLaunch)
    {
        var transforms = _characterLauncher.UnitsTransforms;

        for (int i = 0; i < charactersToLaunch.Count; i++)
        {
            charactersToLaunch[i].CharacterMovement.Teleport(transforms[i].position);
        }
    }

    private void SetLaunchableCharactersBehavior(List<CControl> charactersToLaunchLeft)
    {
        foreach (var character in charactersToLaunchLeft)
        {
            character.unitBehaviorIsAlternate = false;
        }
    }

    private void Update()
    {
        if (GameLoader.Instance.gameLogic.GameOver) return;

        if (GameLoader.Instance.inputManager.IsPressed && _canPressAtCharacters)
        {
            HandlePress();
        }

        if (GameLoader.Instance.inputManager.IsHeld && CharacterToLaunch != null)
        {
            _characterLauncher.AimingTheLaunch(CharacterToLaunch.gameObject);
        }

        if (GameLoader.Instance.inputManager.IsReleased && CharacterToLaunch != null)
        {
            HandleRelease();
        }
    }

    private void HandlePress()
    {
        Ray ray = Camera.main.ScreenPointToRay(GameLoader.Instance.inputManager.Position);
        int layerMask = 1 << 14;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            var character = hit.collider.gameObject.GetComponent<CControl>();

            if (_charactersToLaunchLeft.Contains(character))
            {
                if (hit.collider == CharacterToLaunch.boxCollider)
                {
                    GameLoader.Instance.cameraManager.CameraFollowForRigidBody(CharacterToLaunch.CharacterMovement.Rigidbody);
                }
                else
                {
                    SwapCharacters(_charactersToLaunchLeft.IndexOf(character), 0);
                    UpdateCharacterPositions(_charactersToLaunchLeft);
                }
            }
        }
    }

    private void HandleRelease()
    {
        Camera.main.orthographicSize /= 1.5f;
        _canPressAtCharacters = false;

        _characterLauncher.LaunchUnit(CharacterToLaunch);
        GameLoader.Instance.cameraManager.CameraFollowForRigidBody(CharacterToLaunch.CharacterMovement.Rigidbody);
        StartCoroutine(ControlUnitLaunch(CharacterToLaunch));
    }

    private void SwapCharacters(int indexA, int indexB)
    {
        var tmp = _charactersToLaunchLeft[indexA];
        _charactersToLaunchLeft[indexA] = _charactersToLaunchLeft[indexB];
        _charactersToLaunchLeft[indexB] = tmp;
    }

    private IEnumerator ControlUnitLaunch(CControl control)
    {
        control.hasBeenLaunched = true;
        control.transform.root.gameObject.layer = LayerMask.NameToLayer("Character");
        control.checkGlobalBehavior = true;
        control.hasFinishedLaunchingTurn = false;
        control.canUseAbility = true;

        GameLoader.Instance.cameraManager.ZoomOutCameraAfterLaunch();

        while (!control.hasFinishedLaunchingTurn)
        {
            yield return null;
        }

        GameLoader.Instance.turnManager.AddCharacterToTurnList(control);
        _charactersToLaunchLeft.Remove(control);
        _launchedCharacters.Add(control);

        if (_charactersToLaunchLeft.Count > 0)
        {
            UpdateCharacterPositions(_charactersToLaunchLeft);
            _canPressAtCharacters = true;
        }
    }
}