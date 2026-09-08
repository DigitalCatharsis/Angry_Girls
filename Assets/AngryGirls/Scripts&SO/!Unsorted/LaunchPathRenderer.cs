using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Renders the launch trajectory as a fixed-length 2D world-space curve.
    /// The trajectory is constrained to the gameplay Y/Z plane.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class LaunchPathRenderer : MonoBehaviour
    {
        [Header("Line Renderer")]
        [Tooltip("LineRenderer used to draw the trajectory.")]
        [SerializeField] private LineRenderer _lineRenderer;

        [Tooltip("Base width of the trajectory line.")]
        [SerializeField] private float _width = 0.12f;

        [Tooltip("Additional multiplier applied to the line width.")]
        [SerializeField] private float _widthMultiplier = 1f;

        [Header("Displayed Curve")]
        [Tooltip("Number of texture repetitions visible along the trajectory.")]
        [Min(0.1f)]
        [SerializeField] private float _displayedLengthInTiles = 4f;

        [Tooltip("World-space distance represented by one texture tile.")]
        [Min(0.01f)]
        [SerializeField] private float _tileLengthInWorldUnits = 1f;

        [Tooltip("Maximum physics time used to find the requested visual trajectory length.")]
        [Min(0.1f)]
        [SerializeField] private float _maxTrajectorySearchTime = 10f;

        [Header("Cheat")]
        [Tooltip("Multiplier applied to the displayed trajectory length while trajectory cheat is enabled.")]
        [Min(1f)]
        [SerializeField] private float _cheatLengthMultiplier = 4f;

        [Header("Camera Relative Size")]
        [Tooltip("Orthographic camera size used as the reference for the line width.")]
        [Min(0.01f)]
        [SerializeField] private float _referenceOrthographicSize = 7.5f;

        [Tooltip("Additional multiplier for camera-relative line width.")]
        [Min(0f)]
        [SerializeField] private float _referenceWidthScale = 1f;

        [Header("Texture Animation")]
        [Tooltip("Enables animated texture movement along the trajectory.")]
        [SerializeField] private bool _animateTexture = true;

        [Tooltip("Texture movement speed in UV space. Negative values move the effect from the character.")]
        [SerializeField] private float _textureScrollSpeed = -3.5f;

        [Header("Depth")]
        [Tooltip("World X offset used to place the trajectory relative to the camera.")]
        [SerializeField] private float _cameraDepthOffset = -0.05f;

        private Material _lineMaterial;
        private Vector2 _textureOffset;

        private Vector3 _startPosition;
        private Vector3 _launchVelocity;
        private Vector3 _gravity;

        private float _duration;
        private float _displayedLength;
        private bool _isVisible;

        /// <summary>
        /// Initializes the trajectory renderer.
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

            _displayedLengthInTiles =
                Mathf.Max(
                    0.1f,
                    _displayedLengthInTiles);

            _tileLengthInWorldUnits =
                Mathf.Max(
                    0.01f,
                    _tileLengthInWorldUnits);

            _maxTrajectorySearchTime =
                Mathf.Max(
                    0.1f,
                    _maxTrajectorySearchTime);

            _cheatLengthMultiplier =
                Mathf.Max(
                    1f,
                    _cheatLengthMultiplier);

            transform.localPosition =
                Vector3.zero;

            transform.localRotation =
                Quaternion.identity;

            transform.localScale =
                Vector3.one;

            ConfigureLineRenderer();

            _lineMaterial =
                _lineRenderer.material;

            ResetTextureAnimation();

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
        /// Draws or updates the trajectory.
        /// The visual length is independent from launch force.
        /// </summary>
        public void Draw(
            Vector3 startPosition,
            Vector3 velocity,
            Vector3 gravity,
            float duration)
        {
            if (_lineRenderer == null)
                return;

            var wasVisible =
                _isVisible;

            _startPosition =
                startPosition;

            _startPosition.x +=
                _cameraDepthOffset;

            _launchVelocity =
                velocity;

            _launchVelocity.x = 0f;

            _gravity =
                gravity;

            _gravity.x = 0f;

            var visualLengthMultiplier =
                duration > 5f
                    ? _cheatLengthMultiplier
                    : 1f;

            _displayedLength =
                _displayedLengthInTiles *
                _tileLengthInWorldUnits *
                visualLengthMultiplier;

            _duration =
                FindTimeForDistance(
                    _displayedLength);

            _isVisible =
                true;

            if (!wasVisible)
                ResetTextureAnimation();

            gameObject.SetActive(
                true);

            RebuildTrajectory();
            UpdateCameraRelativeWidth();
        }

        /// <summary>
        /// Hides the trajectory.
        /// </summary>
        public void Hide()
        {
            _isVisible =
                false;

            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount =
                    0;
            }

            ResetTextureAnimation();

            gameObject.SetActive(
                false);
        }

        private void ConfigureLineRenderer()
        {
            _lineRenderer.useWorldSpace =
                true;

            _lineRenderer.alignment =
                LineAlignment.View;

            _lineRenderer.textureMode =
                LineTextureMode.Tile;

            _lineRenderer.positionCount =
                0;
        }

        private float FindTimeForDistance(
            float targetDistance)
        {
            if (targetDistance <= 0.001f)
                return 0.01f;

            var availableDistance =
                CalculateArcLength(
                    _maxTrajectorySearchTime);

            if (availableDistance <= targetDistance)
                return _maxTrajectorySearchTime;

            var lowerTime = 0f;
            var upperTime =
                _maxTrajectorySearchTime;

            for (var i = 0; i < 12; i++)
            {
                var middleTime =
                    (lowerTime + upperTime) *
                    0.5f;

                var distance =
                    CalculateArcLength(
                        middleTime);

                if (distance < targetDistance)
                    lowerTime = middleTime;
                else
                    upperTime = middleTime;
            }

            return upperTime;
        }

        private float CalculateArcLength(
            float duration)
        {
            const int sampleCount = 64;

            var totalDistance = 0f;

            var previousPosition =
                CalculatePosition(
                    0f);

            for (var i = 1;
                 i <= sampleCount;
                 i++)
            {
                var time =
                    duration *
                    i /
                    sampleCount;

                var currentPosition =
                    CalculatePosition(
                        time);

                totalDistance +=
                    Vector3.Distance(
                        previousPosition,
                        currentPosition);

                previousPosition =
                    currentPosition;
            }

            return totalDistance;
        }

        private void RebuildTrajectory()
        {
            var pointCount =
                64;

            _lineRenderer.positionCount =
                pointCount;

            for (var i = 0;
                 i < pointCount;
                 i++)
            {
                var normalizedTime =
                    i /
                    (float)(pointCount - 1);

                var time =
                    normalizedTime *
                    _duration;

                var position =
                    CalculatePosition(
                        time);

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
                _launchVelocity *
                time +
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

        private void ResetTextureAnimation()
        {
            _textureOffset =
                Vector2.zero;

            ApplyTextureOffset();
        }

        private void UpdateTextureAnimation()
        {
            if (!_animateTexture)
                return;

            _textureOffset.x +=
                _textureScrollSpeed *
                Time.unscaledDeltaTime;

            _textureOffset.x =
                Mathf.Repeat(
                    _textureOffset.x,
                    1f);

            ApplyTextureOffset();
        }

        private void ApplyTextureOffset()
        {
            if (_lineMaterial == null)
                return;

            if (_lineMaterial.HasProperty("_BaseMap"))
            {
                _lineMaterial.SetTextureOffset(
                    "_BaseMap",
                    _textureOffset);

                return;
            }

            if (_lineMaterial.HasProperty("_MainTex"))
            {
                _lineMaterial.SetTextureOffset(
                    "_MainTex",
                    _textureOffset);
            }
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
                Destroy(
                    _lineMaterial);
        }
    }
}