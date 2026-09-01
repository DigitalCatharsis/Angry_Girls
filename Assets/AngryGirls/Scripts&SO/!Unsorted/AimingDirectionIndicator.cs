using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Displays an animated launch direction arrow near the aimed character.
    /// The position is anchored to the bottom of the character collider,
    /// while the X offset moves the indicator closer to the camera.
    /// </summary>
    public sealed class AimingDirectionIndicator : MonoBehaviour
    {
        [Header("Target Position")]
        [SerializeField] private float _heightOffset = 0.9f;
        [SerializeField] private float _cameraOffset = -0.5f;

        [Header("Camera Relative Size")]
        [SerializeField] private float _referenceOrthographicSize = 7.5f;
        [SerializeField] private float _referenceScale = 1f;

        [Header("Animation")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _framesPerSecond = 8f;

        private Camera _camera;
        private Transform _target;
        private Collider _targetCollider;

        private Vector3 _launchDirection;

        private int _currentFrame;
        private float _animationTimer;

        private bool _isVisible;

        /// <summary>
        /// Initializes the indicator.
        /// </summary>
        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer == null)
            {
                Debug.LogError(
                    "AimingDirectionIndicator requires a SpriteRenderer.",
                    this);

                enabled = false;
                return;
            }

            _camera = Camera.main;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isVisible || _target == null)
                return;

            UpdatePosition();
            UpdateRotation();
            UpdateScale();
            UpdateAnimation();
        }

        /// <summary>
        /// Shows the indicator for the specified character.
        /// </summary>
        public void Show(
            Transform target,
            Vector3 launchDirection)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            launchDirection.x = 0f;

            if (launchDirection.sqrMagnitude <= 0.0001f)
            {
                Hide();
                return;
            }

            var collider =
                target.GetComponentInChildren<Collider>();

            if (collider == null)
            {
                Hide();
                return;
            }

            _target = target;
            _targetCollider = collider;
            _launchDirection = launchDirection.normalized;

            if (!_isVisible)
                ResetAnimation();

            _isVisible = true;
            gameObject.SetActive(true);

            UpdatePosition();
            UpdateRotation();
            UpdateScale();
        }

        /// <summary>
        /// Updates the launch direction.
        /// </summary>
        public void SetDirection(
            Vector3 launchDirection)
        {
            launchDirection.x = 0f;

            if (launchDirection.sqrMagnitude <= 0.0001f)
                return;

            _launchDirection =
                launchDirection.normalized;
        }

        /// <summary>
        /// Hides the indicator.
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _target = null;
            _targetCollider = null;
            _animationTimer = 0f;

            if (_spriteRenderer != null)
                _spriteRenderer.enabled = false;

            gameObject.SetActive(false);
        }

        private void UpdatePosition()
        {
            if (_targetCollider == null)
                return;

            var bounds =
                _targetCollider.bounds;

            var position =
                bounds.center;

            position.x =
                bounds.center.x + _cameraOffset;

            position.y =
                bounds.min.y + _heightOffset;

            position.z =
                bounds.center.z;

            transform.position =
                position;
        }

        private void UpdateRotation()
        {
            var angle =
                Mathf.Atan2(
                    _launchDirection.z,
                    _launchDirection.y) *
                Mathf.Rad2Deg;

            transform.rotation =
                Quaternion.AngleAxis(
                    angle,
                    Vector3.right) *
                Quaternion.Euler(
                    0f,
                    90f,
                    0f);
        }

        private void UpdateScale()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null ||
                !_camera.orthographic)
            {
                transform.localScale =
                    Vector3.one *
                    _referenceScale;

                return;
            }

            var scaleFactor =
                _camera.orthographicSize /
                Mathf.Max(
                    0.01f,
                    _referenceOrthographicSize);

            transform.localScale =
                Vector3.one *
                (_referenceScale *
                 scaleFactor);
        }

        private void UpdateAnimation()
        {
            if (_spriteRenderer == null ||
                _frames == null ||
                _frames.Length == 0)
            {
                return;
            }

            _spriteRenderer.enabled = true;

            if (_frames.Length == 1)
            {
                _spriteRenderer.sprite =
                    _frames[0];

                return;
            }

            _animationTimer +=
                Time.unscaledDeltaTime;

            var frameDuration =
                1f /
                Mathf.Max(
                    0.01f,
                    _framesPerSecond);

            while (_animationTimer >= frameDuration)
            {
                _animationTimer -= frameDuration;
                _currentFrame =
                    (_currentFrame + 1) %
                    _frames.Length;
            }

            _spriteRenderer.sprite =
                _frames[_currentFrame];
        }

        private void ResetAnimation()
        {
            _currentFrame = 0;
            _animationTimer = 0f;

            if (_spriteRenderer == null ||
                _frames == null ||
                _frames.Length == 0)
            {
                return;
            }

            _spriteRenderer.sprite =
                _frames[0];

            _spriteRenderer.enabled = true;
        }
    }
}