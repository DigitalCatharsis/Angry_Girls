using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public class CharacterLauncher : MonoBehaviour
    {

        [Header("Aiming FX")]
        [SerializeField] private GameObject _aimingHighlightPrefab;
        private GameObject _currentAimingHighlight;

        [Header("Launch Constraints")]
        [SerializeField] private float _minLaunchDistance = 1f;

        public bool IsAiming => _isAiming; // Для менеджера
        private bool _isAiming = false;


        //Direction
        private Vector3 _offsetEndPostion;
        private Vector3 _offsetStartPoint;
        private Vector3 _directionVector;

        //Traectory
        private GameObject[] _trajectoryDots;

        [Header("Launching Setup")]
        [SerializeField] private GameObject _positionsContainer;
        [Space(10)]
        private const float _minZoom = 5.0f;
        private const float _maxZoom = 10.0f;
        [SerializeField] private float _maxZoomFactorValue = 6.1f;
        [SerializeField] private Vector2 _zoomRange = new Vector2(_minZoom, _maxZoom); // Диапазон зума
        [Space(10)]
        [SerializeField] private float _forceFactorUp;
        [SerializeField] private float _forceFactorForward;

        [Header("Trajectory")]
        [SerializeField] private Transform _offsetPoint;
        [SerializeField] private GameObject _trajectoryDotPrefab;
        [SerializeField] private int _dotsNumber;

        [Header("Zoom")]
        [SerializeField] private float _minDistanceForZoom;

        public Transform[] UnitsTransforms { get; private set; }


        private bool _cheatModeActive = false;
        private int _originalDotsNumber;



        public void InitLauncher()
        {
            var transforms = new HashSet<Transform>(_positionsContainer.GetComponentsInChildren<Transform>());
            transforms.Remove(_positionsContainer.transform);
            UnitsTransforms = transforms.ToArray();

            _originalDotsNumber = _dotsNumber; // <-- сохраняем оригинальное
            _trajectoryDots = new GameObject[_dotsNumber];
            SpawnProjectoryDots(UnitsTransforms[0]);


            _offsetStartPoint = new Vector3(UnitsTransforms[0].transform.position.x, UnitsTransforms[0].transform.position.y + 0.4f, UnitsTransforms[0].transform.position.z);

            // Highlight (создаем заранее, скрываем)
            if (_aimingHighlightPrefab)
            {
                _currentAimingHighlight = Instantiate(_aimingHighlightPrefab);
                _currentAimingHighlight.SetActive(false);
            }

            _originalDotsNumber = _dotsNumber;
            _trajectoryDots = new GameObject[_dotsNumber];
            SpawnProjectoryDots(UnitsTransforms[0]);
        }

        public void SetCheatTrajectoryMode(bool enable)
        {
            if (_cheatModeActive == enable)
                return;

            _cheatModeActive = enable;

            // Сохраняем текущую позицию, чтобы переинициализировать
            Transform currentTransform = UnitsTransforms.Length > 0 ? UnitsTransforms[0] : null;

            // Удаляем старые точки
            if (_trajectoryDots != null)
            {
                foreach (var dot in _trajectoryDots)
                {
                    if (dot != null)
                        Destroy(dot);
                }
            }

            // Меняем число точек
            _dotsNumber = enable ? 100 : _originalDotsNumber;

            // Создаём новые
            _trajectoryDots = new GameObject[_dotsNumber];
            if (currentTransform != null)
            {
                SpawnProjectoryDots(currentTransform);
            }
        }


        public void LaunchUnit(CControl characterToLaunch)
        {
            CancelAiming();

            if (_directionVector.z > 0)
            {
                characterToLaunch.CharacterMovement.SetRotation(Quaternion.Euler(0, 180, 0));
            }
            else if (_directionVector.z < 0)
            {
                characterToLaunch.CharacterMovement.SetRotation(Quaternion.Euler(0, 0, 0));
            }
            else if (_directionVector.z == 0)
            {
                ColorDebugLog.Log("Direction vector = 0", System.Drawing.KnownColor.Red);
            }

            characterToLaunch.CharacterMovement.Rigidbody.useGravity = true;
            characterToLaunch.CharacterMovement.SetVelocity(new Vector3(0, -_directionVector.y * _forceFactorUp, -_directionVector.z * _forceFactorForward));
        }       


        public void AimingTheLaunch(GameObject characterToLaunch)
        {
            _isAiming = true;


            CalculateDirection();
            DrawTraectory();
            AdjustCameraZoom();


            ShowAimingHighlight();
        }
        private void ShowAimingHighlight()
        {
            if (_currentAimingHighlight != null)
            {
                _currentAimingHighlight.transform.position = _offsetStartPoint;
                _currentAimingHighlight.SetActive(true);
            }
        }

        private void HideAimingHighlight()
        {
            if (_currentAimingHighlight != null)
            {
                _currentAimingHighlight.SetActive(false);
            }
        }

        public void CancelAiming()
        {
            GameLoader.Instance.cameraManager.StopCameraFollowForRigidBody();
            _isAiming = false;
            DisableTrajectoryDots();
            HideAimingHighlight();
        }

        public bool IsLaunchDistanceSufficient()
        {
            return _directionVector.magnitude >= _minLaunchDistance;
        }


        private void SpawnProjectoryDots(Transform spawnTransform)
        {
            for (int i = 0; i < _dotsNumber; i++)
            {
                _trajectoryDots[i] = Instantiate(_trajectoryDotPrefab, spawnTransform);
            }
        }

        private void CalculateDirection()
        {
            //Calculation direction
            var pointerPosition = GameLoader.Instance.cameraManager.GetPointerWorldPosition();
            _offsetEndPostion = new Vector3(0, pointerPosition.y, pointerPosition.z);
            _directionVector = _offsetEndPostion - _offsetStartPoint;
        }
        private void DrawTraectory()
        {
            EnableTrajectoryDots();
            _offsetPoint.position = _offsetEndPostion;

            for (var i = 0; i < _dotsNumber; ++i)
            {
                _trajectoryDots[i].transform.position = CalculateTraectoryPosition(i * 0.1f);
            }
        }
        private Vector3 CalculateTraectoryPosition(float elapsedTime)
        {
            return new Vector3(0, _offsetStartPoint.y, _offsetStartPoint.z)
                    + new Vector3(0, -_directionVector.y * _forceFactorUp, -_directionVector.z * _forceFactorForward) * elapsedTime
                    + 0.5f * Physics.gravity * elapsedTime * elapsedTime;
        }

        private void DisableTrajectoryDots()
        {
            for (int i = 0; i < _trajectoryDots.Length; i++)
            {
                _trajectoryDots[i].SetActive(false);
            }
        }
        private void EnableTrajectoryDots()
        {
            for (int i = 0; i < _trajectoryDots.Length; i++)
            {
                _trajectoryDots[i].SetActive(true);
            }
        }
        private void AdjustCameraZoom()
        {
            // Calculate distance between _offsetEndPostion and _startPoint
            float distance = Vector3.Distance(_offsetEndPostion, _offsetStartPoint);

            // Calculate zoom based on distance
            float zoomFactor = Mathf.Lerp(_zoomRange.x, _zoomRange.y, distance / _minDistanceForZoom);

            // Apply zoom to camera
            if (zoomFactor >= _maxZoomFactorValue)
            {
                Camera.main.orthographicSize = _maxZoomFactorValue;
            }
            else
            {
                Camera.main.orthographicSize = zoomFactor;
            }

        }
    }
}