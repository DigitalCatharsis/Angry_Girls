using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private readonly Vector3 _cameraStartPosition = new Vector3(13.25f, 1.44f, -0.00999999f);
        [SerializeField] private const float startOrthographicCameraSize = 3f;
        [SerializeField] private const float _secondsCameraWaitsAfterAttack = 2f;
        [SerializeField] private const float _zoomeCameraValueAfterLaunch = 5f;

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSensitivity = 7.0f;
        [SerializeField] private float _minZoom = 1f;
        [SerializeField] private float _maxZoom = 10f;

        [Header("Camera Movement Settings")]
        [SerializeField] private float _movementSpeed = 0.5f;
        [SerializeField] private float _minCameraZ = -10f;
        [SerializeField] private float _maxCameraZ = 30f;

        public float SecondsCameraWaitsAfterAttack => _secondsCameraWaitsAfterAttack;

        [SerializeField] private Rigidbody _characterToFollow;
        [SerializeField] private bool _allowCameraFollow = false;

        [Header("Debug")]
        [SerializeField] private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_allowCameraFollow)
            {
                HandleZoom();
                HandleMovement();
            }
        }

        private void LateUpdate()
        {
            if (_characterToFollow != null && _allowCameraFollow)
            {
                CenterCameraAgainst(_characterToFollow);
            }
        }

        private void HandleZoom()
        {
            float zoomDelta = GameLoader.Instance.inputManager.GetZoomDelta();
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
            if (GameLoader.Instance.inputManager.IsDragging())
            {
                Vector2 delta = GameLoader.Instance.inputManager.GetDragDelta();
                if (!IsPointerOverCharacter(GameLoader.Instance.inputManager.Position))
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

            float speed = _movementSpeed * _mainCamera.orthographicSize * Time.deltaTime;
            Vector3 newPosition = _mainCamera.transform.position + new Vector3(0, 0, -delta.x * speed);
            newPosition.z = Mathf.Clamp(newPosition.z, _minCameraZ, _maxCameraZ);
            _mainCamera.transform.position = newPosition;
        }

        public void CameraFollowForRigidBody(Rigidbody characterToFollow)
        {
            _characterToFollow = characterToFollow;
            _allowCameraFollow = true;
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
            GameLoader.Instance.myExtentions.Lerp_Position(_mainCamera.gameObject, _mainCamera.transform.position, targetPosition, speed);
            if (resetZoom)
            {
                GameLoader.Instance.myExtentions.Lerp_OrthographicCamera_Size(_mainCamera, _mainCamera.orthographicSize, startOrthographicCameraSize, speed);
            }
        }

        public void ReturnCameraToStartPosition(float speed)
        {
            MoveCameraTo(_cameraStartPosition, speed, true);
        }

        public Vector3 GetPointerWorldPosition()
        {
            Vector3 screenPosition = GameLoader.Instance.inputManager.Position;
            screenPosition.z = _mainCamera.nearClipPlane + 1;
            return _mainCamera.ScreenToWorldPoint(screenPosition);
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
    }
}