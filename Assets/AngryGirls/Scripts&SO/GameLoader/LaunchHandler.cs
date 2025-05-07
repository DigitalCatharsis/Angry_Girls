using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class LaunchHandler : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CharacterLauncher _characterLauncher;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private CameraManager _cameraManager;

        [Header("Settings")]
        [SerializeField] private LayerMask _characterLaunchLayer;

        public event Action OnCharacterLaunched;

        private List<CControl> _charactersToLaunch = new();
        private CControl _currentCharacter;
        private bool _isLaunchAllowed;
        private bool _canPressCharacters = true;

        //public void Initialize(List<CControl> characters)
        //{
        //    _charactersToLaunch = characters;
        //    PositionCharacters();
        //    EnableControls();
        //}

        public void BeginLaunchPhase()
        {
            if (_charactersToLaunch.Count == 0)
            {
                Debug.LogWarning("No characters to launch!");
                return;
            }

            EnableControls();
            _currentCharacter = _charactersToLaunch[0];
        }

        private void Update()
        {
            if (!_canPressCharacters) return;

            HandleLaunchInput();
        }

        private void HandleLaunchInput()
        {
            if (_inputManager.IsPressed)
            {
                TrySelectCharacter();
            }
            else if (_inputManager.IsHeld && _isLaunchAllowed)
            {
                _characterLauncher.AimingTheLaunch(_currentCharacter.gameObject);
            }
            else if (_inputManager.IsReleased && _isLaunchAllowed)
            {
                LaunchCurrentCharacter();
            }
        }

        private void TrySelectCharacter()
        {
            var ray = Camera.main.ScreenPointToRay(_inputManager.Position);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _characterLaunchLayer))
            {
                var character = hit.collider.GetComponent<CControl>();

                if (_charactersToLaunch.Contains(character))
                {
                    if (hit.collider == _currentCharacter.boxCollider)
                    {
                        _cameraManager.CameraFollowForRigidBody(_currentCharacter.CharacterMovement.Rigidbody);
                        _isLaunchAllowed = true;
                    }
                    else
                    {
                        SwapCharacters(character);
                    }
                }
            }
        }

        private void SwapCharacters(CControl newCharacter)
        {
            int index = _charactersToLaunch.IndexOf(newCharacter);
            (_charactersToLaunch[0], _charactersToLaunch[index]) = (_charactersToLaunch[index], _charactersToLaunch[0]);

            PositionCharacters();
            _isLaunchAllowed = false;
            _currentCharacter = _charactersToLaunch[0];
        }

        private void LaunchCurrentCharacter()
        {
            _canPressCharacters = false;
            _isLaunchAllowed = false;

            _characterLauncher.LaunchUnit(_currentCharacter);
            _cameraManager.CameraFollowForRigidBody(_currentCharacter.CharacterMovement.Rigidbody);

            StartCoroutine(ProcessLaunch(_currentCharacter));
        }

        private IEnumerator ProcessLaunch(CControl character)
        {
            // Настройка персонажа после запуска
            character.OnLaunchComplete += HandleLaunchComplete;
            yield return character.ExecuteLaunchTurn();

            // Обновляем списки
            _charactersToLaunch.Remove(character);
            OnCharacterLaunched?.Invoke();
        }

        private void HandleLaunchComplete()
        {
            _cameraManager.ReturnCameraToStartPosition(1f);
        }

        private void PositionCharacters()
        {
            var positions = _characterLauncher.GetPositionTransforms();
            for (int i = 0; i < _charactersToLaunch.Count; i++)
            {
                _charactersToLaunch[i].CharacterMovement.Teleport(positions[i].position);
            }
        }

        private void EnableControls() => _canPressCharacters = true;
        private void DisableControls() => _canPressCharacters = false;

        public bool HasCharactersToLaunch() => _charactersToLaunch.Count > 0;
    }
}