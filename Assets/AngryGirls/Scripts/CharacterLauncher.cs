using UnityEngine;

namespace Angry_Girls
{
    public class CharacterLauncher : MonoBehaviour
    {
        private Vector3 _offsetEndPostion;
        private Vector3 _directionVector;

        public bool _isAvaibleForLaunch = false;
        private Rigidbody _charRigidbody;

        private GameObject[] _trajectoryDots;
        private Vector2 _zoomRange = new Vector2(5f, 10f); // Диапазон зума

        [Header("Launching Setup")]
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _offsetPoint;
        [Space(10)]
        [SerializeField] private CharacterControl _characterToLaunch;
        [SerializeField] private float _forceFactor;

        [Header("Trajectory")]
        [SerializeField] private GameObject _trajectoryDotPrefab;
        [SerializeField] private int _dotsNumber;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _minDistanceForZoom;



        private void Start()
        {
            _charRigidbody = _characterToLaunch.rigidBody;
            _trajectoryDots = new GameObject[_dotsNumber];
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startPoint.position = new Vector3(0, _startPoint.position.y, _startPoint.position.z);

                //Spawn projectory dots
                for (int i = 0; i < _dotsNumber; i++)
                {
                    _trajectoryDots[i] = Instantiate(_trajectoryDotPrefab, _startPoint);
                }
            }

            if (Input.GetMouseButton(0))
            {
                //Calculation
                var pointerPosition = GetPointerWorldPosition(Camera.main);
                _offsetEndPostion = new Vector3(0, pointerPosition.y, pointerPosition.z);
                _directionVector = _offsetEndPostion - _startPoint.position;

                //Visual offset
                _offsetPoint.position = _offsetEndPostion;

                //Traectory draw
                for (var i = 0; i < _dotsNumber; ++i)
                {
                    _trajectoryDots[i].transform.position = CalculateTraectoryPosition(i * 0.1f);
                }

                //ZoomCamera
                ZoomCamera();
            }

            if (Input.GetMouseButtonUp(0))
            {
                LaunchUnit();
                DestroyTrajectoryDots();
            }
        }

        private void LaunchUnit()
        {
            _charRigidbody.useGravity = true;
            _charRigidbody.velocity = new Vector3(0, -_directionVector.y * _forceFactor, -_directionVector.z * _forceFactor);
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
            return new Vector3(0, _startPoint.transform.position.y, _startPoint.transform.position.z)
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
            if (Vector3.Distance(_offsetEndPostion, _startPoint.position) > _minDistanceForZoom)
            {
                // Рассчитываем множитель для зума от 0 до 1 в зависимости от оттягивания
                var zoomMultiplier = Mathf.Clamp01(Vector3.Distance(_offsetEndPostion, _startPoint.position));

                // Изменяем зум камеры в зависимости от множителя и скорости зума
                float newZoom = Mathf.Lerp(_zoomRange.x, _zoomRange.y, zoomMultiplier);
                Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, newZoom, _zoomSpeed * Time.deltaTime);
            }            
        }
    }
}