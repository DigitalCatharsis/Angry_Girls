using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Renders the launch trajectory as a 2D world-space curve.
    /// The trajectory exists strictly in the Y/Z gameplay plane.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class LaunchPathRenderer : MonoBehaviour
    {
        [Header("Line")]
        [SerializeField] private LineRenderer _lineRenderer;

        [SerializeField] private float _width = 0.12f;
        [SerializeField] private float _widthMultiplier = 1f;

        [Header("Curve")]
        [SerializeField] private int _points = 64;
        [SerializeField] private float _trajectoryDuration = 1f;

        [Header("Camera Relative Size")]
        [SerializeField] private float _referenceOrthographicSize = 7.5f;
        [SerializeField] private float _referenceWidthScale = 1f;

        [Header("Animation")]
        [SerializeField] private bool _animateTexture = true;
        [SerializeField] private float _textureScrollSpeed = 1.5f;

        [Header("Depth")]
        [SerializeField] private float _cameraDepthOffset = -0.05f;

        private static readonly int MainTextureId = Shader.PropertyToID("_MainTex");

        private Vector2 _textureOffset;

        private Material _materialInstance;

        private Vector3 _startPosition;
        private Vector3 _launchVelocity;
        private Vector3 _gravity;

        private float _duration;
        private bool _isVisible;

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        private void Awake()
        {
            if (_lineRenderer == null)
                _lineRenderer = GetComponent<LineRenderer>();

            if (_lineRenderer == null)
            {
                Debug.LogError(
                    $"{nameof(LaunchPathRenderer)} requires a LineRenderer.",
                    this);

                enabled = false;
                return;
            }

            _points = Mathf.Max(2, _points);

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.alignment = LineAlignment.View;
            _lineRenderer.positionCount = 0;

            CreateMaterialInstance();

            Hide();
        }

        private void Update()
        {
            if (!_isVisible)
                return;

            UpdateCameraRelativeWidth();
            UpdateTextureAnimation();
        }

        /// <summary>
        /// Draws the trajectory using the supplied launch physics.
        /// </summary>
        public void Draw(
            Vector3 startPosition,
            Vector3 velocity,
            Vector3 gravity,
            float duration)
        {
            if (_lineRenderer == null)
                return;

            _startPosition = startPosition;
            _startPosition.x += _cameraDepthOffset;

            _launchVelocity = velocity;
            _launchVelocity.x = 0f;

            _gravity = gravity;
            _gravity.x = 0f;

            _duration = Mathf.Max(
                0.01f,
                duration);

            _isVisible = true;

            gameObject.SetActive(true);

            RebuildTrajectory();
            UpdateCameraRelativeWidth();
        }

        /// <summary>
        /// Hides the trajectory.
        /// </summary>
        public void Hide()
        {
            _isVisible = false;

            if (_lineRenderer != null)
                _lineRenderer.positionCount = 0;

            gameObject.SetActive(false);
        }

        private void RebuildTrajectory()
        {
            if (_lineRenderer == null)
                return;

            var pointCount =
                Mathf.Max(
                    2,
                    _points);

            _lineRenderer.positionCount =
                pointCount;

            for (var i = 0; i < pointCount; i++)
            {
                var normalizedTime =
                    i /
                    (float)(pointCount - 1);

                var time =
                    normalizedTime *
                    _duration;

                var position =
                    CalculatePosition(time);

                position.x =
                    _startPosition.x;

                _lineRenderer.SetPosition(
                    i,
                    position);
            }
        }

        private Vector3 CalculatePosition(
            float time)
        {
            var position =
                _startPosition +
                _launchVelocity * time +
                0.5f *
                _gravity *
                time *
                time;

            position.x =
                _startPosition.x;

            return position;
        }

        private void UpdateCameraRelativeWidth()
        {
            if (_lineRenderer == null)
                return;

            var camera =
                Camera.main;

            if (camera == null ||
                !camera.orthographic)
            {
                _lineRenderer.widthMultiplier =
                    _width *
                    _widthMultiplier;

                return;
            }

            var scale =
                camera.orthographicSize /
                Mathf.Max(
                    0.01f,
                    _referenceOrthographicSize);

            _lineRenderer.widthMultiplier =
                _width *
                _widthMultiplier *
                scale *
                _referenceWidthScale;
        }

        private void CreateMaterialInstance()
        {
            if (_lineRenderer == null ||
                _lineRenderer.sharedMaterial == null)
            {
                return;
            }

            _materialInstance =
                new Material(
                    _lineRenderer.sharedMaterial);

            _lineRenderer.material =
                _materialInstance;
        }

        private void UpdateTextureAnimation()
        {
            if (!_animateTexture || _materialInstance == null)
                return;

            if (!_materialInstance.HasProperty(MainTextureId))
                return;

            _textureOffset.x +=
                _textureScrollSpeed *
                Time.unscaledDeltaTime;

            _textureOffset.x =
                Mathf.Repeat(_textureOffset.x, 1f);

            _materialInstance.SetTextureOffset(
                MainTextureId,
                _textureOffset);
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
                Destroy(_materialInstance);
        }
    }
}