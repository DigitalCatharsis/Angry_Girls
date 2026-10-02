using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Angry_Girls
{
    /// <summary>
    /// Manages gameplay camera movement and zoom for the XY gameplay plane.
    /// World Z is the immutable camera depth axis.
    /// Camera movement and character following operate on world X/Y.
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

        [FormerlySerializedAs("_minCameraZ")]
        [SerializeField] private float _minCameraX = -10f;

        [FormerlySerializedAs("_maxCameraZ")]
        [SerializeField] private float _maxCameraX = 45f;

        [SerializeField] private float _minCameraY = -10f;
        [SerializeField] private float _maxCameraY = 45f;

        [SerializeField] private float _cameraMoveDuration = 0.5f;
        [SerializeField] private Ease _cameraMoveEase = Ease.InOutCubic;

        [Header("Camera Shake")]
        [SerializeField] private float _defaultShakeDuration = 0.3f;
        [SerializeField] private float _defaultShakeMagnitude = 0.05f;

        [Header("Zoom After Ready")]
        [SerializeField] private float _zoomInAfterReadyDuration = 1f;
        [SerializeField] private float _targetZoomAfterReady = 1.55f;

        public float SecondsCameraWaitsAfterAttack =>
            _secondsCameraWaitsAfterAttack;

        [Header("Follow")]
        [SerializeField] private Rigidbody _characterToFollow;
        [SerializeField] private bool _allowCameraFollow;

        [Header("Debug")]
        [SerializeField] private Camera _mainCamera;

        private InputManager _inputManager;
        private Sequence _cameraMoveSequence;
        private SettingsManager _settingsManager;

        private float _fixedCameraZ;
        private bool _fixedPositionInitialized;

        private bool _cameraPanBlockedByUI;

        private Vector3 _cameraShakeOffset;
        private Coroutine _cameraShakeRoutine;

        /// <summary>
        /// Initializes the camera manager.
        /// </summary>
        public override void Initialize()
        {
            KillCameraTwins();

            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError(
                    "CameraManager: Main camera was not found.");

                return;
            }

            _inputManager =
                GameplayCoreManager.Instance
                    .InputManager;

            _settingsManager =
                CoreManager.Instance
                    .SettingsManager;

            ApplyCameraSettingsFromManager(
                SettingsCategory.Camera);

            SubscribeToSettingsChanges();

            NormalizeCameraBounds();

            LockCameraZ();
            EnforceCameraPosition();

            isInitialized = true;
        }

        /// <summary>
        /// Applies camera settings from SettingsManager.
        /// </summary>
        private void ApplyCameraSettingsFromManager(
            SettingsCategory settingsCategory)
        {
            if (_settingsManager == null)
                return;

            if (settingsCategory != SettingsCategory.Camera &&
                settingsCategory != SettingsCategory.All)
            {
                return;
            }

            var settings =
                _settingsManager
                    .GetCurrentSettings();

            _movementSpeed =
                Mathf.Max(
                    0f,
                    settings.cameraMovementSpeed);
        }

        /// <summary>
        /// Subscribes to settings changes.
        /// </summary>
        private void SubscribeToSettingsChanges()
        {
            if (_settingsManager != null)
            {
                _settingsManager
                    .OnSettingsChanged +=
                    ApplyCameraSettingsFromManager;
            }
        }

        private void NormalizeCameraBounds()
        {
            if (_minCameraX > _maxCameraX)
            {
                var minX = _minCameraX;

                _minCameraX = _maxCameraX;
                _maxCameraX = minX;
            }

            if (_minCameraY > _maxCameraY)
            {
                var minY = _minCameraY;

                _minCameraY = _maxCameraY;
                _maxCameraY = minY;
            }
        }

        private void Update()
        {
            if (!isInitialized ||
                _mainCamera == null)
            {
                return;
            }

            UpdateUIPanBlockState();

            EnforceCameraPosition();

            if (!_allowCameraFollow)
            {
                HandleZoom();
                HandleMovement();
            }

            EnforceCameraPosition();
        }

        private void LateUpdate()
        {
            if (!isInitialized ||
                _mainCamera == null)
            {
                return;
            }

            if (_characterToFollow != null &&
                _allowCameraFollow)
            {
                CenterCameraAgainst(
                    _characterToFollow);
            }

            EnforceCameraPosition();
            ApplyCameraShakeOffset();
        }

        /// <summary>
        /// Tracks whether the current drag started over UI.
        /// Once blocked, camera movement remains disabled until release.
        /// </summary>
        private void UpdateUIPanBlockState()
        {
            if (Application.isMobilePlatform)
            {
                UpdateMobileUIPanBlockState();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _cameraPanBlockedByUI =
                    IsPointerOverUI();
            }

            if (Input.GetMouseButtonUp(0))
            {
                _cameraPanBlockedByUI = false;
            }
        }

        private void UpdateMobileUIPanBlockState()
        {
            if (Input.touchCount <= 0)
                return;

            for (var i = 0;
                 i < Input.touchCount;
                 i++)
            {
                var touch =
                    Input.GetTouch(i);

                if (touch.phase ==
                    TouchPhase.Began)
                {
                    _cameraPanBlockedByUI =
                        IsPointerOverUI(
                            touch.fingerId);
                }

                if (touch.phase ==
                    TouchPhase.Ended ||
                    touch.phase ==
                    TouchPhase.Canceled)
                {
                    _cameraPanBlockedByUI = false;
                }
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current
                .IsPointerOverGameObject();
        }

        private bool IsPointerOverUI(
            int fingerId)
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current
                .IsPointerOverGameObject(
                    fingerId);
        }

        /// <summary>
        /// Stores the immutable camera Z coordinate.
        /// </summary>
        private void LockCameraZ()
        {
            if (_mainCamera == null)
                return;

            _fixedCameraZ =
                _mainCamera
                    .transform
                    .position
                    .z;

            _fixedPositionInitialized = true;
        }

        /// <summary>
        /// Restores immutable camera Z while keeping X/Y.
        /// </summary>
        private void EnforceCameraPosition()
        {
            if (!_fixedPositionInitialized ||
                _mainCamera == null)
            {
                return;
            }

            var position =
                _mainCamera
                    .transform
                    .position;

            var x =
                Mathf.Clamp(
                    position.x,
                    _minCameraX,
                    _maxCameraX);

            var y =
                Mathf.Clamp(
                    position.y,
                    _minCameraY,
                    _maxCameraY);

            var targetPosition =
                new Vector3(
                    x,
                    y,
                    _fixedCameraZ);

            if ((position - targetPosition)
                .sqrMagnitude <= 0.0000001f)
            {
                return;
            }

            _mainCamera.transform.position =
                targetPosition;
        }

        private void HandleZoom()
        {
            if (_inputManager == null)
                return;

            var zoomDelta =
                _inputManager.GetZoomDelta();

            if (!Mathf.Approximately(
                    zoomDelta,
                    0f))
            {
                ApplyZoom(
                    zoomDelta *
                    _zoomSensitivity);
            }
        }

        /// <summary>
        /// Changes only camera orthographic size.
        /// </summary>
        private void ApplyZoom(
            float delta)
        {
            if (_mainCamera == null)
                return;

            if (!Mathf.Approximately(
                    delta,
                    0f))
            {
                _allowCameraFollow = false;
            }

            _mainCamera.orthographicSize =
                Mathf.Clamp(
                    _mainCamera.orthographicSize -
                    delta,
                    _minZoom,
                    _maxZoom);

            EnforceCameraPosition();
        }

        private void HandleMovement()
        {
            if (_cameraPanBlockedByUI)
                return;

            if (_inputManager == null ||
                !_inputManager.IsDragging())
            {
                return;
            }

            var delta =
                _inputManager.GetDragDelta();

            if (!IsPointerOverCharacter(
                    _inputManager.Position))
            {
                MoveCamera(delta);
            }
        }

        private bool IsPointerOverCharacter(
            Vector2 screenPosition)
        {
            if (_mainCamera == null)
                return false;

            var ray =
                _mainCamera
                    .ScreenPointToRay(
                        screenPosition);

            var layerMask =
                1 << 14;

            return Physics.Raycast(
                ray,
                Mathf.Infinity,
                layerMask);
        }

        /// <summary>
        /// Moves the camera across the XY gameplay plane.
        /// Dragging the world moves the camera in the opposite direction.
        /// </summary>
        private void MoveCamera(
            Vector2 delta)
        {
            if (_mainCamera == null ||
                delta.sqrMagnitude <= 0f)
            {
                return;
            }

            _allowCameraFollow = false;

            var speed =
                _movementSpeed *
                _mainCamera.orthographicSize *
                Time.deltaTime;

            var currentPosition =
                _mainCamera
                    .transform
                    .position;

            var x =
                currentPosition.x -
                delta.x *
                speed;

            var y =
                currentPosition.y -
                delta.y *
                speed;

            SetCameraPosition(
                x,
                y);
        }

        /// <summary>
        /// Starts following the target Rigidbody.
        /// </summary>
        public void CameraFollowForRigidBody(
            Rigidbody characterToFollow)
        {
            if (characterToFollow == null)
            {
                StopCameraFollowForRigidBody();
                return;
            }

            KillCameraMoveSequence();

            _characterToFollow =
                characterToFollow;

            _allowCameraFollow = true;

            CenterCameraAgainst(
                _characterToFollow);

            EnforceCameraPosition();
        }

        /// <summary>
        /// Stops following a Rigidbody.
        /// </summary>
        public void StopCameraFollowForRigidBody()
        {
            _characterToFollow = null;
            _allowCameraFollow = false;
        }

        private void CenterCameraAgainst(
            Rigidbody target)
        {
            if (target == null ||
                _mainCamera == null)
            {
                return;
            }

            var targetPosition =
                target.transform.position;

            SetCameraPosition(
                targetPosition.x,
                targetPosition.y);
        }

        /// <summary>
        /// Applies post-launch zoom.
        /// </summary>
        public void ZoomOutCameraAfterLaunch()
        {
            if (_mainCamera == null)
                return;

            _mainCamera.orthographicSize =
                Mathf.Clamp(
                    _mainCamera.orthographicSize -
                    (_mainCamera.orthographicSize /
                     _zoomeCameraValueAfterLaunch),
                    _minZoom,
                    _maxZoom);

            EnforceCameraPosition();
        }

        /// <summary>
        /// Smoothly moves camera across the XY gameplay plane.
        /// Z remains immutable.
        /// </summary>
        public void MoveCameraTo(
            Vector3 targetPosition,
            float speed,
            bool resetZoom = false)
        {
            if (_mainCamera == null)
                return;

            StopCameraFollowForRigidBody();
            KillCameraMoveSequence();

            var duration =
                Mathf.Max(
                    0f,
                    speed);

            var targetX =
                Mathf.Clamp(
                    targetPosition.x,
                    _minCameraX,
                    _maxCameraX);

            var targetY =
                Mathf.Clamp(
                    targetPosition.y,
                    _minCameraY,
                    _maxCameraY);

            var cameraTarget =
                new Vector3(
                    targetX,
                    targetY,
                    _fixedCameraZ);

            _cameraMoveSequence =
                DOTween.Sequence();

            _cameraMoveSequence.Append(
                _mainCamera.transform
                    .DOMove(
                        cameraTarget,
                        duration)
                    .SetEase(
                        _cameraMoveEase));

            if (resetZoom)
            {
                _cameraMoveSequence.Join(
                    _mainCamera
                        .DOOrthoSize(
                            startOrthographicCameraSize,
                            duration)
                        .SetEase(
                            _cameraMoveEase));
            }

            _cameraMoveSequence.OnUpdate(
                EnforceCameraPosition);

            _cameraMoveSequence.OnComplete(
                () =>
                {
                    EnforceCameraPosition();
                    _cameraMoveSequence = null;
                });
        }

        /// <summary>
        /// Shakes the camera across the XY gameplay plane.
        /// </summary>
        public void ShakeCamera(
            float shakeDuration = -1f,
            float shakeMagnitude = -1f)
        {
            if (_mainCamera == null)
                return;

            shakeDuration =
                shakeDuration > 0f
                    ? shakeDuration
                    : _defaultShakeDuration;

            shakeMagnitude =
                shakeMagnitude >= 0f
                    ? shakeMagnitude
                    : _defaultShakeMagnitude;

            StopCameraShake();

            KillCameraMoveSequence();

            _cameraShakeRoutine =
                StartCoroutine(
                    ShakeCoroutine(
                        shakeDuration,
                        shakeMagnitude));
        }

        private IEnumerator ShakeCoroutine(
            float shakeDuration,
            float shakeMagnitude)
        {
            var elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                if (_mainCamera == null)
                    yield break;

                _cameraShakeOffset =
                    new Vector3(
                        Random.Range(
                            -1f,
                            1f) *
                        shakeMagnitude,
                        Random.Range(
                            -1f,
                            1f) *
                        shakeMagnitude,
                        0f);

                elapsed +=
                    Time.deltaTime;

                yield return null;
            }

            _cameraShakeOffset = Vector3.zero;
            _cameraShakeRoutine = null;
        }

        private void ApplyCameraShakeOffset()
        {
            if (_mainCamera == null ||
                _cameraShakeOffset.sqrMagnitude <= 0f)
            {
                return;
            }

            _mainCamera.transform.position +=
                _cameraShakeOffset;
        }

        private void StopCameraShake()
        {
            if (_cameraShakeRoutine != null)
            {
                StopCoroutine(
                    _cameraShakeRoutine);

                _cameraShakeRoutine = null;
            }

            _cameraShakeOffset =
                Vector3.zero;
        }

        /// <summary>
        /// Sets camera world X/Y while preserving immutable Z.
        /// </summary>
        private void SetCameraPosition(
            float x,
            float y)
        {
            if (_mainCamera == null)
                return;

            x =
                Mathf.Clamp(
                    x,
                    _minCameraX,
                    _maxCameraX);

            y =
                Mathf.Clamp(
                    y,
                    _minCameraY,
                    _maxCameraY);

            _mainCamera.transform.position =
                new Vector3(
                    x,
                    y,
                    _fixedCameraZ);
        }

        private void KillCameraMoveSequence()
        {
            if (_cameraMoveSequence == null)
                return;

            _cameraMoveSequence.Kill();

            _cameraMoveSequence = null;

            EnforceCameraPosition();
        }

        private void KillCameraTwins()
        {
            var allCameras =
                FindObjectsOfType<Camera>();

            Camera mainCamera = null;

            foreach (var camera in allCameras)
            {
                if (camera != null &&
                    camera.CompareTag(
                        "MainCamera"))
                {
                    mainCamera = camera;
                    break;
                }
            }

            foreach (var camera in allCameras)
            {
                if (camera != null &&
                    camera != mainCamera)
                {
                    Destroy(
                        camera.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (_settingsManager != null)
            {
                _settingsManager
                    .OnSettingsChanged -=
                    ApplyCameraSettingsFromManager;
            }

            StopCameraShake();
            KillCameraMoveSequence();

            KillCameraTwins();
        }

        /// <summary>
        /// Smoothly zooms camera after Ready.
        /// </summary>
        public void ZoomInAfterReady()
        {
            if (_mainCamera == null)
                return;

            KillCameraMoveSequence();

            _mainCamera.DOKill();

            _mainCamera
                .DOOrthoSize(
                    Mathf.Clamp(
                        _targetZoomAfterReady,
                        _minZoom,
                        _maxZoom),
                    Mathf.Max(
                        0f,
                        _zoomInAfterReadyDuration))
                .SetEase(
                    _cameraMoveEase)
                .OnUpdate(
                    EnforceCameraPosition);
        }
    }
}