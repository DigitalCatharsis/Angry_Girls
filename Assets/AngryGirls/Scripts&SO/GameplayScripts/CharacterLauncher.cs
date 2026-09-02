using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages character launching mechanics.
    /// </summary>
    public class CharacterLauncher : MonoBehaviour
    {
        [Header("Aiming FX")]
        [SerializeField] private GameObject _aimingHighlightPrefab;
        private GameObject _currentAimingHighlight;

        [Header("Aiming Direction")]
        [SerializeField] private AimingDirectionIndicator _aimingDirectionIndicatorPrefab;
        private AimingDirectionIndicator _currentAimingDirectionIndicator;

        [Header("Launch Constraints")]
        [SerializeField] private float _minLaunchDistance = 1f;

        public bool IsAiming => _isAiming;

        private bool _isAiming;

        private Vector3 _offsetEndPostion;
        private Vector3 _offsetStartPoint;
        private Vector3 _directionVector;
        private Vector3 _launchVelocity;

        [Header("Launching Setup")]
        [SerializeField] private GameObject _positionsContainer;

        [Space(10)]
        private const float _minZoom = 5.0f;
        private const float _maxZoom = 10.0f;

        [SerializeField] private float _maxZoomFactorValue = 6.1f;
        [SerializeField] private Vector2 _zoomRange = new Vector2(_minZoom, _maxZoom);

        [Space(10)]
        [SerializeField] private float _forceFactorUp;
        [SerializeField] private float _forceFactorForward;

        [Header("Trajectory")]
        [SerializeField] private Transform _offsetPoint;
        [SerializeField] private LaunchPathRenderer _launchPathRenderer;

        [Header("Zoom")]
        [SerializeField] private float _minDistanceForZoom;

        [field: SerializeField]
        public Transform[] UnitsTransforms { get; private set; }

        private bool _cheatModeActive;
        private InputManager _inputManager;

        /// <summary>
        /// Initializes launcher components.
        /// </summary>
        public void InitLauncher()
        {
            _inputManager = GameplayCoreManager.Instance.InputManager;

            if (UnitsTransforms == null || UnitsTransforms.Length == 0 || UnitsTransforms[0] == null)
            {
                Debug.LogError("CharacterLauncher: Launch position is not configured.", this);
                return;
            }

            UpdateLaunchStartPoint(UnitsTransforms[0]);

            if (_aimingHighlightPrefab != null)
            {
                _currentAimingHighlight = Instantiate(_aimingHighlightPrefab);
                _currentAimingHighlight.SetActive(false);
            }

            if (_aimingDirectionIndicatorPrefab != null)
            {
                _currentAimingDirectionIndicator = Instantiate(_aimingDirectionIndicatorPrefab);
                _currentAimingDirectionIndicator.Hide();
            }

            if (_launchPathRenderer != null)
                _launchPathRenderer.Hide();
        }

        /// <summary>
        /// Enables or disables cheat trajectory mode.
        /// </summary>
        public void SetCheatTrajectoryMode(bool enable)
        {
            _cheatModeActive = enable;

            if (!_isAiming)
                return;

            DrawTrajectory();
        }

        /// <summary>
        /// Launches the specified character using the calculated launch velocity.
        /// </summary>
        public void LaunchUnit(CControl characterToLaunch)
        {
            if (characterToLaunch == null)
                return;

            CalculateLaunchVelocity();

            // Keep the original character orientation logic.
            // Launch velocity uses inverted Z, therefore orientation
            // must be determined from the original drag direction.
            if (_directionVector.z > 0f)
            {
                characterToLaunch.CharacterMovement.SetRotation(
                    Quaternion.Euler(0f, 180f, 0f));
            }
            else if (_directionVector.z < 0f)
            {
                characterToLaunch.CharacterMovement.SetRotation(
                    Quaternion.Euler(0f, 0f, 0f));
            }

            CancelAiming();

            characterToLaunch.CharacterMovement.Rigidbody.useGravity = true;
            characterToLaunch.CharacterMovement.SetVelocity(_launchVelocity);
            characterToLaunch.UnitHasBeenLaunched?.Invoke();
        }

        /// <summary>
        /// Starts or updates character aiming.
        /// </summary>
        public void AimingTheLaunch(GameObject characterToLaunch)
        {
            if (characterToLaunch == null)
                return;

            _isAiming = true;

            UpdateLaunchStartPoint(characterToLaunch.transform);
            CalculateDirection();
            CalculateLaunchVelocity();

            DrawTrajectory();
            AdjustCameraZoom();

            ShowAimingHighlight();
            UpdateAimingDirectionIndicator(characterToLaunch.transform);
        }

        private void UpdateLaunchStartPoint(Transform characterTransform)
        {
            if (characterTransform == null)
                return;

            _offsetStartPoint = new Vector3(
                characterTransform.position.x,
                characterTransform.position.y + 0.4f,
                characterTransform.position.z);
        }

        private void ShowAimingHighlight()
        {
            if (_currentAimingHighlight == null)
                return;

            _currentAimingHighlight.transform.position = _offsetStartPoint;
            _currentAimingHighlight.SetActive(true);
        }

        private void HideAimingHighlight()
        {
            if (_currentAimingHighlight == null)
                return;

            _currentAimingHighlight.SetActive(false);
        }

        private void UpdateAimingDirectionIndicator(Transform characterTransform)
        {
            if (_currentAimingDirectionIndicator == null || characterTransform == null)
                return;

            if (_launchVelocity.sqrMagnitude <= 0.0001f)
            {
                _currentAimingDirectionIndicator.Hide();
                return;
            }

            _currentAimingDirectionIndicator.Show(
                characterTransform,
                _launchVelocity.normalized);
        }

        /// <summary>
        /// Cancels the current aiming state.
        /// </summary>
        public void CancelAiming()
        {
            GameplayCoreManager.Instance.CameraManager.StopCameraFollowForRigidBody();

            _isAiming = false;

            if (_launchPathRenderer != null)
                _launchPathRenderer.Hide();

            HideAimingHighlight();

            if (_currentAimingDirectionIndicator != null)
                _currentAimingDirectionIndicator.Hide();
        }

        /// <summary>
        /// Checks whether the current launch distance is sufficient.
        /// </summary>
        public bool IsLaunchDistanceSufficient()
        {
            return _directionVector.magnitude >= _minLaunchDistance;
        }

        private void CalculateDirection()
        {
            if (_inputManager == null)
                return;

            var mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            var screenPosition = _inputManager.Position;
            screenPosition.z = mainCamera.nearClipPlane + 1f;

            var pointerPosition = mainCamera.ScreenToWorldPoint(screenPosition);

            _offsetEndPostion = new Vector3(
                _offsetStartPoint.x,
                pointerPosition.y,
                pointerPosition.z);

            _directionVector = _offsetEndPostion - _offsetStartPoint;
            _directionVector.x = 0f;
        }

        private void CalculateLaunchVelocity()
        {
            _launchVelocity = new Vector3(
                0f,
                -_directionVector.y * _forceFactorUp,
                -_directionVector.z * _forceFactorForward);

            _launchVelocity.x = 0f;
        }

        private void DrawTrajectory()
        {
            if (_offsetPoint != null)
                _offsetPoint.position = _offsetEndPostion;

            if (_launchPathRenderer == null)
                return;

            var duration = _cheatModeActive ? 10f : 1f;

            _launchPathRenderer.Draw(
                _offsetStartPoint,
                _launchVelocity,
                Physics.gravity,
                duration);
        }

        /// <summary>
        /// Calculates a trajectory position using the same physics
        /// values as the actual launch.
        /// </summary>
        public Vector3 CalculateTrajectoryPosition(float elapsedTime)
        {
            var position =
                _offsetStartPoint +
                _launchVelocity * elapsedTime +
                0.5f * Physics.gravity * elapsedTime * elapsedTime;

            position.x = _offsetStartPoint.x;

            return position;
        }

        private void AdjustCameraZoom()
        {
            if (Camera.main == null || _minDistanceForZoom <= 0f)
                return;

            var distance = Vector3.Distance(
                _offsetEndPostion,
                _offsetStartPoint);

            var zoomFactor = Mathf.Lerp(
                _zoomRange.x,
                _zoomRange.y,
                distance / _minDistanceForZoom);

            Camera.main.orthographicSize = Mathf.Min(
                zoomFactor,
                _maxZoomFactorValue);
        }
    }
}