using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Service for launch execution mechanics (aiming, trajectory, force application).
    /// Handles visual feedback (trajectory dots, camera zoom) and physics of character launch.
    /// Decoupled from input handling and character selection logic.
    /// </summary>
    public class LaunchExecutionService : GameplayManagerClass
    {
        private CharacterLauncher _launcher;
        private CameraManager _cameraManager;

        public override void Initialize()
        {
            isInitialized = true;
            GameplayCoreManager.Instance.OnInitialized += LateInitialize;
        }

        private void LateInitialize()
        {
            _cameraManager = GameplayCoreManager.Instance.CameraManager;
            GameplayCoreManager.Instance.OnInitialized -= LateInitialize;
        }

        public void PrepareLaunch(CharacterLauncher launcher, List<CControl> characters, Transform[] positions)
        {
            _launcher = launcher;
            UpdateCharacterPositions(characters, positions);
        }

        public bool TryStartAiming(CControl character/*, bool isCurrentSelection*/)
        {
            //if (!isCurrentSelection) return false;

            _launcher.AimingTheLaunch(character.gameObject);
            _cameraManager.CameraFollowForRigidBody(character.CharacterMovement.Rigidbody);
            return true;
        }

        public void CancelAiming()
        {
            _launcher.CancelAiming();
        }

        public bool TryExecuteLaunch(CControl character)
        {
            if (!_launcher.IsLaunchDistanceSufficient()) return false;

            _launcher.LaunchUnit(character);
            _cameraManager.CameraFollowForRigidBody(character.CharacterMovement.Rigidbody);
            _cameraManager.ZoomOutCameraAfterLaunch();
            return true;
        }

        private void UpdateCharacterPositions(List<CControl> characters, Transform[] positions)
        {
            for (int i = 0; i < characters.Count && i < positions.Length; i++)
            {
                characters[i].CharacterMovement.Teleport(positions[i].position);
            }
        }
    }
}