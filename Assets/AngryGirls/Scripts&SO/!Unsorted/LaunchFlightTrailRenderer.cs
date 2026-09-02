using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Renders the actual path travelled by a launched character.
    /// The trail remains visible until the next launch replaces it.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class LaunchFlightTrailRenderer : MonoBehaviour
    {
        [Header("Renderer")]
        [SerializeField] private LineRenderer _lineRenderer;

        [Header("Trail")]
        [SerializeField] private float _width = 0.12f;
        [SerializeField] private int _maxPoints = 256;
        [SerializeField] private float _minPointDistance = 0.08f;
        [SerializeField] private float _cameraDepthOffset = -0.05f;

        [Header("Ability Marker")]
        [SerializeField] private GameObject _abilityMarkerPrefab;
        [SerializeField] private float _abilityMarkerDepthOffset = -0.08f;

        [Header("Camera Relative Size")]
        [SerializeField] private bool _scaleWithOrthographicCamera = true;
        [SerializeField] private float _referenceOrthographicSize = 7.5f;
        [SerializeField] private float _referenceWidthScale = 1f;

        private GameObject _abilityMarker;
        private Material _materialInstance;

        private Vector3 _lastPoint;
        private bool _hasLastPoint;
        private bool _isDrawing;

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
                    $"{nameof(LaunchFlightTrailRenderer)} requires a LineRenderer.",
                    this);

                enabled = false;
                return;
            }

            ConfigureRenderer();
            CreateMaterialInstance();
            Hide();
        }

        private void Update()
        {
            if (!_isDrawing)
                return;

            UpdateCameraRelativeWidth();
        }

        /// <summary>
        /// Starts a new trail and removes any previous trail and ability marker.
        /// </summary>
        public void BeginTrail(Vector3 startPosition)
        {
            ClearTrail();

            _isDrawing = true;
            _hasLastPoint = false;

            gameObject.SetActive(true);

            AddPoint(startPosition);
        }

        /// <summary>
        /// Adds an actual world-space Rigidbody position to the trail.
        /// </summary>
        public void AddPoint(Vector3 worldPosition)
        {
            if (!_isDrawing ||
                _lineRenderer == null)
            {
                return;
            }

            worldPosition.x += _cameraDepthOffset;

            if (_hasLastPoint &&
                Vector3.Distance(
                    _lastPoint,
                    worldPosition) < _minPointDistance)
            {
                return;
            }

            if (_lineRenderer.positionCount >= _maxPoints)
                RemoveOldestPoint();

            var pointIndex =
                _lineRenderer.positionCount;

            _lineRenderer.positionCount =
                pointIndex + 1;

            _lineRenderer.SetPosition(
                pointIndex,
                worldPosition);

            _lastPoint = worldPosition;
            _hasLastPoint = true;
        }

        /// <summary>
        /// Ends recording of the current trail and keeps it visible.
        /// </summary>
        public void EndTrail(
            Vector3 endPosition)
        {
            if (!_isDrawing)
                return;

            AddPoint(endPosition);

            _isDrawing = false;
        }

        /// <summary>
        /// Marks the ability usage position and keeps the completed trail visible.
        /// </summary>
        public void MarkAbilityUsed(
            Vector3 abilityPosition)
        {
            if (_isDrawing)
                AddPoint(abilityPosition);

            SpawnAbilityMarker(
                abilityPosition);

            _isDrawing = false;
        }

        /// <summary>
        /// Immediately clears the trail and its ability marker.
        /// </summary>
        public void ClearTrail()
        {
            _isDrawing = false;
            _hasLastPoint = false;

            if (_lineRenderer != null)
                _lineRenderer.positionCount = 0;

            DestroyAbilityMarker();

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hides and clears the current trail.
        /// </summary>
        public void Hide()
        {
            ClearTrail();
        }

        private void ConfigureRenderer()
        {
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.alignment = LineAlignment.View;
            _lineRenderer.loop = false;
            _lineRenderer.positionCount = 0;
            _lineRenderer.widthMultiplier = _width;
        }

        private void CreateMaterialInstance()
        {
            if (_lineRenderer.sharedMaterial == null)
                return;

            _materialInstance =
                new Material(
                    _lineRenderer.sharedMaterial);

            _lineRenderer.material =
                _materialInstance;
        }

        private void RemoveOldestPoint()
        {
            var count =
                _lineRenderer.positionCount;

            if (count <= 1)
            {
                _lineRenderer.positionCount = 0;
                _hasLastPoint = false;
                return;
            }

            var positions =
                new Vector3[count];

            _lineRenderer.GetPositions(
                positions);

            for (var i = 1; i < count; i++)
                positions[i - 1] = positions[i];

            _lineRenderer.positionCount =
                count - 1;

            for (var i = 0; i < count - 1; i++)
                _lineRenderer.SetPosition(
                    i,
                    positions[i]);
        }

        private void SpawnAbilityMarker(
            Vector3 position)
        {
            DestroyAbilityMarker();

            if (_abilityMarkerPrefab == null)
                return;

            position.x +=
                _abilityMarkerDepthOffset;

            _abilityMarker =
                Instantiate(
                    _abilityMarkerPrefab,
                    position,
                    Quaternion.identity);

            _abilityMarker.name =
                "LaunchAbilityMarker";
        }

        private void DestroyAbilityMarker()
        {
            if (_abilityMarker == null)
                return;

            Destroy(
                _abilityMarker);

            _abilityMarker = null;
        }

        private void UpdateCameraRelativeWidth()
        {
            if (!_scaleWithOrthographicCamera ||
                _lineRenderer == null)
            {
                return;
            }

            var camera =
                Camera.main;

            if (camera == null ||
                !camera.orthographic)
            {
                _lineRenderer.widthMultiplier =
                    _width *
                    _referenceWidthScale;

                return;
            }

            var scale =
                camera.orthographicSize /
                Mathf.Max(
                    0.01f,
                    _referenceOrthographicSize);

            _lineRenderer.widthMultiplier =
                _width *
                _referenceWidthScale *
                scale;
        }

        private void OnDestroy()
        {
            DestroyAbilityMarker();

            if (_materialInstance != null)
                Destroy(_materialInstance);
        }
    }
}