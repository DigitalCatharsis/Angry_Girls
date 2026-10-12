

using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Displays an animated sprite indicator centered on the currently hovered character.
    /// </summary>
    public sealed class TurnOrderHoverIndicator : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.15f, 0f);

        [Header("Size")]
        [SerializeField] private float _size = 1f;

        [Header("Animation")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _framesPerSecond = 8f;

        private CControl _target;
        private Collider _targetCollider;
        private Camera _mainCamera;

        private int _currentFrame;
        private float _animationTimer;

        /// <summary>
        /// Gets the currently highlighted character.
        /// </summary>
        public CControl Target => _target;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer == null)
            {
                Debug.LogError("TurnOrderHoverIndicator requires a SpriteRenderer.", this);
                enabled = false;
                return;
            }

            _mainCamera = Camera.main;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_target == null || _targetCollider == null)
            {
                Hide();
                return;
            }

            UpdatePosition();
            UpdateRotation();
            UpdateAnimation();
        }

        /// <summary>
        /// Shows the indicator on the specified character.
        /// </summary>
        public void Show(CControl character)
        {
            if (character == null)
            {
                Hide();
                return;
            }

            var collider = character.GetComponentInChildren<Collider>();

            if (collider == null)
            {
                Hide();
                return;
            }

            if (_target != character)
            {
                _target = character;
                _targetCollider = collider;
                ResetAnimation();
            }

            gameObject.SetActive(true);

            UpdatePosition();
            UpdateRotation();
            UpdateScale();
        }

        /// <summary>
        /// Hides the indicator.
        /// </summary>
        public void Hide()
        {
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

            transform.position = _targetCollider.bounds.center + _offset;
        }

        private void UpdateRotation()
        {
            if (_spriteRenderer == null)
                return;

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            // The gameplay plane is XY. Keep the sprite upright in that plane
            // and facing the current camera instead of retaining a legacy YZ rotation.
            Vector3 facing = _mainCamera != null ? -_mainCamera.transform.forward : Vector3.forward;
            Vector3 up = _mainCamera != null ? _mainCamera.transform.up : Vector3.up;

            if (facing.sqrMagnitude <= 0.0001f)
                facing = Vector3.forward;

            if (Vector3.Cross(up, facing).sqrMagnitude <= 0.0001f)
                up = Vector3.up;

            _spriteRenderer.transform.rotation = Quaternion.LookRotation(facing.normalized, up.normalized);
        }

        private void UpdateScale()
        {
            var clampedSize = Mathf.Max(0.01f, _size);
            transform.localScale = Vector3.one * clampedSize;
        }

        private void UpdateAnimation()
        {
            if (_frames == null || _frames.Length == 0 || _spriteRenderer == null)
                return;

            _spriteRenderer.enabled = true;

            if (_frames.Length == 1)
            {
                _spriteRenderer.sprite = _frames[0];
                return;
            }

            _animationTimer += Time.unscaledDeltaTime;

            var frameDuration = 1f / Mathf.Max(0.01f, _framesPerSecond);

            while (_animationTimer >= frameDuration)
            {
                _animationTimer -= frameDuration;
                _currentFrame = (_currentFrame + 1) % _frames.Length;
            }

            _spriteRenderer.sprite = _frames[_currentFrame];
        }

        private void ResetAnimation()
        {
            _currentFrame = 0;
            _animationTimer = 0f;

            if (_spriteRenderer != null && _frames != null && _frames.Length > 0)
            {
                _spriteRenderer.sprite = _frames[0];
                _spriteRenderer.enabled = true;
            }
        }
    }
}

