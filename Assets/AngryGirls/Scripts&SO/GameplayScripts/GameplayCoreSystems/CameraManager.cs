using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    public class CameraManager : GameplayManagerClass
    {
        [Header("Setup")]
        [SerializeField] private const float startOrthographicCameraSize = 3f;

        [Header("Platform Defaults (Override via SettingsManager)")]
        [SerializeField] private float _defaultMovementSpeed = 0.5f;
        [SerializeField] private float _defaultZoomSensitivity = 7.0f;

        [SerializeField] private const float _secondsCameraWaitsAfterAttack = 2f;
        [SerializeField] private const float _zoomeCameraValueAfterLaunch = 5f;

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSensitivity;
        [SerializeField] private float _minZoom = 1f;
        [SerializeField] private float _maxZoom = 10f;

        [Header("Camera Movement Settings")]
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _minCameraZ = -10f;
        [SerializeField] private float _maxCameraZ = 30f;
        [SerializeField] private float _cameraMoveDuration = 0.5f;
        [SerializeField] private Ease _cameraMoveEase = Ease.InOutCubic;

        public float SecondsCameraWaitsAfterAttack => _secondsCameraWaitsAfterAttack;

        [SerializeField] private Rigidbody _characterToFollow;
        [SerializeField] private bool _allowCameraFollow = false;

        [Header("Debug")]
        [SerializeField] private Camera _mainCamera;

        private InputManager _inputManager;
        private Sequence _cameraMoveSequence;
        private SettingsManager _settingsManager;

        public override void Initialize()
        {
            KillCameraTwins();
            _mainCamera = Camera.main;
            _inputManager = GameplayCoreManager.Instance.InputManager;
            _settingsManager = CoreManager.Instance.SettingsManager;

            ApplyCameraSettingsFromManager(SettingsCategory.Camera);
            SubscribeToSettingsChanges();

            isInitialized = true;
        }

        /// <summary>
        /// Apply camera settings from SettingsManager
        /// </summary>
        private void ApplyCameraSettingsFromManager(SettingsCategory settingsCategory)
        {

            if (settingsCategory == SettingsCategory.Camera || settingsCategory == SettingsCategory.All)
            {
                var settings = _settingsManager.GetCurrentSettings();
                _movementSpeed = settings.cameraMovementSpeed;
            }
        }

        /// <summary>
        /// Subscribe to settings changes for runtime updates
        /// </summary>
        private void SubscribeToSettingsChanges()
        {
            _settingsManager.OnSettingsChanged += ApplyCameraSettingsFromManager;
        }

        private void Update()
        {
            if (!isInitialized) { return; }

            if (!_allowCameraFollow)
            {
                HandleZoom();
                HandleMovement();
            }
        }

        private void LateUpdate()
        {
            if (!isInitialized) { return; }

            if (_characterToFollow != null && _allowCameraFollow)
            {
                CenterCameraAgainst(_characterToFollow);
            }
        }

        private void HandleZoom()
        {
            var zoomDelta = _inputManager.GetZoomDelta();
            if (zoomDelta != 0)
            {
                ApplyZoom(zoomDelta * _zoomSensitivity);
            }
        }

        private void ApplyZoom(float delta)
        {
            if (delta != 0)
            {
                _allowCameraFollow = false;
            }
            _mainCamera.orthographicSize = Mathf.Clamp(
                _mainCamera.orthographicSize - delta,
                _minZoom,
                _maxZoom
            );
        }

        private void HandleMovement()
        {
            if (_inputManager.IsDragging())
            {
                var delta = _inputManager.GetDragDelta();
                if (!IsPointerOverCharacter(_inputManager.Position))
                {
                    MoveCamera(delta);
                }
            }
        }

        private bool IsPointerOverCharacter(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            int layerMask = 1 << 14; // Layer "CharacterToLaunch"
            return Physics.Raycast(ray, Mathf.Infinity, layerMask);
        }

        private void MoveCamera(Vector2 delta)
        {
            if (delta.magnitude > 0)
            {
                _allowCameraFollow = false;
            }

            var speed = _movementSpeed * _mainCamera.orthographicSize * Time.deltaTime;
            var newPosition = _mainCamera.transform.position + new Vector3(0, 0, -delta.x * speed);
            newPosition.z = Mathf.Clamp(newPosition.z, _minCameraZ, _maxCameraZ);
            _mainCamera.transform.position = newPosition;
        }

        public void CameraFollowForRigidBody(Rigidbody characterToFollow)
        {
            _characterToFollow = characterToFollow;
            _allowCameraFollow = true;

            KillCameraTwins();
        }

        public void StopCameraFollowForRigidBody()
        {
            _characterToFollow = null;
            _allowCameraFollow = false;
        }

        private void CenterCameraAgainst(Rigidbody target)
        {
            Vector3 targetPosition = new Vector3(
                _mainCamera.transform.position.x,
                target.transform.position.y,
                target.transform.position.z
            );
            _mainCamera.transform.position = targetPosition;
        }

        public void ZoomOutCameraAfterLaunch()
        {
            _mainCamera.orthographicSize -= (_mainCamera.orthographicSize / _zoomeCameraValueAfterLaunch);
        }

        public void MoveCameraTo(Vector3 targetPosition, float speed, bool resetZoom = false)
        {
            StopCameraFollowForRigidBody();
            KillCameraTwins();

            _cameraMoveSequence = DOTween.Sequence();

            _cameraMoveSequence.Append(
                _mainCamera.transform.DOMove(targetPosition, speed)
                    .SetEase(_cameraMoveEase)
            );

            if (resetZoom)
            {
                _cameraMoveSequence.Join(
                    _mainCamera.DOOrthoSize(startOrthographicCameraSize, speed)
                        .SetEase(_cameraMoveEase)
                );
            }

            _cameraMoveSequence.OnComplete(() => _cameraMoveSequence = null);
        }

        public void MoveCameraTo(Vector3 targetPosition, bool resetZoom = false)
        {
            MoveCameraTo(targetPosition, _cameraMoveDuration, resetZoom);
        }

        public void ShakeCamera(float shakeDuration = 0.3f, float shakeMagnitude = 0.05f)
        {
            StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude, _mainCamera.transform.localPosition));
        }

        private IEnumerator ShakeCoroutine(float shakeDuration, float shakeMagnitude, Vector3 originalPosition)
        {
            _allowCameraFollow = false;
            float elapsed = 0.0f;
            var savedY = _mainCamera.transform.localPosition.y;

            while (elapsed < shakeDuration)
            {
                float z = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;

                _mainCamera.transform.localPosition = originalPosition + new Vector3(0, y, z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _mainCamera.transform.localPosition = new Vector3(_mainCamera.transform.localPosition.x, savedY, _mainCamera.transform.localPosition.z);
            _allowCameraFollow = true;
        }
        private void KillCameraTwins()
        {
            // Get all cameras in scene
            var allCameras = FindObjectsOfType<Camera>();

            // Find main camera by tag (more reliable than Camera.main)
            Camera mainCamera = null;
            foreach (var cam in allCameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    mainCamera = cam;
                    break;
                }
            }

            // Destroy all non-main cameras
            foreach (var cam in allCameras)
            {
                if (cam != null && cam != mainCamera)
                {
                    Destroy(cam.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (_settingsManager != null)
            {
                _settingsManager.OnSettingsChanged -= ApplyCameraSettingsFromManager;
            }

            KillCameraTwins();
        }

        #region extraMethod for future

        //public void SmoothZoom(float targetSize, float duration = 0.5f)
        //{
        //    KillCameraTwins();

        //    _mainCamera.DOOrthoSize(
        //        Mathf.Clamp(targetSize, _minZoom, _maxZoom),
        //        duration
        //    ).SetEase(_cameraMoveEase);
        //}

        //public void SmoothMoveAndZoom(Vector3 targetPosition, float targetSize, float duration = 0.5f)
        //{
        //    KillCameraTwins();

        //    _cameraMoveSequence = DOTween.Sequence();
        //    _cameraMoveSequence.Append(_mainCamera.transform.DOMove(targetPosition, duration));
        //    _cameraMoveSequence.Join(_mainCamera.DOOrthoSize(targetSize, duration));
        //    _cameraMoveSequence.SetEase(_cameraMoveEase);
        //    _cameraMoveSequence.OnComplete(() => _cameraMoveSequence = null);
        //}

        #endregion

    }
}