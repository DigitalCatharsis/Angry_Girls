using UnityEngine;

namespace Angry_Girls
{
    public class CharacterLauncher : MonoBehaviour
    {
        public bool _isAvaibleForLaunch = false;

        private Vector3 _startPos;
        private Vector3 _endPos;

        private Rigidbody _rigidbody;
        [SerializeField] private float _forceFactor;

        [SerializeField] private GameObject _trajectoryDotPrefab;
        private GameObject[] _trajectoryDots;
        [SerializeField] private int _dotsNumber;

        private Vector2 _zoomRange = new Vector2(5f, 10f); // Диапазон зума
        [SerializeField] private float _zoomSpeed;

        private Vector3 _directionVector;

        private bool _isZooming;
        [SerializeField] private float _minDistanceForZoom;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _trajectoryDots = new GameObject[_dotsNumber];
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnUnitClick();
            }

            if (Input.GetMouseButton(0))
            {                
                OnUnitDrag();
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isZooming = false;
                LaunchUnit();
                DestroyTrajectoryDots();
                //NullifyValues();
            }
        }

        private void OnUnitClick()
        {
            _startPos = new Vector3(0, gameObject.transform.position.y, gameObject.transform.position.z);
            for (int i = 0; i < _dotsNumber; i++)
            {
                _trajectoryDots[i] = Instantiate(_trajectoryDotPrefab, gameObject.transform);
            }
        }

        private void OnUnitDrag()
        {
            _isZooming = true;
            var pointPosition = GetPointerWorldPosition(Camera.main);
            _endPos = new Vector3(0, pointPosition.y, pointPosition.z);
            gameObject.transform.position = _endPos;
            _directionVector = _endPos - _startPos;

            for (var i = 0; i < _dotsNumber; ++i)
            {
                _trajectoryDots[i].transform.position = CalculateTraectoryPosition(i * 0.1f);
            }
            ZoomCamera();
        }

        private void LaunchUnit()
        {
            _rigidbody.useGravity = true;
            _rigidbody.velocity = new Vector3(0, -_directionVector.y * _forceFactor, -_directionVector.z * _forceFactor);
        }

        private void DestroyTrajectoryDots()
        {
            for (int i = 0; i < _trajectoryDots.Length; i++)
            {
                Destroy(_trajectoryDots[i]);
            }
        }

        private Vector3 CalculateTraectoryPosition(float elapsedTime)
        {
            return new Vector3(0, _endPos.y, _endPos.z)
                    + new Vector3(0, -_directionVector.y * _forceFactor, -_directionVector.z * _forceFactor) * elapsedTime
                    + 0.5f * Physics.gravity * elapsedTime * elapsedTime;
        }

        private Vector3 GetPointerWorldPosition(Camera camera)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = camera.nearClipPlane + 1;
            return camera.ScreenToWorldPoint(screenPosition);
        }

        private void ZoomCamera()
        {
            if (_isZooming && Vector3.Distance(_endPos, _startPos) > _minDistanceForZoom)
            {
                // Рассчитываем множитель для зума от 0 до 1 в зависимости от оттягивания
                var zoomMultiplier = Mathf.Clamp01(Vector3.Distance(_endPos, _startPos));

                // Изменяем зум камеры в зависимости от множителя и скорости зума
                float newZoom = Mathf.Lerp(_zoomRange.x, _zoomRange.y, zoomMultiplier);
                Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, newZoom, _zoomSpeed * Time.deltaTime);
            }            
        }
    }
}