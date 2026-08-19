using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages gameplay camera movement and zoom.
    /// Camera world X and Y coordinates are immutable during gameplay.
    /// Only world Z position and orthographic size may change.
    /// </summary>
    public class CameraManager : GameplayManagerClass
    {
        [Header("Setup")]
        [SerializeField] private const float startOrthographicCameraSize = 3f;

        [Header("Platform Defaults")]
        [SerializeField] private float _defaultMovementSpeed = 0.5f;
        [SerializeField] private float _defaultZoomSensitivity = 7.0f;

        [SerializeField] private const float _secondsCameraWaitsAfterAttack = 2f;
        [SerializeField] private const float _zoomeCameraValueAfterLaunch = 7.5f;

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSensitivity = 1f;
        [SerializeField] private float _minZoom = 1.55f;
        [SerializeField] private float _maxZoom = 15f;

        [Header("Camera Movement Settings")]
        [SerializeField] private float _movementSpeed = 6.5f;
        [SerializeField] private float _minCameraZ = -10f;
        [SerializeField] private float _maxCameraZ = 45f;
        [SerializeField] private float _cameraMoveDuration = 0.5f;
        [SerializeField] private Ease _cameraMoveEase = Ease.InOutCubic;

        [Header("Camera Shake")]
        [SerializeField] private float _defaultShakeDuration = 0.3f;
        [SerializeField] private float _defaultShakeMagnitude = 0.05f;

        [Header("Zoom After Ready")]
        [SerializeField] private float _zoomInAfterReadyDuration = 1f;
        [SerializeField] private float _targetZoomAfterReady = 1.55f;

        public float SecondsCameraWaitsAfterAttack => _secondsCameraWaitsAfterAttack;

        [SerializeField] private Rigidbody _characterToFollow;
        [SerializeField] private bool _allowCameraFollow;

        [Header("Debug")]
        [SerializeField] private Camera _mainCamera;

        private InputManager _inputManager;
        private Sequence _cameraMoveSequence;
        private SettingsManager _settingsManager;

        private float _fixedCameraX;
        private float _fixedCameraY;
        private bool _fixedPositionInitialized;

        /// <summary>
        /// Initializes the camera manager and locks the camera world X/Y coordinates.
        /// </summary>
        public override void Initialize()
        {
            KillCameraTwins();

            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("CameraManager: Main camera was not found.");
                return;
            }

            _inputManager = GameplayCoreManager.Instance.InputManager;
            _settingsManager = CoreManager.Instance.SettingsManager;

            ApplyCameraSettingsFromManager(SettingsCategory.Camera);
            SubscribeToSettingsChanges();

            LockCameraXY();
            EnforceCameraXY();

            isInitialized = true;
        }

        /// <summary>
        /// Applies camera settings from SettingsManager.
        /// </summary>
        private void ApplyCameraSettingsFromManager(SettingsCategory settingsCategory)
        {
            if (_settingsManager == null)
                return;

            if (settingsCategory != SettingsCategory.Camera &&
                settingsCategory != SettingsCategory.All)
            {
                return;
            }

            var settings = _settingsManager.GetCurrentSettings();
            _movementSpeed = Mathf.Max(0f, settings.cameraMovementSpeed);
        }

        /// <summary>
        /// Subscribes to settings changes.
        /// </summary>
        private void SubscribeToSettingsChanges()
        {
            if (_settingsManager != null)
                _settingsManager.OnSettingsChanged += ApplyCameraSettingsFromManager;
        }

        private void Update()
        {
            if (!isInitialized || _mainCamera == null)
                return;

            EnforceCameraXY();

            if (!_allowCameraFollow)
            {
                HandleZoom();
                HandleMovement();
            }

            EnforceCameraXY();
        }

        private void LateUpdate()
        {
            if (!isInitialized || _mainCamera == null)
                return;

            if (_characterToFollow != null && _allowCameraFollow)
            {
                CenterCameraAgainst(_characterToFollow);
            }

            EnforceCameraXY();
        }

        /// <summary>
        /// Stores the camera's initial world X/Y coordinates.
        /// </summary>
        private void LockCameraXY()
        {
            if (_mainCamera == null)
                return;

            var position = _mainCamera.transform.position;

            _fixedCameraX = position.x;
            _fixedCameraY = position.y;
            _fixedPositionInitialized = true;
        }

        /// <summary>
        /// Restores the immutable camera world X/Y coordinates.
        /// </summary>
        private void EnforceCameraXY()
        {
            if (!_fixedPositionInitialized || _mainCamera == null)
                return;

            var position = _mainCamera.transform.position;

            if (Mathf.Approximately(position.x, _fixedCameraX) &&
                Mathf.Approximately(position.y, _fixedCameraY))
            {
                return;
            }

            _mainCamera.transform.position = new Vector3(
                _fixedCameraX,
                _fixedCameraY,
                position.z);
        }

        private void HandleZoom()
        {
            if (_inputManager == null)
                return;

            var zoomDelta = _inputManager.GetZoomDelta();

            if (!Mathf.Approximately(zoomDelta, 0f))
                ApplyZoom(zoomDelta * _zoomSensitivity);
        }

        /// <summary>
        /// Changes only orthographic size.
        /// </summary>
        private void ApplyZoom(float delta)
        {
            if (_mainCamera == null)
                return;

            if (!Mathf.Approximately(delta, 0f))
                _allowCameraFollow = false;

            _mainCamera.orthographicSize = Mathf.Clamp(
                _mainCamera.orthographicSize - delta,
                _minZoom,
                _maxZoom);

            EnforceCameraXY();
        }

        private void HandleMovement()
        {
            if (_inputManager == null || !_inputManager.IsDragging())
                return;

            var delta = _inputManager.GetDragDelta();

            if (!IsPointerOverCharacter(_inputManager.Position))
                MoveCamera(delta);
        }

        private bool IsPointerOverCharacter(Vector2 screenPosition)
        {
            if (_mainCamera == null)
                return false;

            var ray = _mainCamera.ScreenPointToRay(screenPosition);
            var layerMask = 1 << 14;

            return Physics.Raycast(ray, Mathf.Infinity, layerMask);
        }

        /// <summary>
        /// Moves the camera exclusively along world Z.
        /// </summary>
        private void MoveCamera(Vector2 delta)
        {
            if (_mainCamera == null || delta.sqrMagnitude <= 0f)
                return;

            _allowCameraFollow = false;

            var speed = _movementSpeed *
                        _mainCamera.orthographicSize *
                        Time.deltaTime;

            var z = _mainCamera.transform.position.z - delta.x * speed;
            SetCameraZ(z);
        }

        /// <summary>
        /// Follows a Rigidbody target and immediately synchronizes the camera Z position.
        /// </summary>
        public void CameraFollowForRigidBody(Rigidbody characterToFollow)
        {
            if (characterToFollow == null)
            {
                StopCameraFollowForRigidBody();
                return;
            }

            KillCameraMoveSequence();

            _characterToFollow = characterToFollow;
            _allowCameraFollow = true;

            // Use the interpolated Transform position immediately.
            // This prevents a one-frame jump when follow starts.
            CenterCameraAgainst(_characterToFollow);

            EnforceCameraXY();
        }

        /// <summary>
        /// Stops following a rigidbody.
        /// </summary>
        public void StopCameraFollowForRigidBody()
        {
            _characterToFollow = null;
            _allowCameraFollow = false;
        }

        /// <summary>
        /// Follows the launched character using the Rigidbody's interpolated Transform position.
        /// Camera X/Y remain immutable; only Z is updated.
        /// </summary>
        private void CenterCameraAgainst(Rigidbody target)
        {
            if (target == null || _mainCamera == null)
                return;

            var targetZ = target.transform.position.z;
            SetCameraZ(targetZ);
        }

        /// <summary>
        /// Applies the post-launch zoom without modifying camera position.
        /// </summary>
        public void ZoomOutCameraAfterLaunch()
        {
            if (_mainCamera == null)
                return;

            _mainCamera.orthographicSize = Mathf.Clamp(
                _mainCamera.orthographicSize -
                (_mainCamera.orthographicSize / _zoomeCameraValueAfterLaunch),
                _minZoom,
                _maxZoom);

            EnforceCameraXY();
        }

        /// <summary>
        /// Smoothly moves the camera exclusively along world Z.
        /// The supplied X/Y coordinates are intentionally ignored.
        /// </summary>
        public void MoveCameraTo(Vector3 targetPosition, float speed, bool resetZoom = false)
        {
            if (_mainCamera == null)
                return;

            StopCameraFollowForRigidBody();
            KillCameraMoveSequence();

            var duration = Mathf.Max(0f, speed);
            var targetZ = Mathf.Clamp(targetPosition.z, _minCameraZ, _maxCameraZ);

            _cameraMoveSequence = DOTween.Sequence();

            _cameraMoveSequence.Append(
                _mainCamera.transform
                    .DOMoveZ(targetZ, duration)
                    .SetEase(_cameraMoveEase));

            if (resetZoom)
            {
                _cameraMoveSequence.Join(
                    _mainCamera
                        .DOOrthoSize(startOrthographicCameraSize, duration)
                        .SetEase(_cameraMoveEase));
            }

            _cameraMoveSequence.OnUpdate(EnforceCameraXY);

            _cameraMoveSequence.OnComplete(() =>
            {
                EnforceCameraXY();
                _cameraMoveSequence = null;
            });
        }

        /// <summary>
        /// Shakes the camera exclusively along world Z.
        /// </summary>
        public void ShakeCamera(
            float shakeDuration = -1f,
            float shakeMagnitude = -1f)
        {
            if (_mainCamera == null)
                return;

            shakeDuration = shakeDuration > 0f
                ? shakeDuration
                : _defaultShakeDuration;

            shakeMagnitude = shakeMagnitude >= 0f
                ? shakeMagnitude
                : _defaultShakeMagnitude;

            StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
        }

        private IEnumerator ShakeCoroutine(float shakeDuration, float shakeMagnitude)
        {
            if (_mainCamera == null)
                yield break;

            KillCameraMoveSequence();

            var originalZ = _mainCamera.transform.position.z;
            var elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                if (_mainCamera == null)
                    yield break;

                var shakeZ = Random.Range(-1f, 1f) * shakeMagnitude;

                SetCameraZ(originalZ + shakeZ);

                elapsed += Time.deltaTime;
                yield return null;
            }

            SetCameraZ(originalZ);
        }

        /// <summary>
        /// Sets camera world Z while preserving immutable X/Y.
        /// </summary>
        private void SetCameraZ(float z)
        {
            if (_mainCamera == null)
                return;

            z = Mathf.Clamp(z, _minCameraZ, _maxCameraZ);

            _mainCamera.transform.position = new Vector3(
                _fixedCameraX,
                _fixedCameraY,
                z);
        }

        /// <summary>
        /// Cancels the active camera movement tween.
        /// </summary>
        private void KillCameraMoveSequence()
        {
            if (_cameraMoveSequence == null)
                return;

            _cameraMoveSequence.Kill();
            _cameraMoveSequence = null;

            EnforceCameraXY();
        }

        private void KillCameraTwins()
        {
            var allCameras = FindObjectsOfType<Camera>();
            Camera mainCamera = null;

            foreach (var cam in allCameras)
            {
                if (cam != null && cam.CompareTag("MainCamera"))
                {
                    mainCamera = cam;
                    break;
                }
            }

            foreach (var cam in allCameras)
            {
                if (cam != null && cam != mainCamera)
                    Destroy(cam.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_settingsManager != null)
                _settingsManager.OnSettingsChanged -= ApplyCameraSettingsFromManager;

            KillCameraMoveSequence();
            KillCameraTwins();
        }

        /// <summary>
        /// Smoothly zooms the camera to the target size without moving it.
        /// </summary>
        public void ZoomInAfterReady()
        {
            if (_mainCamera == null)
                return;

            _mainCamera.DOKill();

            _mainCamera
                .DOOrthoSize(
                    Mathf.Clamp(_targetZoomAfterReady, _minZoom, _maxZoom),
                    Mathf.Max(0f, _zoomInAfterReadyDuration))
                .SetEase(_cameraMoveEase)
                .OnUpdate(EnforceCameraXY);
        }
    }
}