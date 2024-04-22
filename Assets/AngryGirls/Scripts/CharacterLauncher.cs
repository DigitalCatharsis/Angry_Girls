using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Angry_Girls
{
    public enum PlayableCharacters
    {
        YBot,
        YBot_Green,
        YBot_Red,
        YBot_Yellow
    }

    public class CharacterLauncher : MonoBehaviour
    {
        private Vector3 _offsetEndPostion;
        private Vector3 _directionVector;
        private Vector3 _startPoint;
        private GameObject[] _trajectoryDots;
        private Transform[] _positionTransforms;

        [Header("Launching Setup")]
        [SerializeField] private GameObject _positionsContainer;
        [Space(10)]
        [SerializeField] private Vector2 _zoomRange = new Vector2(5f, 10f); // Диапазон зума
        [Space(10)]
        [SerializeField] private float _forceFactorUp;
        [SerializeField] private float _forceFactorForward;

        [Header("Trajectory")]
        [SerializeField] private Transform _offsetPoint;
        [SerializeField] private GameObject _trajectoryDotPrefab;
        [SerializeField] private int _dotsNumber;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _minDistanceForZoom;

        [Header("Debug")]
        public bool _canProceedLaunch = false;
        [SerializeField] private List<PlayableCharacters> _tempCharactersListToSpawn;
        [SerializeField] private CharacterControl _characterToLaunch;
        [SerializeField] private List<GameObject> _charactersList;


        private void Start()
        {
            _characterToLaunch = _charactersList[0].GetComponent<CharacterControl>();
            _trajectoryDots = new GameObject[_dotsNumber];
        }

        private void Awake()
        {
            //Spawn Characters at spawn points
            var transforms = new HashSet<Transform>(_positionsContainer.GetComponentsInChildren<Transform>());
            transforms.Remove(_positionsContainer.transform); //remove position of _positionsContainer itself
            _positionTransforms = transforms.ToArray();

            SpawnCharacters();
            UpdateCharacterPositions(_positionTransforms);

            //Start position for launch
            _startPoint = _positionTransforms[0].position;
        }

        private void UpdateCharacterPositions(Transform[] transforms)
        {
            for (var i = 0; i < _tempCharactersListToSpawn.Count; i++)
            {
                _charactersList[i].transform.position = transforms[i].position;
            }
        }
        private void SpawnCharacters()
        {
            for (var i = 0; i < _tempCharactersListToSpawn.Count; i++)
            {
                _charactersList[i] = Instantiate(Resources.Load(_tempCharactersListToSpawn[i].ToString())) as GameObject;
            }
        }

        private void Update()
        { 
            if (Input.GetMouseButtonDown(0) && !_characterToLaunch.SubComponentProcessor.launchLogic.hasBeenLaunched)
            {
                //Check if we switching players


                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (_charactersList.Contains(hit.collider.gameObject))
                    {
                        if (hit.collider.gameObject == _characterToLaunch.gameObject)
                        {
                            _canProceedLaunch = true;
                        }
                        else
                        {
                            SwapCharacters(_charactersList.IndexOf(hit.collider.gameObject), 0);
                            UpdateCharacterPositions(_positionTransforms);
                        }
                    }
                    else
                    {
                        _canProceedLaunch = false;
                    }
                }

                //Spawn projectory dots
                for (int i = 0; i < _dotsNumber; i++)
                {
                    _trajectoryDots[i] = Instantiate(_trajectoryDotPrefab, _characterToLaunch.transform);
                }
            }

            if (Input.GetMouseButton(0) && _canProceedLaunch)
            {
                //Calculation direction
                var pointerPosition = GetPointerWorldPosition(Camera.main);
                _offsetEndPostion = new Vector3(0, pointerPosition.y, pointerPosition.z);
                _directionVector = _offsetEndPostion - _startPoint;

                // Center camera on character collider center
                CameraManager.Instance.CenterCameraAgainst(_characterToLaunch.Boxcollider);

                //Visual offset
                _offsetPoint.position = _offsetEndPostion;

                //Traectory draw
                for (var i = 0; i < _dotsNumber; ++i)
                {
                    _trajectoryDots[i].transform.position = CalculateTraectoryPosition(i * 0.1f);
                }

                //ZoomCamera
                AdjustCameraZoom();
            }

            if (Input.GetMouseButtonUp(0) && _canProceedLaunch)
            {
                DestroyTrajectoryDots();
                _canProceedLaunch = false;
                LaunchUnit();
            }
        }

        private void LaunchUnit()
        {
            _characterToLaunch.RigidBody.useGravity = true;
            _characterToLaunch.RigidBody.velocity = new Vector3(0, -_directionVector.y * _forceFactorUp, -_directionVector.z * _forceFactorForward);
            Camera.main.orthographicSize /= 1.5f;
            _characterToLaunch.IsLaunched = true;
            StartCoroutine(_characterToLaunch.SubComponentProcessor.launchLogic.ProcessLaunch());
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
            return new Vector3(0, _startPoint.y, _startPoint.z)
                    + new Vector3(0, -_directionVector.y * _forceFactorUp, -_directionVector.z * _forceFactorForward) * elapsedTime
                    + 0.5f * Physics.gravity * elapsedTime * elapsedTime;
        }

        private Vector3 GetPointerWorldPosition(Camera camera)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = camera.nearClipPlane + 1;
            return camera.ScreenToWorldPoint(screenPosition);
        }

        private void AdjustCameraZoom()
        {
            // Calculate distance between _offsetEndPostion and _startPoint
            float distance = Vector3.Distance(_offsetEndPostion, _startPoint);

            // Calculate zoom based on distance
            float zoomFactor = Mathf.Lerp(_zoomRange.x, _zoomRange.y, distance / _minDistanceForZoom);

            // Apply zoom to camera
            Camera.main.orthographicSize = zoomFactor;
        }

        public void SwapCharacters(int indexA, int indexB)
        {
            var tmp= _charactersList[indexA];
            _charactersList[indexA] = _charactersList[indexB];
            _charactersList[indexB] = tmp;

            _characterToLaunch = _charactersList[0].GetComponent<CharacterControl>();
        }
    }
}